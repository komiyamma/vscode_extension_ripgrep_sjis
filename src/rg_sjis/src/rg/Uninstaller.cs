/*
 * Copyright (C) 2021-2023 Akitsugu Komiyama
 * under the MIT License
 */

using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace RipGrep
{
    /// <summary>
    /// Visual Studio Code の rg.exe を元に戻すアンインストーラー。
    /// </summary>
    internal static class UnInstaller
    {
        private static string m_vscode_path = string.Empty;

        /// <summary>
        /// VSCodeのパスを指定してアンインストール処理を行う。パス未指定時は自動検出。
        /// </summary>
        /// <param name="vscode_path">VSCodeのパス（省略可）</param>
        public static void UnInstall(string vscode_path = "")
        {
            m_vscode_path = vscode_path;
            try
            {
                RgHelpConsoleOutput();

                // VSCodeパスが指定されていれば即アンインストール処理
                if (!string.IsNullOrEmpty(m_vscode_path))
                {
                    proc_OutputDataReceived(null, null);
                    return;
                }

                // VSCodeが複数起動されていない場合のみアンインストールを行う（現状は常に実行）
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = Environment.GetEnvironmentVariable("ComSpec");
                    process.StartInfo.Arguments = "/c where code.cmd";
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                    process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                    process.ErrorDataReceived += proc_ErrorDataReceived;
                    process.OutputDataReceived += proc_OutputDataReceived;

                    process.Start();
                    process.BeginOutputReadLine();
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// rgのヘルプ出力を模倣したエラーメッセージを表示。
        /// </summary>
        private static void RgHelpConsoleOutput()
        {
            Console.WriteLine(@"
error: The following required arguments were not provided:
    <PATTERN>

USAGE:

    rg [OPTIONS] PATTERN [PATH ...]
    rg [OPTIONS] [-e PATTERN ...] [-f PATTERNFILE ...] [PATH ...]
    rg [OPTIONS] --files [PATH ...]
    rg [OPTIONS] --type-list
    command | rg [OPTIONS] PATTERN

For more information try --help
");
        }

        /// <summary>
        /// VSCodeのrg.exeのアンインストール処理本体。
        /// </summary>
        private static void proc_OutputDataReceived(object sender, DataReceivedEventArgs ev)
        {
            string line = string.Empty;
            if (!string.IsNullOrEmpty(m_vscode_path))
            {
                line = Path.Combine(Path.GetDirectoryName(m_vscode_path), "bin", "code.cmd");
            }
            else
            {
                line = ev?.Data;
            }

            if (File.Exists(line))
            {
                string basePath = Path.GetDirectoryName(line);
                string[] relativePaths =
                {
                    @"..\resources\app\node_modules.asar.unpacked\vscode-ripgrep\bin\rg.exe",
                    @"..\resources\app\node_modules.asar.unpacked\@vscode\ripgrep\bin\rg.exe",
                    @"..\resources\app\node_modules\@vscode\ripgrep\bin\rg.exe"
                };
                string rgFullPath = null;
                FileInfo fiRg = null;
                foreach (var rel in relativePaths)
                {
                    var candidate = Path.GetFullPath(Path.Combine(basePath, rel));
                    if (File.Exists(candidate))
                    {
                        rgFullPath = candidate;
                        fiRg = new FileInfo(candidate);
                        break;
                    }
                }
                if (rgFullPath == null)
                    return;

                string rgFullDir = Path.GetDirectoryName(rgFullPath);
                string rgUTF8FullPath = Path.Combine(rgFullDir, "rg_utf8.exe");
                long rgFileSize = fiRg.Length;

                // utf8版とsjis版の両方があり、rg.exeがラッパーであるならば
                if (File.Exists(rgUTF8FullPath) && File.Exists(rgFullPath) && rgFileSize < 1024000)
                {
                    try
                    {
                        File.Copy(rgUTF8FullPath, rgFullPath, true);
                        // File.Delete(rgUTF8FullPath); // 残しておいても弊害がないので削除しない
                        Console.WriteLine("RgSJISUninstallSuccess");
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// 標準エラー出力受信時の処理（標準出力と同じ処理を行う）。
        /// </summary>
        private static void proc_ErrorDataReceived(object sender, DataReceivedEventArgs ev)
        {
            proc_OutputDataReceived(sender, ev);
        }
    }
}

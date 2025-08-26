/*
 * Copyright (C) 2021-2023 Akitsugu Komiyama
 * under the MIT License
 */

using System;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace RipGrep
{
    /// <summary>
    /// Visual Studio Code の rg.exe を SJIS対応版に差し替えるインストーラー。
    /// </summary>
    internal static class Installer
    {
        private static string m_vscode_path = string.Empty;

        /// <summary>
        /// VSCodeのパスを指定してインストール処理を行う。パス未指定時は自動検出。
        /// </summary>
        /// <param name="vscode_path">VSCodeのパス（省略可）</param>
        public static void Install(string vscode_path = "")
        {
            m_vscode_path = vscode_path;
            try
            {
                RgHelpConsoleOutput();

                if (!string.IsNullOrEmpty(m_vscode_path))
                {
                    proc_OutputDataReceived(null, null);
                    return;
                }

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
        /// VSCodeのrg.exe設置ディレクトリをJSONで保存する。
        /// </summary>
        private static void SaveVsCodePath(string path)
        {
            try
            {
                var this_program_dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var data = new { Path = path };
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(Path.Combine(this_program_dir, "rg_sjis.json"), json);
            }
            catch (Exception)
            {
                // 保存失敗時は無視
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
        /// VSCodeのrg.exeの差し替え・設置処理本体。
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
                SaveVsCodePath(rgFullDir);

                string rgUTF8FullPath = Path.Combine(rgFullDir, "rg_utf8.exe");
                string myProgramFullPath = Assembly.GetExecutingAssembly().Location;
                FileInfo fiSjis = new FileInfo(myProgramFullPath);

                // オリジナルrg.exeとラッパーのサイズ比較で差し替え判定
                if (fiRg.Length != fiSjis.Length && fiRg.Length > 1024000)
                {
                    try { File.Copy(rgFullPath, rgUTF8FullPath, true); } catch { }
                    try
                    {
                        File.Copy(myProgramFullPath, rgFullPath, true);
                        if (File.Exists(rgUTF8FullPath))
                        {
                            Console.WriteLine("RgSJISInstallSuccess");
                        }
                    }
                    catch { }
                }
                else if (fiRg.Length != fiSjis.Length && fiRg.Length < 1024000)
                {
                    if (File.Exists(rgUTF8FullPath))
                    {
                        File.Copy(myProgramFullPath, rgFullPath, true);
                        Console.WriteLine("RgSJISInstallSuccess");
                    }
                }
                else
                {
                    if (File.Exists(rgUTF8FullPath))
                    {
                        Console.WriteLine("RgSJISInstallSuccess");
                    }
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

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
    internal class Installer
    {
        static string m_vscode_path = "";
        public static void Install(string vscode_path = "")
        {
            m_vscode_path = vscode_path;

            try
            {
                RgHelpConsoleOutput();

                if (m_vscode_path != "")
                {
                    proc_OutputDataReceived(null, null);
                    return;
                }

                Process process = new Process();

                process.StartInfo.FileName = System.Environment.GetEnvironmentVariable("ComSpec");
                process.StartInfo.Arguments = "/c where code.cmd";

                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;

                //イベントハンドラの追加
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;

                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

                process.ErrorDataReceived += proc_ErrorDataReceived;
                process.OutputDataReceived += proc_OutputDataReceived;


                //起動する
                process.Start();
                process.BeginOutputReadLine();

                process.WaitForExit();

                try
                {
                    if (process != null)
                    {
                        process.Close();
                        process.Kill();
                    }
                }
                catch
                {

                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private static void SaveVsCodePath(string path)
        {
            try
            {
                var this_program_dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                // 保存するデータを作成する
                var data = new
                {
                    Path = path,
                };

                // シリアライズして保存する
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(this_program_dir + "\\rg_sjis.json", json);
            } catch(Exception) { }
        }


        private static void RgHelpConsoleOutput()
        {
            // 先にデフォルトの出力と同じものを出しておく
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

        private static void proc_OutputDataReceived(object sender, DataReceivedEventArgs ev)
        {
            string line = "";
            if (m_vscode_path != "")
            {
                line = Path.GetDirectoryName(m_vscode_path) + "/bin/code.cmd";
            }
            else
            {
                line = ev.Data;
            }
            if (File.Exists(line))
            {
                string basePath = Path.GetDirectoryName(line);
                string relativePath = @"..\resources\app\node_modules.asar.unpacked\vscode-ripgrep\bin\rg.exe";
                FileInfo fiRg = new FileInfo(System.IO.Path.Combine(basePath, relativePath));
                string rgFullPath = fiRg.FullName;

                if (!File.Exists(rgFullPath))
                {
                    relativePath = @"..\resources\app\node_modules.asar.unpacked\@vscode\ripgrep\bin\rg.exe"; // 元々のRipgrepのパス v1.66以降？
                    fiRg = new FileInfo(System.IO.Path.Combine(basePath, relativePath));
                    rgFullPath = fiRg.FullName;
                }

                // rg.exeがvscodeの所定の場所に存在するのか。
                if (File.Exists(rgFullPath))
                {
                    string rgFullDir = Path.GetDirectoryName(rgFullPath);

                    SaveVsCodePath(rgFullDir);

                    string rgUTF8FullPath = rgFullDir + @"\rg_utf8.exe";
                    string myProgramFullPath = Assembly.GetExecutingAssembly().Location;
                    FileInfo fiSjis = new FileInfo(myProgramFullPath);

                    // 両方ともこのプログラム自身と同じであるならば、何もしない。すでにラッパーをプラグインフォルダからラッパーフォルダへとコピー済み
                    // vscodeフォルダにあるのがオリジナルであるならば...
                    if (fiRg.Length != fiSjis.Length && fiRg.Length > 1024000)
                    {

                        try
                        {
                            File.Copy(rgFullPath, rgUTF8FullPath, true);
                        }
                        catch (Exception e)
                        {

                        }
                        try
                        {
                            File.Copy(myProgramFullPath, rgFullPath, true); // 上書き保存
                            if (File.Exists(rgUTF8FullPath))
                            {
                                Console.WriteLine("RgSJISInstallSuccess");
                            }
                        }
                        catch (Exception e)
                        {

                        }
                    }

                    // プログラムは異なるのに、rg.exeのサイズは小さい
                    else if (fiRg.Length != fiSjis.Length && fiRg.Length < 1024000)
                    {
                        // rg_utf8の存在があるならば...
                        if (File.Exists(rgUTF8FullPath))
                        {
                            File.Copy(myProgramFullPath, rgFullPath, true); // 上書き保存
                            Console.WriteLine("RgSJISInstallSuccess");
                        }
                    }
                    // 同じファイルであるため、コピー処理を停止。
                    else
                    {
                        if (File.Exists(rgUTF8FullPath))
                        {
                            Console.WriteLine("RgSJISInstallSuccess");
                        }
                    }
                }
            }

        }

        private static void proc_ErrorDataReceived(object sender, DataReceivedEventArgs ev)
        {
            proc_OutputDataReceived(sender, ev);
        }
    }
}

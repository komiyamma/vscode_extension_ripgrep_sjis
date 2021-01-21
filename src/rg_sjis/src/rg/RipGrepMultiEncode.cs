/*
 * Copyright (C) 2021 Akitsugu Komiyama
 * under the MIT License
 */


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace RipGrep
{
    internal static class StringEncodeExtension
    {
        public static string EncodeCommandLineValue(this string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            var containsSpace = value.IndexOfAny(new[] { ' ', '\t' }) != -1;

            // 「\…\"」をエスケープ
            // やってることは、「"」直前の「\」の数を 2倍+1
            value = _commandLineEscapePattern.Replace(value, @"$1\$&");

            // スペース／タブが含まれる場合はデリミタで囲み、末尾が「\」だった場合、エスケープ
            if (containsSpace)
            {
                value = "\"" + _lastBackSlashPattern.Replace(value, "$1$1") + "\"";
            }
            return value;
        }
        private static readonly Regex _commandLineEscapePattern = new Regex("(\\\\*)\"");
        private static readonly Regex _lastBackSlashPattern = new Regex(@"(\\+)$");

        /// 
        /// コマンドライン引数複数個をエンコードして、スペースで結合
        /// 
        public static string EncodeCommandLineValues(this IEnumerable<string> values)
        {
            if (values == null) throw new ArgumentNullException("values");
            return string.Join(" ", values.Select(v => EncodeCommandLineValue(v)));
        }
    }


    internal class RipGrepMultiEncode
    {
        static Dictionary<string, bool> hit_string_dictionary = new Dictionary<string, bool>();
        static Dictionary<Tuple<string, string>, bool> hit_path_line_dictionary = new Dictionary<Tuple<string, string>, bool>();

        Process process = new Process();

        List<string> arg_list = null;
        List<string> arg_list_head_for_sjis = new List<string> { "-E", "sjis" };

        const string rg_utf8_name = "rg_utf8.exe";

        Encoding enc;

        bool is_search_mode = true;

        public RipGrepMultiEncode(string[] args, bool search_mode)
        {
            is_search_mode = search_mode;
            if (arg_list == null)
            {
                arg_list = new List<string>(args);
            }

            //起動するファイルを指定する
            var self = Assembly.GetExecutingAssembly().Location;
            process.StartInfo.FileName = Path.GetDirectoryName(self) + '/' + rg_utf8_name;

        }

        string MakeArgsString(Encoding enc)
        {
            this.enc = enc;
            if (enc == Encoding.GetEncoding(932))
            {
                // sjisオプション側を先頭にして
                arg_list_head_for_sjis.AddRange(arg_list);

                // 元のリストとする。
                arg_list = arg_list_head_for_sjis;
            }

            string arg_line = arg_list.EncodeCommandLineValues();
            System.Diagnostics.Trace.WriteLine(arg_line);
            return arg_line;
        }

        public void Grep(Encoding enc)
        {
            try
            {
                string arg_line = MakeArgsString(enc);

                process.StartInfo.Arguments = arg_line;

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

        private Tuple<string, string> GetHitPathAndLine(string data)
        {
            dynamic document = Newtonsoft.Json.JsonConvert.DeserializeObject(data);
            string s = document.data?.path?.text;
            string l = document.data?.line_number;
            if (s != null && l != null)
            {
                var t = Tuple.Create<string, string>(s, l);
                return t;
            }

            var n = Tuple.Create<string, string>(null, null);
            return n;
        }

        private void proc_OutputDataReceived(object sender, DataReceivedEventArgs ev)
        {
            string data = ev.Data;
            try
            {
                if (data != null)
                {
                    lock (hit_string_dictionary)
                    {
                        // まだ登録されていない時だけ、出力候補となる
                        if (!hit_string_dictionary.ContainsKey(data))
                        {
                            if (enc == Encoding.UTF8)
                            {
                                // utf8の時のファイルのパスと行を控えておく
                                hit_string_dictionary.Add(data, true);
                                Console.WriteLine(data);

                                if (is_search_mode)
                                {
                                    var t = GetHitPathAndLine(data);
                                    if (t.Item1 != null && t.Item2 != null)
                                    {
                                        hit_path_line_dictionary.Add(t, true);
                                    }
                                }
                            }

                            if (enc == Encoding.GetEncoding(932))
                            {
                                var t = GetHitPathAndLine(data);
                                if (t.Item1 != null && t.Item2 != null)
                                {
                                    // utf8の時にファイルのパスと行がすでにヒットしていたら、sjisはその行は表示しない。(半角英数でヒットしたのだろう)
                                    // ヒットしていなければ表示
                                    if (!hit_path_line_dictionary.ContainsKey(t))
                                    {
                                        Console.WriteLine(data);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

        }

        private void proc_ErrorDataReceived(object sender, DataReceivedEventArgs ev)
        {
            proc_OutputDataReceived(sender, ev);
        }
    }

}



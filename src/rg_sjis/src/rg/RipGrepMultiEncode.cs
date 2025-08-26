/*
 * Copyright (C) 2021-2023 Akitsugu Komiyama
 * under the MIT License
 */

using System;
using System.Collections.Concurrent;
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
        private static readonly Regex CommandLineEscapePattern = new Regex(@"(\\*)\""", RegexOptions.Compiled);
        private static readonly Regex LastBackSlashPattern = new Regex(@"(\\+)$", RegexOptions.Compiled);

        public static string EncodeCommandLineValue(this string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            var containsSpace = value.IndexOfAny(new[] { ' ', '\t' }) != -1;

            // 「\…\"」をエスケープ
            value = CommandLineEscapePattern.Replace(value, @"$1\\$&");

            // スペース／タブが含まれる場合はデリミタで囲み、末尾が「\」だった場合、エスケープ
            if (containsSpace)
            {
                value = "\"" + LastBackSlashPattern.Replace(value, "$1$1") + "\"";
            }
            return value;
        }

        /// <summary>
        /// コマンドライン引数複数個をエンコードして、スペースで結合
        /// </summary>
        public static string EncodeCommandLineValues(this IEnumerable<string> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            return string.Join(" ", values.Select(EncodeCommandLineValue));
        }
    }

    internal sealed class RipGrepMultiEncode : IDisposable
    {
        private static readonly ConcurrentDictionary<string, bool> HitStringDictionary = new ConcurrentDictionary<string, bool>();
        private static readonly ConcurrentDictionary<Tuple<string, string>, bool> HitPathLineDictionary = new ConcurrentDictionary<Tuple<string, string>, bool>();

        private readonly Process _process;
        private readonly List<string> _argList;
        private readonly List<string> _argListHeadForSjis = new List<string> { "-E", "sjis" };
        private readonly bool _isSearchMode;
        private Encoding _enc;

        private const string RgUtf8Name = "rg_utf8.exe";

        public RipGrepMultiEncode(string[] args, bool searchMode)
        {
            _isSearchMode = searchMode;
            _argList = new List<string>(args ?? throw new ArgumentNullException(nameof(args)));
            _process = new Process
            {
                StartInfo =
                {
                    FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, RgUtf8Name),
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                },
                EnableRaisingEvents = false
            };
            _process.ErrorDataReceived += Proc_ErrorDataReceived;
            _process.OutputDataReceived += Proc_OutputDataReceived;
        }

        private string MakeArgsString(Encoding enc)
        {
            _enc = enc ?? throw new ArgumentNullException(nameof(enc));
            var args = _argList;
            if (enc.Equals(Encoding.GetEncoding(932)))
            {
                var sjisArgs = new List<string>(_argListHeadForSjis);
                sjisArgs.AddRange(_argList);
                args = sjisArgs;
            }
            var argLine = args.EncodeCommandLineValues();
            Trace.WriteLine(argLine);
            return argLine;
        }

        public void Grep(Encoding enc)
        {
            try
            {
                _process.StartInfo.Arguments = MakeArgsString(enc);

                using (_process)
                {
                    _process.Start();
                    _process.BeginOutputReadLine();
                    _process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private static Tuple<string, string> GetHitPathAndLine(string data)
        {
            dynamic document = Newtonsoft.Json.JsonConvert.DeserializeObject(data);
            string s = document.data?.path?.text;
            string l = document.data?.line_number;
            return (s != null && l != null)
                ? Tuple.Create(s, l)
                : Tuple.Create<string, string>(null, null);
        }

        private void Proc_OutputDataReceived(object sender, DataReceivedEventArgs ev)
        {
            var data = ev.Data;
            if (data == null) return;

            try
            {
                if (HitStringDictionary.TryAdd(data, true))
                {
                    if (_enc == Encoding.UTF8)
                    {
                        Console.WriteLine(data);

                        if (_isSearchMode)
                        {
                            var t = GetHitPathAndLine(data);
                            if (t.Item1 != null && t.Item2 != null)
                            {
                                HitPathLineDictionary.TryAdd(t, true);
                            }
                        }
                    }
                    else if (_enc == Encoding.GetEncoding(932))
                    {
                        var t = GetHitPathAndLine(data);
                        if (t.Item1 != null && t.Item2 != null && !HitPathLineDictionary.ContainsKey(t))
                        {
                            Console.WriteLine(data);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void Proc_ErrorDataReceived(object sender, DataReceivedEventArgs ev)
        {
            Proc_OutputDataReceived(sender, ev);
        }

        public void Dispose()
        {
            _process?.Dispose();
        }
    }
}



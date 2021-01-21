/*
 * Copyright (C) 2021 Akitsugu Komiyama
 * under the MIT License
 */

using System;
using System.Text;

namespace RipGrep
{
    public class RG
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                RipGrep.Installer.Install();
            }
            else if (args.Length == 2 && args[0] == "--mode-install")
            {
                RipGrep.Installer.Install(args[1]);
            }
            else if (args.Length == 2 && args[0] == "--mode-uninstall")
            {
                RipGrep.UnInstaller.UnInstall(args[1]);
            }
            else
            {
                bool search_mode = IsCallFromVSCodeSearch(args);

                // VSCodeからサーチモードで呼び出されている
                if (search_mode)
                {
                    Console.OutputEncoding = Encoding.UTF8;

                    RipGrepMultiEncode rgcl1 = new RipGrepMultiEncode(args, search_mode);
                    rgcl1.Grep(Encoding.UTF8);

                    RipGrepMultiEncode rgcl2 = new RipGrepMultiEncode(args, search_mode);
                    rgcl2.Grep(Encoding.GetEncoding(932));
                }

                // VSCodeからいろいろな瞬間呼び出されているのでとりあえず、そんまま流しておく
                else
                {
                    Console.OutputEncoding = Encoding.UTF8;

                    RipGrepMultiEncode rgcl1 = new RipGrepMultiEncode(args, search_mode);
                    rgcl1.Grep(Encoding.UTF8);
                }
            }
        }

        private static bool IsCallFromVSCodeSearch(string[] args)
        {
            bool search_mode = false;

            foreach (var s in args)
            {
                if (s.Contains(@".code-search"))
                {
                    search_mode = true;
                }
            }

            return search_mode;
        }
    }
}
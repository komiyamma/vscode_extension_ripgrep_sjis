using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace RipGrep
{
    /// <summary>
    /// Visual Studio Code の起動インスタンス数を取得するためのユーティリティクラス。
    /// </summary>
    internal static class VisualStudioCodeInstanceCounter
    {
        // ウィンドウ情報を格納するリスト。
        private static readonly List<System.Tuple<IntPtr, uint, StringBuilder, StringBuilder>> windowInfoList = new List<System.Tuple<IntPtr, uint, StringBuilder, StringBuilder>>();

        /// <summary>
        /// 指定したパスの Visual Studio Code の起動インスタンス数を取得します。
        /// </summary>
        /// <param name="vsPath">Visual Studio Code の実行ファイルパス</param>
        /// <returns>起動しているインスタンス数</returns>
        public static int GetLaunchCount(string vsPath)
        {
            if (string.IsNullOrWhiteSpace(vsPath))
                throw new ArgumentException("vsPath must not be null or whitespace.", nameof(vsPath));

            windowInfoList.Clear();
            EnumWindows(EnumWindowCallBack, IntPtr.Zero);

            int count = 0;
            var processList = Process.GetProcessesByName("Code");
            foreach (var p in processList)
            {
                try
                {
                    string filepath = p.MainModule?.FileName;
                    if (!string.IsNullOrEmpty(filepath) &&
                        string.Equals(System.IO.Path.GetFullPath(filepath), System.IO.Path.GetFullPath(vsPath), StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var l in windowInfoList)
                        {
                            if (p.Id == l.Item2 && l.Item3.ToString().Contains("Visual Studio Code"))
                            {
                                count++;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // アクセス権限等で MainModule 取得に失敗する場合があるため、無視
                }
            }
            return count;
        }

        // ウィンドウ列挙用デリゲート
        public delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lparam);

        // Win32 API: ウィンドウを列挙
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumWindowsDelegate lpEnumFunc, IntPtr lparam);

        // Win32 API: ウィンドウタイトル取得
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        // Win32 API: ウィンドウタイトル長取得
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        // Win32 API: クラス名取得
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        // Win32 API: プロセスID取得
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        /// <summary>
        /// ウィンドウ列挙時のコールバック。ウィンドウ情報をリストに追加します。
        /// </summary>
        private static bool EnumWindowCallBack(IntPtr hWnd, IntPtr lparam)
        {
            int textLen = GetWindowTextLength(hWnd);
            if (textLen > 0)
            {
                var windowTitle = new StringBuilder(textLen + 1);
                GetWindowText(hWnd, windowTitle, windowTitle.Capacity);

                var className = new StringBuilder(256);
                GetClassName(hWnd, className, className.Capacity);

                uint processId = 0;
                GetWindowThreadProcessId(hWnd, out processId);

                windowInfoList.Add(System.Tuple.Create(hWnd, processId, windowTitle, className));
            }
            // すべてのウィンドウを列挙する
            return true;
        }
    }
}

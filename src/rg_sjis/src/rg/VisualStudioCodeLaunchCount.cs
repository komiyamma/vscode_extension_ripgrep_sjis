
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

internal class JidgeVisualStudioMultiple
{
    static List<Tuple<IntPtr, uint, StringBuilder, StringBuilder>> list = new List<Tuple<IntPtr, uint, StringBuilder, StringBuilder>>();

    /// <summary>
    /// エントリポイント
    /// </summary>
    public static int GetVisualStudioCodeLaunchCount(String vs_path)
    {
        //ウィンドウを列挙する
        EnumWindows(new EnumWindowsDelegate(EnumWindowCallBack), IntPtr.Zero);

        int count = 0;

        //結果を表示する
        var process_list = Process.GetProcessesByName("Code");
        foreach (var p in process_list)
        {
            try
            {
                String filepath = p.MainModule?.FileName;
                if (System.IO.Path.GetFullPath(filepath) == System.IO.Path.GetFullPath(vs_path))
                {
                    foreach (var l in list)
                    {
                        if (p.Id == l.Item2 && l.Item3.ToString().Contains("Visual Studio Code"))
                        {
                            count++;
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        return count;
    }

    public delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lparam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public extern static bool EnumWindows(EnumWindowsDelegate lpEnumFunc, IntPtr lparam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd,
        StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetClassName(IntPtr hWnd,
        StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    private static bool EnumWindowCallBack(IntPtr hWnd, IntPtr lparam)
    {
        //ウィンドウのタイトルの長さを取得する
        int textLen = GetWindowTextLength(hWnd);
        if (0 < textLen)
        {
            //ウィンドウのタイトルを取得する
            StringBuilder tsb = new StringBuilder(textLen + 1);
            GetWindowText(hWnd, tsb, tsb.Capacity);

            //ウィンドウのクラス名を取得する
            StringBuilder csb = new StringBuilder(256);
            GetClassName(hWnd, csb, csb.Capacity);

            uint id = 0;
            GetWindowThreadProcessId(hWnd, out id);

            list.Add(Tuple.Create(hWnd, id, tsb, csb));
        }

        //すべてのウィンドウを列挙する
        return true;
    }
}

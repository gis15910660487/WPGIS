
using System;
using System.Text;
using System.Runtime.InteropServices;

namespace WPGIS.Log
{
    /// <summary>
    /// 备注:Windows窗体消息内容
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CopyDataStruct
    {
        /// <summary>
        /// 目标窗体句柄
        /// </summary>
        public IntPtr dwData;

        /// <summary>
        /// 字符串长度
        /// </summary>
        public int cbData;

        /// <summary>
        /// 字符串内容
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpData;
    }

    /// <summary>
    /// Windows窗体消息
    /// </summary>
    public class WindowMessageHelper
    {
        /// <summary>
        /// 写死了 无需更改
        /// </summary>
        public static  int WM_COPYDATA = 0x004A;

        /// <summary>
        /// 找到窗体
        /// </summary>
        /// <param name="lpClassName"></param>
        /// <param name="lpWindowName"></param>
        /// <returns></returns>
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage
            (
            IntPtr hWnd,                   //目标窗体句柄
            int Msg,                       //WM_COPYDATA
            int wParam,                                             //自定义数值
            ref  CopyDataStruct lParam             //结构体
            );

        /// <summary>
        /// 向窗体发送Windows消息
        /// </summary>
        /// <param name="windowTitle">窗体Title</param>
        /// <param name="strMsg">消息内容</param>
        public static void SendMessage(string windowTitle, string strMsg)
        {
            if (strMsg == null) return;
            IntPtr hwnd = FindWindow(null, windowTitle);

            if (hwnd != IntPtr.Zero)
            {
                CopyDataStruct cds;
                cds.dwData = IntPtr.Zero;
                cds.lpData = strMsg;
                cds.cbData = Encoding.Default.GetBytes(strMsg).Length + 1;
                SendMessage(hwnd, WM_COPYDATA, 0, ref  cds);
            }
        }
    }
}

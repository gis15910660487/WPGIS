
using System;
using System.Windows;
using System.Windows.Interop;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WPGIS.LogMonitor
{
    ///备注:Windows窗体消息内容
    [StructLayout(LayoutKind.Sequential)]
    internal struct CopyDataStruct
    {
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
    
    ///备注:日志输出窗体 接收CommonLog输出的日志
    public partial class LogMonitor : Window
    {
        internal const int WM_COPYDATA = 0x004A;

        private DateTime lastLogtime = DateTime.MaxValue;
        private readonly string strSplitLine;//分割线
        private const string msgLogMonitorListen = "LogMonitor is listening...";

        /// <summary>
        /// 日志输出窗体
        /// </summary>
        public LogMonitor()
        {
            InitializeComponent();
            Loaded += LogMonitor_Loaded;

            WindowState = WindowState.Maximized;
            textMain.Text = msgLogMonitorListen; textMain.IsReadOnly = true;

            //分隔线
            var splitChar = new List<string>();
            for (int i = 0; i < 100; i++)
                splitChar.Add("-");
            strSplitLine = string.Join("", splitChar);
        }

        private void LogMonitor_Loaded(object sender, RoutedEventArgs e)
        {
            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
                hwndSource.AddHook(WndProc);

            WindowState = WindowState.Minimized;
        }
        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            textMain.Text = "NoapLogMonitor is listening...";
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_COPYDATA)
            {
                var cds = (CopyDataStruct)Marshal.PtrToStructure(lParam, typeof(CopyDataStruct));
                string log = cds.lpData;

                int logLevel = Convert.ToInt32(log.Split(new[] { "&" }, StringSplitOptions.None)[0]);
                string logContent = log.Remove(0, 2);

                Application.Current.Dispatcher.Invoke((Action)(() => AppendLog(logLevel, logContent)));
            }
            return hwnd;
        }
        private void AppendLog(int logLevel, string logContent)
        {
            if (textMain.Text == msgLogMonitorListen) //第一条Log
            {
                textMain.Text = string.Empty;
                textMain.AppendText(string.Format("{0}[{1}]{2}{3}", strSplitLine, DateTime.Now.ToString("yyyy-MM-dd HH:mm"), strSplitLine, Environment.NewLine));
            }
            else if ((DateTime.Now - lastLogtime).TotalSeconds > 60)//下一分钟Log
                textMain.AppendText(Environment.NewLine + string.Format("{0}[{1}]{2}{3}", strSplitLine, DateTime.Now.ToString("yyyy-MM-dd HH:mm"), strSplitLine, Environment.NewLine));

            string strLogLevel = string.Empty;
            if (logLevel == 0) strLogLevel = "输出";
            if (logLevel == 1) strLogLevel = "命令";
            if (logLevel == 2) strLogLevel = "异常";

            var log = string.Format("[{0}  {1}]：{2}", DateTime.Now.ToString("HH:mm:ss"), strLogLevel, logContent);
            if (logLevel > 0)
                log = Environment.NewLine + log;//命令和异常单起一行 查看方便

            textMain.AppendText(log + Environment.NewLine);

            textMain.ScrollToEnd();
            lastLogtime = DateTime.Now;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 1; i++)
            {
                AppendLog(0, "普通提示信息:" + Guid.NewGuid().ToString());
                AppendLog(1, "select IMSI,CALLINGNUMBER,CALLEDNUMBER,CALLASSREQTIME,ASSCOMPTIME,CALLSETTIME,CALLALERTTIME,CALLANSTIME,CALLDISCONNTIME,CALLRELTIME,CALLLENGTH,DROP_P,NUMHO_OK_N,NUMHO_FAIL_N,ABIS_DROP_CAUSE,Interface,StartTime,EndTime,ServiceType,IMEI,assg_result,a_result from cdr_cc");
                AppendLog(2, "应用程序发生异常:" + Guid.NewGuid().ToString());
            }
        }
    }
}


using WPGIS.Core;
using System.Windows;
using System.Timers;

namespace WPGIS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer m_timer = null;

        public MainWindow()
        {
            InitializeComponent();

            MapToolbar.setSceneView(MySceneView);
            WindowState = WindowState.Maximized;
            WScene.getInst().initScene(MySceneView);
            TestFunc.getInst().test(MySceneView.Scene);

            m_timer = new Timer(60)
            {
                Enabled = true,
                AutoReset = true
            };
            m_timer.Elapsed += timer_Elapsed;
            m_timer.Start();
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            object lockObj = new object();
            lock(lockObj)
            {
                WScene.getInst().update();
            }            
        }

        /// <summary>
        /// 定时到点执行的事件
        /// </summary>
        /// <param name="value"></param>
        private void TimerUp(object value)
        {
            //说明场景中要素更新是线程安全的        
            WScene.getInst().update();
        }

        // Map initialization logic is contained in MapViewModel.cs
    }
}

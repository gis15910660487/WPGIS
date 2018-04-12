
using WPGIS.Core;
using System.Windows;

namespace WPGIS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            WindowState = WindowState.Maximized;
            this.DataContext = new SceneViewModel(MySceneView);
            MapToolbar.DataContext = new ToolbarViewModel(MySceneView);
        }

        // Map initialization logic is contained in MapViewModel.cs
    }
}

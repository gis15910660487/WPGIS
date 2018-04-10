
using WPGIS.Core;
using WPGIS.DataType;
using System.Windows;
using System.Windows.Controls;
using Esri.ArcGISRuntime.UI.Controls;

using ScreenPoint = System.Windows.Point;
using Esri.ArcGISRuntime.Geometry;
using System;
using System.Windows.Media;

namespace WPGIS
{
    /// <summary>
    /// 场景工具条
    /// </summary>
    public partial class UCMapToolbar : UserControl
    {
        private bool m_isSelectArrow = false;        //选择箭头状态
        private bool m_isDrawArrow = false;          //部署箭头状态
        private bool m_isCreateArrow = false;        //创建箭头状态
        private SceneView m_sceneView = null;
        private SimpleArrowDraw m_arrowDraw = null;
        private ToobarData m_toolbarData = null;

        /// <summary>
        /// 地图工具条
        /// </summary>
        public UCMapToolbar()
        {
            InitializeComponent();

            btnSelectArrow.Click += btnSelectArrow_Click;
            btnEditArrow.Click += btnEditArrow_Click;
            btnDeleteArrow.Click += btnDeleteArrow_Click;
            btnMoveArrow.Click += btnMoveArrow_Click;
            btnDrawArrow.Click += btnDrawArrow_Click;
            //btnRotateArrow.Click += btnRotateArrow_Click;
            btnStopEdit.Click += btnStopEdit_Click;
            //slider.ValueChanged += slider_ValueChanged;

            btnFillColor.Click += btnFillColor_Click;
            btnBorderColor.Click += btnBorderColor_Click;
            DrawManager.getInst().CurrentArrowChangedEvent += UCMapToolbar_CurrentArrowChangedEvent;            

            //初始化工具条数据
            m_toolbarData = new ToobarData(DrawManager.getInst());
            this.DataContext = m_toolbarData;
        }

        private void btnBorderColor_Click(object sender, RoutedEventArgs e)
        {
            SolidColorBrush curBrush = borderPath.Stroke as SolidColorBrush;
            Color borderColor = curBrush.Color;
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            colorDialog.Color = System.Drawing.Color.FromArgb(borderColor.A, borderColor.R, borderColor.G, borderColor.B);
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.SolidBrush sb = new System.Drawing.SolidBrush(colorDialog.Color);
                Color newColor = Color.FromArgb(borderColor.A, sb.Color.R, sb.Color.G, sb.Color.B);
                SolidColorBrush solidColorBrush = new SolidColorBrush(newColor);
                m_toolbarData.BorderColor = solidColorBrush;
            }
        }

        private void UCMapToolbar_CurrentArrowChangedEvent(IDrawInterface draw)
        {
            if(draw == null)
            {
                m_toolbarData.HasCurrentArrow = false;
            }
            else
            {
                m_toolbarData.HasCurrentArrow = true;
                m_toolbarData.BorderColor = new SolidColorBrush(draw.borderColor);
                m_toolbarData.FillColor = new SolidColorBrush(draw.fillColor);
                m_toolbarData.RotateAngle = draw.angleOnXY * 360 / (2 * Math.PI);
            }
        }

        private void btnFillColor_Click(object sender, RoutedEventArgs e)
        {
            SolidColorBrush curBrush = btnFillColor.Background as SolidColorBrush;
            Color fillColor = curBrush.Color;
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            colorDialog.Color = System.Drawing.Color.FromArgb(fillColor.A, fillColor.R, fillColor.G, fillColor.B);
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.SolidBrush sb = new System.Drawing.SolidBrush(colorDialog.Color);
                Color newColor = Color.FromArgb(fillColor.A, sb.Color.R, sb.Color.G, sb.Color.B);
                SolidColorBrush solidColorBrush = new SolidColorBrush(newColor);
                m_toolbarData.FillColor = solidColorBrush;
            }
        }
        
        private void btnStopEdit_Click(object sender, RoutedEventArgs e)
        {
            cancelDrawArrow();
            DrawManager.getInst().stopEdit();
        }

        private void btnRotateArrow_Click(object sender, RoutedEventArgs e)
        {
            cancelDrawArrow();
            IDrawInterface curDraw = DrawManager.getInst().getCurrentDraw();
            if (curDraw != null)
            {
                DrawManager.getInst().startRotate(curDraw);
            }
        }

        private void btnDeleteArrow_Click(object sender, RoutedEventArgs e)
        {
            cancelDrawArrow();
            IDrawInterface curDraw = DrawManager.getInst().getCurrentDraw();
            if(null == curDraw)
            {
                MessageBox.Show("请先选择一个箭头，再执行删除操作！", "提示");
                return;
            }

            MessageBoxResult result = MessageBox.Show("确定删除当前选中的箭头？", "提示", MessageBoxButton.YesNo);
            if(result == MessageBoxResult.Yes)
            {               
                DrawManager.getInst().removeDraw(curDraw);
            }
        }

        private void btnLocate_Click(object sender, RoutedEventArgs e)
        {
            cancelDrawArrow();
        }

        private void btnEditArrow_Click(object sender, RoutedEventArgs e)
        {
            IDrawInterface curDraw = DrawManager.getInst().getCurrentDraw();
            if (curDraw != null)
            {
                DrawManager.getInst().startEdit(curDraw);
            }
        }

        /// <summary>
        /// 选择箭头
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelectArrow_Click(object sender, RoutedEventArgs e)
        {
            cancelDrawArrow();
            m_isSelectArrow = true;
            m_isDrawArrow = false;
            m_isCreateArrow = false;
        }

        /// <summary>
        /// 设置三维场景指针
        /// </summary>
        /// <param name="sceneView"></param>
        public void setSceneView(SceneView sceneView)
        {
            m_sceneView = sceneView;
            m_sceneView.MouseMove += sceneView_MouseMove;
            m_sceneView.MouseLeftButtonDown += sceneView_MouseLeftButtonDown;
            m_sceneView.MouseRightButtonDown += sceneView_MouseRightButtonDown;
        }

        /// <summary>
        /// 取消标绘操作
        /// </summary>
        private void cancelDrawArrow()
        {
            if (m_isDrawArrow && m_isCreateArrow && m_arrowDraw != null)
            {
                m_isDrawArrow = false;
                m_isCreateArrow = false;

                //删除箭头
                if (m_arrowDraw != null)
                {
                    DrawManager.getInst().removeDraw(m_arrowDraw);
                    m_arrowDraw = null;
                }
            }
        }

        /// <summary>
        ///  拖动部署箭头
        /// </summary>
        private void btnDrawArrow_Click(object sender, RoutedEventArgs e)
        {
            if (m_isDrawArrow && m_isCreateArrow && m_arrowDraw != null)
            {
                return;
            }
            m_isSelectArrow = false;
            m_isCreateArrow = false;
            m_isDrawArrow = !m_isDrawArrow;
        }

        private void sceneView_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            cancelDrawArrow();
            m_isSelectArrow = false;
        }

        private void sceneView_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (m_isDrawArrow)
            {
                m_isDrawArrow = false;
                m_isCreateArrow = false;
            }

            //拾取箭头
            if(m_isSelectArrow)
            {
                ScreenPoint hintPnt = e.GetPosition(m_sceneView);
                DrawManager.getInst().pointSelectDraw(hintPnt);
            }
        }

        private void sceneView_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!m_isDrawArrow) return;

            if (!m_isCreateArrow)
            {
                m_isCreateArrow = true;
                m_arrowDraw = DrawManager.getInst().createDraw(DrawType.DrawType_SimpleArrow) as SimpleArrowDraw;
            }
            else
            {
                ScreenPoint hintPnt = e.GetPosition(m_sceneView);
                MapPoint mpnt = m_sceneView.ScreenToBaseSurface(hintPnt);
                if (mpnt != null)
                {
                    m_arrowDraw.moveTo(mpnt);
                }
            }
        }
        
        private void btnMoveArrow_Click(object sender, RoutedEventArgs e)
        {
            cancelDrawArrow();

            IDrawInterface curDraw = DrawManager.getInst().getCurrentDraw();
            if (curDraw != null)
            {
                DrawManager.getInst().startMove(curDraw);
            }
        }
    }
}

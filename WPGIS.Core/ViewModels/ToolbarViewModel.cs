
using System;
using WPGIS.DataType;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI.Controls;

using ScreenPoint = System.Windows.Point;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxButtons = System.Windows.Forms.MessageBoxButtons;


namespace WPGIS.Core
{
    /// <summary>
    /// 工具条ViewModel
    /// </summary>
    public class ToolbarViewModel : ViewModelbase
    {
        private ToobarModel m_model = null;

        private bool m_isSelectArrow = false;        //选择箭头状态
        private SceneView m_sceneView = null;

        public ToolbarViewModel(SceneView sceneView)
        {
            this.m_model = new ToobarModel();
            this.BorderColorCmd = new DelegateCommand(new Action<object>(this.borderColorClick));
            this.FillColorCmd = new DelegateCommand(new Action<object>(this.fillColorClick));
            this.StopEditCmd = new DelegateCommand(new Action<object>(this.stopEditClick));
            this.RotateArrowCmd = new DelegateCommand(new Action<object>(this.rotateArrowClick));
            this.DeleteArrowCmd = new DelegateCommand(new Action<object>(this.deleteArrowClick));
            this.EditArrowCmd = new DelegateCommand(new Action<object>(this.editArrowClick));
            this.SelectArrowCmd = new DelegateCommand(new Action<object>(this.selectArrowClick));
            this.DrawArrowCmd = new DelegateCommand(new Action<object>(this.drawArrowClick));
            this.MoveArrowCmd = new DelegateCommand(new Action<object>(this.moveArrowClick));

            DrawManager.getInst().CurrentArrowChangedEvent += toolbar_CurrentArrowChangedEvent;
            setSceneView(sceneView);
        }

        /// <summary>
        /// 设置三维场景指针
        /// </summary>
        /// <param name="sceneView"></param>
        private void setSceneView(SceneView sceneView)
        {
            m_sceneView = sceneView;
            m_sceneView.MouseMove += sceneView_MouseMove;
            m_sceneView.MouseLeftButtonDown += sceneView_MouseLeftButtonDown;
            m_sceneView.MouseRightButtonDown += sceneView_MouseRightButtonDown;
        }

        /// <summary>
        /// 边框颜色设置命令
        /// </summary>
        public DelegateCommand BorderColorCmd { get; set; }
        /// <summary>
        /// 填充颜色设置命令
        /// </summary>
        public DelegateCommand FillColorCmd { get; set; }
        /// <summary>
        /// 停止编辑命令
        /// </summary>
        public DelegateCommand StopEditCmd { get; set; }
        /// <summary>
        /// 旋转箭头命令
        /// </summary>
        public DelegateCommand RotateArrowCmd { get; set; }
        /// <summary>
        /// 删除箭头命令
        /// </summary>
        public DelegateCommand DeleteArrowCmd { get; set; }
        /// <summary>
        /// 编辑箭头命令
        /// </summary>
        public DelegateCommand EditArrowCmd { get; set; }
        /// <summary>
        /// 选择箭头命令
        /// </summary>
        public DelegateCommand SelectArrowCmd { get; set; }
        /// <summary>
        /// 部署箭头命令
        /// </summary>
        public DelegateCommand DrawArrowCmd { get; set; }
        /// <summary>
        /// 移动箭头命令
        /// </summary>
        public DelegateCommand MoveArrowCmd { get; set; }

        /// <summary>
        /// 是否有选中箭头
        /// </summary>
        public bool HasCurrentArrow
        {
            get
            {
                return m_model.HasCurrentArrow;
            }
            set
            {
                m_model.HasCurrentArrow = value;
                RaisePropertyChanged("HasCurrentArrow");
            }
        }
        /// <summary>
        /// 箭头填充色
        /// </summary>
        public SolidColorBrush FillColor
        {
            get
            {
                return m_model.FillColor;
            }
            set
            {
                m_model.FillColor = value;
                RaisePropertyChanged("FillColor");

                if (DrawManager.getInst().getCurrentDraw() != null)
                {
                    DrawManager.getInst().getCurrentDraw().fillColor = m_model.FillColor.Color;
                }
            }
        }

        /// <summary>
        /// 箭头边框色
        /// </summary>
        public SolidColorBrush BorderColor
        {
            get
            {
                return m_model.BorderColor;
            }
            set
            {
                m_model.BorderColor = value;
                RaisePropertyChanged("BorderColor");

                if (DrawManager.getInst().getCurrentDraw() != null)
                {
                    DrawManager.getInst().getCurrentDraw().borderColor = m_model.BorderColor.Color;
                }
            }
        }

        /// <summary>
        /// 箭头角度(单位度)
        /// </summary>
        public double RotateAngle
        {
            get
            {
                return m_model.RotateAngle;
            }
            set
            {
                m_model.RotateAngle = value;
                RaisePropertyChanged("RotateAngle");
                AngleString = m_model.RotateAngle.ToString("0") + "°";

                if (DrawManager.getInst().getCurrentDraw() != null)
                {
                    DrawManager.getInst().setRotateAngle(m_model.RotateAngle * 2 * Math.PI / 360);
                }
            }
        }

        /// <summary>
        /// 箭头角度字符串
        /// </summary>
        public string AngleString
        {
            get
            {
                return m_model.AngleString;
            }
            set
            {
                m_model.AngleString = value;
                RaisePropertyChanged("AngleString");
            }
        }

        private void toolbar_CurrentArrowChangedEvent(IDrawInterface draw)
        {
            if (draw == null)
            {
                HasCurrentArrow = false;
                m_isSelectArrow = true;
            }
            else
            {
                HasCurrentArrow = true;
                m_isSelectArrow = false;
                BorderColor = new SolidColorBrush(draw.borderColor);
                FillColor = new SolidColorBrush(draw.fillColor);
                RotateAngle = draw.angleOnXY * 360 / (2 * Math.PI);
            }
        }

        private void borderColorClick(object obj)
        {
            Color borderColor = BorderColor.Color;
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = System.Drawing.Color.FromArgb(borderColor.A, borderColor.R, borderColor.G, borderColor.B);
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                System.Drawing.SolidBrush sb = new System.Drawing.SolidBrush(colorDialog.Color);
                Color newColor = Color.FromArgb(borderColor.A, sb.Color.R, sb.Color.G, sb.Color.B);
                SolidColorBrush solidColorBrush = new SolidColorBrush(newColor);
                BorderColor = solidColorBrush;
            }
        }

        private void fillColorClick(object obj)
        {
            Color fillColor = FillColor.Color;
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = System.Drawing.Color.FromArgb(fillColor.A, fillColor.R, fillColor.G, fillColor.B);
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.SolidBrush sb = new System.Drawing.SolidBrush(colorDialog.Color);
                Color newColor = Color.FromArgb(fillColor.A, sb.Color.R, sb.Color.G, sb.Color.B);
                SolidColorBrush solidColorBrush = new SolidColorBrush(newColor);
                FillColor = solidColorBrush;
            }
        }

        private void stopEditClick(object obj)
        {
            cancelDrawArrow();
            DrawManager.getInst().stopEdit();
        }

        private void rotateArrowClick(object obj)
        {
            cancelDrawArrow();
            IDrawInterface curDraw = DrawManager.getInst().getCurrentDraw();
            if (curDraw != null)
            {
                DrawManager.getInst().startRotate(curDraw);
            }
        }

        private void deleteArrowClick(object obj)
        {
            cancelDrawArrow();
            IDrawInterface curDraw = DrawManager.getInst().getCurrentDraw();
            if (null == curDraw)
            {
                MessageBox.Show("请先选择一个箭头，再执行删除操作！", "提示");
                return;
            }

            DialogResult result = MessageBox.Show("确定删除当前选中的箭头？", "提示", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                DrawManager.getInst().removeDraw(curDraw);
            }
        }

        private void editArrowClick(object obj)
        {
            IDrawInterface curDraw = DrawManager.getInst().getCurrentDraw();
            if (curDraw != null)
            {
                DrawManager.getInst().startEdit(curDraw);
            }
        }

        private void selectArrowClick(object obj)
        {
            cancelDrawArrow();
            m_isSelectArrow = true;
        }

        private void drawArrowClick(object obj)
        {
            if (DrawManager.getInst().getCurrentEditMode() == Edit_Type.Edit_Create)
            {
                return;
            }
            m_isSelectArrow = false;
            //需优化，模型（箭头）
            DrawManager.getInst().createDraw(DrawType.DrawType_SimpleArrow);
        }

        private void moveArrowClick(object obj)
        {
            cancelDrawArrow();

            IDrawInterface curDraw = DrawManager.getInst().getCurrentDraw();
            if (curDraw != null)
            {
                DrawManager.getInst().startMove(curDraw);
            }
        }

        private void sceneView_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            cancelDrawArrow();
            m_isSelectArrow = false;
        }

        private void sceneView_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //拾取箭头
            if (m_isSelectArrow)
            {
                ScreenPoint hintPnt = e.GetPosition(m_sceneView);
                DrawManager.getInst().pointSelectDraw(hintPnt);
            }

            if(DrawManager.getInst().getCurrentEditMode() == Edit_Type.Edit_Create)
            {
                DrawManager.getInst().stopEdit();
            }
        }

        private void sceneView_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {    
            if(DrawManager.getInst().getCurrentEditMode() == Edit_Type.Edit_Create && DrawManager.getInst().getCurrentDraw() != null)
            {
                ScreenPoint hintPnt = e.GetPosition(m_sceneView);
                MapPoint mpnt = m_sceneView.ScreenToBaseSurface(hintPnt);
                if (mpnt != null)
                {
                    DrawManager.getInst().getCurrentDraw().moveTo(mpnt);
                }
            }
        }

        /// <summary>
        /// 取消标绘操作
        /// </summary>
        private void cancelDrawArrow()
        {
            if (DrawManager.getInst().getCurrentEditMode() == Edit_Type.Edit_Create)
            {
                DrawManager.getInst().removeCurrentDraw();
            }
        }
    }
}

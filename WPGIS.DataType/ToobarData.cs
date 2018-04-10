
using System.Windows.Media;
using System.ComponentModel;

namespace WPGIS.DataType
{
    /// <summary>
    /// 工具条数据
    /// </summary>
    public class ToobarData : INotifyPropertyChanged
    {
        //标绘管理器
        private IDrawMangerInterface m_drawManager = null;

        public ToobarData(IDrawMangerInterface drawManager)
        {
            m_drawManager = drawManager;
        }

        private bool m_hasCurrentArrow = false;
        /// <summary>
        /// 是否有选中箭头
        /// </summary>
        public bool HasCurrentArrow
        {
            get
            {
                return m_hasCurrentArrow;
            }
            set
            {
                m_hasCurrentArrow = value;
                if(PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("HasCurrentArrow"));
                }
            }
        }
        
        private SolidColorBrush m_fillBrush = new SolidColorBrush(Color.FromArgb(160, 255, 0, 0));
        /// <summary>
        /// 箭头填充色
        /// </summary>
        public SolidColorBrush FillColor
        {
            get
            {
                return m_fillBrush;
            }
            set
            {
                m_fillBrush = value;               
                if (PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("FillColor"));
                }

                if(m_drawManager != null && m_drawManager.getCurrentDraw() != null)
                {
                    m_drawManager.getCurrentDraw().fillColor = m_fillBrush.Color;
                }
            }
        }
        
        private SolidColorBrush m_borderColor = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
        /// <summary>
        /// 箭头边框色
        /// </summary>
        public SolidColorBrush BorderColor
        {
            get
            {
                return m_borderColor;
            }
            set
            {
                m_borderColor = value;
                if (PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("BorderColor"));
                }

                if (m_drawManager != null && m_drawManager.getCurrentDraw() != null)
                {
                    m_drawManager.getCurrentDraw().borderColor = m_borderColor.Color;
                }
            }
        }

        private double m_rotateAngle = 0.0;
        /// <summary>
        /// 箭头角度(单位度)
        /// </summary>
        public double RotateAngle
        {
            get
            {
                return m_rotateAngle;
            }
            set
            {
                m_rotateAngle = value;
                if (PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RotateAngle"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

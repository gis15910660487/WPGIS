
using System.Windows.Media;

namespace WPGIS.DataType
{
    /// <summary>
    /// 工具条数据
    /// </summary>
    public class ToobarModel
    {
        public ToobarModel()
        {
        }
        /// <summary>
        /// 是否有选中箭头
        /// </summary>
        public bool HasCurrentArrow
        {
            get;
            set;
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
            }
        }

        /// <summary>
        /// 箭头角度字符串
        /// </summary>
        public string AngleString { get; set; }
    }
}

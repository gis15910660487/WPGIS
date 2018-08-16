using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using WPGIS.DataType;

namespace WPGIS.Core
{
    public abstract class SimpleDrawBase : IDrawInterface
    {
        protected int m_id = 0;
        protected string m_name = "";
        protected bool m_isSelected = false;
        protected Graphic m_graphic = null;
        protected GraphicsOverlay m_gpOverlay = null;
        protected SceneView m_sceneView = null;
        protected SimpleFillSymbol m_fillSymbol = null;
        //编辑模式
        protected Edit_Type m_editType = Edit_Type.Edit_None;
        protected MapPoint m_pos = new MapPoint(0.0, 0.0, 0.0, SpatialReferences.Wgs84);

        protected Color m_fillColor = Color.FromArgb(160, 255, 0, 0);
        protected Color m_borderColor = Color.FromArgb(180, 0, 255, 0);
        protected Color m_selectedColor = Color.FromArgb(255, 0, 255, 255);
        protected int m_defaultBorderSize = 1;
        protected int m_focusBorderSize = 3;

        //箭头xy平面旋转角度（弧度）             
        protected double m_rotOnXY = 0.0;

        public virtual event SelectCtrlPointEventHandler SelectCtrlPointEvent;

        #region 公共属性

        public int ID
        {
            get
            {
                return m_id;
            }

            set
            {
                m_id = value;
            }
        }

        public string name
        {
            get
            {
                return m_name;
            }

            set
            {
                m_name = value;
            }
        }

        public Graphic graphic
        {
            get
            {
                return m_graphic;
            }
        }

        public bool visible
        {
            get
            {
                return m_graphic.IsVisible;
            }
            set
            {
                m_graphic.IsVisible = value;
            }
        }

        /// <summary>
        /// 返回编辑模式
        /// </summary>
        public Edit_Type editType
        {
            get
            {
                return m_editType;
            }
        }

        /// <summary>
        /// 场景位置
        /// </summary>
        public MapPoint mapPosition
        {
            get
            {
                return m_pos;
            }
        }

        public bool selected
        {
            get
            {
                return m_isSelected;
            }
            set
            {
                m_isSelected = value;
                if (m_graphic != null) m_graphic.IsSelected = m_isSelected;
                if(m_fillSymbol != null)
                {
                    m_fillSymbol.Outline.Color = m_isSelected ? m_selectedColor : m_borderColor;
                    m_fillSymbol.Outline.Width = m_isSelected ? m_focusBorderSize : m_defaultBorderSize;
                }
            }
        }
        /// <summary>
        /// 填充颜色
        /// </summary>
        public Color fillColor
        {
            get
            {
                return m_fillColor;
            }
            set
            {
                m_fillColor = value;
                if(m_fillSymbol != null)
                {
                    m_fillSymbol.Color = m_fillColor;
                }                
            }
        }
        /// <summary>
        /// 边框颜色
        /// </summary>
        public Color borderColor
        {
            get
            {
                return m_borderColor;
            }
            set
            {
                m_borderColor = value;
                if(m_fillSymbol != null)
                {
                    m_fillSymbol.Outline.Color = m_borderColor;
                }               
            }
        }
        /// <summary>
        /// xy平面旋转角度
        /// </summary>
        public double angleOnXY
        {
            get
            {
                return m_rotOnXY;
            }
            set
            {
                m_rotOnXY = value;
                rotateOnXY(m_rotOnXY, true);                
            }
        }

        #endregion

        public SimpleDrawBase(SceneView sceneView)
        {
            m_sceneView = sceneView;
            m_gpOverlay = new GraphicsOverlay();
            m_gpOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Draped;
            m_sceneView.GraphicsOverlays.Add(m_gpOverlay);
        }

        public virtual void Dispose()
        {            
        }

        public virtual void doEdit(MapPoint pnt)
        {            
        }

        public virtual void endEdit()
        {
        }

        public virtual void endRotate()
        {
        }

        public virtual void initGraphic()
        {
        }

        public virtual void moveTo(MapPoint pnt)
        {
        }

        public virtual void rotateOnXY(double angle, bool focusRefresh)
        {
            m_rotOnXY = angle;
            if (m_rotOnXY >= 2 * Math.PI)
            {
                m_rotOnXY = m_rotOnXY - 2 * Math.PI;
            }
            else if (m_rotOnXY < 0.0)
            {
                m_rotOnXY = 2 * Math.PI - m_rotOnXY;
            }
        }

        public virtual void startEdit()
        {
        }

        public virtual void startRotate()
        {
        }

        /// <summary>
        /// 开启移动模式
        /// </summary>
        public virtual void startMove()
        {
            m_editType = Edit_Type.Edit_Transfer;
            selected = true;
        }

        /// <summary>
        /// 结束移动模式
        /// </summary>
        public virtual void endMove()
        {
            m_editType = Edit_Type.Edit_None;
            selected = false;
        }

        /// <summary>
        /// 停止
        /// </summary>
        public virtual void stopAll()
        {
            if (m_editType == Edit_Type.Edit_Geometry)
            {
                endEdit();
            }
            else if (m_editType == Edit_Type.Edit_Transfer)
            {
                endMove();
            }
            else if (m_editType == Edit_Type.Edit_Rotate)
            {
                endRotate();
            }
        }
    }
}

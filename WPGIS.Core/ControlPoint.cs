
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;

using System;
using System.Windows;
using System.Collections.Generic;

using Mathlib;
using WPGIS.DataType;
using Color = System.Windows.Media.Color;
using Esri.ArcGISRuntime.Data;
using System.Threading.Tasks;

namespace WPGIS.Core
{
    /// <summary>
    /// 控制点
    /// </summary>
    public class ControlPoint : IControlPoint
    {
        private Graphic m_graphic = null;
        private GraphicsOverlay m_gpOverlay = null;

        internal ControlPoint(GraphicsOverlay gpOver)
        {
            m_gpOverlay = gpOver;
            initGraphic();
        }

        /// <summary>
        /// 初始化球
        /// </summary>
        private void initGraphic()
        {
            if (null == m_gpOverlay) return;
            //初始化球
            var spereSymbol = new SimpleMarkerSceneSymbol
            {
                Style = SimpleMarkerSceneSymbolStyle.Sphere,
                Color = Color.FromArgb(255, 238, 199, 16),
                Height = 50,
                Width = 50,
                Depth = 50,
                AnchorPosition = SceneSymbolAnchorPosition.Center
            };
            var location = new MapPoint(0, 0, 0, SpatialReferences.Wgs84);
            m_graphic = new Graphic(location, spereSymbol);
            m_graphic.IsVisible = false;
            m_gpOverlay.Graphics.Add(m_graphic);
        }

        /// <summary>
        /// 可见性
        /// </summary>
        public bool visible
        {
            get { return m_graphic.IsVisible; }
            set { m_graphic.IsVisible = value; }
        }

        /// <summary>
        /// 地理位置
        /// </summary>
        public MapPoint mapPosition
        {
            get
            {
                return m_graphic.Geometry as MapPoint;
            }
            set
            {
                m_graphic.Geometry = value;
            }
        }

        /// <summary>
        /// 获取xy二维向量
        /// </summary>
        /// <returns></returns>
        public Vector2D getXY()
        {
            MapPoint mpos = m_graphic.Geometry as MapPoint;
            return new Vector2D(mpos.X, mpos.Y);
        }

        public bool isGraphic(Graphic pGraphic)
        {
            return m_graphic == pGraphic;
        }
    }

    /// <summary>
    /// 控制点管理器
    /// </summary>
    public class ControlPointManager
    {
        private SceneView m_sceneView = null;
        private GraphicsOverlay m_gpOverlay = null;
        private IList<IControlPoint> m_controlPointList = new List<IControlPoint>();

        public ControlPointManager()
        {
        }

        /// <summary>
        /// 初始化管理器
        /// </summary>
        /// <param name="sceneView"></param>
        public void initialize(SceneView sceneView)
        {
            m_sceneView = sceneView;
            m_gpOverlay = new GraphicsOverlay();
            m_gpOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Draped;
            m_sceneView.GraphicsOverlays.Add(m_gpOverlay);
        }

        public void clear()
        {
            m_gpOverlay.Graphics.Clear();
            //删除会引起bug，why？
            //m_sceneView.GraphicsOverlays.Remove(m_gpOverlay);
            // m_gpOverlay = null;
        }

        /// <summary>
        /// 创建控制点
        /// </summary>
        /// <returns></returns>
        public IControlPoint createControlPoint(MapPoint pos)
        {
            IControlPoint ctrlPoint = new ControlPoint(m_gpOverlay);
            ctrlPoint.mapPosition = pos;
            m_controlPointList.Add(ctrlPoint);
            return ctrlPoint;
        }

        /// <summary>
        /// 返回控制个数
        /// </summary>
        /// <returns></returns>
        public int getSize()
        {
            return m_controlPointList.Count;
        }

        /// <summary>
        /// 返回控制点
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IControlPoint getControlPoint(int index)
        {
            if (index < 0 || index >= m_controlPointList.Count)
            {
                return null;
            }

            IControlPoint tCtrolPnt = m_controlPointList[index];
            return tCtrolPnt;
        }

        /// <summary>
        /// 识别控制点
        /// </summary>
        /// <param name="screenPoint">屏幕坐标</param>
        /// <returns>控制点</returns>
        public async Task<IControlPoint> identifyControlPoint(Point snPoint)
        {
            IControlPoint retCtrlPnt = null;

            var tolerance = 5d;
            var maximumResults = 3;
            var onlyReturnPopups = false;

            //选中坐标轴
            IdentifyGraphicsOverlayResult identifyResults = await m_sceneView.IdentifyGraphicsOverlayAsync(
                m_gpOverlay,
                snPoint,
                tolerance,
                onlyReturnPopups,
                maximumResults);

            if (identifyResults.Graphics.Count == 1)
            {
                Graphic pGraphic = identifyResults.Graphics[0];
                foreach (var ctrlPnt in m_controlPointList)
                {
                    if(ctrlPnt.isGraphic(pGraphic))
                    {
                        retCtrlPnt = ctrlPnt;
                        break;
                    }
                }
            }

            return retCtrlPnt;
        }
    }
}

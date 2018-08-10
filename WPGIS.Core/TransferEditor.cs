﻿
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;

using Mathlib;
using System;
using System.Windows.Media;

using WPGIS.DataType;
using ScreenPoint = System.Windows.Point;
using PointCollection = Esri.ArcGISRuntime.Geometry.PointCollection;

namespace WPGIS.Core
{
    /// <summary>
    /// 移动辅助编辑器
    /// </summary>
    public class TransferEditor
    {
        private Color m_spereColor;
        private Color m_xAxisColor;
        private Color m_yAxisColor;
        private Color m_zAxisColor;
        private Color m_activeColor;
        private Color m_focusColor;

        bool m_isVisible = false;

        public event MapPointChangedEventHandler MapPointChangedEvent = null;
        //编辑器位置
        private MapPoint m_pos = new MapPoint(0.0, 0.0, 0.0, SpatialReferences.Wgs84);
        //编辑器xy平面旋转角度（弧度）             
        private double m_rotOnXY = 0.0;
        //编辑器放大系数(默认1)
        private float m_scale = 1.0f;
        //初始化size(默认300)
        private float m_initSize = 300.0f;
        //默认离地高度
        private float m_relativeHeight = 30.0f;
        //场景view               
        private SceneView m_sceneView = null;
        //编辑存储的要素层
        private GraphicsOverlay m_gpOverlay = null;
        private GraphicsOverlay m_gpOverlay1 = null;
        //位置球
        private Graphic m_spereGraphic = null;
        //x轴
        private Graphic m_xAxisGraphic = null;
        //y轴
        private Graphic m_yAxisGraphic = null;
        //z轴
        private Graphic m_zAxisGraphic = null;
        //x轴头部
        private Graphic m_xAxiMarkGraphic = null;
        //y轴头部
        private Graphic m_yAxiMarkGraphic = null;
        //z轴头部
        private Graphic m_zAxiMarkGraphic = null;
        //位置球渲染符号
        private SimpleMarkerSceneSymbol m_spereSymbol = null;
        //x轴头部渲染符号
        private SimpleMarkerSceneSymbol m_xAxisMarkSymbol = null;
        //y轴头部渲染符号
        private SimpleMarkerSceneSymbol m_yAxisMarkSymbol = null;
        //z轴头部渲染符号
        private SimpleMarkerSceneSymbol m_zAxisMarkSymbol = null;
        //x轴渲染符号
        private SimpleLineSymbol m_xAxisSymbol = null;
        //y轴渲染符号
        private SimpleLineSymbol m_yAxisSymbol = null;
        //z轴渲染符号
        private SimpleLineSymbol m_zAxisSymbol = null;
        //当前选中坐标轴类型
        private Axis_Type m_currentAxisType = Axis_Type.Axis_None;
        //移动开始位置
        private ScreenPoint m_moveBeginPoint;
        //移动增量
        Vector3D m_moveDelta;

        public TransferEditor(SceneView sceView)
        {
            m_sceneView = sceView;
            m_spereColor = Color.FromArgb(128, 135, 135, 135);
            m_xAxisColor = Color.FromArgb(255, 255, 0, 0);
            m_yAxisColor = Color.FromArgb(255, 0, 255, 0);
            m_zAxisColor = Color.FromArgb(255, 0, 0, 255);
            m_activeColor = Color.FromArgb(128, 255, 255, 0);
            m_focusColor = Color.FromArgb(200, 255, 255, 0);

            initEditor();
            visible = false;

            m_sceneView.MouseLeftButtonDown += sceneView_MouseLeftButtonDown;
            m_sceneView.MouseLeftButtonUp += sceneView_MouseLeftButtonUp;
            m_sceneView.PreviewMouseMove += sceneView_MouseMove;
            m_sceneView.NavigationCompleted += sceneView_NavigationCompleted;
        }

        private void sceneView_NavigationCompleted(object sender, EventArgs e)
        {
            var camera = m_sceneView.Camera;
            var horDist = GeometryEngine.DistanceGeodetic(camera.Location, m_pos, LinearUnits.Meters, AngularUnits.Degrees, GeodeticCurveType.Geodesic);
            var dist = Math.Sqrt(Math.Pow(horDist.Distance, 2) + Math.Pow(camera.Location.Z - m_pos.Z, 2));
            m_scale = (float)dist / 10000;
            if (m_scale < 1.0f) m_scale = 1.0f;
            refreshGeometry(m_pos, m_scale, m_rotOnXY);
        }

        /// <summary>
        /// 可见性
        /// </summary>
        public bool visible
        {
            get
            {
                return m_isVisible;
            }
            set
            {
                if (value == m_isVisible) return;
                m_isVisible = value;
                m_spereGraphic.IsVisible = m_isVisible;
                m_xAxisGraphic.IsVisible = m_isVisible;
                m_yAxisGraphic.IsVisible = m_isVisible;
                m_zAxisGraphic.IsVisible = m_isVisible;
                m_xAxiMarkGraphic.IsVisible = m_isVisible;
                m_yAxiMarkGraphic.IsVisible = m_isVisible;
                m_zAxiMarkGraphic.IsVisible = m_isVisible;
            }
        }
        /// <summary>
        /// 初始化编辑器
        /// </summary>
        public void initEditor()
        {
            m_gpOverlay = new GraphicsOverlay();
            m_gpOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
            m_sceneView.GraphicsOverlays.Add(m_gpOverlay);
            m_gpOverlay1 = new GraphicsOverlay();
            m_gpOverlay1.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
            m_sceneView.GraphicsOverlays.Add(m_gpOverlay1);

            //初始化球
            m_spereSymbol = new SimpleMarkerSceneSymbol
            {
                Style = SimpleMarkerSceneSymbolStyle.Cube,
                Color = m_spereColor,
                Height = m_initSize / 5,
                Width = m_initSize / 5,
                Depth = m_initSize / 5,
                AnchorPosition = SceneSymbolAnchorPosition.Center
            };
            var location = new MapPoint(0, 0, m_relativeHeight, SpatialReferences.Wgs84);
            m_spereGraphic = new Graphic(location, m_spereSymbol);
            m_gpOverlay.Graphics.Add(m_spereGraphic);

            m_xAxisSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, m_xAxisColor, 2);
            m_yAxisSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, m_yAxisColor, 2);
            m_zAxisSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, m_zAxisColor, 2);
            m_xAxisSymbol.MarkerStyle = SimpleLineSymbolMarkerStyle.Arrow;
            m_xAxisSymbol.MarkerPlacement = SimpleLineSymbolMarkerPlacement.End;

            double agreeScale = CommonUtil.getInst().meter2degree(m_initSize);
            //初始化x轴            
            PointCollection pointsX = new PointCollection(SpatialReferences.Wgs84)
                {
                    new MapPoint(0, 0, m_relativeHeight),
                    new MapPoint(agreeScale, 0, m_relativeHeight),
                };
            Polyline polylineXAxis = new Polyline(pointsX);
            m_xAxisGraphic = new Graphic(polylineXAxis, m_xAxisSymbol);
            m_gpOverlay.Graphics.Add(m_xAxisGraphic);

            //初始化x轴头部箭头
            m_xAxisMarkSymbol = new SimpleMarkerSceneSymbol
            {
                Style = SimpleMarkerSceneSymbolStyle.Diamond,
                Color = m_xAxisColor,
                Height = 10,
                Width = 10,
                Depth = 10,
                AnchorPosition = SceneSymbolAnchorPosition.Bottom
            };
            m_xAxiMarkGraphic = new Graphic(new MapPoint(agreeScale, 0, m_relativeHeight), m_xAxisMarkSymbol);
            m_gpOverlay1.Graphics.Add(m_xAxiMarkGraphic);

            //初始化y轴            
            PointCollection pointsY = new PointCollection(SpatialReferences.Wgs84)
                {
                    new MapPoint(0, 0, m_relativeHeight, SpatialReferences.Wgs84),
                    new MapPoint(0, agreeScale, m_relativeHeight, SpatialReferences.Wgs84),
                };
            Polyline polylineYAxis = new Polyline(pointsY);
            m_yAxisGraphic = new Graphic(polylineYAxis, m_yAxisSymbol);
            m_gpOverlay.Graphics.Add(m_yAxisGraphic);

            //初始化y轴头部箭头
            m_yAxisMarkSymbol = new SimpleMarkerSceneSymbol
            {
                Style = SimpleMarkerSceneSymbolStyle.Diamond,
                Color = m_yAxisColor,
                Height = 10,
                Width = 10,
                Depth = 10,
                AnchorPosition = SceneSymbolAnchorPosition.Center
            };
            m_yAxiMarkGraphic = new Graphic(new MapPoint(0, agreeScale, m_relativeHeight), m_yAxisMarkSymbol);
            m_gpOverlay1.Graphics.Add(m_yAxiMarkGraphic);

            //初始化z轴            
            PointCollection pointsZ = new PointCollection(SpatialReferences.Wgs84)
                {
                    new MapPoint(0, 0, m_relativeHeight),
                    new MapPoint(0, 0, m_initSize + m_relativeHeight),
                };
            Polyline polylineZAxis = new Polyline(pointsZ);
            m_zAxisGraphic = new Graphic(polylineZAxis, m_zAxisSymbol);
            m_gpOverlay1.Graphics.Add(m_zAxisGraphic);

            //初始化z轴头部箭头
            m_zAxisMarkSymbol = new SimpleMarkerSceneSymbol
            {
                Style = SimpleMarkerSceneSymbolStyle.Diamond,
                Color = m_zAxisColor,
                Height = 10,
                Width = 10,
                Depth = 10,
                AnchorPosition = SceneSymbolAnchorPosition.Center
            };
            m_zAxiMarkGraphic = new Graphic(new MapPoint(0, 0, m_initSize + m_relativeHeight), m_zAxisMarkSymbol);
            m_gpOverlay1.Graphics.Add(m_zAxiMarkGraphic);
        }
        /// <summary>
        /// 设置位置
        /// </summary>
        /// <param name="pos">地图位置点</param>
        public void setPosition(MapPoint pos)
        {
            if (m_pos.IsEqual(pos)) return;
            m_pos = pos;
            refreshGeometry(m_pos, m_scale, m_rotOnXY);            
        }
        /// <summary>
        /// 返回编辑器的地图位置
        /// </summary>
        /// <returns>地图位置</returns>
        public MapPoint getPosition()
        {
            return m_pos;
        }
        /// <summary>
        /// 设置xy平面的偏转
        /// </summary>
        /// <param name="angle">偏转角度（弧度）</param>
        public void setRotOnXY(double angle)
        {
            if (Math.Abs(m_rotOnXY - angle) < 0.0001) return;
            m_rotOnXY = angle;
            refreshGeometry(m_pos, m_scale, m_rotOnXY);
        }
        /// <summary>
        /// 返回xy平面的偏转
        /// </summary>
        /// <returns></returns>
        public double getRotOnXY()
        {
            return m_rotOnXY;
        }
        private void refreshGeometry(MapPoint pos, float scale, double angle)
        {
            MapPoint renderPos = new MapPoint(pos.X, pos.Y, m_relativeHeight, pos.SpatialReference);

            //先在原点位置计算放大后各轴终点的坐标
            double newSize = CommonUtil.getInst().meter2degree(m_initSize * m_scale);
            Vector3D xAxisPointEnd = new Vector3D(newSize, 0, 0);
            Vector3D yAxisPointEnd = new Vector3D(0, newSize, 0);
            Vector3D zAxisPointEnd = new Vector3D(0, 0, m_initSize * m_scale);
            //然后继续在原点位置计算各轴终点旋转后的坐标
            if (m_rotOnXY > 0)
            {
                xAxisPointEnd = CommonUtil.getInst().RotateAroundZAxis(xAxisPointEnd, m_rotOnXY);
                yAxisPointEnd = CommonUtil.getInst().RotateAroundZAxis(yAxisPointEnd, m_rotOnXY);
            }

            //平移计算
            var xAxisEndMapPoint = new MapPoint(renderPos.X + xAxisPointEnd.X, renderPos.Y + xAxisPointEnd.Y, renderPos.Z + xAxisPointEnd.Z, renderPos.SpatialReference);
            var yAxisEndMapPoint = new MapPoint(renderPos.X + yAxisPointEnd.X, renderPos.Y + yAxisPointEnd.Y, renderPos.Z + yAxisPointEnd.Z, renderPos.SpatialReference);
            var zAxisEndMapPoint = new MapPoint(renderPos.X + zAxisPointEnd.X, renderPos.Y + zAxisPointEnd.Y, renderPos.Z + zAxisPointEnd.Z, renderPos.SpatialReference);

            m_spereGraphic.Geometry = renderPos;

            m_xAxiMarkGraphic.Geometry = xAxisEndMapPoint;
            m_xAxisGraphic.Geometry = new Polyline(new PointCollection(renderPos.SpatialReference) { renderPos, xAxisEndMapPoint });

            m_yAxiMarkGraphic.Geometry = yAxisEndMapPoint;
            m_yAxisGraphic.Geometry = new Polyline(new PointCollection(renderPos.SpatialReference) { renderPos, yAxisEndMapPoint });

            m_zAxiMarkGraphic.Geometry = zAxisEndMapPoint;
            m_zAxisGraphic.Geometry = new Polyline(new PointCollection(renderPos.SpatialReference) { renderPos, zAxisEndMapPoint });
        }
        private void resetAxisColor()
        {
            m_xAxisSymbol.Color = m_xAxisColor;
            m_yAxisSymbol.Color = m_yAxisColor;
            m_zAxisSymbol.Color = m_zAxisColor;
            m_xAxisMarkSymbol.Color = m_xAxisColor;
            m_yAxisMarkSymbol.Color = m_yAxisColor;
            m_zAxisMarkSymbol.Color = m_zAxisColor;
        }
        private async void activeAxis(ScreenPoint screenPnt)
        {
            resetAxisColor();

            var tolerance = 10d;
            var maximumResults = 3;
            var onlyReturnPopups = false;

            //选中坐标轴
            IdentifyGraphicsOverlayResult identifyResults = await m_sceneView.IdentifyGraphicsOverlayAsync(
                m_gpOverlay,
                screenPnt,
                tolerance,
                onlyReturnPopups,
                maximumResults);

            if (identifyResults.Graphics.Count == 1)
            {
                Graphic pGraphic = identifyResults.Graphics[0];
                if (pGraphic == m_xAxisGraphic)
                {
                    m_xAxisSymbol.Color = m_activeColor;
                    m_xAxisMarkSymbol.Color = m_activeColor;
                }
                else if (pGraphic == m_yAxisGraphic)
                {
                    m_yAxisSymbol.Color = m_activeColor;
                    m_yAxisMarkSymbol.Color = m_activeColor;
                }
                else if (pGraphic == m_zAxisGraphic)
                {
                    m_zAxisSymbol.Color = m_activeColor;
                    m_zAxisMarkSymbol.Color = m_activeColor;
                }
            }
        }
        
        private void moveTo(MapPoint hintMapPnt)
        {
            //更新位置
            m_pos = hintMapPnt;
            //重绘
            refreshGeometry(m_pos, m_scale, m_rotOnXY);
            //触发位置改变事件
            MapPointChangedEvent?.Invoke(m_pos);
        }

        private void sceneView_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!m_isVisible || m_sceneView == null) return;
            if (m_currentAxisType == Axis_Type.Axis_None)
            {
                //激活坐标轴
                activeAxis(e.GetPosition(m_sceneView));
            }
            else
            {
                //移动坐标轴
                ScreenPoint hintPnt = e.GetPosition(m_sceneView);
                MapPoint mPnt = m_sceneView.ScreenToBaseSurface(hintPnt);
                if (mPnt != null)
                {
                    moveTo(mPnt);
                }
            }
        }
        private void sceneView_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (!m_isVisible || m_sceneView == null) return;
                if (m_currentAxisType == Axis_Type.Axis_None) return;

                //恢复选中坐标轴的颜色
                resetAxisColor();
            }
            catch (Exception)
            {
            }
            finally
            {
                //恢复相机使用
                m_sceneView.InteractionOptions.IsEnabled = true;
                m_currentAxisType = Axis_Type.Axis_None;
            }
        }
        private async void sceneView_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!m_isVisible || m_sceneView == null) return;

            var tolerance = 10d;
            var maximumResults = 3;
            var onlyReturnPopups = false;
            ScreenPoint hintPnt = e.GetPosition(m_sceneView);

            //选中坐标轴
            IdentifyGraphicsOverlayResult identifyResults = await m_sceneView.IdentifyGraphicsOverlayAsync(
                m_gpOverlay,
                hintPnt,
                tolerance,
                onlyReturnPopups,
                maximumResults);

            if (identifyResults.Graphics.Count == 1)
            {
                //停止鼠标对三维场景的控制
                m_sceneView.InteractionOptions.IsEnabled = false;

                m_moveBeginPoint = hintPnt;
                Graphic pGraphic = identifyResults.Graphics[0];
                if (pGraphic == m_xAxisGraphic)
                {
                    m_xAxisSymbol.Color = m_focusColor;
                    m_xAxisMarkSymbol.Color = m_focusColor;
                    m_currentAxisType = Axis_Type.Axis_X;
                }
                else if (pGraphic == m_yAxisGraphic)
                {
                    m_yAxisSymbol.Color = m_focusColor;
                    m_yAxisMarkSymbol.Color = m_focusColor;
                    m_currentAxisType = Axis_Type.Axis_Y;
                }
                else if (pGraphic == m_zAxisGraphic)
                {
                    m_zAxisSymbol.Color = m_focusColor;
                    m_zAxisMarkSymbol.Color = m_focusColor;
                    m_currentAxisType = Axis_Type.Axis_Z;
                }
            }
        }
    }
}

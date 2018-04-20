
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
using WPGIS.Log;

namespace WPGIS.Core
{
    /// <summary>
    /// 旋转辅助编辑器
    /// </summary>
    public class RotateEditor
    {
        private Color m_xyCircleColor;              //xy平面圆环颜色
        private Color m_xAxisColor;
        private Color m_yAxisColor;
        private Color m_zAxisColor;
        private Color m_activeColor;                //圆环激活颜色
        private Color m_focusColor;                 //圆环focus颜色
        private Color m_rotatePolyColor;            //旋转多边形颜色
        private Color m_rotatePolyBorderColor;      //旋转多边形边框颜色

        private bool m_isVisible = false;
        //更新类型：0（无需更新）1（更新位置）2（更新角度）
        private int m_refreshType = 0;
        //圆环细分点个数
        private const int m_subPointCount = 101;

        //旋转改变事件
        public event RotateChangedEventHandler RotateChangedEvent = null;
        //编辑器位置
        private MapPoint m_pos = new MapPoint(0.0, 0.0, 0.0, SpatialReferences.Wgs84);
        private MapPoint m_oldPos = null;
        //编辑器放大系数(默认100)
        private float m_scale = 100.0f;
        //场景view               
        private SceneView m_sceneView = null;
        //编辑存储的要素层
        private GraphicsOverlay m_gpOverlayLine = null;
        private GraphicsOverlay m_gpOverlayFill = null;
        private GraphicsOverlay m_gpOverlay = null;
        //xy平面圆环
        private Graphic m_xyCircleGraphic = null;
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

        //旋转多边形
        private Graphic m_rotatePolygonGraphic = null;
        //xy平面渲染符号
        private SimpleLineSymbol m_xyCircleSymbol = null;
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
        //旋转多边形符号
        private SimpleFillSymbol m_rotatePolygonSymbol = null;
        //当前旋转类型
        private Rotate_Type m_rotateType = Rotate_Type.Rotate_None;
        private GlobeCameraController m_globeCameraControl = null;
        private OrbitGeoElementCameraController m_orbitCameraController = null;

        //剖分角度
        private double m_subAngleDelta = 0.0;
        //圆环半径（弧度）
        private double m_dCircleRadius = 0.0;
        //旋转开始角度
        private double m_rotateBeginAngle = 0.0;
        //上一帧角度（不准确）
        private double m_angleDetltaTotal = 0.0;
        //一次旋转角度偏移(有正负)
        private double m_angleDetltaOneRotate = 0.0;
        //上一帧屏幕位置
        private ScreenPoint m_preScreenPoint;

        public RotateEditor(SceneView sceView)
        {
            m_sceneView = sceView;
            m_rotatePolyColor = Color.FromArgb(128, 255, 255, 0);
            m_rotatePolyBorderColor = Color.FromArgb(200, 255, 255, 0);
            m_xyCircleColor = Color.FromArgb(255, 255, 0, 0);
            m_xAxisColor = Color.FromArgb(255, 255, 0, 0);
            m_yAxisColor = Color.FromArgb(255, 0, 255, 0);
            m_zAxisColor = Color.FromArgb(255, 0, 0, 255);
            m_activeColor = Color.FromArgb(128, 255, 255, 0);
            m_focusColor = Color.FromArgb(200, 255, 255, 0);

            initEditor();
            setVisible(false);

            m_globeCameraControl = m_sceneView.CameraController as GlobeCameraController;
            m_orbitCameraController = new OrbitGeoElementCameraController(m_xyCircleGraphic, 20.0)
            {
                CameraPitchOffset = 75.0
            };

            m_sceneView.MouseLeftButtonDown += sceneView_MouseLeftButtonDown;
            m_sceneView.MouseLeftButtonUp += sceneView_MouseLeftButtonUp;
            m_sceneView.PreviewMouseMove += sceneView_MouseMove;
        }
        /// <summary>
        /// 刷新
        /// </summary>
        public void update()
        {
            //说明场景中要素更新是线程安全的        
            refreshGeometry();
        }
        /// <summary>
        /// 初始化编辑器
        /// </summary>
        private void initEditor()
        {
            m_gpOverlayLine = new GraphicsOverlay();
            m_gpOverlayLine.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
            m_sceneView.GraphicsOverlays.Add(m_gpOverlayLine);
            m_gpOverlayFill = new GraphicsOverlay();
            m_gpOverlayFill.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
            m_sceneView.GraphicsOverlays.Add(m_gpOverlayFill);
            m_gpOverlay = new GraphicsOverlay();
            m_gpOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
            m_sceneView.GraphicsOverlays.Add(m_gpOverlay);

            //初始化旋转多边形
            m_rotatePolygonSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, m_rotatePolyColor, null);
            m_rotatePolygonSymbol.Outline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, m_rotatePolyBorderColor, 1);
            m_rotatePolygonGraphic = new Graphic(null, m_rotatePolygonSymbol);
            m_rotatePolygonGraphic.IsVisible = false;
            m_gpOverlayFill.Graphics.Add(m_rotatePolygonGraphic);

            m_xyCircleSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, m_xyCircleColor, 1);
            m_xAxisSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, m_xAxisColor, 2);
            m_yAxisSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, m_yAxisColor, 2);
            m_zAxisSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, m_zAxisColor, 2);

            PointCollection pointsXY = new PointCollection(SpatialReferences.Wgs84);
            PointCollection pointsXZ = new PointCollection(SpatialReferences.Wgs84);
            PointCollection pointsYZ = new PointCollection(SpatialReferences.Wgs84);
            m_subAngleDelta = Math.PI * 2 / (m_subPointCount - 1);
            m_dCircleRadius = CommonUtil.getInst().meter2degree(m_scale);

            //剖分圆环点      
            Vector3D startPntXY = new Vector3D(m_dCircleRadius, 0.0, 0.0);
            pointsXY.Add(new MapPoint(m_dCircleRadius, 0.0, 0.0, SpatialReferences.Wgs84));
            for (int i = 1; i < m_subPointCount + 1; i++)
            {
                Vector3D tPos = CommonUtil.getInst().RotateAroundZAxis(startPntXY, m_subAngleDelta * i);
                double dMeterz = CommonUtil.getInst().degree2meter(tPos.Y);
                pointsXY.Add(new MapPoint(tPos.X, tPos.Y, 0.0, SpatialReferences.Wgs84));
                pointsXZ.Add(new MapPoint(tPos.X, 0.0, dMeterz, SpatialReferences.Wgs84));
                pointsYZ.Add(new MapPoint(0.0, tPos.X, dMeterz, SpatialReferences.Wgs84));
            }
            //初始化xy平面圆环
            Polyline polylineXY = new Polyline(pointsXY);
            m_xyCircleGraphic = new Graphic(polylineXY, m_xyCircleSymbol);
            m_gpOverlayLine.Graphics.Add(m_xyCircleGraphic);

            double agreeScale = CommonUtil.getInst().meter2degree(m_scale);
            //初始化x轴            
            PointCollection pointsX = new PointCollection(SpatialReferences.Wgs84)
                {
                    new MapPoint(0, 0, 0),
                    new MapPoint(agreeScale, 0, 0),
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
                AnchorPosition = SceneSymbolAnchorPosition.Center
            };
            m_xAxiMarkGraphic = new Graphic(new MapPoint(agreeScale, 0, 0), m_xAxisMarkSymbol);
            m_gpOverlay.Graphics.Add(m_xAxiMarkGraphic);

            //初始化y轴            
            PointCollection pointsY = new PointCollection(SpatialReferences.Wgs84)
                {
                    new MapPoint(0, 0, 0, SpatialReferences.Wgs84),
                    new MapPoint(0, agreeScale, 0, SpatialReferences.Wgs84),
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
            m_yAxiMarkGraphic = new Graphic(new MapPoint(0, agreeScale, 0), m_yAxisMarkSymbol);
            m_gpOverlay.Graphics.Add(m_yAxiMarkGraphic);

            //初始化z轴            
            PointCollection pointsZ = new PointCollection(SpatialReferences.Wgs84)
                {
                    new MapPoint(0, 0, 0.1),
                    new MapPoint(0, 0, m_scale),
                };
            Polyline polylineZAxis = new Polyline(pointsZ);
            m_zAxisGraphic = new Graphic(polylineZAxis, m_zAxisSymbol);
            m_gpOverlay.Graphics.Add(m_zAxisGraphic);

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
            m_zAxiMarkGraphic = new Graphic(new MapPoint(0, 0, m_scale), m_zAxisMarkSymbol);
            m_gpOverlay.Graphics.Add(m_zAxiMarkGraphic);
        }
        /// <summary>
        /// 设置可见性
        /// </summary>
        /// <param name="vis"></param>
        public void setVisible(bool vis)
        {
            if (vis == m_isVisible) return;
            m_isVisible = vis;
            m_xyCircleGraphic.IsVisible = m_isVisible;
            m_xAxisGraphic.IsVisible = m_isVisible;
            m_yAxisGraphic.IsVisible = m_isVisible;
            m_zAxisGraphic.IsVisible = m_isVisible;
            m_xAxiMarkGraphic.IsVisible = m_isVisible;
            m_yAxiMarkGraphic.IsVisible = m_isVisible;
            m_zAxiMarkGraphic.IsVisible = m_isVisible;
            m_rotatePolygonGraphic.IsVisible = m_isVisible;
        }
        /// <summary>
        /// 设置编辑器位置
        /// </summary>
        /// <param name="pos">地图位置点</param>
        public void setPosition(MapPoint pos)
        {
            if (m_pos.IsEqual(pos)) return;
            m_oldPos = m_pos;
            m_pos = pos;
            m_refreshType = 1;
            refreshGeometry();
        }
        /// <summary>
        /// 返回编辑器的场景位置
        /// </summary>
        /// <returns>场景位置</returns>
        public MapPoint getPosition()
        {
            return m_pos;
        }
        private void moveGeometry(Graphic pGraphic, Vector3D moveDelta)
        {
            if (pGraphic.Geometry.GeometryType == GeometryType.Polyline)
            {
                Polyline tline = pGraphic.Geometry as Polyline;
                ReadOnlyPart part = tline.Parts[0];
                if (null == part || part.PointCount <= 0) return;
                PointCollection points = new PointCollection(SpatialReferences.Wgs84);
                int iPntSize = part.PointCount;
                for (int i = 0; i < iPntSize; i++)
                {
                    MapPoint tPnt = part.Points[i];
                    MapPoint newPnt = new MapPoint(tPnt.X + moveDelta.X, tPnt.Y + moveDelta.Y, tPnt.Z + moveDelta.Z);
                    points.Add(newPnt);
                }

                Polyline polyline = new Polyline(points);
                pGraphic.Geometry = polyline;
            }
            else if (pGraphic.Geometry.GeometryType == GeometryType.Point)
            {
                MapPoint tPnt = pGraphic.Geometry as MapPoint;
                MapPoint newPnt = new MapPoint(tPnt.X + moveDelta.X, tPnt.Y + moveDelta.Y, tPnt.Z + moveDelta.Z, SpatialReferences.Wgs84);
                pGraphic.Geometry = newPnt;
            }
        }

        private bool isRefresh = false;
        /// <summary>
        /// 更新几何体
        /// </summary>
        private void refreshGeometry()
        {
            if (m_refreshType == 0 || isRefresh) return;
            isRefresh = true;

            //更新位置
            if (m_refreshType == 1 && m_oldPos != null)
            {
                Vector3D moveDelta = new Vector3D(m_pos.X - m_oldPos.X, m_pos.Y - m_oldPos.Y, m_pos.Z - m_oldPos.Z);
                moveGeometry(m_xyCircleGraphic, moveDelta);
                moveGeometry(m_xAxisGraphic, moveDelta);
                moveGeometry(m_xAxiMarkGraphic, moveDelta);
                moveGeometry(m_yAxisGraphic, moveDelta);
                moveGeometry(m_yAxiMarkGraphic, moveDelta);
                moveGeometry(m_zAxisGraphic, moveDelta);
                moveGeometry(m_zAxiMarkGraphic, moveDelta);
                m_oldPos = null;
            }

            //刷新旋转(考虑xy平面)
            if (m_refreshType == 2 && m_rotateType == Rotate_Type.Rotate_XY)
            {
                if (m_angleDetltaTotal == 0)
                {
                    m_rotatePolygonGraphic.IsVisible = false;
                }

                double dAngleBegin, dAngleEnd;
                PointCollection pointsXY = new PointCollection(SpatialReferences.Wgs84);
                if (m_angleDetltaTotal < 0)
                {
                    dAngleBegin = m_rotateBeginAngle + m_angleDetltaTotal;
                    dAngleEnd = m_rotateBeginAngle;
                }
                else
                {
                    dAngleBegin = m_rotateBeginAngle;
                    dAngleEnd = m_rotateBeginAngle + m_angleDetltaTotal;
                }
                //剖分圆环点
                int iIndex = 0;
                double tAngle = dAngleBegin;
                Vector3D startPntXY = new Vector3D(m_dCircleRadius, 0.0, 0.0);
                while (tAngle <= dAngleEnd)
                {
                    tAngle = dAngleBegin + m_subAngleDelta * iIndex;
                    Vector3D tPos = CommonUtil.getInst().RotateAroundZAxis(startPntXY, tAngle);
                    pointsXY.Add(new MapPoint(tPos.X, tPos.Y, 0.0, SpatialReferences.Wgs84));
                    iIndex++;
                }
                pointsXY.Add(m_pos);
                Polygon polygon = new Polygon(pointsXY);
                m_rotatePolygonGraphic.Geometry = polygon;
                m_rotatePolygonGraphic.IsVisible = true;
            }

            isRefresh = false;
            m_refreshType = 0;
        }
        private void resetAxisColor()
        {
            m_xyCircleSymbol.Color = m_xyCircleColor;            
        }
        private async void activeCircle(ScreenPoint screenPnt, bool isFocus)
        {
            resetAxisColor();

            var tolerance = 5d;
            var maximumResults = 3;
            var onlyReturnPopups = false;

            //选中坐标轴
            IdentifyGraphicsOverlayResult identifyResults = await m_sceneView.IdentifyGraphicsOverlayAsync(
                m_gpOverlayLine,
                screenPnt,
                tolerance,
                onlyReturnPopups,
                maximumResults);

            if (identifyResults.Graphics.Count == 1)
            {
                Graphic pGraphic = identifyResults.Graphics[0];
                //暂时只考虑xy平面的旋转
                if (pGraphic == m_xyCircleGraphic)
                {
                    if (!isFocus)
                    {
                        m_xyCircleSymbol.Color = m_activeColor;
                    }
                    else
                    {
                        m_xyCircleSymbol.Color = m_focusColor;
                        m_rotateType = Rotate_Type.Rotate_XY;
                        //记录起始屏幕位置
                        m_preScreenPoint = screenPnt;
                    }
                }

                //停止鼠标对三维场景的控制
                if (isFocus)
                {
                    m_sceneView.CameraController = m_orbitCameraController;
                }
            }
        }
        /// <summary>
        /// 执行旋转
        /// </summary>
        /// <param name="hintPnt">屏幕点击位置</param>
        private void rotate(ScreenPoint hintPnt)
        {
            try
            {
                ScreenPoint centerPos = m_sceneView.LocationToScreen(m_pos);
                Vector2D vec1 = new Vector2D(m_preScreenPoint.X - centerPos.X, m_preScreenPoint.Y - centerPos.Y);
                Vector2D vec2 = new Vector2D(hintPnt.X - centerPos.X, hintPnt.Y - centerPos.Y);
                double curAngle = CommonUtil.getInst().getAngle2D(vec1, vec2)/10;
                
                Vector2D vec3 = new Vector2D(hintPnt.X - m_preScreenPoint.X, hintPnt.Y - m_preScreenPoint.Y);               
                double dot = CommonUtil.getInst().CrossValue(vec1, vec3);
                if (dot > 0.0)
                {
                    m_angleDetltaOneRotate = curAngle;
                }
                else if (dot < 0.0)
                {
                    m_angleDetltaOneRotate = -curAngle;
                }

                //触发位置改变事件
                RotateChangedEvent?.Invoke(Rotate_Type.Rotate_XY, m_angleDetltaOneRotate);
            }
            catch (Exception ex)
            {
                LogManager.AddLog(ex.Message);
            }
        }
        private void sceneView_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!m_isVisible || m_sceneView == null) return;
            if (m_rotateType == Rotate_Type.Rotate_None)
            {
                activeCircle(e.GetPosition(m_sceneView), false);
            }
            else if (m_rotateType == Rotate_Type.Rotate_XY)
            {
                Log.LogManager.AddLog("sceneView_MouseMove");
                rotate(e.GetPosition(m_sceneView));
            }
        }
        private void sceneView_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!m_isVisible || m_sceneView == null) return;
            if (m_rotateType == Rotate_Type.Rotate_None) return;
            m_rotateType = Rotate_Type.Rotate_None;
            Log.LogManager.AddLog("sceneView_MouseLeftButtonUp");
            m_rotateBeginAngle = 0.0;
            m_angleDetltaTotal = 0.0;
            m_angleDetltaOneRotate = 0.0;

            //恢复选中坐标轴的颜色
            resetAxisColor();

            //恢复使用全局相机
            m_sceneView.CameraController = m_globeCameraControl;           
        }
        private void sceneView_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!m_isVisible || m_sceneView == null) return;
            activeCircle(e.GetPosition(m_sceneView), true);
        }
    }
}


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
        private Color m_yzCircleColor;              //yz平面圆环颜色
        private Color m_xzCircleColor;              //xz平面圆环颜色
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
        public event RotateChangedEventHandler RotateChangedEvent;
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
        //xy平面圆环
        private Graphic m_xyCircleGraphic = null;
        //yz平面圆环
        private Graphic m_yzCircleGraphic = null;
        //xz平面圆环
        private Graphic m_xzCircleGraphic = null;
        //旋转多边形
        private Graphic m_rotatePolygonGraphic = null;
        //xy平面渲染符号
        private SimpleLineSymbol m_xyCircleSymbol = null;
        //yz平面渲染符号
        private SimpleLineSymbol m_yzCircleSymbol = null;
        //xz平面渲染符号
        private SimpleLineSymbol m_xzCircleSymbol = null;
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
        //上一帧方向向量
        private Vector2D m_preVector;

        public RotateEditor(SceneView sceView)
        {
            m_sceneView = sceView;
            m_rotatePolyColor = Color.FromArgb(128, 255, 255, 0);
            m_rotatePolyBorderColor = Color.FromArgb(200, 255, 255, 0);
            m_xyCircleColor = Color.FromArgb(255, 255, 0, 0);
            m_yzCircleColor = Color.FromArgb(255, 0, 255, 0);
            m_xzCircleColor = Color.FromArgb(255, 0, 0, 255);
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
            m_sceneView.MouseMove += sceneView_MouseMove;
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

            //初始化旋转多边形
            m_rotatePolygonSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, m_rotatePolyColor, null);
            m_rotatePolygonSymbol.Outline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, m_rotatePolyBorderColor, 1);
            m_rotatePolygonGraphic = new Graphic(null, m_rotatePolygonSymbol);
            m_rotatePolygonGraphic.IsVisible = false;
            m_gpOverlayFill.Graphics.Add(m_rotatePolygonGraphic);

            m_xyCircleSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, m_xyCircleColor, 1);
            m_yzCircleSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, m_xyCircleColor, 1);
            m_xzCircleSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, m_xyCircleColor, 1);

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

            //初始化xz平面圆环
            Polyline polylineXZ = new Polyline(pointsXZ);
            m_xzCircleGraphic = new Graphic(polylineXZ, m_xzCircleSymbol);
            m_gpOverlayFill.Graphics.Add(m_xzCircleGraphic);

            //初始化yz平面圆环
            Polyline polylineYZ = new Polyline(pointsYZ);
            m_yzCircleGraphic = new Graphic(polylineYZ, m_yzCircleSymbol);
            m_gpOverlayFill.Graphics.Add(m_yzCircleGraphic);
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
            m_xzCircleGraphic.IsVisible = m_isVisible;
            m_yzCircleGraphic.IsVisible = m_isVisible;
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
            Polyline tline = pGraphic.Geometry as Polyline;
            ReadOnlyPart part = tline.Parts[0];
            if (null == part || part.PointCount <= 0) return;
            PointCollection points = new PointCollection(SpatialReferences.Wgs84);
            int iPntSize = part.PointCount;
            for (int i = 0; i < iPntSize; i++)
            {
                MapPoint tPnt = part.Points[i];
                MapPoint newPnt = new MapPoint(tPnt.X + moveDelta.X, tPnt.Y + moveDelta.Y, tPnt.Z + moveDelta.Z, tPnt.SpatialReference);
                points.Add(newPnt);
            }

            Polyline polyline = new Polyline(points);
            pGraphic.Geometry = polyline;
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
                moveGeometry(m_xzCircleGraphic, moveDelta);
                moveGeometry(m_yzCircleGraphic, moveDelta);
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
            m_xzCircleSymbol.Color = m_xzCircleColor;
            m_yzCircleSymbol.Color = m_yzCircleColor;
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
                        //计算起始角度
                        ScreenPoint centerSnPoint = m_sceneView.LocationToScreen(m_pos);
                        Vector2D vec1 = new Vector2D(1, 0);
                        m_preVector = new Vector2D(screenPnt.X - centerSnPoint.X, screenPnt.Y - centerSnPoint.Y);
                        m_rotateBeginAngle = CommonUtil.getInst().getAngle2D(vec1, m_preVector);
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
                ScreenPoint centerSnPos = m_sceneView.LocationToScreen(m_pos);
                Vector2D vec2 = new Vector2D(hintPnt.X - centerSnPos.X, hintPnt.Y - centerSnPos.Y);
                double curAngle = CommonUtil.getInst().getAngle2D(m_preVector, vec2);
                Vector2D vec3 = vec2 - m_preVector;
                double dot = m_preVector.Dot(vec3);
                if(dot > 0.0)
                {
                    m_angleDetltaOneRotate = (-curAngle);
                }
                else if(dot < 0.0)
                {
                    m_angleDetltaOneRotate = curAngle;
                }

                m_angleDetltaTotal += m_angleDetltaOneRotate;
                m_preVector = vec2;

                m_refreshType = 2;
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
                rotate(e.GetPosition(m_sceneView));
            }
        }
        private void sceneView_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!m_isVisible || m_sceneView == null) return;
            if (m_rotateType == Rotate_Type.Rotate_None) return;
            m_rotateType = Rotate_Type.Rotate_None;

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


using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;

using Mathlib;
using System;
using System.Windows;
using System.Threading;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Threading;

using WPGIS.DataType;
using ScreenPoint = System.Windows.Point;
using PointCollection = Esri.ArcGISRuntime.Geometry.PointCollection;

namespace WPGIS.Core
{
    /// <summary>
    /// 移动辅助编辑器
    /// </summary>
    public class TransferEditor : IDisposable
    {
        private Color m_spereColor;
        private Color m_xAxisColor;
        private Color m_yAxisColor;
        private Color m_zAxisColor;
        private Color m_activeColor;
        private Color m_focusColor;
        private bool m_isVisible = false;

        public event MapPointChangedEventHandler MapPointChangedEvent = null;
        //编辑器位置
        private MapPoint m_pos = new MapPoint(0.0, 0.0, 0.0, SpatialReferences.Wgs84);
        //编辑器xy平面旋转角度（弧度）             
        private double m_rotOnXY = 0.0;
        //编辑器放大系数(默认1)
        private float m_scale = 1.0f;
        //初始化size(默认300)
        private float m_initSize = 300.0f;
        //离地高度
        private double m_relativeHeight = 0.0;
        //场景view               
        private SceneView m_sceneView = null;
        //编辑存储的要素层
        private GraphicsOverlay m_gpOverlayAxis = null;
        private GraphicsOverlay m_gpOverlayMark = null;
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
        private Vector3D m_moveDelta;
        //移动编辑器的贴地模式
        private SurfacePlacement m_surfacePlacement = SurfacePlacement.Absolute;
        //是否可以穿到地底
        private bool m_isCanUnderGroud = false;

        //移动位置缓冲区
        PointCollection m_preMovePointCollection = new PointCollection(SpatialReferences.Wgs84);
        //计算处理时间计时器
        System.Windows.Forms.Timer m_processTimer = new System.Windows.Forms.Timer();

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
            m_sceneView.ViewpointChanged += OnViewpointChanged;

            m_processTimer.Interval = 1;
            m_processTimer.Enabled = false;
            m_processTimer.Tick += ProcessTimer_Tick;
            m_processTimer.Start();
        }

        private bool m_isProcessing = false;
        private async void ProcessTimer_Tick(object sender, EventArgs e)
        {
            if (m_isProcessing) return;
            m_isProcessing = true;

            MapPoint processPnt = null;
            lock (m_preMovePointCollection)
            {
                if (m_preMovePointCollection.Count > 0)
                {
                    processPnt = m_preMovePointCollection[0];
                    m_preMovePointCollection.RemoveAt(0);
                }
            }

            if (processPnt != null)
            {
                //处理逻辑
                var tProcessedPos = await processPosition(processPnt);
                if (tProcessedPos != null)
                {
                    m_pos = tProcessedPos;
                    refreshGeometry(m_pos, m_scale, m_rotOnXY);
                    //触发位置改变事件
                    MapPointChangedEvent?.Invoke(m_pos);
                }
            }

            m_isProcessing = false;
        }

        /// <summary>
        /// 设置贴地模式
        /// </summary>
        /// <param name="placement">贴地模式</param>
        public async void setSurfacePlacement(SurfacePlacement placement)
        {
            m_surfacePlacement = placement;
            if (m_surfacePlacement == SurfacePlacement.Relative)
            {
                if (!m_pos.IsEqual(new MapPoint(0.0, 0.0, 0.0, SpatialReferences.Wgs84)))
                {
                    double dEvevation = await m_sceneView.Scene.BaseSurface.GetElevationAsync(m_pos);
                    m_relativeHeight = m_pos.Z - dEvevation;
                }
            }
        }

        /// <summary>
        /// 获取相对高度
        /// </summary>
        /// <returns>相对高度</returns>
        public async Task<double> getRelativeHeight()
        {
            double dEvevation = await m_sceneView.Scene.BaseSurface.GetElevationAsync(m_pos);
            return m_pos.Z - dEvevation;
        }

        /// <summary>
        /// 是否可以穿地
        /// </summary>
        public bool CanUnderGroud
        {
            get { return m_isCanUnderGroud; }
            set { m_isCanUnderGroud = value; }
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
            m_gpOverlayAxis = new GraphicsOverlay();
            m_gpOverlayAxis.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            m_sceneView.GraphicsOverlays.Add(m_gpOverlayAxis);
            m_gpOverlayMark = new GraphicsOverlay();
            m_gpOverlayMark.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            m_sceneView.GraphicsOverlays.Add(m_gpOverlayMark);

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
            var location = new MapPoint(0, 0, 0, SpatialReferences.Wgs84);
            m_spereGraphic = new Graphic(location, m_spereSymbol);
            m_gpOverlayAxis.Graphics.Add(m_spereGraphic);

            m_xAxisSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, m_xAxisColor, 2);
            m_yAxisSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, m_yAxisColor, 2);
            m_zAxisSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, m_zAxisColor, 2);
            m_xAxisSymbol.MarkerStyle = SimpleLineSymbolMarkerStyle.Arrow;
            m_xAxisSymbol.MarkerPlacement = SimpleLineSymbolMarkerPlacement.End;

            double agreeScale = CommonUtil.getInst().meter2degree(m_initSize);
            //初始化x轴            
            PointCollection pointsX = new PointCollection(SpatialReferences.Wgs84)
                {
                    new MapPoint(0, 0, 0),
                    new MapPoint(agreeScale, 0, 0),
                };
            Polyline polylineXAxis = new Polyline(pointsX);
            m_xAxisGraphic = new Graphic(polylineXAxis, m_xAxisSymbol);
            m_gpOverlayAxis.Graphics.Add(m_xAxisGraphic);

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
            m_xAxiMarkGraphic = new Graphic(new MapPoint(agreeScale, 0, 0), m_xAxisMarkSymbol);
            m_gpOverlayMark.Graphics.Add(m_xAxiMarkGraphic);

            //初始化y轴            
            PointCollection pointsY = new PointCollection(SpatialReferences.Wgs84)
                {
                    new MapPoint(0, 0, 0, SpatialReferences.Wgs84),
                    new MapPoint(0, agreeScale, 0, SpatialReferences.Wgs84),
                };
            Polyline polylineYAxis = new Polyline(pointsY);
            m_yAxisGraphic = new Graphic(polylineYAxis, m_yAxisSymbol);
            m_gpOverlayAxis.Graphics.Add(m_yAxisGraphic);

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
            m_gpOverlayMark.Graphics.Add(m_yAxiMarkGraphic);

            //初始化z轴            
            PointCollection pointsZ = new PointCollection(SpatialReferences.Wgs84)
                {
                    new MapPoint(0, 0, 0),
                    new MapPoint(0, 0, m_initSize + 0),
                };
            Polyline polylineZAxis = new Polyline(pointsZ);
            m_zAxisGraphic = new Graphic(polylineZAxis, m_zAxisSymbol);
            m_gpOverlayAxis.Graphics.Add(m_zAxisGraphic);

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
            m_zAxiMarkGraphic = new Graphic(new MapPoint(0, 0, m_initSize + 0), m_zAxisMarkSymbol);
            m_gpOverlayMark.Graphics.Add(m_zAxiMarkGraphic);
        }
        /// <summary>
        /// 设置位置
        /// </summary>
        /// <param name="pos">地图位置点</param>
        public async void setPosition(MapPoint pos)
        {
            if (m_pos.IsEqual(pos)) return;
            m_pos = await processPosition(pos);
            ResizeScaleFromCamera();
        }

        private async Task<MapPoint> processPosition(MapPoint pos)
        {
            MapPoint retPnt = null;
            double dEvevation = 0.0f;

            try
            {
                //Console.WriteLine("----------Enter GetElevationAsync-----------\n");
                dEvevation = await m_sceneView.Scene.BaseSurface.GetElevationAsync(pos);
                //Console.WriteLine("----------Leave GetElevationAsync-----------\n");
            }
            catch (Exception ex)
            {
                //Console.WriteLine("----------Catch GetElevationAsync-----------\n");
                return null;
            }

            //先对穿地情况进行处理
            if (m_isCanUnderGroud)
            {
                retPnt = pos;
            }
            else
            {
                retPnt = (pos.Z >= dEvevation) ? pos : new MapPoint(pos.X, pos.Y, dEvevation, pos.SpatialReference);
            }

            if (m_surfacePlacement == SurfacePlacement.Relative)
            {
                if (m_currentAxisType == Axis_Type.Axis_X || m_currentAxisType == Axis_Type.Axis_Y || m_currentAxisType == Axis_Type.Axis_XYZ)
                {
                    GeometryEngine.SetZ(retPnt,dEvevation + m_relativeHeight);
                }
                else
                {
                    //record relative height
                    m_relativeHeight = retPnt.Z - dEvevation;
                }
            }
            else if (m_surfacePlacement == SurfacePlacement.Absolute)
            {
                if (m_currentAxisType == Axis_Type.Axis_X || m_currentAxisType == Axis_Type.Axis_Y || m_currentAxisType == Axis_Type.Axis_XYZ)
                {
                    if (m_pos.Z < dEvevation)
                    {
                        GeometryEngine.SetZ(retPnt, dEvevation);
                    }
                }
            }
            else if (m_surfacePlacement == SurfacePlacement.Draped)
            {
                GeometryEngine.SetZ(retPnt, dEvevation);
            }

            return retPnt;
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
        private void refreshGeometry(MapPoint renderPos, float scale, double angle)
        {
            //球面坐标系下按照一定的方向角平移计算获得x轴和y轴的终点位置
            var pntCollection = new PointCollection(renderPos.SpatialReference) { renderPos };
            var xAxisPointEndCol = GeometryEngine.MoveGeodetic(pntCollection, m_initSize * scale, LinearUnits.Meters, angle, AngularUnits.Radians, GeodeticCurveType.Geodesic);
            var yAxisPointEndCol = GeometryEngine.MoveGeodetic(pntCollection, m_initSize * scale, LinearUnits.Meters, angle + Math.PI / 2, AngularUnits.Radians, GeodeticCurveType.Geodesic);
            var xAxisEndMapPoint = xAxisPointEndCol[0];
            var yAxisEndMapPoint = yAxisPointEndCol[0];
            //计算z轴的终点位置
            var zAxisPointEnd = new Vector3D(0, 0, m_initSize * scale);
            var zAxisEndMapPoint = new MapPoint(renderPos.X + zAxisPointEnd.X, renderPos.Y + zAxisPointEnd.Y, renderPos.Z + zAxisPointEnd.Z, renderPos.SpatialReference);

            //放大中间的方块
            m_spereSymbol.Width = m_initSize * scale / 5;
            m_spereSymbol.Height = m_initSize * scale / 5;
            m_spereSymbol.Depth = m_initSize * scale / 5;
            m_spereSymbol.Heading = angle * 180 / Math.PI; ;
            m_spereGraphic.Geometry = renderPos;

            //更新x轴
            m_xAxiMarkGraphic.Geometry = xAxisEndMapPoint;
            m_xAxisGraphic.Geometry = new Polyline(new PointCollection(renderPos.SpatialReference) { renderPos, xAxisEndMapPoint });
            //更新y轴
            m_yAxiMarkGraphic.Geometry = yAxisEndMapPoint;
            m_yAxisGraphic.Geometry = new Polyline(new PointCollection(renderPos.SpatialReference) { renderPos, yAxisEndMapPoint });
            //更新z轴
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
            m_spereSymbol.Color = m_spereColor;
        }

        private async Task<Graphic> IdentifyAxis(ScreenPoint screenPnt)
        {
            var tolerance = 10d;
            var maximumResults = 4;
            var onlyReturnPopups = false;

            //选中坐标轴
            IdentifyGraphicsOverlayResult identifyResults = await m_sceneView.IdentifyGraphicsOverlayAsync(
                m_gpOverlayAxis,
                screenPnt,
                tolerance,
                onlyReturnPopups,
                maximumResults);

            Graphic pRetGraphic = null;
            if (identifyResults.Graphics.Count == 1)
            {
                pRetGraphic = identifyResults.Graphics[0];
            }
            else if (identifyResults.Graphics.Count > 1)
            {
                pRetGraphic = m_spereGraphic;
            }

            return pRetGraphic;
        }

        private async void activeAxis(ScreenPoint screenPnt)
        {
            resetAxisColor();

            Graphic hitGraphic = await IdentifyAxis(screenPnt);
            if (hitGraphic == m_xAxisGraphic)
            {
                m_xAxisSymbol.Color = m_activeColor;
                m_xAxisMarkSymbol.Color = m_activeColor;
            }
            else if (hitGraphic == m_yAxisGraphic)
            {
                m_yAxisSymbol.Color = m_activeColor;
                m_yAxisMarkSymbol.Color = m_activeColor;
            }
            else if (hitGraphic == m_spereGraphic)
            {
                //只有在Draped模式下可用
                if(m_surfacePlacement == SurfacePlacement.Draped)
                {
                    m_spereSymbol.Color = m_activeColor;
                }                
            }
            else if (hitGraphic == m_zAxisGraphic)
            {
                //除了Draped其他模式可用
                if (m_surfacePlacement != SurfacePlacement.Draped)
                {
                    m_zAxisSymbol.Color = m_activeColor;
                    m_zAxisMarkSymbol.Color = m_activeColor;
                }
            }
        }

        private void moveTo(MapPoint hintMapPnt)
        {
            lock (m_preMovePointCollection)
            {
                if (m_preMovePointCollection.Count < 10)
                {
                    m_preMovePointCollection.Add(hintMapPnt);
                }
                else
                {
                    Random tRan = new Random();
                    int iRandomValue = tRan.Next(3, 7);
                    m_preMovePointCollection.RemoveAt(iRandomValue);
                    m_preMovePointCollection.Add(hintMapPnt);
                }
            }
        }

        /// <summary>
        /// 沿指定轴移动到屏幕点击位置
        /// </summary>
        /// <param name="hintPnt">屏幕点击位置</param>
        /// <param name="pGrahic">轴</param>
        private void moveByAxis(ScreenPoint hintPnt, Graphic pGrahic)
        {
            Polyline tline = pGrahic.Geometry as Polyline;
            MapPoint mapBeginPnt = tline.Parts[0].Points[0];
            MapPoint mapEndPnt = tline.Parts[0].Points[1];
            ScreenPoint scBeginPnt = m_sceneView.LocationToScreen(mapBeginPnt);
            ScreenPoint scEndPnt = m_sceneView.LocationToScreen(mapEndPnt);

            //移动的二维向量
            Vector2D vecMove = new Vector2D(hintPnt.X - m_moveBeginPoint.X, hintPnt.Y - m_moveBeginPoint.Y);
            //轴在屏幕上的二维向量
            Vector2D vecAxis = new Vector2D(scEndPnt.X - scBeginPnt.X, scEndPnt.Y - scBeginPnt.Y);
            double dotValue = vecMove.Dot(vecAxis);
            if (Math.Abs(dotValue) < 0.000001) return;

            Vector2D vecMoveProject = vecAxis * (vecAxis.Dot(vecMove) / vecAxis.MagnitudeSquared);
            //移动比例
            double dRatio = vecMoveProject.Magnitude / vecAxis.Magnitude;

            Vector3D mapVec = new Vector3D(mapBeginPnt.X - mapEndPnt.X, mapBeginPnt.Y - mapEndPnt.Y, mapBeginPnt.Z - mapEndPnt.Z);
            if (dotValue > 0)
            {
                mapVec = -mapVec;
            }
            double mapLength = mapVec.Magnitude;
            double moveLength = mapLength * dRatio;
            //计算出移动的向量增量
            m_moveDelta = mapVec.Normalize() * Math.Abs(moveLength);
            //更新位置并重绘
            var pos = new MapPoint(m_pos.X + m_moveDelta.X, m_pos.Y + m_moveDelta.Y, m_pos.Z + m_moveDelta.Z, m_pos.SpatialReference);
            lock(m_preMovePointCollection)
            {
                if (m_preMovePointCollection.Count < 50)
                {
                    m_preMovePointCollection.Add(pos);
                }
                else
                {
                    Random tRan = new Random();
                    int iRandomValue = tRan.Next(10, 40);
                    m_preMovePointCollection.RemoveAt(iRandomValue);
                    m_preMovePointCollection.Add(pos);
                }
            }     

            m_moveBeginPoint = hintPnt;
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
                //Console.WriteLine("-------- enter sceneView_MouseMove ------\n");                
                //移动坐标轴
                ScreenPoint hintPnt = e.GetPosition(m_sceneView);
                if (m_currentAxisType == Axis_Type.Axis_X)
                {
                    moveByAxis(hintPnt, m_xAxisGraphic);
                }
                else if (m_currentAxisType == Axis_Type.Axis_Y)
                {
                    moveByAxis(hintPnt, m_yAxisGraphic);
                }
                else if (m_currentAxisType == Axis_Type.Axis_Z)
                {
                    moveByAxis(hintPnt, m_zAxisGraphic);
                }
                else if (m_currentAxisType == Axis_Type.Axis_XYZ)
                {
                    MapPoint mPnt = m_sceneView.ScreenToBaseSurface(hintPnt);
                    if (mPnt != null)
                    {
                        moveTo(mPnt);
                    }
                }               
                //Console.WriteLine("-------- leave sceneView_MouseMove ------\n");               
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

            ScreenPoint hintPnt = e.GetPosition(m_sceneView);
            Graphic hitGraphic = await IdentifyAxis(hintPnt);
            if (hitGraphic != null)
            {
                //停止鼠标对三维场景的控制
                m_sceneView.InteractionOptions.IsEnabled = false;
                m_moveBeginPoint = hintPnt;

                if (hitGraphic == m_xAxisGraphic)
                {
                    m_xAxisSymbol.Color = m_focusColor;
                    m_xAxisMarkSymbol.Color = m_focusColor;
                    m_currentAxisType = Axis_Type.Axis_X;
                }
                else if (hitGraphic == m_yAxisGraphic)
                {
                    m_yAxisSymbol.Color = m_focusColor;
                    m_yAxisMarkSymbol.Color = m_focusColor;
                    m_currentAxisType = Axis_Type.Axis_Y;
                }
                else if (hitGraphic == m_spereGraphic)
                {
                    //只有在Draped模式下可用
                    if (m_surfacePlacement == SurfacePlacement.Draped)
                    {
                        m_spereSymbol.Color = m_focusColor;
                        m_currentAxisType = Axis_Type.Axis_XYZ;
                    }                   
                }
                else if (hitGraphic == m_zAxisGraphic)
                {
                    //除了Draped其他模式可用
                    if (m_surfacePlacement != SurfacePlacement.Draped)
                    {
                        m_zAxisSymbol.Color = m_focusColor;
                        m_zAxisMarkSymbol.Color = m_focusColor;
                        m_currentAxisType = Axis_Type.Axis_Z;
                    }                    
                }
            }
        }

        private void OnViewpointChanged(object sender, EventArgs e)
        {
            if (m_isVisible)
            {
                ResizeScaleFromCamera();
            }
        }

        private void ResizeScaleFromCamera()
        {
            var camera = m_sceneView.Camera;
            MapPoint cameraPnt = new MapPoint(camera.Location.X, camera.Location.Y, camera.Location.Z, m_pos.SpatialReference);
            var horDist = GeometryEngine.DistanceGeodetic(cameraPnt, m_pos, LinearUnits.Meters, AngularUnits.Degrees, GeodeticCurveType.Geodesic);
            var dist = Math.Sqrt(Math.Pow(horDist.Distance, 2) + Math.Pow(camera.Location.Z - m_pos.Z, 2));
            m_scale = (float)dist / 10000;
            if (m_scale < 1.0f) m_scale = 1.0f;
            refreshGeometry(m_pos, m_scale, m_rotOnXY);
        }

        public void Dispose()
        {
            if (null == m_sceneView) return;
            m_sceneView.MouseLeftButtonDown -= sceneView_MouseLeftButtonDown;
            m_sceneView.MouseLeftButtonUp -= sceneView_MouseLeftButtonUp;
            m_sceneView.PreviewMouseMove -= sceneView_MouseMove;
            m_sceneView.ViewpointChanged -= OnViewpointChanged;

            if (m_gpOverlayAxis != null)
            {
                m_sceneView.GraphicsOverlays.Remove(m_gpOverlayAxis);
                m_gpOverlayAxis = null;
            }

            if (m_gpOverlayMark != null)
            {
                m_sceneView.GraphicsOverlays.Remove(m_gpOverlayMark);
                m_gpOverlayMark = null;
            }
        }
    }
}

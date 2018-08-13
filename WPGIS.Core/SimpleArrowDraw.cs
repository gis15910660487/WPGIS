
using System;
using Mathlib;
using WPGIS.DataType;
using System.Collections.Generic;
using Color = System.Windows.Media.Color;

using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;

namespace WPGIS.Core
{
    /// <summary>
    /// 简单箭头标绘
    /// </summary>
    public class SimpleArrowDraw : SimpleDrawBase
    {
      
        private ControlPointManager m_controlPointManager = null;
        private IList<Vector2D> m_headPoints = new List<Vector2D>();

        //当前选中的控制点
        private IControlPoint m_selectedCtrlPoint = null;
        //选中控制点的事件
        public override event SelectCtrlPointEventHandler SelectCtrlPointEvent;


        public SimpleArrowDraw(SceneView sceneView)
            :base(sceneView)
        {
            m_sceneView = sceneView;
            m_controlPointManager = new ControlPointManager();
            m_controlPointManager.initialize(m_sceneView);
            //初始化6个箭头控制点
            MapPoint p0 = new MapPoint(0, CommonUtil.getInst().meter2degree(2000.0), 0, SpatialReferences.Wgs84);
            MapPoint p1 = new MapPoint(0, CommonUtil.getInst().meter2degree(1500.0), 0, SpatialReferences.Wgs84);
            MapPoint p2 = new MapPoint(0, CommonUtil.getInst().meter2degree(1000.0), 0, SpatialReferences.Wgs84);
            MapPoint p3 = new MapPoint(0, CommonUtil.getInst().meter2degree(500.0), 0, SpatialReferences.Wgs84);
            MapPoint p4 = new MapPoint(CommonUtil.getInst().meter2degree(-500.0), 0, 0, SpatialReferences.Wgs84);
            MapPoint p5 = new MapPoint(CommonUtil.getInst().meter2degree(500.0), 0, 0, SpatialReferences.Wgs84);
            MapPoint p6 = new MapPoint(0, CommonUtil.getInst().meter2degree(200.0), 0, SpatialReferences.Wgs84);
            m_controlPointManager.createControlPoint(p0);
            m_controlPointManager.createControlPoint(p1);
            m_controlPointManager.createControlPoint(p2);
            m_controlPointManager.createControlPoint(p3);
            m_controlPointManager.createControlPoint(p4);
            m_controlPointManager.createControlPoint(p5);
            m_controlPointManager.createControlPoint(p6);

            m_sceneView.MouseLeftButtonDown += sceneView_MouseLeftButtonDown;
        }

        /// <summary>
        /// 修改形状(通过修改当前控制点位置)
        /// </summary>
        /// <param name="pnt">输入控制点位置</param>
        public override void doEdit(MapPoint pnt)
        {
            if (null == m_selectedCtrlPoint) return;
            m_selectedCtrlPoint.mapPosition = pnt;
            refresh();
        }

        /// <summary>
        /// 左键拾取控制点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void sceneView_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IControlPoint selectPnt = await m_controlPointManager.identifyControlPoint(e.GetPosition(m_sceneView));
            if (selectPnt != null)
            {
                SelectCtrlPointEvent?.Invoke(selectPnt);
                m_selectedCtrlPoint = selectPnt;
            }
        }

        public override void moveTo(MapPoint pnt)
        {
            m_pos = pnt;
            MapPoint p0 = m_controlPointManager.getControlPoint(0).mapPosition;
            MapPoint p1 = m_controlPointManager.getControlPoint(1).mapPosition;
            MapPoint p2 = m_controlPointManager.getControlPoint(2).mapPosition;
            MapPoint p3 = m_controlPointManager.getControlPoint(3).mapPosition;
            MapPoint p4 = m_controlPointManager.getControlPoint(4).mapPosition;
            MapPoint p5 = m_controlPointManager.getControlPoint(5).mapPosition;
            MapPoint p6 = m_controlPointManager.getControlPoint(6).mapPosition;

            //移动方向
            MapPoint moveVec = new MapPoint(pnt.X - p0.X, pnt.Y - p0.Y, 0.0, p0.SpatialReference);
            MapPoint pDes0 = pnt;
            MapPoint pDes1 = new MapPoint(p1.X + moveVec.X, p1.Y + moveVec.Y, p1.Z, p1.SpatialReference);
            MapPoint pDes2 = new MapPoint(p2.X + moveVec.X, p2.Y + moveVec.Y, p2.Z, p2.SpatialReference);
            MapPoint pDes3 = new MapPoint(p3.X + moveVec.X, p3.Y + moveVec.Y, p3.Z, p3.SpatialReference);
            MapPoint pDes4 = new MapPoint(p4.X + moveVec.X, p4.Y + moveVec.Y, p4.Z, p4.SpatialReference);
            MapPoint pDes5 = new MapPoint(p5.X + moveVec.X, p5.Y + moveVec.Y, p5.Z, p5.SpatialReference);
            MapPoint pDes6 = new MapPoint(p6.X + moveVec.X, p6.Y + moveVec.Y, p6.Z, p6.SpatialReference);

            m_controlPointManager.getControlPoint(0).mapPosition = pDes0;
            m_controlPointManager.getControlPoint(1).mapPosition = pDes1;
            m_controlPointManager.getControlPoint(2).mapPosition = pDes2;
            m_controlPointManager.getControlPoint(3).mapPosition = pDes3;
            m_controlPointManager.getControlPoint(4).mapPosition = pDes4;
            m_controlPointManager.getControlPoint(5).mapPosition = pDes5;
            m_controlPointManager.getControlPoint(6).mapPosition = pDes6;

            refresh();
        }
        /// <summary>
        /// 开启旋转模式
        /// </summary>
        public override void startRotate()
        {
            m_editType = Edit_Type.Edit_Rotate;
            selected = true;
        }
        /// <summary>
        /// 在xy平面旋转
        /// </summary>
        /// <param name="delta">角度</param>
        public override void rotateOnXY(double delta, bool focusRefresh)
        {
            m_rotOnXY += delta;
            if(m_rotOnXY >= 2 * Math.PI)
            {
                m_rotOnXY = m_rotOnXY - 2 * Math.PI;
            }
            else if(m_rotOnXY < 0.0)
            {
                m_rotOnXY = 2 * Math.PI - m_rotOnXY;
            }
            //旋转所有的控制点
            int iCtrlPntSize = m_controlPointManager.getSize();
            for (int i = 0; i < iCtrlPntSize; i++)
            {
                MapPoint ctrlPnt = m_controlPointManager.getControlPoint(i).mapPosition;
                //先移到原点
                Vector3D tVec = new Vector3D(ctrlPnt.X - m_pos.X, ctrlPnt.Y - m_pos.Y, 0.0);
                Vector3D desVec = CommonUtil.getInst().RotateAroundZAxis(tVec, delta);
                //再移回
                MapPoint desPos = new MapPoint(m_pos.X + desVec.X, m_pos.Y + desVec.Y, ctrlPnt.Z, ctrlPnt.SpatialReference);
                m_controlPointManager.getControlPoint(i).mapPosition = desPos;
            }

            if (focusRefresh)
            {
                refresh();
            }
        }
        /// <summary>
        /// 结束旋转模式
        /// </summary>
        public override void endRotate()
        {
            m_editType = Edit_Type.Edit_None;
            selected = false;
        }

        public void refresh()
        {
            recomputePoints();
            refreshGeometry();
        }

        public override void initGraphic()
        {
            //添加面要素
            m_fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, m_fillColor, null);
            m_fillSymbol.Outline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, m_borderColor, 1);
            PointCollection points = new PointCollection(SpatialReferences.Wgs84);
            Polygon tPolygon = new Polygon(points);
            m_graphic = new Graphic(tPolygon, m_fillSymbol);
            m_gpOverlay.Graphics.Add(m_graphic);

            refresh();
        }

        /// <summary>
        /// 开始编辑模式
        /// </summary>
        public override void startEdit()
        {
            m_selectedCtrlPoint = null;
            m_editType = Edit_Type.Edit_Geometry;

            //所有控制点可见
            int iControlPntSize = m_controlPointManager.getSize();
            for (int iIndex = 0; iIndex < iControlPntSize; iIndex++)
            {
                IControlPoint cpnt = m_controlPointManager.getControlPoint(iIndex);
                if (cpnt != null)
                {
                    cpnt.visible = true;
                }
            }
            //并调整边框色   
            selected = true;
        }

        /// <summary>
        /// 结束编辑模式
        /// </summary>
        public override void endEdit()
        {
            m_selectedCtrlPoint = null;
            m_editType = Edit_Type.Edit_None;

            //所有控制点可见
            int iControlPntSize = m_controlPointManager.getSize();
            for (int iIndex = 0; iIndex < iControlPntSize; iIndex++)
            {
                IControlPoint cpnt = m_controlPointManager.getControlPoint(iIndex);
                if (cpnt != null)
                {
                    cpnt.visible = false;
                }
            }
            //恢复边框色
            selected = false;
        }

        private int getBezierPos(IList<Vector2D> cp, IList<Vector2D> points, double fStep)
        {
            double ax, bx, cx;
            double ay, by, cy;
            cx = 3.0 * (cp[1].X - cp[0].X);
            bx = 3.0 * (cp[2].X - cp[1].X) - cx;
            ax = cp[3].X - cp[0].X - cx - bx;
            cy = 3.0 * (cp[1].Y - cp[0].Y);
            by = 3.0 * (cp[2].Y - cp[1].Y) - cy;
            ay = cp[3].Y - cp[0].Y - cy - by;

            int nIndex = 0;
            for (double t = 0; t < 1;)
            {
                Vector2D pos = new Vector2D(ax * Math.Pow(t, 3) + bx * Math.Pow(t, 2) + cx * t + cp[0].X, ay * Math.Pow(t, 3) + by * Math.Pow(t, 2) + cy * t + cp[0].Y);
                points.Add(pos);
                t += fStep;
                nIndex++;
            }
            return nIndex;
        }

        /// <summary>
        /// 重新计算点
        /// </summary>
        private void recomputePoints()
        {
            m_headPoints.Clear();

            //计算前两个控制点
            Vector2D v0 = m_controlPointManager.getControlPoint(0).getXY();
            Vector2D v1 = m_controlPointManager.getControlPoint(1).getXY();
            Vector2D v2 = m_controlPointManager.getControlPoint(2).getXY();
            Vector2D v3 = m_controlPointManager.getControlPoint(3).getXY();
            Vector2D v4 = m_controlPointManager.getControlPoint(4).getXY();
            Vector2D v5 = m_controlPointManager.getControlPoint(5).getXY();
            Vector2D v6 = m_controlPointManager.getControlPoint(6).getXY();

            //v1到顶点的距离
            double fLen1_0 = (v1 - v0).Magnitude;
            //第一对控制点的中点
            Vector2D vMid1 = v0 + (v2 - v0).Normalize() * fLen1_0;

            //计算第一对控制点 内角30度
            Vector2D vvleft1, vvright1;
            Vector2D vvleft12, vvright12;
            double flen1;
            {
                flen1 = (fLen1_0 / 5);
                double flen12 = (fLen1_0 / 2);
                Vector2D vDir = (v0 - vMid1).Normalize();
                Vector2D vDirL = new Vector2D(-vDir.Y, vDir.X);
                vDirL = vDirL.Normalize();

                vvleft1 = vMid1 + vDirL * flen1;
                vvright1 = vMid1 - vDirL * flen1;

                vvleft12 = vMid1 + vDirL * flen12;
                vvright12 = vMid1 - vDirL * flen12;
            }

            double fLenwai = fLen1_0 * 2;
            Vector2D vMidWai = v0 - (v0 - v2).Normalize() * fLenwai;

            //计算箭头两端点 外角45度
            Vector2D vvleftwai, vvrightwai;
            double flenwai;
            {
                flenwai = (fLenwai / 2);
                Vector2D vDir = (v0 - vMidWai).Normalize();
                Vector2D vDirL = new Vector2D(-vDir.Y, vDir.X);
                vDirL = vDirL.Normalize();

                vvleftwai = vMidWai + vDirL * flenwai;
                vvrightwai = vMidWai - vDirL * flenwai;
            }

            Vector2D vMid4 = new Vector2D((v5.X + v4.X) / 2, (v5.Y + v4.Y) / 2);
            //尾巴长度
            double fLen4 = (v5 - v4).Magnitude / 2;

            //总的等比线段
            double fw = fLen4 - flen1;
            double fh = (vMid4 - vMid1).Magnitude;

            //控制点2的对称点
            Vector2D vvleft2, vvright2;
            double flen2;
            {
                double flen = (vMid4 - v2).Magnitude;
                flen2 = flen1 + (fh - flen) / fh * fw;
                Vector2D vDir = (v2 - v3).Normalize();
                Vector2D vDirL = new Vector2D(-vDir.Y, vDir.X);

                vvleft2 = v2 + vDirL.Normalize() * flen2;
                vvright2 = v2 - vDirL.Normalize() * flen2;
            }

            //控制点3的对称点
            Vector2D vvleft3, vvright3;
            double flen3;
            {
                double flen = (vMid4 - v3).Magnitude;
                flen3 = flen1 + (fh - flen) / fh * fw;
                Vector2D vDir = (v3 - vMid4).Normalize();
                Vector2D vDirL = new Vector2D(-vDir.Y, vDir.X);

                vvleft3 = v3 + vDirL.Normalize() * flen3;
                vvright3 = v3 - vDirL.Normalize() * flen3;
            }

            //控制点6的对称点
            Vector2D vvleft6, vvright6;
            double flen6;
            {
                double flen = (vMid4 - v6).Magnitude;
                flen6 = flen1 + (fh - flen) / fh * fw;
                Vector2D vDir = (v6 - vMid4).Normalize();
                Vector2D vDirL = new Vector2D(-vDir.Y, vDir.X);

                vvleft6 = v6 + vDirL.Normalize() * flen6;
                vvright6 = v6 - vDirL.Normalize() * flen6;
            }

            IList<Vector2D> myIn1 = new List<Vector2D>();
            IList<Vector2D> myOut1 = new List<Vector2D>();
            IList<Vector2D> myIn2 = new List<Vector2D>();
            IList<Vector2D> myOut2 = new List<Vector2D>();
            IList<Vector2D> myIn3 = new List<Vector2D>();
            IList<Vector2D> myOut3 = new List<Vector2D>();

            myIn1.Add(v4);
            myIn1.Add(vvleft3);
            myIn1.Add(vvleft2);
            myIn1.Add(vvleft1);
            //获取箭头中部左边的贝塞尔曲线
            int nCount = getBezierPos(myIn1, myOut1, 0.02f);

            myIn2.Add(v5);
            myIn2.Add(vvright3);
            myIn2.Add(vvright2);
            myIn2.Add(vvright1);
            //获取箭头中部右边的贝塞尔曲线
            nCount = getBezierPos(myIn2, myOut2, 0.02f);

            myIn3.Add(v4);
            myIn3.Add(vvleft6);
            myIn3.Add(vvright6);
            myIn3.Add(v5);
            //获取箭头底部的贝塞尔曲线
            nCount = getBezierPos(myIn3, myOut3, 0.02f);

            //组织箭头的头部点
            {
                for (int i = 0; i < myOut1.Count; i++)
                {
                    m_headPoints.Add(myOut1[i]);
                }

                m_headPoints.Add(vvleftwai);
                m_headPoints.Add(v0);
                m_headPoints.Add(vvrightwai);

                for (int i = myOut2.Count - 1; i > -1; i--)
                {
                    m_headPoints.Add(myOut2[i]);
                }

                for (int i = myOut3.Count - 1; i > -1; i--)
                {
                    m_headPoints.Add(myOut3[i]);
                }
            }
        }

        MapPoint convert2MapPoint(Vector2D pnt2d)
        {
            return new MapPoint(pnt2d.X, pnt2d.Y, 0.0, SpatialReferences.Wgs84);
        }

        /// <summary>
        /// 刷新几何体
        /// </summary>
        private void refreshGeometry()
        {
            lock (this)
            {
                PointCollection pointsHead = new PointCollection(SpatialReferences.Wgs84);
                foreach (var pnt2d in m_headPoints)
                {
                    MapPoint mPnt = convert2MapPoint(pnt2d);
                    pointsHead.Add(mPnt);
                }
                Polygon tPolygonHead = new Polygon(pointsHead);
                m_graphic.Geometry = tPolygonHead;
            }           
        }

        /// <summary>
        /// 开启移动模式
        /// </summary>
        public override void startMove()
        {
            m_editType = Edit_Type.Edit_Transfer;
            selected = true;
        }

        /// <summary>
        /// 结束移动模式
        /// </summary>
        public override void endMove()
        {
            m_editType = Edit_Type.Edit_None;
            selected = false;
        }

        /// <summary>
        /// 停止
        /// </summary>
        public override void stopAll()
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

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    visible = false;
                    m_headPoints.Clear();
                    m_gpOverlay.Graphics.Remove(m_graphic);
                    m_graphic = null;
                    m_fillSymbol = null;
                    m_controlPointManager.clear();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~SimpleArrowDraw() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public override void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

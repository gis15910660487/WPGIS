
using WPGIS.DataType;
using Esri.ArcGISRuntime.UI;
using System.Collections.Generic;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.Mapping;

using ScreenPoint = System.Windows.Point;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;

namespace WPGIS.Core
{
    /// <summary>
    /// 标绘管理器
    /// </summary>
    public class DrawManager
    {
        private SceneView m_sceneView = null;
        private IDrawInterface m_editDraw = null;
        private TransferEditor m_transferEditor = null;
        private RotateEditor m_rotateEditor = null;
        private Edit_Type m_editType = Edit_Type.Edit_None;
        private IList<IDrawInterface> m_draws = new List<IDrawInterface>();
        //当前箭头改变事件
        public event CurrentArrowChangedEventHandler CurrentArrowChangedEvent = null;

        private static readonly DrawManager instance = new DrawManager();
        /// <summary>
        /// 构造函数
        /// </summary>
        private DrawManager()
        {
        }
        /// <summary>
        /// 标绘管理器单例
        /// </summary>
        /// <returns></returns>
        public static DrawManager getInst()
        {
            return instance;
        }
        /// <summary>
        /// 初始化管理器
        /// </summary>
        /// <param name="sceneView"></param>
        public void initialize(SceneView sceneView)
        {
            m_sceneView = sceneView;

            m_transferEditor = new TransferEditor(m_sceneView);
            m_transferEditor.initEditor();
            m_transferEditor.visible = false;
            m_transferEditor.MapPointChangedEvent += transferEditor_MapPointChangedEvent;

            m_rotateEditor = new RotateEditor(m_sceneView);
            m_rotateEditor.setVisible(false);
            m_rotateEditor.RotateChangedEvent += rotateEditor_RotateChangedEvent;

            //双击开启编辑
            m_sceneView.MouseDoubleClick += sceneView_MouseDoubleClick;
        }
        /// <summary>
        /// 返回当前绘制对象
        /// </summary>
        /// <returns></returns>
        public IDrawInterface getCurrentDraw()
        {
            return m_editDraw;
        }

        /// <summary>
        /// 返回当前编辑模式
        /// </summary>
        /// <returns></returns>
        public Edit_Type getCurrentEditMode() { return m_editType; }

        public int getDrawCount() { return m_draws.Count; }

        public void removeCurrentDraw()
        {
            if(m_editDraw != null)
            {
                removeDraw(m_editDraw);
            }
            m_editType = Edit_Type.Edit_None;
        }

        /// <summary>
        /// 设置旋转角度
        /// </summary>
        /// <param name="angle">角度(弧度)</param>
        public void setRotateAngle(double angle)
        {
            if (m_editDraw != null)
            {
                m_editDraw.angleOnXY = angle;
            }
            if (m_transferEditor != null && m_transferEditor.visible)
            {
                m_transferEditor.setRotOnXY(angle);
            }
        }

        /// <summary>
        /// 开启移动模式
        /// </summary>
        /// <param name="draw">标绘实体</param>
        public void startMove(IDrawInterface draw)
        {
            m_rotateEditor.setVisible(false);
            if (m_editDraw != null)
            {
                m_editDraw.stopAll();
            }
            m_editDraw = draw;
            m_editDraw.startMove();
            //触发当前箭头改变事件
            CurrentArrowChangedEvent?.Invoke(m_editDraw);

            m_transferEditor.visible = true;
            m_transferEditor.setPosition(m_editDraw.mapPosition);
            m_transferEditor.setRotOnXY(m_editDraw.angleOnXY);
            m_editType = Edit_Type.Edit_Transfer;
        }

        /// <summary>
        /// 开启编辑模式
        /// </summary>
        /// <param name="draw"></param>
        public void startEdit(IDrawInterface draw)
        {
            m_rotateEditor.setVisible(false);
            m_transferEditor.visible = false;

            if (m_editDraw != null)
            {
                m_editDraw.stopAll();
            }
            m_editDraw = draw;
            if (m_editDraw != null)
            {
                m_editDraw.startEdit();
                m_editType = Edit_Type.Edit_Geometry;
                m_editDraw.SelectCtrlPointEvent += editDraw_selectCtrlPointEvent;
            }
            //触发当前箭头改变事件
            CurrentArrowChangedEvent?.Invoke(m_editDraw);
        }

        /// <summary>
        /// 开启旋转模式
        /// </summary>
        /// <param name="draw"></param>
        public void startRotate(IDrawInterface draw)
        {
            m_transferEditor.visible = false;
            if (m_editDraw != null)
            {
                m_editDraw.stopAll();
            }
            m_editDraw = draw;
            m_editDraw.startRotate();
            m_rotateEditor.setVisible(true);
            MapPoint cpPnt = m_editDraw.mapPosition;
            MapPoint tePnt = new MapPoint(cpPnt.X, cpPnt.Y, cpPnt.Z, cpPnt.SpatialReference);
            m_rotateEditor.setPosition(tePnt);
            m_editType = Edit_Type.Edit_Rotate;
            //触发当前箭头改变事件
            CurrentArrowChangedEvent?.Invoke(m_editDraw);
        }

        public void stopEdit()
        {
            if (m_editDraw != null)
            {
                m_editDraw.stopAll();
                m_editDraw = null;
                //触发当前箭头改变事件
                CurrentArrowChangedEvent?.Invoke(null);
            }
            m_transferEditor.visible = false;
            m_rotateEditor.setVisible(false);
            m_editType = Edit_Type.Edit_None;
        }

        public void removeDraw(IDrawInterface draw)
        {
            if (draw != null)
            {
                if (draw == m_editDraw)
                {
                    m_editDraw = null;
                    m_transferEditor.visible = false;
                    //触发当前箭头改变事件
                    CurrentArrowChangedEvent?.Invoke(null);
                }
                m_draws.Remove(draw);
                draw.Dispose();
            }
        }

        public IDrawInterface createDraw(DrawType type)
        {
            IDrawInterface objDraw = null;
            switch (type)
            {
                case DrawType.DrawType_SimpleArrow:
                    {
                        objDraw = new SimpleArrowDraw(m_sceneView);
                        objDraw.initGraphic();
                        break;
                    }
                case DrawType.DrawType_SimpleModel:
                    {
                        objDraw = new SimpleModelDraw(m_sceneView);
                        objDraw.initGraphic();
                        break;
                    }
                default:
                    break;
            }
            //添加到容器
            if (objDraw != null)
            {
                m_draws.Add(objDraw);
            }
            m_editType = Edit_Type.Edit_Create;
            m_editDraw = objDraw;

            return objDraw;
        }

        /// <summary>
        /// 拾取绘制实体
        /// </summary>
        /// <param name="snPnt">屏幕坐标</param>        
        public async void pointSelectDraw(ScreenPoint hintPnt)
        {
            if (m_editDraw != null)
            {
                m_editDraw.selected = false;
            }

            var tolerance = 10d;
            var maximumResults = 3;
            var onlyReturnPopups = false;
            //选中坐标轴
            var identifyResults = await m_sceneView.IdentifyGraphicsOverlaysAsync(
                hintPnt,
                tolerance,
                onlyReturnPopups,
                maximumResults);

            if (identifyResults.Count >= 1)
            {
                var identifyResult = identifyResults[0];
                if (identifyResult.Graphics.Count == 1)
                {
                    m_editDraw = findDraw(identifyResult.Graphics[0]);
                    if (m_editDraw != null)
                    {
                        m_editDraw.selected = true;

                        if (m_transferEditor.visible)
                        {
                            MapPoint cpPnt = m_editDraw.mapPosition;
                            MapPoint tePnt = new MapPoint(cpPnt.X, cpPnt.Y, cpPnt.Z, cpPnt.SpatialReference);
                            m_transferEditor.setPosition(tePnt);
                        }

                        //触发当前箭头改变事件
                        CurrentArrowChangedEvent?.Invoke(m_editDraw);
                    }
                }
            }
        }

        #region 私有函数
        private void transferEditor_MapPointChangedEvent(MapPoint mPnt)
        {
            if (m_editDraw == null) return;
            if (m_editType == Edit_Type.Edit_Transfer)
            {
                m_editDraw.moveTo(mPnt);
            }
            else if (m_editType == Edit_Type.Edit_Geometry)
            {
                m_editDraw.doEdit(mPnt);
            }
        }
        private void rotateEditor_RotateChangedEvent(Rotate_Type type, double angle)
        {
            if (m_editDraw == null) return;
            if (type == Rotate_Type.Rotate_XY)
            {
                m_editDraw.rotateOnXY(angle, true);
            }
        }
        private async void sceneView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (m_editDraw != null)
            {
                m_editDraw.stopAll();
                m_editType = Edit_Type.Edit_None;
                m_editDraw.SelectCtrlPointEvent -= editDraw_selectCtrlPointEvent;
                m_transferEditor.visible = false;
            }

            //查找绘制对象
            var tolerance = 2d;
            var maximumResults = 3;
            var onlyReturnPopups = false;
            ScreenPoint hintPnt = e.GetPosition(m_sceneView);

            //选中坐标轴
            var identifyResults = await m_sceneView.IdentifyGraphicsOverlaysAsync(
                hintPnt,
                tolerance,
                onlyReturnPopups,
                maximumResults);

            if (identifyResults.Count >= 1)
            {
                var identifyResult = identifyResults[0];
                if (identifyResult.Graphics.Count == 1)
                {
                    //开启编辑
                    if (identifyResult.Graphics.Count == 1)
                    {
                        m_editDraw = findDraw(identifyResult.Graphics[0]);
                        startEdit(m_editDraw);

                        //触发当前箭头改变事件
                        CurrentArrowChangedEvent?.Invoke(m_editDraw);
                    }
                }
            }            
        }
        private void editDraw_selectCtrlPointEvent(IControlPoint cpnt)
        {
            MapPoint cpPnt = cpnt.mapPosition;
            MapPoint tePnt = new MapPoint(cpPnt.X, cpPnt.Y, cpPnt.Z + 10, cpPnt.SpatialReference);
            m_transferEditor.setPosition(tePnt);
            m_transferEditor.visible = true;
        }
        private IDrawInterface findDraw(Graphic pGraphic)
        {
            IDrawInterface retDraw = null;
            foreach (var draw in m_draws)
            {
                if (draw.graphic == pGraphic)
                {
                    retDraw = draw;
                    break;
                }
            }

            return retDraw;
        }

        #endregion
    }
}

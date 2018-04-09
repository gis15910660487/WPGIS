﻿
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
        private GraphicsOverlay m_gpOverlay = null;
        private IDrawInterface m_editDraw = null;
        private TransferEditor m_transferEditor = null;
        private RotateEditor m_rotateEditor = null;
        private Edit_Type m_editType = Edit_Type.Edit_None;
        private IList<IDrawInterface> m_draws = new List<IDrawInterface>();

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
            m_gpOverlay = new GraphicsOverlay();
            m_gpOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Draped;
            m_sceneView.GraphicsOverlays.Add(m_gpOverlay);

            m_transferEditor = new TransferEditor(m_sceneView);
            m_transferEditor.initEditor();
            m_transferEditor.setVisible(false);
            m_transferEditor.MapPointChangedEvent += transferEditor_MapPointChangedEvent;

            m_rotateEditor = new RotateEditor(m_sceneView);
            m_rotateEditor.setVisible(false);
            m_rotateEditor.RotateChangedEvent += rotateEditor_RotateChangedEvent;

            //双击开启编辑
            m_sceneView.MouseDoubleClick += sceneView_MouseDoubleClick;
        }
        /// <summary>
        /// 刷新绘制管理器
        /// </summary>
        public void update()
        {
            if (m_rotateEditor != null)
            {
                m_rotateEditor.update();
            }

            foreach(var draw in m_draws)
            {
                draw.update();
            }
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
            m_transferEditor.setVisible(true);
            MapPoint cpPnt = m_editDraw.mapPosition;
            MapPoint tePnt = new MapPoint(cpPnt.X, cpPnt.Y, cpPnt.Z, cpPnt.SpatialReference);
            m_transferEditor.setPosition(tePnt);
            m_editType = Edit_Type.Edit_Transfer;
        }

        /// <summary>
        /// 开启编辑模式
        /// </summary>
        /// <param name="draw"></param>
        public void startEdit(IDrawInterface draw)
        {
            m_rotateEditor.setVisible(false);
            m_transferEditor.setVisible(false);
            
            if (m_editDraw != null)
            {
                m_editDraw.stopAll();
            }
            m_editDraw = draw;
            if (m_editDraw != null)
            {
                m_editDraw.startEdit();
                m_editType = Edit_Type.Edit_Geometry;
                m_editDraw.selectCtrlPointEvent += editDraw_selectCtrlPointEvent;
            }
        }

        /// <summary>
        /// 开启旋转模式
        /// </summary>
        /// <param name="draw"></param>
        public void startRotate(IDrawInterface draw)
        {
            m_transferEditor.setVisible(false);
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
        }

        public void stopEdit()
        {
            if (m_editDraw != null)
            {
                m_editDraw.stopAll();
                m_editDraw = null;
            }
            m_transferEditor.setVisible(false);
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
                    m_transferEditor.setVisible(false);
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
                        objDraw = new SimpleArrowDraw(m_sceneView, m_gpOverlay);
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

            return objDraw;
        }

        /// <summary>
        /// 拾取箭头功能
        /// </summary>
        /// <param name="snPnt">屏幕坐标</param>        
        public async void pointSelectDraw(ScreenPoint hintPnt)
        {
            var tolerance = 10d;
            var maximumResults = 3;
            var onlyReturnPopups = false;
            //选中坐标轴
            IdentifyGraphicsOverlayResult identifyResults = await m_sceneView.IdentifyGraphicsOverlayAsync(
                m_gpOverlay,
                hintPnt,
                tolerance,
                onlyReturnPopups,
                maximumResults);

            if (identifyResults.Graphics.Count == 1)
            {
                m_editDraw = findDraw(identifyResults.Graphics[0]);
                if(m_editDraw != null)
                {
                    m_editDraw.selected = true;
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
                m_editDraw.rotateOnXY(angle, false);
            }
        }
        private async void sceneView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (m_editDraw != null)
            {
                m_editDraw.stopAll();
                m_editType = Edit_Type.Edit_None;
                m_editDraw.selectCtrlPointEvent -= editDraw_selectCtrlPointEvent;
                m_transferEditor.setVisible(false);
            }

            //查找绘制对象
            var tolerance = 2d;
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

            //开启编辑
            if (identifyResults.Graphics.Count == 1)
            {
                m_editDraw = findDraw(identifyResults.Graphics[0]);
                startEdit(m_editDraw);
            }
        }
        private void editDraw_selectCtrlPointEvent(IControlPoint cpnt)
        {
            MapPoint cpPnt = cpnt.mapPosition;
            MapPoint tePnt = new MapPoint(cpPnt.X, cpPnt.Y, cpPnt.Z + 10, cpPnt.SpatialReference);
            m_transferEditor.setPosition(tePnt);
            m_transferEditor.setVisible(true);
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

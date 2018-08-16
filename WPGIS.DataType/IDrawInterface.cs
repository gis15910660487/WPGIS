
using System;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Geometry;
using System.Windows.Media;

namespace WPGIS.DataType
{
    /// <summary>
    /// 标绘接口
    /// </summary>
    public interface IDrawInterface : IDisposable
    {
        event SelectCtrlPointEventHandler SelectCtrlPointEvent;

        /// <summary>
        /// 编号
        /// </summary>
        int ID { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        string name { get; set; }

        /// <summary>
        /// 绘制几何体
        /// </summary>
        Graphic graphic { get; }

        /// <summary>
        /// 编辑模式
        /// </summary>
        Edit_Type editType { get; }

        /// <summary>
        /// 场景位置
        /// </summary>
        MapPoint mapPosition { get;}

        /// <summary>
        /// 可见性
        /// </summary>
        bool visible { get; set; }
        /// <summary>
        /// 选中状态
        /// </summary>
        bool selected { get; set; }
        /// <summary>
        /// 填充颜色
        /// </summary>
        Color fillColor { get; set; }
        /// <summary>
        /// 边框颜色
        /// </summary>
        Color borderColor { get; set; }
        /// <summary>
        /// xy平面旋转角度
        /// </summary>
        double angleOnXY { get; set; }

        /// <summary>
        /// 初始化几何体
        /// </summary>
        void initGraphic();       
        /// <summary>
        /// 开启移动模式
        /// </summary>
        void startMove();

        /// <summary>
        /// 移动到新位置
        /// </summary>
        /// <param name="pnt"></param>
        void moveTo(MapPoint pnt);

        /// <summary>
        /// 结束移动模式
        /// </summary>
        void endMove();

        /// <summary>
        /// 开启旋转模式
        /// </summary>
        void startRotate();

        /// <summary>
        /// xy平面旋转
        /// </summary>
        /// <param name="angle">角度</param>
        void rotateOnXY(double angle, bool focusRefresh);

        /// <summary>
        /// 结束旋转模式
        /// </summary>
        void endRotate();

        /// <summary>
        /// 开始编辑模式
        /// </summary>
        void startEdit();

        /// <summary>
        /// 修改形状(通过修改当前控制点位置)
        /// </summary>
        /// <param name="pnt">输入控制点位置</param>
        void doEdit(MapPoint pnt);

        /// <summary>
        /// 结束编辑模式
        /// </summary>
        void endEdit();

        /// <summary>
        /// 停止
        /// </summary>
        void stopAll();
    }
}

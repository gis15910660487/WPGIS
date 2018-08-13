
using Esri.ArcGISRuntime.Geometry;

namespace WPGIS.DataType
{
    /// <summary>
    /// 位置移动事件
    /// </summary>
    public delegate void MapPointChangedEventHandler(MapPoint mPnt);
    /// <summary>
    /// 控制点选中事件
    /// </summary>
    public delegate void SelectCtrlPointEventHandler(IControlPoint cpnt);
    /// <summary>
    /// 旋转改变事件
    /// </summary>
    /// <param name="type">旋转类型</param>
    /// <param name="angle">角度（弧度）</param>
    public delegate void RotateChangedEventHandler(Rotate_Type type, double angle);
    /// <summary>
    /// 当前箭头改变事件
    /// </summary>
    public delegate void CurrentArrowChangedEventHandler(IDrawInterface draw);

    /// <summary>
    /// 旋转类型
    /// </summary>
    public enum Rotate_Type
    {
        Rotate_None,
        Rotate_XY,
        Rotate_XZ,
        Rotate_YZ,
    }

    /// <summary>
    /// 标绘类型
    /// </summary>
    public enum DrawType
    {
        DrawType_SimpleArrow = 1 << 1,      //简单箭头标绘
        DrawType_SimpleModel = 2 << 2,      //简单模型标绘
    }

    /// <summary>
    /// 坐标轴类型
    /// </summary>
    public enum Axis_Type
    {
        Axis_None,
        Axis_X,
        Axis_Y,
        Axis_Z,
        Axis_XYZ
    }

    /// <summary>
    /// 编辑类型
    /// </summary>
    public enum Edit_Type
    {
        Edit_None,                     //默认
        Edit_Create,                   //创建
        Edit_Transfer,                 //移动位置
        Edit_Rotate,                   //旋转位置
        Edit_Geometry,                 //修改形状
    }
}


using Mathlib;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;

namespace WPGIS.DataType
{
    /// <summary>
    /// 控制点接口
    /// </summary>
     public interface IControlPoint
    {
        MapPoint mapPosition
        {
            get;
            set;
        }

        bool visible
        {
            get;
            set;
        }

        Vector2D getXY();

        bool isGraphic(Graphic pGraphic);
    }
}

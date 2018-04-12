
using Esri.ArcGISRuntime.Mapping;

namespace WPGIS.DataType
{
    /// <summary>
    /// 场景数据
    /// </summary>
    public class SceneModel
    {
        public SceneModel()
        {
        }

        /// <summary>
        /// 三维场景
        /// </summary>
        public Scene scene { get; set; }
    }
}

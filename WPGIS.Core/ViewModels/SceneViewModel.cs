
using System;
using WPGIS.DataType;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;

namespace WPGIS.Core
{
    /// <summary>
    /// 三维场景viewmodel
    /// </summary>
    public class SceneViewModel : ViewModelbase
    {
        private SceneModel m_model = null;

        private Scene m_scene = null;
        // URL to the elevation service - provides terrain elevation
        private readonly Uri _elevationServiceUrl = new Uri("http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");

        public SceneViewModel(SceneView sceneView)
        {
            m_model = new SceneModel();
            initScene(sceneView);
        }

        /// <summary>
        /// 三维场景
        /// </summary>
        public Scene scene
        {
            get
            {
                return m_model.scene;
            }
            set
            {
                m_model.scene = value;
                RaisePropertyChanged("scene");
            }
        }

        private void initScene(SceneView sceneView)
        {
            // 创建三维场景
            m_scene = new Scene(Basemap.CreateImagery());

            // 添加地形数据源
            Surface surface = new Surface();
            ElevationSource elevationSource = new ArcGISTiledElevationSource(_elevationServiceUrl);
            surface.ElevationSources.Add(elevationSource);
            m_scene.BaseSurface = surface;
            this.scene = m_scene;

            //设置相机
            var pCamera = new Camera(53.16, -4.14, 6289, 95, 71, 0);
            sceneView.SetViewpointCamera(pCamera);

            // Interaction Options https://devtopia.esri.com/runtime/runtime-mapping-api-design/wiki/GeoView-Interaction-options
            sceneView.InteractionOptions = new Esri.ArcGISRuntime.UI.SceneViewInteractionOptions()
            {
                IsEnabled = true,
                IsFlickEnabled = true,
                WheelZoomDirection = Esri.ArcGISRuntime.UI.WheelZoomDirection.Default
            };

            //初始化标绘管理器
            DrawManager.getInst().initialize(sceneView);
        }
    }
}

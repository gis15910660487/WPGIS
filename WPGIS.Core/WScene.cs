
using System;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using WPGIS.Log;

namespace WPGIS.Core
{
    public class WScene
    {
        private Scene m_scene = null;
        private SceneView m_sceneView = null;
        private static readonly WScene instance = new WScene();
        // URL to the elevation service - provides terrain elevation
        private readonly Uri _elevationServiceUrl = new Uri("http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");

        /// <summary>
        /// 构造函数
        /// </summary>
        private WScene()
        {
        }

        /// <summary>
        /// 单例
        /// </summary>
        /// <returns></returns>
        public static WScene getInst()
        {
            return instance;
        }

        public Scene getScene()
        {
            return m_scene;
        }

        public void initScene(SceneView sceneView)
        {
            m_sceneView = sceneView;
            // 创建三维场景
            m_scene = new Scene(Basemap.CreateImagery());

            // 添加地形数据源
            Surface surface = new Surface();
            ElevationSource elevationSource = new ArcGISTiledElevationSource(_elevationServiceUrl);
            surface.ElevationSources.Add(elevationSource);
            m_scene.BaseSurface = surface;
            m_sceneView.Scene = m_scene;

            //设置相机
            var pCamera = new Camera(53.16, -4.14, 6289, 95, 71, 0);
            m_sceneView.SetViewpointCamera(pCamera);

            //初始化标绘管理器
            DrawManager.getInst().initialize(m_sceneView);
        }

        /// <summary>
        /// 刷新场景
        /// </summary>
        public void update()
        {
            try
            {
                DrawManager.getInst().update();
            }
            catch(Exception ex)
            {
                LogManager.AddLog(ex.Message);
            }            
        }
    }
}

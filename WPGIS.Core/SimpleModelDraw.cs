
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Collections.Generic;

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using WPGIS.DataType;
using Esri.ArcGISRuntime.Symbology;

namespace WPGIS.Core
{
    public class SimpleModelDraw : SimpleDrawBase
    {
        private const string VEHICLE_RES_FILE = @"resources\models\Vehicle.obj";

        public SimpleModelDraw(SceneView sceneView)
            : base(sceneView)
        {
        }

        public override async void initGraphic()
        {
            string modelFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, VEHICLE_RES_FILE);
            if (System.IO.File.Exists(modelFile))
            {

                ModelSceneSymbol symbol = await ModelSceneSymbol.CreateAsync(new Uri(modelFile), 0.1);
                symbol.Heading = 180;
                symbol.Pitch = -90;
                symbol.AnchorPosition = SceneSymbolAnchorPosition.Bottom;

                m_graphic = new Graphic(new MapPoint(0, 0, 0, SpatialReferences.Wgs84), symbol);
                m_gpOverlay.Graphics.Add(m_graphic);
            }
        }

        public override void moveTo(MapPoint pnt)
        {
            if(m_graphic != null)
            {
                m_graphic.Geometry = pnt;
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

                    if (m_graphic != null)
                    {
                        m_gpOverlay.Graphics.Remove(m_graphic);
                        m_graphic = null;
                    }

                    m_fillSymbol = null;
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

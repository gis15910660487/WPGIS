
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
using Esri.ArcGISRuntime.Mapping;

namespace WPGIS.Core
{
    public class SimpleModelDraw : SimpleDrawBase
    {
        private const string VEHICLE_RES_FILE = @"resources\models\Aircraft_size.dae";

        public SimpleModelDraw(SceneView sceneView)
            : base(sceneView)
        {
            m_gpOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
        }

        public override async void initGraphic()
        {
            string modelFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, VEHICLE_RES_FILE);
            if (System.IO.File.Exists(modelFile))
            {

                ModelSceneSymbol symbol = await ModelSceneSymbol.CreateAsync(new Uri(modelFile), 10);
                symbol.Heading = 0;
                symbol.Pitch = 0;
                symbol.AnchorPosition = SceneSymbolAnchorPosition.Bottom;

                m_graphic = new Graphic(new MapPoint(0, 0, 0, SpatialReferences.Wgs84), symbol);
                m_gpOverlay.Graphics.Add(m_graphic);
            }
        }

        public override void rotateOnXY(double delta, bool focusRefresh)
        {
            base.rotateOnXY(delta, focusRefresh);
            if(m_graphic != null)
            {
                ModelSceneSymbol symbol = m_graphic.Symbol as ModelSceneSymbol;
                if(symbol != null)
                {
                    symbol.Heading = (2 * Math.PI- m_rotOnXY) * 180 / Math.PI;
                }
            }
        }

        public override void moveTo(MapPoint pnt)
        {
            m_pos = pnt;
            if (m_graphic != null)
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

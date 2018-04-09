
using System;
using Mathlib;
using WPGIS.Core;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;

namespace WPGIS
{
    class TestFunc
    {
        private static readonly TestFunc instance = new TestFunc();

        /// <summary>
        /// 构造函数
        /// </summary>
        private TestFunc()
        {
        }

        /// <summary>
        /// 单例
        /// </summary>
        /// <returns></returns>
        public static TestFunc getInst()
        {
            return instance;
        }

        public void test(Scene myScene)
        {
            if (null == myScene) return;
            var lonlatPnt = new MapPoint(118.06, 81.04, 1289, myScene.SpatialReference);
            var utmPnt = CommonUtil.getInst().convert2Utm(lonlatPnt);
            var lonlat2 = CommonUtil.getInst().convert2Lonlat(utmPnt);

            Vector3D pos1 = new Vector3D(3.0, 10.0, 4.0);
            Vector3D pos2 = new Vector3D(10.0, 3.0, 4.0);

            double dAngle = CommonUtil.getInst().getAngleXY(pos1, pos2);
            Vector3D pos3 = CommonUtil.getInst().RotateAroundZAxis(pos2, dAngle);
            Vector3D pos4 = CommonUtil.getInst().RotateAroundZAxis(pos1, Math.PI/2);
            Vector3D pos5 = CommonUtil.getInst().RotateAroundZAxis(pos1, Math.PI);            
        }
    }
}

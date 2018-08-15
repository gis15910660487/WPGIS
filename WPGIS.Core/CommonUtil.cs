
using System;
using Mathlib;
using Esri.ArcGISRuntime.Geometry;

namespace WPGIS.Core
{
    /// <summary>
    /// 坐标转换接口
    /// </summary>
    public class CommonUtil
    {

        private static readonly CommonUtil instance = new CommonUtil();

        /// <summary>
        /// 构造函数
        /// </summary>
        private CommonUtil()
        {
        }

        /// <summary>
        /// 单例
        /// </summary>
        /// <returns></returns>
        public static CommonUtil getInst()
        {
            return instance;
        }


        /// <summary>
        /// utm转经纬度
        /// </summary>
        /// <param name="utmPnt"></param>
        /// <returns></returns>
        public MapPoint convert2Lonlat(MapPoint utmPnt)
        {            
            string sLonLat = CoordinateFormatter.ToLatitudeLongitude(utmPnt, LatitudeLongitudeFormat.DecimalDegrees, 4);
            MapPoint lonlatPnt = CoordinateFormatter.FromLatitudeLongitude(sLonLat, SpatialReferences.Wgs84);
            return lonlatPnt;
        }

        /// <summary>
        /// 经纬度转utm
        /// </summary>
        /// <param name="lonlatPnt"></param>
        /// <returns></returns>
        public MapPoint convert2Utm(MapPoint lonlatPnt)
        {
            string sUtm = CoordinateFormatter.ToUtm(lonlatPnt, UtmConversionMode.NorthSouthIndicators, true);
            MapPoint utmPnt = CoordinateFormatter.FromUtm(sUtm, SpatialReferences.Wgs84, UtmConversionMode.NorthSouthIndicators);
            return utmPnt;
        }

        /// <summary>
        /// 获取两个二维向量的夹角
        /// </summary>
        /// <param name="pos1">向量1</param>
        /// <param name="pos2">向量2</param>
        /// <returns>返回角度（弧度）</returns>
        public double getAngle2D(Vector2D pos1, Vector2D pos2)
        {
            Vector2D npos1 = pos1.Normalize();
            Vector2D npos2 = pos2.Normalize();
            return Math.Acos(npos1.Dot(npos2));
        }

        /// <summary>
        /// 获取两个三维向量在xy平面的夹角
        /// </summary>
        /// <param name="pos1">向量1</param>
        /// <param name="pos2">向量2</param>
        /// <returns>返回角度（弧度）</returns>
        public double getAngleXY(Vector3D pos1, Vector3D pos2)
        {
            Vector2D ppos1 = pos1.XY;
            Vector2D ppos2 = pos2.XY;
            Vector2D npos1 = ppos1.Normalize();
            Vector2D npos2 = ppos2.Normalize();
            return Math.Acos(npos1.Dot(npos2));
        }

        /// <summary>
        /// 以原点为中心绕z轴顺时针旋转获得结果点
        /// </summary>
        /// <param name="pos">输入三维点</param>
        /// <param name="theta">角度（弧度）</param>
        /// <returns>旋转后的三维点</returns>
        public Vector3D RotateAroundZAxis(Vector3D pos, double theta)
        {
            Vector3D retPos = pos.RotateAroundAxis(new Vector3D(0.0, 0.0, 1.0), theta);
            return retPos;
        }
        /// <summary>
        /// 以原点为中心绕y轴顺时针旋转获得结果点
        /// </summary>
        /// <param name="pos">输入三维点</param>
        /// <param name="theta">角度（弧度）</param>
        /// <returns>旋转后的三维点</returns>
        public Vector3D RotateAroundYAxis(Vector3D pos, double theta)
        {
            Vector3D retPos = pos.RotateAroundAxis(new Vector3D(0.0, 1.0, 0.0), theta);
            return retPos;
        }
        /// <summary>
        /// 以原点为中心绕x轴顺时针旋转获得结果点
        /// </summary>
        /// <param name="pos">输入三维点</param>
        /// <param name="theta">角度（弧度）</param>
        /// <returns>旋转后的三维点</returns>
        public Vector3D RotateAroundXAxis(Vector3D pos, double theta)
        {
            Vector3D retPos = pos.RotateAroundAxis(new Vector3D(1.0, 0.0, 0.0), theta);
            return retPos;
        }
        /// <summary>
        /// 计算两点距离
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public double getLength(System.Windows.Point p1, System.Windows.Point p2)
        {
            double tValue = (p1.X - p2.X)* (p1.X - p2.X) + (p1.Y - p2.Y)*(p1.Y - p2.Y);
            return Math.Sqrt(tValue);
        }
        public double CrossValue(Vector2D vec1, Vector2D vec2)
        {
            return vec1.X * vec2.Y - vec1.Y * vec2.X;
        }

        /// <summary>
        /// 米转度
        /// </summary>
        /// <param name="dMeter">米</param>
        /// <returns>度</returns>
        public double meter2degree(double dMeter)
        {
            double delta = (2 * Math.PI * 6378137.0) / 360;
            double degree = dMeter / delta;

            return degree;
        }

        /// <summary>
        /// 度转米
        /// </summary>
        /// <param name="degree">度</param>
        /// <returns>米</returns>
        public double degree2meter(double degree)
        {
            double delta = (2 * Math.PI * 6378137.0) / 360;
            double dMeter = degree * delta;

            return dMeter;
        }

    }
}

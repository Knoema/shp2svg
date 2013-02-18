using System;

namespace shp2svg
{
	class Mercator
	{
		private static readonly double R_MAJOR = 6378137.0;
		private static readonly double R_MINOR = 6356752.3142;
		private static readonly double RATIO = R_MINOR / R_MAJOR;
		private static readonly double ECCENT = Math.Sqrt(1.0 - (RATIO * RATIO));
		private static readonly double DEG2RAD = Math.PI / 180.0;
		private static readonly double RAD2DEG = 180.0 / Math.PI;

		public static double[] toPixel(double lon, double lat)
		{
			return new double[] { lonToX(lon), latToY(lat) };
		}

		public static double[] toGeoCoord(double x, double y)
		{
			return new double[] { xToLon(x), yToLat(y) };
		}

		public static double lonToX(double lon)
		{
			return R_MAJOR * DegToRad(lon);
		}

		public static double latToY(double lat)
		{
			lat = Math.Min(89.5, Math.Max(lat, -89.5));
			
			return R_MAJOR * Math.Log(Math.Tan(Math.PI / 4 + DegToRad(lat) / 2) * Math.Pow((1 - ECCENT * Math.Sin(DegToRad(lat))) / (1 + ECCENT * Math.Sin(DegToRad(lat))), ECCENT / 2));
		}

		public static double xToLon(double x)
		{
			return RadToDeg(x) / R_MAJOR;
		}

		public static double yToLat(double y)
		{
			double ts = Math.Exp(-y / R_MAJOR);
			double phi = (Math.PI / 2) - 2 * Math.Atan(ts);
			double dphi = 1.0;
			int i = 0;
			while ((Math.Abs(dphi) > 0.000000001) && (i < 15))
			{
				double con = ECCENT * Math.Sin(phi);
				dphi = (Math.PI / 2) - 2 * Math.Atan(ts * Math.Pow((1.0 - con) / (1.0 + con), 0.5 * ECCENT)) - phi;
				phi += dphi;
				i++;
			}
			return RadToDeg(phi);
		}

		static double RadToDeg(double rad)
		{
			return rad * RAD2DEG;
		}

		static double DegToRad(double deg)
		{
			return deg * DEG2RAD;
		}
	}
}

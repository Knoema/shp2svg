using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shp2svg
{
	abstract class Projection
	{
		private static readonly double DEG2RAD = Math.PI / 180.0;
		private static readonly double RAD2DEG = 180.0 / Math.PI;

		protected static double RadToDeg(double rad)
		{
			return rad * RAD2DEG;
		}

		protected static double DegToRad(double deg)
		{
			return deg * DEG2RAD;
		}

		abstract public double lonToX(double lon);
		abstract public double latToY(double lat);
	}
}

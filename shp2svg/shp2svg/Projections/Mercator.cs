using System;

namespace shp2svg
{
	class Mercator : Projection
	{
		private static readonly double R_MAJOR = 6378137.0;
		private static readonly double R_MINOR = 6356752.3142;
		private static readonly double ECCENT = Math.Sqrt(1.0 - (R_MINOR / R_MAJOR * R_MINOR / R_MAJOR));

		public override double lonToX(double lon)
		{
			return R_MAJOR * DegToRad(lon);
		}

		public override double latToY(double lat)
		{
			return R_MAJOR * Math.Log(Math.Tan(Math.PI / 4 + DegToRad(lat) / 2) * Math.Pow((1 - ECCENT * Math.Sin(DegToRad(lat))) / (1 + ECCENT * Math.Sin(DegToRad(lat))), ECCENT / 2));
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shp2svg
{
	// http://alastaira.wordpress.com/2011/01/23/the-google-maps-bing-maps-spherical-mercator-projection/

	class SphericalMercator: Projection
	{
		private static readonly double Longitude = 20037508.34;

		public override double latToY(double lat)
		{
			return Math.Log(Math.Tan((90 + DegToRad(lat)) * Math.PI / 360)) / (Math.PI / 180) * Longitude / 180;
		}

		public override double lonToX(double lon)
		{
			return DegToRad(lon) * Longitude / 180;
		}
	}
}

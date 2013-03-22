using System;
using System.IO;
using System.Linq;
using CommandLine;

namespace shp2svg
{
	class Program
	{
		static void Main(string[] args)
		{
			var options = new Options();
			var parser = new CommandLineParser();

			if (!parser.ParseArguments(args, options))
				Console.Error.Write(options.Errors());
			else
			{
				Logger.Init(Path.GetDirectoryName(options.Path));

				Projection projection;

				switch (options.Projection)
				{
 					case "EPSG:3857":
						projection = new SphericalMercator();
						break;
					default:
						projection = new Mercator();
						break;
				};
				

				var filePaths = Helper.ShpFilePaths(options.Path).ToList();
				filePaths.ForEach(fp =>
				{
					var svgWriter = new SvgWriter(fp, options.Width, options.Height, options.Tolerance, options.Attr, projection);

					svgWriter.CreateSVG();

					if (options.Meta)
						svgWriter.GetMetadata(fp);

					Logger.LogText(svgWriter.Log);
				});
 
			}			
		}
	}
}

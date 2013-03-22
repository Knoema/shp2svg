using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace shp2svg
{
	class Options : CommandLineOptionsBase
	{	
		[HelpOption(HelpText = "Dispaly this help screen.")]
		public string Errors()
		{
			var help = new HelpText();
		
			if (this.LastPostParsingState.Errors.Count > 0)
			{
				var errors = help.RenderParsingErrorsText(this, 2); 
				if (!string.IsNullOrEmpty(errors))
				{
					help.AddPreOptionsLine(string.Concat(Environment.NewLine, "ERROR(S):"));
					help.AddPreOptionsLine(errors);
				}
			}

			return help;
		}

		[Option("p", "path", Required = true, HelpText = "Path to the shapefile")]
		public string Path { get; set; }

		[Option("w", "width", DefaultValue = 900, HelpText = "Width of svg file")]
		public int Width { get; set; }

		[Option("h", "height", DefaultValue = 600, HelpText = "Height of svg file")]
		public int Height { get; set; }

		[Option("a", "attribute",Required = true, HelpText = "Name of shapefile attribute to find matching id")]
		public string Attr { get; set; }

		[Option("m", "meta", HelpText = "Generate metadata file")]
		public bool Meta { get; set; }

		[Option("t", "tolerance", DefaultValue = 3, HelpText = "Tolerance of simplification")]
		public int Tolerance { get; set; }

		[Option("r", "proj", DefaultValue = "EPSG:3395", HelpText = "Projection")]
		public string Projection { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Catfood.Shapefile;
using Newtonsoft.Json;

namespace shp2svg
{
	class SvgWriter
	{
		string _path;
		int _width;
		int _height;
		int _tolerance;
		Projection _projection;
		string _attr;

		public SvgWriter(string path, int width, int height, int tolerance, string attr, Projection projection)
		{
			_path = path;
			_width = width;
			_height = height;
			_tolerance = tolerance;
			_attr = attr;
			_projection = projection;
		}

		public List<string> Log { private set; get; }

		public void GetMetadata(string path)
		{
			using (Shapefile shapefile = new Shapefile(path))
			{
				var stringBuilder = new StringBuilder();

				var i = 0;
				foreach (Shape shape in shapefile)					
					if (shape.Type == ShapeType.Polygon)
					{
						if (i == 0)
						{
							stringBuilder.AppendLine(string.Join(", ", shape.GetMetadataNames()));
							i++;
						}

						stringBuilder.AppendLine(string.Join(", ", shape.GetMetadataNames().Select(p => shape.GetMetadata(p))));
					}				

				File.WriteAllText(Helper.GetFilePath(path, ".csv"), stringBuilder.ToString());
			}
		}

		bool CompareRegions(string region1, string region2)
		{
			region1 = region1.ToUpperInvariant().Trim();
			region2 = region2.ToUpperInvariant().Trim();

			var count = 0;

			for (var i = 0; i < Math.Min(region1.Length, region2.Length); i++)
			{
				if (region1[i] == region2[i])
					count++;
			}

			if ((double)count / region1.Length > 0.9)
				return true;

			return false;
		}

		string GetId(string name, params Region[] regions)
		{
			var id = string.Empty;

			if(name != null)
				foreach (var r in regions)
				{
					var count = 0;
					for (var i = 0; i < Math.Min(r.Name.Length, name.Length); i++)
					{
						if (r.Name[i] == name[i])
							count++;
					}

					if ((double)count / name.Length > 0.7)
						id = r.Id;
					else
						id = GetId(name, r.Regions);

					if (!string.IsNullOrEmpty(id))
						return id;
				}

			return id;
		}		

		public void CreateSVG()
		{
			if (!File.Exists(_path))
			{
				LogMessage(string.Format("File does not exist. Path: {0}", _path));
				return;
			}

			var regionsPath = Path.Combine(Path.GetDirectoryName(_path), "regions.json");
			if (!File.Exists(regionsPath))
			{
				LogMessage(string.Format("Regions file does not exist. Path: {0}", regionsPath));
				return;
			}

			var regions = JsonConvert.DeserializeObject<Region>(File.ReadAllText(regionsPath));

			using (Shapefile shapefile = new Shapefile(_path))
			{
				LogMessage(string.Format("Start Create SVG for: {0}", Path.GetFileName(_path)));

				var scale = Math.Max(
					Math.Abs(_projection.lonToX(shapefile.BoundingBox.Left) - _projection.lonToX(shapefile.BoundingBox.Right)) / _width,
					Math.Abs(_projection.latToY(shapefile.BoundingBox.Top) - _projection.latToY(shapefile.BoundingBox.Bottom)) /_height
				);

				using (var writer = XmlWriter.Create(Helper.GetFilePath(_path, ".svg"), new XmlWriterSettings() { Indent = true }))
				{
					writer.WriteStartElement("svg", "http://www.w3.org/2000/svg");
					writer.WriteAttributeString("id", "svgmapid");
					writer.WriteAttributeString("viewBox", string.Format("0 0 {0} {1}", _width, _height));
					writer.WriteAttributeString("fill", "transparent");
					writer.WriteAttributeString("stroke", "gray");
					writer.WriteAttributeString("stroke-width", "0.3");

					var failedRegions = new List<string>();

					foreach (Shape shape in shapefile)
						if (shape.Type == ShapeType.Polygon)
						{
							var region = shape.GetMetadata(_attr);
							var id = GetId(region, regions);

							foreach (PointD[] part in (shape as ShapePolygon).Parts)
							{
								writer.WriteStartElement("path");
								writer.WriteAttributeString("id", id);

								if (string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(region))
								{
									failedRegions.Add(region);
									writer.WriteAttributeString("region", region);
								}

								var coords = new List<List<double>>();
								for (int i = 0; i < part.Length; i++)
									coords.Add(new List<double>() { part[i].X, part[i].Y });

								coords = Helper.SimplifyCoordinates(coords, _tolerance);

								var stringBuilder = new StringBuilder();

								for (int i = 0; i < coords.Count; i++)
									stringBuilder.Append(string.Format("{0} {1} {2} {3}",
										   i == 0 ? "M" : "L",
										   (_projection.lonToX(coords[i][0]) - _projection.lonToX(shapefile.BoundingBox.Left)) / scale,
										   (-_projection.latToY(coords[i][1]) + _projection.latToY(shapefile.BoundingBox.Bottom)) / scale,
										   i == coords.Count - 1 ? "Z" : string.Empty
									   )
								   );

								writer.WriteAttributeString("d", stringBuilder.ToString());
								writer.WriteEndElement();
							}
						}

					writer.WriteEndElement();
					writer.Flush();
					writer.Close();

					if (failedRegions.Count > 0)
						LogMessage(string.Format("Unable to match id for region: {0}", string.Join(", ", failedRegions.Distinct())));

					LogMessage(string.Format("End Create SVG for: {0}{1}", Path.GetFileName(_path), Environment.NewLine));
				}
			}
		}		
		

		void LogMessage(string message)
		{
			if (Log == null)
				Log = new List<string>();

			Log.Add(message);
		}
	}	
}

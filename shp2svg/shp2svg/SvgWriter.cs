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
		public List<string> Log { private set; get; }

		public void GetMetadata(string path)
		{
			using (Shapefile shapefile = new Shapefile(path))
			{
				var stringBuilder = new StringBuilder();

				var i = 0;
				foreach (Shape shape in shapefile)			
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

		public void CreateSVG(string path, int width, int height, string attr, int tolerance)
		{
			if (!File.Exists(path))
			{
				LogMessage(string.Format("File does not exist. Path: {0}", path));
				return;
			}

			var regionsPath = Path.Combine(Path.GetDirectoryName(path), "regions.json");
			if (!File.Exists(regionsPath))
			{
				LogMessage(string.Format("Regions file does not exist. Path: {0}", regionsPath));
				return;
			}

			var regions = JsonConvert.DeserializeObject<Region>(File.ReadAllText(regionsPath));

			using (Shapefile shapefile = new Shapefile(path))
			{
				LogMessage(string.Format("Start Create SVG for: {0}", Path.GetFileName(path)));

				var scale = Math.Max(
					Math.Abs(Mercator.lonToX(shapefile.BoundingBox.Left) - Mercator.lonToX(shapefile.BoundingBox.Right)) / width,
					Math.Abs(Mercator.latToY(shapefile.BoundingBox.Top) - Mercator.latToY(shapefile.BoundingBox.Bottom)) / height
				);

				using (var writer = XmlWriter.Create(Helper.GetFilePath(path, ".svg"), new XmlWriterSettings() { Indent = true }))
				{
					writer.WriteStartElement("svg", "http://www.w3.org/2000/svg");
					writer.WriteAttributeString("id", "svgmapid");
					writer.WriteAttributeString("viewBox", string.Format("0 0 {0} {1}", width, height));
					writer.WriteAttributeString("fill", "transparent");
					writer.WriteAttributeString("stroke", "gray");
					writer.WriteAttributeString("stroke-width", "0.3");

					var failedRegions = new List<string>();

					foreach (Shape shape in shapefile)
						if (shape.Type == ShapeType.Polygon)
						{
							var region = shape.GetMetadata(attr);
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

								coords = Helper.SimplifyCoordinates(coords, tolerance);

								var stringBuilder = new StringBuilder();

								for (int i = 0; i < coords.Count; i++)
									 stringBuilder.Append(string.Format("{0} {1} {2} {3}",
											i == 0 ? "M" : "L",
											(Mercator.lonToX(coords[i][0]) - Mercator.lonToX(shapefile.BoundingBox.Left)) / scale,
											(-Mercator.latToY(coords[i][1]) + Mercator.latToY(shapefile.BoundingBox.Bottom)) / scale,
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

					if(failedRegions.Count > 0)
						LogMessage(string.Format("Unable to match id for region: {0}", string.Join(", ", failedRegions.Distinct())));	

					LogMessage(string.Format("End Create SVG for: {0}{1}", Path.GetFileName(path), Environment.NewLine));
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

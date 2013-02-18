using System;
using System.Collections.Generic;
using System.IO;
using Catfood.Shapefile;

namespace shp2svg
{
	class Logger
	{
		private static string LogFileName = "log.txt";
		private static string DirectoryPath = null;
		private static string FilePath = null;

		public static void Init (string dirPath)
		{
			DirectoryPath = dirPath;
			FilePath = Path.Combine(DirectoryPath, LogFileName);
			DeleteOldLogFile();
		}

		public static void LogText(string content)
		{
			content = string.Concat(content, Environment.NewLine);
			File.AppendAllText(FilePath, content); //creates a new file if it doesn't exist
		}

		public static void LogText(IEnumerable<string> contents)
		{
			File.AppendAllLines(FilePath, contents); //creates a new file if it doesn't exist
		}

		//log shapefile on console
		public static void ShapeFileLogger(string path)
		{
			if (!File.Exists(path))
			{
				Console.WriteLine("File does not exist or no input provided");
				return;
			}

			LogText(string.Format("Logging File: {0}", Path.GetFileName(path)));

			// construct shapefile with the path to the .shp file
			using (Shapefile shapefile = new Shapefile(path))
			{
				Console.WriteLine("ShapefileDemo Dumping {0}", path);
				Console.WriteLine();

				// a shapefile contains one type of shape (and possibly null shapes)
				Console.WriteLine("Type: {0}, Shapes: {1:n0}", shapefile.Type, shapefile.Count);

				// a shapefile also defines a bounding box for all shapes in the file
				Console.WriteLine("Bounds: {0},{1} -> {2},{3}",
					shapefile.BoundingBox.Left,
					shapefile.BoundingBox.Top,
					shapefile.BoundingBox.Right,
					shapefile.BoundingBox.Bottom);
				Console.WriteLine();

				// enumerate all shapes
				foreach (Shape shape in shapefile)
				{
					Console.WriteLine("----------------------------------------");
					Console.WriteLine("Shape {0:n0}, Type {1}", shape.RecordNumber, shape.Type);

					// each shape may have associated metadata
					string[] metadataNames = shape.GetMetadataNames();
					if (metadataNames != null)
					{
						Console.WriteLine("Metadata:");
						foreach (string metadataName in metadataNames)
						{
							Console.WriteLine("{0}={1} ({2})", metadataName, shape.GetMetadata(metadataName), shape.DataRecord.GetDataTypeName(shape.DataRecord.GetOrdinal(metadataName)));
						}
						Console.WriteLine();
					}

					// cast shape based on the type
					switch (shape.Type)
					{
						case ShapeType.Point:
							// a point is just a single x/y point
							ShapePoint shapePoint = shape as ShapePoint;
							Console.WriteLine("Point={0},{1}", shapePoint.Point.X, shapePoint.Point.Y);
							break;

						case ShapeType.Polygon:
							// a polygon contains one or more parts - each part is a list of points which
							// are clockwise for boundaries and anti-clockwise for holes 
							// see http://www.esri.com/library/whitepapers/pdfs/shapefile.pdf
							ShapePolygon shapePolygon = shape as ShapePolygon;
							foreach (PointD[] part in shapePolygon.Parts)
							{
								Console.WriteLine("Polygon part:");
								foreach (PointD point in part)
								{
									Console.WriteLine("{0}, {1}", point.X, point.Y);
								}
								Console.WriteLine();
							}
							break;

						default:
							// and so on for other types...
							break;
					}

					Console.WriteLine("----------------------------------------");
					Console.WriteLine();
				}

			}
			Console.WriteLine("Done");
			Console.WriteLine();
		}

		private Logger() { }

		private static void DeleteOldLogFile()
		{
			if (File.Exists(FilePath))
				File.Delete(FilePath);
		}

	}
}

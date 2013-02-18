using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace shp2svg
{
	class Helper
	{
		private static bool IsDirectory(string path)
		{
			return Directory.Exists(path);
		}

		private static bool IsFile(string path)
		{
			return File.Exists(path);
		}

		private static bool IsShpFile(string path)
		{
			var ext = Path.GetExtension(path);
			return string.Equals(ext, ".shp", StringComparison.InvariantCultureIgnoreCase);
		}

		private static IEnumerable<string> ShpFileNames(string path)
		{
			var names = new List<string>();
			if (IsFile(path))
			{
				if (IsShpFile(path))
					names.Add(Path.GetFileNameWithoutExtension(path));
			}
			else if (IsDirectory(path))
			{
				string[] filepaths = Directory.GetFiles(path);
				for (var i = 0; i < filepaths.Length; i++)
				{
					var fp = filepaths[i];
					if (IsShpFile(fp))
						names.Add(Path.GetFileNameWithoutExtension(fp));
				}
			}
			return names.Distinct();
		}

		public static IEnumerable<string> ShpFilePaths(string path)
		{
			var dirPath = Path.GetDirectoryName(path);
			var filenames = ShpFileNames(path).ToList();			
			var filePaths = new List<string>();

			filenames.ForEach(delegate(string fn)
			{
				filePaths.Add(
					Path.Combine(path, string.Concat(fn, ".shp"))
				);
			});

			return filePaths;
		}


		public static string GetFilePath(string path, string extension)
		{
			var dirPath = Path.GetDirectoryName(path);
			var fileName = Path.GetFileNameWithoutExtension(path);
			return Path.Combine(dirPath, string.Concat(fileName, extension));
		}

		public static string GetSVGFilePath(string path)
		{
			var dirPath = Path.GetDirectoryName(path);
			var fileName = Path.GetFileNameWithoutExtension(path);
			return Path.Combine(dirPath, string.Concat(fileName, ".svg"));
		}

		public static void DeleteExistingFile(string FilePath)
		{
			if (File.Exists(FilePath))
				File.Delete(FilePath);
		}

		public static List<List<double>> SimplifyCoordinates(List<List<double>> coords, int tolerance)
		{
			return MathematicalSimplification(coords, tolerance);
		}

		private static List<List<double>> MathematicalSimplification(List<List<double>> coords, int roundToDecimals = 3)
		{
			var outCoords = new List<List<double>>();
			var res = new List<List<double>>();

			coords.ForEach(point =>
			{
				var outPoint = new List<double>();
				point.ForEach(x =>
				{
					outPoint.Add(Math.Round(x, roundToDecimals)); //rounding
				});
				outCoords.Add(outPoint);
			});

			List<double> prevPoint = null;
			outCoords.ForEach(point =>
			{
				bool same = false;
				if (prevPoint != null)
				{
					for (var i = 0; i < point.Count; i++)
					{
						if (point[i] == prevPoint[i])
							same = true;
					}
				}
				if (!same)
				{
					res.Add(point); //remove 2 similar consecutive points
					prevPoint = point;
				}
			});

			return res;
		}
	}
}

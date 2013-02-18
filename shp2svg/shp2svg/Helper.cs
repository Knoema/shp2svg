using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace shp2svg
{
	class Helper
	{
		static bool IsDirectory(string path)
		{
			return Directory.Exists(path);
		}

		static bool IsFile(string path)
		{
			return File.Exists(path);
		}

		static bool IsShpFile(string path)
		{
			return string.Equals(Path.GetExtension(path), ".shp", StringComparison.InvariantCultureIgnoreCase);
		}

		static IEnumerable<string> ShpFileNames(string path)
		{
			var names = new List<string>();

			if (IsFile(path))
			{
				if (IsShpFile(path))
					names.Add(path);
			}
			else
			{
				string[] files;

				if(IsDirectory(path))
					files = Directory.GetFiles(path);
				else
					files = Directory.GetFiles(Directory.GetParent(path).FullName, Path.GetFileName(path));

				foreach (var file in files)
					if (IsShpFile(file))
						names.Add(file);
			}

			return names.Distinct();
		}

		public static IEnumerable<string> ShpFilePaths(string path)
		{
			var filenames = ShpFileNames(path).ToList();			
			var filePaths = new List<string>();

			filenames.ForEach(delegate(string fn)
			{
				filePaths.Add(
					Path.Combine(path, fn)
				);
			});

			return filePaths;
		}

		public static string GetFilePath(string path, string extension)
		{
			return Path.Combine(Path.GetDirectoryName(path), string.Concat(Path.GetFileNameWithoutExtension(path), extension));
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

		static List<List<double>> MathematicalSimplification(List<List<double>> coords, int roundToDecimals = 3)
		{
			var outCoords = new List<List<double>>();
			var result = new List<List<double>>();

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
					result.Add(point); //remove 2 similar consecutive points
					prevPoint = point;
				}
			});

			return result;
		}
	}
}

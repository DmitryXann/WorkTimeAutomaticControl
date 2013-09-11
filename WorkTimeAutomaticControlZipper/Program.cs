using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace WorkTimeAutomaticControlZipper
{
	class Program
	{
		static int Main(string[] args)
		{
			if (args.Length < 2)
				return -1;

			var selectedPath = new DirectoryInfo(args[0]);
			var fullFileName = args[1];

			if (!selectedPath.Exists)
				return -2;

			if (Path.GetInvalidFileNameChars().Any(fullFileName.Contains))
				return -3;

			if (!fullFileName.Contains('.'))
				return -4;

			var selectedFullFilePath = Path.Combine(Path.GetDirectoryName(selectedPath.FullName), fullFileName);

			try
			{
				if (File.Exists(selectedFullFilePath))
					File.Delete(selectedFullFilePath);
			}
			catch (IOException ex)
			{
				Console.WriteLine("IO exception: {0}", ex.Message);
				return -5;
			}
			catch (Exception ex)
			{
				Console.WriteLine("File handling exception: {0}", ex.Message);
				return -6;
			}

			try
			{
				Console.Write(selectedFullFilePath);
				ZipFile.CreateFromDirectory(selectedPath.FullName, selectedFullFilePath, CompressionLevel.NoCompression, false);
				return 0; 
			}
			catch (IOException ex)
			{
				Console.WriteLine("IO exception: {0}", ex.Message);
				return -5;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Zip exception: {0}", ex.Message);
				return -7;
			}
		}
	}
}

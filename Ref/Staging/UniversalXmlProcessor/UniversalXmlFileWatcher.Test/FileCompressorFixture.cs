using System.IO;
using CargoWise.RefDbRepo.UniversalXmlFileWatcher.Utilities;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXmlFileWatcher.Tests
{
	[TestFixture]
	public class FileCompressorFixture
	{
		[TestCase]
		public void CompressFile()
		{
			var tmpPath = Path.GetTempPath();
			tmpPath = Path.Combine(tmpPath, "Archive_TestFolder");
			if (Directory.Exists(tmpPath))
			{
				Directory.Delete(tmpPath, true);
			}
			Directory.CreateDirectory(tmpPath);
			var tmpXmlFile = Path.Combine(tmpPath, "TestXmlFile.xml");
			var tmpTxtFile = Path.Combine(tmpPath, "TestTxtFile_2021_0122.txt");
			File.WriteAllText(tmpXmlFile, "1234567890");
			File.WriteAllText(tmpTxtFile, "1234567890");
			var destXmlFile = Path.Combine(tmpPath, "TestXmlFile.zip");
			var destTxtFile = Path.Combine(tmpPath, "TestTxtFile_2021_0122.zip");

			var compressor = new FileCompressor();

			Assert.False(File.Exists(destXmlFile));
			Assert.False(File.Exists(destTxtFile));
			compressor.CompressFile(tmpXmlFile, destXmlFile);
			compressor.CompressFile(tmpTxtFile, destTxtFile);
			Assert.True(File.Exists(destXmlFile));
			Assert.True(File.Exists(destTxtFile));

			if (Directory.Exists(tmpPath))
			{
				Directory.Delete(tmpPath, true);
			}
		}
	}
}

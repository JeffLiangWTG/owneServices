using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Tests
{
	[TestFixture]
	class ZipFileHelperTests
	{
		[Test]
		public void TestThrowsFileNotFoundException()
		{
			string nonExistentFile = "nonexistent.zip";

			Assert.Throws<FileNotFoundException>(() => ZipFileHelper.GetZipHelper(nonExistentFile));
		}

		[Test]
		public void TestThrowsZipException()
		{
			var invalidFormatFile = Path.Combine(Path.GetTempPath(), "wrongformat.txt");
			using (var fs = File.Create(invalidFormatFile))
			{
				Assert.Throws<ZipException>(() => ZipFileHelper.GetZipHelper(invalidFormatFile));
			}
		}

		[Test]
		public void TestReturnsPKZipHelper()
		{
			var fileName = "example.zip";
			CreateDummyZipFile(fileName);
			Assert.IsInstanceOf<PKZipHelper>(ZipFileHelper.GetZipHelper(fileName));
		}

		[Test]
		public void TestReturnsGZipHelper()
		{
			var fileName = "example.gz";
			CreateDummyGZFile(fileName);
			Assert.IsInstanceOf<GZipHelper>(ZipFileHelper.GetZipHelper(fileName));
		}

		[Test]
		public void TestPKZipHelper_Unzipped()
		{
			string compressedFile = "example.zip";
			string outputPath = Path.GetTempPath();

			CreateDummyZipFile(compressedFile);

			new PKZipHelper().UnzipFile(compressedFile, outputPath);

			Assert.IsTrue(Directory.Exists(outputPath), "Expected output directory does not exist");
			Assert.IsTrue(Directory.GetFiles(outputPath).Length > 0, "Expected files were not extracted");
		}

		[Test]
		public void TestGZipHelper_Unzipped()
		{
			string zipFile = "example.gz";
			string outputPath = Path.GetTempPath();

			CreateDummyGZFile(zipFile);

			new GZipHelper().UnzipFile(zipFile, outputPath);

			string outputFile = Path.Combine(outputPath, "example.xml");
			Assert.IsTrue(File.Exists(outputFile), "Expected output file does not exist");
		}

		void CreateDummyGZFile(string fileName)
		{
			using (var outputFileStream = new FileStream(fileName, FileMode.Create))
			{
				using (var gzipStream = new System.IO.Compression.GZipStream(outputFileStream, System.IO.Compression.CompressionMode.Compress))
				{
					using (var writer = new StreamWriter(gzipStream))
					{
						writer.Write("<dummy></dummy>");
					}
				}
			}
		}

		void CreateDummyZipFile(string fileName)
		{
			using (var zipFileStream = new FileStream(fileName, FileMode.Create))
			{
				using (var zipOutputStream = new ZipOutputStream(zipFileStream))
				{
					ZipEntry entry = new ZipEntry("example.txt");
					zipOutputStream.PutNextEntry(entry);
					byte[] content = System.Text.Encoding.UTF8.GetBytes("Test file content");
					zipOutputStream.Write(content, 0, content.Length);
					zipOutputStream.CloseEntry();
				}
			}
		}
	}
}

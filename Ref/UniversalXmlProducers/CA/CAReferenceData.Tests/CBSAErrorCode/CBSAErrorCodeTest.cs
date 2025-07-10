using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.CBSAErrorCode;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests.CBSAErrorCode
{
	[TestFixture]
	public class CBSAErrorCodeTest
	{
		class CBSAErrorCodeFileDownloaderForTest : CBSAErrorCodeFileDownloader
		{
			public CBSAErrorCodeFileDownloaderForTest(string rootURLForTest, PreProcessChecker checker) : base(checker)
			{
				RootURLForTest = rootURLForTest;
			}

			public string RootURLForTest { get; set; }

			protected override string RootURL => RootURLForTest;
		}

		const string parentPath = "CBSAErrorCode";

		[Test]
		public void TestDownloadFile()
		{
			checker.MarkAsProcessRequired();
			using (var stream2 = TestHelper.GetTestInputFile(string.Format(CultureInfo.InvariantCulture, "{0}.error-erreur.csv", parentPath)))
			using (var stream = TestHelper.GetTestInputFile(string.Format(CultureInfo.InvariantCulture, "{0}.DownloadPage.html", parentPath)))
			using (var htmlStream = new StreamReader(stream))
			{
				var html = htmlStream.ReadToEnd();
				var downloader = new CBSAErrorCodeFileDownloaderForTest("", checker);
				var fileName = Path.GetTempFileName();
				var downLoadCSVPath = Path.Combine(TestHelper.GetCurrentFolder(), @"TestFiles\error-erreur.csv");
				SaveFileTo(stream2, downLoadCSVPath);
				html = html.Replace(@"error-erreur.csv", downLoadCSVPath);
				var result = downloader.DownloadFile("", fileName, false, html);
				Assert.IsTrue(result);
				Assert.True(string.IsNullOrEmpty(downloader.ErrorBuilder.ToString()));
				Assert.That(downloader.PublicationTime, Is.EqualTo(new DateTime(2022, 05, 03)));
			}
		}

		[Test]
		public void TestFailedDownloadFile()
		{
			checker.MarkAsProcessRequired();

			using (var stream = TestHelper.GetTestInputFile(string.Format(CultureInfo.InvariantCulture, "{0}.DownloadPage.html", parentPath)))
			using (var htmlStream = new StreamReader(stream))
			{
				var html = htmlStream.ReadToEnd();
				var downloader = new CBSAErrorCodeFileDownloaderForTest("", checker);
				var fileName = Path.GetTempFileName();
				html = html.Replace(@"error-erreur.csv", "https://www.cbsa-asfc.gc.ca/error-erreur.csv");
				var result = downloader.DownloadFile("", fileName, false, html);
				Assert.IsTrue(!result);
				Assert.IsTrue(downloader.ErrorBuilder.ToString().Contains("File downloads failed. URL:"));

				checker.MarkAsProcessRequired();
				html = html.Replace(@"https://www.cbsa-asfc.gc.ca/error-erreur.csv", "Test");
				result = downloader.DownloadFile("", fileName, false, html);
				Assert.IsTrue(!result);
				Assert.IsTrue(downloader.ErrorBuilder.ToString().Contains("The data file node is not found in the web page, the page layout may have changed."));
			}
		}

		[Test]
		public void TestReadCSVAndExportXML()
		{
			var path = Path.GetTempFileName();
			using (var stream = TestHelper.GetTestInputFile(string.Format(CultureInfo.InvariantCulture, "{0}.error-erreur.csv", parentPath)))
			using (var file = File.Create(path))
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(file);
				stream.Close();
				file.Close();
				var exportFilePath = @"..\..\UniversalXmlProducers\CA\CAReferenceData.Tests\TestFiles\Output\";
				var publicationTime = DateTime.Now;
				var reader = new DataReader(path, exportFilePath, publicationTime);
				
				var result = reader.ReadCSVAndExportXML();
				var file1 = new FileInfo(Path.Combine(exportFilePath, publicationTime.Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "_CACBSAErrorCodes.xml"));
				Assert.IsTrue(result);
				Assert.IsTrue(file1.Exists);
				file1.Delete();
			}
		}

		void SaveFileTo(Stream stream, string path)
		{
			byte[] srcBuf = new byte[stream.Length];
			stream.Read(srcBuf, 0, srcBuf.Length);
			stream.Seek(0, SeekOrigin.Begin);
			using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
			{
				fs.Write(srcBuf, 0, srcBuf.Length);
				fs.Close();
			}
		}

		[SetUp]
		public void SetUp()
		{
			checker = new PreProcessChecker(Constants.ProgramFunctions.CBSAErrorCode);
		}
		PreProcessChecker checker;
	}
}

using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using CargoWise.RefDbRepo.NZReferenceData.Business;
using CargoWise.RefDbRepo.NZReferenceData.CmdLine;
using CargoWise.RefDbRepo.NZReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests
{
	[TestFixture]
	public class SupplierParserTest
	{
		[Test]
		public void TestSupplierList()
		{
			if (File.Exists(SupplierParser.DataFilePath))
			{
				File.Delete(SupplierParser.DataFilePath);
			}

			var localFilePath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"Supplier\TestFiles\Input\SupplierList.txt");
			var expectedImportXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NZReferenceData.Tests.Supplier.TestFiles.Output.RefCusCodeListZZ_NZ_Supplier.xml");

			var outputFolderPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"TestFiles");
			Directory.CreateDirectory(outputFolderPath);

			var outputFileFullName = Path.Combine(outputFolderPath, "RefCusCodeListZZ_NZ_Supplier.xml");
			File.Delete(outputFileFullName);

			SupplierParser.ExportToXMLFile(localFilePath, outputFolderPath, new DateTime(2020, 05, 01));

			var actualOutputXml = File.ReadAllText(outputFileFullName);

			Assert.That(expectedImportXML, Is.EqualTo(actualOutputXml));

		}

		public void TestTempFileDeletedAfterRun()
		{
			var mockDownloader = new Mock<SupplierListFileDownloder>();
			string tempFilePath = null;

			mockDownloader
				.Setup(d => d.Download(It.IsAny<HttpClient>(), It.IsAny<string>(), It.IsAny<string>()))
				.Callback<HttpClient, string, string>((client, url, localFile) => tempFilePath = localFile)
				.Returns((true, new DateTime(2024, 01, 01)));

			using (var httpClient = new HttpClient())
			{
				var supplierUrl = "https://www.customs.govt.nz/api/datafiles/master-supplier-codes";
				SupplierListProgram.RunCore(httpClient, mockDownloader.Object, supplierUrl, new Logger());

				Assert.False(File.Exists(tempFilePath), "Temporary file should be deleted after the Supplier List Program run.");
			}
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
			DeleteDataFile();
		}
		Assembly assembly;

		[TearDown]
		public void TearDown()
		{
			DeleteDataFile();
		}

		void DeleteDataFile()
		{
			if (File.Exists(SupplierParser.DataFilePath))
			{
				File.Delete(SupplierParser.DataFilePath);
			}
		}
	}
}

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.ILReferenceData.Business;
using CargoWise.RefDbRepo.ILReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.Business
{
	[TestFixture]
	public sealed class ILCustomsCodesProcessorTest
	{
		[Test]
		public void TestGenerateFilesFromTableData()
		{
			const string tableName = "2012";
			string message = TestHelper.GetManifestResourceStream($"CargoWise.RefDbRepo.ILReferenceData.Tests.TestFiles.SystemTableResponse_{tableName}.xml");
			var testFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();

			var dataSet = GetDataSet(message);
			AssertOutputFileFromTable(tableName, dataSet, "IL_FAC.xml");
		}

		[Test]
		public void TestGenerateFilesFromTableData_GenrateMultiDataSource_When1091()
		{
			const string tableName = "1091";
			string message = TestHelper.GetManifestResourceStream($"CargoWise.RefDbRepo.ILReferenceData.Tests.TestFiles.SystemTableResponse_{tableName}.xml");
			var testFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();

			var dataSet = GetDataSet(message);


			AssertOutputFileFromTable(tableName, dataSet, "IL_PKG.xml");
			AssertOutputFileFromTable(tableName, dataSet, "IL_MPKG.xml");
		}

		static void AssertOutputFileFromTable(string tableName, string dataSet, string outputFileName)
		{
			var publicationDate = DateTime.ParseExact("2024-07-18 13:00:00", "yyyy-MM-dd HH:mm:ss", null);
			var generator = new ILCustomsCodesProcessor(new Logger());
			generator.GenerateFiles(publicationDate, tableName, dataSet);

			var outputFilePath = Path.Combine(ApplicationConfig.Instance.OutputDirectory, outputFileName);
			Assert.True(File.Exists(outputFilePath));
			var actualFileContent = File.ReadAllText(outputFilePath).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
			var expectedFileContent = TestHelper.GetManifestResourceStream($"CargoWise.RefDbRepo.ILReferenceData.Tests.TestFiles.{outputFileName}").Replace("\r\n", string.Empty).Replace("\n", string.Empty);
			Assert.AreEqual(expectedFileContent, actualFileContent);
		}

		string GetDataSet(string message)
		{
			using (var readerString = new StringReader(message))
			{
				using (var readerXml = XmlReader.Create(readerString))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(SYSTBL_NG_9001_MSG_SystemTablesResponse));
					var response = (SYSTBL_NG_9001_MSG_SystemTablesResponse)serializer.Deserialize(readerXml);

					return response.TableAsDataSetTableData;
				}
			}
		}
	}
}

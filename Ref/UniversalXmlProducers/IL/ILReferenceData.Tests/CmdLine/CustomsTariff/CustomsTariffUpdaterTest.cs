using System;
using System.IO;
using CargoWise.RefDbRepo.ILReferenceData.CmdLine;
using CargoWise.RefDbRepo.ILReferenceData.Services;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using XMLTools;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.CmdLine
{
	[TestFixture]
	public class CustomsTariffUpdaterTest
	{
		[Test]
		public void TestGenerateFiles()
		{
			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(dtp => dtp.Now).Returns(DateTime.ParseExact("20240220", "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture));
			ILReferenceData.Services.DateTimeUtil.DateTimeProvider = dateTimeProvider.Object;

			var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			var customsItemComputedDataFilePath = Path.Combine(tempPath, "CustomsItemComputedData_Test.xml");
			var customsItemDetailsHistoryFilePath = Path.Combine(tempPath, "CustomsItemDetailsHistory_Test.xml");
			var propertiesDetailsHistoryFilePath = Path.Combine(tempPath, "PropertiesDetailsHistory_Test.xml");
			var testFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");

			Directory.CreateDirectory(tempPath);
			ILReferenceData.Business.ApplicationConfig.Instance.OutputDirectory = tempPath;
			var outputFileName = "ILTariffs.xml";
			var outputFilePath = Path.Combine(ILReferenceData.Business.ApplicationConfig.Instance.OutputDirectory, outputFileName);

			try
			{
				var customsItemComputedData = TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.ILReferenceData.Tests.TestFiles.CustomsItemComputedData_Test.xml");
				var customsItemDetailsHistory = TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.ILReferenceData.Tests.TestFiles.CustomsItemDetailsHistory_Test.xml");
				var propertiesDetailsHistory = TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.ILReferenceData.Tests.TestFiles.PropertiesDetailsHistory_Test.xml");

				File.WriteAllText(customsItemComputedDataFilePath, customsItemComputedData);
				File.WriteAllText(customsItemDetailsHistoryFilePath, customsItemDetailsHistory);
				File.WriteAllText(propertiesDetailsHistoryFilePath, propertiesDetailsHistory);

				CustomsTariffUpdater.Run(new Logger(), new string[] { "CUSTOMS_TARIFF_UPDATE", tempPath });

				Assert.True(File.Exists(outputFilePath));
				var expectedFileContent = File.ReadAllText(Path.Combine(testFilesFolder, outputFileName));
				var actualFileContent = File.ReadAllText(outputFilePath);
				XmlComparer xmlComparer = new XmlComparer();
				xmlComparer.CompareXml(expectedFileContent, actualFileContent, true);
			}
			finally
			{
				SafeDelete(outputFilePath);
				SafeDelete(customsItemComputedDataFilePath);
				SafeDelete(customsItemDetailsHistoryFilePath);
				SafeDelete(propertiesDetailsHistoryFilePath);
			}
		}

		static void SafeDelete(string tempPath)
		{
			if (!tempPath.IsNullOrEmpty() &&  File.Exists(tempPath))
			{
				File.Delete(tempPath);
			}
		}
	}
}

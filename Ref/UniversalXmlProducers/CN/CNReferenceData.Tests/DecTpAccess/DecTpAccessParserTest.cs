using System;
using System.IO;
using CargoWise.RefDbRepo.CNReferenceData.Business;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CNReferenceData.Tests
{
	[TestFixture]
	public class DecTpAccessParserTest : TestBase
	{
		[Test]
		public void TestExportToXMLFile()
		{
			GlobalOption.Instance.LogGetter = () => new Logger(Logger.LogLevel.Debug);

			using (GlobalOption.Instance.CreateDisposableLog("CN DecTpAccess Program"))
			{
				var tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				Directory.CreateDirectory(tempFolder);

				var inputResourceName = "CargoWise.RefDbRepo.CNReferenceData.Tests.DecTpAccess.TestFiles.Input.DesignatedSupervisionSiteList.xls";
				var inputStream = TestHelper.GetManifestResourceStream(inputResourceName);

				var expectedResourceName = "CargoWise.RefDbRepo.CNReferenceData.Tests.DecTpAccess.TestFiles.Output.RefCusCodeList_CN_DSS.xml";

				var outputFilePath = Path.Combine(tempFolder, "RefCusCodeList_CN_DSS.xml");
				new DecTpAccessParser().ExportToXMLFile(inputStream, outputFilePath, GlobalOption.Instance.Setting.GetDataSource(Constants.ProgramFunctions.DecTpAccess), new DateTime(2021, 06, 02, 15, 18, 01, 983));

				TestHelper.AssertXmlFileContentEquals(expectedResourceName, outputFilePath);

				File.Delete(outputFilePath);
			}
		}

		[Test]
		public void TestExportToXMLFileWithSetting()
		{
			GlobalOption.Instance.LogGetter = () => new Logger(Logger.LogLevel.Debug);

			using (GlobalOption.Instance.CreateDisposableLog("CN DecTpAccess Program"))
			{
				var tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				Directory.CreateDirectory(tempFolder);

				var inputFileName = "Designated_Supervision_Site_List_Grain.xls";
				var inputResourceName = "CargoWise.RefDbRepo.CNReferenceData.Tests.DecTpAccess.TestFiles.Input." + inputFileName;

				var inputStream = TestHelper.GetManifestResourceStream(inputResourceName);

				var expectedResourceName = "CargoWise.RefDbRepo.CNReferenceData.Tests.DecTpAccess.TestFiles.Output.RefCusCodeList_CN_DSSGN.xml";

				var outputFilePath = Path.Combine(tempFolder, "RefCusCodeList_CN_DSSGN.xml");

				Console.WriteLine(outputFilePath);

				var loadSetting = new DecTpAccessLoader.LoadSetting()
				{
					StartRow = 4,
					ColumnIndexForCustomsCode = 9,
					ColumnIndexForName = 4,
					ColumnIndexForType = 0,
					ColumnIndexForCustomsDistrict = 2,
					FixedType = "DSSGN"
				};

				var jsonForLoadSetting = JsonConvert.SerializeObject(loadSetting);

				new DecTpAccessParser(jsonForLoadSetting).ExportToXMLFile(inputStream, outputFilePath,
					GlobalOption.Instance.Setting.GetDataSource(Constants.ProgramFunctions.DecTpAccess),
					new DateTime(2022, 09, 01, 15, 18, 01, 983));

				TestHelper.AssertXmlFileContentEquals(expectedResourceName, outputFilePath);

				File.Delete(outputFilePath);
			}
		}
	}
}

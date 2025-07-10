using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using NPOI.XSSF.UserModel;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test.ExcelEntityUpdater
{
	[TestFixture]
	class ProcessedRowsTest
	{
		[Test]
		public void TestIsAlreadyProcessed()
		{
			var dataFile = Path.Combine(TestHelper.BaseTestFilePath, @"OGA\Input\2018\OGA_2018DataFile_Sample.xlsx");
			var configFile = @"Res\OGA\2018\OGA_2018Configuration_Import.xml";
			var configuration = EntityConfigurationManager.DeserializeXML<EntityConfiguration>(configFile);
			var workbook = new XSSFWorkbook(dataFile);
			var firstRow = workbook.GetSheetAt(0).GetRow(1);//0101211000, 13, 동물검역증명서
			var secondRow = workbook.GetSheetAt(0).GetRow(2);//0101211000, 71, 국제적멸종위기 동식물 수입허가서

			var processedRows = new ProcessedRows();
			Assert.IsTrue(!processedRows.IsAlreadyProcessed(firstRow, configuration));
			processedRows.RegisterProcessed(firstRow, configuration);
			Assert.IsTrue(processedRows.IsAlreadyProcessed(firstRow, configuration));

			Assert.IsTrue(!processedRows.IsAlreadyProcessed(secondRow, configuration));
			processedRows.RegisterProcessed(secondRow, configuration);
			Assert.IsTrue(processedRows.IsAlreadyProcessed(secondRow, configuration));
		}
	}
}

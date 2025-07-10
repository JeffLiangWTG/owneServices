using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class UnitOfMeasurementNaccsXmlWriterTest : BaseNaccsXmlWriterTest
	{
		protected override string InputFilePath => @"TestFiles\suuryo-2.csv";

		protected override string ExpectedOutputFilePath => @"TestFiles\ExpectedUnitOfMeasurement.xml";

		protected override string ActualOutputFilePath => $"RefCusCodeList_JP_UnitOfMeasurement.xml";

		protected override string FileDownloadUrl => AppConfig.NACCS.CodeLists.UnitOfMeasurementCsvFileDownloadUrl;

		protected override NaccsXmlWriter GetNaccsXmlWriter() => new UnitOfMeasurementNaccsXmlWriter();
	}
}

using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class ConsumptionTaxExemptionCodeExportXMLWriterTest : BaseNaccsXmlWriterTest
	{
		protected override string InputFilePath => @"Services\NACCS\ConsumptionTaxExemptionCodeExport\TestFiles\Input\naisho-e.csv";

		protected override string ExpectedOutputFilePath => @"Services\NACCS\ConsumptionTaxExemptionCodeExport\TestFiles\Output\ExpectedRefCusCodeList_JP_ConsumptionTaxExemptionCodeExport.xml";

		protected override string ActualOutputFilePath => $"RefCusCodeList_JP_ConsumptionTaxExemptionCodeExport.xml";

		protected override string FileDownloadUrl => AppConfig.NACCS.CodeLists.ConsumptionTaxExemptionCodeExportFileDownloadParentUrl;

		protected override NaccsXmlWriter GetNaccsXmlWriter() => new ConsumptionTaxExemptionCodeExportXMLWriter();
	}
}

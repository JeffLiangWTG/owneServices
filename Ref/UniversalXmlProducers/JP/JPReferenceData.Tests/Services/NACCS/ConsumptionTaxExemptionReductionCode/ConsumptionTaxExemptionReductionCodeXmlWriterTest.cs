using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class ConsumptionTaxExemptionReductionCodeXmlWriterTest : BaseNaccsXmlWriterTest
	{
		protected override string InputFilePath => @"Services\NACCS\ConsumptionTaxExemptionReductionCode\TestFiles\Input\naimen_new.csv";

		protected override string ExpectedOutputFilePath => @"Services\NACCS\ConsumptionTaxExemptionReductionCode\TestFiles\Output\ExpectedRefCusCodeList_JP_ConsumptionTaxExemptionReductionCode.xml";

		protected override string ActualOutputFilePath => $"RefCusCodeList_JP_ConsumptionTaxExemptionReductionCode.xml";

		protected override string FileDownloadUrl => AppConfig.NACCS.CodeLists.ConsumptionTaxExemptionReductionCodeCsvFileDownloadUrl;

		protected override NaccsXmlWriter GetNaccsXmlWriter() => new ConsumptionTaxExemptionReductionCodeXmlWriter();
	}
}

using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class DutyExemptionCodeWriterTest : BaseNaccsXmlWriterTest
	{
		protected override string InputFilePath => @"TestFiles\genmen-i.csv";

		protected override string ExpectedOutputFilePath => @"TestFiles\ExpectedRefCusCodeList_JP_DutyExemptionCode.xml";

		protected override string ActualOutputFilePath => $"RefCusCodeList_JP_DutyExemptionCode.xml";

		protected override string FileDownloadUrl => AppConfig.NACCS.CodeLists.DutyExemptionCodeCsvFileDownloadUrl;

		protected override NaccsXmlWriter GetNaccsXmlWriter() => new DutyExemptionCodeXmlWriter();
	}
}

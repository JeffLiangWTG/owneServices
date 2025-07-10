using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class DutyExemptionRefundCodeWriterTest : BaseNaccsXmlWriterTest
	{
		protected override string InputFilePath => @"TestFiles\genmen-e.csv";

		protected override string ExpectedOutputFilePath => @"TestFiles\ExpectedRefCusCodeList_JP_DutyExemptionRefundCode.xml";

		protected override string ActualOutputFilePath => $"RefCusCodeList_JP_DutyExemptionRefundCode.xml";

		protected override string FileDownloadUrl => AppConfig.NACCS.CodeLists.DutyExemptionRefundCodeCsvFileDownloadUrl;

		protected override NaccsXmlWriter GetNaccsXmlWriter() => new DutyExemptionRefundCodeXmlWriter();
	}
}

using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class ImportApprovalCertificateNumberXmlWriterTest : BaseNaccsXmlWriterTest
	{
		protected override string InputFilePath => @"TestFiles\yunyusho1.csv";

		protected override string ExpectedOutputFilePath => @"TestFiles\ExpectedRefCusCodeList_JP_ImportApprovalCertificateNumber.xml";

		protected override string ActualOutputFilePath => $"RefCusCodeList_JP_ImportApprovalCertificateNumber.xml";

		protected override string FileDownloadUrl => AppConfig.NACCS.CodeLists.ImportApprovalCertificateNumberFileDownloadUrl;

		protected override NaccsXmlWriter GetNaccsXmlWriter() => new ImportApprovalCertificateNumberXmlWriter();
	}
}

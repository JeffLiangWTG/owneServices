using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class ExportApprovalCertificateTypeXmlWriterTest : BaseNaccsXmlWriterTest
	{
		protected override string InputFilePath => @"TestFiles\yushutusho1.csv";

		protected override string ExpectedOutputFilePath => @"TestFiles\ExpectedRefCusCodeList_JP_ExportApprovalCertificateType.xml";

		protected override string ActualOutputFilePath => $"RefCusCodeList_JP_ExportApprovalCertificateType.xml";

		protected override string FileDownloadUrl => AppConfig.NACCS.CodeLists.ExportApprovalCertificateCsvFileDownloadUrl;

		protected override NaccsXmlWriter GetNaccsXmlWriter() => new ExportApprovalCertificateTypeXmlWriter();
	}
}

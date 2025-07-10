using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class CertificateOfOriginTypeCodeXMLWriterTest : BaseNaccsXmlWriterTest
	{
		protected override string InputFilePath => @"Services\NACCS\CertificateOfOriginTypeCode\TestFiles\Input\gensanchi.csv";

		protected override string ExpectedOutputFilePath => @"Services\NACCS\CertificateOfOriginTypeCode\TestFiles\Output\CertificateOfOriginTypeCode.xml";

		protected override string ActualOutputFilePath => $"CertificateOfOriginTypeCode.xml";

		protected override string FileDownloadUrl => AppConfig.NACCS.CodeLists.CertificateOfOriginTypeCodeFileDownloadUrl;

		protected override NaccsXmlWriter GetNaccsXmlWriter() => new CertificateOfOriginTypeCodeXMLWriter();
	}
}

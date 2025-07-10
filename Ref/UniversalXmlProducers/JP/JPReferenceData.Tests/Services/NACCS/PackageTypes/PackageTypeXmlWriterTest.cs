using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class PackageTypeXmlWriterTest : BaseNaccsXmlWriterTest
	{
		protected override string InputFilePath => @"TestFiles\housou.csv";

		protected override string ExpectedOutputFilePath => @"TestFiles\ExpectedRefCusCodeList_JP_PackageType.xml";

		protected override string ActualOutputFilePath => $"RefCusCodeList_JP_PackageType.xml";

		protected override string FileDownloadUrl => AppConfig.NACCS.CodeLists.PackageTypeCsvFileDownloadUrl;

		protected override NaccsXmlWriter GetNaccsXmlWriter() => new PackageTypeXmlWriter();
	}
}

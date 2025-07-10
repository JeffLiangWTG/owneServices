using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class ContainerTypeXmlWriterTest : BaseNaccsXmlWriterTest
	{
		protected override string InputFilePath => @"TestFiles\konte-1.csv";

		protected override string ExpectedOutputFilePath => @"TestFiles\ExpectedRefCusCodeList_JP_ContainerType.xml";

		protected override string ActualOutputFilePath => $"RefCusCodeList_JP_ContainerType.xml";

		protected override string FileDownloadUrl => AppConfig.NACCS.CodeLists.ContainerTypeCsvFileDownloadUrl;

		protected override NaccsXmlWriter GetNaccsXmlWriter() => new ContainerTypeXmlWriter();
	}
}

using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class ContainerHeightXmlWriterTest : BaseNaccsXmlWriterTest
	{
		protected override string InputFilePath => @"TestFiles\konte-2.csv";

		protected override string ExpectedOutputFilePath => @"TestFiles\ExpectedRefCusCodeList_JP_ContainerHeight.xml";

		protected override string ActualOutputFilePath => $"RefCusCodeList_JP_ContainerHeight.xml";

		protected override string FileDownloadUrl => AppConfig.NACCS.CodeLists.ContainerLengthAndHeightCsvFileDownloadUrl;

		protected override NaccsXmlWriter GetNaccsXmlWriter() => new ContainerHeightXmlWriter();
	}
}

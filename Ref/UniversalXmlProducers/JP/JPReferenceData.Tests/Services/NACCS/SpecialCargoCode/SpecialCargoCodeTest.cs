using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class SpecialCargoCodeTest : BaseNaccsXmlWriterTest
	{
		protected override string InputFilePath => @"TestFiles\spc.csv";

		protected override string ExpectedOutputFilePath => @"TestFiles\ExpectedRefCusCodeList_JP_SpecialCargoCode.xml";

		protected override string ActualOutputFilePath => $"RefCusCodeList_JP_SpecialCargoCode.xml";

		protected override string FileDownloadUrl => AppConfig.NACCS.CodeLists.SpecialCargoCodeCsvFileDownloadUrl;

		protected override NaccsXmlWriter GetNaccsXmlWriter() => new SpecialCargoCodeXmlWriter();
	}
}

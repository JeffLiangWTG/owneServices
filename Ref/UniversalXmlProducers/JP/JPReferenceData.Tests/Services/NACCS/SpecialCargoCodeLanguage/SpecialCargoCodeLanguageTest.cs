using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class SpecialCargoCodeLanguageTest : BaseNaccsXmlWriterTest
	{
		protected override string InputFilePath => @"TestFiles\spc.csv";

		protected override string ExpectedOutputFilePath => @"TestFiles\ExpectedRefCusCodeList_JP_SpecialCargoCodeEngDesc.xml";

		protected override string ActualOutputFilePath => $"RefCusCodeList_JP_SpecialCargoCodeEngDesc.xml";

		protected override string FileDownloadUrl => AppConfig.NACCS.CodeLists.SpecialCargoCodeCsvFileDownloadUrl;

		protected override NaccsXmlWriter GetNaccsXmlWriter() => new SpecialCargoCodeWithLanguageXmlWriter();

		protected override string InputErrorFilePath => @"TestFiles\spcWrong.csv";

		protected override string ExpectErrorMessage => "Hard coded file for refCusCodeListLanguage is not corresponded with automatically updated refCusCode, please notify the product manager to update it.";
	}
}

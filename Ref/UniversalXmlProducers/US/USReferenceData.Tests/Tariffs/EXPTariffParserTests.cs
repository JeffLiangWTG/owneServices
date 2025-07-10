using System;
using CargoWise.RefDbRepo.USReferenceData.Business;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	class EXPTariffParserTests : TariffParserTests
	{
		protected override string TariffType => Constants.TariffTypes.EXP;

		protected override string WebServiceMockResultFileName => Constants.DownloadFileNames.EXP;

		protected override Type ParserObjectType => typeof(EXPTariffParser);

		protected override string DownloadFileName => Constants.DownloadFileNames.EXP;

		protected override string UpdateInfoNodeKeyword => Constants.FileStructureAndUpdateInfoNodeKeywords.EXP;
	}
}

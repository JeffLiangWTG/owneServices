using System;
using CargoWise.RefDbRepo.USReferenceData.Business;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	class SHBTariffParserTests : TariffParserTests
	{
		protected override string TariffType => Constants.TariffTypes.SHB;

		protected override string WebServiceMockResultFileName => Constants.DownloadFileNames.SHB;

		protected override Type ParserObjectType => typeof(SHBTariffParser);

		protected override string DownloadFileName => Constants.DownloadFileNames.SHB;

		protected override string UpdateInfoNodeKeyword => Constants.FileStructureAndUpdateInfoNodeKeywords.SHB;
	}
}

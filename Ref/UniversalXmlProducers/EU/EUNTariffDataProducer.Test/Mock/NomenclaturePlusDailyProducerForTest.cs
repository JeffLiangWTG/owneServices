using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	sealed class NomenclaturePlusDailyProducerForTest : NomenclaturePlusDailyProducer
	{
		public NomenclaturePlusDailyProducerForTest(INomenclaturePlusDailyDataTestProvider dataTestProvider,
			DailyTariffUpdatesFileProviderMock dailyTariffUpdatesFileProvider,
			DateTime now)
			: base(dailyTariffUpdatesFileProvider)
		{
			this.now = now;
			this.dataTestProvider = dataTestProvider;
			dailyTariffUpdatesFileProviderMock = dailyTariffUpdatesFileProvider;
		}

		protected override IWebDriverHelper GetNewWebDriverHelper()
			=> new Mock<IWebDriverHelper>().Object;

		protected override IWebFileLocator GetNewWebFileLocator(IWebDriverHelper webDriverHelper, IEnumerable<string> filesToLocate)
		{
			var webFileLocatorMock = new Mock<IWebFileLocator>();
			webFileLocatorMock
				.Setup(x => x.GetLocationOfLatestFiles(It.IsAny<string>(), It.IsAny<bool>()))
				.Returns<string, bool>((x, y) => GetLocationOfLatestFiles());

			return webFileLocatorMock.Object;
		}

		protected override DateTime GetNow() => now;

		protected override IChapterToSectionMapper GetNewChapterToSectionMapper()
		{
			var chapterToSectionMapper = new Mock<IChapterToSectionMapper>();
			chapterToSectionMapper.Setup(x => x.GetAllSection()).Returns(dataTestProvider.Sections.Select(x => new Section(x.Number, x.Description)).ToArray());
			chapterToSectionMapper.Setup(x => x.GetSection(It.IsAny<int>())).Returns<int>(x => x);

			return chapterToSectionMapper.Object;
		}

		IEnumerable<IWebFileInfo> GetLocationOfLatestFiles()
		{
			var declarableCodePath = TestHelper.CopyToDownloadPath(dataTestProvider.DeclarableCodeFile.Path, DeclarableCodeFileName, dataTestProvider.DeclarableCodeFile.PublicationDate);
			yield return new WebFileInfo(DeclarableCodeFileName, declarableCodePath, dataTestProvider.DeclarableCodeFile.PublicationDate);

			var i = 0;
			foreach (var (filePath, publicationDate) in dataTestProvider.NomenclatureFileCollection)
			{
				var nomenclatureFileName = $"Nomenclature_{i}.xlsx";
				var nomenclaturePath = TestHelper.CopyToDownloadPath(filePath, nomenclatureFileName, publicationDate);
				yield return new WebFileInfo(nomenclatureFileName, nomenclaturePath, publicationDate);
				i++;
			}
		}

		public DateTime? MinDate => dailyTariffUpdatesFileProviderMock.MinDate;

		readonly DateTime now;
		readonly INomenclaturePlusDailyDataTestProvider dataTestProvider;
		readonly DailyTariffUpdatesFileProviderMock dailyTariffUpdatesFileProviderMock;

		const string DeclarableCodeFileName = "Declarable Codes.xlsx";
	}
}

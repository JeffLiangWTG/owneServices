using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	class ChapterNameGeneratorD : BaseChapterNameGenerator
	{
		public ChapterNameGeneratorD(ImportCustomsDeclarationDocumentWrapper wrapper)
			: base(wrapper)
		{ }

		protected override TariffChapter GetTariffChapter(ZInt prefix4, ZString classificationId)
		{
			var result = TariffChapter.None;
			if ((100 <= prefix4 && prefix4 < 2700) ||
				(6800 <= prefix4 && prefix4 < 7200) ||
				(9800 <= prefix4 && prefix4 < 9900))
			{
				result = TariffChapter.FirstTariffChapter;
			}
			else if (2700 <= prefix4 && prefix4 < 6800)
			{
				result = TariffChapter.SecondTariffChapter;
			}
			else if (7200 <= prefix4 && prefix4 < 9800)
			{
				result = TariffChapter.ThirdTariffChapter;
			}
			return result;
		}

		protected override Dictionary<TariffChapter, ZString> TariffChapterDictionary => new Dictionary<TariffChapter, ZString>()
			{
				{ TariffChapter.None, ZString.Empty },
				{ TariffChapter.FirstTariffChapter, BranchName.Branch11 },
				{ TariffChapter.SecondTariffChapter, BranchName.Branch12 },
				{ TariffChapter.ThirdTariffChapter, BranchName.Branch13 },
				{ TariffChapter.FourthTariffChapter, ZString.Empty },
				{ TariffChapter.FifthTariffChapter, ZString.Empty },
				{ TariffChapter.SixthTariffChapter, ZString.Empty },
				{ TariffChapter.SeventhTariffChapter, ZString.Empty },
				{ TariffChapter.EighthTariffChapter, ZString.Empty },
				{ TariffChapter.NinthTariffChapter, ZString.Empty },
			};
	}
}

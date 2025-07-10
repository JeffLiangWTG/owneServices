using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	class ChapterNameGeneratorBE : ChapterNameGeneratorB
	{
		public ChapterNameGeneratorBE(ImportCustomsDeclarationDocumentWrapper wrapper)
			: base(wrapper)
		{ }

		protected override TariffChapter GetTariffChapter(ZInt prefix4, ZString classificationId)
		{
			var result = TariffChapter.None;
			if (100 <= prefix4 && prefix4 < 2300)
			{
				result = TariffChapter.FirstTariffChapter;
			}
			else if (2300 <= prefix4 && prefix4 < 4700)
			{
				result = TariffChapter.SecondTariffChapter;
			}
			else if (4700 <= prefix4 && prefix4 < 7400)
			{
				result = TariffChapter.ThirdTariffChapter;
			}
			else if (7400 <= prefix4 && prefix4 < 9800)
			{
				result = TariffChapter.FourthTariffChapter;
			}
			return result;
		}

		protected override Dictionary<TariffChapter, ZString> TariffChapterDictionary => new Dictionary<TariffChapter, ZString>()
			{
				{ TariffChapter.None, ZString.Empty },
				{ TariffChapter.FirstTariffChapter, BranchName.Branch11 },
				{ TariffChapter.SecondTariffChapter, BranchName.Branch12 },
				{ TariffChapter.ThirdTariffChapter, BranchName.Branch13 },
				{ TariffChapter.FourthTariffChapter, BranchName.Branch14 },
				{ TariffChapter.FifthTariffChapter, ZString.Empty },
				{ TariffChapter.SixthTariffChapter, ZString.Empty },
				{ TariffChapter.SeventhTariffChapter, ZString.Empty },
				{ TariffChapter.EighthTariffChapter, ZString.Empty },
				{ TariffChapter.NinthTariffChapter, ZString.Empty },
			};
	}
}

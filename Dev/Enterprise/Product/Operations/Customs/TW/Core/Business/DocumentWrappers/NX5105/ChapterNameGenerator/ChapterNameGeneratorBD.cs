using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	class ChapterNameGeneratorBD : ChapterNameGeneratorB
	{
		public ChapterNameGeneratorBD(ImportCustomsDeclarationDocumentWrapper wrapper)
			: base(wrapper)
		{ }

		protected override TariffChapter GetTariffChapter(ZInt prefix4, ZString classificationId)
		{
			var result = TariffChapter.None;
			if (100 <= prefix4 && prefix4 < 2500)
			{
				result = TariffChapter.FirstTariffChapter;
			}
			else if (2500 <= prefix4 && prefix4 < 4100)
			{
				result = TariffChapter.SecondTariffChapter;
			}
			else if (4100 <= prefix4 && prefix4 < 6400)
			{
				result = TariffChapter.ThirdTariffChapter;
			}
			else if (6400 <= prefix4 && prefix4 < 7200)
			{
				result = TariffChapter.FourthTariffChapter;
			}
			else if ((7200 <= prefix4 && prefix4 < 8400) ||
				(9000 <= prefix4 && prefix4 < 9800))
			{
				result = TariffChapter.FifthTariffChapter;
			}
			else if (8400 <= prefix4 && prefix4 < 9000)
			{
				result = TariffChapter.SixthTariffChapter;
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
				{ TariffChapter.FifthTariffChapter, BranchName.Branch21 },
				{ TariffChapter.SixthTariffChapter, BranchName.Branch22 },
				{ TariffChapter.SeventhTariffChapter, ZString.Empty },
				{ TariffChapter.EighthTariffChapter, ZString.Empty },
				{ TariffChapter.NinthTariffChapter, ZString.Empty },
			};
	}
}

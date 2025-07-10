using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	class ChapterNameGeneratorA : BaseChapterNameGenerator
	{
		public ChapterNameGeneratorA(ImportCustomsDeclarationDocumentWrapper wrapper)
			: base(wrapper)
		{ }

		protected override TariffChapter GetTariffChapter(ZInt prefix4, ZString classificationId)
		{
			var result = TariffChapter.None;
			if (100 <= prefix4 && prefix4 < 2200)
			{
				result = TariffChapter.FirstTariffChapter;
			}
			else if (2200 <= prefix4 && prefix4 < 3400)
			{
				result = TariffChapter.SecondTariffChapter;
			}
			else if (3400 <= prefix4 && prefix4 < 4400)
			{
				result = TariffChapter.ThirdTariffChapter;
			}
			else if (4400 <= prefix4 && prefix4 < 6400)
			{
				result = TariffChapter.FourthTariffChapter;
			}
			else if (6400 <= prefix4 && prefix4 < 8400)
			{
				result = TariffChapter.FifthTariffChapter;
			}
			else if (8600 <= prefix4 && prefix4 < 8800)
			{
				result = TariffChapter.SixthTariffChapter;
			}
			else if ((8400 <= prefix4 && prefix4 < 8500) || (9000 <= prefix4 && prefix4 < 9100))
			{
				result = TariffChapter.SeventhTariffChapter;
			}
			else if (8500 <= prefix4 && prefix4 < 8600)
			{
				result = TariffChapter.EighthTariffChapter;
			}
			else if ((8800 <= prefix4 && prefix4 < 9000) || (9100 <= prefix4 && prefix4 < 9800))
			{
				result = TariffChapter.NinthTariffChapter;
			}
			return result;
		}

		protected override Dictionary<TariffChapter, ZString> TariffChapterDictionary => new Dictionary<TariffChapter, ZString>()
			{
				{ TariffChapter.None, ZString.Empty },
				{ TariffChapter.FirstTariffChapter, BranchName.Branch11 },
				{ TariffChapter.SecondTariffChapter, BranchName.Branch12 },
				{ TariffChapter.ThirdTariffChapter, BranchName.Branch13 },
				{ TariffChapter.FourthTariffChapter, BranchName.Branch21 },
				{ TariffChapter.FifthTariffChapter, BranchName.Branch22 },
				{ TariffChapter.SixthTariffChapter, BranchName.Branch23 },
				{ TariffChapter.SeventhTariffChapter, BranchName.Branch31 },
				{ TariffChapter.EighthTariffChapter, BranchName.Branch32 },
				{ TariffChapter.NinthTariffChapter, BranchName.Branch33 },
			};
	}
}

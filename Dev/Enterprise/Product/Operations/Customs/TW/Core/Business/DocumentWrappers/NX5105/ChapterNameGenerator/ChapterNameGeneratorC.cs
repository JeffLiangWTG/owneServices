using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	class ChapterNameGeneratorC : BaseChapterNameGenerator
	{
		public ChapterNameGeneratorC(ImportCustomsDeclarationDocumentWrapper wrapper)
			: base(wrapper)
		{ }

		protected override TariffChapter GetTariffChapter(ZInt prefix4, ZString classificationId)
		{
			var result = TariffChapter.None;
			if (100 <= prefix4 && prefix4 < 3900)
			{
				result = TariffChapter.FirstTariffChapter;
			}
			else if (3900 <= prefix4 && prefix4 < 7200)
			{
				result = TariffChapter.SecondTariffChapter;
			}
			else if (7200 <= prefix4 && prefix4 < 8500)
			{
				result = TariffChapter.ThirdTariffChapter;
			}
			else if (8501 <= prefix4 && prefix4 < 8539)
			{
				result = TariffChapter.FourthTariffChapter;
			}
			else if (8539 <= prefix4 && prefix4 < 9000)
			{
				result = TariffChapter.FifthTariffChapter;
			}
			else if ((9000 <= prefix4 && prefix4 < 9800) || classificationId == "98990000006")
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
				{ TariffChapter.FourthTariffChapter, BranchName.Branch21 },
				{ TariffChapter.FifthTariffChapter, BranchName.Branch22 },
				{ TariffChapter.SixthTariffChapter, BranchName.Branch23 },
				{ TariffChapter.SeventhTariffChapter, ZString.Empty },
				{ TariffChapter.EighthTariffChapter, ZString.Empty },
				{ TariffChapter.NinthTariffChapter, ZString.Empty },
			};
	}
}

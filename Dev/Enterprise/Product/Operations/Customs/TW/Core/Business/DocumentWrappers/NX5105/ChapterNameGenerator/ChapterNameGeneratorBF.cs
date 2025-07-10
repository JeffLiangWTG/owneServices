using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	class ChapterNameGeneratorBF : ChapterNameGeneratorB
	{
		public ChapterNameGeneratorBF(ImportCustomsDeclarationDocumentWrapper wrapper)
			: base(wrapper)
		{ }

		protected override TariffChapter GetTariffChapter(ZInt prefix4, ZString classificationId)
		{
			var result = TariffChapter.None;
			if (100 <= prefix4 && prefix4 < 8400)
			{
				result = TariffChapter.FirstTariffChapter;
			}
			else if (8400 <= prefix4 && prefix4 < 9900)
			{
				result = TariffChapter.SecondTariffChapter;
			}
			return result;
		}

		protected override Dictionary<TariffChapter, ZString> TariffChapterDictionary => new Dictionary<TariffChapter, ZString>()
			{
				{ TariffChapter.None, ZString.Empty },
				{ TariffChapter.FirstTariffChapter, BranchName.Branch1 },
				{ TariffChapter.SecondTariffChapter, BranchName.Branch2 },
				{ TariffChapter.ThirdTariffChapter, ZString.Empty },
				{ TariffChapter.FourthTariffChapter, ZString.Empty },
				{ TariffChapter.FifthTariffChapter, ZString.Empty },
				{ TariffChapter.SixthTariffChapter, ZString.Empty },
				{ TariffChapter.SeventhTariffChapter, ZString.Empty },
				{ TariffChapter.EighthTariffChapter, ZString.Empty },
				{ TariffChapter.NinthTariffChapter, ZString.Empty },
			};
	}
}

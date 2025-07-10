using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	abstract class ChapterNameGeneratorB : BaseChapterNameGenerator
	{
		protected ChapterNameGeneratorB(ImportCustomsDeclarationDocumentWrapper wrapper)
			: base(wrapper)
		{
		}

		public override ZString GetTariffChapterName()
		{
			var result = ZString.Empty;
			var firstClassificationId = goodsItems.FirstOrDefault().Commodity?.Classifications?.FirstOrDefault(x => x.IdentificationTypeCode == MessageConstants.IdentificationTypeCodes.HS)?.ID ?? ZString.Empty;
			if (ZInt.TryParse(firstClassificationId.SubstringSafe(0, 4), out var prefix4))
			{
				TariffChapterDictionary.TryGetValue(GetTariffChapter(prefix4, firstClassificationId), out result);
			}
			return GetCompleteChapterName(result);
		}
	}
}

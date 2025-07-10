using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Business.ICS2
{
	public class TypeOfGoodsDataParser : CommonDataParser, IAdditionalTranslationSupporter
	{
		public TypeOfGoodsDataParser() :
			base(Constants.TypeOfGoods.RDEntityAttributeValue, Constants.TypeOfGoods.CodeAttributeValue)
		{ }

		public string DataParserKey => "TypeOfGoodsDataParser";
	}
}

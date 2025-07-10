using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.KindOfPackages.Business
{
	public sealed class KindOfPackagesDataParser : CommonDataParser, IAdditionalTranslationSupporter
	{
		public KindOfPackagesDataParser()
			: base(Constants.KindOfPackages.RDEntityAttributeValue, Constants.KindOfPackages.CodeAttributeValue)
		{
		}

		public string DataParserKey => "KindOfPackagesDataParser";
	}
}

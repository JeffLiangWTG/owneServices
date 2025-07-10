using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.AdditionalInformationCodeSubset.Business
{
	public class AdditionalInformationCodeSubsetDataParser : CommonDataParser
	{
		public AdditionalInformationCodeSubsetDataParser()
			: base(Constants.AdditionalInformationSubsetCode.RDEntityAttributeValue, Constants.AdditionalInformationSubsetCode.CodeAttributeValue)
		{

		}
	}
}

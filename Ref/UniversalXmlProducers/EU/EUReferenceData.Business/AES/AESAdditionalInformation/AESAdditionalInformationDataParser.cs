using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.AESAdditionalInformation.Business
{
	public class AESAdditionalInformationDataParser : CommonDataParser
	{
		public AESAdditionalInformationDataParser()
			: base(Constants.AESAdditionalInformation.RDEntityAttributeValue, Constants.AESAdditionalInformation.CodeAttributeValue)
		{
		}
	}
}

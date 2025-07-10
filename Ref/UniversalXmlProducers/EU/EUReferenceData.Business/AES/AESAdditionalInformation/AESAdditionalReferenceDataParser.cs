using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.AESAdditionalReference.Business
{
	public class AESAdditionalReferenceDataParser : CommonDataParser
	{
		public AESAdditionalReferenceDataParser()
			: base(Constants.AESAdditionalReference.RDEntityAttributeValue, Constants.AESAdditionalReference.CodeAttributeValue)
		{
		}
	}
}

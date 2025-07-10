using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.AESNationality.Business
{
	public class AESNationalityDataParser : CommonDataParser
	{
		public AESNationalityDataParser()
			: base(Constants.AESNationality.RDEntityAttributeValue, Constants.AESNationality.CodeAttributeValue)
		{
		}
	}
}

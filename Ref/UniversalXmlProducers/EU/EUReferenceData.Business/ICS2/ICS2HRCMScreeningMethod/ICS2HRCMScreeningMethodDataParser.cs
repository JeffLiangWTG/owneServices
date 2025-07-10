using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.ICS2HRCMScreeningMethod.Business
{
	public class ICS2HRCMScreeningMethodDataParser : CommonDataParser
	{
		public ICS2HRCMScreeningMethodDataParser()
			: base(Constants.ICS2HRCMScreeningMethod.RDEntityAttributeValue, Constants.ICS2HRCMScreeningMethod.RoleAttributeValue)
		{
		}
	}
}

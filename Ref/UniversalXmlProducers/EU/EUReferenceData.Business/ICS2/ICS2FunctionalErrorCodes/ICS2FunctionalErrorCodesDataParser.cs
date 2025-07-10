using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.ICS2FunctionalErrorCodes.Business
{
	public class ICS2FunctionalErrorCodesDataParser : CommonDataParser
	{
		public ICS2FunctionalErrorCodesDataParser()
			: base(Constants.ICS2FunctionalErrorCodes.RDEntityAttributeValue, Constants.ICS2FunctionalErrorCodes.CodeAttributeValue)
		{

		}
	}
}

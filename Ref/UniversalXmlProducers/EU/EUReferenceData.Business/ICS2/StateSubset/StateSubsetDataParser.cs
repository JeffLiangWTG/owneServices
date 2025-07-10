using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.StateSubset.Business
{
	public class StateSubsetDataParser : CommonDataParser
	{
		public StateSubsetDataParser() :
			base(Constants.StateSubset.RDEntityAttributeValue, Constants.StateSubset.CodeAttributeValue)
		{
		}
	}
}

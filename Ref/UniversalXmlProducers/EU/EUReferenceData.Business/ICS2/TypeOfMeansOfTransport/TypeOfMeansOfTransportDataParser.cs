using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.TypeOfMeansOfTransport.Business
{
	public class TypeOfMeansOfTransportDataParser : CommonDataParser
	{
		public TypeOfMeansOfTransportDataParser() :
			base(Constants.TypeOfMeansOfTransport.RDEntityAttributeValue, Constants.TypeOfMeansOfTransport.RDEntityAttributeValue)
		{
		}
	}
}

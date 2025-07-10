using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.MethodOfPayment.Business
{
	public class MethodOfPaymentDataParser : CommonDataParser
	{
		public MethodOfPaymentDataParser()
			: base(Constants.MethodOfPayment.RDEntityAttributeValue, Constants.MethodOfPayment.RDEntityAttributeValue)
		{

		}
	}
}

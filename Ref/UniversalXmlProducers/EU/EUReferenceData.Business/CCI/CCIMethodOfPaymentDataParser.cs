using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class CCIMethodOfPaymentDataParser : CommonDataParser
	{
		public CCIMethodOfPaymentDataParser() :
			base(Constants.CCIMethodOfPayment.RDEntityAttributeValue,
				  Constants.CCIMethodOfPayment.DataItemAttributeValue)
		{
		}
	}
}

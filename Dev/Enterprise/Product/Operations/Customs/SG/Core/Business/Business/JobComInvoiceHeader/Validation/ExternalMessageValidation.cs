
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class ExternalMessageValidation : Customs.Business.ExternalMessageValidation
	{
		public ExternalMessageValidation(BusinessObject businessObject)
			: base(businessObject)
		{
		}

		protected override string GetAdviceHowToFixNoValidExchangeRates()
		{
			string result = "\r\nPlease check with your system administrator to ensure that your batchprocessor is downloading customs exchange rate correctly.";
			result += "\r\nAlternatively, If this is an currency for which Singapore Customs do no publish an exchange rate, please manually enter the appropriate customs exchange rate in Reference Files|Currencies.";

			return result;
		}
	}
}

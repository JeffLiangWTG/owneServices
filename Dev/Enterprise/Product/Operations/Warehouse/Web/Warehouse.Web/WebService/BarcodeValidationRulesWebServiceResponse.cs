using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class BarcodeValidationRulesWebServiceResponse : WebServiceResponse
	{
		public BarcodeValidationRuleInfo[] Rules { get; set; }
	}
}

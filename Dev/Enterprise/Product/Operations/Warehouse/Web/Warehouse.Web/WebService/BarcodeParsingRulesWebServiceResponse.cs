using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class BarcodeParsingRulesWebServiceResponse : WebServiceResponse
	{
		public BarcodeParsingRuleInfo[] Rules { get; set; }
	}
}

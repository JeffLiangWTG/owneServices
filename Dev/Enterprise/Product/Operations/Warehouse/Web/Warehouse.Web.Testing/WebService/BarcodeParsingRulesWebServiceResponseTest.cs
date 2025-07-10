using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class BarcodeParsingRulesWebServiceResponseTest : WebServiceResponseTestCase
	{
		#region TestRules

		public void TestRules()
		{
			var response = new BarcodeParsingRulesWebServiceResponse();
			AssertNull(response.Rules);

			var rule = new BarcodeParsingRuleInfo();
			response.Rules = new[] { rule };
			AssertContainsExactElementsInAnyOrder(new[] { rule }, response.Rules);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new BarcodeParsingRulesWebServiceResponse();
		}

		#endregion
	}
}

using System;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.BarcodeParsing.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region GetBarcodeParsingRules

		[WebMethod(Description = "Get Barcode Parsing Rules")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public BarcodeParsingRulesWebServiceResponse GetBarcodeParsingRules(string clientCode, string supplierCode, Guid productPK, bool isGS1)
		{
			return HandleWebServiceRequest<BarcodeParsingRulesWebServiceResponse>(result => GetBarcodeParsingRulesCore(result, clientCode, supplierCode, productPK, isGS1));
		}

		#region GetBarcodeParsingRulesCore

		void GetBarcodeParsingRulesCore(BarcodeParsingRulesWebServiceResponse response, string clientCode, string supplierCode, Guid productPK, bool isGS1)
		{
			var rulesMatched = BarcodeRule.LoadMatchingRules(Factory, BarcodeModuleTypes.Codes.Warehouse, clientCode, supplierCode, productPK, isGS1);
			response.Rules = rulesMatched.Select(r => new BarcodeParsingRuleInfo(r)).ToArray();
		}

		#endregion

		#endregion
	}
}

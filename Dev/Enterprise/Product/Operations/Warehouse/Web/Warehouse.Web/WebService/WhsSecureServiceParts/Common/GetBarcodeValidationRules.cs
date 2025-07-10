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
		#region GetBarcodeValidationRules

		[WebMethod(Description = "Get Barcode Validation Rules")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public BarcodeValidationRulesWebServiceResponse GetBarcodeValidationRules(string clientCode, string supplierCode, Guid productPK)
		{
			return HandleWebServiceRequest<BarcodeValidationRulesWebServiceResponse>(result => GetBarcodeValidationRules(result, clientCode, supplierCode, productPK));
		}

		#region GetBarcodeValidationRules

		void GetBarcodeValidationRules(BarcodeValidationRulesWebServiceResponse response, string clientCode, string supplierCode, Guid productPK)
		{
			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, BarcodeModuleTypes.Codes.Warehouse, clientCode, supplierCode, productPK);
			response.Rules = rulesMatched.Select(r => new BarcodeValidationRuleInfo(r)).ToArray();
		}

		#endregion

		#endregion
	}
}

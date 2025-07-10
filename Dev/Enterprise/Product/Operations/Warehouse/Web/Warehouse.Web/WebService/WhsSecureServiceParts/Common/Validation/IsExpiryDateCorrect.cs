using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region IsExpiryDateCorrect

		[WebMethod(Description = "Is Expiry Date Correct")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse IsExpiryDateCorrect(Guid productPK, string clientCode, DateTime expiryDate)
		{
			return HandleWebServiceRequest<WebServiceResponse>(r => IsExpiryDateCorrect(r, productPK, clientCode, ((ZDateTime)expiryDate).Date));
		}

		void IsExpiryDateCorrect(WebServiceResponse webResponse, Guid productPK, string clientCode, ZDate expiryDate)
		{
			WebServiceHelper.LoadAndValidateClientAndPartBeforeRunningAction(webResponse, Factory, productPK, clientCode, (response, part, client) => CheckExpiryDate(response, expiryDate, part, client));
		}

		void CheckExpiryDate(WebServiceResponse response, ZDate expiryDate, OrgSupplierPart part, OrgHeader client)
		{
			var product = WhsProduct.GetWhsProduct(part);
			var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);

			if (Transactions.Business.PartAttributeValidation.ExpiryDateLessThanExpiryNotificationPeriod(client, warehouse, product, expiryDate))
			{
				response.Error = ErrorTypes.WarningOnly;
				response.ErrorMessage = Transactions.Business.PartAttributeValidation.ExpiryDateLessThanExpiryNotificationPeriodMessage;
			}
			else if (expiryDate < ZDate.Today)
			{
				response.Error = ErrorTypes.YesNoEnquiry;
				var message = Res.GetString("b174b8c8-78d6-4356-af87-2faaf7ab1fe9", "Product has already expired. Are you sure this is correct?");
				response.ErrorMessage = message;
			}
		}

		#endregion
	}
}

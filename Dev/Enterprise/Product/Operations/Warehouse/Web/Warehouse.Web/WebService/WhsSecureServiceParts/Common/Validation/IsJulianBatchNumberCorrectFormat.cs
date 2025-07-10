using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region IsJulianBatchNumberCorrectFormat

		[WebMethod(Description = "Is Julian Batch Number Correct Format")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public JulianBatchNumberFormatWebServiceResponse IsJulianBatchNumberCorrectFormat(Guid productPK, string clientCode, string julianBatchNumber)
		{
			return HandleWebServiceRequest<JulianBatchNumberFormatWebServiceResponse>(r => IsJulianBatchNumberCorrectFormat(r, productPK, clientCode, julianBatchNumber));
		}

		void IsJulianBatchNumberCorrectFormat(JulianBatchNumberFormatWebServiceResponse webResponse, Guid productPK, string clientCode, string julianBatchNumber)
		{
			WebServiceHelper.LoadAndValidateClientAndPartBeforeRunningAction(webResponse, Factory, productPK, clientCode, (response, part, client) => CheckJulianBatchNumber(response, julianBatchNumber, part, client));
		}

		void CheckJulianBatchNumber(JulianBatchNumberFormatWebServiceResponse response, string julianBatchNumber, OrgSupplierPart part, OrgHeader client)
		{
			var product = WhsProduct.GetWhsProduct(part);
			response.IsCorrectFormat = product.IsAValidJulianBatchNumberFormat(client, julianBatchNumber);
			if (response.IsCorrectFormat)
			{
				var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
				response.PackingDate = product.GetPackingDateFromJulianBatchNumber(client, warehouse, julianBatchNumber).ToDateTime();
				response.ExpiryDate = product.CalculateExpiryDate(client, warehouse, julianBatchNumber).ToDateTime();
			}
			else
			{
				response.LogError(ErrorTypes.BusinessValidationError, Transactions.Business.PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage);
			}
		}

		#endregion
	}
}

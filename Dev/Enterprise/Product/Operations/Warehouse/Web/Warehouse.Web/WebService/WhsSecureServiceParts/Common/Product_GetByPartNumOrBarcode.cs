using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region Product_GetByPartNumOrBarcode

		[WebMethod(Description = "Get a product for part number or barcode")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsProductWebServiceResponse Product_GetByPartNumOrBarcode(string partNumOrBarcode, string clientCode)
		{
			return HandleWebServiceRequest_WithValidateWarehouseAndStaff<WhsProductWebServiceResponse>(r => Product_GetByPartNumOrBarcode(r, partNumOrBarcode, clientCode));
		}

		void Product_GetByPartNumOrBarcode(WhsProductWebServiceResponse response, string partNumOrBarcode, string clientCode)
		{
			var orgSupplierPart = GetPartByPartNumOrBarcode(partNumOrBarcode, clientCode, out var client, out var errorMessage);
			if (orgSupplierPart != null)
			{
				var whs = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
				response.Product = WhsProductInfo.GetInfo(orgSupplierPart, GoodsHandlingInstructionsType.None);
				response.ProductPartAttributes = WhsProductPartAttributesInfo.GetInfo(client, orgSupplierPart, whs);
			}

			if (!string.IsNullOrEmpty(errorMessage))
			{
				response.LogBusinessValidationError(errorMessage);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Out parameter required by design")]
		OrgSupplierPart GetPartByPartNumOrBarcode(string partNumOrBarcode, string clientCode, out OrgHeader client, out string errorMessage)
		{
			return WebServiceHelper.GetPartByPartNumOrBarcode(Factory, SecurityHeader.WarehouseCode, partNumOrBarcode, clientCode, false, out client, out string barcode, out errorMessage);
		}

		#endregion
	}
}

using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region Product_GetByPartNumOrBarcodeForAllClients

		[WebMethod(Description = "Get a products for part number or barcode with all the clients for them")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsManyProductWebServiceResponse Product_GetByPartNumOrBarcodeForAllClients(string partNumOrBarcode)
		{
			return HandleWebServiceRequest<WhsManyProductWebServiceResponse>(response => Product_GetByPartNumOrBarcodeForAllClientsCore(response, partNumOrBarcode));
		}

		void Product_GetByPartNumOrBarcodeForAllClientsCore(WhsManyProductWebServiceResponse response, string partNumOrBarcode)
		{
			var products = WebServiceHelper.GetOrderedProductsFromPartNumOrBarcode(partNumOrBarcode, null, Factory).Where(part => part.RelatedOrganisations.Any()).ToArray(); //No client passed
			var productsFound = false;
			if (products.Any())
			{
				var allOrgPartRelations = products.SelectMany(part => part.RelatedOrganisations.Cast<OrgPartRelation>().Where(ou => ou.IsOwner || ou.IsBoth).ToArray());
				if (allOrgPartRelations.Any())
				{
					var whs = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
					response.Products = products.Select(part => WhsProductInfo.GetInfo(part, GoodsHandlingInstructionsType.None)).ToArray();
					response.ProductPartAttributes = allOrgPartRelations.Select(partRelation => WhsProductPartAttributesInfo.GetInfo(partRelation.Organisation, partRelation.SupplierPart, whs)).ToArray();
					productsFound = true;
				}
			}
			if (!productsFound)
			{
				response.ErrorMessage = Res.GetString("2803ccb9-feaf-4272-9bd3-c9d215519a54", "Product {0} could not be found. Please provide a valid product code or barcode.", partNumOrBarcode);
				response.Error = ErrorTypes.BusinessValidationError;
			}
		}

		#endregion
	}
}

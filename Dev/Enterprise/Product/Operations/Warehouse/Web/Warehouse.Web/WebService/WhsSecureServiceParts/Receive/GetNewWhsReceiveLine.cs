using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region GetNewWhsReceiveLine

		[WebMethod(Description = "Get New Receive Line")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsDocketLineAndConversionsWebServiceResponse GetNewWhsReceiveLine(string partNumOrBarcode, string clientCode, Guid docketPK)
		{
			return HandleWebServiceRequest<WhsDocketLineAndConversionsWebServiceResponse>(r => GenerateNewWhsReceiveLine(r, partNumOrBarcode, clientCode, docketPK));
		}

		void GenerateNewWhsReceiveLine(WhsDocketLineAndConversionsWebServiceResponse response, string partNumOrBarcode, string clientCode, Guid docketPK)
		{
			var part = WebServiceHelper.GetPartByPartNumOrBarcode(Factory, SecurityHeader.WarehouseCode, partNumOrBarcode, clientCode, true, out OrgHeader client, out string barcode, out string errorMessage);
			if (!string.IsNullOrEmpty(errorMessage))
			{
				response.LogBusinessValidationError(errorMessage);
			}
			else if (part.ShouldPreventReceiveOfPartWithNoWeightOrDims(client))
			{
				response.LogBusinessValidationError(Res.GetString("111ae0f6-4382-471f-8589-947573c589ac", "Product cannot be received as the Product Master is missing weight or dimensions."));
				part = null;
			}

			response.LineInfo = (part != null)
				? new WhsDocketLineInfo(client, part, barcode, docketPK, WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode))
				: new WhsDocketLineInfo();
			response.Conversions = (part != null)
				? new UnitConversionCollection(part)
				: new UnitConversionCollection();
		}

		#endregion
	}
}

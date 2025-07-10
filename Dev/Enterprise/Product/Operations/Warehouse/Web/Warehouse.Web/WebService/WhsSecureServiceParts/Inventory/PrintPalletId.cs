using System;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		[WebMethod(Description = "Print Pallet ID")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse PrintPalletID(string palletId, Guid printerPK)
		{
			return HandleWebServiceRequest<WebServiceResponse>(response => PrintPalletIDCore(response, palletId, printerPK));
		}

		void PrintPalletIDCore(WebServiceResponse response, string palletID, Guid printerPK)
		{
			var criteria = new WhsInventorySearchCriteriaInfo(string.Empty, string.Empty, string.Empty, palletID);
			var inventoryLoader = new WhsInventoryLoader(Factory, SecurityHeader.WarehouseCode);
			var availableInventories = inventoryLoader.LoadWhsInventory(criteria);

			if (availableInventories.Any())
			{
				PrintPalletIdLabel(response, availableInventories.First(), printerPK);
			}
			else
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("068F1D21-66EE-4568-A6A3-0E42641F5089", "Pallet Id {0} does not exist on any inventory in this warehouse.", palletID);
			}
		}

		void PrintPalletIdLabel(WebServiceResponse response, WhsInventoryView whsInventory, Guid printerPK)
		{
			EventHandler<PrintFailedEventArgs> palletLabelPrintFailed = (sender, e) =>
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = e.Message;
			};

			whsInventory.PalletLabelPrintFailed += palletLabelPrintFailed;
			try
			{
				whsInventory.PrintPalletIdLabel(printerPK, 1);
			}
			finally
			{
				whsInventory.PalletLabelPrintFailed -= palletLabelPrintFailed;
			}
		}
	}
}

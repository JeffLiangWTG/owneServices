using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Environment;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region GetDefaultPrintingInfo

		[WebMethod(Description = "Get Default Printing Information")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public DefaultPrintingInfoWebServiceResponse GetDefaultPrintingInfo(string orderDocketID, PackageForPrintingInfo packageInfo)
		{
			return HandleWebServiceRequest<DefaultPrintingInfoWebServiceResponse>(r => SetDefaultPrintingInfo(r, orderDocketID, packageInfo));
		}

		void SetDefaultPrintingInfo(DefaultPrintingInfoWebServiceResponse response, string orderDocketID, PackageForPrintingInfo packageInfo)
		{
			var order = WebServiceHelper.GetOrderByDocketID(Factory, response, SecurityHeader.WarehouseCode, orderDocketID);
			if (order != null)
			{
				if (packageInfo != null)
				{
					var package = WebServiceHelper.GetPackageByPackagePK(order, packageInfo.PK); // do null check on package and return error
					if (package != null)
					{
						SetDefaultPrintingInfoCore(response, order, package, packageInfo.SupportsCarrierLabelIntegration);
					}
					else
					{
						response.Error = ErrorTypes.BusinessValidationError;
						response.ErrorMessage = Res.GetString("E133F8E4-61EB-4F18-AB43-95D6CC657D0E", "No package found with package ID: {0}.", packageInfo.PackageID);
					}
				}
				else
				{
					response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("b986d8ff-ddfe-426f-9935-a7b09cf462fb", "Invalid Request for Printing Information."));
				}
			}
		}

		void SetDefaultPrintingInfoCore(DefaultPrintingInfoWebServiceResponse response, WhsOrder order, PkgPackage package, bool supportsCarrierLabelIntegration)
		{
			// if we support carrier label integration, then it must be used and the Document on Close fallback is ignored
			var menuItemToPrint = !supportsCarrierLabelIntegration ? new PackageLabelAutoPrinter(order.PackageJob).GetDocumentToPrint(package) : null;
			if (supportsCarrierLabelIntegration || menuItemToPrint != null)
			{
				var pickPackParameter = WebServiceHelper.GetPickPackParameter(order);
				response.NumberOfLabelsToPrintOnClose = (int?)pickPackParameter?.WPP_NumberOfLabelsToPrintOnClose ?? 0;
				response.NumberOfLabelsToPrintOnNew = (int?)pickPackParameter?.WPP_NumberOfLabelsToPrintOnNew ?? 0;
				SetAvailablePrinters(response, includeEmptyPrinter: false);

				if (response.Printers.Length > 0)
				{
					var printer = order.GetRFPickPackPrinter(Env.CurrentUserContext.User, menuItemToPrint);
					response.DefaultPrinter = printer != null ? printer.PK.ToGuid() : Guid.Empty;
				}
				else
				{
					response.Error = ErrorTypes.BusinessValidationError;
					response.ErrorMessage = Res.GetString("38117d32-3bc2-4926-8f56-345c5c108639", "No printers exist in the system.");
				}
			}
		}

		#endregion
	}
}

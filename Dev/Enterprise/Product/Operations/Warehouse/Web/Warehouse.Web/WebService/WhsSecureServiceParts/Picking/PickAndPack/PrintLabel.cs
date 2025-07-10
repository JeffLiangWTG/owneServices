using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region PrintLabel

		[WebMethod(Description = "Print Label for Packing")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse PrintLabel(string orderDocketID, string packageID, Guid printerPK, int numberOfLabelsToPrint)
		{
			return HandleWebServiceRequest<WebServiceResponse>(r => PrintLabel(r, orderDocketID, packageID, printerPK, numberOfLabelsToPrint));
		}

		void PrintLabel(WebServiceResponse response, string orderDocketID, string packageID, Guid printerPK, int numberOfLabelsToPrint)
		{
			var order = WebServiceHelper.GetOrderByDocketID(Factory, response, SecurityHeader.WarehouseCode, orderDocketID);
			if (order != null)
			{
				var package = WebServiceHelper.GetPackageByPackageID(order, packageID);
				if (package != null)
				{
					EventHandler<AutoPrintFailedEventArgs> autoPrintFailed = (sender, e) =>
					{
						response.Error = ErrorTypes.BusinessValidationError;
						response.ErrorMessage = e.Message;
					};

					order.PackageJob.AutoPrintFailed += autoPrintFailed;

					try
					{
						package.PrintLabel(printerPK, numberOfLabelsToPrint);
					}
					finally
					{
						order.PackageJob.AutoPrintFailed -= autoPrintFailed;
					}
				}
				else
				{
					response.Error = ErrorTypes.BusinessValidationError;
					response.ErrorMessage = Res.GetString("312b3130-1a5f-44de-a812-653c65da1162", "Package ID '{0}' does not exist.", packageID);
				}
			}
		}

		#endregion
	}
}

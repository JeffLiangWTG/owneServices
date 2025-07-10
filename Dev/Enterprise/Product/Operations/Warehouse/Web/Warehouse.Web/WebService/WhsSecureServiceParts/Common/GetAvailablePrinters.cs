using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region GetAvailablePrinters

		[WebMethod(Description = "Get available Printers")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public PrintersWebServiceResponse GetAvailablePrinters()
		{
			return HandleWebServiceRequest<PrintersWebServiceResponse>(r => SetAvailablePrinters(r, includeEmptyPrinter: true));
		}

		void SetAvailablePrinters(PrintersWebServiceResponse response, bool includeEmptyPrinter)
		{
			response.Printers = GetPrintersUtil(includeEmptyPrinter);
		}

		PrinterInfo[] GetPrintersUtil(bool includeEmptyPrinter)
		{
			var printers = WhsCommonLookups.GetPrintersList(Factory);
			return printers.Count == 0 ? System.Array.Empty<PrinterInfo>() : GetPrinters(printers, includeEmptyPrinter).ToArray();
		}

		static IEnumerable<PrinterInfo> GetPrinters(IBusinessObjectCollection printers, bool includeEmptyPrinter)
		{
			if (includeEmptyPrinter)
			{
				yield return new PrinterInfo(); // add empty printer to allow selection of no printer.
			}

			foreach (var printer in printers.Cast<IStmPrintQueue>().Select(p => new PrinterInfo(p)))
			{
				yield return printer;
			}
		}

		#endregion
	}
}

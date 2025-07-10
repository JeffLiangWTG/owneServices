using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class PrintersWebServiceResponse : WebServiceResponse
	{
		public PrinterInfo[] Printers
		{
			get;
			set;
		}
	}
}

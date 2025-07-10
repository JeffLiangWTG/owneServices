using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsTransfersWebServiceResponse : WebServiceResponse
	{
		public WhsDocketInfo[] Transfers { get; set; }

		public bool IsStockCommittedOrReserved { get; set; }

		public string SourceLocation { get; set; }

		public bool ShowStockOnHandWarningOnPutaway { get; set; }
	}
}

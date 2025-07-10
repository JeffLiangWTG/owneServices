using System;

namespace Enterprise.Warehouse.Web.WebService
{
	public class BeginRFDirectedPackingTaskWebServiceResponse : WhsOrdersWebServiceResponse
	{
		public WhsLocationInfo PackingStation { get; set; }

		public Guid WhsPickPK { get; set; }
	}
}

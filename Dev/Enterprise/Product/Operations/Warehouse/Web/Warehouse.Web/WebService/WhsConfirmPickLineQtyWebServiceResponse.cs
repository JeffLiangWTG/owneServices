using System;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsConfirmPickLineQtyWebServiceResponse : WebServiceResponse
	{
		public Guid[] ShortedOrderLinePKs
		{
			get { return shortedOrderLinePKs ?? (shortedOrderLinePKs = Array.Empty<Guid>()); }
			set { shortedOrderLinePKs = value; }
		}
		Guid[] shortedOrderLinePKs;
	}
}

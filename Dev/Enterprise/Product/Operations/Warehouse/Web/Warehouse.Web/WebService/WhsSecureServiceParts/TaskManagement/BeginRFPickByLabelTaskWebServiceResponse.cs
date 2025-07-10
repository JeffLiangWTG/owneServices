using System;

namespace Enterprise.Warehouse.Web.WebService
{
	public class BeginRFPickByLabelTaskWebServiceResponse : WhsPickByLabelActiveJobWebServiceResponse
	{
		public Guid WhsPickPK { get; set; }
	}
}

using System;

namespace Enterprise.Warehouse.Web.WebService
{
	public class JulianBatchNumberFormatWebServiceResponse : WebServiceResponse
	{
		public bool IsCorrectFormat { get; set; }
		public DateTime ExpiryDate { get; set; }
		public DateTime PackingDate { get; set; }
	}
}
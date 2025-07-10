using System;

namespace Enterprise.Warehouse.Web.WebService
{
	public class DefaultPrintingInfoWebServiceResponse : PrintersWebServiceResponse
	{
		public Guid DefaultPrinter
		{
			get;
			set;
		}

		public int NumberOfLabelsToPrintOnNew
		{
			get;
			set;
		}

		public int NumberOfLabelsToPrintOnClose
		{
			get;
			set;
		}
	}
}

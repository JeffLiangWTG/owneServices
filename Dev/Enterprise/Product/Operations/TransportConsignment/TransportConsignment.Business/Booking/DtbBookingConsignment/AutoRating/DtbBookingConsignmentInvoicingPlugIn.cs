using Enterprise.TransportCommon.Business;

namespace Enterprise.TransportConsignment.Business
{
	class DtbBookingConsignmentInvoicingPlugIn : DtbTransportInvoicingPlugIn
	{
		public DtbBookingConsignmentInvoicingPlugIn(DtbBookingConsignment consignment)
			: base(consignment)
		{
		}

		DtbBookingConsignment Consignment
		{
			get { return (DtbBookingConsignment)Transport; }
		}

		#region InvoicingSupporter

		protected override DtbTransportInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new DtbBookingConsignmentInvoicingSupporter(Consignment);
		}

		#endregion
	}
}

using System;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Testing;

namespace Enterprise.TransportConsignment.Business.Testing
{
	public class DtbBookingConsignmentInvoicingPlugInTest : DtbTransportInvoicingPlugInTest
	{
		#region TestInvoicingSupporter

		protected override Type ExpectedInvoicingSupporterType
		{
			get { return typeof(DtbBookingConsignmentInvoicingSupporter); }
		}

		#endregion

		#region Implementation

		protected override DtbTransportInvoicingPlugIn GetNewDtbTransportInvoicingPlugIn(DtbTransport transport)
		{
			return new DtbBookingConsignmentInvoicingPlugIn((DtbBookingConsignment)transport);
		}

		protected override DtbTransport GetNewDtbTransportJob()
		{
			return Helper.CreateBookingConsignment();
		}

		#region Helper

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion

		#endregion
	}
}

using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommonConsolidatedTransportBooking))]
	sealed class CommonConsolidatedTransportBookingBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var commonConsolidatedTransportBooking = Factory.New<CommonConsolidatedTransportBooking>();
			AssertEquals("Transport Booking", commonConsolidatedTransportBooking.HumanReadableName);

			commonConsolidatedTransportBooking.D1_BookingReference = "2";
			AssertEquals("Transport Booking 2", commonConsolidatedTransportBooking.HumanReadableName);
		}

		#endregion

		#region IDocumentSupportable

		public void TestDocumentSupporter()
		{
			AssertEquals(typeof(CommonConsolidatedTransportBookingDocumentSupporter), ((IDocumentSupportable)Factory.New<CommonConsolidatedTransportBooking>()).DocumentSupporter.GetType());
		}

		#endregion

		#region IEDocsProvider

		public void TestGetEDocsProviderSupporter()
		{
			AssertEquals(typeof(JobInvoicingEDocsProviderSupporter), ((IEDocsProvider)Factory.New<CommonConsolidatedTransportBooking>()).GetEDocsProviderSupporter().GetType());
		}

		#endregion

		#region IDocManagerSupport

		public void TestDocManagerInfo()
		{
			AssertEquals(typeof(ConsolidatedTransportBookingDocManagerInfo), ((IDocManagerSupport)Factory.New<CommonConsolidatedTransportBooking>()).DocManagerInfo.GetType());
		}

		#endregion
	}
}

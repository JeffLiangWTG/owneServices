using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonConsolidatedTransportBookingDocumentSupporterTest : TestCaseWithFactory
	{
		[ExpectExceptionMessage(typeof(ArgumentException), "Invalid DataContext: Shipment")]
		public void TestRunSheetDocSupporterThrowsExceptionWithInvalidDataContext()
		{
			iBooking.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.Shipment, null);
		}

		public void TestSupportedDataContext()
		{
			AssertEquals("Constants.DataContext.ConsolidatedTransportBooking is Supported", true, iBooking.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.ConsolidatedTransportBooking)));
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals(Env.Security.ConsolidatedTransportBookingCustomiseDocuments, iBooking.DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.TransportBooking, iBooking.DocumentSupporter.BusinessContext);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			booking = Factory.New<CommonConsolidatedTransportBooking>();
			iBooking = booking;
		}
		CommonConsolidatedTransportBooking booking;
		IDocumentSupportable iBooking;

		#endregion
	}
}

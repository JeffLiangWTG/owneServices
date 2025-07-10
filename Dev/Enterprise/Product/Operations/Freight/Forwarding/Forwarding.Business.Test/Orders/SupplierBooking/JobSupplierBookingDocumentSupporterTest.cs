using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(JobSupplierBookingDocumentSupporter))]
	sealed class JobSupplierBookingDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestSupportedDataContext()
		{
			var supplierBooking = Factory.New<JobSupplierBooking>();
			AssertEquals("Core.Constants.DataContext.JobSupplierBooking is Supported", true, supplierBooking.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.JobSupplierBooking)));
			AssertEquals("Core.Constants.DataContext.GenericFreightJob is not Supported", true, supplierBooking.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GenericFreightJob)));
		}

		public void TestBusinessContext()
		{
			var supplierBooking = Factory.New<JobSupplierBooking>();
			AssertEquals(BusinessContext.JobSupplierBooking, supplierBooking.DocumentSupporter.BusinessContext);
		}

		public void TestShowReasonForNotPrinting()
		{
			var supplierBooking = Factory.New<JobSupplierBooking>();
			AssertEquals("Core.Constants.DataContext.JobSupplierBooking should support printing", false, supplierBooking.DocumentSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.JobSupplierBooking, null));
			AssertEquals("Core.Constants.DataContext.GenericFreightJob should not support printing", false, supplierBooking.DocumentSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.GenericFreightJob, null));
		}

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<JobSupplierBooking>();
		}

		#endregion
	}
}

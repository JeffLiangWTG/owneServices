using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Application.InversionOfControl;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.eTail.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class HVLVItemRTUSBookingProviderTest : RTUSBookingProviderTest<HVLVItemRTUSBookingProvider, HVLVItem>
	{
		protected override HVLVItemRTUSBookingProvider GetLastMileCarrierBookingForTest() => new HVLVItemRTUSBookingProvider(Factory);

		public void TestBookLastMileCarrier_Success()
		{
			BookLastMileCarrier_Success(1);
		}

		public void TestBookLastMileCarrier_Failed_RTUSMalfunction()
		{
			var expectedErrorMessage = @"Booking failed for ItemId01 : I don't know why but something terrible's happened.";
			BookLastMileCarrier_Failed_RTUSMalfunction(new[] { "KK00101" }, 1, expectedErrorMessage);
		}

		public void TestBookLastMileCarrier_Failed_RTUSReturnsNull()
		{
			var expectedErrorMessage = @"Failed to get available RTUS response from https://book.carrier.com/ for ItemId01";

			BookLastMileCarrier_Failed_RTUSReturnsNull(expectedErrorMessage);
		}

		public void TestBookLastMileCarrier_Exception_Empty_CreateUniversalShipments()
		{
			var pk = bizo.PK.ToGuid();
			var expectedErrorMessage = $"Unable to find HVLVItem by PK [{pk}]";

			BookLastMileCarrier_Exception_Empty_CreateUniversalShipments(expectedErrorMessage);
		}

		public void TestBookLastMileCarrier_Failed_InvalidRTUSConfig()
		{
			var expectedErrorMessage = "RTUS is required for making Carrier Booking. Please raise an eRequest (CR9) for RTUS configuration";
			ILastMileCarrierBookingResponseCollection responseCollection = new LastMileCarrierBookingResponseCollection();

			AssertEquals("Precondition", false, responseCollection.HasError);
			AssertNoExceptionThrown("Expected no exception to be thrown if bad RTU configuration", () => responseCollection = (LastMileCarrierBookingResponseCollection)provider.BookLastMileCarrier(bizo.PK.ToGuid()));
			Assert("Expected response collection HasError to be true", responseCollection.HasError);
			AssertEquals("Expected the following error message", expectedErrorMessage, responseCollection.ErrorMessage);
		}

		protected override HVLVItem CreateBusinessObjectForTest()
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_CurrentBarcode = "KK00101";
			item.HVI_CarrierBookingStatus = "NON";
			item.Consignment.HVC_OH_LastMileCarrierBookingAgent = BookingAgentPK;

			Factory.Save();

			item.HVI_ItemId = "ItemId01";
			return item;
		}

		protected override void AssertBookRequestedButFailedLogs(IEnumerable<StmALog> logs)
		{
			var logBKQ = logs.Single(l => l.SL_SE_NKEvent == AutoEvents.BookingRequestedCode);
			AssertEquals("|TYP=Last Mile Carrier", logBKQ.SL_Reference);

			var logBKJ = logs.Single(l => l.SL_SE_NKEvent == AutoEvents.BookingRejectedCode);
			AssertEquals("|TYP=Last Mile Carrier|REF=ItemId01", logBKJ.SL_Reference);
		}

		protected override void AssertBookRequestedAndSucceededLogs(IEnumerable<StmALog> logs)
		{
			var logBKQ = logs.Single(l => l.SL_SE_NKEvent == AutoEvents.BookingRequestedCode);
			AssertEquals("|TYP=Last Mile Carrier", logBKQ.SL_Reference);

			var logBKC = logs.Single(l => l.SL_SE_NKEvent == AutoEvents.BookingConfirmedCode);
			AssertEquals("|TYP=Last Mile Carrier|OLD=KK00101|NEW=ItemId01", logBKC.SL_Reference);
		}

		protected override void AssertBizoUpdatedAfterBookingSucceeded()
		{
			AssertEquals("ItemId01", bizo.HVI_CurrentBarcode);
			AssertEquals("Carrier booking status should update to Booking Confirmed", "BKC", bizo.HVI_CarrierBookingStatus);
		}

		protected override void AssertBizoUpdatedAfterBookingFailed()
		{
			AssertEquals("Carrier booking status should update to Booking Rejected", "BKJ", bizo.HVI_CarrierBookingStatus);
		}

		public void TestHVLVItemRTUSBookingProviderIsNotSingleton()
		{
			var definition = (ObjectDefinition)typeof(ObjectFactory)
				.InvokeMember(
					"GetObjectDefinition",
					BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Static,
					null,
					null,
					new object[] { nameof(HVLVItemRTUSBookingProvider) });
			Assert("HVLVItemRTUSBookingProvider instance should not be singleton", !definition.IsSingleton);
		}

		[UseSnapshotProtection]
		public void TestProviderAlwaysHasLatestBizoInfo()
		{
			// Create an instance so that ObjectFactory may cache this for singleton use
			var newProvider = ObjectFactory.Get<ILastMileCarrierBookingService>(nameof(HVLVItemRTUSBookingProvider), Factory) as HVLVItemRTUSBookingProvider;
			SetUpRTUSConfig();

			AssertEquals("Precondition", "KK00101", bizo.HVI_CurrentBarcode);

			Db.Connection.BeginTransaction(); // Updating item's barcode without notifying BusinessObjectFactory
			try
			{
				ConcurrencyInfo.SetConcurrencyPolicy(bizo, nameof(HVLVItem.HVI_CurrentBarcode), ConcurrencyPolicy.Ignore);
				Db.Connection.ExecuteNonQuery($"UPDATE dbo.HVLVItem SET HVI_CurrentBarCode = 'NEWBARCODE001' WHERE HVI_PK='{bizo.PK}'");
			}
			finally
			{
				Db.Connection.CommitTransaction(); // Updating item's barcode without notifying BusinessObjectFactory
			}

			// newProvider uses new factory as it isn't singleton mode
			newProvider = ObjectFactory.Get<ILastMileCarrierBookingService>(nameof(HVLVItemRTUSBookingProvider), new BusinessObjectFactory()) as HVLVItemRTUSBookingProvider;
			newProvider.BookLastMileCarrier(bizo.PK.ToGuid());

			var log = bizo.Logs.GetAllLogs().OfType<StmALog>().Single(l => l.SL_SE_NKEvent == AutoEvents.BookingConfirmedCode);
			AssertEquals("|TYP=Last Mile Carrier|OLD=NEWBARCODE001|NEW=" + bizo.HVI_ItemId, log.SL_Reference);
		}

		public void TestCancellationRequestLog()
		{
			var newProvider = ObjectFactory.Get<ILastMileCarrierBookingService>(nameof(HVLVItemRTUSBookingProvider), Factory) as HVLVItemRTUSBookingProvider;
			SetUpRTUSConfig();

			newProvider.CancelBooking(bizo.PK.ToGuid());
			var log = bizo.Logs.GetAllLogs().OfType<StmALog>().Single(l => l.SL_SE_NKEvent == AutoEvents.BookingCancelledCode);
			AssertEquals("|TYP=Last Mile Carrier Booking|REF=" + bizo.HVI_CurrentBarcode, log.SL_Reference);
			AssertEquals("Carrier booking status should update to Booking Canceled", "BKL", bizo.HVI_CarrierBookingStatus);
		}

		public void TestCancellationNullResponse()
		{
			CancellationNullResponse(ExcpectedeDocsFileNames[0], 2);
		}

		public void TestCancellationRejected()
		{
			SetUpRTUSConfig();
			processor.FailStrings = new[] { "ItemId01" };
			var response = provider.CancelBooking(bizo.PK.ToGuid()).Single();

			AssertEquals("Cancellation failed for ItemId01 : I don't know why but something terrible's happened.", response.ErrorMessage);

			var logs = bizo.Logs.GetAllLogs().OfType<StmALog>();
			var rejected = logs.Where(l => l.SL_SE_NKEvent == AutoEvents.BookingRejectedCode).OrderBy(l => l.SL_EventTime).ToArray();

			AssertEquals("Should have one rejected logs", 1, rejected.Length);
			AssertEquals("|TYP=Last Mile Carrier|REF=ItemId01", rejected[0].SL_Reference);

			AssertEquals("Carrier booking status should not change", "NON", bizo.HVI_CarrierBookingStatus);
		}

		protected override void DeleteItems()
		{
			bizo.Delete();
			Factory.Save();
		}

		protected override int ExpectedeDocsCount => 1;

		protected override string[] ExcpectedeDocsFileNames => new[] { "ItemId01" };
	}
}

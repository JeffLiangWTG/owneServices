using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class HVLVConsignmentRTUSBookingProviderTest : RTUSBookingProviderTest<HVLVConsignmentRTUSBookingProvider, HVLVConsignment>
	{
		protected override HVLVConsignmentRTUSBookingProvider GetLastMileCarrierBookingForTest() => new HVLVConsignmentRTUSBookingProvider(Factory);

		public void TestBookLastMileCarrier_Success()
		{
			BookLastMileCarrier_Success(2);
		}

		public void TestBookLastMileCarrier_Failed_RTUSMalfunction()
		{
			var expectedErrorMessage = @"Booking failed for ItemId01 : I don't know why but something terrible's happened.
Booking failed for ItemId02 : I don't know why but something terrible's happened.";
			BookLastMileCarrier_Failed_RTUSMalfunction(new[] { "KK00101", "KK00102" }, 2, expectedErrorMessage);
		}

		public void TestBookLastMileCarrier_Partial_Failed_RTUSMalfunction()
		{
			var expectedErrorMessage = @"Booking failed for ItemId01 : I don't know why but something terrible's happened.";

			SetUpRTUSConfig();
			processor.FailStrings = new[] { "KK00101" };
			var responseCollection = provider.BookLastMileCarrier(bizo.PK.ToGuid());

			AssertEquals("Response count should match", 2, responseCollection.Count);
			Assert("HasError should be true", responseCollection.HasError);
			AssertEquals("Aggregated error Messages should match", expectedErrorMessage, responseCollection.ErrorMessage);

			var logs = bizo.Logs.GetAllLogs().OfType<StmALog>();
			AssertEquals(4, logs.Count());

			var logBKQ = logs.Single(l => l.SL_SE_NKEvent == AutoEvents.BookingRequestedCode);
			var rejected = logs.Single(l => l.SL_SE_NKEvent == AutoEvents.BookingRejectedCode);
			var succeeded = logs.Single(l => l.SL_SE_NKEvent == AutoEvents.BookingConfirmedCode);

			AssertEquals("|TYP=Last Mile Carrier", logBKQ.SL_Reference);
			AssertEquals("|TYP=Last Mile Carrier|REF=ItemId01", rejected.SL_Reference);
			AssertEquals("|TYP=Last Mile Carrier|REF=ItemId02", succeeded.SL_Reference);
		}

		public void TestBookLastMileCarrier_Exception_Empty_CreateUniversalShipments()
		{
			var expectedErrorMessage = @"Failed to create Universal Shipment for HVLVConsignment";

			BookLastMileCarrier_Exception_Empty_CreateUniversalShipments(expectedErrorMessage);
		}

		public void TestBookLastMileCarrier_Failed_RTUSReturnsNull()
		{
			var expectedErrorMessage = @"Failed to get available RTUS response from https://book.carrier.com/ for ItemId01
Failed to get available RTUS response from https://book.carrier.com/ for ItemId02";

			BookLastMileCarrier_Failed_RTUSReturnsNull(expectedErrorMessage);
		}

		public void TestBookLastMileCarrier_Failed_InvalidRTUSConfig()
		{
			var expectedErrorMessage = "RTUS is required for making Carrier Booking. Please raise an eRequest (CR9) for RTUS configuration\r\nRTUS is required for making Carrier Booking. Please raise an eRequest (CR9) for RTUS configuration";
			var responseCollection = new LastMileCarrierBookingResponseCollection();

			AssertEquals("Precondition", false, responseCollection.HasError);
			AssertNoExceptionThrown("Expected no exception to be thrown if bad RTU configuration", () => responseCollection = (LastMileCarrierBookingResponseCollection)provider.BookLastMileCarrier(bizo.PK.ToGuid()));
			Assert("Expected response collection HasError to be true", responseCollection.HasError);
			AssertEquals("Expected the following error message", expectedErrorMessage, responseCollection.ErrorMessage);
		}

		protected override HVLVConsignment CreateBusinessObjectForTest()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_OH_LastMileCarrierBookingAgent = BookingAgentPK;

			var item1 = CreateHVLVItem("KK00101", consignment.PK);
			var item2 = CreateHVLVItem("KK00102", consignment.PK);

			Factory.Save();

			item1.HVI_ItemId = "ItemId01";
			item2.HVI_ItemId = "ItemId02";

			return consignment;
		}

		protected override void AssertBookRequestedButFailedLogs(IEnumerable<StmALog> logs)
		{
			AssertEquals(4, logs.Count());
			var logBKQ = logs.Single(l => l.SL_SE_NKEvent == AutoEvents.BookingRequestedCode);
			AssertEquals("|TYP=Last Mile Carrier", logBKQ.SL_Reference);

			var rejected = logs.Where(l => l.SL_SE_NKEvent == AutoEvents.BookingRejectedCode).ToArray();
			AssertEquals(2, rejected.Length);

			var expectedReferencesForRejectedLogs = new[] { "|TYP=Last Mile Carrier|REF=ItemId01", "|TYP=Last Mile Carrier|REF=ItemId02" };
			AssertContainsExactElementsInAnyOrder(expectedReferencesForRejectedLogs, rejected.Select(x => x.SL_Reference));
		}

		protected override void AssertBookRequestedAndSucceededLogs(IEnumerable<StmALog> logs)
		{
			AssertEquals(4, logs.Count());
			var logBKQ = logs.Single(l => l.SL_SE_NKEvent == AutoEvents.BookingRequestedCode);
			AssertEquals("|TYP=Last Mile Carrier", logBKQ.SL_Reference);

			var succeeded = logs.Where(l => l.SL_SE_NKEvent == AutoEvents.BookingConfirmedCode).ToArray();
			AssertEquals("Should have two booking succeeded", 2, succeeded.Length);

			AssertContainsExactElementsInAnyOrder("", new ZString[] { "|TYP=Last Mile Carrier|REF=ItemId01", "|TYP=Last Mile Carrier|REF=ItemId02" }, new ZString[] { succeeded[0].SL_Reference, succeeded[1].SL_Reference });
		}

		protected override void AssertBizoUpdatedAfterBookingSucceeded()
		{
			for (var i = 1; i <= bizo.Items.Count; i++)
			{
				var item = bizo.Items[i - 1];
				AssertEquals("ItemId0" + i, item.HVI_CurrentBarcode);
			}
		}

		public void TestCancellationNullResponse()
		{
			CancellationNullResponse(ExcpectedeDocsFileNames[1], 4);
		}

		public void TestCancellationRejected()
		{
			SetUpRTUSConfig();
			processor.FailStrings = new[] { "ItemId01", "ItemId02" };
			var responseCollection = provider.CancelBooking(bizo.PK.ToGuid());

			AssertEquals("Should have two responses", 2, responseCollection.Count);

			AssertEquals("Cancellation failed for ItemId01 : I don't know why but something terrible's happened.", responseCollection[0].ErrorMessage);
			AssertEquals("Cancellation failed for ItemId02 : I don't know why but something terrible's happened.", responseCollection[1].ErrorMessage);

			var logs = bizo.Logs.GetAllLogs().OfType<StmALog>();
			var rejected = logs.Where(l => l.SL_SE_NKEvent == AutoEvents.BookingRejectedCode).ToArray();

			AssertEquals("Should have two rejected logs", 2, rejected.Length);
			AssertContainsExactElementsInAnyOrder(new[] { "|TYP=Last Mile Carrier|REF=ItemId01", "|TYP=Last Mile Carrier|REF=ItemId02" }, new[] { rejected[0].SL_Reference, rejected[1].SL_Reference });
		}

		#region Implementation

		protected HVLVItem CreateHVLVItem(string barCode, ZGuid consignmentPk)
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_CurrentBarcode = barCode;
			item.Consignment.HVC_OH_LastMileCarrierBookingAgent = BookingAgentPK;
			item.HVI_HVC_Consignment = consignmentPk;

			return item;
		}

		protected override void DeleteItems()
		{
			bizo.Items.RemoveAndDeleteAll();
			Factory.Save();
		}

		protected override int ExpectedeDocsCount => 2;

		protected override string[] ExcpectedeDocsFileNames => new[] { "ItemId01", "ItemId02" };

		#endregion
	}
}

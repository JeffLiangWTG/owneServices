using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(CoLoadForwardingShipmentCollection))]
	public class CoLoadForwardingShipmentCollectionTest : ColoadShipmentCollectionTest
	{
		public void TestShouldUpdateScreeningStatus_OnAdded()
		{
			AssertParentShipmentShouldUpdateScreeningStatusOnChildAdded(ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.JobCleared, false);
			AssertParentShipmentShouldUpdateScreeningStatusOnChildAdded(ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.Clear, false);
			AssertParentShipmentShouldUpdateScreeningStatusOnChildAdded(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.JobCleared, true);
			AssertParentShipmentShouldUpdateScreeningStatusOnChildAdded(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Clear, true);

			void AssertParentShipmentShouldUpdateScreeningStatusOnChildAdded(string parentShipmentScreeningStatus,
				string childShipmentScreeningStatus, bool expectedParentShipmentShouldUpdateScreeningStatus)
			{
				var parentShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				parentShipment.JS_ScreeningStatus = parentShipmentScreeningStatus;
				AssertEquals("PreCondition", false, ((IShouldUpdateScreeningStatus)parentShipment).ShouldUpdateScreeningStatus);

				var childShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				childShipment.JS_ScreeningStatus = childShipmentScreeningStatus;
				parentShipment.CoLoadShipments.Add(childShipment);
				AssertEquals(expectedParentShipmentShouldUpdateScreeningStatus, ((IShouldUpdateScreeningStatus)parentShipment).ShouldUpdateScreeningStatus);
			}
		}

		public void TestShouldUpdateScreeningStatus_OnRemoved()
		{
			AssertParentShipmentShouldUpdateScreeningStatusOnChildRemoved(ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.JobCleared, false);
			AssertParentShipmentShouldUpdateScreeningStatusOnChildRemoved(ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.Clear, false);
			AssertParentShipmentShouldUpdateScreeningStatusOnChildRemoved(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.JobCleared, true);
			AssertParentShipmentShouldUpdateScreeningStatusOnChildRemoved(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Clear, true);

			void AssertParentShipmentShouldUpdateScreeningStatusOnChildRemoved(string parentShipmentScreeningStatus,
				string childShipmentScreeningStatus, bool expectedParentShipmentShouldUpdateScreeningStatus)
			{
				var parentShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				parentShipment.JS_ScreeningStatus = parentShipmentScreeningStatus;

				var childShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				childShipment.JS_ScreeningStatus = childShipmentScreeningStatus;
				parentShipment.CoLoadShipments.Add(childShipment);
				((IShouldUpdateScreeningStatus)parentShipment).ShouldUpdateScreeningStatus = false;
				AssertEquals("PreCondition", false, ((IShouldUpdateScreeningStatus)parentShipment).ShouldUpdateScreeningStatus);

				parentShipment.CoLoadShipments.RemoveAll();
				AssertEquals(expectedParentShipmentShouldUpdateScreeningStatus, ((IShouldUpdateScreeningStatus)parentShipment).ShouldUpdateScreeningStatus);
			}
		}

		public void TestSetBookingPartyDocumentaryAddressChildOnMaster_NoRelatedShipments()
		{
			var parentShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var subShipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			parentShipment.CoLoadShipments.Add(subShipment1);

			AssertEquals("Master no BPDA, Sub no BPDA", true, parentShipment.BookingPartyDocumentaryAddress.IsEmpty);

			parentShipment.CoLoadShipments.RemoveAll();

			var bookingParty1 = Factory.New<OrgHeader>();
			bookingParty1.OH_Code = "BPDA_SYD";
			bookingParty1.OH_FullName = "Booking Party Documentary Address Sydney";
			bookingParty1.OH_RL_NKClosestPort = "AUSYD";
			bookingParty1.MainAddress.Address1 = "Unit 200";
			bookingParty1.MainAddress.Address2 = "55 Why Lane";
			bookingParty1.MainAddress.City = "Sydney";
			bookingParty1.MainAddress.Postcode = "2000";
			bookingParty1.MainAddress.OA_RN_NKCountryCode = "AU";

			subShipment1.BookingPartyDocumentaryAddress.OrganisationPK = bookingParty1.PK;
			parentShipment.CoLoadShipments.Add(subShipment1);

			AssertEquals("Master no BPDA, Sub with BPDA", false, parentShipment.BookingPartyDocumentaryAddress.IsEmpty);
			AssertEquals("Master no BPDA, Sub with BPDA", "Booking Party Documentary Address Sydney", parentShipment.BookingPartyDocumentaryAddress.CompanyName);

			parentShipment.BookingPartyDocumentaryAddress.Delete();

			var subShipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			subShipment2.BookingPartyDocumentaryAddress.OrganisationPK = bookingParty1.PK;
			parentShipment.CoLoadShipments.Add(subShipment2);

			AssertEquals("Master no BPDA, existing related shipment with same BPDA, Sub with BPDA", false, parentShipment.BookingPartyDocumentaryAddress.IsEmpty);
			AssertEquals("Master no BPDA, existing related shipment with same BPDA, Sub with BPDA", "Booking Party Documentary Address Sydney", parentShipment.BookingPartyDocumentaryAddress.CompanyName);

			var bookingParty2 = Factory.New<OrgHeader>();
			bookingParty2.OH_Code = "BPDA_MEL";
			bookingParty2.OH_FullName = "Booking Party Documentary Address Melbourne";
			bookingParty2.OH_RL_NKClosestPort = "AUMEL";
			bookingParty2.MainAddress.Address1 = "Unit 200";
			bookingParty2.MainAddress.Address2 = "55 Why Lane";
			bookingParty2.MainAddress.City = "Melbourne";
			bookingParty2.MainAddress.Postcode = "2000";
			bookingParty2.MainAddress.OA_RN_NKCountryCode = "AU";
			parentShipment.BookingPartyDocumentaryAddress.OrganisationPK = bookingParty2.PK;

			var subShipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			subShipment3.BookingPartyDocumentaryAddress.OrganisationPK = bookingParty1.PK;
			parentShipment.CoLoadShipments.Add(subShipment3);

			AssertEquals("Master with BPDA, Sub with BPDA", false, parentShipment.BookingPartyDocumentaryAddress.IsEmpty);
			AssertEquals("Master with BPDA, Sub with BPDA", "Booking Party Documentary Address Melbourne", parentShipment.BookingPartyDocumentaryAddress.CompanyName);

			parentShipment.BookingPartyDocumentaryAddress.Delete();

			var bookingParty3 = Factory.New<OrgHeader>();
			bookingParty3.OH_Code = "BPDA_ANT";
			bookingParty3.OH_FullName = "Booking Party Documentary Address Antwerp";
			bookingParty3.OH_RL_NKClosestPort = "BEANR";
			bookingParty3.MainAddress.Address1 = "Unit 200";
			bookingParty3.MainAddress.Address2 = "55 Why Lane";
			bookingParty3.MainAddress.City = "Antwerp";
			bookingParty3.MainAddress.Postcode = "2000";
			bookingParty3.MainAddress.OA_RN_NKCountryCode = "BE";

			var subShipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			subShipment4.BookingPartyDocumentaryAddress.OrganisationPK = bookingParty3.PK;
			parentShipment.CoLoadShipments.Add(subShipment4);

			AssertEquals("Master no BPDA, existing related shipment with different BPDA, Sub with BPDA", true, parentShipment.BookingPartyDocumentaryAddress.IsEmpty);

			parentShipment.BookingPartyDocumentaryAddress.Delete();
			parentShipment.CoLoadShipments.Remove(subShipment4);

			AssertEquals("Master no BPDA, existing related shipment with same BPDA", false, parentShipment.BookingPartyDocumentaryAddress.IsEmpty);
			AssertEquals("Master no BPDA, existing related shipment with same BPDA", "Booking Party Documentary Address Sydney", parentShipment.BookingPartyDocumentaryAddress.CompanyName);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CoLoadForwardingShipmentCollection((ForwardingShipment)Helper.MasterShipment, Factory);
		}

		protected override ColoadShipmentTestHelper GetHelper()
		{
			return new ColoadForwardingShipmentTestHelper(Factory);
		}

		public class ColoadForwardingShipmentTestHelper : ColoadShipmentTestHelper
		{
			public ColoadForwardingShipmentTestHelper(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override CommonShipment GetNewShipmentCore()
			{
				return factory.New<ForwardingShipment>();
			}
		}
	}
}

using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Tracking.Business.Testing
{
	public class BookingDocumentsServiceTest : TestCaseWithFactory
	{
		public void TestPrintFreightLabels_NoContact()
		{
			var booking = GetNewBooking();
			Factory.Save();

			var result = GetResult(TrackingDocumentTypes.FreightLabels, Guid.NewGuid(), booking.PK.ToGuid());

			AssertNull(result);
		}

		public void TestPrintHouseBills_NoContact()
		{
			var booking = GetNewBooking();
			Factory.Save();

			var result = GetResult(TrackingDocumentTypes.HouseBills, Guid.NewGuid(), booking.PK.ToGuid());

			AssertNull(result);
		}

		public void TestPrintFreightLabels_NoPacks()
		{
			var contact = GetNewContact();
			var booking = GetNewBooking(false);
			Factory.Save();

			var result = GetResult(TrackingDocumentTypes.FreightLabels, contact.PK.ToGuid(), booking.PK.ToGuid());

			AssertEquals("Cannot produce this Document because the data required to do so is not present", result?.ErrorMessage);
		}

		public void TestPrintHouseBill_NotUnitedStates()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var contact = GetNewContact();
				var booking = GetNewBooking();
				Factory.Save();

				var result = GetResult(TrackingDocumentTypes.HouseBills, contact.PK.ToGuid(), booking.PK.ToGuid());

				AssertNull(result);
			}
		}

		public void TestPrintFreightLabels()
		{
			var contact = GetNewContact();
			var booking = GetNewBooking();
			Factory.Save();

			var result = GetResult(TrackingDocumentTypes.FreightLabels, contact.PK.ToGuid(), booking.PK.ToGuid());

			CombineAssertions(() =>
			{
				AssertNotNull(result);
				AssertEquals("Freight Label.pdf", result.FileName);
				Assert(result.FileContents.Length > 0);
			});
		}

		public void TestPrintHouseBill_BookingHouseBill()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var contact = GetNewContact();
				var booking = GetNewBooking();
				booking.JS_TransportMode = "AIR";
				Factory.Save();

				var result = GetResult(TrackingDocumentTypes.HouseBills, contact.PK.ToGuid(), booking.PK.ToGuid());

				CombineAssertions(() =>
				{
					AssertNotNull(result);
					AssertEquals("Booking House Bill.pdf", result.FileName);
					Assert(result.FileContents.Length > 0);
				});
			}
		}

		public void TestPrintHouseBill_BillOfLading()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var contact = GetNewContact();
				var booking = GetNewBooking();
				booking.JS_TransportMode = "SEA";
				Factory.Save();

				var result = GetResult(TrackingDocumentTypes.HouseBills, contact.PK.ToGuid(), booking.PK.ToGuid());

				CombineAssertions(() =>
				{
					AssertNotNull(result);
					AssertEquals("Booking House Bill.pdf", result.FileName);
					Assert(result.FileContents.Length > 0);
				});
			}
		}

		IWebTrackerPrintResult GetResult(TrackingDocumentTypes docType, Guid contactPK, Guid shipmentPK)
		{
			switch (docType)
			{
				case TrackingDocumentTypes.FreightLabels:
					return service.PrintFreightLabel(contactPK, shipmentPK);
				case TrackingDocumentTypes.HouseBills:
					return service.PrintHouseBill(contactPK, shipmentPK);
				default:
					throw new ArgumentException(docType.ToString());
			}
		}

		OrgContact GetNewContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = org.OH_Code;
			return org.Contacts.AddNew();
		}

		ForwardingShipment GetNewBooking(bool addPack = true)
		{
			var booking = Factory.NewWithValidTestData<ForwardingShipment>();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			booking.ConsigneePK = consignee.PK;
			booking.ConsignorPK = consignor.PK;
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "USORD";
			booking.JS_BookingReference = "Test Booking";

			if (addPack)
			{
				var line = booking.OuterPackLines.AddNew();
				line.FillWithValidTestData();
				line.JL_PackageCount = 1;
			}

			return booking;
		}

		protected override void SetUp()
		{
			base.SetUp();
			service = new BookingDocumentsService();
		}
		BookingDocumentsService service;
	}
}

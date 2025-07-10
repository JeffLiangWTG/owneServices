using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.GUI;
using Enterprise.DocumentEngine.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	[HttpContextEnabledTest]
	public class TrackingBookingDocumentsMenuHelperTest : DocumentsMenuHelperTest
	{
		protected override void SetUp()
		{
			Globals.IsWeb = true;
			base.SetUp();
		}

		protected override void TearDown()
		{
			Globals.IsWeb = false;
			base.TearDown();
		}

		#region TestConstructorByPK

		public void TestConstructorByPK()
		{
			TestHelper testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);

			AssertConstructorByPK(typeof(TrackingBookingDocumentsMenuHelper));
		}

		#endregion

		#region TestIDocumentsMenuHelper

		public void TestGetAvailableDocumentsForFreightLabelsOnly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				TrackingBooking = new TrackingBooking(Factory, new TrackingSiteUser());
				Helper = new TrackingBookingDocumentsMenuHelper(TrackingBooking, TrackingDocumentTypes.FreightLabels);

				AssertFreightLabel(Core.Constants.ContainerModes.LCL, true);
				AssertFreightLabel(Core.Constants.ContainerModes.FCL, false);

				AssertHousebill(Core.Constants.TransportModes.Air, false, "Booking House Bill");
				AssertHousebill(Core.Constants.TransportModes.Sea, false, "Booking House Bill");

				AssertHousebill(Core.Constants.TransportModes.Sea, false, "Bill Of Lading");
				AssertHousebill(Core.Constants.TransportModes.Air, false, "Bill Of Lading");
			}
		}

		public void TestFreightLabelIsNotAvailableWhenDocumentUnpublished()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				TrackingBooking = new TrackingBooking(Factory, new TrackingSiteUser());
				var helper = new TrackingBookingDocumentsMenuHelper(TrackingBooking, TrackingDocumentTypes.FreightLabels);

				TrackingBooking.Booking.JS_PackingMode = Core.Constants.ContainerModes.LCL;

				var menuItems = helper.GetAvailableDocuments();
				AssertEquals(1, menuItems.Count);

				var query = typeof(TrackingBookingDocumentsMenuHelper).GetMethod("GetFreightLabelFilter", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(helper, null) as ZQuery;
				var menuItem = Factory.Load<StmMenuItemBase>(query).Single(x => x.SU_MenuName.Equals("Freight Label"));
				menuItem.SU_IsPublished = false;

				Factory.Save();

				menuItems = helper.GetAvailableDocuments();
				AssertEquals(0, menuItems.Count);
			}
		}

		public void TestGetAvailableDocumentsForHousebillsOnly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				TrackingBooking = new TrackingBooking(Factory, new TrackingSiteUser());
				Helper = new TrackingBookingDocumentsMenuHelper(TrackingBooking, TrackingDocumentTypes.HouseBills);

				AssertFreightLabel(Core.Constants.ContainerModes.LCL, false);
				AssertFreightLabel(Core.Constants.ContainerModes.FCL, false);

				AssertHousebill(Core.Constants.TransportModes.Air, true, "Booking House Bill");
				AssertHousebill(Core.Constants.TransportModes.Sea, false, "Booking House Bill");

				AssertHousebill(Core.Constants.TransportModes.Sea, true, "Bill Of Lading");
				AssertHousebill(Core.Constants.TransportModes.Air, false, "Bill Of Lading");
			}
		}

		public void TestHouseBillIsNotAvailableWhenDocumentUnpublished()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				TrackingBooking = new TrackingBooking(Factory, new TrackingSiteUser());
				var helper = new TrackingBookingDocumentsMenuHelper(TrackingBooking, TrackingDocumentTypes.HouseBills);

				TrackingBooking.Booking.JS_TransportMode = Core.Constants.TransportModes.Air;

				var menuItems = helper.GetAvailableDocuments();
				AssertEquals(1, menuItems.Count);

				var query = typeof(TrackingBookingDocumentsMenuHelper).GetMethod("GetLaserHAWBFilter", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(helper, null) as ZQuery;
				var menuItem = Factory.Load<StmMenuItemBase>(query).Single(x => x.SU_MenuName.Equals("Booking House Bill"));
				menuItem.SU_IsPublished = false;

				Factory.Save();

				menuItems = helper.GetAvailableDocuments();
				AssertEquals(0, menuItems.Count);
			}
		}

		public void TestBillOfLadingIsNotAvailableWhenDocumentUnpublished()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				TrackingBooking = new TrackingBooking(Factory, new TrackingSiteUser());
				var helper = new TrackingBookingDocumentsMenuHelper(TrackingBooking, TrackingDocumentTypes.HouseBills);

				TrackingBooking.Booking.JS_TransportMode = Core.Constants.TransportModes.Sea;
				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.MiscServ.OM_EXAllowedToPrintOriginalBL = true;
				TrackingBooking.ConsignorOrganisationPK = consignor.PK;

				var menuItems = helper.GetAvailableDocuments();
				AssertEquals(1, menuItems.Count);

				var query = typeof(TrackingBookingDocumentsMenuHelper).GetMethod("GetBillOfLadingFilter", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(helper, null) as ZQuery;
				var menuItem = Factory.Load<StmMenuItemBase>(query).Single(x => x.SU_MenuName.Equals("Bill Of Lading"));
				menuItem.SU_IsPublished = false;

				Factory.Save();

				menuItems = helper.GetAvailableDocuments();
				AssertEquals(0, menuItems.Count);
			}
		}

		public void TestGetAvailableDocumentsForAnyDocuments()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				TrackingBooking = new TrackingBooking(Factory, new TrackingSiteUser());
				Helper = new TrackingBookingDocumentsMenuHelper(TrackingBooking);

				AssertEquals("PKForBizOCreation should be equal to trackingBooking.BookingPK", TrackingBooking.BookingPK, Helper.PKForBizOCreation);

				AssertFreightLabel(Core.Constants.ContainerModes.LCL, true);
				AssertFreightLabel(Core.Constants.ContainerModes.FCL, false);

				AssertHousebill(Core.Constants.TransportModes.Air, true, "Booking House Bill");
				AssertHousebill(Core.Constants.TransportModes.Sea, false, "Booking House Bill");

				AssertHousebill(Core.Constants.TransportModes.Sea, true, "Bill Of Lading");
				AssertHousebill(Core.Constants.TransportModes.Air, false, "Bill Of Lading");
			}
		}

		public void TestGetAvailableDocumentsForNullBooking()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				TrackingBooking = new TrackingBooking(Factory, new TrackingSiteUser());
				Helper = new TrackingBookingDocumentsMenuHelper(null);

				AssertFreightLabel(Core.Constants.ContainerModes.LCL, false);
				AssertFreightLabel(Core.Constants.ContainerModes.FCL, false);

				AssertHousebill(Core.Constants.TransportModes.Air, false, "Booking House Bill");
				AssertHousebill(Core.Constants.TransportModes.Sea, false, "Booking House Bill");

				AssertHousebill(Core.Constants.TransportModes.Sea, false, "Bill Of Lading");
				AssertHousebill(Core.Constants.TransportModes.Air, false, "Bill Of Lading");

				AssertEquals(0, Helper.GetAvailableDocuments().Count);
			}
		}

		void AssertFreightLabel(string containerMode, bool expectFreightLabel)
		{
			TrackingBooking.Booking.JS_PackingMode = containerMode;
			List<DocumentsMenuItem> menuItems = Helper.GetAvailableDocuments();

			DocumentsMenuItem menuItem = FindDocumentsMenuItem(menuItems, "Freight Label");

			if (expectFreightLabel)
			{
				AssertNotNull("Freight Label document is expected", menuItem);
			}
			else
			{
				AssertNull("Freight Label document is not expected", menuItem);
			}
		}

		public void TestBillOfLadingAvailability()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				const string bookingLaserHAWB = "Booking House Bill";

				TestHelper webTestHelper = new TestHelper(Factory);
				webTestHelper.TestSiteUser.Login(webTestHelper.TestOrg.OH_Code, webTestHelper.TestContact.OC_Email, webTestHelper.TestContact.PasswordForTesting);
				webTestHelper.TestSiteUser.LoggedInOrganisation.OH_IsConsignor = true;

				TrackingBooking = new TrackingBooking(Factory, webTestHelper.TestSiteUser);

				Helper = new TrackingBookingDocumentsMenuHelper(TrackingBooking);

				List<DocumentsMenuItem> menuItems = Helper.GetAvailableDocuments();
				AssertNull("Bill of Lading should not be seen when rights are not granted and it is not sea", FindBillOfLadingMenuItem(menuItems));
				AssertNotNull("Booking Laser HAWB is expected instead of Bill of Lading", FindDocumentsMenuItem(menuItems, bookingLaserHAWB));

				TrackingBooking.Booking.Consignor.MiscServ.OM_EXAllowedToPrintOriginalBL = true;

				menuItems = Helper.GetAvailableDocuments();
				AssertNull("Bill of Lading should not be seen when rights are granted but it is not sea", FindBillOfLadingMenuItem(menuItems));
				AssertNotNull("Booking Laser HAWB is expected instead of Bill of Lading", FindDocumentsMenuItem(menuItems, bookingLaserHAWB));

				TrackingBooking.Booking.JS_TransportMode = Core.Constants.TransportModes.Sea;

				menuItems = Helper.GetAvailableDocuments();
				AssertNotNull("Bill of Lading should be seen when rights are granted and it is sea", FindBillOfLadingMenuItem(menuItems));
				AssertNull("Booking Laser HAWB should not be here", FindDocumentsMenuItem(menuItems, bookingLaserHAWB));

				TrackingBooking.Booking.Consignor.MiscServ.OM_EXAllowedToPrintOriginalBL = false;

				menuItems = Helper.GetAvailableDocuments();
				AssertNull("Bill of Lading should not be seen when rights are not granted even if it is sea", FindBillOfLadingMenuItem(menuItems));
				AssertNotNull("Booking Laser HAWB is expected instead of Bill of Lading", FindDocumentsMenuItem(menuItems, bookingLaserHAWB));
			}
		}

		void AssertHousebill(string transportMode, bool expectHousebill, string expectedName = null)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				TrackingBooking.Booking.JS_TransportMode = transportMode;
				if (transportMode == Core.Constants.TransportModes.Sea)
				{
					var consignor = Factory.NewWithValidTestData<OrgHeader>();
					consignor.MiscServ.OM_EXAllowedToPrintOriginalBL = true;
					TrackingBooking.ConsignorOrganisationPK = consignor.PK;
				}
				List<DocumentsMenuItem> menuItems = Helper.GetAvailableDocuments();

				DocumentsMenuItem menuItem = FindDocumentsMenuItem(menuItems, expectedName);

				if (expectHousebill)
				{
					AssertNotNull("Booking Housebill document is expected", menuItem);
				}
				else
				{
					AssertNull("Booking Housebill document is not expected", menuItem);
				}
			}
		}

		public void TestGetFreightLabelFilter()
		{
			AssertDocumentQuery("GetFreightLabelFilter");
		}

		public void TestGetLaserHAWBFilter()
		{
			AssertDocumentQuery("GetLaserHAWBFilter");
		}

		public void TestGetBillOfLadingFilter()
		{
			AssertDocumentQuery("GetBillOfLadingFilter");
		}

		void AssertDocumentQuery(string queryName)
		{
			TrackingBooking trackingBooking = new TrackingBooking(Factory, new TrackingSiteUser());
			TrackingBookingDocumentsMenuHelper helper = new TrackingBookingDocumentsMenuHelper(trackingBooking);
			ZQuery query = typeof(TrackingBookingDocumentsMenuHelper).GetMethod(queryName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(helper, null) as ZQuery;

			AssertNotNull(string.Format("ZQuery {0} not found", queryName), query);

			StmMenuItemBase[] documents = Factory.Load<StmMenuItemBase>(query);

			AssertEquals(1, documents.Length);
			AssertNotNull(documents[0]);
		}

		public void TestNewMenuItem()
		{
			StmMenuItemBaseCollection collection = new StmMenuItemBaseCollection(Factory);
			StmMenuItemBase newMenuItem = collection.AddNew();
			newMenuItem.SU_IsPublished = true;
			newMenuItem.SU_IsSystemDefined = false;
			newMenuItem.SU_BusinessContext = nameof(BusinessContext.QuotedBooking);
			newMenuItem.SU_MenuPath = "Domestic";
			newMenuItem.SU_MenuName = "Test Menu Item";

			Factory.Save();

			TrackingBooking trackingBooking = new TrackingBooking(Factory, new TrackingSiteUser());
			TrackingBookingDocumentsMenuHelper helper = new TrackingBookingDocumentsMenuHelper(trackingBooking);
			ZQuery query = typeof(TrackingBookingDocumentsMenuHelper).GetMethod("GetFreightLabelFilter", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(helper, null) as ZQuery;

			AssertNotNull(string.Format("ZQuery {0} not found", "GetFreightLabelFilter"), query);

			StmMenuItemBase[] documents = Factory.Load<StmMenuItemBase>(query);

			documents = Factory.Load<StmMenuItemBase>(query);
			AssertEquals(1, documents.Length);
			AssertNotNull(documents[0]);
			AssertEquals("Only Freight Label menu items should be selected", "Freight Label", documents[0].SU_MenuName);
		}

		#endregion

		#region Implementation

		TrackingBooking TrackingBooking;
		DocumentsMenuHelper Helper;

		DocumentsMenuItem FindDocumentsMenuItem(List<DocumentsMenuItem> menuItems, string name)
		{
			return menuItems.Find(match => match.DocumentCommand.SU_MenuName == name);
		}

		DocumentsMenuItem FindBillOfLadingMenuItem(List<DocumentsMenuItem> menuItems)
		{
			return menuItems.Find(match => match.Name == "Bill Of Lading");
		}

		#endregion

	}
}

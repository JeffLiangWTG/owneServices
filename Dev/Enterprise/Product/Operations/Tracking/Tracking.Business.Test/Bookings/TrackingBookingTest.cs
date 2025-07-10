using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingBooking))]
	sealed class TrackingBookingTest : NonPersistentBusinessObjectTestCase
	{
		[HttpContextEnabledTest]
		public void TestEventReference_SpotQuote_LocalClient()
		{
			var clientContact = CreateContactAndLogin();

			var spotQuote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			CreateJob(clientContact, spotQuote);

			var trackingBooking = new TrackingBooking(spotQuote, clientContact);

			AssertNull(trackingBooking.Booking);
			AssertNotNull(trackingBooking.QuotedBooking);
			AssertEquals(WebPartyType.LocalClient, trackingBooking.EventReference);
		}

		[HttpContextEnabledTest]
		public void TestEventReference_BookingWithQuote_LocalClient()
		{
			var clientContact = CreateContactAndLogin();

			var bookingWithQuote = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			bookingWithQuote.Booking.Consols.AddNew();
			CreateJob(clientContact, bookingWithQuote);

			var trackingBooking = new TrackingBooking(bookingWithQuote, clientContact);

			AssertNotNull(trackingBooking.QuotedBooking);
			AssertNotNull(trackingBooking.Booking);
			AssertEquals(WebPartyType.LocalClient, trackingBooking.EventReference);
		}

		OrgContact CreateContactAndLogin()
		{
			var clientContact = Factory.NewWithValidTestData<OrgContact>();
			clientContact.OC_Email = "test@test.com";
			var password = "12345";
			clientContact.SetHashedPassword(password);
			clientContact.OC_WebAccessEnabled = true;
			Factory.Save();
			var user = new TrackingSiteUser();
			user.Login(clientContact.OrganisationCode, clientContact.OC_Email, password);
			AssertEquals("Precondition: User is logged in", true, user.IsLoggedIn);

			return clientContact;
		}

		void CreateJob(OrgContact contact, QuotedBooking quotedBooking)
		{
			var client = contact.ParentOrg;
			client.OH_IsActive = true;
			client.MainAddress.OA_Address1 = "123 Main Street";
			client.MainAddress.OA_City = "Chicago";
			client.MainAddress.OA_PostCode = "60077";
			client.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
			var countryData = Factory.New<OrgCountryData>();
			countryData.OV_OA_ApprovedLocation = client.MainAddress.PK;
			countryData.OV_EXApprovedOrMajorExporter = "YES";
			countryData.OV_OH_OrgHeader = client.PK;

			var job = new JobHeader.Loader(quotedBooking).TryCreate();
			job.JH_GE = Factory.NewWithValidTestData<GlbDepartment>().PK;
			job.JH_OA_LocalChargesAddr = client.MainAddress.PK;
		}

		#region Milestones

		public void TestMilestones()
		{
			var testBooking = (TrackingBooking)GetNewBusinessObject();
			AssertNotNull(testBooking.Milestones);
			AssertEquals(0, testBooking.Milestones.Count);

			var milestone1 = testBooking.QuotedBooking.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "lastCompleted1";
			milestone1.P9_IsPublished = true;
			milestone1.SetMilestoneActualDateForTest(DateTime.Now);
			milestone1.P9_Sequence = 1;

			var milestone2 = testBooking.QuotedBooking.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Description = "lastCompleted2";
			milestone2.P9_IsPublished = true;
			milestone2.SetMilestoneActualDateForTest(DateTime.Now);
			milestone2.P9_Sequence = 1;
			testBooking.Factory.Save();
			AssertEquals(0, testBooking.Milestones.Count);

			testBooking.ReloadMilestones();
			AssertEquals(2, testBooking.Milestones.Count);
		}

		#endregion

		#region TestAviationSecurityInspectionHasNoErrors

		public void TestAviationSecurityInspectionHasNoErrors()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				string cachedShipmentInspectionTypeRegistry = FreightDataRegistry.Instance.ShipmentInspectionTypeDefault.Value;
				FreightDataRegistry.Instance.ShipmentInspectionTypeDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

				var client = Factory.NewWithValidTestData<OrgHeader>();
				client.OH_IsActive = true;
				client.MainAddress.OA_Address1 = "123 Main Street";
				client.MainAddress.OA_City = "Chicago";
				client.MainAddress.OA_PostCode = "60077";
				client.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
				var countryData = Factory.New<OrgCountryData>();
				countryData.OV_OA_ApprovedLocation = client.MainAddress.PK;
				countryData.OV_EXApprovedOrMajorExporter = "YES";
				countryData.OV_OH_OrgHeader = client.PK;
				var clientContact = client.Contacts.AddNew();
				clientContact.OC_Email = "test@test.com";
				var password = "12345";
				clientContact.SetHashedPassword(password);
				clientContact.OC_WebAccessEnabled = true;
				Factory.Save();
				TrackingSiteUser user = new TrackingSiteUser();
				user.Login(clientContact.OrganisationCode, clientContact.OC_Email, password);
				AssertEquals("Precondition: User is logged in", true, user.IsLoggedIn);

				var testBooking = (TrackingBooking)GetNewBusinessObject();
				testBooking.Booking.JS_TransportMode = Constants.TransportModes.Air;
				testBooking.ConsigneeDeliveryAddress.E2_Address1 = "1234";
				testBooking.ConsigneeDeliveryAddress.E2_CompanyName = "2323";
				testBooking.ConsigneeDeliveryAddress.E2_City = "Chicago";
				testBooking.ConsigneeDeliveryAddress.E2_RN_NKCountryCode = "US";
				testBooking.ConsigneeDeliveryAddress.E2_State = "IL";
				testBooking.ConsigneeDeliveryAddress.E2_Postcode = "60084";
				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				consignee.OH_IsConsignee = true;
				testBooking.ConsigneeOrganisationPK = consignee.PK;

				testBooking.ConsignorPickupAddress.E2_Address1 = "1234";
				testBooking.ConsignorPickupAddress.E2_CompanyName = "2323";
				testBooking.ConsignorPickupAddress.E2_City = "Los Angeles";
				testBooking.ConsignorPickupAddress.E2_RN_NKCountryCode = "US";
				testBooking.ConsignorPickupAddress.E2_State = "CA";
				testBooking.ConsignorPickupAddress.E2_Postcode = "90072";
				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_IsConsignor = true;
				testBooking.ConsignorOrganisationPK = consignor.PK;

				testBooking.Origin = "USORD";
				testBooking.Destination = "USLAX";

				testBooking.RunPreSaveValidation();
				Assert("Expected no errors on Booking", !testBooking.HasErrors());
				testBooking.Factory.Save();
				AssertEquals(BaseJobShipmentLookups.InspectionType_Web, testBooking.Booking.JS_InspectionTypeCode);
				Assert("Expected Booking saved in the database", testBooking.IsInDatabase);

				FreightDataRegistry.Instance.ShipmentInspectionTypeDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedShipmentInspectionTypeRegistry);
			}
		}

		#endregion

		#region AvailableOrders

		[HttpContextEnabledTest]
		public void TestAvailableOrders()
		{
			var helper = new TestHelper(Factory);
			var testBooking = (TrackingBooking)GetNewBusinessObject();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			testBooking.ConsigneeOrganisationPK = consignee.PK;
			testBooking.Booking.JS_TransportMode = Constants.RateMode.AIR;
			AssertEquals(0, testBooking.AvailableOrders.Count);

			var cachedRegistryValue = WebDataRegistry.Instance.BookingAttachOrdersWithoutSupplier.Value;
			WebDataRegistry.Instance.BookingAttachOrdersWithoutSupplier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var testOrder1 = Factory.NewWithValidTestData<Order>();
			testOrder1.SupplierPK = ZGuid.Empty;
			testOrder1.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			testOrder1.JD_TransportMode = Constants.RateMode.AIR;
			testOrder1.JD_ContainerMode = Constants.RateMode.LSE;

			var testOrder2 = Factory.NewWithValidTestData<Order>();
			testOrder2.SupplierPK = ZGuid.Empty;
			testOrder2.BuyerPK = consignee.PK;
			testOrder2.JD_TransportMode = Constants.RateMode.AIR;
			testOrder2.JD_ContainerMode = Constants.RateMode.LSE;

			var testOrder3 = Factory.NewWithValidTestData<Order>();
			testOrder3.SupplierPK = consignee.PK;
			testOrder3.BuyerPK = helper.TestSiteUser.LoggedInOrganisation.PK;
			testOrder3.JD_TransportMode = Constants.RateMode.AIR;
			testOrder3.JD_ContainerMode = Constants.RateMode.LSE;

			var testOrder4 = Factory.NewWithValidTestData<Order>();
			testOrder4.SupplierPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			testOrder4.BuyerPK = helper.TestSiteUser.LoggedInOrganisation.PK;
			testOrder4.JD_TransportMode = Constants.RateMode.AIR;
			testOrder4.JD_ContainerMode = Constants.RateMode.LSE;

			var testOrder5 = Factory.NewWithValidTestData<Order>();
			testOrder5.SupplierPK = ZGuid.Empty;
			testOrder5.BuyerPK = helper.TestSiteUser.LoggedInOrganisation.PK;
			testOrder5.JD_TransportMode = Constants.RateMode.AIR;
			testOrder5.JD_ContainerMode = Constants.RateMode.LSE;

			var testOrder6 = Factory.NewWithValidTestData<Order>();
			testOrder6.SupplierPK = helper.TestSiteUser.LoggedInOrganisation.PK;
			testOrder6.BuyerPK = consignee.PK;
			testOrder6.JD_TransportMode = Constants.RateMode.AIR;
			testOrder6.JD_ContainerMode = Constants.RateMode.LSE;

			var testOrder7 = Factory.NewWithValidTestData<Order>();
			testOrder7.SupplierPK = helper.TestSiteUser.LoggedInOrganisation.PK;
			testOrder7.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			testOrder7.JD_TransportMode = Constants.RateMode.AIR;
			testOrder7.JD_ContainerMode = Constants.RateMode.LSE;

			var testOrder8 = Factory.NewWithValidTestData<Order>();
			testOrder8.SupplierPK = helper.TestSiteUser.LoggedInOrganisation.PK;
			testOrder8.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			testOrder8.JD_TransportMode = Constants.RateMode.AIR;
			testOrder8.JD_ContainerMode = Constants.RateMode.LSE;
			testOrder8.JD_OrderStatus = Constants.OrderStatus.Cancelled;

			Factory.Save();

			AssertEquals(5, testBooking.AvailableOrders.Count);
			Assert(!testBooking.AvailableOrders.Contains(testOrder1));
			Assert(!testBooking.AvailableOrders.Contains(testOrder2));
			Assert(testBooking.AvailableOrders.Contains(testOrder3));
			Assert(testBooking.AvailableOrders.Contains(testOrder4));
			Assert(testBooking.AvailableOrders.Contains(testOrder5));
			Assert(testBooking.AvailableOrders.Contains(testOrder6));
			Assert(testBooking.AvailableOrders.Contains(testOrder7));
			Assert(!testBooking.AvailableOrders.Contains(testOrder8));

			WebDataRegistry.Instance.BookingAttachOrdersWithoutSupplier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(6, testBooking.AvailableOrders.Count);
			Assert(!testBooking.AvailableOrders.Contains(testOrder1));
			Assert(testBooking.AvailableOrders.Contains(testOrder2));
			Assert(testBooking.AvailableOrders.Contains(testOrder3));
			Assert(testBooking.AvailableOrders.Contains(testOrder4));
			Assert(testBooking.AvailableOrders.Contains(testOrder5));
			Assert(testBooking.AvailableOrders.Contains(testOrder6));
			Assert(testBooking.AvailableOrders.Contains(testOrder7));
			Assert(!testBooking.AvailableOrders.Contains(testOrder8));

			testOrder1.JD_TransportMode = Constants.RateMode.AIR;
			testOrder1.JD_ContainerMode = Constants.RateMode.LCL;

			testOrder2.JD_TransportMode = Constants.RateMode.SEA;
			testOrder2.JD_ContainerMode = Constants.RateMode.AIR;

			testOrder3.JD_TransportMode = Constants.RateMode.AIR;
			testOrder3.JD_ContainerMode = Constants.RateMode.LCL;

			testOrder4.JD_TransportMode = Constants.RateMode.SEA;
			testOrder4.JD_ContainerMode = Constants.RateMode.AIR;

			testOrder5.JD_TransportMode = Constants.RateMode.AIR;
			testOrder5.JD_ContainerMode = Constants.RateMode.LCL;

			testOrder6.JD_TransportMode = Constants.RateMode.SEA;
			testOrder6.JD_ContainerMode = Constants.RateMode.AIR;

			testOrder7.JD_TransportMode = Constants.RateMode.AIR;
			testOrder7.JD_ContainerMode = Constants.RateMode.LCL;

			testOrder8.JD_TransportMode = Constants.RateMode.AIR;
			testOrder8.JD_ContainerMode = Constants.RateMode.LCL;
			Factory.Save();

			AssertEquals(3, testBooking.AvailableOrders.Count);
			Assert(!testBooking.AvailableOrders.Contains(testOrder1));
			Assert(!testBooking.AvailableOrders.Contains(testOrder2));
			Assert(testBooking.AvailableOrders.Contains(testOrder3));
			Assert(!testBooking.AvailableOrders.Contains(testOrder4));
			Assert(testBooking.AvailableOrders.Contains(testOrder5));
			Assert(!testBooking.AvailableOrders.Contains(testOrder6));
			Assert(testBooking.AvailableOrders.Contains(testOrder7));
			Assert(!testBooking.AvailableOrders.Contains(testOrder8));

			WebDataRegistry.Instance.BookingAttachOrdersWithoutSupplier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedRegistryValue);
		}

		#endregion

		#region AttachedOrders

		[HttpContextEnabledTest]
		public void TestAttachedOrdersValidation()
		{
			TestHelper helper = new TestHelper(Factory);
			TrackingBooking testBooking = (TrackingBooking)GetNewBusinessObject();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			testBooking.ConsigneeOrganisationPK = consignee.PK;
			testBooking.Booking.JS_TransportMode = Constants.RateMode.AIR;
			AssertEquals(0, testBooking.AvailableOrders.Count);

			bool cachedRegistryValue = WebDataRegistry.Instance.BookingAttachOrdersWithoutSupplier.Value;
			WebDataRegistry.Instance.BookingAttachOrdersWithoutSupplier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			Order testOrder1 = Factory.NewWithValidTestData<Order>();
			testOrder1.SupplierPK = ZGuid.Empty;
			testOrder1.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			testOrder1.JD_TransportMode = Constants.RateMode.AIR;
			testOrder1.JD_ContainerMode = Constants.RateMode.LSE;

			Order testOrder2 = Factory.NewWithValidTestData<Order>();
			testOrder2.SupplierPK = ZGuid.Empty;
			testOrder2.BuyerPK = consignee.PK;
			testOrder2.JD_TransportMode = Constants.RateMode.AIR;
			testOrder2.JD_ContainerMode = Constants.RateMode.LSE;

			Order testOrder3 = Factory.NewWithValidTestData<Order>();
			testOrder3.SupplierPK = consignee.PK;
			testOrder3.BuyerPK = helper.TestSiteUser.LoggedInOrganisation.PK;
			testOrder3.JD_TransportMode = Constants.RateMode.AIR;
			testOrder3.JD_ContainerMode = Constants.RateMode.LSE;

			Order testOrder4 = Factory.NewWithValidTestData<Order>();
			testOrder4.SupplierPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			testOrder4.BuyerPK = helper.TestSiteUser.LoggedInOrganisation.PK;
			testOrder4.JD_TransportMode = Constants.RateMode.AIR;
			testOrder4.JD_ContainerMode = Constants.RateMode.LSE;

			Order testOrder5 = Factory.NewWithValidTestData<Order>();
			testOrder5.SupplierPK = ZGuid.Empty;
			testOrder5.BuyerPK = helper.TestSiteUser.LoggedInOrganisation.PK;
			testOrder5.JD_TransportMode = Constants.RateMode.AIR;
			testOrder5.JD_ContainerMode = Constants.RateMode.LSE;

			Order testOrder6 = Factory.NewWithValidTestData<Order>();
			testOrder6.SupplierPK = helper.TestSiteUser.LoggedInOrganisation.PK;
			testOrder6.BuyerPK = consignee.PK;
			testOrder6.JD_TransportMode = Constants.RateMode.AIR;
			testOrder6.JD_ContainerMode = Constants.RateMode.LSE;

			Order testOrder7 = Factory.NewWithValidTestData<Order>();
			testOrder7.SupplierPK = helper.TestSiteUser.LoggedInOrganisation.PK;
			testOrder7.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			testOrder7.JD_TransportMode = Constants.RateMode.AIR;
			testOrder7.JD_ContainerMode = Constants.RateMode.LSE;
			Factory.Save();

			TrackingBookingOrderLink link1 = new TrackingBookingOrderLink(testBooking, testOrder1.PK);
			TrackingBookingOrderLink link2 = new TrackingBookingOrderLink(testBooking, testOrder2.PK);
			TrackingBookingOrderLink link3 = new TrackingBookingOrderLink(testBooking, testOrder3.PK);
			TrackingBookingOrderLink link4 = new TrackingBookingOrderLink(testBooking, testOrder4.PK);
			TrackingBookingOrderLink link5 = new TrackingBookingOrderLink(testBooking, testOrder5.PK);
			TrackingBookingOrderLink link6 = new TrackingBookingOrderLink(testBooking, testOrder6.PK);
			TrackingBookingOrderLink link7 = new TrackingBookingOrderLink(testBooking, testOrder7.PK);

			testBooking.AttachedOrderLinks.Add(link1);
			testBooking.AttachedOrderLinks.Add(link2);
			testBooking.AttachedOrderLinks.Add(link3);
			testBooking.AttachedOrderLinks.Add(link4);
			testBooking.AttachedOrderLinks.Add(link5);
			testBooking.AttachedOrderLinks.Add(link6);
			testBooking.AttachedOrderLinks.Add(link7);

			AssertEquals(7, testBooking.AttachedOrderLinks.Count);
			AssertEquals(5, testBooking.AttachedOrders.Count);

			AssertHasError(link1.OrderPKInfo, "This Order is not available for this Booking.");
			AssertHasError(link2.OrderPKInfo, "This Order is not available for this Booking.");

			testBooking.AttachedOrderLinks.RemoveAll();
			AssertEquals(0, testBooking.AttachedOrderLinks.Count);
			AssertEquals(0, testBooking.AttachedOrders.Count);

			WebDataRegistry.Instance.BookingAttachOrdersWithoutSupplier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			testBooking.AttachedOrderLinks.Add(link1);
			testBooking.AttachedOrderLinks.Add(link2);
			testBooking.AttachedOrderLinks.Add(link3);
			testBooking.AttachedOrderLinks.Add(link4);
			testBooking.AttachedOrderLinks.Add(link5);
			testBooking.AttachedOrderLinks.Add(link6);
			testBooking.AttachedOrderLinks.Add(link7);

			AssertEquals(7, testBooking.AttachedOrderLinks.Count);
			AssertEquals(6, testBooking.AttachedOrders.Count);

			Assert(!testBooking.AttachedOrders.Contains(testOrder1));
			Assert(testBooking.AttachedOrders.Contains(testOrder2));
			Assert(testBooking.AttachedOrders.Contains(testOrder3));
			Assert(testBooking.AttachedOrders.Contains(testOrder4));
			Assert(testBooking.AttachedOrders.Contains(testOrder5));
			Assert(testBooking.AttachedOrders.Contains(testOrder6));
			Assert(testBooking.AttachedOrders.Contains(testOrder7));

			testBooking.Mode = Constants.RateMode.FCL;
			AssertHasError(link2.OrderPKInfo, "This Order cannot be chosen here as the Transport Mode is different to that of the Shipment.");
			AssertHasError(link3.OrderPKInfo, "This Order cannot be chosen here as the Transport Mode is different to that of the Shipment.");
			AssertHasError(link4.OrderPKInfo, "This Order cannot be chosen here as the Transport Mode is different to that of the Shipment.");
			AssertHasError(link5.OrderPKInfo, "This Order cannot be chosen here as the Transport Mode is different to that of the Shipment.");
			AssertHasError(link6.OrderPKInfo, "This Order cannot be chosen here as the Transport Mode is different to that of the Shipment.");
			AssertHasError(link7.OrderPKInfo, "This Order cannot be chosen here as the Transport Mode is different to that of the Shipment.");

			testOrder1.JD_ContainerMode = Constants.RateMode.AIR;
			testOrder2.JD_ContainerMode = Constants.RateMode.AIR;
			testOrder3.JD_ContainerMode = Constants.RateMode.AIR;
			testOrder4.JD_ContainerMode = Constants.RateMode.AIR;
			testOrder5.JD_ContainerMode = Constants.RateMode.AIR;
			testOrder6.JD_ContainerMode = Constants.RateMode.AIR;
			testOrder7.JD_ContainerMode = Constants.RateMode.AIR;
			testBooking.Mode = Constants.RateMode.LSE;

			AssertHasWarning(link2.OrderPKInfo, "The Container Mode of this Order is different to that of the Shipment.");
			AssertHasWarning(link3.OrderPKInfo, "The Container Mode of this Order is different to that of the Shipment.");
			AssertHasWarning(link4.OrderPKInfo, "The Container Mode of this Order is different to that of the Shipment.");
			AssertHasWarning(link5.OrderPKInfo, "The Container Mode of this Order is different to that of the Shipment.");
			AssertHasWarning(link6.OrderPKInfo, "The Container Mode of this Order is different to that of the Shipment.");
			AssertHasWarning(link7.OrderPKInfo, "The Container Mode of this Order is different to that of the Shipment.");

			WebDataRegistry.Instance.BookingAttachOrdersWithoutSupplier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedRegistryValue);
		}

		#endregion

		#region Order reference

		public void TestOrderItemsAsString()
		{
			TrackingBooking testBooking = (TrackingBooking)GetNewBusinessObject();
			AssertEquals(0, testBooking.AvailableOrders.Count);

			Order testOrder1 = Factory.NewWithValidTestData<Order>();
			testOrder1.JD_OrderNumber = "AO1";

			Order testOrder2 = Factory.NewWithValidTestData<Order>();
			testOrder2.JD_OrderNumber = "AO2";

			testBooking.AttachedOrders.Add(testOrder1);
			testBooking.AttachedOrders.Add(testOrder2);

			AssertEquals(2, testBooking.AttachedOrders.Count);
			AssertEquals("AO1, AO2", testBooking.OrderItemsAsString);
		}

		public void TestOrderItemsIncludeSplit()
		{
			var testBooking = (TrackingBooking)GetNewBusinessObject();

			var testOrder1 = Factory.NewWithValidTestData<Order>();
			testOrder1.JD_OrderNumber = "AO1";

			var testOrder2 = Factory.NewWithValidTestData<Order>();
			testOrder2.JD_OrderNumber = "AO1";
			testOrder2.JD_OrderNumberSplit = 1;

			testBooking.AttachedOrders.Add(testOrder1);
			testBooking.AttachedOrders.Add(testOrder2);

			AssertEquals(2, testBooking.AttachedOrders.Count);
			AssertEquals("AO1, AO1-1", testBooking.OrderItemsAsString);
		}

		#endregion

		#region ITemplateReversible Members

		public void TestReverse()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();

			TrackingBooking testBooking = (TrackingBooking)GetNewBusinessObject();
			testBooking.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			testBooking.ConsigneeDeliveryAddress.E2_CompanyName = "Test Company";

			testBooking.ConsignorPickupAddress.E2_AddressOverride = false;
			testBooking.ConsignorPickupAddress.OrganisationPK = testOrg.PK;

			testBooking.Origin = "USCHI";
			testBooking.Destination = "AUSYD";

			((ITemplateReversible)testBooking).Reverse();

			AssertEquals("Origin", "AUSYD", testBooking.Origin);
			AssertEquals("Destination", "USCHI", testBooking.Destination);

			AssertEquals("Consignee Address Override", false, testBooking.ConsigneeDeliveryAddress.E2_AddressOverride);
			AssertEquals("Consignee Organisation", testOrg.PK, testBooking.ConsigneeDeliveryAddress.OrganisationPK);

			AssertEquals("Consignor Address Override", true, testBooking.ConsignorPickupAddress.E2_AddressOverride);
			AssertEquals("Consignor Company Name", "Test Company", testBooking.ConsignorPickupAddress.E2_CompanyName);
		}

		#endregion

		#region ITemplateCopyable Members

		public void TestTemplateCopy()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();

			TrackingBooking testBooking = (TrackingBooking)GetNewBusinessObject();
			testBooking.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			testBooking.ConsigneeDeliveryAddress.E2_CompanyName = "Test Company";

			testBooking.ConsignorPickupAddress.E2_AddressOverride = false;
			testBooking.ConsignorPickupAddress.OrganisationPK = testOrg.PK;

			testBooking.Origin = "USCHI";
			testBooking.Destination = "AUSYD";

			TrackingBooking testBookingCopy = (TrackingBooking)((ITemplateCopyable)testBooking).TemplateCopy();

			AssertNotEquals(testBooking, testBookingCopy);
			AssertEquals("Origin", "USCHI", testBookingCopy.Origin);
			AssertEquals("Destination", "AUSYD", testBookingCopy.Destination);

			AssertEquals("Consignee Address Override", true, testBookingCopy.ConsigneeDeliveryAddress.E2_AddressOverride);
			AssertEquals("Consignee Company Name", "Test Company", testBookingCopy.ConsigneeDeliveryAddress.E2_CompanyName);

			AssertEquals("Consignor Address Override", false, testBookingCopy.ConsignorPickupAddress.E2_AddressOverride);
			AssertEquals("Consignor Organisation", testOrg.PK, testBookingCopy.ConsignorPickupAddress.OrganisationPK);
		}

		#endregion

		public void TestBookingPartyDocumentaryAddress()
		{
			var testBooking = (TrackingBooking)GetNewBusinessObject();
			AssertNotNull(testBooking);
			AssertNotNull(testBooking.Booking);

			AssertNotNull(testBooking.BookingPartyDocumentaryAddress);
			AssertNull(testBooking.BookingPartyDocumentaryAddress.Address);

			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testBooking.BookingPartyDocumentaryAddress.OrganisationPK = testOrg.PK;
			AssertNotNull(testBooking.BookingPartyDocumentaryAddress.Address);
			AssertEquals(testOrg.MainAddress, testBooking.BookingPartyDocumentaryAddress.Address);
		}

		public void TestChargeableWeightHasNoErrors()
		{
			TrackingBooking testBooking = (TrackingBooking)GetNewBusinessObject();
			testBooking.Booking.JS_UnitOfWeight = Constants.Weight.Kilograms;
			testBooking.Booking.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			testBooking.Booking.JS_ActualVolume = 100000m;
			AssertHasErrors(testBooking.Booking.JS_ActualChargeableInfo);

			testBooking.RunPreSaveValidation();
			AssertEquals(0m, testBooking.Booking.JS_ActualChargeable);
			AssertNoErrors(testBooking.Booking.JS_ActualChargeableInfo);

			testBooking.Booking.JS_ActualVolume = 0m;
			testBooking.Booking.JS_ActualVolume = 100000m;
			AssertHasErrors(testBooking.Booking.JS_ActualChargeableInfo);
			testBooking.Factory.Save();
			AssertEquals(0m, testBooking.Booking.JS_ActualChargeable);
			AssertNoErrors(testBooking.Booking.JS_ActualChargeableInfo);
		}

		public void TestIsDomesticFreightValueChanged()
		{
			TrackingBooking testBooking = (TrackingBooking)GetNewBusinessObject();
			testBooking.IsDomesticFreight = false;
			AssertEquals(false, testBooking.IsDomesticFreight);

			testBooking.IsDomesticFreightValueChanged += OnIsDomesticFreightChangedForTesting;

			OnIsDomesticFreightChangedForTestingHasBeenCalled = false;
			AssertEquals(false, OnIsDomesticFreightChangedForTestingHasBeenCalled);
			testBooking.IsDomesticFreight = true;
			AssertEquals(true, testBooking.IsDomesticFreight);
			AssertEquals(true, OnIsDomesticFreightChangedForTestingHasBeenCalled);

			OnIsDomesticFreightChangedForTestingHasBeenCalled = false;
			AssertEquals(false, OnIsDomesticFreightChangedForTestingHasBeenCalled);
			testBooking.Booking.IsDomesticFreight = false;
			AssertEquals(false, testBooking.IsDomesticFreight);
			AssertEquals(true, OnIsDomesticFreightChangedForTestingHasBeenCalled);

			OnIsDomesticFreightChangedForTestingHasBeenCalled = false;
			AssertEquals(false, OnIsDomesticFreightChangedForTestingHasBeenCalled);
			testBooking.IsDomesticFreight = false;
			AssertEquals(true, OnIsDomesticFreightChangedForTestingHasBeenCalled);

			OnIsDomesticFreightChangedForTestingHasBeenCalled = false;
			AssertEquals(false, OnIsDomesticFreightChangedForTestingHasBeenCalled);
			testBooking.Booking.IsDomesticFreight = false;
			AssertEquals(false, OnIsDomesticFreightChangedForTestingHasBeenCalled);
		}

		void OnIsDomesticFreightChangedForTesting(object sender, EventArgs e)
		{
			OnIsDomesticFreightChangedForTestingHasBeenCalled = true;
		}
		bool OnIsDomesticFreightChangedForTestingHasBeenCalled;

		#region MAWBNumber

		public void TestMAWBNumber()
		{
			TrackingBooking testBooking = (TrackingBooking)GetNewBusinessObject();
			testBooking.Booking.JS_IsDirectBooking = false;
			testBooking.Booking.JS_HouseBill = "";
			AssertEquals("", testBooking.MAWBNumber);

			testBooking.Booking.JS_HouseBill = "020323232";
			AssertEquals("", testBooking.MAWBNumber);

			testBooking.Booking.JS_IsDirectBooking = true;
			testBooking.Booking.JS_HouseBill = "020323232";
			AssertEquals("020323232", testBooking.MAWBNumber);

			testBooking.Booking.JS_HouseBill = "020323234";
			AssertEquals("020323234", testBooking.MAWBNumber);
		}

		public void TestMAWBNumberInfo()
		{
			TrackingBooking testBooking = (TrackingBooking)GetNewBusinessObject();
			AssertEquals("MAWBNumber", testBooking.MAWBNumberInfo.Name);
		}

		#endregion

		#region ConsigneeFullName

		public void TestConsigneeFullName()
		{
			TrackingBooking testBooking = (TrackingBooking)GetNewBusinessObject();
			AssertNull(testBooking.Booking.Consignee);
			AssertEquals("", testBooking.ConsigneeFullName);

			testBooking.Booking.ConsigneeNameOrPK = Factory.NewWithValidTestData<OrgHeader>().PK.ToString();
			testBooking.Booking.Consignee.OH_FullName = "TEST1";
			AssertEquals("TEST1", testBooking.ConsigneeFullName);

			testBooking.Booking.Consignee.OH_FullName = "TEST2";
			AssertEquals("TEST2", testBooking.ConsigneeFullName);
		}

		public void TestConsigneeFullNameInfo()
		{
			TrackingBooking testBooking = (TrackingBooking)GetNewBusinessObject();
			AssertEquals("ConsigneeFullName", testBooking.ConsigneeFullNameInfo.Name);
		}

		#endregion

		#region ThirdPartyAddress

		public void TestThirdPartyAddress()
		{
			TrackingBooking testBooking = (TrackingBooking)GetNewBusinessObject();
			int originalDocAdrCount = testBooking.Booking.DocAddresses.Count;
			AssertNull("should not have client spec billing party address originaly", testBooking.Booking.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty));

			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			testBooking.ThirdPartyAddressPK = header.MainAddress.PK;

			AssertEquals("assigning third party address should initiate new docaddress", originalDocAdrCount + 1, testBooking.Booking.DocAddresses.Count);
			var address = testBooking.Booking.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty);
			AssertNotNull("should create address of type ClientSpecifiedBillingParty", address);
			AssertEquals(address.E2_OA_Address, header.MainAddress.PK);
		}

		#endregion

		#region ConsignorFullName

		public void TestConsignorFullName()
		{
			TrackingBooking testBooking = (TrackingBooking)GetNewBusinessObject();
			AssertNull(testBooking.Booking.Consignor);
			AssertEquals("", testBooking.ConsignorFullName);

			testBooking.Booking.ConsignorNameOrPK = Factory.NewWithValidTestData<OrgHeader>().PK.ToString();
			testBooking.Booking.Consignor.OH_FullName = "TEST1";
			AssertEquals("TEST1", testBooking.ConsignorFullName);

			testBooking.Booking.Consignor.OH_FullName = "TEST2";
			AssertEquals("TEST2", testBooking.ConsignorFullName);
		}

		public void TestConsignorFullNameInfo()
		{
			TrackingBooking testBooking = (TrackingBooking)GetNewBusinessObject();
			AssertEquals("ConsignorFullName", testBooking.ConsignorFullNameInfo.Name);
		}

		#endregion

		#region DepotCutOff

		public void TestDepotCutOff()
		{
			TrackingBooking testBooking = (TrackingBooking)GetNewBusinessObject();
			AssertNull(testBooking.Booking.Sailing);
			AssertEquals(ZDateTime.Empty, testBooking.DepotCutOff);

			testBooking.Booking.JS_JX = Factory.NewWithValidTestData<JobSailing>().PK;
			testBooking.Booking.Sailing.JX_DepotCutOff = new ZDateTime(2009, 09, 22);
			AssertEquals(new ZDateTime(2009, 09, 22), testBooking.DepotCutOff);
		}

		public void TestDepotCutOffInfo()
		{
			TrackingBooking testBooking = (TrackingBooking)GetNewBusinessObject();
			AssertEquals("DepotCutOff", testBooking.DepotCutOffInfo.Name);
		}

		#endregion

		public void TestModeAndBillDefaulting()
		{
			TrackingBooking testBooking = new TrackingBooking(Factory, null);
			AssertEquals("WebScheduleChooserControl needs Mode to be not empty on new Bookings",
									 Constants.RateMode.LSE,
						 testBooking.QuotedBooking.Mode);
			AssertEquals("Bill of Lading printing needs HBL type to be defaulted on new Bookings",
						 WebDataRegistry.Instance.BookingDefaultHouseBillType.Value,
						 testBooking.Booking.JS_HouseBillOfLadingType);

			testBooking = new TrackingBooking(QuotedBooking.New(ZGuid.Empty, QuotedBooking.CreateNewBooking(Factory).PK, Factory), null);
			AssertEquals("WebScheduleChooserControl needs Mode to be not empty on new Bookings",
									 Constants.RateMode.LSE,
						 testBooking.QuotedBooking.Mode);
			AssertEquals("Bill of Lading printing needs HBL type to be defaulted on new Bookings",
						 WebDataRegistry.Instance.BookingDefaultHouseBillType.Value,
						 testBooking.Booking.JS_HouseBillOfLadingType);
		}

		#region TestDocAddressChangedEvent()

		public void TestDocAddressChangedEventForIsDomesticFreightEvent()
		{
			var user = CreateTestContactAndLogin();
			user.LoggedInOrganisation.OH_IsConsignor = true;
			user.LoggedInOrganisation.OH_IsConsignee = false;
			user.LoggedInOrganisation.Factory.Save();

			var booking = new TrackingBooking(Factory, user);
			booking.IsDomesticFreight = false;
			Assert(!booking.IsDomesticFreight);

			booking.Origin = "NZ";
			booking.Destination = "NZ";

			IsDomesticFreightValueChangedEventTriggered = false;
			booking.IsDomesticFreightValueChanged += new EventHandler(Test_OnIsDomesticFreightValueChangedEvent);
			booking.Origin = "AU";
			Assert(IsDomesticFreightValueChangedEventTriggered);

			IsDomesticFreightValueChangedEventTriggered = false;
			booking.Origin = "US";
			Assert(IsDomesticFreightValueChangedEventTriggered);

			IsDomesticFreightValueChangedEventTriggered = false;
			booking.Destination = "US";
			Assert(IsDomesticFreightValueChangedEventTriggered);

			IsDomesticFreightValueChangedEventTriggered = false;
			booking.Origin = "US";
			Assert(IsDomesticFreightValueChangedEventTriggered);

			IsDomesticFreightValueChangedEventTriggered = false;
			booking.Origin = "AU";
			Assert(IsDomesticFreightValueChangedEventTriggered);
		}

		void Test_OnIsDomesticFreightValueChangedEvent(object sender, EventArgs e)
		{
			IsDomesticFreightValueChangedEventTriggered = true;
		}

		bool IsDomesticFreightValueChangedEventTriggered;

		#endregion

		public void TestEnterpriseURL()
		{
			ZGuid bookingPK = QuotedBooking.CreateNewBooking(Factory).PK;
			TrackingBooking testBooking = new TrackingBooking(bookingPK, Factory, null);
			AssertEquals(bookingPK, ((IBizOChangesEmailNotification)testBooking).PK);
			AssertEquals(ControllerIDs.QuotedBookings, ((IBizOChangesEmailNotification)testBooking).ControllerForEnterpriseUrl);
		}

		public void TestServiceLevels()
		{
			bool initialValue = Globals.IsWeb;
			try
			{
				Globals.IsWeb = false;
				TrackingBooking testBooking = new TrackingBooking(QuotedBooking.CreateNewBooking(Factory).PK, Factory, null);
				AssertEquals(typeof(RefServiceLevelCollection).ToString(), testBooking.QuotedBooking.ServiceLevels.GetType().ToString());

				Globals.IsWeb = true;
				Assert(testBooking.QuotedBooking.ServiceLevels is WebServiceLevelCollection);

				AssertEquals(new ZQuery(), testBooking.GetTemporaryPublishedServiceLevelQuery());

				ZGuid temporaryLevelGuid = new ZGuid("F9163C5E-CEB2-4faa-B5BF-329BF39FA1E4");
				testBooking.TemporaryServiceLevelPK = temporaryLevelGuid;
				AssertEquals(new ZQuery(RefServiceLevelSchema.PK, temporaryLevelGuid).LiteralTextSqlFormatted, testBooking.GetTemporaryPublishedServiceLevelQuery().LiteralTextSqlFormatted);
			}
			finally
			{
				Globals.IsWeb = initialValue;
			}
		}

		public void TestConstructorWhenLoggedInContactIsNotShipperAndNotConsignee()
		{
			var user = CreateTestContactAndLogin();
			user.LoggedInOrganisation.OH_IsConsignor = false;
			user.LoggedInOrganisation.OH_IsConsignee = false;
			user.LoggedInOrganisation.Factory.Save();

			var booking = new TrackingBooking(Factory, user);
			AssertNotNull("booking.LoggedInContact", booking.LoggedInContact);
			AssertEquals("LoggedInContact", booking.LoggedInContact.PK, user.LoggedInUser.PK);

			AssertEquals("Default Pickup Address", booking.ConsignorPickupAddress.OrganisationPK, ZGuid.Empty);
			AssertEquals("Default Pickup Address Contact", booking.ConsignorPickupAddress.ContactPK, ZGuid.Empty);

			AssertEquals("Default Delivery Address", booking.ConsigneeDeliveryAddress.OrganisationPK, ZGuid.Empty);
			AssertEquals("Default Delivery Address Contact", booking.ConsigneeDeliveryAddress.ContactPK, ZGuid.Empty);

			CoreTestForAddressDefaultingOnTheWeb(user, booking);
		}

		public void TestConstructorWhenLoggedInContactIsShipper()
		{
			var user = CreateTestContactAndLogin();
			user.LoggedInOrganisation.OH_IsConsignor = true;
			user.LoggedInOrganisation.OH_IsConsignee = false;
			user.LoggedInOrganisation.Factory.Save();

			var booking = new TrackingBooking(Factory, user);
			AssertNotNull("booking.LoggedInContact", booking.LoggedInContact);
			AssertEquals("LoggedInContact", booking.LoggedInContact.PK, user.LoggedInUser.PK);

			AssertEquals("Default Pickup Address", booking.ConsignorPickupAddress.OrganisationPK, user.LoggedInOrganisation.PK);
			AssertEquals("Default Pickup Address Contact", booking.ConsignorPickupAddress.ContactPK, user.LoggedInUser.PK);

			AssertEquals("Default Delivery Address", booking.ConsigneeDeliveryAddress.OrganisationPK, ZGuid.Empty);
			AssertEquals("Default Delivery Address Contact", booking.ConsigneeDeliveryAddress.ContactPK, ZGuid.Empty);

			CoreTestForAddressDefaultingOnTheWeb(user, booking);
		}

		public void TestConstructorWhenLoggedInContactIsConsignee()
		{
			var user = CreateTestContactAndLogin();
			user.LoggedInOrganisation.OH_IsConsignor = false;
			user.LoggedInOrganisation.OH_IsConsignee = true;
			user.LoggedInOrganisation.Factory.Save();

			var booking = new TrackingBooking(Factory, user);
			AssertNotNull("booking.LoggedInContact", booking.LoggedInContact);
			AssertEquals("LoggedInContact", booking.LoggedInContact.PK, user.LoggedInUser.PK);

			AssertEquals("Default Pickup Address", booking.ConsignorPickupAddress.OrganisationPK, ZGuid.Empty);
			AssertEquals("Default Pickup Address Contact", booking.ConsignorPickupAddress.ContactPK, ZGuid.Empty);

			AssertEquals("Default Delivery Address", booking.ConsigneeDeliveryAddress.OrganisationPK, user.LoggedInOrganisation.PK);
			AssertEquals("Default Delivery Address Contact", booking.ConsigneeDeliveryAddress.ContactPK, user.LoggedInUser.PK);

			CoreTestForAddressDefaultingOnTheWeb(user, booking);
		}

		public void TestConstructorWhenLoggedInContactIsShipperAndConsignee()
		{
			var user = CreateTestContactAndLogin();
			user.LoggedInOrganisation.OH_IsConsignor = true;
			user.LoggedInOrganisation.OH_IsConsignee = true;
			user.LoggedInOrganisation.Factory.Save();

			var booking = new TrackingBooking(Factory, user);

			AssertNotNull("booking.LoggedInContact", booking.LoggedInContact);
			AssertEquals("LoggedInContact", user.LoggedInUser.PK, booking.LoggedInContact.PK);

			AssertEquals("Default Pickup Address", user.LoggedInOrganisation.PK, booking.ConsignorPickupAddress.OrganisationPK);
			AssertEquals("Default Pickup Address Contact", user.LoggedInUser.PK, booking.ConsignorPickupAddress.ContactPK);

			AssertEquals("Default Delivery Address", ZGuid.Empty, booking.ConsigneeDeliveryAddress.OrganisationPK);
			AssertEquals("Default Delivery Address Contact", ZGuid.Empty, booking.ConsigneeDeliveryAddress.ContactPK);
		}

		public void TestConstructorQuotedBookingAddressFallback()
		{
			var user = CreateTestContactAndLogin();
			var quotedBooking = QuotedBooking.CreateNewBooking(Factory);
			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			quotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = consigneeOrg.PK;
			var consignorOrg = Factory.NewWithValidTestData<OrgHeader>();
			quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignorOrg.PK;
			Factory.Save();

			var expectedConsigneeContactPK = quotedBooking.ConsigneeDocumentaryAddress.ContactPK;
			var expectedConsignorContactPK = quotedBooking.ConsignorDocumentaryAddress.ContactPK;

			var testBooking = new TrackingBooking(quotedBooking, Factory, user);

			AssertEquals("Default Delivery Address From Quoted Booking Org", consigneeOrg.PK, testBooking.ConsigneeDeliveryAddress.OrganisationPK);
			AssertEquals("Default Delivery Address From Quoted Booking", consigneeOrg.MainAddress.PK, testBooking.ConsigneeDeliveryAddress.E2_OA_Address);
			AssertEquals("Default Delivery Address From Quoted Booking Contact", expectedConsigneeContactPK, testBooking.ConsigneeDeliveryAddress.ContactPK);
			AssertEquals("Default Pickup Address From Quoted Booking Org", consignorOrg.PK, testBooking.ConsignorPickupAddress.OrganisationPK);
			AssertEquals("Default Pickup Address From Quoted Booking", consignorOrg.MainAddress.PK, testBooking.ConsignorPickupAddress.E2_OA_Address);
			AssertEquals("Default Pickup Address From Quoted Booking Contact", expectedConsignorContactPK, testBooking.ConsignorPickupAddress.ContactPK);

			CoreTestForAddressDefaultingOnTheWeb(user, testBooking);
		}

		public void TestConstructorWhenLoggedInContactIsShipper_QuotedBookingAddressFallback()
		{
			var user = CreateTestContactAndLogin();
			var quotedBooking = QuotedBooking.CreateNewBooking(Factory);
			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			quotedBooking.ConsigneeDocumentaryAddress.OrganisationPK = consigneeOrg.PK;
			user.LoggedInOrganisation.OH_IsConsignor = true;
			user.LoggedInOrganisation.OH_IsConsignee = false;
			user.LoggedInOrganisation.Factory.Save();
			Factory.Save();

			var expectedConsigneeContactPK = quotedBooking.ConsigneeDocumentaryAddress.ContactPK;

			var booking = new TrackingBooking(quotedBooking, Factory, user);

			AssertEquals("Default Pickup Address", user.LoggedInOrganisation.PK, booking.ConsignorPickupAddress.OrganisationPK);
			AssertEquals("Default Pickup Address Contact", user.LoggedInUser.PK, booking.ConsignorPickupAddress.ContactPK);
			AssertEquals("Default Delivery Address From Quoted Booking Org", consigneeOrg.PK, booking.ConsigneeDeliveryAddress.OrganisationPK);
			AssertEquals("Default Delivery Address From Quoted Booking", consigneeOrg.MainAddress.PK, booking.ConsigneeDeliveryAddress.E2_OA_Address);
			AssertEquals("Default Delivery Address From Quoted Booking Contact", expectedConsigneeContactPK, booking.ConsigneeDeliveryAddress.ContactPK);

			CoreTestForAddressDefaultingOnTheWeb(user, booking);
		}

		public void TestConstructorWhenLoggedInContactIsConsignee_QuotedBookingAddressFallback()
		{
			var user = CreateTestContactAndLogin();
			var quotedBooking = QuotedBooking.CreateNewBooking(Factory);
			var consignorOrg = Factory.NewWithValidTestData<OrgHeader>();
			quotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignorOrg.PK;
			user.LoggedInOrganisation.OH_IsConsignor = false;
			user.LoggedInOrganisation.OH_IsConsignee = true;
			user.LoggedInOrganisation.Factory.Save();
			Factory.Save();

			var expectedConsignorContactPK = quotedBooking.ConsignorDocumentaryAddress.ContactPK;
			var booking = new TrackingBooking(quotedBooking, Factory, user);

			AssertEquals("Default Pickup Address From Quoted Booking Org", consignorOrg.PK, booking.ConsignorPickupAddress.OrganisationPK);
			AssertEquals("Default Pickup Address From Quoted Booking", consignorOrg.MainAddress.PK, booking.ConsignorPickupAddress.E2_OA_Address);
			AssertEquals("Default Pickup Address From Quoted Booking Contact", expectedConsignorContactPK, booking.ConsignorPickupAddress.ContactPK);
			AssertEquals("Default Delivery Address", user.LoggedInOrganisation.PK, booking.ConsigneeDeliveryAddress.OrganisationPK);
			AssertEquals("Default Delivery Address Contact", user.LoggedInUser.PK, booking.ConsigneeDeliveryAddress.ContactPK);

			CoreTestForAddressDefaultingOnTheWeb(user, booking);
		}

		#region TestConstructorWhenWebUserDefaultSettingsForDocAddressExist

		public void TestConstructorWhenWebUserDefaultSettingsForDocAddressExist()
		{
			TrackingSiteUser user = CreateTestContactAndLogin();
			user.LoggedInOrganisation.OH_IsConsignor = true;
			user.LoggedInOrganisation.OH_IsConsignee = true;
			user.LoggedInOrganisation.Factory.Save();

			WebUserDefaultSettingsForDocAddress settings = new WebUserDefaultSettingsForDocAddress(user.LoggedInOrganisation.PK, user.LoggedInOrganisation.Addresses[0].PK, user.LoggedInOrganisation.Contacts[0].PK, nameof(DocAddressType.ConsigneePickupDeliveryAddress));
			Env.Registry.SetWebUserDefaultSettingsForDocAddress(user.LoggedInUser.PK.ToGuid(), settings);

			TrackingBooking booking = new TrackingBooking(Factory, user);
			AssertTestConstructorWhenWebUserDefaultSettingsForDocAddressExist(user, booking.ConsigneeDeliveryAddress, booking.ConsignorPickupAddress);

			settings.DocAddressType = nameof(DocAddressType.ConsignorDocumentaryAddress);
			Env.Registry.SetWebUserDefaultSettingsForDocAddress(user.LoggedInUser.PK.ToGuid(), settings);

			booking = new TrackingBooking(Factory, user);
			AssertTestConstructorWhenWebUserDefaultSettingsForDocAddressExist(user, booking.ConsignorPickupAddress, booking.ConsigneeDeliveryAddress);
		}

		void AssertTestConstructorWhenWebUserDefaultSettingsForDocAddressExist(TrackingSiteUser user, JobDocAddress defaultedJobDocAddress, JobDocAddress emptyJobDocAddress)
		{
			AssertEquals(emptyJobDocAddress.OrganisationPK, ZGuid.Empty);

			AssertEquals("Default Org", defaultedJobDocAddress.OrganisationPK, user.LoggedInOrganisation.PK);
			AssertEquals("Default Address", defaultedJobDocAddress.E2_OA_Address, user.LoggedInOrganisation.Addresses[0].PK);
			AssertEquals("Default Contact", defaultedJobDocAddress.ContactPK, user.LoggedInUser.PK);
		}

		#endregion

		TrackingSiteUser CreateTestContactAndLogin()
		{
			OrgContact userContact = Factory.NewWithValidTestData<OrgContact>();
			userContact.OC_Email = "testOrderItemsAsStringcargowise.com";
			userContact.OC_WebAccessEnabled = true;
			userContact.SetHashedPassword("test");
			Factory.Save();

			TrackingSiteUser user = new TrackingSiteUser();
			user.Login(userContact.OrganisationCode, "testOrderItemsAsStringcargowise.com", "test");
			AssertEquals("Precondition: User is logged in", true, user.IsLoggedIn);
			return user;
		}

		void CoreTestForAddressDefaultingOnTheWeb(TrackingSiteUser user, TrackingBooking booking)
		{
			AssertJobDocAddressesAreEqual(booking.ConsignorPickupAddress, booking.Booking.ConsignorDocumentaryAddress);
			AssertJobDocAddressesAreEqual(booking.ConsigneeDeliveryAddress, booking.Booking.ConsigneeDocumentaryAddress);

			booking.ConsignorPickupAddress.E2_AddressOverride = true;
			AssertJobDocAddressesAreEqual(booking.ConsignorPickupAddress, booking.Booking.ConsignorDocumentaryAddress);
			AssertJobDocAddressesAreEqual(booking.ConsigneeDeliveryAddress, booking.Booking.ConsigneeDocumentaryAddress);

			booking.ConsignorPickupAddress.OrganisationNameOrPK = "Overridden Shipper Name";
			booking.ConsignorPickupAddress.E2_Address1 = "Pickup Address Line 1";
			booking.ConsignorPickupAddress.E2_Address2 = "Pickup Address Line 2";
			booking.ConsignorPickupAddress.E2_Postcode = "2000";
			booking.ConsignorPickupAddress.E2_City = "Sydney";
			booking.ConsignorPickupAddress.E2_State = "NSW";
			booking.ConsignorPickupAddress.E2_RN_NKCountryCode = "AU";
			AssertJobDocAddressesAreEqual(booking.ConsignorPickupAddress, booking.Booking.ConsignorDocumentaryAddress);
			AssertJobDocAddressesAreEqual(booking.ConsigneeDeliveryAddress, booking.Booking.ConsigneeDocumentaryAddress);

			booking.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			AssertJobDocAddressesAreEqual(booking.ConsignorPickupAddress, booking.Booking.ConsignorDocumentaryAddress);
			AssertJobDocAddressesAreEqual(booking.ConsigneeDeliveryAddress, booking.Booking.ConsigneeDocumentaryAddress);

			booking.ConsigneeDeliveryAddress.OrganisationNameOrPK = "Overridden Consignee Name";
			booking.ConsigneeDeliveryAddress.E2_Address1 = "Delivery Address Line 1";
			booking.ConsigneeDeliveryAddress.E2_Address2 = "Delivery Address Line 2";
			booking.ConsigneeDeliveryAddress.E2_Postcode = "2000";
			booking.ConsigneeDeliveryAddress.E2_City = "Melbourne";
			booking.ConsigneeDeliveryAddress.E2_State = "VIC";
			booking.ConsigneeDeliveryAddress.E2_RN_NKCountryCode = "AU";

			AssertJobDocAddressesAreEqual(booking.ConsignorPickupAddress, booking.Booking.ConsignorDocumentaryAddress);
			AssertJobDocAddressesAreEqual(booking.ConsigneeDeliveryAddress, booking.Booking.ConsigneeDocumentaryAddress);
		}

		void AssertJobDocAddressesAreEqual(JobDocAddress editableAddress, JobDocAddress linkedAddress)
		{
			AssertNotEquals("PK", editableAddress.PK, linkedAddress.PK);
			AssertEquals("E2_AddressOverride", editableAddress.E2_AddressOverride, linkedAddress.E2_AddressOverride);

			AssertEquals("OrganisationPK", editableAddress.OrganisationPK, linkedAddress.OrganisationPK);
			AssertEquals("OrganisationNameOrPK", editableAddress.OrganisationNameOrPK, linkedAddress.OrganisationNameOrPK);
			AssertEquals("E2_OA_Address", editableAddress.E2_OA_Address, linkedAddress.E2_OA_Address);
			AssertEquals("E2_CompanyName", editableAddress.E2_CompanyName, linkedAddress.E2_CompanyName);
			AssertEquals("E2_Address1", editableAddress.E2_Address1, linkedAddress.E2_Address1);
			AssertEquals("E2_Address2", editableAddress.E2_Address2, linkedAddress.E2_Address2);
			AssertEquals("E2_Postcode", editableAddress.E2_Postcode, linkedAddress.E2_Postcode);
			AssertEquals("E2_City", editableAddress.E2_City, linkedAddress.E2_City);
			AssertEquals("E2_State", editableAddress.E2_State, linkedAddress.E2_State);
			AssertEquals("E2_RN_NKCountryCode", editableAddress.E2_RN_NKCountryCode, linkedAddress.E2_RN_NKCountryCode);
		}

		#region TestNotificationOptions

		public void TestNotificationOptions()
		{
			AssertEquals(WebDataRegistry.Instance.BookingNotificationOptions, ((IBizOChangesEmailNotification)GetNewBusinessObject()).NotificationSendingRule);
		}

		#endregion

		#region TestNotificationStaffRole

		public void TestNotificationStaffRole()
		{
			var testBooking = (TrackingBooking)GetNewBusinessObject();

			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.OH_RL_NKClosestPort = "HKHKG";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = testBooking.LoggedInOrganisation.OH_RL_NKClosestPort;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var staffAssign1 = shipper.StaffAssignments.AddNew();
			staffAssign1.O8_Department = "FIS";
			staffAssign1.O8_GS_NKPersonResponsible = staff1.GS_Code;
			staffAssign1.O8_Role = "MAN";

			var staffAssign2 = consignee.StaffAssignments.AddNew();
			staffAssign2.O8_Department = "FES";
			staffAssign2.O8_GS_NKPersonResponsible = staff2.GS_Code;
			staffAssign2.O8_Role = "MAN";

			testBooking.Booking.JS_RL_NKOrigin = "HKHKG";
			testBooking.Booking.JS_RL_NKDestination = testBooking.LoggedInOrganisation.OH_RL_NKClosestPort;
			testBooking.Booking.JS_TransportMode = Constants.TransportModes.Sea;

			var staffGuid = ((IBizOChangesEmailNotification)testBooking).GetStaffGuid(shipper.StaffAssignments, new CodeDescriptionBool() { Bool = true, Code = "MAN" });
			AssertEquals("Should be same as staff1", staff1.PK, staffGuid);

			staffGuid = ((IBizOChangesEmailNotification)testBooking).GetStaffGuid(consignee.StaffAssignments, new CodeDescriptionBool() { Bool = true, Code = "MAN" });
			AssertEquals("Should be empty", ZGuid.Empty, staffGuid);

			testBooking.Booking.JS_RL_NKOrigin = testBooking.LoggedInOrganisation.OH_RL_NKClosestPort;
			testBooking.Booking.JS_RL_NKDestination = "HKHKG";

			staffGuid = ((IBizOChangesEmailNotification)testBooking).GetStaffGuid(consignee.StaffAssignments, new CodeDescriptionBool() { Bool = true, Code = "MAN" });
			AssertEquals("Should be same as staff2", staff2.PK, staffGuid);

			staffGuid = ((IBizOChangesEmailNotification)testBooking).GetStaffGuid(shipper.StaffAssignments, new CodeDescriptionBool() { Bool = true, Code = "MAN" });
			AssertEquals("Should be empty", ZGuid.Empty, staffGuid);
		}

		#endregion

		#region TestRelatedOrg

		public void TestRelatedOrg()
		{
			TrackingBooking testBizO = (TrackingBooking)GetNewBusinessObject();
			IBizOChangesEmailNotification testBizOWithNotifier = testBizO;

			AssertNull(testBizOWithNotifier.RelatedOrg);

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			testBizO.Booking.ConsignorDocumentaryAddress.OrganisationPK = org1.PK;
			AssertNull(testBizOWithNotifier.RelatedOrg);

			testBizO.Booking.ConsignorPickupAddress.OrganisationPK = org2.PK;
			AssertEquals(org2, testBizOWithNotifier.RelatedOrg);
		}

		#endregion

		#region TestPropertiesForEmailReporting

		public void TestPropertiesForEmailReporting()
		{
			TrackingBooking testBizO = (TrackingBooking)GetNewBusinessObject();
			testBizO.Booking.JS_PackingMode = Constants.ContainerModes.FCL;

			var notifier = new BusinessObjectChangesEmailNotifier(testBizO);

			var propertyInfos = testBizO.GetPropertiesForEmailReporting();
			AssertEquals(31, propertyInfos.Length);
			AssertEquals(1, propertyInfos.Where(x => x.HumanReadableName.ToString().Equals("Carrier")).Count());

			var rc = Factory.LoadTop1<RefContainer>(new ZQuery());
			var container = testBizO.Containers.AddNew();
			container.JC_ContainerNum = "ABC123";
			container.JC_RC = rc.PK;
			container.JC_ContainerCount = 1;

			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_FullName = "testOrg";
			testBizO.Booking.JS_OA_BookedShippingLineAddress = testOrg.MainAddress.PK;

			testBizO.Factory.Save();
			propertyInfos = testBizO.GetPropertiesForEmailReporting();
			AssertEquals(32, propertyInfos.Length);
			AssertEquals(1, propertyInfos.Where(x => x.HumanReadableName.ToString().Equals("Container 1")).Count());
			AssertEquals(testOrg.OH_FullName, propertyInfos.Where(x => x.HumanReadableName.ToString().Equals("Carrier")).First().UpdatedValue);
		}

		#endregion

		#region TestEventBranch

		public void TestEventBranch()
		{
			TrackingBooking testBizO = (TrackingBooking)GetNewBusinessObject();
			IBizOChangesEmailNotification testBizOWithNotifier = testBizO;

			GlbCompany company1 = Factory.NewWithValidTestData<GlbCompany>();

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_RL_NKClosestPort = "AUMEL";
			testBizO.Booking.ConsignorPickupAddress.OrganisationPK = consignor.PK;

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = "USCHI";
			testBizO.Booking.ConsigneeDeliveryAddress.OrganisationPK = consignee.PK;

			AssertNull("No controlling branch and no consignor's closest port -- should return null", testBizOWithNotifier.EventBranch);

			GlbBranch branch1 = CreateBranch(consignor, false, "AUMEL");
			Factory.Save();
			AssertEquals("No controlling branch so it searches by consignor's closest port", branch1, testBizOWithNotifier.EventBranch);

			GlbBranch branch3 = CreateBranch(consignor, true, "USPTQ");
			Factory.Save();
			AssertEquals("It searches by controlling branch", branch3, testBizOWithNotifier.EventBranch);

			testBizO.Booking.ConsignorPickupAddress.E2_AddressOverride = true;
			AssertEquals("It searches by loggedInUser's org related branch", "BNE", testBizOWithNotifier.EventBranch.GB_Code);
		}

		GlbBranch CreateBranch(OrgHeader org, bool setControllingBranch, string homePort)
		{
			OrgCompanyData orgCompanyData = org.CompanyDataCollection.AddNew();

			GlbCompany glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_IsActive = true;

			GlbBranch glbBranch = Factory.NewWithValidTestData<GlbBranch>();
			glbBranch.GB_GC = glbCompany.PK;
			glbBranch.GB_IsActive = true;
			glbBranch.GB_RL_NKHomePort = homePort;

			orgCompanyData.OB_GC = glbCompany.PK;

			if (setControllingBranch)
			{
				orgCompanyData.OB_GB_ControllingBranch = glbBranch.PK;
			}

			return glbBranch;
		}

		#endregion

		public void TestDocumnetSupporterAndFreightLabelsDocumentWrapper()
		{
			TrackingBooking testBooking = (TrackingBooking)GetNewBusinessObject();
			testBooking.OuterPacks = 3;

			AssertNotNull("Is WebLCLShipmentDocumentSupporter", testBooking.DocumentSupporter as TrackingBooking.TrackingBookingDocumentSupporter);

			DocumentWrapper[] wrappers = testBooking.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.FreightLabels, StmMenuItem.GetForTesting(DocumentDirection.DEP));
			AssertNotNull("Wrappers", wrappers);
			AssertEquals("Should be one wrapper", 1, wrappers.Length);

			CommonShipment wrappedBooking = wrappers[0].WrappedObject as CommonShipment;
			AssertNotNull("Should wrap FreightShipment", wrappedBooking);
			AssertEquals("Should be TestBooking", testBooking.Booking.PK, wrappedBooking.PK);

			DocShipment docShipment = wrappers[0] as DocShipment;
			AssertNotNull("Should be DocShipment", docShipment);
			AssertEquals("Number of labels to print", testBooking.OuterPacks, docShipment.NumberOfLabelsToPrint);
			AssertEquals("Contains Consignee by default", true, docShipment.IncludeConsignee);
		}

		public void TestVoyageFlightWithSuppression()
		{
			TrackingBooking testBooking = (TrackingBooking)GetNewBusinessObject();

			testBooking.Booking.JS_TransportMode = Constants.TransportModes.Sea;
			testBooking.Booking.JS_E_DEP = DateTime.Now.AddDays(1);

			JobSailing sailing = Factory.New<JobSailing>();
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			VoyageDestination destination = Factory.New<VoyageDestination>();

			voyage.JV_VoyageFlight = "123";
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			origin.JA_JV = voyage.PK;
			destination.JB_JV = voyage.PK;
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			testBooking.Booking.JS_JX = sailing.PK;

			AssertEquals("Should not be suppressed on sea shipments.", "123", testBooking.VoyageFlightWithSuppression);

			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			testBooking.Booking.JS_TransportMode = Constants.TransportModes.Air;
			testBooking.Booking.JS_JX = sailing.PK;

			AssertEquals("Should not be suppressed on air shipments by default.", "123", testBooking.VoyageFlightWithSuppression);

			SuppressionForTest.CacheObjectClear();
			SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForDomestic, true);

			AssertEquals("Should be set to suppressed.", "*SUPPRESSED*", testBooking.VoyageFlightWithSuppression);
		}

		public void TestVessel()
		{
			TrackingBooking testBooking = (TrackingBooking)GetNewBusinessObject();

			testBooking.Booking.JS_TransportMode = Constants.TransportModes.Sea;
			testBooking.Booking.JS_E_DEP = DateTime.Now.AddDays(1);

			JobSailing sailing = Factory.New<JobSailing>();
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			VoyageDestination destination = Factory.New<VoyageDestination>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "ORIENTAL PHOENIX";

			voyage.JV_VoyageFlight = "123";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			origin.JA_JV = voyage.PK;
			destination.JB_JV = voyage.PK;
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			testBooking.Booking.JS_JX = sailing.PK;

			AssertEquals("ORIENTAL PHOENIX", testBooking.Vessel);
		}

		public void TestETDWithSuppression()
		{
			ZDateTime testDate = DateTime.Now.AddDays(1);

			TrackingBooking testBooking = (TrackingBooking)GetNewBusinessObject();

			testBooking.Booking.JS_TransportMode = Constants.TransportModes.Sea;
			testBooking.Booking.JS_E_DEP = testDate;

			JobSailing sailing = Factory.New<JobSailing>();
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			VoyageDestination destination = Factory.New<VoyageDestination>();

			voyage.JV_VoyageFlight = "123";
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			origin.JA_JV = voyage.PK;
			destination.JB_JV = voyage.PK;
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			testBooking.Booking.JS_JX = sailing.PK;

			AssertEquals("Should not be suppressed on sea shipments.", testDate, testBooking.ETDWithSuppression);

			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			testBooking.Booking.JS_TransportMode = Constants.TransportModes.Air;
			testBooking.Booking.JS_JX = sailing.PK;

			AssertEquals("Should be unsuppressed by default on air shipments.", testDate, testBooking.ETDWithSuppression);

			SuppressionForTest.CacheObjectClear();
			SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForDomestic, true);

			AssertEquals("Should be suppressed", Suppression.SuppressedDate, testBooking.ETDWithSuppression);
		}

		public void TestETAWithSuppression()
		{
			ZDateTime testDate = DateTime.Now.AddDays(1);

			TrackingBooking testBooking = (TrackingBooking)GetNewBusinessObject();

			testBooking.Booking.JS_TransportMode = Constants.TransportModes.Sea;
			testBooking.Booking.JS_E_DEP = ZDateTime.Today;
			testBooking.Booking.JS_E_ARV = testDate;

			JobSailing sailing = Factory.New<JobSailing>();
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			VoyageDestination destination = Factory.New<VoyageDestination>();

			voyage.JV_VoyageFlight = "123";
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			origin.JA_JV = voyage.PK;
			destination.JB_JV = voyage.PK;
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			testBooking.Booking.JS_JX = sailing.PK;

			AssertEquals(testDate, testBooking.ETAWithSuppression);

			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			testBooking.Booking.JS_TransportMode = Constants.TransportModes.Air;
			testBooking.Booking.JS_JX = sailing.PK;

			AssertEquals("Should should be unsuppressed by default on air shipments where the ETD has not passed.", testDate, testBooking.ETAWithSuppression);

			SuppressionForTest.CacheObjectClear();
			SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForDomestic, true);

			AssertEquals("Should be suppressed", Suppression.SuppressedDate, testBooking.ETAWithSuppression);
		}

		public void TestHouseBillNumberCustomisation()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch branchA = Factory.NewWithValidTestData<GlbBranch>();
			branchA.GB_GC = company.PK;
			GlbBranch branchB = Factory.NewWithValidTestData<GlbBranch>();
			branchB.GB_GC = company.PK;
			Factory.Save();

			BillCustomisationByServiceLevelRegistryItem registryItem = FreightDataRegistry.Instance.HouseBillNumberCustomisation;
			Guid departmentPK = AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingDomesticRoad.Value.GetDefaultDepartment().ToGuid();

			BillOfLadingNumberCustomisationsByServiceLevel settingsByServiceLevelA = new BillOfLadingNumberCustomisationsByServiceLevel();
			BillOfLadingNumberCustomisation settingsA = settingsByServiceLevelA.BillOfLadingNumberCustomisations["ALL"];
			settingsA.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.Standard;
			settingsA.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Include = true;
			settingsA.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Order = 1;
			registryItem.SetValue(Guid.Empty, branchA.PK.ToGuid(), departmentPK, settingsByServiceLevelA);

			BillOfLadingNumberCustomisationsByServiceLevel settingsByServiceLevelB = new BillOfLadingNumberCustomisationsByServiceLevel();
			BillOfLadingNumberCustomisation settingsB = settingsByServiceLevelB.BillOfLadingNumberCustomisations["ALL"];
			settingsB.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.None;
			settingsB.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Include = true;
			settingsB.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Order = 2;
			registryItem.SetValue(Guid.Empty, branchB.PK.ToGuid(), departmentPK, settingsByServiceLevelB);

			OrgHeader orgA = Factory.NewWithValidTestData<OrgHeader>();
			orgA.CompanyData.OB_GB_ControllingBranch = branchA.PK;

			OrgHeader orgB = Factory.NewWithValidTestData<OrgHeader>();
			orgB.CompanyData.OB_GB_ControllingBranch = branchB.PK;

			ForwardingShipment booking = Factory.New<ForwardingShipment>();
			booking.JS_IsBooking = true;

			TrackingBookingWithMethodsExposedForTest testBooking = new TrackingBookingWithMethodsExposedForTest(booking.PK, Factory);
			testBooking.Origin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			testBooking.Destination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			testBooking.Booking.JS_TransportMode = Constants.TransportModes.Road;

			AssertEquals("Precondition: Test data should be set up to default Department to Forwarding Domestic Road",
				departmentPK, DepartmentChooser.New(Factory).GetDepartment(booking));

			testBooking.Booking.ConsignorDocumentaryAddress.OrganisationPK = orgA.PK;
			NumberGeneratorTarget target = new BillOfLadingNumberGeneratorTarget();
			target.Context = testBooking.GetBillOfLadingCustomisationExposedForTest();
			BillOfLadingNumberCustomisation resultForOrgA = target.NumberCustomisation;

			testBooking.Booking.ConsignorDocumentaryAddress.OrganisationPK = orgB.PK;
			target = new BillOfLadingNumberGeneratorTarget();
			target.Context = testBooking.GetBillOfLadingCustomisationExposedForTest();
			BillOfLadingNumberCustomisation resultForOrgB = target.NumberCustomisation;

			AssertEquals("Settings (A): CheckDigitAlgorithm", settingsA.CheckDigitAlgorithm, resultForOrgA.CheckDigitAlgorithm);
			AssertEquals("Settings (A): CompanyCode", settingsA.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Order,
				resultForOrgA.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Order);

			AssertEquals("Settings (B): CheckDigitAlgorithm", settingsB.CheckDigitAlgorithm, resultForOrgB.CheckDigitAlgorithm);
			AssertEquals("Settings (B): CompanyCode", settingsB.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Order,
				resultForOrgB.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Order);
		}

		public void TestHouseBillNumberCustomisationForDomesticFreightWithNoPorts()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch branchA = Factory.NewWithValidTestData<GlbBranch>();
			branchA.GB_GC = company.PK;
			GlbBranch branchB = Factory.NewWithValidTestData<GlbBranch>();
			branchB.GB_GC = company.PK;
			Factory.Save();

			BillCustomisationByServiceLevelRegistryItem registryItem = FreightDataRegistry.Instance.HouseBillNumberCustomisation;
			Guid departmentPK = AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingDomesticRoad.Value.GetDefaultDepartment().ToGuid();

			BillOfLadingNumberCustomisationsByServiceLevel settingsByServiceLevelA = new BillOfLadingNumberCustomisationsByServiceLevel();
			BillOfLadingNumberCustomisation settingsA = settingsByServiceLevelA.BillOfLadingNumberCustomisations["ALL"];
			settingsA.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.Standard;
			settingsA.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Include = true;
			settingsA.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Order = 1;
			registryItem.SetValue(Guid.Empty, branchA.PK.ToGuid(), departmentPK, settingsByServiceLevelA);

			BillOfLadingNumberCustomisationsByServiceLevel settingsByServiceLevelB = new BillOfLadingNumberCustomisationsByServiceLevel();
			BillOfLadingNumberCustomisation settingsB = settingsByServiceLevelB.BillOfLadingNumberCustomisations["ALL"];
			settingsB.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.None;
			settingsB.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Include = true;
			settingsB.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Order = 2;
			registryItem.SetValue(Guid.Empty, branchB.PK.ToGuid(), departmentPK, settingsByServiceLevelB);

			OrgHeader orgA = Factory.NewWithValidTestData<OrgHeader>();
			orgA.CompanyData.OB_GB_ControllingBranch = branchA.PK;

			OrgHeader orgB = Factory.NewWithValidTestData<OrgHeader>();
			orgB.CompanyData.OB_GB_ControllingBranch = branchB.PK;

			ForwardingShipment booking = Factory.New<ForwardingShipment>();
			booking.JS_IsBooking = true;

			TrackingBookingWithMethodsExposedForTest testBooking = new TrackingBookingWithMethodsExposedForTest(booking.PK, Factory);
			testBooking.Origin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			testBooking.Destination = ZString.Empty; // If you override the Job Doc Address, we can't default a Destination Port
			testBooking.Booking.JS_TransportMode = Constants.TransportModes.Road;
			testBooking.IsDomesticFreight = true;

			AssertEquals("Precondition: Test data should be set up to default Department to *nothing* (because there is no destination port)",
				ZGuid.Empty, DepartmentChooser.New(Factory).GetDepartment(booking));

			testBooking.Booking.ConsignorDocumentaryAddress.OrganisationPK = orgA.PK;
			NumberGeneratorTarget target = new BillOfLadingNumberGeneratorTarget();
			target.Context = testBooking.GetBillOfLadingCustomisationExposedForTest();
			BillOfLadingNumberCustomisation resultForOrgA = target.NumberCustomisation;

			testBooking.Booking.ConsignorDocumentaryAddress.OrganisationPK = orgB.PK;
			target = new BillOfLadingNumberGeneratorTarget();
			target.Context = testBooking.GetBillOfLadingCustomisationExposedForTest();
			BillOfLadingNumberCustomisation resultForOrgB = target.NumberCustomisation;

			AssertEquals("Settings (A): CheckDigitAlgorithm", settingsA.CheckDigitAlgorithm, resultForOrgA.CheckDigitAlgorithm);
			AssertEquals("Settings (A): CompanyCode", settingsA.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Order,
				resultForOrgA.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Order);

			AssertEquals("Settings (B): CheckDigitAlgorithm", settingsB.CheckDigitAlgorithm, resultForOrgB.CheckDigitAlgorithm);
			AssertEquals("Settings (B): CompanyCode", settingsB.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Order,
				resultForOrgB.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Order);
		}

		public void TestGetBillOfLadingCustomisationWhenCreateTrackBooking()
		{
			CombineAssertions("No exception thrown when calling GetBillOfLadingCustomisation", () =>
			{
				AssertTrackingBookingWithMethodsExposed(false, false);
				AssertTrackingBookingWithMethodsExposed(false, true);
				AssertTrackingBookingWithMethodsExposed(true, false);
				AssertTrackingBookingWithMethodsExposed(true, true);
			});

			void AssertTrackingBookingWithMethodsExposed(bool customDepartmentDefaultingRuleEngineConfiguration, bool isDomesticFreight)
			{
				var trackingBooking = QuotedBooking.CreateNewBooking(Factory);

				using (AccountingConfigurationRegistry.Instance.CustomDepartmentDefaultingRuleEngineConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customDepartmentDefaultingRuleEngineConfiguration))
				{
					TrackingBookingWithMethodsExposedForTest testBooking = new TrackingBookingWithMethodsExposedForTest(trackingBooking.PK, Factory);
					testBooking.Origin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
					testBooking.Destination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
					testBooking.Booking.JS_TransportMode = Constants.TransportModes.Road;
					testBooking.IsDomesticFreight = isDomesticFreight;
					var job = Factory.NewJobForTesting<JobHeader>();
					job.JH_ParentID = trackingBooking.PK;
					AssertNoExceptionThrown(() => testBooking.GetBillOfLadingCustomisationExposedForTest());
				}
			}
		}

		public void TestHasChanges()
		{
			TrackingBooking booking = GetBizOToTestOnFactorySaving();
			Factory.Save();
			Assert(!booking.HasChanges);
			booking.UserEditableNoteHelper.EditableNoteText = "123";
			Assert(booking.HasChanges);
		}

		public void TestHasChangesWhenChangesInEditableMilestones()
		{
			var booking = GetBizOToTestOnFactorySaving();
			Factory.Save();
			Assert(!booking.HasChanges);
			AssertEquals(0, booking.EditableMilestones.Count);

			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Type = "MIL";
			var milestone = new TrackingMilestone(task);
			booking.EditableMilestones.Add(milestone);

			Assert(booking.HasChanges);
		}

		public void TestDetailedGoodsDescriptionNoteHelper()
		{
			TrackingBooking booking = GetBizOToTestOnFactorySaving();
			booking.DetailedGoodsDescriptionNoteHelper.EditableNoteText = "Test Detailed Goods Description";
			Factory.Save();
			AssertEquals("Test Detailed Goods Description", booking.DetailedGoodsDescriptionNoteHelper.EditableNoteText);
		}

		public void TestOnFactorySavingSetsNotifyPartyIfConsignorAndConsingeeAreNotLoggedInOrg()
		{
			TrackingBooking testBizO = GetBizOToTestOnFactorySaving();

			Factory.Save();
			AssertNotNull("NotifyParty should be not null", testBizO.Booking.NotifyParty);
			AssertEquals("NotifyParty should be set to the LoggedInOrg", testBizO.LoggedInContact.OC_OH, testBizO.Booking.NotifyParty.PK);

			testBizO = GetBizOToTestOnFactorySaving();
			testBizO.Booking.ConsignorDocumentaryAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgAddress>().PK;

			Factory.Save();
			AssertEquals("NotifyParty should be set to the LoggedInOrg", testBizO.LoggedInContact.OC_OH, testBizO.Booking.NotifyParty.PK);

			testBizO = GetBizOToTestOnFactorySaving();
			testBizO.Booking.ConsignorDocumentaryAddress.OrganisationPK = testBizO.LoggedInContact.OC_OH;
			testBizO.Booking.ConsignorDocumentaryAddress.E2_Contact = testBizO.LoggedInContact.OC_ContactName;
			AssertNotNull("Consignor should be set", testBizO.Booking.Consignor);
			AssertEquals("Consignor should be set to LoggedInOrg", testBizO.LoggedInContact.OC_OH, testBizO.Booking.Consignor.PK);

			Factory.Save();
			AssertNull("NotifyParty should not be set", testBizO.Booking.NotifyParty);

			testBizO = GetBizOToTestOnFactorySaving();
			testBizO.Booking.ConsigneeDocumentaryAddress.OrganisationPK = testBizO.LoggedInContact.OC_OH;
			testBizO.Booking.ConsigneeDocumentaryAddress.E2_Contact = testBizO.LoggedInContact.OC_ContactName;
			AssertNotNull("Consignee should be set", testBizO.Booking.Consignee);
			AssertEquals("Consignee should be set to LoggedInOrg", testBizO.LoggedInContact.OC_OH, testBizO.Booking.Consignee.PK);

			Factory.Save();
			AssertNull("NotifyParty should not be set", testBizO.Booking.NotifyParty);
		}

		public void TestGeneratePackLineDetailsForEmailReporting()
		{
			ForwardingShipment booking = Factory.New<ForwardingShipment>();
			booking.JS_IsBooking = true;
			TrackingBookingWithMethodsExposedForTest testBooking = new TrackingBookingWithMethodsExposedForTest(booking.PK, Factory);

			ForwardingPackLine packLine1 = Factory.New<ForwardingPackLine>();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_F3_NKPackType = "FCL";
			packLine1.JL_ActualWeight = 1;
			packLine1.JL_ActualWeightUQ = "KG";
			packLine1.JL_ActualVolume = 1;
			packLine1.JL_ActualVolumeUQ = "M3";
			packLine1.JL_Length = 1;
			packLine1.JL_Width = 1;
			packLine1.JL_Height = 1;
			packLine1.JL_UnitOfDimension = "M";
			packLine1.JL_MarksAndNumbers = "Marks";
			packLine1.JL_HarmonisedCode = "HARMONY";
			packLine1.JL_LinePrice = 1;
			packLine1.JL_Description = "desc";

			ZString expectedString = string.Format("Packs: 1{0}Pack Type: FCL{0}Weight: 1 KG{0}Volume: 1 M3{0}Length: 1 M{0}Width: 1 M{0}Height: 1 M{0}Marks & Numbers: Marks{0}Tariff #: HARMONY{0}Line Price: 1{0}Description: desc", System.Environment.NewLine);

			AssertEquals("PackLine Details Generation", expectedString, testBooking.GeneratePackLineDetailsForEmailReportingExposedForTest(packLine1));
		}

		public void TestShipmentIsUpdatedFromOuterPackLines()
		{
			TrackingBooking testBooking = new TrackingBooking(Factory, null);

			ForwardingPackLine packLine1 = testBooking.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 34;
			packLine1.JL_F3_NKPackType = "FCL";
			packLine1.JL_ActualWeight = 1;
			packLine1.JL_ActualWeightUQ = "KG";
			packLine1.JL_ActualVolume = 1;
			packLine1.JL_ActualVolumeUQ = "M3";

			ForwardingPackLine packLine2 = testBooking.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 35;
			packLine2.JL_F3_NKPackType = "FCL";
			packLine2.JL_ActualWeight = 1;
			packLine2.JL_ActualWeightUQ = "KG";
			packLine2.JL_ActualVolume = 1;
			packLine2.JL_ActualVolumeUQ = "M3";
			packLine2.JL_JS = testBooking.Booking.PK;

			testBooking.OuterPacks = 34;
			testBooking.ActualWeight = ZDecimal.Zero;
			testBooking.ActualVolume = ZDecimal.Zero;
			Factory.Save();

			AssertEquals("Precondition:", "PLT", testBooking.Booking.JS_F3_NKPackType);

			AssertEquals(34, testBooking.Booking.JS_OuterPacks);
			AssertEquals(new ZDecimal(0), testBooking.Booking.JS_ActualWeight);
			AssertEquals(new ZDecimal(0), testBooking.Booking.JS_ActualVolume);

			testBooking.OuterPacks = ZInt.Zero;
			testBooking.ActualWeight = 34;
			testBooking.ActualVolume = ZDecimal.Zero;
			Factory.Save();

			AssertEquals(0, testBooking.Booking.JS_OuterPacks);
			AssertEquals(new ZDecimal(34), testBooking.Booking.JS_ActualWeight);
			AssertEquals(new ZDecimal(0), testBooking.Booking.JS_ActualVolume);

			testBooking.OuterPacks = ZInt.Zero;
			testBooking.ActualWeight = ZDecimal.Zero;
			testBooking.ActualVolume = 34;
			Factory.Save();

			AssertEquals(0, testBooking.Booking.JS_OuterPacks);
			AssertEquals(new ZDecimal(0), testBooking.Booking.JS_ActualWeight);
			AssertEquals(new ZDecimal(34), testBooking.Booking.JS_ActualVolume);

			testBooking.OuterPacks = ZInt.Zero;
			testBooking.ActualWeight = ZDecimal.Zero;
			testBooking.ActualVolume = ZDecimal.Zero;

			Factory.Save();
			AssertEquals(69, testBooking.Booking.JS_OuterPacks);
			AssertEquals(new ZDecimal(2), testBooking.Booking.JS_ActualWeight);
			AssertEquals(new ZDecimal(2), testBooking.Booking.JS_ActualVolume);

			testBooking.Booking.JS_PackingMode = Constants.ContainerModes.FCL;

			packLine1.JL_PackageCount = 35;
			packLine1.JL_ActualWeight = 2;
			packLine1.JL_ActualVolume = 3;

			packLine2.JL_PackageCount = 36;
			packLine2.JL_ActualWeight = 4;
			packLine2.JL_ActualVolume = 5;

			Factory.Save();
			AssertEquals(71, testBooking.Booking.JS_OuterPacks);
			AssertEquals(new ZDecimal(6), testBooking.Booking.JS_ActualWeight);
			AssertEquals(new ZDecimal(8), testBooking.Booking.JS_ActualVolume);
		}

		public void TestGenerateContainerDetailsForEmailReporting()
		{
			ForwardingShipment booking = Factory.New<ForwardingShipment>();
			booking.JS_IsBooking = true;
			TrackingBookingWithMethodsExposedForTest testBooking = new TrackingBookingWithMethodsExposedForTest(booking.PK, Factory);

			RefContainer rc = Factory.NewWithValidTestData<RefContainer>();
			ForwardingContainer container1 = Factory.New<ForwardingContainer>();
			container1.JC_ContainerNum = "ABC123";
			container1.JC_RC = rc.PK;
			rc.RC_Code = "COD";
			container1.JC_ContainerCount = 2;

			ZString expectedString = string.Format("Container #: ABC123{0}Type: COD{0}Count: 2", System.Environment.NewLine);

			AssertEquals("Container Details Generation", expectedString, testBooking.GenerateContainerDetailsForEmailReportingExposedForTest(container1));
		}

		public void TestGenerateContainerDetailsForEmailReporting_NullRefContainer_Issue00196595()
		{
			ForwardingShipment booking = Factory.New<ForwardingShipment>();
			booking.JS_IsBooking = true;
			TrackingBookingWithMethodsExposedForTest testBooking = new TrackingBookingWithMethodsExposedForTest(booking.PK, Factory);

			RefContainer rc = Factory.NewWithValidTestData<RefContainer>();
			ForwardingContainer container1 = Factory.New<ForwardingContainer>();
			container1.JC_ContainerNum = "ABC123";
			//Container1.JC_RC = is invalid or unable to load here.
			rc.RC_Code = "COD";
			container1.JC_ContainerCount = 2;

			ZString expectedString = string.Format("Container #: ABC123{0}Type: UNKNOWN{0}Count: 2", System.Environment.NewLine);

			AssertNoExceptionThrown("Should not throw NullReferenceException when Container1.JC_RC is invalid PK.",
				() => testBooking.GenerateContainerDetailsForEmailReportingExposedForTest(container1));

			AssertEquals("Container Details Generation", expectedString, testBooking.GenerateContainerDetailsForEmailReportingExposedForTest(container1));
		}

		public void TestHumanReadableName()
		{
			TrackingBooking qb = GetBizOToTestOnFactorySaving();
			Factory.Save();
			AssertEquals("Booking " + qb.Number, qb.HumanReadableName);
		}

		[ExpectNoExceptions]
		public void TestNoExceptionsOnSave()
		{
			TrackingBooking testBizO = new TrackingBooking(Factory, null);
			testBizO.LoggedInContact = Factory.NewWithValidTestData<OrgContact>();
			AssertNotEquals("OC_OH should not be empty", ZGuid.Empty, testBizO.LoggedInContact.OC_OH);

			OrgHeader validHeader = Factory.NewWithValidTestData<OrgHeader>();
			testBizO.ConsigneeDeliveryAddress.OrganisationPK = validHeader.PK;

			WebJobDocAddress testWJDA = new WebJobDocAddress(testBizO.ConsignorPickupAddress);
			testWJDA.CompanyName = "asdf";
			testWJDA.Address1 = "fdsa";
			testWJDA.PostCode = "badfa";
			testWJDA.City = "poiu";
			testWJDA.State = "asb";
			testWJDA.CountryCode = "US";
			Assert(testBizO.ConsignorPickupAddress.E2_AddressOverride);
			Assert(!testWJDA.SaveAsNew);
			testWJDA.SaveAsNew = true;
			testWJDA.RunPreSaveValidation();
			Assert(testWJDA.SaveAsNewInfo.HasErrors());
			AssertNotNull(testBizO.ConsignorPickupAddress.Organisation.CountryData);
			ZGuid cdGuid = testBizO.ConsignorPickupAddress.Organisation.CountryData.PK;
			testWJDA.SaveAsNew = false;
			testWJDA.RunPreSaveValidation();
			Assert(!testWJDA.SaveAsNewInfo.HasErrors());
			Assert(testBizO.ConsignorPickupAddress.E2_AddressOverride);
			testBizO.RunPreSaveValidation();
			testBizO.Factory.Save();
			AssertNull(Factory.Load<OrgCountryData>(cdGuid));
		}

		[ExpectNoExceptions]
		public void TestRunPreSaveValidationAndSaveThrowsNoException()
		{
			TrackingBooking testBooking = GetBizOToTestOnFactorySaving();

			testBooking.Mode = "AIR";

			testBooking.ConsignorPickupAddress.E2_AddressOverride = true;
			testBooking.ConsignorPickupAddress.E2_CompanyName = "Consignor";
			testBooking.ConsignorPickupAddress.E2_Address1 = "Pickup";
			testBooking.ConsignorPickupAddress.E2_City = "Pickup";
			testBooking.ConsignorPickupAddress.E2_RN_NKCountryCode = "";

			testBooking.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			testBooking.ConsigneeDeliveryAddress.E2_CompanyName = "Consignee";
			testBooking.ConsigneeDeliveryAddress.E2_Address1 = "Delivery";
			testBooking.ConsigneeDeliveryAddress.E2_City = "Delivery";
			testBooking.ConsigneeDeliveryAddress.E2_RN_NKCountryCode = "";

			testBooking.RunPreSaveValidation();
			AssertEquals("No Errors", false, testBooking.HasErrors);

			Factory.Save();
		}

		public void TestTransportMode()
		{
			TrackingBooking testBooking = new TrackingBooking(Factory, null);
			testBooking.Booking.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(Constants.TransportModes.Sea, testBooking.TransportMode);

			testBooking = new TrackingBooking(QuotedBooking.New(QuoteBookingType.SpotQuote, Factory), null);
			AssertEquals(ZString.Empty, testBooking.TransportMode);
		}

		public void TestUseFormBuilderBillsOfLading()
		{
			TrackingBooking testBooking = new TrackingBooking(Factory, null);
			AssertEquals("Every property which is used in the Bill Of Lading document filter needs to be implemented on TrackingBooking, including UseFormBuilderBillsOfLading",
				false, testBooking.UseFormBuilderBillsOfLading);
		}

		public void TestEmptyProductsRemovedFromLinesOnSave()
		{
			var testBooking = new TrackingBooking(Factory, null);

			var packLine1 = testBooking.OuterPackLines.AddNew();
			var product = packLine1.Products.AddNew();
			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			orderLine.JO_Partno = "Z1";
			orderLine.JO_Quantity = 1;
			orderLine.JO_F3_NKPackType = Constants.PkgUnit.Unit;
			product.D2_JO = orderLine.PK;

			var emptyProduct = packLine1.Products.AddNew();
			emptyProduct.D2_JO = ZGuid.Empty;

			Factory.Save();

			Assert(!product.IsDeleted);
			Assert(emptyProduct.IsDeleted);
		}

		[HttpContextEnabledTest]
		public void TestGetFromRefPK_Booking()
		{
			var siteUser = GetSiteUser();
			var viewBooking = Factory.New<ViewTrackingBooking>();
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			booking.Booking.JS_OH_DeliveryAgent = siteUser.LoggedInOrganisation.PK;
			viewBooking.VB_JS = booking.Booking.PK;
			Factory.Save();

			var bookingPK = booking.Booking.PK;
			var testBooking = TrackingBooking.GetFromRefPK(Factory, bookingPK, siteUser);
			AssertNotNull(testBooking);
			AssertEquals(bookingPK, testBooking.BookingPK);
		}

		[HttpContextEnabledTest]
		public void TestGetFromRefPK_Quote()
		{
			var siteUser = GetSiteUser();
			var viewBooking = Factory.New<ViewTrackingBooking>();
			var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			booking.Quote.TH_OH = siteUser.LoggedInOrganisation.PK;
			var job = new JobHeader.Loader(booking.Quote).TryCreate();
			job.JH_GE = Factory.NewWithValidTestData<GlbDepartment>().PK;
			job.JH_OA_LocalChargesAddr = siteUser.LoggedInOrganisation.MainAddress.PK;
			viewBooking.VB_TH = booking.Quote.PK;
			Factory.Save();

			var quotePK = booking.Quote.PK;
			var testBooking = TrackingBooking.GetFromRefPK(Factory, quotePK, siteUser);
			AssertNotNull(testBooking);
			AssertEquals(quotePK, testBooking.QuotedBooking.Quote.PK);
		}

		public void TestGetFromRefPK_CanceledBookingWithQuote()
		{
			var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			var siteUser = GetSiteUser();
			booking.Booking.ConsigneePK = siteUser.LoggedInOrganisation.PK;
			Factory.Save();

			booking.Booking.IsCancelled = true;
			Factory.Save();

			var quotePK = booking.Quote.PK;
			var testBooking = TrackingBooking.GetFromRefPK(Factory, quotePK, siteUser);
			AssertNull(testBooking);
		}

		public void TestGetFromRefPK_Booking_UserRestricted()
		{
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var siteUser = GetSiteUser();
			Factory.Save();

			var bookingPK = booking.Booking.PK;
			var testBooking = TrackingBooking.GetFromRefPK(Factory, bookingPK, siteUser);
			AssertNull(testBooking);
		}

		public void TestGetFromRefPK_Quote_UserRestricted()
		{
			var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			var siteUser = GetSiteUser();
			Factory.Save();

			var quotePK = booking.Quote.PK;
			var testBooking = TrackingBooking.GetFromRefPK(Factory, quotePK, siteUser);
			AssertNull(testBooking);
		}

		public void TestGetFromRefPK_Booking_QuickView()
		{
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var siteUser = GetSiteUser();
			siteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			Factory.Save();

			var bookingPK = booking.Booking.PK;
			var testBooking = TrackingBooking.GetFromRefPK(Factory, bookingPK, siteUser);
			AssertNotNull(testBooking);
			AssertEquals(bookingPK, testBooking.BookingPK);
		}

		public void TestGetFromRefPK_UnsupportedType()
		{
			var testBooking = TrackingBooking.GetFromRefPK(Factory, Guid.NewGuid(), GetSiteUser());
			AssertNull(testBooking);
		}

		TrackingSiteUser GetSiteUser()
		{
			var helper = new TestHelper(Factory);

			return helper.TestSiteUser;
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Globals.IsWeb = true;
		}

		protected override void TearDown()
		{
			SuppressionForTest.CacheObjectClear();
			Globals.IsWeb = false;
			base.TearDown();
		}

		TrackingBooking GetBizOToTestOnFactorySaving()
		{
			TrackingBooking testBizO = new TrackingBooking(Factory, null);
			testBizO.LoggedInContact = Factory.NewWithValidTestData<OrgContact>();
			AssertNotEquals("OC_OH should not be empty", ZGuid.Empty, testBizO.LoggedInContact.OC_OH);

			AssertNull("NotifyParty should not be set", testBizO.Booking.NotifyParty);
			AssertNull("Consignor should not be set", testBizO.Booking.Consignor);
			AssertNull("ConsignEE should not be set", testBizO.Booking.Consignee);

			return testBizO;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TrackingBooking(Factory, new TestHelper(Factory).TestSiteUser);
		}

		#region TrackingBookingWithMethodsExposedForTest

		class TrackingBookingWithMethodsExposedForTest : TrackingBooking
		{
			public TrackingBookingWithMethodsExposedForTest(ZGuid bookingPK, BusinessObjectFactory factory)
				: base(bookingPK, factory, null)
			{
			}

			public NumberGeneratorContext GetBillOfLadingCustomisationExposedForTest()
			{
				return GetBillOfLadingCustomisation();
			}

			public ZString GeneratePackLineDetailsForEmailReportingExposedForTest(ForwardingPackLine packLine)
			{
				return GeneratePackLineDetailsForEmailReporting(packLine);
			}

			public ZString GenerateContainerDetailsForEmailReportingExposedForTest(ForwardingContainer container)
			{
				return GenerateContainerDetailsForEmailReporting(container);
			}
		}

		#endregion

		#endregion
	}
}

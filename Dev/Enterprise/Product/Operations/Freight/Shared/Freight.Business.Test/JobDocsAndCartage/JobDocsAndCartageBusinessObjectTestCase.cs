using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;
using Constants = CargoWise.EventReference.Constants;
using EventParameters = Enterprise.Core.Constants.EventReferenceParameterTypes;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobDocsAndCartage))]
	public class JobDocsAndCartageBusinessObjectTestCase : EnterpriseBusinessObjectTestCase
	{
		#region GetLogParentForEventDateProperty

		protected override BusinessObject GetLogParentForEventDateProperty()
		{
			var shipment = Factory.New<CommonShipment>();
			var docsAndCartage = (JobDocsAndCartage)BusinessObject;
			docsAndCartage.JP_ParentID = shipment.PK;
			docsAndCartage.JP_ParentTableCode = shipment.TablePrefix;
			var hit = shipment.DocsAndCartage;
			return shipment;
		}

		#endregion

		#region TestNoSelfConcurrencyErrorWhenUpdatingOrderItems

		public void TestNoSelfConcurrencyErrorWhenUpdatingOrderItems()
		{
			var consol1 = (CommonConsol)Factory.New<IForwardingConsol>();
			var shipment1 = consol1.Shipments.AddNew();
			var docsAndCartage1 = shipment1.DocsAndCartage;
			docsAndCartage1.JP_OrderItemsAsString = "1,2,3";

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("prerequisite",
				new[] { "1", "2", "3" },
				docsAndCartage1.OrderItems.Cast<AutoJobOrderItem>().Select(o => o.JT_OrderReference.ToString()));

			var otherFactory = new BusinessObjectFactory();
			var shipment2 = (CommonShipment)otherFactory.Load<IForwardingShipment>(shipment1.PK);
			var docsAndCartage2 = shipment2.DocsAndCartage;

			docsAndCartage2.JP_OrderItemsAsString = "";

			docsAndCartage1.OrderItems.Cast<AutoJobOrderItem>().First(o => o.JT_OrderReference == "2").JT_OrderReference = "4";
			Factory.Save();

			AssertEquals("Should not restore already deleted elements", 0, docsAndCartage2.OrderItems.Count);

			AssertNoExceptionThrown("expected no concurrency errors", otherFactory.Save);

			AssertEquals("Should not restore already deleted elements", 0, docsAndCartage2.OrderItems.Count);
			AssertEquals("Should correctly refresh first factory", 0, docsAndCartage1.OrderItems.Count);
		}

		#endregion

		#region DefaultDropMode

		public void TestDefaultDropMode()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			shipment.OuterPackLines.AddNew();

			CommonPickupDeliveryConfirm pickupConfirm = shipment.PickupConfirms.AddNew();
			AssertEquals("", pickupConfirm.EU_DropMode);

			CommonPickupDeliveryConfirm deliveryConfirm = shipment.DeliveryConfirms.AddNew();
			AssertEquals("", deliveryConfirm.EU_DropMode);

			shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "111";
			shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = "222";

			AssertEquals("111", pickupConfirm.EU_DropMode);
			AssertEquals("222", deliveryConfirm.EU_DropMode);
		}

		#endregion

		#region EventDateProperty

		#region Estimated Dates

		public void TestEventDateProperty_JP_EstimatedPickup()
		{
			var now = ZDateTimeOffset.Now;
			var shipment = Factory.New<CommonShipment>();
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_EstimatedPickup);

			shipment.Logs.GetAllLogs().AddNew(Events.PickupCartageCompleteFinalised, now.AddHours(-2));
			AssertEquals("Should not have populated for the actual date", ZDateTime.Empty, shipment.DocsAndCartage.JP_EstimatedPickup);

			shipment.Logs.GetAllLogs().AddNew(Events.PickupCartageCompleteFinalised, now, true);
			AssertEquals("Should have populated for the estimated date", now.ToZDateTime(), shipment.DocsAndCartage.JP_EstimatedPickup);
		}

		public void TestEventDateProperty_JP_EstimatedDelivery()
		{
			var now = ZDateTimeOffset.Now;
			var shipment = Factory.New<CommonShipment>();
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_EstimatedDelivery);

			shipment.Logs.GetAllLogs().AddNew(Events.DeliveryCartageCompleteFinalised, now.AddHours(-2));
			AssertEquals("Should not have populated for the actual date", ZDateTime.Empty, shipment.DocsAndCartage.JP_EstimatedDelivery);

			shipment.Logs.GetAllLogs().AddNew(Events.DeliveryCartageCompleteFinalised, now, true);
			AssertEquals("Should have populated for the estimated date", now.ToZDateTime(), shipment.DocsAndCartage.JP_EstimatedDelivery);
		}

		#endregion

		#region EventDateProperty_JP_PickupCartageCompleted

		public void TestEventDateProperty_JP_PickupCartageCompleted()
		{
			var now = ZDateTimeOffset.Now;
			var shipment = Factory.New<CommonShipment>();
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_PickupCartageCompleted);

			shipment.Logs.GetAllLogs().AddNew(Events.PickupCartageCompleteFinalised, now);
			AssertEquals(now.ToZDateTime(), shipment.DocsAndCartage.JP_PickupCartageCompleted);
		}

		#endregion

		#region EventDateProperty_JP_DeliveryCartageCompleted

		public void TestEventDateProperty_JP_DeliveryCartageCompleted()
		{
			var now = ZDateTimeOffset.Now;
			var shipment = Factory.New<CommonShipment>();
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_DeliveryCartageCompleted);

			shipment.Logs.GetAllLogs().AddNew(Events.DeliveryCartageCompleteFinalised, now);
			AssertEquals(now.ToZDateTime(), shipment.DocsAndCartage.JP_DeliveryCartageCompleted);
		}

		#endregion

		#region EventDateProperty_JP_PickupCartageAdvised

		public void TestEventDateProperty_JP_PickupCartageAdvised()
		{
			var now = ZDateTimeOffset.Now;
			var shipment = Factory.New<CommonShipment>();
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_PickupCartageAdvised);

			shipment.Logs.GetAllLogs().AddNew(Events.PickupCartageAdvised, now, true);
			AssertEquals("Should not have populated for the estimated date", ZDateTime.Empty, shipment.DocsAndCartage.JP_PickupCartageAdvised);

			shipment.Logs.GetAllLogs().AddNew(Events.PickupCartageAdvised, now);
			AssertEquals("Should have populated for the actual date", now.ToZDateTime(), shipment.DocsAndCartage.JP_PickupCartageAdvised);
		}

		#endregion

		#region EventDateProperty_JP_DeliveryCartageAdvised

		public void TestEventDateProperty_JP_DeliveryCartageAdvised()
		{
			var now = ZDateTimeOffset.Now;
			var shipment = Factory.New<CommonShipment>();
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_DeliveryCartageAdvised);

			shipment.Logs.GetAllLogs().AddNew(Events.DeliveryCartageAdvised, now, true);
			AssertEquals("Should not have populated for the estimated date", ZDateTime.Empty, shipment.DocsAndCartage.JP_DeliveryCartageAdvised);

			shipment.Logs.GetAllLogs().AddNew(Events.DeliveryCartageAdvised, now);
			AssertEquals("Should have populated for the actual date", now.ToZDateTime(), shipment.DocsAndCartage.JP_DeliveryCartageAdvised);
		}

		#endregion

		#region EventDateProperty_BKQEventAdded

		public void TestEventDateProperty_BKQEventAdded_Pickup()
		{
			var shipment = Factory.New<CommonShipment>();
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_PickupCartageAdvised);
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_DeliveryCartageAdvised);

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = shipment.PK;
				log.SL_Table = "JobShipment";
				log.SL_SE_NKEvent = Events.BookingRequested.Code;
				log.SL_EventTime = ZDateTime.Today;
				log.Parameters.Add(Constants.EventReferenceParameters.Codes.Type, EventParameters.PickupTransport);
			}
			shipment.Logs.GetAllLogs().Add(log);
			AssertEquals("Should not have populated for the delivery date", ZDateTime.Empty, shipment.DocsAndCartage.JP_DeliveryCartageAdvised);
			AssertEquals("Should have populated for the pickup date", ZDateTime.Today, shipment.DocsAndCartage.JP_PickupCartageAdvised);
		}

		public void TestEventDateProperty_BKQEventAdded_Delivery()
		{
			var shipment = Factory.New<CommonShipment>();
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_PickupCartageAdvised);
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_DeliveryCartageAdvised);

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = shipment.PK;
				log.SL_Table = "JobShipment";
				log.SL_SE_NKEvent = Events.BookingRequested.Code;
				log.SL_EventTime = ZDateTime.Today;
				log.Parameters.Add(Constants.EventReferenceParameters.Codes.Type, EventParameters.DeliveryTransport);
			}
			shipment.Logs.GetAllLogs().Add(log);
			AssertEquals("Should have populated for the delivery date", ZDateTime.Today, shipment.DocsAndCartage.JP_DeliveryCartageAdvised);
			AssertEquals("Should not have populated for the pickup date", ZDateTime.Empty, shipment.DocsAndCartage.JP_PickupCartageAdvised);
		}

		public void TestEventDateProperty_BKQEventAdded_NoType()
		{
			var shipment = Factory.New<CommonShipment>();
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_PickupCartageAdvised);
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_DeliveryCartageAdvised);

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = shipment.PK;
				log.SL_Table = "JobShipment";
				log.SL_SE_NKEvent = Events.BookingRequested.Code;
				log.SL_EventTime = ZDateTime.Today;
			}
			shipment.Logs.GetAllLogs().Add(log);
			AssertEquals("Should have populated for the delivery date", ZDateTime.Empty, shipment.DocsAndCartage.JP_DeliveryCartageAdvised);
			AssertEquals("Should not have populated for the pickup date", ZDateTime.Empty, shipment.DocsAndCartage.JP_PickupCartageAdvised);
		}

		#endregion

		#endregion

		#region CargoAvailable event

		public void TestFactorySave_ParentIsShipment_AddCargoAvailableEventToShipmentIfAvailabilityDateHasChanges()
		{
			EnsureCargoAvailableEventCreated(Enterprise.Core.Constants.ContainerModes.FCL, d => d.JP_FCLAvailable, Constants.Facilities.Code.Terminal);
			EnsureCargoAvailableEventCreated(Enterprise.Core.Constants.ContainerModes.LCL, d => d.JP_LCLAvailable, Constants.Facilities.Code.Depot);
		}

		void EnsureCargoAvailableEventCreated(ZString containerMode, Func<JobDocsAndCartage, ZDateTime> expectedEventTime, string expectedFacility)
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_PackingMode = containerMode;
			shipment.JS_RL_NKDestination = "UAIEV";

			Factory.Save();

			shipment.DocsAndCartage.JP_FCLAvailable = 5.DaysAgo();
			shipment.DocsAndCartage.JP_LCLAvailable = 10.DaysAgo();

			Factory.Save();

			var log = shipment.Logs.MostRecentLogByEventTime(Events.CargoAvailable);
			AssertNotNull(string.Format("Log with {0} event", Events.CargoAvailable.Code), log);
			AssertEquals("Event date", expectedEventTime(shipment.DocsAndCartage), log.SL_EventTime);
			AssertEquals("Location", "UAIEV", log.Parameters[Constants.EventReferenceParameters.Codes.Location]);
			AssertEquals("Facility", expectedFacility, log.Parameters[Constants.EventReferenceParameters.Codes.Facility]);
		}

		#endregion

		public void TestNeedsServiceEvents()
		{
			var jobDeclaration = Factory.New<IBaseJobDeclaration>();
			var docsAndCartage = JobDocsAndCartage.New(jobDeclaration as IDocsAndCartageParent);
			Assert("NeedsServiceEvents should be true", ((IHaveServices)docsAndCartage).NeedsServiceEvents);
		}

		public void TestGetAvailableOrStorageDateNRE()
		{
			var consol = Factory.New<CommonConsol>();
			var leg = consol.Transports[0];
			leg.JW_JX = Factory.New<JobSailing>().PK;

			var shipment = consol.Shipments.AddNew();
			var docsAndCartage = JobDocsAndCartage.New(shipment);

			ZDateTime x;
			AssertNoExceptionThrown(() => x = docsAndCartage.JP_FCLAvailable);
		}

		public void TestExcludeEventsFromAdd()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			JobDocsAndCartage docsAndCartage = JobDocsAndCartage.New(shipment);
			AssertEquals(false, shipment.GetLogs().EventsThatCannotBeAdded.Contains(Events.CustomsCleared));
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "JobDocsAndCartage BizOs can only be instantiated through JobDocsAndCartage.New(IDocsAndCartageParent Parent).")]
		public void TestRejectNonStaticInstantiation()
		{
			JobDocsAndCartage docsAndCartage = Factory.New<JobDocsAndCartage>();
		}

		public void TestDeliveryAddressOverrideReadOnlyFields()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			JobDocAddress bO = shipment.ConsigneeDeliveryAddress;

			Assert("Default value", !bO.E2_AddressOverride);
			AssertEquals("Default value", ZGuid.Empty, bO.E2_OA_Address);

			OrgHeader orgH = Factory.New<OrgHeader>();
			bO.E2_OA_Address = orgH.MainAddress.PK;

			Assert(!bO.E2_OA_AddressInfo.ReadOnly);
			Assert(bO.E2_Address1Info.ReadOnly);
			Assert(bO.E2_Address2Info.ReadOnly);
			Assert(bO.E2_CityInfo.ReadOnly);
			Assert(bO.E2_PostcodeInfo.ReadOnly);
			Assert(bO.E2_StateInfo.ReadOnly);

			Assert(bO.E2_RN_NKCountryCodeInfo.ReadOnly);
			Assert(bO.E2_PhoneInfo.ReadOnly);
			Assert(bO.E2_FaxInfo.ReadOnly);
			Assert(bO.E2_EmailInfo.ReadOnly);
			Assert(bO.E2_CompanyNameInfo.ReadOnly);
			Assert(!bO.E2_ContactInfo.ReadOnly);

			bO.E2_AddressOverride = true;
			Assert(bO.E2_OA_AddressInfo.ReadOnly);
			Assert(!bO.E2_Address1Info.ReadOnly);
			Assert(!bO.E2_Address2Info.ReadOnly);
			Assert(!bO.E2_CityInfo.ReadOnly);
			Assert(!bO.E2_PostcodeInfo.ReadOnly);
			Assert(!bO.E2_StateInfo.ReadOnly);

			Assert(!bO.E2_RN_NKCountryCodeInfo.ReadOnly);
			Assert(!bO.E2_PhoneInfo.ReadOnly);
			Assert(!bO.E2_FaxInfo.ReadOnly);
			Assert(!bO.E2_EmailInfo.ReadOnly);
			Assert(!bO.E2_CompanyNameInfo.ReadOnly);
			Assert(!bO.E2_ContactInfo.ReadOnly);
		}

		public void TestDeliveryAddressOverrideShowsCorrectFields()
		{
			OrgHeader org = OrgHeader.New(Factory);
			org.MainAddress.OA_Address1 = "18 Henricks Avenue";
			org.MainAddress.OA_Address2 = "Our House";
			org.MainAddress.OA_City = "Newington";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_PostCode = "2127";

			OrgAddress address2 = org.Addresses.AddNew();
			address2.AddressCapability.SetCapabilityEnabled("DLV");
			address2.OA_Address1 = "27 Simpson Street";
			address2.OA_Address2 = "Her House";
			address2.OA_City = "Dundas";
			address2.OA_State = "NSW";
			address2.OA_PostCode = "2117";

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.ConsigneePK = org.PK;
			JobDocAddress bO = shipment.ConsigneeDeliveryAddress;

			bO.E2_AddressOverride = false;
			AssertEquals("Default org delivery address shown", address2.OA_Address1, bO.E2_Address1);
			AssertEquals("Default org delivery address shown", address2.OA_Address2, bO.E2_Address2);
			AssertEquals("Default org delivery address shown", address2.OA_City, bO.E2_City);
			AssertEquals("Default org delivery address shown", address2.OA_PostCode, bO.E2_Postcode);
			AssertEquals("Default org delivery address shown", address2.OA_State, bO.E2_State);

			bO.E2_AddressOverride = true;
			AssertEquals("Default org delivery address still shown", address2.OA_Address1, bO.E2_Address1);
			AssertEquals("Default org delivery address still shown", address2.OA_Address2, bO.E2_Address2);
			AssertEquals("Default org delivery address still shown", address2.OA_City, bO.E2_City);
			AssertEquals("Default org delivery address still shown", address2.OA_PostCode, bO.E2_Postcode);
			AssertEquals("Default org delivery address still shown", address2.OA_State, bO.E2_State);

			bO.E2_AddressOverride = false;
			bO.E2_OA_Address = org.MainAddress.PK;

			AssertEquals("Specified address shown", org.MainAddress.OA_Address1, bO.E2_Address1);
			AssertEquals("Specified address shown", org.MainAddress.OA_Address2, bO.E2_Address2);
			AssertEquals("Specified address shown", org.MainAddress.OA_City, bO.E2_City);
			AssertEquals("Specified address shown", org.MainAddress.OA_PostCode, bO.E2_Postcode);
			AssertEquals("Specified address shown", org.MainAddress.OA_State, bO.E2_State);

			bO.E2_AddressOverride = true;

			AssertEquals("Foreign key address is MISC", OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress, bO.E2_OA_Address);
			AssertEquals("Specified address still shown", org.MainAddress.OA_Address1, bO.E2_Address1);
			AssertEquals("Specified address still shown", org.MainAddress.OA_Address2, bO.E2_Address2);
			AssertEquals("Specified address still shown", org.MainAddress.OA_City, bO.E2_City);
			AssertEquals("Specified address still shown", org.MainAddress.OA_PostCode, bO.E2_Postcode);
			AssertEquals("Specified address still shown", org.MainAddress.OA_State, bO.E2_State);

			bO.E2_Address1 = "26 Myrtle Street";
			bO.E2_Address2 = "Our Old House";
			bO.E2_City = "Prospect";
			bO.E2_Postcode = "2149";
			bO.E2_State = "VIC";

			AssertEquals("Overidden address shown", "26 Myrtle Street", bO.E2_Address1);
			AssertEquals("Overidden address shown", "Our Old House", bO.E2_Address2);
			AssertEquals("Overidden address shown", "Prospect", bO.E2_City);
			AssertEquals("Overidden address shown", "2149", bO.E2_Postcode);
			AssertEquals("Overidden address shown", "VIC", bO.E2_State);

			bO.E2_AddressOverride = false;
			shipment.ConsigneePK = ZGuid.Empty;

			AssertEquals("Default org DLV address should be shown.", "", bO.E2_Address1);
			AssertEquals("Default org DLV address should be shown.", "", bO.E2_Address2);
			AssertEquals("Default org DLV address should be shown.", "", bO.E2_City);
			AssertEquals("Default org DLV address should be shown.", "", bO.E2_Postcode);
			AssertEquals("Default org DLV address should be shown.", "", bO.E2_State);

			bO.OrganisationPK = ZGuid.Empty;
			AssertEquals("No consignee, so blank address", "", bO.E2_Address1);
			AssertEquals("No consignee, so blank address", "", bO.E2_Address2);
			AssertEquals("No consignee, so blank address", "", bO.E2_City);
			AssertEquals("No consignee, so blank address", "", bO.E2_Postcode);
			AssertEquals("No consignee, so blank address", "", bO.E2_State);
		}

		public void TestPickupAddressOverrideReadOnlyFields()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			JobDocAddress bO = shipment.ConsignorPickupAddress;
			Assert("Default value", !bO.E2_AddressOverride);
			AssertEquals("Default value", ZGuid.Empty, bO.E2_OA_Address);

			OrgHeader orgH = Factory.New<OrgHeader>();
			bO.E2_OA_Address = orgH.MainAddress.PK;

			Assert(!bO.E2_OA_AddressInfo.ReadOnly);
			Assert(bO.E2_Address1Info.ReadOnly);
			Assert(bO.E2_Address2Info.ReadOnly);
			Assert(bO.E2_CityInfo.ReadOnly);
			Assert(bO.E2_PostcodeInfo.ReadOnly);
			Assert(bO.E2_StateInfo.ReadOnly);

			bO.E2_AddressOverride = true;
			Assert(bO.E2_OA_AddressInfo.ReadOnly);
			Assert(!bO.E2_Address1Info.ReadOnly);
			Assert(!bO.E2_Address2Info.ReadOnly);
			Assert(!bO.E2_CityInfo.ReadOnly);
			Assert(!bO.E2_PostcodeInfo.ReadOnly);
			Assert(!bO.E2_StateInfo.ReadOnly);
		}

		public void TestPickupAddressOverrideShowsCorrectFields()
		{
			OrgHeader org = OrgHeader.New(Factory);
			org.MainAddress.OA_Address1 = "18 Henricks Avenue";
			org.MainAddress.OA_Address2 = "Our House";
			org.MainAddress.OA_City = "Newington";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_PostCode = "2127";
			OrgAddress address2 = org.Addresses.AddNew();
			address2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			address2.OA_Address1 = "27 Simpson Street";
			address2.OA_Address2 = "Her House";
			address2.OA_City = "Dundas";
			address2.OA_State = "NSW";
			address2.OA_PostCode = "2117";

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.ConsignorPK = org.PK;
			JobDocAddress bO = shipment.ConsignorPickupAddress;

			bO.E2_AddressOverride = false;
			bO.E2_OA_Address = org.MainAddress.PK;

			AssertEquals("Specified address shown", org.MainAddress.OA_Address1, bO.E2_Address1);
			AssertEquals("Specified address shown", org.MainAddress.OA_Address2, bO.E2_Address2);
			AssertEquals("Specified address shown", org.MainAddress.OA_City, bO.E2_City);
			AssertEquals("Specified address shown", org.MainAddress.OA_PostCode, bO.E2_Postcode);
			AssertEquals("Specified address shown", org.MainAddress.OA_State, bO.E2_State);

			bO.E2_AddressOverride = true;

			AssertEquals("Foreign key address is MISC", OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress, bO.E2_OA_Address);
			AssertEquals("Specified address still shown", org.MainAddress.OA_Address1, bO.E2_Address1);
			AssertEquals("Specified address still shown", org.MainAddress.OA_Address2, bO.E2_Address2);
			AssertEquals("Specified address still shown", org.MainAddress.OA_City, bO.E2_City);
			AssertEquals("Specified address still shown", org.MainAddress.OA_PostCode, bO.E2_Postcode);
			AssertEquals("Specified address still shown", org.MainAddress.OA_State, bO.E2_State);

			bO.E2_Address1 = "26 Myrtle Street";
			bO.E2_Address2 = "Our Old House";
			bO.E2_City = "Prospect";
			bO.E2_Postcode = "2149";
			bO.E2_State = "VIC";

			AssertEquals("Overidden address shown", "26 Myrtle Street", bO.E2_Address1);
			AssertEquals("Overidden address shown", "Our Old House", bO.E2_Address2);
			AssertEquals("Overidden address shown", "Prospect", bO.E2_City);
			AssertEquals("Overidden address shown", "2149", bO.E2_Postcode);
			AssertEquals("Overidden address shown", "VIC", bO.E2_State);

			bO.E2_AddressOverride = false; //so now delivery is pointing back to main address

			shipment.ConsignorPK = ZGuid.Empty;
			AssertEquals("Default org pickup address shown.", "", bO.E2_Address1);
			AssertEquals("Default org pickup address shown.", "", bO.E2_Address2);
			AssertEquals("Default org pickup address shown.", "", bO.E2_City);
			AssertEquals("Default org pickup address shown.", "", bO.E2_Postcode);
			AssertEquals("Default org pickup address shown.", "", bO.E2_State);

			bO.OrganisationPK = ZGuid.Empty;
			AssertEquals("No consignor, so blank address.", "", bO.E2_Address1);
			AssertEquals("No consignor, so blank address.", "", bO.E2_Address2);
			AssertEquals("No consignor, so blank address.", "", bO.E2_City);
			AssertEquals("No consignor, so blank address.", "", bO.E2_Postcode);
			AssertEquals("No consignor, so blank address.", "", bO.E2_State);
		}

		public void TestStaticCreateNewFromShipment()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			JobDocsAndCartage dC = shipment.DocsAndCartage;

			AssertNotNull("DocsAndCartage should be a valid Bizo.", dC);
			AssertEquals("Shipment's DocsAndCartage ParentID should be Parent's PK.", dC.JP_ParentID, shipment.PK);
			AssertEquals("Shipment's DocsAndCartage TableCode should be Parent's.", dC.JP_ParentTableCode, "JS");
		}

		public void TestStaticCreateNewFromABizOWithAShipment()
		{
			AnyBizOWithShipmentForTest bizO = Factory.New<AnyBizOWithShipmentForTest>();
			JobDocsAndCartage dC1 = bizO.DocsAndCartage;

			AssertNotNull("DocsAndCartage should be a valid Bizo.", dC1);
			AssertEquals("Shipment's DocsAndCartage ParentID should be Parent's PK.", dC1.JP_ParentID, bizO.PK);

			CommonShipment shipment = Factory.New<CommonShipment>();
			JobDocsAndCartage dC2 = shipment.DocsAndCartage;

			AssertNotNull("DocsAndCartage should be a valid Bizo.", dC2);
			AssertEquals("Shipment's DocsAndCartage ParentID should be Parent's PK.", dC2.JP_ParentID, shipment.PK);

			bizO.SetShipment(shipment);

			AssertNotNull("DocsAndCartage should be a valid Bizo.", bizO.DocsAndCartage);
			AssertEquals("Shipment's DocsAndCartage should be Parent's DocsAndCartage.", bizO.DocsAndCartage.PK, dC2.PK);
			AssertEquals("Original Bizo's DocsAndCartage should be deleted.", true, dC1.IsDeleted);
		}

		public void TestDates_DateKind_Unspecified()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var jobDocsAndCartage = shipment.DocsAndCartage;

			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, jobDocsAndCartage.JP_EstimatedPickup.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, jobDocsAndCartage.JP_EstimatedDelivery.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, jobDocsAndCartage.JP_PickupCartageCompleted.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, jobDocsAndCartage.JP_DeliveryCartageCompleted.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, jobDocsAndCartage.JP_PickupCartageAdvised.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, jobDocsAndCartage.JP_DeliveryCartageAdvised.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, jobDocsAndCartage.JP_PickupRequiredBy.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, jobDocsAndCartage.JP_DeliveryRequiredBy.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, jobDocsAndCartage.JP_FCLAvailable.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, jobDocsAndCartage.JP_FCLStorageCommences.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, jobDocsAndCartage.JP_LCLAvailable.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, jobDocsAndCartage.JP_LCLStorageCommences.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, jobDocsAndCartage.JP_PickupLabourTime.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, jobDocsAndCartage.JP_PickupTruckWaitTime.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, jobDocsAndCartage.JP_DeliveryLabourTime.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, jobDocsAndCartage.JP_DeliveryTruckWaitTime.Kind);
		}

		public void TestStaticLoadExistingFromShipment()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			JobDocsAndCartage dC = shipment.DocsAndCartage;

			AssertNotNull("DocsAndCartage should be a valid Bizo.", dC);
			AssertEquals("Shipment's DocsAndCartage ParentID should be Parent's PK.", dC.JP_ParentID, shipment.PK);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonShipment shipmentCopy = newFactory.Load<CommonShipment>(shipment.PK);
			AssertEquals("Both Shipments' DocsAndCartage should be the same.", dC.PK, shipmentCopy.DocsAndCartage.PK);
		}

		public void TestMakeNonPersistent()
		{
			JobDocsAndCartage bO = (JobDocsAndCartage)GetNewBusinessObject();
			bO.HasChanges = true;
			Assert("Should be saved", bO.IsSavedByFactory);
			bO.MakeNonPersistent();
			Assert("Should no longer be saved", !bO.IsSavedByFactory);
		}

		public virtual void TestJobServices()
		{
			JobDocsAndCartage docsAndCartage = JobDocsAndCartage.New(new MockJobDocsAndCartageParent(Factory));
			AssertNotNull("DocsAndCartage should contain a job services collection", docsAndCartage.Services);
		}

		public virtual void TestJobServicesGetsLoadedProperly()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			JobService fumigation = shipment.DocsAndCartage.Services.AddNew();
			fumigation.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonShipment loadedShipment = newFactory.Load<CommonShipment>(shipment.PK);
			Assert("Shipment's job services did not load correctly", loadedShipment.DocsAndCartage.Services.Count > 0);
		}

		public virtual void TestJobServicesIsRegisteredEditableChild()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.DocsAndCartage.SetReadOnlyIncludingChildren(true);
			AssertEquals("Job services should be read only (registered editable child of JobDocsAndCartage)", true, shipment.DocsAndCartage.Services.ReadOnly);
		}

		public virtual void TestJobRequiredDocuments()
		{
			JobDocsAndCartage docsAndCartage = JobDocsAndCartage.New(new MockJobDocsAndCartageParent(Factory));
			AssertNotNull("DocsAndCartage should contain a job docs collection", docsAndCartage.RequiredDocuments);
		}

		public void TestSetDefaultValues()
		{
			Env.Registry.Freight.AirWaybill.HAWBDimensionsDefault = Core.Constants.AWB.Dimensions.DEF;
			try
			{
				JobDocsAndCartage docsAndCartage = JobDocsAndCartage.New(new MockJobDocsAndCartageParent(Factory));
				AssertEquals(Core.Constants.AWB.Dimensions.DEF, docsAndCartage.JP_PrintOptionForPackagesOnAWB);

				Env.Registry.Freight.AirWaybill.HAWBDimensionsDefault = Core.Constants.AWB.Dimensions.M3;

				docsAndCartage = JobDocsAndCartage.New(new MockJobDocsAndCartageParent(Factory));
				AssertEquals(Core.Constants.AWB.Dimensions.M3, docsAndCartage.JP_PrintOptionForPackagesOnAWB);
			}
			finally
			{
				Env.Registry.Freight.AirWaybill.HAWBDimensionsDefault = Core.Constants.AWB.Dimensions.DEF;
			}
		}

		public virtual void TestJobRequiredDocumentsGetsLoadedProperly()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			JobRequiredDocument originalBill = shipment.DocsAndCartage.RequiredDocuments.AddNew();
			originalBill.EQ_DocType = Core.Constants.RefDocTypes.MasterBill;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonShipment loadedShipment = newFactory.Load<CommonShipment>(shipment.PK);
			Assert("Shipment's job docs did not load correctly", loadedShipment.DocsAndCartage.RequiredDocuments.Count > 0);
		}

		public virtual void TestJobRequiredDocumentsIsRegisteredEditableChild()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.DocsAndCartage.SetReadOnlyIncludingChildren(true);
			AssertEquals("Job docs should be read only (registered editable child of JobDocsAndCartage)", true, shipment.DocsAndCartage.RequiredDocuments.ReadOnly);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return JobDocsAndCartage.New(new MockJobDocsAndCartageParent(Factory));
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var shipment = factory.NewWithValidTestData<CommonShipment>();
			return shipment.DocsAndCartage;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var parent = Factory.New<CommonShipment>();
			return JobDocsAndCartage.New(parent);
		}

		internal class AnyBizOWithShipmentForTest : CommonShipment, IShipmentProvider
		{
			public AnyBizOWithShipmentForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override ConsolCollection GetNewConsolCollection()
			{
				return new ConsolCollection(this);
			}

			public new JobDocsAndCartage DocsAndCartage
			{
				get
				{
					if (fDocsAndCartage == null)
					{
						fDocsAndCartage = base.DocsAndCartage;
					}
					return fDocsAndCartage;
				}
			}
			JobDocsAndCartage fDocsAndCartage;

			CommonShipment IShipmentProvider.Shipment
			{
				get { return fShipment; }
			}

			public void SetShipment(CommonShipment shipment)
			{
				fShipment = shipment;
				fDocsAndCartage = JobDocsAndCartage.SwitchDocsAndCartageAndRemoveOriginal(this);
			}
			CommonShipment fShipment;
		}

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Shared;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using ContainerModes = Enterprise.Core.Constants.ContainerModes;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingContainer))]
	public class TrackingContainerBOTest : CommonContainerBusinessObjectTest
	{
		#region Milestones

		public void TestMilestones()
		{
			var testContainer = Factory.NewWithValidTestData<TrackingContainer>();
			AssertNotNull(testContainer.Milestones);
			AssertEquals(0, testContainer.Milestones.Count);

			var milestone1 = testContainer.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "lastCompleted1";
			milestone1.P9_IsPublished = true;
			milestone1.SetMilestoneActualDateForTest(DateTime.Now);
			milestone1.P9_Sequence = 1;

			var milestone2 = testContainer.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Description = "lastCompleted2";
			milestone2.P9_IsPublished = true;
			milestone2.SetMilestoneActualDateForTest(DateTime.Now);
			milestone2.P9_Sequence = 1;
			testContainer.Factory.Save();
			AssertEquals(0, testContainer.Milestones.Count);

			testContainer.ReloadMilestones();
			AssertEquals(2, testContainer.Milestones.Count);
		}

		[HttpContextEnabledTest]
		public void TestEditableMilestones()
		{
			var helper = new TestHelper(Factory);
			var testContainer = Factory.NewWithValidTestData<TrackingContainer>();
			testContainer.JC_ContainerNum = "123456";

			var testShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			testShipment.JS_OH_DeliveryAgent = helper.TestSiteUser.LoggedInOrganisation.PK;
			var testPackLine = Factory.New<PackLine>();
			testPackLine.JL_FreightMode = "AIR";
			testPackLine.JL_JS = testShipment.PK;
			testPackLine.JL_JC = testContainer.PK;
			testContainer.PackLines.Add(testPackLine);

			var milestone1 = testContainer.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "Editable";
			milestone1.TriggerConditions.TriggerEventCode = "AAA";
			milestone1.P9_IsPublished = true;
			milestone1.SetMilestoneScheduledDateForTest(ZDateTimeOffset.Now);

			var milestone2 = testContainer.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Description = "Not Editable";
			milestone2.TriggerConditions.TriggerEventCode = "BBB";
			milestone2.P9_IsPublished = true;
			milestone2.SetMilestoneScheduledDateForTest(ZDateTimeOffset.Now);

			Factory.Save();
			var containerMilestoneEventUpdatesCollection = new ContainerMilestoneEventUpdatesCollection();
			containerMilestoneEventUpdatesCollection.AddNew("AAA", WebPartyType.DeliveryAgent);
			WebDataRegistry.Instance.ContainerMilestoneEventUpdates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, containerMilestoneEventUpdatesCollection);

			var editableMilestonesCollection = testContainer.EditableMilestones;
			AssertEquals(1, editableMilestonesCollection.Count);
			AssertEquals(editableMilestonesCollection[0].EventCode, "AAA");
		}

		#endregion

		#region Last Free Day

		public void TestLastFreeDay()
		{
			TrackingContainer container = Factory.NewWithValidTestData<TrackingContainer>();
			AssertEquals(ZDateTime.Empty, container.JC_ArrivalCTOStorageStartDate);
			AssertEquals(ZDateTime.Empty, container.JC_LastFreeDay);

			ZDateTime testDate = ZDateTime.Now;
			container.JC_ArrivalCTOStorageStartDate = testDate;
			AssertEquals(testDate.Date.AddDays(-1), container.JC_LastFreeDay);

			testDate = ZDateTime.Now.AddDays(10);
			container.JC_ArrivalCTOStorageStartDate = testDate;
			AssertEquals(testDate.Date.AddDays(-1), container.JC_LastFreeDay);
		}

		#endregion

		#region Storage Begins

		public void TestStorageBegins()
		{
			TrackingContainer container = Factory.NewWithValidTestData<TrackingContainer>();
			AssertEquals(ZDateTime.Empty, container.JC_ArrivalCTOStorageStartDate);
			AssertEquals(ZDateTime.Empty, container.StorageBegins);

			ZDateTime testDate = ZDateTime.Now;
			container.JC_ArrivalCTOStorageStartDate = testDate;
			AssertEquals(testDate, container.StorageBegins);

			testDate = ZDateTime.Now.AddDays(10);
			container.JC_ArrivalCTOStorageStartDate = testDate;
			AssertEquals(testDate, container.StorageBegins);
		}

		#endregion

		[HttpContextEnabledTest]
		public void TestOrders()
		{
			TestHelper helper = new TestHelper(Factory);
			AssertNotNull("Precondition: SiteUser has been set up", helper.TestSiteUser);
			Assert("WebUser should be logged in", helper.TestSiteUser.IsLoggedIn);

			TrackingContainer container = Factory.NewWithValidTestData<TrackingContainer>();
			container.JC_ContainerNum = "123456";

			Order order = Factory.NewWithValidTestData<Order>();
			order.SupplierPK = helper.TestOrg.PK;
			OrderLine line1 = order.OrderLines.AddNew();
			line1.JO_ContainerNumber = container.ContainerNumber;
			OrderLine line2 = order.OrderLines.AddNew();
			OrderLineDelivery delivery = line2.Deliveries.AddNew();
			OrderLineDeliverContainer deliverContainer = delivery.Containers.AddNew();
			deliverContainer.J5_ContainerNum = container.ContainerNumber;
			OrderLine line3 = order.OrderLines.AddNew();

			Order order2 = Factory.NewWithValidTestData<Order>();
			line1 = order2.OrderLines.AddNew();
			line1.JO_ContainerNumber = container.ContainerNumber;

			Factory.Save();
			AssertEquals("Orders.Count", 0, container.Orders.Count);
			AssertEquals("TrackingContainer OrderLines", 0, container.OrderLines.Count);

			var plannedContainer = order.PlannedContainers.AddNew();
			plannedContainer.J1_ContainerNumber = container.JC_ContainerNum;
			plannedContainer.J1_RC = container.JC_RC;

			Factory.Save();
			var anotherFactory = new BusinessObjectFactory();
			container = anotherFactory.Load<TrackingContainer>(container.PK);
			AssertEquals("Orders.Count", 1, container.Orders.Count);
		}

		public void TestConsigneesComesFromDeclarationIfConfirmIsEmpty()
		{
			TrackingContainer testContainer = Factory.New<TrackingContainer>();
			BaseJobDeclaration testDeclaration = Factory.New<BaseJobDeclaration>();
			BaseCusContainer customsContainer = Factory.New<BaseCusContainer>();

			customsContainer.CO_JE = testDeclaration.PK;
			customsContainer.CO_JC = testContainer.PK;
			testDeclaration.ImporterDeliveryAddress.E2_AddressOverride = ZBool.True;
			testDeclaration.ImporterDeliveryAddress.E2_CompanyName = "COMPANY";
			testDeclaration.ImporterDeliveryAddress.E2_Address1 = "Address1";
			testDeclaration.ImporterDeliveryAddress.E2_Address2 = "Address2";
			testDeclaration.ImporterDeliveryAddress.E2_Postcode = "Post";
			testDeclaration.ImporterDeliveryAddress.E2_RN_NKCountryCode = ZString.Empty;

			AssertEquals(testContainer.Consignees, "COMPANY\r\nADDRESS1\r\nADDRESS2\r\nPOST\r\n");
		}

		public void TestShipmentNumbers()
		{
			TrackingContainer testContainer = Factory.New<TrackingContainer>();
			BaseJobDeclaration testDeclaration = Factory.New<BaseJobDeclaration>();
			BaseCusContainer customsContainer = Factory.New<BaseCusContainer>();

			AssertEquals(testContainer.ShipmentNumbers, ZString.Empty);
			customsContainer.CO_JE = testDeclaration.PK;
			customsContainer.CO_JC = testContainer.PK;

			testDeclaration.JE_DeclarationReference = "DECREF123";
			testContainer.ShipmentNumbersHasBeenCalculated = false;
			AssertEquals(testContainer.ShipmentNumbers, "DECREF123");
		}

		public void TestCustomsContainer()
		{
			TrackingContainer testContainer = Factory.New<TrackingContainer>();
			BaseCusContainer customsContainer = Factory.New<BaseCusContainer>();
			customsContainer.CO_JC = testContainer.PK;

			AssertEquals(customsContainer, testContainer.CustomsContainer);
		}

		public void TestArrival()
		{
			TrackingContainer testContainer = Factory.New<TrackingContainer>();

			BaseJobDeclaration testDeclaration = Factory.New<BaseJobDeclaration>();
			BaseCusContainer customsContainer = Factory.New<BaseCusContainer>();
			CommonConsol consol = Factory.New<CommonConsol>();
			testContainer.JC_JK = consol.PK;

			customsContainer.CO_JE = testDeclaration.PK;
			customsContainer.CO_JC = testContainer.PK;

			ZDateTime result = ZDateTime.Now.AddDays(-6);
			customsContainer.Declaration.JE_DateOfFirstArrival = result;
			AssertEquals(result, testContainer.Arrival);

			result = ZDateTime.Now.AddDays(-5);
			customsContainer.Declaration.JE_DateOfArrival = result;
			AssertEquals(result, testContainer.Arrival);

			result = ZDateTime.Now.AddDays(-4);
			Transport testTransport = testContainer.Consol.Transports[0] ?? testContainer.Consol.Transports.AddNew();
			testTransport.JW_ETA = result;
			AssertEquals(result, testContainer.Arrival);

			result = ZDateTime.Now.AddDays(-3);
			testTransport.JW_ATA = result;
			AssertEquals(result, testContainer.Arrival);
		}

		public void TestVesselAndVoyage_ConsolTransport()
		{
			var testContainer = Factory.New<TrackingContainer>();
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports[0] ?? consol.Transports.AddNew();
			transport.JW_Vessel = "EVER GIVEN 123";
			transport.JW_VoyageFlight = "FLIGHT 123";
			testContainer.JC_JK = consol.PK;

			AssertEquals(testContainer.VesselName, "EVER GIVEN 123");
			AssertEquals(testContainer.Voyage, "FLIGHT 123");
		}

		public void TestVesselAndVoyage_Declaration()
		{
			var testContainer = Factory.New<TrackingContainer>();
			var testDeclaration = Factory.New<BaseJobDeclaration>();
			var customsContainer = Factory.New<BaseCusContainer>();
			customsContainer.CO_JE = testDeclaration.PK;
			customsContainer.CO_JC = testContainer.PK;

			testDeclaration.JE_VesselName = "abcd";
			testDeclaration.JE_VoyageFlightNo = "123";

			AssertEquals(testContainer.VesselName, "abcd");
			AssertEquals(testContainer.Voyage, "123");
		}

		public void TestDeparture()
		{
			TrackingContainer testContainer = Factory.New<TrackingContainer>();

			BaseJobDeclaration testDeclaration = Factory.New<BaseJobDeclaration>();
			BaseCusContainer customsContainer = Factory.New<BaseCusContainer>();
			CommonConsol consol = Factory.New<CommonConsol>();
			testContainer.JC_JK = consol.PK;

			customsContainer.CO_JE = testDeclaration.PK;
			customsContainer.CO_JC = testContainer.PK;

			ZDateTime result = ZDateTime.Now.AddDays(-5);
			customsContainer.Declaration.JE_ExportDate = result;
			AssertEquals(result, testContainer.Departure);

			result = ZDateTime.Now.AddDays(-4);
			Transport testTransport = testContainer.Consol.Transports[0] ?? testContainer.Consol.Transports.AddNew();
			testTransport.JW_ETD = result;
			AssertEquals(result, testContainer.Departure);

			result = ZDateTime.Now.AddDays(-3);
			testTransport.JW_ATD = result;
			AssertEquals(result, testContainer.Departure);
		}

		public void TestWebEmailNotificationNumber()
		{
			TrackingContainer container = Factory.NewWithValidTestData<TrackingContainer>();
			AssertEquals("Number is a ContainerNumber", container.ContainerNumber, ((IBizOChangesEmailNotification)container).Number);
		}

		public void TestWebEmailNotificationIsCancelled()
		{
			TrackingContainer container = Factory.NewWithValidTestData<TrackingContainer>();
			AssertEquals("Is not Cancelled", false, ((IBizOChangesEmailNotification)container).IsCancelled);
		}

		public void TestNotificationOptions()
		{
			AssertEquals(WebDataRegistry.Instance.ContainerNotificationOptions, ((IBizOChangesEmailNotification)Factory.NewWithValidTestData<TrackingContainer>()).NotificationSendingRule);
		}

		public void TestWebEmailNotificationEventBranch()
		{
			OrgHeader controllingParty = Factory.NewWithValidTestData<OrgHeader>();

			OrgCompanyData orgCompanyData = controllingParty.CompanyDataCollection.AddNew();

			GlbCompany glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_IsActive = true;

			GlbBranch glbBranch = Factory.NewWithValidTestData<GlbBranch>();
			glbBranch.GB_GC = glbCompany.PK;
			glbBranch.GB_IsActive = true;

			orgCompanyData.OB_GC = glbCompany.PK;
			Factory.Save();

			TrackingContainer container = Factory.NewWithValidTestData<TrackingContainer>();
			AssertNull("No Consol no Forwarder no Branch", ((IBizOChangesEmailNotification)container).EventBranch);

			TrackingConsol consol = Factory.NewWithValidTestData<TrackingConsol>();
			container.JC_JK = consol.PK;
			AssertNull("Forwarder is empty", ((IBizOChangesEmailNotification)container).RelatedOrg);
			AssertNull("No Forwarder no Branch", ((IBizOChangesEmailNotification)container).EventBranch);

			controllingParty.OH_RL_NKClosestPort = "USNYC";
			consol.SetDefaultReceivingForwarderAddress(controllingParty);
			AssertEquals("RelatedOrg", controllingParty, ((IBizOChangesEmailNotification)container).RelatedOrg);
			AssertNull("No Branch no exception -- fall back to enterprise level", ((IBizOChangesEmailNotification)container).EventBranch);

			controllingParty.CompanyDataCollection[0].OB_GB_ControllingBranch = glbBranch.PK;
			Factory.Save();
			AssertEquals("It searches by controllung branch", glbBranch, ((IBizOChangesEmailNotification)container).EventBranch);
		}

		public void TestWebEmailRelatedOrg_FCLImportConsol()
		{
			AssertWebEmailRelatedOrgImportShipmentConsignee(ContainerModes.FCL);
		}

		public void TestWebEmailRelatedOrg_BCNImportConsol()
		{
			AssertWebEmailRelatedOrgImportShipmentConsignee(ContainerModes.BuyersConsol);
		}

		void AssertWebEmailRelatedOrgImportShipmentConsignee(string consolMode)
		{
			var shipmentConsignee = Factory.NewWithValidTestData<OrgHeader>();
			var orgCompanyData = shipmentConsignee.CompanyDataCollection.AddNew();
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_IsActive = true;
			var glbBranch = Factory.NewWithValidTestData<GlbBranch>();
			glbBranch.GB_GC = glbCompany.PK;
			glbBranch.GB_IsActive = true;
			orgCompanyData.OB_GC = glbCompany.PK;
			Factory.Save();

			var container = Factory.NewWithValidTestData<TrackingContainer>();
			var consol = Factory.NewWithValidTestData<TrackingConsol>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "MYPKG";
			shipment.JS_RL_NKDestination = glbBranch.GB_RL_NKHomePort;
			shipment.ConsigneeDeliveryAddress.OrganisationPK = shipmentConsignee.PK;
			AssertEquals("Precondition", true, shipment.IsImport());
			consol.JK_ConsolMode = consolMode;
			consol.Shipments.Add(shipment);
			container.JC_JK = consol.PK;

			AssertEquals("RelatedOrg is import shipment consignee", shipmentConsignee, ((IBizOChangesEmailNotification)container).RelatedOrg);
		}

		public void TestWebContainerStaffRolesToNotify()
		{
			TrackingContainer container = Factory.NewWithValidTestData<TrackingContainer>();
			AssertEquals("ContainerStaffRolesEmailNotificationGroup", WebDataRegistry.Instance.ContainerNotificationStaffRoles, ((IBizOChangesEmailNotification)container).StaffRolesToNotify);
		}

		public void TestWebEmailNotificationEmailGroupRegistryItem()
		{
			TrackingContainer container = Factory.NewWithValidTestData<TrackingContainer>();
			AssertEquals("ContainerEmailNotificationGroup", WebDataRegistry.Instance.ContainerNotificationEmailGroup, ((IBizOChangesEmailNotification)container).EmailGroupRegistryItem);
		}

		public void TestWebEmailNotificationControllerForEnterpriseUrl()
		{
			TrackingContainer container = Factory.NewWithValidTestData<TrackingContainer>();
			AssertEquals("Containers Controller", ControllerIDs.Containers, ((IBizOChangesEmailNotification)container).ControllerForEnterpriseUrl);
		}

		#region TestSortessages

		public void TestSortMessages()
		{
			ZDateTime testTime = ZDateTime.Now;

			CMRCARSTMessage m1 = Factory.NewWithValidTestData<CMRCARSTMessage>();
			m1.EM_SystemCreateTimeUtc = testTime.AddDays(1);
			SetInterchangeNumber(m1, "1");
			m1.EM_MessageNum = "6";

			CMRCARSTMessage m2 = Factory.NewWithValidTestData<CMRCARSTMessage>();
			m2.EM_SystemCreateTimeUtc = testTime;
			SetInterchangeNumber(m2, "2");
			m2.EM_MessageNum = "6";

			CMRCARSTMessage m3 = Factory.NewWithValidTestData<CMRCARSTMessage>();
			m3.EM_SystemCreateTimeUtc = testTime.AddDays(1);
			SetInterchangeNumber(m3, "2");
			m3.EM_MessageNum = "6";

			CMRCARSTMessage m4 = Factory.NewWithValidTestData<CMRCARSTMessage>();
			m4.EM_SystemCreateTimeUtc = testTime.AddDays(1);
			SetInterchangeNumber(m4, "2");
			m4.EM_MessageNum = "5";

			CMRCARSTMessage m6 = Factory.NewWithValidTestData<CMRCARSTMessage>();
			m6.EM_SystemCreateTimeUtc = testTime.AddDays(1);
			m6.EM_MessageNum = "6";

			CMRCARSTMessage m7 = Factory.NewWithValidTestData<CMRCARSTMessage>();
			m7.EM_SystemCreateTimeUtc = testTime.AddDays(1);
			SetInterchangeNumber(m7, "2");

			Factory.Save();

			List<CMRCARSTMessage> messages = new List<CMRCARSTMessage> { m7, m4, m1, m3, m2, m6 };

			Factory.New<DummyTrackingContainerForTest>().SortMessagesForTest(messages);

			AssertEquals(m2.PK, messages[0].PK);
			AssertEquals(m6.PK, messages[1].PK);
			AssertEquals(m1.PK, messages[2].PK);
			AssertEquals(m7.PK, messages[3].PK);
			AssertEquals(m4.PK, messages[4].PK);
			AssertEquals(m3.PK, messages[5].PK);
		}

		void SetInterchangeNumber(CMRCARSTMessage message, string number)
		{
			EDIInterchange ei = Factory.NewWithValidTestData<EDIInterchange>();
			ei.EI_InterchangeNum = number;
			message.EM_EI = ei.PK;
		}

		#endregion

		public void TestPickupAndDeliveryAddressesAreOnlyShownForShipmentsRelatedToLoggedInUsers()
		{
			OrgHeader notTheLoggedInOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			notTheLoggedInOrganisation.OH_FullName = "Some Other Organization";

			TestHelper helper = new TestHelper(Factory);
			helper.TestOrg.OH_FullName = "The Logged in Organization";

			ForwardingShipment shipmentA = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingConsol consol = shipmentA.Consols.AddNew();
			ForwardingPackLine lineA = shipmentA.OuterPackLines.AddNew();

			shipmentA.JS_OH_DeliveryAgent = helper.TestOrg.PK;
			shipmentA.ConsigneeDeliveryAddress.OrganisationPK = helper.TestOrg.PK;
			shipmentA.ConsignorPickupAddress.E2_AddressOverride = true;
			shipmentA.ConsignorPickupAddress.E2_CompanyName = "Overridden Consignor A";

			ForwardingShipment shipmentB = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentB.ConsigneeDocumentaryAddress.OrganisationPK = notTheLoggedInOrganisation.PK;
			shipmentB.ConsignorPickupAddress.E2_AddressOverride = true;
			shipmentB.ConsignorPickupAddress.E2_CompanyName = "Overridden Consignor B";
			consol.Shipments.Add(shipmentB);
			ForwardingPackLine lineB = shipmentB.OuterPackLines.AddNew();

			TrackingContainer container = Factory.New<TrackingContainer>();
			consol.Containers.Add(container);
			lineA.SetContainer(consol, container);
			lineB.SetContainer(consol, container);

			container.SiteUser = helper.TestSiteUser;
			Factory.Save();

			AssertEquals("Precondition: ShipmentA's Consignee", "The Logged in Organization", shipmentA.ConsigneeDeliveryAddress.E2_CompanyName);
			AssertEquals("Precondition: ShipmentA's Consignor", "Overridden Consignor A", shipmentA.ConsignorPickupAddress.E2_CompanyName);
			AssertEquals("Precondition: ShipmentB's Consignee", "Some Other Organization", shipmentB.ConsigneeDeliveryAddress.E2_CompanyName);
			AssertEquals("Precondition: ShipmentB's Consignor", "Overridden Consignor B", shipmentB.ConsignorPickupAddress.E2_CompanyName);

			AssertNotNull("Container.OriginConfirm", container.OriginConfirm);
			AssertNotNull("OriginConfirm.Shipment", container.OriginConfirm.FirstShipment);
			AssertEquals("OriginConfirm.Shipment", shipmentA.PK, container.OriginConfirm.FirstShipment.PK);

			AssertNotNull("Container.DestinationConfirm", container.DestinationConfirm);
			AssertNotNull("DestinationConfirm.Shipment", container.DestinationConfirm.FirstShipment);
			AssertEquals("DestinationConfirm.Shipment", shipmentA.PK, container.DestinationConfirm.FirstShipment.PK);

			AssertContains("Consignees should show Shipment A's Consignee",
			shipmentA.ConsigneeDeliveryAddress.E2_CompanyName, container.ConsigneesExtended);
			AssertContains("Consignors should show Shipment A's Consignor",
			shipmentA.ConsignorPickupAddress.E2_CompanyName, container.ConsignorsExtended);

			AssertNotContains("Consignees should NOT show Shipment B's Consignee",
			shipmentB.ConsignorPickupAddress.E2_CompanyName, container.ConsigneesExtended);
			AssertNotContains("Consignors should NOT show Shipment B's Consignor",
			shipmentB.ConsignorPickupAddress.E2_CompanyName, container.ConsignorsExtended);

			container.OriginConfirm.ConfirmAddress.E2_AddressOverride = true;
			container.OriginConfirm.ConfirmAddress.OrganisationNameOrPK = "Overriden Origin Confirm";
			AssertContains("Consignors should show Origin Confirm Address", "Overriden Origin Confirm", container.ConsignorsExtended);

			container.DestinationConfirm.ConfirmAddress.E2_AddressOverride = true;
			container.DestinationConfirm.ConfirmAddress.OrganisationNameOrPK = "Overriden Destination Confirm";
			AssertContains("Consignees should show Destination Confirm Address", "Overriden Destination Confirm", container.ConsigneesExtended);
		}

		public void TestSuppressionOfWeightDetails()
		{
			AssertWeightDetailsAreSuppressed(ContainerModes.BuyersConsol, false);
			AssertWeightDetailsAreSuppressed(ContainerModes.FCL, false);
			AssertWeightDetailsAreSuppressed(ContainerModes.LCL, true);
			AssertWeightDetailsAreSuppressed(ContainerModes.Groupage, true);

			AssertWeightDetailsAreSuppressed(ContainerModes.AIR, true);
			AssertWeightDetailsAreSuppressed(ContainerModes.AgentConsol, true);
		}

		void AssertWeightDetailsAreSuppressed(ZString containerMode, ZBool shouldBeSuppressed)
		{
			TrackingContainer container = Factory.New<TrackingContainer>();
			container.ContainerModeForBinding = containerMode;
			AssertEquals("Precondition: Container Mode should be set correctly", containerMode, container.ContainerModeForBinding);

			ZString expectedTareWeight = shouldBeSuppressed ? SuppressUtil.SuppressedText.ToString() : container.JC_TareWeight.ToString();
			AssertEquals("Tare Weight for Container Mode: [" + containerMode + "]", expectedTareWeight, container.JC_TareWeightWithSuppression);

			ZString expectedTotalWeight = shouldBeSuppressed ? SuppressUtil.SuppressedText.ToString() : container.JC_Calc_TotalWeight.ToString();
			AssertEquals("Total Weight for Container Mode: [" + containerMode + "]", expectedTotalWeight, container.JC_TotalWeightWithSuppression);
		}

		public void TestRequiredDeliveryStatus()
		{
			TrackingContainer container = Factory.New<TrackingContainer>();
			AssertEquals("Initially should be empty", true, container.RequiredDelivery.IsEmpty);
			AssertEquals("Initially status should be empty", true, container.RequiredDeliveryStatus.IsEmpty);

			container.JC_ArrivalCTOStorageStartDate = ZDateTime.Now.Date.AddDays(-1); //LastFreeDay
			AssertEquals("Status should be Overdue", Constants.DateTimeStatus.Overdue, container.RequiredDeliveryStatus);

			container.RequiredDelivery = ZDateTime.Now.Date;
			AssertEquals("Status should be Late", Constants.DateTimeStatus.Late, container.RequiredDeliveryStatus);

			container.JC_ArrivalCTOStorageStartDate = ZDateTime.Now.Date.AddDays(1).AddMinutes(11); //LastFreeDay
			AssertEquals("Status should be OnTime", Constants.DateTimeStatus.OnTime, container.RequiredDeliveryStatus);
		}

		public void TestConfirmedDeliveryStatus()
		{
			TrackingContainer container = Factory.New<TrackingContainer>();
			AssertEquals("Initially should be empty", true, container.ConfirmedDelivery.IsEmpty);
			AssertEquals("Initially status should be empty", true, container.ConfirmedDeliveryStatus.IsEmpty);

			container.RequiredDelivery = ZDateTime.Now.AddDays(-1);
			AssertEquals("Status should be Overdue", Constants.DateTimeStatus.Overdue, container.ConfirmedDeliveryStatus);

			container.ConfirmedDelivery = ZDateTime.Now;
			AssertEquals("Status should be Late", Constants.DateTimeStatus.Late, container.ConfirmedDeliveryStatus);

			container.RequiredDelivery = container.ConfirmedDelivery;
			AssertEquals("Status should be OnTime", Constants.DateTimeStatus.OnTime, container.ConfirmedDeliveryStatus);
		}

		public void TestActualDeliveryStatus()
		{
			TrackingContainer container = Factory.New<TrackingContainer>();
			AssertEquals("Initially should be empty", true, container.ActualDelivery.IsEmpty);
			AssertEquals("Initially status should be empty", true, container.ActualDeliveryStatus.IsEmpty);

			container.ConfirmedDelivery = ZDateTime.Now.AddDays(-1);
			AssertEquals("Status should be Overdue", Constants.DateTimeStatus.Overdue, container.ActualDeliveryStatus);

			container.ActualDelivery = ZDateTime.Now;
			AssertEquals("Status should be Late", Constants.DateTimeStatus.Late, container.ActualDeliveryStatus);

			container.ActualDelivery = container.ConfirmedDelivery;
			AssertEquals("Status should be OnTime", Constants.DateTimeStatus.OnTime, container.ActualDeliveryStatus);
		}

		public void TestEmptyReadyStatus()
		{
			TrackingContainer container = Factory.New<TrackingContainer>();
			AssertEquals("Initially should be empty", true, container.EmptyReady.IsEmpty);
			AssertEquals("Initially status should be empty", true, container.EmptyReadyStatus.IsEmpty);

			container.JC_EmptyReturnedBy = ZDateTime.Now.AddDays(-1); // EmptyReturnRequired
			AssertEquals("Status should be Overdue", Constants.DateTimeStatus.Overdue, container.EmptyReadyStatus);

			container.EmptyReady = ZDateTime.Now;
			AssertEquals("Status should be Late", Constants.DateTimeStatus.Late, container.EmptyReadyStatus);

			container.JC_EmptyReturnedBy = container.EmptyReady; // EmptyReturnRequired
			AssertEquals("Status should be OnTime", Constants.DateTimeStatus.OnTime, container.EmptyReadyStatus);
		}

		public void TestEmptyPickupShowsCorrectData()
		{
			TrackingContainer container = Factory.New<TrackingContainer>();
			AssertEquals("Initially should be empty", true, container.EmptyPickup.IsEmpty);

			var now = ZDateTime.Now;
			container.JC_EmptyReturnedBy = now;
			AssertEquals("EmptyPickup should return JC_EmptyReturnedBy", now, container.EmptyPickup);

			container.JC_EmptyRequired = ZDateTime.Now.AddDays(-1);
			AssertNotEquals("EmptyPickup should not return JC_EmptyRequired", container.JC_EmptyRequired, container.EmptyPickup);
		}

		public void TestActualDehireStatus()
		{
			TrackingContainer container = Factory.New<TrackingContainer>();
			AssertEquals("Initially should be empty", true, container.ActualDehire.IsEmpty);
			AssertEquals("Initially status should be empty", true, container.ActualDehireStatus.IsEmpty);

			container.JC_EmptyReturnedBy = ZDateTime.Now.AddDays(-1); // EmptyReturnRequired
			AssertEquals("Status should be Overdue", Constants.DateTimeStatus.Overdue, container.ActualDehireStatus);

			container.ActualDehire = ZDateTime.Now;
			AssertEquals("Status should be Late", Constants.DateTimeStatus.Late, container.ActualDehireStatus);

			container.JC_EmptyReturnedBy = container.ActualDehire;
			AssertEquals("Status should be OnTime", Constants.DateTimeStatus.OnTime, container.ActualDehireStatus);
		}

		[HttpContextEnabledTest]
		public void TestSettingLoggingReference()
		{
			TestHelper helper = new TestHelper(Factory);
			AssertNotNull("Precondition: SiteUser has been set up", helper.TestSiteUser);

			TrackingContainer container = Factory.New<TrackingContainer>();
			AssertNotNull("Precondition: SiteUser not null", container.SiteUser);
			Assert("Precondition: Site User has contact+company reference", container.SiteUser.ContactAndCompanyReference.Length > 0);
			AssertEquals("Logging Reference Set On Container", container.SiteUser.ContactAndCompanyReference, container.Logs.AutoCreatedLogDefaultSL_Reference);

			OrgContact newContact = helper.TestOrg.Contacts.AddNew();
			newContact.OC_ContactName = "newcontact";
			newContact.OC_Email = "newcontact@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("test123");
			Factory.Save();

			helper.TestSiteUser.Login("", "newcontact@cargowise.com", "test123");
			Assert("New user logged in", helper.TestSiteUser.IsLoggedIn);
			AssertEquals("Email of new user", newContact.OC_Email, helper.TestSiteUser.LoggedInUser.OC_Email);

			AssertNotEquals("Logged in site user has different reference to container", container.Logs.AutoCreatedLogDefaultSL_Reference, helper.TestSiteUser.ContactAndCompanyReference);

			container.SiteUser = helper.TestSiteUser;
			AssertEquals("Logging reference set on new SiteUser", helper.TestSiteUser.ContactAndCompanyReference, container.Logs.AutoCreatedLogDefaultSL_Reference);
		}

		[HttpContextEnabledTest]
		public void TestOrderLines()
		{
			TestHelper helper = new TestHelper(Factory);
			AssertNotNull("Precondition: SiteUser has been set up", helper.TestSiteUser);
			Assert("WebUser should be logged in", helper.TestSiteUser.IsLoggedIn);

			TrackingContainer container = Factory.NewWithValidTestData<TrackingContainer>();
			container.JC_ContainerNum = "123456";
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			PackLine packLine = Factory.New<PackLine>();
			packLine.JL_FreightMode = "AIR";
			packLine.JL_JS = shipment.PK;
			packLine.JL_JC = container.PK;
			container.PackLines.Add(packLine);

			ForwardingConsol consol = shipment.Consols.AddNew();
			container.JC_JK = consol.PK;
			consol.SetDefaultSendingForwarderAddress(helper.TestOrg);

			Order order = Factory.NewWithValidTestData<Order>();
			order.JD_JS = shipment.PK;
			order.SupplierPK = helper.TestOrg.PK;

			OrderLine line1 = order.OrderLines.AddNew();
			line1.JO_ContainerNumber = container.ContainerNumber;

			OrderLine line2 = order.OrderLines.AddNew();
			OrderLineDelivery delivery = line2.Deliveries.AddNew();
			OrderLineDeliverContainer deliverContainer = delivery.Containers.AddNew();
			deliverContainer.J5_ContainerNum = container.ContainerNumber;

			OrderLine line3 = order.OrderLines.AddNew();

			Factory.Save();
			AssertEquals("PackLines.Count", 1, container.PackLines.Count);
			AssertEquals("Shipments.Count", 1, container.Shipments.Count);
			AssertEquals("Orders.Count", 1, container.Orders.Count);
			AssertEquals("TrackingContainer OrderLines", 2, container.OrderLines.Count);
		}

		[HttpContextEnabledTest]
		public void TestQuarantineCode()
		{
			var helper = new TestHelper(Factory);
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.SetDefaultSendingForwarderAddress(helper.TestOrg);
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "PIXU2145650";
			container.JC_ContainerMode = ContainerModes.FCL;
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "2165489595";
			shipment.JS_OH_ImportBroker = helper.TestSiteUser.LoggedInOrganisation.PK;

			var packing = shipment.OuterPackLines.AddNew();
			packing.SetContainer(container.PK);
			packing.JL_PackageCount = 10;

			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			oceanBill.DefaultFromConsol();
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_JS = shipment.PK;
			houseBill.DefaultFromShipment();
			Factory.Save();
			AssertEquals(1, houseBill.Pivot.Count);

			var pivot = houseBill.Pivot[0];
			var message = pivot.Messages.AddNew(typeof(CMRCARSTMessage));
			message.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+34:::CARST+471J B2D5 GFB5:1+8'DTM+9:20150812135056298386:ZZZ'FTX+AHN+++CONSOLIDATED STATUS:WITHDRAWN'FTX+AHN+++CARGO REPORT SAC:N/A'TDT+20+326545++11++++9044748::11'LOC+12+AUSYD::6'NAD+MR+AAA374M::95'NAD+UD+41065894724::95'RFF+ABO:S00046151/CMT2::2'RFF+MB:1234578787'RFF+BH:2165489595'RFF+AAQ:PIXU2145650'DOC+1'PAC+++FCL:67:95'UNT+16+000001'";

			var trackingContainer = Factory.Load<TrackingContainer>(container.PK);
			trackingContainer.SiteUser = helper.TestSiteUser;
			AssertEquals("WITHDRAWN", trackingContainer.QuarantineCode);
		}

		#region FromPKFilteredBySiteUserTest

		[HttpContextEnabledTest]
		public void TestFromPKFilteredBySiteUser()
		{
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			AssertNotNull("TestOrg", helper.TestOrg);
			AssertNotNull("TestUser", helper.TestSiteUser);
			Assert("WebUser should be logged in", helper.TestSiteUser.IsLoggedIn);
			AssertEquals(false, helper.TestSiteUser.IsShipmentQuickViewUser);

			TrackingContainer containerWithNoConsole = Factory.NewWithValidTestData<TrackingContainer>();
			containerWithNoConsole.JC_ContainerNum = "123458";
			Factory.Save();

			TrackingContainer testContainerFromNumberWithNoConsole = TrackingContainer.FromPKFilteredBySiteUser(Factory, containerWithNoConsole.PK, helper.TestSiteUser);
			AssertEquals(null, testContainerFromNumberWithNoConsole);

			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			AssertNotNull("TestOrg", helper.TestOrg);
			AssertNotNull("TestUser", helper.TestSiteUser);
			Assert("WebUser should be logged in", helper.TestSiteUser.IsLoggedIn);
			AssertEquals(false, helper.TestSiteUser.IsShipmentQuickViewUser);

			TrackingContainer containerWithConsole = Factory.NewWithValidTestData<TrackingContainer>();
			containerWithConsole.JC_ContainerNum = "123456";
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			ForwardingConsol consol = shipment.Consols.AddNew();
			containerWithConsole.JC_JK = consol.PK;
			consol.SetDefaultSendingForwarderAddress(helper.TestOrg);
			Factory.Save();

			TrackingContainer testContainerFromNumberWithConsole = TrackingContainer.FromPKFilteredBySiteUser(Factory, containerWithConsole.PK, helper.TestSiteUser);
			AssertEquals(containerWithConsole, testContainerFromNumberWithConsole);

			helper.TestSiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			AssertNotNull("TestOrg", helper.TestOrg);
			AssertNotNull("TestUser", helper.TestSiteUser);
			Assert("WebUser should be logged in", helper.TestSiteUser.IsLoggedIn);
			AssertEquals(true, helper.TestSiteUser.IsShipmentQuickViewUser);

			TrackingContainer container = Factory.NewWithValidTestData<TrackingContainer>();
			container.JC_ContainerNum = "123456";
			Factory.Save();

			TrackingContainer testContainerFromNumber = TrackingContainer.FromPKFilteredBySiteUser(Factory, container.PK, helper.TestSiteUser);
			AssertEquals(container, testContainerFromNumber);
		}

		#endregion FromPKFilteredBySiteUserTest

		#region Implementation

		class DummyTrackingContainerForTest : TrackingContainer
		{
			public DummyTrackingContainerForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override List<ZGuid> GetShipmentPKsFromPackLines()
			{
				return ShipmentPKsFromPackLinesForTest;
			}

			public List<ZGuid> ShipmentPKsFromPackLinesForTest
			{
				get
				{
					if (fShipmentPKsFromPackLinesForTest == null)
					{
						fShipmentPKsFromPackLinesForTest = new List<ZGuid>();
					}

					return fShipmentPKsFromPackLinesForTest;
				}
			}

			List<ZGuid> fShipmentPKsFromPackLinesForTest;

			public void SortMessagesForTest(List<CMRCARSTMessage> messages)
			{
				SortMessages(messages);
			}
		}

		#endregion
	}
}

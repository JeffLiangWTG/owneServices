using System;
using System.Collections;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class BillOfLadingShipmentTest : BaseAgencyTest
	{
		public void TestDtbBookingParentControllerID()
		{
			var billOfLading = Factory.New<AgencyBooking>();
			AssertEquals(ControllerIDs.AgencyBooking, ((IDtbBookingParent)billOfLading).ControllerID);
		}

		public void TestGetNewValidation()
		{
			BillOfLading shipment = Factory.NewWithValidTestData<BillOfLading>();
			AssertEquals("Type of Validation", typeof(BillOfLadingValidation), shipment.Validation.GetType());
		}

		public void TestContainerReadonlyness()
		{
			BillOfLading booking = Factory.New<BillOfLading>();
			AssertEquals("BookedContainers should be readonly", true, booking.BookedContainers.ReadOnly);
			AssertEquals("RealContainers should not be readonly", false, booking.RealContainers.ReadOnly);
		}

		public void TestConfirm_TopLevelPacks_RealContainersWouldNotBeReaonly()
		{
			var booking = Factory.New<AgencyBooking>();
			booking.JS_PackingMode = Constants.ContainerModes.BreakBulk;
			booking.TopLevelPacks.AddNew();
			Factory.Save();
			var shipment = Factory.Load<BillOfLading>(booking.PK);
			AssertEquals("Precondition: booked container is readonly", true, shipment.TopLevelPacks.FirstOrDefault().ReadOnly);
			shipment.Confirm();
			AssertEquals("Converted real container is not readonly", false, shipment.TopLevelPacks.FirstOrDefault().ReadOnly);
		}

		[UseDummyCustomsMessageStatusProvider]
		public void TestShouldShowCustomsDetail()
		{
			BillOfLading billOfLading = Factory.New<BillOfLading>();
			DummyCustomsMessageStatusProvider.Instance.ShouldShow = true;
			AssertEquals(true, billOfLading.ShouldShowCustomsDetail);
			DummyCustomsMessageStatusProvider.Instance.ShouldShow = false;
			AssertEquals(false, billOfLading.ShouldShowCustomsDetail);
		}

		[UseDummyCustomsMessageStatusProvider]
		public void TestCustomsMessage()
		{
			BillOfLading billOfLading = Factory.New<BillOfLading>();
			DummyCustomsMessageStatusProvider.Instance.CustomsStatus = "Status1";
			AssertEquals("Status1", billOfLading.CustomsStatus);
			DummyCustomsMessageStatusProvider.Instance.CustomsStatus = "Status2";
			AssertEquals("Status2", billOfLading.CustomsStatus);
		}

		[UseDummyCustomsMessageStatusProvider]
		public void TestMessageStatus()
		{
			BillOfLading billOfLading = Factory.New<BillOfLading>();
			DummyCustomsMessageStatusProvider.Instance.MessageStatus = "Status1";
			AssertEquals("Status1", billOfLading.MessageStatus);
			DummyCustomsMessageStatusProvider.Instance.MessageStatus = "Status2";
			AssertEquals("Status2", billOfLading.MessageStatus);
		}

		[UseDummyCustomsMessageStatusProvider]
		public void TestUserFriendlyStatusMessage()
		{
			BillOfLading billOfLading = Factory.New<BillOfLading>();
			DummyCustomsMessageStatusProvider.Instance.UserFriendlyStatusMessage = "Message1";
			AssertEquals("Message1", billOfLading.UserFriendlyStatusMessage);
			DummyCustomsMessageStatusProvider.Instance.UserFriendlyStatusMessage = "Message2";
			AssertEquals("Message2", billOfLading.UserFriendlyStatusMessage);
		}

		public void TestJS_ShipmentStatus_ReadOnly()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var bill = Factory.NewWithValidTestData<BillOfLading>();
				bill.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicShippingInstruction;
				bill.Logs.AddNew(Events.StatusUpdated, "|NEW=ESI|TYP=Shipment Status");
				Factory.Save();

				Assert(!bill.JS_ShipmentStatus_ReadOnly);
				Assert(bill.IsReceivedElectronicShippingInstruction());

				bill.JS_ShipmentStatus = ShipmentStatusList.Codes.SIRejected;
				Assert(bill.JS_ShipmentStatus_ReadOnly);

				bill.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
				Assert(bill.JS_ShipmentStatus_ReadOnly);
			}
		}

		public void TestGetAddressBookSelection()
		{
			BillOfLading billOfLading = Factory.New<BillOfLading>();
			AddressBookSelection selection = ((ISendEmailSource)billOfLading).GetAddressBookSelection();
			AssertEquals(0, selection.Recipients.Count);
			OrgHeader notifyParty = Factory.New<OrgHeader>();
			OrgContact notifyPartyContact = notifyParty.Contacts.AddNew();
			notifyPartyContact.OC_ContactName = "notifyPartyContact";
			OrgHeader notifyParty2 = Factory.New<OrgHeader>();
			OrgContact notifyParty2Contact = notifyParty2.Contacts.AddNew();
			notifyParty2Contact.OC_ContactName = "notifyParty2";
			OrgHeader notifyParty3 = Factory.New<OrgHeader>();
			OrgContact notifyParty3Contact = notifyParty3.Contacts.AddNew();
			notifyParty3Contact.OC_ContactName = "notifyParty3";
			billOfLading.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;
			billOfLading.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;
			billOfLading.NotifyParty3DocumentaryAddress.E2_OA_Address = notifyParty3.MainAddress.PK;
			selection = ((ISendEmailSource)billOfLading).GetAddressBookSelection();
			AssertEquals(3, selection.Recipients.Count);
			AssertEquals(notifyPartyContact.OC_ContactName, selection.Recipients[0].Name);
			AssertEquals(notifyParty2Contact.OC_ContactName, selection.Recipients[1].Name);
			AssertEquals(notifyParty3Contact.OC_ContactName, selection.Recipients[2].Name);
		}

		public void TestEmailSubject()
		{
			BillOfLading billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_UniqueConsignRef = "V00001";
			String emailSubject = ((ISendEmailSource)billOfLading).EmailSubject;
			AssertEquals("Bill Of Lading - V00001", emailSubject);
		}

		public void TestDocumentSupporterReturnsCorrectBusinessContext()
		{
			BillOfLading shipment = Factory.New<BillOfLading>();
			AssertEquals("DocumentSupporter should be of the correct type", typeof(AgencyShipmentDocumentSupporter), shipment.DocumentSupporter.GetType());
			AssertEquals("DocumentSupporter BusinessContext", BusinessContext.AgencyDocumentation, shipment.DocumentSupporter.BusinessContext);
		}

		public void TestGetTheSameBOsWhenReferingBackToSelfFromThePackLine()
		{
			BillOfLading shipment = Factory.New<BillOfLading>();
			AgencyShipmentPackLine packLine = shipment.OuterPackLines.AddNew();
			AssertEquals("Should at the very least have the same PK", shipment.PK, packLine.Shipment.PK);
			AssertSame("need the same Shipment instance", shipment, packLine.Shipment);
			AssertSame("need the same collection instance", shipment.RealContainers, packLine.Shipment.RealContainers);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("JS_IsShipping must be set.", true, Shipment.JS_IsShipping);
			AssertEquals("JS_ShipmentStatus must be Confirmed.", ShipmentStatusList.Codes.Confirmed, Shipment.JS_ShipmentStatus);
			AssertEquals("JS_IsBooking must not be set.", false, Shipment.JS_IsBooking);
			AssertEquals("JS_IsForwardRegistered must not be set.", false, Shipment.JS_IsForwardRegistered);
			AssertEquals("JS_IsCFSRegistered must not be set.", false, Shipment.JS_IsCFSRegistered);
		}

		public void TestReadOnlyIfCanceled()
		{
			AssertEquals("Shipment.ReadOnly", false, Shipment.ReadOnly);
			Shipment.JS_IsCancelled = true;
			Factory.Save();
			AssertEquals("Shipment.ReadOnly", true, Shipment.ReadOnly);
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BillOfLading shipment2 = factory2.Load<BillOfLading>(Shipment.PK);
			AssertEquals("Shipment2.ReadOnly", true, shipment2.ReadOnly);
		}

		public void TestNotReadOnlyIfConfirmed()
		{
			AssertEquals("Shipment.ReadOnly", false, Shipment.ReadOnly);
			Factory.Save();
			AssertEquals("Shipment.ReadOnly", false, Shipment.ReadOnly);
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BillOfLading shipment2 = factory2.Load<BillOfLading>(Shipment.PK);
			AssertEquals("Shipment2.Readonly", false, shipment2.ReadOnly);
		}

		[TestDate(2014, 12, 1)]
		public void TestRealContainersMovementEvents()
		{
			var today = DateTime.Today;
			var depot = Factory.NewWithValidTestData<OrgHeader>();
			depot.MainAddress.OA_RL_NKRelatedPortCode = "NLAMS";
			var validMovementCode = AgencyShipmentContainerMovementEventSynchroniser.MovementTypeCodeEventMap.Keys.First();
			var correspondingEventCode = AgencyShipmentContainerMovementEventSynchroniser.MovementTypeCodeEventMap[validMovementCode];
			RefContainerStock stock1 = Factory.NewWithValidTestData<RefContainerStock>();
			stock1.R6_ContainerNum = "TEST4100013";
			stock1.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			JobVoyage voyage1 = Factory.NewWithValidTestData<JobVoyage>();
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage1.GenerateSailings();
			JobVoyage voyage2 = Factory.NewWithValidTestData<JobVoyage>();
			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage2.GenerateSailings();
			ContainerMovement movement1 = stock1.Movements.AddNew();
			movement1.E9_JV = voyage1.PK;
			movement1.E9_MovementDate = today.AddDays(-10);
			movement1.E9_OA_Depot = depot.MainAddress.PK;
			movement1.E9_MovementType = validMovementCode;
			ContainerMovement movement2 = stock1.Movements.AddNew();
			movement2.E9_JV = voyage2.PK;
			movement2.E9_MovementDate = today.AddDays(-20);
			movement2.E9_OA_Depot = depot.MainAddress.PK;
			movement2.E9_MovementType = validMovementCode;
			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_JX = voyage1.Sailings[0].PK;
			var container = billOfLading.RealContainers.AddNew();
			container.JC_ContainerNum = stock1.R6_ContainerNum;
			Factory.Save();
			var filter = new ZQuery();
			filter.AddToFilter(StmALogSchema.SL_EventTime, movement1.E9_MovementDate);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, correspondingEventCode);
			AssertEquals(1, container.Logs.Find(filter).Length);
			billOfLading.JS_JX = voyage2.Sailings[0].PK;
			Factory.Save();
			filter = new ZQuery();
			filter.AddToFilter(StmALogSchema.SL_EventTime, movement1.E9_MovementDate);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, correspondingEventCode);
			var log = container.Logs.Find(filter).Single();
			Assert(log.IsCancelled);
			filter = new ZQuery();
			filter.AddToFilter(StmALogSchema.SL_EventTime, movement2.E9_MovementDate);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, correspondingEventCode);
			AssertEquals(1, container.Logs.Find(filter).Length);
			JobVoyage voyage3 = Factory.New<JobVoyage>();
			voyage3.JV_RV_NKVessel = RefVessel.LookupVesselByName("CONDOR", Factory).First().RV_FK;
			voyage3.JV_VoyageFlight = "012";
			voyage3.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage3.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage3.GenerateSailings();
			ContainerMovement movement3 = stock1.Movements.AddNew();
			movement3.E9_JV = voyage3.PK;
			movement3.E9_MovementDate = today.AddDays(-30);
			movement3.E9_OA_Depot = depot.MainAddress.PK;
			movement3.E9_MovementType = validMovementCode;
			Factory.Save();
			filter = new ZQuery();
			filter.AddToFilter(StmALogSchema.SL_EventTime, movement3.E9_MovementDate);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, correspondingEventCode);
			AssertEquals(0, container.Logs.Find(filter).Length);
			var transport = billOfLading.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_IsLinked = true;
			transport.JW_Vessel = "CONDOR";
			transport.JW_VoyageFlight = "012";
			transport.JW_RL_NKLoadPort = "AUBNE";
			transport.JW_RL_NKDiscPort = "SGSIN";
			Factory.Save();
			AssertEquals(1, container.Logs.Find(filter).Length);
		}

		public void TestIModuleToModuleMembers()
		{
			Shipment.JS_HouseBill = "7906423001";
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "Consignor";
			Shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "Consignee";
			Shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			Shipment.JS_CFSReference = "100002301";
			var iModuleToModule = (IModuleToModule)Shipment;
			AssertNull(iModuleToModule.GetRelatedObject());
			AssertEquals(GlbBranch.CurrentBranch.OrgProxy, iModuleToModule.RecipientOrganisation);
			var error = ZString.Empty;
			Assert(iModuleToModule.CanExportData(out error));
		}

		public void TestNoteTypesCore()
		{
			BillOfLading bol = Factory.New<BillOfLading>();
			Assert(((IList)bol.NoteTypes).Contains(PredefinedNoteTypes.Instance.AdditionalBillClauses));
		}

		public void TestUseNewFormBuilderBillOfLading()
		{
			var bol = Factory.New<BillOfLading>();

			using (AgencyRegistry.Instance.UseNewFormBuilderBillOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(bol.UseNewFormBuilderBillOfLading);
			}

			using (AgencyRegistry.Instance.UseNewFormBuilderBillOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert(!bol.UseNewFormBuilderBillOfLading);
			}
		}

		#region TestAddresses
		public void TestAddresses()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "TESTCNR";
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "TESTCNE";
			Factory.Save();
			var bol = Factory.New<BillOfLading>();
			bol.ConsignorPK = consignor.PK;
			bol.ConsigneePK = consignee.PK;
			var cnePickupDeliveryAddress = bol.DocAddresses.FindDocAddressesByType(DocAddressType.ConsignorPickupDeliveryAddress);
			var cnrPickupDeliveryAddress = bol.DocAddresses.FindDocAddressesByType(DocAddressType.ConsigneePickupDeliveryAddress);
			AssertEquals("Should not have Consignee Pickup/Delivery Address", 0, cnePickupDeliveryAddress.Length);
			AssertEquals("Should not have Consignor Pickup/Delivery Address", 0, cnrPickupDeliveryAddress.Length);
		}

		#endregion
		#region Confirm
		public void TestConfirm_ExistingWorkflowItemsWouldBeDeleted()
		{
			var shipment = Factory.New<BillOfLading>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			var existingProcessTasks = new[] { shipment.WorkflowItems.Tasks.AddNew(), shipment.WorkflowItems.Milestones.AddNew(), shipment.WorkflowItems.Triggers.AddNew() };
			AssertEquals("Precondition: not in BOL stage", false, shipment.IsBillOfLadingStage);
			AssertEquals("Precondition: process tasks not deleted", true, existingProcessTasks.All(t => !t.IsDeleted));
			shipment.Confirm();
			AssertEquals("All existing process tasks were deleted", 0, shipment.WorkflowItems.Count);
			AssertEquals("All existing process tasks were deleted", true, existingProcessTasks.All(t => t.IsDeleted));
		}

		public void TestConfirm_PropagationIsEnforcedOnlyOnFirstSave()
		{
			var shipment = Factory.New<BillOfLading>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			var container = shipment.RealContainers.AddNew();
			container.Logs.AddNew(Events.Manifested);
			Factory.Save();
			shipment.Confirm();
			Factory.Save();
			var milestone = shipment.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.ManifestedCode;
			milestone.P9_RespondToCascadedEvents = true;
			milestone.ProcessTaskNotifications.AddNew();
			milestone.P9_ActualDateForBinding = new ZDateTimeOffset(ZDateTime.Empty);
			shipment.JS_GoodsDescription = "Changing BO itself to trigger save";
			Factory.Save();
			AssertEquals("Propagation should not be enforced again", ZDateTime.Empty, milestone.P9_ActualDate.ToZDateTime());
			var anotherFactory = new BusinessObjectFactory();
			var billOfLadingReloaded = anotherFactory.Load<BillOfLading>(shipment.PK);
			billOfLadingReloaded.JS_GoodsDescription = "One more time on reloaded BOL";
			var shipmentTaskReloaded = anotherFactory.Load<ProcessTask>(milestone.PK);
			anotherFactory.Save();
			AssertEquals("Propagation should not be enforced again", ZDateTime.Empty, shipmentTaskReloaded.P9_ActualDate.ToZDateTime());
		}

		[TestDate(2012, 12, 21)]
		public void TestConfirm_RealContainersEventsArePropagatedOnFirstSave()
		{
			var shipment = Factory.New<BillOfLading>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			Func<Event[], AgencyShipmentContainer> addContainerWithEvents = (eventsToAdd) =>
			{
				var realContainer = shipment.RealContainers.AddNew();
				foreach (Event eventToAdd in eventsToAdd)
				{
					realContainer.Logs.AddNew(eventToAdd);
				}

				Factory.Save();
				return realContainer;
			};
			addContainerWithEvents(new[] { Events.Manifested, Events.PickedUp });
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(10);
			DateTime expectedPropagatedEventDate = TestDateAttribute.Date;
			addContainerWithEvents(new[] { Events.Manifested, Events.PickedUp });
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-5);
			var container = addContainerWithEvents(new[] { Events.Manifested });
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(20);
			container.Logs.AddNew(Events.Manifested, TestDateAttribute.Date, isEstimate: true);
			Factory.Save();
			shipment.Confirm();
			Func<string, ProcessTask> addShipmentWorkflowItem = (eventCode) =>
			{
				var workflowItem = shipment.WorkflowItems.AddNew();
				workflowItem.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
				workflowItem.TriggerConditions.TriggerEventCode = eventCode;
				workflowItem.P9_RespondToCascadedEvents = true;
				workflowItem.ProcessTaskNotifications.AddNew();
				return workflowItem;
			};
			var shipmentTask1 = addShipmentWorkflowItem(Events.ManifestedCode);
			var shipmentTask2 = addShipmentWorkflowItem(Events.PickedUpCode);
			AssertEquals("Precondition: event on shipment is not fired", ZDateTime.Empty, shipmentTask1.P9_ActualDate.ToZDateTime());
			AssertEquals("Precondition: event on shipment is not fired", ZDateTime.Empty, shipmentTask2.P9_ActualDate.ToZDateTime());
			Factory.Save();
			AssertEquals("Event was propagated to shipment", expectedPropagatedEventDate, shipmentTask1.P9_ActualDate.ToZDateTime());
			AssertEquals("Event was not propagated as some containers does not have it", ZDateTime.Empty, shipmentTask2.P9_ActualDate.ToZDateTime());
		}

		public void TestConfirm_BookedContainersEventsAreNotPropagated()
		{
			var shipment = Factory.New<BillOfLading>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			var bookedContainer = shipment.BookedContainers.AddNew();
			bookedContainer.Logs.AddNew(Events.Manifested);
			Factory.Save();
			shipment.Confirm();
			var shipmentTask = shipment.WorkflowItems.AddNew();
			shipmentTask.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			shipmentTask.TriggerConditions.TriggerEventCode = Events.ManifestedCode;
			shipmentTask.P9_RespondToCascadedEvents = true;
			shipmentTask.ProcessTaskNotifications.AddNew();
			Factory.Save();
			AssertEquals("Event was not propagated to shipment", ZDateTime.Empty, shipmentTask.P9_ActualDate.ToZDateTime());
		}

		#endregion
		#region Freight Load/Unload Events Special Cascading
		public void TestOnSaving_TopLevelPacks_CascadeFreshFreightLoadedUnloadedEvents()
		{
			var billOfLading = Factory.NewWithValidTestData<BillOfLading>();
			billOfLading.JS_PackingMode = Constants.ContainerModes.BreakBulk;
			var container1 = billOfLading.RealContainers.AddNew();
			var container2 = billOfLading.RealContainers.AddNew();
			container2.JC_FCLOnBoardVessel = ZDateTime.Now.AddDays(-1);
			container2.JC_FCLUnloadFromVessel = ZDateTime.Now;
			billOfLading.Logs.AddNew(Events.FreightLoaded, "BOL event");
			billOfLading.Logs.AddNew(Events.FreightUnloaded, "BOL event");
			Factory.Save();
			Action<BusinessObject, Event, bool> assertHasCascadedEvent = (target, freightEvent, shouldHaveEvent) =>
			{
				var eventLog = target.GetLogs().MostRecentLogByEventTime(freightEvent, "BOL event");
				AssertEquals(shouldHaveEvent, eventLog != null);
			};
			assertHasCascadedEvent(container1, Events.FreightLoaded, true);
			assertHasCascadedEvent(container1, Events.FreightUnloaded, true);
			assertHasCascadedEvent(container2, Events.FreightLoaded, false);
			assertHasCascadedEvent(container2, Events.FreightUnloaded, true);
		}

		public void TestOnSaving_FCL_DoNotCascadeFreshFreightLoadedUnloadedEvents()
		{
			var billOfLading = Factory.NewWithValidTestData<BillOfLading>();
			billOfLading.JS_PackingMode = Constants.ContainerModes.FCL;
			var container1 = billOfLading.RealContainers.AddNew();
			billOfLading.Logs.AddNew(Events.FreightLoaded, "BOL event");
			billOfLading.Logs.AddNew(Events.FreightUnloaded, "BOL event");
			Factory.Save();
			Action<BusinessObject, Event, bool> assertHasCascadedEvent = (target, freightEvent, shouldHaveEvent) =>
			{
				var eventLog = target.GetLogs().MostRecentLogByEventTime(freightEvent, "BOL event");
				AssertEquals(shouldHaveEvent, eventLog != null);
			};
			assertHasCascadedEvent(container1, Events.FreightLoaded, false);
			assertHasCascadedEvent(container1, Events.FreightUnloaded, false);
		}

		public void TestGetCustomBusinessObject()
		{
			var workflowTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.BillOfLadingWorkflowDescriptorCode);
			AddCustomField(workflowTemplate, "stringField", AddOnColumnDataType.Codes.String);
			AddCustomField(workflowTemplate, "intField", AddOnColumnDataType.Codes.Integer);
			AddCustomField(workflowTemplate, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
			AddCustomField(workflowTemplate, "boolField", AddOnColumnDataType.Codes.Boolean);
			Factory.Save();
			var billOfLadingProvider = (ICustomFieldProvider)Shipment;
			var billOfLadingProviderCustomBizo = billOfLadingProvider.GetCustomBusinessObject();
			var billOfLadingDynamicBizo = (IDynamicBusinessObject)billOfLadingProviderCustomBizo;
			AssertNotNull(billOfLadingDynamicBizo.GetProperty("__STRINGFIELD__prop__ZString"));
			AssertNotNull(billOfLadingDynamicBizo.GetProperty("__INTFIELD__prop__ZInt"));
			AssertNotNull(billOfLadingDynamicBizo.GetProperty("__DATETIMEFIELD__prop__ZDateTime"));
			AssertNotNull(billOfLadingDynamicBizo.GetProperty("__BOOLFIELD__prop__ZBool"));
		}

		#region Custom Fields Test

		[TestedType(typeof(BillOfLading))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
		}

		#endregion

		public GenCustomColumnDefinition AddCustomField(ProcessTaskTemplate template, ZString name, ZString addOnColumnDataTypeCode)
		{
			var result = template.GenCustomColumnDefinitions.AddNew();
			result.XC_Name = name;
			result.XC_Type = addOnColumnDataTypeCode;
			return result;
		}

		#endregion
		#region Implementation
		BillOfLading Shipment
		{
			get
			{
				return shipment ?? (shipment = Factory.New<BillOfLading>());
			}
		}

		BillOfLading shipment;
		JobVoyage Voyage;
		VoyageOrigin Origin1;
		VoyageOrigin Origin2;
		VoyageOrigin Origin3;
		VoyageDestination Destination1;
		VoyageDestination Destination2;
		VoyageDestination Destination3;
		JobSailing Sailing4;
		JobVoyage OtherVoyage;
		VoyageOrigin OtherOrigin1;
		JobSailing OtherSailing1;
		public void SetSailings()
		{
			Voyage = Factory.New<JobVoyage>();
			Origin1 = Voyage.Origins.AddNew();
			Origin1.JA_RL_NKPortOfLoading = "AUSYD";
			Origin1.JA_E_DEP = ZDateTime.Now.AddDays(10);
			Origin2 = Voyage.Origins.AddNew();
			Origin2.JA_RL_NKPortOfLoading = "AUBNE";
			Origin2.JA_E_DEP = ZDateTime.Now.AddDays(16);
			Origin3 = Voyage.Origins.AddNew();
			Origin3.JA_RL_NKPortOfLoading = "NZAKL";
			Origin3.JA_E_DEP = ZDateTime.Now.AddDays(22);
			Destination1 = Voyage.Destinations.AddNew();
			Destination1.JB_RL_NKPortOfDischarge = "AUBNE";
			Destination1.JB_E_ARV = ZDateTime.Now.AddDays(14);
			Destination2 = Voyage.Destinations.AddNew();
			Destination2.JB_RL_NKPortOfDischarge = "NZAKL";
			Destination2.JB_E_ARV = ZDateTime.Now.AddDays(20);
			Destination3 = Voyage.Destinations.AddNew();
			Destination3.JB_RL_NKPortOfDischarge = "SGSIN";
			Destination3.JB_E_ARV = ZDateTime.Now.AddDays(26);
			Sailing4 = Voyage.Sailings.GetSailingFromLoadAndDischarge(Origin3.JA_RL_NKPortOfLoading, Destination3.JB_RL_NKPortOfDischarge);
			Sailing4.JX_JA = Origin3.PK;
			Sailing4.JX_JB = Destination3.PK;
		}

		public void SetOtherSailing()
		{
			OtherVoyage = Factory.New<JobVoyage>();
			OtherOrigin1 = OtherVoyage.Origins.AddNew();
			OtherOrigin1.JA_RL_NKPortOfLoading = "AUSYD";
			OtherOrigin1.JA_E_DEP = ZDateTime.Now.AddDays(10);
			VoyageDestination destination1 = OtherVoyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AUBNE";
			destination1.JB_E_ARV = ZDateTime.Now.AddDays(14);
			OtherSailing1 = OtherVoyage.Sailings.GetSailingFromLoadAndDischarge(OtherOrigin1.JA_RL_NKPortOfLoading, destination1.JB_RL_NKPortOfDischarge);
			OtherSailing1.JX_JA = OtherOrigin1.PK;
			OtherSailing1.JX_JB = destination1.PK;
		}
		#endregion
	}
}

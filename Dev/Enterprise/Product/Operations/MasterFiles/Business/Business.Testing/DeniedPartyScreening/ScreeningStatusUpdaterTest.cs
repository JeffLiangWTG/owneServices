using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;
using IForwardingConsol = Enterprise.Integration.Forwarding.IForwardingConsol;
using IForwardingShipment = Enterprise.Integration.Forwarding.IForwardingShipment;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class ScreeningStatusUpdaterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestUpdateRelatedJobsByDatesAndBillingsFromOrgHeader_ExpectNoExceptions()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var result = ScreeningStatusUpdater.UpdateRelatedJobsByDatesAndBillings(header);

			Assert(result == 0);
		}

		[ExpectNoExceptions]
		public void TestUpdateRelatedJobsFromOrgHeader_ExpectNoExceptions()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			ScreeningStatusUpdater.UpdateRelatedJobs(header);
		}

		[ExpectNoExceptions]
		public void TestUpdateRelatedJobsFromRefVessel_ExpectNoExceptions()
		{
			RefVessel vessel = Factory.NewWithValidTestData<RefVessel>();
			ScreeningStatusUpdater.UpdateRelatedJobs(vessel);
		}

		public void TestRefreshDeniedPartyStatusUpdatedLog()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var shipmentBizo = shipment as BusinessObject;
			DpsWorkflowTrackingEvent.AddNew(shipmentBizo, ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Matched);
			Factory.Save();
			var latestLog = shipmentBizo.GetLogs().MostRecentLogByEventTime(AutoEvents.DeniedPartyStatusUpdated);
			AssertContains("|NEW=MAT|OLD=UNK|TYP=MAN", latestLog.SL_Reference);
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, shipment.JS_ScreeningStatus);
			ScreeningStatusUpdater.RefreshDeniedPartyStatusUpdatedLog(new List<BusinessObject>() { shipmentBizo });
			latestLog = shipmentBizo.GetLogs().MostRecentLogByEventTime(AutoEvents.DeniedPartyStatusUpdated);
			AssertContains("|NEW=NOT|OLD=MAT|TYP=MAN", latestLog.SL_Reference);
		}

		public void TestRefreshDeniedPartyStatusUpdatedLogWithErrorFormat()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var shipmentBizo = shipment as BusinessObject;
			shipmentBizo.GetLogs().AddNew(AutoEvents.DeniedPartyStatusUpdated);
			Factory.Save();
			var latestLog = shipmentBizo.GetLogs().MostRecentLogByEventTime(AutoEvents.DeniedPartyStatusUpdated);
			AssertEquals(string.Empty, latestLog.SL_Reference);
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, shipment.JS_ScreeningStatus);
			ScreeningStatusUpdater.RefreshDeniedPartyStatusUpdatedLog(new List<BusinessObject>() { shipmentBizo });
			latestLog = shipmentBizo.GetLogs().MostRecentLogByEventTime(AutoEvents.DeniedPartyStatusUpdated);
			AssertContains("|NEW=NOT|TYP=MAN", latestLog.SL_Reference);
		}

		public void TestRefreshDeniedPartyStatusUpdatedLogWithNoPreviousLog()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var shipmentBizo = shipment as BusinessObject;
			Factory.Save();
			var latestLog = shipmentBizo.GetLogs().MostRecentLogByEventTime(AutoEvents.DeniedPartyStatusUpdated);
			AssertNull(latestLog);
			ScreeningStatusUpdater.RefreshDeniedPartyStatusUpdatedLog(new List<BusinessObject>() { shipmentBizo });
			latestLog = shipmentBizo.GetLogs().MostRecentLogByEventTime(AutoEvents.DeniedPartyStatusUpdated);
			AssertContains("|NEW=NOT|TYP=MAN", latestLog.SL_Reference);
		}

		public void TestRefreshDeniedPartyStatusUpdatedLog_NoNeedRefresh()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var shipmentBizo = shipment as BusinessObject;
			DpsWorkflowTrackingEvent.AddNew(shipmentBizo, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.NotScreened);
			Factory.Save();
			var latestLog = shipmentBizo.GetLogs().MostRecentLogByEventTime(AutoEvents.DeniedPartyStatusUpdated);
			AssertContains("|NEW=NOT|OLD=MAT|TYP=MAN", latestLog.SL_Reference);
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, shipment.JS_ScreeningStatus);
			ScreeningStatusUpdater.RefreshDeniedPartyStatusUpdatedLog(new List<BusinessObject>() { shipmentBizo });
			latestLog = shipmentBizo.GetLogs().MostRecentLogByEventTime(AutoEvents.DeniedPartyStatusUpdated);
			AssertContains("|NEW=NOT|OLD=MAT|TYP=MAN", latestLog.SL_Reference);
		}

		public void TestRefreshDeniedPartyStatusUpdatedLog_HandlesZSaveException()
		{
			var bizO = Factory.NewWithValidTestData<OrgHeader>();
			bizO.OH_FullName = "Apple";
			var shipment1 = Factory.New<IForwardingShipment>() as BusinessObject;
			Factory.Save();
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipment2 = factory2.New<IForwardingShipment>() as BusinessObject;
			factory2.Save();

			var factory2BizO = factory2.Load<OrgHeader>(bizO.PK);
			factory2BizO.OH_FullName = "Banana";
			var cmd = Db.Connection.Command($"UPDATE dbo.OrgHeader SET OH_FullName = 'Blueberry' WHERE OH_Code = '{bizO.OH_Code}'");
			cmd.ExecuteNonQuery();

			AssertNoExceptionThrown(() => { ScreeningStatusUpdater.RefreshDeniedPartyStatusUpdatedLog(new List<BusinessObject>() { shipment1, shipment2 }); });
		}

		public void TestRefreshDeniedPartyStatusUpdatedLog_SavedEachFactoryOnlyOnce()
		{
			var bizO = Factory.NewWithValidTestData<OrgHeader>();
			bizO.OH_FullName = "Apple";
			var shipment1 = Factory.New<IForwardingShipment>() as BusinessObject;
			var shipment2 = Factory.New<IForwardingShipment>() as BusinessObject;
			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipment3 = factory2.New<IForwardingShipment>() as BusinessObject;
			var shipment4 = factory2.New<IForwardingShipment>() as BusinessObject;
			factory2.Save();

			var countFactorySavedHits = 0;
			Factory.Saved += (o, e) => countFactorySavedHits++;
			var countFactory2SavedHits = 0;
			factory2.Saved += (o, e) => countFactory2SavedHits++;

			ScreeningStatusUpdater.RefreshDeniedPartyStatusUpdatedLog(new List<BusinessObject>() { shipment1, shipment2, shipment3, shipment4 });
			AssertEquals("Factory was saved 1 time", 1, countFactorySavedHits);
			AssertEquals("Factory2 was saved 1 time", 1, countFactory2SavedHits);
			AssertEquals(false, shipment1.HasChanges);
			AssertEquals(false, shipment2.HasChanges);
			AssertEquals(false, shipment3.HasChanges);
			AssertEquals(false, shipment4.HasChanges);
		}

		public void TestRefreshDeniedPartyStatusUpdatedLog_ShouldReloadStatusFromDatabase()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			DpsWorkflowTrackingEvent.AddNew(header, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Matched);
			Factory.Save();
			AssertEquals("The screening status of current orgHeader is 'MAT'", ScreeningStatusesList.Codes.Matched, header.OH_ScreeningStatus);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var headerInNewFactory = newFactory.Load<OrgHeader>(header.PK);
			headerInNewFactory.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			DpsWorkflowTrackingEvent.AddNew(headerInNewFactory, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Clear);
			newFactory.Save();
			AssertEquals("The screening status of database's orgHeader is 'CLR'", ScreeningStatusesList.Codes.Clear, headerInNewFactory.OH_ScreeningStatus);

			ScreeningStatusUpdater.RefreshDeniedPartyStatusUpdatedLog(new List<BusinessObject>() { header });

			var logFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DeniedPartyStatusUpdated.Code);
			var logs = headerInNewFactory.GetLogs().Find(logFilter).OrderBy(x => x.SL_EventTime).ToList();
			AssertEquals(2, logs.Count);
			AssertContains("|NEW=MAT|OLD=NOT|TYP=MAN", logs[0].SL_Reference);
			AssertContains("|NEW=CLR|OLD=MAT|TYP=MAN", logs[1].SL_Reference);
		}

		public void TestApplyJobClearStatus_HandleConcurrencyError()
		{
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			var shipmentBizo = shipment as BusinessObject;
			var consignorAddress = shipment.ConsignorDocumentaryAddress;
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignorAddress.E2_OA_Address = consignor.MainAddress.PK;

			var customValue = Factory.New<GenCustomAddOnValue>();
			customValue.XV_ParentID = shipment.PK;
			customValue.XV_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			customValue.XV_Name = "OOO";
			customValue.XV_Type = AddOnColumnDataType.Codes.String;
			customValue.XV_Data = "ABC";
			Factory.Save();

			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_GC = GlbCompany.CurrentCompany.PK;
			processTaskTemplate.P0_IsActive = true;
			processTaskTemplate.P0_OH_Client = consignor.PK;
			processTaskTemplate.P0_ProcessType = "SHP";
			processTaskTemplate.P0_RecalculateScheduledDate = true;

			var task = Factory.New<IForwardingShipmentProcessTask>() as ProcessTask;
			var triggerConditions = task as ITriggerConditions;
			task.P9_ParentID = shipment.PK;
			task.P9_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			triggerConditions.TriggerEventCode = AutoEvents.DeniedPartyStatusUpdatedCode;
			task.P9_Type = Constants.Workflow.MilestoneType;
			task.P9_LineTriggerType = string.Empty;
			task.P9_TriggerContext = "DEF";
			triggerConditions.TriggerFiredCountdown = 100;
			task.P9_Type = Constants.Workflow.WorkflowTriggerType;
			task.P9_ParentTemplateID = processTaskTemplate.PK;

			var notification = Factory.NewWithValidTestData<ProcessTaskNotification>();
			notification.PQ_EmailAddr = "GetCustomField(OOO)";
			notification.PQ_EmailText = "ConcurrencyValueForTest";
			notification.PQ_P9 = task.PK;
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;

			var customerColumnDefination = Factory.NewWithValidTestData<GenCustomColumnDefinition>();
			customerColumnDefination.XC_ParentID = processTaskTemplate.PK;
			customerColumnDefination.XC_ParentTableCode = ProcessTaskTemplateSchema.Constants.Prefix;
			customerColumnDefination.XC_Name = "OOO";
			customerColumnDefination.XC_Type = AddOnColumnDataType.Codes.String;
			Factory.Save();

			TestConnection.ExecuteNonQuery("UPDATE dbo.GenCustomAddOnValue SET XV_Data = 'ABCDE', XV_SystemLastEditUser = 'E', XV_SystemLastEditTimeUtc = GetDate() WHERE XV_Name = 'OOO'");

			AssertNoExceptionThrown(() =>
			{
				ScreeningStatusUpdater.ApplyJobClearStatus(shipmentBizo, string.Empty);
				var latestCustomAddOnValue = Factory.Load<GenCustomAddOnValue>(customValue.PK);
				AssertEquals("ABCDE", latestCustomAddOnValue.XV_Data);
				var latestLog = shipmentBizo.GetLogs().MostRecentLogByEventTime(AutoEvents.DeniedPartyStatusUpdated);
				AssertContains("|NEW=JCL|OLD=NOT|TYP=MAN", latestLog.SL_Reference);
			});
		}

		public void TestApplyJobClearStatusOnJobDeclaration()
		{
			var forwardingConsol = Factory.New<IBaseJobDeclaration>();
			forwardingConsol.JE_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			Factory.Save();
			AssertJobClearStatusAndReason(forwardingConsol as BusinessObject, "Dummy Reason");
		}

		public void TestApplyJobClearStatusOnConsol()
		{
			var forwardingConsol = Factory.New<IForwardingConsol>();
			forwardingConsol.JK_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			Factory.Save();
			AssertJobClearStatusAndReason(forwardingConsol as BusinessObject, string.Empty);
		}

		public void TestApplyJobClearStatusOnConsolWithRelatedShipment()
		{
			var consol = Factory.New<IForwardingConsol>();
			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			var consolRelatedShipment = Factory.New<IForwardingShipment>();
			consolRelatedShipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			consolRelatedShipment.JS_IsForwardRegistered = true;
			consol.AddShipment(consolRelatedShipment);
			(consol as IShouldUpdateScreeningStatus).ShouldUpdateScreeningStatus = false;
			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.NotScreened, (consol as IScreeningStatusProvider)?.ScreeningStatus);
			AssertEquals(ScreeningStatusesList.Codes.Clear, (consolRelatedShipment as IScreeningStatusProvider)?.ScreeningStatus);

			consolRelatedShipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			AssertJobClearStatusAndReason(consol as BusinessObject);
			AssertSubShipmentsJobClearStatusAndReason(consolRelatedShipment as BusinessObject);
		}

		public void TestApplyJobClearStatusOnConsolWith2LevelSubShipments()
		{
			var consol = Factory.New<IForwardingConsol>();
			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			var consolRelatedShipment = Factory.New<IForwardingShipment>();
			consolRelatedShipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			consolRelatedShipment.JS_ShipmentType = ShipmentTypes.CoLoadMaster;
			consolRelatedShipment.JS_IsForwardRegistered = true;
			consol.AddShipment(consolRelatedShipment);

			var shipmentRelatedShipment = Factory.New<IForwardingShipment>();
			shipmentRelatedShipment.JS_ShipmentType = ShipmentTypes.CoLoadMaster;
			shipmentRelatedShipment.JS_JS_ColoadMasterShipment = consolRelatedShipment.PK;
			shipmentRelatedShipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

			var shipmentRelatedDeliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			shipmentRelatedDeliveryAgent.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			shipmentRelatedShipment.JS_OH_DeliveryAgent = shipmentRelatedDeliveryAgent.PK;
			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.NotScreened, (consol as IScreeningStatusProvider)?.ScreeningStatus);
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, (consolRelatedShipment as IScreeningStatusProvider)?.ScreeningStatus);
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, (shipmentRelatedShipment as IScreeningStatusProvider)?.ScreeningStatus);

			AssertJobClearStatusAndReason(consol as BusinessObject, "I AM AUTHORISED TO CLEAR THIS JOB");

			AssertSubShipmentsJobClearStatusAndReason(consolRelatedShipment as BusinessObject, "I AM AUTHORISED TO CLEAR THIS JOB");
			AssertSubShipmentsJobClearStatusAndReason(shipmentRelatedShipment as BusinessObject, "I AM AUTHORISED TO CLEAR THIS JOB", true);
		}

		public void TestApplyJobClearStatusOnShipment()
		{
			var forwardingShipment = Factory.New<IForwardingShipment>();
			forwardingShipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			Factory.Save();
			AssertJobClearStatusAndReason(forwardingShipment as BusinessObject);
		}

		public void TestApplyJobClearStatusWithRelatedJobConsols()
		{
			var forwardingShipment = Factory.New<IForwardingShipment>();
			forwardingShipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			var shipmentRelatedConsol = Factory.New<IForwardingConsol>();
			shipmentRelatedConsol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			var jobConShipLink = Factory.New<IJobConShipLink>();
			jobConShipLink.JN_JK = shipmentRelatedConsol.PK;
			jobConShipLink.JN_JS = forwardingShipment.PK;
			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.NotScreened, (forwardingShipment as IScreeningStatusProvider)?.ScreeningStatus);
			AssertEquals(ScreeningStatusesList.Codes.Clear, (shipmentRelatedConsol as IScreeningStatusProvider)?.ScreeningStatus);

			shipmentRelatedConsol.JK_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			var consolRelatedTransport = shipmentRelatedConsol.Transports_Get(0);
			(consolRelatedTransport as INeedRow).Row[JobConsolTransportSchema.Constants.JW_TransportMode] = TransportModes.Sea;
			consolRelatedTransport.JW_Vessel = "TestCode";
			consolRelatedTransport.JW_VesselScreeningStatus = ScreeningStatusesList.Codes.Matched;

			AssertJobClearStatusAndReason(forwardingShipment as BusinessObject);
			AssertEquals(ScreeningStatusesList.Codes.Matched, (shipmentRelatedConsol as IScreeningStatusProvider)?.ScreeningStatus);
		}

		public void TestApplyJobClearStatusToShipmentWithRelatedSubShipments()
		{
			var forwardingShipment = Factory.New<IForwardingShipment>();
			forwardingShipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			forwardingShipment.JS_ShipmentType = ShipmentTypes.CoLoadMaster;

			var shipmentRelatedShipment = Factory.New<IForwardingShipment>();
			shipmentRelatedShipment.JS_JS_ColoadMasterShipment = forwardingShipment.PK;

			var shipmentRelatedDeliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			shipmentRelatedDeliveryAgent.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			shipmentRelatedShipment.JS_OH_DeliveryAgent = shipmentRelatedDeliveryAgent.PK;

			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.NotScreened, (forwardingShipment as IScreeningStatusProvider)?.ScreeningStatus);
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, (shipmentRelatedShipment as IScreeningStatusProvider)?.ScreeningStatus);

			shipmentRelatedDeliveryAgent.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			AssertJobClearStatusAndReason(forwardingShipment as BusinessObject);
			AssertSubShipmentsJobClearStatusAndReason(shipmentRelatedShipment as BusinessObject, "", true);
		}

		public void TestApplyJobClearStatusToShipmentWith2LevelSubShipments()
		{
			var forwardingShipment = Factory.New<IForwardingShipment>();
			forwardingShipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			forwardingShipment.JS_ShipmentType = ShipmentTypes.CoLoadMaster;

			var shipmentRelatedShipment = Factory.New<IForwardingShipment>();
			shipmentRelatedShipment.JS_JS_ColoadMasterShipment = forwardingShipment.PK;
			shipmentRelatedShipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			shipmentRelatedShipment.JS_ShipmentType = ShipmentTypes.CoLoadMaster;

			var secondLevelSubShipment = Factory.New<IForwardingShipment>();

			secondLevelSubShipment.JS_JS_ColoadMasterShipment = shipmentRelatedShipment.PK;
			secondLevelSubShipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			secondLevelSubShipment.JS_ShipmentType = ShipmentTypes.CoLoadMaster;
			var shipmentRelatedDeliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			shipmentRelatedDeliveryAgent.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			secondLevelSubShipment.JS_OH_DeliveryAgent = shipmentRelatedDeliveryAgent.PK;

			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.NotScreened, (forwardingShipment as IScreeningStatusProvider)?.ScreeningStatus);
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, (shipmentRelatedShipment as IScreeningStatusProvider)?.ScreeningStatus);
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, (secondLevelSubShipment as IScreeningStatusProvider)?.ScreeningStatus);

			AssertJobClearStatusAndReason(forwardingShipment as BusinessObject);

			AssertSubShipmentsJobClearStatusAndReason(shipmentRelatedShipment as BusinessObject);
			AssertSubShipmentsJobClearStatusAndReason(secondLevelSubShipment as BusinessObject, "", true);
		}

		public void TestApplyJobClearStatusToShipmentWith2LevelSubNotMasterShipments()
		{
			var forwardingShipment = Factory.New<IForwardingShipment>();
			forwardingShipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			forwardingShipment.JS_ShipmentType = ShipmentTypes.CoLoadMaster;
			var topShipmentRelatedDeliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			topShipmentRelatedDeliveryAgent.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			forwardingShipment.JS_OH_DeliveryAgent = topShipmentRelatedDeliveryAgent.PK;

			var shipmentRelatedShipment = Factory.New<IForwardingShipment>();
			shipmentRelatedShipment.JS_JS_ColoadMasterShipment = forwardingShipment.PK;
			shipmentRelatedShipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			var firstLevelshipmentRelatedDeliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			firstLevelshipmentRelatedDeliveryAgent.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			shipmentRelatedShipment.JS_OH_DeliveryAgent = firstLevelshipmentRelatedDeliveryAgent.PK;

			var secondLevelSubShipment = Factory.New<IForwardingShipment>();
			secondLevelSubShipment.JS_JS_ColoadMasterShipment = shipmentRelatedShipment.PK;
			secondLevelSubShipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			secondLevelSubShipment.JS_ShipmentType = ShipmentTypes.CoLoadMaster;
			var shipmentRelatedDeliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			shipmentRelatedDeliveryAgent.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			secondLevelSubShipment.JS_OH_DeliveryAgent = shipmentRelatedDeliveryAgent.PK;

			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.NotScreened, (forwardingShipment as IScreeningStatusProvider)?.ScreeningStatus);
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, (shipmentRelatedShipment as IScreeningStatusProvider)?.ScreeningStatus);
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, (secondLevelSubShipment as IScreeningStatusProvider)?.ScreeningStatus);

			AssertJobClearStatusAndReason(forwardingShipment as BusinessObject, "", true);

			AssertSubShipmentsJobClearStatusAndReason(shipmentRelatedShipment as BusinessObject, "", true);
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, (secondLevelSubShipment as IScreeningStatusProvider)?.ScreeningStatus);
		}

		public void TestApplyJobClearStatusWithRelatedMasterShipment()
		{
			var forwardingShipment = Factory.New<IForwardingShipment>();
			forwardingShipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			forwardingShipment.JS_ShipmentType = ShipmentTypes.CoLoadMaster;

			var shipmentRelatedShipment = Factory.New<IForwardingShipment>();
			shipmentRelatedShipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			shipmentRelatedShipment.JS_JS_ColoadMasterShipment = forwardingShipment.PK;

			var shipmentRelatedDeliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			shipmentRelatedDeliveryAgent.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			forwardingShipment.JS_OH_DeliveryAgent = shipmentRelatedDeliveryAgent.PK;

			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.NotScreened, (forwardingShipment as IScreeningStatusProvider)?.ScreeningStatus);
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, (shipmentRelatedShipment as IScreeningStatusProvider)?.ScreeningStatus);

			shipmentRelatedDeliveryAgent.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();

			AssertJobClearStatusAndReason(shipmentRelatedShipment as BusinessObject);
			AssertEquals(ScreeningStatusesList.Codes.Matched, (forwardingShipment as IScreeningStatusProvider)?.ScreeningStatus);
		}

		public void TestWorkflowTrackingEvent_WithAddNew()
		{
			AssertDisplayEventReference(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.NotScreened, "New Status: Not Screened, Old Status: Clear, Type: Manual Screen", ScreeningType.Manual, string.Empty);
			AssertDisplayEventReference(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Matched, "New Status: Matched, Old Status: Matched, Type: Manual Screen", ScreeningType.Manual, string.Empty);
			AssertDisplayEventReference(ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Clear, "New Status: Clear, Old Status: Not Screened, Type: Manual Screen", ScreeningType.Manual, string.Empty);
			AssertDisplayEventReference(ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.Unknown, "New Status: Unknown, Old Status: Job Cleared, Type: Manual Screen", ScreeningType.Manual, string.Empty);
			AssertDisplayEventReference(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.JobCleared, "New Status: Job Cleared, Old Status: Clear, Type: Manual Screen", ScreeningType.Manual, string.Empty);
			AssertDisplayEventReference(ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.PermanentClear, "New Status: Permanent Clear, Old Status: Job Cleared, Type: Manual Screen", ScreeningType.Manual, string.Empty);

			AssertDisplayEventReference(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.NotScreened, "New Status: Not Screened, Old Status: Clear, Type: Re-Screen Advice", ScreeningType.RescreenAdvice, string.Empty);
			AssertDisplayEventReference(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Matched, "New Status: Matched, Old Status: Matched, Type: Re-Screen Advice", ScreeningType.RescreenAdvice, string.Empty);
			AssertDisplayEventReference(ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Clear, "New Status: Clear, Old Status: Not Screened, Type: Re-Screen Advice", ScreeningType.RescreenAdvice, string.Empty);
			AssertDisplayEventReference(ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.Unknown, "New Status: Unknown, Old Status: Job Cleared, Type: Re-Screen Advice", ScreeningType.RescreenAdvice, string.Empty);
			AssertDisplayEventReference(ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.PermanentClear, "New Status: Permanent Clear, Old Status: Job Cleared, Type: Re-Screen Advice", ScreeningType.RescreenAdvice, string.Empty);
			AssertDisplayEventReference(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.JobCleared, "New Status: Job Cleared, Old Status: Clear, Type: Re-Screen Advice", ScreeningType.RescreenAdvice, string.Empty);
			AssertDisplayEventReference(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.JobCleared, "New Status: Job Cleared, Old Status: Clear, Type: Re-Screen Advice", ScreeningType.RescreenAdvice, null);
			AssertDisplayEventReference(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.JobCleared, "New Status: Job Cleared, Old Status: Clear, Type: Re-Screen Advice", ScreeningType.RescreenAdvice, string.Empty);

			AssertDisplayEventReference(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.JobCleared, "New Status: Job Cleared, Old Status: Clear", null, "50");
		}

		public void TestWorkflowTrackingEvent_WithAddNewRescreenAdvice()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			DpsWorkflowTrackingEvent.AddNew(org, ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.NotScreened, null, ScreeningType.RescreenAdvice, null);
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeniedPartyStatusUpdatedCode);
			var result = org.GetLogs().Find(query);
			AssertContains("NEW=NOT|OLD=CLR|TYP=RSA", result.Single().SL_Reference);
		}

		public void TestGetWorstScreeningStatus_ForSelfStatues_False()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			var creditorAddress = Factory.NewWithValidTestData<OrgAddress>();
			creditorAddress.OA_OH = creditor.PK;
			var consol = Factory.New<IForwardingConsol>();
			consol.JK_OA_CreditorAddress = creditorAddress.PK;
			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;

			var parties = new IScreeningPartyProvider[] { consol as IScreeningPartyProvider };
			var result = ScreeningStatusUpdater.GetWorstScreeningStatus(parties);
			AssertEquals(ScreeningStatusesList.Codes.Matched, result);

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			result = ScreeningStatusUpdater.GetWorstScreeningStatus(parties);
			AssertEquals(ScreeningStatusesList.Codes.Matched, result);

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			result = ScreeningStatusUpdater.GetWorstScreeningStatus(parties);
			AssertEquals(ScreeningStatusesList.Codes.JobCleared, result);

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			result = ScreeningStatusUpdater.GetWorstScreeningStatus(parties);
			AssertEquals(ScreeningStatusesList.Codes.Matched, result);

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			result = ScreeningStatusUpdater.GetWorstScreeningStatus(parties);
			AssertEquals(ScreeningStatusesList.Codes.Matched, result);

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			result = ScreeningStatusUpdater.GetWorstScreeningStatus(parties);
			AssertEquals(ScreeningStatusesList.Codes.Matched, result);
		}

		public void TestUpdateJobStatusFromItsScreeningParties_ForSelfStatues_False()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			var creditorAddress = Factory.NewWithValidTestData<OrgAddress>();
			creditorAddress.OA_OH = creditor.PK;
			var consol = Factory.New<IForwardingConsol>();
			consol.JK_OA_CreditorAddress = creditorAddress.PK;
			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;

			var provider = consol as IScreeningPartyProvider;
			var result = ScreeningStatusUpdater.UpdateJobStatusFromItsScreeningParties(provider);
			AssertEquals(ScreeningStatusesList.Codes.Matched, result);

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			result = ScreeningStatusUpdater.UpdateJobStatusFromItsScreeningParties(provider);
			AssertEquals(ScreeningStatusesList.Codes.Matched, result);

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			result = ScreeningStatusUpdater.UpdateJobStatusFromItsScreeningParties(provider);
			AssertEquals(ScreeningStatusesList.Codes.Matched, result);

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			result = ScreeningStatusUpdater.UpdateJobStatusFromItsScreeningParties(provider);
			AssertEquals(ScreeningStatusesList.Codes.Matched, result);

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			result = ScreeningStatusUpdater.UpdateJobStatusFromItsScreeningParties(provider);
			AssertEquals(ScreeningStatusesList.Codes.Matched, result);

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			result = ScreeningStatusUpdater.UpdateJobStatusFromItsScreeningParties(provider);
			AssertEquals(ScreeningStatusesList.Codes.Matched, result);
		}

		public void TestScreeningStatusRating()
		{
			CombineAssertions(() =>
			{
				AssertEquals(-1, ScreeningStatusUpdater.ScreeningStatusRating(ScreeningStatusesList.Codes.PermanentClear));
				AssertEquals(0, ScreeningStatusUpdater.ScreeningStatusRating(ScreeningStatusesList.Codes.Clear));
				AssertEquals(1, ScreeningStatusUpdater.ScreeningStatusRating(ScreeningStatusesList.Codes.JobCleared));
				AssertEquals(2, ScreeningStatusUpdater.ScreeningStatusRating(ScreeningStatusesList.Codes.Unknown));
				AssertEquals(3, ScreeningStatusUpdater.ScreeningStatusRating(ScreeningStatusesList.Codes.RequiresReview));
				AssertEquals(4, ScreeningStatusUpdater.ScreeningStatusRating(ScreeningStatusesList.Codes.NotScreened));
				AssertEquals(5, ScreeningStatusUpdater.ScreeningStatusRating(ScreeningStatusesList.Codes.Matched));
				AssertEquals("Default Rating Should Be CLP", -1, ScreeningStatusUpdater.ScreeningStatusRating("Dummy"));
				AssertEquals("Default Rating Should Be CLP", -1, ScreeningStatusUpdater.ScreeningStatusRating(string.Empty));
			});
		}

		public void TestGetWorstScreeningStatus()
		{
			var dummyProvider = new DummyScreeningPartyProvider();
			dummyProvider.DummyStatusList = new List<ZString>() { ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.Unknown };
			AssertEquals(ScreeningStatusesList.Codes.Unknown, dummyProvider.GetWorstScreeningStatus());

			dummyProvider.DummyStatusList = new List<ZString>() { ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.PermanentClear, ScreeningStatusesList.Codes.JobCleared };
			AssertEquals(ScreeningStatusesList.Codes.JobCleared, dummyProvider.GetWorstScreeningStatus());

			dummyProvider.DummyStatusList = new List<ZString>() { ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.PermanentClear, ScreeningStatusesList.Codes.Matched };
			AssertEquals(ScreeningStatusesList.Codes.Matched, dummyProvider.GetWorstScreeningStatus());

			dummyProvider.DummyStatusList = new List<ZString>() { ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.PermanentClear };
			AssertEquals(ScreeningStatusesList.Codes.Clear, dummyProvider.GetWorstScreeningStatus());

			dummyProvider.DummyStatusList = new List<ZString>() { ScreeningStatusesList.Codes.PermanentClear };
			AssertEquals(ScreeningStatusesList.Codes.PermanentClear, dummyProvider.GetWorstScreeningStatus());

			dummyProvider.DummyStatusList = null;
			dummyProvider.ScreeningPartyProviders = new List<IScreeningPartyProvider>() {
				new DummyScreeningPartyProvider()
				{
					DummyStatusList = new List<ZString>() { ScreeningStatusesList.Codes.PermanentClear }
				},
				new DummyScreeningPartyProvider()
				{
					DummyStatusList = new List<ZString>() { ScreeningStatusesList.Codes.PermanentClear }
				}
			};
			AssertEquals(ScreeningStatusesList.Codes.PermanentClear, dummyProvider.GetWorstScreeningStatus());

			dummyProvider.ScreeningPartyProviders = new List<IScreeningPartyProvider>() {
				new DummyScreeningPartyProvider()
				{
					DummyStatusList = new List<ZString>() { ScreeningStatusesList.Codes.PermanentClear }
				},
				new DummyScreeningPartyProvider()
				{
					DummyStatusList = new List<ZString>() { ScreeningStatusesList.Codes.PermanentClear, ScreeningStatusesList.Codes.Unknown }
				}
			};
			AssertEquals(ScreeningStatusesList.Codes.Unknown, dummyProvider.GetWorstScreeningStatus());
		}

		public void TestTestGetWorstScreeningStatus_HasEmptyScreeningStatus()
		{
			var dummyProvider = new DummyScreeningPartyProvider();
			dummyProvider.DummyStatusList = new List<ZString>() { string.Empty, ScreeningStatusesList.Codes.PermanentClear };
			AssertEquals("Should Be CLP", ScreeningStatusesList.Codes.PermanentClear, dummyProvider.GetWorstScreeningStatus());

			dummyProvider.DummyStatusList = new List<ZString>() { string.Empty, ScreeningStatusesList.Codes.JobCleared };
			AssertEquals("Should Be JCL", ScreeningStatusesList.Codes.JobCleared, dummyProvider.GetWorstScreeningStatus());

			dummyProvider.DummyStatusList = new List<ZString>() { string.Empty, ScreeningStatusesList.Codes.Clear };
			AssertEquals("Should Be CLR", ScreeningStatusesList.Codes.Clear, dummyProvider.GetWorstScreeningStatus());

			dummyProvider.ScreeningPartyProviders = new List<IScreeningPartyProvider>() {
				new DummyScreeningPartyProvider()
				{
					DummyStatusList = new List<ZString>() { string.Empty, ScreeningStatusesList.Codes.PermanentClear }
				}
			};
			AssertEquals("Should Be CLP", ScreeningStatusesList.Codes.PermanentClear, dummyProvider.GetWorstScreeningStatus());

			dummyProvider.ScreeningPartyProviders = new List<IScreeningPartyProvider>() {
				new DummyScreeningPartyProvider()
				{
					DummyStatusList = new List<ZString>() { string.Empty, ScreeningStatusesList.Codes.JobCleared }
				}
			};
			AssertEquals("Should Be JCL", ScreeningStatusesList.Codes.JobCleared, dummyProvider.GetWorstScreeningStatus());
		}

		public void TestUpdateJobStatusFromItsScreeningParties()
		{
			var dummyProvider = new DummyScreeningPartyProvider()
			{
				ScreeningStatus = ScreeningStatusesList.Codes.Matched,
				DummyWorstScreeningStatus = ScreeningStatusesList.Codes.PermanentClear
			};

			AssertEquals("Precondition", ScreeningStatusesList.Codes.Matched, dummyProvider.ScreeningStatus);
			ScreeningStatusUpdater.UpdateJobStatusFromItsScreeningParties(dummyProvider);
			AssertEquals(ScreeningStatusesList.Codes.Clear, dummyProvider.ScreeningStatus);

			dummyProvider.ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			dummyProvider.DummyWorstScreeningStatus = ScreeningStatusesList.Codes.Clear;
			ScreeningStatusUpdater.UpdateJobStatusFromItsScreeningParties(dummyProvider);
			AssertEquals(ScreeningStatusesList.Codes.Clear, dummyProvider.ScreeningStatus);

			dummyProvider.ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			dummyProvider.DummyWorstScreeningStatus = string.Empty;
			ScreeningStatusUpdater.UpdateJobStatusFromItsScreeningParties(dummyProvider);
			AssertEquals(ScreeningStatusesList.Codes.Clear, dummyProvider.ScreeningStatus);

			dummyProvider.ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			dummyProvider.DummyWorstScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			ScreeningStatusUpdater.UpdateJobStatusFromItsScreeningParties(dummyProvider);
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, dummyProvider.ScreeningStatus);

			dummyProvider.ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			dummyProvider.DummyWorstScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			ScreeningStatusUpdater.UpdateJobStatusFromItsScreeningParties(dummyProvider, dummyProvider, dummyProvider.DummyWorstScreeningStatus?.ToString());
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, dummyProvider.ScreeningStatus);

			dummyProvider.ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			dummyProvider.DummyWorstScreeningStatus = ScreeningStatusesList.Codes.Matched;
			ScreeningStatusUpdater.UpdateJobStatusFromItsScreeningParties(dummyProvider, dummyProvider, dummyProvider.DummyWorstScreeningStatus?.ToString());
			AssertEquals(ScreeningStatusesList.Codes.JobCleared, dummyProvider.ScreeningStatus);

			dummyProvider.ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			dummyProvider.DummyWorstScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			ScreeningStatusUpdater.UpdateJobStatusFromItsScreeningParties(dummyProvider, dummyProvider, dummyProvider.DummyWorstScreeningStatus?.ToString());
			AssertEquals(ScreeningStatusesList.Codes.JobCleared, dummyProvider.ScreeningStatus);
		}

		#region UpdateRelatedJobsForOrganisation

		public void TestForwardingRegistryDynamicProperties()
		{
			var type = typeof(ScreeningStatusUpdater);

			var forwardingRegistryTypeString = type.GetField("forwardingRegistryTypeString", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null).ToString();
			AssertEquals("Enterprise.Freight.Forwarding.Registry.ForwardingConfigurationRegistry, Enterprise.Freight.Forwarding.Registry", forwardingRegistryTypeString);

			var instance = type.GetProperty("ForwardingRegistryInvoker", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
			AssertNotNull(instance);
			AssertEquals("Enterprise.Freight.Forwarding.Registry.ForwardingConfigurationRegistry", instance.GetType().FullName);

			var consolPhasesTable = ScreeningStatusUpdater.ConsolPhasesToUpdate;
			AssertNotNull(consolPhasesTable);
			AssertEquals(1, consolPhasesTable.Columns.Count);
			AssertEquals("Value", consolPhasesTable.Columns[0].ColumnName);
			AssertEquals(typeof(string), consolPhasesTable.Columns[0].DataType);

			var shipmentPhasesTable = ScreeningStatusUpdater.ShipmentPhasesToUpdate;
			AssertNotNull(shipmentPhasesTable);
			AssertEquals(1, consolPhasesTable.Columns.Count);
			AssertEquals("Value", consolPhasesTable.Columns[0].ColumnName);
			AssertEquals(typeof(string), consolPhasesTable.Columns[0].DataType);

			AssertNotNull(ScreeningStatusUpdater.ShipmentUpdateOption);
			AssertNotNull(ScreeningStatusUpdater.ConsolUpdateOption);
		}

		public void TestShouldUpdateByDatesAndBilling()
		{
			AssertEquals(true, ScreeningStatusUpdater.ShouldUpdateByDatesAndBilling("DAB"));
			AssertEquals(true, ScreeningStatusUpdater.ShouldUpdateByDatesAndBilling("COM"));
			AssertEquals(false, ScreeningStatusUpdater.ShouldUpdateByDatesAndBilling("PHS"));
		}

		public void TestShouldUpdateByPhases()
		{
			AssertEquals(false, ScreeningStatusUpdater.ShouldUpdateByPhases("DAB"));
			AssertEquals(true, ScreeningStatusUpdater.ShouldUpdateByPhases("COM"));
			AssertEquals(true, ScreeningStatusUpdater.ShouldUpdateByPhases("PHS"));
		}

		public void TestDpsUpdateOptionsCodes()
		{
			var type = Type.GetType("Enterprise.Freight.Forwarding.Registry.DpsStatusUpdateOptions+Codes, Enterprise.Freight.Forwarding.Registry");
			AssertNotNull(type);
			AssertEquals("DAB", type.GetField("DAB").GetValue(null));
			AssertEquals("PHS", type.GetField("PHS").GetValue(null));
			AssertEquals("COM", type.GetField("COM").GetValue(null));
		}

		#region Shipment

		public void TestVesselChangedToNOTWillChangeShipmentFromClearToNot()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			Db.Connection.ExecuteNonQuery($"INSERT INTO dbo.JobConsolTransport(JW_PK, JW_ParentGUID, JW_ParentType, JW_Vessel) VALUES(newid(), '{shipment.PK}', 'SHP', '{vessel.RV_Code}')");
			Factory.Save();

			Db.Connection.ExecuteNonQuery(@$"
UPDATE dbo.JobShipment
SET
	JS_ScreeningStatus = 'CLR',
	JS_SystemLastEditTimeUtc = GETUTCDATE(),
	JS_SystemLastEditUser = '{GlbStaff.CurrentUser.GS_Code.ToString()}'
WHERE
	JS_PK = '{shipment.PK}'");
			Db.Connection.ExecuteNonQuery(@$"UPDATE dbo.RefVessel SET RV_ScreeningStatus = 'CLR' WHERE RV_PK = '{vessel.PK}'");

			vessel.Reload();
			var newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<IForwardingShipment>(shipment.PK);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals(ScreeningStatusesList.Codes.Clear, shipment.JS_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, vessel.RV_ScreeningStatus);
			});

			Db.Connection.ExecuteNonQuery($"UPDATE dbo.RefVessel SET RV_ScreeningStatus = 'NOT' WHERE RV_PK = '{vessel.PK}'");

			vessel.Reload();
			newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<IForwardingShipment>(shipment.PK);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals(ScreeningStatusesList.Codes.Clear, shipment.JS_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.NotScreened, vessel.RV_ScreeningStatus);
			});

			ScreeningStatusUpdater.UpdateRelatedJobs(vessel);
			newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<IForwardingShipment>(shipment.PK);

			AssertEquals("Shipment screening status should be 'NOT'", ScreeningStatusesList.Codes.NotScreened, shipment.JS_ScreeningStatus);
		}

		public void TestVesselChangedToNOTShouldIgnoreShipmentWhenCpwEnabled()
		{
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true));

			var shipment = Factory.New<IForwardingShipment>();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			Db.Connection.ExecuteNonQuery($"INSERT INTO dbo.JobConsolTransport(JW_PK, JW_ParentGUID, JW_ParentType, JW_Vessel) VALUES(newid(), '{shipment.PK}', 'SHP', '{vessel.RV_Code}')");
			Factory.Save();

			Db.Connection.ExecuteNonQuery(@$"
UPDATE dbo.JobShipment
SET
	JS_ScreeningStatus = 'CLR',
	JS_SystemLastEditTimeUtc = GETUTCDATE(),
	JS_SystemLastEditUser = '{GlbStaff.CurrentUser.GS_Code.ToString()}'
WHERE
	JS_PK = '{shipment.PK}'");
			Db.Connection.ExecuteNonQuery(@$"UPDATE dbo.RefVessel SET RV_ScreeningStatus = 'CLR' WHERE RV_PK = '{vessel.PK}'");

			vessel.Reload();
			var newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<IForwardingShipment>(shipment.PK);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals(ScreeningStatusesList.Codes.Clear, shipment.JS_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, vessel.RV_ScreeningStatus);
			});

			Db.Connection.ExecuteNonQuery($"UPDATE dbo.RefVessel SET RV_ScreeningStatus = 'NOT' WHERE RV_PK = '{vessel.PK}'");

			vessel.Reload();
			newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<IForwardingShipment>(shipment.PK);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals(ScreeningStatusesList.Codes.Clear, shipment.JS_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.NotScreened, vessel.RV_ScreeningStatus);
			});

			ScreeningStatusUpdater.UpdateRelatedJobs(vessel);
			newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<IForwardingShipment>(shipment.PK);

			AssertEquals("Not update shipment's DPS status when CPW enabled", ScreeningStatusesList.Codes.Clear, shipment.JS_ScreeningStatus);
		}

		public void TestOrgChangedFromMATToUNKWillChangeShipmentStatusFromMATToUNK()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();
			IScreeningPartyProvider shipment = ((IScreeningPartyProvider)job);

			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_OA_Address = header.Addresses[0].PK;
			jobDocAddress.E2_ParentID = job.PK;
			jobDocAddress.E2_ParentTableCode = "JS";
			jobDocAddress.E2_AddressOverride = false;

			Factory.Save();

			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			jobDocAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			shipment.ScreeningStatus = "";

			Factory.Save();

			AssertEquals("OH_ScreeningStatus", ScreeningStatusesList.Codes.Matched, header.OH_ScreeningStatus);
			AssertEquals("JS_ScreeningStatus", ScreeningStatusesList.Codes.Matched, shipment.ScreeningStatus);

			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			Factory.Save();

			AssertEquals("OH_ScreeningStatus", ScreeningStatusesList.Codes.Unknown, header.OH_ScreeningStatus);
			AssertEquals("JS_ScreeningStatus", ScreeningStatusesList.Codes.Matched, shipment.ScreeningStatus);

			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			shipment.ScreeningStatus = "";
			((IShouldUpdateScreeningStatus)shipment).ShouldUpdateScreeningStatus = true;

			Factory.Save();
			AssertEquals("OH_ScreeningStatus", ScreeningStatusesList.Codes.Unknown, header.OH_ScreeningStatus);
			AssertEquals("JS_ScreeningStatus", ScreeningStatusesList.Codes.Unknown, shipment.ScreeningStatus);
		}

		public void TestOrgLocalClientUpdatedToMATShouldMATShipment()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_OA_LocalChargesAddr] = orgAddress.PK;
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgAddress>(orgAddress.PK);
			partyLoaded.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(orgAddress.Header));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgLocalClientUpdatedToMATShouldIgnoreShipmentWhenCpwEnabled()
		{
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true));

			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_OA_LocalChargesAddr] = orgAddress.PK;
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgAddress>(orgAddress.PK);
			partyLoaded.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(orgAddress.Header));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);
			AssertNotEquals("Not update shipment's DPS status when CPW enabled", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgExportBrokerUpdatedToMATShouldMATShipment()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgImportBrokerUpdatedToMATShouldMATShipment()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobShipmentSchema.JS_OH_ImportBroker] = org.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgExportCFSUpdatedToMATShouldMATShipment()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			job[JobShipmentSchema.JS_OA_ExportReceivingDepot] = orgAddress.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgAddress>(orgAddress.PK);
			partyLoaded.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(orgAddress.Header));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgImportCFSUpdatedToMATShouldMATShipment()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			job[JobShipmentSchema.JS_OA_ImportReleaseDepot] = orgAddress.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgAddress>(orgAddress.PK);
			partyLoaded.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(orgAddress.Header));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldNotAffectShipmentWithStatusCMP()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;
			jobHeader[JobHeaderSchema.JH_Status] = "CMP";

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldNotAffectShipmentWithStatusCLS()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;
			jobHeader[JobHeaderSchema.JH_Status] = "CLS";

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMATShipmentWithStatusWRK()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;
			jobHeader[JobHeaderSchema.JH_Status] = "WRK";

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMATShipmentWithAllDatesNullAndLastModifiedDateWithinLookbackPeriod()
		{
			AssertScreeningStatus(ScreeningStatusesList.Codes.NotScreened, 12, 13);
			AssertScreeningStatus(ScreeningStatusesList.Codes.Matched, 12, 11);

			void AssertScreeningStatus(string expectedStatus, int lookbackPeriod, int lastEditLookbackMonth)
			{
				using (OrganisationsDataRegistry.Instance.JobUpdatePeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, lookbackPeriod))
				{
					var loadFactory = new BusinessObjectFactory();
					var job = (BusinessObject)Factory.New<IForwardingShipment>();
					var org = Factory.NewWithValidTestData<OrgHeader>();
					job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;
					Factory.Save();

					var forPartyFactory = new BusinessObjectFactory();
					var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
					partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
					forPartyFactory.Save();

					Db.Connection.Command($"UPDATE dbo.JobShipment SET JS_SystemLastEditTimeUtc = DATEADD(month, {-lastEditLookbackMonth}, GETUTCDATE()), JS_SystemLastEditUser = '{GlbStaff.CurrentUser.GS_Code.ToString()}' WHERE JS_PK = '{job.PK}'").ExecuteNonQuery();

					AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));
					var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);
					AssertEquals("Screening Status", expectedStatus, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
				}
			}
		}

		public void TestOrgUpdatedToMATShouldMATShipmentWithETAOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;
			job[JobShipmentSchema.JS_E_ARV] = DateTime.Now;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMATShipmentWithETDOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;
			job[JobShipmentSchema.JS_E_DEP] = DateTime.Now;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldNotAffectShipmentWithBothETAAndETDCompleted()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			job[JobShipmentSchema.JS_E_ARV] = Convert.ToDateTime("2006-08-30 14:45:00");
			job[JobShipmentSchema.JS_E_DEP] = Convert.ToDateTime("2006-08-30 15:45:00");

			Factory.Save();

			var forOrgFactory = new BusinessObjectFactory();
			var orgLoaded = forOrgFactory.Load<OrgHeader>(org.PK);
			orgLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forOrgFactory.Save();
			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMATShipmentWithRouteLegETAOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;

			jobVoyDestination[JobVoyDestinationSchema.JB_E_ARV] = DateTime.Now;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMATShipmentWithRouteLegATAOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;

			jobVoyDestination[JobVoyDestinationSchema.JB_A_ARV] = DateTime.Now;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMATShipmentWithRouteLegETDOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;

			jobVoyOrigin[JobVoyOriginSchema.JA_E_DEP] = DateTime.Now;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMATShipmentWithRouteLegATDOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;

			jobVoyOrigin[JobVoyOriginSchema.JA_A_DEP] = DateTime.Now;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldNotAffectShipmentWithRouteLegAllDatesCompleted()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_IsLinked] = true;
			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;

			jobVoyOrigin[JobVoyOriginSchema.JA_E_DEP] = Convert.ToDateTime("2006-08-30 14:45:00");
			jobVoyOrigin[JobVoyOriginSchema.JA_A_DEP] = Convert.ToDateTime("2006-08-30 14:45:00");
			jobVoyDestination[JobVoyDestinationSchema.JB_E_ARV] = Convert.ToDateTime("2006-08-30 14:45:00");
			jobVoyDestination[JobVoyDestinationSchema.JB_A_ARV] = Convert.ToDateTime("2006-08-30 14:45:00");

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMATShipmentWithConsolRouteLegETAOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			var consol = (BusinessObject)Factory.New<IForwardingConsol>();
			ZQuery filter = new ZQuery(JobConsolTransportSchema.JW_ParentGUID, consol.PK);
			var transport = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ITransport>(filter);
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = consol.GetType();
			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;

			var jobConShipLink = (BusinessObject)Factory.New<IJobConShipLink>();
			jobConShipLink[JobConShipLinkSchema.JN_JS] = job.PK;
			jobConShipLink[JobConShipLinkSchema.JN_JK] = consol.PK;

			jobVoyDestination[JobVoyDestinationSchema.JB_E_ARV] = DateTime.Now;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToUNKShouldUpdateCLRShipmentToUNK()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			var job = (BusinessObject)Factory.New<IForwardingShipment>();
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Unknown, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldUpdateCLRShipmentToMAT()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			var job = (BusinessObject)Factory.New<IForwardingShipment>();
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToCLRShouldUpdateUNKShipmentToCLR()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Clear, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldUpdateUNKShipmentToMAT()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToCLRShouldUpdateMATShipmentToCLR()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TestOrgSSU";
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			var job = (BusinessObject)Factory.New<IForwardingShipment>();
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Clear, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToUNKShouldUpdateMATShipmentToUNK()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TestOrgSSU";
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			var job = (BusinessObject)Factory.New<IForwardingShipment>();
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Unknown, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToUNKShouldNotAffectMATShipmentWithAnotherMATOrg()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var anotherOrg = Factory.New<OrgHeader>();
			anotherOrg.OH_Code = "TestAOrgSSU";
			anotherOrg.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			job[JobShipmentSchema.JS_OH_ImportBroker] = anotherOrg.PK;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TestOrgSSU";
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Shipment Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToUNKShouldNotAffectMATShipmentWithMATVessel()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TestOrgSSU";
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Shipment Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMatchMultiShipmentsAndNoConcurrencyException()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();
			var anotherJob = (BusinessObject)Factory.New<IForwardingShipment>();

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TestOrgSSU";
			job[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;
			anotherJob[JobShipmentSchema.JS_OH_ExportBroker] = org.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);
			var anotherJobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(anotherJob.PK);

			AssertEquals("Shipment Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
			AssertEquals("Shipment Screening Status", ScreeningStatusesList.Codes.Matched, anotherJobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());

			loadFactory.Save(); //Save jobs again to check if Concurrency Exception happens
		}

		public void TestOrgUpdatedToClear_ShipmentSyncStatusToDeclaration_OrgOnlyRelatedToDeclaration()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			var jobDocAddressForShipment = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddressForShipment.E2_OA_Address = header.MainAddress.PK;
			jobDocAddressForShipment.E2_ParentID = shipment.PK;
			jobDocAddressForShipment.E2_ParentTableCode = "JS";
			jobDocAddressForShipment.E2_AddressOverride = false;

			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.NotScreened, shipment.JS_ScreeningStatus);
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, declaration.JE_ScreeningStatus);

			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			AssertEquals("Precondition", ScreeningStatusesList.Codes.Clear, header.OH_ScreeningStatus);

			ScreeningStatusUpdater.UpdateRelatedJobs(header);

			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<IBaseJobDeclaration>(declaration.PK);
			shipment = newFactory.Load<IForwardingShipment>(shipment.PK);
			AssertEquals("Should be updated to clear", ScreeningStatusesList.Codes.Clear, shipment.JS_ScreeningStatus);
			AssertEquals("Should be updated to clear", ScreeningStatusesList.Codes.Clear, declaration.JE_ScreeningStatus);
		}

		public void TestOrgUpdatedToClear_ShipmentScreeningStatusUpdated_OrgOnlyRelatedToDeclaration()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			var jobDocAddressForDeclaration = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddressForDeclaration.E2_OA_Address = header.MainAddress.PK;
			jobDocAddressForDeclaration.E2_ParentID = declaration.PK;
			jobDocAddressForDeclaration.E2_ParentTableCode = "JE";
			jobDocAddressForDeclaration.E2_AddressOverride = false;

			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.NotScreened, shipment.JS_ScreeningStatus);
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, declaration.JE_ScreeningStatus);

			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			AssertEquals("Precondition", ScreeningStatusesList.Codes.Clear, header.OH_ScreeningStatus);

			ScreeningStatusUpdater.UpdateRelatedJobs(header);

			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<IBaseJobDeclaration>(declaration.PK);
			shipment = newFactory.Load<IForwardingShipment>(shipment.PK);
			AssertEquals("Should be updated to clear", ScreeningStatusesList.Codes.Clear, shipment.JS_ScreeningStatus);
			AssertEquals("Should be updated to clear", ScreeningStatusesList.Codes.Clear, declaration.JE_ScreeningStatus);
		}

		public void TestUpdateRelatedJobsForOrg_ShipmentWithDeclaration()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "TESTORG1";
			orgHeader1.OH_FullName = "Organization For Test 1";

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "TESTORG2";
			orgHeader2.OH_FullName = "Organization For Test 2";

			Factory.Save();

			var shipment = Factory.New<IForwardingShipment>();
			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			declaration.JE_OH_Supplier = orgHeader1.PK;
			shipment.JS_OH_ImportBroker = orgHeader2.PK;

			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.NotScreened, shipment.JS_ScreeningStatus);
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, declaration.JE_ScreeningStatus);

			orgHeader1.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			orgHeader2.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();

			ScreeningStatusUpdater.UpdateRelatedJobs(orgHeader1);

			((BusinessObject)shipment).Reload();
			((BusinessObject)declaration).Reload();
			AssertEquals(ScreeningStatusesList.Codes.Matched, shipment.JS_ScreeningStatus);
			AssertEquals("'Override Freight Defaults' declaration has same screening status with shipment", ScreeningStatusesList.Codes.Matched, declaration.JE_ScreeningStatus);

			declaration.JE_OverrideFreightDefaults = false;
			declaration.JE_OH_Supplier = orgHeader1.PK;
			Factory.Save();

			ScreeningStatusUpdater.UpdateRelatedJobs(orgHeader1);

			((BusinessObject)shipment).Reload();
			((BusinessObject)declaration).Reload();
			AssertEquals(ScreeningStatusesList.Codes.Matched, shipment.JS_ScreeningStatus);
			AssertEquals("Not 'Override Freight Defaults' declaration has same screening status with shipment", ScreeningStatusesList.Codes.Matched, declaration.JE_ScreeningStatus);
		}

		public void TestOrgUpdatedToMATShouldUpdateItsShipmentsRelatedCoLoadMasterShipments()
		{
			var dummyShip = Factory.New<IForwardingShipment>();
			dummyShip.JS_ShipmentType = ShipmentTypes.ShippersConsolLead;

			var assemblyMasterShipment1 = Factory.New<IForwardingShipment>();
			assemblyMasterShipment1.JS_ShipmentType = ShipmentTypes.AssemblyMaster;

			var assemblyMasterShipment2 = Factory.New<IForwardingShipment>();
			assemblyMasterShipment2.JS_ShipmentType = ShipmentTypes.AssemblyMaster;

			var coLoadMasterShipment = Factory.New<IForwardingShipment>();
			coLoadMasterShipment.JS_ShipmentType = ShipmentTypes.CoLoadMaster;

			var standardShipment = Factory.New<IForwardingShipment>();
			standardShipment.JS_ShipmentType = ShipmentTypes.StandardHouse;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			standardShipment.JS_OH_ExportBroker = org.PK;

			standardShipment.JS_JS_ColoadMasterShipment = assemblyMasterShipment1.PK;
			assemblyMasterShipment1.JS_JS_ColoadMasterShipment = assemblyMasterShipment2.PK;
			assemblyMasterShipment2.JS_JS_ColoadMasterShipment = coLoadMasterShipment.PK;
			coLoadMasterShipment.JS_JS_ColoadMasterShipment = dummyShip.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(ScreeningStatusesList.Codes.NotScreened, (assemblyMasterShipment1 as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.NotScreened, (assemblyMasterShipment2 as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.NotScreened, (coLoadMasterShipment as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.NotScreened, (standardShipment as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, (dummyShip as IScreeningStatusProvider)?.ScreeningStatus);
				AssertContainsExactElementsInAnyOrder(new[] { assemblyMasterShipment1.PK, assemblyMasterShipment2.PK, coLoadMasterShipment.PK, standardShipment.PK }, org.AssociatedShipments);
			});

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();

			ScreeningStatusUpdater.UpdateRelatedJobs(org);

			var loadFactory = new BusinessObjectFactory();
			assemblyMasterShipment1 = loadFactory.Load<IForwardingShipment>(assemblyMasterShipment1.PK);
			assemblyMasterShipment2 = loadFactory.Load<IForwardingShipment>(assemblyMasterShipment2.PK);
			coLoadMasterShipment = loadFactory.Load<IForwardingShipment>(coLoadMasterShipment.PK);
			standardShipment = loadFactory.Load<IForwardingShipment>(standardShipment.PK);

			CombineAssertions(() =>
			{
				AssertEquals(ScreeningStatusesList.Codes.Matched, (assemblyMasterShipment1 as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Matched, (assemblyMasterShipment2 as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Matched, (coLoadMasterShipment as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Matched, (standardShipment as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, (dummyShip as IScreeningStatusProvider)?.ScreeningStatus);
			});
		}

		public void TestOrgUpdatedToMATShouldUpdateItsShipmentsRelatedBlindCoLoadMasterShipments()
		{
			var assemblyMasterShipment = Factory.New<IForwardingShipment>();
			assemblyMasterShipment.JS_ShipmentType = ShipmentTypes.AssemblyMaster;

			var blindCoLoadMasterShipment = Factory.New<IForwardingShipment>();
			blindCoLoadMasterShipment.JS_ShipmentType = ShipmentTypes.BlindCoLoadMaster;

			var standardShipment = Factory.New<IForwardingShipment>();
			standardShipment.JS_ShipmentType = ShipmentTypes.StandardHouse;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			standardShipment.JS_OH_ExportBroker = org.PK;

			standardShipment.JS_JS_ColoadMasterShipment = assemblyMasterShipment.PK;
			assemblyMasterShipment.JS_JS_ColoadMasterShipment = blindCoLoadMasterShipment.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(ScreeningStatusesList.Codes.NotScreened, (assemblyMasterShipment as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.NotScreened, (blindCoLoadMasterShipment as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.NotScreened, (standardShipment as IScreeningStatusProvider)?.ScreeningStatus);
				AssertContainsExactElementsInAnyOrder(new[] { assemblyMasterShipment.PK, blindCoLoadMasterShipment.PK, standardShipment.PK }, org.AssociatedShipments);
			});

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();

			ScreeningStatusUpdater.UpdateRelatedJobs(org);

			var loadFactory = new BusinessObjectFactory();
			assemblyMasterShipment = loadFactory.Load<IForwardingShipment>(assemblyMasterShipment.PK);
			blindCoLoadMasterShipment = loadFactory.Load<IForwardingShipment>(blindCoLoadMasterShipment.PK);
			standardShipment = loadFactory.Load<IForwardingShipment>(standardShipment.PK);

			CombineAssertions(() =>
			{
				AssertEquals(ScreeningStatusesList.Codes.Matched, (assemblyMasterShipment as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Matched, (blindCoLoadMasterShipment as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Matched, (standardShipment as IScreeningStatusProvider)?.ScreeningStatus);
			});
		}

		public void TestVesselUpdatedToMATShouldUpdateItsShipmentsRelatedCoLoadMasterShipments()
		{
			var dummyShip = Factory.New<IForwardingShipment>();
			dummyShip.JS_ShipmentType = ShipmentTypes.ShippersConsolLead;

			var assemblyMasterShipment1 = Factory.New<IForwardingShipment>();
			assemblyMasterShipment1.JS_ShipmentType = ShipmentTypes.AssemblyMaster;

			var assemblyMasterShipment2 = Factory.New<IForwardingShipment>();
			assemblyMasterShipment2.JS_ShipmentType = ShipmentTypes.AssemblyMaster;

			var coLoadMasterShipment = Factory.New<IForwardingShipment>();
			coLoadMasterShipment.JS_ShipmentType = ShipmentTypes.CoLoadMaster;

			var standardShipment = Factory.New<IForwardingShipment>();
			standardShipment.JS_ShipmentType = ShipmentTypes.StandardHouse;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "VESSEL1";

			var jobConsolTransport = Factory.New<Enterprise.Integration.Freight.ITransport>();
			jobConsolTransport.ParentType = standardShipment.GetType();
			jobConsolTransport.JW_ParentGUID = standardShipment.PK;
			jobConsolTransport.JW_Vessel = vessel.RV_Code;
			(jobConsolTransport as BusinessObject)[JobConsolTransportSchema.JW_ParentType.Name] = "SHP";

			standardShipment.JS_JS_ColoadMasterShipment = assemblyMasterShipment1.PK;
			assemblyMasterShipment1.JS_JS_ColoadMasterShipment = assemblyMasterShipment2.PK;
			assemblyMasterShipment2.JS_JS_ColoadMasterShipment = coLoadMasterShipment.PK;
			coLoadMasterShipment.JS_JS_ColoadMasterShipment = dummyShip.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(ScreeningStatusesList.Codes.NotScreened, (assemblyMasterShipment1 as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.NotScreened, (assemblyMasterShipment2 as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.NotScreened, (coLoadMasterShipment as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.NotScreened, (standardShipment as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, (dummyShip as IScreeningStatusProvider)?.ScreeningStatus);
			});

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();

			ScreeningStatusUpdater.UpdateRelatedJobs(vessel);

			var loadFactory = new BusinessObjectFactory();
			assemblyMasterShipment1 = loadFactory.Load<IForwardingShipment>(assemblyMasterShipment1.PK);
			assemblyMasterShipment2 = loadFactory.Load<IForwardingShipment>(assemblyMasterShipment2.PK);
			coLoadMasterShipment = loadFactory.Load<IForwardingShipment>(coLoadMasterShipment.PK);
			standardShipment = loadFactory.Load<IForwardingShipment>(standardShipment.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Assembly Master 1 update to MAT", ScreeningStatusesList.Codes.Matched, (assemblyMasterShipment1 as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals("Assembly Master 2 update to MAT", ScreeningStatusesList.Codes.Matched, (assemblyMasterShipment2 as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals("CoLoad Master update to MAT", ScreeningStatusesList.Codes.Matched, (coLoadMasterShipment as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals("Standard House update to MAT", ScreeningStatusesList.Codes.Matched, (standardShipment as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals("Dummy Shipment not updated", ScreeningStatusesList.Codes.Clear, (dummyShip as IScreeningStatusProvider)?.ScreeningStatus);
			});
		}

		public void TestVesselUpdatedToMATShouldUpdateItsShipmentsRelatedBlindCoLoadMasterShipments()
		{
			var assemblyMasterShipment = Factory.New<IForwardingShipment>();
			assemblyMasterShipment.JS_ShipmentType = ShipmentTypes.AssemblyMaster;

			var blindCoLoadMasterShipment = Factory.New<IForwardingShipment>();
			blindCoLoadMasterShipment.JS_ShipmentType = ShipmentTypes.BlindCoLoadMaster;

			var standardShipment = Factory.New<IForwardingShipment>();
			standardShipment.JS_ShipmentType = ShipmentTypes.StandardHouse;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "VESSEL1";
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

			var jobConsolTransport = Factory.New<Enterprise.Integration.Freight.ITransport>();
			jobConsolTransport.ParentType = standardShipment.GetType();
			jobConsolTransport.JW_ParentGUID = standardShipment.PK;
			jobConsolTransport.JW_Vessel = vessel.RV_Code;
			(jobConsolTransport as BusinessObject)[JobConsolTransportSchema.JW_ParentType.Name] = "SHP";

			standardShipment.JS_JS_ColoadMasterShipment = assemblyMasterShipment.PK;
			assemblyMasterShipment.JS_JS_ColoadMasterShipment = blindCoLoadMasterShipment.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(ScreeningStatusesList.Codes.NotScreened, (assemblyMasterShipment as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.NotScreened, (blindCoLoadMasterShipment as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.NotScreened, (standardShipment as IScreeningStatusProvider)?.ScreeningStatus);
			});

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();

			ScreeningStatusUpdater.UpdateRelatedJobs(vessel);

			var loadFactory = new BusinessObjectFactory();
			assemblyMasterShipment = loadFactory.Load<IForwardingShipment>(assemblyMasterShipment.PK);
			blindCoLoadMasterShipment = loadFactory.Load<IForwardingShipment>(blindCoLoadMasterShipment.PK);
			standardShipment = loadFactory.Load<IForwardingShipment>(standardShipment.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Assembly Master update to MAT", ScreeningStatusesList.Codes.Matched, (assemblyMasterShipment as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals("Blind CoLoad Master update to MAT", ScreeningStatusesList.Codes.Matched, (blindCoLoadMasterShipment as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEquals("Standard House update to MAT", ScreeningStatusesList.Codes.Matched, (standardShipment as IScreeningStatusProvider)?.ScreeningStatus);
			});
		}

		public void TestNoDuplicatePKsWhenGetAssociatedShipments()
		{
			var assemblyMasterShipment = Factory.New<IForwardingShipment>();
			assemblyMasterShipment.JS_ShipmentType = ShipmentTypes.AssemblyMaster;

			var standardShipment = Factory.New<IForwardingShipment>();
			standardShipment.JS_ShipmentType = ShipmentTypes.StandardHouse;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

			standardShipment.JS_OH_ExportBroker = org.PK;
			assemblyMasterShipment.JS_OH_ExportBroker = org.PK;

			standardShipment.JS_JS_ColoadMasterShipment = assemblyMasterShipment.PK;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { assemblyMasterShipment.PK, standardShipment.PK }, org.AssociatedShipments);
			AssertEquals(2, org.AssociatedShipments.Count);
		}

		#endregion

		#region Consol

		public void TestOrgDocAddressesUpdatedToMATShouldMATConsol()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress[JobDocAddressSchema.E2_OA_Address] = orgAddress.PK;
			jobDocAddress[JobDocAddressSchema.E2_ParentID] = job.PK;
			jobDocAddress[JobDocAddressSchema.E2_ParentTableCode] = "JK";

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgAddress>(orgAddress.PK);
			partyLoaded.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(orgAddress.Header));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgDocAddressesUpdatedToMATShouldIgnoreConsolWhenCpwEnabled()
		{
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true));

			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress[JobDocAddressSchema.E2_OA_Address] = orgAddress.PK;
			jobDocAddress[JobDocAddressSchema.E2_ParentID] = job.PK;
			jobDocAddress[JobDocAddressSchema.E2_ParentTableCode] = "JK";

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgAddress>(orgAddress.PK);
			partyLoaded.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(orgAddress.Header));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);
			AssertNotEquals("Not update consol's DPS status when CPW enabled", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgLocalClientUpdatedToMATShouldMATConsol()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_OA_LocalChargesAddr] = orgAddress.PK;
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgAddress>(orgAddress.PK);
			partyLoaded.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(orgAddress.Header));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgCreditorUpdatedToMATShouldMATConsol()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobConsolSchema.JK_OA_CreditorAddress] = org.MainAddress.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgSendingAgentUpdatedToMATShouldMATConsol()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			job[JobConsolSchema.JK_OA_SendingForwarderAddress] = orgAddress.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgAddress>(orgAddress.PK);
			partyLoaded.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(orgAddress.Header));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgReceivingAgentUpdatedToMATShouldMATConsol()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			job[JobConsolSchema.JK_OA_ReceivingForwarderAddress] = orgAddress.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgAddress>(orgAddress.PK);
			partyLoaded.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(orgAddress.Header));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgCarrierUpdatedToMATShouldMATConsol()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			job[JobConsolSchema.JK_OA_ShippingLineAddress] = orgAddress.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgAddress>(orgAddress.PK);
			partyLoaded.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(orgAddress.Header));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgDepartureCTOUpdatedToMATShouldMATConsol()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			job[JobConsolSchema.JK_OA_DepartureCTOAddress] = orgAddress.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgAddress>(orgAddress.PK);
			partyLoaded.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(orgAddress.Header));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgPackDepotUpdatedToMATShouldMATConsol()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			job[JobConsolSchema.JK_OA_PackDepotAddress] = orgAddress.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgAddress>(orgAddress.PK);
			partyLoaded.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(orgAddress.Header));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgPickupContainerYardUpdatedToMATShouldMATConsol()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			job[JobConsolSchema.JK_OA_ContainerYardEmptyPickupAddress] = orgAddress.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgAddress>(orgAddress.PK);
			partyLoaded.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(orgAddress.Header));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgArrivalCTOUpdatedToMATShouldMATConsol()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			job[JobConsolSchema.JK_OA_ArrivalCTOAddress] = orgAddress.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgAddress>(orgAddress.PK);
			partyLoaded.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(orgAddress.Header));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgUnpackDepotUpdatedToMATShouldMATConsol()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			job[JobConsolSchema.JK_OA_UnpackDepotAddress] = orgAddress.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgAddress>(orgAddress.PK);
			partyLoaded.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(orgAddress.Header));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgReturnContainerYardUpdatedToMATShouldMATConsol()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			job[JobConsolSchema.JK_OA_ContainerYardEmptyReturnAddress] = orgAddress.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgAddress>(orgAddress.PK);
			partyLoaded.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(orgAddress.Header));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldNotAffectConsolWithStatusCMP()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobConsolSchema.JK_OA_CreditorAddress] = org.MainAddress.PK;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;
			jobHeader[JobHeaderSchema.JH_Status] = "CMP";

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldNotAffectConsolWithStatusCLS()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobConsolSchema.JK_OA_CreditorAddress] = org.MainAddress.PK;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;
			jobHeader[JobHeaderSchema.JH_Status] = "CLS";

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMATConsolWithStatusWRK()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobConsolSchema.JK_OA_CreditorAddress] = org.MainAddress.PK;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;
			jobHeader[JobHeaderSchema.JH_Status] = "WRK";

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMATConsolWithAllDatesNullAndLastModifiedDateWithinLookbackPeriod()
		{
			AssertScreeningStatus(ScreeningStatusesList.Codes.NotScreened, 12, 13);
			AssertScreeningStatus(ScreeningStatusesList.Codes.Matched, 12, 11);

			void AssertScreeningStatus(string expectedStatus, int lookbackPeriod, int lastEditLookbackMonth)
			{
				using (OrganisationsDataRegistry.Instance.JobUpdatePeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, lookbackPeriod))
				{
					var loadFactory = new BusinessObjectFactory();
					var job = (BusinessObject)Factory.New<IForwardingConsol>();
					var org = Factory.NewWithValidTestData<OrgHeader>();
					job[JobConsolSchema.JK_OA_CreditorAddress] = org.MainAddress.PK;
					Factory.Save();

					var forPartyFactory = new BusinessObjectFactory();
					var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
					partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
					forPartyFactory.Save();

					Db.Connection.Command($"UPDATE dbo.JobConsol SET JK_SystemLastEditTimeUtc = DATEADD(month, {-lastEditLookbackMonth}, GETUTCDATE()), JK_SystemLastEditUser = '~BP' WHERE JK_PK = '{job.PK}'").ExecuteNonQuery();

					AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));
					var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);
					AssertEquals("Screening Status", expectedStatus, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
				}
			}
		}

		public void TestOrgUpdatedToMATShouldMATConsolWithETAOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobConsolSchema.JK_OA_CreditorAddress] = org.MainAddress.PK;

			ZQuery filter = new ZQuery(JobConsolTransportSchema.JW_ParentGUID, job.PK);
			var transport = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ITransport>(filter);
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();

			transport[JobConsolTransportSchema.JW_ETA] = DateTime.Now;
			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMATConsolWithETDOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobConsolSchema.JK_OA_CreditorAddress] = org.MainAddress.PK;

			ZQuery filter = new ZQuery(JobConsolTransportSchema.JW_ParentGUID, job.PK);
			var transport = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ITransport>(filter);
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();

			transport[JobConsolTransportSchema.JW_ETD] = DateTime.Now;
			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldNotAffectConsolWithETAAndETDCompleted()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobConsolSchema.JK_OA_CreditorAddress] = org.MainAddress.PK;

			ZQuery filter = new ZQuery(JobConsolTransportSchema.JW_ParentGUID, job.PK);
			var transport = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ITransport>(filter);
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();

			transport[JobConsolTransportSchema.JW_ETA] = Convert.ToDateTime("2006-08-30 14:45:00");
			transport[JobConsolTransportSchema.JW_ETD] = Convert.ToDateTime("2006-08-30 14:45:00");
			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMATConsolWithRouteLegETAOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobConsolSchema.JK_OA_CreditorAddress] = org.MainAddress.PK;

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			ZQuery filter = new ZQuery(JobConsolTransportSchema.JW_ParentGUID, job.PK);
			var transport = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ITransport>(filter);
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;

			jobVoyDestination[JobVoyDestinationSchema.JB_E_ARV] = DateTime.Now;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMATConsolWithRouteLegATAOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobConsolSchema.JK_OA_CreditorAddress] = org.MainAddress.PK;

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			ZQuery filter = new ZQuery(JobConsolTransportSchema.JW_ParentGUID, job.PK);
			var transport = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ITransport>(filter);
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;

			jobVoyDestination[JobVoyDestinationSchema.JB_A_ARV] = DateTime.Now;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMATConsolWithRouteLegETDOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobConsolSchema.JK_OA_CreditorAddress] = org.MainAddress.PK;

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			ZQuery filter = new ZQuery(JobConsolTransportSchema.JW_ParentGUID, job.PK);
			var transport = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ITransport>(filter);
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;

			jobVoyOrigin[JobVoyOriginSchema.JA_E_DEP] = DateTime.Now;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMATConsolWithRouteLegATDOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobConsolSchema.JK_OA_CreditorAddress] = org.MainAddress.PK;

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			ZQuery filter = new ZQuery(JobConsolTransportSchema.JW_ParentGUID, job.PK);
			var transport = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ITransport>(filter);
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;

			jobVoyOrigin[JobVoyOriginSchema.JA_A_DEP] = DateTime.Now;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldNotAffectConsolWithRouteLegAllDatesCompleted()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobConsolSchema.JK_OA_CreditorAddress] = org.MainAddress.PK;

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			ZQuery filter = new ZQuery(JobConsolTransportSchema.JW_ParentGUID, job.PK);
			var transport = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ITransport>(filter);
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;

			jobVoyOrigin[JobVoyOriginSchema.JA_E_DEP] = Convert.ToDateTime("2006-08-30 14:45:00");
			jobVoyOrigin[JobVoyOriginSchema.JA_A_DEP] = Convert.ToDateTime("2006-08-30 14:45:00");
			jobVoyDestination[JobVoyDestinationSchema.JB_E_ARV] = Convert.ToDateTime("2006-08-30 14:45:00");
			jobVoyDestination[JobVoyDestinationSchema.JB_A_ARV] = Convert.ToDateTime("2006-08-30 14:45:00");

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		#endregion

		#region Declaration

		public void TestVesselChangedToNOTWillChangeDeclarationFromClearToNot()
		{
			var declaration = Factory.New<IBaseJobDeclaration>();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			Db.Connection.ExecuteNonQuery($"INSERT INTO dbo.JobConsolTransport(JW_PK, JW_ParentGUID, JW_ParentType, JW_Vessel) VALUES(newid(), '{declaration.PK}', 'DEC', '{vessel.RV_Code}')");
			Factory.Save();

			Db.Connection.ExecuteNonQuery(@$"
UPDATE dbo.JobDeclaration
SET
	JE_ScreeningStatus = 'CLR',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '{GlbStaff.CurrentUser.GS_Code.ToString()}'
WHERE
	JE_PK = '{declaration.PK}'");
			Db.Connection.ExecuteNonQuery(@$"
UPDATE dbo.RefVessel
SET
	RV_ScreeningStatus = 'CLR'
WHERE
	RV_PK = '{vessel.PK}'");

			vessel.Reload();
			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<IBaseJobDeclaration>(declaration.PK);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals(ScreeningStatusesList.Codes.Clear, declaration.JE_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, vessel.RV_ScreeningStatus);
			});

			Db.Connection.ExecuteNonQuery($"UPDATE dbo.RefVessel SET RV_ScreeningStatus = 'NOT' WHERE RV_PK = '{vessel.PK}'");

			vessel.Reload();
			newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<IBaseJobDeclaration>(declaration.PK);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals(ScreeningStatusesList.Codes.Clear, declaration.JE_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.NotScreened, vessel.RV_ScreeningStatus);
			});

			ScreeningStatusUpdater.UpdateRelatedJobs(vessel);
			newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<IBaseJobDeclaration>(declaration.PK);

			AssertEquals("Declaration screening status should be 'NOT'", ScreeningStatusesList.Codes.NotScreened, declaration.JE_ScreeningStatus);
		}

		public void TestScreeningShipmentAndDeclaration_UpdatesStatusFromRelatedOrg()
		{
			AssertScreeningShipmentAndDeclaration_UpdatesStatusFromRelatedOrg(ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.NotScreened);
			AssertScreeningShipmentAndDeclaration_UpdatesStatusFromRelatedOrg(ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Unknown);
			AssertScreeningShipmentAndDeclaration_UpdatesStatusFromRelatedOrg(ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Matched);
			AssertScreeningShipmentAndDeclaration_UpdatesStatusFromRelatedOrg(ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Clear);

			AssertScreeningShipmentAndDeclaration_UpdatesStatusFromRelatedOrg(ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.NotScreened);
			AssertScreeningShipmentAndDeclaration_UpdatesStatusFromRelatedOrg(ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Unknown);
			AssertScreeningShipmentAndDeclaration_UpdatesStatusFromRelatedOrg(ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Matched);
			AssertScreeningShipmentAndDeclaration_UpdatesStatusFromRelatedOrg(ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Clear);

			AssertScreeningShipmentAndDeclaration_UpdatesStatusFromRelatedOrg(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.NotScreened);
			AssertScreeningShipmentAndDeclaration_UpdatesStatusFromRelatedOrg(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Unknown);
			AssertScreeningShipmentAndDeclaration_UpdatesStatusFromRelatedOrg(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Matched);
			AssertScreeningShipmentAndDeclaration_UpdatesStatusFromRelatedOrg(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Clear);

			AssertScreeningShipmentAndDeclaration_UpdatesStatusFromRelatedOrg(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.NotScreened);
			AssertScreeningShipmentAndDeclaration_UpdatesStatusFromRelatedOrg(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Unknown);
			AssertScreeningShipmentAndDeclaration_UpdatesStatusFromRelatedOrg(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Matched);
			AssertScreeningShipmentAndDeclaration_UpdatesStatusFromRelatedOrg(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Clear);

			AssertScreeningShipmentAndDeclaration_UpdatesStatusFromRelatedOrg(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.NotScreened);
			AssertScreeningShipmentAndDeclaration_UpdatesStatusFromRelatedOrg(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Unknown);
			AssertScreeningShipmentAndDeclaration_UpdatesStatusFromRelatedOrg(ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Matched);
			AssertScreeningShipmentAndDeclaration_UpdatesStatusFromRelatedOrg(ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Clear);
		}

		void AssertScreeningShipmentAndDeclaration_UpdatesStatusFromRelatedOrg(string originalHeaderStatus, string originalDeclarationStatus, string originalShipmentStatus, string expectedStatus)
		{
			var shipment = Factory.New<IForwardingShipment>();
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			var jobDocAddressForShipment = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddressForShipment.E2_OA_Address = header.MainAddress.PK;
			jobDocAddressForShipment.E2_ParentID = shipment.PK;
			jobDocAddressForShipment.E2_ParentTableCode = "JS";
			jobDocAddressForShipment.E2_AddressOverride = false;

			var jobDocAddressForDeclaration = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddressForDeclaration.E2_OA_Address = header.MainAddress.PK;
			jobDocAddressForDeclaration.E2_ParentID = declaration.PK;
			jobDocAddressForDeclaration.E2_ParentTableCode = "JE";
			jobDocAddressForDeclaration.E2_AddressOverride = false;

			Factory.Save();

			Db.Connection.ExecuteNonQuery(@$"
UPDATE dbo.OrgHeader
SET
	OH_ScreeningStatus = '{originalHeaderStatus}'
WHERE
	OH_PK = '{header.PK}'");
			Db.Connection.ExecuteNonQuery(@$"
UPDATE dbo.JobDeclaration
SET
	JE_ScreeningStatus = '{originalDeclarationStatus}',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '{GlbStaff.CurrentUser.GS_Code.ToString()}'
WHERE
	JE_PK = '{declaration.PK}'");
			Db.Connection.ExecuteNonQuery(@$"
UPDATE dbo.JobShipment
SET
	JS_ScreeningStatus = '{originalShipmentStatus}',
	JS_SystemLastEditTimeUtc = GETUTCDATE(),
	JS_SystemLastEditUser = '{GlbStaff.CurrentUser.GS_Code.ToString()}'
WHERE
	JS_PK = '{shipment.PK}'");

			header.Reload();
			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<IBaseJobDeclaration>(declaration.PK);
			shipment = newFactory.Load<IForwardingShipment>(shipment.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Original Header Status", originalHeaderStatus, header.OH_ScreeningStatus);
				AssertEquals("Original Declaration Status", originalDeclarationStatus, declaration.JE_ScreeningStatus);
				AssertEquals("Original Shipment Status", originalShipmentStatus, shipment.JS_ScreeningStatus);
			});

			Db.Connection.ExecuteNonQuery($"UPDATE dbo.OrgHeader SET OH_ScreeningStatus = '{expectedStatus}' WHERE OH_PK = '{header.PK}'");
			ScreeningStatusUpdater.UpdateRelatedJobs(header);

			header.Reload();
			newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<IBaseJobDeclaration>(declaration.PK);
			shipment = newFactory.Load<IForwardingShipment>(shipment.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Expected Header Status", expectedStatus, header.OH_ScreeningStatus);
				AssertEquals("Expected Declaration Status", expectedStatus, declaration.JE_ScreeningStatus);
				AssertEquals("Expected Shipment Status", expectedStatus, shipment.JS_ScreeningStatus);
			});
		}

		public void TestOrgDocAddressesUpdatedToMATShouldMATDeclaration()
		{
			var job = Factory.New<IBaseJobDeclaration>();

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress[JobDocAddressSchema.E2_OA_Address] = orgAddress.PK;
			jobDocAddress[JobDocAddressSchema.E2_ParentID] = job.PK;
			jobDocAddress[JobDocAddressSchema.E2_ParentTableCode] = "JE";

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgAddress>(orgAddress.PK);
			partyLoaded.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(orgAddress.Header));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestOrgLocalClientUpdatedToMATShouldMATDeclaration()
		{
			var job = Factory.New<IBaseJobDeclaration>();

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_OA_LocalChargesAddr] = orgAddress.PK;
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgAddress>(orgAddress.PK);
			partyLoaded.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(orgAddress.Header));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestOrgSupplierUpdatedToMATShouldMATDeclaration()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobDeclarationSchema.JE_OH_Supplier] = org.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestOrgImporterUpdatedToMATShouldMATDeclaration()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CountryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedStates;
			job[JobDeclarationSchema.JE_OH_Importer] = org.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestOrgCarrierUpdatedToMATShouldMATDeclaration()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobDeclarationSchema.JE_OH_ShippingLine] = org.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestUpdateRelatedJobs_WhenOrgUpdatedToMAT_ShouldNotAffectJobsStatusWithJCE()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobDeclarationSchema.JE_OH_Supplier] = org.PK;

			Factory.Save();

			var jobDeclaration = Factory.Load<IBaseJobDeclaration>(job.PK);
			jobDeclaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.JobClearedExternal;
			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.JobClearedExternal, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}
		public void TestUpdateRelatedJobs_WhenOrgUpdatedToCLR_ShouldNotAffectJobsStatusWithJBE()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobDeclarationSchema.JE_OH_Supplier] = org.PK;

			Factory.Save();

			var jobDeclaration = Factory.Load<IBaseJobDeclaration>(job.PK);
			jobDeclaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.JobBlockedExternal;
			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.JobBlockedExternal, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}
		public void TestOrgForwarderUpdatedToMATShouldMATDeclaration()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobDeclarationSchema.JE_OH_Forwarder] = org.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestOrgCarrierProviderUpdatedToMATShouldMATDeclaration()
		{
			var job = Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "DEC";
			transport[JobConsolTransportSchema.JW_OA_CarrierAddress] = org.MainAddress.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestOrgIntermediateConsigneeProviderUpdatedToMATShouldMATDeclaration()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();

			job[JobDeclarationSchema.JE_OH_Consignee] = org.PK.ToString();

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestOrgInvoiceUltimateSupplierUpdatedToMATShouldMATDeclaration()
		{
			var job = Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var invoice = (BusinessObject)Factory.New<Shared.IBaseJobComInvoiceHeader>();
			invoice[JobComInvoiceHeaderSchema.JZ_JE] = job.PK;
			invoice[JobComInvoiceHeaderSchema.JZ_OH_Supplier] = org.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestOrgInvoiceUltimateConsigneeUpdatedToMATShouldMATDeclaration()
		{
			var job = Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var invoice = (BusinessObject)Factory.New<Shared.IBaseJobComInvoiceHeader>();
			invoice[JobComInvoiceHeaderSchema.JZ_JE] = job.PK;
			invoice[JobComInvoiceHeaderSchema.JZ_OH_Buyer] = org.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestOrgInvoiceIntermediateConsigneeUpdatedToMATShouldMATDeclaration()
		{
			var job = Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var invoice = (BusinessObject)Factory.New<Shared.IBaseJobComInvoiceHeader>();
			invoice[JobComInvoiceHeaderSchema.JZ_JE] = job.PK;
			invoice[JobComInvoiceHeaderSchema.JZ_OH_Consignee] = org.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldNotAffectDeclarationWithStatusCMP()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobDeclarationSchema.JE_OH_Supplier] = org.PK;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;
			jobHeader[JobHeaderSchema.JH_Status] = "CMP";

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldNotAffectDeclarationWithStatusCLS()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobDeclarationSchema.JE_OH_Supplier] = org.PK;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;
			jobHeader[JobHeaderSchema.JH_Status] = "CLS";

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMATDeclarationWithStatusWRK()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobDeclarationSchema.JE_OH_Supplier] = org.PK;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;
			jobHeader[JobHeaderSchema.JH_Status] = "WRK";

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMATDeclarationWithAllDatesNullAndLastModifiedDateWithinLookbackPeriod()
		{
			AssertScreeningStatus(ScreeningStatusesList.Codes.NotScreened, 12, 13);
			AssertScreeningStatus(ScreeningStatusesList.Codes.Matched, 12, 11);

			void AssertScreeningStatus(string expectedStatus, int lookbackPeriod, int lastEditLookbackMonth)
			{
				using (OrganisationsDataRegistry.Instance.JobUpdatePeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, lookbackPeriod))
				{
					var loadFactory = new BusinessObjectFactory();
					var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();
					var org = Factory.NewWithValidTestData<OrgHeader>();
					job[JobDeclarationSchema.JE_OH_Supplier] = org.PK;
					Factory.Save();

					var forPartyFactory = new BusinessObjectFactory();
					var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
					partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
					forPartyFactory.Save();

					Db.Connection.Command($"UPDATE dbo.JobDeclaration SET JE_SystemLastEditTimeUtc = DATEADD(month, {-lastEditLookbackMonth}, GETUTCDATE()), JE_SystemLastEditUser = '~BP' WHERE JE_PK = '{job.PK}'").ExecuteNonQuery();

					AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));
					var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);
					AssertEquals("Screening Status", expectedStatus, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
				}
			}
		}

		public void TestOrgUpdatedToMATShouldMATDeclarationWithETDOutstanding()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobDeclarationSchema.JE_OH_Supplier] = org.PK;

			job[JobDeclarationSchema.JE_MessageType] = "EXP";
			job[JobDeclarationSchema.JE_ExportDate] = DateTime.Now;
			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMATDeclarationWithATDOutstanding()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobDeclarationSchema.JE_OH_Supplier] = org.PK;

			job[JobDeclarationSchema.JE_MessageType] = "IMP";
			job[JobDeclarationSchema.JE_ExportDate] = DateTime.Now;
			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldNotAffectDeclarationWithETDCompleted()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobDeclarationSchema.JE_OH_Supplier] = org.PK;

			job[JobDeclarationSchema.JE_MessageType] = "EXP";
			job[JobDeclarationSchema.JE_ExportDate] = Convert.ToDateTime("2006-08-30 14:45:00");
			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldNotAffectDeclarationWithATDCompleted()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobDeclarationSchema.JE_OH_Supplier] = org.PK;

			job[JobDeclarationSchema.JE_MessageType] = "IMP";
			job[JobDeclarationSchema.JE_ExportDate] = Convert.ToDateTime("2006-08-30 14:45:00");
			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMATDeclarationWithRouteLegETAOutstanding()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobDeclarationSchema.JE_OH_Supplier] = org.PK;

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;

			jobVoyDestination[JobVoyDestinationSchema.JB_E_ARV] = DateTime.Now;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMATDeclarationWithRouteLegATAOutstanding()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobDeclarationSchema.JE_OH_Supplier] = org.PK;

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;

			jobVoyDestination[JobVoyDestinationSchema.JB_A_ARV] = DateTime.Now;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMATDeclarationWithRouteLegETDOutstanding()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobDeclarationSchema.JE_OH_Supplier] = org.PK;

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;

			jobVoyOrigin[JobVoyOriginSchema.JA_E_DEP] = DateTime.Now;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldMATDeclarationWithRouteLegATDOutstanding()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobDeclarationSchema.JE_OH_Supplier] = org.PK;

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;

			jobVoyOrigin[JobVoyOriginSchema.JA_A_DEP] = DateTime.Now;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldNotAffectDeclarationWithRouteLegAllDatesCompleted()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobDeclarationSchema.JE_OH_Supplier] = org.PK;

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_IsLinked] = true;
			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;

			jobVoyOrigin[JobVoyOriginSchema.JA_E_DEP] = Convert.ToDateTime("2006-08-30 14:45:00");
			jobVoyOrigin[JobVoyOriginSchema.JA_A_DEP] = Convert.ToDateTime("2006-08-30 14:45:00");
			jobVoyDestination[JobVoyDestinationSchema.JB_E_ARV] = Convert.ToDateTime("2006-08-30 14:45:00");
			jobVoyDestination[JobVoyDestinationSchema.JB_A_ARV] = Convert.ToDateTime("2006-08-30 14:45:00");

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestOrgUpdatedToMATShouldAffectDeclarationWithNonUSBranch()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobDeclarationSchema.JE_OH_Supplier] = org.PK;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			job[JobDeclarationSchema.JE_GB] = branch.PK;
			branch.GB_RL_NKHomePort = "AUSYD";

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<OrgHeader>(org.PK);
			partyLoaded.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(org));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		#endregion

		#endregion

		#region UpdateRelatedJobsForVessel

		#region Declaration

		public void TestVesselUpdatedToMATShouldMATDeclarationWithStatusWRK()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "DEC";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;
			jobHeader[JobHeaderSchema.JH_Status] = "WRK";

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestUpdateRelatedJobs_WhenVesselUpdatedToMAT_ShouldNotAffectJobsStatusWithJCE()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "DEC";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			Factory.Save();

			var jobDeclaration = Factory.Load<IBaseJobDeclaration>(job.PK);
			jobDeclaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.JobClearedExternal;
			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.JobClearedExternal, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestUpdateRelatedJobs_WhenVesselUpdatedToCLR_ShouldNotAffectJobsStatusWithJBE()
		{
			var job = (BusinessObject)Factory.New<IBaseJobDeclaration>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "DEC";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			Factory.Save();

			var jobDeclaration = Factory.Load<IBaseJobDeclaration>(job.PK);
			jobDeclaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.JobBlockedExternal;
			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.JobBlockedExternal, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestUpdateRelatedJobsForVessel_ShipmentWithDeclaration()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();
			var declarationConnectedToJob = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			((IBaseJobDeclaration)declarationConnectedToJob).JE_JS = job.PK;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;
			jobHeader[JobHeaderSchema.JH_Status] = "WRK";

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, job[JobShipmentSchema.JS_ScreeningStatus].ToString());
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, declarationConnectedToJob[JobDeclarationSchema.JE_ScreeningStatus].ToString());

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);
			var declarationLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(declarationConnectedToJob.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, declarationLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		#endregion

		#region Shipment

		public void TestVesselUpdatedToMATShouldNotAffectShipmentWithStatusCMP()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;
			jobHeader[JobHeaderSchema.JH_Status] = "CMP";

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestVesselUpdatedToMATShouldNotAffectShipmentWithStatusCLS()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;
			jobHeader[JobHeaderSchema.JH_Status] = "CLS";

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestVesselUpdatedToMATShouldMATShipmentWithStatusWRK()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;
			jobHeader[JobHeaderSchema.JH_Status] = "WRK";

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestVesselUpdatedToMATShouldMATShipmentWithStatusWRKAndVesselFromJobVoyage()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;
			jobHeader[JobHeaderSchema.JH_Status] = "WRK";

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			jobVoy[JobVoyageSchema.JV_RV_NKVessel] = vessel.RV_FK;
			transport[JobConsolTransportSchema.JW_IsLinked] = true;
			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestVesselUpdatedToMATShouldMATShipmentWithAllDatesNullAndLastModifiedDateWithinLookbackPeriod()
		{
			AssertScreeningStatus(ScreeningStatusesList.Codes.NotScreened, 12, 13);
			AssertScreeningStatus(ScreeningStatusesList.Codes.Matched, 12, 11);

			void AssertScreeningStatus(string expectedStatus, int lookbackPeriod, int lastEditLookbackMonth)
			{
				using (OrganisationsDataRegistry.Instance.JobUpdatePeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, lookbackPeriod))
				{
					var job = (BusinessObject)Factory.New<IForwardingShipment>();
					var vessel = Factory.NewWithValidTestData<RefVessel>();
					var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
					((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
					transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
					transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
					transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;
					Factory.Save();

					var forPartyFactory = new BusinessObjectFactory();
					var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
					partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
					forPartyFactory.Save();

					((IScreeningStatusProvider)job).ScreeningStatus = ScreeningStatusesList.Codes.Clear;
					Db.Connection.Command($"UPDATE dbo.JobShipment SET JS_SystemLastEditTimeUtc = DATEADD(month, {-lastEditLookbackMonth}, GETUTCDATE()), JS_SystemLastEditUser = '~BP' WHERE JS_PK = '{job.PK}'").ExecuteNonQuery();

					AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

					var loadFactory = new BusinessObjectFactory();
					var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);
					AssertEquals("Screening Status", expectedStatus, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
				}
			}
		}

		public void TestVesselUpdatedToMATShouldMATShipmentWithETAOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			job[JobShipmentSchema.JS_E_ARV] = DateTime.Now;
			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestVesselUpdatedToMATShouldMATShipmentWithETDOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			job[JobShipmentSchema.JS_E_DEP] = DateTime.Now;
			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestVesselUpdatedToMATShouldNotAffectShipmentWithBothETAAndETDCompleted()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			job[JobShipmentSchema.JS_E_ARV] = Convert.ToDateTime("2006-08-30 14:45:00");
			job[JobShipmentSchema.JS_E_DEP] = Convert.ToDateTime("2006-08-30 15:45:00");
			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestVesselUpdatedToMATShouldMATShipmentWithRouteLegETAOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();

			jobVoy[JobVoyageSchema.JV_RV_NKVessel] = vessel.RV_FK;
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_E_ARV] = DateTime.Now;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestVesselUpdatedToMATShouldMATShipmentWithRouteLegATAOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();

			jobVoy[JobVoyageSchema.JV_RV_NKVessel] = vessel.RV_FK;
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_A_ARV] = DateTime.Now;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestVesselUpdatedToMATShouldMATShipmentWithRouteLegETDOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();

			jobVoy[JobVoyageSchema.JV_RV_NKVessel] = vessel.RV_FK;
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyOrigin[JobVoyOriginSchema.JA_E_DEP] = DateTime.Now;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestVesselUpdatedToMATShouldMATShipmentWithRouteLegATDOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();

			jobVoy[JobVoyageSchema.JV_RV_NKVessel] = vessel.RV_FK;
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyOrigin[JobVoyOriginSchema.JA_A_DEP] = DateTime.Now;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestVesselUpdatedToMATShouldNotAffectShipmentWithRouteLegAllDatesCompleted()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			transport[JobConsolTransportSchema.JW_IsLinked] = true;
			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;

			jobVoyOrigin[JobVoyOriginSchema.JA_E_DEP] = Convert.ToDateTime("2006-08-30 14:45:00");
			jobVoyOrigin[JobVoyOriginSchema.JA_A_DEP] = Convert.ToDateTime("2006-08-30 14:45:00");
			jobVoyDestination[JobVoyDestinationSchema.JB_E_ARV] = Convert.ToDateTime("2006-08-30 14:45:00");
			jobVoyDestination[JobVoyDestinationSchema.JB_A_ARV] = Convert.ToDateTime("2006-08-30 14:45:00");

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		public void TestVesselUpdatedToMATShouldMATShipmentWithRouteLegETAOutstandingAndVesselFromJobVoyage()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			jobVoy[JobVoyageSchema.JV_RV_NKVessel] = vessel.RV_FK;
			transport[JobConsolTransportSchema.JW_IsLinked] = true;
			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;

			jobVoyDestination[JobVoyDestinationSchema.JB_E_ARV] = DateTime.Now;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
		}

		#endregion

		#region Consol

		public void TestVesselChangedToNOTWillChangeConsolFromClearToNot()
		{
			var jobConsol = Factory.New<IForwardingConsol>();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			Db.Connection.ExecuteNonQuery($"INSERT INTO dbo.JobConsolTransport(JW_PK, JW_ParentGUID, JW_ParentType, JW_Vessel) VALUES(newid(), '{jobConsol.PK}', 'CON', '{vessel.RV_Code}')");
			Factory.Save();

			Db.Connection.ExecuteNonQuery(@$"
UPDATE dbo.JobConsol
SET
	JK_ScreeningStatus = 'CLR',
	JK_SystemLastEditTimeUtc = GETUTCDATE(),
	JK_SystemLastEditUser = '{GlbStaff.CurrentUser.GS_Code.ToString()}'
WHERE
	JK_PK = '{jobConsol.PK}'");
			Db.Connection.ExecuteNonQuery(@$"UPDATE dbo.RefVessel SET RV_ScreeningStatus = 'CLR' WHERE RV_PK = '{vessel.PK}'");

			vessel.Reload();
			var newFactory = new BusinessObjectFactory();
			jobConsol = newFactory.Load<IForwardingConsol>(jobConsol.PK);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals(ScreeningStatusesList.Codes.Clear, jobConsol.JK_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, vessel.RV_ScreeningStatus);
			});

			Db.Connection.ExecuteNonQuery($"UPDATE dbo.RefVessel SET RV_ScreeningStatus = 'NOT' WHERE RV_PK = '{vessel.PK}'");

			vessel.Reload();
			newFactory = new BusinessObjectFactory();
			jobConsol = newFactory.Load<IForwardingConsol>(jobConsol.PK);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals(ScreeningStatusesList.Codes.Clear, jobConsol.JK_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.NotScreened, vessel.RV_ScreeningStatus);
			});

			ScreeningStatusUpdater.UpdateRelatedJobs(vessel);
			newFactory = new BusinessObjectFactory();
			jobConsol = newFactory.Load<IForwardingConsol>(jobConsol.PK);

			AssertEquals("Declaration screening status should be 'NOT'", ScreeningStatusesList.Codes.NotScreened, jobConsol.JK_ScreeningStatus);
		}

		public void TestVesselChangedToNOTShouldIgnoreConsolWhenCpwEnabled()
		{
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true));

			var jobConsol = Factory.New<IForwardingConsol>();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			Db.Connection.ExecuteNonQuery($"INSERT INTO dbo.JobConsolTransport(JW_PK, JW_ParentGUID, JW_ParentType, JW_Vessel) VALUES(newid(), '{jobConsol.PK}', 'CON', '{vessel.RV_Code}')");
			Factory.Save();

			Db.Connection.ExecuteNonQuery(@$"
UPDATE dbo.JobConsol
SET
	JK_ScreeningStatus = 'CLR',
	JK_SystemLastEditTimeUtc = GETUTCDATE(),
	JK_SystemLastEditUser = '{GlbStaff.CurrentUser.GS_Code.ToString()}'
WHERE
	JK_PK = '{jobConsol.PK}'");
			Db.Connection.ExecuteNonQuery(@$"UPDATE dbo.RefVessel SET RV_ScreeningStatus = 'CLR' WHERE RV_PK = '{vessel.PK}'");

			vessel.Reload();
			var newFactory = new BusinessObjectFactory();
			jobConsol = newFactory.Load<IForwardingConsol>(jobConsol.PK);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals(ScreeningStatusesList.Codes.Clear, jobConsol.JK_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, vessel.RV_ScreeningStatus);
			});

			Db.Connection.ExecuteNonQuery($"UPDATE dbo.RefVessel SET RV_ScreeningStatus = 'NOT' WHERE RV_PK = '{vessel.PK}'");

			vessel.Reload();
			newFactory = new BusinessObjectFactory();
			jobConsol = newFactory.Load<IForwardingConsol>(jobConsol.PK);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals(ScreeningStatusesList.Codes.Clear, jobConsol.JK_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.NotScreened, vessel.RV_ScreeningStatus);
			});

			ScreeningStatusUpdater.UpdateRelatedJobs(vessel);
			newFactory = new BusinessObjectFactory();
			jobConsol = newFactory.Load<IForwardingConsol>(jobConsol.PK);

			AssertEquals("Not update consol's DPS status when CPW enabled", ScreeningStatusesList.Codes.Clear, jobConsol.JK_ScreeningStatus);
		}

		public void TestVesselUpdatedToMATShouldNotAffectConsolWithStatusCMP()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			ZQuery filter = new ZQuery(JobConsolTransportSchema.JW_ParentGUID, job.PK);
			var transport = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ITransport>(filter);
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentType] = "CON";
			transport[JobConsolTransportSchema.JW_IsLinked] = false;
			transport[JobConsolTransportSchema.JW_Vessel] = "TestCode";

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;
			jobVoy[JobVoyageSchema.JV_RV_NKVessel] = vessel.RV_FK;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;
			jobHeader[JobHeaderSchema.JH_Status] = "CMP";

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestVesselUpdatedToMATShouldNotAffectConsolWithStatusCLS()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			ZQuery filter = new ZQuery(JobConsolTransportSchema.JW_ParentGUID, job.PK);
			var transport = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ITransport>(filter);
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentType] = "CON";
			transport[JobConsolTransportSchema.JW_IsLinked] = false;
			transport[JobConsolTransportSchema.JW_Vessel] = "TestCode";

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;
			jobVoy[JobVoyageSchema.JV_RV_NKVessel] = vessel.RV_FK;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;
			jobHeader[JobHeaderSchema.JH_Status] = "CLS";

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestVesselUpdatedToMATShouldMATConsolWithStatusWRK()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			ZQuery filter = new ZQuery(JobConsolTransportSchema.JW_ParentGUID, job.PK);
			var transport = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ITransport>(filter);
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentType] = "CON";

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;
			jobVoy[JobVoyageSchema.JV_RV_NKVessel] = vessel.RV_FK;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;
			jobHeader[JobHeaderSchema.JH_Status] = "WRK";

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestVesselUpdatedToMATShouldMATConsolWithStatusWRKAndVesselNameFromNewAddedRouteLeg()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;
			jobHeader[JobHeaderSchema.JH_Status] = "WRK";

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			//Add a new route leg
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "CON";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestVesselUpdatedToMATShouldMATConsolWithAllDatesNullAndLastModifiedDateWithinLookbackPeriod()
		{
			AssertScreeningStatus(ScreeningStatusesList.Codes.Clear, 12, 13);
			AssertScreeningStatus(ScreeningStatusesList.Codes.Matched, 12, 11);

			void AssertScreeningStatus(string expectedStatus, int lookbackPeriod, int lastEditLookbackMonth)
			{
				using (OrganisationsDataRegistry.Instance.JobUpdatePeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, lookbackPeriod))
				{
					var job = (BusinessObject)Factory.New<IForwardingConsol>();
					var vessel = Factory.NewWithValidTestData<RefVessel>();
					ZQuery filter = new ZQuery(JobConsolTransportSchema.JW_ParentGUID, job.PK);
					var transport = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ITransport>(filter);
					((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
					transport[JobConsolTransportSchema.JW_ParentType] = "CON";

					var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
					var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
					var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
					jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
					jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

					var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
					jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
					jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

					transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;
					jobVoy[JobVoyageSchema.JV_RV_NKVessel] = vessel.RV_FK;
					Factory.Save();

					var forPartyFactory = new BusinessObjectFactory();
					var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
					partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
					forPartyFactory.Save();

					((IScreeningStatusProvider)job).ScreeningStatus = ScreeningStatusesList.Codes.Clear;
					Db.Connection.Command($"UPDATE dbo.JobConsol SET JK_SystemLastEditTimeUtc = DATEADD(month, {-lastEditLookbackMonth}, GETUTCDATE()), JK_SystemLastEditUser = '~BP' WHERE JK_PK = '{job.PK}'").ExecuteNonQuery();

					AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

					var loadFactory = new BusinessObjectFactory();
					var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);
					AssertEquals("Screening Status", expectedStatus, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
				}
			}
		}

		public void TestVesselUpdatedToMATShouldNotAffectConsolWithETAAndETDCompleted()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			ZQuery filter = new ZQuery(JobConsolTransportSchema.JW_ParentGUID, job.PK);
			var transport = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ITransport>(filter);
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentType] = "CON";

			var creditorOrg = Factory.NewWithValidTestData<OrgHeader>();
			creditorOrg.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			job[JobConsolSchema.JK_OA_CreditorAddress] = creditorOrg.MainAddress.PK;

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;
			jobVoy[JobVoyageSchema.JV_RV_NKVessel] = vessel.RV_FK;

			transport[JobConsolTransportSchema.JW_ETA] = Convert.ToDateTime("2006-08-30 14:45:00");
			transport[JobConsolTransportSchema.JW_ETD] = Convert.ToDateTime("2006-08-30 14:45:00");
			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestVesselUpdatedToMATShouldMATConsolWithRouteLegETAOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			ZQuery filter = new ZQuery(JobConsolTransportSchema.JW_ParentGUID, job.PK);
			var transport = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ITransport>(filter);
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentType] = "CON";

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;
			jobVoy[JobVoyageSchema.JV_RV_NKVessel] = vessel.RV_FK;

			jobVoyDestination[JobVoyDestinationSchema.JB_E_ARV] = DateTime.Now;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestVesselUpdatedToMATShouldMATConsolWithRouteLegATAOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			ZQuery filter = new ZQuery(JobConsolTransportSchema.JW_ParentGUID, job.PK);
			var transport = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ITransport>(filter);
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentType] = "CON";

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;
			jobVoy[JobVoyageSchema.JV_RV_NKVessel] = vessel.RV_FK;

			jobVoyDestination[JobVoyDestinationSchema.JB_A_ARV] = DateTime.Now;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestVesselUpdatedToMATShouldMATConsolWithRouteLegETDOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			ZQuery filter = new ZQuery(JobConsolTransportSchema.JW_ParentGUID, job.PK);
			var transport = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ITransport>(filter);
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentType] = "CON";

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;
			jobVoy[JobVoyageSchema.JV_RV_NKVessel] = vessel.RV_FK;

			jobVoyOrigin[JobVoyOriginSchema.JA_E_DEP] = DateTime.Now;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestVesselUpdatedToMATShouldMATConsolWithRouteLegATDOutstanding()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			ZQuery filter = new ZQuery(JobConsolTransportSchema.JW_ParentGUID, job.PK);
			var transport = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ITransport>(filter);
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentType] = "CON";

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;
			jobVoy[JobVoyageSchema.JV_RV_NKVessel] = vessel.RV_FK;

			jobVoyOrigin[JobVoyOriginSchema.JA_A_DEP] = DateTime.Now;

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Matched, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		public void TestVesselUpdatedToMATShouldNotAffectConsolWithRouteLegAllDatesCompleted()
		{
			var job = (BusinessObject)Factory.New<IForwardingConsol>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			ZQuery filter = new ZQuery(JobConsolTransportSchema.JW_ParentGUID, job.PK);
			var transport = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ITransport>(filter);
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentType] = "CON";
			transport[JobConsolTransportSchema.JW_IsLinked] = false;
			transport[JobConsolTransportSchema.JW_Vessel] = "TestCode";

			var jobVoy = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobVoyage>();
			var jobVoyOrigin = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			var jobVoyDestination = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			jobVoyOrigin[JobVoyOriginSchema.JA_JV] = jobVoy.PK;
			jobVoyDestination[JobVoyDestinationSchema.JB_JV] = jobVoy.PK;

			var jobSailing = (BusinessObject)Factory.New<Enterprise.Integration.Freight.IJobSailing>();
			jobSailing[JobSailingSchema.JX_JB] = jobVoyDestination.PK;
			jobSailing[JobSailingSchema.JX_JA] = jobVoyOrigin.PK;

			transport[JobConsolTransportSchema.JW_JX] = jobSailing.PK;
			jobVoy[JobVoyageSchema.JV_RV_NKVessel] = vessel.RV_FK;

			jobVoyOrigin[JobVoyOriginSchema.JA_E_DEP] = Convert.ToDateTime("2006-08-30 14:45:00");
			jobVoyOrigin[JobVoyOriginSchema.JA_A_DEP] = Convert.ToDateTime("2006-08-30 14:45:00");
			jobVoyDestination[JobVoyDestinationSchema.JB_E_ARV] = Convert.ToDateTime("2006-08-30 14:45:00");
			jobVoyDestination[JobVoyDestinationSchema.JB_A_ARV] = Convert.ToDateTime("2006-08-30 14:45:00");

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingConsol>(job.PK);

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, jobLoaded[JobConsolSchema.JK_ScreeningStatus].ToString());
		}

		#endregion

		#region Declaration

		public void TestVesselUpdatedToMATShouldMATDeclarationWithAllDatesNullAndLastModifiedDateWithinLookbackPeriod()
		{
			AssertScreeningStatus(ScreeningStatusesList.Codes.NotScreened, 12, 13);
			AssertScreeningStatus(ScreeningStatusesList.Codes.Matched, 12, 11);

			void AssertScreeningStatus(string expectedStatus, int lookbackPeriod, int lastEditLookbackMonth)
			{
				using (OrganisationsDataRegistry.Instance.JobUpdatePeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, lookbackPeriod))
				{
					var declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
					declaration[JobDeclarationSchema.JE_TransportMode] = Constants.TransportModes.Sea;

					var vessel = Factory.NewWithValidTestData<RefVessel>();
					var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
					((Enterprise.Integration.Freight.ITransport)transport).ParentType = declaration.GetType();
					transport[JobConsolTransportSchema.JW_ParentGUID] = declaration.PK;
					transport[JobConsolTransportSchema.JW_ParentType] = "DEC";
					transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

					var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
					jobHeader[JobHeaderSchema.JH_ParentID] = declaration.PK;
					jobHeader[JobHeaderSchema.JH_Status] = "WRK";
					Factory.Save();

					Db.Connection.Command($"UPDATE dbo.JobDeclaration SET JE_SystemLastEditTimeUtc = DATEADD(month, {-lastEditLookbackMonth}, GETUTCDATE()), JE_SystemLastEditUser = '~BP' WHERE JE_PK = '{declaration.PK}'").ExecuteNonQuery();

					var forPartyFactory = new BusinessObjectFactory();
					var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
					partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
					forPartyFactory.Save();

					AssertEquals(0, ScreeningStatusUpdater.UpdateRelatedJobs(vessel));

					var loadFactory = new BusinessObjectFactory();
					var jobLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(declaration.PK);
					AssertEquals("Screening Status", expectedStatus, jobLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
				}
			}
		}

		#endregion

		#endregion

		public void TestShouldUpdateScreeningStatus()
		{
			var dummyProvider = new DummyScreeningPartyProvider()
			{
				ScreeningStatus = ScreeningStatusesList.Codes.JobCleared,
				ShouldUpdateScreeningStatus = false
			};
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(dummyProvider, dummyProvider.ScreeningStatus, Factory, orgHeader.PK);
			AssertEquals(dummyProvider.ShouldUpdateScreeningStatus, false);

			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(dummyProvider, dummyProvider.ScreeningStatus, Factory, orgHeader.PK);
			AssertEquals(dummyProvider.ShouldUpdateScreeningStatus, false);

			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(dummyProvider, dummyProvider.ScreeningStatus, Factory, orgHeader.PK);
			AssertEquals(dummyProvider.ShouldUpdateScreeningStatus, false);

			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(dummyProvider, dummyProvider.ScreeningStatus, Factory, orgHeader.PK);
			AssertEquals(dummyProvider.ShouldUpdateScreeningStatus, true);

			dummyProvider.ShouldUpdateScreeningStatus = false;
			dummyProvider.ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(dummyProvider, dummyProvider.ScreeningStatus, Factory, orgHeader.PK);
			AssertEquals(dummyProvider.ShouldUpdateScreeningStatus, true);

			dummyProvider.ShouldUpdateScreeningStatus = false;
			dummyProvider.ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;

			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgAddress(dummyProvider, dummyProvider.ScreeningStatus, Factory, orgHeader.Addresses[0].PK);
			AssertEquals(dummyProvider.ShouldUpdateScreeningStatus, false);

			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgAddress(dummyProvider, dummyProvider.ScreeningStatus, Factory, orgHeader.Addresses[0].PK);
			AssertEquals(dummyProvider.ShouldUpdateScreeningStatus, false);

			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgAddress(dummyProvider, dummyProvider.ScreeningStatus, Factory, orgHeader.Addresses[0].PK);
			AssertEquals(dummyProvider.ShouldUpdateScreeningStatus, false);

			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgAddress(dummyProvider, dummyProvider.ScreeningStatus, Factory, orgHeader.Addresses[0].PK);
			AssertEquals(dummyProvider.ShouldUpdateScreeningStatus, true);

			dummyProvider.ShouldUpdateScreeningStatus = false;
			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			dummyProvider.ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgAddress(dummyProvider, dummyProvider.ScreeningStatus, Factory, orgHeader.Addresses[0].PK);
			AssertEquals(dummyProvider.ShouldUpdateScreeningStatus, true);
		}

		public void TestSetShouldUpdateScreeningStatus()
		{
			var consol = Factory.New<IForwardingConsol>();
			var shipment = Factory.New<IForwardingShipment>();

			((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus = false;
			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			ScreeningStatusUpdater.SetShouldUpdateScreeningStatusWhenPartAddToBizO((IShouldUpdateScreeningStatus)consol, (BusinessObject)consol, (BusinessObject)shipment);
			AssertEquals(((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus, false);

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			ScreeningStatusUpdater.SetShouldUpdateScreeningStatusWhenPartAddToBizO((IShouldUpdateScreeningStatus)consol, (BusinessObject)consol, (BusinessObject)shipment);
			AssertEquals(((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus, false);

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			ScreeningStatusUpdater.SetShouldUpdateScreeningStatusWhenPartAddToBizO((IShouldUpdateScreeningStatus)consol, (BusinessObject)consol, (BusinessObject)shipment);
			AssertEquals(((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus, true);

			((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus = false;
			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			ScreeningStatusUpdater.SetShouldUpdateScreeningStatusWhenPartAddToBizO((IShouldUpdateScreeningStatus)consol, (BusinessObject)consol, (BusinessObject)shipment);
			AssertEquals(((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus, true);

			((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus = false;
			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			ScreeningStatusUpdater.SetShouldUpdateScreeningStatusWhenPartAddToBizO((IShouldUpdateScreeningStatus)consol, (BusinessObject)consol, (BusinessObject)shipment);
			AssertEquals(((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus, true);

			((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus = false;
			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			ScreeningStatusUpdater.SetShouldUpdateScreeningStatusWhenPartRemovedFromBizO((IShouldUpdateScreeningStatus)consol, (BusinessObject)consol);
			AssertEquals(((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus, false);

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			ScreeningStatusUpdater.SetShouldUpdateScreeningStatusWhenPartRemovedFromBizO((IShouldUpdateScreeningStatus)consol, (BusinessObject)consol);
			AssertEquals(((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus, true);
		}

		public void TestGetScreenStatusUpdateTo()
		{
			var dummy = new DummyScreeningPartyProvider();
			dummy.DummyWorstScreeningStatus = ScreeningStatusesList.Codes.Clear;
			AssertEquals(ScreeningStatusesList.Codes.Clear, ScreeningStatusUpdater.GetScreenStatusUpdateTo(dummy));

			dummy.DummyWorstScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			AssertEquals(ScreeningStatusesList.Codes.Clear, ScreeningStatusUpdater.GetScreenStatusUpdateTo(dummy));

			dummy.DummyWorstScreeningStatus = ZString.Empty;
			AssertEquals(ScreeningStatusesList.Codes.Clear, ScreeningStatusUpdater.GetScreenStatusUpdateTo(dummy));

			dummy.DummyWorstScreeningStatus = ScreeningStatusesList.Codes.Matched;
			AssertEquals(ScreeningStatusesList.Codes.Matched, ScreeningStatusUpdater.GetScreenStatusUpdateTo(dummy));
		}

		public void TestDeclarationsAssociatedWithOrgOrDocAddress_IncludeInvoicePickupAddress()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<US.IJobDeclaration>();
			var invoiceHeader = Factory.New<US.IJobComInvoiceHeader>();
			invoiceHeader.JZ_JE = declaration.PK;
			var address = Factory.New<JobDocAddress>();
			address.E2_AddressType = AutoDocAddressTypes.Codes.SupplierPickupDeliveryAddress;
			address.E2_ParentTableCode = JobComInvoiceHeaderSchema.Constants.Prefix;
			address.E2_OA_Address = orgHeader.MainAddress.PK;
			address.E2_ParentID = invoiceHeader.PK;
			Factory.Save();

			var query = "SELECT * FROM DeclarationsAssociatedWithOrgOrDocAddress(@entityPK, @companyPK)";
			var results = new List<ZGuid>();

			using (var cmd = Db.Connection.Command(query))
			{
				cmd.AddParameter("@entityPK", SqlDbType.UniqueIdentifier, orgHeader.PK.ToGuid());
				cmd.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						results.Add(new ZGuid(reader[0]));
					}
				}
			}

			AssertEquals(declaration.PK, results.Single());
		}

		public void TestGetWorstPartyScreeningStatusOfDeclaration_IncludeInvoicePickupAddress()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<US.IJobDeclaration>();
			var invoiceHeader = Factory.New<US.IJobComInvoiceHeader>();
			invoiceHeader.JZ_JE = declaration.PK;
			var address = Factory.New<JobDocAddress>();
			address.E2_AddressType = AutoDocAddressTypes.Codes.SupplierPickupDeliveryAddress;
			address.E2_ParentTableCode = JobComInvoiceHeaderSchema.Constants.Prefix;
			address.E2_OA_Address = orgHeader.MainAddress.PK;
			address.E2_ParentID = invoiceHeader.PK;
			Factory.Save();

			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			var query = "SELECT * FROM GetWorstPartyScreeningStatusOfDeclaration(@jobPKs, @companyPK)";
			var results = new List<(ZGuid, string)>();

			var pkTable = new DataTable();
			pkTable.Locale = CultureInfo.InvariantCulture;
			pkTable.Columns.Add("Value", typeof(Guid));
			pkTable.Rows.Add(declaration.PK.ToGuid());

			using (var cmd = Db.Connection.Command(query))
			{
				cmd.AddTableValuedParameter("@jobPKs", "dbo.TVP_uniqueidentifier", pkTable);
				cmd.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						results.Add((new ZGuid(reader[0]), reader[1].ToString()));
					}
				}
			}

			AssertEquals((declaration.PK, ScreeningStatusesList.Codes.Clear), results.Single());
		}

		#region Invalidate Local Data Changes

		public void TestProcessInvalidateLocalDataChangesForOrganization_ShouldUpdateStatusToUnknow()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var entityProvider = (IDpsEntityProvider)organization;

			organization.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			ScreeningStatusUpdater.ProcessInvalidateLocalDataChanges(entityProvider, () => { });

			AssertEquals(ScreeningStatusesList.Codes.Unknown, organization.OH_ScreeningStatus);
			AssertEquals(true, entityProvider.ShouldUpdateRelatedJobs);
		}

		public void TestProcessInvalidateLocalDataChangesForOrganization_WhenPermanentClear_ShouldNotUpdateStatus()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			Factory.Save();

			var entityProvider = (IDpsEntityProvider)organization;

			ScreeningStatusUpdater.ProcessInvalidateLocalDataChanges(entityProvider, () => { });

			AssertEquals(ScreeningStatusesList.Codes.PermanentClear, organization.OH_ScreeningStatus);
			AssertEquals(false, entityProvider.ShouldUpdateRelatedJobs);
		}

		public void TestProcessInvalidateLocalDataChangesForVessel_ShouldUpdateStatusToUnknow()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			Factory.Save();

			var entityProvider = (IDpsEntityProvider)vessel;

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			ScreeningStatusUpdater.ProcessInvalidateLocalDataChanges(entityProvider, () => { });

			AssertEquals(ScreeningStatusesList.Codes.Unknown, vessel.RV_ScreeningStatus);
			AssertEquals(true, entityProvider.ShouldUpdateRelatedJobs);
		}

		public void TestProcessInvalidateLocalDataChangesForVessel_WhenPermanentClear_ShouldNotUpdateStatus()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			Factory.Save();

			var entityProvider = (IDpsEntityProvider)vessel;

			ScreeningStatusUpdater.ProcessInvalidateLocalDataChanges(entityProvider, () => { });

			AssertEquals(ScreeningStatusesList.Codes.PermanentClear, vessel.RV_ScreeningStatus);
			AssertEquals(false, entityProvider.ShouldUpdateRelatedJobs);
		}

		public void TestProcessInvalidateLocalDataChangesForOrganization_ShouldUpdateStatuses()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var address = organization.Addresses.AddNew();
			address.Address1 = "York St";
			address.City = "Sydney";
			address.Postcode = "2000";
			address.State = "NSW";
			Factory.Save();
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, organization.OH_ScreeningStatus);

			organization.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			organization.OH_FullName = "WiseTech";
			Factory.Save();
			AssertEquals(ScreeningStatusesList.Codes.Unknown, organization.OH_ScreeningStatus);

			organization.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			organization.OH_FullName = "WiseTech Global";
			organization.InvalidateScreeningStatuses();
			AssertEquals(ScreeningStatusesList.Codes.Unknown, organization.OH_ScreeningStatus);

			organization.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			address.Address1 = "William St";
			organization.InvalidateScreeningStatuses();
			AssertEquals(ScreeningStatusesList.Codes.Unknown, organization.OH_ScreeningStatus);

			organization.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			Factory.Save();
			address.Address1 = "Gilbert St";
			organization.InvalidateScreeningStatuses();
			AssertEquals(ScreeningStatusesList.Codes.PermanentClear, organization.OH_ScreeningStatus);

			organization.OH_IsActive = false;
			Factory.Save();
			AssertEquals(ScreeningStatusesList.Codes.PermanentClear, organization.OH_ScreeningStatus);

			organization.OH_IsActive = true;
			organization.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			Factory.Save();
			AssertEquals(ScreeningStatusesList.Codes.Unknown, organization.OH_ScreeningStatus);
		}

		public void TestProcessInvalidateLocalDataChangesForVessel_ShouldUpdateStatuses()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			Factory.Save();
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, vessel.RV_ScreeningStatus);

			vessel.RV_Name = "Maersk";
			Factory.Save();
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, vessel.RV_ScreeningStatus);

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			vessel.RV_Code = "CargoSphere";
			vessel.InvalidateScreeningStatuses();
			AssertEquals(ScreeningStatusesList.Codes.Unknown, vessel.RV_ScreeningStatus);

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			vessel.RV_LloydsNumber = "0123";
			vessel.InvalidateScreeningStatuses();
			AssertEquals(ScreeningStatusesList.Codes.Unknown, vessel.RV_ScreeningStatus);

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			vessel.RV_Name = "Voyage";
			Factory.Save();
			AssertEquals(ScreeningStatusesList.Codes.PermanentClear, vessel.RV_ScreeningStatus);

			vessel.RV_IsActive = false;
			Factory.Save();
			AssertEquals(ScreeningStatusesList.Codes.PermanentClear, vessel.RV_ScreeningStatus);

			var vessel2 = vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "Maersk Container";
			vessel.RV_IsActive = false;
			Factory.Save();
			AssertEquals(ScreeningStatusesList.Codes.Unknown, vessel.RV_ScreeningStatus);

			vessel2.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			vessel.RV_IsActive = true;
			Factory.Save();
			AssertEquals(ScreeningStatusesList.Codes.Matched, vessel.RV_ScreeningStatus);
		}

		#endregion

		#region Implementation

		void AssertSubShipmentsJobClearStatusAndReason(BusinessObject bizO, string reason = null, bool addOrDeleteParty = false)
		{
			CombineAssertions(() =>
			{
				AssertEquals(ScreeningStatusesList.Codes.JobCleared, (bizO as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEventLog(bizO);
				AssertDPSLog(bizO, reason, addOrDeleteParty);
			});
		}

		void AssertJobClearStatusAndReason(BusinessObject bizO, string reason = null, bool addOrDeleteParty = false)
		{
			ScreeningStatusUpdater.ApplyJobClearStatus(bizO, reason);
			CombineAssertions(() =>
			{
				AssertEquals(ScreeningStatusesList.Codes.JobCleared, (bizO as IScreeningStatusProvider)?.ScreeningStatus);
				AssertEventLog(bizO);
				AssertDPSLog(bizO, reason, addOrDeleteParty);
			});
		}

		void AssertEventLog(BusinessObject bizO)
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeniedPartyStatusUpdatedCode);
			var result = bizO.GetLogs().Find(query);
			AssertEquals("New Status: Job Cleared, Old Status: Not Screened, Type: Manual Screen", result.FirstOrDefault()?.DisplayEventReference);
			AssertContains("|NEW=JCL|OLD=NOT|TYP=MAN", result.FirstOrDefault()?.SL_Reference);
		}

		void AssertDPSLog(BusinessObject bizO, string reason, bool addOrDeleteParty = false)
		{
			var screeningLogCollection = (IStmEntityScreeningLogCollection)Activator.CreateInstance(ObjectFactory.GetType<IStmEntityScreeningLogCollection>(), bizO);
			var index = screeningLogCollection.Count - 1;
			var count = addOrDeleteParty ? 2 : 1;
			AssertEquals(count, screeningLogCollection.Count);
			AssertEquals("Mark As Job Clear", screeningLogCollection[index].StatusDescription);
			if (!string.IsNullOrWhiteSpace(reason))
			{
				AssertEquals(reason, screeningLogCollection[index].PJ_ClearedReason);
			}
			else
			{
				AssertEquals("Not Requested", screeningLogCollection[index].PJ_ClearedReason);
			}
		}

		void AssertDisplayEventReference(string rawScreeningStatus, string toScreeningStatus, string expectStr, string type, string score)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			DpsWorkflowTrackingEvent.AddNew(org, rawScreeningStatus, toScreeningStatus, null, type, score);
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeniedPartyStatusUpdatedCode);
			var result = org.GetLogs().Find(query);
			AssertEquals(expectStr, result.Single().DisplayEventReference);
		}

		bool rawEnableComplianceRisk;
		EnableComplianceWiseRegistryBusinessObject rawFreightComplianceWiseRegistry;

		protected override void SetUp()
		{
			base.SetUp();
			rawEnableComplianceRisk = RawDataRegistry.Instance.EnableComplianceRisk.Value;
			rawFreightComplianceWiseRegistry = FreightDataRegistry.Instance.FreightEnableComplianceWise.DefaultValue;

			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false));
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		protected override void TearDown()
		{
			base.TearDown();
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawEnableComplianceRisk);
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawFreightComplianceWiseRegistry);
		}

		#endregion
	}

	public class ScreeningStatusUpdaterPerformanceTest : TestCaseWithFactory
	{
		public void TestRelatedTransportsStatusUpdated_From_UpdateRelatedJobsForChangedParties()
		{
			GenericTestRelatedTransportsStatusUpdated_From_Method((RefVessel vessel) => ScreeningStatusUpdater.UpdateRelatedJobsForChangedParties(new BusinessObject[] { vessel }, null));
		}

		public void TestRelatedTransportsStatusUpdated_From_UpdateRelatedJobsForChangedParty()
		{
			GenericTestRelatedTransportsStatusUpdated_From_Method((RefVessel vessel) => ScreeningStatusUpdater.UpdateRelatedJobsForChangedParty(vessel, null));
		}

		public void TestGetJobScreeningStatus_WhenStatusNotScreenedMatchedUnknown_ShouldReturnBlock()
		{
			var statuses = new List<ZString>() { ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Unknown };
			AssertEquals(ScreeningStatusesList.Codes.Block, ScreeningStatusUpdater.GetJobScreeningStatus(statuses));
		}

		public void TestGetJobScreeningStatus_WhenStatusClearJobCLeared_ShouldReturnRelease()
		{
			var statuses = new List<ZString>() { ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.JobCleared };
			AssertEquals(ScreeningStatusesList.Codes.Release, ScreeningStatusUpdater.GetJobScreeningStatus(statuses));
		}

		void GenericTestRelatedTransportsStatusUpdated_From_Method(Action<RefVessel> action)
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "vessel";
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			var consol = Factory.New<IForwardingConsol>();

			var transport1 = consol.Transports_AddNew();
			var transport2 = consol.Transports_AddNew();

			transport1.JW_Vessel = vessel.RV_Name;
			transport2.JW_Vessel = "other vessel";

			Factory.Save();

			AssertEquals("Pre-condition: Vessel status", ScreeningStatusesList.Codes.Matched, vessel.RV_ScreeningStatus);
			AssertEquals("Pre-condition: Transport 1 status", ScreeningStatusesList.Codes.NotScreened, transport1.JW_VesselScreeningStatus);
			AssertEquals("Pre-condition: Transport 2 status", ScreeningStatusesList.Codes.NotScreened, transport2.JW_VesselScreeningStatus);

			action(vessel);

			var newFactory = new BusinessObjectFactory();
			transport1 = newFactory.Load<Enterprise.Integration.Freight.ITransport>(((BusinessObject)transport1).PK);
			transport2 = newFactory.Load<Enterprise.Integration.Freight.ITransport>(((BusinessObject)transport2).PK);

			AssertEquals("Expected vessel Transport status to update", ScreeningStatusesList.Codes.Matched, transport1.JW_VesselScreeningStatus);
			AssertEquals("Expected unrelated Transport status unchanged", ScreeningStatusesList.Codes.NotScreened, transport2.JW_VesselScreeningStatus);
		}
	}

	class DummyScreeningPartyProvider : IScreeningPartyProvider, IShouldUpdateScreeningStatus
	{
		public ZString ScreeningStatus { get; set; }

		public ZString? DummyWorstScreeningStatus { get; set; }

		public List<ZString> DummyStatusList { get; set; }

		public List<IScreeningPartyProvider> ScreeningPartyProviders { get; set; }

		public ZString GetWorstScreeningStatus()
		{
			var worstScreeningStatus = "Dummy";

			if (DummyWorstScreeningStatus.HasValue)
			{
				worstScreeningStatus = DummyWorstScreeningStatus.Value;
			}

			if (DummyStatusList != null)
			{
				worstScreeningStatus = ScreeningStatusUpdater.GetWorstScreeningStatus(DummyStatusList);
			}

			if (ScreeningPartyProviders != null)
			{
				worstScreeningStatus = ScreeningStatusUpdater.GetWorstScreeningStatus(ScreeningPartyProviders);
			}

			return worstScreeningStatus;
		}

		public ZString GetWorstScreeningStatusUnlessManuallyCleared()
		{
			return GetWorstScreeningStatus();
		}

		public ScreeningParty[] ScreeningParties => throw new NotImplementedException();

		public ZBool ShouldUpdateScreeningStatus { get; set; } = true;
	}
}

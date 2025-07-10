using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.PickByLabel.Testing
{
	[TestedType(typeof(WhsPickByLabelJob))]
	internal class WhsPickByLabelJobTest : WhsBusinessObjectTestCase
	{
		#region TestPerformance_FinaliseOrdersAndSave_WithPickByLabelJobs

		public void TestPerformance_FinalisePickAndSave_WithPickByLabelJobsDbHits() => TestPerformance_FinalisePickAndSave_WithPickByLabelJobsDbHits_Core();

		public void TestPerformance_FinalisePickAndSave_WithPickByLabelJobsDbHits_HeldGoodsForOrdersDisabled() => TestPerformance_FinalisePickAndSave_WithPickByLabelJobsDbHits_Core(enableHeldGoodsForOrders: false);

		void TestPerformance_FinalisePickAndSave_WithPickByLabelJobsDbHits_Core(bool enableHeldGoodsForOrders = true)
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableHeldGoodsForOrders))
			{
				var expectedNoMoreThan_5_DBHitsPerTable = new Dictionary<string, int>
				{
					{ JobDocAddressSchema.Constants.TableName, 7 },
					{ ProcessTaskNotificationSchema.Constants.TableName, 8 },
					{ WhsDocketLineSchema.Constants.TableName, 14 },
					{ WhsPickLineSchema.Constants.TableName, 17 },
					{ StmALogSchema.Constants.TableName, 114 }, // to be addressed in the future possibly by Workflow team.
					{ GenAddOnColumnSchema.Constants.TableName, 7 }
				};

				TestPerformance_WithPickByLabelJobs(expectedNoMoreThan_5_DBHitsPerTable,
					(pick) => // prepare data
					{
						pick.FinaliseAllOrders();
						pick.Factory.Save();
						AssertEquals("Ensure functionality worked.", true, pick.Orders.Cast<WhsOrder>().All(o => o.IsFinalised));
					},
					(pick) => // test performance
					{
						pick.FinalisePick();
						pick.Factory.Save();
						AssertEquals("Ensure functionality worked.", true, pick.IsFinalised);
					});
			}
		}

		public void TestPerformance_FinaliseOrderAndPickAndSave_WithPickByLabelJobsDBHits()
		{
			var expectedNoMoreThan_5_DBHitsPerTable = new Dictionary<string, int>
			{
				{ JobDocAddressSchema.Constants.TableName, 9 },
				{ ProcessTaskNotificationSchema.Constants.TableName, 8 },
				{ WhsDocketSchema.Constants.TableName, 6 },
				{ WhsDocketLineSchema.Constants.TableName, 17 },
				{ WhsPickLineSchema.Constants.TableName, 17 },
				{ StmALogSchema.Constants.TableName, 114 }, // to be addressed in the future possibly by Workflow team.
				{ GenAddOnColumnSchema.Constants.TableName, 8 }
			};

			TestPerformance_WithPickByLabelJobs(expectedNoMoreThan_5_DBHitsPerTable,
				(pick) => // prepare data
				{
				},
				(pick) => // test performance
				{
					pick.FinaliseAllOrders();
					pick.FinalisePick();
					pick.Factory.Save();
					AssertEquals("Ensure functionality worked.", true, pick.IsFinalised);
				});
		}

		void TestPerformance_WithPickByLabelJobs(Dictionary<string, int> expectedNoMoreThan_5_DBHitsPerTable, Action<WhsPick> preparePick, Action<WhsPick> testPick)
		{
			const int NumberOfOrders = 100;

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Box)).F3_UOMType = UOMPackTypesList.Codes.Case;

			data.Part1.PartUnits.DeleteAll();
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Box, 5m);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Notify);
			for (int i = 0; i < NumberOfOrders; i++)
			{
				Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, 5m, "A", arrival: ZDateTimeOffset.Today.AddHours(-i));
			}
			adjustment.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustment);
			Factory.Save();

			var pick = Helper.CreatePickNew();
			pick.WP_PickCasesByLabel = true;
			for (int i = 0; i < NumberOfOrders; i++)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O" + i, data.Part1, 5m);
				pick.Orders.Add(order);
			}

			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			pick.AllocatePackageLabels();
			Factory.Save();
			var packages = pick.Orders.Cast<WhsOrder>().SelectMany(o => o.PackageJob.Packages).ToArray();
			AssertEquals("Precondition", NumberOfOrders, packages.Length);

			var pickByLabelJobs = new List<WhsPickByLabelJob>();
			for (int i = 0; i < 10; i++)
			{
				var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "E", data.Whs1.WW_DefaultOutboundDockDoor);
				for (int j = 0; j < NumberOfOrders / 10; j++)
				{
					WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, packages[i * 10 + j].PK);
				}
				pickByLabelJobs.Add(pickByLabelJob);
			}

			foreach (var pickLine in pick.GetAllPickLines())
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			}

			foreach (var pickByLabelJob in pickByLabelJobs)
			{
				WhsPickByLabelHelper.PutawayPickedLabelsAndSplitJobIfNecessary(pickByLabelJob);
				AssertEquals("Precondition", true, !pickByLabelJob.WTK_FinalisedDate.IsEmpty);
			}
			Factory.Save();

			preparePick(pick);

			var otherFactory = new BusinessObjectFactory();
			var pickInOtherFactory = otherFactory.Load<WhsPick>(pick.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedNoMoreThan_5_DBHitsPerTable, otherFactory, true))
			{
				testPick(pickInOtherFactory);
			}
		}

		public void TestPerformance_MCM()
		{
			const int NumberOfOrders = 100;

			var data = new TestDataSimpleEnvironment(Factory, NumberOfOrders, 5);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Box)).F3_UOMType = UOMPackTypesList.Codes.Case;

			data.Part1.PartUnits.DeleteAll();
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Box, 5m);

			var printer = Factory.New<IStmPrintQueue>();

			// setting up simplified version of MCM workflow templates.
			var addOnRuleDateTime = Factory.New<GenCustomAddOnRule>();
			addOnRuleDateTime.XR_Code = "DateTime";
			addOnRuleDateTime.XR_IsActive = true;
			addOnRuleDateTime.XR_SourceCode = @"<sourceCode>
  <rules>
    <rule code=""DateTimeFormat"">
      <details>
        <format>Long</format>
      </details>
    </rule>
  </rules>
</sourceCode>";

			var addOnRuleString = Factory.New<GenCustomAddOnRule>();
			addOnRuleString.XR_Code = "String";
			addOnRuleString.XR_IsActive = true;
			addOnRuleString.XR_SourceCode = @"<sourceCode>
  <rules>
    <rule code=""CheckEntered"" enabled=""false"">
      <details />
    </rule>
    <rule code=""DateTimeFormat"" enabled=""false"">
      <details>
        <format>Short</format>
      </details>
    </rule>
    <rule code=""CreateEvent"" enabled=""false"">
      <details>
        <CreateEventRuleCode>Z97</CreateEventRuleCode>
      </details>
    </rule>
    <rule code=""InvalidCode"" enabled=""false"">
      <details />
    </rule>
  </rules>
</sourceCode>";

			var addOnRuleBoolean = Factory.New<GenCustomAddOnRule>();
			addOnRuleBoolean.XR_Code = "Boolean";
			addOnRuleBoolean.XR_IsActive = true;
			addOnRuleBoolean.XR_SourceCode = @"<sourceCode>
  <rules>
    <rule code=""CheckEntered"" enabled=""false"">
      <details />
    </rule>
    <rule code=""DateTimeFormat"" enabled=""false"">
      <details>
        <format>Short</format>
      </details>
    </rule>
    <rule code=""CreateEvent"" enabled=""false"">
      <details>
        <CreateEventRuleCode>Z97</CreateEventRuleCode>
      </details>
    </rule>
    <rule code=""InvalidCode"" enabled=""false"">
      <details>
        <codeDescriptionList>
          <codeDescription code=""SEND"" description=""SEND"" />
        </codeDescriptionList>
      </details>
    </rule>
  </rules>
</sourceCode>";

			var warehousePickTemplate = Helper.CreateWorkflowTemplate("PICK1", WorkflowDescriptors.WhsPickWorkflowDescriptorCode, client: data.Org1);
			Helper.CreateWorkflowMilestone(warehousePickTemplate.WorkflowItems, "M1", 1, AutoEvents.PackingCommencedCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			Helper.CreateWorkflowMilestone(warehousePickTemplate.WorkflowItems, "M2", 2, AutoEvents.PackingCompletedCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			var warehousePickTrigger = Helper.CreateWorkflowTrigger(warehousePickTemplate.WorkflowItems, "T1", 1, AutoEvents.WorkflowTemplateAppliedCode);
			Helper.CreateWorkflowNotification(warehousePickTrigger, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: WhsPickSchema.Constants.WP_PickCasesByLabel, fieldValue: "TRUE");
			Helper.CreateWorkflowNotification(warehousePickTrigger, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: WhsPickSchema.Constants.WP_ForcePickByCaseUOMTypeAllocation, fieldValue: "TRUE");

			var warehouseReceiveTemplate1 = Helper.CreateWorkflowTemplate("RECEIVE1", WorkflowDescriptors.WhsReceiveWorkflowDescriptorCode);
			var warehouseReceive1Trigger = Helper.CreateWorkflowTrigger(warehouseReceiveTemplate1.WorkflowItems, "T1", 1, AutoEvents.EditedARecordCode);
			Helper.CreateWorkflowNotification(warehouseReceive1Trigger, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "<Containers.WC_ItemCount>", fieldValue: "<GetCustomField(Total Line Items)>");
			Helper.CreateWorkflowNotification(warehouseReceive1Trigger, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "<GetCustomField(Total Line Items)>", fieldValue: "<Multiply(\"<WD_TotalUnitsFromLines>\",\"1\")>");
			Helper.AddCustomField(warehouseReceiveTemplate1, "Total Line Items", AddOnColumnDataType.Codes.String);
			Helper.AddCustomField(warehouseReceiveTemplate1, "Container Type", AddOnColumnDataType.Codes.String);

			var warehouseReceiveTemplate2 = Helper.CreateWorkflowTemplate("RECEIVE2", WorkflowDescriptors.WhsReceiveWorkflowDescriptorCode);
			Helper.CreateWorkflowMilestone(warehouseReceiveTemplate2.WorkflowItems, "M1", 1, AutoEvents.WarehouseJobEnteredCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			Helper.CreateWorkflowMilestone(warehouseReceiveTemplate2.WorkflowItems, "M2", 2, AutoEvents.WarehouseReceiptETANotificationCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			Helper.CreateWorkflowMilestone(warehouseReceiveTemplate2.WorkflowItems, "M3", 3, AutoEvents.WarehouseReceiptArrivedCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			Helper.CreateWorkflowMilestone(warehouseReceiveTemplate2.WorkflowItems, "M4", 4, AutoEvents.WarehouseReceiptUnloadedCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			Helper.CreateWorkflowMilestone(warehouseReceiveTemplate2.WorkflowItems, "M5", 5, AutoEvents.WarehouseReceiptPuttingAwayCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			var warehouseReceive2Milestone = Helper.CreateWorkflowMilestone(warehouseReceiveTemplate2.WorkflowItems, "M6", 6, AutoEvents.ItemDocumentJobFinalisedCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			var receiveConfirmationDocumentQuery = new ZQuery(StmMenuItemSchema.SU_MenuName, "Receive Confirmation");
			receiveConfirmationDocumentQuery.AddToFilter(StmMenuItemSchema.SU_MenuPath, ""); // new, not legacy document
			var receiveConfirmationDocument = Factory.LoadTop1<StmMenuItem>(receiveConfirmationDocumentQuery);
			Helper.CreateWorkflowNotification(warehouseReceive2Milestone, WorkflowTriggerActionTypeConstants.Codes.SendDocument, recepient: MessageRecipientPartyTypeList.Codes.AutoDocumentDelivery, document: receiveConfirmationDocument.PK);
			Helper.CreateWorkflowNotification(warehouseReceive2Milestone, WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML, recepient: MessageRecipientPartyTypeList.Codes.Client);
			Helper.CreateWorkflowNotification(warehouseReceive2Milestone, WorkflowTriggerActionTypeConstants.Codes.AutoRateRevenue);
			var warehouseReceive2Trigger1 = Helper.CreateWorkflowTrigger(warehouseReceiveTemplate2.WorkflowItems, "T1", 1, AutoEvents.EditedARecordCode);
			var containerType = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40FR"));
			Helper.CreateWorkflowNotification(warehouseReceive2Trigger1, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "<Containers.WC_RC>", fieldValue: containerType.PK.ToString());
			var warehouseReceive2Trigger2 = Helper.CreateWorkflowTrigger(warehouseReceiveTemplate2.WorkflowItems, "T2", 2, AutoEvents.EditedARecordCode);
			Helper.CreateWorkflowNotification(warehouseReceive2Trigger2, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "<Containers.WC_ItemCount>", fieldValue: "<GetCustomField(Total Line Items)>");
			Helper.CreateWorkflowNotification(warehouseReceive2Trigger2, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "<GetCustomField(Total Line Items)>", fieldValue: "<Multiply(\"<WD_TotalUnitsFromLines>\",\"1\")>");
			Helper.AddCustomField(warehouseReceiveTemplate2, "MISSED SLA", AddOnColumnDataType.Codes.Datetime, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "INSUFFICIENT RESOURCING", AddOnColumnDataType.Codes.Datetime, displaySequence: 1, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "SPACE MANAGEMENT", AddOnColumnDataType.Codes.Datetime, displaySequence: 2, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "NO ASN(F)", AddOnColumnDataType.Codes.Datetime, displaySequence: 3, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "INCORRECT SKU(S)-ENTIRE ASN(F)", AddOnColumnDataType.Codes.Datetime, displaySequence: 4, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "INCORRECT SKU(S)-PARTIAL ASN(T)", AddOnColumnDataType.Codes.Datetime, displaySequence: 5, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "UNPLANNED ARRIVAL(T)", AddOnColumnDataType.Codes.Datetime, displaySequence: 6, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "PRODUCT NOT IN SYSTEM(F)", AddOnColumnDataType.Codes.Datetime, displaySequence: 7, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "NON - COMPLIANT", AddOnColumnDataType.Codes.Datetime, displaySequence: 8, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "CONTAMINATION", AddOnColumnDataType.Codes.Datetime, displaySequence: 9, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "LARGE VOLUMES", AddOnColumnDataType.Codes.Datetime, displaySequence: 10, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "NO ASN(T)", AddOnColumnDataType.Codes.Datetime, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "INCORRECT SKU(S)-ENTIRE ASN(T)", AddOnColumnDataType.Codes.Datetime, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "INCORRECT SKU(S)-PARTIAL ASN(F)", AddOnColumnDataType.Codes.Datetime, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "UNPLANNED ARRIVAL(F)", AddOnColumnDataType.Codes.Datetime, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "PRODUCT NOT IN SYSTEM(T)", AddOnColumnDataType.Codes.Datetime, addOnRule: addOnRuleDateTime);

			var warehouseReceiveTemplate3 = Helper.CreateWorkflowTemplate("RECEIVE3", WorkflowDescriptors.WhsReceiveWorkflowDescriptorCode);
			Helper.CreateWorkflowMilestone(warehouseReceiveTemplate3.WorkflowItems, "M1", 1, AutoEvents.WarehouseJobEnteredCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			Helper.CreateWorkflowMilestone(warehouseReceiveTemplate3.WorkflowItems, "M2", 2, AutoEvents.WarehouseReceiptETANotificationCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			Helper.CreateWorkflowMilestone(warehouseReceiveTemplate3.WorkflowItems, "M3", 3, AutoEvents.WarehouseReceiptArrivedCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			Helper.CreateWorkflowMilestone(warehouseReceiveTemplate3.WorkflowItems, "M4", 4, AutoEvents.WarehouseReceiptUnloadedCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			Helper.CreateWorkflowMilestone(warehouseReceiveTemplate3.WorkflowItems, "M5", 5, AutoEvents.WarehouseReceiptPuttingAwayCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			var warehouseReceive3Milestone = Helper.CreateWorkflowMilestone(warehouseReceiveTemplate3.WorkflowItems, "M6", 6, AutoEvents.ItemDocumentJobFinalisedCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			Helper.CreateWorkflowNotification(warehouseReceive3Milestone, WorkflowTriggerActionTypeConstants.Codes.AutoRateRevenue);
			var warehouseReceive3Trigger = Helper.CreateWorkflowTrigger(warehouseReceiveTemplate3.WorkflowItems, "T1", 1, AutoEvents.WarehouseJobEnteredCode);
			Helper.CreateWorkflowNotification(warehouseReceive3Trigger, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "<Containers.WC_RC>", fieldValue: containerType.PK.ToString());

			var warehouseReleaseTemplate1 = Helper.CreateWorkflowTemplate("RELEASE1", WorkflowDescriptors.WhsOrderWorkflowDescriptorCode);
			Helper.CreateWorkflowMilestone(warehouseReleaseTemplate1.WorkflowItems, "M1", 1, AutoEvents.WarehouseJobEnteredCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			Helper.CreateWorkflowMilestone(warehouseReleaseTemplate1.WorkflowItems, "M2", 2, AutoEvents.WarehouseOrderPickingCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			Helper.CreateWorkflowMilestone(warehouseReleaseTemplate1.WorkflowItems, "M3", 3, AutoEvents.ItemDocumentJobFinalisedCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			var warehouseRelease1Milestone = Helper.CreateWorkflowMilestone(warehouseReleaseTemplate1.WorkflowItems, "M4", 4, AutoEvents.ReleasedCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			Helper.CreateWorkflowNotification(warehouseRelease1Milestone, WorkflowTriggerActionTypeConstants.Codes.AutoRateRevenue);
			Helper.AddCustomField(warehouseReleaseTemplate1, "REQUEST FROM", AddOnColumnDataType.Codes.Datetime, addOnRule: addOnRuleDateTime, displaySequence: 0);
			Helper.AddCustomField(warehouseReleaseTemplate1, "REQUESTED TO", AddOnColumnDataType.Codes.Datetime, addOnRule: addOnRuleDateTime, displaySequence: 1);
			Helper.AddCustomField(warehouseReleaseTemplate1, "PICKUP REQUIRED", AddOnColumnDataType.Codes.Datetime, addOnRule: addOnRuleDateTime, displaySequence: 2);

			var warehouseReleaseTemplate2 = Helper.CreateWorkflowTemplate("RELEASE2", WorkflowDescriptors.WhsOrderWorkflowDescriptorCode, isPartial: true);
			Helper.CreateWorkflowMilestone(warehouseReleaseTemplate2.WorkflowItems, "M1", 92, AutoEvents.CustomisableEvent50Code, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);

			var warehouseReleaseTemplate3 = Helper.CreateWorkflowTemplate("RELEASE3", WorkflowDescriptors.WhsOrderWorkflowDescriptorCode, isPartial: true);
			var warehouseRelease3Milestone = Helper.CreateWorkflowMilestone(warehouseReleaseTemplate3.WorkflowItems, "M1", 93, AutoEvents.DeliveredCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			Helper.CreateWorkflowNotification(warehouseRelease3Milestone, WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML, recepient: MessageRecipientPartyTypeList.Codes.Client);

			var warehouseReleaseTemplate4 = Helper.CreateWorkflowTemplate("RELEASE4", WorkflowDescriptors.WhsOrderWorkflowDescriptorCode, isPartial: true);
			Helper.CreateWorkflowMilestone(warehouseReleaseTemplate4.WorkflowItems, "M1", 120, AutoEvents.CustomisableEvent53Code, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);

			var warehouseReleaseTemplate5 = Helper.CreateWorkflowTemplate("RELEASE5", WorkflowDescriptors.WhsOrderWorkflowDescriptorCode, isPartial: true);
			Helper.CreateWorkflowMilestone(warehouseReleaseTemplate5.WorkflowItems, "M1", 91, AutoEvents.CustomisableEvent51Code, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);

			var warehouseReleaseTemplate6 = Helper.CreateWorkflowTemplate("RELEASE6", WorkflowDescriptors.WhsOrderWorkflowDescriptorCode, isPartial: true);
			Helper.CreateWorkflowMilestone(warehouseReleaseTemplate6.WorkflowItems, "M1", 90, AutoEvents.CustomisableEvent52Code, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);

			var warehouseReleaseTemplate7 = Helper.CreateWorkflowTemplate("RELEASE7", WorkflowDescriptors.WhsOrderWorkflowDescriptorCode, client: data.Org1);
			var warehouseRelease7Milestone1 = Helper.CreateWorkflowMilestone(warehouseReleaseTemplate7.WorkflowItems, "M1", 10, AutoEvents.WarehouseJobEnteredCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			Helper.CreateWorkflowNotification(warehouseRelease7Milestone1, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "WD_RequiredDate", fieldValue: "<now>");
			Helper.CreateWorkflowMilestone(warehouseReleaseTemplate7.WorkflowItems, "M2", 20, AutoEvents.WarehouseOrderPickingCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			var warehouseRelease7Milestone3 = Helper.CreateWorkflowMilestone(warehouseReleaseTemplate7.WorkflowItems, "M3", 30, AutoEvents.PackingCompletedCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			Helper.CreateWorkflowNotification(warehouseRelease7Milestone3, WorkflowTriggerActionTypeConstants.Codes.AutoRateRevenue);
			var warehouseRelease7Milestone4 = Helper.CreateWorkflowMilestone(warehouseReleaseTemplate7.WorkflowItems, "M4", 40, AutoEvents.ItemDocumentJobFinalisedCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			Helper.CreateWorkflowNotification(warehouseRelease7Milestone4, WorkflowTriggerActionTypeConstants.Codes.AutoRateRevenue);
			Helper.CreateWorkflowMilestone(warehouseReleaseTemplate7.WorkflowItems, "M5", 99, AutoEvents.ItemDocumentJobFinalisedCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			var warehouseRelease7Milestone6 = Helper.CreateWorkflowMilestone(warehouseReleaseTemplate7.WorkflowItems, "M6", 110, AutoEvents.DocumentImportedCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			warehouseRelease7Milestone6.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			warehouseRelease7Milestone6.TriggerConditions.TriggerConditionValue = "LAB";
			Helper.CreateWorkflowMilestone(warehouseReleaseTemplate7.WorkflowItems, "M7", 111, AutoEvents.CustomisableEvent09Code, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			Helper.CreateWorkflowMilestone(warehouseReleaseTemplate7.WorkflowItems, "M8", 112, AutoEvents.ReleasedCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			var warehouseRelease7Milestone9 = Helper.CreateWorkflowMilestone(warehouseReleaseTemplate7.WorkflowItems, "M9", 113, AutoEvents.ItemDocumentJobFinalisedCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			Helper.CreateWorkflowNotification(warehouseRelease7Milestone9, WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML, recepient: MessageRecipientPartyTypeList.Codes.Client).PQ_MessagePurpose = "APP";
			Helper.CreateWorkflowMilestone(warehouseReleaseTemplate7.WorkflowItems, "M10", 114, AutoEvents.MessageRejectedCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);
			var warehouseRelease7Trigger1 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T1", 1, AutoEvents.CustomisableEvent50Code);
			var t1Notify = Helper.CreateWorkflowNotification(warehouseRelease7Trigger1, WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce);
			t1Notify.PQ_P0_WorkflowTemplate = warehouseReleaseTemplate7.PK;
			var warehouseRelease7Trigger2 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T2", 1, AutoEvents.EditedARecordCode, EventReferenceConditionList.Codes.ConditionWithMacros, "Source.TransportCo.OH_Code==\"STATRAMEL\"");
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger2, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "WD_PL_NKCarrierServiceLevel", fieldValue: "EXP");
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger2, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "WD_IsAuthorisedToLeave", fieldValue: "TRUE");
			var warehouseRelease7Trigger3 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T3", 1, AutoEvents.DataImportCode);
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger3, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "WD_RequiredDate", fieldValue: "<now>");
			var warehouseRelease7Trigger4 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T4", 2, AutoEvents.CustomisableEvent51Code);
			var t4Notify = Helper.CreateWorkflowNotification(warehouseRelease7Trigger4, WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce);
			t4Notify.PQ_P0_WorkflowTemplate = warehouseReleaseTemplate7.PK;
			var warehouseRelease7Trigger5 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T5", 2, AutoEvents.DataImportCode, EventReferenceConditionList.Codes.ConditionWithMacros,
				"(Source.TransportCo.OH_Code==\"MCMLOGMEL\" || Source.TransportCo.OH_Code==\"AUSPOSMEL\" || Source.TransportCo.OH_Code == none) && Source.WD_TotalUnitsFromLines>=2 &&  (Source.Warehouse.WarehouseAddress.OA_State==\"WA\" ||  Source.Warehouse.WarehouseAddress.OA_State==\"SA\" || Source.Warehouse.WarehouseAddress.OA_State==\"QLD\" || Source.Warehouse.WarehouseAddress.OA_State==\"NSW\" || Source.Warehouse.WarehouseAddress.OA_State==\"VIC\")");
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger5, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "TransportCoDocAddress.OrganisationPK", fieldValue: data.Org1.PK.ToString());
			var warehouseRelease7Trigger6 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T6", 2, AutoEvents.DataImportCode, EventReferenceConditionList.Codes.ConditionWithMacros,
				"(Source.TransportCo.OH_Code==\"MCMLOGMEL\" || Source.TransportCo.OH_Code == none)  && Source.WD_TotalUnitsFromLines<2 && (Source.Warehouse.WarehouseAddress.OA_State==\"WA\" ||  Source.Warehouse.WarehouseAddress.OA_State==\"SA\" || Source.Warehouse.WarehouseAddress.OA_State==\"QLD\" || Source.Warehouse.WarehouseAddress.OA_State==\"NSW\" || Source.Warehouse.WarehouseAddress.OA_State==\"VIC\")");
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger5, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "TransportCoDocAddress.OrganisationPK", fieldValue: data.Org1.PK.ToString());
			Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T7", 3, AutoEvents.ServiceCompletedCode);
			Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T8", 4, AutoEvents.PackingCompletedCode);
			var warehouseRelease7Trigger9 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T9", 5, AutoEvents.EditedARecordCode, EventReferenceConditionList.Codes.ConditionWithMacros,
				"Source.TransportCo.OH_Code==\"COUPLEMEL\"");
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger9, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "<WD_TransportReference>", fieldValue: "CPBXM4Z<WD_DocketID>");
			var warehouseRelease7Trigger10 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T10", 7, AutoEvents.CustomisableEvent52Code);
			var t10Notify = Helper.CreateWorkflowNotification(warehouseRelease7Trigger10, WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce);
			t10Notify.PQ_P0_WorkflowTemplate = warehouseReleaseTemplate7.PK;
			var warehouseRelease7Trigger11 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T11", 8, AutoEvents.EditedARecordCode);
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger11, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "<GetCustomField(POST CODE)>", fieldValue: "<ConsigneeDocAddress.E2_Postcode>");
			var warehouseRelease7Trigger12 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T12", 9, AutoEvents.CustomisableEvent12Code, EventReferenceConditionList.Codes.ConditionWithMacros,
				"(Source.TransportCo.OH_Code==\"AUSPOSMEL\" && Source.WD_PL_NKCarrierServiceLevel==\"10\") || (Source.TransportCo.OH_Code==\"STATRAMEL\" && Source.WD_PL_NKCarrierServiceLevel==\"EXP\")");
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger12, WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML, recepient: MessageRecipientPartyTypeList.Codes.TransportCo).PQ_MessagePurpose = "APP";
			var warehouseRelease7Trigger13 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T13", 10, AutoEvents.AddressValidationStatusCode);
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger13, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "<GetCustomField(AddressValidationFailed)>", fieldValue: "TRUE");
			var warehouseRelease7Trigger14 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T14", 11, AutoEvents.AddressValidationStatusCode);
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger14, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "<GetCustomField(AddressValidationFailed)>", fieldValue: "");
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger14, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "<GetCustomField(ADDRESS ISSUE)>", fieldValue: "<now>");
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger14, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "WD_RequiredDate", fieldValue: "<now>");
			var warehouseRelease7Trigger15 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T15", 12, AutoEvents.DataImportCode, EventReferenceConditionList.Codes.ConditionWithMacros, "Source.TransportCo.OH_Code == \"1234\"");
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger15, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "WD_PackingAfterPickingRequired", fieldValue: "true");
			var warehouseRelease7Trigger16 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T16", 13, AutoEvents.MessageRejectedCode, EventReferenceConditionList.Codes.ConditionWithMacros, "Source.TransportCo.OH_Code==\"AUSPOSMEL\"");
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger16, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, recepient: MessageRecipientPartyTypeList.Codes.Email).PQ_EmailAddr = "111@111.com";
			var warehouseRelease7Trigger17 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T17", 16, AutoEvents.CustomisableEvent09Code, EventReferenceConditionList.Codes.ConditionWithMacros, "Source.Warehouse.WarehouseAddress.OA_State==\"NSW\"");
			var carrierLabelDocumentQuery = new ZQuery(StmMenuItemSchema.SU_MenuName, "Carrier Label (Top Level)");
			carrierLabelDocumentQuery.AddToFilter(StmMenuItemSchema.SU_BusinessContext, "WhsOrder");
			var carrierLabelDocument = Factory.LoadTop1<StmMenuItem>(carrierLabelDocumentQuery);
			var t17Notify = Helper.CreateWorkflowNotification(warehouseRelease7Trigger17, WorkflowTriggerActionTypeConstants.Codes.SendDocument, recepient: MessageRecipientPartyTypeList.Codes.Print, document: carrierLabelDocument.PK); //needs printer?
			t17Notify.PQ_SQ = printer.PK;
			var warehouseRelease7Trigger18 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T18", 17, AutoEvents.CustomisableEvent09Code, EventReferenceConditionList.Codes.ConditionWithMacros, "Source.Warehouse.WarehouseAddress.OA_State==\"VIC\"");
			var t18Notify = Helper.CreateWorkflowNotification(warehouseRelease7Trigger18, WorkflowTriggerActionTypeConstants.Codes.SendDocument, recepient: MessageRecipientPartyTypeList.Codes.Print, document: carrierLabelDocument.PK);
			t18Notify.PQ_SQ = printer.PK;
			var warehouseRelease7Trigger19 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T19", 17, AutoEvents.CustomisableEvent09Code, EventReferenceConditionList.Codes.ConditionWithMacros, "Source.Warehouse.WarehouseAddress.OA_State==\"QLD\"");
			var t19Notify = Helper.CreateWorkflowNotification(warehouseRelease7Trigger19, WorkflowTriggerActionTypeConstants.Codes.SendDocument, recepient: MessageRecipientPartyTypeList.Codes.Print, document: carrierLabelDocument.PK);
			t19Notify.PQ_SQ = printer.PK;
			var warehouseRelease7Trigger20 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T20", 18, AutoEvents.CustomisableEvent09Code, EventReferenceConditionList.Codes.ConditionWithMacros, "Source.Warehouse.WarehouseAddress.OA_State==\"SA\"");
			var t20Notify = Helper.CreateWorkflowNotification(warehouseRelease7Trigger20, WorkflowTriggerActionTypeConstants.Codes.SendDocument, recepient: MessageRecipientPartyTypeList.Codes.Print, document: carrierLabelDocument.PK);
			t20Notify.PQ_SQ = printer.PK;
			var warehouseRelease7Trigger21 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T21", 19, AutoEvents.CustomisableEvent09Code, EventReferenceConditionList.Codes.ConditionWithMacros, "Source.Warehouse.WarehouseAddress.OA_State==\"WA\"");
			var t21Notify = Helper.CreateWorkflowNotification(warehouseRelease7Trigger21, WorkflowTriggerActionTypeConstants.Codes.SendDocument, recepient: MessageRecipientPartyTypeList.Codes.Print, document: carrierLabelDocument.PK);
			t21Notify.PQ_SQ = printer.PK;
			var warehouseRelease7Trigger22 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T22", 20, AutoEvents.CustomisableEvent53Code);
			var t22Notify = Helper.CreateWorkflowNotification(warehouseRelease7Trigger22, WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce);
			t22Notify.PQ_P0_WorkflowTemplate = warehouseReleaseTemplate7.PK;
			var warehouseRelease7Trigger23 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T23", 21, AutoEvents.DataImportCode, EventReferenceConditionList.Codes.ConditionWithMacros, "Source.TransportCo.OH_Code==\"COUPLEMEL\"");
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger23, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "<GetCustomField(CouriersPleaseWave)>", fieldValue: "<TransportCoDocAddress.Address.AddressCode>");
			var warehouseRelease7Trigger24 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T24", 24, AutoEvents.EditedARecordCode, EventReferenceConditionList.Codes.ConditionWithMacros, "Source.TransportCo.OH_Code==\"ROARUNTAS\"");
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger24, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "<WD_TransportReference>", fieldValue: "TSL<WD_DocketID>");
			var warehouseRelease7Trigger25 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T25", 25, AutoEvents.DataImportCode, EventReferenceConditionList.Codes.ConditionWithMacros, "Source.Warehouse.WarehouseAddress.OA_State==\"TAS\"");
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger25, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "TransportCoDocAddress.OrganisationPK", fieldValue: data.Org1.PK.ToString());
			var warehouseRelease7Trigger26 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T26", 26, AutoEvents.CustomisableEvent90Code, EventReferenceConditionList.Codes.ConditionWithMacros, "Source.TransportCo.OH_Code==\"ROARUNTAS\"");
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger26, WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML, recepient: MessageRecipientPartyTypeList.Codes.TransportCo).PQ_MessagePurpose = "APP";
			var warehouseRelease7Trigger27 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T27", 61, AutoEvents.EditedARecordCode, EventReferenceConditionList.Codes.ConditionWithMacros, "Source.TransportCo.OH_Code==\"AUSPOSMEL\"");
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger27, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "WD_PL_NKCarrierServiceLevel", fieldValue: "10");
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger27, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "WD_IsAuthorisedToLeave", fieldValue: "true");
			var warehouseRelease7Trigger28 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T28", 62, AutoEvents.DataImportCode, EventReferenceConditionList.Codes.ConditionWithMacros,
				"(Source.TransportCo.OH_Code==\"COUPLEMEL\" || Source.TransportCo.OH_Code == none) && Source.WD_TotalUnitsFromLines>5 && (Source.Warehouse.WarehouseAddress.OA_State==\"WA\" ||  Source.Warehouse.WarehouseAddress.OA_State==\"SA\" || Source.Warehouse.WarehouseAddress.OA_State==\"QLD\" || Source.Warehouse.WarehouseAddress.OA_State==\"NSW\" || Source.Warehouse.WarehouseAddress.OA_State==\"VIC\")");
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger28, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "TransportCoDocAddress.OrganisationPK", fieldValue: data.Org1.PK.ToString());
			var warehouseRelease7Trigger29 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T29", 63, AutoEvents.DataImportCode, EventReferenceConditionList.Codes.ConditionWithMacros,
				"(Source.TransportCo.OH_Code==\"MCMLOGMEL - DISCOUNTINUED\" || Source.TransportCo.OH_Code==none) && Source.Warehouse.WarehouseAddress.OA_State==\"QLD - TURN OFF\"");
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger29, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "TransportCoDocAddress.OrganisationPK", fieldValue: data.Org1.PK.ToString());
			var warehouseRelease7Trigger30 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T30", 64, AutoEvents.DataImportCode);
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger30, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "<GetCustomField(SUBURB)>", fieldValue: "<ConsigneeDocAddress.E2_City>");
			var warehouseRelease7Trigger31 = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate7.WorkflowItems, "T31", 65, AutoEvents.DataImportCode, EventReferenceConditionList.Codes.ConditionWithMacros, "Source.ShortfallExists == \"Y\"");
			Helper.CreateWorkflowNotification(warehouseRelease7Trigger31, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "<GetCustomField(SHORTFALL TEST)>", fieldValue: "<now>");
			Helper.AddCustomField(warehouseReceiveTemplate2, "POST CODE", AddOnColumnDataType.Codes.String, displaySequence: 0);
			Helper.AddCustomField(warehouseReceiveTemplate2, "CouriersPleaseWave", AddOnColumnDataType.Codes.String, displaySequence: 2);
			Helper.AddCustomField(warehouseReceiveTemplate2, "AddressValidationFailed", AddOnColumnDataType.Codes.String, displaySequence: 2);
			Helper.AddCustomField(warehouseReceiveTemplate2, "Request Auspost Labels", AddOnColumnDataType.Codes.String, displaySequence: 3);
			Helper.AddCustomField(warehouseReceiveTemplate2, "Print Auspost Labels", AddOnColumnDataType.Codes.String, displaySequence: 4);
			Helper.AddCustomField(warehouseReceiveTemplate2, "BOOKED (IFS)", AddOnColumnDataType.Codes.String, displaySequence: 5);
			Helper.AddCustomField(warehouseReceiveTemplate2, "KINGS JOB NUMBER", AddOnColumnDataType.Codes.String, displaySequence: 6);
			Helper.AddCustomField(warehouseReceiveTemplate2, "Send Shipped", AddOnColumnDataType.Codes.Boolean, displaySequence: 0, addOnRule: addOnRuleBoolean);
			Helper.AddCustomField(warehouseReceiveTemplate2, "BOOK ROAD RUNNERS", AddOnColumnDataType.Codes.String, displaySequence: 7);
			Helper.AddCustomField(warehouseReceiveTemplate2, "SUBURB", AddOnColumnDataType.Codes.String, displaySequence: 0);
			Helper.AddCustomField(warehouseReceiveTemplate2, "ADDRESS ISSUE", AddOnColumnDataType.Codes.Datetime, displaySequence: 88, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "OUT OF STOCK (T)", AddOnColumnDataType.Codes.Datetime, displaySequence: 86, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "LARGE VOLUMES", AddOnColumnDataType.Codes.Datetime, displaySequence: 90, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "SLA MISSED", AddOnColumnDataType.Codes.Datetime, displaySequence: 89, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "OUT OF STOCK (F)", AddOnColumnDataType.Codes.Datetime, displaySequence: 87, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "DATA INTEGRITY ISSUE (T)", AddOnColumnDataType.Codes.Datetime, displaySequence: 80, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "DATA INTEGRITY ISSUE (F)", AddOnColumnDataType.Codes.Datetime, displaySequence: 81, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "ON HOLD - CLIENT REQUEST (T)", AddOnColumnDataType.Codes.Datetime, displaySequence: 82, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "ON HOLD - CLIENT REQUEST (F)", AddOnColumnDataType.Codes.Datetime, displaySequence: 83, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "SYSTEM DOWN (F)", AddOnColumnDataType.Codes.Datetime, displaySequence: 85, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "SYSTEM DOWN (T)", AddOnColumnDataType.Codes.Datetime, displaySequence: 84, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "DOCUMENTATION REQUIRED (T)", AddOnColumnDataType.Codes.Datetime, displaySequence: 0, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "DOCUMENTATION REQUIRED (F)", AddOnColumnDataType.Codes.Datetime, displaySequence: 0, addOnRule: addOnRuleDateTime);
			Helper.AddCustomField(warehouseReceiveTemplate2, "SHORTFALL TEST", AddOnColumnDataType.Codes.Datetime, displaySequence: 0, addOnRule: addOnRuleDateTime);

			var warehouseReleaseTemplate8 = Helper.CreateWorkflowTemplate("RELEASE8", WorkflowDescriptors.WhsOrderWorkflowDescriptorCode);
			Helper.CreateWorkflowMilestone(warehouseReleaseTemplate8.WorkflowItems, "M1", 90, AutoEvents.CustomisableEvent53Code, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily);

			var warehouseTransferTemplate = Helper.CreateWorkflowTemplate("TRANSFER1", WorkflowDescriptors.WhsTransferWorkflowDescriptorCode, triggerFallback: FallbackTypeList.Codes.NeverFallback);
			var warehouseTransferTrigger1 = Helper.CreateWorkflowTrigger(warehouseTransferTemplate.WorkflowItems, "T1", 1, AutoEvents.WarehouseJobEnteredCode, EventReferenceConditionList.Codes.ConditionWithMacros, "Source.Warehouse.WarehouseAddress.OA_State==\"NSWa\"");
			var transferSlipDocumentQuery = new ZQuery(StmMenuItemSchema.SU_MenuName, "Transfer Slip");
			transferSlipDocumentQuery.AddToFilter(StmMenuItemSchema.SU_MenuPath, ""); // new, not legacy document
			var transferSlipDocument = Factory.LoadTop1<StmMenuItem>(transferSlipDocumentQuery);
			Helper.CreateWorkflowNotification(warehouseTransferTrigger1, WorkflowTriggerActionTypeConstants.Codes.SendDocument, recepient: MessageRecipientPartyTypeList.Codes.Print, document: transferSlipDocument.PK);
			var warehouseTransferTrigger2 = Helper.CreateWorkflowTrigger(warehouseTransferTemplate.WorkflowItems, "T2", 2, AutoEvents.WarehouseJobEnteredCode, EventReferenceConditionList.Codes.ConditionWithMacros, "Source.Warehouse.WarehouseAddress.OA_State==\"SAa\"");
			Helper.CreateWorkflowNotification(warehouseTransferTrigger2, WorkflowTriggerActionTypeConstants.Codes.SendDocument, recepient: MessageRecipientPartyTypeList.Codes.Print, document: transferSlipDocument.PK);
			var warehouseTransferTrigger3 = Helper.CreateWorkflowTrigger(warehouseTransferTemplate.WorkflowItems, "T3", 3, AutoEvents.WarehouseJobEnteredCode, EventReferenceConditionList.Codes.ConditionWithMacros, "Source.Warehouse.WarehouseAddress.OA_State==\"VICa\"");
			Helper.CreateWorkflowNotification(warehouseTransferTrigger3, WorkflowTriggerActionTypeConstants.Codes.SendDocument, recepient: MessageRecipientPartyTypeList.Codes.Print, document: transferSlipDocument.PK);
			var warehouseTransferTrigger4 = Helper.CreateWorkflowTrigger(warehouseTransferTemplate.WorkflowItems, "T4", 4, AutoEvents.WarehouseJobEnteredCode, EventReferenceConditionList.Codes.ConditionWithMacros, "Source.Warehouse.WarehouseAddress.OA_State==\"WAa\"");
			Helper.CreateWorkflowNotification(warehouseTransferTrigger4, WorkflowTriggerActionTypeConstants.Codes.SendDocument, recepient: MessageRecipientPartyTypeList.Codes.Print, document: transferSlipDocument.PK);
			var warehouseTransferTrigger5 = Helper.CreateWorkflowTrigger(warehouseTransferTemplate.WorkflowItems, "T5", 5, AutoEvents.WarehouseJobEnteredCode, EventReferenceConditionList.Codes.ConditionWithMacros, "Source.Warehouse.WarehouseAddress.OA_State==\"QLDa\"");
			Helper.CreateWorkflowNotification(warehouseTransferTrigger5, WorkflowTriggerActionTypeConstants.Codes.SendDocument, recepient: MessageRecipientPartyTypeList.Codes.Print, document: transferSlipDocument.PK);

			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			for (int i = 0; i < NumberOfOrders; i++)
			{
				Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			data.Org1.OH_Code = "AUSPOSMEL";
			data.Whs1.WarehouseAddress.OA_State = "NSW";
			Factory.Save();

			var pick = Helper.CreatePickNew();
			pick.WP_PickCasesByLabel = true;
			for (int i = 0; i < NumberOfOrders; i++)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O" + i, data.Part1, 5m);
				order.TransportCoDocAddress.OrganisationPK = data.Org1.PK;
				pick.Orders.Add(order);
			}

			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			pick.AllocatePackageLabels();
			Factory.Save();
			var packages = pick.Orders.Cast<WhsOrder>().SelectMany(o => o.PackageJob.Packages).ToArray();
			AssertEquals("Precondition: Number of packages correct", NumberOfOrders, packages.Length);

			var pickByLabelJobs = new List<WhsPickByLabelJob>();
			for (int i = 0; i < 10; i++)
			{
				var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, data.Whs1.PK, "E", data.Whs1.WW_DefaultOutboundDockDoor);
				for (int j = 0; j < NumberOfOrders / 10; j++)
				{
					WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, packages[i * 10 + j].PK);
				}
				pickByLabelJobs.Add(pickByLabelJob);
			}

			foreach (var pickLine in pick.GetAllPickLines())
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			}

			foreach (var pickByLabelJob in pickByLabelJobs)
			{
				WhsPickByLabelHelper.PutawayPickedLabelsAndSplitJobIfNecessary(pickByLabelJob);
				AssertEquals("Precondition", true, !pickByLabelJob.WTK_FinalisedDate.IsEmpty);
			}
			Factory.Save();

			// Asserts
			var otherFactory = new BusinessObjectFactory();
			var pickInOtherFactory = otherFactory.Load<WhsPick>(pick.PK);

			var expectedNoMoreThan_5_DBHitsPerTable = new Dictionary<string, int>
			{
				{ JobDocAddressSchema.Constants.TableName, 9 }, // all batched db hits
				{ ProcessTaskNotificationSchema.Constants.TableName, 69 }, // batched hits
				{ ProcessTasksSchema.Constants.TableName, 10 }, // all of these hits a batched pk loads or queries with order by's so fetch hints don't work.
				{ StmALogSchema.Constants.TableName, 173 }, // all of these hits a batched pk loads or queries with order by's so fetch hints don't work.
				{ StmEventSchema.Constants.TableName, 6 }, // batched hits
				{ WhsDocketSchema.Constants.TableName, 6 }, // batched hits
				{ WhsDocketLineSchema.Constants.TableName, 17 }, // batched hits
				{ WhsPickLineSchema.Constants.TableName, 17 }, // batched hits
				{ GenAddOnColumnSchema.Constants.TableName, 7 }, // batched hits
				{ JobHeaderSchema.Constants.TableName, 6 }, // batched hits
				{ WhsLoadOrderSchema.Constants.TableName, 2 } // batched hits
			};

			using (WorkflowDataRegistry.Instance.EnableTemplateApplicationConcurrencyProtection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TemplateApplicationRaceConditionHandlerOptions.Codes.UserInterfaceAndServiceTasks))
			using (AssertDbHitsWithUsefulQueryInformation(expectedNoMoreThan_5_DBHitsPerTable, otherFactory, true))
			{
				pickInOtherFactory.FinaliseAllOrders();
				pickInOtherFactory.FinalisePick();
				otherFactory.Save();
			}
			AssertEquals("Ensure functionality worked.", true, pickInOtherFactory.IsFinalised);
		}

		#endregion

		#region Labels

		public void TestLabels_AddNew()
		{
			var job = Factory.New<WhsPickByLabelJob>();
			var label = job.Labels.AddNew();
			var labelInFactory = Factory.LoadTop1<WhsPickByLabelLabel>(new ZQuery(WhsPickByLabelLabelSchema.WTL_WTK_PickByLabelJob, job.PK));
			AssertEquals(label.PK, labelInFactory.PK);
		}

		#endregion

		#region TestLabels_Load

		public void TestLabels_Load()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");

			var pickByLabelJob = WhsPickByLabelHelperForTesting.CreateWhsPickByLabelJob(Factory, data.Whs1.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, "Bob");
			var pickByLabelLabel = WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package.PK);
			Factory.Save();

			AssertEquals("Should be able to load related label.", pickByLabelLabel.PK, new BusinessObjectFactory().Load<WhsPickByLabelJob>(pickByLabelJob.PK).Labels.Single().PK);
			pickByLabelLabel.Delete();
			Factory.Save();
			AssertEquals("After deleting label in other factory should not have label anymore.", 0, new BusinessObjectFactory().Load<WhsPickByLabelJob>(pickByLabelJob.PK).Labels.Count);
		}

		#endregion

		#region ClosePickByLabelJob

		[TestDate(2018, 09, 11, 12, 10, 20)]
		public void TestClosePickByLabelJob()
		{
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Core.Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var staff = Helper.CreateGlbStaff("Bob", "Bob");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var picklines = pick.GetAllPickLines();
			AssertEquals("Precondition.", true, picklines.All(pl => pl.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet));

			var package = order.PackageJob.Packages.AddNew(Core.Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, order.Lines[0].PickLines[0]);
			Helper.Factory.Save();

			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "~BP", package.PK);
			var pickByLabelJob = Factory.Load<WhsPickByLabelJob>(new ZQuery()).Single();
			var labels = pickByLabelJob.Labels.Cast<WhsPickByLabelLabel>().ToArray();

			picklines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			WhsPickByLabelHelper.PutawayPickedLabelsAndSplitJobIfNecessary(pickByLabelJob);
			AssertEquals("FinalisedDate should have current date.", new ZDateTimeOffset(2018, 09, 11, 12, 10, 20, TimeSpan.Zero), pickByLabelJob.WTK_FinalisedDate);
		}

		[TestDate(2018, 09, 11, 12, 10, 20)]
		public void TestClosePickByLabelJob_MultiJob()
		{
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Core.Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var staff = Helper.CreateGlbStaff("Bob", "Bob");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var picklines = pick.GetAllPickLines();
			AssertEquals("Precondition.", true, picklines.All(pl => pl.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet));

			var package1 = order.PackageJob.Packages.AddNew(Core.Constants.PkgUnit.Unit, "PACKAGE-1");
			var package2 = order.PackageJob.Packages.AddNew(Core.Constants.PkgUnit.Unit, "PACKAGE-2");
			packingHelper.CreatePackageDivot(package1, order.Lines[0].PickLines[0]);
			packingHelper.CreatePackageDivot(package2, order.Lines[0].PickLines[1]);
			Helper.Factory.Save();

			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "~BP", package1.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "~BP", package2.PK);
			var pickByLabelJob = Factory.Load<WhsPickByLabelJob>(new ZQuery()).Single();
			var labels = pickByLabelJob.Labels.Cast<WhsPickByLabelLabel>().ToArray();

			picklines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			WhsPickByLabelHelper.PutawayPickedLabelsAndSplitJobIfNecessary(pickByLabelJob);
			AssertEquals("FinalisedDate should have current date.", new ZDateTimeOffset(2018, 09, 11, 12, 10, 20, TimeSpan.Zero), pickByLabelJob.WTK_FinalisedDate);
		}

		#endregion

		#region IWhsPickByLabelJob Members

		public void TestIWhsPickByLabelJob_PK()
		{
			var pickByLabelJob = Factory.New<WhsPickByLabelJob>();
			AssertEquals("PK is correct", pickByLabelJob.PK, ((IWhsPickByLabelJob)pickByLabelJob).PK);
		}

		public void TestIWhsPickByLabelJob_WTK_FinalisedDate()
		{
			var dateTimeOffset = new ZDateTimeOffset();
			var pickByLabelJob = Factory.New<WhsPickByLabelJob>();
			pickByLabelJob.WTK_FinalisedDate = dateTimeOffset;
			AssertEquals("Finalised Date is correct", pickByLabelJob.WTK_FinalisedDate, ((IWhsPickByLabelJob)pickByLabelJob).WTK_FinalisedDate);
		}

		public void TestIWhsPickByLabelJob_WTK_GS_NKAssignedTo()
		{
			var pickByLabelJob = Factory.New<WhsPickByLabelJob>();
			pickByLabelJob.WTK_GS_NKAssignedTo = "RFT";
			AssertEquals("AssignedTo is correct", pickByLabelJob.WTK_GS_NKAssignedTo, ((IWhsPickByLabelJob)pickByLabelJob).WTK_GS_NKAssignedTo);
		}

		public void TestIWhsPickByLabelJob_WTK_WL_DockDoor()
		{
			var dockdoorPK = ZGuid.NewZGuid();
			var pickByLabelJob = Factory.New<WhsPickByLabelJob>();
			pickByLabelJob.WTK_WL_DockDoor = dockdoorPK;
			AssertEquals("DockDoor is correct", pickByLabelJob.WTK_WL_DockDoor, ((IWhsPickByLabelJob)pickByLabelJob).WTK_WL_DockDoor);
		}

		public void TestIWhsPickByLabelJob_WTK_WW_Warehouse()
		{
			var warehousePK = ZGuid.NewZGuid();
			var pickByLabelJob = Factory.New<WhsPickByLabelJob>();
			pickByLabelJob.WTK_WW_Warehouse = warehousePK;
			AssertEquals("Warehouse is correct", pickByLabelJob.WTK_WW_Warehouse, ((IWhsPickByLabelJob)pickByLabelJob).WTK_WW_Warehouse);
		}

		#endregion

		#region TestConstructorSetsConcurrencyPolicy

		public void TestConstructorSetsConcurrencyPolicy()
		{
			var pickByLabelJob = Factory.New<WhsPickByLabelJob>();
			AssertEquals(ConcurrencyPolicy.Strict, pickByLabelJob.WTK_WL_PutawayLocationInfo.ConcurrencyPolicy);
		}

		#endregion

		#region Implementations

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetWhsPickByLabelJobWithValidDataForTesting(factory);

		public static BusinessObject GetWhsPickByLabelJobWithValidDataForTesting(BusinessObjectFactory factory)
		{
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			var result = factory.New<WhsPickByLabelJob>();
			result.WTK_GS_NKAssignedTo = "~BP";
			result.WTK_WW_Warehouse = data.Whs1.PK;
			result.WTK_WL_DockDoor = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			return result;
		}

		#endregion
	}
}

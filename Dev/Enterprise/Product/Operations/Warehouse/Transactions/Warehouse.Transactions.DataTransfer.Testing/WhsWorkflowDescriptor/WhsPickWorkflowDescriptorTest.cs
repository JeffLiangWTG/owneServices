using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.Warehouse.Transactions.DataTransfer.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPickWorkflowDescriptor))]
	public class WhsPickWorkflowDescriptorTest : WorkflowDescriptorTestCase<WhsPickWorkflowDescriptor>
	{
		#region TestDocumentBusinessContext

		public void TestDocumentBusinessContext()
		{
			AssertContainsExactElementsInAnyOrder(new[] { BusinessContext.WhsDespatch }, WorkflowDescriptor.DocumentBusinessContext);
		}

		#endregion

		#region TestGetWorkflowTriggerAction

		public void TestGetWorkflowTriggerAction()
		{
			var testPick = Helper.CreatePickNew();
			var task1 = testPick.WorkflowItems.Triggers.AddNew();
			var task2 = testPick.WorkflowItems.Triggers.AddNew();
			var notification1 = task1.ProcessTaskNotifications.AddNew();
			var notification2 = task2.ProcessTaskNotifications.AddNew();
			notification1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AutoFinalisation;
			notification2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.PrintAllPackageLabels;

			var printerPK = ZGuid.NewZGuid();
			notification2.PQ_SQ = printerPK;

			var processor1 = WorkflowDescriptor.GetWorkflowTriggerAction(testPick.WorkflowItems.Cast<ProcessTask>().Single(pt => pt.PK == task1.PK).ProcessTaskNotifications[0], new QueuedLogForTesting(Factory));
			var processor2 = WorkflowDescriptor.GetWorkflowTriggerAction(testPick.WorkflowItems.Cast<ProcessTask>().Single(pt => pt.PK == task2.PK).ProcessTaskNotifications[0], new QueuedLogForTesting(Factory));
			AssertEquals("Should get correct processor.", typeof(WhsOrderAndPickAutoFinalisationProcessor), processor1.GetType());
			AssertEquals("Should get correct processor.", typeof(PrintPackageLabelsProcessor), processor2.GetType());
		}

		public void TestGetWorkflowTriggerAction_PrintAllCarrierLabels()
		{
			var testPick = Helper.CreatePickNew();
			var task = testPick.WorkflowItems.Triggers.AddNew();
			var notification = task.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.PrintAllCarrierLabels;

			var processor = WorkflowDescriptor.GetWorkflowTriggerAction(testPick.WorkflowItems.Cast<ProcessTask>().Single(pt => pt.PK == task.PK).ProcessTaskNotifications[0], new QueuedLogForTesting(Factory));
			AssertEquals("Should get correct processor.", typeof(PrintCarrierLabelsProcessor), processor.GetType());
		}

		public void TestGetWorkflowTriggerAction_PrintPackageLabelsProcessor_EndToEnd()
		{
			var printer = Factory.New<IStmPrintQueue>();
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var helper = new WhsTestHelperFunctions(Factory);

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var packingHelper = new PackingTestHelper(Factory);
			helper.CreateProductUnit(data.Part1, "PLT", 10);
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType = UOMPackTypesList.Codes.Pallet;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();
			AssertEquals("Precondition.", 1, pick.OuterPackages.Count);

			var task1 = pick.WorkflowItems.Triggers.AddNew();
			var notification1 = task1.ProcessTaskNotifications.AddNew();
			notification1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.PrintAllPackageLabels;
			notification1.PQ_SQ = printer.PK;

			Factory.Save();

			var processor = WorkflowDescriptor.GetWorkflowTriggerAction(pick.WorkflowItems.Cast<ProcessTask>().Single().ProcessTaskNotifications[0], new QueuedLogForTesting(Factory));
			processor.Process(new TestNotificationBuffer());

			var printJobs = Factory.Load<StmPrintJob>(new ZQuery()).OrderBy(spj => spj.SP_Sequence).ToArray();
			AssertEquals("Should be 3 documents.", 3, printJobs.Length);
			AssertEquals("Product/Delivery" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJobs[0].SP_DocumentName);
			AssertEquals("End of Pallet" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJobs[1].SP_DocumentName);
			AssertEquals("End of Area" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJobs[2].SP_DocumentName);

			foreach (var printJob in printJobs)
			{
				AssertEquals("JobType should be PRN", "PRN", printJob.SP_JobType);
				AssertEquals("Parent", order.PackageJob.PK, printJob.SP_ParentGuid);
				AssertNotNull("Should have set printer.", printJob.PrintQueue);
				AssertEquals("Should have set printer.", printer.PK, printJob.SP_SQ);
			}
		}

		public void TestGetWorkflowTriggerAction_SetTaskPlanningStatusToReadyForPlanning()
		{
			var pick = Helper.CreatePickNew();
			var task = pick.WorkflowItems.Triggers.AddNew();
			var notification = task.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = ActionTypes.Codes.SetTaskPlanningStatusToReadyForPlanning;

			var processor = WorkflowDescriptor.GetWorkflowTriggerAction(pick.WorkflowItems.Cast<ProcessTask>().Single().ProcessTaskNotifications[0], new QueuedLogForTesting(Factory));
			AssertEquals("Should get correct processor.", typeof(SetTaskPlanningStatusToReadyForPlanningProcessor), processor.GetType());
		}

		#endregion

		#region TestWorkflowTriggerActionTypes

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get
			{
				var result = new List<CodeDescriptionPair>(base.ExpectedAdditionalWorkflowTriggerActionTypes);
				if (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.Value)
				{
					result.Add(new CodeDescriptionPair(ActionTypes.Codes.SetTaskPlanningStatusToReadyForPlanning, ActionTypes.Descriptions.SetTaskPlanningStatusToReadyForPlanning));
				}

				return result.ToArray();
			}
		}

		public void TestWorkflowTriggerActionTypes_TaskManagementEnabled()
		{
			using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestWorkflowTriggerActionTypes();
			}
		}

		public void TestWorkflowTriggerActionTypes_TaskManagementNotEnabled()
		{
			TestWorkflowTriggerActionTypes();
		}

		#endregion

		#region Implementation

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
					MessageRecipientPartyType.Email |
					MessageRecipientPartyType.OrgProxy;
			}
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				Pick
			};
		}

		#region ExpectedWorkflowTriggerFieldColumns

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns
		{
			get { return new[] { WhsPickSchema.WP_IsCartonised }; }
		}

		#endregion

		#region IsPrintingTriggerActionsCore

		protected override string[] IsPrintingTriggerActionsCore
		{
			get { return new[] { WorkflowTriggerActionTypeConstants.Codes.PrintAllPackageLabels, WorkflowTriggerActionTypeConstants.Codes.PrintAllCarrierLabels }; }
		}

		#endregion

		#region IsPrintOnlyTriggerActionsCore

		protected override string[] IsPrintOnlyTriggerActionsCore
		{
			get { return new[] { WorkflowTriggerActionTypeConstants.Codes.PrintAllCarrierLabels }; }
		}

		#endregion

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.WhsPickWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Warehouse Pick", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		#region TestRequiresWarehouse

		protected override bool RequiresWarehouseExpectedResult
		{
			get { return true; }
		}

		#endregion

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		#endregion

		#region Helper

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		protected WhsPick Pick
		{
			get { return pick ?? (pick = Factory.New<WhsPick>()); }
		}

		WhsPick pick;

		#endregion
	}
}

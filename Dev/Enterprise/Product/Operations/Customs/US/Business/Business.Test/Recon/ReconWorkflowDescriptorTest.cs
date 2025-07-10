using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ReconWorkflowDescriptor))]
	sealed class ReconWorkflowDescriptorTest : WorkflowDescriptorTestCase<ReconWorkflowDescriptor>
	{
		public void TestGetWorkflowTriggerAction()
		{
			var parentBO = Factory.New<JobDeclaration>();
			var processTask = parentBO.WorkflowItems.Triggers.AddNew();
			var action = processTask.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage;
			var workFlowDescriptor = new ReconWorkflowDescriptor();
			var result = workFlowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory)
			{ SJ_SE_NKEvent = Events.WorkflowTriggerEventCode });
			AssertEquals(true, result is Customs.Business.BatchProcessor.CustomsStmProcessQueueCreatorProcessor);
		}

		public void TestGetAdditionalWorkflowTriggerActionTypeList()
		{
			var testProviders = GetParentsWithConfiguredOrganisationPartiesForTest();
			Assert("WorkflowDescriptor supports XML message delivering.\r\n"
				+ "GetParentsWithConfiguredOrganisationPartiesForTest() must be implemented to return at least 1 IWorkflowProvider",
				testProviders != null && testProviders.Length > 0);
			var job = testProviders[0];
			var processTask = job.WorkflowItems.Triggers.AddNew();
			processTask.P9_Description = "Start Work";
			processTask.ReferenceCode = "REF";
			var action = processTask.ProcessTaskNotifications.AddNew();
			var task = action.Parent;
			var parent = task.GetJob();
			var actions = WorkflowDescriptor.GetWorkflowTriggerActionTypes();
			AssertEquals(25, actions.Count);
			AssertEquals(true, actions.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage));
		}

		public void TestGetSupportsValidateForCustomsMessagingTriggerActions()
		{
			var testProviders = GetParentsWithConfiguredOrganisationPartiesForTest();
			Assert("WorkflowDescriptor supports XML message delivering.\r\n"
				+ "GetParentsWithConfiguredOrganisationPartiesForTest() must be implemented to return at least 1 IWorkflowProvider",
				testProviders != null && testProviders.Length > 0);
			var job = testProviders[0];
			var processTask = job.WorkflowItems.Triggers.AddNew();
			processTask.P9_Description = "Start Work";
			processTask.ReferenceCode = "REF";
			var action = processTask.ProcessTaskNotifications.AddNew();
			var task = action.Parent;
			var parent = task.GetJob();
			var actions = WorkflowDescriptor.GetSupportsValidateForCustomsMessagingTriggerActions(task, parent);
			AssertEquals(1, actions.Count());
			AssertEquals(WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging, actions.ToArray()[0]);
		}

		public void TestFieldColumnsCount()
		{
			AssertEquals(3, WorkflowDescriptor.GetWorkflowTriggerFieldColumns().Length);
		}

		public new void TestGetFieldColumnDescription()
		{
			AssertEquals("Estimated Recon. Date", WorkflowDescriptor.GetFieldColumnDescription(Factory, USAddInfoSchema.US_EstimatedEntryDate));
			AssertEquals("Issue Code", WorkflowDescriptor.GetFieldColumnDescription(Factory, USAddInfoSchema.US_IssueCode));
			AssertEquals("Surety Code", WorkflowDescriptor.GetFieldColumnDescription(Factory, USAddInfoSchema.US_SuretyCode));
			foreach (var fieldColumn in WorkflowDescriptor.GetWorkflowTriggerFieldColumns())
			{
				var description = WorkflowDescriptor.GetFieldColumnDescription(Factory, fieldColumn);
				AssertEquals("A description for field column '" + fieldColumn.Name + "' must be specified", false, description.Contains("_"));
			}
		}

		public void TestControllerID()
		{
			AssertEquals(ControllerIDs.Customs.US.Recon, WorkflowDescriptor.ControllerID);
		}

		public override void TestDescription()
		{
			AssertEquals("Reconciliation declaration job", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals("REC", WorkflowDescriptor.Code);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestSubTypes()
		{
			AssertEquals("0 sub type", 0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestWorkflowProviderType()
		{
			AssertEquals(typeof(JobDeclaration), WorkflowDescriptor.WorkflowProviderType);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var rec = Factory.New<JobDeclaration>();
			rec.JE_MessageType = JobMessageTypeList.Codes.Recon;
			return new IWorkflowProvider[] { rec };
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email;

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns => new SchemaColumn[] { USAddInfoSchema.US_EstimatedEntryDate, USAddInfoSchema.US_IssueCode, USAddInfoSchema.US_SuretyCode };

		protected override BusinessObject NewBusinessObjectInTable(ITableSchema table)
		{
			var rec = Factory.New<JobDeclaration>();
			rec.JE_MessageType = JobMessageTypeList.Codes.Recon;
			return rec;
		}

		public override string GetPrefixFromColumnName(string fieldColumnName)
		{
			var dec = Factory.New<JobDeclaration>();
			return dec.TablePrefix;
		}

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get
			{
				List<CodeDescriptionPair> list = new List<CodeDescriptionPair>(base.ExpectedAdditionalWorkflowTriggerActionTypes);
				list.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage, WorkflowTriggerActionTypeConstants.Descriptions.SendEntryDeclarationMessage));
				return list.ToArray();
			}
		}
	}
}

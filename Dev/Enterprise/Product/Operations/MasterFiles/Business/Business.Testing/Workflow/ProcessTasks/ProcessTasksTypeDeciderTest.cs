using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class ProcessTasksTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForNew()
		{
			Type result = new ProcessTaskTypeDecider().GetTypeForNew();
			AssertEquals("For new, always return the base type", typeof(ProcessTask), result);
		}

		public void TestGetTypeForBinding()
		{
			Type result = new ProcessTaskTypeDecider().GetTypeForBinding();
			AssertEquals("For binding, always return the base type", typeof(ProcessTask), result);
		}

		public void TestGetTypeForLoad()
		{
			AssertGetTypeForLoad("", typeof(ProcessTask));
			AssertGetTypeForLoad(JobHeaderSchema.Constants.Prefix);
			AssertGetTypeForLoad(ProcessTaskTemplateSchema.Constants.Prefix, typeof(TemplateProcessTask));
			AssertGetTypeForLoad(JobOrderHeaderSchema.Constants.Prefix);
			AssertGetTypeForLoad(JobConsolSchema.Constants.Prefix, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsolProcessTask>());
			AssertGetTypeForLoad(JobShipmentPreplanningSchema.Constants.Prefix);
			AssertGetTypeForLoad(JobDeclarationSchema.Constants.Prefix);
			AssertGetTypeForLoad(OrgOpportunitySchema.Constants.Prefix, typeof(OpportunityProcessTasks));
			AssertGetTypeForLoad(GlbCompanyCampaignSchema.Constants.Prefix);
			AssertGetTypeForLoad(JobContainerSchema.Constants.Prefix);
			AssertGetTypeForLoad(JobCartageSchema.Constants.Prefix);
			AssertGetTypeForLoad(RatingHeaderSchema.Constants.Prefix);
			AssertGetTypeForLoad(CusMAWBSchema.Constants.Prefix);
			AssertGetTypeForLoad(CusUnderbondSchema.Constants.Prefix);
			AssertGetTypeForLoad(CusISFHeaderSchema.Constants.Prefix);
			AssertGetTypeForLoad(JPAFRHeaderSchema.Constants.Prefix);
			AssertGetTypeForLoad(CusSCAOceanBillSchema.Constants.Prefix);

			AssertGetTypeForLoadWhenAttachedToJobHeader(JobConsolSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsolProcessTask>());
			AssertGetTypeForLoadWhenAttachedToJobHeader(JobShipmentSchema.Constants.TableName, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipmentProcessTask>());
		}

		public void TestGetTypeForLoad_WhenNoJobHeader()
		{
			ProcessTask task = Factory.New<ProcessTask>();
			task.P9_ParentTableCode = JobHeaderSchema.Constants.Prefix;
			Factory.Save();

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();
			ProcessTask loadedTask = loadingFactory.Load<ProcessTask>(task.PK);
			AssertEquals("Task loads without an exception", task.PK, loadedTask.PK);
		}

		#region TestGetInstance

		public void TestGetInstance()
		{
			var originalClientHook = ClientHookLoader.Instance.ClientHook;
			Type expectedOriginalTypeDeciderType = typeof(ProcessTaskTypeDecider);
			if (originalClientHook != null && originalClientHook.ClientTypeDeciders.ContainsKey(typeof(ProcessTask)))
			{
				expectedOriginalTypeDeciderType = originalClientHook.ClientTypeDeciders[typeof(ProcessTask)].GetType();
			}

			AssertEquals(expectedOriginalTypeDeciderType, ProcessTaskTypeDecider.GetInstance().GetType());

			using (ClientHookLoader.Instance.OverrideClientHookForTest(new TestClientOverride()))
			{
				AssertEquals(typeof(ClientSpecificProcessTaskTypeDeciderForTest), ProcessTaskTypeDecider.GetInstance().GetType());
			}

			AssertEquals(expectedOriginalTypeDeciderType, ProcessTaskTypeDecider.GetInstance().GetType());
		}

		class TestClientOverride : ClientHook
		{
			public override ITypeDeciderDictionary ClientTypeDeciders
			{
				get
				{
					Dictionary<Type, ITypeDecider> result = new Dictionary<Type, ITypeDecider>();
					result.Add(typeof(ProcessTask), new ClientSpecificProcessTaskTypeDeciderForTest());
					return new TypeDeciderDictionary(result);
				}
			}

			public override Clients Client
			{
				get { return Clients.EDI; }
			}

			public override string ClientDisplayName
			{
				get { return "For Test"; }
			}
		}

		class ClientSpecificProcessTaskTypeDeciderForTest : ProcessTaskTypeDecider
		{
		}

		#endregion

		public void TestGetQueryForLoad()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			ProcessTask dummyTask = dummy.WorkflowItems.Tasks.AddNew();
			ProcessTask dummyTrigger = dummy.WorkflowItems.Triggers.AddNew();
			OrgOpportunity opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			ProcessTask opportunityTask = opportunity.WorkflowItems.Tasks.AddNew();
			ProcessTask opportunityMilestone = opportunity.WorkflowItems.Milestones.AddNew();
			Factory.Save();

			AssertNotNull(DummyWorkflowDescriptor.Instance);
			WorkflowDescriptors descriptors = WorkflowDescriptors.Instance;
			ProcessTaskTypeDecider typeDecider = new ProcessTaskTypeDecider();
			WorkflowDescriptor descriptor;
			descriptors.TryGetValue(OpportunityWorkflowDescriptor.WorkflowTypeCode, out descriptor);
			ProcessTask[] processTasks = Factory.Load<ProcessTask>(typeDecider.GetQueryForLoad(descriptor));
			AssertEquals(2, processTasks.Length);
			AssertCollectionContains(opportunityTask, processTasks);
			AssertCollectionContains(opportunityMilestone, processTasks);

			descriptors.TryGetValue("DUM", out descriptor);
			processTasks = Factory.Load<ProcessTask>(typeDecider.GetQueryForLoad(descriptor));
			AssertEquals(2, processTasks.Length);
			AssertCollectionContains(dummyTask, processTasks);
			AssertCollectionContains(dummyTrigger, processTasks);

			DummyWorkflowDescriptor.Instance.SetAreTasksCompanySpecific(true);
			DummyWithWorkflow anotherDummy = Factory.New<DummyWithWorkflow>();
			ProcessTask anotherDummyTask = anotherDummy.WorkflowItems.Tasks.AddNew();
			ProcessTask anotherDummyMilestone = anotherDummy.WorkflowItems.Milestones.AddNew();
			Factory.Save();
			processTasks = Factory.Load<ProcessTask>(typeDecider.GetQueryForLoad(descriptor));
			AssertEquals(4, processTasks.Length);
			AssertCollectionContains(dummyTask, processTasks);
			AssertCollectionContains(dummyTrigger, processTasks);
			AssertCollectionContains(anotherDummyTask, processTasks);
			AssertCollectionContains(anotherDummyMilestone, processTasks);
		}

		#region Implementation

		void AssertGetTypeForLoad(ZString tablePrefix)
		{
			ProcessTask.P9_ParentTableCode = tablePrefix;
			AssertNotNull("Table Prefix '" + tablePrefix + "' is unknown", new ProcessTaskTypeDecider().GetTypeForLoad(((INeedRow)ProcessTask).Row, Factory));
		}

		void AssertGetTypeForLoad(ZString tablePrefix, Type expectedTaskType)
		{
			ProcessTask.P9_ParentTableCode = tablePrefix;
			Type decidedTaskType = new ProcessTaskTypeDecider().GetTypeForLoad(((INeedRow)ProcessTask).Row, Factory);
			AssertEquals("Template Task", expectedTaskType, decidedTaskType);
		}

		void AssertGetTypeForLoadWhenAttachedToJobHeader(ZString tableName, Type expectedTaskType)
		{
			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(tableName);
			ProcessTask.P9_ParentTableCode = JobHeaderSchema.Constants.Prefix;
			ProcessTask.P9_ParentID = job.PK;

			Type decidedTaskType = new ProcessTaskTypeDecider().GetTypeForLoad(((INeedRow)ProcessTask).Row, Factory);
			AssertEquals("Template Task", expectedTaskType, decidedTaskType);
		}

		ProcessTask ProcessTask
		{
			get
			{
				if (processTask == null)
				{
					processTask = Factory.New<ProcessTask>();
				}
				return processTask;
			}
		}
		ProcessTask processTask;

		#endregion

	}
}

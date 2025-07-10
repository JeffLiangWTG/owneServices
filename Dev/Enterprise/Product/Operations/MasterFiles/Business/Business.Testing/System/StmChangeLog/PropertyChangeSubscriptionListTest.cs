using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class PropertyChangeSubscriptionListTest : TestCaseWithFactory
	{
		public void TestPropertyNamesToLogAlways()
		{
			ICollection<string> propertyNamesToLogAlways = Business.PropertyChangeSubscriptionList.PropertyNamesToLogAlways;
			AssertEquals(true, propertyNamesToLogAlways.Contains(JobVoyOriginSchema.Constants.JA_E_DEP));
			AssertEquals(true, propertyNamesToLogAlways.Contains(JobVoyOriginSchema.Constants.JA_A_DEP));
			AssertEquals(true, propertyNamesToLogAlways.Contains(JobVoyOriginSchema.Constants.JA_RL_NKPortOfLoading));
			AssertEquals(true, propertyNamesToLogAlways.Contains(JobVoyDestinationSchema.Constants.JB_E_ARV));
			AssertEquals(true, propertyNamesToLogAlways.Contains(JobVoyDestinationSchema.Constants.JB_A_ARV));
			AssertEquals(true, propertyNamesToLogAlways.Contains(JobVoyDestinationSchema.Constants.JB_RL_NKPortOfDischarge));
			AssertEquals(true, propertyNamesToLogAlways.Contains(JobVoyageSchema.Constants.JV_RV_NKVessel));
			AssertEquals(true, propertyNamesToLogAlways.Contains(JobVoyageSchema.Constants.JV_VoyageFlight));
		}

		public void TestWorkflowTriggerPropertyNamesThatMayBeLogged()
		{
			StringBuilder missingTriggerFields = new StringBuilder();
			foreach (string propertyName in ExpectedWorkflowTriggerPropertyNamesThatMayBeLogged)
			{
				if (!PropertyChangeSubscriptionListForTest.AllPropertyNamesThatMayBeLogged.Contains(propertyName))
				{
					missingTriggerFields.AppendLine(propertyName);
				}
			}
			if (missingTriggerFields.Length > 0)
			{
				Fail("The following trigger fields must be registered in WorkflowTriggerPropertyNamesThatMayBeLogged:\r\n" + missingTriggerFields);
			}
			Assert(true);
		}

		public void TestWorkflowTriggerPropertyNamesThatMayBeLogged_DoesntIncludeUnnecessaryTriggerFields()
		{
			WorkflowDescriptors.OverrideProductivityWiseInclusionConsideration_ForTest.Value = true; // So that all workflow descriptors with field change triggers are included.

			var unnecessaryTriggerFields = new StringBuilder();

			foreach (string propertyName in PropertyChangeSubscriptionListForTest.WorkflowTriggerPropertyNamesThatMayBeLogged)
			{
				if (!ExpectedWorkflowTriggerPropertyNamesThatMayBeLogged.Contains(propertyName))
				{
					unnecessaryTriggerFields.AppendLine(propertyName);
				}
			}
			if (unnecessaryTriggerFields.Length > 0)
			{
				Fail(
					"The following trigger fields are registered in WorkflowTriggerPropertyNamesThatMayBeLogged unnecessarily.\r\n" +
					"Consider removing them from here, or add them to a WorkflowDescriptor's WorkflowTriggerFieldColumns property.\r\n" +
					unnecessaryTriggerFields);
			}
			Assert(true);
		}

		public void TestShouldLogChanges_ForWorkflowTriggers()
		{
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_VarCharMax);

			AssertEquals("If there's no workflow triggers don't log field changes", false, PropertyChangeSubscriptionList.ShouldLogChanges(Dummy.Z0_VarCharMaxInfo));
			ProcessTask trigger = Dummy.RelatedWorkflowProvider.RelatedWorkflowProvider.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerFieldName = DummyBizoSchema.Z0_VarCharMax.Name;
			AssertEquals("If there's a workflow trigger, log field changes", true, PropertyChangeSubscriptionList.ShouldLogChanges(Dummy.Z0_VarCharMaxInfo));
		}

		public void TestShouldLogChanges_ForWorkflowTriggers_WhenSaved()
		{
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_VarCharMax);
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_Description);

			AssertEquals("If there's no workflow triggers don't log field changes", false, PropertyChangeSubscriptionList.ShouldLogChanges(Dummy.Z0_VarCharMaxInfo));
			ProcessTask trigger = Dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerFieldName = DummyBizoSchema.Z0_VarCharMax.Name;
			ProcessTask trigger2 = Dummy.RelatedWorkflowProvider.WorkflowItems.Triggers.AddNew();
			trigger2.TriggerConditions.TriggerFieldName = DummyBizoSchema.Z0_Description.Name;

			Factory.Save();
			AssertEquals("If there's a workflow trigger, log field changes", true, PropertyChangeSubscriptionList.ShouldLogChanges(Dummy.Z0_VarCharMaxInfo));
			AssertEquals("If there's a related workflow trigger, log field changes", true, PropertyChangeSubscriptionList.ShouldLogChanges(Dummy.Z0_DescriptionInfo));
		}

		public void TestShouldLogChanges_ForMilestoneTriggers()
		{
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_VarCharMax);

			AssertEquals("If there's no milestone triggers don't log field changes", false, PropertyChangeSubscriptionList.ShouldLogChanges(Dummy.Z0_VarCharMaxInfo));
			ProcessTask milestone = Dummy.RelatedWorkflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerFieldName = DummyBizoSchema.Z0_VarCharMax.Name;
			AssertEquals("If there's a milestone trigger, log field changes", true, PropertyChangeSubscriptionList.ShouldLogChanges(Dummy.Z0_VarCharMaxInfo));
		}

		public void TestGetRelatedTriggersStackOverflowException()
		{
			var dummy = Factory.New<DummyWithParentWorkflow>();
			var workflowProviders = new IWorkflowProvider[1000];
			for (int i = 0; i < 1000; i++)
			{
				workflowProviders[i] = Factory.New<DummyWithWorkflow>();
				var workflowItem = workflowProviders[i].WorkflowItems.AddNew();
				workflowItem.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
				workflowItem.TriggerConditions.TriggerFieldName = "ZZ_TriggerMe";
			}
			dummy.ParentWorkflowProviders = workflowProviders;
			var result = PropertyChangeSubscriptionList.GetRelatedTriggers(dummy);
			AssertEquals(1000, result.Length);
		}

		#region Implementation

		IList<string> ExpectedWorkflowTriggerPropertyNamesThatMayBeLogged
		{
			get
			{
				if (expectedWorkflowTriggerPropertyNamesThatMayBeLogged == null)
				{
					List<string> list = new List<string>();
					foreach (WorkflowDescriptor workflowProvider in WorkflowDescriptors.Instance.Values)
					{
						foreach (SchemaColumn triggerField in workflowProvider.GetWorkflowTriggerFieldColumns())
						{
							if (!list.Contains(triggerField.Name))
							{
								list.Add(triggerField.Name);
							}
						}
					}
					expectedWorkflowTriggerPropertyNamesThatMayBeLogged = list.ToArray();
				}
				return expectedWorkflowTriggerPropertyNamesThatMayBeLogged;
			}
		}
		string[] expectedWorkflowTriggerPropertyNamesThatMayBeLogged;

		PropertyChangeSubscriptionListForTest PropertyChangeSubscriptionList
		{
			get { return PropertyChangeSubscriptionListForTest.GetInstance(Factory); }
		}

		DummyWithWorkflow Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyWithWorkflow>()); }
		}
		DummyWithWorkflow dummy;

		sealed class DummyWithParentWorkflow : DummyWithChangeLogging, IWorkflowProvider, IWorkflowTriggerFieldChangeSource
		{
			public DummyWithParentWorkflow(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			#region IWorkflowProvider

			ZString IWorkflowProviderCore.WorkflowType
			{
				get { return "DUM"; }
			}

			IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

			public IProcessHeaderCollection Workflows
			{
				get
				{
					if (workflows == null)
					{
						workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					}

					return workflows;
				}
			}
			IProcessHeaderCollection workflows;

			ProcessTaskCollection IWorkflowProvider.WorkflowItems
			{
				get { return WorkflowItems; }
			}

			public DummyProcessTaskCollection WorkflowItems
			{
				get
				{
					if (fWorkflowItems == null)
					{
						fWorkflowItems = this.GetOrCreateProcessTaskCollection(() => new DummyProcessTaskCollection(this));
						RegisterEditableChildObject(fWorkflowItems);
					}
					return fWorkflowItems;
				}
			}
			DummyProcessTaskCollection fWorkflowItems;

			IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
			{
				return null;
			}

			IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
			{
				return new ColumnValueRanker();
			}

			#endregion

			public IReadOnlyList<IWorkflowProvider> ParentWorkflowProviders
			{
				get;
				set;
			}

			public Logs Logs => null;
			public BusinessObjectFactory LogsFactory => null;
		}

		#endregion
	}
}

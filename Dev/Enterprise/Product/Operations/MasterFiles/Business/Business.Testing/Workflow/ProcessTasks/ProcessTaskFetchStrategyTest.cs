using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ProcessTaskFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForView_P9_ParentTemplateID()
		{
			var tasks = new List<ZGuid>();
			for (int i = 0; i < 5; i++)
			{
				var task = Factory.New<ProcessTask>();
				task.FillWithValidTestData();
				task.P9_ParentTemplateID = ZGuid.NewZGuid();
				tasks.Add(task.PK);
			}
			Factory.Save();

			var viewFactory = new BusinessObjectFactory();
			var testProcessTaskCollection = viewFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.PK, tasks));

			foreach (var taks in testProcessTaskCollection)
			{
				taks.FetchStrategy.FetchForView(new[] { new TableColumn("", ProcessTasks.Schema.P9_ParentTemplateID) });
			}

			foreach (var task in testProcessTaskCollection)
			{
				object hitProperty = task.TemplateVersion;
			}

			var hitCount = Factory.GetTableHitCount(ProcessTasksSchema.Constants.TableName);

			var expectedDbHits = new Dictionary<string, int>
			{
				{ ProcessTasksSchema.Constants.TableName, 2 }
			};
			AssertDbHits(expectedDbHits, viewFactory);
		}

		public void TestFetchHintsForView_ProcessTaskNotification()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			AssertFetchHints_ProcessTaskNotificationHints(dummy, shouldUseHints: true, fetchForViewOnly: true);

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			AssertFetchHints_ProcessTaskNotificationHints(template, shouldUseHints: true, fetchForViewOnly: true);
		}

		public void TestFetchHintsForLoad_ProcessTaskNotification_OnlyForTemplates()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			AssertFetchHints_ProcessTaskNotificationHints(dummy, shouldUseHints: false);

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			AssertFetchHints_ProcessTaskNotificationHints(template, shouldUseHints: true);
		}

		void AssertFetchHints_ProcessTaskNotificationHints<T>(T provider, bool shouldUseHints, bool fetchForViewOnly = false) where T : BusinessObject, IWorkflowProvider
		{
			for (int i = 0; i < 5; i++)
			{
				var trigger = provider.WorkflowItems.Triggers.AddNew();
				trigger.ProcessTaskNotifications.AddNew();

				var milestone = provider.WorkflowItems.Milestones.AddNew();
				milestone.ProcessTaskNotifications.AddNew();
			}

			provider.Factory.Save();

			var loadFactory = new BusinessObjectFactory();
			var tasks = loadFactory.Load<T>(provider.PK).WorkflowItems;

			foreach (var task in tasks)
			{
				if (fetchForViewOnly)
				{
					task.FetchStrategy.FetchForView(new[] { new TableColumn("", ProcessTasks.Schema.P9_Description) });
				}
				else
				{
					task.FetchStrategy.FetchForLoad();
				}
			}

			foreach (var task in tasks)
			{
				var hitProperty = (task as ProcessTask).ProcessTaskNotifications.First();
			}

			var expectedDbHits = new Dictionary<string, int>
			{
				{ provider.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessTaskNotificationSchema.Constants.TableName, shouldUseHints ? 1 : 10 }
			};
			AssertDbHits(expectedDbHits, loadFactory);
		}

		public void TestFetchHints_ProcessTaskNotification_TemplateApplication()
		{
			var dummy = new BusinessObjectFactory().New<DummyWithWorkflow>();
			var template = new BusinessObjectFactory().NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

			for (int i = 0; i < 5; i++)
			{
				var trigger = dummy.WorkflowItems.Triggers.AddNew();
				trigger.ProcessTaskNotifications.AddNew();

				var templateTrigger = template.WorkflowItems.Triggers.AddNew();
				templateTrigger.P9_Description = "Trigger" + i;
				templateTrigger.TemplateConditions.TemplateCondition2 = "UDF";
				templateTrigger.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";
				templateTrigger.ProcessTaskNotifications.AddNew();
			}
			dummy.Factory.Save();
			template.Factory.Save();
			AssertEquals("Template should not be applied yet", 5, dummy.WorkflowItems.Triggers.Count);

			var loadFactory = new BusinessObjectFactory();
			dummy = loadFactory.Load<DummyWithWorkflow>(dummy.PK);
			dummy.ApplyWorkflowTemplates();
			AssertEquals("Template should be applied", 10, dummy.WorkflowItems.Triggers.Count);

			var hitCount = loadFactory.GetTableHitCount(ProcessTaskNotificationSchema.Constants.TableName);
			AssertEquals("All required process task notifications should be loaded with once db 1", 1, hitCount);
		}
	}
}

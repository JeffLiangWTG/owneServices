using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	abstract class MilestoneOrTriggerCollectionViewTest<T> : ProcessTaskBaseCollectionViewTest<T> where T : MilestoneOrTriggerCollectionView
	{
		#region CreateItemsFromTemplate

		public void TestCreateItemsFromTemplate_P9_GC_IsSet()
		{
			ProcessTaskTemplate template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "SHP";
			template1.P0_LoadPortCountry = "AUBNE";
			template1.P0_DischargePortCountry = "USLAX";

			ProcessTask task1 = template1.WorkflowItems.AddNew();
			task1.P9_Type = Core.Constants.Workflow.MilestoneType;
			task1.TriggerConditions.TriggerEventCode = "t1";

			ProcessTask task2 = template1.WorkflowItems.AddNew();
			task2.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			task2.TriggerConditions.TriggerEventCode = "t2";

			ProcessTask task3 = template1.WorkflowItems.AddNew();
			task3.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			task3.TriggerConditions.TriggerEventCode = "t3";

			Factory.Save();

			BusinessObject shipment = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>());
			ZPropertyInfo<ZString> shipmentLoadPort = (ZPropertyInfo<ZString>)shipment.FindPropertyInfo("JS_RL_NKOrigin");
			shipmentLoadPort.Value = "AUBNE";
			ZPropertyInfo<ZString> shipmentDischargePort = (ZPropertyInfo<ZString>)shipment.FindPropertyInfo("JS_RL_NKDestination");
			shipmentDischargePort.Value = "USLAX";
			IWorkflowProvider shipmentWorkflow = (IWorkflowProvider)shipment;
			shipmentWorkflow.WorkflowItems.RemoveAndDeleteAll();
			shipmentWorkflow.WorkflowItems.Milestones.CreateItemsFromTemplate();
			shipmentWorkflow.WorkflowItems.Triggers.CreateItemsFromTemplate();
			shipmentWorkflow.WorkflowItems.Tasks.CreateItemsFromTemplate();

			AssertEquals("Should create 3 tasks", 3, shipmentWorkflow.WorkflowItems.Count);
			for (int i = 0; i < shipmentWorkflow.WorkflowItems.Count; i++)
			{
				AssertEquals(Environment.Env.CurrentCompany.PK, shipmentWorkflow.WorkflowItems[i].P9_GC);
			}
		}

		public void TestCreateItemsFromTemplate_ProcessTaskNotification()
		{
			ProcessTaskTemplate template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "SHP";
			template1.P0_LoadPortCountry = "AUBNE";
			template1.P0_DischargePortCountry = "USLAX";

			ProcessTask task1 = template1.WorkflowItems.AddNew();
			task1.P9_Type = Core.Constants.Workflow.MilestoneType;
			task1.TriggerConditions.TriggerEventCode = "t1";

			ProcessTask task2 = template1.WorkflowItems.AddNew();
			task2.P9_Type = Core.Constants.Workflow.MilestoneType;
			task2.TriggerConditions.TriggerEventCode = "t2";

			ProcessTaskNotification notification1 = Factory.NewWithValidTestData<ProcessTaskNotification>();
			notification1.PQ_P9 = task1.PK;
			notification1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;

			ProcessTaskNotification notification2 = Factory.NewWithValidTestData<ProcessTaskNotification>();
			notification2.PQ_P9 = task2.PK;
			notification2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;

			ProcessTaskNotification notification3sameAs1 = Factory.NewWithValidTestData<ProcessTaskNotification>();
			notification3sameAs1.PQ_P9 = task2.PK;
			notification3sameAs1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;

			ProcessTaskNotification notification4 = Factory.NewWithValidTestData<ProcessTaskNotification>();
			notification4.PQ_P9 = task2.PK;
			notification4.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			notification4.PQ_FieldName = "Field 1";
			notification4.PQ_FieldValue = "Value 1";

			ProcessTaskNotification notification5 = Factory.NewWithValidTestData<ProcessTaskNotification>();
			notification5.PQ_P9 = task2.PK;
			notification5.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			notification5.PQ_FieldName = "Field 2";
			notification5.PQ_FieldValue = "Value 2";

			task1.ProcessTaskNotifications.Add(notification1);
			task2.ProcessTaskNotifications.Add(notification2);
			task2.ProcessTaskNotifications.Add(notification3sameAs1);

			Factory.Save();

			BusinessObject shipment = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>());
			ZPropertyInfo<ZString> shipmentLoadPort = (ZPropertyInfo<ZString>)shipment.FindPropertyInfo("JS_RL_NKOrigin");
			shipmentLoadPort.Value = "AUBNE";
			ZPropertyInfo<ZString> shipmentDischargePort = (ZPropertyInfo<ZString>)shipment.FindPropertyInfo("JS_RL_NKDestination");
			shipmentDischargePort.Value = "USLAX";
			IWorkflowProvider shipmentWorkflow = (IWorkflowProvider)shipment;
			shipmentWorkflow.WorkflowItems.RemoveAndDeleteAll();
			shipmentWorkflow.WorkflowItems.Milestones.CreateItemsFromTemplate();

			AssertEquals("Should create 2 tasks", 2, shipmentWorkflow.WorkflowItems.Count);
			for (int i = 0; i < shipmentWorkflow.WorkflowItems.Count; i++)
			{
				if (shipmentWorkflow.WorkflowItems[i].P9_SE_NKMilestoneEvent == "t1")
				{
					AssertEquals("task with P9_SE_NKMilestoneEvent=='t1' should have 1 notification", 1, shipmentWorkflow.WorkflowItems[i].ProcessTaskNotifications.Count);
					AssertReadOnlyForProcessTaskNotificationFields(shipmentWorkflow.WorkflowItems[i].ProcessTaskNotifications[0]);
				}
				else if (shipmentWorkflow.WorkflowItems[i].P9_SE_NKMilestoneEvent == "t2")
				{
					AssertEquals("task with P9_SE_NKMilestoneEvent=='t2' should have 4 notifications", 4, shipmentWorkflow.WorkflowItems[i].ProcessTaskNotifications.Count);
					AssertReadOnlyForProcessTaskNotificationFields(shipmentWorkflow.WorkflowItems[i].ProcessTaskNotifications[0]);
					AssertReadOnlyForProcessTaskNotificationFields(shipmentWorkflow.WorkflowItems[i].ProcessTaskNotifications[1]);
					AssertReadOnlyForProcessTaskNotificationFields(shipmentWorkflow.WorkflowItems[i].ProcessTaskNotifications[2]);
					AssertReadOnlyForProcessTaskNotificationFields(shipmentWorkflow.WorkflowItems[i].ProcessTaskNotifications[3]);

					bool firstSetFieldTriggerFound = false;
					bool secondSetFieldTriggerFound = false;

					foreach (ProcessTaskNotification notification in shipmentWorkflow.WorkflowItems[i].ProcessTaskNotifications)
					{
						if (notification.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SetField &&
							notification.PQ_FieldName == "Field 1")
						{
							firstSetFieldTriggerFound = true;
							AssertEquals("Value 1", notification.PQ_FieldValue);
						}
						else if (notification.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SetField &&
							notification.PQ_FieldName == "Field 2")
						{
							secondSetFieldTriggerFound = true;
							AssertEquals("Value 2", notification.PQ_FieldValue);
						}
					}

					Assert(firstSetFieldTriggerFound);
					Assert(secondSetFieldTriggerFound);
				}
				else
				{
					Fail("only 2 tasks with P9_SE_NKMilestoneEvent=='t1' and P9_SE_NKMilestoneEvent=='t2' respectively should be created");
				}
			}
		}

		#endregion

		public void TestIndexer()
		{
			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.ArrivalCode;

			ProcessTask decoy = Dummy.WorkflowItems.Milestones.AddNew();
			decoy.TriggerConditions.TriggerEventCode = Events.DepartureCode;

			AssertEquals("Should return the correct milestone", milestone, Dummy.WorkflowItems.Milestones[Events.Arrival]);
			AssertEquals("Should return the correct milestone", milestone, Dummy.WorkflowItems.Milestones["ARV"]);
		}

		#region TestMergedActionsFromDifferentTemplatesWithSameEmailText

		public void TestMergedActionsFromDifferentTemplatesWithSameEmailText()
		{
			var milestoneTemplate1 = CreateTemplateAndMilestone("FOO", "xx@yy.zz", "aa@aa.aa");
			var milestoneTemplate2 = CreateTemplateAndMilestone("", "xx@yy.zz", "bb@bb.bb");
			Factory.Save();

			AssertEquals("Precondition - no milestones yet", 0, Dummy.WorkflowItems.Milestones.Count);

			Dummy.SubType1 = "FOO";
			Dummy.ApplyWorkflowTemplates();
			var milestone1 = (ProcessTask)Dummy.WorkflowItems.Milestones.Single();

			AssertNotNull(milestone1);
			AssertEquals(milestoneTemplate1.Item2.PK, milestone1.P9_ParentTemplateID);
			AssertEquals(3, milestone1.ProcessTaskNotifications.Count);
			milestone1.ProcessTaskNotifications.ApplySort(ProcessTaskNotificationSchema.Constants.PQ_EmailAddr, ListSortDirection.Ascending);
			AssertEquals("aa@aa.aa", milestone1.ProcessTaskNotifications[0].PQ_EmailAddr);
			AssertEquals("bb@bb.bb", milestone1.ProcessTaskNotifications[1].PQ_EmailAddr);
			AssertEquals("xx@yy.zz", milestone1.ProcessTaskNotifications[2].PQ_EmailAddr);
		}

		Tuple<ProcessTaskTemplate, ProcessTask> CreateTemplateAndMilestone(string subType, params string[] actionTexts)
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			template.P0_MilestoneFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			template.P0_SubType1 = subType;

			var milestoneTemplate = template.WorkflowItems.Milestones.AddNew();
			milestoneTemplate.TriggerConditions.TriggerEventCode = Events.IncidentClosedCode;

			bool first = true;

			foreach (var text in actionTexts)
			{
				var notificationTemplate = milestoneTemplate.ProcessTaskNotifications.AddNew();
				notificationTemplate.PQ_TriggerType = "NTF";
				notificationTemplate.PQ_TriggerParty = "EML";
				notificationTemplate.PQ_EmailAddr = text;
				notificationTemplate.PQ_EmailText = first ? new ZString(text) : ZString.Empty;
				first = false;
			}

			milestoneTemplate.ProcessTaskNotifications.ApplySort(ProcessTaskNotificationSchema.Constants.PQ_EmailAddr, ListSortDirection.Ascending);

			return Tuple.Create(template, milestoneTemplate);
		}

		#endregion

		#region TestJobWithMultipleTriggersMatchedTemplateTrigger_ActionShouldNotReAdd

		public void TestJobWithMultipleTriggersMatchedTemplateTrigger_ActionShouldNotReAdd()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			DummyWorkflowDescriptor.Instance.SupportedTriggerLineTypes = new[] { TriggerLineTypes.Codes.ForwardingShipment };

			var templateTrigger = template.WorkflowItems.Triggers.AddNew();
			templateTrigger.TriggerConditions.TriggerEventCode = Events.IncidentClosedCode;

			var templateTriggerAction = templateTrigger.ProcessTaskNotifications.AddNew();
			templateTriggerAction.PQ_TriggerType = "XUD";

			var jobTrigger1 = Dummy.WorkflowItems.Triggers.AddNew();
			jobTrigger1.TriggerConditions.TriggerEventCode = Events.IncidentClosedCode;
			jobTrigger1.P9_ParentTemplateID = templateTrigger.PK;

			var jobTrigger2 = Dummy.WorkflowItems.Triggers.AddNew();
			jobTrigger2.TriggerConditions.TriggerEventCode = Events.IncidentClosedCode;
			jobTrigger2.P9_ParentTemplateID = templateTrigger.PK;

			var jobTrigger2Action = jobTrigger2.ProcessTaskNotifications.AddNew();
			jobTrigger2Action.PQ_TriggerType = "XUD";

			Factory.Save();

			var message = @"GIVEN job with multiple triggers that match template-trigger
WHEN calling FindOrCreateItemFromTemplate > 1x
SHOULD not adding same action";

			CombineAssertions(message, () =>
			{
				var ignoredFieldsForMatching = new[] { "TemplateConditions.TemplateCondition1", "TemplateConditions.TemplateCondition2", "TemplateConditions.TemplateCondition2Value", ProcessTasksSchema.Constants.P9_Description, ProcessTasksSchema.Constants.P9_LineTriggerType, ProcessTasksSchema.Constants.P9_EstimatedDefaultedFrom };
				var assertMessage = @"GIVEN templateTrigger.{0} != jobTrigger1.{0} && templateTrigger.{0} == jobTrigger2.{0} 
WHEN caling FindOrCreateItemFromTemplate > 1x 
SHOULD find trigger1 and only add action if not exist";
				AssertSameTemplateAction_ShouldOnlyApplyOnce(assertMessage, ignoredFieldsForMatching, template, templateTrigger, jobTrigger1, jobTrigger2);

				var fieldsForMatching = new[] { "TriggerConditions.TriggerCondition", ProcessTasksSchema.Constants.P9_RespondToCascadedEvents, ProcessTasksSchema.Constants.P9_CascadedEventsContext, "TriggerConditions.TriggerFieldName", "TriggerConditions.TriggerConditionValue" };
				assertMessage = @"GIVEN templateTrigger.{0} != jobTrigger1.{0} && templateTrigger.{0} == jobTrigger2.{0}
WHEN calling FindOrCreateItemFromTemplate > 1x
SHOULD find trigger2 and not re-add action because it has pre-existed action";
				AssertSameTemplateAction_ShouldOnlyApplyOnce(assertMessage, fieldsForMatching, template, templateTrigger, jobTrigger1, jobTrigger2);
			});
		}

		void AssertSameTemplateAction_ShouldOnlyApplyOnce(string message, string[] fieldsToTest, ProcessTaskTemplate template, ProcessTask templateTrigger, ProcessTask jobTrigger1, ProcessTask jobTrigger2)
		{
			foreach (var fieldsTestCombination in GenericTestHelper.GenerateBinaryCombination(fieldsToTest.Length).Where(c => c.Contains(true)))
			{
				templateTrigger.Reload();
				jobTrigger1.Reload();
				jobTrigger2.Reload();

				var fieldsWithDifferentValue = new ZStringBuilder();

				for (var i = 0; i < fieldsTestCombination.Count; i++)
				{
					if (fieldsTestCombination[i])
					{
						fieldsWithDifferentValue.Append(fieldsToTest[i]);

						if (ProcessTasksSchema.Constants.P9_LineTriggerType == fieldsToTest[i])
						{
							// Using a valid Line Trigger Type is important.
							templateTrigger.P9_LineTriggerType = TriggerLineTypes.Codes.ForwardingShipment;
							jobTrigger1.P9_LineTriggerType = TriggerLineTypes.Codes.ForwardingShipment;
							jobTrigger2.P9_LineTriggerType = TriggerLineTypes.Codes.ForwardingShipment;
						}
						else if (ReflectionExtensions.IsString(templateTrigger, fieldsToTest[i]))
						{
							ReflectionExtensions.SetPropertyValue(templateTrigger, fieldsToTest[i], new ZString("ABC"));
							ReflectionExtensions.SetPropertyValue(jobTrigger1, fieldsToTest[i], new ZString("DEF"));
							ReflectionExtensions.SetPropertyValue(jobTrigger2, fieldsToTest[i], new ZString("ABC"));
						}
						else if (ReflectionExtensions.IsBool(templateTrigger, fieldsToTest[i]))
						{
							ReflectionExtensions.SetPropertyValue(templateTrigger, fieldsToTest[i], new ZBool(true));
							ReflectionExtensions.SetPropertyValue(jobTrigger1, fieldsToTest[i], new ZBool(false));
							ReflectionExtensions.SetPropertyValue(jobTrigger2, fieldsToTest[i], new ZBool(true));
						}
						else
						{
							throw new NotImplementedException(string.Format("type is not implemented"));
						}
					}
				}

				var assertMessage = string.Format(message, fieldsWithDifferentValue.ToStringWithDelimiterBetweenAppends(","));
				AssertSameTemplateAction_ShouldOnlyApplyOnce(assertMessage, template, templateTrigger, jobTrigger1, jobTrigger2);
			}
		}

		void AssertSameTemplateAction_ShouldOnlyApplyOnce(string message, ProcessTaskTemplate template, ProcessTask templateTrigger, ProcessTask jobTrigger1, ProcessTask jobTrigger2)
		{
			for (var i = 1; i <= 3; i++)
			{
				Dummy.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificTemplates(new[] { template }));

				var foundTrigger = Dummy.WorkflowItems.Triggers.Cast<ProcessTask>().FirstOrDefault();
				AssertCollectionContains(string.Format("{0}: {1}", message, "No new trigger should be created"), foundTrigger.PK, new[] { jobTrigger1.PK, jobTrigger2.PK });
				AssertEquals(string.Format("{0}: {1}", message, "Found trigger should be created from template"), templateTrigger.PK, foundTrigger.P9_ParentTemplateID);
				AssertEquals(string.Format("{0}: {1}", message, "New action should not keep adding on jobTrigger1 if it is already existed"), 1, jobTrigger1.ProcessTaskNotifications.Count);
				AssertEquals(string.Format("{0}: {1}", message, "No new action should not be added on jobTrigger2 because it has pre-existed action"), 1, jobTrigger2.ProcessTaskNotifications.Count);
			}
		}

		#endregion

		#region Implementation

		void AssertReadOnlyForProcessTaskNotificationFields(ProcessTaskNotification notification)
		{
			AssertEquals("FindOrCreateItemFromTemplate() method should call ProcessTaskNotification.RefreshReadOnlyForAllProperties() for each notification created from template to refresh notification fields' ReadOnly", notification.PQ_TriggerType != WorkflowTriggerActionTypeConstants.Codes.SendDocument, notification.PQ_SU_DocumentInfo.ReadOnly);
			AssertEquals("FindOrCreateItemFromTemplate() method should call ProcessTaskNotification.RefreshReadOnlyForAllProperties() for each notification created from template to refresh notification fields' ReadOnly", notification.PQ_TriggerType != WorkflowTriggerActionTypeConstants.Codes.NotificationEmail && notification.PQ_TriggerType != WorkflowTriggerActionTypeConstants.Codes.SendDocument, notification.PQ_Calc_TriggerPartyInfo.ReadOnly);
			AssertEquals("FindOrCreateItemFromTemplate() method should call ProcessTaskNotification.RefreshReadOnlyForAllProperties() for each notification created from template to refresh notification fields' ReadOnly", notification.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendDocument || notification.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SetField, notification.PQ_MessagePurposeInfo.ReadOnly);
			AssertEquals("FindOrCreateItemFromTemplate() method should call ProcessTaskNotification.RefreshReadOnlyForAllProperties() for each notification created from template to refresh notification fields' ReadOnly", notification.PQ_TriggerType != WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, notification.OverrideEmailInfo.ReadOnly);
		}

		protected override void SetUp()
		{
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			base.SetUp();
		}

		#endregion
	}
}

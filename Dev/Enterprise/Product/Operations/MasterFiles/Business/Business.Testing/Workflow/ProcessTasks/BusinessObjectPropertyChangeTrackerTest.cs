using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	class BusinessObjectPropertyChangeTrackerTest : TestCaseWithFactory
	{
		public void TestOnPropertyChanged()
		{
			var bizo = Factory.New<DumpWorkflowDisableWorkflowSettingPropertiesAfterOnSaving>();
			bizo.Z0_IsSystem = false;
			bizo.Z0_Description = "Not update";

			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;
			dummy.Z0_Description = "Not set";

			var trigger = PrepareTrigger(dummy);

			dummy.Z0_DescriptionInfo.ValueChanged += (sender, e) =>
			{
				bizo.Z0_Description = "Updated";
			};

			AssertNoExceptionThrown("Should not throw exception: ", () => Factory.Save());
			AssertType<DeveloperNotificationException>("Should report DeveloperNotificationException", ErrorReporter.LastExceptionReported);
			AssertContains("Should report error with message", "WorkflowSettingPropertiesAfterOnSaving", ErrorReporter.LastKeyReported);

			var expectMessage = @$"The Dummy Business Object Updated is protected from making changes when saving and its properties are not allowed to be changed by the Immediate Field Change (IFC) trigger action. Consider using a Set Field (FLD) trigger action instead.
Trigger Event Code: WTM.
Trigger Description: Edited a record.
Factory ID: {Factory._Instance}.
Trigger Actions:
IFC - <Z0_Description> - Set";
			AssertContains(expectMessage, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestTrackerInMultipleThreads()
		{
			var factory1 = new BusinessObjectFactory();
			var bizo1 = factory1.New<DumpWorkflowDisableWorkflowSettingPropertiesAfterOnSaving>();
			bizo1.Z0_IsSystem = false;
			bizo1.Z0_Description = "Not update";

			var trigger = PrepareTrigger(bizo1);

			using (new BusinessObjectPropertyChangeTracker(trigger, bizo1.Factory))
			{
				Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						var factory2 = new BusinessObjectFactory();
						var bizo2 = factory2.New<DumpWorkflowDisableWorkflowSettingPropertiesAfterOnSaving>();
						{
							bizo2.Z0_Description = "Updated in Thread 2";
						}
					}
				}).Wait();

				AssertNullOrEmpty(ErrorReporter.LastMessageReported);

				bizo1.Z0_Description = "Updated in Thread 1";

				AssertContains("The Dummy Business Object Updated in Thread 1 is protected from making changes when saving and its properties are not allowed to be changed by the Immediate Field Change (IFC) trigger action. Consider using a Set Field (FLD) trigger action instead.", ErrorReporter.LastMessageReported);
			}

			ErrorReporter.Clear();
		}

		public void TestOnPropertyChangedCausedByGetNull()
		{
			var sourceBizO = Factory.New<DummyWithWorkflow>();
			sourceBizO.Z0_IsSystem = false;
			sourceBizO.Z0_Description = "Not set";

			var trigger = PrepareTrigger(sourceBizO);

			sourceBizO.Z0_DescriptionInfo.ValueChanged += (_, _) =>
			{
				Factory.GetNull<DumpWorkflowDisableWorkflowSettingPropertiesAfterOnSaving>();
			};
			Factory.Save();

			AssertNullOrEmpty("Should not report because the protected BizO was created via GetNull(). It will NOT be saved into DB.", ErrorReporter.LastMessageReported);
		}

		IWorkflowTrigger PrepareTrigger(IWorkflowProvider bizo)
		{
			var trigger = (DummyProcessTask)bizo.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.WorkflowItemsManuallyAddedCode;
			trigger.P9_Description = "Edited a record";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<Z0_Description>";
			action.PQ_FieldValue = "Set";

			return trigger;
		}
	}
}

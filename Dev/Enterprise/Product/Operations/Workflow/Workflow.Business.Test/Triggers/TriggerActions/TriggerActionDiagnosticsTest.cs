using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(TriggerActionDiagnostics))]
	class TriggerActionDiagnosticsTest : WorkflowTestCase
	{
		public void TestGetUnsavedFiredTriggersInformation()
		{
			var dummyBusinessObject = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummyBusinessObject.Z0_Description = "XXX00001234";

			var creator = ObjectFactory.Get<ITriggerActionDiagnostics>();
			var actualMessage = creator.GetUnsavedFiredTriggersInformation(dummyBusinessObject.Factory);
			AssertNullOrEmpty(actualMessage);

			var ifcFieldName = DummyBizoSchema.Z0_NVarCharMax.Name;
			var ifcFieldValue = "IFC IFC IFC IFC";
			var trigger1 = dummyBusinessObject.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "Trigger 1";
			trigger1.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent01.Code;
			trigger1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			trigger1.TriggerConditions.TriggerConditionValue = "CMP=NOG";
			var action1 = trigger1.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action1.PQ_FieldName = ifcFieldName;
			action1.PQ_FieldValue = ifcFieldValue;

			var fldFieldName = DummyBizoSchema.Z0_NVarCharMax.Name;
			var fldFieldValue = "FLD FLD FLD FLD";
			var trigger2 = dummyBusinessObject.WorkflowItems.Triggers.AddNew();
			trigger2.P9_Description = "Trigger 2";
			trigger2.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent02.Code;
			trigger2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			trigger2.TriggerConditions.TriggerConditionValue = "DEP=NOG";
			var action2 = trigger2.ProcessTaskNotifications.AddNew();
			action2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action2.PQ_FieldName = fldFieldName;
			action2.PQ_FieldValue = fldFieldValue;

			var trigger3 = dummyBusinessObject.WorkflowItems.Triggers.AddNew();
			trigger3.P9_Description = "Trigger 3";
			trigger3.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent03.Code;
			trigger3.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			trigger3.TriggerConditions.TriggerConditionValue = "NAM=NOG";
			var action3 = trigger3.ProcessTaskNotifications.AddNew();
			action3.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;

			dummyBusinessObject.Logs.AddNew(AutoEvents.CustomisableEvent01, new KeyValuePair<string, string>("CMP", "NOG"));
			dummyBusinessObject.Logs.AddNew(AutoEvents.CustomisableEvent02, new KeyValuePair<string, string>("DEP", "NOG"));
			dummyBusinessObject.Logs.AddNew(AutoEvents.CustomisableEvent03, new KeyValuePair<string, string>("NAM", "NOG"));

			actualMessage = creator.GetUnsavedFiredTriggersInformation(dummyBusinessObject.Factory);
			AssertContains("Last Fired Time", actualMessage);
			AssertContains("Parent Table Code: Z0", actualMessage);
			AssertContains("Parent ID", actualMessage);
			AssertContains("Trigger 1 (Type: TRG, Event: Z01, Field: , Trigger Condition: RFP, Trigger Condition Value: CMP=NOG", actualMessage);
			AssertContains("Trigger Action - Trigger 1 'IFC'", actualMessage);
			AssertContains("Field Name: Z0_NVarCharMax, Field Value: IFC IFC IFC IFC", actualMessage);
			AssertContains("Trigger 2 (Type: TRG, Event: Z02, Field: , Trigger Condition: RFP, Trigger Condition Value: DEP=NOG", actualMessage);
			AssertContains("Trigger Action - Trigger 2 'FLD'", actualMessage);
			AssertContains("Field Name: Z0_NVarCharMax, Field Value: FLD FLD FLD FLD", actualMessage);
			AssertContains("Trigger 3 (Type: TRG, Event: Z03, Field: , Trigger Condition: RFP, Trigger Condition Value: NAM=NOG", actualMessage);
			AssertContains("Trigger Action - Trigger 3 'DOC'", actualMessage);

			AssertEquals("Should not have hit the database.", 0, dummyBusinessObject.Factory.GetTableHitCount(StmALogSchema.Constants.TableName));
		}
	}
}

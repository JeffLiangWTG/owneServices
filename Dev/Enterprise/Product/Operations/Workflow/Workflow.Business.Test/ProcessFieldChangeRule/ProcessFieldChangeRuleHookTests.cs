using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	public class ProcessFieldChangeRuleHookTests : TestCaseWithFactory
	{
		[TestDate(2020, 10, 25)]
		public void TestSingleFieldGroupUpdate()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";

			var field = rule.Fields.AddNew();
			field.PFL_FieldName = "Z0_Date";
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();

			var startDate = ZDateTime.UtcNow;
			dummy.Z0_Date = startDate;
			AssertMatch(new Regex($".*Z0_Date.*{Regex.Escape(ProcessFieldOnChangeHook.EmptyValue)}.*{startDate.ToString(null, CultureInfo.InvariantCulture)}"), dummy.Logs.GetAllLogs()[0].Parameters["CHG"]);
			factory.Save();

			TestDateAttribute.AddMinutes(5);
			var updateDate = ZDateTime.UtcNow;
			dummy.Z0_Date = updateDate;
			AssertMatch(new Regex($".*Z0_Date.*{startDate.ToString(null, CultureInfo.InvariantCulture)}.*{updateDate.ToString(null, CultureInfo.InvariantCulture)}"), dummy.Logs.GetAllLogs()[1].Parameters["CHG"]);
		}

		[TestDate(2020, 10, 2)]
		public void TestUnfiringFieldEventChangeEvents()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = AutoEvents.CustomisableEvent00.Code;
			rule.PFR_IsActive = true;

			ProcessFieldChangeRuleField.TurnOffBlacklistForTest.Value = true;

			var field = rule.Fields.AddNew();
			field.PFL_FieldName = "Z0_VarCharMax";

			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_VarCharMax = "AStartingValue";
			var triggerA = dummy.WorkflowItems.Triggers.AddNew();
			triggerA.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00.Code;
			triggerA.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			triggerA.TriggerConditions.TriggerConditionValue = "\"<Z0_VarCharMax>\"==\"One\"";
			triggerA.TriggerConditions.TriggerFiredCountdown = 10;

			var triggerB = dummy.WorkflowItems.Triggers.AddNew();
			triggerB.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00.Code;
			triggerB.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			triggerB.TriggerConditions.TriggerConditionValue = "\"<Z0_VarCharMax>\"==\"Two\"";
			triggerB.TriggerConditions.TriggerFiredCountdown = 10;

			Factory.Save();
			dummy.Z0_VarCharMax = "One";
			dummy.Z0_VarCharMax = "Two";
			AssertEquals((short)9, triggerB.TriggerConditions.TriggerFiredCountdown);
			AssertEquals((short)10, triggerA.TriggerConditions.TriggerFiredCountdown);
		}

		[TestDate(2020, 10, 25)]
		public void TestChildWithoutWorkflowToParent()
		{
			ProcessFieldChangeRuleField.TurnOffBlacklistForTest.Value = true;
			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("P9", new List<IBusinessObjectParentLocator>() { new ProcessTaskParentLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("P9", new ProcessTaskFieldChangeState());
			});

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "P9_ActualDate";
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent05.Code;
			dummy.Logs.AddNew(AutoEvents.CustomisableEvent05);

			AssertMatch(new Regex($".*P9_ActualDate.*{Regex.Escape(ProcessFieldOnChangeHook.EmptyValue)}.*{ZDateTime.UtcNow.ToString(null, CultureInfo.InvariantCulture)}"), dummy.Logs.GetAllLogs()[1].Parameters["CHG"]);
			factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedDummy = newFactory.Load<DummyWithWorkflow>(dummy.PK);

			TestDateAttribute.AddMinutes(5);
			loadedDummy.Logs.AddNew(AutoEvents.CustomisableEvent05);
			AssertMatch(new Regex($".*P9_ActualDate.*{ZDateTime.UtcNow.ToString(null, CultureInfo.InvariantCulture)}"), loadedDummy.Logs.GetAllLogs()[4].Parameters["CHG"]);
		}

		public void TestParentLocatorWithoutChildState_ErrorReport()
		{
			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("P9", new List<IBusinessObjectParentLocator>() { new ProcessTaskParentLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
			});

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "P9_ActualDate";
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent05.Code;
			dummy.Logs.AddNew(AutoEvents.CustomisableEvent05);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("ProcessFieldOnChangeHook.IBusinessObjectFieldChangeState", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestChildWithWorkflowToParent_RulesInParentOnly()
		{
			ProcessFieldChangeRuleField.TurnOffBlacklistForTest.Value = true;

			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new DummyWithWorkflow.ShipmentToDummyLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ShipmentFieldStateChange());
			});

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_GoodsDescription = "Odds";

			AssertEquals(0, dummy.Logs.GetAllLogs().Count);
			AssertEquals(0, shipment.Logs.GetAllLogs().Count);

			factory.Save();

			dummy.AttachChildShipment(shipment);

			AssertEquals(0, dummy.Logs.GetAllLogs().Count);
			AssertEquals(1, shipment.Logs.GetAllLogs().Count);
			AssertEquals(AutoEvents.AddedARecordToTheSystemCode, shipment.Logs.GetAllLogs()[0].Event.SE_Code);

			shipment.JS_GoodsDescription = "Sods";

			AssertMatch(new Regex($".*JS_GoodsDescription.*Odds.*Sods"), dummy.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertEquals(1, shipment.Logs.GetAllLogs().Count);

			factory.Save();
			AssertEquals(2, shipment.Logs.GetAllLogs().Count);
			AssertEquals(AutoEvents.EditedARecordCode, shipment.Logs.GetAllLogs()[1].Event.SE_Code);

			dummy.DetachChildShipment(shipment);

			shipment.JS_GoodsDescription = "Cods";
			AssertEquals(1, dummy.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Odds.*Sods"), dummy.Logs.GetAllLogs()[0].Parameters["CHG"]);
		}

		public void TestChildWithWorkflowToParent_RulesInChildOnly()
		{
			ProcessFieldChangeRuleField.TurnOffBlacklistForTest.Value = true;

			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new DummyWithWorkflow.ShipmentToDummyLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ShipmentFieldStateChange());
			});

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "SHP";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_GoodsDescription = "Odds";

			AssertEquals(0, dummy.Logs.GetAllLogs().Count);
			AssertEquals(1, shipment.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*JS_GoodsDescription.*{Regex.Escape(ProcessFieldOnChangeHook.EmptyValue)}.*Odds"), shipment.Logs.GetAllLogs()[0].Parameters["CHG"]);

			factory.Save();

			dummy.AttachChildShipment(shipment);

			AssertEquals(0, dummy.Logs.GetAllLogs().Count);
			AssertEquals(2, shipment.Logs.GetAllLogs().Count);

			shipment.JS_GoodsDescription = "Sods";

			AssertMatch(new Regex($".*JS_GoodsDescription.*Odds.*Sods"), shipment.Logs.GetAllLogs()[2].Parameters["CHG"]);
			AssertEquals(0, dummy.Logs.GetAllLogs().Count);
			AssertEquals(3, shipment.Logs.GetAllLogs().Count);

			factory.Save();
			AssertEquals(4, shipment.Logs.GetAllLogs().Count);
			AssertEquals(AutoEvents.EditedARecordCode, shipment.Logs.GetAllLogs()[3].Event.SE_Code);

			dummy.DetachChildShipment(shipment);

			shipment.JS_GoodsDescription = "Cods";
			AssertEquals(0, dummy.Logs.GetAllLogs().Count);
			AssertEquals(5, shipment.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Sods.*Cods"), shipment.Logs.GetAllLogs()[4].Parameters["CHG"]);
		}

		public void TestChildWithWorkflowToParent_RulesInChildAndParent()
		{
			ProcessFieldChangeRuleField.TurnOffBlacklistForTest.Value = true;

			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new DummyWithWorkflow.ShipmentToDummyLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ShipmentFieldStateChange());
			});

			var dummyRule = Factory.New<ProcessFieldChangeRule>();
			dummyRule.PFR_ProcessType = "DUM";
			dummyRule.PFR_GroupName = "Group1";
			dummyRule.PFR_SE_NKEvent = "Z02";
			dummyRule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			var shipmentRule = Factory.New<ProcessFieldChangeRule>();
			shipmentRule.PFR_ProcessType = "SHP";
			shipmentRule.PFR_GroupName = "Group2";
			shipmentRule.PFR_SE_NKEvent = "Z05";
			shipmentRule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			dummy.AttachChildShipment(shipment);
			shipment.JS_GoodsDescription = "Odds";

			AssertEquals(1, dummy.Logs.GetAllLogs().Count);
			AssertEquals(1, shipment.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*JS_GoodsDescription.*{Regex.Escape(ProcessFieldOnChangeHook.EmptyValue)}.*Odds"), dummy.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertMatch(new Regex($".*JS_GoodsDescription.*{Regex.Escape(ProcessFieldOnChangeHook.EmptyValue)}.*Odds"), shipment.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertEquals(AutoEvents.CustomisableEvent02.Code, dummy.Logs.GetAllLogs()[0].SL_SE_NKEvent);
			AssertEquals(AutoEvents.CustomisableEvent05.Code, shipment.Logs.GetAllLogs()[0].SL_SE_NKEvent);

			factory.Save();
			shipment.JS_GoodsDescription = "Sods";

			AssertMatch(new Regex($".*JS_GoodsDescription.*Odds.*Sods"), dummy.Logs.GetAllLogs()[1].Parameters["CHG"]);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Odds.*Sods"), shipment.Logs.GetAllLogs()[2].Parameters["CHG"]);
			AssertEquals(AutoEvents.CustomisableEvent02.Code, dummy.Logs.GetAllLogs()[1].SL_SE_NKEvent);
			AssertEquals(AutoEvents.CustomisableEvent05.Code, shipment.Logs.GetAllLogs()[2].SL_SE_NKEvent);
		}

		public void TestChildToParentRulesUpdateOnAttachDetach()
		{
			ProcessFieldChangeRuleConfigurationManager.ConfigurationForTest.Value = (WorkflowDescriptors.DummyWorkflowDescriptorCode, new DummyProcessFieldChangeRuleConfiguration());
			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new DummyWithWorkflow.ShipmentToDummyLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ShipmentFieldStateChange());
			});

			var dummyRule = Factory.New<ProcessFieldChangeRule>();
			dummyRule.PFR_ProcessType = "DUM";
			dummyRule.PFR_GroupName = "Group1";
			dummyRule.PFR_SE_NKEvent = "Z02";
			dummyRule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			var shipmentRule = Factory.New<ProcessFieldChangeRule>();
			shipmentRule.PFR_ProcessType = "SHP";
			shipmentRule.PFR_GroupName = "Group2";
			shipmentRule.PFR_SE_NKEvent = "Z05";
			shipmentRule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_GoodsDescription = "Odds";

			AssertEquals(0, dummy.Logs.GetAllLogs().Count);
			AssertEquals(1, shipment.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*JS_GoodsDescription.*{Regex.Escape(ProcessFieldOnChangeHook.EmptyValue)}.*Odds"), shipment.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertEquals(AutoEvents.CustomisableEvent05.Code, shipment.Logs.GetAllLogs()[0].SL_SE_NKEvent);
			factory.Save();

			dummy.AttachChildShipment(shipment);
			shipment.JS_GoodsDescription = "Sods";

			AssertEquals(1, dummy.Logs.GetAllLogs().Count);
			AssertEquals(3, shipment.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Odds.*Sods"), dummy.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Odds.*Sods"), shipment.Logs.GetAllLogs()[2].Parameters["CHG"]);
			AssertEquals(AutoEvents.CustomisableEvent02.Code, dummy.Logs.GetAllLogs()[0].SL_SE_NKEvent);
			AssertEquals(AutoEvents.CustomisableEvent05.Code, shipment.Logs.GetAllLogs()[2].SL_SE_NKEvent);
			factory.Save();

			dummy.DetachChildShipment(shipment);
			shipment.JS_GoodsDescription = "Cods";

			AssertEquals(1, dummy.Logs.GetAllLogs().Count);
			AssertEquals(5, shipment.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Sods.*Cods"), shipment.Logs.GetAllLogs()[4].Parameters["CHG"]);
			AssertEquals(AutoEvents.CustomisableEvent05.Code, shipment.Logs.GetAllLogs()[2].SL_SE_NKEvent);
		}

		public void TestChildWithWorkflowToParent_RulesInChildOnlyApplyWhenChildIsRoot()
		{
			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new DummyWithWorkflow.ShipmentToDummyLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ShipmentFieldStateChange());
			});

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "SHP";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			var shipment = QuotedBooking.New(QuoteBookingType.QuickBooking, factory).Booking;
			shipment.JS_GoodsDescription = "Odds";

			AssertEquals(0, dummy.Logs.GetAllLogs().Count);
			AssertEquals(0, shipment.Logs.GetAllLogs().Count);

			factory.Save();

			dummy.AttachChildShipment(shipment);

			AssertEquals(0, dummy.Logs.GetAllLogs().Count);
			AssertEquals(1, shipment.Logs.GetAllLogs().Count);

			new BuildConsolHelper().TurnBookingIntoShipment(shipment, null);
			shipment.JS_GoodsDescription = "Sods";

			AssertEquals(0, dummy.Logs.GetAllLogs().Count);
			AssertEquals(2, shipment.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Odds.*Sods"), shipment.Logs.GetAllLogs()[1].Parameters["CHG"]);
		}

		public void TestChildToMultipleParents()
		{
			ProcessFieldChangeRuleConfigurationManager.ConfigurationForTest.Value = (WorkflowDescriptors.DummyWorkflowDescriptorCode, new DummyProcessFieldChangeRuleConfiguration());
			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new DummyWithWorkflow.ShipmentToDummyLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ShipmentFieldStateChange());
			});

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy1 = factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummy2 = factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummy3 = factory.NewWithValidTestData<DummyWithWorkflow>();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_GoodsDescription = "Odds";

			AssertEquals(0, dummy1.Logs.GetAllLogs().Count);
			AssertEquals(0, dummy2.Logs.GetAllLogs().Count);
			AssertEquals(0, dummy3.Logs.GetAllLogs().Count);
			AssertEquals(0, shipment.Logs.GetAllLogs().Count);

			factory.Save();

			dummy1.AttachChildShipment(shipment);
			dummy2.AttachChildShipment(shipment);
			dummy3.AttachChildShipment(shipment);

			AssertEquals(0, dummy1.Logs.GetAllLogs().Count);
			AssertEquals(0, dummy2.Logs.GetAllLogs().Count);
			AssertEquals(0, dummy3.Logs.GetAllLogs().Count);
			AssertEquals(1, shipment.Logs.GetAllLogs().Count);
			AssertEquals(AutoEvents.AddedARecordToTheSystemCode, shipment.Logs.GetAllLogs()[0].Event.SE_Code);

			shipment.JS_GoodsDescription = "Sods";

			AssertMatch(new Regex($".*JS_GoodsDescription.*Odds.*Sods"), dummy1.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Odds.*Sods"), dummy2.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Odds.*Sods"), dummy3.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertEquals(1, shipment.Logs.GetAllLogs().Count);

			factory.Save();
			AssertEquals(2, shipment.Logs.GetAllLogs().Count);
			AssertEquals(AutoEvents.EditedARecordCode, shipment.Logs.GetAllLogs()[1].Event.SE_Code);

			dummy1.DetachChildShipment(shipment);
			dummy2.DetachChildShipment(shipment);

			shipment.JS_GoodsDescription = "Cods";
			AssertEquals(1, dummy1.Logs.GetAllLogs().Count);
			AssertEquals(1, dummy2.Logs.GetAllLogs().Count);
			AssertEquals(2, dummy3.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Sods.*Cods"), dummy3.Logs.GetAllLogs()[1].Parameters["CHG"]);
		}

		public void TestMultipleDifferentChildren_FieldChangeEventsOnParentAndChild()
		{
			ProcessFieldChangeRuleField.TurnOffBlacklistForTest.Value = true;
			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new ShipmentToQuotedBookingParentLocator() });
				businessObjectParentLocators.Add("TH", new List<IBusinessObjectParentLocator>() { new QuoteToQuotedBookingParentLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ShipmentFieldStateChange());
				fieldChangeState.Add("TH", new QuoteFieldStateChange());
			});

			var quotedBookingRule = Factory.New<IProcessFieldChangeRule>();
			quotedBookingRule.PFR_ProcessType = "QBK";
			quotedBookingRule.PFR_GroupName = "Group1";
			quotedBookingRule.PFR_SE_NKEvent = "Z01";
			quotedBookingRule.Fields.AddNew().PFL_FieldName = "TH_GlobalRateDescription";
			quotedBookingRule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			var quoteRule = Factory.New<IProcessFieldChangeRule>();
			quoteRule.PFR_ProcessType = "QTN";
			quoteRule.PFR_GroupName = "Group2";
			quoteRule.PFR_SE_NKEvent = "Z02";
			quoteRule.Fields.AddNew().PFL_FieldName = "TH_GlobalRateDescription";

			var shipmentRule = Factory.New<IProcessFieldChangeRule>();
			shipmentRule.PFR_ProcessType = "SHP";
			shipmentRule.PFR_GroupName = "Group3";
			shipmentRule.PFR_SE_NKEvent = "Z03";
			shipmentRule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";
			Factory.Save();

			var factory = new BusinessObjectFactory();

			ViewQuotedBooking viewQuotedBooking = factory.New<ViewQuotedBooking>();
			Quote quote = QuotedBooking.CreateNewQuote(factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			viewQuotedBooking.VB_TH = quote.PK;
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory).Booking;
			viewQuotedBooking.VB_JS = booking.PK;
			QuotedBooking quotedBooking = viewQuotedBooking.QuotedBooking;
			quote.TH_GlobalRateDescription = "Rate";
			booking.JS_GoodsDescription = "Odds";

			AssertEquals(1, quotedBooking.Logs.GetAllLogs().Count);
			AssertEquals(1, quote.Logs.GetAllLogs().Count);
			AssertEquals(0, booking.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*TH_GlobalRateDescription.*Rate"), quote.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertMatch(new Regex($".*TH_GlobalRateDescription.*Rate.*JS_GoodsDescription.*Odds"), quotedBooking.Logs.GetAllLogs()[0].Parameters["CHG"]);
		}

		public void TestMultipleDifferentChildren_EventUpdateUpdateAfterSave()
		{
			ProcessFieldChangeRuleField.TurnOffBlacklistForTest.Value = true;
			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new ShipmentToQuotedBookingParentLocator() });
				businessObjectParentLocators.Add("TH", new List<IBusinessObjectParentLocator>() { new QuoteToQuotedBookingParentLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ShipmentFieldStateChange());
				fieldChangeState.Add("TH", new QuoteFieldStateChange());
			});

			var quotedBookingRule = Factory.New<IProcessFieldChangeRule>();
			quotedBookingRule.PFR_ProcessType = "QBK";
			quotedBookingRule.PFR_GroupName = "Group1";
			quotedBookingRule.PFR_SE_NKEvent = "Z01";
			quotedBookingRule.Fields.AddNew().PFL_FieldName = "TH_GlobalRateDescription";
			quotedBookingRule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			var quoteRule = Factory.New<IProcessFieldChangeRule>();
			quoteRule.PFR_ProcessType = "QTN";
			quoteRule.PFR_GroupName = "Group2";
			quoteRule.PFR_SE_NKEvent = "Z02";
			quoteRule.Fields.AddNew().PFL_FieldName = "TH_GlobalRateDescription";

			var shipmentRule = Factory.New<IProcessFieldChangeRule>();
			shipmentRule.PFR_ProcessType = "SHP";
			shipmentRule.PFR_GroupName = "Group3";
			shipmentRule.PFR_SE_NKEvent = "Z03";
			shipmentRule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";
			Factory.Save();

			var factory = new BusinessObjectFactory();

			ViewQuotedBooking viewQuotedBooking = factory.New<ViewQuotedBooking>();
			Quote quote = QuotedBooking.CreateNewQuote(factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			viewQuotedBooking.VB_TH = quote.PK;
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory).Booking;
			viewQuotedBooking.VB_JS = booking.PK;
			QuotedBooking quotedBooking = viewQuotedBooking.QuotedBooking;
			booking.JS_GoodsDescription = "Odds";
			quote.TH_GlobalRateDescription = "Rate";
			factory.Save();
			booking.JS_GoodsDescription = "Sodds";

			AssertMatch(new Regex($".*TH_GlobalRateDescription.*Rate"), quote.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertMatch(new Regex($".*TH_GlobalRateDescription.*Rate.*JS_GoodsDescription.*Odds"), quotedBooking.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Sodds"), quotedBooking.Logs.GetAllLogs()[1].Parameters["CHG"]);
		}

		public void TestMultipleObjectsWrappedAroundARow()
		{
			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new DummyWithWorkflow.ShipmentToDummyLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ShipmentFieldStateChange());
			});

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy1 = factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummy2 = factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummy3 = factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummyWrap1 = factory.Load<DummyBusinessObject>(dummy1.PK);
			var dummyWrap2 = factory.Load<DummyBusinessObject>(dummy2.PK);
			var dummyWrap3 = factory.Load<DummyBusinessObject>(dummy2.PK);
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_GoodsDescription = "Odds";

			AssertEquals(0, dummy1.Logs.GetAllLogs().Count);
			AssertEquals(0, dummy2.Logs.GetAllLogs().Count);
			AssertEquals(0, dummy3.Logs.GetAllLogs().Count);
			AssertEquals(0, shipment.Logs.GetAllLogs().Count);

			factory.Save();

			dummy1.AttachChildShipment(shipment);
			dummy2.AttachChildShipment(shipment);
			dummy3.AttachChildShipment(shipment);

			AssertEquals(0, dummy1.Logs.GetAllLogs().Count);
			AssertEquals(0, dummy2.Logs.GetAllLogs().Count);
			AssertEquals(0, dummy3.Logs.GetAllLogs().Count);
			AssertEquals(1, shipment.Logs.GetAllLogs().Count);
			AssertEquals(AutoEvents.AddedARecordToTheSystemCode, shipment.Logs.GetAllLogs()[0].Event.SE_Code);

			shipment.JS_GoodsDescription = "Sods";
		}

		public void TestParentChildExceptionHandling_IBusinessObjectParentLocator_TryLocateParentBusinessObjectsThrows()
		{
			ProcessFieldChangeRuleConfigurationManager.ConfigurationForTest.Value = (WorkflowDescriptors.DummyWorkflowDescriptorCode, new DummyProcessFieldChangeRuleConfiguration());
			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new ThrowingBusinessObjectParentLocator(true, "JS") });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ShipmentFieldStateChange());
			});

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			dummy.AttachChildShipment(shipment);
			shipment.JS_GoodsDescription = "Odds";

			AssertEquals("ProcessFieldOnChangeHook.IBusinessObjectParentLocatorException", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestParentChildExceptionHandling_BusinessObjectRelationChangeSendsRubbish()
		{
			ProcessFieldChangeRuleField.TurnOffBlacklistForTest.Value = true;

			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new DummyWithWorkflow.ShipmentToDummyLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ShipmentFieldStateChange());
			});

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			dummy.AttachChildShipment(shipment);

			AssertExceptionThrown<Exception>(() => dummy.BusinessObjectRelationChanged(null, BusinessObjectParentLocatorEvent.ParentAdded));
			dummy.BusinessObjectRelationChanged(shipment, BusinessObjectParentLocatorEvent.ParentAdded);
			AssertEquals("ProcessFieldOnChangeHook.ParentDoubleHooked", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();

			shipment.JS_GoodsDescription = "Odds";

			AssertEquals(1, dummy.Logs.GetAllLogs().Count);
			AssertEquals(0, shipment.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Odds"), dummy.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
			factory.Save();

			AssertExceptionThrown<Exception>(() => dummy.BusinessObjectRelationChanged(null, BusinessObjectParentLocatorEvent.ParentRemoved));
			shipment.JS_GoodsDescription = "Sods";

			AssertEquals(2, dummy.Logs.GetAllLogs().Count);
			AssertEquals(1, shipment.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Odds.*Sods"), dummy.Logs.GetAllLogs()[1].Parameters["CHG"]);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
			factory.Save();

			dummy.BusinessObjectRelationChanged(shipment, BusinessObjectParentLocatorEvent.ParentRemoved);
			dummy.BusinessObjectRelationChanged(shipment, BusinessObjectParentLocatorEvent.ParentRemoved);
			AssertEquals("ProcessFieldOnChangeHook.ParentRemovedIncorrectly", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();

			dummy.BusinessObjectRelationChanged(shipment, BusinessObjectParentLocatorEvent.ParentAdded);
			shipment.JS_GoodsDescription = "Cods";

			AssertEquals(3, dummy.Logs.GetAllLogs().Count);
			AssertEquals(2, shipment.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Sods.*Cods"), dummy.Logs.GetAllLogs()[2].Parameters["CHG"]);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
			factory.Save();

			var someOtherShipment = factory.NewWithValidTestData<ForwardingShipment>();
			dummy.BusinessObjectRelationChanged(someOtherShipment, BusinessObjectParentLocatorEvent.ParentRemoved);
			AssertEquals("ProcessFieldOnChangeHook.ParentRemovedIncorrectly", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();

			shipment.JS_GoodsDescription = "Rods";

			AssertEquals(4, dummy.Logs.GetAllLogs().Count);
			AssertEquals(3, shipment.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Cods.*Rods"), dummy.Logs.GetAllLogs()[3].Parameters["CHG"]);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestParentChildExceptionHandling_IBusinessObjectFieldChangeState_IsObjectInitializedThrows()
		{
			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new DummyWithWorkflow.ShipmentToDummyLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ThrowingShipmentFieldStateChange(true, false, false));
			});

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			dummy.AttachChildShipment(shipment);
			shipment.JS_GoodsDescription = "Odds";

			AssertEquals("ProcessFieldOnChangeHook.IBusinessObjectFieldChangeStateException", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestParentChildExceptionHandling_IBusinessObjectFieldChangeState_IsRootObjectThrows()
		{
			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new DummyWithWorkflow.ShipmentToDummyLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ThrowingShipmentFieldStateChange(false, true, false));
			});

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			dummy.AttachChildShipment(shipment);
			shipment.JS_GoodsDescription = "Odds";

			AssertEquals("ProcessFieldOnChangeHook.IBusinessObjectFieldChangeStateException", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestParentChildExceptionHandling_IBusinessObjectFieldChangeState_RegisterBusinessObjectFieldStateChangedThrows()
		{
			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new DummyWithWorkflow.ShipmentToDummyLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ThrowingShipmentFieldStateChange(false, false, true));
			});

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			dummy.AttachChildShipment(shipment);
			shipment.JS_GoodsDescription = "Odds";

			AssertEquals("ProcessFieldOnChangeHook.IBusinessObjectFieldChangeStateException", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestParentChildExceptionHandling_BusinessObjectFieldStateChangeSendsRubbish()
		{
			ProcessFieldChangeRuleField.TurnOffBlacklistForTest.Value = true;

			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new DummyWithWorkflow.ShipmentToDummyLocator() });
			});

			var fieldStateChange = new ThrowingShipmentFieldStateChange(false, false, false);
			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", fieldStateChange);
			});

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			var shipment = QuotedBooking.CreateNewBooking(factory);
			dummy.AttachChildShipment(shipment);

			shipment.FieldChangeTrackerStateChanged(BusinessObjectFieldStateChangeEvent.ObjectIntialized);
			AssertEquals("ProcessFieldOnChangeHook.IBusinessObjectFieldChangeState", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();

			shipment.JS_GoodsDescription = "Odds";

			AssertEquals(1, dummy.Logs.GetAllLogs().Count);
			AssertEquals(0, shipment.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Odds"), dummy.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
			factory.Save();

			new BuildConsolHelper().TurnBookingIntoShipment(shipment, null);
			shipment.FieldChangeTrackerStateChanged(BusinessObjectFieldStateChangeEvent.ObjectNowRoot);
			AssertEquals("ProcessFieldOnChangeHook.IBusinessObjectFieldChangeState", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();

			shipment.JS_GoodsDescription = "Sods";

			AssertEquals(2, dummy.Logs.GetAllLogs().Count);
			AssertEquals(1, shipment.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Odds.*Sods"), dummy.Logs.GetAllLogs()[1].Parameters["CHG"]);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestDelayedInitialisation_RootObject()
		{
			ProcessFieldChangeRuleField.TurnOffBlacklistForTest.Value = true;

			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new DummyWithWorkflow.ShipmentToDummyLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ShipmentFieldStateChange());
			});

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			var rule2 = Factory.New<ProcessFieldChangeRule>();
			rule2.PFR_ProcessType = "SHP";
			rule2.PFR_GroupName = "Group2";
			rule2.PFR_SE_NKEvent = "Z01";
			rule2.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			ForwardingShipment shipment1;
			using (ShipmentFieldStateChange.InitializingShipment(factory))
			{
				shipment1 = factory.NewWithValidTestData<ForwardingShipment>();
				dummy.AttachChildShipment(shipment1);
			}

			shipment1.JS_GoodsDescription = "Odds";

			AssertEquals(1, dummy.Logs.GetAllLogs().Count);
			AssertEquals(1, shipment1.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Odds"), dummy.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertMatch(new Regex($".*JS_GoodsDescription.*Odds"), shipment1.Logs.GetAllLogs()[0].Parameters["CHG"]);
		}

		public void TestMultipleObjectWrappedAroundARow_DelayedInitialisation()
		{
			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("Z0", new List<IBusinessObjectParentLocator>() { new DummyToDummyParentLocator() });
			});

			var dummyStateChange = new DummyDelayedInitialisationStateChange();

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("Z0", dummyStateChange);
			});

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "Z0_NVarChar";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Description";

			Factory.Save();

			var factory = new BusinessObjectFactory();

			DummyWithWorkflow dummyWithWorkflow;
			DummyBusinessObject dummy;

			using (dummyStateChange.Initialize())
			{
				dummyWithWorkflow = factory.NewWithValidTestData<DummyWithWorkflow>();
				dummy = factory.Load<DummyBusinessObject>(dummyWithWorkflow.PK);
			}

			dummy.Z0_NVarChar = "Hello";

			AssertEquals(1, dummyWithWorkflow.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*Z0_NVarChar.*Hello"), dummyWithWorkflow.Logs.GetAllLogs()[0].Parameters["CHG"]);

			dummyWithWorkflow.Z0_Description = "Goodbye";

			AssertEquals(1, dummyWithWorkflow.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*Z0_Description.*Goodbye"), dummyWithWorkflow.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertMatch(new Regex($".*Z0_NVarChar.*Hello"), dummyWithWorkflow.Logs.GetAllLogs()[0].Parameters["CHG"]);
		}

		public void TestMultipleObjectWrappedAroundARow_OneObjectDoesNotSupportWorkflow_ParentAddRemove()
		{
			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("Z0", new List<IBusinessObjectParentLocator>() { new DummyToDummyParentLocator() });
			});

			var dummyStateChange = new DummyDelayedInitialisationStateChange();

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("Z0", dummyStateChange);
			});

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "Z0_NVarChar";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Description";

			Factory.Save();

			var factory = new BusinessObjectFactory();

			DummyWithWorkflow parent;
			DummyWithWorkflow dummyWithWorkflow;
			DummyBusinessObject dummy;

			using (dummyStateChange.Initialize())
			{
				parent = factory.NewWithValidTestData<DummyWithWorkflow>();
				dummyWithWorkflow = factory.NewWithValidTestData<DummyWithWorkflow>();
				dummy = factory.Load<DummyBusinessObject>(dummyWithWorkflow.PK);
			}

			parent.BusinessObjectRelationChanged(dummy, BusinessObjectParentLocatorEvent.ParentAdded);
			dummy.Z0_NVarChar = "Hello";

			AssertEquals(1, dummyWithWorkflow.Logs.GetAllLogs().Count);
			AssertEquals(1, parent.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*Z0_NVarChar.*Hello"), parent.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertMatch(new Regex($".*Z0_NVarChar.*Hello"), dummyWithWorkflow.Logs.GetAllLogs()[0].Parameters["CHG"]);

			dummyWithWorkflow.Z0_Description = "Goodbye";

			AssertEquals(1, dummyWithWorkflow.Logs.GetAllLogs().Count);
			AssertEquals(1, parent.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*Z0_Description.*Goodbye"), dummyWithWorkflow.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertMatch(new Regex($".*Z0_NVarChar.*Hello"), dummyWithWorkflow.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertMatch(new Regex($".*Z0_Description.*Goodbye"), parent.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertMatch(new Regex($".*Z0_NVarChar.*Hello"), parent.Logs.GetAllLogs()[0].Parameters["CHG"]);

			parent.BusinessObjectRelationChanged(dummy, BusinessObjectParentLocatorEvent.ParentRemoved);

			dummy.Z0_Description = "AAA";
			dummyWithWorkflow.Z0_NVarChar = "BBB";

			AssertEquals(1, dummyWithWorkflow.Logs.GetAllLogs().Count);
			AssertEquals(1, parent.Logs.GetAllLogs().Count);
			AssertNoMatch(new Regex($".*Z0_Description.*AAA"), parent.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertNoMatch(new Regex($".*Z0_Description.*BBB"), parent.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertMatch(new Regex($".*Z0_NVarChar.*BBB"), dummyWithWorkflow.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertMatch(new Regex($".*Z0_Description.*AAA"), dummyWithWorkflow.Logs.GetAllLogs()[0].Parameters["CHG"]);
		}

		public void TestMultipleObjectWrappedAroundARow_OneObjectDoesNotSupportWorkflow_DoubleAttachReportsError()
		{
			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("Z0", new List<IBusinessObjectParentLocator>() { new DummyToDummyParentLocator() });
			});

			var dummyStateChange = new DummyDelayedInitialisationStateChange();

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("Z0", dummyStateChange);
			});

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "Z0_NVarChar";

			Factory.Save();

			var factory = new BusinessObjectFactory();

			DummyWithWorkflow parent;
			DummyWithWorkflow dummyWithWorkflow;
			DummyBusinessObject dummy;

			using (dummyStateChange.Initialize())
			{
				parent = factory.NewWithValidTestData<DummyWithWorkflow>();
				dummyWithWorkflow = factory.NewWithValidTestData<DummyWithWorkflow>();
				dummy = factory.Load<DummyBusinessObject>(dummyWithWorkflow.PK);
			}

			parent.BusinessObjectRelationChanged(dummyWithWorkflow, BusinessObjectParentLocatorEvent.ParentAdded);
			parent.BusinessObjectRelationChanged(dummy, BusinessObjectParentLocatorEvent.ParentAdded);

			AssertEquals("ProcessFieldOnChangeHook.ParentDoubleHooked", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();

			dummy.Z0_NVarChar = "Hello";

			AssertEquals(1, dummyWithWorkflow.Logs.GetAllLogs().Count);
			AssertEquals(1, parent.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*Z0_NVarChar.*Hello"), parent.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertMatch(new Regex($".*Z0_NVarChar.*Hello"), dummyWithWorkflow.Logs.GetAllLogs()[0].Parameters["CHG"]);
		}

		public void TestMultipleObjectWrappedAroundARow_OneObjectDoesNotSupportWorkflow_DoubleDetachReportsError()
		{
			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("Z0", new List<IBusinessObjectParentLocator>() { new DummyToDummyParentLocator() });
			});

			var dummyStateChange = new DummyDelayedInitialisationStateChange();

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("Z0", dummyStateChange);
			});

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "Z0_NVarChar";

			Factory.Save();

			var factory = new BusinessObjectFactory();

			DummyWithWorkflow parent;
			DummyWithWorkflow dummyWithWorkflow;
			DummyBusinessObject dummy;

			using (dummyStateChange.Initialize())
			{
				parent = factory.NewWithValidTestData<DummyWithWorkflow>();
				dummyWithWorkflow = factory.NewWithValidTestData<DummyWithWorkflow>();
				dummy = factory.Load<DummyBusinessObject>(dummyWithWorkflow.PK);
			}

			parent.BusinessObjectRelationChanged(dummyWithWorkflow, BusinessObjectParentLocatorEvent.ParentAdded);
			parent.BusinessObjectRelationChanged(dummy, BusinessObjectParentLocatorEvent.ParentRemoved);
			AssertEquals(0, ErrorReporter.TotalErrorCount);

			parent.BusinessObjectRelationChanged(dummyWithWorkflow, BusinessObjectParentLocatorEvent.ParentRemoved);

			AssertEquals("ProcessFieldOnChangeHook.ParentRemovedIncorrectly", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestParentAddedEvent_ConvertQuoteToQuotedBooking()
		{
			var quotedBookingRule = Factory.New<IProcessFieldChangeRule>();
			quotedBookingRule.PFR_ProcessType = "QBK";
			quotedBookingRule.PFR_GroupName = "Group1";
			quotedBookingRule.PFR_SE_NKEvent = "Z01";
			quotedBookingRule.Fields.AddNew().PFL_FieldName = "TH_GlobalRateDescription";
			quotedBookingRule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			Factory.Save();

			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };

			var quotedBooking1 = factory1.Load<QuotedBooking>(quotedBooking.PK);
			quotedBooking1.ConvertQuoteToQuotedBooking();
			factory1.Save();
			var quotedBooking2 = factory2.Load<QuotedBooking>(quotedBooking.PK);
			quotedBooking2.ConvertQuoteToQuotedBooking();
			factory2.Save();

			AssertEquals("Parent Added event should not be added twice", ErrorReporter.TotalErrorCount, 0);

			quotedBooking2.BusinessObjectRelationChanged(quotedBooking2.Booking, BusinessObjectParentLocatorEvent.ParentAdded);
			AssertEquals("Parent Added was already added so expecting error report", "ProcessFieldOnChangeHook.ParentDoubleHooked", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestParentAddedEvent_NewQuotedBookingFromTemplateRecord()
		{
			var quotedBookingRule = Factory.New<IProcessFieldChangeRule>();
			quotedBookingRule.PFR_ProcessType = "QBK";
			quotedBookingRule.PFR_GroupName = "Group1";
			quotedBookingRule.PFR_SE_NKEvent = "Z01";
			quotedBookingRule.Fields.AddNew().PFL_FieldName = "TH_GlobalRateDescription";
			quotedBookingRule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";
			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			var templateRecord = factory1.NewWithValidTestData<StmTemplateRecord>();
			templateRecord.STR_ModuleID = "QuotedBookings";
			var quotedBooking = QuotedBooking.New(factory1, templateRecord);

			AssertEquals("Parent Added event should not be added twice", ErrorReporter.TotalErrorCount, 0);

			quotedBooking.BusinessObjectRelationChanged(quotedBooking.Booking, BusinessObjectParentLocatorEvent.ParentAdded);
			AssertEquals("Parent Added was already added so expecting error report", "ProcessFieldOnChangeHook.ParentDoubleHooked", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestFieldChangesAreIgnoredDuringIntialization()
		{
			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("Z0", new List<IBusinessObjectParentLocator>() { new DummyToDummyParentLocator() });
			});

			var dummyStateChange = new DummyDelayedInitialisationStateChange();

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("Z0", dummyStateChange);
			});

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "Z0_NVarChar";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Description";

			Factory.Save();

			var factory = new BusinessObjectFactory();
			DummyWithWorkflow dummyWithWorkflow;

			using (dummyStateChange.Initialize())
			{
				dummyWithWorkflow = factory.NewWithValidTestData<DummyWithWorkflow>();
				dummyWithWorkflow.Z0_NVarChar = "Hello";
			}

			AssertEquals(0, dummyWithWorkflow.Logs.GetAllLogs().Count);

			dummyWithWorkflow.Z0_NVarChar = "Goodbye";
			AssertEquals(1, dummyWithWorkflow.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*Z0_NVarChar.*Hello.*Goodbye"), dummyWithWorkflow.Logs.GetAllLogs()[0].Parameters["CHG"]);
		}

		public void TestMultipleObjectWrappedAroundARow_OneObjectDoesNotSupportWorkflow_CreationOrder()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "Z0_NVarChar";

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();
			var dummyWithWorkflow = factory.Load<DummyWithWorkflow>(dummy.PK);

			dummy.Z0_NVarChar = "Hello";
			AssertEquals(1, dummyWithWorkflow.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*Z0_NVarChar.*Hello"), dummyWithWorkflow.Logs.GetAllLogs()[0].Parameters["CHG"]);

			dummyWithWorkflow.Z0_NVarChar = "Goodbye";
			AssertEquals(1, dummyWithWorkflow.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*Z0_NVarChar.*Goodbye"), dummyWithWorkflow.Logs.GetAllLogs()[0].Parameters["CHG"]);

			// Test the opposite creation order

			dummyWithWorkflow = factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy = factory.Load<DummyWithWorkflow>(dummyWithWorkflow.PK);

			dummy.Z0_NVarChar = "Hello";
			AssertEquals(1, dummyWithWorkflow.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*Z0_NVarChar.*Hello"), dummyWithWorkflow.Logs.GetAllLogs()[0].Parameters["CHG"]);

			dummyWithWorkflow.Z0_NVarChar = "Goodbye";
			AssertEquals(1, dummyWithWorkflow.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*Z0_NVarChar.*Goodbye"), dummyWithWorkflow.Logs.GetAllLogs()[0].Parameters["CHG"]);
		}

		public void TestBusinessObjectDoesNotHandleReentrancy()
		{
			// TODO: This test will be filled out once the reentrancy is actually handled.
			// TODO: Uncomment DummyThatDoesNotLikeReEntrancy below
			Assert(true);
		}

		[TestDate(2020, 10, 25)]
		public void TestSingleFieldGroupUpdate_NoUpdateWhenChangeReverted_BizoNotYetSaved()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";

			var field = rule.Fields.AddNew();
			field.PFL_FieldName = "Z0_Decimal";
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_Decimal = 52.3;

			AssertMatch(new Regex($"{Regex.Escape(ProcessFieldOnChangeHook.EmptyValue)}.*{dummy.Z0_Decimal}]"), dummy.Logs.GetAllLogs()[0].Parameters["CHG"]);

			TestDateAttribute.AddMinutes(5);
			dummy.Z0_Decimal = 0;

			factory.Save();
			AssertEquals(0, dummy.Logs.DatabaseCount);
		}

		[TestDate(2020, 10, 25)]
		public void TestSingleFieldGroupUpdate_NoUpdateWhenChangeReverted_BizoFirstSaved()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";

			var field = rule.Fields.AddNew();
			field.PFL_FieldName = "Z0_Decimal";
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_Decimal = 52.3;

			AssertMatch(new Regex($"{Regex.Escape(ProcessFieldOnChangeHook.EmptyValue)}.*{dummy.Z0_Decimal}]"), dummy.Logs.GetAllLogs()[0].Parameters["CHG"]);
			factory.Save();

			TestDateAttribute.AddMinutes(5);
			dummy.Z0_Decimal = 65.5;

			AssertMatch(new Regex($"{52}.*{dummy.Z0_Decimal}"), dummy.Logs.GetAllLogs()[1].Parameters["CHG"]);

			TestDateAttribute.AddMinutes(5);
			dummy.Z0_Decimal = 52.3;
			AssertMatch(new Regex($"{Regex.Escape(ProcessFieldOnChangeHook.EmptyValue)}.*{dummy.Z0_Decimal}]"), dummy.Logs.GetAllLogs()[0].Parameters["CHG"]);
			AssertEquals(1, dummy.Logs.GetAllLogs().Count);

			factory.Save();
			AssertEquals(1, dummy.Logs.DatabaseCount);
		}

		[TestDate(2020, 10, 25)]
		public void TestUnfireTriggerWithMCRConditionWhenChangeReverted()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = AutoEvents.CustomisableEvent00Code;

			var field = rule.Fields.AddNew();
			field.PFL_FieldName = "Z0_Decimal";
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			trigger.TriggerConditions.TriggerConditionValue = "TriggerSource.Z0_Decimal == \"65\"";
			factory.Save();

			TestDateAttribute.AddMinutes(5);
			dummy.Z0_Decimal = 65;
			AssertEquals("Expecting 1 field change event", 1, dummy.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.CustomisableEvent00Code).Count());
			AssertEquals("Expecting trigger to fire once", 1, trigger.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.WorkflowTriggerEventCode).Count());
			TestDateAttribute.AddMinutes(5);
			dummy.Z0_Decimal = 0;

			factory.Save();

			AssertEquals("Expecting no event as field change was reverted", 0, dummy.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.CustomisableEvent00Code).Count());
			AssertEquals("Trigger should unfire when event is deleted", 0, trigger.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.WorkflowTriggerEventCode).Count());
		}

		[TestDate(2020, 10, 25)]
		public void TestMultiFieldGroupUpdate()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";

			rule.Fields.AddNew().PFL_FieldName = "Z0_Date";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Decimal";
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();

			var startDate = ZDateTime.UtcNow;
			dummy.Z0_Date = startDate;

			var changeLog = dummy.Logs.GetAllLogs()[0].Parameters["CHG"];
			AssertMatch(new Regex($".*Z0_Date.*{Regex.Escape(ProcessFieldOnChangeHook.EmptyValue)}.*{startDate.ToString(null, CultureInfo.InvariantCulture)}]"), changeLog);
			factory.Save();

			TestDateAttribute.AddMinutes(5);
			var updateDate = ZDateTime.UtcNow;
			dummy.Z0_Date = updateDate;
			changeLog = dummy.Logs.GetAllLogs()[1].Parameters["CHG"];

			AssertMatch(new Regex($".*Z0_Date.*{startDate.ToString(null, CultureInfo.InvariantCulture)}.*{updateDate.ToString(null, CultureInfo.InvariantCulture)}]"), changeLog);

			TestDateAttribute.AddMinutes(5);
			dummy.Z0_Decimal = 52.3;
			changeLog = dummy.Logs.GetAllLogs()[1].Parameters["CHG"];

			AssertMatch(new Regex($".*Z0_Date.*{startDate.ToString(null, CultureInfo.InvariantCulture)}.*{updateDate.ToString(null, CultureInfo.InvariantCulture)}]"), changeLog);
			AssertMatch(new Regex($".*Z0_Decimal.*{Regex.Escape(ProcessFieldOnChangeHook.EmptyValue)}.*{dummy.Z0_Decimal}]"), changeLog);
		}

		public void TestMultiFieldGroup_NoChangeNoUpdate()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";

			rule.Fields.AddNew().PFL_FieldName = "Z0_AnotherDecimal";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Decimal";
			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			factory.Save();

			dummy.Z0_Decimal = 0;
			AssertEquals(null, dummy.Logs.MostRecentLogByPostedDate);
		}

		public void TestMultiFieldGroup_UpdatesOnlyShowChangesInSave()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";

			rule.Fields.AddNew().PFL_FieldName = "Z0_AnotherDecimal";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Decimal";
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_Decimal = 22;
			factory.Save();

			var changeLog = dummy.Logs.GetAllLogs()[0].Parameters["CHG"];
			AssertContains(dummy.Z0_Decimal.ToString(), changeLog);
			AssertNotContains(DummyBizoSchema.Constants.Z0_AnotherDecimal, changeLog);

			dummy.Z0_AnotherDecimal = 42.2;

			changeLog = dummy.Logs.GetAllLogs()[1].Parameters["CHG"];
			AssertContains(dummy.Z0_AnotherDecimal.ToString(), changeLog);
			AssertNotContains(dummy.Z0_Decimal.ToString(), changeLog);
		}

		public void TestMultiFieldGroup_MCRMacroWorks()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";

			rule.Fields.AddNew().PFL_FieldName = "Z0_AnotherDecimal";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Decimal";
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger1 = dummy.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "Hello";
			trigger1.TriggerConditions.TriggerEventCode = "Z00";
			trigger1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			trigger1.TriggerConditions.TriggerConditionValue = @"Source.Z0_Decimal == 33";

			factory.Save();
			dummy.Z0_Decimal = 44;
			AssertEquals((short)100, trigger1.P9_TriggerFiredCountdown);
			dummy.Z0_Decimal = 33;
			AssertEquals((short)99, trigger1.P9_TriggerFiredCountdown);
		}

		public void TestMultiFieldGroup_MCR_OldToNew()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";

			rule.Fields.AddNew().PFL_FieldName = "Z0_AnotherDecimal";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Decimal";
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger1 = dummy.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "Hello";
			trigger1.TriggerConditions.TriggerEventCode = "Z00";
			trigger1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			trigger1.TriggerConditions.TriggerConditionValue = @"Source.Z0_Decimal == 33 && Source.Z0_DecimalInfo.OriginalValue == 11";
			dummy.Z0_Decimal = 11;
			factory.Save();
			AssertEquals((short)100, trigger1.P9_TriggerFiredCountdown);
			dummy.Z0_Decimal = 33;
			AssertEquals((short)99, trigger1.P9_TriggerFiredCountdown);
		}

		[TestDate(2020, 10, 25)]
		public void TestMultiFieldGroupUpdateOutOfSpace_ManyParameterNames()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "Z0_AnotherDate";
			rule.Fields.AddNew().PFL_FieldName = "Z0_AnotherNumber";
			rule.Fields.AddNew().PFL_FieldName = "Z0_BitFalse";
			rule.Fields.AddNew().PFL_FieldName = "Z0_BitFiltered";
			rule.Fields.AddNew().PFL_FieldName = "Z0_BitTrue";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Byte";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Bool";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Code";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Date";
			rule.Fields.AddNew().PFL_FieldName = "Z0_DateOnly";
			rule.Fields.AddNew().PFL_FieldName = "Z0_DateTimeOffset";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Decimal";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Description";
			rule.Fields.AddNew().PFL_FieldName = "Z0_FK_Code";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Long";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Money";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Number";
			rule.Fields.AddNew().PFL_FieldName = "Z0_NVarChar";
			rule.Fields.AddNew().PFL_FieldName = "Z0_NVarCharMax";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Short";
			rule.Fields.AddNew().PFL_FieldName = "Z0_SmallDateTime";
			rule.Fields.AddNew().PFL_FieldName = "Z0_VarBinaryMax";
			rule.Fields.AddNew().PFL_FieldName = "Z0_VarCharMax";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Xml";
			rule.Fields.AddNew().PFL_FieldName = "PFR_Reference";

			rule.PFR_Reference = new string('A', ProcessFieldChangeRuleSchema.PFR_Reference.MaxLength);

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();

			var startDate = ZDateTime.UtcNow;
			dummy.Z0_AnotherDate = startDate;
			dummy.Z0_AnotherNumber = 1;
			dummy.Z0_BitFalse = !dummy.Z0_BitFalse;
			dummy.Z0_BitFiltered = !dummy.Z0_BitFiltered;
			dummy.Z0_BitTrue = !dummy.Z0_BitTrue;
			dummy.Z0_Bool = !dummy.Z0_Bool;
			dummy.Z0_Byte = 1;
			dummy.Z0_Code = "ABC";
			dummy.Z0_Date = startDate;
			dummy.Z0_DateOnly = startDate.Date;
			dummy.Z0_DateTimeOffset = new DateTimeOffset();
			dummy.Z0_Decimal = 52.3;
			dummy.Z0_Description = "ABC";
			dummy.Z0_FK_Code = "ABC";
			dummy.Z0_Long = 1;
			dummy.Z0_Money = 1;
			dummy.Z0_Number = 1;
			dummy.Z0_NVarChar = "ABC";
			dummy.Z0_NVarCharMax = new string('A', StmALogSchema.SL_Reference.MaxLength);
			dummy.Z0_Short = 1;
			dummy.Z0_SmallDateTime = startDate.Date;
			dummy.Z0_VarBinaryMax = new byte[10];
			dummy.Z0_VarCharMax = "ABC";
			dummy.Z0_Xml = "ABC";

			var changeLog = dummy.Logs.GetAllLogs()[0].Parameters["CHG"];

			AssertContains(DummyBizoSchema.Constants.Z0_AnotherDate, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_AnotherNumber, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_BitFalse, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_BitFiltered, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_BitTrue, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_Bool, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_Byte, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_Code, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_Date, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_DateOnly, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_DateTimeOffset, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_Decimal, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_Description, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_FK_Code, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_Long, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_Money, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_Number, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_NVarChar, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_NVarCharMax, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_Short, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_SmallDateTime, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_VarBinaryMax, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_VarCharMax, changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_Xml, changeLog);
		}

		public void TestMultiFieldGroupUpdateOutOfSpace_StringShrink()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "Z0_VarCharMax";
			rule.PFR_Reference = new string('A', ProcessFieldChangeRuleSchema.PFR_Reference.MaxLength);
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_VarCharMax = "A string that is quite long and thus too long to fit" + new string('A', StmALogSchema.SL_Reference.MaxLength - ProcessFieldChangeRuleSchema.PFR_Reference.MaxLength - 50);

			var changeLog = dummy.Logs.GetAllLogs()[0].Parameters["CHG"];
			AssertContains(DummyBizoSchema.Constants.Z0_VarCharMax, changeLog);
			AssertContains($"{dummy.Z0_VarCharMax.Length} bytes", changeLog);
		}

		public void TestMultiFieldGroupUpdateOutOfSpace_StringShrinkBiggest()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "Z0_VarCharMax";
			rule.Fields.AddNew().PFL_FieldName = "Z0_NVarChar";
			rule.Fields.AddNew().PFL_FieldName = "Z0_Description";
			rule.PFR_Reference = new string('A', ProcessFieldChangeRuleSchema.PFR_Reference.MaxLength);
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_NVarChar = "Small string 1";
			dummy.Z0_VarCharMax = "A string that is quite long and thus too long to fit" + new string('A', StmALogSchema.SL_Reference.MaxLength - ProcessFieldChangeRuleSchema.PFR_Reference.MaxLength - 50);
			dummy.Z0_Description = "Small string 2";

			var changeLog = dummy.Logs.GetAllLogs()[0].Parameters["CHG"];
			AssertContains(DummyBizoSchema.Constants.Z0_VarCharMax, changeLog);
			AssertContains($"{dummy.Z0_VarCharMax.Length} bytes", changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_NVarChar, changeLog);
			AssertContains("Small string 1", changeLog);
			AssertContains(DummyBizoSchema.Constants.Z0_Description, changeLog);
			AssertContains("Small string 2", changeLog);
		}

		public void TestMultiFieldGroupUpdate_StringHandlesUnicode()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "Z0_NVarChar";
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_NVarChar = "井の中の蛙、大海を知らず";
			factory.Save();
			factory.ReloadAll<DummyWithWorkflow>();

			var changeLog = dummy.Logs.GetAllLogs()[0].Parameters["CHG"];
			AssertContains(DummyBizoSchema.Constants.Z0_NVarChar, changeLog);
			AssertContains("12 bytes", changeLog);
		}

		public void TestMultiUnderscoreFieldName()
		{
			ProcessFieldChangeRuleField.TurnOffBlacklistForTest.Value = true;

			ParentLocatorFactoryOverridesForTest.SetOnGetParentLocatorsHookForTest((ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators) =>
			{
				businessObjectParentLocators = new Dictionary<string, List<IBusinessObjectParentLocator>>();
				businessObjectParentLocators.Add("JS", new List<IBusinessObjectParentLocator>() { new DummyWithWorkflow.ShipmentToDummyLocator() });
			});

			ParentLocatorFactoryOverridesForTest.SetOnGetBusinessObjectStatesHookForTest((ref Dictionary<string, IBusinessObjectFieldChangeState> fieldChangeState) =>
			{
				fieldChangeState = new Dictionary<string, IBusinessObjectFieldChangeState>();
				fieldChangeState.Add("JS", new ShipmentFieldStateChange());
			});

			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";
			rule.Fields.AddNew().PFL_FieldName = "JS_E_DEP";

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			dummy.AttachChildShipment(shipment);
			shipment.JS_E_DEP = ZDateTime.Now;

			AssertEquals(1, shipment.Logs.GetAllLogs().Count);
			AssertMatch(new Regex($".*JS_E_DEP."), dummy.Logs.GetAllLogs()[0].Parameters["CHG"]);
		}

		public void TestDataReaderExceptionHandled()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "DUM";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z00";

			var field = rule.Fields.AddNew();
			field.PFL_FieldName = "Z0_Date";
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();

			var processHook = new ProcessFieldOnChangeHook(() => CachedParentLocators.BusinessObjectParentLocators, () => CachedParentLocators.BusinessObjectFieldChangeStates);

			factory.ClearCachedValue<HashSet<string>>("PFRLookups.HookedTables");

			PersistentFactoryCacheManager.Instance.ClearAllQueryCaches();

			using (DbCommand command = ((IDbConnected)factory).Connection.Command("Select * From dbo.ProcessFieldChangeRule"))
			{
				using (command.ExecuteReader())
				{
					AssertNoExceptionThrown(() => { processHook.OnBusinessObjectInitialized(dummy); });
				}
			}
		}

		[StressTest]
		public void TestAllConfigurableFieldsRaiseEvents()
		{
			var workflowDescriptors = WorkflowDescriptors.Instance.Values;
			var configManager = new ProcessFieldChangeRuleConfigurationManager();
			int groupCounter = 0;

			foreach (var workflowDescriptor in workflowDescriptors)
			{
				++groupCounter;
				var rule = Factory.New<ProcessFieldChangeRule>();
				rule.PFR_ProcessType = workflowDescriptor.Code;
				rule.PFR_GroupName = $"Group{groupCounter}";
				rule.PFR_SE_NKEvent = "Z00";
				foreach (var field in configManager.GetFields(workflowDescriptor.Code))
				{
					rule.Fields.AddNew().PFL_FieldName = ((ICodeDescription)field).Code;
				}
			}

			Factory.Save();

			CombineAssertions(() =>
			{
				var propertiesWeCannaSet = ExpectedPropertiesWeCannaSet();

				foreach (var workflowDescriptor in workflowDescriptors.Where(c => !noRealWorkflowProviderCodes.Value.Contains(c.Code)))
				{
					var factory = new BusinessObjectFactory();
					BusinessObject bizo;
					try
					{
						bizo = factory.New(workflowDescriptor.WorkflowProviderType);
						bizo.SuspendValidation();
					}
					catch
					{
						try
						{
							TryHarder();
						}
						catch (Exception)
						{
							// Uncomment this fail to see what throws exceptions despite the trying of hardness
							// Fail($"Exception {ex.InnerException?.Message ?? ex.Message} occured creating new {workflowDescriptor.WorkflowProviderType}");
							continue;
						}

						continue;
					}

					foreach (var field in configManager.GetFields(workflowDescriptor.Code))
					{
						if (propertiesWeCannaSet.Contains(((ICodeDescription)field).Code))
						{
							continue;
						}

						object val;

						try
						{
							val = bizo.GetPropertyValue(((ICodeDescription)field).Code);
						}
						catch (Exception ex)
						{
							Fail($"Exception {ex.InnerException?.Message ?? ex.Message} occured getting property value {((ICodeDescription)field).Code} for type {bizo.GetType().FullName}, workflow {workflowDescriptor.Code}");
							CleanUpFactory();
							continue;
						}

						var newVal = val;

						if (typeof(ZString).IsAssignableFrom(val.GetType()))
						{
							newVal = new ZString("~");
						}
						else if (typeof(ZShort).IsAssignableFrom(val.GetType()))
						{
							newVal = new ZShort(42);
						}
						else if (typeof(ZInt).IsAssignableFrom(val.GetType()))
						{
							newVal = new ZInt(42);
						}
						else if (typeof(ZDecimal).IsAssignableFrom(val.GetType()))
						{
							newVal = new ZDecimal(42);
						}
						else if (typeof(ZDate).IsAssignableFrom(val.GetType()))
						{
							newVal = ZDate.Today;
						}
						else if (typeof(ZDateTime).IsAssignableFrom(val.GetType()))
						{
							newVal = ZDateTime.Now;
						}
						else if (typeof(ZDateTimeOffset).IsAssignableFrom(val.GetType()))
						{
							newVal = ZDateTimeOffset.Now;
						}
						else if (typeof(ZGeography).IsAssignableFrom(val.GetType()))
						{
							newVal = new ZGeography("-121 48");
						}
						else if (typeof(ZGuid).IsAssignableFrom(val.GetType()))
						{
							newVal = ZGuid.NewZGuid();
						}
						else if (typeof(ZBool).IsAssignableFrom(val.GetType()))
						{
							newVal = new ZBool(!(ZBool)val);
						}
						else if (typeof(ZByte).IsAssignableFrom(val.GetType()))
						{
							newVal = new ZByte(0x42);
						}
						else if (typeof(ZBlob).IsAssignableFrom(val.GetType()))
						{
							newVal = new ZBlob(new byte[] { 1, 4, 9, 16, 25, 10, 13, 34, 43, 89, 10, 13, 25, 87, 34, 87, 9 });
						}
						else
						{
							Fail($"Unknown property type {val.GetType().FullName} for {((ICodeDescription)field).Code} for type {bizo.GetType().FullName}, workflow {workflowDescriptor.Code}");
							CleanUpFactory();
							continue;
						}

						if (((IStmALogProvider)bizo).Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.CustomisableEvent00.Code)).Where(log => !log.IsInDatabase).ToList().Count > 0)
						{
							Fail($"Event exists without setting anything {((ICodeDescription)field).Code} for type {bizo.GetType().FullName}, workflow {workflowDescriptor.Code}");
							CleanUpFactory();
							continue;
						}

						try
						{
							bizo.SetPropertyValue(((ICodeDescription)field).Code, newVal);
						}
						catch
						{
							try
							{
								TryHarder();
								bizo.SetPropertyValue(((ICodeDescription)field).Code, newVal);
							}
							catch (Exception)
							{
								// Uncomment this fail to see what throws exceptions despite the trying of hardness
								// Fail($"Exception {ex.InnerException?.Message ?? ex.Message} occured setting property value {((ICodeDescription)field).Code} for type {bizo.GetType().FullName}, workflow {workflowDescriptor.Code}");
								CleanUpFactory();
								continue;
							}
						}

						if (((IStmALogProvider)bizo).Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.CustomisableEvent00.Code)).Where(log => !log.IsInDatabase).ToList().Count != 1)
						{
							Fail($"No event for {((ICodeDescription)field).Code} for type {bizo.GetType().FullName}, workflow {workflowDescriptor.Code}");
							CleanUpFactory();
							continue;
						}

						try
						{
							bizo.SetPropertyValue(((ICodeDescription)field).Code, val);
							if (bizo.GetPropertyValue(((ICodeDescription)field).Code).Equals(val))
							{
								if (((IStmALogProvider)bizo).Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.CustomisableEvent00.Code)).Where(log => !log.IsInDatabase).ToList().Count != 0)
								{
									AssertNotContains($"Event not cleared for {((ICodeDescription)field).Code} for type {bizo.GetType().FullName}, workflow {workflowDescriptor.Code}", $"{((ICodeDescription)field).Code} ", ((IStmALogProvider)bizo).Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.CustomisableEvent00.Code)).Where(log => !log.IsInDatabase).ToList()[0].SL_Reference);
									CleanUpFactory();
									continue;
								}
							}
							else
							{
								CleanUpFactory();
								continue;
							}
						}
						catch (Exception ex)
						{
							Fail($"Exception {ex.InnerException?.Message ?? ex.Message} occured setting property value back to its original value {((ICodeDescription)field).Code} for type {bizo.GetType().FullName}, workflow {workflowDescriptor.Code}");
							CleanUpFactory();
							continue;
						}
					}

					void CleanUpFactory()
					{
						factory = new BusinessObjectFactory();
						bizo = factory.New(workflowDescriptor.WorkflowProviderType);
						bizo.SuspendValidation();
					}

					void TryHarder()
					{
						factory = new BusinessObjectFactory();
						bizo = factory.NewWithValidTestData(workflowDescriptor.WorkflowProviderType);
						bizo.SuspendValidation();
						factory.Save();
					}
				}

				// Our attempts at setting things will cause errors
				ErrorReporter.Clear();
			});

			HashSet<string> ExpectedPropertiesWeCannaSet()
			{
				// If adding to this list, please add a unit test for setting this property to validate that the
				// FieldChangeRule will work with it (or add it to the blacklist for your module).
				return new HashSet<string>()
					{
						JobContainerSchema.Constants.JC_FCLOnBoardVessel,
						JobContainerSchema.Constants.JC_FCLUnloadFromVessel,
						JobContainerSchema.Constants.JC_FCLWharfGateIn,
						JobContainerSchema.Constants.JC_FCLWharfGateOut,
						JobComInvoiceLineSchema.Constants.JI_Tariff,
						JobConsolSchema.Constants.JK_ConsolChargeable,
						JobConsolSchema.Constants.JK_CorrectedConsolVolume,
						JobConsolSchema.Constants.JK_CorrectedConsolVolumeUnit,
						JobConsolSchema.Constants.JK_CorrectedConsolWeight,
						JobConsolSchema.Constants.JK_CorrectedConsolWeightUnit,
						JobConsolSchema.Constants.JK_OA_ReceivingForwarderAddress,
						JobConsolSchema.Constants.JK_OA_SendingForwarderAddress,
						JobConsolSchema.Constants.JK_ConsolChargeable,
						JobConsolSchema.Constants.JK_CorrectedConsolVolume,
						JobConsolSchema.Constants.JK_CorrectedConsolVolumeUnit,
						JobConsolSchema.Constants.JK_CorrectedConsolWeight,
						JobConsolSchema.Constants.JK_CorrectedConsolWeightUnit,
						JobShipmentSchema.Constants.JS_DeliveryDueDate,
						JobShipmentSchema.Constants.JS_RevisedDeliveryDueDate,
						HVLVConsignmentSchema.Constants.HVC_ActualVolume,
						HVLVConsignmentSchema.Constants.HVC_ActualWeight,
						HVLVConsignmentSchema.Constants.HVC_ItemCount,
						HVLVConsignmentSchema.Constants.HVC_ManifestedVolume,
						HVLVConsignmentSchema.Constants.HVC_ManifestedWeight,
						RatingHeaderSchema.Constants.TH_OH,
						RatingHeaderSchema.Constants.TH_QuoteDate,
						DtbBookingSchema.Constants.KM_Chargeable,
					};
			}
		}

		static readonly Lazy<ImmutableHashSet<string>> noRealWorkflowProviderCodes = new(() => new HashSet<string>(
		[
			"TSK",
			"EXP",
			WorkflowDescriptors.PkgPackageWorkflowDecriptorCode,
			WorkflowDescriptors.CusEntryHeaderWorkflowDescriptorCode,
			WorkflowDescriptors.CusExitReportWorkflowDescriptorCode,
			WorkflowDescriptors.ServiceWorkflowDescriptorCode,
			WorkflowDescriptors.CusNOEmmaMessageGeneratorWorkflowDescriptorCode,
			WorkflowDescriptors.CYDDeliveryWorkflowDescriptorCode,
			WorkflowDescriptors.CYDPickupWorkflowDescriptorCode,
		]).ToImmutableHashSet());

		public class ProcessTaskParentLocator : IBusinessObjectParentLocator
		{
			public bool TryLocateParentBusinessObjects(BusinessObject child, out IEnumerable<BusinessObject> parents)
			{
				var workflowItem = child as IWorkflowItem;
				parents = workflowItem?.GetJob().Yield();
				return parents.Any();
			}
		}

		public class ProcessTaskFieldChangeState : IBusinessObjectFieldChangeState
		{
			public bool IsObjectInitialized(BusinessObject bizo)
			{
				var workflowItem = bizo as IWorkflowItem;
				return workflowItem?.ParentID != ZGuid.Empty;
			}

			public bool IsRootObject(BusinessObject bizo)
			{
				return false;
			}

			public void NotifyObjectHooked(BusinessObject child)
			{
				if (IsObjectInitialized(child))
				{
					return;
				}

				ZPropertyValueChangedEventHandler propertyValueChangedEventHandler = null;
				propertyValueChangedEventHandler = (_, args) =>
				{
					if (IsObjectInitialized(child))
					{
						child.PropertyValueChanged -= propertyValueChangedEventHandler;
						child.FieldChangeTrackerStateChanged(BusinessObjectFieldStateChangeEvent.ObjectIntialized);
						return;
					}
				};

				child.PropertyValueChanged += propertyValueChangedEventHandler;
			}
		}

		class ThrowingBusinessObjectParentLocator : IBusinessObjectParentLocator
		{
			readonly bool throwOnTryLocate;

			public ThrowingBusinessObjectParentLocator(bool throwOnTryLocate, string tablePrefix)
			{
				this.throwOnTryLocate = throwOnTryLocate;
				ParentTableCodePrefix = tablePrefix;
			}

			public string ParentTableCodePrefix { get; }

			public bool TryLocateParentBusinessObjects(BusinessObject child, out IEnumerable<BusinessObject> parents)
			{
				parents = Enumerable.Empty<BusinessObject>();
				if (throwOnTryLocate)
				{
					throw new Exception();
				}
				else
				{
					return false;
				}
			}
		}

		sealed class ThrowingShipmentFieldStateChange : IBusinessObjectFieldChangeState
		{
			readonly ShipmentFieldStateChange shipmentFieldStateChange = new ShipmentFieldStateChange();
			readonly bool isObjectInitializedThrows;
			readonly bool isRootObjectThrows;
			readonly bool registerBusinessObjectFieldStateChangedThrows;

			public ThrowingShipmentFieldStateChange(bool isObjectInitializedThrows, bool isRootObjectThrows, bool registerBusinessObjectFieldStateChangedThrows)
			{
				this.isObjectInitializedThrows = isObjectInitializedThrows;
				this.isRootObjectThrows = isRootObjectThrows;
				this.registerBusinessObjectFieldStateChangedThrows = registerBusinessObjectFieldStateChangedThrows;
			}

			public bool IsObjectInitialized(BusinessObject bizo)
			{
				if (isObjectInitializedThrows)
				{
					throw new Exception();
				}

				return shipmentFieldStateChange.IsObjectInitialized(bizo);
			}

			public bool IsRootObject(BusinessObject bizo)
			{
				if (isRootObjectThrows)
				{
					throw new Exception();
				}

				return shipmentFieldStateChange.IsRootObject(bizo);
			}

			public void NotifyObjectHooked(BusinessObject bizo)
			{
				if (registerBusinessObjectFieldStateChangedThrows)
				{
					throw new Exception();
				}

				shipmentFieldStateChange.NotifyObjectHooked(bizo);
			}
		}

		sealed class DummyDelayedInitialisationStateChange : IBusinessObjectFieldChangeState
		{
			bool isInitialised;
			readonly List<BusinessObject> hookedObjects = new List<BusinessObject>();

			public IDisposable Initialize()
			{
				return new DisposableAction(() =>
				{
					isInitialised = true;
					foreach (var hookedObject in hookedObjects)
					{
						hookedObject.FieldChangeTrackerStateChanged(BusinessObjectFieldStateChangeEvent.ObjectIntialized);
					}

					hookedObjects.Clear();
				});
			}

			public bool IsObjectInitialized(BusinessObject bizo)
			{
				return isInitialised;
			}

			public bool IsRootObject(BusinessObject bizo)
			{
				return true;
			}

			public void NotifyObjectHooked(BusinessObject bizo)
			{
				hookedObjects.Add(bizo);
			}
		}

		sealed class DummyToDummyParentLocator : IBusinessObjectParentLocator
		{
			public List<BusinessObject> Parents => new List<BusinessObject>();

			public bool TryLocateParentBusinessObjects(BusinessObject child, out IEnumerable<BusinessObject> parents)
			{
				parents = Parents;
				return Parents.Count > 0;
			}
		}

		// For unit test: TestBusinessObjectDoesNotHandleReentrancy
		//sealed class DummyThatDoesNotLikeReEntrancy : DummyWithWorkflow
		//{
		//	bool noRentrancy;

		//	public DummyThatDoesNotLikeReEntrancy(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
		//	{
		//	}

		//	public override ZString Z0_NVarChar
		//	{
		//		get
		//		{
		//			if (noRentrancy)
		//			{
		//				throw new Exception();
		//			}

		//			return base.Z0_NVarChar;
		//		}
		//		set
		//		{
		//			if (noRentrancy)
		//			{
		//				throw new Exception();
		//			}

		//			using (new DisposableAction(() => noRentrancy = true, () => noRentrancy = false))
		//			{
		//				base.Z0_NVarChar = value;
		//				Z0_Description = value;
		//			}
		//		}
		//	}
		//}
	}
}

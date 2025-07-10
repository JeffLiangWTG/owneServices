using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	sealed class UserDefinedPropertyCollectionTest : TestCaseWithFactory
	{
		IDisposable nudgingControllerSubstitution;

		protected override void SetUp()
		{
			base.SetUp();
			nudgingControllerSubstitution = ObjectFactory.Substitute<INudgingController>(Mock.Of<INudgingController>());
		}

		protected override void TearDown()
		{
			nudgingControllerSubstitution?.Dispose();
			base.TearDown();
		}

		public void TestPartIdentifiersAreEqual()
		{
			//Since the latter solution can't reference the former solution, keep them in sync with a unit test.
			AssertEquals(AddOnColumnDataType.PartIdentifier, BODocDataProvider.PartIdentifier);
		}

		public void TestGetSet()
		{
			var bizObj = Factory.New<DummyBusinessObjectWithUserDefinedFields>();
			bizObj.Z0_VarCharMax = "olo";
			bizObj.SetUserDefinedValue("UserText2", (ZString)"Test!");

			var cusObj = GetCustomBusinessObject(bizObj);

			AssertEquals("olo", cusObj[CustomPropertyHelper.GeneratePropertyIdentifier("Z0_VARCHARMAX", typeof(ZString))]);
			AssertEquals(ZString.Empty, cusObj[CustomPropertyHelper.GeneratePropertyIdentifier("USERTEXT1", typeof(ZString))]);
			AssertEquals(ZDecimal.Zero, cusObj[CustomPropertyHelper.GeneratePropertyIdentifier("USERDECIMAL1", typeof(ZDecimal))]);
			AssertEquals("Test!", cusObj[CustomPropertyHelper.GeneratePropertyIdentifier("USERTEXT2", typeof(ZString))]);

			cusObj[CustomPropertyHelper.GeneratePropertyIdentifier("Z0_VARCHARMAX", typeof(ZString))] = "Ttffgg";
			cusObj[CustomPropertyHelper.GeneratePropertyIdentifier("USERTEXT1", typeof(ZString))] = "Test1";
			cusObj[CustomPropertyHelper.GeneratePropertyIdentifier("USERDECIMAL1", typeof(ZDecimal))] = 89;
			cusObj[CustomPropertyHelper.GeneratePropertyIdentifier("USERTEXT2", typeof(ZString))] = "Test2";

			AssertEquals("Ttffgg", bizObj.Z0_VarCharMax);
			AssertEquals("Test1", bizObj.GetUserDefinedValue<ZString>("UserText1"));
			AssertEquals(89m, bizObj.GetUserDefinedValue<ZDecimal>("UserDecimal1"));
			AssertEquals("Test2", bizObj.GetUserDefinedValue<ZString>("UserText2"));
		}

		public void TestMetaData()
		{
			var bizObj = Factory.New<DummyBusinessObjectWithUserDefinedFields>();
			bizObj.SetUserDefinedValue("UserText2", (ZString)"Test!");

			var cusObj = GetCustomBusinessObject(bizObj);

			AssertEquals(100, MetaData.GetMaxLength(cusObj, cusObj.FindPropertyInfo(CustomPropertyHelper.GeneratePropertyIdentifier("Z0_VARCHARMAX", typeof(ZString))).PropertyDescriptor));
			AssertEquals("Real Field", MetaData.GetDescription(cusObj, cusObj.FindPropertyInfo(CustomPropertyHelper.GeneratePropertyIdentifier("Z0_VARCHARMAX", typeof(ZString))).PropertyDescriptor).GetDescription(0));
		}

		public void TestValidation()
		{
			var cusObj = GetCustomBusinessObject(Factory.New<DummyBusinessObjectWithUserDefinedFields>());

			cusObj[CustomPropertyHelper.GeneratePropertyIdentifier("USERTEXT1", typeof(ZString))] = "\u0445\u0443\u0439";
			cusObj[CustomPropertyHelper.GeneratePropertyIdentifier("Z0_VARCHARMAX", typeof(ZString))] = "\u0445\u0443\u0439";
			cusObj[CustomPropertyHelper.GeneratePropertyIdentifier("Z0_NVARCHAR", typeof(ZString))] = "\u0445\u0443\u0439";

			AssertNoErrors(cusObj.FindPropertyInfo(CustomPropertyHelper.GeneratePropertyIdentifier("USERTEXT1", typeof(ZString))));
			AssertHasError(cusObj.FindPropertyInfo(CustomPropertyHelper.GeneratePropertyIdentifier("Z0_VARCHARMAX", typeof(ZString))), "Real Field only accepts Western European languages characters.");
			AssertNoErrors(cusObj.FindPropertyInfo(CustomPropertyHelper.GeneratePropertyIdentifier("Z0_NVARCHAR", typeof(ZString))));

			var notifications = new ZNotificationCollector(cusObj, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetFatalNotifications();
			AssertEquals(1, notifications.Count());
			AssertEquals("Real Field: Real Field only accepts Western European languages characters.", notifications.ElementAt(0).Message);
		}

		public void TestWarningOnInActiveAddOnRule()
		{
			var rules = Factory.Load<GenCustomAddOnRule>(new ZQuery());
			AssertEquals(0, rules.Length);

			var cusObj = GetCustomBusinessObject(Factory.New<DummyBusinessObjectWithUserDefinedFields>());
			var customProperty = CustomPropertyHelper.GeneratePropertyIdentifier("USERDATETIME1", typeof(ZDateTime));
			var propInfo = cusObj.FindPropertyInfo(customProperty);

			cusObj[customProperty] = ZDateTime.Now;
			AssertNoWarnings(propInfo);

			var rule = Factory.LoadTop1<GenCustomAddOnRule>(new ZQuery());
			AssertNotNull(rule);
			rule.XR_IsActive = false;

			cusObj[customProperty] = ZDateTime.Now.AddMinutes(10);
			AssertHasWarning(propInfo, "The Add On Rule is inactive - The Workflow Template configuration should be updated.");
			AssertNoErrors(propInfo);
		}

		public void TestAddOnRule()
		{
			var cusObj = GetCustomBusinessObject(Factory.New<DummyBusinessObjectWithUserDefinedFields>());
			var propInfo = cusObj.FindPropertyInfo(CustomPropertyHelper.GeneratePropertyIdentifier("USERDATETIME1", typeof(ZDateTime)));

			AssertEquals(KDateTimeFormat.Time, MetaData.GetMetaData(cusObj, propInfo.PropertyDescriptor, MetaDataTypes.DateTimeFormat));

			cusObj.Validation.Validate(CustomPropertyHelper.GeneratePropertyIdentifier("USERDATETIME1", typeof(ZDateTime)));
			AssertHasError(propInfo, "Please enter an UserDateTime1.");

			cusObj[CustomPropertyHelper.GeneratePropertyIdentifier("USERDATETIME1", typeof(ZDateTime))] = ZDateTime.Now;
			AssertNoErrors(propInfo);
		}

		public void TestGetCustomFieldOnlyUsesEnglishName()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000100";
			var shipmentAsWorkflow = ((IWorkflowProvider)shipment);

			var processTaskTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			var customFieldDefinition = MasterFilesTestHelper.CreateCustomField(processTaskTemplate, "CFENG");

			var trigger = shipmentAsWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.P9_Description = "Bella trigger me";

			Factory.Save();

			((BusinessObject)shipment).SetPossiblyCustomProperty("__CFENG__prop__ZString", new ZString("Hello"));

			AssertEquals("Hello", new GetCustomFieldStrategy((BusinessObject)shipment).GetCustomField("CFENG"));

			Factory.Save();

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<GetCustomField(CFENG)>";
			action.PQ_FieldValue = "Ciao";

			Factory.Save();

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Italian))
			{
				using (var mockRes = Res.UseMockData())
				{
					string resKey = ((ResourceString)customFieldDefinition.XC_NameMultilingual).ResourceKey;
					mockRes.Put(resKey, new ResourceStringData(resKey, "CFIT"));

					shipmentAsWorkflow.Logs.AddNew(Events.CustomisableEvent00);

					Factory.Save();

					var logs = MasterFilesTestHelper.RunLogWalker();
				}
			}

			var shipmentReloaded = new BusinessObjectFactory().Load<Forwarding.IForwardingShipment>(shipment.PK);
			AssertEquals("Ciao", new GetCustomFieldStrategy((BusinessObject)shipmentReloaded).GetCustomField("CFENG"));
		}

		public void TestAddOnRule2()
		{
			var rules = new GenCustomAddOnRule[3];
			for (int i = 0; i < rules.Length; i++)
			{
				rules[i] = Factory.New<GenCustomAddOnRule>();
				rules[i].SetRules(new DateTimeFormatRule { Format = (KDateTimeFormat)i + 1, IsEnabled = true });
			}

			var template = Factory.New<ProcessTaskTemplate>();
			for (int i = 0; i < 6; i++)
			{
				var column = template.GenCustomColumnDefinitions.AddNew();
				column.XC_Name = "User" + (i + 1);
				column.XC_Type = AddOnColumnDataType.Codes.Datetime;
				if (i < 3)
				{
					column.XC_XR = rules[i].PK;
				}
			}

			var dummy = Factory.New<DummyBusinessObjectWithUserDefinedFields>();
			UserDefinedPropertyManager propertyManager = new UserDefinedPropertyManager(dummy);
			propertyManager.SetValue("User2", AddOnColumnDataType.Codes.Datetime, new ZDateTime(2009, 9, 2));
			propertyManager.SetValue("User3", AddOnColumnDataType.Codes.Datetime, new ZDateTime(2009, 9, 3));
			propertyManager.SetValue("User5", AddOnColumnDataType.Codes.Datetime, new ZDateTime(2009, 9, 5));
			propertyManager.SetValue("User6", AddOnColumnDataType.Codes.Datetime, new ZDateTime(2009, 9, 6));
			propertyManager.SetValue("User7", AddOnColumnDataType.Codes.Datetime, new ZDateTime(2009, 9, 7));
			propertyManager.SetValue("User8", AddOnColumnDataType.Codes.Datetime, new ZDateTime(2009, 9, 8));
			propertyManager.GetProperty("User3", AddOnColumnDataType.Codes.Datetime).XV_XR_Rule = rules[0].PK;
			propertyManager.GetProperty("User6", AddOnColumnDataType.Codes.Datetime).XV_XR_Rule = rules[1].PK;
			propertyManager.GetProperty("User8", AddOnColumnDataType.Codes.Datetime).XV_XR_Rule = rules[2].PK;

			var cusObj = new CustomBusinessObject(null, new UserDefinedPropertyCollection(dummy) { new ProcessTaskTemplateMatches(new[] { template }) });
			ZPropertyInfo[] infos = new ZPropertyInfo[8];
			for (int i = 0; i < infos.Length; i++)
			{
				infos[i] = cusObj.FindPropertyInfo(CustomPropertyHelper.GeneratePropertyIdentifier("USER" + (i + 1) + "", typeof(ZDateTime)));
			}

			AssertEquals(KDateTimeFormat.Long, MetaData.GetMetaData(cusObj, infos[0], MetaDataTypes.DateTimeFormat));
			AssertEquals(KDateTimeFormat.Short, MetaData.GetMetaData(cusObj, infos[1], MetaDataTypes.DateTimeFormat));
			AssertEquals(KDateTimeFormat.Time, MetaData.GetMetaData(cusObj, infos[2], MetaDataTypes.DateTimeFormat));
			AssertEquals(KDateTimeFormat.Short, MetaData.GetMetaData(cusObj, infos[3], MetaDataTypes.DateTimeFormat));
			AssertEquals(KDateTimeFormat.Short, MetaData.GetMetaData(cusObj, infos[4], MetaDataTypes.DateTimeFormat));
			AssertEquals(KDateTimeFormat.Short, MetaData.GetMetaData(cusObj, infos[5], MetaDataTypes.DateTimeFormat));
			AssertEquals(KDateTimeFormat.Short, MetaData.GetMetaData(cusObj, infos[6], MetaDataTypes.DateTimeFormat));
			AssertEquals(KDateTimeFormat.Time, MetaData.GetMetaData(cusObj, infos[7], MetaDataTypes.DateTimeFormat));

			for (int i = 0; i < infos.Length; i++)
			{
				cusObj[CustomPropertyHelper.GeneratePropertyIdentifier("USER" + (i + 1) + "", typeof(ZDateTime))] = new ZDateTime(2009, 10, i + 1);
			}

			AssertEquals(rules[0].PK, propertyManager.GetProperty("User1", AddOnColumnDataType.Codes.Datetime).XV_XR_Rule);
			AssertEquals(rules[1].PK, propertyManager.GetProperty("User2", AddOnColumnDataType.Codes.Datetime).XV_XR_Rule);
			AssertEquals(rules[2].PK, propertyManager.GetProperty("User3", AddOnColumnDataType.Codes.Datetime).XV_XR_Rule);
			AssertEquals(ZGuid.Empty, propertyManager.GetProperty("User4", AddOnColumnDataType.Codes.Datetime).XV_XR_Rule);
			AssertEquals(ZGuid.Empty, propertyManager.GetProperty("User5", AddOnColumnDataType.Codes.Datetime).XV_XR_Rule);
			AssertEquals(ZGuid.Empty, propertyManager.GetProperty("User6", AddOnColumnDataType.Codes.Datetime).XV_XR_Rule);
			AssertEquals(ZGuid.Empty, propertyManager.GetProperty("User7", AddOnColumnDataType.Codes.Datetime).XV_XR_Rule);
			AssertEquals(rules[2].PK, propertyManager.GetProperty("User8", AddOnColumnDataType.Codes.Datetime).XV_XR_Rule);
		}

		Tuple<DummyCustomFieldBizo, CustomBusinessObject> CreateCustomBizo(string type = null, bool isEstimate = false)
		{
			var rule = Factory.New<GenCustomAddOnRule>();
			rule.SetRules(new CreateEventRule { EventCode = Events.CustomisableEvent00Code, EventReference = "BOBBO", IsEstimate = isEstimate, IsEnabled = true });

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var column = template.GenCustomColumnDefinitions.AddNew();
			column.XC_Name = "User1";
			column.XC_Type = type ?? AddOnColumnDataType.Codes.Datetime;
			column.XC_XR = rule.PK;
			var dummy = Factory.New<DummyCustomFieldBizo>();

			return Tuple.Create(dummy, new CustomBusinessObject(null, new UserDefinedPropertyCollection(dummy) { new ProcessTaskTemplateMatches(template) }));
		}

		#region TestCreateEventRule

		public void TestCreateEventRule_RaiseEvent()
		{
			var dummy = CreateCustomBizo();
			dummy.Item2[CustomPropertyHelper.GeneratePropertyIdentifier("USER1", typeof(ZDateTime))] = ZDateTime.Now;
			Factory.Save();
			var log = dummy.Item1.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code && l.SL_Reference.EndsWith("BOBBO")).Single();
			AssertEquals(false, log.SL_IsCancelled);
		}

		public void TestCreateEventRule_EventReference_OldValueIsOriginalValue()
		{
			var rule = Factory.New<GenCustomAddOnRule>();
			rule.SetRules(new CreateEventRule { EventCode = Events.CustomisableEvent00Code, IsEnabled = true });

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var column = template.GenCustomColumnDefinitions.AddNew();
			column.XC_Name = "Test field";
			column.XC_Type = AddOnColumnDataType.Codes.String;
			column.XC_XR = rule.PK;
			var dummyCustomFieldBizo = Factory.New<DummyCustomFieldBizo>();
			
			var dummyBizObTuple = Tuple.Create(dummyCustomFieldBizo, new CustomBusinessObject(Factory, null, new UserDefinedPropertyCollection(dummyCustomFieldBizo) { new ProcessTaskTemplateMatches(template) }));
			var propIdentifier = CustomPropertyHelper.GeneratePropertyIdentifier("Test field", typeof(ZString));

			dummyBizObTuple.Item2[propIdentifier] = "Change1";
			dummyBizObTuple.Item2[propIdentifier] = "Change2";
			dummyBizObTuple.Item2[propIdentifier] = "Change3";
			
			Factory.Save();

			dummyBizObTuple.Item2[propIdentifier] = "Change4";
			dummyBizObTuple.Item2[propIdentifier] = "Change5";

			Factory.Save();

			var logs = dummyBizObTuple.Item1.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code).OrderBy(i => i.SL_PostedTimeUtc);

			AssertEquals(2, logs.Count());
			AssertEquals("|NAM=Test field|NEW=Change3|OLD=", logs.First().SL_Reference);
			AssertEquals("|NAM=Test field|NEW=Change5|OLD=Change3", logs.Last().SL_Reference);
		}

		public void TestCreateEventRule_EventReference_LogIsDeletedWhenValueIsResetToOriginalValue()
		{
			var rule = Factory.New<GenCustomAddOnRule>();
			rule.SetRules(new CreateEventRule { EventCode = Events.CustomisableEvent00Code, IsEnabled = true });

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var column = template.GenCustomColumnDefinitions.AddNew();
			column.XC_Name = "Test field";
			column.XC_Type = AddOnColumnDataType.Codes.String;
			column.XC_XR = rule.PK;
			var dummyCustomFieldBizo = Factory.New<DummyCustomFieldBizo>();

			var dummyBizObTuple = Tuple.Create(dummyCustomFieldBizo, new CustomBusinessObject(Factory, null, new UserDefinedPropertyCollection(dummyCustomFieldBizo) { new ProcessTaskTemplateMatches(template) }));
			var propIdentifier = CustomPropertyHelper.GeneratePropertyIdentifier("Test field", typeof(ZString));

			dummyBizObTuple.Item2[propIdentifier] = "Change1";

			Factory.Save();

			// Do some changes without saving
			dummyBizObTuple.Item2[propIdentifier] = "Change2";
			dummyBizObTuple.Item2[propIdentifier] = "Change3";
			dummyBizObTuple.Item2[propIdentifier] = "Change4";

			// Change back to the original value
			dummyBizObTuple.Item2[propIdentifier] = "Change1";

			Factory.Save();

			var logs = dummyBizObTuple.Item1.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code).OrderBy(i => i.SL_PostedTimeUtc);

			AssertEquals(1, logs.Count());
			AssertEquals("|NAM=Test field|NEW=Change1|OLD=", logs.First().SL_Reference);
		}

		public void TestCreateEventRule_EventReference_ComboBox()
		{
			var addOnRulesXml = @"
<sourceCode>
  <rules>
    <rule code='InvalidCode' enabled='true'>
      <details>
        <codeDescriptionList>
          <codeDescription code='CBO1' description='Code 1' />
          <codeDescription code='CBO2' description='Code 2' />
        </codeDescriptionList>
      </details>
    </rule>
    <rule code='CreateEvent' enabled='true'>
      <details>
        <CreateEventRuleCode>Z00</CreateEventRuleCode>
      </details>
    </rule>
  </rules>
</sourceCode>";
			var dummyCustomFactory = ObjectFactory.Get<IDummyWithCustomFieldsFactory>();
			var dummy = dummyCustomFactory.GetDummyWithCustomFields(Factory, addOnRulesXml, out var addOnRules, out var addCustomField, out var setCustomField, out var getCustomField, out var getCustomFieldIdentifier);
			
			addCustomField(("Test Combo", "CBO"));

			var part1 = ("Test ComboPART1", "STR");
			var part2 = ("Test ComboPART2", "STR");

			setCustomField(part1, new ZString("Test1"));
			setCustomField(part2, new ZString("Test 1"));
			Factory.Save();

			// Change Description
			setCustomField(part2, new ZString("Test 2"));
			Factory.Save();

			// Change Code
			setCustomField(part1, new ZString("Test3"));
			Factory.Save();

			// Change both
			setCustomField(part1, new ZString("Test4"));
			setCustomField(part2, new ZString("Test 4"));
			Factory.Save();

			var logs = dummy.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code).OrderBy(i => i.SL_PostedTimeUtc).ToList();
			AssertEquals(4, logs.Count);

			AssertEquals("|NAM=Test Combo|NEW=Test1:Test 1|OLD=", logs[0].SL_Reference);
			AssertEquals("|NAM=Test Combo|NEW=Test1:Test 2|OLD=Test1:Test 1", logs[1].SL_Reference);
			AssertEquals("|NAM=Test Combo|NEW=Test3:Test 2|OLD=Test1:Test 2", logs[2].SL_Reference);
			AssertEquals("|NAM=Test Combo|NEW=Test4:Test 4|OLD=Test3:Test 2", logs[3].SL_Reference);
		}

		public void TestCreateEventRule_EventReference_WrappedUserDefined()
		{
			var bizObj = Factory.New<DummyBusinessObjectWithUserDefinedFields>();
			bizObj.Z0_VarCharMax = "old value";

			var rule = Factory.New<GenCustomAddOnRule>();
			rule.SetRules(new CreateEventRule { EventCode = Events.CustomisableEvent00Code, IsEnabled = true });

			var columnDefinition = new CustomColumnDefinition(ZGuid.NewZGuid(),
				DummyBusinessObject.Schema.Z0_VarCharMax, "Real Field", AddOnColumnDataType.Codes.String, int.MaxValue,
				null, rule.PK, Factory);

			var customProperty = columnDefinition.CreateWrapperCustomProperty(bizObj);
			var properties = new UserDefinedPropertyCollection(bizObj) { customProperty };
			var cusObj = new CustomBusinessObject(bizObj, properties);

			var propIdentifier = CustomPropertyHelper.GeneratePropertyIdentifier("Z0_VARCHARMAX", typeof(ZString));

			cusObj[propIdentifier] = "new value 1";
			cusObj[propIdentifier] = "new value 2";
			cusObj[propIdentifier] = "new value 3";

			Factory.Save();

			cusObj[propIdentifier] = "new value 4";
			cusObj[propIdentifier] = "new value 5";

			Factory.Save();

			var logs = bizObj.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code).OrderBy(i => i.SL_PostedTimeUtc).ToList();

			AssertEquals(2, logs.Count);
			AssertEquals("|NAM=Z0_VarCharMax|NEW=new value 3|OLD=old value", logs[0].SL_Reference);
			AssertEquals("|NAM=Z0_VarCharMax|NEW=new value 5|OLD=new value 3", logs[1].SL_Reference);
		}

		public void TestCreateEventRule_EventWithParametersFiresTrigger()
		{
			var addOnRulesXml = @"
<sourceCode>
  <rules>
    <rule code='InvalidCode' enabled='true'>
      <details>
        <codeDescriptionList>
          <codeDescription code='Blah' description='Rah' />
          <codeDescription code='Nah' description='Far' />
        </codeDescriptionList>
      </details>
    </rule>
    <rule code='CreateEvent' enabled='true'>
      <details>
        <CreateEventRuleCode>Z01</CreateEventRuleCode>
        <CreateEventRuleReference>|MEE=Hello</CreateEventRuleReference>
      </details>
    </rule>
    <rule code='DateTimeFormat' enabled='false'>
      <details>
        <format>Short</format>
      </details>
    </rule>
    <rule code='CheckEntered' enabled='false'>
      <details />
    </rule>
  </rules>
</sourceCode>";

			var dummyCustomFactory = ObjectFactory.Get<IDummyWithCustomFieldsFactory>();
			var dummy = (IWorkflowProvider)dummyCustomFactory.GetDummyWithCustomFields(Factory, addOnRulesXml, out var addOnRules, out var addCustomField, out var setCustomField, out var getCustomField, out var getCustomFieldIdentifier);

			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Some Description";
			trigger.TriggerConditions.TriggerEventCode = "Z01";
			trigger.TriggerConditions.TriggerCondition = "RFP";
			trigger.TriggerConditions.TriggerConditionValue = "MEE=Hello";
			addCustomField(("Some Field", "STR"));
			Factory.Save();

			setCustomField(("Some Field", "STR"), new ZString("Blah"));
			AssertEquals((short)99, trigger.TriggerConditions.TriggerFiredCountdown);
		}

		public void TestDisabledRulesAreReallyDisabled()
		{
			var addOnRulesXml = @"<sourceCode>
  <rules>
    <rule code='InvalidCode' enabled='false'>
      <details>
        <codeDescriptionList>
          <codeDescription code='TEST' description='TEST' />
          <codeDescription code='ABC' description='ABC' />
          <codeDescription code='NEW' description='NEW' />
        </codeDescriptionList>
      </details>
    </rule>
    <rule code='CreateEvent' enabled='false'>
      <details>
        <CreateEventRuleCode>Z01</CreateEventRuleCode>
        <CreateEventRuleReference>Z01</CreateEventRuleReference>
      </details>
    </rule>
    <rule code='DateTimeFormat' enabled='false'>
      <details>
        <format>Short</format>
      </details>
    </rule>
    <rule code='CheckEntered' enabled='false'>
      <details />
    </rule>
  </rules>
</sourceCode>";

			var dummyCustomFactory = ObjectFactory.Get<IDummyWithCustomFieldsFactory>();
			var dummy = dummyCustomFactory.GetDummyWithCustomFields(Factory, addOnRulesXml, out var addOnRules, out var addCustomField, out var setCustomField, out var getCustomField, out var getCustomFieldIdentifier);
			addCustomField(("Some Field", "STR"));
			AssertNoExceptionThrown(() => setCustomField(("Some Field", "STR"), new ZString("A Value")));
		}

		public void TestCreateEventRule_CancelEventOnClear()
		{
			var dummy = CreateCustomBizo();
			dummy.Item2[CustomPropertyHelper.GeneratePropertyIdentifier("USER1", typeof(ZDateTime))] = ZDateTime.Now;
			dummy.Item2[CustomPropertyHelper.GeneratePropertyIdentifier("USER1", typeof(ZDateTime))] = ZDateTime.Empty;
			AssertNull(dummy.Item1.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code && l.SL_Reference.EndsWith("BOBBO")).SingleOrDefault());
		}

		public void TestCreateEventRule_EventNotCancelledOnClear_IfPreviouslySaved()
		{
			var dummy = CreateCustomBizo();
			dummy.Item2[CustomPropertyHelper.GeneratePropertyIdentifier("USER1", typeof(ZDateTime))] = ZDateTime.Now;
			Factory.Save();
			dummy.Item2[CustomPropertyHelper.GeneratePropertyIdentifier("USER1", typeof(ZDateTime))] = ZDateTime.Empty;
			var log = dummy.Item1.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code && l.SL_Reference.EndsWith("BOBBO")).SingleOrDefault();
			AssertEquals(false, log.SL_IsCancelled);
		}

		public void TestCreateEventRule_UpdateEvent()
		{
			var dummy = CreateCustomBizo();
			dummy.Item2[CustomPropertyHelper.GeneratePropertyIdentifier("USER1", typeof(ZDateTime))] = ZDateTime.Now;
			dummy.Item2[CustomPropertyHelper.GeneratePropertyIdentifier("USER1", typeof(ZDateTime))] = ZDateTime.Now;
			var log = dummy.Item1.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code && l.SL_Reference.EndsWith("BOBBO")).Single();
			AssertEquals(false, log.SL_IsCancelled);
		}

		public void TestCreateEventRule_UpdateEvent_String()
		{
			var dummy = CreateCustomBizo(AddOnColumnDataType.Codes.String);
			dummy.Item2[CustomPropertyHelper.GeneratePropertyIdentifier("USER1", typeof(ZString))] = new ZString("Garryyyyyy");
			AssertNotNull(dummy.Item1.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code && l.SL_Reference.EndsWith("BOBBO")).Single());
			dummy.Item2[CustomPropertyHelper.GeneratePropertyIdentifier("USER1", typeof(ZString))] = new ZString();
			AssertNull(dummy.Item1.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code && l.SL_Reference.EndsWith("BOBBO")).SingleOrDefault());
		}

		public void TestCreateEventRule_UpdateEvent_ZInt()
		{
			var dummy = CreateCustomBizo(AddOnColumnDataType.Codes.Integer);
			dummy.Item2[CustomPropertyHelper.GeneratePropertyIdentifier("USER1", typeof(ZInt))] = new ZInt(12);
			AssertNotNull(dummy.Item1.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code && l.SL_Reference.EndsWith("BOBBO")).Single());
		}

		public void TestCreateEventRule_UpdateEvent_UseFixedEventDate()
		{
			var dummy = CreateCustomBizo();
			var date = new ZDateTime(2017, 03, 02, 01, 01, 04);
			dummy.Item2[CustomPropertyHelper.GeneratePropertyIdentifier("USER1", typeof(ZDateTime))] = date;
			var log = dummy.Item1.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code && l.SL_Reference.EndsWith("BOBBO")).Single();
			AssertEquals(date, log.SL_EventTime);
		}

		public void TestCreateEventRule_UpdateEvent_IsEstimate()
		{
			var dummy = CreateCustomBizo(isEstimate: true);
			dummy.Item2[CustomPropertyHelper.GeneratePropertyIdentifier("USER1", typeof(ZDateTime))] = ZDateTime.Now;
			var log = dummy.Item1.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code && l.SL_Reference.EndsWith("BOBBO")).Single();
			AssertEquals(true, log.SL_IsEstimate);
		}

		public void TestCreateEventRule_UpdateEvent_CancelExistingEvent()
		{
			var rule = Factory.New<GenCustomAddOnRule>();
			var createEventRule = new CreateEventRule { EventCode = Events.CustomisableEvent00Code, EventReference = "|NEW=<NEW>|TIME=<OLD>", IsEstimate = false, IsEnabled = true };
			rule.SetRules(createEventRule);

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var column = template.GenCustomColumnDefinitions.AddNew();
			column.XC_Name = "User1";
			column.XC_Type = AddOnColumnDataType.Codes.Datetime;
			column.XC_XR = rule.PK;
			var dummy = Factory.New<DummyCustomFieldBizo>();

			var fields = new CustomBusinessObject(null, new UserDefinedPropertyCollection(dummy) { new ProcessTaskTemplateMatches(template) });
			var date = ZDateTime.UtcNow;
			fields[CustomPropertyHelper.GeneratePropertyIdentifier("USER1", typeof(ZDateTime))] = date;

			var log = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, dummy.PK).AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomisableEvent00Code));
			AssertEquals(date, log.Single().SL_EventTime);

			createEventRule.EventReference = "ITS THE PHONE WITH APPEAL!";
			rule.SetRules(createEventRule);
			fields = new CustomBusinessObject(null, new UserDefinedPropertyCollection(dummy) { new ProcessTaskTemplateMatches(template) });
			fields[CustomPropertyHelper.GeneratePropertyIdentifier("USER1", typeof(ZDateTime))] = date.AddDays(1);

			var anotherLog = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, dummy.PK).AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomisableEvent00Code));
			AssertEquals(date.AddDays(1), anotherLog.Single().SL_EventTime);
		}

		#endregion

		public void TestComboBoxRuleWithEvent()
		{
			var addOnRulesXml1 = @"
<sourceCode>
  <rules>
    <rule code='InvalidCode' enabled='true'>
      <details>
        <codeDescriptionList>
          <codeDescription code='Blah' description='Rah' />
          <codeDescription code='Nah' description='Far' />
        </codeDescriptionList>
      </details>
    </rule>
    <rule code='CreateEvent' enabled='true'>
      <details>
        <CreateEventRuleCode>Z01</CreateEventRuleCode>
        <CreateEventRuleReference>|MEE=Hello</CreateEventRuleReference>
      </details>
    </rule>
    <rule code='DateTimeFormat' enabled='false'>
      <details>
        <format>Short</format>
      </details>
    </rule>
    <rule code='CheckEntered' enabled='false'>
      <details />
    </rule>
  </rules>
</sourceCode>";

			var addOnRulesXml2 = @"
<sourceCode>
  <rules>
    <rule code='InvalidCode' enabled='true'>
      <details>
        <codeDescriptionList>
          <codeDescription code='Blah' description='Rah' />
          <codeDescription code='Nah' description='Far' />
        </codeDescriptionList>
      </details>
    </rule>
    <rule code='CreateEvent' enabled='true'>
      <details>
        <CreateEventRuleCode>Z02</CreateEventRuleCode>
        <CreateEventRuleReference>|MEE=Hello</CreateEventRuleReference>
      </details>
    </rule>
    <rule code='DateTimeFormat' enabled='false'>
      <details>
        <format>Short</format>
      </details>
    </rule>
    <rule code='CheckEntered' enabled='false'>
      <details />
    </rule>
  </rules>
</sourceCode>";

			var dummyCustomFactory = ObjectFactory.Get<IDummyWithCustomFieldsFactory>();
			var dummy = (IWorkflowProvider)dummyCustomFactory.GetDummyWithCustomFields(Factory, out var addCustomField, out var setCustomField, out var getCustomField, out var getCustomFieldIdentifier);

			var trigger1 = dummy.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "Trigger 1";
			trigger1.TriggerConditions.TriggerEventCode = "Z01";
			trigger1.TriggerConditions.TriggerCondition = "RFP";
			trigger1.TriggerConditions.TriggerConditionValue = "MEE=Hello";

			var trigger2 = dummy.WorkflowItems.Triggers.AddNew();
			trigger2.P9_Description = "Trigger 2";
			trigger2.TriggerConditions.TriggerEventCode = "Z02";
			trigger2.TriggerConditions.TriggerCondition = "RFP";
			trigger2.TriggerConditions.TriggerConditionValue = "MEE=Hello";

			addCustomField(("Some Combo1", "CBO"), addOnRulesXml1);
			addCustomField(("Some Combo2", "CBO"), addOnRulesXml2);
			Factory.Save();

			setCustomField(("Some Combo1PART2", "STR"), new ZString("Something"));
			// Only once both parts are set should the event be raised
			AssertEquals((short)100, trigger1.TriggerConditions.TriggerFiredCountdown);
			setCustomField(("Some Combo1PART1", "STR"), new ZString("Blah"));
			AssertEquals((short)99, trigger1.TriggerConditions.TriggerFiredCountdown);

			// But the order shouldn't matter
			setCustomField(("Some Combo2PART1", "STR"), new ZString("Rah"));
			AssertEquals((short)100, trigger2.TriggerConditions.TriggerFiredCountdown);
			setCustomField(("Some Combo2PART2", "STR"), new ZString("SomethingElse"));
			AssertEquals((short)99, trigger2.TriggerConditions.TriggerFiredCountdown);
		}

		public void TestComboBoxBySettingValidValueThenDefaultValue()
		{
			var dummyCustomFactory = ObjectFactory.Get<IDummyWithCustomFieldsFactory>();
			var dummy = (IWorkflowProvider)dummyCustomFactory.GetDummyWithCustomFields(Factory, out var addCustomField, out var setCustomField, out var getCustomField, out var getCustomFieldIdentifier);

			addCustomField(("Some Combo1", "CBO"));
			Factory.Save();

			setCustomField(("Some Combo1PART2", "STR"), new ZString("Something"));
			AssertNoExceptionThrown(() => setCustomField(("Some Combo1PART1", "STR"), ZString.Empty));
		}

		public void TestAddCustomPropertiesFromWorkflowTemplate()
		{
			WorkflowDataRegistry.Instance.ShowCustomFieldsFromCurrentTemplateOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertAddCustomPropertiesFromWorkflowTemplate(false);

			WorkflowDataRegistry.Instance.ShowCustomFieldsFromCurrentTemplateOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertAddCustomPropertiesFromWorkflowTemplate(true);
		}

		void AssertAddCustomPropertiesFromWorkflowTemplate(bool expectCurrentOnly)
		{
			var template = Factory.New<ProcessTaskTemplate>();

			var dummy = Factory.New<DummyBusinessObjectWithUserDefinedFields>();
			var propertyManager = new UserDefinedPropertyManager(dummy);

			for (int i = 0; i < 3; i++)
			{
				var column = template.GenCustomColumnDefinitions.AddNew();
				column.XC_Name = "Field" + (i + 1);
				column.XC_Type = AddOnColumnDataType.Codes.Integer;

				propertyManager.SetValue("Field" + (i + 3), AddOnColumnDataType.Codes.Integer, new ZInt(i + 3));
			}

			var propertyCollection = new UserDefinedPropertyCollection(dummy) { new ProcessTaskTemplateMatches(template) };

			AssertEquals(expectCurrentOnly ? 3 : 5, propertyCollection.Count());
			AssertEquals(true, propertyCollection.Any(p => p.Identifier == CustomPropertyHelper.GeneratePropertyIdentifier("FIELD1", typeof(ZInt))));
			AssertEquals(true, propertyCollection.Any(p => p.Identifier == CustomPropertyHelper.GeneratePropertyIdentifier("FIELD2", typeof(ZInt))));
			AssertEquals(true, propertyCollection.Any(p => p.Identifier == CustomPropertyHelper.GeneratePropertyIdentifier("FIELD3", typeof(ZInt))));
			AssertEquals(!expectCurrentOnly, propertyCollection.Any(p => p.Identifier == CustomPropertyHelper.GeneratePropertyIdentifier("FIELD4", typeof(ZInt))));
			AssertEquals(!expectCurrentOnly, propertyCollection.Any(p => p.Identifier == CustomPropertyHelper.GeneratePropertyIdentifier("FIELD5", typeof(ZInt))));
		}

		public void TestGetValueWithIncorrectType()
		{
			var dummy = Factory.New<DummyBusinessObjectWithUserDefinedFields>();

			var columnDefinition = Factory.New<GenCustomColumnDefinition>();
			columnDefinition.XC_Name = "XColumn1";
			columnDefinition.XC_Type = "DEC"; // Decimal

			var customProperties = new UserDefinedPropertyCollection(dummy);
			customProperties.AddProperty(columnDefinition);

			var customObj = new CustomBusinessObject(null, customProperties);
			var propertyIdentifier = CustomPropertyHelper.GeneratePropertyIdentifier(columnDefinition.XC_Name, typeof(ZDecimal));

			AssertEquals(0m, customObj[propertyIdentifier]);
		}

		public void TestCollectionIsFilteredByParentTableCode()
		{
			var dummy = Factory.New<DummyBusinessObjectWithUserDefinedFields>();
			Factory.Save();

			var columnValue = AddCustomField(dummy, "name2", "AAA");
			columnValue.XV_ParentTableCode = "AA";

			var customProperties = new UserDefinedPropertyCollection(dummy);
			customProperties.LoadAllPresavedPropertiesOnParent();
			AssertEquals("does not load with incorrect parent table code", 0, customProperties.Count());

			AddCustomField(dummy, "name1", "BBB");
			customProperties.LoadAllPresavedPropertiesOnParent();
			AssertEquals("loads with correct parent table code", 1, customProperties.Count());
		}

		static GenCustomAddOnValue AddCustomField(BusinessObject bizo, ZString customValueName, ZString value, string type = AddOnColumnDataType.Codes.String)
		{
			var customAddOnValue = bizo.Factory.New<GenCustomAddOnValue>();
			customAddOnValue.XV_ParentID = bizo.PK;
			customAddOnValue.XV_ParentTableCode = bizo.TablePrefix;
			customAddOnValue.XV_Name = customValueName;
			customAddOnValue.XV_Type = type;
			customAddOnValue.XV_Data = value;

			return customAddOnValue;
		}
		public void TestGetValueDoesNotChangeParentTableCode()
		{
			// When a quick booking is converted to shipment, it has the same PK
			// So XV_ParentID can be the same across the two tables
			var dummy = Factory.New<DummyBusinessObjectWithUserDefinedFields>();

			var columnDefinition = Factory.New<GenCustomColumnDefinition>();
			columnDefinition.XC_Name = "XColumn1";
			columnDefinition.XC_Type = "STR";

			var columnValue = Factory.New<GenCustomAddOnValue>();
			columnValue.XV_Name = columnDefinition.XC_Name;
			columnValue.XV_Type = "STR";
			columnValue.XV_Data = "XYZ";
			columnValue.XV_ParentID = dummy.PK;
			columnValue.XV_ParentTableCode = "AA";

			var customProperties = new UserDefinedPropertyCollection(dummy);
			customProperties.AddProperty(columnDefinition);

			var customObj = new CustomBusinessObject(null, customProperties);
			var propertyIdentifier = CustomPropertyHelper.GeneratePropertyIdentifier(columnDefinition.XC_Name, typeof(ZString));
			_ = customObj[propertyIdentifier]; // Test Getter

			AssertEquals("ParentTableCode should be unchanged from getter", "AA", columnValue.XV_ParentTableCode);
		}

		public void TestAddTranslatedColumnDefinition()
		{
			var dummy = Factory.New<DummyBusinessObjectWithUserDefinedFields>();

			var columnDefinition = Factory.New<GenCustomColumnDefinition>();
			columnDefinition.XC_Name = "Hello";
			columnDefinition.XC_Type = AddOnColumnDataType.Codes.String;

			using (var mock = Res.UseMockData())
			{
				var key = columnDefinition.XC_NameInfo.CustomizableDataResourceStrings.Source.GetKey(null, columnDefinition.XC_Name);
				mock.Put(key, new ResourceStringData(key, "你好"));

				var customProperties = new UserDefinedPropertyCollection(dummy);
				customProperties.AddProperty(columnDefinition);

				var cusObj = new CustomBusinessObject(null, customProperties);

				ZPropertyInfo info = cusObj.FindPropertyInfo(CustomPropertyHelper.GeneratePropertyIdentifier("HELLO", typeof(ZString)));
				AssertEquals("你好", info.Description);
			}
		}

		public void TestAddProperty()
		{
			var customObject = new UserDefinedPropertyCollection(Factory.New<DummyBusinessObject>());
			AssertEquals(0, customObject.Count());

			customObject.Add(ZGuid.NewZGuid(), "Aaa", "Caption 1", typeof(ZString), 100);
			AssertEquals(1, customObject.Count());

			customObject.Add(ZGuid.NewZGuid(), "Bbb", "Caption 2", typeof(ZString), 100);
			AssertEquals(2, customObject.Count());

			customObject.Add(ZGuid.NewZGuid(), "Aaa", "Caption 3", typeof(ZString), 100);
			AssertEquals("Should not add property with same name", 2, customObject.Count());

			customObject.Add(ZGuid.NewZGuid(), "aaa", "Caption 4", typeof(ZString), 100);
			AssertEquals("Should not add property with same name even in different case", 2, customObject.Count());

			customObject.Add(ZGuid.NewZGuid(), "Ccc", "Caption 5", typeof(ZString), 100);
			AssertEquals(3, customObject.Count());
		}

		public void TestCheckMaximumLengthShouldNotReportAnyError()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			var addOnRule = Factory.New<GenCustomAddOnRule>();
			addOnRule.XR_IsActive = true;
			addOnRule.XR_SourceCode = @"<sourceCode>
  <rules>
    <rule code='InvalidCode'>
      <details>
        <codeDescriptionList>
          <codeDescription code='C1' description='Desc 1' />
        </codeDescriptionList>
      </details>
    </rule>
  </rules>
</sourceCode>";

			var column = template.GenCustomColumnDefinitions.AddNew();
			column.XC_Name = "Field 1";
			column.XC_Type = AddOnColumnDataType.Codes.String;
			column.XC_XR = addOnRule.PK;

			var dummy = Factory.New<DummyBusinessObjectWithUserDefinedFields>();
			var propertyCollection = new UserDefinedPropertyCollection(dummy) { new ProcessTaskTemplateMatches(template) };
			var customBizo = new CustomBusinessObject(null, propertyCollection);
			var propertyIdentifier = CustomPropertyHelper.GeneratePropertyIdentifier("FIELD 1", typeof(ZString));
			customBizo[propertyIdentifier] = "TEST VALUE";

			var propertyInfo = customBizo.FindPropertyInfo(propertyIdentifier);
			Assert(propertyInfo.HasError($"The maximum length of '{propertyIdentifier}' has been exceeded.\n The maximum length of this property is 2 characters, but 10 were entered. New value: {customBizo[propertyIdentifier]}. Old value: "));
		}

		public void TestAddPropertyWithDifferentGenCustomColumnDefinitionType()
		{
			var template = Factory.New<ProcessTaskTemplate>();

			var addOnRule = Factory.New<GenCustomAddOnRule>();
			addOnRule.XR_IsActive = true;
			addOnRule.XR_SourceCode = @"<sourceCode>
  <rules>
    <rule code='InvalidCode'>
      <details>
        <codeDescriptionList>
          <codeDescription code='C1' description='Desc 1' />
          <codeDescription code='C2' description='Desc 2' />
          <codeDescription code='C3' description='Desc 3' />
          <codeDescription code='C4' description='Desc 4' />
        </codeDescriptionList>
      </details>
    </rule>
  </rules>
</sourceCode>";

			var column = template.GenCustomColumnDefinitions.AddNew();
			column.XC_Name = "Field 1";
			column.XC_Type = AddOnColumnDataType.Codes.String;
			column.XC_XR = addOnRule.PK;

			var dummy = Factory.New<DummyBusinessObjectWithUserDefinedFields>();
			UserDefinedPropertyManager propertyManager = new UserDefinedPropertyManager(dummy);
			propertyManager.SetValue("Field 1", AddOnColumnDataType.Codes.Datetime, new ZDateTime(2009, 9, 2));

			var propertyCollection = new UserDefinedPropertyCollection(dummy) { new ProcessTaskTemplateMatches(template) };
			var customBizo = new CustomBusinessObject(null, propertyCollection);
			customBizo.Validation.ValidateAll();

			var propertyInfo = customBizo.FindPropertyInfo(CustomPropertyHelper.GeneratePropertyIdentifier("FIELD 1", typeof(ZString)));

			AssertEquals(column.XC_Type.GetType(), propertyInfo.PropertyType);
		}

		public void TestComboBox()
		{
			var template = Factory.New<ProcessTaskTemplate>();

			var addOnRule = Factory.New<GenCustomAddOnRule>();
			addOnRule.XR_IsActive = true;
			addOnRule.XR_SourceCode = @"<sourceCode>
  <rules>
    <rule code='InvalidCode'>
      <details>
        <codeDescriptionList>
          <codeDescription code='C1' description='Desc 1' />
          <codeDescription code='C2' description='Desc 2' />
          <codeDescription code='C3' description='Desc 3' />
          <codeDescription code='C4' description='Desc 4' />
        </codeDescriptionList>
      </details>
    </rule>
  </rules>
</sourceCode>";

			var column = template.GenCustomColumnDefinitions.AddNew();
			column.XC_Name = "Unrestricted";
			column.XC_Type = AddOnColumnDataType.Codes.ComboBox;

			column = template.GenCustomColumnDefinitions.AddNew();
			column.XC_Name = "Restricted";
			column.XC_Type = AddOnColumnDataType.Codes.ComboBox;
			column.XC_XR = addOnRule.PK;

			var dummy = Factory.New<DummyBusinessObjectWithUserDefinedFields>();
			UserDefinedPropertyManager propertyManager = new UserDefinedPropertyManager(dummy);
			var propertyCollection = new UserDefinedPropertyCollection(dummy) { new ProcessTaskTemplateMatches(template) };
			var customBizo = new CustomBusinessObject(null, propertyCollection);
			customBizo.Validation.ValidateAll();

			var unrestricted1 = customBizo.FindPropertyInfo(CustomPropertyHelper.GeneratePropertyIdentifier("UNRESTRICTED" + AddOnColumnDataType.PartIdentifier.ToUpperInvariant() + "1" + "", typeof(ZString)));
			var unrestricted2 = customBizo.FindPropertyInfo(CustomPropertyHelper.GeneratePropertyIdentifier("UNRESTRICTED" + AddOnColumnDataType.PartIdentifier.ToUpperInvariant() + "2" + "", typeof(ZString)));
			var restricted1 = customBizo.FindPropertyInfo(CustomPropertyHelper.GeneratePropertyIdentifier("RESTRICTED" + AddOnColumnDataType.PartIdentifier.ToUpperInvariant() + "1" + "", typeof(ZString)));
			var restricted2 = customBizo.FindPropertyInfo(CustomPropertyHelper.GeneratePropertyIdentifier("RESTRICTED" + AddOnColumnDataType.PartIdentifier.ToUpperInvariant() + "2" + "", typeof(ZString)));

			AssertEquals(typeof(ZString), unrestricted1.PropertyType);
			AssertEquals(typeof(ZString), unrestricted2.PropertyType);
			AssertEquals(typeof(ZString), restricted1.PropertyType);
			AssertEquals(typeof(ZString), restricted2.PropertyType);
			AssertEquals(1, MetaData.GetMetaData(customBizo, unrestricted1.PropertyDescriptor, MetaDataTypes.CustomFieldPosition));
			AssertEquals(2, MetaData.GetMetaData(customBizo, unrestricted2.PropertyDescriptor, MetaDataTypes.CustomFieldPosition));
			AssertEquals(1, MetaData.GetMetaData(customBizo, restricted1.PropertyDescriptor, MetaDataTypes.CustomFieldPosition));
			AssertEquals(2, MetaData.GetMetaData(customBizo, restricted2.PropertyDescriptor, MetaDataTypes.CustomFieldPosition));
			AssertEquals(0, MetaData.GetMetaData(customBizo, unrestricted1.PropertyDescriptor, MetaDataTypes.Position));
			AssertEquals(0, MetaData.GetMetaData(customBizo, unrestricted2.PropertyDescriptor, MetaDataTypes.Position));
			AssertEquals(0, MetaData.GetMetaData(customBizo, restricted1.PropertyDescriptor, MetaDataTypes.Position));
			AssertEquals(0, MetaData.GetMetaData(customBizo, restricted2.PropertyDescriptor, MetaDataTypes.Position));

			AssertNotEquals(2, unrestricted1.MaxLength);
			AssertNotEquals(2, unrestricted2.MaxLength);
			AssertEquals(2, restricted1.MaxLength);
			AssertNotEquals(2, restricted2.MaxLength);

			//Both validations will be triggered when we change just one property thanks to new 'RelatedProperties' logic.
			unrestricted1.Value = (ZString)"asdf";
			AssertNoErrors(unrestricted1);
			AssertHasErrors(unrestricted2);
			unrestricted2.Value = (ZString)"asdf";
			AssertNoErrors(unrestricted1);
			AssertNoErrors(unrestricted2);
			unrestricted1.Value = (ZString)"";
			AssertHasErrors(unrestricted1);
			AssertNoErrors(unrestricted2);

			restricted1.Value = (ZString)"AS";
			AssertHasErrors(restricted1);
			AssertHasErrors(restricted2);
			restricted2.Value = (ZString)"asdf";
			AssertHasErrors(restricted1);
			AssertNoErrors(restricted2);
			restricted1.Value = (ZString)"C1";
			AssertNoErrors(restricted1);
			AssertNoErrors(restricted2);
			restricted1.Value = (ZString)"AS";
			AssertHasErrors(restricted1);
			AssertNoErrors(restricted2);
		}

		public void TestPropertyValidator()
		{
			var template = Factory.New<ProcessTaskTemplate>();

			var column = template.GenCustomColumnDefinitions.AddNew();
			column.XC_Name = "Field 1";
			column.XC_Type = AddOnColumnDataType.Codes.String;

			var dummy = Factory.New<DummyBusinessObjectWithUserDefinedFields>();
			var propertyCollection = new UserDefinedPropertyCollection(dummy) { new ProcessTaskTemplateMatches(template) };
			var customBizo = new CustomBusinessObject(null, propertyCollection);

			var customAddOnValue = Factory.New<GenCustomAddOnValue>();
			customAddOnValue.XV_Name = "Field 1";
			customAddOnValue.XV_ParentID = dummy.PK;
			customAddOnValue.XV_ParentTableCode = dummy.TablePrefix;

			var propertyInfo = customBizo.FindPropertyInfo(CustomPropertyHelper.GeneratePropertyIdentifier("FIELD 1", typeof(ZString)));

			customAddOnValue.XV_Type = ZString.Empty;
			customBizo.Validation.ValidateAll();
			AssertHasError(propertyInfo, "Error - XV_Type: Please enter a 'Field 1' custom field value type.");

			customAddOnValue.XV_Type = AddOnColumnDataType.Codes.String;
			customBizo.Validation.ValidateAll();
			AssertNoErrors(propertyInfo);
		}

		#region TestSameCustomEvents

		public void TestSameCustomEvents_Created()
		{
			var shipment = GetShipmentThatCreatesEventsWithSameCodeAtSameTime();
			AddEdtEvent(shipment);

			Factory.Save();

			var event1LogQuery = new ZQuery();
			event1LogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, "Z45");
			event1LogQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.EndsWith, "REFONE");
			event1LogQuery.AddToFilter(StmALogSchema.SL_IsCancelled, false);

			var event2LogQuery = new ZQuery();
			event2LogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, "Z45");
			event2LogQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.EndsWith, "REFTWO");
			event2LogQuery.AddToFilter(StmALogSchema.SL_IsCancelled, false);

			var logs = Factory.Load<StmALog>(event1LogQuery);
			AssertEquals("Object edited and saved => Z45 event with reference REFONE should be created", 1, logs.Length);
			logs = Factory.Load<StmALog>(event2LogQuery);
			AssertEquals("Object edited and saved => Z45 event with reference REFTWO should be created", 1, logs.Length);
		}

		public void TestSameCustomEvents_NothingCancelled()
		{
			var shipment = GetShipmentThatCreatesEventsWithSameCodeAtSameTime();
			AddEdtEvent(shipment);

			Factory.Save();

			var event1LogQuery = new ZQuery();
			event1LogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, "Z45");
			event1LogQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.EndsWith, "REFONE");
			event1LogQuery.AddToFilter(StmALogSchema.SL_IsCancelled, false);

			var event2LogQuery = new ZQuery();
			event2LogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, "Z45");
			event2LogQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.EndsWith, "REFTWO");
			event2LogQuery.AddToFilter(StmALogSchema.SL_IsCancelled, false);

			var customBizo = ((ICustomFieldProvider)shipment).GetCustomBusinessObject();

			// Setting to Empty will create event
			customBizo[CustomPropertyHelper.GeneratePropertyIdentifier("field 1", typeof(ZString))] = ZString.Empty;

			var logs = Factory.Load<StmALog>(event1LogQuery);
			AssertEquals("Object edited and saved => Z45 event with reference REFONE should not be cancelled", 2, logs.Length);
			logs = Factory.Load<StmALog>(event2LogQuery);
			AssertEquals("Object edited and saved => Z45 event with reference REFTWO should not be cancelled", 2, logs.Length);

			// Setting to Empty will create event
			customBizo[CustomPropertyHelper.GeneratePropertyIdentifier("field 2", typeof(ZString))] = ZString.Empty;

			logs = Factory.Load<StmALog>(event1LogQuery);
			AssertEquals("Object edited and saved => Z45 event with reference REFONE should not be cancelled", 2, logs.Length);
			logs = Factory.Load<StmALog>(event2LogQuery);
			AssertEquals("Object edited and saved => Z45 event with reference REFTWO should not be cancelled", 3, logs.Length);
		}

		public void TestSameCustomEvents_Recreated()
		{
			var shipment = GetShipmentThatCreatesEventsWithSameCodeAtSameTime();
			AddEdtEvent(shipment);

			Factory.Save();

			var event1LogQuery = new ZQuery();
			event1LogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, "Z45");
			event1LogQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.EndsWith, "REFONE");
			event1LogQuery.AddToFilter(StmALogSchema.SL_IsCancelled, false);

			var event2LogQuery = new ZQuery();
			event2LogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, "Z45");
			event2LogQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.EndsWith, "REFTWO");
			event2LogQuery.AddToFilter(StmALogSchema.SL_IsCancelled, false);

			// Setting to Empty will create event
			var customBizo = ((ICustomFieldProvider)shipment).GetCustomBusinessObject();
			customBizo[CustomPropertyHelper.GeneratePropertyIdentifier("field 1", typeof(ZString))] = ZString.Empty;
			customBizo[CustomPropertyHelper.GeneratePropertyIdentifier("field 2", typeof(ZString))] = ZString.Empty;

			AddEdtEvent(shipment);
			Factory.Save();

			var logs = Factory.Load<StmALog>(event1LogQuery);
			AssertEquals("Custom field edited, saved, cleared, edited again re-saved => One Z45 event with reference REFONE should be created", 3, logs.Length);
			logs = Factory.Load<StmALog>(event2LogQuery);
			AssertEquals("Custom field edited, saved, cleared, edited again re-saved => Two Z45 event with reference REFTWO should be created", 3, logs.Length);
		}

		#endregion

		void AddEdtEvent(Forwarding.IForwardingShipment shipment)
		{
			var edtEvent = Factory.New<StmALog>();
			using (edtEvent.LockForUpdatingKeyFieldsForTesting())
			{
				edtEvent.SL_Parent = shipment.PK;
				edtEvent.SL_Table = "JobShipment";
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				edtEvent.SL_SE_NKEvent = Events.EditedARecord.Code;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
			}
		}

		Forwarding.IForwardingShipment GetShipmentThatCreatesEventsWithSameCodeAtSameTime()
		{
			#region template

			var template = Factory.New(ObjectFactory.GetType(typeof(IProcessTaskTemplate)));
			template.FillWithValidTestData();
			template[ProcessTaskTemplateSchema.P0_ProcessType] = "SHP";
			template[ProcessTaskTemplateSchema.P0_GC] = EnvProxy.Instance.CurrentCompany.PK;
			template[ProcessTaskTemplateSchema.P0_IsActive] = true;
			template[ProcessTaskTemplateSchema.P0_IsSystem] = false;

			#endregion

			#region add on rules

			var addOnRule1 = Factory.New(Type.GetType("Enterprise.MasterFiles.Business.CustomValues.GenCustomAddOnRule, Enterprise.MasterFiles.Business"));
			addOnRule1[GenCustomAddOnRuleSchema.XR_Code] = "ONE";
			addOnRule1[GenCustomAddOnRuleSchema.XR_IsActive] = true;
			addOnRule1[GenCustomAddOnRuleSchema.XR_SourceCode] = @"<sourceCode>
  <rules>
    <rule code='InvalidCode' enabled='false'>
      <details>
        <codeDescriptionList />
      </details>
    </rule>
    <rule code='CreateEvent' enabled='true'>
      <details>
        <CreateEventRuleCode>Z45</CreateEventRuleCode>
        <CreateEventRuleReference>REFONE</CreateEventRuleReference>
      </details>
    </rule>
    <rule code='DateTimeFormat' enabled='false'>
      <details>
        <format>Short</format>
      </details>
    </rule>
    <rule code='CheckEntered' enabled='false'>
      <details />
    </rule>
  </rules>
</sourceCode>";
			var addOnRule2 = Factory.New(Type.GetType("Enterprise.MasterFiles.Business.CustomValues.GenCustomAddOnRule, Enterprise.MasterFiles.Business"));
			addOnRule2[GenCustomAddOnRuleSchema.XR_Code] = "TWO";
			addOnRule2[GenCustomAddOnRuleSchema.XR_IsActive] = true;
			addOnRule2[GenCustomAddOnRuleSchema.XR_SourceCode] = @"<sourceCode>
  <rules>
    <rule code='InvalidCode' enabled='false'>
      <details>
        <codeDescriptionList />
      </details>
    </rule>
    <rule code='CreateEvent' enabled='true'>
      <details>
        <CreateEventRuleCode>Z45</CreateEventRuleCode>
        <CreateEventRuleReference>REFTWO</CreateEventRuleReference>
      </details>
    </rule>
    <rule code='DateTimeFormat' enabled='false'>
      <details>
        <format>Short</format>
      </details>
    </rule>
    <rule code='CheckEntered' enabled='false'>
      <details />
    </rule>
  </rules>
</sourceCode>";

			#endregion

			#region custom fields

			var addNew = template["GenCustomColumnDefinitions"].GetType().GetMethod("AddNew");
			var field1 = (BusinessObject)addNew.Invoke(template["GenCustomColumnDefinitions"], null);
			field1[GenCustomColumnDefinitionSchema.XC_Name] = "field 1";
			field1[GenCustomColumnDefinitionSchema.XC_Type] = "STR";
			field1[GenCustomColumnDefinitionSchema.XC_XR] = addOnRule1.PK;
			var field2 = (BusinessObject)addNew.Invoke(template["GenCustomColumnDefinitions"], null);
			field2[GenCustomColumnDefinitionSchema.XC_Name] = "field 2";
			field2[GenCustomColumnDefinitionSchema.XC_Type] = "STR";
			field2[GenCustomColumnDefinitionSchema.XC_XR] = addOnRule2.PK;

			#endregion

			Factory.Save();
			Factory.ClearQueryCache();

			var shipment = (Forwarding.IForwardingShipment)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			Factory.Save();

			#region triggers

			var edtTrigger = (IProcessTask)((BusinessObjectCollection)((BusinessObject)shipment)["WorkflowItems"]).AddNew();
			edtTrigger.P9_Type = "TRG";
			edtTrigger.P9_Description = "edt trigger";
			((BusinessObject)edtTrigger)["IsWorkflowTrigger"] = true;
			((IBaseTrigger)edtTrigger).TriggerEventCode = "EDT";

			var z45Trigger = (IProcessTask)((BusinessObjectCollection)((BusinessObject)shipment)["WorkflowItems"]).AddNew();
			z45Trigger.P9_Type = "TRG";
			z45Trigger.P9_Description = "z trigger";
			((BusinessObject)z45Trigger)["IsWorkflowTrigger"] = true;
			((IBaseTrigger)z45Trigger).TriggerEventCode = "Z45";

			var edtTriggerAction = (IProcessTaskNotification)((IBusinessObjectCollection)((BusinessObject)edtTrigger)["ProcessTaskNotifications"]).AddNew();
			edtTriggerAction.PQ_TriggerType = "IFC";
			edtTriggerAction.PQ_EmailText = "<NOW>";
			edtTriggerAction.PQ_P9 = edtTrigger.PK;
			edtTriggerAction.PQ_EmailAddr = "<GetCustomField(field 1)>";

			var z45TriggerAction = (IProcessTaskNotification)((IBusinessObjectCollection)((BusinessObject)z45Trigger)["ProcessTaskNotifications"]).AddNew();
			z45TriggerAction.PQ_TriggerType = "IFC";
			z45TriggerAction.PQ_EmailText = "<NOW>";
			z45TriggerAction.PQ_P9 = z45Trigger.PK;
			z45TriggerAction.PQ_EmailAddr = "<GetCustomField(field 2)>";

			#endregion

			return shipment;
		}

		CustomBusinessObject GetCustomBusinessObject(DummyBusinessObject bizObj)
		{
			var rule1 = Factory.New<GenCustomAddOnRule>();
			rule1.SetRules(new CheckEnteredRule { IsEnabled = true }, new DateTimeFormatRule { Format = KDateTimeFormat.Time, IsEnabled = true });

			var template = GetProcessTaskTemplate();
			template.GenCustomColumnDefinitions[2].XC_XR = rule1.PK;

			var properties = new UserDefinedPropertyCollection(bizObj)
			{
				{ ZGuid.NewZGuid(), DummyBusinessObject.Schema.Z0_VarCharMax, "Real Field" },
				{ ZGuid.NewZGuid(), DummyBusinessObject.Schema.Z0_NVarChar, "My Field" },
				{ new ProcessTaskTemplateMatches(template) },
			};

			return new CustomBusinessObject(bizObj, properties);
		}

		ProcessTaskTemplate GetProcessTaskTemplate()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			var column1 = template.GenCustomColumnDefinitions.AddNew();
			column1.XC_Name = "UserText1";
			column1.XC_Type = AddOnColumnDataType.Codes.String;
			var column2 = template.GenCustomColumnDefinitions.AddNew();
			column2.XC_Name = "UserDecimal1";
			column2.XC_Type = AddOnColumnDataType.Codes.Decimal;
			var column3 = template.GenCustomColumnDefinitions.AddNew();
			column3.XC_Name = "UserDateTime1";
			column3.XC_Type = AddOnColumnDataType.Codes.Datetime;

			return template;
		}
	}
}

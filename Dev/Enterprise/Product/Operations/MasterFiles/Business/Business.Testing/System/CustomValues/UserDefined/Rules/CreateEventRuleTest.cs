using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	sealed class CreateEventRuleTest : TestCaseWithFactory
	{
		public void TestGetMetaData()
		{
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<DynamicMetaData>(), new CreateEventRule().GetMetaData());
		}

		public void TestGetValidator()
		{
			AssertNull(new CreateEventRule().GetValidator());
		}

		public void TestToXml()
		{
			var obj = new CreateEventRule();
			obj.EventCode = "EVT";
			obj.EventReference = "Biggy";
			obj.IsEstimate = true;

			var element = new RulesFactory().ToXml(obj);
			var expected = "<sourceCode><rules><rule code=\"CreateEvent\" enabled=\"false\"><details><CreateEventRuleCode>EVT</CreateEventRuleCode><CreateEventRuleReference>Biggy</CreateEventRuleReference><IsEstimate /></details></rule></rules></sourceCode>";
			AssertEquals(expected, element.ToString(SaveOptions.DisableFormatting));
		}

		public void TestFromXml()
		{
			var obj = new CreateEventRule();
			obj.EventCode = "EVT";
			obj.EventReference = "Biggy";
			obj.IsEstimate = true;

			var element = new RulesFactory().ToXml(obj);
			var obj2 = (CreateEventRule)new RulesFactory().FromXml(element).SingleOrDefault();
			AssertEquals(obj.EventCode, obj2.EventCode);
			AssertEquals(obj.EventReference, obj2.EventReference);
			AssertEquals(obj.IsEstimate, obj2.IsEstimate);
		}

		public void TestFromXml_WithDefault()
		{
			var obj = new CreateEventRule();
			var element = new RulesFactory().ToXml(obj);
			var obj2 = (CreateEventRule)new RulesFactory().FromXml(element).SingleOrDefault();
			AssertEquals(Events.CustomisableEvent00Code, obj2.EventCode);
		}

		[ExpectNoExceptions]
		public void TestFromXml_Empty()
		{
			var obj = new CreateEventRule();
			new RulesFactory().FromXml(new XElement("b")).SingleOrDefault();
		}

		public void TestCanBeApplied()
		{
			var rule = new CreateEventRule();

			Assert(rule.CanBeApplied(typeof(ZString)));
			Assert(rule.CanBeApplied(typeof(ZDateTime)));
			Assert(!rule.CanBeApplied(typeof(ZBool)));
		}

		public void TestOnSetImpl_ShortensFreeTextInLongReference()
		{
			var bizo = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();

			var parameters = new string('1', StmALogSchema.SL_Reference.MaxLength + 20);
			var createEventRule = new CreateEventRule { EventCode = Events.CustomisableEvent00Code, EventReference = parameters, IsEstimate = false, IsEnabled = true };

			var guid = Guid.NewGuid();

			var args = new CustomAddOnRuleArgs
			{
				NewValue = new ZString("a")
			};
			var onSet = createEventRule.GetOnSetBehaviour();
			onSet(bizo, null, guid, args);

			var log = bizo.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code).First();
			var expected = new string('1', StmALogSchema.SL_Reference.MaxLength);

			AssertEquals("Free text was not correctly truncated", expected, log.SL_Reference);
		}

		public void TestOnSetImpl_InvalidDateTime()
		{
			var bizo = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var createEventRule = new CreateEventRule { EventCode = Events.CustomisableEvent00Code, IsEstimate = false, IsEnabled = true };
			var guid = Guid.NewGuid();

			var onSet = createEventRule.GetOnSetBehaviour();
			onSet(bizo, null, guid, new CustomAddOnRuleArgs
			{
				NewValue = ZDateTime.Now
			});

			var log = bizo.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code).FirstOrDefault();
			AssertNotNull(log);

			onSet(bizo, null, guid, new CustomAddOnRuleArgs
			{
				NewValue = ZDateTime.Invalid
			});
			AssertEquals(true, log.IsDeleted);

			onSet(bizo, null, guid, new CustomAddOnRuleArgs
			{
				NewValue = ZDateTime.Now
			});
			log = bizo.GetLogs().Find(l => !l.IsDeleted && l.SL_SE_NKEvent == Events.CustomisableEvent00Code).FirstOrDefault();
			AssertNotNull("New log should be created without tripping over the one we just deleted.", log);
		}

		public void TestOnSetWithLogEmptyEventTime()
		{
			var bizo = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var bizoWorkflowProvider = (IWorkflowProvider)bizo;
			
			var trigger = bizoWorkflowProvider.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action.PQ_EmailAddr = "abc@email.com";
			action.PQ_EmailText = "Test";

			var guid = Guid.NewGuid();

			var existingLog = bizoWorkflowProvider.Logs.AddNew(new EventValue(Events.CustomisableEvent00, inMemoryIdentifier: guid));

			var createEventRule = new CreateEventRule { EventCode = Events.CustomisableEvent00Code, EventReference = existingLog.SL_Reference, IsEstimate = false, IsEnabled = true };

			var onSet = createEventRule.GetOnSetBehaviour();
			onSet(bizo, null, guid, new CustomAddOnRuleArgs { NewValue = ZDateTime.Empty });
			AssertEquals(true, existingLog.IsDeleted);
		}

		public void TestOnSetWithLogEmptyEventTimeErrorReports()
		{
			var bizo = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var bizoWorkflowProvider = (IWorkflowProvider)bizo;
			var trigger = bizoWorkflowProvider.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action.PQ_EmailAddr = "abc@email.com";
			action.PQ_EmailText = "Test";
			var guid = Guid.NewGuid();
			var existingLog = bizoWorkflowProvider.Logs.AddNew(new EventValue(Events.CustomisableEvent00, inMemoryIdentifier: guid));

			var errorReporterMock = new Mock<IErrorReporter>();

			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				using (existingLog.LockForUpdatingKeyFieldsForTesting())
				{
					existingLog.SL_EventTime = ZDateTime.Empty;
				}
				var createEventRule = new CreateEventRule { EventCode = Events.CustomisableEvent00Code, EventReference = existingLog.SL_Reference, IsEstimate = false, IsEnabled = true };
				var onSet = createEventRule.GetOnSetBehaviour();
				onSet(bizo, null, guid, new CustomAddOnRuleArgs { NewValue = ZDateTime.Empty });
				AssertEquals(2, ErrorReporter.TotalErrorCount);
				errorReporterMock.Verify(
					reporter => reporter.Report(It.IsAny<string>(), It.Is<string>((value) => value == StmALog.LogMessages.InvalidEventTimeError(existingLog.PK)), It.IsAny<Exception>()),
					Times.Once);
				errorReporterMock.Verify(
					reporter => reporter.Report(It.IsAny<string>(), It.IsRegex("A StmALog with an empty EventTime is invalid and should not create a 'WTE' event.\nPK: .*, Code: Z00, Reference: .*, Estimate: .*, EventTime: .*, Parent ID: .*, SL_Parent: .*"), It.IsAny<Exception>()),
					Times.Once);

				ErrorReporter.Clear();
			}
		}

		public void TestUnfiringOnCustomFieldValueChanged()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";

			var customField = template.GenCustomColumnDefinitions.AddNew();
			customField.XC_Name = "User1";
			customField.XC_Type = AddOnColumnDataType.Codes.String;

			var createEventRule1 = new CreateEventRule {  EventCode = Events.CustomisableEvent01Code, EventReference = "123", IsEstimate = false, IsEnabled = true };
			var rule1 = Factory.NewWithValidTestData<GenCustomAddOnRule>();
			rule1.SetRules(createEventRule1);
			customField.XC_XR = rule1.PK;
			var triggerA = template.WorkflowItems.Triggers.AddNew();
			triggerA.P9_Description = "TRIGGER1";
			triggerA.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
			triggerA.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			triggerA.TriggerConditions.TriggerConditionValue = "\"<GetCustomField(User1)>\"==\"SomeValue\"";
			triggerA.TriggerConditions.TriggerFiredCountdown = 10;
			var triggerB = template.WorkflowItems.Triggers.AddNew();
			triggerB.P9_Description = "TRIGGER2";
			triggerB.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
			triggerB.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			triggerB.TriggerConditions.TriggerConditionValue = "\"<GetCustomField(User1)>\"==\"ADifferentValue\"";
			triggerB.TriggerConditions.TriggerFiredCountdown = 10;
			Factory.Save();

			var dummy = Factory.New<DummyWithCustomFields>();
			dummy.ApplyWorkflowTemplates();
			Factory.Save();

			var fields = new CustomBusinessObject(null, new UserDefinedPropertyCollection(dummy) { new ProcessTaskTemplateMatches(template) });
			string id = CustomPropertyHelper.GeneratePropertyIdentifier("USER1", typeof(ZString));
			fields[id] = "SomeValue";
			fields[id] = "ADifferentValue";
			var ordered = dummy.WorkflowItems.Triggers.OrderBy(t => t.TriggerConditions.TriggerFiredCountdown);
			var triggerFired = ordered.First();
			var triggerUnfired = ordered.Last();
			AssertEquals((short)9,triggerFired.TriggerConditions.TriggerFiredCountdown);
			AssertEquals((short)10, triggerUnfired.TriggerConditions.TriggerFiredCountdown);
		}

		static readonly ZDateTime OldDateValue = new(2025, 01, 01, 12, 23, 55);
		static readonly ZDateTime NewDateValue = new(2026, 12, 23, 14, 15, 16);

		public void TestReferenceValueFromLogDefaultBehavior()
		{
			CombineAssertions(() =>
			{
				TestCaseString("String-OldValueIsNull", null, "new text", "|NAM=String-OldValueIsNull|NEW=new text|OLD=");
				TestCaseString("String-OldValueIsEmpty", ZString.Empty, "new text", "|NAM=String-OldValueIsEmpty|NEW=new text|OLD=");
				TestCaseString("String-ModifyExisting", "old text", "new text", "|NAM=String-ModifyExisting|NEW=new text|OLD=old text");
				TestCaseString("String-SetToDefault", "old text", ZString.Empty, "|NAM=String-SetToDefault|NEW=|OLD=old text");

				TestCaseInt("Int-OldValueIsNull", null, 1, "|NAM=Int-OldValueIsNull|NEW=1|OLD=0");
				TestCaseInt("Int-OldValueIsZero", ZInt.Zero, 2, "|NAM=Int-OldValueIsZero|NEW=2|OLD=0");
				TestCaseInt("Int-ModifyExisting", 1, 2, "|NAM=Int-ModifyExisting|NEW=2|OLD=1");
				TestCaseInt("Int-SetToDefault", 1, ZInt.Zero, "|NAM=Int-SetToDefault|NEW=0|OLD=1");

				TestCaseDecimal("Dec-OldValueIsNull", null, 1.1M, $"|NAM=Dec-OldValueIsNull|NEW={1.1M}|OLD=0");
				TestCaseDecimal("Dec-OldValueIsZero", ZDecimal.Zero, 2.3M, $"|NAM=Dec-OldValueIsZero|NEW={2.3M}|OLD=0");
				TestCaseDecimal("Dec-ModifyExisting", 1.5M, 2.6M, $"|NAM=Dec-ModifyExisting|NEW={2.6M}|OLD={1.5M}");
				TestCaseInt("Dec-SetToDefault", 1, ZInt.Zero, "|NAM=Dec-SetToDefault|NEW=0|OLD=1");

				TestCaseDateTime("Date-OldValueIsNull", null, NewDateValue, $"|NAM=Date-OldValueIsNull|NEW={NewDateValue}|OLD=");
				TestCaseDateTime("Date-OldValueIsEmpty", ZDateTime.Empty, NewDateValue, $"|NAM=Date-OldValueIsEmpty|NEW={NewDateValue}|OLD=");
				TestCaseDateTime("Date-ModifyExisting", OldDateValue, NewDateValue, $"|NAM=Date-ModifyExisting|NEW={NewDateValue}|OLD={OldDateValue}");
				TestCaseDateTime("Date-SetToDefault", OldDateValue, ZDateTime.Empty, $"|NAM=Date-SetToDefault|NEW=|OLD={OldDateValue}");
			});
			void TestCase(string fieldName, IZType oldValue, IZType newValue, string expectedResult, string type)
			{
				var bizo = Factory.New<DummyCustomFieldBizo>();
				var createEventRule = new CreateEventRule { EventCode = Events.CustomisableEvent00Code, IsEnabled = true };
				var onSet = createEventRule.GetOnSetBehaviour();
				var guid = Guid.NewGuid();
				var args = new CustomAddOnRuleArgs
				{
					FieldName = fieldName,
					OldValue = oldValue,
					NewValue = newValue,
					Type = type
				};

				onSet(bizo, null, guid, args);

				var log = bizo.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code).FirstOrDefault();
				AssertNotNull(log);
				AssertEquals(fieldName, expectedResult, log!.SL_Reference);
			}

			void TestCaseString(string fieldName, string oldValue, string newValue, string expectedResult)
			{
				TestCase(fieldName, new ZString(oldValue), new ZString(newValue), expectedResult, AddOnColumnDataType.Codes.String);
			}

			void TestCaseInt(string fieldName, int? oldValue, int? newValue, string expectedResult)
			{
				TestCase(fieldName, new ZInt(oldValue), new ZInt(newValue), expectedResult, AddOnColumnDataType.Codes.Integer);
			}

			void TestCaseDecimal(string fieldName, decimal? oldValue, decimal? newValue, string expectedResult)
			{
				TestCase(fieldName, new ZDecimal(oldValue), new ZDecimal(newValue), expectedResult, AddOnColumnDataType.Codes.Decimal);
			}

			void TestCaseDateTime(string fieldName, ZDateTime? oldValue, ZDateTime? newValue, string expectedResult)
			{
				TestCase(fieldName, oldValue, newValue, expectedResult, AddOnColumnDataType.Codes.Datetime);
			}
		}

		public void TestReferenceValueFromDateTypeWithDateTimeFormatRule()
		{
			CombineAssertions(() =>
			{
				TestCase("ShortDateField", OldDateValue, NewDateValue, KDateTimeFormat.Short, "|NAM=ShortDateField|NEW=23-Dec-26|OLD=01-Jan-25");
				TestCase("LongDateField", OldDateValue, NewDateValue, KDateTimeFormat.Long, "|NAM=LongDateField|NEW=23 Dec 2026 14:15|OLD=01 Jan 2025 12:23");
				TestCase("TimeField", OldDateValue, NewDateValue, KDateTimeFormat.Time, "|NAM=TimeField|NEW=14:15|OLD=12:23");

				// Empty Date tests
				TestCase("FormattedDate-OldValueIsNull", null, NewDateValue, KDateTimeFormat.Short, "|NAM=FormattedDate-OldValueIsNull|NEW=23-Dec-26|OLD=");
				TestCase("FormattedDate-OldValueIsEmpty", ZDateTime.Empty, NewDateValue, KDateTimeFormat.Short, "|NAM=FormattedDate-OldValueIsEmpty|NEW=23-Dec-26|OLD=");

				// Technically not available selection in UI but is part of the KDateTimeFormat
				TestCase("CustomDateField", OldDateValue, NewDateValue, KDateTimeFormat.Custom, "|NAM=CustomDateField|NEW=23 Dec 2026 14:15|OLD=01 Jan 2025 12:23");
			});
			void TestCase(string fieldName, ZDateTime? oldValue, ZDateTime newValue, KDateTimeFormat format, string expectedResult)
			{
				var bizo = Factory.New<DummyCustomFieldBizo>();
				var createEventRule = new CreateEventRule { EventCode = Events.CustomisableEvent00Code, IsEnabled = true };
				var guid = Guid.NewGuid();
				var args = new CustomAddOnRuleArgs
				{
					FieldName = fieldName,
					OldValue = oldValue,
					NewValue = newValue,
					Type = AddOnColumnDataType.Codes.Datetime,
					Rules =
					[
						new DateTimeFormatRule
						{
							Format = format
						}
					]
				};

				var onSet = createEventRule.GetOnSetBehaviour();
				onSet(bizo, null, guid, args);

				var log = bizo.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code).FirstOrDefault();
				AssertNotNull(log);
				AssertEquals(fieldName, expectedResult, log!.SL_Reference);
			}
		}

		public void TestReferenceValueWithInvalidCodeRule()
		{
			var ruleList = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("CMB1", "Code 1"), new CodeDescriptionPair("CMB2", "Code 2")
			};

			CombineAssertions(() =>
			{
				TestCaseString("OldValueIsNull", null, "CMB1", "|NAM=OldValueIsNull|NEW=CMB1:Code 1|OLD=");
				TestCaseString("OldValueIsEmpty", ZString.Empty, "CMB2", "|NAM=OldValueIsEmpty|NEW=CMB2:Code 2|OLD=");
				TestCaseString("ModifyExisting", "CMB1", "CMB2", "|NAM=ModifyExisting|NEW=CMB2:Code 2|OLD=CMB1:Code 1");

				// Code not existing on the list
				TestCaseString("NotExistingValues", "CMB5", "CMB3", "|NAM=NotExistingValues|NEW=CMB3|OLD=CMB5");
				TestCaseString("NotExistingOldValue", "CMB3", "CMB2", "|NAM=NotExistingOldValue|NEW=CMB2:Code 2|OLD=CMB3");
				TestCaseString("NotExistingNewValue", "CMB1", "CMB3", "|NAM=NotExistingNewValue|NEW=CMB3|OLD=CMB1:Code 1");

				// Clear the list to test the empty list case
				ruleList.Clear();
				TestCaseString("EmptyList", "CMB1", "CMB2", "|NAM=EmptyList|NEW=CMB2|OLD=CMB1");

				// Non string typed value test
				TestCaseInt("Int-ModifyExisting", 1, 2, "|NAM=Int-ModifyExisting|NEW=2|OLD=1");
			});
			void TestCase(string fieldName, IZType oldValue, IZType newValue, string expectedResult, string type, CodeDescriptionPairList list = null)
			{
				var bizo = Factory.New<DummyCustomFieldBizo>();
				var createEventRule = new CreateEventRule { EventCode = Events.CustomisableEvent00Code, IsEnabled = true };
				var guid = Guid.NewGuid();
				var args = new CustomAddOnRuleArgs
				{
					FieldName = fieldName,
					OldValue = oldValue,
					NewValue = newValue,
					Type = type,
					Rules =
					[
						new InvalidCodeRule
						{
							List = list ?? new CodeDescriptionPairList()
						}
					]
				};

				var onSet = createEventRule.GetOnSetBehaviour();
				onSet(bizo, null, guid, args);

				var log = bizo.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code).FirstOrDefault();
				AssertNotNull(log);
				AssertEquals(fieldName, expectedResult, log!.SL_Reference);
			}

			void TestCaseString(string fieldName, string oldValue, string newValue, string expectedResult)
			{
				TestCase(fieldName, new ZString(oldValue), new ZString(newValue), expectedResult, AddOnColumnDataType.Codes.String, ruleList);
			}

			void TestCaseInt(string fieldName, int? oldValue, int? newValue, string expectedResult)
			{
				TestCase(fieldName, new ZInt(oldValue), new ZInt(newValue), expectedResult, AddOnColumnDataType.Codes.Integer);
			}
		}

		public void TestReferenceValueFromComboBoxType()
		{
			CombineAssertions(() =>
			{
				TestCase("OldValueIsNull", null, null, new ZString("CMB2"), new ZString("Code 2"), "|NAM=OldValueIsNull|NEW=CMB2:Code 2|OLD=");
				TestCase("OldValueIsEmpty", ZString.Empty, ZString.Empty, new ZString("CMB2"), new ZString("Code 2"), "|NAM=OldValueIsEmpty|NEW=CMB2:Code 2|OLD=");
				TestCase("OldCodeIsNull", null, new ZString("Code 1"), new ZString("CMB2"), new ZString("Code 2"), "|NAM=OldCodeIsNull|NEW=CMB2:Code 2|OLD=Code 1");
				TestCase("OldCodeIsEmpty", ZString.Empty, new ZString("Code 1"), new ZString("CMB2"), new ZString("Code 2"), "|NAM=OldCodeIsEmpty|NEW=CMB2:Code 2|OLD=Code 1");
				TestCase("OldDescIsNull", new ZString("CMB1"), null, new ZString("CMB2"), new ZString("Code 2"), "|NAM=OldDescIsNull|NEW=CMB2:Code 2|OLD=CMB1");
				TestCase("OldDescIsEmpty", new ZString("CMB1"), ZString.Empty, new ZString("CMB2"), new ZString("Code 2"), "|NAM=OldDescIsEmpty|NEW=CMB2:Code 2|OLD=CMB1");
				TestCase("ModifyExisting", new ZString("CMB1"), new ZString("Code 1"), new ZString("CMB2"), new ZString("Code 2"), "|NAM=ModifyExisting|NEW=CMB2:Code 2|OLD=CMB1:Code 1");
				TestCase("SetToDefault", new ZString("CMB1"), new ZString("Code 1"), ZString.Empty, ZString.Empty, "|NAM=SetToDefault|NEW=|OLD=CMB1:Code 1");
			});

			void TestCase(string fieldName, ZString oldValue, ZString oldDesc, ZString newValue, ZString newDesc, string expectedResult)
			{
				var bizo = Factory.New<DummyCustomFieldBizo>();
				var createEventRule = new CreateEventRule { EventCode = Events.CustomisableEvent00Code, IsEnabled = true };
				var onSet = createEventRule.GetOnSetBehaviour();
				var guid = Guid.NewGuid();
				var args = new CustomAddOnRuleArgs
				{
					FieldName = fieldName,
					OldValue = oldValue,
					OldDescription = oldDesc,
					NewValue = newValue,
					NewDescription = newDesc,
					Type = AddOnColumnDataType.Codes.ComboBox
				};

				onSet(bizo, null, guid, args);

				var log = bizo.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code).FirstOrDefault();
				AssertNotNull(log);
				AssertEquals(fieldName, expectedResult, log!.SL_Reference);
			}
		}

		public void TestLogIsDeletedWhenValueIsResetToOriginalValue()
		{
			var stringOriginalValue = new ZString("origValue");
			var stringNewValue = new ZString("newValue");
			var intOriginalValue = new ZInt(21);
			var intNewValue = new ZInt(22);
			var decOriginalValue = new ZDecimal(12.34M);
			var decNewValue = new ZDecimal(55.66M);
			var dateOriginalValue = new ZDateTime(2025, 01, 01, 12, 23, 55);
			var dateNewValue = new ZDateTime(2026, 02, 02, 13, 44, 22);

			CombineAssertions(() =>
			{
				TestCase(AddOnColumnDataType.Codes.String, stringOriginalValue, stringNewValue);
				TestCase(AddOnColumnDataType.Codes.Integer, intOriginalValue, intNewValue);
				TestCase(AddOnColumnDataType.Codes.Decimal, decOriginalValue, decNewValue);
				TestCase(AddOnColumnDataType.Codes.Datetime, dateOriginalValue, dateNewValue);
				TestCase(AddOnColumnDataType.Codes.ComboBox, stringOriginalValue, stringNewValue, stringOriginalValue, stringNewValue);
			});
			void TestCase(string type, IZType oldValue, IZType newValue = null, IZType oldDesc = null, IZType newDesc = null)
			{
				var bizo = Factory.New<DummyCustomFieldBizo>();
				var createEventRule = new CreateEventRule { EventCode = Events.CustomisableEvent00Code, IsEnabled = true };
				var onSet = createEventRule.GetOnSetBehaviour();
				var guid = Guid.NewGuid();
				var args = new CustomAddOnRuleArgs
				{
					FieldName = type,
					OldValue = oldValue,
					OldDescription = oldDesc,
					NewValue = newValue,
					NewDescription = newDesc,
					Type = type
				};

				onSet(bizo, null, guid, args);

				var log = bizo.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code).FirstOrDefault();
				AssertNotNull(log);

				args.NewValue = oldValue;
				args.NewDescription = oldDesc;

				onSet(bizo, null, guid, args);

				log = bizo.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code).FirstOrDefault();
				AssertNull(log);
			}
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	[TestedType(typeof(GenCustomAddOnValue))]
	sealed class GenCustomAddOnValueTest : EnterpriseBusinessObjectTestCase
	{
		public void TestConcurrencyPolicyForXVDataNotInGUI()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var customValue = new BusinessObjectFactory { RefreshEnabled = false }.NewWithValidTestData<GenCustomAddOnValue>();
				customValue.Factory.Save();

				var customValue2 = new BusinessObjectFactory { RefreshEnabled = false }.Load<GenCustomAddOnValue>(customValue.PK);
				customValue2.XV_Data = ZDateTime.Now.AddSeconds(5).ToString();
				customValue2.Factory.Save();

				customValue.XV_Data = ZDateTime.Now.ToString();

				AssertNoExceptionThrown(() => customValue.Factory.Save());
			}
		}

		public void TestConcurrencyPolicyForXVDataInGUI()
		{
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				var customValue = new BusinessObjectFactory { RefreshEnabled = false }.NewWithValidTestData<GenCustomAddOnValue>();
				customValue.Factory.Save();

				var customValue2 = new BusinessObjectFactory { RefreshEnabled = false }.Load<GenCustomAddOnValue>(customValue.PK);
				customValue2.XV_Data = ZDateTime.Now.AddSeconds(5).ToString();
				customValue2.Factory.Save();

				customValue.XV_Data = ZDateTime.Now.ToString();

				AssertExceptionThrown<ZSaveConcurrencyException>(() => customValue.Factory.Save());
			}
		}

		public void TestDeleteOfAddOnValues()
		{
			var bizO = Factory.New<DynamicDummy>();
			bizO.SetUserDefinedValue("Property", new ZString("hello"));
			Factory.Save();
			int count = Factory.GetDatabaseCount(typeof(GenCustomAddOnValue));
			bizO.Delete();
			Factory.Save();
			AssertEquals(count - 1, Factory.GetDatabaseCount(typeof(GenCustomAddOnValue)));
		}

		public void TestGlowCustomAddOnValueType()
		{
			string[] glowAddOnValueTypes = { "BOO", "BYT", "CBO", "DTE", "DAT", "DEC", "GUI", "INT", "SHO", "STR" };
			AssertContainsExactElementsInAnyOrder("Please update the AddOnValueType.cs in glow", glowAddOnValueTypes, new AddOnColumnDataType().GetAllCodes());
		}

		public void TestCustomColumnDefinitionProvider_DoesntAccessDeletedProperties()
		{
			var value = Factory.New<GenCustomAddOnValue>();
			value.XV_Name = "Test";
			value.XV_ParentID = ZGuid.NewZGuid();
			value.XV_ParentTableCode = "**";
			value.XV_Data = "AAA";
			value.XV_Type = "STR";

			var rule = Factory.NewWithValidTestData<GenCustomAddOnRule>();
			const string sourceCodeXML = @"<sourceCode>
  <rules>
    <rule code=""InvalidCode"" enabled=""false"">
      <details>
        <codeDescriptionList>
        </codeDescriptionList>
      </details>
    </rule>
    <rule code=""CreateEvent"" enabled=""false"">
      <details>
        <CreateEventRuleCode>Z00</CreateEventRuleCode>
      </details>
    </rule>
    <rule code=""DateTimeFormat"" enabled=""false"">
      <details>
        <format>Short</format>
      </details>
    </rule>
    <rule code=""CheckEntered"" enabled=""false"">
      <details />
    </rule>
  </rules>
</sourceCode>";
			rule.XR_SourceCode = sourceCodeXML;
			value.XV_XR_Rule = rule.PK;
			Factory.Save();

			var columnDefinition = value.GetCustomColumnDefinition();

			value.Delete();

			AssertEquals("Test", columnDefinition.Name);
			AssertEquals("Test", columnDefinition.NameLocalized);
			AssertEquals(rule.PK, columnDefinition.RuleDefinitionReference);
			AssertEquals(null, columnDefinition.Sequence);
			AssertEquals("STR", columnDefinition.Type);
			AssertEquals(GenCustomAddOnValue.Schema.XV_DataMaxLength, columnDefinition.MaxLength);
			AssertEquals(4, columnDefinition.GetRules().Length);
		}

		public void TestUniqueValueOnParentUniqueIndexFailureHandler()
		{
			var customValueInDb = new BusinessObjectFactory { RefreshEnabled = false }.New<GenCustomAddOnValue>();
			customValueInDb.XV_Name = "Test Custom V X01";
			customValueInDb.XV_ParentID = ZGuid.NewZGuid();
			customValueInDb.XV_ParentTableCode = "**";
			customValueInDb.XV_Data = "AAA";
			customValueInDb.XV_Type = "STR";
			customValueInDb.Factory.Save();

			AssertCustomValue(customValueInDb.XV_Name, customValueInDb.XV_ParentID, customValueInDb.XV_ParentTableCode, "AAA");

			var newCustomValue = Factory.New<GenCustomAddOnValue>();
			newCustomValue.XV_Name = customValueInDb.XV_Name;
			newCustomValue.XV_ParentID = customValueInDb.XV_ParentID;
			newCustomValue.XV_ParentTableCode = customValueInDb.XV_ParentTableCode;
			newCustomValue.XV_Data = "BBB";
			newCustomValue.XV_Type = "STR";

			ZExceptionReporting.ProcessWithSaveExceptionHandling(Factory.Save, null);

			AssertCustomValue(customValueInDb.XV_Name, customValueInDb.XV_ParentID, customValueInDb.XV_ParentTableCode, "BBB");
			AssertEquals("You changes to custom field 'Test Custom V X01' conflict with changes made by other user. If you want to override value entered by other user, press OK and try to save again. Otherwise cancel your changes and reopen this form to load already existing value.",
				UnitTestUserNotification.Instance.LastMessage.Text);
		}

		void AssertCustomValue(ZString name, ZGuid parentID, ZString parentTableCode, ZString expectedValue)
		{
			var query = new ZQuery(GenCustomAddOnValueSchema.XV_Name, name);
			query.AddToFilter(GenCustomAddOnValueSchema.XV_ParentID, parentID);
			query.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, parentTableCode);

			var customValue = new BusinessObjectFactory { RefreshEnabled = false }.LoadTop1<GenCustomAddOnValue>(query);
			if (customValue != null)
			{
				AssertEquals(expectedValue, customValue.XV_Data);
			}
			else if (!expectedValue.IsEmpty)
			{
				Fail("Custom value was not found.");
			}
		}

		public void TestIsSavedByFactory()
		{
			var customValue = Factory.New<GenCustomAddOnValue>();
			Assert("Should not save non-initialized custom value", !customValue.IsSavedByFactory);

			customValue.XV_Name = ZString.Empty;
			customValue.XV_Type = "STR";
			Assert("Should not save custom value with empty name", !customValue.IsSavedByFactory);

			customValue.XV_Name = "Field 1";
			customValue.XV_Type = ZString.Empty;
			Assert("Should not save custom value with empty type", !customValue.IsSavedByFactory);

			customValue.XV_Type = "STR";
			Assert("Name and type initialized - should save", customValue.IsSavedByFactory);

			customValue.XV_Name = ZString.Empty;
			customValue.XV_Type = ZString.Empty;
			Assert("Should not save non-initialized custom value", !customValue.IsSavedByFactory);

			customValue.Delete();
			Assert("Should save deleted record", customValue.IsSavedByFactory);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var customValue = (GenCustomAddOnValue)base.GetNewBusinessObjectForDeleteTest(factory);
			customValue.XV_Name = "Field 1";
			customValue.XV_Type = "STR";
			return customValue;
		}

		#endregion
	}
}

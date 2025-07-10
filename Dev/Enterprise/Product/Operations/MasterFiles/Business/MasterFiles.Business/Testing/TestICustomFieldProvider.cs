#if DEBUG
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(ICustomFieldProvider), typeof(TestExcludeBusinessObjectsAllHaveTestCasesAttribute), RequireTestOnlyInFirstSubLevel = true)]
	public abstract class TestICustomFieldProvider : TestCaseWithFactory
	{
		protected virtual BusinessObject GetBizo()
		{
			return Factory.New(TestedTypeHelper.GetTestedType(GetType()));
		}

		public void TestCustomFields()
		{
			var bizobj = GetBizo();
			bizobj.SetUserDefinedValue("CustomFieldString", (ZString)"Enquiry or Inquiry Preference");
			bizobj.SetUserDefinedValue("CustomFieldInt", (ZInt)1234567);
			bizobj.SetUserDefinedValue("CustomFieldBool", ZBool.True);
			bizobj.SetUserDefinedValue("CustomFieldDatetime", new ZDateTime(2013, 11, 27));

			AssertEquals((ZString)"Enquiry or Inquiry Preference", ((ICustomFieldProvider)bizobj).GetCustomField("CustomFieldString", AddOnColumnDataType.Codes.String));
			AssertEquals((ZInt)1234567, ((ICustomFieldProvider)bizobj).GetCustomField("CustomFieldInt", AddOnColumnDataType.Codes.Integer));
			AssertEquals(ZBool.True, ((ICustomFieldProvider)bizobj).GetCustomField("CustomFieldBool", AddOnColumnDataType.Codes.Boolean));
			AssertEquals(new ZDateTime(2013, 11, 27), ((ICustomFieldProvider)bizobj).GetCustomField("CustomFieldDatetime", AddOnColumnDataType.Codes.Datetime));
		}

		public void TestCustomFields_Deleted()
		{
			var bizo = GetBizo();
			var customFieldProvider = bizo as ICustomFieldProvider;
			bizo.SetUserDefinedValue("CustomFieldString", (ZString)"TEST");
			var customBizo = customFieldProvider.GetCustomBusinessObject();
			var addOnValues = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, bizo.PK));
			AssertEquals(1, addOnValues.Length);

			bizo.SetUserDefinedValue("CustomFieldString", ZString.Empty);//This deletes the GenCustomAddOnValue
			addOnValues = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, bizo.PK));
			AssertEquals(0, addOnValues.Length);
			AssertEquals("The property value has been deleted but the property definition should still exist", 1, (customBizo as ICustomPropertyContainer).CustomProperties.Count());

			var propertyName = CustomPropertyHelper.GeneratePropertyIdentifier("CustomFieldString", typeof(ZString));
			AssertNoExceptionThrown("Can safey set the field", () => customBizo[propertyName] = "blah");

			bizo.SetUserDefinedValue("CustomFieldString", (ZString)"TEST");
			AssertEquals((ZString)"TEST", customFieldProvider.GetCustomField("CustomFieldString", AddOnColumnDataType.Codes.String));
		}

		public void TestCustomFieldsWithRulesFetchHints()
		{
			var bizobj = GetBizo();
			var factory = bizobj.Factory;
			factory.RefreshEnabled = false;
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			//make a bunch of rules

			var rules = new GenCustomAddOnRule[10];

			for (var i = 0; i < 10; ++i)
			{
				var rule = factory2.NewWithValidTestData<GenCustomAddOnRule>();
				rules[i] = rule;
				rule.XR_IsActive = true;
				rule.XR_Code = "R" + i;
				rule.XR_SourceCode = @"<sourceCode><rules><rule code=""InvalidCode""><details><codeDescriptionList><codeDescription code=""abc"" description =""abcd"" /><codeDescription code=""123"" description =""123"" /></codeDescriptionList></details></rule></rules></sourceCode>";
			}

			//make a bunch of columns that use 'em

			for (var i = 0; i < 10; ++i)
			{
				var columndef = factory.NewWithValidTestData<GenCustomAddOnValue>();
				columndef.XV_Name = "V" + i;
				columndef.XV_Type = "STR";
				columndef.XV_Data = "" + i;
				columndef.XV_IsRuleEnabled = true;
				columndef.XV_ParentTableCode = bizobj.TablePrefix;
				columndef.XV_ParentID = bizobj.PK;
				columndef.XV_XR_Rule = rules[i].PK;
			}

			//testing template fetch hints
			var factory3 = new BusinessObjectFactory() { RefreshEnabled = false };
			var bizo3 = GetBizo();

			var template = factory3.New<ProcessTaskTemplate>();

			for (var i = 0; i < 10; ++i)
			{
				var column = template.GenCustomColumnDefinitions.AddNew();
				column.XC_Name = "UserText" + i;
				column.XC_Type = AddOnColumnDataType.Codes.String;
				column.XC_XR = rules[i].PK;
			}

			//save rules - we can't save them sooner or else validation will have loaded them by now
			factory2.Save();

			var initialcount = factory.GetTableHitCount("GenCustomAddOnRule");

			AssertEquals((ZString)"0", ((ICustomFieldProvider)bizobj).GetCustomField("V0", AddOnColumnDataType.Codes.String));

			AssertEquals(initialcount + 1, factory.GetTableHitCount("GenCustomAddOnRule"));

			initialcount = factory3.GetTableHitCount("GenCustomAddOnRule");

			var properties = new UserDefinedPropertyCollection(bizo3) { new ProcessTaskTemplateMatches(template) };

			AssertEquals(initialcount, factory3.GetTableHitCount("GenCustomAddOnRule"));
		}
	}
}
#endif

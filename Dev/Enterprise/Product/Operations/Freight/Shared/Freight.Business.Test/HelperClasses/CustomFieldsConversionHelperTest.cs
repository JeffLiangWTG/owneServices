using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CustomFieldsConversionHelperTest : TestCaseWithFactory
	{
		public void TestMoveCustomFields()
		{
			AssertNoExceptionThrown(() => CustomFieldsConversionHelper.MoveCustomFields(null, null));
			AssertNoExceptionThrown(() => CustomFieldsConversionHelper.MoveCustomFields(Factory.New<DummyBusinessObject>(), Factory.New<OtherDummyBizObj>()));

			var taskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			taskTemplate.P0_ProcessType = "XXX";

			var customField1 = taskTemplate.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "custom1";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;

			var customField2 = taskTemplate.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "custom2";
			customField2.XC_Type = AddOnColumnDataType.Codes.Integer;

			var dummy = Factory.New<DummyBusinessObject>();

			var customAddOnValue1 = AddCustomValue(dummy, customField1, "aaa");
			var customAddOnValue2 = AddCustomValue(dummy, customField1, "333");

			var otherDummyBizObj = Factory.New<OtherDummyBizObj>();

			AssertCustomValues(dummy, new[] { customAddOnValue1, customAddOnValue2 });
			AssertCustomValues(otherDummyBizObj, System.Array.Empty<GenCustomAddOnValue>());

			CustomFieldsConversionHelper.MoveCustomFields(dummy, otherDummyBizObj);

			AssertCustomValues(dummy, System.Array.Empty<GenCustomAddOnValue>());
			AssertCustomValues(otherDummyBizObj, new[] { customAddOnValue1, customAddOnValue2 });

			foreach (GenCustomAddOnValue customAddOnValue in Factory.Load<GenCustomAddOnValue>(new ZQuery()))
			{
				customAddOnValue.Delete();
			}

			customAddOnValue1 = AddCustomValue(dummy, customField1, "aaa");
			customAddOnValue2 = AddCustomValue(dummy, customField1, "333");

			CustomFieldsConversionHelper.MoveCustomFields(otherDummyBizObj.Factory, dummy.TablePrefix, dummy.PK, otherDummyBizObj.TablePrefix, otherDummyBizObj.PK);

			AssertCustomValues(dummy, System.Array.Empty<GenCustomAddOnValue>());
			AssertCustomValues(otherDummyBizObj, new[] { customAddOnValue1, customAddOnValue2 });
		}

		public void TestCopyCustomFields()
		{
			AssertNoExceptionThrown(() => CustomFieldsConversionHelper.CopyCustomFields(null, null));
			AssertNoExceptionThrown(() => CustomFieldsConversionHelper.CopyCustomFields(Factory.New<DummyBusinessObject>(), Factory.New<OtherDummyBizObj>()));

			var taskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			taskTemplate.P0_ProcessType = "XXX";

			var customField = taskTemplate.GenCustomColumnDefinitions.AddNew();
			customField.XC_Name = "custom1";
			customField.XC_Type = AddOnColumnDataType.Codes.String;

			var dummy = Factory.New<DummyBusinessObject>();
			var otherDummyBizObj = Factory.New<OtherDummyBizObj>();

			var customAddOnValue = AddCustomValue(dummy, customField, "aaa");

			AssertCustomValues(dummy, new[] { customAddOnValue });
			AssertCustomValues(otherDummyBizObj, System.Array.Empty<GenCustomAddOnValue>());

			CustomFieldsConversionHelper.CopyCustomFields(dummy, otherDummyBizObj);

			AssertCustomValues(dummy, new[] { customAddOnValue });
			AssertCopiedCustomValues(otherDummyBizObj, dummy);

			foreach (GenCustomAddOnValue value in Factory.Load<GenCustomAddOnValue>(new ZQuery()))
			{
				value.Delete();
			}

			customAddOnValue = AddCustomValue(dummy, customField, "aaa");

			CustomFieldsConversionHelper.CopyCustomFields(otherDummyBizObj.Factory, dummy.TablePrefix, dummy.PK, otherDummyBizObj.TablePrefix, otherDummyBizObj.PK);

			AssertCustomValues(dummy, new[] { customAddOnValue });
			AssertCopiedCustomValues(otherDummyBizObj, dummy);
		}

		public void TestCopyCustomFields_Duplicates()
		{
			var taskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			taskTemplate.P0_ProcessType = "XXX";

			var customField1 = taskTemplate.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "custom1";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;

			var dummy = Factory.New<DummyBusinessObject>();
			var otherDummy = Factory.New<OtherDummyBizObj>();
			var customAddOnValue1 = AddCustomValue(dummy, customField1, "aaa");
			var customAddOnValue2 = AddCustomValue(otherDummy, customField1, "bbb");

			AssertCustomValues(otherDummy, new[] { customAddOnValue2 });

			CustomFieldsConversionHelper.CopyCustomFields(dummy, otherDummy);

			AssertCustomValues(otherDummy, new[] { customAddOnValue2 });
		}

		#region Implementation

		GenCustomAddOnValue AddCustomValue(BusinessObject parent, GenCustomColumnDefinition definition, ZString value)
		{
			GenCustomAddOnValue customAddOnValue = Factory.New<GenCustomAddOnValue>();
			customAddOnValue.XV_ParentID = parent.PK;
			customAddOnValue.XV_ParentTableCode = parent.TablePrefix;
			customAddOnValue.XV_Name = definition.XC_Name;
			customAddOnValue.XV_Type = definition.XC_Type;
			customAddOnValue.XV_Data = value;

			return customAddOnValue;
		}

		void AssertCustomValues(BusinessObject parent, GenCustomAddOnValue[] expectedValues)
		{
			ZQuery query = new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, parent.PK);
			query.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, parent.TablePrefix);

			AssertContainsExactElementsInAnyOrder(expectedValues, Factory.Load<GenCustomAddOnValue>(query));
		}

		void AssertCopiedCustomValues(BusinessObject parent, BusinessObject otherParent)
		{
			ZQuery query = new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, parent.PK);
			query.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, parent.TablePrefix);
			var values = Factory.Load<GenCustomAddOnValue>(query);

			query = new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, otherParent.PK);
			query.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, otherParent.TablePrefix);
			var otherValues = Factory.Load<GenCustomAddOnValue>(query);

			AssertEquals(1, values.Length);
			AssertEquals(1, otherValues.Length);
			CustomFieldTestHelper.AssertDifferentCustomFieldsButSameValues(values[0], otherValues[0]);
		}

		class OtherDummyBizObj : DummyBusinessObject
		{
			public OtherDummyBizObj(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override string TablePrefix
			{
				get { return "XX"; }
			}
		}

		#endregion
	}
}

using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class CustomFieldsDataObjectWritingHelperTest : TestCaseWithFactory
	{
		public void TestPopulateUserDefinedValues()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "kilimandzaro";
			dummy.Z0_Decimal = 456m;
			dummy.Z0_Date = new ZDateTime(2012, 7, 25);
			dummy.Z0_Bool = true;
			dummy.Z0_Code = "XXX";
			dummy.Z0_Number = 123;

			var customFieldInfos = new List<CustomFieldInfo>
			{
				GetCustomFieldInfo<ZString>("Description", DummyBizoSchema.Z0_Description.Name),
				GetCustomFieldInfo<ZDecimal>("Decimal", DummyBizoSchema.Z0_Decimal.Name),
				GetCustomFieldInfo<ZDateTime>("Date", DummyBizoSchema.Z0_Date.Name),
				GetCustomFieldInfo<ZBool>("Bool", DummyBizoSchema.Z0_Bool.Name),
				GetCustomFieldInfo<ZString>("Code", DummyBizoSchema.Z0_Code.Name),
				GetCustomFieldInfo<ZInt>("Number", DummyBizoSchema.Z0_Number.Name)
			};

			var customizedFieldDescriptor = new DummyCustomFieldsDescriptor(customFieldInfos);
			var helper = new CustomFieldsDataObjectWritingHelper<DummyBusinessObject>(dummy, customizedFieldDescriptor);

			AssertContainsExactElementsInAnyOrder("exported IPropertyValue values ",
			new[]
			{
				"Description|kilimandzaro",
				"Decimal|456",
				"Date|25-Jul-12 00:00:00",
				"Bool|Y",
				"Code|XXX",
				"Number|123"
			},
			helper.GetUserDefinedValues().Select(FormatPropertyValue));

			AssertContainsExactElementsInAnyOrder("exported CustomizedField values ",
			new[]
			{
				"Description|kilimandzaro",
				"Decimal|456",
				"Date|2012-07-25T00:00:00",
				"Bool|true",
				"Code|XXX",
				"Number|123"
			},
			helper.GetCustomizedFieldValues().Select(FormatPropertyValue));
		}

		public void TestPopulateUserDefinedValues_SkipInactive()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "kilimandzaro";
			dummy.Z0_Decimal = 456m;

			var customFieldInfos = new List<CustomFieldInfo>
			{
				GetCustomFieldInfo<ZString>("Description", DummyBizoSchema.Z0_Description.Name, false),
				GetCustomFieldInfo<ZDecimal>("Decimal", DummyBizoSchema.Z0_Decimal.Name)
			};

			var customizedFieldDescriptor = new DummyCustomFieldsDescriptor(customFieldInfos);
			var helper = new CustomFieldsDataObjectWritingHelper<DummyBusinessObject>(dummy, customizedFieldDescriptor);

			AssertContainsExactElementsInAnyOrder("exported IPropertyValue values ",
			new[]
			{
				"Decimal|456"
			},
			helper.GetUserDefinedValues().Select(FormatPropertyValue));

			AssertContainsExactElementsInAnyOrder("exported CustomizedField values ",
			new[]
			{
				"Decimal|456"
			},
			helper.GetCustomizedFieldValues().Select(FormatPropertyValue));
		}

		#region Implementation

		string FormatPropertyValue(IPropertyValue propertyValue)
		{
			return string.Format("{0}|{1}", propertyValue.PropertyName, propertyValue.Value);
		}

		string FormatPropertyValue(CustomizedField customizedField)
		{
			return string.Format("{0}|{1}", customizedField.Key, customizedField.Value);
		}

		CustomFieldInfo GetCustomFieldInfo<T>(string caption, string schemaColumnName, bool isActive = true) where T : IZType
		{
			return CustomFieldInfo.New<T>(isActive,
				caption,
				string.Empty,
				schemaColumnName);
		}

		#endregion
	}
}

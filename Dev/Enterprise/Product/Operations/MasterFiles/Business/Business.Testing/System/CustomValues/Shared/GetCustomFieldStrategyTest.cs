using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	sealed class GetCustomFieldStrategyTest : TestCaseWithFactory
	{
		#region TestGetCustomField

		public void TestGetCustomField()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			var collection = new GenCustomColumnDefinitionCollection(template);
			collection.AddNew();

			var lookups = new GenCustomColumnDefinitionLookups(collection[0]);
			var types = lookups.Types;
			AssertEquals("6 types should be supported by CustomFields", 6, types.Count);
			Assert(types.ContainsCode(AddOnColumnDataType.Codes.Boolean));
			Assert(types.ContainsCode(AddOnColumnDataType.Codes.ComboBox));
			Assert(types.ContainsCode(AddOnColumnDataType.Codes.Datetime));
			Assert(types.ContainsCode(AddOnColumnDataType.Codes.Decimal));
			Assert(types.ContainsCode(AddOnColumnDataType.Codes.Integer));
			Assert(types.ContainsCode(AddOnColumnDataType.Codes.String));

			var aBool = ZBool.True;
			var aDateTime = ZDateTime.Now;
			var aDecimal = (ZDecimal)42.19;
			var aInt = (ZInt)27;
			var aString = (ZString)"Hello World";

			var dummy = Factory.New<UserDefinableDummyBusinessObject>();
			dummy.SetUserDefinedValue("Custom Bool", aBool);
			dummy.SetUserDefinedValue("Custom Datetime", aDateTime);
			dummy.SetUserDefinedValue("Custom Decimal", aDecimal);
			dummy.SetUserDefinedValue("Custom Integer", aInt);
			dummy.SetUserDefinedValue("Custom String", aString);

			var strategy = new GetCustomFieldStrategy(dummy);

			AssertEquals("strategy.GetCustomField(\"Custom Bool\")", aBool, strategy.GetCustomField("Custom Bool"));
			AssertEquals("strategy.GetCustomField(\"Custom Datetime\")", aDateTime, strategy.GetCustomField("Custom Datetime"));
			AssertEquals("strategy.GetCustomField(\"Custom Decimal\")", aDecimal, strategy.GetCustomField("Custom Decimal"));
			AssertEquals("strategy.GetCustomField(\"Custom Integer\")", aInt, strategy.GetCustomField("Custom Integer"));
			AssertEquals("strategy.GetCustomField(\"Custom String\")", aString, strategy.GetCustomField("Custom String"));
		}

		public void TestGetCustomFieldWhenDeleted()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			var collection = new GenCustomColumnDefinitionCollection(template);
			collection.AddNew();

			var theString = (ZString)"Hello World";

			var dummy = Factory.New<UserDefinableDummyBusinessObject>();
			dummy.SetUserDefinedValue("Custom String", theString);

			collection.DeleteAll();
			dummy.Delete();

			var strategy = new GetCustomFieldStrategy(dummy);

			AssertEquals("strategy.GetCustomField(\"Custom String\")", ZString.Empty, strategy.GetCustomField("Custom String"));
		}

		[UserDefinedValues]
		class UserDefinableDummyBusinessObject : DummyBusinessObject
		{
			public UserDefinableDummyBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		#endregion

		#region TestGetCustomProperty

		public void TestGetCustomProperty()
		{
			var dummy = Factory.New<UserDefinableDummyBusinessObject>();

			dummy.SetUserDefinedValue("Custom Text", (ZString)"Hello World");
			dummy.SetUserDefinedValue("Custom Number", (ZInt)28);

			var strategy = new GetCustomFieldStrategy(dummy);
			CustomBusinessObject customBizo;

			AssertNotNull(strategy.GetCustomProperty("Custom Text", null, out customBizo));
			AssertEquals("Hello World", strategy.GetCustomProperty("Custom Text", null, out customBizo).GetValue(dummy));
			AssertNotNull(strategy.GetCustomProperty("Custom Number", null, out customBizo));
			AssertEquals(28, strategy.GetCustomProperty("Custom Number", null, out customBizo).GetValue(dummy));
			AssertNull(strategy.GetCustomProperty("Custom Field 3", null, out customBizo));
		}

		#endregion

		#region TestGetCustomFiledCodeDescription

		public void TestGetCustomFiledCodeDescription()
		{
			var dummy = Factory.New<DummyBizoWithCustomBizo>();

			var strategy = new GetCustomFieldStrategy(dummy);

			dummy.SetUserDefinedValue(DummyBizoWithCustomBizo.CustomFieldName, null, (ZString)"Hello World");
			AssertEquals("Hello World", strategy.GetCustomFieldCodeDescription(DummyBizoWithCustomBizo.CustomFieldName));

			dummy.SetUserDefinedValue(DummyBizoWithCustomBizo.CustomFieldName, null, (ZString)"Bye World");
			AssertEquals("Bye World", strategy.GetCustomFieldCodeDescription(DummyBizoWithCustomBizo.CustomFieldName));

			dummy.SetUserDefinedValue(DummyBizoWithCustomBizo.CustomFieldName, null, (ZString)"ZZZ");
			AssertNotNull(strategy.GetCustomFieldCodeDescription(DummyBizoWithCustomBizo.CustomFieldName));
		}

		[UserDefinedValues]
		class DummyBizoWithCustomBizo : DummyBusinessObject
		{
			public const string CustomFieldName = "Custom Property";

			public DummyBizoWithCustomBizo(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public CustomBusinessObject GetCustomBusinessObject()
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddPair("XXX", "Hello World");
				list.AddPair("YYY", "Bye World");

				var values = new Dictionary<string, object>();

				var propertyCollection = new CustomPropertyCollectionImpl(
					propertyName =>
					{
						object value;
						return values.TryGetValue(propertyName, out value) ? value : null;
					},
					(propertyName, value) =>
					{
						values[propertyName] = value;
						return true;
					})
				{
					{ typeof(string), CustomFieldName, new DynamicMetaData[] { DynamicMetaData.Description(new Description { Caption = CustomFieldName }), DynamicMetaData.ListDataSource(list) } }
				};

				return new CustomBusinessObject(Factory, this, propertyCollection);
			}

			public IZType GetCustomField(string fieldName)
			{
				return this.GetUserDefinedValue<ZString>(CustomFieldName);
			}

			class Description : IDescription
			{
				public string Caption { get; set; }

				public string GetDescription(int index)
				{
					return Caption;
				}

				public string GetDescription(int index, CultureInfo culture)
				{
					return Caption;
				}

				public int Count
				{
					get { return 1; }
				}
			}
		}

		#endregion
	}
}

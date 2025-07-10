using System.Collections.Generic;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(NumberRangeByUnitFilter))]
	public class NumberRangeByUnitFilterTest : ModuleFilterTestCase<NumberRangeByUnitFilter>
	{
		#region TestProperty1Validation

		public void TestProperty1Validation()
		{
			string errorText = "Property should have this error message!";
			Filter.Property1Validation = null;
			Filter.Property1 = 0m;

			Filter.Validation.ValidateProperty1();
			AssertNoError(Filter.Property1Info, errorText);

			Filter.Property1Validation = delegate(ZPropertyInfo info)
			{
				if (info.Value.Equals(0m))
				{
					info.AddError(errorText);
				}
			};

			Filter.Validation.ValidateProperty1();
			AssertHasError(Filter.Property1Info, errorText);
		}

		#endregion

		#region TestProperty2Validation

		public void TestProperty2Validation()
		{
			string errorText = "Property should have this error message!";
			Filter.Property2Validation = null;
			Filter.Property2 = 0m;

			Filter.Validation.ValidateProperty2();
			AssertNoError(Filter.Property2Info, errorText);

			Filter.Property2Validation = delegate(ZPropertyInfo info)
			{
				if (info.Value.Equals(0m))
				{
					info.AddError(errorText);
				}
			};

			Filter.Validation.ValidateProperty2();
			AssertHasError(Filter.Property2Info, errorText);
		}

		#endregion

		#region TestQueryIsEmptyByDefault

		public override void TestQueryIsEmptyByDefault()
		{
			AssertEquals("ModuleNumberRangeFilter.Query is never empty (as the default value of 0 has meaning).", false, Filter.IsEmpty);
		}

		#endregion

		#region TestDeserializePropertiesFromXml

		public void TestDeserializePropertiesFromXml()
		{
			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter = new DummyModuleNumberRangeByUnitFilter(GetWeightUnitRangeQuery);

			filter.Property1 = (ZDecimal)1;
			filter.Property2 = (ZDecimal)2;
			filter.Property = "KG";

			filterStripBizO.AddModuleFilterForTest(filter);

			FilterStrip strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;

			var savedFilter = SaveLayout(filterStripBizO, "savedFilter");

			strip.FilterDescription = "";
			strip.Delete();

			filterStripBizO.LoadLayout(savedFilter);

			var loadedFilter = (NumberRangeByUnitFilter)filterStripBizO[filter.Description];

			AssertEquals((ZDecimal)0, loadedFilter.Property1);
			AssertEquals((ZDecimal)2, loadedFilter.Property2);
			AssertEquals("", loadedFilter.Property);
		}

		StmModuleFilter SaveLayout(DummyFilterStripBusinessObject filterStripBizO, ZString layoutName, bool global = false)
		{
			StmModuleFilter result = null;
			if (!layoutName.IsEmpty && filterStripBizO.FilterStrips.Count > 0)
			{
				result = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, layoutName, false, global, SaveColumnLayout.Ignore);
			}
			return result;
		}

		#endregion

		#region Implementation

		ZQuery GetWeightUnitRangeQuery(INumericZType value1, INumericZType value2, ZString unit)
		{
			var result = new ZDBOnlyQuery(typeof(WhsDocket));
			return result;
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.TextSearch; }
		}

		protected override NumberRangeByUnitFilter GetNewModuleFilter()
		{
			return new NumberRangeByUnitFilter("moo", GetWeightUnitRangeQuery, new CodeDescriptionPairList());
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override Dictionary<string, IZType> GetDummyValuesForCacheInvalidationTest(NumberRangeByUnitFilter filter)
		{
			var values = base.GetDummyValuesForCacheInvalidationTest(filter);
			values.Add(nameof(filter.Property1), new ZDecimal(10m));
			values.Add(nameof(filter.Property2), new ZDecimal(20m));

			return values;
		}

		#endregion

		#region DummyModuleNumberRangeFilter

		public class DummyModuleNumberRangeByUnitFilter : NumberRangeByUnitFilter
		{
			public DummyModuleNumberRangeByUnitFilter(GetUnitRangeQuery queryDelegate)
				: base("DummyModuleNumberRangeByUnitFilter", queryDelegate, new CodeDescriptionPairList())
			{
			}

			protected override void SerializePropertiesToXml(XmlWriter writer)
			{
				writer.WriteElementString("Property2", Property2.ToString());
			}
		}

		#endregion
	}
}

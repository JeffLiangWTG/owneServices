using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCVisaTariffFilterStripBusinessObject))]
	sealed class USCVisaTariffFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestTextileCategoryNumberQuery()
		{
			var filter = new USCVisaTariffFilterStripBusinessObject();
			var textileCategoryNumberFilter = (ModuleTextFilter)filter[USCVisaTariffFilterStripBusinessObject.Schema.TextileCategoryNumber];
			textileCategoryNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			textileCategoryNumberFilter.Property = "111";
			textileCategoryNumberFilter.IsActive = true;
			var coll = new USCVisaTariffNonDependentCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals(2, coll.Count);
			var tariffNumberFilter = (ModuleTextFilter)filter[USCVisaTariffFilterStripBusinessObject.Schema.TariffNo];
			tariffNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			tariffNumberFilter.Property = "6205424211";
			tariffNumberFilter.IsActive = true;
			coll = new USCVisaTariffNonDependentCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(tariff1, coll[0]);
		}

		public void TestCountryOfOriginQuery()
		{
			var filter = new USCVisaTariffFilterStripBusinessObject();
			var coFilter = (ModuleTextFilter)filter[USCVisaTariffFilterStripBusinessObject.Schema.CountryOfOrigin];
			coFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			coFilter.Property = "KR";
			coFilter.IsActive = true;
			var coll = new USCVisaTariffNonDependentCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals(2, coll.Count);
			var tariffNumberFilter = (ModuleTextFilter)filter[USCVisaTariffFilterStripBusinessObject.Schema.TariffNo];
			tariffNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			tariffNumberFilter.Property = "6205424200";
			tariffNumberFilter.IsActive = true;
			coll = new USCVisaTariffNonDependentCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(tariff4, coll[0]);
		}

		public void TestFilters()
		{
			var filter = new USCVisaTariffFilterStripBusinessObject();
			AssertNotNull(filter[USCVisaTariffFilterStripBusinessObject.Schema.TariffNo]);
			AssertNotNull(filter[USCVisaTariffFilterStripBusinessObject.Schema.TextileCategoryNumber]);
			AssertNotNull(filter[USCVisaTariffFilterStripBusinessObject.Schema.CountryOfOrigin]);
			AssertNotNull(filter[USCVisaTariffFilterStripBusinessObject.Schema.VisaBeginningDate]);
			AssertNotNull(filter[USCVisaTariffFilterStripBusinessObject.Schema.VisaEndingDate]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new USCVisaTariffFilterStripBusinessObject();

		USCVisa visa1;
		USCVisaTariff tariff1;
		USCVisa visa2;
		USCVisaTariff tariff4;
		protected override void SetUp()
		{
			base.SetUp();
			visa1 = Factory.New<USCVisa>();
			visa1.UO_TextileCategoryNo = "111";
			visa1.UO_UC_NKOriginCountry = "AU";
			visa1.UO_BeginDate = new ZDateTime(2001, 1, 1);
			visa1.UO_EndDate = new ZDateTime(2001, 12, 31);
			tariff1 = visa1.Tariffs.GetOrCreateFor("6205424211");
			visa1.Tariffs.GetOrCreateFor("6205424200");
			visa2 = Factory.New<USCVisa>();
			visa2.UO_TextileCategoryNo = "222";
			visa2.UO_UC_NKOriginCountry = "KR";
			visa2.UO_BeginDate = new ZDateTime(2001, 1, 1);
			visa2.UO_EndDate = new ZDateTime(2001, 12, 31);
			visa2.Tariffs.GetOrCreateFor("6205424211");
			tariff4 = visa2.Tariffs.GetOrCreateFor("6205424200");
			Factory.Save();
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CusCalculationRulesFilterBusinessObject))]
	public class CusCalculationRulesFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestEffectiveDate()
		{
			BusinessObject[] filteredDecs = null;
			var cusCalculationRule1 = Factory.NewWithValidTestData<CusCalculationRule>();
			cusCalculationRule1.CCR_GC_Company = GlbCompany.CurrentCompany.PK;
			cusCalculationRule1.CCR_StartDate = ZDateTimeOffset.Today.AddDays(-1);
			cusCalculationRule1.CCR_EndDate = ZDateTimeOffset.Today.AddDays(1);
			var cusCalculationRule2 = Factory.NewWithValidTestData<CusCalculationRule>();
			cusCalculationRule2.CCR_GC_Company = GlbCompany.CurrentCompany.PK;
			cusCalculationRule2.CCR_StartDate = ZDateTimeOffset.Today.AddDays(2);
			cusCalculationRule2.CCR_EndDate = ZDateTimeOffset.Today.AddDays(3);
			Factory.Save();
			var filter = (ModuleSingleDateFilter)filterBO[CusCalculationRulesFilterBusinessObject.FilterConstants.EffectiveDate];
			filter.IsActive = true;
			CombineAssertions(() =>
			{
				AssertEquals(FilterCategories.Dates, filter.Category);
				filter.Property1 = ZDate.Today;
				filteredDecs = Factory.Load(typeof(CusCalculationRule), filterBO.Filter);
				AssertEquals("Filter In", 1, filteredDecs.Length);
				AssertEquals("Filter In", cusCalculationRule1, filteredDecs[0]);
				filter.Property1 = ZDate.BrettsBirthday;
				filteredDecs = Factory.Load(typeof(CusCalculationRule), filterBO.Filter);
				AssertEquals("Filter Out", 0, filteredDecs.Length);
			});
		}

		public void TestRuleType()
		{
			BusinessObject[] filteredDecs = null;
			var cusCalculationRule1 = Factory.NewWithValidTestData<CusCalculationRule>();
			cusCalculationRule1.CCR_RuleType = "INS";
			cusCalculationRule1.CCR_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			var filter = (ModuleTextFilter)filterBO[CusCalculationRulesFilterBusinessObject.FilterConstants.RuleType];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			CombineAssertions(() =>
			{
				AssertEquals(FilterCategories.ModesAndTypes, filter.Category);
				filter.Property = "INS";
				filteredDecs = Factory.Load(typeof(CusCalculationRule), filterBO.Filter);
				AssertEquals("Filter In", 1, filteredDecs.Length);
				AssertEquals("Filter In", cusCalculationRule1, filteredDecs[0]);
				filter.Property = "111";
				filteredDecs = Factory.Load(typeof(CusCalculationRule), filterBO.Filter);
				AssertEquals("Filter Out", 0, filteredDecs.Length);
			});
		}

		public void TestTransportMode()
		{
			BusinessObject[] filteredDecs = null;
			var cusCalculationRule1 = Factory.NewWithValidTestData<CusCalculationRule>();
			cusCalculationRule1.CCR_TransportMode = "SEA";
			cusCalculationRule1.CCR_GC_Company = GlbCompany.CurrentCompany.PK;
			var cusCalculationRule2 = Factory.NewWithValidTestData<CusCalculationRule>();
			cusCalculationRule2.CCR_TransportMode = "AIR";
			cusCalculationRule2.CCR_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			var filter = (ModuleTextFilter)filterBO[CusCalculationRulesFilterBusinessObject.FilterConstants.TransportMode];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			CombineAssertions(() =>
			{
				AssertEquals(FilterCategories.ModesAndTypes, filter.Category);
				filter.Property = "SEA";
				filteredDecs = Factory.Load(typeof(CusCalculationRule), filterBO.Filter);
				AssertEquals("Filter In", 1, filteredDecs.Length);
				AssertEquals("Filter In", cusCalculationRule1, filteredDecs[0]);
				filter.Property = "MAI";
				filteredDecs = Factory.Load(typeof(CusCalculationRule), filterBO.Filter);
				AssertEquals("Filter Out", 0, filteredDecs.Length);
			});
		}

		public void TestImporterSearch()
		{
			var org = OrgHeader.New(Factory);
			org.OH_Code = "ABC";
			org.OH_IsActive = true;
			var cusCalculationRule = Factory.NewWithValidTestData<CusCalculationRule>();
			cusCalculationRule.CCR_GC_Company = GlbCompany.CurrentCompany.PK;
			cusCalculationRule.CCR_OH_Importer = org.PK;
			Factory.Save();
			var filter = filterBO[CusCalculationRulesFilterBusinessObject.FilterConstants.Importer] as ModuleGuidFilter;
			filter.Property = org.PK;
			filter.IsActive = true;
			var collection = Factory.Load<CusCalculationRule>(filterBO.Filter);

			AssertEquals("Matched " + CusCalculationRulesFilterBusinessObject.FilterConstants.Importer + " on cusCalculationRule", 1, collection.Length);
			AssertEquals("Matched " + CusCalculationRulesFilterBusinessObject.FilterConstants.Importer + " on cusCalculationRule", org.PK, collection[0].CCR_OH_Importer);
		}

		public virtual void TestLookups()
		{
			AssertEquals("RuleType of correct type", typeof(CusCalculationRulesFilterLookups), filterBO.Lookups.GetType());
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => filterBO;

		protected override void SetUp()
		{
			base.SetUp();
			filterBO = new CusCalculationRulesFilterBusinessObject();
			filterBO.QueryObjectType = typeof(CusCalculationRule);
		}

		CusCalculationRulesFilterBusinessObject filterBO;
	}
}

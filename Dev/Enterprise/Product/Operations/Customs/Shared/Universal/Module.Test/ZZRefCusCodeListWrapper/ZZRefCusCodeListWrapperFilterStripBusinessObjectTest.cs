using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(ZZRefCusCodeListWrapperFilterStripBusinessObject))]
	class ZZRefCusCodeListWrapperFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void LoadFilter()
		{
			var otherFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(otherFactory);
			helper.CreateCusCodeType("CUSOF", "Customs Office");
			var cusCodeList1 = helper.CreateCusCodeList("ZZ", "CUSOF", "1111", "11 DESC", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(cusCodeList1.PK, "Type", "1");
			var cusCodeList2 = helper.CreateCusCodeList("ZZ", "CUSOF", "2222", "22 DESC", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(cusCodeList2.PK, "Type", "2");
			var cusCodeList3 = helper.CreateCusCodeList("ZZ", "CUSOF", "3333", "11 DESC", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(cusCodeList3.PK, "Type", "3");
			var cusCodeList4 = helper.CreateCusCodeList("ZZ", "CUSOF", "4444", "44 DESC", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(cusCodeList4.PK, "Type", "1");
			otherFactory.Save();
			var filterBO = new DummyRefCusCodeListWrapperFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO["Code"];
			filter.Property = "1111";
			filter.IsActive = true;
			Assert(cusCodeList1.MatchesFilter(filterBO.Filter));
			Assert(!cusCodeList2.MatchesFilter(filterBO.Filter));
			Assert(!cusCodeList3.MatchesFilter(filterBO.Filter));
			Assert(!cusCodeList4.MatchesFilter(filterBO.Filter));
			filter.IsActive = false;
			filter = (ModuleTextFilter)filterBO["Description"];
			filter.Property = "11";
			filter.IsActive = true;
			Assert(cusCodeList1.MatchesFilter(filterBO.Filter));
			Assert(!cusCodeList2.MatchesFilter(filterBO.Filter));
			Assert(cusCodeList3.MatchesFilter(filterBO.Filter));
			Assert(!cusCodeList4.MatchesFilter(filterBO.Filter));
			filter.IsActive = false;
			filter = (ModuleTextFilter)filterBO["Type"];
			filter.Property = "1";
			filter.IsActive = true;
			Assert(cusCodeList1.MatchesFilter(filterBO.Filter));
			Assert(!cusCodeList2.MatchesFilter(filterBO.Filter));
			Assert(!cusCodeList3.MatchesFilter(filterBO.Filter));
			Assert(cusCodeList4.MatchesFilter(filterBO.Filter));
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			Assert(!cusCodeList1.MatchesFilter(filterBO.Filter));
			Assert(cusCodeList2.MatchesFilter(filterBO.Filter));
			Assert(cusCodeList3.MatchesFilter(filterBO.Filter));
			Assert(!cusCodeList4.MatchesFilter(filterBO.Filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ZZRefCusCodeListWrapperFilterStripBusinessObject();
	}
}

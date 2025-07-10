using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TradeLanePivotCollection<DummyBusinessObject>))]
	sealed class DummyTradeLanePivotCollectionTest : ActiveBusinessObjectCollectionTestCase<TradeLanePivotCollection<DummyBusinessObject>>
	{
		public void TestRelationshipDefaultsForNewElement()
		{
			var pivot = Collection.AddNew();
			AssertEquals("Master", Master.PK, pivot.SVP_ActivityId);
			AssertEquals("ActivityTableCode", Master.TablePrefix, pivot.SVP_ActivityTableCode);
			AssertEquals("TradeTableCode", "", pivot.SVP_TradeTableCode);
		}

		public void TestAddPivotFor()
		{
			AssertEquals("Precondition", 0, Collection.Count);

			var tradeLane = Factory.NewWithValidTestData<OrgSales>();
			var pivot = Collection.AddPivotFor(tradeLane);
			AssertEquals("Pivot added", 1, Collection.Count);
			AssertEquals("Pivot added", true, Collection.Contains(pivot));
			AssertEquals("Pivot for TradeLane", tradeLane.PK, pivot.SVP_TradeId);

			var tradeLane1 = Factory.NewWithValidTestData<OrgSales>();
			pivot = Collection.AddPivotFor(tradeLane1);
			AssertEquals("Pivot added", 2, Collection.Count);
			AssertEquals("Pivot added", true, Collection.Contains(pivot));
			AssertEquals("Pivot for TradeLane1", tradeLane1.PK, pivot.SVP_TradeId);

			Collection.AddPivotFor(tradeLane);
			AssertEquals("New pivot not added if same pivot already exists", 2, Collection.Count);

			pivot = Collection.AddPivotFor(null);
			AssertEquals("New pivot for null not added", 2, Collection.Count);
			AssertEquals("Null pivot", null, pivot);
		}

		public void TestGetRelatedPivot()
		{
			var tradeLane1 = Factory.NewWithValidTestData<OrgSales>();
			var tradeLane2 = Factory.NewWithValidTestData<OrgSales>();
			Collection.AddPivotFor(tradeLane1);
			Collection.AddPivotFor(tradeLane2);

			AssertEquals("Pivot for correct trade lane", tradeLane1.PK, Collection.GetRelatedPivot(tradeLane1).SVP_TradeId);
			AssertEquals("Pivot for correct trade lane", tradeLane2.PK, Collection.GetRelatedPivot(tradeLane2).SVP_TradeId);

			var tradeLane3 = Factory.NewWithValidTestData<OrgSales>();
			AssertEquals("Returns null", null, Collection.GetRelatedPivot(tradeLane3));
			AssertEquals("Returns null", null, Collection.GetRelatedPivot(null));

			var pivot = Collection.AddPivotFor(tradeLane3);
			AssertEquals("Pivot for correct trade lane", tradeLane3.PK, Collection.GetRelatedPivot(tradeLane3).SVP_TradeId);
			pivot.Delete();
			AssertEquals("No pivot returned if pivot is deleted", null, Collection.GetRelatedPivot(tradeLane3));
		}

		public void TestContains()
		{
			var tradeLane1 = Factory.NewWithValidTestData<OrgSales>();
			var tradeLane2 = Factory.NewWithValidTestData<OrgSales>();
			var tradeLane3 = Factory.NewWithValidTestData<OrgSales>();
			Collection.AddPivotFor(tradeLane1);
			Collection.AddPivotFor(tradeLane2);

			AssertEquals(true, Collection.Contains(tradeLane1));
			AssertEquals(true, Collection.Contains(tradeLane2));
			AssertEquals(false, Collection.Contains(tradeLane3));
			AssertEquals(false, Collection.Contains(null));
		}

		public void TestDeletePivotFor()
		{
			var tradeLane1 = Factory.NewWithValidTestData<OrgSales>();
			var tradeLane2 = Factory.NewWithValidTestData<OrgSales>();
			var pivot1 = Collection.AddPivotFor(tradeLane1);
			var pivot2 = Collection.AddPivotFor(tradeLane2);

			AssertEquals(false, pivot1.IsDeleted);
			AssertEquals(false, pivot2.IsDeleted);
			AssertEquals(2, Collection.Count);

			Collection.DeletePivotFor(tradeLane1);
			AssertEquals(true, pivot1.IsDeleted);
			AssertEquals(false, pivot2.IsDeleted);
			AssertEquals(1, Collection.Count);
		}

		public void TestGetTradeLanes()
		{
			var tradeLane1 = Factory.NewWithValidTestData<OrgSales>();
			var tradeLane2 = Factory.NewWithValidTestData<OrgSales>();
			var tradeLane3 = Factory.NewWithValidTestData<OrgSales>();
			Collection.AddPivotFor(tradeLane1);
			Collection.AddPivotFor(tradeLane2);
			Collection.AddPivotFor(tradeLane3);

			var results = new List<OrgSales>(Collection.GetTradeLanes());
			AssertEquals("All trade lanes returned", 3, results.Count);
			AssertEquals(true, results.Contains(tradeLane1));
			AssertEquals(true, results.Contains(tradeLane2));
			AssertEquals(true, results.Contains(tradeLane3));
		}

		public void TestDeleteObsoletePivots()
		{
			var tradeLane1 = Factory.NewWithValidTestData<OrgSales>();
			var tradeLane2 = Factory.NewWithValidTestData<OrgSales>();
			var tradeLane3 = Factory.NewWithValidTestData<OrgSales>();
			var pivot1 = Collection.AddPivotFor(tradeLane1);
			var pivot2 = Collection.AddPivotFor(tradeLane2);
			var pivot3 = Collection.AddPivotFor(tradeLane3);
			AssertEquals("Precondition", 3, Collection.Count);

			Collection.DeleteObsoletePivots(new OrgSales[] { tradeLane1, tradeLane2 });
			AssertEquals("Obsolete pivots deleted", 2, Collection.Count);
			AssertEquals(true, Collection.Contains(tradeLane1));
			AssertEquals(true, Collection.Contains(tradeLane2));
			AssertEquals(false, Collection.Contains(tradeLane3));

			Collection.DeleteObsoletePivots(new OrgSales[] { tradeLane1 });
			AssertEquals("Obsolete pivots deleted", 1, Collection.Count);
			AssertEquals(true, Collection.Contains(tradeLane1));
			AssertEquals(false, Collection.Contains(tradeLane2));
			AssertEquals(false, Collection.Contains(tradeLane3));
		}

		#region Implementation

		DummyBusinessObject Master;

		protected override TradeLanePivotCollection<DummyBusinessObject> GetCollectionToTest()
		{
			Master = Factory.NewWithValidTestData<DummyBusinessObject>();
			return new TradeLanePivotCollection<DummyBusinessObject>(Master);
		}

		#endregion
	}
}

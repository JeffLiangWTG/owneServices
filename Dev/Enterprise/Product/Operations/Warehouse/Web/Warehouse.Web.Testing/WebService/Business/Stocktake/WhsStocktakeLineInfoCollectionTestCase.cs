using System;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WhsStocktakeLineInfoCollectionTestCase : DataObjectInfoCollectionTestCase<WhsStocktakeLineInfo>
	{
		public void TestAdditionalConstructors()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stockTakeInfo = new WhsStocktakeInfo { Filter = new WhsStocktakeFilterInfo() };
			var collection = new WhsStocktakeLineInfoCollection(stockTakeInfo, new WhsStocktakeLineCollectionND(Factory));
			AssertNotNull(collection);
			AssertEquals(0, collection.Count);

			var lines = new WhsStocktakeLineCollectionND(Factory);
			var line1 = lines.AddNew();
			line1.WU_OP = data.Part1.PK;
			line1.WU_OH_Client = data.Org1.PK;
			line1.WU_WL = data.Whs1.DefaultLocation.PK;
			AssertEquals(1, lines.Count);
			collection = new WhsStocktakeLineInfoCollection(stockTakeInfo, lines);
			AssertNotNull(collection);
			AssertEquals(1, collection.Count);
			AssertEquals(line1.PK, collection[0].PK);

			var line2 = lines.AddNew();
			line2.WU_OP = data.Part1.PK;
			line2.WU_OH_Client = data.Org1.PK;
			line2.WU_WL = data.Whs1.DefaultLocation.PK;
			AssertEquals(2, lines.Count);
			collection = new WhsStocktakeLineInfoCollection(stockTakeInfo, lines);
			AssertNotNull(collection);
			AssertEquals(2, collection.Count);
			AssertEquals(line1.PK, collection[0].PK);
			AssertEquals(line2.PK, collection[1].PK);
		}

		#region Implementation

		protected new WhsStocktakeLineInfoCollection Parent
		{
			get
			{
				return (WhsStocktakeLineInfoCollection)base.Parent;
			}
		}

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(WhsStocktakeLineInfo);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WhsStocktakeLineInfoCollection);
		}

		protected override WhsStocktakeLineInfo GetNewObjectInfo()
		{
			return new WhsStocktakeLineInfo();
		}

		protected override DataObjectInfoCollection<WhsStocktakeLineInfo> GetNewObjectInfoCollection()
		{
			return new WhsStocktakeLineInfoCollection();
		}

		#endregion
	}
}

using System;
using System.Linq;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsStocktakeInfo))]
	public class WhsStocktakeInfoTestCase : DataObjectInfoTestCase<WhsStocktakeInfo>
	{
		#region TestAdditionalConstructors

		public void TestAdditionalConstructors()
		{
			var stocktake = Helper.CreateWhsStocktake(Data.Org1, Data.Whs1);

			var stocktakeInfo = new WhsStocktakeInfo(stocktake);

			AssertEquals("PK", stocktake.PK.ToGuid(), stocktakeInfo.PK);
			AssertEquals("Number", stocktake.WS_StocktakeNumber, stocktakeInfo.Number);
			AssertEquals("ClientCode", Data.Org1.OH_Code, stocktakeInfo.ClientCode);
			AssertEquals("ClientPK", Data.Org1.PK, stocktakeInfo.ClientPK);
			AssertEquals("WarehouseCode", Data.Whs1.WW_WarehouseCode, stocktakeInfo.WarehouseCode);
			AssertNotNull("Filter", stocktakeInfo.Filter);
		}

		public void TestAdditionalConstructors_NoClient()
		{
			var stocktake = Helper.CreateWhsStocktake(null, Data.Whs1);
			var stocktakeInfo = new WhsStocktakeInfo(stocktake);

			AssertEquals("PK", stocktake.PK.ToGuid(), stocktakeInfo.PK);
			AssertEquals("Number", stocktake.WS_StocktakeNumber, stocktakeInfo.Number);
			Assert("ClientCode", string.IsNullOrEmpty(stocktakeInfo.ClientCode));
			AssertEquals("ClientPK", Guid.Empty, stocktakeInfo.ClientPK);
			AssertEquals("WarehouseCode", Data.Whs1.WW_WarehouseCode, stocktakeInfo.WarehouseCode);
			AssertNotNull("Filter", stocktakeInfo.Filter);
		}

		#endregion

		#region TestProductInfos

		public void TestProductInfos()
		{
			var stocktake = Helper.CreateWhsStocktake(Data.Org1, Data.Whs1);

			var stockTakeLineList = new WhsStocktakeLineCollectionND(Factory);
			stockTakeLineList.Add(Helper.CreateWhsStocktakeLine(stocktake, Data.Org1, Data.Part1));

			stockTakeLineList.Add(Helper.CreateWhsStocktakeLine(stocktake, Data.Org1, Data.Part1));

			stockTakeLineList.Add(Helper.CreateWhsStocktakeLine(stocktake, Data.Org1, Data.Part2));
			stockTakeLineList.Add(Helper.CreateWhsStocktakeLine(stocktake, Data.Org1, Data.Part2));
			stockTakeLineList.Add(Helper.CreateWhsStocktakeLine(stocktake, Data.Org1, Data.Part2));

			foreach (WhsStocktakeLine line in stockTakeLineList)
			{
				line.WU_WL = Data.Whs1.DefaultLocation.PK;
			}

			var stocktakeInfo = new WhsStocktakeInfo(stocktake);
			new WhsStocktakeLineInfoCollection(stocktakeInfo, stockTakeLineList);

			AssertEquals("Should have 2 ProductInfos. One for each distinct product", 2, stocktakeInfo.ProductInfos.Count);
			AssertContainsExactElementsInAnyOrder(new[] { Data.Part1.PK, Data.Part2.PK }, stocktakeInfo.ProductInfos.Select(u => u.PK));
		}

		#endregion

		#region TestProductPartAttributesInfos

		public void TestProductPartAttributesInfos()
		{
			var stocktake = Helper.CreateWhsStocktake(Data.Org1, Data.Whs1);
			var stockTakeLineList = new WhsStocktakeLineCollectionND(Factory);

			stockTakeLineList.Add(Helper.CreateWhsStocktakeLine(stocktake, Data.Org1, Data.Part1));
			stockTakeLineList.Add(Helper.CreateWhsStocktakeLine(stocktake, Data.Org1, Data.Part1));

			stockTakeLineList.Add(Helper.CreateWhsStocktakeLine(stocktake, Data.Org1, Data.Part2));

			var client2 = Helper.CreateClient("CL2");
			stockTakeLineList.Add(Helper.CreateWhsStocktakeLine(stocktake, client2, Data.Part2));
			stockTakeLineList.Add(Helper.CreateWhsStocktakeLine(stocktake, client2, Data.Part2));

			foreach (WhsStocktakeLine line in stockTakeLineList)
			{
				line.WU_WL = Data.Whs1.DefaultLocation.PK;
			}

			var stocktakeInfo = new WhsStocktakeInfo(stocktake);
			new WhsStocktakeLineInfoCollection(stocktakeInfo, stockTakeLineList);
			AssertEquals("Should have 3 ProductPartAttributesInfos. One for each distinct pair of product+client", 3, stocktakeInfo.ProductPartAttributesInfos.Count);
			AssertEquals(1, stocktakeInfo.ProductPartAttributesInfos.Count(pp => pp.ClientPK == Data.Org1.PK && pp.ProductPK == Data.Part1.PK));
			AssertEquals(1, stocktakeInfo.ProductPartAttributesInfos.Count(pp => pp.ClientPK == Data.Org1.PK && pp.ProductPK == Data.Part2.PK));
			AssertEquals(1, stocktakeInfo.ProductPartAttributesInfos.Count(pp => pp.ClientPK == client2.PK && pp.ProductPK == Data.Part2.PK));
		}

		#endregion

		#region TestEmptyLine

		public void TestEmptyLine()
		{
			var stocktake = Helper.CreateWhsStocktake(Data.Org1, Data.Whs1);
			var stockTakeLineList = new WhsStocktakeLineCollectionND(Factory);

			var line = Helper.CreateWhsStocktakeLine(stocktake, Data.Org1, null);
			line.WU_Status = "EMP";
			stockTakeLineList.Add(line);

			Assert(stockTakeLineList.Cast<WhsStocktakeLine>().Single().IsEmptyLocation);

			var stocktakeInfo = new WhsStocktakeInfo(stocktake);
			var lines = new WhsStocktakeLineInfoCollection(stocktakeInfo, stockTakeLineList);
			Assert(lines.Cast<WhsStocktakeLineInfo>().Single().IsEmptyLocation);
		}

		#endregion

		#region Implementation

		protected new WhsStocktakeInfo Parent
		{
			get
			{
				return (WhsStocktakeInfo)base.Parent;
			}
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsStocktakeInfo();
		}

		#endregion
	}
}

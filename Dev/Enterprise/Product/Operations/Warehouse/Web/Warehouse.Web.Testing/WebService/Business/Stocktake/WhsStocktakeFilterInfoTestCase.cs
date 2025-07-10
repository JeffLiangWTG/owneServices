using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsStocktakeFilterInfo))]
	public class WhsStocktakeFilterInfoTestCase : DataObjectInfoTestCase<WhsStocktakeFilterInfo>
	{
		public void TestAdditionalConstructors()
		{
			var stocktake = Helper.CreateWhsStocktake(Data.Org1, Data.Whs1, Data.Part1);

			stocktake.WS_StocktakeCycle = "CY";
			stocktake.WS_PickMethod = "PM";
			stocktake.WS_ABCAnalysisCategory = "ABC";
			stocktake.WS_WL_Location = Data.Whs1.FindLocation("A").PK;
			stocktake.WS_StocktakeType = "T1";

			var stocktakeFilterInfo = new WhsStocktakeFilterInfo(stocktake);

			AssertEquals("Cycle", "CY", stocktakeFilterInfo.Cycle);
			AssertEquals("Product", Data.Part1.OP_PartNum, stocktakeFilterInfo.Product);
			AssertEquals("Commodity", string.Empty, stocktakeFilterInfo.Commodity);
			AssertEquals("PickMethod", "PM", stocktakeFilterInfo.PickMethod);
			AssertEquals("Row", string.Empty, stocktakeFilterInfo.Row);
			AssertEquals("Area", null, stocktakeFilterInfo.Area);
			AssertEquals("ABCAnalysisCategory", "ABC", stocktakeFilterInfo.ABCAnalysisCategory);
			AssertEquals("Location", "A", stocktakeFilterInfo.Location);
			AssertEquals("Stocktake Type", "T1", stocktakeFilterInfo.StocktakeType);

			Helper.CreateWhsStocktakeProductFilter(stocktake, Data.Part2);
			AssertEquals("Many", Data.Part1.OP_PartNum, stocktakeFilterInfo.Product);
		}

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

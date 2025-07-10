using System;
using System.Collections.Specialized;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsStocktakeJobFountainUniqueIndexFailureHandlingTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		#region NumberFountainUniqueIndexFailureHandlingTest

		protected override Type BizOTypeToTest
		{
			get { return typeof(WhsStocktake); }
		}

		protected override SchemaColumn ColumnThatUsesNumberFountain
		{
			get { return WhsStocktakeSchema.WS_StocktakeNumber; }
		}

		protected override INumberFountainProxy NumberFountainToTest
		{
			get { return Env.NumberFountains.WarehouseStocktakeNumber; }
		}

		protected override void SetExtraPropertyValuesAfterCreatingBizO(BusinessObject testBizO)
		{
			base.SetExtraPropertyValuesAfterCreatingBizO(testBizO);

			var stocktake = (WhsStocktake)testBizO;
			stocktake.WS_StocktakeType = "STD";
			stocktake.WS_StocktakeStatus = "LOD";
			stocktake.WS_WW_Whs = Warehouse.PK;
		}

		protected override NameValueCollection AdditionalInsertValues
		{
			get
			{
				var values = base.AdditionalInsertValues;
				values.Add(WhsStocktakeSchema.Constants.WS_StocktakeDate,
					$"'{ZDateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'");
				values.Add(WhsStocktakeSchema.Constants.WS_StocktakeCycle, "'FOO'");
				values.Add(WhsStocktakeSchema.Constants.WS_PickMethod, "'FOO'");
				values.Add(WhsStocktakeSchema.Constants.WS_ABCAnalysisCategory, "'FOO'");
				values.Add(WhsStocktakeSchema.Constants.WS_RH_NKCommodityCode, "'FOOO'");
				values.Add(WhsStocktakeSchema.Constants.WS_CountEmptyLocationsCategory, "'OMT'");
				return values;
			}
		}

		#endregion

		#region Data

		WhsTestHelperFunctionsEnv Helper => helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory));
		WhsTestHelperFunctionsEnv helper;

		WhsWarehouse Warehouse
		{
			get
			{
				if (warehouse == null)
				{
					warehouse = Helper.CreateWarehouse("Warehouse", "WH1", "A");
				}

				return warehouse;
			}
		}

		WhsWarehouse warehouse;

		#endregion
	}
}

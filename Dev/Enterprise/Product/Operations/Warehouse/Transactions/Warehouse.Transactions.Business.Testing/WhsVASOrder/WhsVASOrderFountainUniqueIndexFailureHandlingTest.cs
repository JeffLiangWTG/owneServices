using System;
using System.Collections.Specialized;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsVASOrderFountainUniqueIndexFailureHandlingTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		protected override Type BizOTypeToTest => typeof(WhsVASOrder);

		protected override SchemaColumn ColumnThatUsesNumberFountain => WhsVASOrderSchema.WVO_JobID;

		protected override INumberFountainProxy NumberFountainToTest => Env.NumberFountains.WarehouseVASOrderJobID;

		protected override void SetExtraPropertyValuesAfterCreatingBizO(BusinessObject testBizO)
		{
			base.SetExtraPropertyValuesAfterCreatingBizO(testBizO);
			var order = (WhsVASOrder)testBizO;
			order.WVO_WA_ServiceArea = Area.PK;
			order.WVO_OH_Client = Warehouse.WarehouseAddress.OA_OH;
		}

		protected override NameValueCollection AdditionalInsertValues
		{
			get
			{
				var values = base.AdditionalInsertValues;

				var area = Area;
				values.Add(WhsVASOrderSchema.Constants.WVO_CustomerReferenceNo, "'FOO'");
				values.Add(WhsVASOrderSchema.Constants.WVO_WA_ServiceArea, string.Format(Culture.Invariant, "'{0}'", area.PK));
				values.Add(WhsVASOrderSchema.Constants.WVO_OH_Client, string.Format(Culture.Invariant, "'{0}'", Warehouse.WarehouseAddress.OA_OH));
				values.Add(WhsVASOrderSchema.Constants.WVO_SystemCreateTimeUtc, ZDateTime.Now.ToString("yyyy-MM-dd"));
				values.Add(WhsVASOrderSchema.Constants.WVO_SystemCreateUser, "'FOO'");
				values.Add(WhsVASOrderSchema.Constants.WVO_SystemLastEditTimeUtc, ZDateTime.Now.ToString("yyyy-MM-dd"));
				values.Add(WhsVASOrderSchema.Constants.WVO_SystemLastEditUser, "'FOO'");
				return values;
			}
		}

		#region data

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		WhsWarehouse Warehouse
		{
			get
			{
				if (warehouse == null)
				{
					var org = Factory.NewWithValidTestData<OrgHeader>();
					warehouse = Factory.New<WhsWarehouse>();
					warehouse.WW_OA_WarehouseAddress = org.MainAddress.PK;
					warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK; // Used to avoid registry changes
					warehouse.WW_WarehouseCode = "Wh1";
					warehouse.WW_WLT_DefaultLocationType = Helper.CreateLocationType("DJ1").PK;
				}
				return warehouse;
			}
		}
		WhsWarehouse warehouse;

		WhsArea Area => area ?? (area = Helper.CreateServiceAreaForVASOrder(Warehouse));
		WhsArea area;

		#endregion
	}
}

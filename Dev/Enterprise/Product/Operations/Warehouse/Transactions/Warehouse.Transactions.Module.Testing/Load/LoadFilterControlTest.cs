using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public class LoadFilterControlTest : WhsFilterControlDBHitsTestCase<WhsLoadCollection, LoadFilterBusinessObject>
	{
		#region SetupData

		protected override void SetupData()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			for (int i = 0; i < 10; i++)
			{
				GetLoad(data, i.ToString());
			}
		}

		#endregion

		#region GetExpectedHitsDictionary

		protected override Dictionary<string, Dictionary<string, int>> GetExpectedHitsDictionary() => new Dictionary<string, Dictionary<string, int>>();

		#endregion

		#region GetBaseHits

		protected override Dictionary<string, int> GetBaseHits()
		{
			var baseHits = new Dictionary<string, int>();
			baseHits.Add(WhsLoadSchema.Constants.TableName, 1);
			return baseHits;
		}

		#endregion

		#region GetNewCollection

		protected override WhsLoadCollection GetNewCollection(BusinessObjectFactory factory) => new WhsLoadCollection(factory);

		#endregion

		#region GetNewFilterBusinessObject

		protected override LoadFilterBusinessObject GetNewFilterBusinessObject() => new LoadFilterBusinessObject();

		#endregion

		#region GetNewFilterControl

		protected override ZFilterStripControl GetNewFilterControl(WhsLoadCollection collection, LoadFilterBusinessObject filterBizO) => new LoadFilterControl(collection, filterBizO);

		#endregion

		#region Implementation

		protected override WhsTestHelperFunctionsEnv GetNewTestHelperFunctions() => new WhsTestHelperFunctions(Factory);

		protected override ZFilterStripControl GetNewFilterStripControl()
		{
			var loads = new WhsLoadCollection(Factory);
			var filterBO = new LoadFilterBusinessObject();
			return new LoadFilterControl(loads, filterBO);
		}

		new WhsTestHelperFunctions Helper => base.Helper;

		WhsLoad GetLoad(TestDataSimpleEnvironment data, string suffix)
		{
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;

			var transportCompany = Helper.CreateClient("TC" + suffix);
			var carrierServicelevel = transportCompany.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";

			var truck = Helper.CreateEquipment("T00" + suffix, 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			return Helper.CreateWhsLoad(
				transportCompany,
				dockDoorLocation,
				"WL0000000" + suffix,
				carrierServicelevel.PL_Code,
				truck);
		}

		#endregion
	}
}

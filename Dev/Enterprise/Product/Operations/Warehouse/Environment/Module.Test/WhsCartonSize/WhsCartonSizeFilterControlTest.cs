using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	class WhsCartonSizeFilterControlTest : WhsEnvFilterControlDBHitsTestCase<WhsCartonSizeCollection, WhsCartonSizeFilterBusinessObject>
	{
		#region SetupData

		protected override void SetupData()
		{
			for (int i = 0; i < 10; i++)
			{
				Helper.CreateWhsCartonSize(i.ToString());
			}
		}

		#endregion

		#region GetExpectedHitsDictionary

		protected override Dictionary<string, Dictionary<string, int>> GetExpectedHitsDictionary()
		{
			// Should be nothing but base hits
			return new Dictionary<string, Dictionary<string, int>>();
		}

		#endregion

		#region GetBaseHits

		protected override Dictionary<string, int> GetBaseHits()
		{
			var baseHits = new Dictionary<string, int>();
			baseHits.Add(WhsCartonSizeSchema.Constants.TableName, 1);
			return baseHits;
		}

		#endregion

		#region GetNewCollection

		protected override WhsCartonSizeCollection GetNewCollection(BusinessObjectFactory factory)
		{
			return new WhsCartonSizeCollection(factory);
		}

		#endregion

		#region GetNewFilterBusinessObject

		protected override WhsCartonSizeFilterBusinessObject GetNewFilterBusinessObject() => new WhsCartonSizeFilterBusinessObject();

		#endregion

		#region GetNewFilterControl

		protected override ZFilterStripControl GetNewFilterControl(WhsCartonSizeCollection collection, WhsCartonSizeFilterBusinessObject filterBizO)
		{
			return new WhsCartonSizeFilterControl(collection, filterBizO);
		}

		#endregion
	}
}

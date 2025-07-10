using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	class DynamicPickFacesFilterControlDbHitsTest : WhsEnvFilterControlDBHitsTestCase<WhsDynamicPickFaceViewCollection, DynamicPickFacesFilterBusinessObject>
	{
		protected override Dictionary<string, int> GetBaseHits()
		{
			var dict = new Dictionary<string, int>
			{
				{ WhsDynamicPickFaceViewSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 }
			};
			return dict;
		}

		protected override Dictionary<string, Dictionary<string, int>> GetExpectedHitsDictionary() => new Dictionary<string, Dictionary<string, int>>();

		protected override WhsDynamicPickFaceViewCollection GetNewCollection(BusinessObjectFactory factory)
		{
			var result = new WhsDynamicPickFaceViewCollection(factory);
			result.ApplySort(nameof(WhsDynamicPickFaceView.WDP_LocationString), System.ComponentModel.ListSortDirection.Ascending);
			return result;
		}

		protected override DynamicPickFacesFilterBusinessObject GetNewFilterBusinessObject() => new DynamicPickFacesFilterBusinessObject();

		protected override ZFilterStripControl GetNewFilterControl(WhsDynamicPickFaceViewCollection collection, DynamicPickFacesFilterBusinessObject filterBizO)
			=> new DynamicPickFacesFilterControl(collection, filterBizO);

		protected override void SetupData()
		{
			var data = new DynamicPickFaceTestHelper(Factory);
			for (var i = 0; i < 10; i++)
			{
				var warehouse = Helper.CreateWarehouse($"W{i}", $"R{i}");
				var client = Helper.CreateClient($"{i}", $"C{i}");
				var product = Helper.CreateProduct($"P{i}", client);
				Factory.Save();

				var location = warehouse.FindLocation($"R{i}");
				data.BindProductToDynamicPickingArea(product, warehouse, client, Helper.CreateDynamicPF(warehouse, location));
				Factory.Save();
			}
		}
	}
}

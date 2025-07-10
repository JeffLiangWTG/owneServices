using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsProductParamsByWhsAndClientLookups : AutoWhsProductParamsByWhsAndClientLookups
	{
		public WhsProductParamsByWhsAndClientLookups(AutoWhsProductParamsByWhsAndClient parent)
			: base(parent)
		{
		}

		#region Parent

		public new WhsProductParamsByWhsAndClient Parent => (WhsProductParamsByWhsAndClient)base.Parent;

		#endregion

		#region PickGroups

		public ICodeDescriptionPairList PickGroups => WarehouseDataRegistry.Instance.PickGroups.Value;

		#endregion

		#region Headers

		public override OrgHeaderCollection Headers
			=> Factory.GetCachedValue("WhsProductParamsByWhsAndClientLookups|Headers", () => new WarehouseClientCollection(Factory));

		#endregion

		#region Warehouses

		public WhsWarehouseCollection Warehouses
			=> Factory.GetCachedValue("WhsProductParamsByWhsAndClientLookups|Warehouses", () => new WhsWarehouseCollectionWithSecurityCheck(Factory));

		#endregion

		#region Areas

		public WhsAreaCollection PutawayAreas => GetAreas(WhsAreaCollection.GetPutawayAreas);

		public WhsAreaCollection DynamicPickFaceAreas => GetAreas(WhsAreaCollection.GetPickingAreas);

		WhsAreaCollection GetAreas(Func<BusinessObjectFactory, ZGuid, WhsAreaCollection> getAreas)
		{
			WhsAreaCollection result;

			var warehouse = Parent.Warehouse;
			if (warehouse == null)
			{
				// we pass in a null warehouse to make the area collection empty.
				result = getAreas(Factory, ZGuid.Empty);

				if (result.FilterBusinessObjectDefaults.ContainsDefaultFor("Warehouse:Property"))
				{
					result.FilterBusinessObjectDefaults.Remove("Warehouse:Property");
				}
			}
			else
			{
				result = getAreas(Factory, warehouse.PK);

				if (!result.FilterBusinessObjectDefaults.ContainsDefaultFor("Warehouse:Property"))
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Warehouse", "Property", () => { return warehouse.PK; }));
				}
			}

			return result;
		}

		#endregion

		#region Locations

		public WhsLocationCollection StagingLocationsBOM => GetLocations();

		WhsLocationCollection GetLocations()
		{
			WhsLocationCollection result;

			var warehouse = Parent.Warehouse;
			var key = "WhsProductParamsByWhsAndClientLookups|StagingLocationsBOM";    // Key used in factory cache
			if (warehouse == null)
			{
				result = Factory.GetCachedValue(key, () => WhsLocationCollection.GetEmptyLocationCollection(Factory));
			}
			else
			{
				result = Factory.GetCachedValue(Invariant($"{key}{warehouse.PK}"), () => new WhsLocationCollection(warehouse));  // Key used in factory cache
			}

			return result;
		}

		#endregion

		#region StockTakeCycles

		public ReadOnlyCodeDescriptionPairList StockTakeCycles
			=> Factory.GetCachedValue("WhsProductParamsByWhsAndClientLookups|StockTakeCycles", () => WarehouseDataRegistry.Instance.StocktakeCycle.Value);

		#endregion

		#region PackTypes

		public CodeDescriptionPairList PackTypes => new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits();

		#endregion

		#region PutawayGroups

		public WhsPutawayGroupCollection PutawayGroups
			=> Factory.GetCachedValue("WhsProductParamsByWhsAndClientLookups|PutawayGroups", () => new WhsPutawayGroupCollection(Factory));

		#endregion
	}
}

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsStocktakeLineLookups : AutoWhsStocktakeLineLookups
	{
		public WhsStocktakeLineLookups(AutoWhsStocktakeLine parent)
			: base(parent)
		{
		}

		#region Parent

		WhsStocktakeLine Line
		{
			get { return (WhsStocktakeLine)base.Parent; }
		}

		#endregion

		#region SupplierParts

		public override OrgSupplierPartCollection SupplierParts
		{
			get
			{
				return Factory.GetCachedValue(string.Format(Culture.Invariant, "WhsStocktakeLineLookups|SupplierParts-{0}", Line.WU_OH_Client.ToStringKey()), () =>  // Key used in Factory Cache
					{
						WhsOrgSupplierPartCollection result;

						if (!Line.WU_OH_Client.IsEmpty)
						{
							result = new WhsOrgSupplierPartCollection(Factory, null, Line.Client, false);
							result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property1", Line.WU_OH_Client));
						}
						else
						{
							result = new WhsOrgSupplierPartCollection(Factory);
						}

						return result;
					});
			}
		}

		#endregion

		#region InventoryStatuses

		public CodeDescriptionPairList InventoryStatuses
		{
			get
			{
				return Factory.GetCachedValue("WhsStocktakeLineLookups|InventoryStatuses", () => // Key used in Factory Cache
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(StocktakeInventoryStatus.Codes.Available, StocktakeInventoryStatus.Descriptions.Available);
						result.AddPair(StocktakeInventoryStatus.Codes.Damaged, StocktakeInventoryStatus.Descriptions.Damaged);
						result.AddPair(StocktakeInventoryStatus.Codes.Held, StocktakeInventoryStatus.Descriptions.Held);
						return result;
					});
			}
		}

		#endregion

		#region Locations

		public WhsLocationCollection Locations
		{
			get
			{
				var warehouse = Line.Stocktake != null ? Line.Stocktake.Warehouse : null;
				return warehouse != null
					? Factory.GetCachedValue(string.Format(Culture.Invariant, (NoResString)"WhsStocktakeLineLookups|Locations-{0}", warehouse.PK.ToStringKey()), () => new WhsLocationCollection(Line.Stocktake.Warehouse))   // Key used in Factory Cache
					: Factory.GetCachedValue("WhsStocktakeLineLookups|Locations", () => new WhsLocationCollection(Factory));   // Key used in Factory Cache
			}
		}

		#endregion
	}
}

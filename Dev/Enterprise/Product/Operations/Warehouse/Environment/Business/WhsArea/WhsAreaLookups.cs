using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsAreaLookups : AutoWhsAreaLookups
	{
		public WhsAreaLookups(AutoWhsArea parent)
			: base(parent)
		{
		}

		protected new WhsArea Parent
		{
			get { return (WhsArea)base.Parent; }
		}

		#region Locations

		public LocationCollection Locations
		{
			get { return Factory.GetCachedValue("TransitWarehouseLocationCollection", delegate { return new TransitWarehouseLocationCollection(Factory); }); }
		}

		#endregion

		#region Warehouses

		public WhsWarehouseCollection Warehouses
		{
			get { return Factory.GetCachedValue("WhsAreaLookups|Warehouses", GetWarehouses); }
		}

		WhsWarehouseCollection GetWarehouses()
		{
			var result = new WhsWarehouseCollectionWithSecurityCheck(Factory);
			((IWhsWarehouseCollection)result).WarehouseCollectionType = WarehouseCollectionType.All;
			return result;
		}

		#endregion

		#region AreaTypes

		public AreaTypes AreaTypes => Factory.GetCachedValue(nameof(AreaTypes), () => new AreaTypes());

		#endregion

		#region Printers

		public IBusinessObjectCollection Printers => WhsCommonLookups.GetPrintersList(Factory);

		#endregion

		#region WeightUnitTypes

		public CodeDescriptionPairList WeightUnitTypes
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		#endregion

		#region VolumeUnitTypes

		public CodeDescriptionPairList VolumeUnitTypes
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		#endregion
	}
}


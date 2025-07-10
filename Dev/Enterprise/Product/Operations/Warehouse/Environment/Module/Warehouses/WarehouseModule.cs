using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.Module
{
	public class WarehouseModule : ZFilterGridModule
	{
		public WarehouseModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.WhsConfigWarehouse; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsConfigWarehouse);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new WarehouseFilterControl(GridCollection, (WarehouseFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new WarehouseFilterBusinessObject(WarehouseCollectionType);
		}

		WarehouseCollectionType WarehouseCollectionType
		{
			get { return ((IWhsWarehouseCollection)GridCollection).WarehouseCollectionType; }
		}

		bool IsProductWarehouseOnly
		{
			get
			{
				var collectionType = ((IWhsWarehouseCollection)GridCollection).WarehouseCollectionType;
				return collectionType == WarehouseCollectionType.ProductWarehouse;
			}
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			IWhsWarehouseCollection collection = new WhsWarehouseCollection(Factory);
			collection.WarehouseCollectionType = WarehouseCollectionType.All;
			return collection;
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.WarehouseManagerCoreAnd4PL; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.WhsConfigWarehouse; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		protected override bool ShowRecentItemsCore()
		{
			return base.ShowRecentItemsCore() && !IsProductWarehouseOnly;
		}
	}
}

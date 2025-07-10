using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.Module
{
	public class LocationModule : ZFilterGridModule
	{
		public override bool AllowNew => false;

		public override bool AllowDelete => false;

		public override bool AllowEdit => false;

		public override bool AllowView => false;

		public override ModuleIdentifier ID => ModuleIDs.WhsConfigLocation;

		protected override Type TypeOfTopLevelBusinessObjectCore => typeof(WhsLocation);

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			throw new NotSupportedException("LocationModule.GetNewController is not supported.");
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new LocationFilterControl(GridCollection, (LocationFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new LocationFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsLocationCollection(Factory); // this is not actually used at runtime and only exists to satisfy tests.
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.WarehouseManagerCoreAnd4PL;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsConfigLocation;
	}
}

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
	class WhsSalesChannelModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.WhsSalesChannel;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsConfigSalesChannel;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.WarehouseManagerOperationsAnd3PL;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.WhsSalesChannel);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new WhsSalesChannelFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new WhsSalesChannelFilterControl(GridCollection, (WhsSalesChannelFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new WhsSalesChannelCollection(Factory);

		public override bool AllowDelete => true;
	}
}

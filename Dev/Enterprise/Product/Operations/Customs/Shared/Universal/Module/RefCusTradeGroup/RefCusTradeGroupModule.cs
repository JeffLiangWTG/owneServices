using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.Module
{
	public class RefCusTradeGroupModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.Universal.RefCusTradeGroup;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.GlobalTariffs;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override bool AllowNew => false;

		public override bool AllowDelete => false;

		public override bool AllowEdit => false;

		public override bool AllowView => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.Universal.RefCusTradeGroup);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new RefCusTradeGroupFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new RefCusTradeGroupFilterControl(GridCollection, (RefCusTradeGroupFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new RefCusTradeGroupCollection(Factory);
	}
}

using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.Module
{
	public class HVLVOuterPackageModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.HVLVOuterPackage;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Forwarder;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => null;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new HVLVOuterPackageFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new HVLVOuterPackageFilterControl(GridCollection, (HVLVOuterPackageFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new HVLVOuterPackageCollection(Factory);

		protected override bool ShowRecentItemsCore() => false;

		public override bool AllowNew => false;

		public override bool AllowEdit => false;

		public override bool AllowDelete => false;

		public override bool AllowUniversalCopy => false;

		public override bool AllowView => false;
	}
}

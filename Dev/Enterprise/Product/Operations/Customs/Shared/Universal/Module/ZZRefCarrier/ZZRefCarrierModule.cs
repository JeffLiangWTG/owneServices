using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.Module
{
	class ZZRefCarrierModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.Universal.ZZRefCarrier;

		public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.GlobalCarriers;

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ZZRefCarrierFilterStripBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ZZRefCarrierFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override CargoWise.EntityFramework.IBusinessObjectCollection GetNewGridCollection()
		{
			return new ZZRefCarrierCombinedCollection(Factory);
		}

		public override bool AllowNew => true;

		public override bool AllowEdit => true;

		public override bool AllowDelete => true;

		public override bool AllowUniversalCopy => false;

		public override bool AllowView => true;

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		protected override ZController GetNewController(CargoWise.EntityFramework.BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.Universal.ZZRefCarrier);
		}
	}
}

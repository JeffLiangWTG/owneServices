#define CODE_ANALYSIS
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.Module
{
	class ZZRefCusMapModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.Universal.ZZRefCusMap; }
		}

		public override Security.SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.GlobalCodes; }
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		protected override ZController GetNewController(CargoWise.EntityFramework.BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.Universal.ZZRefCusMap);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ZZRefCusMapFilterStripBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ZZRefCusMapFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override CargoWise.EntityFramework.IBusinessObjectCollection GetNewGridCollection()
		{
			return new ZZRefCusMapCombinedCollection(Factory);
		}

		public override bool AllowNew
		{
			get { return true; }
		}

		public override bool AllowEdit
		{
			get { return true; }
		}

		public override bool AllowDelete
		{
			get { return true; }
		}

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}
	}
}

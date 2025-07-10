#define CODE_ANALYSIS
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.Module
{
	class ZZRefCusCodeListModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.Universal.ZZRefCusCodeList; }
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
			return ZControllerFactory.Create(ControllerIDs.Customs.Universal.ZZRefCusCodeList);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ZZRefCusCodeListFilterStripBusinessObject(GridCollection as ZZRefCusCodeListCombinedCollection);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ZZRefCusCodeListFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override CargoWise.EntityFramework.IBusinessObjectCollection GetNewGridCollection()
		{
			return new ZZRefCusCodeListCombinedCollection(Factory);
		}

		protected override void DefaultLayoutContext(FilterStripBusinessObject filterBusinessObject)
		{
			if (filterBusinessObject is IFilterStripBusinessObjectInternals fboInternals && fboInternals.LayoutContext.IsEmpty)
			{
				fboInternals.LayoutContext = ID.Name;
			}
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

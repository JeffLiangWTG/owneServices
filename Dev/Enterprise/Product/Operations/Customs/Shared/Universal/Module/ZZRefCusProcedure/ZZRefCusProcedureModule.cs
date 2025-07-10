using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.Module
{
	public class ZZRefCusProcedureModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.Universal.ZZRefCusProcedure; }
		}

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.Universal.ZZRefCusProcedure);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ZZRefCusProcedureFilterStripControl(GridCollection, (ZZRefCusProcedureFilterStripBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new RefCusProcedureCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today);
		}

		public override Security.SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ZZRefCusProcedureFilterStripBusinessObject(Factory);
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowEdit
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}

		public override bool AllowView
		{
			get { return true; }
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}
	}
}

using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.Module
{
	public class ZZRefCusRulingModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.Universal.ZZRefCusRuling;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.GlobalCodes;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.Universal.ZZRefCusRuling);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ZZRefCusRulingFilterStripBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ZZRefCusRulingFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ZZRefCusRulingCombinedCollection(Factory);
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

		public override bool AllowCopyFilterGridHyperlinkToClipboard
		{
			get { return false; }
		}

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}
	}
}

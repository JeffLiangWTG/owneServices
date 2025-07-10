using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.Module
{
	public class RefDataGroupingModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.Universal.RefDataGrouping;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.GlobalCodes;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override bool AllowDelete => false;
		public override bool AllowNew => false;
		public override bool AllowEdit => false;
		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.Universal.RefDataGrouping);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new RefDataGroupingCollection(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new RefDataGroupingFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new RefDataGroupingFilterStripBusinessObject();
		}
	}
}

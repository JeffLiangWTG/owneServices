using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class CusRefPreferenceModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.CusRefPreference;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.GlobalCodes;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.CusRefPreference);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusRefPreferenceCollection(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CusRefPreferenceFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CusRefPreferenceFilterStripBusinessObject();
		}
	}
}

using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class CusRefRateCodeModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.CusRefRateCode;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.GlobalCodes;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override bool AllowDelete => false;
		public override bool AllowNew => true;
		public override bool AllowEdit => true;
		public override bool AllowCopyFilterGridHyperlinkToClipboard => true;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.CusRefRateCode);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CusRefRateCodeFilterSrtipBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CusRefRateCodeFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusRefRateCodeCollection(Factory);
		}

		protected override IZForm ShowEditForm(BusinessObject selectedBusinessObject)
		{
			IZForm form = null;
			if (selectedBusinessObject is CusRefRateCode rateCode && rateCode.CR7_RN_NKCountryCode != GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				Globals.Message.ShowError(Res.GetString("afe192e0-8173-4d5a-bee6-e62315c3c042", "Rate Codes that do not belong to your country cannot be edited."));
			}
			else
			{
				form = base.ShowEditForm(selectedBusinessObject);
			}

			return form;
		}
	}
}

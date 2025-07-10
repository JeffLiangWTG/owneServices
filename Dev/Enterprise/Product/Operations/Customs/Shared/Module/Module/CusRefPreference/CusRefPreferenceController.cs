using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class CusRefPreferenceController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.CusRefPreference;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.CusRefPreference;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusRefPreference);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.GlobalCodesView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.GlobalCodesNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.GlobalCodesEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.GlobalCodesDelete;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CusRefPreferenceForm((CusRefPreference)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return Factory.New<CusRefPreference>();
		}
	}
}

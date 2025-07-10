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
	class CusRefTariffVersionController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.Customs.CusRefTariffVersion;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.CusRefTariffVersion;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusRefTariffVersion);

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.GlobalTariffsDelete;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.GlobalTariffsEdit;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.GlobalTariffsNew;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.GlobalTariffsView;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CusRefTariffVersionForm((CusRefTariffVersion)businessEntity);
		}
	}
}

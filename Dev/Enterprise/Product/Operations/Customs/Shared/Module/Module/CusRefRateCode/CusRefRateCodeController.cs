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
	public class CusRefRateCodeController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.CusRefRateCode;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.CusRefRateCode;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusRefRateCode);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.GlobalCodesView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.GlobalCodesNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.GlobalCodesEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.GlobalCodesDelete;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CusRefRateCodeForm((CusRefRateCode)businessEntity);
		}
	}
}

using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class CourtesyNoticesOfLiquidationController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.CourtesyNoticesOfLiquidation;

		public override ControllerID ID => ControllerIDs.Customs.US.CourtesyNoticesOfLiquidation;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusLiquidation);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CourtesyNoticesOfLiquidationMessagesView;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override IZForm GetForm(IBusiness businessEntity) => new CusLiquidationForm((CusLiquidation)businessEntity);
	}
}

using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.Module
{
	public class RefHarbourRateController : ZController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefHarbourRateForm((RefHarbourRate)businessEntity);
		}

		public override ControllerID ID => ControllerIDs.Customs.Universal.RefHarbourRate;
		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.Universal.RefHarbourRate;
		public override Type TypeOfTopLevelBusinessObject => typeof(RefHarbourRate);
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;
		protected override SecurityCheckpoint CheckPointForView => Env.Security.GlobalHarbourRateView;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.GlobalHarbourRateNew;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.GlobalHarbourRateModify;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.GlobalHarbourRateDelete;
	}
}

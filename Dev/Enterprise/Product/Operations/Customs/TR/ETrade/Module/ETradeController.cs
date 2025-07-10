using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.TR.ETrade.Business;
using Enterprise.Customs.TR.ETrade.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.TR.ETrade.Module
{
	public class ETradeController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.TR.ETrade;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.TR.ETrade;

		public override Type TypeOfTopLevelBusinessObject => typeof(AsycudaManifestHeader);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Security

		protected override Security.SecurityCheckpoint CheckPointForView => Environment.Env.Security.TRETrade;

		protected override Security.SecurityCheckpoint CheckPointForNew => Environment.Env.Security.TRETrade;

		protected override Security.SecurityCheckpoint CheckPointForEdit => Environment.Env.Security.TRETrade;

		protected override Security.SecurityCheckpoint CheckPointForDelete => Environment.Env.Security.TRETrade;

		#endregion

		protected override IZForm GetForm(IBusiness businessEntity) => new ETradeForm((AsycudaManifestHeader)businessEntity);
	}
}

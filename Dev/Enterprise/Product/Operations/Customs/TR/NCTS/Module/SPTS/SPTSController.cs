using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.Customs.TR.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.TR.NCTS.Module
{
	public class SPTSController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.TR.SimplifiedProcedureTransitSystem;
		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.TR.SimplifiedProcedureTransitSystem;
		public override Type TypeOfTopLevelBusinessObject => typeof(SPTSHeader);
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Security

		protected override Security.SecurityCheckpoint CheckPointForView => Environment.Env.Security.SimplifiedProcedureTransitSystem;
		protected override Security.SecurityCheckpoint CheckPointForNew => Environment.Env.Security.SimplifiedProcedureTransitSystem;
		protected override Security.SecurityCheckpoint CheckPointForEdit => Environment.Env.Security.SimplifiedProcedureTransitSystem;
		protected override Security.SecurityCheckpoint CheckPointForDelete => Environment.Env.Security.SimplifiedProcedureTransitSystem;

		#endregion

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var sptsHeader = (SPTSHeader)businessEntity;
			return new SPTSHeaderForm(sptsHeader);
		}
	}
}


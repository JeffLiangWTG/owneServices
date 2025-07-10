using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI.Protest;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module.Protest
{
	public class ProtestController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.Protest;

		public override ControllerID ID => ControllerIDs.Customs.US.Protest;

		public override Type TypeOfTopLevelBusinessObject => typeof(JobDeclaration);

		protected override IZForm GetForm(IBusiness businessEntity) => new ProtestForm(new Business.Protest.Protest((JobDeclaration)businessEntity));

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			var protest = sourceEntity as Business.Protest.Protest;
			return base.GetLoadedBusinessEntityInLocalFactory(protest == null ? sourceEntity : protest.Declaration);
		}

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.USProtestModify;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.USProtestModify;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.USProtestModify;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.USProtestView;
	}
}

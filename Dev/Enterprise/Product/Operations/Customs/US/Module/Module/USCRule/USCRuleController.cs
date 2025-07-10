using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class USCRuleController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.Customs.US.USCRule;

		public override ModuleIdentifier ModuleID => null;

		public override Type TypeOfTopLevelBusinessObject => typeof(USCRule);

		protected override IZForm GetForm(IBusiness businessEntity) => new USCRuleForm((USCRule)businessEntity);

		protected override Security.SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override Security.SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override Security.SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override Security.SecurityCheckpoint CheckPointForView => Env.Security.USCustomsTariffRuleView;
	}
}

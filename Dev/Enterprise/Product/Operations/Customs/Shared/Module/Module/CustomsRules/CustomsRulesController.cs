using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class CustomsRulesController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.CustomsRules;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.CustomsRules;

		public override Type TypeOfTopLevelBusinessObject => typeof(CustomsRule);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CustomsRuleForm((CustomsRule)businessEntity);
		}
	}
}

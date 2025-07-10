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
	public abstract class CusCalculationRulesController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.CusCalculationRules;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.CusCalculationRules;

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CusCalculationRulesView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CusCalculationRulesNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CusCalculationRulesModify;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CusCalculationRulesDelete;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusCalculationRule);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CusCalculationRuleForm();
		}
	}
}

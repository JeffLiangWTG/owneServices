using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Workflow.Business;
using Enterprise.Workflow.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Workflow.Module
{
	class ProcessFieldChangeRuleController : ZController
	{
		public override ModuleIdentifier ModuleID => ModuleIDs.ProcessFieldChangeRule;
		public override ControllerID ID => ControllerIDs.ProcessFieldChangeRule;
		public override Type TypeOfTopLevelBusinessObject => typeof(ProcessFieldChangeRule);
		protected override IZForm GetForm(IBusiness businessEntity) => new ProcessFieldChangeRuleForm((ProcessFieldChangeRule)businessEntity);
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.ProcessFieldChangeRuleView;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.ProcessFieldChangeRuleEdit;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.ProcessFieldChangeRuleNew;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.ProcessFieldChangeRuleDelete;

		#endregion
	}
}

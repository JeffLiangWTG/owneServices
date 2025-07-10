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
	class ProcessCompanyLinkRuleController : ZController
	{
		public override ModuleIdentifier ModuleID => ModuleIDs.ProcessCompanyLinkRule;
		public override ControllerID ID => ControllerIDs.ProcessCompanyLinkRule;
		public override Type TypeOfTopLevelBusinessObject => typeof(ProcessCompanyLinkRule);
		protected override IZForm GetForm(IBusiness businessEntity) => new ProcessCompanyLinkRuleForm((ProcessCompanyLinkRule)businessEntity);
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.ProcessCompanyLinkRuleView;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.ProcessCompanyLinkRuleEdit;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.ProcessCompanyLinkRuleNew;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.ProcessCompanyLinkRuleDelete;

		#endregion
	}
}

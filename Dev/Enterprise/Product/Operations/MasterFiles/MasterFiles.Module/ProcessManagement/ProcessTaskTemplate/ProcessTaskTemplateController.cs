using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class ProcessTaskTemplateController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ProcessTemplates; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ProcessTemplates; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ProcessTaskTemplate); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ProcessTaskTemplateForm((ProcessTaskTemplate)businessEntity);
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.WorkflowTaskTemplatesView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.WorkflowTaskTemplatesEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.WorkflowTaskTemplatesNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.WorkflowTaskTemplatesDelete; }
		}

		protected override SecurityCheckpoint GetCheckPointForCopy(BusinessObject inMemorySourceEntity)
		{
			return Env.Security.WorkflowTaskTemplateCopyInactive.IsAllowed ? Env.Security.WorkflowTaskTemplateCopyInactive : base.GetCheckPointForCopy(inMemorySourceEntity);
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return (bizObject != null && bizObject is ProcessTaskTemplate processTaskTemplate && !processTaskTemplate.P0_IsActive) ? Env.Security.WorkflowTaskTemplatesEditInactive : Env.Security.WorkflowTaskTemplatesEdit;
		}

		#endregion
	}
}

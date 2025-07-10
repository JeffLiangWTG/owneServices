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
	public class WorkflowExceptionTypesController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID => ModuleIDs.WorkflowExceptionTypes;

		public override ControllerID ID => ControllerIDs.WorkflowExceptionTypes;

		public override Type TypeOfTopLevelBusinessObject => typeof(ProcessWorkflowExceptionType);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new WorkflowExceptionTypeForm((ProcessWorkflowExceptionType)businessEntity);
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WorkflowExceptionTypesView;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WorkflowExceptionTypesEdit;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.WorkflowExceptionTypesNew;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.WorkflowExceptionTypesDeactivate;

		#endregion
	}
}

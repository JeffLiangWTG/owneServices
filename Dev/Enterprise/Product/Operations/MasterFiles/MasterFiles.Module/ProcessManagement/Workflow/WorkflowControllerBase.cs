using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public abstract class WorkflowControllerBase : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override Type TypeOfTopLevelBusinessObject => typeof(ProcessTask);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "{0} Module does not support New and Delete functionalities", ID.Name));
		}

		public override IZForm ShowNewForm()
		{
			throw new ControllerShowNewFormNotSupportedException(string.Format(CultureInfo.InvariantCulture, "{0} Module does not support New functionality", ID.Name));
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			throw new ControllerShowDeleteFormNotSupportedException(string.Format(CultureInfo.InvariantCulture, "{0} Module does not support Delete functionality", ID.Name));
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			return ShowForm(sourceEntity, false);
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			return ShowForm(sourceEntity, true);
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WorkflowSection;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.WorkflowSection;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WorkflowSection;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.WorkflowSection;

		#endregion

		#region Implementation

		static IZForm ShowForm(BusinessObject sourceEntity, ZBool editAllowed)
		{
			var businessEntity = (ProcessTask)sourceEntity;
			IZForm parentForm = null;

			var parentControllerID = businessEntity.ParentControllerID;
			if (businessEntity.ParentControllerID != null && businessEntity.ParentBusinessObject != null)
			{
				parentForm = WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(businessEntity, () => ZControllerFactory.Create(parentControllerID), editAllowed);
			}

			return parentForm;
		}

		#endregion
	}
}

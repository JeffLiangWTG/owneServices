using System;
using CargoWise.EntityFramework;

namespace Enterprise.ProcessManagement.Business
{
	public class ProjectKeyViewModelValidation : ZValidation
	{
		public ProjectKeyViewModelValidation(ProjectKeyViewModel parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly ProjectKeyViewModel parent;

		public override Type AutoValidationType => typeof(ProjectKeyViewModelValidation);

		internal void ValidateProjectKey()
		{
			ValidateCalculatedProperty(parent.ProjectKeyInfo);
		}

		protected void CheckProjectKey()
		{
			if (!parent.ParentViewModel.ShouldImportAllProjects)
			{
				MandatoryValidation.CheckEntered(parent.ProjectKeyInfo);
			}
		}

		public override void ValidateAll()
		{
			ValidateProjectKey();
		}
	}
}

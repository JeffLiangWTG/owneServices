using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ProcessManagement.Business
{
	public class ProjectKeyViewModel : NonPersistentBusinessObject<ProjectKeyViewModelValidation>
	{
		public ProjectKeyViewModel(ProjectsToImportViewModel parentViewModel)
			: base(parentViewModel.Factory)
		{
			ParentViewModel = parentViewModel;
		}

		internal ProjectsToImportViewModel ParentViewModel { get; }

		[ResourceStringData("ProjectKeyViewModel.ProjectKey", Caption = "Project Key", FullDescription = "The value listed in the Key field of the project in Jira to be imported.")]
		public ZString ProjectKey
		{
			get => projectKey;
			set
			{
				SetNonPersistentPropertyValue(ProjectKeyInfo, ref projectKey, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateProjectKey();
				}
			}
		}

		ZString projectKey;

		public ZPropertyInfo ProjectKeyInfo => GetZPropertyInfo(nameof(ProjectKey));

		public override ProjectKeyViewModelValidation GetNewValidation() => new ProjectKeyViewModelValidation(this);
	}
}

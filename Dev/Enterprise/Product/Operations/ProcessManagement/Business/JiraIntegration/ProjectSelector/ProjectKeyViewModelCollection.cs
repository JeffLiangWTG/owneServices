using CargoWise.EntityFramework;

namespace Enterprise.ProcessManagement.Business
{
	public class ProjectKeyViewModelCollection : NonPersistentBusinessObjectCollection<ProjectKeyViewModel>
	{
		public ProjectKeyViewModelCollection(ProjectsToImportViewModel parentViewModel)
			: base(parentViewModel.Factory)
		{
			this.parentViewModel = parentViewModel;
		}

		readonly ProjectsToImportViewModel parentViewModel;

		protected override BusinessObject CreateNonPersistentBusinessObject() => new ProjectKeyViewModel(parentViewModel);
	}
}

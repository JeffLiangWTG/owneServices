using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Business
{
	public class RelatedDeclarationHelper
	{
		public RelatedDeclarationHelper(IFormPresenter presenter)
		{
			this.presenter = presenter;
		}

		BaseJobDeclaration relatedDeclaration;
		BaseJobDeclaration sourceDeclaration;
		readonly IFormPresenter presenter;

		public BaseJobDeclaration CreateNewRelated(BaseJobDeclaration sourceObject, string relationshipType = null)
		{
			if (sourceObject.IsInDatabase)
			{
				sourceDeclaration = sourceObject;
				relatedDeclaration = sourceDeclaration.GetNewRelatedDeclaration(new BusinessObjectFactory(), relationshipType);
				presenter.ShowNew(ControllerIDs.Customs.JobDeclaration, relatedDeclaration);
			}
			else
			{
				presenter.ShowError(Res.GetString("32bc8e12-7454-47d0-8d02-29f79dd5d4a5", "Please save the declaration before attempting to create a related declaration."), Res.GetString("dce83928-0fe5-4ad7-be1a-12cf58174df5", "Cannot create related declaration"));
			}
			return relatedDeclaration;
		}

		public void EditExisting(BusinessObject bo)
		{
			presenter.ShowEdit(ControllerIDs.Customs.JobDeclaration, bo);
		}

		public void ViewExisting(BusinessObject bo)
		{
			presenter.ShowView(ControllerIDs.Customs.JobDeclaration, bo);
		}
	}
}

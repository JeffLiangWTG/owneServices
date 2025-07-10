using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class RelatableActivityActionMenuStrategy : IRelatableActivityActionMenuStrategy
	{
		public void AddRelatableActivityMenuItemsIfApplicable(Form form)
		{
			var zForm = form as ZForm;
			if (zForm != null)
			{
				var relatableActivity = zForm.BusinessEntity as IRelatableActivity;
				if (relatableActivity != null && relatableActivity.SupportViewRelatedCommunications)
				{
					var viewRelatedCommunicationsMenuItem = ZFormMenuStrategy.AddActionsMenuItem(zForm, new ZMenuItem(ResString.GetMultilingualString("22f1a371-4a5d-460e-9eaa-3ca7d7f4dd83", "View Related Communications"),
								delegate
								{
									ZFormModaliser.Show(new RelatedCommunicationForm(new RelatedOrgSalesCallCollection(relatableActivity)), form);
								}));
					viewRelatedCommunicationsMenuItem.Name = "ViewRelatedCommunications";
				}

				var salesActivity = relatableActivity as ISalesRelationActivity;
				if (salesActivity != null && !(salesActivity is OrgSalesCall))
				{
					var viewSalesRelationsMenuItem = ZFormMenuStrategy.AddActionsMenuItem(zForm, new ZMenuItem(ResString.GetMultilingualString("3a721697-d90e-4c02-bdee-66c25dee51e8", "View Sales Relations"),
								delegate
								{
									ZFormModaliser.Show(new SalesRelationPopupForm(salesActivity.SalesRelationModel), form);
								}));
					viewSalesRelationsMenuItem.Name = "ViewSalesRelations";
				}
			}
		}
	}
}

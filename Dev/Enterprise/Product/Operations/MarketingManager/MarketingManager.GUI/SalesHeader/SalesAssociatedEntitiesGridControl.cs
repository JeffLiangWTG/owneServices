using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.GUI
{
	public partial class SalesAssociatedEntitiesGridControl : ZUserControl
	{
		public SalesAssociatedEntitiesGridControl()
		{
			InitializeComponent();
		}

		public new ISalesValue CurrentDataItem
		{
			get { return (ISalesValue)base.CurrentDataItem; }
		}

		void Grid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (grid.HitTest(e.X, e.Y).Row > -1)
			{
				OnAssociatedActivitiesGridRowDoubleClicked();
			}
		}

		protected void OnAssociatedActivitiesGridRowDoubleClicked()
		{
			if (grid.ListManager != null && grid.ListManager.Count > 0)
			{
				var associationPivot = grid.ListManager.GetCurrent() as OrgSalesValueAssociationPivot;
				if (associationPivot != null)
				{
					var associatedEntity = associationPivot.AssociatedEntity;
					if (associatedEntity != null)
					{
						var controllerId = associatedEntity.ControllerID;
						if (controllerId != null)
						{
							var controller = ZControllerFactory.Create(controllerId);
							controller.ShowEditForm((BusinessObject)associatedEntity);
						}
					}
				}
			}
		}
	}
}

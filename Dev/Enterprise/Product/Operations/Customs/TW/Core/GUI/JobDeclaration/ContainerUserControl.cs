using System.Linq;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.TW.GUI
{
	public partial class ContainerUserControl : BaseCustomsCusContainersWithTrackingUserControl
	{
		public ContainerUserControl()
		{
			InitializeComponent();
			ReSetBindingMember();
			CusContainersBoundGrid.InnerGrid.GridId = "GridLayoutb2j7ztwz5jEBb5tiCBrqlw=="; // DataImportWizard GridID.
		}

		private void ReSetBindingMember()
		{
			if (containersUserControl1.Controls.Find("ExportIsEmptyContainerCheckBox", true)?.FirstOrDefault() is ZCheckBox exportIsEmptyContainerCheckBox)
			{
				BindingSource.SetBindingMember(exportIsEmptyContainerCheckBox, "JC_IsEmptyContainer");
				exportIsEmptyContainerCheckBox.BindTo = "JC_IsEmptyContainer";
			}
		}
	}
}

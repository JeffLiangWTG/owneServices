using System;
using System.Windows.Forms;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using BaseAsycudaContainer = Enterprise.Customs.ManifestBase.AsycudaContainer;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class ContainersSelectionForm : ZChildForm
	{
		public ContainersSelectionForm(BaseAsycudaManifestHeaderCopyBO copyBO)
			: base(copyBO)
		{
			InitializeComponent();
		}

		public override string FormHeading => Res.GetString("06851452-f9c1-46bf-8e90-896ae4333792", "Containers Selection");

		void OkButton_Click(object sender, EventArgs e)
		{
			if (ContainersGrid.SelectedElements.Length == 0 || ContainersGrid.SelectedElements.Length > 1)
			{
				Globals.Message.ShowWarning(Res.GetString("692008fd-feb7-451d-acdb-623d89c517d8", "Please select one item."));
			}
			else
			{
				var headerCopyBO = DataSource as BaseAsycudaManifestHeaderCopyBO;
				var container = ContainersGrid.SelectedElements[0] as BaseAsycudaContainer;
				if (headerCopyBO != null && container != null)
				{
					headerCopyBO.SourceContainerToCopy = container;
					headerCopyBO.CopyValuesFromGlobalManifest();
				}

				Close();
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			if (Globals.Message.Show(Res.GetString("338a4bf1-0dc5-4fb1-a80b-fa8cd7ef4188", "Do you want to cancel the copy process?"), "Cancel Copying", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
			{
				Close();
			}
		}
	}
}

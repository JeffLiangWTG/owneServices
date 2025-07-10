using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Packing.GUI
{
	public partial class PackingPopupDialog : ZChildForm
	{
		#region Construction

		public PackingPopupDialog(PkgPackageJob packageJob)
			: base(packageJob)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();

			InitializeComponent();
			CaptionRenderingEnabled = true;
			TreeUserControl.Tree.AllowMultiSelectFromDifferentParents = true;
			TreeUserControl.ViewMode = PackingViewMode.NoEditing;
		}

		#endregion

		#region Attach

		public IEnumerable<PkgPackage> SelectedPackages
		{
			get;
			private set;
		}

		void AttachButton_Click(object sender, EventArgs e)
		{
			AttachPackages();
		}

		void AttachPackages()
		{
			if (!TreeUserControl.Tree.NodeSelector.IsAnyPackageSelected)
			{
				Globals.Message.ShowError(Res.GetString("cbb2c5a5-a657-400b-92f6-2c922be483be", "No packages are selected."));
			}
			else
			{
				SelectedPackages = TreeUserControl.Tree.SelectedPackages;
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		#endregion

		#region Cancel

		void CancelChangesButton_Click(object sender, EventArgs e)
		{
			CancelAttach();
		}

		void CancelAttach()
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		#endregion
	}
}

using System;

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ControllingBranchesForm : ZChildForm
	{
		public ControllingBranchesForm(OrgHeader org)
			: base(org)
		{
			InitializeComponent();
		}

		ZButton CloseButton;

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		ZGrid branchesGrid;

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}

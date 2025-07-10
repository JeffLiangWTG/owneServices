using System;
using CargoWise.Definitions;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbDepartmentForm : ZForm
	{
		public GlbDepartmentForm()
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
		}

		public GlbDepartmentForm(GlbDepartment department) : base(department)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ControlDpiScalingHelper.SetTop(ref PostingButtonsUserControl, MainStatusBar.Top - PostingButtonsUserControl.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);

			IsCostCentreCheckBox.Visible = (ClientHookLoader.Instance.Client == Clients.EDI);
		}

		public bool IsCostCentreCheckBoxVisible => IsCostCentreCheckBox.Visible;
	}
}

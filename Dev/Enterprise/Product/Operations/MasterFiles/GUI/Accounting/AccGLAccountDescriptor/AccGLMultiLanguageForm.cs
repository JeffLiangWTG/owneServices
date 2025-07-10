using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccGLAccountDescriptorForm : ZForm
	{
		public AccGLAccountDescriptorForm()
		{
			InitializeComponent();
		}

		public AccGLAccountDescriptorForm(AccGLAccountDescriptor bO)
			: base(bO)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			PlugIns.Add(ControllerIDs.Audit);
		}
		protected override void SaveInternal()
		{
			base.SaveInternal();
			this.PivotCollectionTabPage.TabVisible = BusinessEntity.ReportSetupVisible;
		}
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			PivotCollectionTabPage.TabVisible = BusinessEntity.ReportSetupVisible;
			ControlDpiScalingHelper.SetTop(ref PostingButtonsUserControl, MainStatusBar.Top - PostingButtonsUserControl.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
		}

		void LocalAccountTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = (BusinessEntity.AJ_Language == Core.Constants.Languages.ChineseSimplified || BusinessEntity.AJ_Language == Core.Constants.Languages.ChineseTraditional) &&
				!BusinessEntity.IsValidCharForChineseGLAccount(e.KeyChar);
		}

		public new AccGLAccountDescriptor BusinessEntity
		{
			get { return (AccGLAccountDescriptor)base.BusinessEntity; }
		}

		protected internal ZTemplateTabControl MainTabControl;	

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}

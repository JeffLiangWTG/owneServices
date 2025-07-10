using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.GUI
{
	public partial class CostingForm
	{
		private ClientInformationUserControl clientInformationUserControl1;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private CostingTabControl costingTabControl1;
		private ZTranslatableTextControl descriptionTextBox;
		internal RateEntryFilterStripControl rateEntryFilterStripControl;

		new void InitializeComponent()
		{
			this.clientInformationUserControl1 = new ClientInformationUserControl();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.costingTabControl1 = new CostingTabControl();
			this.descriptionTextBox = new ZTranslatableTextControl();
			this.rateEntryFilterStripControl = new RateEntryFilterStripControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 701, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(985);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Costing);
			// 
			// clientInformationUserControl1
			// 
			this.clientInformationUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.clientInformationUserControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			this.clientInformationUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.clientInformationUserControl1.Name = "clientInformationUserControl1";
			this.clientInformationUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 41, true);
			this.clientInformationUserControl1.TabIndex = 0;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(760, 675, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 2;
			// 
			// costingTabControl1
			// 
			this.costingTabControl1.AllowDrop = true;
			this.costingTabControl1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
			| System.Windows.Forms.AnchorStyles.Left
			| System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.costingTabControl1, ".");
			this.costingTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 137, true);
			this.costingTabControl1.Name = "costingTabControl1";
			this.costingTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 537, true);
			this.costingTabControl1.TabIndex = 1;
			// 
			// descriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.descriptionTextBox, "TH_GlobalRateDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Costing)(null)).TH_GlobalRateDescription);
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.descriptionTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.descriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 69, true);
			this.descriptionTextBox.Name = "descriptionTextBox";
			this.descriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.descriptionTextBox.TabIndex = 7;
			// 
			// rateEntryFilterStripControl1
			// 
			this.rateEntryFilterStripControl.AllowDrop = true;
			this.rateEntryFilterStripControl.CaptionRenderingEnabled = true;
			this.rateEntryFilterStripControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left
			| System.Windows.Forms.AnchorStyles.Right;
			this.rateEntryFilterStripControl.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.rateEntryFilterStripControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			this.rateEntryFilterStripControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(314, 0);
			this.rateEntryFilterStripControl.Name = "rateEntryFilterStripControl";
			this.rateEntryFilterStripControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(710, 136);
			this.rateEntryFilterStripControl.TabIndex = 9;
			// 
			// CostingForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 725, true);
			this.Controls.Add(this.rateEntryFilterStripControl);
			this.Controls.Add(this.descriptionTextBox);
			this.Controls.Add(this.costingTabControl1);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.clientInformationUserControl1);
			this.DataSourceAssemblyName = "Enterprise.Rating.Business";
			this.DataSourceType = typeof(Costing);
			this.DataSourceTypeName = "Enterprise.Rating.Business.Costing";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 725, true);
			this.Name = "CostingForm";
			this.Text = "CostingForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.clientInformationUserControl1, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.costingTabControl1, 0);
			this.Controls.SetChildIndex(this.descriptionTextBox, 0);
			this.Controls.SetChildIndex(this.rateEntryFilterStripControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}

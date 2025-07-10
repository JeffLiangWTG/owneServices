namespace Enterprise.MasterFiles.GUI
{
	public partial class DuplicateOrgForm
	{

		#region Windows Form Designer generated code

		protected Enterprise.MasterFiles.GUI.SimilarOrgsDisplayGrid SimilarOrgsDisplayGrid;
		internal protected Enterprise.ZArchitecture.GUI.ZButton SaveNewOrgButton;
		internal protected Enterprise.ZArchitecture.GUI.ZButton CancelSaveButton;
		private CargoWise.Windows.UI.KPanel WarningIconPanel;
		protected Enterprise.ZArchitecture.ZLabel WarningLabel;

		new void InitializeComponent()
		{
			this.WarningLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SimilarOrgsDisplayGrid = new Enterprise.MasterFiles.GUI.SimilarOrgsDisplayGrid();
			this.SaveNewOrgButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelSaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.WarningIconPanel = new CargoWise.Windows.UI.KPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SimilarOrgsDisplayGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 351, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(294);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(294);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// WarningLabel
			// 
			this.WarningLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DuplicateOrgForm|01625eed-3a41-45c8-a7e5-47fad62f4c6f", "Similar organizations already exist. Are you sure you wish to save a new organization?");
			this.WarningLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 16, true);
			this.WarningLabel.Name = "WarningLabel";
			this.WarningLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 32, true);
			this.WarningLabel.TabIndex = 2;
			// 
			// SimilarOrgsDisplayGrid
			// 
			this.SimilarOrgsDisplayGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SimilarOrgsDisplayGrid, "SimilarOrgMatches");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OS_Rank)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OH_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OH_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_Address1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_Address2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_City)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_State)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_PostCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OS_UNLOCO)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_Phone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_Fax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).LocalBusinessNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SimilarOrgMatches)).SyncRoot)).OH_Calc_Email)));
			this.SimilarOrgsDisplayGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 64, true);
			this.SimilarOrgsDisplayGrid.Name = "SimilarOrgsDisplayGrid";
			this.SimilarOrgsDisplayGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(587, 246, true);
			this.SimilarOrgsDisplayGrid.TabIndex = 3;
			// 
			// SaveNewOrgButton
			// 
			this.SaveNewOrgButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveNewOrgButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DuplicateOrgForm|13f65a50-55e2-4020-b2b4-c32c837c6ca6", "&Save New Organization");
			this.SaveNewOrgButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 318, true);
			this.SaveNewOrgButton.Name = "SaveNewOrgButton";
			this.SaveNewOrgButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 23, true);
			this.SaveNewOrgButton.TabIndex = 4;
			this.SaveNewOrgButton.Click += new System.EventHandler(this.SaveNewOrgButton_Click);
			// 
			// CancelSaveButton
			// 
			this.CancelSaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelSaveButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DuplicateOrgForm|1a003323-5f36-4f9f-955c-ce547c70acb2", "&Cancel");
			this.CancelSaveButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelSaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(518, 318, true);
			this.CancelSaveButton.Name = "CancelSaveButton";
			this.CancelSaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelSaveButton.TabIndex = 5;
			this.CancelSaveButton.Click += new System.EventHandler(this.CancelSaveButton_Click);
			// 
			// WarningIconPanel
			// 
			this.WarningIconPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.WarningIconPanel.Name = "WarningIconPanel";
			this.WarningIconPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 48, true);
			this.WarningIconPanel.TabIndex = 6;
			// 
			// DuplicateOrgForm
			// 
			this.CancelButton = this.CancelSaveButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 375, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DuplicateOrgForm|6be5f008-d8d8-45f6-b0f0-65b23d01091e", "Possible Duplicate Organization");
			this.Controls.Add(this.WarningLabel);
			this.Controls.Add(this.WarningIconPanel);
			this.Controls.Add(this.CancelSaveButton);
			this.Controls.Add(this.SaveNewOrgButton);
			this.Controls.Add(this.SimilarOrgsDisplayGrid);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			this.MinimizeBox = false;
			this.Name = "DuplicateOrgForm";
			this.Controls.SetChildIndex(this.SimilarOrgsDisplayGrid, 0);
			this.Controls.SetChildIndex(this.SaveNewOrgButton, 0);
			this.Controls.SetChildIndex(this.CancelSaveButton, 0);
			this.Controls.SetChildIndex(this.WarningIconPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.WarningLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SimilarOrgsDisplayGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

	}
}

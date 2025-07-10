namespace Enterprise.MasterFiles.GUI
{
	public partial class TemporaryOrgRemoverForm
	{

		#region Windows Form Designer generated code

		private Enterprise.ZArchitecture.ZGrid OrgGrid;
		internal Enterprise.ZArchitecture.GUI.ZButton DeleteButton;
		private Enterprise.ZArchitecture.GUI.ZButton CancelButtonX;

		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.OrgGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DeleteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButtonX = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OrgGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 454, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(518, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(241);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.TemporaryOrgRemover.Remover);
			// 
			// OrgGrid
			// 
			this.OrgGrid.AllowNavigation = false;
			this.OrgGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OrgGrid, "Organisations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TemporaryOrgRemover.Remover)(null)).Organisations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.TemporaryOrgRemover.TemporaryOrg)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TemporaryOrgRemover.Remover)(null)).Organisations)).SyncRoot)).IncludeInDelete)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TemporaryOrgRemover.TemporaryOrg)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TemporaryOrgRemover.Remover)(null)).Organisations)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TemporaryOrgRemover.TemporaryOrg)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TemporaryOrgRemover.Remover)(null)).Organisations)).SyncRoot)).FullName)));
			this.OrgGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrgRemoverForm|2784bf81-974f-4cd1-b2a4-6186d15cf6b9", "Delete");
			zCheckBoxColumnStyleInfo1.ColumnName = "IncludeInDelete";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrgRemoverForm|c885e5f7-4aad-46c6-8c63-3c99d645d64d", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrgRemoverForm|58269ea6-757f-4ed3-9971-05907b3df5b1", "Name");
			zTextBoxColumnStyleInfo2.ColumnName = "FullName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.OrgGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.OrgGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrgGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OrgGrid.GridId = "854e737d-5a11-4b95-ba81-7d21362088e1";
			this.OrgGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrgGrid.LayoutKey = "zGrid1";
			this.OrgGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 45, true);
			this.OrgGrid.Name = "OrgGrid";
			this.OrgGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 375, true);
			this.OrgGrid.TabIndex = 1;
			// 
			// DeleteButton
			// 
			this.DeleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DeleteButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrgRemoverForm|db5af1bc-0a74-4cb9-8afe-075a23a3e258", "Delete");
			this.DeleteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(306, 431, true);
			this.DeleteButton.Name = "DeleteButton";
			this.DeleteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 23, true);
			this.DeleteButton.TabIndex = 4;
			this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButtonX.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrgRemoverForm|b12fbfbf-def6-491e-8c17-394b3bc9a425", "Cancel");
			this.CancelButtonX.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(410, 431, true);
			this.CancelButtonX.Name = "CancelButtonX";
			this.CancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 23, true);
			this.CancelButtonX.TabIndex = 5;
			this.CancelButtonX.Click += new System.EventHandler(this.CancelButtonX_Click);
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrgRemoverForm|7d7e8bfa-a3d9-47a9-9a21-621c5851fbd0", "", "These Temporary Organizations are not used anywhere in your system. \r\nThey can safely be deleted if required.");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 10, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(507, 13, true);
			this.zLabel1.TabIndex = 6;
			// 
			// TemporaryOrgRemoverForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(518, 478, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrgRemoverForm|ecf52612-33bc-4d54-b7c5-bea312bade59", "Delete Temporary Organizations");
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.DeleteButton);
			this.Controls.Add(this.CancelButtonX);
			this.Controls.Add(this.OrgGrid);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.TemporaryOrgRemover.Remover);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 279, true);
			this.Name = "TemporaryOrgRemoverForm";
			this.Controls.SetChildIndex(this.OrgGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			this.Controls.SetChildIndex(this.DeleteButton, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OrgGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}

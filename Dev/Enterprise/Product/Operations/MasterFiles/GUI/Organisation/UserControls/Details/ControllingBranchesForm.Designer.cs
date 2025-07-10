namespace Enterprise.MasterFiles.GUI
{
	public partial class ControllingBranchesForm
	{
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.branchesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.branchesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 188, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ControllingBranchesForm|cd1cdcef-cfa8-4a74-81c3-e98ce1af2a53", "Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(577, 161, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CloseButton.TabIndex = 12;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// branchesGrid
			// 
			this.branchesGrid.AllowNavigation = false;
			this.branchesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.branchesGrid, "CompanyDataCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyDataCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgCompanyData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyDataCollection)).SyncRoot)).OB_GC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCompanyData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyDataCollection)).SyncRoot)).Company.GC_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgCompanyData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyDataCollection)).SyncRoot)).OB_GB_ControllingBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCompanyData)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyDataCollection)).SyncRoot)).ControllingBranch.GB_BranchName)));
			this.branchesGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OB_GC";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo1.ColumnName = "Company+GC_Name";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "OB_GB_ControllingBranch";
			zGuidFindBoxColumnStyleInfo2.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.ColumnName = "ControllingBranch+GB_BranchName";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			this.branchesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.branchesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.branchesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.branchesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.branchesGrid.GridId = "98e1af6c-bd01-4114-ba3b-ad0bc4dfd39f";
			this.branchesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.branchesGrid.IsCustomiseMenuVisible = false;
			this.branchesGrid.LayoutKey = "branchesGrid";
			this.branchesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.branchesGrid.Name = "branchesGrid";
			this.branchesGrid.ReadOnly = true;
			this.branchesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 152, true);
			this.branchesGrid.TabIndex = 7;
			// 
			// ControllingBranchesForm
			// 
			this.AcceptButton = this.CloseButton;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 212, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ControllingBranchesForm|6551b2b7-112b-4417-8817-75d706c68be2", "Controlling Branches");
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.branchesGrid);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 250, true);
			this.Name = "ControllingBranchesForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.branchesGrid, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.branchesGrid)).EndInit();
			this.ResumeLayout(false);
		}

	}
}

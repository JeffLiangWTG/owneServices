using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.GUI
{
	partial class CommissionAgreementItemsControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.ProductsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ServicesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SubModulesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ConditionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.tableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ProductsGrid)).BeginInit();
			this.ProductsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ServicesGrid)).BeginInit();
			this.ServicesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SubModulesGrid)).BeginInit();
			this.SubModulesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConditionsGrid)).BeginInit();
			this.ConditionsGrid.SuspendLayout();
			this.tableLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgCommissionAgreement);
			// 
			// ProductsGrid
			// 
			this.ProductsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ProductsGrid, "ProductItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).ProductItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).ProductItems)).SyncRoot)).CAI_IsInclude)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).ProductItems)).SyncRoot)).CAI_Code)));
			this.ProductsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "CAI_IsInclude";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(43);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e6650746-c80a-440b-9387-654b49c5644d", "Product");
			zDropEditColumnStyleInfo1.ColumnName = "CAI_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			this.ProductsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ProductsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ProductsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProductsGrid.GridId = "ed3160d8-0dbc-466a-8beb-621a9db0cb0b";
			this.ProductsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProductsGrid.LayoutKey = "ProductsGrid";
			this.ProductsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 2, true);
			this.ProductsGrid.Name = "ProductsGrid";
			this.ProductsGrid.RowHeadersVisible = false;
			this.ProductsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 195, true);
			this.ProductsGrid.TabIndex = 0;
			// 
			// ServicesGrid
			// 
			this.ServicesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ServicesGrid, "ProductItems.ChildServiceItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).ProductItems)).SyncRoot)).ChildServiceItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).ProductItems)).SyncRoot)).ChildServiceItems)).SyncRoot)).CAI_IsInclude)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).ProductItems)).SyncRoot)).ChildServiceItems)).SyncRoot)).CAI_Code)));
			this.ServicesGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo2.ColumnName = "CAI_IsInclude";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(43);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dbb65af6-b260-49a0-884a-e0dc8645f542", "Service");
			zDropEditColumnStyleInfo2.ColumnName = "CAI_Code";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			this.ServicesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ServicesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ServicesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ServicesGrid.GridId = "ee70097d-bdce-4a8f-b2c8-6091f70409ad";
			this.ServicesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ServicesGrid.LayoutKey = "ServicesGrid";
			this.ServicesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.ServicesGrid.Name = "ServicesGrid";
			this.ServicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 195, true);
			this.ServicesGrid.TabIndex = 1;
			// 
			// SubModulesGrid
			// 
			this.SubModulesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SubModulesGrid, "ProductItems.ChildServiceItems.ChildSubModuleItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).ProductItems)).SyncRoot)).ChildServiceItems)).SyncRoot)).ChildSubModuleItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).ProductItems)).SyncRoot)).ChildServiceItems)).SyncRoot)).ChildSubModuleItems)).SyncRoot)).CAI_IsInclude)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).ProductItems)).SyncRoot)).ChildServiceItems)).SyncRoot)).ChildSubModuleItems)).SyncRoot)).CAI_Code)));
			this.SubModulesGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo3.ColumnName = "CAI_IsInclude";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(43);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4421890D-A745-4E38-9A0A-EBE8DEEACE71", "Mode");
			zDropEditColumnStyleInfo3.ColumnName = "CAI_Code";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			this.SubModulesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.SubModulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.SubModulesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SubModulesGrid.GridId = "7e77ea99-2f44-490a-a6ae-33eac6540577";
			this.SubModulesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SubModulesGrid.LayoutKey = "SubModulesGrid";
			this.SubModulesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 2, true);
			this.SubModulesGrid.Name = "SubModulesGrid";
			this.SubModulesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 195, true);
			this.SubModulesGrid.TabIndex = 2;
			// 
			// ConditionsGrid
			// 
			this.ConditionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ConditionsGrid, "ProductItems.ConditionCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).ProductItems)).SyncRoot)).ConditionCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementItemCondition)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).ProductItems)).SyncRoot)).ConditionCollection)).SyncRoot)).CIC_RL_NKOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementItemCondition)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).ProductItems)).SyncRoot)).ConditionCollection)).SyncRoot)).CIC_RL_NKDestination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementItemCondition)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).ProductItems)).SyncRoot)).ConditionCollection)).SyncRoot)).CIC_Mode)));
			this.ConditionsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("41AF150E-A53A-4153-A23A-08396727174B", "Origin");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CIC_RL_NKOrigin";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CA2609AD-B384-4BDE-A1B7-68E62D85EEB4", "Destination");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "CIC_RL_NKDestination";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.ColumnName = "CIC_Mode";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EEB5BE6D-1322-4B0C-A0A1-CFBA05864DFC", "Mode");
			this.ConditionsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ConditionsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.ConditionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ConditionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConditionsGrid.GridId = "49E5C1F8-234F-45B7-BE62-7AD96D575444";
			this.ConditionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ConditionsGrid.LayoutKey = "ConditionsGrid";
			this.ConditionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 2, true);
			this.ConditionsGrid.Name = "ConditionsGrid";
			this.ConditionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 195, true);
			this.ConditionsGrid.TabIndex = 3;
			// 
			// tableLayoutPanel
			// 
			this.tableLayoutPanel.ColumnCount = 4;
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 66.66666F));
			this.tableLayoutPanel.Controls.Add(this.ConditionsGrid, 3, 0);
			this.tableLayoutPanel.Controls.Add(this.SubModulesGrid, 2, 0);
			this.tableLayoutPanel.Controls.Add(this.ServicesGrid, 1, 0);
			this.tableLayoutPanel.Controls.Add(this.ProductsGrid, 0, 0);
			this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tableLayoutPanel.Name = "tableLayoutPanel";
			this.tableLayoutPanel.RowCount = 1;
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(16)));
			this.tableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 200, true);
			this.tableLayoutPanel.TabIndex = 0;
			// 
			// CommissionAgreementItemsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.tableLayoutPanel);
			this.Name = "CommissionAgreementItemsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 200, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ProductsGrid)).EndInit();
			this.ProductsGrid.ResumeLayout(false);
			this.ProductsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ServicesGrid)).EndInit();
			this.ServicesGrid.ResumeLayout(false);
			this.ServicesGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SubModulesGrid)).EndInit();
			this.SubModulesGrid.ResumeLayout(false);
			this.SubModulesGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConditionsGrid)).EndInit();
			this.ConditionsGrid.ResumeLayout(false);
			this.ConditionsGrid.PerformLayout();
			this.tableLayoutPanel.ResumeLayout(false);
			this.tableLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid ProductsGrid;
		private ZArchitecture.ZGrid ServicesGrid;
		private ZArchitecture.ZGrid SubModulesGrid;
		private ZArchitecture.ZGrid ConditionsGrid;
		private CargoWise.Windows.UI.KTableLayoutPanel tableLayoutPanel;
	}
}

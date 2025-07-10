namespace Enterprise.eTail.GUI
{
	partial class HVLVSelectConsignmentForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.gridConsignments = new Enterprise.ZArchitecture.ZGrid();
			this.mainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.btnMerge = new Enterprise.ZArchitecture.GUI.ZButton();
			this.btnIgnore = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridConsignments)).BeginInit();
			this.gridConsignments.SuspendLayout();
			this.mainPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 410, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.eTail.Business.HVLVCommonConsigneeConsignmentCollection);
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("df31bf5c-ac47-40d7-a8e5-c458904ef704", "Other Consignments that match Consignee details exist in this Shipment. You may select one or more to merge with Consignment {0} and create one standalone declaration.");
			this.zLabel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(15, 0, 15, 0, true);
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 64, true);
			this.zLabel1.TabIndex = 1;
			this.zLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// gridConsignments
			// 
			this.BindingSource.SetBindingMember(this.gridConsignments, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.eTail.Business.HVLVConsignment)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVConsignment)(null)).HVC_ConsignmentId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVConsignment)(null)).HVC_ConsigneeName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVConsignment)(null)).HVC_ConsigneeAddress1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVConsignment)(null)).HVC_ConsigneeAddress2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVConsignment)(null)).HVC_ConsigneePostcode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVConsignment)(null)).HVC_ConsigneeCity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVConsignment)(null)).HVC_ConsigneeState)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVConsignment)(null)).HVC_RN_NKConsigneeCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVConsignment)(null)).HVC_WaybillNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVConsignment)(null)).HVC_ShipperReference)));
			this.gridConsignments.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "HVC_ConsignmentId";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "HVC_ConsigneeName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "HVC_ConsigneeAddress1";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "HVC_ConsigneeAddress2";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "HVC_ConsigneePostcode";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "HVC_ConsigneeCity";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "HVC_ConsigneeState";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "HVC_RN_NKConsigneeCountryCode";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.ColumnName = "HVC_WaybillNumber";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.ColumnName = "HVC_ShipperReference";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.gridConsignments.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.gridConsignments.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.gridConsignments.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.gridConsignments.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.gridConsignments.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.gridConsignments.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.gridConsignments.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.gridConsignments.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.gridConsignments.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.gridConsignments.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.gridConsignments.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.gridConsignments.GridId = "5601eb7f-de0e-41d9-9fb5-5ce0f32213c3";
			this.gridConsignments.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gridConsignments.IsWholeRowSelectedOnClick = true;
			this.gridConsignments.LayoutKey = "gridConsignments";
			this.gridConsignments.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 80, true);
			this.gridConsignments.Name = "gridConsignments";
			this.gridConsignments.ReadOnly = true;
			this.gridConsignments.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 350, true);
			this.gridConsignments.TabIndex = 2;
			this.gridConsignments.SelectedRowsChangedInMouseDown += GridConsignments_SelectedRowsChangedInMouseDown;
			// 
			// mainPanel
			// 
			this.mainPanel.Controls.Add(this.zLabel1);
			this.mainPanel.Controls.Add(this.gridConsignments);
			this.mainPanel.Controls.Add(this.btnMerge);
			this.mainPanel.Controls.Add(this.btnIgnore);
			this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainPanel.Name = "mainPanel";
			this.mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 470, true);
			this.mainPanel.TabIndex = 1;
			// 
			// btnMerge
			// 
			this.btnMerge.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("86eaa9fe-8750-46e0-8470-aaf9cec9d670", "Merge");
			this.btnMerge.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			this.btnMerge.Enabled = false;
			this.btnMerge.IsCaptionOverridden = false;
			this.btnMerge.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(580, 440, true);
			this.btnMerge.Name = "btnMerge";
			this.btnMerge.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.btnMerge.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.btnMerge.TabIndex = 3;
			this.btnMerge.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.btnMerge.ToolTipCaption = null;
			this.btnMerge.Click += ButtonClick_Merge;
			// 
			// btnIgnore
			// 
			this.btnIgnore.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("ec909189-7c03-4478-8a5e-2ce061a3de0e", "Ignore");
			this.btnIgnore.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			this.btnIgnore.IsCaptionOverridden = false;
			this.btnIgnore.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(665, 440, true);
			this.btnIgnore.Name = "btnIgnore";
			this.btnIgnore.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.btnIgnore.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.btnIgnore.TabIndex = 4;
			this.btnIgnore.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.btnIgnore.ToolTipCaption = null;
			this.btnIgnore.Click += ButtonClick_Ignore;
			// 
			// HVLVSelectConsignmentForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("816fc2ec-5903-42f7-81b0-9b951d38fdc4", "Merge Declaration with Consignment {0}");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 470, true);
			this.Controls.Add(this.mainPanel);
			this.DataSourceType = typeof(Enterprise.eTail.Business.HVLVCommonConsigneeConsignmentCollection);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "HVLVSelectConsignmentForm";
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridConsignments)).EndInit();
			this.gridConsignments.ResumeLayout(false);
			this.gridConsignments.PerformLayout();
			this.mainPanel.ResumeLayout(false);
			this.mainPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZGrid gridConsignments;
		private ZArchitecture.GUI.ZButton btnMerge;
		private ZArchitecture.GUI.ZButton btnIgnore;
		private ZArchitecture.GUI.ZPanel mainPanel;
	}
}

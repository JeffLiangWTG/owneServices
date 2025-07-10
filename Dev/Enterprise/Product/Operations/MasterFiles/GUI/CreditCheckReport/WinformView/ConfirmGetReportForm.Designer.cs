namespace Enterprise.MasterFiles.GUI
{
	partial class ConfirmGetReportForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.confirmGetReportTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.operationDetailLabel = new Enterprise.ZArchitecture.ZLabel();
			this.detailTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.companyNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.companyAddressLabel = new Enterprise.ZArchitecture.ZLabel();
			this.cityStatePostInfoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.identityItemsLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.buttonsFlowLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.getReportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.confirmGetReportTableLayoutPanel.SuspendLayout();
			this.detailTableLayoutPanel.SuspendLayout();
			this.buttonsFlowLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.GUI.ConfirmGetReportInfoModel);
			// 
			// confirmGetReportTableLayoutPanel
			// 
			this.confirmGetReportTableLayoutPanel.AutoSize = true;
			this.confirmGetReportTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.confirmGetReportTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.confirmGetReportTableLayoutPanel.Controls.Add(this.operationDetailLabel, 0, 0);
			this.confirmGetReportTableLayoutPanel.Controls.Add(this.detailTableLayoutPanel, 0, 2);
			this.confirmGetReportTableLayoutPanel.Controls.Add(this.buttonsFlowLayoutPanel, 0, 4);
			this.confirmGetReportTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.confirmGetReportTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.confirmGetReportTableLayoutPanel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 0, true);
			this.confirmGetReportTableLayoutPanel.Name = "confirmGetReportTableLayoutPanel";
			this.confirmGetReportTableLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(27, 8, 4, 0, true);
			this.confirmGetReportTableLayoutPanel.RowCount = 5;
			this.confirmGetReportTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.confirmGetReportTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(13)));
			this.confirmGetReportTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.confirmGetReportTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(13)));
			this.confirmGetReportTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.confirmGetReportTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(9)));
			this.confirmGetReportTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 164, true);
			this.confirmGetReportTableLayoutPanel.TabIndex = 0;
			// 
			// operationDetailLabel
			// 
			this.operationDetailLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.operationDetailLabel, "OperationDetail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.GUI.ConfirmGetReportInfoModel)(null)).OperationDetail)));
			this.operationDetailLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.operationDetailLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
			this.operationDetailLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 8, true);
			this.operationDetailLabel.Name = "operationDetailLabel";
			this.operationDetailLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(7, 10, 10, 10, true);
			this.operationDetailLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 36, true);
			this.operationDetailLabel.TabIndex = 1;
			this.operationDetailLabel.UseMnemonic = false;
			// 
			// detailTableLayoutPanel
			// 
			this.detailTableLayoutPanel.AutoSize = true;
			this.detailTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.detailTableLayoutPanel.ColumnCount = 2;
			this.detailTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 56F));
			this.detailTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 44F));
			this.detailTableLayoutPanel.Controls.Add(this.companyNameLabel, 0, 0);
			this.detailTableLayoutPanel.Controls.Add(this.companyAddressLabel, 0, 1);
			this.detailTableLayoutPanel.Controls.Add(this.cityStatePostInfoLabel, 0, 2);
			this.detailTableLayoutPanel.Controls.Add(this.identityItemsLayoutPanel, 1, 0);
			this.detailTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 59, true);
			this.detailTableLayoutPanel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 0, true);
			this.detailTableLayoutPanel.Name = "detailTableLayoutPanel";
			this.detailTableLayoutPanel.RowCount = 3;
			this.detailTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.detailTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.detailTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.detailTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 53, true);
			this.detailTableLayoutPanel.TabIndex = 2;
			// 
			// companyNameLabel
			// 
			this.companyNameLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.companyNameLabel, "CompanyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.GUI.ConfirmGetReportInfoModel)(null)).CompanyName)));
			this.companyNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.companyNameLabel.IsFontBold = true;
			this.companyNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
			this.companyNameLabel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 0, true);
			this.companyNameLabel.Name = "companyNameLabel";
			this.companyNameLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(7, 0, 0, 0, true);
			this.companyNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(7, 13, true);
			this.companyNameLabel.TabIndex = 3;
			this.companyNameLabel.UseMnemonic = false;
			// 
			// companyAddressLabel
			// 
			this.companyAddressLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.companyAddressLabel, "CompanyAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.GUI.ConfirmGetReportInfoModel)(null)).CompanyAddress)));
			this.companyAddressLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
			this.companyAddressLabel.ForeColor = System.Drawing.Color.Gray;
			this.companyAddressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 13, true);
			this.companyAddressLabel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 0, true);
			this.companyAddressLabel.Name = "companyAddressLabel";
			this.companyAddressLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(7, 4, 0, 0, true);
			this.companyAddressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(7, 20, true);
			this.companyAddressLabel.TabIndex = 4;
			this.companyAddressLabel.UseMnemonic = false;
			// 
			// cityStatePostInfoLabel
			// 
			this.cityStatePostInfoLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.cityStatePostInfoLabel, "CityStatePostInfo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.GUI.ConfirmGetReportInfoModel)(null)).CityStatePostInfo)));
			this.cityStatePostInfoLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
			this.cityStatePostInfoLabel.ForeColor = System.Drawing.Color.Gray;
			this.cityStatePostInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 33, true);
			this.cityStatePostInfoLabel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 0, true);
			this.cityStatePostInfoLabel.Name = "cityStatePostInfoLabel";
			this.cityStatePostInfoLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(7, 4, 0, 0, true);
			this.cityStatePostInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(7, 20, true);
			this.cityStatePostInfoLabel.TabIndex = 5;
			this.cityStatePostInfoLabel.UseMnemonic = false;
			// 
			// identityItemsLayoutPanel
			// 
			this.identityItemsLayoutPanel.AutoSize = true;
			this.identityItemsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.identityItemsLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.identityItemsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 0, true);
			this.identityItemsLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.identityItemsLayoutPanel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 0, true);
			this.identityItemsLayoutPanel.Name = "identityItemsLayoutPanel";
			this.identityItemsLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 0, 0, true);
			this.detailTableLayoutPanel.SetRowSpan(this.identityItemsLayoutPanel, 3);
			this.identityItemsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 53, true);
			this.identityItemsLayoutPanel.TabIndex = 6;
			// 
			// buttonsFlowLayoutPanel
			// 
			this.buttonsFlowLayoutPanel.Controls.Add(this.getReportButton);
			this.buttonsFlowLayoutPanel.Controls.Add(this.cancelButton);
			this.buttonsFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.buttonsFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.buttonsFlowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 126, true);
			this.buttonsFlowLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 3, 3, true);
			this.buttonsFlowLayoutPanel.Name = "buttonsFlowLayoutPanel";
			this.buttonsFlowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 35, true);
			this.buttonsFlowLayoutPanel.TabIndex = 7;
			// 
			// getReportButton
			// 
			this.getReportButton.BackColor = System.Drawing.Color.DeepSkyBlue;
			this.BindingSource.SetBindingMember(this.getReportButton, "ActionName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.GUI.ConfirmGetReportInfoModel)(null)).ActionName)));
			this.getReportButton.FlatAppearance.BorderSize = 0;
			this.getReportButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.getReportButton.ForeColor = System.Drawing.Color.White;
			this.getReportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 2, true);
			this.getReportButton.Name = "getReportButton";
			this.getReportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 26, true);
			this.getReportButton.TabIndex = 9;
			this.getReportButton.ToolTipCaption = null;
			this.getReportButton.UseVisualStyleBackColor = false;
			this.getReportButton.Click += new System.EventHandler(this.GetReportButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.BackColor = System.Drawing.Color.DimGray;
			this.BindingSource.SetBindingMember(this.cancelButton, "Cancel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.GUI.ConfirmGetReportInfoModel)(null)).Cancel)));
			this.cancelButton.FlatAppearance.BorderSize = 0;
			this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cancelButton.ForeColor = System.Drawing.Color.Black;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 2, true);
			this.cancelButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 2, 8, 2, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 26, true);
			this.cancelButton.TabIndex = 8;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = false;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// ConfirmGetReportForm
			// 
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(225)))), ((int)(((byte)(228)))));
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 164, true);
			this.Controls.Add(this.confirmGetReportTableLayoutPanel);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ConfirmGetReportForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.confirmGetReportTableLayoutPanel.ResumeLayout(false);
			this.confirmGetReportTableLayoutPanel.PerformLayout();
			this.detailTableLayoutPanel.ResumeLayout(false);
			this.detailTableLayoutPanel.PerformLayout();
			this.buttonsFlowLayoutPanel.ResumeLayout(false);
			this.buttonsFlowLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel confirmGetReportTableLayoutPanel;
		private CargoWise.Windows.UI.KTableLayoutPanel detailTableLayoutPanel;
		private CargoWise.Windows.UI.KFlowLayoutPanel identityItemsLayoutPanel;
		private CargoWise.Windows.UI.KFlowLayoutPanel buttonsFlowLayoutPanel;
		private Enterprise.ZArchitecture.ZLabel operationDetailLabel;
		private Enterprise.ZArchitecture.ZLabel companyNameLabel;
		private Enterprise.ZArchitecture.ZLabel companyAddressLabel;
		private Enterprise.ZArchitecture.ZLabel cityStatePostInfoLabel;
		private Enterprise.ZArchitecture.GUI.ZButton getReportButton;
		private Enterprise.ZArchitecture.GUI.ZButton cancelButton;
	}
}

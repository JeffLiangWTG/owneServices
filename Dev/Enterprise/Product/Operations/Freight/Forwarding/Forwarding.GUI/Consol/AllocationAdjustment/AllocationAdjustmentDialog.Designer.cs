namespace Enterprise.Freight.Forwarding.GUI
{
	partial class AllocationAdjustmentDialog
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AdjustmentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CancelZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AdjustButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AuthorizationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LoginTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AdjustmentsGrid)).BeginInit();
			this.AuthorizationGroupBox.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 293, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.AllocationAdjustmentsSecurity);
			// 
			// MessageLabel
			// 
			this.MessageLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 76, true);
			this.MessageLabel.TabIndex = 2;
			// 
			// AdjustmentsGrid
			// 
			this.AdjustmentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdjustmentsGrid, "Adjustments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AllocationAdjustmentsSecurity)(null)).Adjustments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AllocationAdjustment)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AllocationAdjustmentsSecurity)(null)).Adjustments)).SyncRoot)).UniqueConsignRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AllocationAdjustment)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AllocationAdjustmentsSecurity)(null)).Adjustments)).SyncRoot)).AllocatedWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AllocationAdjustment)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AllocationAdjustmentsSecurity)(null)).Adjustments)).SyncRoot)).NewAllocatedWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AllocationAdjustment)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AllocationAdjustmentsSecurity)(null)).Adjustments)).SyncRoot)).AllocatedVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AllocationAdjustment)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AllocationAdjustmentsSecurity)(null)).Adjustments)).SyncRoot)).NewAllocatedVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AllocationAdjustment)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AllocationAdjustmentsSecurity)(null)).Adjustments)).SyncRoot)).AllocatedChargeable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AllocationAdjustment)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AllocationAdjustmentsSecurity)(null)).Adjustments)).SyncRoot)).NewAllocatedChargeable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AllocationAdjustment)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AllocationAdjustmentsSecurity)(null)).Adjustments)).SyncRoot)).AllocatedShipmentCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AllocationAdjustment)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AllocationAdjustmentsSecurity)(null)).Adjustments)).SyncRoot)).NewAllocatedShipmentCount)));
			this.AdjustmentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocationAdjustmentDialog|e1dfc855-cea3-4f2a-8be7-8b628dc332ac", "Consol ID");
			zTextBoxColumnStyleInfo1.ColumnName = "UniqueConsignRef";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocationAdjustmentDialog|7b48bd76-56ea-4f0f-a70d-a5e2b8782a13", "Weight");
			zCalcEditColumnStyleInfo1.ColumnName = "AllocatedWeight";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocationAdjustmentDialog|2a8c61f3-71cf-4200-98e7-7b70b8f2d4ec", "New Weight");
			zCalcEditColumnStyleInfo2.ColumnName = "NewAllocatedWeight";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocationAdjustmentDialog|f33d2bb6-ed5f-4292-9d44-439b4375fa49", "UW");
			zTextBoxColumnStyleInfo2.ColumnName = "AllocatedWeightUnit";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocationAdjustmentDialog|c8ef300a-0476-43df-8f20-630ef02408ab", "Volume");
			zCalcEditColumnStyleInfo3.ColumnName = "AllocatedVolume";
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocationAdjustmentDialog|7e18dc72-b0b3-4ec8-bd67-442df821ecfd", "New Volume");
			zCalcEditColumnStyleInfo4.ColumnName = "NewAllocatedVolume";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocationAdjustmentDialog|488f6ba4-e0a8-4e52-8925-f915f9dafb12", "UV");
			zTextBoxColumnStyleInfo3.ColumnName = "AllocatedVolumeUnit";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocationAdjustmentDialog|cafd5981-b4f4-40b4-aa45-53d8d3956515", "Chargeable");
			zCalcEditColumnStyleInfo5.ColumnName = "AllocatedChargeable";
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocationAdjustmentDialog|4f65be9a-25f6-44b3-8461-4b7277f2f959", "New Chargeable");
			zCalcEditColumnStyleInfo6.ColumnName = "NewAllocatedChargeable";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocationAdjustmentDialog|fa00ef14-a2f9-4676-8219-c13e403cd989", "UC");
			zTextBoxColumnStyleInfo4.ColumnName = "AllocatedChargeableUnit";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocationAdjustmentDialog|ef52963b-a33b-4597-b417-39e70db3a177", "Ship. Count");
			zCalcEditColumnStyleInfo7.ColumnName = "AllocatedShipmentCount";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocationAdjustmentDialog|e8f8f4aa-e463-4bdd-b067-7eb31b2a7084", "New Ship. Count");
			zCalcEditColumnStyleInfo8.ColumnName = "NewAllocatedShipmentCount";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.AdjustmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AdjustmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.AdjustmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.AdjustmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AdjustmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.AdjustmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.AdjustmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AdjustmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.AdjustmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.AdjustmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.AdjustmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.AdjustmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.AdjustmentsGrid.GridId = "15a91b9d-deec-403a-86b0-7a3364dc47d9";
			this.AdjustmentsGrid.CopySelectedRowsAllowed = true;
			this.AdjustmentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdjustmentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdjustmentsGrid.LayoutKey = "zGrid1";
			this.AdjustmentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 76, true);
			this.AdjustmentsGrid.Name = "AdjustmentsGrid";
			this.AdjustmentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 173, true);
			this.AdjustmentsGrid.TabIndex = 2;
			// 
			// CancelZButton
			// 
			this.CancelZButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocationAdjustmentDialog|ffb12b5a-7dbd-42b1-bfe1-45dd12553265", "Cancel");
			this.CancelZButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(702, 10, true);
			this.CancelZButton.Name = "CancelZButton";
			this.CancelZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.CancelZButton.TabIndex = 1;
			this.CancelZButton.UseVisualStyleBackColor = true;
			this.CancelZButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// AdjustButton
			// 
			this.AdjustButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocationAdjustmentDialog|e69ad1ad-9fb5-49a5-8cc6-5a8ad10d842a", "Adjust");
			this.AdjustButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(596, 10, true);
			this.AdjustButton.Name = "AdjustButton";
			this.AdjustButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.AdjustButton.TabIndex = 0;
			this.AdjustButton.UseVisualStyleBackColor = true;
			this.AdjustButton.Click += new System.EventHandler(this.AdjustButton_Click);
			// 
			// AuthorizationGroupBox
			// 
			this.AuthorizationGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocationAdjustmentDialog|bc69c29e-51df-4536-92b9-c98d34e84c68", "Authorization");
			this.AuthorizationGroupBox.Controls.Add(this.PasswordTextBox);
			this.AuthorizationGroupBox.Controls.Add(this.LoginTextBox);
			this.AuthorizationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AuthorizationGroupBox.Name = "AuthorizationGroupBox";
			this.AuthorizationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 40, true);
			this.AuthorizationGroupBox.TabIndex = 5;
			this.AuthorizationGroupBox.TabStop = false;
			// 
			// PasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.PasswordTextBox, "Password");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AllocationAdjustmentsSecurity)(null)).Password)));
			this.PasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PasswordTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocationAdjustmentDialog|26f3b080-851b-464c-a8b7-57c81898ef29", "Password");
			this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 12, true);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.PasswordChar = '*';
			this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.PasswordTextBox.TabIndex = 1;
			// 
			// LoginTextBox
			// 
			this.BindingSource.SetBindingMember(this.LoginTextBox, "Login");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AllocationAdjustmentsSecurity)(null)).Login)));
			this.LoginTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LoginTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocationAdjustmentDialog|e0d9c84f-9fe0-4041-bd4b-bb5e23ccd12b", "Login");
			this.LoginTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 13, true);
			this.LoginTextBox.Name = "LoginTextBox";
			this.LoginTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.LoginTextBox.TabIndex = 0;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.AdjustmentsGrid);
			this.MainPanel.Controls.Add(this.MessageLabel);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 249, true);
			this.MainPanel.TabIndex = 6;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.AuthorizationGroupBox);
			this.BottomPanel.Controls.Add(this.AdjustButton);
			this.BottomPanel.Controls.Add(this.CancelZButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 249, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 44, true);
			this.BottomPanel.TabIndex = 8;
			// 
			// AllocationAdjustmentDialog
			// 
			this.AcceptButton = this.AdjustButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelZButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 317, true);
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AllocationAdjustmentDialog|9e030949-4c98-478b-b9f8-f5497567a3f8", "Allocations Adjustment");
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.AllocationAdjustmentsSecurity);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "AllocationAdjustmentDialog";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "AllocationAdjustmentDialog";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AdjustmentsGrid)).EndInit();
			this.AuthorizationGroupBox.ResumeLayout(false);
			this.AuthorizationGroupBox.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.BottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		protected ZArchitecture.ZLabel MessageLabel;
		private ZArchitecture.ZGrid AdjustmentsGrid;
		private ZArchitecture.GUI.ZButton CancelZButton;
		protected ZArchitecture.GUI.ZButton AdjustButton;
		protected ZArchitecture.GUI.ZGroupBox AuthorizationGroupBox;
		private ZArchitecture.ZTextBox PasswordTextBox;
		private ZArchitecture.ZTextBox LoginTextBox;
		private ZArchitecture.GUI.ZPanel MainPanel;
		private ZArchitecture.GUI.ZPanel BottomPanel;
	}
}

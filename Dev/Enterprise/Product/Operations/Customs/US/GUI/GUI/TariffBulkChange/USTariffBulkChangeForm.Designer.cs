

namespace Enterprise.Customs.US.GUI
{
	partial class USTariffBulkChangeForm
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
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.TariffsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TariffsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TextLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SHBRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.HTSRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ContinueBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TariffsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TariffsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 458, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 24, true);
			this.MainStatusBar.TabIndex = 6;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.USTariffBulkChange);
			// 
			// TariffsGroupBox
			// 
			this.TariffsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.TariffsGroupBox.Controls.Add(this.TariffsGrid);
			this.TariffsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 134, true);
			this.TariffsGroupBox.Name = "TariffsGroupBox";
			this.TariffsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 290, true);
			this.TariffsGroupBox.TabIndex = 3;
			this.TariffsGroupBox.TabStop = false;
			this.TariffsGroupBox.Text = "Tariffs";
			// 
			// TariffsGrid
			// 
			this.TariffsGrid.AllowNavigation = false;
			this.TariffsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TariffsGrid, "Tariffs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.USTariffBulkChange)(null)).Tariffs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.TariffToChange)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USTariffBulkChange)(null)).Tariffs)).SyncRoot)).FormattedOldTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.TariffToChange)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USTariffBulkChange)(null)).Tariffs)).SyncRoot)).FormattedNewTariff)));
			this.TariffsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.Caption = "Old Tariff";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "FormattedOldTariff";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo2.Caption = "New Tariff";
			zCodeFindBoxColumnStyleInfo2.ColumnName = "FormattedNewTariff";
			zCodeFindBoxColumnStyleInfo2.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.TariffsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.TariffsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.TariffsGrid.GridId = "a736cc6f-5bd7-4f10-9f09-30ed12c14357";
			this.TariffsGrid.CopySelectedRowsAllowed = true;
			this.TariffsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TariffsGrid.LayoutKey = "TariffsGrid";
			this.TariffsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 17, true);
			this.TariffsGrid.Name = "TariffsGrid";
			this.TariffsGrid.ShouldSetErrorsOnTabPage = false;
			this.TariffsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 267, true);
			this.TariffsGrid.TabIndex = 0;
			// 
			// TextLabel
			// 
			this.TextLabel.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("USTariffBulkChangeForm|31b63983-e30e-43aa-92c8-01355aa03b39", "", "All tariffs on products and lookups will be changed from 'Old Tariff' to 'New Tariff'.\r\n\r\nNote that only active products and lookups will be affected.");
			this.TextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 9, true);
			this.TextLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(20, true);
			this.TextLabel.Name = "TextLabel";
			this.TextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 63, true);
			this.TextLabel.TabIndex = 0;
			this.TextLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// SHBRadioButton
			// 
			this.SHBRadioButton.AutoCheck = false;
			this.SHBRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SHBRadioButton, "SHBTariffFlag");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.USTariffBulkChange)(null)).SHBTariffFlag)));
			this.SHBRadioButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("USTariffBulkChangeForm|cb757a1b-91a8-4bb2-8484-b19877173c7c", "SHB Tariffs");
			this.SHBRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SHBRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 98, true);
			this.SHBRadioButton.Name = "SHBRadioButton";
			this.SHBRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 17, true);
			this.SHBRadioButton.TabIndex = 2;
			this.SHBRadioButton.UseVisualStyleBackColor = true;
			// 
			// HTSRadioButton
			// 
			this.HTSRadioButton.AutoCheck = false;
			this.HTSRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HTSRadioButton, "HTSTariffFlag");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.USTariffBulkChange)(null)).HTSTariffFlag)));
			this.HTSRadioButton.Checked = true;
			this.HTSRadioButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("USTariffBulkChangeForm|3e735b1f-5805-4763-b12f-bfda9327a6a5", "HTS Tariffs");
			this.HTSRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HTSRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 98, true);
			this.HTSRadioButton.Name = "HTSRadioButton";
			this.HTSRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 17, true);
			this.HTSRadioButton.TabIndex = 1;
			this.HTSRadioButton.TabStop = true;
			this.HTSRadioButton.UseVisualStyleBackColor = true;
			// 
			// ContinueBtn
			// 
			this.ContinueBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ContinueBtn.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("USTariffBulkChangeForm|5ba1669f-156b-4763-a5b5-1331b6f34f5c", "Update");
			this.ContinueBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 430, true);
			this.ContinueBtn.Name = "ContinueBtn";
			this.ContinueBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ContinueBtn.TabIndex = 4;
			this.ContinueBtn.UseVisualStyleBackColor = true;
			this.ContinueBtn.Click += new System.EventHandler(this.ContinueBtn_Click);
			// 
			// CancelBtn
			// 
			this.CancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelBtn.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("USTariffBulkChangeForm|423c116d-359c-430d-bf87-3d1b780a491d", "Cancel");
			this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 430, true);
			this.CancelBtn.Name = "CancelBtn";
			this.CancelBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelBtn.TabIndex = 5;
			this.CancelBtn.UseVisualStyleBackColor = true;
			this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
			// 
			// USTariffBulkChangeForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 482, true);
			this.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("USTariffBulkChangeForm|b9656aa2-5715-4ece-b6e0-85d8135b38b7", "Tariff Bulk Change");
			this.Controls.Add(this.ContinueBtn);
			this.Controls.Add(this.CancelBtn);
			this.Controls.Add(this.SHBRadioButton);
			this.Controls.Add(this.HTSRadioButton);
			this.Controls.Add(this.TextLabel);
			this.Controls.Add(this.TariffsGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Customs.US.Business";
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.USTariffBulkChange);
			this.DataSourceTypeName = "USTariffBulkChange";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 3000, true);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 516, true);
			this.Name = "USTariffBulkChangeForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.TariffsGroupBox, 0);
			this.Controls.SetChildIndex(this.TextLabel, 0);
			this.Controls.SetChildIndex(this.HTSRadioButton, 0);
			this.Controls.SetChildIndex(this.SHBRadioButton, 0);
			this.Controls.SetChildIndex(this.CancelBtn, 0);
			this.Controls.SetChildIndex(this.ContinueBtn, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TariffsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TariffsGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox TariffsGroupBox;
		private Enterprise.ZArchitecture.ZGrid TariffsGrid;
		private Enterprise.ZArchitecture.ZLabel TextLabel;
		private Enterprise.ZArchitecture.GUI.ZRadioButton SHBRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton HTSRadioButton;
		public Enterprise.ZArchitecture.GUI.ZButton ContinueBtn;
		private Enterprise.ZArchitecture.GUI.ZButton CancelBtn;
	}
}


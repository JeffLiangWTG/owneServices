using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ACEManifest.GUI
{
	partial class FlightSelectionDialog
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
			if (disposing)
			{
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FlightDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FlightDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AdditionalInformationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FlightDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FlightDetailsGrid)).BeginInit();
			this.FlightDetailsGrid.SuspendLayout();
			this.ButtonsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 154, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 24, true);
			this.MainStatusBar.TabIndex = 7;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.ACEManifest.Business.AdditionalMessageInformation);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.IsCaptionOverridden = true;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 9, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 5;
			this.OKButton.Text = "OK";
			this.OKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Cancel_Button.IsCaptionOverridden = true;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 9, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.Cancel_Button.TabIndex = 6;
			this.Cancel_Button.Text = "Cancel";
			this.Cancel_Button.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.Cancel_Button.ToolTipCaption = null;
			this.Cancel_Button.UseVisualStyleBackColor = true;
			this.Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
			// 
			// FlightDetailsGroupBox
			// 
			this.FlightDetailsGroupBox.Controls.Add(this.FlightDetailsGrid);
			this.FlightDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.FlightDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FlightDetailsGroupBox.Name = "FlightDetailsGroupBox";
			this.FlightDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 111, true);
			this.FlightDetailsGroupBox.TabIndex = 0;
			this.FlightDetailsGroupBox.TabStop = false;
			this.FlightDetailsGroupBox.Text = "Flight Details";
			// 
			// FlightDetailsGrid
			// 
			this.FlightDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.FlightDetailsGrid, "FlightArrivalDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.ACEManifest.Business.AdditionalMessageInformation)(null)).FlightArrivalDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.ACEManifest.Business.FlightDetail)(((System.Collections.IList)(((Enterprise.Customs.US.ACEManifest.Business.AdditionalMessageInformation)(null)).FlightArrivalDetails)).SyncRoot)).Selected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ACEManifest.Business.FlightDetail)(((System.Collections.IList)(((Enterprise.Customs.US.ACEManifest.Business.AdditionalMessageInformation)(null)).FlightArrivalDetails)).SyncRoot)).FlightNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.US.ACEManifest.Business.FlightDetail)(((System.Collections.IList)(((Enterprise.Customs.US.ACEManifest.Business.AdditionalMessageInformation)(null)).FlightArrivalDetails)).SyncRoot)).FlightArrivalDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ACEManifest.Business.FlightDetail)(((System.Collections.IList)(((Enterprise.Customs.US.ACEManifest.Business.AdditionalMessageInformation)(null)).FlightArrivalDetails)).SyncRoot)).FlightReference)));
			this.FlightDetailsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.Caption = "Send?";
			zCheckBoxColumnStyleInfo1.ColumnName = "Selected";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo1.Caption = "Flight No.";
			zTextBoxColumnStyleInfo1.ColumnName = "FlightNo";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDateEditColumnStyleInfo1.Caption = "ETA";
			zDateEditColumnStyleInfo1.ColumnName = "FlightArrivalDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsMandatory = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.Caption = "Reference";
			zTextBoxColumnStyleInfo2.ColumnName = "FlightReference";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.FlightDetailsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FlightDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FlightDetailsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FlightDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FlightDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FlightDetailsGrid.GridId = "b522c5b0-62cf-4d63-bafd-fe451fca4952";
			this.FlightDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FlightDetailsGrid.LayoutKey = "zGrid1";
			this.FlightDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.FlightDetailsGrid.Name = "FlightDetailsGrid";
			this.FlightDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 92, true);
			this.FlightDetailsGrid.TabIndex = 0;
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.Controls.Add(this.Cancel_Button);
			this.ButtonsPanel.Controls.Add(this.OKButton);
			this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 119, true);
			this.ButtonsPanel.Name = "ButtonsPanel";
			this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 35, true);
			this.ButtonsPanel.TabIndex = 8;
			// 
			// AdditionalInformationPanel
			// 
			this.AdditionalInformationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInformationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 111, true);
			this.AdditionalInformationPanel.Name = "AdditionalInformationPanel";
			this.AdditionalInformationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 8, true);
			this.AdditionalInformationPanel.TabIndex = 9;
			// 
			// FlightSelectionDialog
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 178, true);
			this.Controls.Add(this.AdditionalInformationPanel);
			this.Controls.Add(this.ButtonsPanel);
			this.Controls.Add(this.FlightDetailsGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Customs.US.ACEManifest.Business";
			this.DataSourceType = typeof(Enterprise.Customs.US.ACEManifest.Business.AdditionalMessageInformation);
			this.DataSourceTypeName = "Enterprise.Customs.US.ACEManifest.Business.AdditionalMessageInformation";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "FlightSelectionDialog";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.FlightDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.ButtonsPanel, 0);
			this.Controls.SetChildIndex(this.AdditionalInformationPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FlightDetailsGroupBox.ResumeLayout(false);
			this.FlightDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FlightDetailsGrid)).EndInit();
			this.FlightDetailsGrid.ResumeLayout(false);
			this.FlightDetailsGrid.PerformLayout();
			this.ButtonsPanel.ResumeLayout(false);
			this.ButtonsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ZButton OKButton;
		public Enterprise.ZArchitecture.GUI.ZButton Cancel_Button;
		protected ZArchitecture.GUI.ZGroupBox FlightDetailsGroupBox;
		public ZArchitecture.ZGrid FlightDetailsGrid;
		protected ZPanel ButtonsPanel;
		protected ZPanel AdditionalInformationPanel;
	}
}

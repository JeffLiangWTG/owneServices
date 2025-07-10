
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ACEManifest.GUI
{
	partial class SplitShipments
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

		new void InitializeComponent()
		{
			this.IsConsolidationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsSplitShipmentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BoardedQtyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BoardedWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BoardedWeightUQ = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AgentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FlightDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FlightDetailsGrid)).BeginInit();
			this.FlightDetailsGrid.SuspendLayout();
			this.ButtonsPanel.SuspendLayout();
			this.AdditionalInformationPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BoardedWeightUQ.SuspendLayout();
			this.SuspendLayout();
			// 
			// OKButton
			// 
			this.OKButton.TabIndex = 0;
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.TabIndex = 1;
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 246, true);
			this.ButtonsPanel.TabIndex = 2;
			// 
			// AdditionalInformationPanel
			// 
			this.AdditionalInformationPanel.Controls.Add(this.AgentTextBox);
			this.AdditionalInformationPanel.Controls.Add(this.BoardedWeightUQ);
			this.AdditionalInformationPanel.Controls.Add(this.BoardedWeightCalcEdit);
			this.AdditionalInformationPanel.Controls.Add(this.BoardedQtyCalcEdit);
			this.AdditionalInformationPanel.Controls.Add(this.IsSplitShipmentCheckBox);
			this.AdditionalInformationPanel.Controls.Add(this.IsConsolidationCheckBox);
			this.AdditionalInformationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 135, true);
			this.AdditionalInformationPanel.TabIndex = 1;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 281, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// IsConsolidationCheckBox
			// 
			this.IsConsolidationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsConsolidationCheckBox, "AM_IsConsolidation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.ACEManifest.Business.AdditionalMessageInformation)(null)).AM_IsConsolidation)));
			this.IsConsolidationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsConsolidationCheckBox.Checked = true;
			this.IsConsolidationCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.IsConsolidationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsConsolidationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 16, true);
			this.IsConsolidationCheckBox.Name = "IsConsolidationCheckBox";
			this.IsConsolidationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 16, true);
			this.IsConsolidationCheckBox.TabIndex = 0;
			this.IsConsolidationCheckBox.Text = "Consolidation";
			this.IsConsolidationCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsSplitShipmentCheckBox
			// 
			this.IsSplitShipmentCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsSplitShipmentCheckBox, "AM_IsSplitShipment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.ACEManifest.Business.AdditionalMessageInformation)(null)).AM_IsSplitShipment)));
			this.IsSplitShipmentCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsSplitShipmentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSplitShipmentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 59, true);
			this.IsSplitShipmentCheckBox.Name = "IsSplitShipmentCheckBox";
			this.IsSplitShipmentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 16, true);
			this.IsSplitShipmentCheckBox.TabIndex = 2;
			this.IsSplitShipmentCheckBox.Text = "Split Shipment";
			this.IsSplitShipmentCheckBox.UseVisualStyleBackColor = true;
			// 
			// BoardedQtyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.BoardedQtyCalcEdit, "AM_BoardedQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.ACEManifest.Business.AdditionalMessageInformation)(null)).AM_BoardedQty)));
			this.BoardedQtyCalcEdit.CaptionResourceString = null;
			this.BoardedQtyCalcEdit.DecimalPlaces = 0;
			this.BoardedQtyCalcEdit.Decimals = 0;
			this.BoardedQtyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 79, true);
			this.BoardedQtyCalcEdit.Name = "BoardedQtyCalcEdit";
			this.BoardedQtyCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.BoardedQtyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 17, true);
			this.BoardedQtyCalcEdit.TabIndex = 3;
			this.BoardedQtyCalcEdit.Text = "0";
			this.BoardedQtyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BoardedWeightCalcEdit
			// 
			this.BoardedWeightCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.BoardedWeightCalcEdit, "AM_BoardedWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.ACEManifest.Business.AdditionalMessageInformation)(null)).AM_BoardedWeight)));
			this.BoardedWeightCalcEdit.CaptionResourceString = null;
			this.BoardedWeightCalcEdit.DecimalPlaces = 3;
			this.BoardedWeightCalcEdit.Decimals = 3;
			this.BoardedWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 101, true);
			this.BoardedWeightCalcEdit.Name = "BoardedWeightCalcEdit";
			this.BoardedWeightCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.BoardedWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 17, true);
			this.BoardedWeightCalcEdit.TabIndex = 4;
			this.BoardedWeightCalcEdit.Text = "0.000";
			this.BoardedWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BoardedWeightUQ
			// 
			this.BoardedWeightUQ.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BoardedWeightUQ, "AM_BoardedWeightUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.ACEManifest.Business.AdditionalMessageInformation)(null)).AM_BoardedWeightUQ)));
			this.BoardedWeightUQ.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 101, true);
			this.BoardedWeightUQ.Name = "BoardedWeightUQ";
			this.BoardedWeightUQ.PreBoundMaxLength = 1;
			this.BoardedWeightUQ.ShouldResizeByMaxLength = true;
			this.BoardedWeightUQ.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 17, true);
			this.BoardedWeightUQ.TabIndex = 5;
			// 
			// AgentTextBox
			// 
			this.BindingSource.SetBindingMember(this.AgentTextBox, "AM_Agent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ACEManifest.Business.AdditionalMessageInformation)(null)).AM_Agent)));
			this.AgentTextBox.CaptionResourceString = null;
			this.AgentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 37, true);
			this.AgentTextBox.Name = "AgentTextBox";
			this.AgentTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.AgentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.AgentTextBox.TabIndex = 1;
			// 
			// SplitShipments
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 305, true);
			this.Name = "SplitShipments";
			this.FlightDetailsGroupBox.ResumeLayout(false);
			this.FlightDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FlightDetailsGrid)).EndInit();
			this.FlightDetailsGrid.ResumeLayout(false);
			this.FlightDetailsGrid.PerformLayout();
			this.ButtonsPanel.ResumeLayout(false);
			this.ButtonsPanel.PerformLayout();
			this.AdditionalInformationPanel.ResumeLayout(false);
			this.AdditionalInformationPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BoardedWeightUQ.ResumeLayout(true);
			this.BoardedWeightUQ.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZCheckBox IsConsolidationCheckBox;
		public ZArchitecture.GUI.ZCheckBox IsSplitShipmentCheckBox;
		public ZArchitecture.ZCalcEdit BoardedQtyCalcEdit;
		public ZArchitecture.ZCalcEdit BoardedWeightCalcEdit;
		public ZDropEdit BoardedWeightUQ;
		private ZArchitecture.ZTextBox AgentTextBox;
	}
}

using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class ExceptionReasonDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private ZTextBox AdditionalReasonTextBox;
		private ZDropEdit LateReportingReasonDropEdit;
		readonly ForwardingShipmentProcessTask milestoneException;

		#region Windows Form Designer generated code

		internal Enterprise.ZArchitecture.GUI.ZButton YesButtonX;
		private ZLabel ButtonsLabel;
		private ZLabel AdditionalReasonLabel;
		private ZButton Cancel;

		private System.ComponentModel.Container components = null;

		protected override void InitializeComponent()
		{
			this.YesButtonX = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AdditionalReasonLabel = new Enterprise.ZArchitecture.ZLabel();
			this.Cancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AdditionalReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LateReportingReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LateReportingReasonDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 321, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 26, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 9;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(377);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(377);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingShipmentProcessTask);
			// 
			// YesButtonX
			// 
			this.YesButtonX.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("f07d02ed-9c47-4215-b675-15fd5b19b7c7", "OK");
			this.YesButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(337, 289, true);
			this.YesButtonX.Name = "YesButtonX";
			this.YesButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 21, true);
			this.YesButtonX.TabIndex = 1;
			this.YesButtonX.ToolTipCaption = null;
			this.YesButtonX.Click += new System.EventHandler(this.OKButtonX_Click);
			// 
			// ButtonsLabel
			// 
			this.ButtonsLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("233cbbe0-a5ac-4fed-a635-3110e2b69791", "Do you want to send the message?");
			this.ButtonsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ButtonsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 289, true);
			this.ButtonsLabel.Name = "ButtonsLabel";
			this.ButtonsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.ButtonsLabel.TabIndex = 11;
			// 
			// AdditionalReasonLabel
			// 
			this.AdditionalReasonLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("d716206e-5a8c-48c1-b40b-4ffd6dfdc9e2", "Additional reason for the late reporting of this Cargo Report.");
			this.AdditionalReasonLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AdditionalReasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 40, true);
			this.AdditionalReasonLabel.Name = "AdditionalReasonLabel";
			this.AdditionalReasonLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 22, true);
			this.AdditionalReasonLabel.TabIndex = 12;
			// 
			// Cancel
			// 
			this.Cancel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("030588bd-5da9-447e-b3da-af34e396df98", "Cancel");
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(427, 289, true);
			this.Cancel.Name = "Cancel";
			this.Cancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 21, true);
			this.Cancel.TabIndex = 3;
			this.Cancel.ToolTipCaption = null;
			// 
			// AdditionalReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.AdditionalReasonTextBox, "LateCargoReportText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipmentProcessTask)(null)).LateCargoReportText)));
			this.AdditionalReasonTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AdditionalReasonTextBox, false);
			this.AdditionalReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 67, true);
			this.AdditionalReasonTextBox.Multiline = true;
			this.AdditionalReasonTextBox.Name = "AdditionalReasonTextBox";
			this.AdditionalReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(505, 212, true);
			this.AdditionalReasonTextBox.TabIndex = 1;
			// 
			// LateReportingReasonDropEdit
			// 
			this.LateReportingReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LateReportingReasonDropEdit, "LateCargoReportReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingShipmentProcessTask)(null)).LateCargoReportReason)));
			this.LateReportingReasonDropEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("413b89b5-50dd-4a53-9653-511a04b17c67", "Late Reporting Reason Code:");
			this.LateReportingReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(167, 16, true);
			this.LateReportingReasonDropEdit.Name = "LateReportingReasonDropEdit";
			this.LateReportingReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 20, true);
			this.LateReportingReasonDropEdit.TabIndex = 0;
			// 
			// ExceptionReasonDialog
			// 
			this.CancelButton = this.Cancel;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("d006aa32-75fd-468a-8635-244b997df05c", "Late Cargo Report Reason");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 347, true);
			this.Controls.Add(this.LateReportingReasonDropEdit);
			this.Controls.Add(this.AdditionalReasonTextBox);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.AdditionalReasonLabel);
			this.Controls.Add(this.ButtonsLabel);
			this.Controls.Add(this.YesButtonX);
			this.DataSourceAssemblyName = "Enterprise.Freight.Forwarding.Business";
			this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingShipmentProcessTask);
			this.DataSourceTypeName = "Enterprise.Freight.Forwarding.Business.ForwardingShipmentProcessTask";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "ExceptionReasonDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.YesButtonX, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonsLabel, 0);
			this.Controls.SetChildIndex(this.AdditionalReasonLabel, 0);
			this.Controls.SetChildIndex(this.Cancel, 0);
			this.Controls.SetChildIndex(this.AdditionalReasonTextBox, 0);
			this.Controls.SetChildIndex(this.LateReportingReasonDropEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LateReportingReasonDropEdit.ResumeLayout(true);
			this.LateReportingReasonDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
	}
}

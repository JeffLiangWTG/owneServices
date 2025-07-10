using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	partial class PrintFinalMasterActionMethodForm
	{
		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Windows Form Designer generated code

		Enterprise.ZArchitecture.GUI.ZButton OKButton;
		Enterprise.ZArchitecture.GUI.ZButton Cancel_Button;
		private ZCheckBox AllowPrintingWithMsgErrorsCheckBox;
		private MAWBPrintOptionsControl MAWBPrintOptionsControl;
		private CIMPOptionsControl CIMPOptionsControl;
		System.ComponentModel.Container components = null;

		protected new void InitializeComponent()
		{
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AllowPrintingWithMsgErrorsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MAWBPrintOptionsControl = new Enterprise.Freight.Forwarding.GUI.AWB.MAWBPrintOptionsControl();
			this.CIMPOptionsControl = new Enterprise.Freight.Forwarding.GUI.AWB.CIMPOptionsControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MAWBPrintOptionsControl.SuspendLayout();
			this.CIMPOptionsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 317, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 5;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = 227;
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = 227;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.AWB.BulkConsolAWBActions);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("7dc3e32d-8410-4fc2-8f33-7a1b843c1357", "&OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(291, 291, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 3;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("0560ae9d-61b7-4aaf-9769-382b42d397d0", "&Cancel");
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 291, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.Cancel_Button.TabIndex = 4;
			// 
			// AllowPrintingWithMsgErrorsCheckBox
			// 
			this.AllowPrintingWithMsgErrorsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AllowPrintingWithMsgErrorsCheckBox, "AllowPrintWithMessageErrors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.BulkConsolAWBActions)(null)).AllowPrintWithMessageErrors)));
			this.AllowPrintingWithMsgErrorsCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("271b44e8-464f-42f8-b61d-74307dc15257", "Allow Printing with Message Errors", "Specifies whether consols with message errors will still print AWB documents");
			this.AllowPrintingWithMsgErrorsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AllowPrintingWithMsgErrorsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 255, true);
			this.AllowPrintingWithMsgErrorsCheckBox.Name = "AllowPrintingWithMsgErrorsCheckBox";
			this.AllowPrintingWithMsgErrorsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 17, true);
			this.AllowPrintingWithMsgErrorsCheckBox.TabIndex = 2;
			this.AllowPrintingWithMsgErrorsCheckBox.Visible = false;
			// 
			// MAWBPrintOptionsControl
			// 
			this.MAWBPrintOptionsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MAWBPrintOptionsControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Forwarding.Business.AWB.IPrintMAWB)(((Enterprise.Freight.Forwarding.Business.AWB.BulkConsolAWBActions)(null)))));
			this.MAWBPrintOptionsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MAWBPrintOptionsControl.Name = "MAWBPrintOptionsControl";
			this.MAWBPrintOptionsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 146, true);
			this.MAWBPrintOptionsControl.TabIndex = 0;
			// 
			// CIMPOptionsControl
			// 
			this.CIMPOptionsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CIMPOptionsControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Forwarding.Business.AWB.ISendCIMP)(((Enterprise.Freight.Forwarding.Business.AWB.BulkConsolAWBActions)(null)))));
			this.CIMPOptionsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 154, true);
			this.CIMPOptionsControl.Name = "CIMPOptionsControl";
			this.CIMPOptionsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 96, true);
			this.CIMPOptionsControl.TabIndex = 1;
			// 
			// PrintFinalMasterActionMethodForm
			// 
			this.CancelButton = this.Cancel_Button;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("2dc917f7-d75b-48d1-92ca-bea0ba61a7bb", "Print Final Master Settings");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 341, true);
			this.ControlBox = false;
			this.Controls.Add(this.CIMPOptionsControl);
			this.Controls.Add(this.MAWBPrintOptionsControl);
			this.Controls.Add(this.AllowPrintingWithMsgErrorsCheckBox);
			this.Controls.Add(this.Cancel_Button);
			this.Controls.Add(this.OKButton);
			this.DataSourceAssemblyName = "Enterprise.Freight.Forwarding.Business";
			this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.AWB.BulkConsolAWBActions);
			this.DataSourceTypeName = "Enterprise.Freight.Forwarding.Business.AWB.BulkConsolAWBActions";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "PrintFinalMasterActionMethodForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.AllowPrintingWithMsgErrorsCheckBox, 0);
			this.Controls.SetChildIndex(this.MAWBPrintOptionsControl, 0);
			this.Controls.SetChildIndex(this.CIMPOptionsControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MAWBPrintOptionsControl.ResumeLayout(true);
			this.MAWBPrintOptionsControl.PerformLayout();
			this.CIMPOptionsControl.ResumeLayout(true);
			this.CIMPOptionsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
	}
}

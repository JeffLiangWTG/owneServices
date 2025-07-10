using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Customs.US.ACEManifest.GUI
{
	partial class TransferBillInBondNumberAllocationUserControl
	{
		void InitializeComponent()
		{
			this.InBondNumberResetButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InBondNumberAllocationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InBondNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.ACEManifest.Business.AsycudaTransferBill);
			// 
			// InBondNumberResetButton
			// 
			this.InBondNumberResetButton.IsCaptionOverridden = true;
			this.InBondNumberResetButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(317, 4, true);
			this.InBondNumberResetButton.Name = "InBondNumberResetButton";
			this.InBondNumberResetButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.InBondNumberResetButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.InBondNumberResetButton.TabIndex = 2;
			this.InBondNumberResetButton.Text = "Reset";
			this.InBondNumberResetButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.InBondNumberResetButton.ToolTipCaption = null;
			this.InBondNumberResetButton.UseVisualStyleBackColor = true;
			this.InBondNumberResetButton.Click += new System.EventHandler(this.InBondNumberResetButton_Click);
			// 
			// InBondNumberAllocationButton
			// 
			this.InBondNumberAllocationButton.IsCaptionOverridden = true;
			this.InBondNumberAllocationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(236, 4, true);
			this.InBondNumberAllocationButton.Name = "InBondNumberAllocationButton";
			this.InBondNumberAllocationButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.InBondNumberAllocationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.InBondNumberAllocationButton.TabIndex = 1;
			this.InBondNumberAllocationButton.Text = "Allocate";
			this.InBondNumberAllocationButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.InBondNumberAllocationButton.ToolTipCaption = null;
			this.InBondNumberAllocationButton.UseVisualStyleBackColor = true;
			this.InBondNumberAllocationButton.Click += new System.EventHandler(this.InBondNumberAllocationButton_Click);
			// 
			// InBondNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.InBondNumberTextBox, "InBondNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ACEManifest.Business.AsycudaTransferBill)(null)).InBondNumber)));
			this.InBondNumberTextBox.CaptionResourceString = null;
			this.InBondNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 6, true);
			this.InBondNumberTextBox.Name = "InBondNumberTextBox";
			this.InBondNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.InBondNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.InBondNumberTextBox.TabIndex = 0;
			// 
			// TransferBillInBondNumberAllocationUserControl
			// 
			this.Controls.Add(this.InBondNumberResetButton);
			this.Controls.Add(this.InBondNumberAllocationButton);
			this.Controls.Add(this.InBondNumberTextBox);
			this.Name = "TransferBillInBondNumberAllocationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 40, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.GUI.ZButton InBondNumberResetButton;
		private Enterprise.ZArchitecture.GUI.ZButton InBondNumberAllocationButton;
		private Enterprise.ZArchitecture.ZTextBox InBondNumberTextBox;
	}
}

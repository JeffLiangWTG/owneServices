namespace Enterprise.Warehouse.Transactions.Module.Order
{
	partial class OverrideWhsOrderFulfilmentRuleReasonControl
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
			this.OverrideReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OverrideReasonLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// OverrideReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.OverrideReasonTextBox, "OverrideReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Module.OverrideWhsOrderFulfillmentRuleActionMethodApplicator)(null)).OverrideReason)));
			this.OverrideReasonTextBox.CaptionResourceString = null;
			this.OverrideReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 34, true);
			this.OverrideReasonTextBox.Name = "OverrideReasonTextBox";
			this.OverrideReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 20, true);
			this.OverrideReasonTextBox.TabIndex = 0;
			this.OverrideReasonTextBox.MaxLength = 80;
			// 
			// OverrideReasonLabel
			// 
			this.OverrideReasonLabel.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("4227b150-1b57-4068-83c8-88a14bc34752", "Override Reason:");
			this.OverrideReasonLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.OverrideReasonLabel.IsFontBold = true;
			this.OverrideReasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 31, true);
			this.OverrideReasonLabel.Name = "OverrideReasonLabel";
			this.OverrideReasonLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 23, true);
			this.OverrideReasonLabel.TabIndex = 1;
			// 
			// OverrideWarehouseOrderReasonControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.OverrideReasonLabel);
			this.Controls.Add(this.OverrideReasonTextBox);
			this.Name = "OverrideWarehouseOrderReasonControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(523, 151, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox OverrideReasonTextBox;
		private ZArchitecture.ZLabel OverrideReasonLabel;
	}
}

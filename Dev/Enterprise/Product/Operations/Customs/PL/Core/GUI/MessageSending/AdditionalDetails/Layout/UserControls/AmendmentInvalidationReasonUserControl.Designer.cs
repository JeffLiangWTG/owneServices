using Enterprise.ZArchitecture;

namespace Enterprise.Customs.PL.GUI
{
	partial class AmendmentInvalidationReasonUserControl
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
			this.AmendmentInvalidationReasonLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AmendmentInvalidationReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.BaseMessageSendingObject);
			// 
			// AmendmentInvalidationReasonLabel
			// 
			this.AmendmentInvalidationReasonLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AmendmentInvalidationReasonLabel.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("1A352AA3-7A85-451C-A065-CD0C3B99B576", "Reason For Amendment / Invalidation");
			this.AmendmentInvalidationReasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 3, true);
			this.AmendmentInvalidationReasonLabel.Name = "AmendmentInvalidationReasonLabel";
			this.AmendmentInvalidationReasonLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 18, true);
			this.AmendmentInvalidationReasonLabel.TabIndex = 1;
			this.AmendmentInvalidationReasonLabel.UseMnemonic = false;
			// 
			// AmendmentInvalidationReasonTextBox
			// 
			this.AmendmentInvalidationReasonTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AmendmentInvalidationReasonTextBox, "AmendmentInvalidationReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.BaseMessageSendingObject)(null)).AmendmentInvalidationReason)));
			this.AmendmentInvalidationReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 23, true);
			this.AmendmentInvalidationReasonTextBox.Multiline = true;
			this.AmendmentInvalidationReasonTextBox.Name = "AmendmentInvalidationReasonTextBox";
			this.AmendmentInvalidationReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(563, 94, true);
			this.AmendmentInvalidationReasonTextBox.TabIndex = 2;
			// 
			// AmendmentInvalidationReasonUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AmendmentInvalidationReasonLabel);
			this.Controls.Add(this.AmendmentInvalidationReasonTextBox);
			this.Name = "AmendmentInvalidationReasonUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 120, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZLabel AmendmentInvalidationReasonLabel;
		internal ZTextBox AmendmentInvalidationReasonTextBox;
	}
}

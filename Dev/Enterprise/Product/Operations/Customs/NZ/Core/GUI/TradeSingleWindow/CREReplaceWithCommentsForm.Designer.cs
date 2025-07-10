namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow
{
	partial class CREReplaceWithCommentsForm
	{
		private System.ComponentModel.IContainer components = null;

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
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChangeCancelReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ManualProcessingGroupBox.SuspendLayout();
			this.AdditionalInformationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// ManualProcessingGroupBox
			// 
			this.ManualProcessingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 207, true);
			this.ManualProcessingGroupBox.TabIndex = 2;
			// 
			// ManualProcessingTextBox
			// 
			this.ManualProcessingTextBox.TabIndex = 0;
			// 
			// OKButton
			// 
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 313, true);
			this.OKButton.TabIndex = 3;
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(401, 313, true);
			this.Cancel_Button.TabIndex = 4;
			// 
			// AdditionalInformationGroupBox
			// 
			this.AdditionalInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 106, true);
			this.AdditionalInformationGroupBox.TabIndex = 1;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 342, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 24, true);
			this.MainStatusBar.TabIndex = 5;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Controls.Add(this.ChangeCancelReasonTextBox);
			this.zGroupBox2.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("099354B2-8194-485B-A0F9-16087E209721", "Change Reason");
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 101, true);
			this.zGroupBox2.TabIndex = 0;
			this.zGroupBox2.TabStop = false;
			// 
			// ChangeCancelReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.ChangeCancelReasonTextBox, "AM_AdditionalStatementText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.TradeSingleWindow.AdditionalMessageInformation)(null)).AM_AdditionalStatementText)));
			this.ChangeCancelReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ChangeCancelReasonTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChangeCancelReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ChangeCancelReasonTextBox.Multiline = true;
			this.ChangeCancelReasonTextBox.Name = "ChangeCancelReasonTextBox";
			this.ChangeCancelReasonTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ChangeCancelReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(474, 82, true);
			this.ChangeCancelReasonTextBox.TabIndex = 0;
			// 
			// CREReplaceWithCommentsForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 366, true);
			this.Controls.Add(this.zGroupBox2);
			this.Name = "CREReplaceWithCommentsForm";
			this.Controls.SetChildIndex(this.AdditionalInformationGroupBox, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ManualProcessingGroupBox, 0);
			this.Controls.SetChildIndex(this.zGroupBox2, 0);
			this.ManualProcessingGroupBox.ResumeLayout(false);
			this.ManualProcessingGroupBox.PerformLayout();
			this.AdditionalInformationGroupBox.ResumeLayout(false);
			this.AdditionalInformationGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		protected ZArchitecture.GUI.ZGroupBox zGroupBox2;
		public ZArchitecture.ZTextBox ChangeCancelReasonTextBox;

	}
}

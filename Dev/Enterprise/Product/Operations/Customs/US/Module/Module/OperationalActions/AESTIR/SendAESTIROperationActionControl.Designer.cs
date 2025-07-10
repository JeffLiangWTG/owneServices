using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.Module.OperationalActions
{
	partial class SendAESTIROperationActionControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SendGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ApportionWeight = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SendWithErrorsRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.SendRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SendGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Module.OperationalActions.SendAESTIRActionMethodApplicator);
			// 
			// SendGroupBox
			// 
			this.SendGroupBox.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("4a5a835a-1c1c-43a5-adb6-4535b5728066", "Send Messages");
			this.SendGroupBox.Controls.Add(this.ApportionWeight);
			this.SendGroupBox.Controls.Add(this.SendWithErrorsRadioButton);
			this.SendGroupBox.Controls.Add(this.SendRadioButton);
			this.SendGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.SendGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SendGroupBox.Name = "SendGroupBox";
			this.SendGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 146, true);
			this.SendGroupBox.TabIndex = 0;
			this.SendGroupBox.TabStop = false;
			// 
			// ApportionWeight
			// 
			this.BindingSource.SetBindingMember(this.ApportionWeight, "ApportionWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Module.OperationalActions.SendEntrySummaryActionMethodApplicator)(null)).ApportionWeight)));
			this.ApportionWeight.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("b916cad9-0e0f-4199-824f-082315cc61de", "Apportion Weight ?");
			this.ApportionWeight.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ApportionWeight.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 65, true);
			this.ApportionWeight.Name = "ApportionWeight";
			this.ApportionWeight.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 16, true);
			this.ApportionWeight.TabIndex = 2;
			this.ApportionWeight.UseVisualStyleBackColor = true;
			// 
			// SendWithErrorsRadioButton
			// 
			this.SendWithErrorsRadioButton.AutoCheck = false;
			this.SendWithErrorsRadioButton.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.SendWithErrorsRadioButton, "SendWithMessageErrors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Module.OperationalActions.SendEntrySummaryActionMethodApplicator)(null)).SendWithMessageErrors)));
			this.SendWithErrorsRadioButton.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("18c69f47-031f-4348-8ec0-15e30a4eaf71", "Send ignoring errors");
			this.SendWithErrorsRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SendWithErrorsRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 44, true);
			this.SendWithErrorsRadioButton.Name = "SendWithErrorsRadioButton";
			this.SendWithErrorsRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 18, true);
			this.SendWithErrorsRadioButton.TabIndex = 1;
			this.SendWithErrorsRadioButton.UseVisualStyleBackColor = true;
			// 
			// SendRadioButton
			// 
			this.SendRadioButton.AutoCheck = false;
			this.SendRadioButton.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.SendRadioButton, "Send");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Module.OperationalActions.SendEntrySummaryActionMethodApplicator)(null)).Send)));
			this.SendRadioButton.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("d1fb2e42-00cc-4399-9958-f124030c77f1", "Send (send only where there are no errors)");
			this.SendRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SendRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 19, true);
			this.SendRadioButton.Name = "SendRadioButton";
			this.SendRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 19, true);
			this.SendRadioButton.TabIndex = 0;
			this.SendRadioButton.UseVisualStyleBackColor = true;
			// 
			// SendAESTIROperationActionControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SendGroupBox);
			this.Name = "SendAESTIROperationActionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 283, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SendGroupBox.ResumeLayout(false);
			this.SendGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		ZRadioButton SendWithErrorsRadioButton;
		ZRadioButton SendRadioButton;
		ZCheckBox ApportionWeight;
		ZGroupBox SendGroupBox;
	}
}

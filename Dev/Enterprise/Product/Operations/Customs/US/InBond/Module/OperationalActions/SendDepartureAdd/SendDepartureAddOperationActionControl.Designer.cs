
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.InBond.Module.OperationalActions
{
	partial class SendDepartureAddOperationActionControl
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
			this.SendGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SendWithErrorsRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.SendRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SendGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.InBond.Module.OperationalActions.SendDepartureAddActionMethodApplicator);
			// 
			// SendGroupBox
			// 
			this.SendGroupBox.CaptionResourceString = Enterprise.Customs.US.InBond.Module.Res.GetData("FDA1F6AD-B506-48E7-8E55-19E171749E10", "Send Messages");
			this.SendGroupBox.Controls.Add(this.SendWithErrorsRadioButton);
			this.SendGroupBox.Controls.Add(this.SendRadioButton);
			this.SendGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.SendGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SendGroupBox.Name = "SendGroupBox";
			this.SendGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 146, true);
			this.SendGroupBox.TabIndex = 0;
			this.SendGroupBox.TabStop = false;
			// 
			// SendWithErrorsRadioButton
			// 
			this.SendWithErrorsRadioButton.AutoCheck = false;
			this.SendWithErrorsRadioButton.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.SendWithErrorsRadioButton, "SendWithMessageErrors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Module.OperationalActions.SendEntrySummaryActionMethodApplicator)(null)).SendWithMessageErrors)));
			this.SendWithErrorsRadioButton.CaptionResourceString = Enterprise.Customs.US.InBond.Module.Res.GetData("71A454A3-2F1F-4B8A-AD15-BE4E35841ADA", "Send ignoring errors");
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
			this.SendRadioButton.CaptionResourceString = Enterprise.Customs.US.InBond.Module.Res.GetData("B3921BD8-05DE-4F4D-8193-2A9CBD3F598F", "Send (send only where there are no errors)");
			this.SendRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SendRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 19, true);
			this.SendRadioButton.Name = "SendRadioButton";
			this.SendRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 19, true);
			this.SendRadioButton.TabIndex = 0;
			this.SendRadioButton.UseVisualStyleBackColor = true;
			// 
			// SendDepartureAddOperationActionControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SendGroupBox);
			this.Name = "SendDepartureAddOperationActionControl";
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
		ZGroupBox SendGroupBox;
	}
}

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	partial class CIMPOptionsControl
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
			this.CIMGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.fwbGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.includeECSDCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SendFWBCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SentDateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.SendFHLCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CIMGroupBox.SuspendLayout();
			this.fwbGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.AWB.ISendCIMP);
			// 
			// CIMGroupBox
			// 
			this.CIMGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("7b4b5331-7440-4f6d-962b-d64c4a1679c6", "Cargo Interchange Messages");
			this.CIMGroupBox.Controls.Add(this.fwbGroupBox);
			this.CIMGroupBox.Controls.Add(this.SentDateLabel);
			this.CIMGroupBox.Controls.Add(this.zLabel2);
			this.CIMGroupBox.Controls.Add(this.SendFHLCheckBox);
			this.CIMGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CIMGroupBox.Name = "CIMGroupBox";
			this.CIMGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 86, true);
			this.CIMGroupBox.TabIndex = 24;
			this.CIMGroupBox.TabStop = false;
			// 
			// fwbGroupBox
			// 
			this.fwbGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("9a738de4-a23f-497f-ae85-1b0b1cb4cb8e", "FWB");
			this.fwbGroupBox.Controls.Add(this.includeECSDCheckBox);
			this.fwbGroupBox.Controls.Add(this.SendFWBCheckBox);
			this.fwbGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 17, true);
			this.fwbGroupBox.Name = "fwbGroupBox";
			this.fwbGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 65, true);
			this.fwbGroupBox.TabIndex = 28;
			this.fwbGroupBox.TabStop = false;
			// 
			// includeECSDCheckBox
			// 
			this.BindingSource.SetBindingMember(this.includeECSDCheckBox, "IncludeSecurityDeclaration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.ISendCIMP)(null)).IncludeSecurityDeclaration)));
			this.includeECSDCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("88637b90-173b-478e-81ea-1bfb04a501a6", "Include eCSD");
			this.includeECSDCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.includeECSDCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 38, true);
			this.includeECSDCheckBox.Name = "includeECSDCheckBox";
			this.includeECSDCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 19, true);
			this.includeECSDCheckBox.TabIndex = 1;
			this.includeECSDCheckBox.UseVisualStyleBackColor = true;
			// 
			// SendFWBCheckBox
			// 
			this.SendFWBCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SendFWBCheckBox, "SendFWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.ISendCIMP)(null)).SendFWB)));
			this.SendFWBCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("3afb452e-b9a7-428e-92e9-6f804e70dd38", "Send FWB", "Specifies that the FWB message will be sent.");
			this.SendFWBCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SendFWBCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 17, true);
			this.SendFWBCheckBox.Name = "SendFWBCheckBox";
			this.SendFWBCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 17, true);
			this.SendFWBCheckBox.TabIndex = 0;
			// 
			// SentDateLabel
			// 
			this.SentDateLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SentDateLabel, "DateLastSent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ISendCIMP)(null)).DateLastSent)));
			this.SentDateLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("e74cedaf-8bec-4d23-bc5d-21a8a490b5d3", "Date");
			this.SentDateLabel.IsFontBold = true;
			this.SentDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 34, true);
			this.SentDateLabel.Name = "SentDateLabel";
			this.SentDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 14, true);
			this.SentDateLabel.TabIndex = 27;
			// 
			// zLabel2
			// 
			this.zLabel2.AutoSize = true;
			this.zLabel2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("1a5c4b44-15c5-4142-8d79-93d9a45169cc", "Sent Date:");
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(234, 34, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 14, true);
			this.zLabel2.TabIndex = 26;
			// 
			// SendFHLCheckBox
			// 
			this.SendFHLCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SendFHLCheckBox, "SendFHL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.ISendCIMP)(null)).SendFHL)));
			this.SendFHLCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("4624ceb0-442a-4aa8-ad66-af21a56bb5e1", "Send FHL", "Specifies that the FHL message will be sent.");
			this.SendFHLCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SendFHLCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 34, true);
			this.SendFHLCheckBox.Name = "SendFHLCheckBox";
			this.SendFHLCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 17, true);
			this.SendFHLCheckBox.TabIndex = 1;
			// 
			// CIMPOptionsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CIMGroupBox);
			this.Name = "CIMPOptionsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 94, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CIMGroupBox.ResumeLayout(false);
			this.CIMGroupBox.PerformLayout();
			this.fwbGroupBox.ResumeLayout(false);
			this.fwbGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox CIMGroupBox;
		private ZArchitecture.ZLabel SentDateLabel;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.GUI.ZCheckBox SendFHLCheckBox;
		private ZArchitecture.GUI.ZCheckBox SendFWBCheckBox;
		private ZArchitecture.GUI.ZGroupBox fwbGroupBox;
		private ZArchitecture.GUI.ZCheckBox includeECSDCheckBox;
	}
}

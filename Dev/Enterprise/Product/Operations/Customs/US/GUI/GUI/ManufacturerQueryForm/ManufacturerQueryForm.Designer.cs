
namespace Enterprise.Customs.US.GUI
{
	partial class ManufacturerQueryForm
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

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GiveUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.US_MIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AutoCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 102, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.USMIDQuery);
			// 
			// SendButton
			// 
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 73, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SendButton.TabIndex = 2;
			this.SendButton.Text = "&Send";
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// GiveUpButton
			// 
			this.GiveUpButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.GiveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 73, true);
			this.GiveUpButton.Name = "GiveUpButton";
			this.GiveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.GiveUpButton.TabIndex = 3;
			this.GiveUpButton.Text = "&Cancel";
			this.GiveUpButton.UseVisualStyleBackColor = true;
			this.GiveUpButton.Click += new System.EventHandler(this.GiveUpButton_Click);
			// 
			// US_MIDTextBox
			// 
			this.US_MIDTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.US_MIDTextBox, "US_MID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USMIDQuery)(null)).US_MID)));
			this.US_MIDTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ManufacturerQueryForm|b4834971-03ab-4c35-af58-1ef47a724dd0", "MID to query");
			this.US_MIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 12, true);
			this.US_MIDTextBox.Name = "US_MIDTextBox";
			this.US_MIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.US_MIDTextBox.TabIndex = 0;
			// 
			// AutoCheckBox
			// 
			this.AutoCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoCheckBox, "US_AutoCreateOrganization");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.USMIDQuery)(null)).US_AutoCreateOrganization)));
			this.AutoCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.AutoCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ManufacturerQueryForm|d4b2c0d1-f25e-42cb-9898-31a8f387ebee", "Auto create organization based on response");
			this.AutoCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AutoCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(276, 45, true);
			this.AutoCheckBox.Name = "AutoCheckBox";
			this.AutoCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.AutoCheckBox.TabIndex = 1;
			this.AutoCheckBox.UseVisualStyleBackColor = true;
			// 
			// ManufacturerQueryForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 126, true);
			this.Controls.Add(this.AutoCheckBox);
			this.Controls.Add(this.US_MIDTextBox);
			this.Controls.Add(this.GiveUpButton);
			this.Controls.Add(this.SendButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.US.Business";
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.USMIDQuery);
			this.DataSourceTypeName = "Enterprise.Customs.US.Business.ManufacturerQueryMessageData";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "ManufacturerQueryForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "ManufacturerQueryForm";
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.GiveUpButton, 0);
			this.Controls.SetChildIndex(this.US_MIDTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.AutoCheckBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZButton SendButton;
		internal Enterprise.ZArchitecture.GUI.ZButton GiveUpButton;
		private Enterprise.ZArchitecture.ZTextBox US_MIDTextBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox AutoCheckBox;
	}
}

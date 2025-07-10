namespace Enterprise.Customs.US.GUI
{
	partial class Box29Form
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
		protected override void InitializeComponent()
		{
			this.Box29TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Box29IncludeContainersCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 158, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.Box29Data);
			// 
			// Box29TextBox
			// 
			this.BindingSource.SetBindingMember(this.Box29TextBox, "US_Box29Text");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.Box29Data)(null)).US_Box29Text)));
			this.Box29TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Box29TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 19, true);
			this.Box29TextBox.Multiline = true;
			this.Box29TextBox.Name = "Box29TextBox";
			this.Box29TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 62, true);
			this.Box29TextBox.TabIndex = 0;
			// 
			// OKButton
			// 
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 129, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.Text = "&OK";
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// Box29IncludeContainersCheckBox
			// 
			this.Box29IncludeContainersCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.Box29IncludeContainersCheckBox, "US_Box29IncludeContainers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.Box29Data)(null)).US_Box29IncludeContainers)));
			this.Box29IncludeContainersCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.Box29IncludeContainersCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 87, true);
			this.Box29IncludeContainersCheckBox.Name = "Box29IncludeContainersCheckBox";
			this.Box29IncludeContainersCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 17, true);
			this.Box29IncludeContainersCheckBox.TabIndex = 1;
			this.Box29IncludeContainersCheckBox.Text = "Include Containers";
			this.Box29IncludeContainersCheckBox.UseVisualStyleBackColor = true;
			// 
			// MainGroupBox
			// 
			this.MainGroupBox.Controls.Add(this.Box29TextBox);
			this.MainGroupBox.Controls.Add(this.Box29IncludeContainersCheckBox);
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 117, true);
			this.MainGroupBox.TabIndex = 0;
			this.MainGroupBox.TabStop = false;
			this.MainGroupBox.Text = "Box 29 Data";
			// 
			// Box29Form
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 182, true);
			this.Controls.Add(this.MainGroupBox);
			this.Controls.Add(this.OKButton);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.Box29Data);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MinimizeBox = false;
			this.Name = "Box29Form";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Box 29 Data";
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.MainGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainGroupBox.ResumeLayout(false);
			this.MainGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox Box29TextBox;
		private Enterprise.ZArchitecture.GUI.ZButton OKButton;
		private Enterprise.ZArchitecture.GUI.ZCheckBox Box29IncludeContainersCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox MainGroupBox;
	}
}

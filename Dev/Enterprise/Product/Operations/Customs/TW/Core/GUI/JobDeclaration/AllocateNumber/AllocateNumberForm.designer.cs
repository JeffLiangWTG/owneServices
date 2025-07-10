namespace Enterprise.Customs.TW.GUI
{
	partial class AllocateNumberForm
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
		protected new void InitializeComponent()
		{
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AllocateNumberGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.Part5NumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Part4NumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Part3NumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Part2NumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.Part1NumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AllocateNumberGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 158, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.AllocateNumber);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("6a057d1b-59ba-40b9-ba74-e101b2ce60ba", "Cancel");
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Cancel_Button, false);
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 129, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.Cancel_Button.TabIndex = 2;
			this.Cancel_Button.ToolTipCaption = null;
			this.Cancel_Button.UseVisualStyleBackColor = true;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OKButton, false);
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 129, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 23, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// AllocateNumberGroupBox
			// 
			this.AllocateNumberGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("d0bce8c7-5068-476f-a068-161fae16876c", "Entry Number");
			this.AllocateNumberGroupBox.Controls.Add(this.Part5NumberTextBox);
			this.AllocateNumberGroupBox.Controls.Add(this.Part4NumberTextBox);
			this.AllocateNumberGroupBox.Controls.Add(this.Part3NumberTextBox);
			this.AllocateNumberGroupBox.Controls.Add(this.Part2NumberTextBox);
			this.AllocateNumberGroupBox.Controls.Add(this.DescriptionLabel);
			this.AllocateNumberGroupBox.Controls.Add(this.Part1NumberTextBox);
			this.AllocateNumberGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.AllocateNumberGroupBox.Name = "AllocateNumberGroupBox";
			this.AllocateNumberGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(361, 114, true);
			this.AllocateNumberGroupBox.TabIndex = 0;
			this.AllocateNumberGroupBox.TabStop = false;
			// 
			// Part5NumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.Part5NumberTextBox, "Part5Number");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.AllocateNumber)(null)).Part5Number)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Part5NumberTextBox, false);
			this.Part5NumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 76, true);
			this.Part5NumberTextBox.Name = "Part5NumberTextBox";
			this.Part5NumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 20, true);
			this.Part5NumberTextBox.TabIndex = 6;
			this.Part5NumberTextBox.TextChanged += new System.EventHandler(this.Part5NumberTextBox_TextChanged);
			// 
			// Part4NumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.Part4NumberTextBox, "Part4Number");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.AllocateNumber)(null)).Part4Number)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Part4NumberTextBox, false);
			this.Part4NumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 76, true);
			this.Part4NumberTextBox.Name = "Part4NumberTextBox";
			this.Part4NumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 20, true);
			this.Part4NumberTextBox.TabIndex = 5;
			// 
			// Part3NumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.Part3NumberTextBox, "Part3Number");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.AllocateNumber)(null)).Part3Number)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Part3NumberTextBox, false);
			this.Part3NumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 76, true);
			this.Part3NumberTextBox.Name = "Part3NumberTextBox";
			this.Part3NumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 20, true);
			this.Part3NumberTextBox.TabIndex = 4;
			// 
			// Part2NumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.Part2NumberTextBox, "Part2Number");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.AllocateNumber)(null)).Part2Number)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Part2NumberTextBox, false);
			this.Part2NumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 76, true);
			this.Part2NumberTextBox.Name = "Part2NumberTextBox";
			this.Part2NumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 20, true);
			this.Part2NumberTextBox.TabIndex = 3;
			// 
			// DescriptionLabel
			// 
			this.DescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 27, true);
			this.DescriptionLabel.Name = "DescriptionLabel";
			this.DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 36, true);
			this.DescriptionLabel.TabIndex = 0;
			this.DescriptionLabel.UseMnemonic = false;
			// 
			// Part1NumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.Part1NumberTextBox, "Part1Number");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.AllocateNumber)(null)).Part1Number)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Part1NumberTextBox, false);
			this.Part1NumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 76, true);
			this.Part1NumberTextBox.Name = "Part1NumberTextBox";
			this.Part1NumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 20, true);
			this.Part1NumberTextBox.TabIndex = 2;
			// 
			// AllocateNumberForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 182, true);
			this.Controls.Add(this.AllocateNumberGroupBox);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.Cancel_Button);
			this.DataSourceType = typeof(Enterprise.Customs.TW.Business.AllocateNumber);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "AllocateNumberForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "AllocateEntryNumberForm";
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.AllocateNumberGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AllocateNumberGroupBox.ResumeLayout(false);
			this.AllocateNumberGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton Cancel_Button;
		private Enterprise.ZArchitecture.GUI.ZButton OKButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox AllocateNumberGroupBox;
		private Enterprise.ZArchitecture.ZTextBox Part1NumberTextBox;
		private Enterprise.ZArchitecture.ZLabel DescriptionLabel;
		public ZArchitecture.ZTextBox Part5NumberTextBox;
		private ZArchitecture.ZTextBox Part4NumberTextBox;
		private ZArchitecture.ZTextBox Part3NumberTextBox;
		private ZArchitecture.ZTextBox Part2NumberTextBox;
	}
}

namespace Enterprise.Customs.US.GUI
{
	partial class AllocateInBondNumberForm
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
		new void InitializeComponent()
		{
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AllocateInBondNumberGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DescriptionLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.DescriptionLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.AI_InBondNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AllocateInBondNumberGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 182, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.AllocateInBondNumber);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("AllocateInBondNumberForm|1f85eb5b-97f2-4fce-827b-9520305dd985", "&Cancel");
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 153, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.Cancel_Button.TabIndex = 2;
			this.Cancel_Button.UseVisualStyleBackColor = true;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("AllocateInBondNumberForm|0aa35c06-e04f-4159-b06d-060bc8103211", "&Allocate In-Bond Number");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 153, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 23, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// AllocateInBondNumberGroupBox
			// 
			this.AllocateInBondNumberGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("AllocateInBondNumberForm|f1b6f3d1-8a2b-4f11-9610-39e514cb364c", "In-Bond Number");
			this.AllocateInBondNumberGroupBox.Controls.Add(this.DescriptionLabel2);
			this.AllocateInBondNumberGroupBox.Controls.Add(this.DescriptionLabel1);
			this.AllocateInBondNumberGroupBox.Controls.Add(this.AI_InBondNumberTextBox);
			this.AllocateInBondNumberGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.AllocateInBondNumberGroupBox.Name = "AllocateInBondNumberGroupBox";
			this.AllocateInBondNumberGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(361, 145, true);
			this.AllocateInBondNumberGroupBox.TabIndex = 0;
			this.AllocateInBondNumberGroupBox.TabStop = false;
			// 
			// DescriptionLabel2
			// 
			this.DescriptionLabel2.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			this.DescriptionLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 61, true);
			this.DescriptionLabel2.Name = "DescriptionLabel2";
			this.DescriptionLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 34, true);
			this.DescriptionLabel2.TabIndex = 3;
			this.DescriptionLabel2.Text = "Alternatively, enter an In-Bond Number and click Allocate to allocate that number" +
				" to this job.";
			// 
			// DescriptionLabel1
			// 
			this.DescriptionLabel1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("AllocateInBondNumberForm|1ee2ae00-69c0-4df5-92fb-7bf1a180e58a", "", "Enter the \'In-Bond Number\' and click OK or leave In-Bond Number blank and the next available In-Bond Number will be allocated.");
			this.DescriptionLabel1.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			this.DescriptionLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 27, true);
			this.DescriptionLabel1.Name = "DescriptionLabel1";
			this.DescriptionLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 34, true);
			this.DescriptionLabel1.TabIndex = 0;
			this.DescriptionLabel1.Text = "Leave In-Bond Number blank and click \'Allocate In-Bond Number\' for the next avail" +
				"able In-Bond Number to be allocated.";
			// 
			// AI_InBondNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.AI_InBondNumberTextBox, "AI_InBondNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AllocateInBondNumber)(null)).AI_InBondNumber)));
			this.AI_InBondNumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("AllocateInBondNumberForm|3a2f87c6-6d06-4349-93c6-1992067c7923", "In-Bond Number");
			this.AI_InBondNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 109, true);
			this.AI_InBondNumberTextBox.Name = "AI_InBondNumberTextBox";
			this.AI_InBondNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.AI_InBondNumberTextBox.TabIndex = 2;
			// 
			// AllocateInBondNumberForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.Cancel_Button;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 206, true);
			this.Controls.Add(this.AllocateInBondNumberGroupBox);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.Cancel_Button);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.AllocateInBondNumber);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "AllocateInBondNumberForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.AllocateInBondNumberGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AllocateInBondNumberGroupBox.ResumeLayout(false);
			this.AllocateInBondNumberGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton Cancel_Button;
		public Enterprise.ZArchitecture.GUI.ZButton OKButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox AllocateInBondNumberGroupBox;
		private Enterprise.ZArchitecture.ZTextBox AI_InBondNumberTextBox;
		private Enterprise.ZArchitecture.ZLabel DescriptionLabel1;
		private ZArchitecture.ZLabel DescriptionLabel2;
	}
}

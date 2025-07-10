namespace Enterprise.Customs.GUI
{
	partial class MessageSendingFormBottomSectionUserControl
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
			this.AdditionalWarningsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalWarningsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AdditionalWarningsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// AdditionalWarningsGroupBox
			// 
			this.AdditionalWarningsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("99d4c4a6-6165-4de1-ba6b-6fdec9473303", "Additional Warnings");
			this.AdditionalWarningsGroupBox.Controls.Add(this.AdditionalWarningsTextBox);
			this.AdditionalWarningsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalWarningsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalWarningsGroupBox.Name = "AdditionalWarningsGroupBox";
			this.AdditionalWarningsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 122, true);
			this.AdditionalWarningsGroupBox.TabIndex = 3;
			this.AdditionalWarningsGroupBox.TabStop = false;
			// 
			// AdditionalWarningsTextBox
			// 
			this.BindingSource.SetBindingMember(this.AdditionalWarningsTextBox, "AdditionalWarnings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.DataSourceType = typeof(Enterprise.Customs.Business.IJobDeclarationMessageSendingObjectParent);
			this.AdditionalWarningsTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ec8a0903-2b4b-41d6-b036-4e9c5598f80e", "Additional Warnings");
			this.AdditionalWarningsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AdditionalWarningsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AdditionalWarningsTextBox, false);
			this.AdditionalWarningsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.AdditionalWarningsTextBox.Multiline = true;
			this.AdditionalWarningsTextBox.Name = "AdditionalWarningsTextBox";
			this.AdditionalWarningsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.AdditionalWarningsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 106, true);
			this.AdditionalWarningsTextBox.TabIndex = 0;
			// 
			// MessageSendingFormBottomSectionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalWarningsGroupBox);
			this.Name = "MessageSendingFormBottomSectionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 122, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalWarningsGroupBox.ResumeLayout(false);
			this.AdditionalWarningsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox AdditionalWarningsGroupBox;
		private ZArchitecture.ZTextBox AdditionalWarningsTextBox;
	}
}

namespace Enterprise.Customs.TW.GUI
{
	partial class DocumentNumbersUserControl
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
            this.DocumentNumbersEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.DocumentNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.CusEntryInstruction);
            // 
            // DocumentNumbersEditButton
            // 
            this.DocumentNumbersEditButton.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("471ecddd-b238-4471-aac0-a7b1732fd1b5", "More ...");
            this.DocumentNumbersEditButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.DocumentNumbersEditButton.IsCaptionOverridden = false;
            this.DocumentNumbersEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 0, true);
            this.DocumentNumbersEditButton.Name = "DocumentNumbersEditButton";
            this.DocumentNumbersEditButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
            this.DocumentNumbersEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
            this.DocumentNumbersEditButton.TabIndex = 1;
            this.DocumentNumbersEditButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            this.DocumentNumbersEditButton.ToolTipCaption = null;
            this.DocumentNumbersEditButton.UseVisualStyleBackColor = true;
            this.DocumentNumbersEditButton.Click += new System.EventHandler(this.DocumentNumbersEditButton_Click);
            // 
            // DocumentNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.DocumentNumberTextBox, "AttachedDocumentNumbersAsString");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusEntryInstruction)(null)).AttachedDocumentNumbersAsString)));
            this.DocumentNumberTextBox.CaptionResourceString = null;
            this.DocumentNumberTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DocumentNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.DocumentNumberTextBox.Name = "DocumentNumberTextBox";
            this.DocumentNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 20, true);
            this.DocumentNumberTextBox.TabIndex = 0;
            // 
            // DocumentNumbersUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Transparent;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.DocumentNumberTextBox);
            this.Controls.Add(this.DocumentNumbersEditButton);
            this.Name = "DocumentNumbersUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 20, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton DocumentNumbersEditButton;
		private ZArchitecture.ZTextBox DocumentNumberTextBox;
	}
}

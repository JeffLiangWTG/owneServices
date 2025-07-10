namespace Enterprise.MasterFiles.GUI
{
	partial class FormStateXmlExplorerForm
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
		private void InitializeComponent()
		{
			this.richTextBox = new CargoWise.Windows.UI.KRichTextBox();
			this.exitButton = new CargoWise.Windows.UI.KButton();
			this.validateAndCommitButton = new CargoWise.Windows.UI.KButton();
			this.validateButton = new CargoWise.Windows.UI.KButton();
			this.SuspendLayout();
			// 
			// richTextBox
			// 
			this.richTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.richTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.richTextBox.Name = "richTextBox";
			this.richTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 407, true);
			this.richTextBox.TabIndex = 0;
			this.richTextBox.Text = "";
			// 
			// exitButton
			// 
			this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.exitButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 416, true);
			this.exitButton.Name = "exitButton";
			this.exitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.exitButton.TabIndex = 1;
			this.exitButton.Text = "Exit";
			this.exitButton.UseVisualStyleBackColor = true;
			// 
			// validateAndCommitButton
			// 
			this.validateAndCommitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.validateAndCommitButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 416, true);
			this.validateAndCommitButton.Name = "validateAndCommitButton";
			this.validateAndCommitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 23, true);
			this.validateAndCommitButton.TabIndex = 2;
			this.validateAndCommitButton.Text = "Validate && Commit";
			this.validateAndCommitButton.UseVisualStyleBackColor = true;
			// 
			// validateButton
			// 
			this.validateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.validateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 416, true);
			this.validateButton.Name = "validateButton";
			this.validateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.validateButton.TabIndex = 4;
			this.validateButton.Text = "Validate";
			this.validateButton.UseVisualStyleBackColor = true;
			// 
			// FormStateXmlExplorerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 446, true);
			this.Controls.Add(this.validateButton);
			this.Controls.Add(this.validateAndCommitButton);
			this.Controls.Add(this.exitButton);
			this.Controls.Add(this.richTextBox);
			this.Name = "FormStateXmlExplorerForm";
			this.ShowIcon = false;
			this.Text = "Form State Xml Explorer";
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KRichTextBox richTextBox;
		private CargoWise.Windows.UI.KButton exitButton;
		private CargoWise.Windows.UI.KButton validateAndCommitButton;
		private CargoWise.Windows.UI.KButton validateButton;
	}
}
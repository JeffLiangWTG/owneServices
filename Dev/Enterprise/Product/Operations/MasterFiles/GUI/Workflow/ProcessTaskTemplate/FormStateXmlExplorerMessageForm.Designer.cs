namespace Enterprise.MasterFiles.GUI
{
	partial class FormStateXmlExplorerMessageForm
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
			this.button2 = new CargoWise.Windows.UI.KButton();
			this.messageTextBox = new CargoWise.Windows.UI.KTextBox();
			this.button1 = new CargoWise.Windows.UI.KButton();
			this.SuspendLayout();
			// 
			// button2
			// 
			this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 285, true);
			this.button2.Name = "button2";
			this.button2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.button2.TabIndex = 0;
			this.button2.UseVisualStyleBackColor = true;
			// 
			// messageTextBox
			// 
			this.messageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.messageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.messageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 2, true);
			this.messageTextBox.Multiline = true;
			this.messageTextBox.Name = "messageTextBox";
			this.messageTextBox.ReadOnly = true;
			this.messageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 275, true);
			this.messageTextBox.TabIndex = 2;
			this.messageTextBox.WordWrap = false;
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 285, true);
			this.button1.Name = "button1";
			this.button1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.button1.TabIndex = 3;
			this.button1.UseVisualStyleBackColor = true;
			// 
			// FormStateXmlExplorerMessageForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 316, true);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.messageTextBox);
			this.Controls.Add(this.button2);
			this.MinimizeBox = false;
			this.Name = "FormStateXmlExplorerMessageForm";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Message";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KButton button2;
		private CargoWise.Windows.UI.KTextBox messageTextBox;
		private CargoWise.Windows.UI.KButton button1;
	}
}
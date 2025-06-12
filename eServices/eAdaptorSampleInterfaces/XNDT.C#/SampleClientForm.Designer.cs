using System.Windows.Forms;

namespace WTG.XNDT.SampleClient
{
	partial class SampleClientForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private Label addressLabel;
		private TextBox addressTextBox;
		private string endpointConfigurationName;
		private Label responseLabel;
		private Label requestLabel;
		private string remoteAddress;
		private TextBox requestTextBox;
		private TextBox responseTextBox;
		private Button retrieveButton;
		private Button updateButton;

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
			this.addressLabel = new System.Windows.Forms.Label();
			this.addressTextBox = new System.Windows.Forms.TextBox();
			this.requestLabel = new System.Windows.Forms.Label();
			this.requestTextBox = new System.Windows.Forms.TextBox();
			this.responseTextBox = new System.Windows.Forms.TextBox();
			this.responseLabel = new System.Windows.Forms.Label();
			this.updateButton = new System.Windows.Forms.Button();
			this.retrieveButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// addressLabel
			// 
			this.addressLabel.AutoSize = true;
			this.addressLabel.Location = new System.Drawing.Point(11, 9);
			this.addressLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.addressLabel.Name = "addressLabel";
			this.addressLabel.Size = new System.Drawing.Size(87, 13);
			this.addressLabel.TabIndex = 0;
			this.addressLabel.Text = "Service Address:";
			// 
			// addressTextBox
			// 
			this.addressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.addressTextBox.Location = new System.Drawing.Point(102, 6);
			this.addressTextBox.Margin = new System.Windows.Forms.Padding(2);
			this.addressTextBox.Name = "addressTextBox";
			this.addressTextBox.Size = new System.Drawing.Size(611, 20);
			this.addressTextBox.TabIndex = 1;
			// 
			// requestLabel
			// 
			this.requestLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.requestLabel.AutoSize = true;
			this.requestLabel.Location = new System.Drawing.Point(11, 44);
			this.requestLabel.Name = "requestLabel";
			this.requestLabel.Size = new System.Drawing.Size(50, 13);
			this.requestLabel.TabIndex = 4;
			this.requestLabel.Text = "Request:";
			// 
			// requestTextBox
			// 
			this.requestTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.requestTextBox.Location = new System.Drawing.Point(4, 60);
			this.requestTextBox.Multiline = true;
			this.requestTextBox.Name = "requestTextBox";
			this.requestTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.requestTextBox.Size = new System.Drawing.Size(451, 540);
			this.requestTextBox.TabIndex = 5;
			this.requestTextBox.WordWrap = false;
			// 
			// responseTextBox
			// 
			this.responseTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.responseTextBox.Location = new System.Drawing.Point(461, 60);
			this.responseTextBox.Multiline = true;
			this.responseTextBox.Name = "responseTextBox";
			this.responseTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.responseTextBox.Size = new System.Drawing.Size(490, 540);
			this.responseTextBox.TabIndex = 7;
			this.responseTextBox.WordWrap = false;
			// 
			// responseLabel
			// 
			this.responseLabel.AutoSize = true;
			this.responseLabel.Location = new System.Drawing.Point(467, 44);
			this.responseLabel.Name = "responseLabel";
			this.responseLabel.Size = new System.Drawing.Size(58, 13);
			this.responseLabel.TabIndex = 6;
			this.responseLabel.Text = "Response:";
			// 
			// updateButton
			// 
			this.updateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.updateButton.Location = new System.Drawing.Point(838, 4);
			this.updateButton.Name = "updateButton";
			this.updateButton.Size = new System.Drawing.Size(114, 23);
			this.updateButton.TabIndex = 3;
			this.updateButton.Text = "Update";
			this.updateButton.UseVisualStyleBackColor = true;
			this.updateButton.Click += new System.EventHandler(this.UpdateButton_Click);
			// 
			// retrieveButton
			// 
			this.retrieveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.retrieveButton.Location = new System.Drawing.Point(718, 4);
			this.retrieveButton.Name = "retrieveButton";
			this.retrieveButton.Size = new System.Drawing.Size(114, 23);
			this.retrieveButton.TabIndex = 2;
			this.retrieveButton.Text = "Retrieve";
			this.retrieveButton.UseVisualStyleBackColor = true;
			this.retrieveButton.Click += new System.EventHandler(this.RetrieveButton_Click);
			// 
			// SampleClientForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(955, 603);
			this.Controls.Add(this.responseTextBox);
			this.Controls.Add(this.requestTextBox);
			this.Controls.Add(this.responseLabel);
			this.Controls.Add(this.requestLabel);
			this.Controls.Add(this.addressTextBox);
			this.Controls.Add(this.addressLabel);
			this.Controls.Add(this.retrieveButton);
			this.Controls.Add(this.updateButton);
			this.MinimumSize = new System.Drawing.Size(971, 642);
			this.Name = "SampleClientForm";
			this.Text = "XNDT Sample Client";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}

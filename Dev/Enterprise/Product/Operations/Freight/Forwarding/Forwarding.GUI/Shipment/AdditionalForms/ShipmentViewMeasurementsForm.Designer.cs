using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using System;
using System.Windows.Forms;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class ShipmentViewMeasurementsForm
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

		ZButton CloseButton;
		RichTextBox TextBox;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TextBox = new RichTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// CloseButton
			//
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.IsCaptionOverridden = true;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(467, 320, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 23, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.Text = Res.GetString("cfbc824f-2087-da86-48e8-425a8a42428b", "Close");
			this.CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			//
			// TextBox
			//
			this.TextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 10, true);
			this.TextBox.Multiline = true;
			this.TextBox.Name = "TextBox";
			this.TextBox.ReadOnly = true;
			this.TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 275, true);
			this.TextBox.TabIndex = 1;
			this.TextBox.TabStop = false;
			this.TextBox.Text = Res.GetString("84e14575-05d9-4b91-4461-957f70bd1001", "No text found");
			this.TextBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            this.TextBox.WordWrap = false;
			//
			// TextDialogWithCloseButtonForm
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 375, true);
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.Controls.Add(this.TextBox);
			this.Controls.Add(this.CloseButton);
			this.Name = "ShipmentViewMeasurementsForm";
			this.Text = Res.GetString("7883f143-76de-f1bc-471f-62817c327cbf", "Form");
			this.CaptionRenderingEnabled = false;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.TextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		#region Close

		void CloseButton_Click(object sender, EventArgs e) => Close();

		#endregion
	}
}

using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.CFS.GUI
{
	/// <summary>
	/// Summary description for CoLoadWizardForm.
	/// </summary>
	public partial class CoLoadWizardForm : ZChildForm
	{
		ZGroupBox DividingLineGroupBox;
		ZPanel CurrentWizardPagePanel;
		ZButton NextButton;
		ZButton BackButton;
		ZButton CancelFormButton;

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.NextButton = new ZButton();
			this.BackButton = new ZButton();
			this.CancelFormButton = new ZButton();
			this.DividingLineGroupBox = new ZGroupBox();
			this.CurrentWizardPagePanel = new ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = ControlDpiScalingHelper.NewScaledPoint(0, 415, true);
			this.MainStatusBar.Size = ControlDpiScalingHelper.NewScaledSize(424, 23, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(212);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(212);
			// 
			// NextButton
			// 
			this.NextButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			this.NextButton.CaptionResourceString = Res.GetData("CoLoadWizardForm|25fcadc3-78bf-4d07-85aa-c1c3ae9d2359", "&Next >");
			this.NextButton.Location = ControlDpiScalingHelper.NewScaledPoint(232, 388, true);
			this.NextButton.Name = "NextButton";
			this.NextButton.Size = ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.NextButton.TabIndex = 3;
			this.NextButton.Click += new EventHandler(this.NextButton_Click);
			// 
			// BackButton
			// 
			this.BackButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			this.BackButton.CaptionResourceString = Res.GetData("CoLoadWizardForm|4edc1c22-2fb2-46e6-8d3b-f06591bcd215", "< &Back");
			this.BackButton.Location = ControlDpiScalingHelper.NewScaledPoint(152, 388, true);
			this.BackButton.Name = "BackButton";
			this.BackButton.Size = ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.BackButton.TabIndex = 4;
			this.BackButton.Click += new EventHandler(this.BackButton_Click);
			// 
			// CancelFormButton
			// 
			this.CancelFormButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			this.CancelFormButton.CaptionResourceString = Res.GetData("CoLoadWizardForm|3e4ab643-ad70-4077-8d73-4c18dccb6560", "Cancel");
			this.CancelFormButton.DialogResult = DialogResult.Cancel;
			this.CancelFormButton.Location = ControlDpiScalingHelper.NewScaledPoint(328, 388, true);
			this.CancelFormButton.Name = "CancelFormButton";
			this.CancelFormButton.Size = ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelFormButton.TabIndex = 5;
			// 
			// DividingLineGroupBox
			// 
			this.DividingLineGroupBox.Anchor = ((AnchorStyles)(((AnchorStyles.Bottom | AnchorStyles.Left)
						| AnchorStyles.Right)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DividingLineGroupBox, false);
			this.DividingLineGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(8, 373, true);
			this.DividingLineGroupBox.Name = "DividingLineGroupBox";
			this.DividingLineGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(408, 7, true);
			this.DividingLineGroupBox.TabIndex = 2;
			this.DividingLineGroupBox.TabStop = false;
			// 
			// CurrentWizardPagePanel
			// 
			this.CurrentWizardPagePanel.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom)
						| AnchorStyles.Left)
						| AnchorStyles.Right)));
			this.CurrentWizardPagePanel.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CurrentWizardPagePanel.Name = "CurrentWizardPagePanel";
			this.CurrentWizardPagePanel.Size = ControlDpiScalingHelper.NewScaledSize(424, 365, true);
			this.CurrentWizardPagePanel.TabIndex = 0;
			// 
			// CoLoadWizardForm
			// 
			this.AcceptButton = this.NextButton;

			this.CancelButton = this.CancelFormButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = ControlDpiScalingHelper.NewScaledSize(424, 438, true);
			this.Controls.Add(this.CurrentWizardPagePanel);
			this.Controls.Add(this.DividingLineGroupBox);
			this.Controls.Add(this.CancelFormButton);
			this.Controls.Add(this.BackButton);
			this.Controls.Add(this.NextButton);
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.HelpButton = true;
			this.MinimizeBox = false;
			this.Name = "CoLoadWizardForm";
			this.RememberFormSize = false;
			this.Controls.SetChildIndex(this.NextButton, 0);
			this.Controls.SetChildIndex(this.BackButton, 0);
			this.Controls.SetChildIndex(this.CancelFormButton, 0);
			this.Controls.SetChildIndex(this.DividingLineGroupBox, 0);
			this.Controls.SetChildIndex(this.CurrentWizardPagePanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion
	}
}

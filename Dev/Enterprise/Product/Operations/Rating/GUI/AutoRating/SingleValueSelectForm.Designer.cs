namespace Enterprise.Rating.GUI
{
	partial class SingleValueSelectForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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
			this.InstructionsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.checkedListBox = new Enterprise.ZArchitecture.GUI.ZCheckedListBox();
			this.noButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.yesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 265, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// InstructionsLabel
			// 
			this.InstructionsLabel.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("2fb6952d-0f1a-4093-b7dc-8f90a02fa974", "Instructions");
			this.InstructionsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.InstructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 6, true);
			this.InstructionsLabel.Name = "InstructionsLabel";
			this.InstructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(373, 58, true);
			this.InstructionsLabel.TabIndex = 0;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("bfaad9f5-15e5-4f3d-889e-077f38278b3b", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(345, 235, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("3d6059de-af17-4247-b5df-e119d5c22455", "OK");
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(285, 235, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.okButton.TabIndex = 2;
			this.okButton.ToolTipCaption = null;
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// checkedListBox
			// 
			this.checkedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.checkedListBox.BindingItems = null;
			this.checkedListBox.CheckOnClick = true;
			this.checkedListBox.FormattingEnabled = true;
			this.checkedListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 70, true);
			this.checkedListBox.Name = "checkedListBox";
			this.checkedListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(373, 156, true);
			this.checkedListBox.TabIndex = 1;
			// 
			// noButton
			// 
			this.noButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.noButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("2a4dcd34-bfd9-444d-8315-64088c851a39", "No");
			this.noButton.DialogResult = System.Windows.Forms.DialogResult.No;
			this.noButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 235, true);
			this.noButton.Name = "noButton";
			this.noButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.noButton.TabIndex = 5;
			this.noButton.ToolTipCaption = null;
			this.noButton.UseVisualStyleBackColor = true;
			this.noButton.Visible = false;
			// 
			// yesButton
			// 
			this.yesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.yesButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("e37c5ba5-afa0-432d-9949-7a5b4d70a4ae", "Yes");
			this.yesButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
			this.yesButton.Enabled = false;
			this.yesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 235, true);
			this.yesButton.Name = "yesButton";
			this.yesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.yesButton.TabIndex = 6;
			this.yesButton.ToolTipCaption = null;
			this.yesButton.UseVisualStyleBackColor = true;
			this.yesButton.Visible = false;
			// 
			// SingleValueSelectForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("b01fc860-d797-44ce-b3c5-9448f5fa3d76", "Single Value Selection");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 289, true);
			this.Controls.Add(this.yesButton);
			this.Controls.Add(this.noButton);
			this.Controls.Add(this.checkedListBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.InstructionsLabel);
			this.Name = "SingleValueSelectForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.InstructionsLabel, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.checkedListBox, 0);
			this.Controls.SetChildIndex(this.noButton, 0);
			this.Controls.SetChildIndex(this.yesButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.ZLabel InstructionsLabel;
		ZArchitecture.GUI.ZButton cancelButton;
		ZArchitecture.GUI.ZButton okButton;
		ZArchitecture.GUI.ZCheckedListBox checkedListBox;
		ZArchitecture.GUI.ZButton noButton;
		ZArchitecture.GUI.ZButton yesButton;
	}
}

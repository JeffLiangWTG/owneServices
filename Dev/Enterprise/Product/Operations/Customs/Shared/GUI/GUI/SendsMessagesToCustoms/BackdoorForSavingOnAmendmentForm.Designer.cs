namespace Enterprise.Customs.GUI
{
	partial class BackdoorForSavingOnAmendmentForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		protected Enterprise.ZArchitecture.ZLabel TextLabel;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox OptionsGroupBox;
		protected internal Enterprise.ZArchitecture.GUI.ZRadioButton SendWithoutSendingAmendmentAtAllRadioButton;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton SaveWithoutSendingToDeferAmendmentSendingRadioButton;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton SendAmendmentRadioButton;
		protected Enterprise.ZArchitecture.GUI.ZButton OKButton;
		protected internal Enterprise.ZArchitecture.GUI.ZButton CancelButton2;
		protected internal Enterprise.ZArchitecture.GUI.ZButton SendAmendmentExplanationButton;
		protected internal Enterprise.ZArchitecture.GUI.ZButton SaveWithoutEntryChangesExplanationButton;
		protected internal Enterprise.ZArchitecture.GUI.ZButton SaveWithEntryChangesExplanationButton;
		private System.ComponentModel.Container components = null;

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BackdoorForSavingOnAmendmentForm));
			this.TextLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SaveWithEntryChangesExplanationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SaveWithoutEntryChangesExplanationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendAmendmentExplanationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendWithoutSendingAmendmentAtAllRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.SaveWithoutSendingToDeferAmendmentSendingRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.SendAmendmentRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton2 = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OptionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 185, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 23, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(333);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(333);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.DeferredAmendmentSavingOptions);
			// 
			// TextLabel
			// 
			this.TextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.TextLabel.Name = "TextLabel";
			this.TextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 45, true);
			this.TextLabel.TabIndex = 0;
			this.TextLabel.Text = resources.GetString("TextLabel.Text");
			// 
			// OptionsGroupBox
			// 
			this.OptionsGroupBox.Controls.Add(this.SaveWithEntryChangesExplanationButton);
			this.OptionsGroupBox.Controls.Add(this.SaveWithoutEntryChangesExplanationButton);
			this.OptionsGroupBox.Controls.Add(this.SendAmendmentExplanationButton);
			this.OptionsGroupBox.Controls.Add(this.SendWithoutSendingAmendmentAtAllRadioButton);
			this.OptionsGroupBox.Controls.Add(this.SaveWithoutSendingToDeferAmendmentSendingRadioButton);
			this.OptionsGroupBox.Controls.Add(this.SendAmendmentRadioButton);
			this.OptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 52, true);
			this.OptionsGroupBox.Name = "OptionsGroupBox";
			this.OptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(654, 97, true);
			this.OptionsGroupBox.TabIndex = 1;
			this.OptionsGroupBox.TabStop = false;
			this.OptionsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("0CF5813A-8B53-42F1-98BA-6ADE074B9AA4", "Options");
			// 
			// SaveWithEntryChangesExplanationButton
			// 
			this.SaveWithEntryChangesExplanationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 68, true);
			this.SaveWithEntryChangesExplanationButton.Name = "SaveWithEntryChangesExplanationButton";
			this.SaveWithEntryChangesExplanationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 21, true);
			this.SaveWithEntryChangesExplanationButton.TabIndex = 4;
			this.SaveWithEntryChangesExplanationButton.Text = "?";
			this.SaveWithEntryChangesExplanationButton.Click += new System.EventHandler(this.SaveWithEntryChangesExplanationButton_Click);
			// 
			// SaveWithoutEntryChangesExplanationButton
			// 
			this.SaveWithoutEntryChangesExplanationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 40, true);
			this.SaveWithoutEntryChangesExplanationButton.Name = "SaveWithoutEntryChangesExplanationButton";
			this.SaveWithoutEntryChangesExplanationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 21, true);
			this.SaveWithoutEntryChangesExplanationButton.TabIndex = 2;
			this.SaveWithoutEntryChangesExplanationButton.Text = "?";
			this.SaveWithoutEntryChangesExplanationButton.Click += new System.EventHandler(this.SaveWithoutEntryChangesExplanationButton_Click);
			// 
			// SendAmendmentExplanationButton
			// 
			this.SendAmendmentExplanationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 14, true);
			this.SendAmendmentExplanationButton.Name = "SendAmendmentExplanationButton";
			this.SendAmendmentExplanationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 21, true);
			this.SendAmendmentExplanationButton.TabIndex = 0;
			this.SendAmendmentExplanationButton.Text = "?";
			this.SendAmendmentExplanationButton.Click += new System.EventHandler(this.SendAmendmentExplanationButton_Click);
			// 
			// SendWithoutSendingAmendmentAtAllRadioButton
			// 
			this.SendWithoutSendingAmendmentAtAllRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.SendWithoutSendingAmendmentAtAllRadioButton, "SaveWithoutEntryChanges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.DeferredAmendmentSavingOptions)(null)).SaveWithoutEntryChanges)));
			this.SendWithoutSendingAmendmentAtAllRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SendWithoutSendingAmendmentAtAllRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 38, true);
			this.SendWithoutSendingAmendmentAtAllRadioButton.Name = "SendWithoutSendingAmendmentAtAllRadioButton";
			this.SendWithoutSendingAmendmentAtAllRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 24, true);
			this.SendWithoutSendingAmendmentAtAllRadioButton.TabIndex = 3;
			this.SendWithoutSendingAmendmentAtAllRadioButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("9E792C72-C92C-4451-8389-2BD686BFD82C", " Save WITHOUT sending amendment. Changes do NOT affect Customs Entry(s).");
			// 
			// SaveWithoutSendingToDeferAmendmentSendingRadioButton
			// 
			this.SaveWithoutSendingToDeferAmendmentSendingRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.SaveWithoutSendingToDeferAmendmentSendingRadioButton, "SaveWithEntryChanges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.DeferredAmendmentSavingOptions)(null)).SaveWithEntryChanges)));
			this.SaveWithoutSendingToDeferAmendmentSendingRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SaveWithoutSendingToDeferAmendmentSendingRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 66, true);
			this.SaveWithoutSendingToDeferAmendmentSendingRadioButton.Name = "SaveWithoutSendingToDeferAmendmentSendingRadioButton";
			this.SaveWithoutSendingToDeferAmendmentSendingRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 24, true);
			this.SaveWithoutSendingToDeferAmendmentSendingRadioButton.TabIndex = 5;
			this.SaveWithoutSendingToDeferAmendmentSendingRadioButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("01B62D9D-4172-4EDB-B6F2-EEE415847430",
				" Save WITHOUT sending amendment. Entry will be PENDING lodgement. Reflect all changes on screens and Entry prints.");
			// 
			// SendAmendmentRadioButton
			// 
			this.SendAmendmentRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.SendAmendmentRadioButton, "SendAmendment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.DeferredAmendmentSavingOptions)(null)).SendAmendment)));
			this.SendAmendmentRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SendAmendmentRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 17, true);
			this.SendAmendmentRadioButton.Name = "SendAmendmentRadioButton";
			this.SendAmendmentRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 15, true);
			this.SendAmendmentRadioButton.TabIndex = 1;
			this.SendAmendmentRadioButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("D09F09AD-6545-4F6C-8AF1-87D0BE8743AD", " Send an amendment message NOW.");
			// 
			// OKButton
			// 
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(478, 156, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("D8D2FB65-536F-4635-9A89-8B46F6550BAD", "OK");
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CancelButton2
			// 
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(574, 156, true);
			this.CancelButton2.Name = "CancelButton2";
			this.CancelButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.CancelButton2.TabIndex = 3;
			this.CancelButton2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("6844DB70-9FB8-4093-A5C5-BAB23FDD790D", "Cancel");
			this.CancelButton2.Click += new System.EventHandler(this.CancelButton2_Click);
			// 
			// BackdoorForSavingOnAmendmentForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 208, true);
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CancelButton2);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.OptionsGroupBox);
			this.Controls.Add(this.TextLabel);
			this.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.DataSourceType = typeof(Enterprise.Customs.Business.DeferredAmendmentSavingOptions);
			this.DataSourceTypeName = "Enterprise.Customs.Business.DeferredAmendmentSavingOptions";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "BackdoorForSavingOnAmendmentForm";
			this.Text = "CustomisedMessageBoxForm";
			this.Controls.SetChildIndex(this.TextLabel, 0);
			this.Controls.SetChildIndex(this.OptionsGroupBox, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelButton2, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OptionsGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion
	}
}

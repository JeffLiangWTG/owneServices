namespace Enterprise.DeniedPartyScreening.GUI
{
	partial class StandAloneScreeningForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.ScreenButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InstructionsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.pictureBox1 = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OtherInfoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OtherInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.CityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Address2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Address1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FullNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CountryFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.IdNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IdIssuingCountryFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ContactGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.VesselPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.VesselCallsignTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VesselLloydsNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VesselCountyOfRegFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TextboxPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.VesselRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.OrgRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.PersonRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.OtherInfoGroupBox.SuspendLayout();
			this.ContactGroupBox.SuspendLayout();
			this.VesselPanel.SuspendLayout();
			this.TextboxPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 402, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(702, 24, true);
			this.MainStatusBar.TabIndex = 8;
			// 
			// ScreenButton
			// 
			this.ScreenButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ScreenButton.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("StandAloneScreeningForm|19eac7f6-28e3-4f52-a707-51213291e178", "Start Screening");
			this.ScreenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 373, true);
			this.ScreenButton.Name = "ScreenButton";
			this.ScreenButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 23, true);
			this.ScreenButton.TabIndex = 6;
			this.ScreenButton.UseVisualStyleBackColor = true;
			this.ScreenButton.Click += new System.EventHandler(this.ScreenButton_Click);
			// 
			// InstructionsLabel
			// 
			this.InstructionsLabel.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("StandAloneScreeningForm|bc458bee-1930-4d4e-837c-832f1d02e43e", "", "Specify the information about the individual or business you are searching for and then press the \'Start Screening\' button.");
			this.InstructionsLabel.IsFontBold = true;
			this.InstructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 9, true);
			this.InstructionsLabel.Name = "InstructionsLabel";
			this.InstructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 41, true);
			this.InstructionsLabel.TabIndex = 0;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = global::Enterprise.DeniedPartyScreening.GUI.Properties.Resources.denied;
			this.pictureBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 5, true);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 67, true);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 6;
			this.pictureBox1.TabStop = false;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CloseButton.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("StandAloneScreeningForm|c6f84ba7-7166-456b-b1ca-bb6e6fc8990e", "Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 373, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 23, true);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// OtherInfoTextBox
			// 
			this.OtherInfoTextBox.AcceptsReturn = true;
			this.OtherInfoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.OtherInfoTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OtherInfoTextBox, false);
			this.OtherInfoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 33, true);
			this.OtherInfoTextBox.Multiline = true;
			this.OtherInfoTextBox.Name = "OtherInfoTextBox";
			this.OtherInfoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 52, true);
			this.OtherInfoTextBox.TabIndex = 0;
			// 
			// OtherInfoGroupBox
			// 
			this.OtherInfoGroupBox.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("StandAloneScreeningForm|2de3b423-d09e-4172-98c1-4afa30989dba", "Other Details");
			this.OtherInfoGroupBox.Controls.Add(this.zLabel2);
			this.OtherInfoGroupBox.Controls.Add(this.OtherInfoTextBox);
			this.OtherInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 266, true);
			this.OtherInfoGroupBox.Name = "OtherInfoGroupBox";
			this.OtherInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(685, 91, true);
			this.OtherInfoGroupBox.TabIndex = 2;
			this.OtherInfoGroupBox.TabStop = false;
			// 
			// zLabel2
			// 
			this.zLabel2.AutoSize = true;
			this.zLabel2.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("StandAloneScreeningForm|32f32a0f-cefa-4845-8d3d-f189f92b0028", "", "Enter any other search words, using a line break to separate them.");
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 14, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 13, true);
			this.zLabel2.TabIndex = 0;
			// 
			// CityTextBox
			// 
			this.CityTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CityTextBox.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("StandAloneScreeningForm|d6efc1e0-d1b7-422e-84f8-12daf0d2f581", "City");
			this.CityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 55, true);
			this.CityTextBox.Name = "CityTextBox";
			this.CityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.CityTextBox.TabIndex = 7;
			// 
			// Address2TextBox
			// 
			this.Address2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Address2TextBox.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("StandAloneScreeningForm|5109c6a2-1b38-4bc8-8043-2d590b95a761", "Address 2");
			this.Address2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 29, true);
			this.Address2TextBox.Name = "Address2TextBox";
			this.Address2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.Address2TextBox.TabIndex = 6;
			// 
			// StateTextBox
			// 
			this.StateTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.StateTextBox.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("StandAloneScreeningForm|c2947778-9183-429a-81e6-f5d9b9e250cb", "State");
			this.StateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 29, true);
			this.StateTextBox.Name = "StateTextBox";
			this.StateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.StateTextBox.TabIndex = 10;
			// 
			// Address1TextBox
			// 
			this.Address1TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Address1TextBox.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("StandAloneScreeningForm|96b56078-40d6-4873-a1af-858180481b7d", "Address 1");
			this.Address1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 3, true);
			this.Address1TextBox.Name = "Address1TextBox";
			this.Address1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.Address1TextBox.TabIndex = 5;
			// 
			// FullNameTextBox
			// 
			this.FullNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FullNameTextBox.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("StandAloneScreeningForm|2f4add30-5666-416a-be4d-8c0d59cbbeca", "Full Name");
			this.FullNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 19, true);
			this.FullNameTextBox.Name = "FullNameTextBox";
			this.FullNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 20, true);
			this.FullNameTextBox.TabIndex = 1;
			// 
			// PostCodeTextBox
			// 
			this.PostCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PostCodeTextBox.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("StandAloneScreeningForm|6961cb1b-3b7c-4326-9207-b71633f320b9", "Postcode");
			this.PostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 84, true);
			this.PostCodeTextBox.Name = "PostCodeTextBox";
			this.PostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.PostCodeTextBox.TabIndex = 8;
			// 
			// CountryFindBox
			// 
			this.CountryFindBox.AllowDrop = true;
			this.CountryFindBox.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("4D980296-C9A6-4113-9936-D18E87B0608D", "Country");
			this.CountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 3, true);
			this.CountryFindBox.Name = "CountryFindBox";
			this.CountryFindBox.PreBoundMaxLength = 3;
			this.CountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.CountryFindBox.TabIndex = 9;
			this.CountryFindBox.CodeBox.MaxLength = 2;
			// 
			// IdNumberTextBox
			// 
			this.IdNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.IdNumberTextBox.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("StandAloneScreeningForm|2c3b973e-3e95-4eb9-afec-2ad002111541", "Id Number");
			this.IdNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 55, true);
			this.IdNumberTextBox.Name = "IdNumberTextBox";
			this.IdNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.IdNumberTextBox.TabIndex = 11;
			// 
			// IdIssuingCountryFindBox
			// 
			this.IdIssuingCountryFindBox.AllowDrop = true;
			this.IdIssuingCountryFindBox.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("75c3714e-0a5e-42e9-ae2a-f937f919ab0d", "Id Issuing Country");
			this.IdIssuingCountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 84, true);
			this.IdIssuingCountryFindBox.Name = "IdIssuingCountryFindBox";
			this.IdIssuingCountryFindBox.PreBoundMaxLength = 3;
			this.IdIssuingCountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.IdIssuingCountryFindBox.TabIndex = 12;
			this.IdIssuingCountryFindBox.CodeBox.MaxLength = 2;
			// 
			// ContactGroupBox
			// 
			this.ContactGroupBox.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("StandAloneScreeningForm|be65ae94-c77e-4788-b4f7-3fa1b23b60e1", "Screening Details");
			this.ContactGroupBox.Controls.Add(this.VesselPanel);
			this.ContactGroupBox.Controls.Add(this.TextboxPanel);
			this.ContactGroupBox.Controls.Add(this.VesselRadioButton);
			this.ContactGroupBox.Controls.Add(this.OrgRadioButton);
			this.ContactGroupBox.Controls.Add(this.PersonRadioButton);
			this.ContactGroupBox.Controls.Add(this.FullNameTextBox);
			this.ContactGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 78, true);
			this.ContactGroupBox.Name = "ContactGroupBox";
			this.ContactGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 182, true);
			this.ContactGroupBox.TabIndex = 1;
			this.ContactGroupBox.TabStop = false;
			// 
			// VesselPanel
			// 
			this.VesselPanel.Controls.Add(this.VesselCallsignTextBox);
			this.VesselPanel.Controls.Add(this.VesselLloydsNumberTextBox);
			this.VesselPanel.Controls.Add(this.VesselCountyOfRegFindBox);
			this.VesselPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 67, true);
			this.VesselPanel.Name = "VesselPanel";
			this.VesselPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 111, true);
			this.VesselPanel.TabIndex = 14;
			this.VesselPanel.Visible = false;
			// 
			// VesselCallsignTextBox
			// 
			this.VesselCallsignTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.VesselCallsignTextBox.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("StandAloneScreeningForm|257d1b41-37da-43ff-8d52-d2107079d05d", "Radio Call-sign");
			this.VesselCallsignTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 29, true);
			this.VesselCallsignTextBox.Name = "VesselCallsignTextBox";
			this.VesselCallsignTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.VesselCallsignTextBox.TabIndex = 6;
			// 
			// VesselLloydsNumberTextBox
			// 
			this.VesselLloydsNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.VesselLloydsNumberTextBox.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("StandAloneScreeningForm|f99604af-8b84-45a3-b871-1b1ee8a74235", "Lloyds Number");
			this.VesselLloydsNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 3, true);
			this.VesselLloydsNumberTextBox.Name = "VesselLloydsNumberTextBox";
			this.VesselLloydsNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.VesselLloydsNumberTextBox.TabIndex = 5;
			// 
			// VesselCountyOfRegFindBox
			// 
			this.VesselCountyOfRegFindBox.AllowDrop = true;
			this.VesselCountyOfRegFindBox.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("775fef73-82ae-43b2-b5e8-9c16ba3a3286", "Country of Registration");
			this.VesselCountyOfRegFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 3, true);
			this.VesselCountyOfRegFindBox.Name = "VesselCountyOfRegFindBox";
			this.VesselCountyOfRegFindBox.PreBoundMaxLength = 3;
			this.VesselCountyOfRegFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.VesselCountyOfRegFindBox.TabIndex = 9;
			this.VesselCountyOfRegFindBox.CodeBox.MaxLength = 2;
			// 
			// TextboxPanel
			// 
			this.TextboxPanel.Controls.Add(this.CityTextBox);
			this.TextboxPanel.Controls.Add(this.Address2TextBox);
			this.TextboxPanel.Controls.Add(this.StateTextBox);
			this.TextboxPanel.Controls.Add(this.Address1TextBox);
			this.TextboxPanel.Controls.Add(this.PostCodeTextBox);
			this.TextboxPanel.Controls.Add(this.CountryFindBox);
			this.TextboxPanel.Controls.Add(this.IdIssuingCountryFindBox);
			this.TextboxPanel.Controls.Add(this.IdNumberTextBox);
			this.TextboxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 67, true);
			this.TextboxPanel.Name = "TextboxPanel";
			this.TextboxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 111, true);
			this.TextboxPanel.TabIndex = 13;
			// 
			// VesselRadioButton
			// 
			this.VesselRadioButton.AutoCheck = false;
			this.VesselRadioButton.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("StandAloneScreeningForm|65449859-c7e7-4334-9f17-590cdfa131ed", "Vessel");
			this.VesselRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.VesselRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 44, true);
			this.VesselRadioButton.Name = "VesselRadioButton";
			this.VesselRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.VesselRadioButton.TabIndex = 4;
			this.VesselRadioButton.TabStop = true;
			this.VesselRadioButton.UseVisualStyleBackColor = true;
			this.VesselRadioButton.CheckedChanged += new System.EventHandler(this.RadioButtons_CheckedChanged);
			// 
			// OrgRadioButton
			// 
			this.OrgRadioButton.AutoCheck = false;
			this.OrgRadioButton.Checked = true;
			this.OrgRadioButton.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("StandAloneScreeningForm|a6ffc3a4-e44c-4b0b-9d24-e26ecc0dc963", "Organization");
			this.OrgRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OrgRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 44, true);
			this.OrgRadioButton.Name = "OrgRadioButton";
			this.OrgRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.OrgRadioButton.TabIndex = 2;
			this.OrgRadioButton.TabStop = true;
			this.OrgRadioButton.UseVisualStyleBackColor = true;
			this.OrgRadioButton.CheckedChanged += new System.EventHandler(this.RadioButtons_CheckedChanged);
			// 
			// PersonRadioButton
			// 
			this.PersonRadioButton.AutoCheck = false;
			this.PersonRadioButton.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("StandAloneScreeningForm|a7aa0883-f6cb-4812-984c-d0466ff28757", "Person");
			this.PersonRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PersonRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 44, true);
			this.PersonRadioButton.Name = "PersonRadioButton";
			this.PersonRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.PersonRadioButton.TabIndex = 3;
			this.PersonRadioButton.TabStop = true;
			this.PersonRadioButton.UseVisualStyleBackColor = true;
			this.PersonRadioButton.CheckedChanged += new System.EventHandler(this.RadioButtons_CheckedChanged);
			// 
			// StandAloneScreeningForm
			// 
			this.AcceptButton = this.ScreenButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(702, 426, true);
			this.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("StandAloneScreeningForm|13c8efab-98b9-4dea-89bb-863f12c29362", "Denied Party Screening");
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.InstructionsLabel);
			this.Controls.Add(this.ScreenButton);
			this.Controls.Add(this.ContactGroupBox);
			this.Controls.Add(this.OtherInfoGroupBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "StandAloneScreeningForm";
			this.Controls.SetChildIndex(this.OtherInfoGroupBox, 0);
			this.Controls.SetChildIndex(this.ContactGroupBox, 0);
			this.Controls.SetChildIndex(this.ScreenButton, 0);
			this.Controls.SetChildIndex(this.InstructionsLabel, 0);
			this.Controls.SetChildIndex(this.pictureBox1, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.OtherInfoGroupBox.ResumeLayout(false);
			this.OtherInfoGroupBox.PerformLayout();
			this.ContactGroupBox.ResumeLayout(false);
			this.ContactGroupBox.PerformLayout();
			this.VesselPanel.ResumeLayout(false);
			this.VesselPanel.PerformLayout();
			this.TextboxPanel.ResumeLayout(false);
			this.TextboxPanel.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton ScreenButton;
		private Enterprise.ZArchitecture.ZLabel InstructionsLabel;
		private Enterprise.ZArchitecture.GUI.ZPictureBox pictureBox1;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.ZTextBox OtherInfoTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox OtherInfoGroupBox;
		private Enterprise.ZArchitecture.ZLabel zLabel2;
		private Enterprise.ZArchitecture.ZTextBox CityTextBox;
		internal  Enterprise.ZArchitecture.ZTextBox Address2TextBox;
		internal Enterprise.ZArchitecture.ZTextBox StateTextBox;
		internal  Enterprise.ZArchitecture.ZTextBox Address1TextBox;
		internal Enterprise.ZArchitecture.ZTextBox FullNameTextBox;
		internal Enterprise.ZArchitecture.ZTextBox PostCodeTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox CountryFindBox;
		protected Enterprise.ZArchitecture.ZTextBox IdNumberTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox IdIssuingCountryFindBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ContactGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZRadioButton VesselRadioButton;
		internal Enterprise.ZArchitecture.GUI.ZRadioButton OrgRadioButton;
		internal Enterprise.ZArchitecture.GUI.ZRadioButton PersonRadioButton;
		private Enterprise.ZArchitecture.GUI.ZPanel TextboxPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel VesselPanel;
		private Enterprise.ZArchitecture.ZTextBox VesselCallsignTextBox;
		private Enterprise.ZArchitecture.ZTextBox VesselLloydsNumberTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox VesselCountyOfRegFindBox;
	}
}

namespace Enterprise.MasterData.GUI
{
	partial class PersonFilterContentControl
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
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.FindButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ClearButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DropEditActiveStatus = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PanelCheckBoxList = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CheckBoxLow = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CheckBoxMedium = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CheckBoxHigh = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zPanel3 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TextBoxName = new Enterprise.ZArchitecture.ZTextBox();
			this.DropEditName = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zPanel4 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TextBoxPhone = new Enterprise.ZArchitecture.ZTextBox();
			this.DropEditPhone = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.TextBoxEmail = new Enterprise.ZArchitecture.ZTextBox();
			this.PanelEmail = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DropEditEmail = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DropEditActiveStatus.SuspendLayout();
			this.PanelCheckBoxList.SuspendLayout();
			this.zPanel3.SuspendLayout();
			this.DropEditName.SuspendLayout();
			this.zPanel4.SuspendLayout();
			this.DropEditPhone.SuspendLayout();
			this.PanelEmail.SuspendLayout();
			this.DropEditEmail.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterData.GUI.PersonFilterDataSource);
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("be93735b-02ed-471b-9614-07ae1517fcad", "Active Status");
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 4, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.zLabel1.TabIndex = 0;
			this.zLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.zLabel1.UseMnemonic = false;
			// 
			// zLabel2
			// 
			this.zLabel2.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("3540b532-970b-49f1-99a9-82baf9f524ee", "Confidence");
			this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 31, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.zLabel2.TabIndex = 1;
			this.zLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.zLabel2.UseMnemonic = false;
			// 
			// zLabel4
			// 
			this.zLabel4.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("90b01ea8-b242-4042-9361-db52419c5e89", "Name");
			this.zLabel4.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 84, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.zLabel4.TabIndex = 3;
			this.zLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.zLabel4.UseMnemonic = false;
			// 
			// zLabel5
			// 
			this.zLabel5.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("a5a18979-26a6-45a6-81c1-eb8c93feafc8", "Phone");
			this.zLabel5.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 110, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.zLabel5.TabIndex = 4;
			this.zLabel5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.zLabel5.UseMnemonic = false;
			// 
			// ButtonFind
			// 
			this.FindButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("5f9a1300-1454-4ddd-a382-cea150d410b9", "Find");
			this.FindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 132, true);
			this.FindButton.Name = "FindButton";
			this.FindButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 23, true);
			this.FindButton.TabIndex = 5;
			this.FindButton.ToolTipCaption = null;
			this.FindButton.UseVisualStyleBackColor = true;
			// 
			// ButtonClear
			// 
			this.ClearButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("af9a1300-1454-4ddd-a382-cea150d410b1", "Clear");
			this.ClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(394, 132, true);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 23, true);
			this.ClearButton.TabIndex = 6;
			this.ClearButton.ToolTipCaption = null;
			this.ClearButton.UseVisualStyleBackColor = true;
			// 
			// DropEditActiveStatus
			// 
			this.DropEditActiveStatus.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DropEditActiveStatus, "PersonFilterActiveDescription");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterData.GUI.PersonFilterDataSource)(null)).PersonFilterActiveDescription)));
			this.DropEditActiveStatus.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DropEditActiveStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 4, true);
			this.DropEditActiveStatus.Name = "DropEditActiveStatus";
			this.DropEditActiveStatus.ShowDescriptionBox = false;
			this.DropEditActiveStatus.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			this.DropEditActiveStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 28, true);
			this.DropEditActiveStatus.TabIndex = 7;
			// 
			// PanelCheckBoxList
			// 
			this.PanelCheckBoxList.AutoSize = true;
			this.PanelCheckBoxList.Controls.Add(this.CheckBoxLow);
			this.PanelCheckBoxList.Controls.Add(this.CheckBoxMedium);
			this.PanelCheckBoxList.Controls.Add(this.CheckBoxHigh);
			this.PanelCheckBoxList.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 33, true);
			this.PanelCheckBoxList.Name = "PanelCheckBoxList";
			this.PanelCheckBoxList.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 18, true);
			this.PanelCheckBoxList.TabIndex = 8;
			// 
			// CheckBoxLow
			// 
			this.CheckBoxLow.Text = TextConstant.ConfidenceLabelLow;
			this.CheckBoxLow.Dock = System.Windows.Forms.DockStyle.Left;
			this.CheckBoxLow.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 0, true);
			this.CheckBoxLow.Name = "CheckBoxLow";
			this.CheckBoxLow.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(16, 0, 0, 0, true);
			this.CheckBoxLow.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 18, true);
			this.CheckBoxLow.TabIndex = 2;
			this.CheckBoxLow.UseVisualStyleBackColor = true;
			// 
			// CheckBoxMedium
			// 
			this.CheckBoxMedium.Text = TextConstant.ConfidenceLabelMedium;
			this.CheckBoxMedium.Dock = System.Windows.Forms.DockStyle.Left;
			this.CheckBoxMedium.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(46, 0, true);
			this.CheckBoxMedium.Name = "CheckBoxMedium";
			this.CheckBoxMedium.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(16, 0, 0, 0, true);
			this.CheckBoxMedium.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 18, true);
			this.CheckBoxMedium.TabIndex = 1;
			this.CheckBoxMedium.UseVisualStyleBackColor = true;
			// 
			// CheckBoxHigh
			// 
			this.CheckBoxHigh.Text = TextConstant.ConfidenceLabelHigh;
			this.CheckBoxHigh.Dock = System.Windows.Forms.DockStyle.Left;
			this.CheckBoxHigh.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CheckBoxHigh.Name = "CheckBoxHigh";
			this.CheckBoxHigh.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 18, true);
			this.CheckBoxHigh.TabIndex = 0;
			this.CheckBoxHigh.UseVisualStyleBackColor = true;
			// 
			// zPanel3
			// 
			this.zPanel3.Controls.Add(this.TextBoxName);
			this.zPanel3.Controls.Add(this.DropEditName);
			this.zPanel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 83, true);
			this.zPanel3.Name = "zPanel3";
			this.zPanel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 20, true);
			this.zPanel3.TabIndex = 10;
			// 
			// TextBoxName
			// 
			this.BindingSource.SetBindingMember(this.TextBoxName, "PersonFilterNameKeyword");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.GUI.PersonFilterDataSource)(null)).PersonFilterNameKeyword)));
			this.TextBoxName.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TextBoxName.Dock = System.Windows.Forms.DockStyle.Right;
			this.TextBoxName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 0, true);
			this.TextBoxName.MaxLength = 254;
			this.TextBoxName.Name = "TextBoxName";
			this.TextBoxName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 28, true);
			this.TextBoxName.TabIndex = 1;
			// 
			// DropEditName
			// 
			this.DropEditName.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DropEditName, "PersonFilterNameDescription");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterData.GUI.PersonFilterDataSource)(null)).PersonFilterNameDescription)));
			this.DropEditName.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DropEditName.Dock = System.Windows.Forms.DockStyle.Left;
			this.DropEditName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DropEditName.Name = "DropEditName";
			this.DropEditName.ShowDescriptionBox = false;
			this.DropEditName.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			this.DropEditName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 28, true);
			this.DropEditName.TabIndex = 0;
			// 
			// zPanel4
			// 
			this.zPanel4.Controls.Add(this.TextBoxPhone);
			this.zPanel4.Controls.Add(this.DropEditPhone);
			this.zPanel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 109, true);
			this.zPanel4.Name = "zPanel4";
			this.zPanel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 20, true);
			this.zPanel4.TabIndex = 11;
			// 
			// TextBoxPhone
			// 
			this.BindingSource.SetBindingMember(this.TextBoxPhone, "PersonFilterPhoneKeyword");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.GUI.PersonFilterDataSource)(null)).PersonFilterPhoneKeyword)));
			this.TextBoxPhone.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TextBoxPhone.Dock = System.Windows.Forms.DockStyle.Right;
			this.TextBoxPhone.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 0, true);
			this.TextBoxPhone.MaxLength = 254;
			this.TextBoxPhone.Name = "TextBoxPhone";
			this.TextBoxPhone.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 28, true);
			this.TextBoxPhone.TabIndex = 1;
			// 
			// DropEditPhone
			// 
			this.DropEditPhone.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DropEditPhone, "PersonFilterPhoneDescription");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterData.GUI.PersonFilterDataSource)(null)).PersonFilterPhoneDescription)));
			this.DropEditPhone.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DropEditPhone.Dock = System.Windows.Forms.DockStyle.Left;
			this.DropEditPhone.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DropEditPhone.Name = "DropEditPhone";
			this.DropEditPhone.ShowDescriptionBox = false;
			this.DropEditPhone.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			this.DropEditPhone.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 28, true);
			this.DropEditPhone.TabIndex = 0;
			// 
			// zLabel3
			// 
			this.zLabel3.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("ad643ab7-1a5e-4c63-ab15-fae0185845a9", "Email");
			this.zLabel3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 58, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.zLabel3.TabIndex = 2;
			this.zLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.zLabel3.UseMnemonic = false;
			// 
			// TextBoxEmail
			// 
			this.BindingSource.SetBindingMember(this.TextBoxEmail, "PersonFilterEmailKeyword");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.GUI.PersonFilterDataSource)(null)).PersonFilterEmailKeyword)));
			this.TextBoxEmail.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TextBoxEmail.Dock = System.Windows.Forms.DockStyle.Right;
			this.TextBoxEmail.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 0, true);
			this.TextBoxEmail.MaxLength = 254;
			this.TextBoxEmail.Name = "TextBoxEmail";
			this.TextBoxEmail.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 28, true);
			this.TextBoxEmail.TabIndex = 1;
			// 
			// PanelEmail
			// 
			this.PanelEmail.Controls.Add(this.TextBoxEmail);
			this.PanelEmail.Controls.Add(this.DropEditEmail);
			this.PanelEmail.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 58, true);
			this.PanelEmail.Name = "PanelEmail";
			this.PanelEmail.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 20, true);
			this.PanelEmail.TabIndex = 9;
			// 
			// DropEditEmail
			// 
			this.DropEditEmail.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DropEditEmail, "PersonFilterEmailDescription");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterData.GUI.PersonFilterDataSource)(null)).PersonFilterEmailDescription)));
			this.DropEditEmail.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DropEditEmail.Dock = System.Windows.Forms.DockStyle.Left;
			this.DropEditEmail.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DropEditEmail.Name = "DropEditEmail";
			this.DropEditEmail.ShowDescriptionBox = false;
			this.DropEditEmail.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			this.DropEditEmail.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 28, true);
			this.DropEditEmail.TabIndex = 0;
			// 
			// PersonFilterContentControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zPanel4);
			this.Controls.Add(this.zPanel3);
			this.Controls.Add(this.PanelEmail);
			this.Controls.Add(this.PanelCheckBoxList);
			this.Controls.Add(this.DropEditActiveStatus);
			this.Controls.Add(this.ClearButton);
			this.Controls.Add(this.FindButton);
			this.Controls.Add(this.zLabel5);
			this.Controls.Add(this.zLabel4);
			this.Controls.Add(this.zLabel3);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.zLabel1);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 160, true);
			this.Name = "PersonFilterContentControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(611, 160, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DropEditActiveStatus.ResumeLayout(true);
			this.DropEditActiveStatus.PerformLayout();
			this.PanelCheckBoxList.ResumeLayout(false);
			this.PanelCheckBoxList.PerformLayout();
			this.zPanel3.ResumeLayout(false);
			this.zPanel3.PerformLayout();
			this.DropEditName.ResumeLayout(true);
			this.DropEditName.PerformLayout();
			this.zPanel4.ResumeLayout(false);
			this.zPanel4.PerformLayout();
			this.DropEditPhone.ResumeLayout(true);
			this.DropEditPhone.PerformLayout();
			this.PanelEmail.ResumeLayout(false);
			this.PanelEmail.PerformLayout();
			this.DropEditEmail.ResumeLayout(true);
			this.DropEditEmail.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.ZLabel zLabel4;
		private ZArchitecture.ZLabel zLabel5;
		private ZArchitecture.GUI.ZButton FindButton;
		private ZArchitecture.GUI.ZButton ClearButton;
		private ZArchitecture.GUI.ZDropEdit DropEditActiveStatus;
		private ZArchitecture.GUI.ZPanel PanelCheckBoxList;
		private ZArchitecture.GUI.ZCheckBox CheckBoxLow;
		private ZArchitecture.GUI.ZCheckBox CheckBoxMedium;
		private ZArchitecture.GUI.ZCheckBox CheckBoxHigh;
		private ZArchitecture.GUI.ZPanel zPanel3;
		private ZArchitecture.ZTextBox TextBoxName;
		private ZArchitecture.GUI.ZDropEdit DropEditName;
		private ZArchitecture.GUI.ZPanel zPanel4;
		private ZArchitecture.ZTextBox TextBoxPhone;
		private ZArchitecture.GUI.ZDropEdit DropEditPhone;
		private ZArchitecture.ZLabel zLabel3;
		private ZArchitecture.ZTextBox TextBoxEmail;
		private ZArchitecture.GUI.ZPanel PanelEmail;
		private ZArchitecture.GUI.ZDropEdit DropEditEmail;
	}
}

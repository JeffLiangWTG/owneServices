using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.V4.GUI
{
	partial class SGStaffCredentialsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.VisibleOnlyToDeveloperTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NTPVisibleOnlyToDeveloperTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NTPGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NTPPasswordStatusDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.NTPUserIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NTPNextPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NTPCurrentPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NTPDeclarantCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Tradenetv4GroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.Tradenetv4PasswordGP_PasswordStatusDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.Tradenetv4PasswordGP_PasswordStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.Tradenetv4PasswordUserIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Tradenetv4PasswordNextDecryptedPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Tradenetv4PasswordCurrentDecryptedPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Tradenetv4PasswordMailBoxIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AccessVisibleOnlyToDeveloperTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AccessGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AccessPasswordGP_PasswordStatusDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AccessPasswordStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AccessPasswordUserIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AccessPasswordNextDecryptedPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AccessPasswordCurrentDecryptedPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NTPGroupBox.SuspendLayout();
			this.Tradenetv4GroupBox.SuspendLayout();
			this.AccessGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.SG.V4.Business.SGGlbStaffWrapper);
			// 
			// VisibleOnlyToDeveloperTextBox
			// 
			this.VisibleOnlyToDeveloperTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.VisibleOnlyToDeveloperTextBox, "Tradenetv4Password.CurrentDecryptedPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.SGGlbStaffWrapper)(null)).Tradenetv4Password.CurrentDecryptedPassword)));
			this.VisibleOnlyToDeveloperTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("GlbStaffForm|1f1e530e-c044-41f6-9485-c12ac60226a0", "Current Password");
			this.VisibleOnlyToDeveloperTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.VisibleOnlyToDeveloperTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 179, true);
			this.VisibleOnlyToDeveloperTextBox.Name = "VisibleOnlyToDeveloperTextBox";
			this.VisibleOnlyToDeveloperTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.VisibleOnlyToDeveloperTextBox.TabIndex = 2;
			this.VisibleOnlyToDeveloperTextBox.Text = "Actual Password Visible to Developer Only";
			this.VisibleOnlyToDeveloperTextBox.Visible = false;
			// 
			// NTPVisibleOnlyToDeveloperTextBox
			// 
			this.NTPVisibleOnlyToDeveloperTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.NTPVisibleOnlyToDeveloperTextBox, "SGNationalTradePlatformPassword.CurrentDecryptedPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.SGGlbStaffWrapper)(null)).SGNationalTradePlatformPassword.CurrentDecryptedPassword)));
			this.NTPVisibleOnlyToDeveloperTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NTPVisibleOnlyToDeveloperTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 365, true);
			this.NTPVisibleOnlyToDeveloperTextBox.Name = "NTPVisibleOnlyToDeveloperTextBox";
			this.NTPVisibleOnlyToDeveloperTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.NTPVisibleOnlyToDeveloperTextBox.TabIndex = 6;
			this.NTPVisibleOnlyToDeveloperTextBox.Text = "Actual Password Visible to Developer Only";
			this.NTPVisibleOnlyToDeveloperTextBox.Visible = false;
			// 
			// NTPGroupBox
			// 
			this.NTPGroupBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("9f7fdb8a-65ae-41a9-92a3-17e6f6ddbb02", "NTP Identification");
			this.NTPGroupBox.Controls.Add(this.NTPPasswordStatusDescriptionLabel);
			this.NTPGroupBox.Controls.Add(this.zLabel2);
			this.NTPGroupBox.Controls.Add(this.NTPUserIDTextBox);
			this.NTPGroupBox.Controls.Add(this.NTPNextPasswordTextBox);
			this.NTPGroupBox.Controls.Add(this.NTPCurrentPasswordTextBox);
			this.NTPGroupBox.Controls.Add(this.NTPDeclarantCodeTextBox);
			this.NTPGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 211, true);
			this.NTPGroupBox.Name = "NTPGroupBox";
			this.NTPGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 151, true);
			this.NTPGroupBox.TabIndex = 5;
			this.NTPGroupBox.TabStop = false;
			// 
			// NTPPasswordStatusDescriptionLabel
			// 
			this.BindingSource.SetBindingMember(this.NTPPasswordStatusDescriptionLabel, "SGNationalTradePlatformPassword.GP_PasswordStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.SGGlbStaffWrapper)(null)).SGNationalTradePlatformPassword.GP_PasswordStatusDescription)));
			this.NTPPasswordStatusDescriptionLabel.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("c743490d-11e0-4b8d-abae-c531fd61e1a7", "Password Status Description");
			this.NTPPasswordStatusDescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.NTPPasswordStatusDescriptionLabel.IsFontBold = true;
			this.NTPPasswordStatusDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 122, true);
			this.NTPPasswordStatusDescriptionLabel.Name = "NTPPasswordStatusDescriptionLabel";
			this.NTPPasswordStatusDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.NTPPasswordStatusDescriptionLabel.TabIndex = 5;
			// 
			// zLabel2
			// 
			this.zLabel2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("22b1e038-e94c-459c-b699-4b8e1d6cc213", "Password Status:");
			this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 126, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 13, true);
			this.zLabel2.TabIndex = 4;
			// 
			// NTPUserIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.NTPUserIDTextBox, "SGNationalTradePlatformPassword.GP_UserID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.SGGlbStaffWrapper)(null)).SGNationalTradePlatformPassword.GP_UserID)));
			this.NTPUserIDTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("9599127a-0b86-4138-ab0e-816608476261", "NTP User ID");
			this.NTPUserIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NTPUserIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 24, true);
			this.NTPUserIDTextBox.Name = "NTPUserIDTextBox";
			this.NTPUserIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.NTPUserIDTextBox.TabIndex = 0;
			// 
			// NTPNextPasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.NTPNextPasswordTextBox, "SGNationalTradePlatformPassword.NextDecryptedPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.SGGlbStaffWrapper)(null)).SGNationalTradePlatformPassword.NextDecryptedPassword)));
			this.NTPNextPasswordTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("06e3a4e7-46dc-4bee-ad34-f6c211816846", "Next Password");
			this.NTPNextPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NTPNextPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 96, true);
			this.NTPNextPasswordTextBox.Name = "NTPNextPasswordTextBox";
			this.NTPNextPasswordTextBox.PasswordChar = '*';
			this.NTPNextPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.NTPNextPasswordTextBox.TabIndex = 3;
			// 
			// NTPCurrentPasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.NTPCurrentPasswordTextBox, "SGNationalTradePlatformPassword.CurrentDecryptedPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.SGGlbStaffWrapper)(null)).SGNationalTradePlatformPassword.CurrentDecryptedPassword)));
			this.NTPCurrentPasswordTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("981262ca-9807-4727-a5c4-2d760d02325b", "Current Password");
			this.NTPCurrentPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NTPCurrentPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 72, true);
			this.NTPCurrentPasswordTextBox.Name = "NTPCurrentPasswordTextBox";
			this.NTPCurrentPasswordTextBox.PasswordChar = '*';
			this.NTPCurrentPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.NTPCurrentPasswordTextBox.TabIndex = 2;
			// 
			// NTPDeclarantCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.NTPDeclarantCodeTextBox, "SGNationalTradePlatformPassword.GP_MailBoxID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.SGGlbStaffWrapper)(null)).SGNationalTradePlatformPassword.GP_MailBoxID)));
			this.NTPDeclarantCodeTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("3a24bb83-659a-4315-84c4-9001afb3363f", "Declarant Code");
			this.NTPDeclarantCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 48, true);
			this.NTPDeclarantCodeTextBox.Name = "NTPDeclarantCodeTextBox";
			this.NTPDeclarantCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.NTPDeclarantCodeTextBox.TabIndex = 1;
			// 
			// Tradenetv4GroupBox
			// 
			this.Tradenetv4GroupBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("GlbStaffForm|9fd316ba-3b54-4455-8cce-4934ece3e1d8", "Declarant Identification V4");
			this.Tradenetv4GroupBox.Controls.Add(this.Tradenetv4PasswordGP_PasswordStatusDescriptionLabel);
			this.Tradenetv4GroupBox.Controls.Add(this.Tradenetv4PasswordGP_PasswordStatusLabel);
			this.Tradenetv4GroupBox.Controls.Add(this.Tradenetv4PasswordUserIDTextBox);
			this.Tradenetv4GroupBox.Controls.Add(this.Tradenetv4PasswordNextDecryptedPasswordTextBox);
			this.Tradenetv4GroupBox.Controls.Add(this.Tradenetv4PasswordCurrentDecryptedPasswordTextBox);
			this.Tradenetv4GroupBox.Controls.Add(this.Tradenetv4PasswordMailBoxIDTextBox);
			this.Tradenetv4GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 25, true);
			this.Tradenetv4GroupBox.Name = "Tradenetv4GroupBox";
			this.Tradenetv4GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 151, true);
			this.Tradenetv4GroupBox.TabIndex = 0;
			this.Tradenetv4GroupBox.TabStop = false;
			// 
			// Tradenetv4PasswordGP_PasswordStatusDescriptionLabel
			// 
			this.BindingSource.SetBindingMember(this.Tradenetv4PasswordGP_PasswordStatusDescriptionLabel, "Tradenetv4Password.GP_PasswordStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.SGGlbStaffWrapper)(null)).Tradenetv4Password.GP_PasswordStatusDescription)));
			this.Tradenetv4PasswordGP_PasswordStatusDescriptionLabel.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("GlbStaffForm|a50553a6-7fbd-49cc-8938-48058644e9d4", "Password Status Description");
			this.Tradenetv4PasswordGP_PasswordStatusDescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.Tradenetv4PasswordGP_PasswordStatusDescriptionLabel.IsFontBold = true;
			this.Tradenetv4PasswordGP_PasswordStatusDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 123, true);
			this.Tradenetv4PasswordGP_PasswordStatusDescriptionLabel.Name = "Tradenetv4PasswordGP_PasswordStatusDescriptionLabel";
			this.Tradenetv4PasswordGP_PasswordStatusDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.Tradenetv4PasswordGP_PasswordStatusDescriptionLabel.TabIndex = 5;
			// 
			// Tradenetv4PasswordGP_PasswordStatusLabel
			// 
			this.Tradenetv4PasswordGP_PasswordStatusLabel.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("GlbStaffForm|141c92ff-94f2-449f-8b31-55aec5342d79", "Password Status:");
			this.Tradenetv4PasswordGP_PasswordStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.Tradenetv4PasswordGP_PasswordStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 127, true);
			this.Tradenetv4PasswordGP_PasswordStatusLabel.Name = "Tradenetv4PasswordGP_PasswordStatusLabel";
			this.Tradenetv4PasswordGP_PasswordStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 13, true);
			this.Tradenetv4PasswordGP_PasswordStatusLabel.TabIndex = 4;
			// 
			// Tradenetv4PasswordUserIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.Tradenetv4PasswordUserIDTextBox, "Tradenetv4Password.GP_UserID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.SGGlbStaffWrapper)(null)).Tradenetv4Password.GP_UserID)));
			this.Tradenetv4PasswordUserIDTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("GlbStaffForm|6fd16881-52d1-4b1e-8fc1-89a208ccb44b", "TradeNet User ID");
			this.Tradenetv4PasswordUserIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Tradenetv4PasswordUserIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 24, true);
			this.Tradenetv4PasswordUserIDTextBox.Name = "Tradenetv4PasswordUserIDTextBox";
			this.Tradenetv4PasswordUserIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.Tradenetv4PasswordUserIDTextBox.TabIndex = 0;
			// 
			// Tradenetv4PasswordNextDecryptedPasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.Tradenetv4PasswordNextDecryptedPasswordTextBox, "Tradenetv4Password.NextDecryptedPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.SGGlbStaffWrapper)(null)).Tradenetv4Password.NextDecryptedPassword)));
			this.Tradenetv4PasswordNextDecryptedPasswordTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("GlbStaffForm|503DF254-CF34-4A58-BD91-6711CEE4A59F", "Next Password");
			this.Tradenetv4PasswordNextDecryptedPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Tradenetv4PasswordNextDecryptedPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 96, true);
			this.Tradenetv4PasswordNextDecryptedPasswordTextBox.Name = "Tradenetv4PasswordNextDecryptedPasswordTextBox";
			this.Tradenetv4PasswordNextDecryptedPasswordTextBox.PasswordChar = '*';
			this.Tradenetv4PasswordNextDecryptedPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.Tradenetv4PasswordNextDecryptedPasswordTextBox.TabIndex = 3;
			// 
			// Tradenetv4PasswordCurrentDecryptedPasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.Tradenetv4PasswordCurrentDecryptedPasswordTextBox, "Tradenetv4Password.CurrentDecryptedPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.SGGlbStaffWrapper)(null)).Tradenetv4Password.CurrentDecryptedPassword)));
			this.Tradenetv4PasswordCurrentDecryptedPasswordTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("GlbStaffForm|7dfec23f-4e06-4f2f-92ef-af23633abf3e", "Current Password");
			this.Tradenetv4PasswordCurrentDecryptedPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Tradenetv4PasswordCurrentDecryptedPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 72, true);
			this.Tradenetv4PasswordCurrentDecryptedPasswordTextBox.Name = "Tradenetv4PasswordCurrentDecryptedPasswordTextBox";
			this.Tradenetv4PasswordCurrentDecryptedPasswordTextBox.PasswordChar = '*';
			this.Tradenetv4PasswordCurrentDecryptedPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.Tradenetv4PasswordCurrentDecryptedPasswordTextBox.TabIndex = 2;
			// 
			// Tradenetv4PasswordMailBoxIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.Tradenetv4PasswordMailBoxIDTextBox, "Tradenetv4Password.GP_MailBoxID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.SGGlbStaffWrapper)(null)).Tradenetv4Password.GP_MailBoxID)));
			this.Tradenetv4PasswordMailBoxIDTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("GlbStaffForm|c4a3dca4-35a3-4b60-b008-19502da88ca9", "Declarant Code");
			this.Tradenetv4PasswordMailBoxIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 48, true);
			this.Tradenetv4PasswordMailBoxIDTextBox.Name = "Tradenetv4PasswordMailBoxIDTextBox";
			this.Tradenetv4PasswordMailBoxIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.Tradenetv4PasswordMailBoxIDTextBox.TabIndex = 1;
			// 
			// AccessVisibleOnlyToDeveloperTextBox
			// 
			this.AccessVisibleOnlyToDeveloperTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AccessVisibleOnlyToDeveloperTextBox, "AccessPassword.CurrentDecryptedPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.SGGlbStaffWrapper)(null)).AccessPassword.CurrentDecryptedPassword)));
			this.AccessVisibleOnlyToDeveloperTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AccessVisibleOnlyToDeveloperTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(429, 179, true);
			this.AccessVisibleOnlyToDeveloperTextBox.Name = "AccessVisibleOnlyToDeveloperTextBox";
			this.AccessVisibleOnlyToDeveloperTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.AccessVisibleOnlyToDeveloperTextBox.TabIndex = 4;
			this.AccessVisibleOnlyToDeveloperTextBox.Text = "Actual Password Visible to Developer Only";
			this.AccessVisibleOnlyToDeveloperTextBox.Visible = false;
			// 
			// AccessGroupBox
			// 
			this.AccessGroupBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("GlbStaffForm|5be9f717-d973-4d95-97c4-0360d7f3f2f6", "ACCESS Identification");
			this.AccessGroupBox.Controls.Add(this.AccessPasswordGP_PasswordStatusDescriptionLabel);
			this.AccessGroupBox.Controls.Add(this.AccessPasswordStatusLabel);
			this.AccessGroupBox.Controls.Add(this.AccessPasswordUserIDTextBox);
			this.AccessGroupBox.Controls.Add(this.AccessPasswordNextDecryptedPasswordTextBox);
			this.AccessGroupBox.Controls.Add(this.AccessPasswordCurrentDecryptedPasswordTextBox);
			this.AccessGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(429, 25, true);
			this.AccessGroupBox.Name = "AccessGroupBox";
			this.AccessGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 151, true);
			this.AccessGroupBox.TabIndex = 3;
			this.AccessGroupBox.TabStop = false;
			// 
			// AccessPasswordGP_PasswordStatusDescriptionLabel
			// 
			this.BindingSource.SetBindingMember(this.AccessPasswordGP_PasswordStatusDescriptionLabel, "AccessPassword.GP_PasswordStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.SGGlbStaffWrapper)(null)).AccessPassword.GP_PasswordStatusDescription)));
			this.AccessPasswordGP_PasswordStatusDescriptionLabel.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("GlbStaffForm|c49be880-5f04-4c94-8899-07882adc7031", "Password Status Description");
			this.AccessPasswordGP_PasswordStatusDescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.AccessPasswordGP_PasswordStatusDescriptionLabel.IsFontBold = true;
			this.AccessPasswordGP_PasswordStatusDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 96, true);
			this.AccessPasswordGP_PasswordStatusDescriptionLabel.Name = "AccessPasswordGP_PasswordStatusDescriptionLabel";
			this.AccessPasswordGP_PasswordStatusDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.AccessPasswordGP_PasswordStatusDescriptionLabel.TabIndex = 4;
			// 
			// AccessPasswordStatusLabel
			// 
			this.AccessPasswordStatusLabel.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("GlbStaffForm|150cd279-4ca0-49e4-b502-b2b68163080e", "Password Status:");
			this.AccessPasswordStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AccessPasswordStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 100, true);
			this.AccessPasswordStatusLabel.Name = "AccessPasswordStatusLabel";
			this.AccessPasswordStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 13, true);
			this.AccessPasswordStatusLabel.TabIndex = 3;
			this.AccessPasswordStatusLabel.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("22b1e038-e94c-459c-b699-4b8e1d6cc213", "Password Status:");
			// 
			// AccessPasswordUserIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.AccessPasswordUserIDTextBox, "AccessPassword.GP_UserID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.SGGlbStaffWrapper)(null)).AccessPassword.GP_UserID)));
			this.AccessPasswordUserIDTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("GlbStaffForm|af54c3d1-d28f-4a59-97e7-fdb84bc9f46f", "ACCESS User ID");
			this.AccessPasswordUserIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AccessPasswordUserIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 24, true);
			this.AccessPasswordUserIDTextBox.Name = "AccessPasswordUserIDTextBox";
			this.AccessPasswordUserIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.AccessPasswordUserIDTextBox.TabIndex = 0;
			// 
			// AccessPasswordNextDecryptedPasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.AccessPasswordNextDecryptedPasswordTextBox, "AccessPassword.NextDecryptedPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.SGGlbStaffWrapper)(null)).AccessPassword.NextDecryptedPassword)));
			this.AccessPasswordNextDecryptedPasswordTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("GlbStaffForm|d4c8ef7d-6e39-43b2-ba91-aeb7c9c8361a", "Next Password");
			this.AccessPasswordNextDecryptedPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AccessPasswordNextDecryptedPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 72, true);
			this.AccessPasswordNextDecryptedPasswordTextBox.Name = "AccessPasswordNextDecryptedPasswordTextBox";
			this.AccessPasswordNextDecryptedPasswordTextBox.PasswordChar = '*';
			this.AccessPasswordNextDecryptedPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.AccessPasswordNextDecryptedPasswordTextBox.TabIndex = 2;
			// 
			// AccessPasswordCurrentDecryptedPasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.AccessPasswordCurrentDecryptedPasswordTextBox, "AccessPassword.CurrentDecryptedPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.SGGlbStaffWrapper)(null)).AccessPassword.CurrentDecryptedPassword)));
			this.AccessPasswordCurrentDecryptedPasswordTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("GlbStaffForm|78f01689-5c6b-421b-a55d-66d9703930e7", "Current Password");
			this.AccessPasswordCurrentDecryptedPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AccessPasswordCurrentDecryptedPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 48, true);
			this.AccessPasswordCurrentDecryptedPasswordTextBox.Name = "AccessPasswordCurrentDecryptedPasswordTextBox";
			this.AccessPasswordCurrentDecryptedPasswordTextBox.PasswordChar = '*';
			this.AccessPasswordCurrentDecryptedPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.AccessPasswordCurrentDecryptedPasswordTextBox.TabIndex = 1;
			// 
			// SGStaffCredentialsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.AccessVisibleOnlyToDeveloperTextBox);
			this.Controls.Add(this.AccessGroupBox);
			this.Controls.Add(this.VisibleOnlyToDeveloperTextBox);
			this.Controls.Add(this.NTPGroupBox);
			this.Controls.Add(this.NTPVisibleOnlyToDeveloperTextBox);
			this.Controls.Add(this.Tradenetv4GroupBox);
			this.Name = "SGStaffCredentialsUserControl";
			this.Controls.SetChildIndex(this.CredentialsHintLabel, 0);
			this.Controls.SetChildIndex(this.Tradenetv4GroupBox, 0);
			this.Controls.SetChildIndex(this.NTPVisibleOnlyToDeveloperTextBox, 0);
			this.Controls.SetChildIndex(this.NTPGroupBox, 0);
			this.Controls.SetChildIndex(this.VisibleOnlyToDeveloperTextBox, 0);
			this.Controls.SetChildIndex(this.AccessGroupBox, 0);
			this.Controls.SetChildIndex(this.AccessVisibleOnlyToDeveloperTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.NTPGroupBox.ResumeLayout(false);
			this.NTPGroupBox.PerformLayout();
			this.Tradenetv4GroupBox.ResumeLayout(false);
			this.Tradenetv4GroupBox.PerformLayout();
			this.AccessGroupBox.ResumeLayout(false);
			this.AccessGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZTextBox AccessVisibleOnlyToDeveloperTextBox;
		public ZGroupBox AccessGroupBox;
		private ZLabel AccessPasswordGP_PasswordStatusDescriptionLabel;
		private ZLabel AccessPasswordStatusLabel;
		private ZTextBox AccessPasswordUserIDTextBox;
		private ZTextBox AccessPasswordNextDecryptedPasswordTextBox;
		private ZTextBox AccessPasswordCurrentDecryptedPasswordTextBox;
		internal ZTextBox NTPVisibleOnlyToDeveloperTextBox;
		public ZGroupBox NTPGroupBox;
		private ZLabel NTPPasswordStatusDescriptionLabel;
		private ZTextBox NTPUserIDTextBox;
		private ZTextBox NTPNextPasswordTextBox;
		private ZTextBox NTPCurrentPasswordTextBox;
		private ZTextBox NTPDeclarantCodeTextBox;
		private ZLabel zLabel2;
		public ZGroupBox Tradenetv4GroupBox;
		private ZTextBox Tradenetv4PasswordUserIDTextBox;
		private ZTextBox Tradenetv4PasswordNextDecryptedPasswordTextBox;
		private ZTextBox Tradenetv4PasswordCurrentDecryptedPasswordTextBox;
		private ZTextBox Tradenetv4PasswordMailBoxIDTextBox;
		private ZTextBox VisibleOnlyToDeveloperTextBox;
		private ZLabel Tradenetv4PasswordGP_PasswordStatusDescriptionLabel;
		private ZLabel Tradenetv4PasswordGP_PasswordStatusLabel;

		#endregion
	}
}

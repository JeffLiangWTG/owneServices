using System.ComponentModel;
using System.Windows.Forms;

namespace Enterprise.MarketingManager.GUI
{
	public partial class ImageDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components;

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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1091:Do Not Set CausesValidation to false", Justification = "cancel buttons shouldn't cause validation")]
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.txtToolTip = new Enterprise.ZArchitecture.ZTextBox();
			this.toolTip1 = new CargoWise.Windows.UI.KToolTip(this.components);
			this.btnBrowseFile = new Enterprise.ZArchitecture.GUI.ZButton();
			this.buttonUploadImage = new Enterprise.ZArchitecture.GUI.ZButton();
			this.grpToolTip = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.grpURL = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.pnlUrl = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.rdMacro = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.rdoLocalFile = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.rdInternetURL = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.txtURL = new Enterprise.ZArchitecture.ZTextBox();
			this.groupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.txtAlt = new Enterprise.ZArchitecture.ZTextBox();
			this.cmbAlign = new CargoWise.Windows.UI.KComboBox();
			this.groupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.cmbBorderStyle = new CargoWise.Windows.UI.KComboBox();
			this.chkBorderColor = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.chkBorderStyle = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.txtBorder = new Enterprise.ZArchitecture.ZTextBox();
			this.lnkBgColor = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.chkBorderThickness = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.chkAlignment = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.txtBgColor = new Enterprise.ZArchitecture.ZTextBox();
			this.groupBox4 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.chkLockAspectRatio = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.txtHeight = new Enterprise.ZArchitecture.ZTextBox();
			this.txtWidth = new Enterprise.ZArchitecture.ZTextBox();
			this.chkWidth = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.chkHeight = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.btnCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.btnOK = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UploadImageMenuStrip = new CargoWise.Windows.UI.KContextMenuStrip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.grpToolTip.SuspendLayout();
			this.grpURL.SuspendLayout();
			this.pnlUrl.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.UploadImageMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// txtToolTip
			// 
			this.txtToolTip.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.txtToolTip.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtToolTip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.txtToolTip.Name = "txtToolTip";
			this.txtToolTip.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtToolTip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 17, true);
			this.txtToolTip.TabIndex = 0;
			// 
			// btnBrowseFile
			// 
			this.btnBrowseFile.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("51de4e20-586b-4cbd-902d-190f04399ac8", "Browse");
			this.btnBrowseFile.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnBrowseFile.Enabled = false;
			this.btnBrowseFile.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnBrowseFile.Image = global::Enterprise.MarketingManager.GUI.Properties.Resources.BrowseButtonImage;
			this.btnBrowseFile.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnBrowseFile.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 76, true);
			this.btnBrowseFile.Name = "btnBrowseFile";
			this.btnBrowseFile.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 24, true);
			this.btnBrowseFile.TabIndex = 53;
			this.btnBrowseFile.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.btnBrowseFile.ToolTipCaption = null;
			this.btnBrowseFile.UseVisualStyleBackColor = true;
			this.btnBrowseFile.Click += new System.EventHandler(this.btnBrowseFile_Click);
			// 
			// buttonUploadImage
			// 
			this.buttonUploadImage.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("d36a0ef5-5781-4d77-8373-72ce2626ebf2", "&Upload Image");
			this.buttonUploadImage.Cursor = System.Windows.Forms.Cursors.Hand;
			this.buttonUploadImage.Enabled = false;
			this.buttonUploadImage.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonUploadImage.Image = global::Enterprise.MarketingManager.GUI.Properties.Resources.InsertMacroImage;
			this.buttonUploadImage.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonUploadImage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 106, true);
			this.buttonUploadImage.Name = "buttonUploadImage";
			this.buttonUploadImage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 24, true);
			this.buttonUploadImage.TabIndex = 61;
			this.buttonUploadImage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonUploadImage.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.buttonUploadImage.ToolTipCaption = null;
			this.buttonUploadImage.UseVisualStyleBackColor = true;
			this.buttonUploadImage.Click += new System.EventHandler(this.ButtonUploadImage_Click);
			// 
			// grpToolTip
			// 
			this.grpToolTip.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("34ec8b12-dae9-494e-a792-ddd24ceb0362", "Tool Tip");
			this.grpToolTip.Controls.Add(this.txtToolTip);
			this.grpToolTip.Dock = System.Windows.Forms.DockStyle.Top;
			this.grpToolTip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 164, true);
			this.grpToolTip.Name = "grpToolTip";
			this.grpToolTip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 42, true);
			this.grpToolTip.TabIndex = 42;
			this.grpToolTip.TabStop = false;
			// 
			// grpURL
			// 
			this.grpURL.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("10bb1b5c-36a0-4b7e-a00d-7048bbddad95", "Picture Source URL");
			this.grpURL.Controls.Add(this.pnlUrl);
			this.grpURL.Dock = System.Windows.Forms.DockStyle.Top;
			this.grpURL.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.grpURL.Name = "grpURL";
			this.grpURL.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 164, true);
			this.grpURL.TabIndex = 40;
			this.grpURL.TabStop = false;
			// 
			// pnlUrl
			// 
			this.pnlUrl.Controls.Add(this.rdMacro);
			this.pnlUrl.Controls.Add(this.buttonUploadImage);
			this.pnlUrl.Controls.Add(this.btnBrowseFile);
			this.pnlUrl.Controls.Add(this.rdoLocalFile);
			this.pnlUrl.Controls.Add(this.rdInternetURL);
			this.pnlUrl.Controls.Add(this.txtURL);
			this.pnlUrl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 17, true);
			this.pnlUrl.Name = "pnlUrl";
			this.pnlUrl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(537, 141, true);
			this.pnlUrl.TabIndex = 52;
			this.pnlUrl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnlUrl_MouseMove);
			// 
			// rdMacro
			// 
			this.rdMacro.AutoCheck = false;
			this.rdMacro.AutoSize = true;
			this.rdMacro.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("b9e49908-51e5-4c8e-a338-140c80c7996a", "Macro");
			this.rdMacro.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 106, true);
			this.rdMacro.Name = "rdMacro";
			this.rdMacro.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 16, true);
			this.rdMacro.TabIndex = 56;
			this.rdMacro.TabStop = true;
			this.rdMacro.UseVisualStyleBackColor = true;
			this.rdMacro.CheckedChanged += new System.EventHandler(this.rdMacro_CheckedChanged);
			// 
			// rdoLocalFile
			// 
			this.rdoLocalFile.AutoCheck = false;
			this.rdoLocalFile.AutoSize = true;
			this.rdoLocalFile.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("5650e201-8966-447e-aca4-ccd6d27a40cd", "Local File with absolute path");
			this.rdoLocalFile.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 79, true);
			this.rdoLocalFile.Name = "rdoLocalFile";
			this.rdoLocalFile.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 16, true);
			this.rdoLocalFile.TabIndex = 55;
			this.rdoLocalFile.TabStop = true;
			this.rdoLocalFile.UseVisualStyleBackColor = true;
			this.rdoLocalFile.CheckedChanged += new System.EventHandler(this.rdoLocalFile_CheckedChanged);
			// 
			// rdInternetURL
			// 
			this.rdInternetURL.AutoCheck = false;
			this.rdInternetURL.AutoSize = true;
			this.rdInternetURL.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("b5e4ba1b-8b2a-41b7-b7cc-2e88991d128f", "Internet URL");
			this.rdInternetURL.Checked = true;
			this.rdInternetURL.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 56, true);
			this.rdInternetURL.Name = "rdInternetURL";
			this.rdInternetURL.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 16, true);
			this.rdInternetURL.TabIndex = 54;
			this.rdInternetURL.TabStop = true;
			this.rdInternetURL.UseVisualStyleBackColor = true;
			this.rdInternetURL.CheckedChanged += new System.EventHandler(this.rdInternetURL_CheckedChanged);
			// 
			// txtURL
			// 
			this.txtURL.BackColor = System.Drawing.SystemColors.Window;
			this.txtURL.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.txtURL.Dock = System.Windows.Forms.DockStyle.Top;
			this.txtURL.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.txtURL.Multiline = true;
			this.txtURL.Name = "txtURL";
			this.txtURL.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtURL.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(537, 50, true);
			this.txtURL.TabIndex = 52;
			this.txtURL.Validating += new System.ComponentModel.CancelEventHandler(this.txtURL_Validating);
			this.txtURL.Validated += new System.EventHandler(this.TxtURL_Validated);
			// 
			// groupBox1
			// 
			this.groupBox1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("56af7717-d112-4ded-a49f-116b5f2e29de", "Alternative Text");
			this.groupBox1.Controls.Add(this.txtAlt);
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.groupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 206, true);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 42, true);
			this.groupBox1.TabIndex = 43;
			this.groupBox1.TabStop = false;
			// 
			// txtAlt
			// 
			this.txtAlt.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.txtAlt.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtAlt.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.txtAlt.Name = "txtAlt";
			this.txtAlt.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtAlt.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 17, true);
			this.txtAlt.TabIndex = 0;
			// 
			// cmbAlign
			// 
			this.cmbAlign.Enabled = false;
			this.cmbAlign.FormattingEnabled = true;
			this.cmbAlign.Items.AddRange(new object[] {
            "Left",
            "Right",
            "Bottom",
            "Middle",
            "Top"});
			this.cmbAlign.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 18, true);
			this.cmbAlign.Name = "cmbAlign";
			this.cmbAlign.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 19, true);
			this.cmbAlign.TabIndex = 44;
			// 
			// groupBox2
			// 
			this.groupBox2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("7ec32042-848c-4726-a137-578804d48d52", "Layout");
			this.groupBox2.Controls.Add(this.cmbBorderStyle);
			this.groupBox2.Controls.Add(this.chkBorderColor);
			this.groupBox2.Controls.Add(this.chkBorderStyle);
			this.groupBox2.Controls.Add(this.txtBorder);
			this.groupBox2.Controls.Add(this.lnkBgColor);
			this.groupBox2.Controls.Add(this.chkBorderThickness);
			this.groupBox2.Controls.Add(this.chkAlignment);
			this.groupBox2.Controls.Add(this.txtBgColor);
			this.groupBox2.Controls.Add(this.cmbAlign);
			this.groupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 250, true);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 70, true);
			this.groupBox2.TabIndex = 46;
			this.groupBox2.TabStop = false;
			// 
			// cmbBorderStyle
			// 
			this.cmbBorderStyle.Enabled = false;
			this.cmbBorderStyle.FormattingEnabled = true;
			this.cmbBorderStyle.Items.AddRange(new object[] {
            "None",
            "Dotted",
            "Dashed",
            "Solid",
            "Double",
            "Groove",
            "Ridge",
            "Inset",
            "Outset"});
			this.cmbBorderStyle.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 42, true);
			this.cmbBorderStyle.Name = "cmbBorderStyle";
			this.cmbBorderStyle.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 19, true);
			this.cmbBorderStyle.TabIndex = 51;
			// 
			// chkBorderColor
			// 
			this.chkBorderColor.AutoSize = true;
			this.chkBorderColor.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(193, 20, true);
			this.chkBorderColor.Name = "chkBorderColor";
			this.chkBorderColor.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.chkBorderColor.TabIndex = 48;
			this.chkBorderColor.UseVisualStyleBackColor = true;
			this.chkBorderColor.CheckedChanged += new System.EventHandler(this.chkBorderColor_CheckedChanged);
			// 
			// chkBorderStyle
			// 
			this.chkBorderStyle.AutoSize = true;
			this.chkBorderStyle.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("15b6f670-979f-466b-937b-5a0b3b83357c", "Border Style");
			this.chkBorderStyle.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(193, 44, true);
			this.chkBorderStyle.Name = "chkBorderStyle";
			this.chkBorderStyle.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 16, true);
			this.chkBorderStyle.TabIndex = 47;
			this.chkBorderStyle.UseVisualStyleBackColor = true;
			this.chkBorderStyle.CheckedChanged += new System.EventHandler(this.chkBorderStyle_CheckedChanged);
			// 
			// txtBorder
			// 
			this.txtBorder.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.txtBorder.Enabled = false;
			this.txtBorder.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 42, true);
			this.txtBorder.Name = "txtBorder";
			this.txtBorder.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 17, true);
			this.txtBorder.TabIndex = 45;
			this.txtBorder.Validating += new System.ComponentModel.CancelEventHandler(this.txtBorder_Validating);
			// 
			// lnkBgColor
			// 
			this.lnkBgColor.AutoSize = true;
			this.lnkBgColor.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("68e3c45b-c803-44fb-84e5-838a7896960d", "Border Color");
			this.lnkBgColor.Enabled = false;
			this.lnkBgColor.IsFontBold = false;
			this.lnkBgColor.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 21, true);
			this.lnkBgColor.Name = "lnkBgColor";
			this.lnkBgColor.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 13, true);
			this.lnkBgColor.TabIndex = 50;
			this.lnkBgColor.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkBgColor_LinkClicked);
			// 
			// chkBorderThickness
			// 
			this.chkBorderThickness.AutoSize = true;
			this.chkBorderThickness.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("0cde4467-9b7f-460e-8fe2-c79690e39270", "Border Width");
			this.chkBorderThickness.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 44, true);
			this.chkBorderThickness.Name = "chkBorderThickness";
			this.chkBorderThickness.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 16, true);
			this.chkBorderThickness.TabIndex = 1;
			this.chkBorderThickness.UseVisualStyleBackColor = true;
			this.chkBorderThickness.CheckedChanged += new System.EventHandler(this.chkBorderThickness_CheckedChanged);
			// 
			// chkAlignment
			// 
			this.chkAlignment.AutoSize = true;
			this.chkAlignment.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("0ac48029-a3f1-4303-9424-ba191198e72b", "Alignment");
			this.chkAlignment.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 20, true);
			this.chkAlignment.Name = "chkAlignment";
			this.chkAlignment.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 16, true);
			this.chkAlignment.TabIndex = 0;
			this.chkAlignment.UseVisualStyleBackColor = true;
			this.chkAlignment.CheckedChanged += new System.EventHandler(this.chkAlignment_CheckedChanged);
			// 
			// txtBgColor
			// 
			this.txtBgColor.BackColor = System.Drawing.Color.White;
			this.txtBgColor.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.txtBgColor.Enabled = false;
			this.txtBgColor.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 17, true);
			this.txtBgColor.Name = "txtBgColor";
			this.txtBgColor.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 17, true);
			this.txtBgColor.TabIndex = 49;
			// 
			// groupBox4
			// 
			this.groupBox4.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("b1329410-3b13-47c3-b161-a323b1c21ffb", "Size");
			this.groupBox4.Controls.Add(this.chkLockAspectRatio);
			this.groupBox4.Controls.Add(this.txtHeight);
			this.groupBox4.Controls.Add(this.txtWidth);
			this.groupBox4.Controls.Add(this.chkWidth);
			this.groupBox4.Controls.Add(this.chkHeight);
			this.groupBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(369, 250, true);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 70, true);
			this.groupBox4.TabIndex = 48;
			this.groupBox4.TabStop = false;
			// 
			// chkLockAspectRatio
			// 
			this.chkLockAspectRatio.AutoSize = true;
			this.chkLockAspectRatio.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("135a0928-ff81-425b-991c-30445b1efd6c", "Lock Aspect Ratio");
			this.chkLockAspectRatio.Checked = true;
			this.chkLockAspectRatio.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkLockAspectRatio.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 0, true);
			this.chkLockAspectRatio.Name = "chkLockAspectRatio";
			this.chkLockAspectRatio.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 16, true);
			this.chkLockAspectRatio.TabIndex = 48;
			this.chkLockAspectRatio.UseVisualStyleBackColor = true;
			// 
			// txtHeight
			// 
			this.txtHeight.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.txtHeight.Enabled = false;
			this.txtHeight.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 17, true);
			this.txtHeight.Name = "txtHeight";
			this.txtHeight.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 17, true);
			this.txtHeight.TabIndex = 46;
			this.txtHeight.TextChanged += new System.EventHandler(this.txtHeight_TextChanged);
			// 
			// txtWidth
			// 
			this.txtWidth.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.txtWidth.Enabled = false;
			this.txtWidth.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 42, true);
			this.txtWidth.Name = "txtWidth";
			this.txtWidth.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 17, true);
			this.txtWidth.TabIndex = 47;
			this.txtWidth.TextChanged += new System.EventHandler(this.txtWidth_TextChanged);
			// 
			// chkWidth
			// 
			this.chkWidth.AutoSize = true;
			this.chkWidth.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("793ff773-e025-4007-b0b2-a0d86eead709", "Width");
			this.chkWidth.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 42, true);
			this.chkWidth.Name = "chkWidth";
			this.chkWidth.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 16, true);
			this.chkWidth.TabIndex = 2;
			this.chkWidth.UseVisualStyleBackColor = true;
			this.chkWidth.CheckedChanged += new System.EventHandler(this.chkWidth_CheckedChanged);
			// 
			// chkHeight
			// 
			this.chkHeight.AutoSize = true;
			this.chkHeight.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("d4af844e-f783-4374-b577-b412b78d5c92", "Height");
			this.chkHeight.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.chkHeight.Name = "chkHeight";
			this.chkHeight.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 16, true);
			this.chkHeight.TabIndex = 1;
			this.chkHeight.UseVisualStyleBackColor = true;
			this.chkHeight.CheckedChanged += new System.EventHandler(this.chkHeight_CheckedChanged);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("6e39a518-855d-4dfc-af53-a2d0ec9b9ebd", "Cancel");
			this.btnCancel.CausesValidation = false;
			this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(457, 331, true);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 23, true);
			this.btnCancel.TabIndex = 38;
			this.btnCancel.ToolTipCaption = null;
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// btnOK
			// 
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOK.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("9d0c6e42-4ee8-40ef-bfa1-b0cecb7d4132", "OK");
			this.btnOK.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnOK.ForeColor = System.Drawing.SystemColors.ControlText;
			this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnOK.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 331, true);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 23, true);
			this.btnOK.TabIndex = 37;
			this.btnOK.ToolTipCaption = null;
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// UploadImageMenuStrip
			// 
			this.UploadImageMenuStrip.ImageScalingSize = new System.Drawing.Size(96, 96);
			this.UploadImageMenuStrip.Name = "UploadImageMenuStrip";
			this.UploadImageMenuStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 3, true);
			// 
			// ImageDialog
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("fdda22c6-e44f-444b-8752-bf47239bbbe5", "Image Insert Dialog");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 354, true);
			this.Controls.Add(this.groupBox4);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.grpToolTip);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.grpURL);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "ImageDialog";
			this.Load += new System.EventHandler(this.ImageInsertDialog_Load);
			this.Controls.SetChildIndex(this.grpURL, 0);
			this.Controls.SetChildIndex(this.btnOK, 0);
			this.Controls.SetChildIndex(this.btnCancel, 0);
			this.Controls.SetChildIndex(this.grpToolTip, 0);
			this.Controls.SetChildIndex(this.groupBox1, 0);
			this.Controls.SetChildIndex(this.groupBox2, 0);
			this.Controls.SetChildIndex(this.groupBox4, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.grpToolTip.ResumeLayout(false);
			this.grpToolTip.PerformLayout();
			this.grpURL.ResumeLayout(false);
			this.grpURL.PerformLayout();
			this.pnlUrl.ResumeLayout(false);
			this.pnlUrl.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			this.UploadImageMenuStrip.ResumeLayout(false);
			this.UploadImageMenuStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KToolTip toolTip1;
		private Enterprise.ZArchitecture.ZTextBox txtToolTip;
		private Enterprise.ZArchitecture.GUI.ZGroupBox grpToolTip;
		private Enterprise.ZArchitecture.GUI.ZButton btnCancel;
		private Enterprise.ZArchitecture.GUI.ZButton btnOK;
		private Enterprise.ZArchitecture.GUI.ZGroupBox grpURL;
		private Enterprise.ZArchitecture.GUI.ZGroupBox groupBox1;
		private Enterprise.ZArchitecture.ZTextBox txtAlt;
		private CargoWise.Windows.UI.KComboBox cmbAlign;
		private Enterprise.ZArchitecture.GUI.ZGroupBox groupBox2;
		private Enterprise.ZArchitecture.ZTextBox txtBorder;
		private Enterprise.ZArchitecture.GUI.ZCheckBox chkBorderThickness;
		private Enterprise.ZArchitecture.GUI.ZCheckBox chkAlignment;
		private Enterprise.ZArchitecture.GUI.ZGroupBox groupBox4;
		private Enterprise.ZArchitecture.ZTextBox txtWidth;
		private Enterprise.ZArchitecture.ZTextBox txtHeight;
		private Enterprise.ZArchitecture.GUI.ZCheckBox chkWidth;
		private Enterprise.ZArchitecture.GUI.ZCheckBox chkHeight;
		private Enterprise.ZArchitecture.GUI.ZCheckBox chkBorderStyle;
		private CargoWise.Windows.UI.KComboBox cmbBorderStyle;
		private Enterprise.ZArchitecture.GUI.ZCheckBox chkBorderColor;
		private Enterprise.ZArchitecture.GUI.ZLinkLabel lnkBgColor;
		private Enterprise.ZArchitecture.ZTextBox txtBgColor;
		private Enterprise.ZArchitecture.GUI.ZCheckBox chkLockAspectRatio;
		private Enterprise.ZArchitecture.GUI.ZPanel pnlUrl;
		private Enterprise.ZArchitecture.GUI.ZButton btnBrowseFile;
		private Enterprise.ZArchitecture.GUI.ZRadioButton rdoLocalFile;
		private Enterprise.ZArchitecture.GUI.ZRadioButton rdInternetURL;
		private Enterprise.ZArchitecture.ZTextBox txtURL;
		private Enterprise.ZArchitecture.GUI.ZButton buttonUploadImage;
		private CargoWise.Windows.UI.KContextMenuStrip UploadImageMenuStrip;
		private ZArchitecture.GUI.ZRadioButton rdMacro;
	}
}

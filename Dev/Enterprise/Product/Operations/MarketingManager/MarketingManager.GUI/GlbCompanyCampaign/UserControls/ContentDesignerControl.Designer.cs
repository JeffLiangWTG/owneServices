namespace Enterprise.MarketingManager.GUI
{
	partial class ContentDesignerControl
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (htmlFileDialog != null)
				{
					htmlFileDialog.Dispose();
				}

				if (HtmlEditorTempFileDirectory != null)
				{
					HtmlEditorTempFileDirectory.Dispose();
				}

				ImageDialog?.Dispose();
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.splitContainer4 = new CargoWise.Windows.UI.KSplitContainer();
			this.splitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.macroGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.publishedListGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.publishedListDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.macroPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.splitContainer3 = new CargoWise.Windows.UI.KSplitContainer();
			this.mapTreeUserControl = new Enterprise.DocumentEngine.GUI.ReflectiveFieldMap.MapTreeUserControl();
			this.macroCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.infoTextBox = new CargoWise.Windows.UI.KTextBox();
			this.macroToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.copyMacroButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.addMacroButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.htmlContentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.emailContentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.buttonsToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.insertImageButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.saveButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.uploadButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.copyToClipboardButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.clearButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.linkGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.linkGrid = new Enterprise.ZArchitecture.ZGrid();
			this.trackAllLinksToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.trackAllLinksButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.htmlFileDialog = new Enterprise.ZArchitecture.GUI.ZOpenFileDialog();
			this.searchTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.findNextButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.findPreviousButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.matchCaseCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.previewContentLink = new ZArchitecture.GUI.ZLinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();

			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.macroGroupBox.SuspendLayout();
			this.publishedListGroupBox.SuspendLayout();
			this.publishedListDropEdit.SuspendLayout();
			this.macroPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
			this.splitContainer3.Panel1.SuspendLayout();
			this.splitContainer3.Panel2.SuspendLayout();
			this.splitContainer3.SuspendLayout();
			this.mapTreeUserControl.SuspendLayout();
			this.macroToolStrip.SuspendLayout();
			this.htmlContentGroupBox.SuspendLayout();
			this.buttonsToolStrip.SuspendLayout();
			this.linkGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.linkGrid)).BeginInit();
			this.linkGrid.SuspendLayout();
			this.trackAllLinksToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.GlbCompanyCampaign);
			// 
			// splitContainer1
			// 
			this.splitContainer1.BackColor = System.Drawing.Color.Transparent;
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.htmlContentGroupBox);
			this.splitContainer1.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(750);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.BackColor = System.Drawing.Color.Transparent;
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			this.splitContainer1.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(610);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1382, 764, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(758);
			this.splitContainer1.TabIndex = 2;

			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.BackColor = System.Drawing.Color.Transparent;
			this.splitContainer2.Panel1.Controls.Add(this.macroGroupBox);
			this.splitContainer2.Panel1.Controls.Add(this.publishedListGroupBox);
			this.splitContainer2.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(450);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.linkGroupBox);
			this.splitContainer2.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.splitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 586, true);
			this.splitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(325);
			this.splitContainer2.TabIndex = 1;
			// 
			// macroGroupBox
			// 
			this.macroGroupBox.BackColor = System.Drawing.Color.Transparent;
			this.macroGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("dfa4753b-bea9-4e07-be05-535ea4921262", "Macros");
			this.macroGroupBox.Controls.Add(this.macroPanel);
			this.macroGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.macroGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.macroGroupBox.Name = "macroGroupBox";
			this.macroGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 325, true);
			this.macroGroupBox.TabIndex = 0;
			this.macroGroupBox.TabStop = false;
			// 
			// publishedListGroupBox
			// 
			this.publishedListGroupBox.BackColor = System.Drawing.Color.Transparent;
			this.publishedListGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("c0ef8a4f-ad0f-4c98-831d-cdd6ee74608d", "Published Subscription List");
			this.publishedListGroupBox.Controls.Add(this.publishedListDropEdit);
			this.publishedListGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.publishedListGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 192, true);
			this.publishedListGroupBox.Name = "publishedListGroupBox";
			this.publishedListGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 40, true);
			this.publishedListGroupBox.TabIndex = 1;
			this.publishedListGroupBox.TabStop = false;
			// 
			// publishedListDropEdit
			// 
			this.publishedListDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.publishedListDropEdit, "PublishedListCodeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_PublishedListCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).PublishedListCodeDescription)));
			this.publishedListDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("636c48c4-db3e-4fe5-8368-63026727173c", "Published List");
			this.publishedListDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.publishedListDropEdit.Name = "publishedListDropEdit";
			this.publishedListDropEdit.PreBoundMaxLength = 50;
			this.publishedListDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 18, true);
			this.publishedListDropEdit.DescriptionBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 18, true);
			this.publishedListDropEdit.ShowDescriptionBox = false;
			this.publishedListDropEdit.ShowInDropDown = ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.publishedListDropEdit.TabIndex = 0;
			// 
			// macroPanel
			// 
			this.macroPanel.Controls.Add(this.splitContainer3);
			this.macroPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.macroPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.macroPanel.Name = "macroPanel";
			this.macroPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(614, 306, true);
			this.macroPanel.TabIndex = 0;
			// 
			// splitContainer3
			// 
			this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer3.Name = "splitContainer3";
			// 
			// splitContainer3.Panel1
			// 
			this.splitContainer3.Panel1.Controls.Add(this.mapTreeUserControl);
			this.splitContainer3.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
			// 
			// splitContainer3.Panel2
			// 
			this.splitContainer3.Panel2.Controls.Add(this.macroCheckBox);
			this.splitContainer3.Panel2.Controls.Add(this.infoTextBox);
			this.splitContainer3.Panel2.Controls.Add(this.macroToolStrip);
			this.splitContainer3.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(344);
			this.splitContainer3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(614, 306, true);
			this.splitContainer3.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(309);
			this.splitContainer3.TabIndex = 0;
			// 
			// mapTreeUserControl
			// 
			this.mapTreeUserControl.AllowDrop = true;
			this.mapTreeUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mapTreeUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mapTreeUserControl.Name = "mapTreeUserControl";
			this.mapTreeUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(309, 306, true);
			this.mapTreeUserControl.TabIndex = 1;
			// 
			// macroCheckBox
			// 
			this.macroCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.macroCheckBox.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.macroCheckBox, "TemplateEditor.EnableMacroDataPreview");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).TemplateEditor.EnableMacroDataPreview)));
			this.macroCheckBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("b08be5f2-27cc-4c4e-bf8b-76254114d71b", "Enable Macro Simulation");
			this.macroCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.macroCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 280, true);
			this.macroCheckBox.Name = "macroCheckBox";
			this.macroCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 17, true);
			this.macroCheckBox.TabIndex = 3;
			this.macroCheckBox.UseVisualStyleBackColor = false;
			this.macroCheckBox.Visible = true;
			// 
			// infoTextBox
			// 
			this.infoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.infoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 23, true);
			this.infoTextBox.Multiline = true;
			this.infoTextBox.Name = "infoTextBox";
			this.infoTextBox.ReadOnly = true;
			this.infoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 252, true);
			this.infoTextBox.TabIndex = 2;
			// 
			// macroToolStrip
			// 
			this.macroToolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.macroToolStrip.BackColor = System.Drawing.Color.Transparent;
			this.macroToolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.macroToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.macroToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
						this.copyMacroButton, this.addMacroButton});
			this.macroToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(202, 277, true);
			this.macroToolStrip.Name = "macroToolStrip";
			this.macroToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 25, true);
			this.macroToolStrip.TabIndex = 1;
			this.macroToolStrip.Text = "zToolStrip1";
			// 
			// copyMacroButton
			// 
			this.copyMacroButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("5b2d5ca7-39fb-4e8e-8891-a4ada270018c", "Copy Macro");
			this.copyMacroButton.Name = "copyMacroButton";
			this.copyMacroButton.Image = global::Enterprise.MarketingManager.GUI.Properties.Resources.CopyMacroImage;
			this.copyMacroButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.copyMacroButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 22, true);
			this.copyMacroButton.Click += new System.EventHandler(this.CopyMacroButton_Click);
			// 
			// addMacroButton
			// 
			this.addMacroButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("6231bb00-a9bd-4c88-b029-920b7f9603cc", "Insert Macro");
			this.addMacroButton.Image = global::Enterprise.MarketingManager.GUI.Properties.Resources.InsertMacroImage;
			this.addMacroButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.addMacroButton.Name = "addMacroButton";
			this.addMacroButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 22, true);
			this.addMacroButton.Click += new System.EventHandler(this.AddMacroButton_Click);
			// 
			// htmlContentGroupBox
			// 
			this.htmlContentGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.htmlContentGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("d6c632c9-0012-4ce8-837d-8b4262806fe0", "Content in HTML");
			this.htmlContentGroupBox.Controls.Add(this.previewContentLink);
			this.htmlContentGroupBox.Controls.Add(this.matchCaseCheckbox);
			this.htmlContentGroupBox.Controls.Add(this.findPreviousButton);
			this.htmlContentGroupBox.Controls.Add(this.findNextButton);
			this.htmlContentGroupBox.Controls.Add(this.searchTextBox);
			this.htmlContentGroupBox.Controls.Add(this.emailContentTextBox);
			this.htmlContentGroupBox.Controls.Add(this.buttonsToolStrip);
			this.htmlContentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
			this.htmlContentGroupBox.Name = "htmlContentGroupBox";
			this.htmlContentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.htmlContentGroupBox.TabIndex = 9;
			this.htmlContentGroupBox.TabStop = false;
			// 
			// emailContentTextBox
			// 
			this.emailContentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.emailContentTextBox, "TemplateEditor.TemplateHtmlText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).TemplateEditor.TemplateHtmlText)));
			this.emailContentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.emailContentTextBox, false);
			this.emailContentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 43, true);
			this.emailContentTextBox.Multiline = true;
			this.emailContentTextBox.Name = "emailContentTextBox";
			this.emailContentTextBox.ResetPosition = false;
			this.emailContentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.emailContentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(740, 670, true);
			this.emailContentTextBox.TabIndex = 2;
			this.emailContentTextBox.TextChanged += new System.EventHandler(this.EmailContentTextBox_TextChanged);
			this.emailContentTextBox.WordWrap = false;
			// 
			// buttonsToolStrip
			// 
			this.buttonsToolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonsToolStrip.BackColor = System.Drawing.Color.Transparent;
			this.buttonsToolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.buttonsToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.buttonsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
						this.insertImageButton,
						this.saveButton,
						this.uploadButton,
						this.copyToClipboardButton,
						this.clearButton});
			this.buttonsToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(403, 720, true);
			this.buttonsToolStrip.Name = "buttonsToolStrip";
			this.buttonsToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 25, true);
			this.buttonsToolStrip.TabIndex = 8;
			this.buttonsToolStrip.Text = "zToolStrip1";
			// 
			// InsertImageButton
			// 
			this.insertImageButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("5ed08df2-1d70-428d-a956-cd9b338ca4da", "Insert Image");
			this.insertImageButton.Image = global::Enterprise.MarketingManager.GUI.Properties.Resources.AddImageImage;
			this.insertImageButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.insertImageButton.Name = "insertImageButton";
			this.insertImageButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 22, true);
			this.insertImageButton.Click += new System.EventHandler(this.InsertImageButton_Click);
			// 
			// saveButton
			// 
			this.saveButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("7423efe9-aa2a-4cf5-b6a3-15377eff13fa", "Save to File");
			this.saveButton.Image = global::Enterprise.MarketingManager.GUI.Properties.Resources.SaveImage;
			this.saveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 22, true);
			this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// uploadButton
			// 
			this.uploadButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("9464423c-eea7-4221-9cc5-f210a2da756b", "Upload");
			this.uploadButton.Image = global::Enterprise.MarketingManager.GUI.Properties.Resources.UploadImage;
			this.uploadButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.uploadButton.Name = "uploadButton";
			this.uploadButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 22, true);
			this.uploadButton.Click += new System.EventHandler(this.UploadButton_Click);
			// 
			// copyToClipboardButton
			// 
			this.copyToClipboardButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("0b7f54b4-3634-4114-be84-c75719346df6", "Copy to Clipboard");
			this.copyToClipboardButton.Image = global::Enterprise.MarketingManager.GUI.Properties.Resources.CopyToClipboardImage2;
			this.copyToClipboardButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.copyToClipboardButton.Name = "copyToClipboardButton";
			this.copyToClipboardButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 22, true);
			this.copyToClipboardButton.Click += new System.EventHandler(this.CopyToClipboard_Click);
			// 
			// clearButton
			// 
			this.clearButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("4d2663fa-3c6d-4107-a67b-dc8fd9509c6f", "Clear");
			this.clearButton.Image = global::Enterprise.MarketingManager.GUI.Properties.Resources.ClearImage;
			this.clearButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.clearButton.Name = "clearButton";
			this.clearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 22, true);
			this.clearButton.Click += new System.EventHandler(this.ClearButton_Click);
			// 
			// linkGroupBox
			// 
			this.linkGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("aa2bfff9-7823-4ae6-8c03-8e74dcbc5d36", "Track Link Activity");
			this.linkGroupBox.Controls.Add(this.linkGrid);
			this.linkGroupBox.Controls.Add(this.trackAllLinksToolStrip);
			this.linkGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.linkGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.linkGroupBox.Name = "linkGroupBox";
			this.linkGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 174, true);
			this.linkGroupBox.TabIndex = 0;
			this.linkGroupBox.TabStop = false;
			// 
			// linkGrid
			// 
			this.linkGrid.AllowCopyToNewRowMenuItem = false;
			this.linkGrid.AllowDragDropWithChanges = false;
			this.linkGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.linkGrid, "TemplateEditor.TrackedLinks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).TemplateEditor.TrackedLinks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignLink)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).TemplateEditor.TrackedLinks)).SyncRoot)).HasTrackingMacro)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignLink)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).TemplateEditor.TrackedLinks)).SyncRoot)).GCL_Context)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignLink)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).TemplateEditor.TrackedLinks)).SyncRoot)).GCL_IsImage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignLink)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).TemplateEditor.TrackedLinks)).SyncRoot)).UnicodeUrl)));
			this.linkGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("5239a6df-09c4-4bcb-9eb9-d68c76d9a11c", "Track");
			zCheckBoxColumnStyleInfo1.ColumnName = "HasTrackingMacro";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("0e6ef390-27d3-4735-b952-66bfa780b7ec", "Context");
			zTextBoxColumnStyleInfo1.ColumnName = "GCL_Context";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("7c6bfb86-403d-4e39-9eee-ed43cae1efed", "URL");
			zTextBoxColumnStyleInfo2.ColumnName = "UnicodeUrl";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			this.linkGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.linkGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.linkGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.linkGrid.CopySelectedRowsAllowed = false;
			this.linkGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			this.linkGrid.GridId = "eabdc4aa-4a2c-415d-b209-a5da5d5d4868";
			this.linkGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.linkGrid.LayoutKey = "linkGrid";
			this.linkGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.linkGrid.Name = "linkGrid";
			this.linkGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(614, 250, true);
			this.linkGrid.TabIndex = 35;
			this.linkGrid.DoubleClick += new System.EventHandler(this.linkGrid_DoubleClick);
			// 
			// trackAllLinksToolStrip
			// 
			this.trackAllLinksToolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.trackAllLinksToolStrip.BackColor = System.Drawing.Color.Transparent;
			this.trackAllLinksToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.trackAllLinksToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
				this.trackAllLinksButton});
			this.trackAllLinksToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(521, 266, true);
			this.trackAllLinksToolStrip.Name = "trackAllLinksToolStrip";
			this.trackAllLinksToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 25, true);
			this.trackAllLinksToolStrip.TabIndex = 1;
			this.trackAllLinksToolStrip.Text = "zToolStrip1";
			// 
			// trackAllLinksButton
			// 
			this.trackAllLinksButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("b22eed11-2b4d-4461-899a-08dd51486013", "Track All Clickable Links");
			this.trackAllLinksButton.Image = global::Enterprise.MarketingManager.GUI.Properties.Resources.TrackAllClickableLinks;
			this.trackAllLinksButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.trackAllLinksButton.Name = "trackAllLinksButton";
			this.trackAllLinksButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 22, true);
			this.trackAllLinksButton.Click += new System.EventHandler(this.TrackAllLinksButton_Click);
			// 
			// htmlFileDialog
			// 
			this.htmlFileDialog.AddExtension = true;
			this.htmlFileDialog.CheckFileExists = true;
			this.htmlFileDialog.CheckPathExists = true;
			this.htmlFileDialog.DefaultExt = "*.htm|*.html";
			this.htmlFileDialog.DereferenceLinks = true;
			this.htmlFileDialog.Filter = "HTML files (*.htm, *.html)|*.htm;*.html|All files (*.*)|*.*";
			this.htmlFileDialog.FilterIndex = 1;
			this.htmlFileDialog.InitialDirectory = "";
			this.htmlFileDialog.Multiselect = false;
			this.htmlFileDialog.ReadOnlyChecked = false;
			this.htmlFileDialog.RestoreDirectory = false;
			this.htmlFileDialog.ShowHelp = false;
			this.htmlFileDialog.SupportMultiDottedExtensions = false;
			this.htmlFileDialog.Title = "";
			this.htmlFileDialog.ValidateNames = true;
			this.htmlFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.HTMLFileDialog_FileOk);
			// 
			// searchTextBox
			// 
			this.searchTextBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("32848507-6838-4ee4-9706-443ce244063d", "Find");
			this.searchTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(34, 18, true);
			this.searchTextBox.Name = "searchTextBox";
			this.searchTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 15, true);
			this.searchTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.searchTextBox.TabIndex = 4;
			// 
			// findNextButton
			// 
			this.findNextButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("e35b372b-a535-483e-92b0-e35f828eb902", "Find Next");
			this.findNextButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(226, 15, true);
			this.findNextButton.Name = "findNextButton";
			this.findNextButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 23, true);
			this.findNextButton.TabIndex = 5;
			this.findNextButton.ToolTipCaption = null;
			this.findNextButton.UseVisualStyleBackColor = true;
			this.findNextButton.Click += new System.EventHandler(this.FindNextButton_Click);
			// 
			// findPreviousButton
			// 
			this.findPreviousButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ed471284-f4c7-48b7-b5a8-e16d77e1bebc", "Find Previous");
			this.findPreviousButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 15, true);
			this.findPreviousButton.Name = "findPreviousButton";
			this.findPreviousButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 23, true);
			this.findPreviousButton.TabIndex = 6;
			this.findPreviousButton.ToolTipCaption = null;
			this.findPreviousButton.UseVisualStyleBackColor = true;
			this.findPreviousButton.Click += new System.EventHandler(this.FindPreviousButton_Click);
			// 
			// matchCaseCheckbox
			// 
			this.matchCaseCheckbox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("8827beb9-d02c-4271-b9c2-f1c412cecc16", "Match Case");
			this.matchCaseCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 19, true);
			this.matchCaseCheckbox.Name = "matchCaseCheckbox";
			this.matchCaseCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 17, true);
			this.matchCaseCheckbox.TabIndex = 7;
			this.matchCaseCheckbox.UseVisualStyleBackColor = true;
			//
			// previewContentLink
			//
			this.previewContentLink.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("396053f8-8083-43e1-87d7-62ab7c13f4a7", "Preview Content");
			this.previewContentLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 19, true);
			this.previewContentLink.AutoSize = true;
			this.previewContentLink.Name = "previewContentLink";
			this.previewContentLink.TabIndex = 8;
			this.previewContentLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.PreviewContentLink_Click);
			//
			// ContentDesignerControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Name = "ContentDesignerControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1382, 764, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.splitContainer2.PerformLayout();
			this.macroGroupBox.ResumeLayout(false);
			this.macroGroupBox.PerformLayout();
			this.publishedListGroupBox.ResumeLayout(false);
			this.publishedListGroupBox.PerformLayout();
			this.publishedListDropEdit.ResumeLayout(true);
			this.publishedListDropEdit.PerformLayout();
			this.macroPanel.ResumeLayout(false);
			this.macroPanel.PerformLayout();
			this.splitContainer3.Panel1.ResumeLayout(false);
			this.splitContainer3.Panel2.ResumeLayout(false);
			this.splitContainer3.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
			this.splitContainer3.ResumeLayout(false);
			this.splitContainer3.PerformLayout();
			this.mapTreeUserControl.ResumeLayout(true);
			this.mapTreeUserControl.PerformLayout();
			this.macroToolStrip.ResumeLayout(false);
			this.macroToolStrip.PerformLayout();
			this.htmlContentGroupBox.ResumeLayout(false);
			this.htmlContentGroupBox.PerformLayout();
			this.buttonsToolStrip.ResumeLayout(false);
			this.buttonsToolStrip.PerformLayout();
			this.linkGroupBox.ResumeLayout(false);
			this.linkGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.linkGrid)).EndInit();
			this.linkGrid.ResumeLayout(false);
			this.linkGrid.PerformLayout();
			this.trackAllLinksToolStrip.ResumeLayout(false);
			this.trackAllLinksToolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private ZArchitecture.GUI.ZCheckBox macroCheckBox;
		private CargoWise.Windows.UI.KSplitContainer splitContainer4;
		private CargoWise.Windows.UI.KSplitContainer splitContainer2;
		private ZArchitecture.GUI.ZGroupBox macroGroupBox;
		private ZArchitecture.GUI.ZPanel macroPanel;
		private CargoWise.Windows.UI.KSplitContainer splitContainer3;
		protected DocumentEngine.GUI.ReflectiveFieldMap.MapTreeUserControl mapTreeUserControl;
		protected CargoWise.Windows.UI.KTextBox infoTextBox;
		private ZArchitecture.GUI.ZToolStrip macroToolStrip;
		private ZArchitecture.GUI.ZToolStripButton addMacroButton;
		private ZArchitecture.GUI.ZToolStripButton copyMacroButton;
		private ZArchitecture.GUI.ZGroupBox htmlContentGroupBox;
		protected ZArchitecture.ZTextBox emailContentTextBox;
		private ZArchitecture.GUI.ZToolStrip buttonsToolStrip;
		private ZArchitecture.GUI.ZToolStripButton insertImageButton;
		private ZArchitecture.GUI.ZToolStripButton saveButton;
		private ZArchitecture.GUI.ZToolStripButton uploadButton;
		private ZArchitecture.GUI.ZToolStripButton copyToClipboardButton;
		private ZArchitecture.GUI.ZToolStripButton clearButton;
		private ZArchitecture.GUI.ZGroupBox linkGroupBox;
		internal ZArchitecture.ZGrid linkGrid;
		private ZArchitecture.GUI.ZToolStrip trackAllLinksToolStrip;
		private ZArchitecture.GUI.ZToolStripButton trackAllLinksButton;
		private Enterprise.ZArchitecture.GUI.ZOpenFileDialog htmlFileDialog;
		private ZArchitecture.GUI.ZGroupBox publishedListGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit publishedListDropEdit;
		protected ZArchitecture.GUI.ZLinkLabel previewContentLink;
		protected ZArchitecture.GUI.ZCheckBox matchCaseCheckbox;
		protected ZArchitecture.GUI.ZButton findPreviousButton;
		protected ZArchitecture.GUI.ZButton findNextButton;
		protected ZArchitecture.ZTextBox searchTextBox;
	}
}

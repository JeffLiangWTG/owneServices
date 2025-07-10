namespace Enterprise.MasterFiles.GUI
{
	partial class RecipientSelectionForm
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
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.GoButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SearchTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AddressBookDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AddressBookRecipientsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AdvancedSearchLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.ToTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ToButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CcTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CcButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BccTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BccButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NewCancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SearchGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AddressBookGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdvancedSearchFiltersBeingAppliedMessageLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AddressBookRecipientsGrid)).BeginInit();
			this.SearchGroupBox.SuspendLayout();
			this.AddressBookGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 354, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(722, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RecipientSelection);
			// 
			// GoButton
			// 
			this.GoButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dbcb3d4d-ec78-4392-a11d-437375447f14", "Go");
			this.GoButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 17, true);
			this.GoButton.Name = "GoButton";
			this.GoButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.GoButton.TabIndex = 3;
			this.GoButton.Click += new System.EventHandler(this.GoButton_Click);
			// 
			// SearchTextBox
			// 
			this.SearchTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.SearchTextBox, "SearchQuery");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RecipientSelection)(null)).SearchQuery)));
			this.SearchTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SearchTextBox, false);
			this.SearchTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.SearchTextBox.Name = "SearchTextBox";
			this.SearchTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 20, true);
			this.SearchTextBox.TabIndex = 2;
			// 
			// AddressBookDropEdit
			// 
			this.AddressBookDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressBookDropEdit, "AddressBook");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RecipientSelection)(null)).AddressBook)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AddressBookDropEdit, false);
			this.AddressBookDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.AddressBookDropEdit.Name = "AddressBookDropEdit";
			this.AddressBookDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(357, 20, true);
			this.AddressBookDropEdit.TabIndex = 7;
			// 
			// AddressBookRecipientsGrid
			// 
			this.AddressBookRecipientsGrid.AllowBeginDrag = false;
			this.AddressBookRecipientsGrid.AllowDragDropWithChanges = false;
			this.AddressBookRecipientsGrid.AllowNavigation = false;
			this.AddressBookRecipientsGrid.AllowReadOnlyToModifyTabStop = true;
			this.AddressBookRecipientsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AddressBookRecipientsGrid, "AvailableRecipients");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RecipientSelection)(null)).AvailableRecipients)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AddressBookRecipient)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RecipientSelection)(null)).AvailableRecipients)).SyncRoot)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AddressBookRecipient)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RecipientSelection)(null)).AvailableRecipients)).SyncRoot)).Title)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AddressBookRecipient)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RecipientSelection)(null)).AvailableRecipients)).SyncRoot)).Phone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AddressBookRecipient)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RecipientSelection)(null)).AvailableRecipients)).SyncRoot)).Location)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AddressBookRecipient)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RecipientSelection)(null)).AvailableRecipients)).SyncRoot)).Email)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AddressBookRecipient)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RecipientSelection)(null)).AvailableRecipients)).SyncRoot)).Role)));
			this.AddressBookRecipientsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0622c0c9-978a-47e4-95be-0c7d9a378f81", "Name");
			zTextBoxColumnStyleInfo1.ColumnName = "Name";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.Caption = "";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("da53fad2-4532-41b2-8add-ffc6c9e05e90", "Title");
			zTextBoxColumnStyleInfo2.ColumnName = "Title";
			zTextBoxColumnStyleInfo3.Caption = "";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("01246ae3-e52e-4f27-8227-d21eae66664d", "Phone");
			zTextBoxColumnStyleInfo3.ColumnName = "Phone";
			zTextBoxColumnStyleInfo4.Caption = "";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7e350df4-9761-4783-b15d-5c764686484e", "Location");
			zTextBoxColumnStyleInfo4.ColumnName = "Location";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.Caption = "";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d82da40b-7d59-44f6-9093-32155f1e4cf9", "Email");
			zTextBoxColumnStyleInfo5.ColumnName = "Email";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.Caption = "";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dcb32405-ecdd-4d47-8149-7f46c683bfe5", "Role");
			zTextBoxColumnStyleInfo6.ColumnName = "Role";
			this.AddressBookRecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AddressBookRecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AddressBookRecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AddressBookRecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.AddressBookRecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.AddressBookRecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.AddressBookRecipientsGrid.CopySelectedRowsAllowed = true;
			this.AddressBookRecipientsGrid.GridId = "6f46d90b-ed13-49e8-a1cf-3a3fd234851b";
			this.AddressBookRecipientsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AddressBookRecipientsGrid.LayoutKey = "AddressBookRecipientsGrid";
			this.AddressBookRecipientsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 77, true);
			this.AddressBookRecipientsGrid.Name = "AddressBookRecipientsGrid";
			this.AddressBookRecipientsGrid.ReadOnly = true;
			this.AddressBookRecipientsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.AddressBookRecipientsGrid.ShouldSetErrorsOnTabPage = false;
			this.AddressBookRecipientsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 159, true);
			this.AddressBookRecipientsGrid.TabIndex = 8;
			this.AddressBookRecipientsGrid.TabStop = false;
			this.AddressBookRecipientsGrid.DoubleClick += new System.EventHandler(this.AddressBookRecipientsGrid_DoubleClick);
			// 
			// AdvancedSearchLinkLabel
			// 
			this.AdvancedSearchLinkLabel.AutoSize = true;
			this.AdvancedSearchLinkLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("390cc4ff-4c06-4647-915e-cf479f3b4d2a", "Advanced Search");
			this.AdvancedSearchLinkLabel.IsFontBold = false;
			this.AdvancedSearchLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(226, 46, true);
			this.AdvancedSearchLinkLabel.Name = "AdvancedSearchLinkLabel";
			this.AdvancedSearchLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 13, true);
			this.AdvancedSearchLinkLabel.TabIndex = 6;
			this.AdvancedSearchLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.AdvancedSearchLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.AdvancedSearchLinkLabel_LinkClicked);
			// 
			// ToTextBox
			// 
			this.ToTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ToTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ToTextBox, "ToEmailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RecipientSelection)(null)).ToEmailAddress)));
			this.ToTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ToTextBox, false);
			this.ToTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 242, true);
			this.ToTextBox.Name = "ToTextBox";
			this.ToTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(612, 20, true);
			this.ToTextBox.TabIndex = 10;
			// 
			// ToButton
			// 
			this.ToButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ToButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("87727304-d4ed-494e-87af-4cc19a297391", "To ->");
			this.ToButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 240, true);
			this.ToButton.Name = "ToButton";
			this.ToButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.ToButton.TabIndex = 9;
			this.ToButton.Click += new System.EventHandler(this.ToButton_Click);
			// 
			// CcTextBox
			// 
			this.CcTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CcTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CcTextBox, "Cc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RecipientSelection)(null)).Cc)));
			this.CcTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CcTextBox, false);
			this.CcTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 268, true);
			this.CcTextBox.Name = "CcTextBox";
			this.CcTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(612, 20, true);
			this.CcTextBox.TabIndex = 12;
			// 
			// CcButton
			// 
			this.CcButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CcButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6150e7bc-712e-4987-b737-e7e4344cb11e", "Cc ->");
			this.CcButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 266, true);
			this.CcButton.Name = "CcButton";
			this.CcButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.CcButton.TabIndex = 11;
			this.CcButton.Click += new System.EventHandler(this.CcButton_Click);
			// 
			// BccTextBox
			// 
			this.BccTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BccTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.BccTextBox, "Bcc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RecipientSelection)(null)).Bcc)));
			this.BccTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BccTextBox, false);
			this.BccTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 294, true);
			this.BccTextBox.Name = "BccTextBox";
			this.BccTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(612, 20, true);
			this.BccTextBox.TabIndex = 14;
			// 
			// BccButton
			// 
			this.BccButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BccButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("253b7e35-ba9a-4508-a9f3-239e8c1e1ec9", "Bcc ->");
			this.BccButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 292, true);
			this.BccButton.Name = "BccButton";
			this.BccButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.BccButton.TabIndex = 13;
			this.BccButton.Click += new System.EventHandler(this.BccButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("57162f6d-46d5-437b-8b3b-0f82a7b92a00", "OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(554, 320, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 15;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// NewCancelButton
			// 
			this.NewCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.NewCancelButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5be981d8-084e-446e-b253-2c7341aebad6", "Cancel");
			this.NewCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.NewCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(635, 320, true);
			this.NewCancelButton.Name = "NewCancelButton";
			this.NewCancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.NewCancelButton.TabIndex = 16;
			this.NewCancelButton.UseVisualStyleBackColor = true;
			this.NewCancelButton.Click += new System.EventHandler(this.NewCancelButton_Click);
			// 
			// SearchGroupBox
			// 
			this.SearchGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("45937000-0f89-4355-a9ae-86f211c4ccdf", "Search");
			this.SearchGroupBox.Controls.Add(this.GoButton);
			this.SearchGroupBox.Controls.Add(this.SearchTextBox);
			this.SearchGroupBox.Controls.Add(this.AdvancedSearchLinkLabel);
			this.SearchGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 3, true);
			this.SearchGroupBox.Name = "SearchGroupBox";
			this.SearchGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 68, true);
			this.SearchGroupBox.TabIndex = 1;
			this.SearchGroupBox.TabStop = false;
			// 
			// AddressBookGroupBox
			// 
			this.AddressBookGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("fb8b9332-5bc5-487d-8e8a-b1921369a90e", "Address Book");
			this.AddressBookGroupBox.Controls.Add(this.AddressBookDropEdit);
			this.AddressBookGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 3, true);
			this.AddressBookGroupBox.Name = "AddressBookGroupBox";
			this.AddressBookGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 45, true);
			this.AddressBookGroupBox.TabIndex = 2;
			this.AddressBookGroupBox.TabStop = false;
			// 
			// AdvancedSearchFiltersBeingAppliedMessageLabel
			// 
			this.AdvancedSearchFiltersBeingAppliedMessageLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AdvancedSearchFiltersBeingAppliedMessageLabel, "AdvancedSearchFiltersBeingAppliedMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RecipientSelection)(null)).AdvancedSearchFiltersBeingAppliedMessage)));
			this.AdvancedSearchFiltersBeingAppliedMessageLabel.ForeColor = System.Drawing.Color.Green;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AdvancedSearchFiltersBeingAppliedMessageLabel, false);
			this.AdvancedSearchFiltersBeingAppliedMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 51, true);
			this.AdvancedSearchFiltersBeingAppliedMessageLabel.Name = "AdvancedSearchFiltersBeingAppliedMessageLabel";
			this.AdvancedSearchFiltersBeingAppliedMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.AdvancedSearchFiltersBeingAppliedMessageLabel.TabIndex = 17;
			// 
			// RecipientSelectionForm
			// 
			this.AcceptButton = this.ToButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.NewCancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("371b03f3-dfc7-4f1b-9213-926fca85cf59", "Select Names");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(722, 378, true);
			this.Controls.Add(this.AdvancedSearchFiltersBeingAppliedMessageLabel);
			this.Controls.Add(this.AddressBookGroupBox);
			this.Controls.Add(this.SearchGroupBox);
			this.Controls.Add(this.NewCancelButton);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.BccButton);
			this.Controls.Add(this.CcButton);
			this.Controls.Add(this.ToButton);
			this.Controls.Add(this.BccTextBox);
			this.Controls.Add(this.CcTextBox);
			this.Controls.Add(this.ToTextBox);
			this.Controls.Add(this.AddressBookRecipientsGrid);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RecipientSelection);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 409, true);
			this.Name = "RecipientSelectionForm";
			this.Controls.SetChildIndex(this.AddressBookRecipientsGrid, 0);
			this.Controls.SetChildIndex(this.ToTextBox, 0);
			this.Controls.SetChildIndex(this.CcTextBox, 0);
			this.Controls.SetChildIndex(this.BccTextBox, 0);
			this.Controls.SetChildIndex(this.ToButton, 0);
			this.Controls.SetChildIndex(this.CcButton, 0);
			this.Controls.SetChildIndex(this.BccButton, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.NewCancelButton, 0);
			this.Controls.SetChildIndex(this.SearchGroupBox, 0);
			this.Controls.SetChildIndex(this.AddressBookGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.AdvancedSearchFiltersBeingAppliedMessageLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AddressBookRecipientsGrid)).EndInit();
			this.SearchGroupBox.ResumeLayout(false);
			this.SearchGroupBox.PerformLayout();
			this.AddressBookGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.ZGrid AddressBookRecipientsGrid;
		private ZArchitecture.GUI.ZDropEdit AddressBookDropEdit;
		private ZArchitecture.ZTextBox SearchTextBox;
		private ZArchitecture.GUI.ZButton GoButton;
		private ZArchitecture.GUI.ZLinkLabel AdvancedSearchLinkLabel;
		protected ZArchitecture.ZTextBox ToTextBox;
		private ZArchitecture.GUI.ZButton ToButton;
		protected ZArchitecture.ZTextBox CcTextBox;
		private ZArchitecture.GUI.ZButton CcButton;
		protected ZArchitecture.ZTextBox BccTextBox;
		private ZArchitecture.GUI.ZButton BccButton;
		private ZArchitecture.GUI.ZButton OKButton;
		private ZArchitecture.GUI.ZButton NewCancelButton;
		private ZArchitecture.GUI.ZGroupBox SearchGroupBox;
		private ZArchitecture.GUI.ZGroupBox AddressBookGroupBox;
		private ZArchitecture.ZLabel AdvancedSearchFiltersBeingAppliedMessageLabel;

	}
}

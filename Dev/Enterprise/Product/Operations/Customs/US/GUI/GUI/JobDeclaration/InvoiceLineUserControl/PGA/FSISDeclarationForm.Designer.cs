namespace Enterprise.Customs.US.GUI
{
	partial class FSISDeclarationForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.Customs.US.GUI.SealNumberColumnStyleInfo sealNumberColumnStyleInfo1 = new Enterprise.Customs.US.GUI.SealNumberColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FSISSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.CertificateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FSISCertificatesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ImportingEstNoCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DateOfInspectDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CommercialDescMultiLineTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProductIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertificateNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IntendedUseDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ProductIDQualifierDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryOfOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.IssuerCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.sealNumberUserControl = new Enterprise.Customs.US.GUI.SealNumberUserControl();
			this.ExportingEstNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PGAContactEmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PGAContactPhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PGAContantNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertifyingIndividualDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FSISSplitContainer)).BeginInit();
			this.FSISSplitContainer.Panel1.SuspendLayout();
			this.FSISSplitContainer.Panel2.SuspendLayout();
			this.FSISSplitContainer.SuspendLayout();
			this.CertificateGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FSISCertificatesGrid)).BeginInit();
			this.FSISCertificatesGrid.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.DateOfInspectDateEdit.SuspendLayout();
			this.IntendedUseDropEdit.SuspendLayout();
			this.ProductIDQualifierDropEdit.SuspendLayout();
			this.CountryOfOriginCodeFindBox.SuspendLayout();
			this.IssuerCountryCodeFindBox.SuspendLayout();
			this.sealNumberUserControl.SuspendLayout();
			this.CertifyingIndividualDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 500, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.JobDeclaration);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(663, 373, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 19;
			this.OKButton.Text = "OK";
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// FSISSplitContainer
			// 
			this.FSISSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FSISSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FSISSplitContainer.Name = "FSISSplitContainer";
			this.FSISSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// FSISSplitContainer.Panel1
			// 
			this.FSISSplitContainer.Panel1.Controls.Add(this.CertificateGroupBox);
			this.FSISSplitContainer.Panel1.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.FSISSplitContainer.Panel1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			// 
			// FSISSplitContainer.Panel2
			// 
			this.FSISSplitContainer.Panel2.Controls.Add(this.DetailsGroupBox);
			this.FSISSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(783, 524, true);
			this.FSISSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(111);
			this.FSISSplitContainer.TabIndex = 3;
			// 
			// CertificateGroupBox
			// 
			this.CertificateGroupBox.Controls.Add(this.FSISCertificatesGrid);
			this.CertificateGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CertificateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CertificateGroupBox.Name = "CertificateGroupBox";
			this.CertificateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(777, 105, true);
			this.CertificateGroupBox.TabIndex = 0;
			this.CertificateGroupBox.TabStop = false;
			this.CertificateGroupBox.Text = "Certificates";
			// 
			// FSISCertificatesGrid
			// 
			this.FSISCertificatesGrid.AllowNavigation = false;
			this.FSISCertificatesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FSISCertificatesGrid, "FSISLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_HealthCertificateNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_CommercialDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_UC_NKCertificateIssuerCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_UC_NKCountryOfOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_ProductID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_ProductIDQualifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_IntendedUseCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_ExportingEstNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_ImportingEstNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_SealNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_DateOfInspection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_CertifyingIndividual)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_PGAContactName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_PGAContactEmail)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_PGAContactPhoneNo)));
			this.FSISCertificatesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Certificate";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "US_HealthCertificateNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(86);
			zMultiLineTextBoxColumnInfo1.Caption = "Description of Product";
			zMultiLineTextBoxColumnInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiLineTextBoxColumnInfo1.ColumnName = "US_CommercialDescription";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			zCodeFindBoxColumnStyleInfo1.Caption = "Issued";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "US_UC_NKCertificateIssuerCountry";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.Caption = "Origin";
			zCodeFindBoxColumnStyleInfo2.ColumnName = "US_UC_NKCountryOfOrigin";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.Caption = "Product ID";
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "US_ProductID";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.Caption = "Qualifier";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "US_ProductIDQualifier";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.Caption = "Intended Use";
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "US_IntendedUseCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.Caption = "Export Est. No.";
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "US_ExportingEstNo";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92);
			zCodeFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("15d27e45-1053-453a-a098-890b615a7674", "Import Est.");
			zCodeFindBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCodeFindBoxColumnStyleInfo3.ColumnName = "US_ImportingEstNo";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			sealNumberColumnStyleInfo1.Caption = "Seals";
			sealNumberColumnStyleInfo1.ColumnName = "US_SealNumbers";
			sealNumberColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(158);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2b81aebd-c221-4b0f-bd9a-bb20238c622a", "Date Of Inspection");
			zDateEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo1.ColumnName = "US_DateOfInspection";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo3.Caption = "Certifying Individual";
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "US_CertifyingIndividual";
			zDropEditColumnStyleInfo3.IsMandatory = true;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo5.Caption = "PGA Contact Name";
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "US_PGAContactName";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.Caption = "PGA Contact Email";
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "US_PGAContactEmail";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo7.Caption = "PGA Contact Phone No";
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "US_PGAContactPhoneNo";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.FSISCertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FSISCertificatesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.FSISCertificatesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.FSISCertificatesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.FSISCertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FSISCertificatesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.FSISCertificatesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.FSISCertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FSISCertificatesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.FSISCertificatesGrid.ColumnStyles.Add(sealNumberColumnStyleInfo1);
			this.FSISCertificatesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FSISCertificatesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.FSISCertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FSISCertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FSISCertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FSISCertificatesGrid.DataSource = this.BindingSource;
			this.FSISCertificatesGrid.GridId = "079516e5-dd0d-4ec7-b211-871b80cbca55";
			this.FSISCertificatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FSISCertificatesGrid.LayoutKey = "FSISCertificatesGrid";
			this.FSISCertificatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 19, true);
			this.FSISCertificatesGrid.Name = "FSISCertificatesGrid";
			this.FSISCertificatesGrid.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.FSISCertificatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 67, true);
			this.FSISCertificatesGrid.TabIndex = 2;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.AutoSize = true;
			this.DetailsGroupBox.Controls.Add(this.ImportingEstNoCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.DateOfInspectDateEdit);
			this.DetailsGroupBox.Controls.Add(this.CommercialDescMultiLineTextBox);
			this.DetailsGroupBox.Controls.Add(this.ProductIDTextBox);
			this.DetailsGroupBox.Controls.Add(this.CertificateNumberTextBox);
			this.DetailsGroupBox.Controls.Add(this.IntendedUseDropEdit);
			this.DetailsGroupBox.Controls.Add(this.ProductIDQualifierDropEdit);
			this.DetailsGroupBox.Controls.Add(this.CountryOfOriginCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.IssuerCountryCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.sealNumberUserControl);
			this.DetailsGroupBox.Controls.Add(this.ExportingEstNoTextBox);
			this.DetailsGroupBox.Controls.Add(this.PGAContactEmailTextBox);
			this.DetailsGroupBox.Controls.Add(this.PGAContactPhoneTextBox);
			this.DetailsGroupBox.Controls.Add(this.PGAContantNameTextBox);
			this.DetailsGroupBox.Controls.Add(this.CertifyingIndividualDropEdit);
			this.DetailsGroupBox.Controls.Add(this.OKButton);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(783, 409, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			this.DetailsGroupBox.Text = "Details";
			this.DetailsGroupBox.UseCompatibleTextRendering = true;
			// 
			// ImportingEstNoCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.ImportingEstNoCodeFindBox, "FSISLines.US_ImportingEstNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_ImportingEstNo)));
			this.ImportingEstNoCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d8d6052c-43f6-42cc-9fae-91c0cd994d68", "Import Est.");
			this.ImportingEstNoCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 220, true);
			this.ImportingEstNoCodeFindBox.Name = "ImportingEstNoCodeFindBox";
			this.ImportingEstNoCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.ImportingEstNoCodeFindBox.TabIndex = 12;
			// 
			// DateOfInspectDateEdit
			// 
			this.DateOfInspectDateEdit.AllowDrop = true;
			this.DateOfInspectDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateOfInspectDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateOfInspectDateEdit, "FSISLines.US_DateOfInspection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_DateOfInspection)));
			this.DateOfInspectDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("cef61c56-d25c-4994-968b-d4e5d6575ed3", "Date Of Inspection");
			this.DateOfInspectDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 273, true);
			this.DateOfInspectDateEdit.Name = "DateOfInspectDateEdit";
			this.DateOfInspectDateEdit.TabIndex = 14;
			// 
			// CommercialDescMultiLineTextBox
			// 
			this.BindingSource.SetBindingMember(this.CommercialDescMultiLineTextBox, "FSISLines.US_CommercialDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_CommercialDescription)));
			this.CommercialDescMultiLineTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("594cc220-f25c-443e-8032-f94fa46c2397", "Description of Product");
			this.CommercialDescMultiLineTextBox.IsDynamicMultiline = true;
			this.CommercialDescMultiLineTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 37, true);
			this.CommercialDescMultiLineTextBox.Multiline = true;
			this.CommercialDescMultiLineTextBox.Name = "CommercialDescMultiLineTextBox";
			this.CommercialDescMultiLineTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 20, true);
			this.CommercialDescMultiLineTextBox.TabIndex = 5;
			// 
			// ProductIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.ProductIDTextBox, "FSISLines.US_ProductID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_ProductID)));
			this.ProductIDTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("55feb670-ef98-410c-8323-bb18921307c5", "Product ID");
			this.ProductIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 115, true);
			this.ProductIDTextBox.Name = "ProductIDTextBox";
			this.ProductIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ProductIDTextBox.TabIndex = 8;
			// 
			// CertificateNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CertificateNumberTextBox, "FSISLines.US_HealthCertificateNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_HealthCertificateNumber)));
			this.CertificateNumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("10f8d3da-34b8-4d03-ac8a-16b4c37ec4ef", "Health Certificate No.");
			this.CertificateNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 11, true);
			this.CertificateNumberTextBox.Name = "CertificateNumberTextBox";
			this.CertificateNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CertificateNumberTextBox.TabIndex = 4;
			// 
			// IntendedUseDropEdit
			// 
			this.IntendedUseDropEdit.AllowDrop = true;
			this.IntendedUseDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IntendedUseDropEdit, "FSISLines.US_IntendedUseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_IntendedUseCode)));
			this.IntendedUseDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("97a79a78-14fd-4223-9f2f-5acd08cdd8cd", "Intended Use");
			this.IntendedUseDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 168, true);
			this.IntendedUseDropEdit.Name = "IntendedUseDropEdit";
			this.IntendedUseDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(309, 20, true);
			this.IntendedUseDropEdit.TabIndex = 10;
			// 
			// ProductIDQualifierDropEdit
			// 
			this.ProductIDQualifierDropEdit.AllowDrop = true;
			this.ProductIDQualifierDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ProductIDQualifierDropEdit, "FSISLines.US_ProductIDQualifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_ProductIDQualifier)));
			this.ProductIDQualifierDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("eb4dee2f-c54b-497e-8deb-cae1dd4383e5", "Qualifier");
			this.ProductIDQualifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 141, true);
			this.ProductIDQualifierDropEdit.Name = "ProductIDQualifierDropEdit";
			this.ProductIDQualifierDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(309, 20, true);
			this.ProductIDQualifierDropEdit.TabIndex = 9;
			// 
			// CountryOfOriginCodeFindBox
			// 
			this.CountryOfOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfOriginCodeFindBox, "FSISLines.US_UC_NKCountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_UC_NKCountryOfOrigin)));
			this.CountryOfOriginCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e642224e-5244-43b2-aca5-c8f62cce3ac6", "Origin");
			this.CountryOfOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 89, true);
			this.CountryOfOriginCodeFindBox.Name = "CountryOfOriginCodeFindBox";
			this.CountryOfOriginCodeFindBox.PreBoundMaxLength = 2;
			this.CountryOfOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.CountryOfOriginCodeFindBox.TabIndex = 7;
			// 
			// IssuerCountryCodeFindBox
			// 
			this.IssuerCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IssuerCountryCodeFindBox, "FSISLines.US_UC_NKCertificateIssuerCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_UC_NKCertificateIssuerCountry)));
			this.IssuerCountryCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("73b6521d-6d63-470c-b124-13d7c0f1b8cd", "Issued");
			this.IssuerCountryCodeFindBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.IssuerCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 63, true);
			this.IssuerCountryCodeFindBox.Name = "IssuerCountryCodeFindBox";
			this.IssuerCountryCodeFindBox.PreBoundMaxLength = 2;
			this.IssuerCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.IssuerCountryCodeFindBox.TabIndex = 6;
			// 
			// sealNumberUserControl
			// 
			this.sealNumberUserControl.AllowDrop = true;
			this.sealNumberUserControl.BindTo = "FSISLines.US_SealNumbers";
			this.sealNumberUserControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c618b221-ac9d-46f1-97ab-9823951448c6", "Seals");
			this.sealNumberUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 246, true);
			this.sealNumberUserControl.Name = "sealNumberUserControl";
			this.sealNumberUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 21, true);
			this.sealNumberUserControl.TabIndex = 13;
			// 
			// ExportingEstNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportingEstNoTextBox, "FSISLines.US_ExportingEstNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_ExportingEstNo)));
			this.ExportingEstNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6edb93ed-ed72-4687-beeb-adc9f26c4252", "Exporting Est. No.");
			this.ExportingEstNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 194, true);
			this.ExportingEstNoTextBox.Name = "ExportingEstNoTextBox";
			this.ExportingEstNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.ExportingEstNoTextBox.TabIndex = 11;
			// 
			// PGAContactEmailTextBox
			// 
			this.BindingSource.SetBindingMember(this.PGAContactEmailTextBox, "FSISLines.US_PGAContactEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_PGAContactEmail)));
			this.PGAContactEmailTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("53711445-C489-4C63-A8D4-9C1D3C3DE00F", "PGA Contact Email");
			this.PGAContactEmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 376, true);
			this.PGAContactEmailTextBox.Name = "PGAContactEmailTextBox";
			this.PGAContactEmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.PGAContactEmailTextBox.TabIndex = 18;
			// 
			// PGAContactPhoneTextBox
			// 
			this.BindingSource.SetBindingMember(this.PGAContactPhoneTextBox, "FSISLines.US_PGAContactPhoneNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_PGAContactPhoneNo)));
			this.PGAContactPhoneTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("B81CECB2-2015-4AD8-A375-FBB87C5A1BDA", "PGA Contact Phone");
			this.PGAContactPhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 350, true);
			this.PGAContactPhoneTextBox.Name = "PGAContactPhoneTextBox";
			this.PGAContactPhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.PGAContactPhoneTextBox.TabIndex = 17;
			// 
			// PGAContantNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.PGAContantNameTextBox, "FSISLines.US_PGAContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_PGAContactName)));
			this.PGAContantNameTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("AF552607-0825-4B5F-AD49-F73E94CFE597", "PGA Contact Name");
			this.PGAContantNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 324, true);
			this.PGAContantNameTextBox.Name = "PGAContantNameTextBox";
			this.PGAContantNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.PGAContantNameTextBox.TabIndex = 16;
			// 
			// CertifyingIndividualDropEdit
			// 
			this.CertifyingIndividualDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertifyingIndividualDropEdit, "FSISLines.US_CertifyingIndividual");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.USDeclarationFSISLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FSISLines)).SyncRoot)).US_CertifyingIndividual)));
			this.CertifyingIndividualDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9DE9FE44-4217-4C1A-8C48-793810D5FC5A", "Certifying Individual");
			this.CertifyingIndividualDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 298, true);
			this.CertifyingIndividualDropEdit.Name = "CertifyingIndividualDropEdit";
			this.CertifyingIndividualDropEdit.PreBoundMaxLength = 3;
			this.CertifyingIndividualDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.CertifyingIndividualDropEdit.TabIndex = 15;
			// 
			// FSISDeclarationForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 524, true);
			this.Controls.Add(this.FSISSplitContainer);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.JobDeclaration);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(662, 563, true);
			this.Name = "FSISDeclarationForm";
			this.Text = "FSIS Certificates";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.FSISSplitContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FSISSplitContainer.Panel1.ResumeLayout(false);
			this.FSISSplitContainer.Panel2.ResumeLayout(false);
			this.FSISSplitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FSISSplitContainer)).EndInit();
			this.FSISSplitContainer.ResumeLayout(false);
			this.FSISSplitContainer.PerformLayout();
			this.CertificateGroupBox.ResumeLayout(false);
			this.CertificateGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FSISCertificatesGrid)).EndInit();
			this.FSISCertificatesGrid.ResumeLayout(false);
			this.FSISCertificatesGrid.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.DateOfInspectDateEdit.ResumeLayout(true);
			this.DateOfInspectDateEdit.PerformLayout();
			this.IntendedUseDropEdit.ResumeLayout(true);
			this.IntendedUseDropEdit.PerformLayout();
			this.ProductIDQualifierDropEdit.ResumeLayout(true);
			this.ProductIDQualifierDropEdit.PerformLayout();
			this.CountryOfOriginCodeFindBox.ResumeLayout(true);
			this.CountryOfOriginCodeFindBox.PerformLayout();
			this.IssuerCountryCodeFindBox.ResumeLayout(true);
			this.IssuerCountryCodeFindBox.PerformLayout();
			this.sealNumberUserControl.ResumeLayout(true);
			this.sealNumberUserControl.PerformLayout();
			this.CertifyingIndividualDropEdit.ResumeLayout(true);
			this.CertifyingIndividualDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZButton OKButton;
		private CargoWise.Windows.UI.KSplitContainer FSISSplitContainer;
		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.GUI.ZDropEdit IntendedUseDropEdit;
		private ZArchitecture.GUI.ZDropEdit ProductIDQualifierDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox CountryOfOriginCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox IssuerCountryCodeFindBox;
		private SealNumberUserControl sealNumberUserControl;
		private ZArchitecture.ZTextBox ExportingEstNoTextBox;
		private ZArchitecture.GUI.ZGroupBox CertificateGroupBox;
		private ZArchitecture.ZTextBox CertificateNumberTextBox;
		public ZArchitecture.ZGrid FSISCertificatesGrid;
		private ZArchitecture.ZTextBox ProductIDTextBox;
		private ZArchitecture.ZTextBox CommercialDescMultiLineTextBox;
		protected ZArchitecture.GUI.ZDateEdit DateOfInspectDateEdit;
		private ZArchitecture.GUI.ZCodeFindBox ImportingEstNoCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit CertifyingIndividualDropEdit;
		internal ZArchitecture.ZTextBox PGAContactEmailTextBox;
		internal ZArchitecture.ZTextBox PGAContactPhoneTextBox;
		internal ZArchitecture.ZTextBox PGAContantNameTextBox;
	}
}

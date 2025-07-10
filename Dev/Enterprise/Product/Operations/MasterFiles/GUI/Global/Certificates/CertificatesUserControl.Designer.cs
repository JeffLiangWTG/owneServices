using System.Windows.Forms;

namespace Enterprise.MasterFiles.GUI
{
	using Enterprise.ZArchitecture;

	partial class CertificatesUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.CertificateDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.IssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CertificateTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.CommentsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertificateNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SplitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.CountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.StateCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CertificatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CertificatesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CertificateDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).BeginInit();
			this.SplitContainer1.Panel1.SuspendLayout();
			this.SplitContainer1.Panel2.SuspendLayout();
			this.SplitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer2)).BeginInit();
			this.SplitContainer2.Panel1.SuspendLayout();
			this.SplitContainer2.Panel2.SuspendLayout();
			this.SplitContainer2.SuspendLayout();
			this.CertificatesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CertificatesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ICertificatesProvider);
			// 
			// CertificateDetailsGroupBox
			// 
			this.CertificateDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("48191c7f-0306-4f4c-a5be-ba753a4947e7", "Certificate Details");
			this.CertificateDetailsGroupBox.Controls.Add(this.ExpiryDateEdit);
			this.CertificateDetailsGroupBox.Controls.Add(this.IssueDateEdit);
			this.CertificateDetailsGroupBox.Controls.Add(this.CertificateTypeDropEdit);
			this.CertificateDetailsGroupBox.Controls.Add(this.SplitContainer1);
			this.CertificateDetailsGroupBox.Controls.Add(this.SplitContainer2);
			this.CertificateDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.CertificateDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 112, true);
			this.CertificateDetailsGroupBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 70, true);
			this.CertificateDetailsGroupBox.Name = "CertificateDetailsGroupBox";
			this.CertificateDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 70, true);
			this.CertificateDetailsGroupBox.TabIndex = 1;
			this.CertificateDetailsGroupBox.TabStop = false;
			// 
			// ExpiryDateEdit
			// 
			this.ExpiryDateEdit.AllowDrop = true;
			this.ExpiryDateEdit.AutoCompleteMonthThreshold = 1;
			this.ExpiryDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ExpiryDateEdit, "Certificates.XZ_ExpiryOrDueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GenRegCertAccredMaintList)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ICertificatesProvider)(null)).Certificates)).SyncRoot)).XZ_ExpiryOrDueDate)));
			this.ExpiryDateEdit.CaptionResourceString = null;
			this.ExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 16, true);
			this.ExpiryDateEdit.Name = "ExpiryDateEdit";
			this.ExpiryDateEdit.TabIndex = 1;
			// 
			// IssueDateEdit
			// 
			this.IssueDateEdit.AllowDrop = true;
			this.IssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.IssueDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.IssueDateEdit, "Certificates.XZ_IssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GenRegCertAccredMaintList)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ICertificatesProvider)(null)).Certificates)).SyncRoot)).XZ_IssueDate)));
			this.IssueDateEdit.CaptionResourceString = null;
			this.IssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 43, true);
			this.IssueDateEdit.Name = "IssueDateEdit";
			this.IssueDateEdit.TabIndex = 2;
			// 
			// CertificateTypeDropEdit
			// 
			this.CertificateTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateTypeDropEdit, "Certificates.XZ_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GenRegCertAccredMaintList)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ICertificatesProvider)(null)).Certificates)).SyncRoot)).XZ_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GenRegCertAccredMaintList)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ICertificatesProvider)(null)).Certificates)).SyncRoot)).XZ_Comment)));
			this.CertificateTypeDropEdit.BindToForDescription = "Certificates.XZ_Comment";
			this.CertificateTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("85eae403-8996-438b-939c-b5cc2f6c3afe", "Type", "Certificate type.");
			this.CertificateTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 16, true);
			this.CertificateTypeDropEdit.Name = "CertificateTypeDropEdit";
			this.CertificateTypeDropEdit.PreBoundMaxLength = 3;
			this.CertificateTypeDropEdit.ShowDescriptionBox = false;
			this.CertificateTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.CertificateTypeDropEdit.TabIndex = 0;
			// 
			// SplitContainer1
			// 
			this.SplitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SplitContainer1.IsSplitterFixed = true;
			this.SplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 13, true);
			this.SplitContainer1.Name = "SplitContainer1";
			// 
			// SplitContainer1.Panel1
			// 
			this.SplitContainer1.Panel1.Controls.Add(this.CommentsTextBox);
			// 
			// SplitContainer1.Panel2
			// 
			this.SplitContainer1.Panel2.Controls.Add(this.CertificateNumberTextBox);
			this.SplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 26, true);
			this.SplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(205);
			this.SplitContainer1.TabIndex = 0;
			// 
			// DescriptionTextBox
			// 
			this.CommentsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CommentsTextBox, "Certificates.XZ_Comment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GenRegCertAccredMaintList)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ICertificatesProvider)(null)).Certificates)).SyncRoot)).XZ_Comment)));
			this.CommentsTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b0766db3-e255-48ff-aa41-1e0b0454ddc2", "Comments");
			this.CommentsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 3, true);
			this.CommentsTextBox.Name = "DescriptionTextBox";
			this.CommentsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 20, true);
			this.CommentsTextBox.TabIndex = 1;
			CommentsTextBox.CharacterCasing = CharacterCasing.Normal;
			// 
			// CertificateNumberTextBox
			// 
			this.CertificateNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CertificateNumberTextBox, "Certificates.XZ_RefNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GenRegCertAccredMaintList)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ICertificatesProvider)(null)).Certificates)).SyncRoot)).XZ_RefNumber)));
			this.CertificateNumberTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8f0f522f-b175-4a60-92cc-e58b150b22a0", "Number", "Certificate Number", "");
			this.CertificateNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 3, true);
			this.CertificateNumberTextBox.Name = "CertificateNumberTextBox";
			this.CertificateNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 20, true);
			this.CertificateNumberTextBox.TabIndex = 0;
			CertificateNumberTextBox.CharacterCasing = CharacterCasing.Normal;
			// 
			// SplitContainer2
			// 
			this.SplitContainer2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SplitContainer2.IsSplitterFixed = true;
			this.SplitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 40, true);
			this.SplitContainer2.Name = "SplitContainer2";
			// 
			// SplitContainer2.Panel1
			// 
			this.SplitContainer2.Panel1.Controls.Add(this.CountryCodeFindBox);
			// 
			// SplitContainer2.Panel2
			// 
			this.SplitContainer2.Panel2.Controls.Add(this.StateCodeDropEdit);
			this.SplitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 26, true);
			this.SplitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(205);
			this.SplitContainer2.TabIndex = 2;
			// 
			// CountryCodeFindBox
			// 
			this.CountryCodeFindBox.AllowDrop = true;
			this.CountryCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CountryCodeFindBox, "Certificates.XZ_RN_NKCountryOfIssuance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GenRegCertAccredMaintList)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ICertificatesProvider)(null)).Certificates)).SyncRoot)).XZ_RN_NKCountryOfIssuance)));
			this.CountryCodeFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f6f868b4-c385-4e86-abbe-c49d573fc194", "Ctry/Rgn.", "Country/Region", "Certificate country/region of issuance.");
			this.CountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 3, true);
			this.CountryCodeFindBox.Name = "CountryCodeFindBox";
			this.CountryCodeFindBox.PreBoundMaxLength = 2;
			this.CountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 20, true);
			this.CountryCodeFindBox.TabIndex = 0;
			// 
			// StateCodeDropEdit
			// 
			this.StateCodeDropEdit.AllowDrop = true;
			this.StateCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.StateCodeDropEdit, "Certificates.XZ_StateOrProvinceOfIssuance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GenRegCertAccredMaintList)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ICertificatesProvider)(null)).Certificates)).SyncRoot)).XZ_StateOrProvinceOfIssuance)));
			this.StateCodeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("007c4667-97d0-42fd-9460-43d37965dd79", "State", "Certificate state/province of issuance.");
			this.StateCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 3, true);
			this.StateCodeDropEdit.Name = "StateCodeDropEdit";
			this.StateCodeDropEdit.PreBoundMaxLength = 3;
			this.StateCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 20, true);
			this.StateCodeDropEdit.TabIndex = 0;
			// 
			// CertificatesGroupBox
			// 
			this.CertificatesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("53ff0737-9877-4e9b-b91e-620b2ea0be1c", "Certificates and Identification");
			this.CertificatesGroupBox.Controls.Add(this.CertificatesGrid);
			this.CertificatesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CertificatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CertificatesGroupBox.Name = "CertificatesGroupBox";
			this.CertificatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 112, true);
			this.CertificatesGroupBox.TabIndex = 0;
			this.CertificatesGroupBox.TabStop = false;
			// 
			// CertificatesGrid
			// 
			this.CertificatesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CertificatesGrid, "Certificates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ICertificatesProvider)(null)).Certificates)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GenRegCertAccredMaintList)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ICertificatesProvider)(null)).Certificates)).SyncRoot)).XZ_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GenRegCertAccredMaintList)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ICertificatesProvider)(null)).Certificates)).SyncRoot)).XZ_Comment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GenRegCertAccredMaintList)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ICertificatesProvider)(null)).Certificates)).SyncRoot)).XZ_RefNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.GenRegCertAccredMaintList)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ICertificatesProvider)(null)).Certificates)).SyncRoot)).XZ_ExpiryOrDueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GenRegCertAccredMaintList)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ICertificatesProvider)(null)).Certificates)).SyncRoot)).XZ_RN_NKCountryOfIssuance)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GenRegCertAccredMaintList)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ICertificatesProvider)(null)).Certificates)).SyncRoot)).XZ_StateOrProvinceOfIssuance)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.GenRegCertAccredMaintList)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ICertificatesProvider)(null)).Certificates)).SyncRoot)).XZ_IssueDate)));
			this.CertificatesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = null;
			zDropEditColumnStyleInfo1.CaptionResourceString = null;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "XZ_Type";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ca962410-4b9b-4d25-b140-0219a4a7e699", "Type Description"); ;
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo3.ColumnName = "XZ_TypeDescription";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d9c0e1ee-8fc0-4a42-b19d-68d3aac5d03d", "Comments");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo1.ColumnName = "XZ_Comment";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = null;
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo2.ColumnName = "XZ_RefNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = null;
			zDateEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo1.ColumnName = "XZ_ExpiryOrDueDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zCodeFindBoxColumnStyleInfo1.Caption = null;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ddcc92b9-9e69-4ba9-b700-ab7f17f8e6eb", "Ctry/Rgn.", "Country/Region", "Country/Region Of Issuance", "");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "XZ_RN_NKCountryOfIssuance";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zDropEditColumnStyleInfo2.Caption = null;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("25fcd8e5-aaa6-4297-89ce-a7fc644835ff", "State", "State Or Province", "State Or Province Of Issuance", "");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "XZ_StateOrProvinceOfIssuance";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zDateEditColumnStyleInfo2.Caption = null;
			zDateEditColumnStyleInfo2.CaptionResourceString = null;
			zDateEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo2.ColumnName = "XZ_IssueDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.CertificatesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CertificatesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CertificatesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CertificatesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CertificatesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.CertificatesGrid.CopySelectedRowsAllowed = true;
			this.CertificatesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CertificatesGrid.GridId = "c7ac9746-ac40-4fc7-9a1c-bed11c3f00c0";
			this.CertificatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CertificatesGrid.LayoutKey = "CertificatesGrid";
			this.CertificatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CertificatesGrid.Name = "CertificatesGrid";
			this.CertificatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 93, true);
			this.CertificatesGrid.TabIndex = 0;
			// 
			// CertificatesUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CertificatesGroupBox);
			this.Controls.Add(this.CertificateDetailsGroupBox);
			this.Name = "CertificatesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 182, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CertificateDetailsGroupBox.ResumeLayout(false);
			this.SplitContainer1.Panel1.ResumeLayout(false);
			this.SplitContainer1.Panel1.PerformLayout();
			this.SplitContainer1.Panel2.ResumeLayout(false);
			this.SplitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).EndInit();
			this.SplitContainer1.ResumeLayout(false);
			this.SplitContainer2.Panel1.ResumeLayout(false);
			this.SplitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer2)).EndInit();
			this.SplitContainer2.ResumeLayout(false);
			this.CertificatesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CertificatesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		ZArchitecture.GUI.ZGroupBox CertificateDetailsGroupBox;
		ZArchitecture.GUI.ZDropEdit CertificateTypeDropEdit;
		ZArchitecture.GUI.ZGroupBox CertificatesGroupBox;
		internal ZArchitecture.ZGrid CertificatesGrid;
		ZArchitecture.GUI.ZDropEdit StateCodeDropEdit;
		ZArchitecture.GUI.ZCodeFindBox CountryCodeFindBox;
		ZArchitecture.GUI.ZDateEdit ExpiryDateEdit;
		ZArchitecture.GUI.ZDateEdit IssueDateEdit;
		CargoWise.Windows.UI.KSplitContainer SplitContainer2;
		private CargoWise.Windows.UI.KSplitContainer SplitContainer1;
		private ZArchitecture.ZTextBox CertificateNumberTextBox;
		private ZArchitecture.ZTextBox CommentsTextBox;
		ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1;
		ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2;
	}
}

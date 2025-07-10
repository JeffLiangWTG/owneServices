using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class TemporaryOrganisationsPopup
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.NameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PortFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.EmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FaxTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Address1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Address2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SimilarOrgMatchesBoundGrid = new Enterprise.ZArchitecture.GUI.ZDisplayGrid();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SimilarOrganisationsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BusinessRegNZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SearchResultsStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CountryFindBox = new Enterprise.MasterFiles.GUI.Internal.ZCodeFindBoxFixedPreBoundMaxLength();
			this.CityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StateDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.oGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PayablesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ReceivablesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ConsignorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ConsigneeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SalesLeadCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OM_OJ_ARDebtorGroupGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OM_OG_APCreditorGroupGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ValidateAddressButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PortFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SimilarOrgMatchesBoundGrid)).BeginInit();
			this.CountryFindBox.SuspendLayout();
			this.SimilarOrgMatchesBoundGrid.SuspendLayout();
			this.StateDropDownEdit.SuspendLayout();
			this.oGroupBox1.SuspendLayout();
			this.OM_OJ_ARDebtorGroupGuidFindBox.SuspendLayout();
			this.OM_OG_APCreditorGroupGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 427, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 24, true);
			this.MainStatusBar.TabIndex = 33;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Integration.IOrgHeader);
			// 
			// NameTextBox
			// 
			this.BindingSource.SetBindingMember(this.NameTextBox, "OH_FullName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IOrgHeader)(null)).OH_FullName)));
			this.NameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 8, true);
			this.NameTextBox.Name = "NameTextBox";
			this.NameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.NameTextBox.TabIndex = 1;
			// 
			// PortFindBox
			// 
			this.PortFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortFindBox, "OH_RL_NKClosestPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IOrgHeader)(null)).OH_RL_NKClosestPort)));
			this.PortFindBox.BindToList = "Lookups.ClosestPorts";
			this.PortFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 152, true);
			this.PortFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.PortFindBox.Name = "PortFindBox";
			this.PortFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.PortFindBox.TabIndex = 14;
			// 
			// EmailTextBox
			// 
			this.BindingSource.SetBindingMember(this.EmailTextBox, "MainAddress.OA_Email");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IOrgHeader)(null)).MainAddress.OA_Email)));
			this.EmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(456, 56, true);
			this.EmailTextBox.Name = "EmailTextBox";
			this.EmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.EmailTextBox.TabIndex = 23;
			// 
			// FaxTextBox
			// 
			this.BindingSource.SetBindingMember(this.FaxTextBox, "MainAddress.OA_Fax_Formatted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.FaxTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(456, 32, true);
			this.FaxTextBox.Name = "FaxTextBox";
			this.FaxTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.FaxTextBox.TabIndex = 21;
			// 
			// PhoneTextBox
			// 
			this.BindingSource.SetBindingMember(this.PhoneTextBox, "MainAddress.OA_Phone_Formatted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.PhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(456, 8, true);
			this.PhoneTextBox.Name = "PhoneTextBox";
			this.PhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.PhoneTextBox.TabIndex = 19;
			// 
			// Address1TextBox
			// 
			this.BindingSource.SetBindingMember(this.Address1TextBox, "MainAddress.OA_Address1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IOrgHeader)(null)).MainAddress.OA_Address1)));
			this.Address1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 32, true);
			this.Address1TextBox.Name = "Address1TextBox";
			this.Address1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.Address1TextBox.TabIndex = 5;
			this.Address1TextBox.TextChanged += AddressTextBox_TextChanged;
			// 
			// Address2TextBox
			// 
			this.BindingSource.SetBindingMember(this.Address2TextBox, "MainAddress.OA_Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IOrgHeader)(null)).MainAddress.OA_Address2)));
			this.Address2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 56, true);
			this.Address2TextBox.Name = "Address2TextBox";
			this.Address2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.Address2TextBox.TabIndex = 6;
			this.Address2TextBox.TextChanged += AddressTextBox_TextChanged;
			// 
			// SimilarOrgMatchesBoundGrid
			// 
			this.SimilarOrgMatchesBoundGrid.AllowNavigation = false;
			this.SimilarOrgMatchesBoundGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SimilarOrgMatchesBoundGrid, "SimilarOrgMatches");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.SimilarOrgMatchesBoundGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrganisationsPopup|d5228b7d-b31f-4f32-a987-632911243e70", "Rank");
			zCalcEditColumnStyleInfo1.ColumnName = "OS_Rank";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrganisationsPopup|4bfc9496-b604-479f-904b-d40dddfcf3fa", "Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "OH_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrganisationsPopup|08ed70d8-1ba0-42c4-94ce-e94f2adfc7b5", "Full Name");
			zTextBoxColumnStyleInfo2.ColumnName = "OH_FullName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.ColumnName = "OS_UNLOCO";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrganisationsPopup|962b6f15-d87f-46ef-84a4-d7de2746f492", "Street", "Address 1", "");
			zTextBoxColumnStyleInfo4.ColumnName = "OH_Calc_Address1";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrganisationsPopup|e8b3067d-5339-4f8e-8f16-340c67bd78b4", "Street 2", "Address 2", "");
			zTextBoxColumnStyleInfo5.ColumnName = "OH_Calc_Address2";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrganisationsPopup|02fe7675-820f-4c08-8308-a66549947981", "City", "City", "");
			zTextBoxColumnStyleInfo6.ColumnName = "OH_Calc_City";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrganisationsPopup|77f17af5-0862-4701-a52a-7552a393949d", "P. Code", "Post Code", "");
			zTextBoxColumnStyleInfo7.ColumnName = "OH_Calc_PostCode";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrganisationsPopup|a33d3c79-8885-419a-949f-683c47dea83c", "Phone", "Phone", "");
			zTextBoxColumnStyleInfo8.ColumnName = "OH_Calc_Phone";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrganisationsPopup|bd4b588e-9f40-4744-8fee-c0c0e13955d8", "Fax", "Fax", "");
			zTextBoxColumnStyleInfo9.ColumnName = "OH_Calc_Fax";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrganisationsPopup|16f6a8de-8e6b-4096-bc2c-6924afe8910b", "Email", "Email", "");
			zTextBoxColumnStyleInfo10.ColumnName = "OH_Calc_Email";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrganisationsPopup|2f1fd835-3b26-401c-9e9b-63a5b37d7ea6", "Reg. #", "Business Reg No.", "");
			zTextBoxColumnStyleInfo11.ColumnName = "LocalBusinessNumber";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrganisationsPopup|c8e76da3-7f99-4d22-b372-94989344a427", "State", "State", "");
			zTextBoxColumnStyleInfo12.ColumnName = "OH_Calc_State";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("41de0c81-e36e-4a8c-af0c-0875c6285eb2", "Address Types");
			zTextBoxColumnStyleInfo13.ColumnName = "AddressCapabilityCodes";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.SimilarOrgMatchesBoundGrid.CopySelectedRowsAllowed = true;
			this.SimilarOrgMatchesBoundGrid.GridId = "e4e5fe12-4b12-4e95-a5d1-09472f4eb7e4";
			this.SimilarOrgMatchesBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SimilarOrgMatchesBoundGrid.IsWholeRowSelectedOnClick = true;
			this.SimilarOrgMatchesBoundGrid.LayoutKey = "zGrid1";
			this.SimilarOrgMatchesBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 224, true);
			this.SimilarOrgMatchesBoundGrid.Name = "SimilarOrgMatchesBoundGrid";
			this.SimilarOrgMatchesBoundGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.SimilarOrgMatchesBoundGrid.ShouldSetErrorsOnTabPage = false;
			this.SimilarOrgMatchesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 160, true);
			this.SimilarOrgMatchesBoundGrid.TabIndex = 28;
			this.SimilarOrgMatchesBoundGrid.DoubleClick += new System.EventHandler(this.SimilarOrgMatchesBoundGrid_DoubleClick);
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 392, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.SaveButton.TabIndex = 30;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrganisationsPopup|fc5652f4-d6ef-4f7a-bcb5-cd35d8b7cb16", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(474, 392, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 31;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CancelBtn
			// 
			this.CancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(551, 392, true);
			this.CancelBtn.Name = "CancelBtn";
			this.CancelBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelBtn.TabIndex = 32;
			// 
			// SimilarOrganisationsLabel
			// 
			this.SimilarOrganisationsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrganisationsPopup|fabba857-9854-457e-a16d-f32f13f24ceb", "Similar Organizations");
			this.SimilarOrganisationsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 208, true);
			this.SimilarOrganisationsLabel.Name = "SimilarOrganisationsLabel";
			this.SimilarOrganisationsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 13, true);
			this.SimilarOrganisationsLabel.TabIndex = 27;
			// 
			// PostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.PostCodeTextBox, "MainAddress.OA_PostCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IOrgHeader)(null)).MainAddress.OA_PostCode)));
			this.PostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 104, true);
			this.PostCodeTextBox.Name = "PostCodeTextBox";
			this.PostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.PostCodeTextBox.TabIndex = 10;
			this.PostCodeTextBox.TextChanged += AddressTextBox_TextChanged;
			// 
			// BusinessRegNZTextBox
			// 
			this.BindingSource.SetBindingMember(this.BusinessRegNZTextBox, "PrimaryRegistrationNumber+Number");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.BusinessRegNZTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrganisationsPopup|d1f35bc7-d100-4d43-b0ee-682b4ee549d9", "Business Reg No.");
			this.BusinessRegNZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(456, 80, true);
			this.BusinessRegNZTextBox.Name = "BusinessRegNZTextBox";
			this.BusinessRegNZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.BusinessRegNZTextBox.TabIndex = 25;
			// 
			// SearchResultsStatusLabel
			// 
			this.SearchResultsStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SearchResultsStatusLabel.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.SearchResultsStatusLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrganisationsPopup|166a4571-370f-4399-b93a-5694d0ec1d4f", "None Found");
			this.SearchResultsStatusLabel.ForeColor = System.Drawing.SystemColors.Menu;
			this.SearchResultsStatusLabel.IsFontBold = true;
			this.SearchResultsStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 247, true);
			this.SearchResultsStatusLabel.Name = "SearchResultsStatusLabel";
			this.SearchResultsStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(614, 17, true);
			this.SearchResultsStatusLabel.TabIndex = 29;
			this.SearchResultsStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// CountryFindBox
			// 
			this.CountryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryFindBox, "MainAddress.OA_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IOrgHeader)(null)).MainAddress.OA_RN_NKCountryCode)));
			this.CountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 80, true);
			this.CountryFindBox.Name = "CountryFindBox";
			this.CountryFindBox.PreBoundMaxLength = 3;
			this.CountryFindBox.ShowDescriptionBox = false;
			this.CountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 17, true);
			this.CountryFindBox.TabIndex = 7;
			this.CountryFindBox.CodeBox.TextChanged += AddressTextBox_TextChanged;
			// 
			// CityTextBox
			// 
			this.BindingSource.SetBindingMember(this.CityTextBox, "MainAddress.OA_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IOrgHeader)(null)).MainAddress.OA_City)));
			this.CityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(172, 80, true);
			this.CityTextBox.Name = "CityTextBox";
			this.CityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.CityTextBox.TabIndex = 8;
			this.CityTextBox.TextChanged += AddressTextBox_TextChanged;
			// 
			// StateDropDownEdit
			// 
			this.StateDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StateDropDownEdit, "MainAddress.OA_State");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Integration.IOrgHeader)(null)).MainAddress.OA_State)));
			this.StateDropDownEdit.BindToList = "MainAddress.OA_State_List";
			this.StateDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 128, true);
			this.StateDropDownEdit.Name = "StateDropDownEdit";
			this.StateDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.StateDropDownEdit.TabIndex = 11;
			this.StateDropDownEdit.CodeBox.TextChanged += AddressTextBox_TextChanged;
			// 
			// oGroupBox1
			// 
			this.oGroupBox1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrganisationsPopup|6f429710-5e1a-4d32-a8e3-6455a959a392", "Organization Type");
			this.oGroupBox1.Controls.Add(this.PayablesCheckBox);
			this.oGroupBox1.Controls.Add(this.ReceivablesCheckBox);
			this.oGroupBox1.Controls.Add(this.ConsignorCheckBox);
			this.oGroupBox1.Controls.Add(this.ConsigneeCheckBox);
			this.oGroupBox1.Controls.Add(this.SalesLeadCheckBox);
			this.oGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 112, true);
			this.oGroupBox1.Name = "oGroupBox1";
			this.oGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 88, true);
			this.oGroupBox1.TabIndex = 26;
			this.oGroupBox1.TabStop = false;
			// 
			// PayablesCheckBox
			// 
			this.PayablesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PayablesCheckBox, "OH_IsCreditor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Integration.IOrgHeader)(null)).OH_IsCreditor)));
			this.PayablesCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrganisationsPopup|d1b37da3-3997-49cd-8987-27324924c203", "Payables");
			this.PayablesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PayablesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 56, true);
			this.PayablesCheckBox.Name = "PayablesCheckBox";
			this.PayablesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.PayablesCheckBox.TabIndex = 3;
			// 
			// ReceivablesCheckBox
			// 
			this.ReceivablesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ReceivablesCheckBox, "OH_IsDebtor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Integration.IOrgHeader)(null)).OH_IsDebtor)));
			this.ReceivablesCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrganisationsPopup|7db7022a-3c84-4d65-950c-6d815c2dd4e7", "Receivables");
			this.ReceivablesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReceivablesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 24, true);
			this.ReceivablesCheckBox.Name = "ReceivablesCheckBox";
			this.ReceivablesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ReceivablesCheckBox.TabIndex = 1;
			// 
			// ConsignorCheckBox
			// 
			this.ConsignorCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ConsignorCheckBox, "OH_IsConsignor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Integration.IOrgHeader)(null)).OH_IsConsignor)));
			this.ConsignorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ConsignorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.ConsignorCheckBox.Name = "ConsignorCheckBox";
			this.ConsignorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ConsignorCheckBox.TabIndex = 2;
			// 
			// ConsigneeCheckBox
			// 
			this.ConsigneeCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ConsigneeCheckBox, "OH_IsConsignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Integration.IOrgHeader)(null)).OH_IsConsignee)));
			this.ConsigneeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ConsigneeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 24, true);
			this.ConsigneeCheckBox.Name = "ConsigneeCheckBox";
			this.ConsigneeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ConsigneeCheckBox.TabIndex = 0;
			// 
			// SalesLeadCheckBox
			// 
			this.SalesLeadCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SalesLeadCheckBox, "OH_IsSalesLead");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Integration.IOrgHeader)(null)).OH_IsSalesLead)));
			this.SalesLeadCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SalesLeadCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 24, true);
			this.SalesLeadCheckBox.Name = "SalesLeadCheckBox";
			this.SalesLeadCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.SalesLeadCheckBox.TabIndex = 4;
			// 
			// OM_OJ_ARDebtorGroupGuidFindBox
			// 
			this.OM_OJ_ARDebtorGroupGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_OJ_ARDebtorGroupGuidFindBox, "MiscServ.OM_OJ_ARDebtorGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.OM_OJ_ARDebtorGroupGuidFindBox.BindToList = "MiscServ.DebtorGroups";
			this.OM_OJ_ARDebtorGroupGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrganisationsPopup|588ea6c3-3ecf-45b2-a42f-a8f3bf8fd270", "Debtor Group");
			this.OM_OJ_ARDebtorGroupGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 176, true);
			this.OM_OJ_ARDebtorGroupGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgDebtorGroup;
			this.OM_OJ_ARDebtorGroupGuidFindBox.Name = "OM_OJ_ARDebtorGroupGuidFindBox";
			this.OM_OJ_ARDebtorGroupGuidFindBox.PreBoundMaxLength = 3;
			this.OM_OJ_ARDebtorGroupGuidFindBox.ShowDescriptionBox = false;
			this.OM_OJ_ARDebtorGroupGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.OM_OJ_ARDebtorGroupGuidFindBox.TabIndex = 15;
			// 
			// OM_OG_APCreditorGroupGuidFindBox
			// 
			this.OM_OG_APCreditorGroupGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_OG_APCreditorGroupGuidFindBox, "MiscServ.OM_OG_APCreditorGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.OM_OG_APCreditorGroupGuidFindBox.BindToList = "MiscServ.CreditorGroups";
			this.OM_OG_APCreditorGroupGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrganisationsPopup|26ba2dd4-5807-431f-85d1-5d122178fc79", "Creditor Group");
			this.OM_OG_APCreditorGroupGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 176, true);
			this.OM_OG_APCreditorGroupGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgCreditorGroup;
			this.OM_OG_APCreditorGroupGuidFindBox.Name = "OM_OG_APCreditorGroupGuidFindBox";
			this.OM_OG_APCreditorGroupGuidFindBox.PreBoundMaxLength = 3;
			this.OM_OG_APCreditorGroupGuidFindBox.ShowDescriptionBox = false;
			this.OM_OG_APCreditorGroupGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.OM_OG_APCreditorGroupGuidFindBox.TabIndex = 17;
			// 
			// ValidateAddressButton
			// 
			this.ValidateAddressButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(323, 31, true);
			this.ValidateAddressButton.Name = "ValidateAddressButton";
			this.ValidateAddressButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ValidateAddressButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 22, true);
			this.ValidateAddressButton.TabIndex = 39;
			this.ValidateAddressButton.UseVisualStyleBackColor = true;
			this.ValidateAddressButton.Click += new System.EventHandler(this.ValidateAddressButton_Click);

			// 
			// TemporaryOrganisationsPopup
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrganisationsPopup|d282db7a-0069-41f6-95c3-d8a36ba3a4fd", "Temporary Organization");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 451, true);
			this.Controls.Add(this.ValidateAddressButton);
			this.Controls.Add(this.OM_OG_APCreditorGroupGuidFindBox);
			this.Controls.Add(this.OM_OJ_ARDebtorGroupGuidFindBox);
			this.Controls.Add(this.oGroupBox1);
			this.Controls.Add(this.StateDropDownEdit);
			this.Controls.Add(this.CountryFindBox);
			this.Controls.Add(this.CityTextBox);
			this.Controls.Add(this.SearchResultsStatusLabel);
			this.Controls.Add(this.BusinessRegNZTextBox);
			this.Controls.Add(this.PostCodeTextBox);
			this.Controls.Add(this.SimilarOrganisationsLabel);
			this.Controls.Add(this.CancelBtn);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.SimilarOrgMatchesBoundGrid);
			this.Controls.Add(this.Address2TextBox);
			this.Controls.Add(this.Address1TextBox);
			this.Controls.Add(this.PhoneTextBox);
			this.Controls.Add(this.FaxTextBox);
			this.Controls.Add(this.EmailTextBox);
			this.Controls.Add(this.PortFindBox);
			this.Controls.Add(this.NameTextBox);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Integration.IOrgHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 480, true);
			this.Name = "TemporaryOrganisationsPopup";
			this.Controls.SetChildIndex(this.NameTextBox, 0);
			this.Controls.SetChildIndex(this.PortFindBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.EmailTextBox, 0);
			this.Controls.SetChildIndex(this.FaxTextBox, 0);
			this.Controls.SetChildIndex(this.PhoneTextBox, 0);
			this.Controls.SetChildIndex(this.Address1TextBox, 0);
			this.Controls.SetChildIndex(this.Address2TextBox, 0);
			this.Controls.SetChildIndex(this.SimilarOrgMatchesBoundGrid, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CancelBtn, 0);
			this.Controls.SetChildIndex(this.SimilarOrganisationsLabel, 0);
			this.Controls.SetChildIndex(this.PostCodeTextBox, 0);
			this.Controls.SetChildIndex(this.BusinessRegNZTextBox, 0);
			this.Controls.SetChildIndex(this.SearchResultsStatusLabel, 0);
			this.Controls.SetChildIndex(this.CityTextBox, 0);
			this.Controls.SetChildIndex(this.StateDropDownEdit, 0);
			this.Controls.SetChildIndex(this.oGroupBox1, 0);
			this.Controls.SetChildIndex(this.OM_OJ_ARDebtorGroupGuidFindBox, 0);
			this.Controls.SetChildIndex(this.OM_OG_APCreditorGroupGuidFindBox, 0);
			this.Controls.SetChildIndex(this.ValidateAddressButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PortFindBox.ResumeLayout(true);
			this.PortFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SimilarOrgMatchesBoundGrid)).EndInit();
			this.SimilarOrgMatchesBoundGrid.ResumeLayout(false);
			this.SimilarOrgMatchesBoundGrid.PerformLayout();
			this.StateDropDownEdit.ResumeLayout(true);
			this.StateDropDownEdit.PerformLayout();
			this.CountryFindBox.ResumeLayout(true);
			this.CountryFindBox.PerformLayout();
			this.oGroupBox1.ResumeLayout(false);
			this.oGroupBox1.PerformLayout();
			this.OM_OJ_ARDebtorGroupGuidFindBox.ResumeLayout(true);
			this.OM_OJ_ARDebtorGroupGuidFindBox.PerformLayout();
			this.OM_OG_APCreditorGroupGuidFindBox.ResumeLayout(true);
			this.OM_OG_APCreditorGroupGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZTextBox NameTextBox;
		ZCodeFindBox PortFindBox;
		Enterprise.ZArchitecture.ZTextBox EmailTextBox;
		Enterprise.ZArchitecture.ZTextBox FaxTextBox;
		Enterprise.ZArchitecture.ZTextBox PhoneTextBox;
		protected Enterprise.ZArchitecture.ZTextBox Address1TextBox;
		Enterprise.ZArchitecture.ZTextBox Address2TextBox;
		Enterprise.ZArchitecture.GUI.ZButton SaveButton;
		Enterprise.ZArchitecture.GUI.ZButton OKButton;
		Enterprise.ZArchitecture.GUI.ZButton CancelBtn;
		protected Enterprise.ZArchitecture.GUI.ZDisplayGrid SimilarOrgMatchesBoundGrid;
		Enterprise.ZArchitecture.ZTextBox PostCodeTextBox;
		Enterprise.ZArchitecture.ZTextBox BusinessRegNZTextBox;
		Enterprise.ZArchitecture.ZLabel SimilarOrganisationsLabel;
		Internal.ZCodeFindBoxFixedPreBoundMaxLength CountryFindBox;
		Enterprise.ZArchitecture.ZTextBox CityTextBox;
		Enterprise.ZArchitecture.ZLabel SearchResultsStatusLabel;
		Enterprise.ZArchitecture.GUI.ZDropEdit StateDropDownEdit;
		ZGroupBox oGroupBox1;
		Enterprise.ZArchitecture.GUI.ZCheckBox PayablesCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox ReceivablesCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox ConsignorCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox ConsigneeCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox SalesLeadCheckBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox OM_OJ_ARDebtorGroupGuidFindBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox OM_OG_APCreditorGroupGuidFindBox;
		protected ZButton ValidateAddressButton;

		#endregion

	}
}

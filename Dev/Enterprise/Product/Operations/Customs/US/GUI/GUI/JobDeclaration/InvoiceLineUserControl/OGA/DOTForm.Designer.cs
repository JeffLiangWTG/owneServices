

namespace Enterprise.Customs.US.GUI
{
	partial class DOTForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DOTGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.US_DOTBondSuretyCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.US_DOTPriorApprovalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.US_DOTImpSubstStatementCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.US_DOTTireBrandNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TireBrandNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.US_DOTTireIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TireIDLabel = new Enterprise.ZArchitecture.ZLabel();
			this.US_DOTClarCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ClassificationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.US_DOTCountryOfOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PassportNoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.US_DOTPassportTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SuretyCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.US_DOTBoxNoDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OriginLabel = new Enterprise.ZArchitecture.ZLabel();
			this.US_DOTCommercialDescTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BoxNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.VehicleDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.VINGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DOTGroupBox.SuspendLayout();
			this.VehicleDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.VINGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 371, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.DOT);
			// 
			// DOTGroupBox
			// 
			this.DOTGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.DOTGroupBox.Controls.Add(this.US_DOTBondSuretyCodeTextBox);
			this.DOTGroupBox.Controls.Add(this.US_DOTPriorApprovalCheckBox);
			this.DOTGroupBox.Controls.Add(this.US_DOTImpSubstStatementCheckBox);
			this.DOTGroupBox.Controls.Add(this.US_DOTTireBrandNameTextBox);
			this.DOTGroupBox.Controls.Add(this.TireBrandNameLabel);
			this.DOTGroupBox.Controls.Add(this.US_DOTTireIDTextBox);
			this.DOTGroupBox.Controls.Add(this.TireIDLabel);
			this.DOTGroupBox.Controls.Add(this.US_DOTClarCodeDropEdit);
			this.DOTGroupBox.Controls.Add(this.ClassificationLabel);
			this.DOTGroupBox.Controls.Add(this.US_DOTCountryOfOriginCodeFindBox);
			this.DOTGroupBox.Controls.Add(this.PassportNoLabel);
			this.DOTGroupBox.Controls.Add(this.US_DOTPassportTextBox);
			this.DOTGroupBox.Controls.Add(this.SuretyCodeLabel);
			this.DOTGroupBox.Controls.Add(this.US_DOTBoxNoDropEdit);
			this.DOTGroupBox.Controls.Add(this.DescriptionLabel);
			this.DOTGroupBox.Controls.Add(this.OriginLabel);
			this.DOTGroupBox.Controls.Add(this.US_DOTCommercialDescTextBox);
			this.DOTGroupBox.Controls.Add(this.BoxNumberLabel);
			this.DOTGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 8, true);
			this.DOTGroupBox.Name = "DOTGroupBox";
			this.DOTGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 324, true);
			this.DOTGroupBox.TabIndex = 0;
			this.DOTGroupBox.TabStop = false;
			this.DOTGroupBox.Text = "DOT";
			// 
			// US_DOTBondSuretyCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_DOTBondSuretyCodeTextBox, "US_DOTBondSuretyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DOT)(null)).US_DOTBondSuretyCode)));
			this.US_DOTBondSuretyCodeTextBox.DecimalPlaces = 0;
			this.US_DOTBondSuretyCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 154, true);
			this.US_DOTBondSuretyCodeTextBox.Name = "US_DOTBondSuretyCodeTextBox";
			this.US_DOTBondSuretyCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.US_DOTBondSuretyCodeTextBox.TabIndex = 9;
			this.US_DOTBondSuretyCodeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// US_DOTPriorApprovalCheckBox
			// 
			this.BindingSource.SetBindingMember(this.US_DOTPriorApprovalCheckBox, "US_DOTPriorApproval");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.DOT)(null)).US_DOTPriorApproval)));
			this.US_DOTPriorApprovalCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.US_DOTPriorApprovalCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.US_DOTPriorApprovalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 261, true);
			this.US_DOTPriorApprovalCheckBox.Name = "US_DOTPriorApprovalCheckBox";
			this.US_DOTPriorApprovalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 24, true);
			this.US_DOTPriorApprovalCheckBox.TabIndex = 16;
			this.US_DOTPriorApprovalCheckBox.Text = "Prior Approval Letter:";
			// 
			// US_DOTImpSubstStatementCheckBox
			// 
			this.BindingSource.SetBindingMember(this.US_DOTImpSubstStatementCheckBox, "US_DOTImpSubstStatement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.DOT)(null)).US_DOTImpSubstStatement)));
			this.US_DOTImpSubstStatementCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.US_DOTImpSubstStatementCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.US_DOTImpSubstStatementCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 292, true);
			this.US_DOTImpSubstStatementCheckBox.Name = "US_DOTImpSubstStatementCheckBox";
			this.US_DOTImpSubstStatementCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 24, true);
			this.US_DOTImpSubstStatementCheckBox.TabIndex = 17;
			this.US_DOTImpSubstStatementCheckBox.Text = "Importer Statement:";
			// 
			// US_DOTTireBrandNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_DOTTireBrandNameTextBox, "US_DOTTireBrandName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DOT)(null)).US_DOTTireBrandName)));
			this.US_DOTTireBrandNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 207, true);
			this.US_DOTTireBrandNameTextBox.Name = "US_DOTTireBrandNameTextBox";
			this.US_DOTTireBrandNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.US_DOTTireBrandNameTextBox.TabIndex = 13;
			// 
			// TireBrandNameLabel
			// 
			this.TireBrandNameLabel.AutoSize = true;
			this.TireBrandNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 211, true);
			this.TireBrandNameLabel.Name = "TireBrandNameLabel";
			this.TireBrandNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 13, true);
			this.TireBrandNameLabel.TabIndex = 12;
			this.TireBrandNameLabel.Text = "Tire Brand Name:";
			// 
			// US_DOTTireIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_DOTTireIDTextBox, "US_DOTTireID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DOT)(null)).US_DOTTireID)));
			this.US_DOTTireIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 180, true);
			this.US_DOTTireIDTextBox.Name = "US_DOTTireIDTextBox";
			this.US_DOTTireIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.US_DOTTireIDTextBox.TabIndex = 11;
			// 
			// TireIDLabel
			// 
			this.TireIDLabel.AutoSize = true;
			this.TireIDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 184, true);
			this.TireIDLabel.Name = "TireIDLabel";
			this.TireIDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 13, true);
			this.TireIDLabel.TabIndex = 10;
			this.TireIDLabel.Text = "Tire ID:";
			// 
			// US_DOTClarCodeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.US_DOTClarCodeDropEdit, "US_DOTClarCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.DOT)(null)).US_DOTClarCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.DOT)(null)).AddInfoLookups.ClarificationCodes)));
			this.US_DOTClarCodeDropEdit.BindToList = "AddInfoLookups+ClarificationCodes";
			this.US_DOTClarCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 56, true);
			this.US_DOTClarCodeDropEdit.Name = "US_DOTClarCodeDropEdit";
			this.US_DOTClarCodeDropEdit.PreBoundMaxLength = 1;
			this.US_DOTClarCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 20, true);
			this.US_DOTClarCodeDropEdit.TabIndex = 3;
			// 
			// ClassificationLabel
			// 
			this.ClassificationLabel.AutoSize = true;
			this.ClassificationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 60, true);
			this.ClassificationLabel.Name = "ClassificationLabel";
			this.ClassificationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 13, true);
			this.ClassificationLabel.TabIndex = 2;
			this.ClassificationLabel.Text = "Clarification:";
			// 
			// US_DOTCountryOfOriginCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.US_DOTCountryOfOriginCodeFindBox, "US_DOTCountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DOT)(null)).US_DOTCountryOfOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.DOT)(null)).AddInfoLookups.USCountries)));
			this.US_DOTCountryOfOriginCodeFindBox.BindToList = "AddInfoLookups+USCountries";
			this.US_DOTCountryOfOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 126, true);
			this.US_DOTCountryOfOriginCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Country;
			this.US_DOTCountryOfOriginCodeFindBox.Name = "US_DOTCountryOfOriginCodeFindBox";
			this.US_DOTCountryOfOriginCodeFindBox.PreBoundMaxLength = 2;
			this.US_DOTCountryOfOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 20, true);
			this.US_DOTCountryOfOriginCodeFindBox.TabIndex = 7;
			// 
			// PassportNoLabel
			// 
			this.PassportNoLabel.AutoSize = true;
			this.PassportNoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 238, true);
			this.PassportNoLabel.Name = "PassportNoLabel";
			this.PassportNoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 13, true);
			this.PassportNoLabel.TabIndex = 14;
			this.PassportNoLabel.Text = "Passport No:";
			// 
			// US_DOTPassportTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_DOTPassportTextBox, "US_DOTPassport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DOT)(null)).US_DOTPassport)));
			this.US_DOTPassportTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 234, true);
			this.US_DOTPassportTextBox.Multiline = true;
			this.US_DOTPassportTextBox.Name = "US_DOTPassportTextBox";
			this.US_DOTPassportTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.US_DOTPassportTextBox.TabIndex = 15;
			// 
			// SuretyCodeLabel
			// 
			this.SuretyCodeLabel.AutoSize = true;
			this.SuretyCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 157, true);
			this.SuretyCodeLabel.Name = "SuretyCodeLabel";
			this.SuretyCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 13, true);
			this.SuretyCodeLabel.TabIndex = 8;
			this.SuretyCodeLabel.Text = "Surety Code:";
			// 
			// US_DOTBoxNoDropEdit
			// 
			this.BindingSource.SetBindingMember(this.US_DOTBoxNoDropEdit, "US_DOTBoxNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.DOT)(null)).US_DOTBoxNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.DOT)(null)).AddInfoLookups.BoxNumbers)));
			this.US_DOTBoxNoDropEdit.BindToList = "AddInfoLookups+BoxNumbers";
			this.US_DOTBoxNoDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 29, true);
			this.US_DOTBoxNoDropEdit.Name = "US_DOTBoxNoDropEdit";
			this.US_DOTBoxNoDropEdit.PreBoundMaxLength = 2;
			this.US_DOTBoxNoDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 20, true);
			this.US_DOTBoxNoDropEdit.TabIndex = 1;
			// 
			// DescriptionLabel
			// 
			this.DescriptionLabel.AutoSize = true;
			this.DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 88, true);
			this.DescriptionLabel.Name = "DescriptionLabel";
			this.DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 13, true);
			this.DescriptionLabel.TabIndex = 4;
			this.DescriptionLabel.Text = "Description:";
			// 
			// OriginLabel
			// 
			this.OriginLabel.AutoSize = true;
			this.OriginLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 130, true);
			this.OriginLabel.Name = "OriginLabel";
			this.OriginLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 13, true);
			this.OriginLabel.TabIndex = 6;
			this.OriginLabel.Text = "Origin:";
			// 
			// US_DOTCommercialDescTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_DOTCommercialDescTextBox, "US_DOTCommercialDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DOT)(null)).US_DOTCommercialDesc)));
			this.US_DOTCommercialDescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 83, true);
			this.US_DOTCommercialDescTextBox.Multiline = true;
			this.US_DOTCommercialDescTextBox.Name = "US_DOTCommercialDescTextBox";
			this.US_DOTCommercialDescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 36, true);
			this.US_DOTCommercialDescTextBox.TabIndex = 5;
			// 
			// BoxNumberLabel
			// 
			this.BoxNumberLabel.AutoSize = true;
			this.BoxNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 33, true);
			this.BoxNumberLabel.Name = "BoxNumberLabel";
			this.BoxNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 13, true);
			this.BoxNumberLabel.TabIndex = 0;
			this.BoxNumberLabel.Text = "Box Number:";
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(830, 342, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.Text = "&OK";
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// VehicleDetailsGroupBox
			// 
			this.VehicleDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.VehicleDetailsGroupBox.Controls.Add(this.VINGrid);
			this.VehicleDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 8, true);
			this.VehicleDetailsGroupBox.Name = "VehicleDetailsGroupBox";
			this.VehicleDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 324, true);
			this.VehicleDetailsGroupBox.TabIndex = 1;
			this.VehicleDetailsGroupBox.TabStop = false;
			this.VehicleDetailsGroupBox.Text = "Vehicle Details";
			// 
			// VINGrid
			// 
			this.VINGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.VINGrid, "DOTVINs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.DOT)(null)).DOTVINs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DOTVIN)(((System.Collections.IList)(((Enterprise.Customs.US.Business.DOT)(null)).DOTVINs)).SyncRoot)).US_DOTMake)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DOTVIN)(((System.Collections.IList)(((Enterprise.Customs.US.Business.DOT)(null)).DOTVINs)).SyncRoot)).US_DOTModel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DOTVIN)(((System.Collections.IList)(((Enterprise.Customs.US.Business.DOT)(null)).DOTVINs)).SyncRoot)).US_DOTRINo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.DOTVIN)(((System.Collections.IList)(((Enterprise.Customs.US.Business.DOT)(null)).DOTVINs)).SyncRoot)).US_DOTYear)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DOTVIN)(((System.Collections.IList)(((Enterprise.Customs.US.Business.DOT)(null)).DOTVINs)).SyncRoot)).US_DOTVEN)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.DOTVIN)(((System.Collections.IList)(((Enterprise.Customs.US.Business.DOT)(null)).DOTVINs)).SyncRoot)).US_DOTVIN)));
			this.VINGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Make";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "US_DOTMake";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Caption = "Model";
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "US_DOTModel";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo3.Caption = "NHTSA RI No";
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "US_DOTRINo";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Year";
			zCalcEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo1.ColumnName = "US_DOTYear";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.ShowGroupSeparators = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.Caption = "Veh. Eligibility No:";
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "US_DOTVEN";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.Caption = "VIN";
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "US_DOTVIN";
			zTextBoxColumnStyleInfo5.IsMandatory = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.VINGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.VINGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.VINGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.VINGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.VINGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.VINGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.VINGrid.GridId = "2f64de7a-d538-4af9-9b6b-4a98f79f13d2";
			this.VINGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VINGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.VINGrid.LayoutKey = "VINGrid";
			this.VINGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.VINGrid.Name = "VINGrid";
			this.VINGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 305, true);
			this.VINGrid.TabIndex = 0;
			// 
			// DOTForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 395, true);
			this.Controls.Add(this.VehicleDetailsGroupBox);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.DOTGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.DOT);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 421, true);
			this.Name = "DOTForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "DOT";
			this.Controls.SetChildIndex(this.DOTGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.VehicleDetailsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DOTGroupBox.ResumeLayout(false);
			this.DOTGroupBox.PerformLayout();
			this.VehicleDetailsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.VINGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox DOTGroupBox;
		public Enterprise.ZArchitecture.GUI.ZButton OKButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox VehicleDetailsGroupBox;
		private Enterprise.ZArchitecture.ZLabel DescriptionLabel;
		private Enterprise.ZArchitecture.ZTextBox US_DOTCommercialDescTextBox;
		private Enterprise.ZArchitecture.ZLabel SuretyCodeLabel;
		private Enterprise.ZArchitecture.ZLabel OriginLabel;
		private Enterprise.ZArchitecture.ZLabel BoxNumberLabel;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox US_DOTCountryOfOriginCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit US_DOTBoxNoDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox US_DOTImpSubstStatementCheckBox;
		private Enterprise.ZArchitecture.ZTextBox US_DOTTireBrandNameTextBox;
		private Enterprise.ZArchitecture.ZLabel TireBrandNameLabel;
		private Enterprise.ZArchitecture.ZTextBox US_DOTTireIDTextBox;
		private Enterprise.ZArchitecture.ZLabel TireIDLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit US_DOTClarCodeDropEdit;
		private Enterprise.ZArchitecture.ZLabel ClassificationLabel;
		private Enterprise.ZArchitecture.ZLabel PassportNoLabel;
		private Enterprise.ZArchitecture.ZTextBox US_DOTPassportTextBox;
		private Enterprise.ZArchitecture.ZGrid VINGrid;
		private Enterprise.ZArchitecture.GUI.ZCheckBox US_DOTPriorApprovalCheckBox;
		private Enterprise.ZArchitecture.ZTextBox US_DOTBondSuretyCodeTextBox;
	}
}

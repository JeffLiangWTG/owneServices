namespace Enterprise.Customs.US.GUI
{
	partial class ExportCustomsClassificationUserControl
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.ExportTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HTSAESDDTCDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HTSAESDDTCUnitDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.HTSAESDDTCUSMLCategoryCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.HTSAESDDTCPartyCertificationIndicatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.HTSAESDDTCMilitaryEquipmentIndicatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.HTSAESDDTCRegistrationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HTSAESDDTCCategoryXXIDeterminationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HTSAESDDTCITARExemptionNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.HTSAESGeneralDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ClassificationDescriptionTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.CI_UsageCommentTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.unitCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.HTSAESECCNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HTSAESLicenseNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HTSAESLicenseTypeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.HTSAESExportCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.HTSAESOriginIndicatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PGARequirementsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PGARequirementsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AMSTabPage = new Enterprise.Customs.GUI.BaseDeclarationTabPage();
			this.ATFTabPage = new Enterprise.Customs.GUI.BaseDeclarationTabPage();
			this.DEATabPage = new Enterprise.Customs.GUI.BaseDeclarationTabPage();
			this.FWSTabPage = new Enterprise.Customs.GUI.BaseDeclarationTabPage();
			this.NMFSTabPage = new Enterprise.Customs.GUI.BaseDeclarationTabPage();
			this.TTBTabPage = new Enterprise.Customs.GUI.BaseDeclarationTabPage();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExportTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.HTSAESDDTCDetailsGroupBox.SuspendLayout();
			this.HTSAESDDTCUnitDropEdit.SuspendLayout();
			this.HTSAESDDTCUSMLCategoryCodeDropEdit.SuspendLayout();
			this.HTSAESDDTCPartyCertificationIndicatorDropEdit.SuspendLayout();
			this.HTSAESDDTCMilitaryEquipmentIndicatorDropEdit.SuspendLayout();
			this.HTSAESDDTCITARExemptionNumberDropEdit.SuspendLayout();
			this.HTSAESGeneralDetailsGroupBox.SuspendLayout();
			this.ClassificationDescriptionTextBox.SuspendLayout();
			this.CI_UsageCommentTextBox.SuspendLayout();
			this.unitCalcFindBox.SuspendLayout();
			this.HTSAESLicenseTypeCodeFindBox.SuspendLayout();
			this.HTSAESExportCodeDropEdit.SuspendLayout();
			this.HTSAESOriginIndicatorDropEdit.SuspendLayout();
			this.PGARequirementsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PGARequirementsGrid)).BeginInit();
			this.PGARequirementsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.CusClassPartPivot);
			// 
			// ExportTabControl
			// 
			this.ExportTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ExportTabControl.Controls.Add(this.DetailsTabPage);
			this.ExportTabControl.Controls.Add(this.PGARequirementsTabPage);
			this.ExportTabControl.Controls.Add(this.AMSTabPage);
			this.ExportTabControl.Controls.Add(this.ATFTabPage);
			this.ExportTabControl.Controls.Add(this.DEATabPage);
			this.ExportTabControl.Controls.Add(this.FWSTabPage);
			this.ExportTabControl.Controls.Add(this.NMFSTabPage);
			this.ExportTabControl.Controls.Add(this.TTBTabPage);
			this.ExportTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExportTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExportTabControl.Name = "ExportTabControl";
			this.ExportTabControl.SelectedIndex = 0;
			this.ExportTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 299, true);
			this.ExportTabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.DetailsTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("81089899-3635-45ae-9409-cbf766bba147", "Details");
			this.DetailsTabPage.Controls.Add(this.HTSAESDDTCDetailsGroupBox);
			this.DetailsTabPage.Controls.Add(this.HTSAESGeneralDetailsGroupBox);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 272, true);
			this.DetailsTabPage.TabIndex = 0;
			this.DetailsTabPage.Text = "Details";
			// 
			// HTSAESDDTCDetailsGroupBox
			// 
			this.HTSAESDDTCDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("63837b16-9c05-45cd-8c30-29a5fb5e6ee9", "DDTC Details");
			this.HTSAESDDTCDetailsGroupBox.Controls.Add(this.HTSAESDDTCUnitDropEdit);
			this.HTSAESDDTCDetailsGroupBox.Controls.Add(this.HTSAESDDTCUSMLCategoryCodeDropEdit);
			this.HTSAESDDTCDetailsGroupBox.Controls.Add(this.HTSAESDDTCPartyCertificationIndicatorDropEdit);
			this.HTSAESDDTCDetailsGroupBox.Controls.Add(this.HTSAESDDTCMilitaryEquipmentIndicatorDropEdit);
			this.HTSAESDDTCDetailsGroupBox.Controls.Add(this.HTSAESDDTCRegistrationNumberTextBox);
			this.HTSAESDDTCDetailsGroupBox.Controls.Add(this.HTSAESDDTCCategoryXXIDeterminationNumberTextBox);
			this.HTSAESDDTCDetailsGroupBox.Controls.Add(this.HTSAESDDTCITARExemptionNumberDropEdit);
			this.HTSAESDDTCDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.HTSAESDDTCDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 167, true);
			this.HTSAESDDTCDetailsGroupBox.Name = "HTSAESDDTCDetailsGroupBox";
			this.HTSAESDDTCDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 125, true);
			this.HTSAESDDTCDetailsGroupBox.TabIndex = 7;
			this.HTSAESDDTCDetailsGroupBox.TabStop = false;
			// 
			// HTSAESDDTCUnitDropEdit
			// 
			this.HTSAESDDTCUnitDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HTSAESDDTCUnitDropEdit, "CD_DDTCUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CD_DDTCUnit)));
			this.HTSAESDDTCUnitDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5fdfad1e-16fe-4fae-bda9-5de0ef73f258", "Unit");
			this.HTSAESDDTCUnitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(478, 65, true);
			this.HTSAESDDTCUnitDropEdit.Name = "HTSAESDDTCUnitDropEdit";
			this.HTSAESDDTCUnitDropEdit.PreBoundMaxLength = 2;
			this.HTSAESDDTCUnitDropEdit.ShouldResizeByMaxLength = true;
			this.HTSAESDDTCUnitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.HTSAESDDTCUnitDropEdit.TabIndex = 6;
			// 
			// HTSAESDDTCUSMLCategoryCodeDropEdit
			// 
			this.HTSAESDDTCUSMLCategoryCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HTSAESDDTCUSMLCategoryCodeDropEdit, "CD_DDTCUSMLCategoryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CD_DDTCUSMLCategoryCode)));
			this.HTSAESDDTCUSMLCategoryCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f51fdefe-66b7-42f1-a5aa-0ce59e8511c5", "USML Category Code");
			this.HTSAESDDTCUSMLCategoryCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(478, 39, true);
			this.HTSAESDDTCUSMLCategoryCodeDropEdit.Name = "HTSAESDDTCUSMLCategoryCodeDropEdit";
			this.HTSAESDDTCUSMLCategoryCodeDropEdit.PreBoundMaxLength = 2;
			this.HTSAESDDTCUSMLCategoryCodeDropEdit.ShouldResizeByMaxLength = true;
			this.HTSAESDDTCUSMLCategoryCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.HTSAESDDTCUSMLCategoryCodeDropEdit.TabIndex = 5;
			// 
			// HTSAESDDTCCategoryXXIDeterminationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.HTSAESDDTCCategoryXXIDeterminationNumberTextBox, "CD_DDTCJurisdictionNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CD_DDTCJurisdictionNumber)));
			this.HTSAESDDTCCategoryXXIDeterminationNumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("F24E32F9-BFF1-4EA4-AC1D-577B0639F7F7", "Cat. XXI Determ. No.", "Cat. XXI Determination No.", "Category XXI Determination Number", "");
			this.HTSAESDDTCCategoryXXIDeterminationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 91, true);
			this.HTSAESDDTCCategoryXXIDeterminationNumberTextBox.Name = "HTSAESDDTCCategoryXXIDeterminationNumberTextBox";
			this.HTSAESDDTCCategoryXXIDeterminationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.HTSAESDDTCCategoryXXIDeterminationNumberTextBox.TabIndex = 3;
			// 
			// HTSAESDDTCPartyCertificationIndicatorDropEdit
			// 
			this.HTSAESDDTCPartyCertificationIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HTSAESDDTCPartyCertificationIndicatorDropEdit, "CD_PartyCertInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CD_PartyCertInd)));
			this.HTSAESDDTCPartyCertificationIndicatorDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("11da5e6e-97a2-4eba-ba24-dcbdf84c5eb8", "Party Cert. Ind.", "Party Certification Ind.", "Party Certification Indicator", "");
			this.HTSAESDDTCPartyCertificationIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 65, true);
			this.HTSAESDDTCPartyCertificationIndicatorDropEdit.Name = "HTSAESDDTCPartyCertificationIndicatorDropEdit";
			this.HTSAESDDTCPartyCertificationIndicatorDropEdit.PreBoundMaxLength = 1;
			this.HTSAESDDTCPartyCertificationIndicatorDropEdit.ShouldResizeByMaxLength = true;
			this.HTSAESDDTCPartyCertificationIndicatorDropEdit.ShowDescriptionBox = false;
			this.HTSAESDDTCPartyCertificationIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.HTSAESDDTCPartyCertificationIndicatorDropEdit.TabIndex = 2;
			// 
			// HTSAESDDTCMilitaryEquipmentIndicatorDropEdit
			// 
			this.HTSAESDDTCMilitaryEquipmentIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HTSAESDDTCMilitaryEquipmentIndicatorDropEdit, "CD_MilitaryEquipInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CD_MilitaryEquipInd)));
			this.HTSAESDDTCMilitaryEquipmentIndicatorDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("180e7956-32f9-49d2-b5b2-5b93c5168859", "Military Equip. Ind.", "Military Equipment Ind.", "Military Equipment Indicator", "");
			this.HTSAESDDTCMilitaryEquipmentIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 39, true);
			this.HTSAESDDTCMilitaryEquipmentIndicatorDropEdit.Name = "HTSAESDDTCMilitaryEquipmentIndicatorDropEdit";
			this.HTSAESDDTCMilitaryEquipmentIndicatorDropEdit.PreBoundMaxLength = 1;
			this.HTSAESDDTCMilitaryEquipmentIndicatorDropEdit.ShouldResizeByMaxLength = true;
			this.HTSAESDDTCMilitaryEquipmentIndicatorDropEdit.ShowDescriptionBox = false;
			this.HTSAESDDTCMilitaryEquipmentIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.HTSAESDDTCMilitaryEquipmentIndicatorDropEdit.TabIndex = 1;
			// 
			// HTSAESDDTCRegistrationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.HTSAESDDTCRegistrationNumberTextBox, "CD_DDTCRegoNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CD_DDTCRegoNo)));
			this.HTSAESDDTCRegistrationNumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ec5d7e8a-fdba-492c-9c5d-f3ada5dc3e41", "Registration Number");
			this.HTSAESDDTCRegistrationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(478, 13, true);
			this.HTSAESDDTCRegistrationNumberTextBox.Name = "HTSAESDDTCRegistrationNumberTextBox";
			this.HTSAESDDTCRegistrationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.HTSAESDDTCRegistrationNumberTextBox.TabIndex = 4;
			// 
			// HTSAESDDTCITARExemptionNumberDropEdit
			// 
			this.HTSAESDDTCITARExemptionNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HTSAESDDTCITARExemptionNumberDropEdit, "CD_ITARExemptionNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CD_ITARExemptionNo)));
			this.HTSAESDDTCITARExemptionNumberDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("bd8d843e-f0c1-45a4-b593-f013e9349402", "ITAR Exemption No.", "ITAR Exemption Number", "");
			this.HTSAESDDTCITARExemptionNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 13, true);
			this.HTSAESDDTCITARExemptionNumberDropEdit.Name = "HTSAESDDTCITARExemptionNumberDropEdit";
			this.HTSAESDDTCITARExemptionNumberDropEdit.PreBoundMaxLength = 12;
			this.HTSAESDDTCITARExemptionNumberDropEdit.ShouldResizeByMaxLength = true;
			this.HTSAESDDTCITARExemptionNumberDropEdit.ShowDescriptionBox = false;
			this.HTSAESDDTCITARExemptionNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 20, true);
			this.HTSAESDDTCITARExemptionNumberDropEdit.TabIndex = 0;
			// 
			// HTSAESGeneralDetailsGroupBox
			// 
			this.HTSAESGeneralDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8c7a8e01-e06b-46fa-8595-393683c0786d", "General Details");
			this.HTSAESGeneralDetailsGroupBox.Controls.Add(this.ClassificationDescriptionTextBox);
			this.HTSAESGeneralDetailsGroupBox.Controls.Add(this.CI_UsageCommentTextBox);
			this.HTSAESGeneralDetailsGroupBox.Controls.Add(this.unitCalcFindBox);
			this.HTSAESGeneralDetailsGroupBox.Controls.Add(this.HTSAESECCNTextBox);
			this.HTSAESGeneralDetailsGroupBox.Controls.Add(this.HTSAESLicenseNoTextBox);
			this.HTSAESGeneralDetailsGroupBox.Controls.Add(this.HTSAESLicenseTypeCodeFindBox);
			this.HTSAESGeneralDetailsGroupBox.Controls.Add(this.HTSAESExportCodeDropEdit);
			this.HTSAESGeneralDetailsGroupBox.Controls.Add(this.HTSAESOriginIndicatorDropEdit);
			this.HTSAESGeneralDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.HTSAESGeneralDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HTSAESGeneralDetailsGroupBox.Name = "HTSAESGeneralDetailsGroupBox";
			this.HTSAESGeneralDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 164, true);
			this.HTSAESGeneralDetailsGroupBox.TabIndex = 6;
			this.HTSAESGeneralDetailsGroupBox.TabStop = false;
			// 
			// ClassificationDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClassificationDescriptionTextBox, "CI_Description");
			this.ClassificationDescriptionTextBox.AllowDrop = true;
			this.ClassificationDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.ClassificationDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 123, true);
			this.ClassificationDescriptionTextBox.Name = "ClassificationDescriptionTextBox";
			this.ClassificationDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 20, true);
			this.ClassificationDescriptionTextBox.TabIndex = 7;
			// 
			// CI_UsageCommentTextBox
			// 
			this.CI_UsageCommentTextBox.AllowDrop = true;
			this.CI_UsageCommentTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("154B349D-84AD-44E5-A39B-B17544060D48", "Usage Comment");
			this.CI_UsageCommentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.CI_UsageCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 97, true);
			this.CI_UsageCommentTextBox.Name = "CI_UsageCommentTextBox";
			this.CI_UsageCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 20, true);
			this.CI_UsageCommentTextBox.TabIndex = 6;
			// 
			// unitCalcFindBox
			// 
			this.unitCalcFindBox.AllowDrop = true;
			this.unitCalcFindBox.BindToAmount = "CD_PerUnitCost";
			this.unitCalcFindBox.BindToUnit = "CD_RX_NKPerUnitCostCurr";
			this.unitCalcFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("70bb3fc1-d1d5-4dff-a4c9-a85ba4d1752b", "Price / Unit");
			this.unitCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.unitCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(478, 71, true);
			this.unitCalcFindBox.Name = "unitCalcFindBox";
			this.unitCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.unitCalcFindBox.TabIndex = 5;
			// 
			// HTSAESECCNTextBox
			// 
			this.BindingSource.SetBindingMember(this.HTSAESECCNTextBox, "CD_ECCN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CD_ECCN)));
			this.HTSAESECCNTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("b6de4b63-34ad-42b2-b3df-9eab7721ae41", "ECCN");
			this.HTSAESECCNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 71, true);
			this.HTSAESECCNTextBox.Name = "HTSAESECCNTextBox";
			this.HTSAESECCNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			this.HTSAESECCNTextBox.TabIndex = 2;
			// 
			// HTSAESLicenseNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.HTSAESLicenseNoTextBox, "CD_LicenceNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CD_LicenceNo)));
			this.HTSAESLicenseNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8dfa87d2-b003-462d-ae9a-1209f0594204", "License No.", "License Number", "");
			this.HTSAESLicenseNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(478, 45, true);
			this.HTSAESLicenseNoTextBox.Name = "HTSAESLicenseNoTextBox";
			this.HTSAESLicenseNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			this.HTSAESLicenseNoTextBox.TabIndex = 4;
			// 
			// HTSAESLicenseTypeCodeFindBox
			// 
			this.HTSAESLicenseTypeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HTSAESLicenseTypeCodeFindBox, "CD_LicenceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CD_LicenceType)));
			this.HTSAESLicenseTypeCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6ec83538-8050-45af-82c6-5f5657a52edd", "License Type");
			this.HTSAESLicenseTypeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 45, true);
			this.HTSAESLicenseTypeCodeFindBox.Name = "HTSAESLicenseTypeCodeFindBox";
			this.HTSAESLicenseTypeCodeFindBox.PreBoundMaxLength = 3;
			this.HTSAESLicenseTypeCodeFindBox.ShouldResize = true;
			this.HTSAESLicenseTypeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.HTSAESLicenseTypeCodeFindBox.TabIndex = 1;
			// 
			// HTSAESExportCodeDropEdit
			// 
			this.HTSAESExportCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HTSAESExportCodeDropEdit, "CD_ExportCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CD_ExportCode)));
			this.HTSAESExportCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a41e47f8-1723-4fbe-a44a-a64c92959789", "Export Code");
			this.HTSAESExportCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 19, true);
			this.HTSAESExportCodeDropEdit.Name = "HTSAESExportCodeDropEdit";
			this.HTSAESExportCodeDropEdit.PreBoundMaxLength = 2;
			this.HTSAESExportCodeDropEdit.ShouldResizeByMaxLength = true;
			this.HTSAESExportCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.HTSAESExportCodeDropEdit.TabIndex = 0;
			// 
			// HTSAESOriginIndicatorDropEdit
			// 
			this.HTSAESOriginIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HTSAESOriginIndicatorDropEdit, "CD_OriginIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CD_OriginIndicator)));
			this.HTSAESOriginIndicatorDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("438fd523-2d53-4bf2-8163-bf736f8e3aeb", "Origin Indicator");
			this.HTSAESOriginIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(478, 19, true);
			this.HTSAESOriginIndicatorDropEdit.Name = "HTSAESOriginIndicatorDropEdit";
			this.HTSAESOriginIndicatorDropEdit.PreBoundMaxLength = 1;
			this.HTSAESOriginIndicatorDropEdit.ShouldResizeByMaxLength = true;
			this.HTSAESOriginIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.HTSAESOriginIndicatorDropEdit.TabIndex = 3;
			// 
			// PGARequirementsTabPage
			// 
			this.PGARequirementsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.PGARequirementsTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("da8ef5ac-0d95-432d-8a55-f22d9815357f", "PGA Requirements");
			this.PGARequirementsTabPage.Controls.Add(this.PGARequirementsGrid);
			this.PGARequirementsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PGARequirementsTabPage.Name = "PGARequirementsTabPage";
			this.PGARequirementsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PGARequirementsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 272, true);
			this.PGARequirementsTabPage.TabIndex = 1;
			this.PGARequirementsTabPage.Text = "PGA Requirements";
			// 
			// PGARequirementsGrid
			// 
			this.PGARequirementsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PGARequirementsGrid, "ExportPGAAgencyRequirements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).ExportPGAAgencyRequirements)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.OGAAgencyRequirement)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).ExportPGAAgencyRequirements)).SyncRoot)).AgencyCodeWithDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.OGAAgencyRequirement)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).ExportPGAAgencyRequirements)).SyncRoot)).Indicator)));
			this.PGARequirementsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "AgencyCodeWithDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			zDropEditColumnStyleInfo1.ColumnName = "Indicator";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PGARequirementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PGARequirementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PGARequirementsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PGARequirementsGrid.GridId = "266378e2-ea3e-43a9-9e98-ab6ae83cb0dc";
			this.PGARequirementsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PGARequirementsGrid.LayoutKey = "PGARequirementsGrid";
			this.PGARequirementsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PGARequirementsGrid.Name = "PGARequirementsGrid";
			this.PGARequirementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 266, true);
			this.PGARequirementsGrid.TabIndex = 1;
			// 
			// AMSTabPage
			// 
			this.AMSTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.AMSTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d824bb81-2aa9-4d17-8c2c-778ac27328b3", "AMS/EPA");
			this.AMSTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AMSTabPage.Name = "AMSTabPage";
			this.AMSTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AMSTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 272, true);
			this.AMSTabPage.TabIndex = 2;
			this.AMSTabPage.Text = "AMS/EPA";
			// 
			// ATFTabPage
			// 
			this.ATFTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.ATFTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("82bbe81a-11bd-4342-a5a8-2232227ced9e", "ATF");
			this.ATFTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ATFTabPage.Name = "ATFTabPage";
			this.ATFTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ATFTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 272, true);
			this.ATFTabPage.TabIndex = 3;
			this.ATFTabPage.Text = "ATF";
			// 
			// DEATabPage
			// 
			this.DEATabPage.BackColor = System.Drawing.SystemColors.Control;
			this.DEATabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8fa5f0f0-3761-4a51-a32c-269b0bd30408", "DEA");
			this.DEATabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DEATabPage.Name = "DEATabPage";
			this.DEATabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DEATabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 272, true);
			this.DEATabPage.TabIndex = 4;
			this.DEATabPage.Text = "DEA";
			// 
			// FWSTabPage
			// 
			this.FWSTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.FWSTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6b522c73-e5fb-4bf0-a7bf-035656c49b23", "FWS");
			this.FWSTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.FWSTabPage.Name = "FWSTabPage";
			this.FWSTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.FWSTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 272, true);
			this.FWSTabPage.TabIndex = 5;
			this.FWSTabPage.Text = "FWS";
			// 
			// NMFSTabPage
			// 
			this.NMFSTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.NMFSTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7ec3cdae-007b-4b42-bb5b-6ef43fadd6f5", "NMFS");
			this.NMFSTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NMFSTabPage.Name = "NMFSTabPage";
			this.NMFSTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.NMFSTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 272, true);
			this.NMFSTabPage.TabIndex = 6;
			this.NMFSTabPage.Text = "NMFS";
			// 
			// TTBTabPage
			// 
			this.TTBTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.TTBTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7d2af064-2387-46cc-86d8-ddd88b1c9631", "TTB");
			this.TTBTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TTBTabPage.Name = "TTBTabPage";
			this.TTBTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TTBTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 272, true);
			this.TTBTabPage.TabIndex = 7;
			this.TTBTabPage.Text = "TTB";
			// 
			// ExportCustomsClassificationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ExportTabControl);
			this.Name = "ExportCustomsClassificationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 299, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExportTabControl.ResumeLayout(false);
			this.ExportTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.HTSAESDDTCDetailsGroupBox.ResumeLayout(false);
			this.HTSAESDDTCDetailsGroupBox.PerformLayout();
			this.HTSAESDDTCUnitDropEdit.ResumeLayout(true);
			this.HTSAESDDTCUnitDropEdit.PerformLayout();
			this.HTSAESDDTCUSMLCategoryCodeDropEdit.ResumeLayout(true);
			this.HTSAESDDTCUSMLCategoryCodeDropEdit.PerformLayout();
			this.HTSAESDDTCPartyCertificationIndicatorDropEdit.ResumeLayout(true);
			this.HTSAESDDTCPartyCertificationIndicatorDropEdit.PerformLayout();
			this.HTSAESDDTCMilitaryEquipmentIndicatorDropEdit.ResumeLayout(true);
			this.HTSAESDDTCMilitaryEquipmentIndicatorDropEdit.PerformLayout();
			this.HTSAESDDTCITARExemptionNumberDropEdit.ResumeLayout(true);
			this.HTSAESDDTCITARExemptionNumberDropEdit.PerformLayout();
			this.HTSAESGeneralDetailsGroupBox.ResumeLayout(false);
			this.HTSAESGeneralDetailsGroupBox.PerformLayout();
			this.ClassificationDescriptionTextBox.ResumeLayout(true);
			this.ClassificationDescriptionTextBox.PerformLayout();
			this.CI_UsageCommentTextBox.ResumeLayout(true);
			this.CI_UsageCommentTextBox.PerformLayout();
			this.unitCalcFindBox.ResumeLayout(true);
			this.unitCalcFindBox.PerformLayout();
			this.HTSAESLicenseTypeCodeFindBox.ResumeLayout(true);
			this.HTSAESLicenseTypeCodeFindBox.PerformLayout();
			this.HTSAESExportCodeDropEdit.ResumeLayout(true);
			this.HTSAESExportCodeDropEdit.PerformLayout();
			this.HTSAESOriginIndicatorDropEdit.ResumeLayout(true);
			this.HTSAESOriginIndicatorDropEdit.PerformLayout();
			this.PGARequirementsTabPage.ResumeLayout(false);
			this.PGARequirementsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PGARequirementsGrid)).EndInit();
			this.PGARequirementsGrid.ResumeLayout(false);
			this.PGARequirementsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabControl ExportTabControl;
		private ZArchitecture.GUI.ZTabPage DetailsTabPage;
		private ZArchitecture.GUI.ZTabPage PGARequirementsTabPage;
		internal Enterprise.Customs.GUI.BaseDeclarationTabPage AMSTabPage;
		internal Enterprise.Customs.GUI.BaseDeclarationTabPage ATFTabPage;
		internal Enterprise.Customs.GUI.BaseDeclarationTabPage DEATabPage;
		internal Enterprise.Customs.GUI.BaseDeclarationTabPage FWSTabPage;
		internal Enterprise.Customs.GUI.BaseDeclarationTabPage NMFSTabPage;
		internal Enterprise.Customs.GUI.BaseDeclarationTabPage TTBTabPage;
		private ZArchitecture.GUI.ZGroupBox HTSAESDDTCDetailsGroupBox;
		private ZArchitecture.GUI.ZDropEdit HTSAESDDTCUnitDropEdit;
		private ZArchitecture.GUI.ZDropEdit HTSAESDDTCUSMLCategoryCodeDropEdit;
		private ZArchitecture.GUI.ZDropEdit HTSAESDDTCPartyCertificationIndicatorDropEdit;
		private ZArchitecture.GUI.ZDropEdit HTSAESDDTCMilitaryEquipmentIndicatorDropEdit;
		private ZArchitecture.ZTextBox HTSAESDDTCRegistrationNumberTextBox;
		private ZArchitecture.ZTextBox HTSAESDDTCCategoryXXIDeterminationNumberTextBox;
		private ZArchitecture.GUI.ZDropEdit HTSAESDDTCITARExemptionNumberDropEdit;
		private ZArchitecture.GUI.ZGroupBox HTSAESGeneralDetailsGroupBox;
		private ZArchitecture.GUI.ZCalcFindBox unitCalcFindBox;
		private ZArchitecture.ZTextBox HTSAESECCNTextBox;
		private ZArchitecture.ZTextBox HTSAESLicenseNoTextBox;
		private ZArchitecture.GUI.ZCodeFindBox HTSAESLicenseTypeCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit HTSAESExportCodeDropEdit;
		private ZArchitecture.GUI.ZDropEdit HTSAESOriginIndicatorDropEdit;
		private ZArchitecture.ZGrid PGARequirementsGrid;
		private Enterprise.Customs.GUI.LongTextControl CI_UsageCommentTextBox;
		private Customs.GUI.LongTextControl ClassificationDescriptionTextBox;
	}
}

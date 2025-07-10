namespace Enterprise.Customs.ZA.GUI
{
	partial class RemovalDetailsControl
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
            this.RemovalDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.AdditionalBondGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ProvisionalPaymentSuretyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.BondHolderBondGuaranteeValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.BondHolderFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.BondGuaranteeValueLabel2 = new Enterprise.ZArchitecture.ZLabel();
            this.SubContractorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.SubContractorBondGuaranteeValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.SubContractorFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.RemoverEDICheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.TotalBondSuretyAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.RemoverBondGuaranteeValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.TotalBNDLabel = new Enterprise.ZArchitecture.ZLabel();
            this.EDILabel = new Enterprise.ZArchitecture.ZLabel();
            this.BondGuaranteeValueLabel1 = new Enterprise.ZArchitecture.ZLabel();
            this.RemoverFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.RemovalDetailsGroupBox.SuspendLayout();
            this.AdditionalBondGroupBox.SuspendLayout();
            this.BondHolderFindBox.SuspendLayout();
            this.SubContractorFindBox.SuspendLayout();
            this.RemoverFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.JobDeclaration);
            // 
            // RemovalDetailsGroupBox
            // 
            this.RemovalDetailsGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("d7d9922e-6b13-4978-95b0-5e60efb5200c", "Removal Details");
            this.RemovalDetailsGroupBox.Controls.Add(this.AdditionalBondGroupBox);
            this.RemovalDetailsGroupBox.Controls.Add(this.SubContractorCheckBox);
            this.RemovalDetailsGroupBox.Controls.Add(this.SubContractorBondGuaranteeValueCalcEdit);
            this.RemovalDetailsGroupBox.Controls.Add(this.SubContractorFindBox);
            this.RemovalDetailsGroupBox.Controls.Add(this.RemoverEDICheckBox);
            this.RemovalDetailsGroupBox.Controls.Add(this.TotalBondSuretyAmountCalcEdit);
            this.RemovalDetailsGroupBox.Controls.Add(this.RemoverBondGuaranteeValueCalcEdit);
            this.RemovalDetailsGroupBox.Controls.Add(this.TotalBNDLabel);
            this.RemovalDetailsGroupBox.Controls.Add(this.EDILabel);
            this.RemovalDetailsGroupBox.Controls.Add(this.BondGuaranteeValueLabel1);
            this.RemovalDetailsGroupBox.Controls.Add(this.RemoverFindBox);
            this.RemovalDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RemovalDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.RemovalDetailsGroupBox.Name = "RemovalDetailsGroupBox";
            this.RemovalDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 164, true);
            this.RemovalDetailsGroupBox.TabIndex = 0;
            this.RemovalDetailsGroupBox.TabStop = false;
            // 
            // AdditionalBondGroupBox
            // 
            this.AdditionalBondGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AdditionalBondGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("b7d73bf6-6679-4f63-9822-e4ba8fdd319e", "Additional Bond");
            this.AdditionalBondGroupBox.Controls.Add(this.ProvisionalPaymentSuretyCalcEdit);
            this.AdditionalBondGroupBox.Controls.Add(this.BondHolderBondGuaranteeValueCalcEdit);
            this.AdditionalBondGroupBox.Controls.Add(this.BondHolderFindBox);
            this.AdditionalBondGroupBox.Controls.Add(this.BondGuaranteeValueLabel2);
            this.AdditionalBondGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 81, true);
            this.AdditionalBondGroupBox.Name = "AdditionalBondGroupBox";
            this.AdditionalBondGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 80, true);
            this.AdditionalBondGroupBox.TabIndex = 11;
            this.AdditionalBondGroupBox.TabStop = false;
            // 
            // ProvisionalPaymentSuretyCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.ProvisionalPaymentSuretyCalcEdit, "CustomsEntryInstructions.CEI_ProvisionalPaymentSuretyAmount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_ProvisionalPaymentSuretyAmount)));
            this.ProvisionalPaymentSuretyCalcEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("908f65f5-e44b-4ba3-9ff8-6a62ca4b8bbb", "PPS Provisional Payment Surety");
            this.ProvisionalPaymentSuretyCalcEdit.DecimalPlaces = 0;
            this.ProvisionalPaymentSuretyCalcEdit.Decimals = 0;
            this.ProvisionalPaymentSuretyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 53, true);
            this.ProvisionalPaymentSuretyCalcEdit.Name = "ProvisionalPaymentSuretyCalcEdit";
            this.ProvisionalPaymentSuretyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
            this.ProvisionalPaymentSuretyCalcEdit.TabIndex = 3;
            this.ProvisionalPaymentSuretyCalcEdit.Text = "0";
            this.ProvisionalPaymentSuretyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ProvisionalPaymentSuretyCalcEdit.TrackDisposedAccess = true;
            // 
            // BondHolderBondGuaranteeValueCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.BondHolderBondGuaranteeValueCalcEdit, "CustomsEntryInstructions.BondHolderBondGuaranteeValue");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).BondHolderBondGuaranteeValue)));
            this.BondHolderBondGuaranteeValueCalcEdit.DecimalPlaces = 0;
            this.BondHolderBondGuaranteeValueCalcEdit.Decimals = 0;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BondHolderBondGuaranteeValueCalcEdit, false);
            this.BondHolderBondGuaranteeValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 27, true);
            this.BondHolderBondGuaranteeValueCalcEdit.Name = "BondHolderBondGuaranteeValueCalcEdit";
            this.BondHolderBondGuaranteeValueCalcEdit.ReadOnly = true;
            this.BondHolderBondGuaranteeValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
            this.BondHolderBondGuaranteeValueCalcEdit.TabIndex = 2;
            this.BondHolderBondGuaranteeValueCalcEdit.Text = "0";
            this.BondHolderBondGuaranteeValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.BondHolderBondGuaranteeValueCalcEdit.TrackDisposedAccess = true;
            // 
            // BondHolderFindBox
            // 
            this.BondHolderFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.BondHolderFindBox, "CustomsEntryInstructions.CEI_OH_BondHolder");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ZA.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OH_BondHolder)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).Lookups.Organisations)));
            this.BondHolderFindBox.BindToList = "Lookups+Organisations";
            this.BondHolderFindBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ed5a369c-8c69-425e-ac60-a42292976a13", "BHR Bond Holder");
            this.BondHolderFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 27, true);
            this.BondHolderFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
            this.BondHolderFindBox.Name = "BondHolderFindBox";
            this.BondHolderFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.BondHolderFindBox.ParentType = null;
            this.BondHolderFindBox.ShowDescriptionBox = false;
            this.BondHolderFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
            this.BondHolderFindBox.TabIndex = 0;
            // 
            // BondGuaranteeValueLabel2
            // 
            this.BondGuaranteeValueLabel2.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("de65ae69-b96c-4d80-97f7-e2098da461f6", "Registered BGV");
            this.BondGuaranteeValueLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.BondGuaranteeValueLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 11, true);
            this.BondGuaranteeValueLabel2.Name = "BondGuaranteeValueLabel2";
            this.BondGuaranteeValueLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 13, true);
            this.BondGuaranteeValueLabel2.TabIndex = 1;
            this.BondGuaranteeValueLabel2.UseMnemonic = false;
            // 
            // SubContractorCheckBox
            // 
            this.BindingSource.SetBindingMember(this.SubContractorCheckBox, "CustomsEntryInstructions.CEI_SubContractorEDI");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ZA.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_SubContractorEDI)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SubContractorCheckBox, false);
            this.SubContractorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 58, true);
            this.SubContractorCheckBox.Name = "SubContractorCheckBox";
            this.SubContractorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
            this.SubContractorCheckBox.TabIndex = 8;
            this.SubContractorCheckBox.UseVisualStyleBackColor = true;
            // 
            // SubContractorBondGuaranteeValueCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.SubContractorBondGuaranteeValueCalcEdit, "CustomsEntryInstructions.SubContractorBondGuaranteeValue");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).SubContractorBondGuaranteeValue)));
            this.SubContractorBondGuaranteeValueCalcEdit.DecimalPlaces = 0;
            this.SubContractorBondGuaranteeValueCalcEdit.Decimals = 0;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SubContractorBondGuaranteeValueCalcEdit, false);
            this.SubContractorBondGuaranteeValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 54, true);
            this.SubContractorBondGuaranteeValueCalcEdit.Name = "SubContractorBondGuaranteeValueCalcEdit";
            this.SubContractorBondGuaranteeValueCalcEdit.ReadOnly = true;
            this.SubContractorBondGuaranteeValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
            this.SubContractorBondGuaranteeValueCalcEdit.TabIndex = 7;
            this.SubContractorBondGuaranteeValueCalcEdit.Text = "0";
            this.SubContractorBondGuaranteeValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.SubContractorBondGuaranteeValueCalcEdit.TrackDisposedAccess = true;
            // 
            // SubContractorFindBox
            // 
            this.SubContractorFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SubContractorFindBox, "CustomsEntryInstructions.OH_SubContractor");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ZA.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).OH_SubContractor)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).Lookups.CarrierOrganisations)));
            this.SubContractorFindBox.BindToList = "Lookups+CarrierOrganisations";
            this.SubContractorFindBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("6132136a-6c4a-4a33-93a1-6fc6a6b26674", "Sub-Contractor");
            this.SubContractorFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 55, true);
            this.SubContractorFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
            this.SubContractorFindBox.Name = "SubContractorFindBox";
            this.SubContractorFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.SubContractorFindBox.ParentType = null;
            this.SubContractorFindBox.ShowDescriptionBox = false;
            this.SubContractorFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
            this.SubContractorFindBox.TabIndex = 6;
            // 
            // RemoverEDICheckBox
            // 
            this.BindingSource.SetBindingMember(this.RemoverEDICheckBox, "CustomsEntryInstructions.CEI_RemoverEDI");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ZA.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_RemoverEDI)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RemoverEDICheckBox, false);
            this.RemoverEDICheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 32, true);
            this.RemoverEDICheckBox.Name = "RemoverEDICheckBox";
            this.RemoverEDICheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
            this.RemoverEDICheckBox.TabIndex = 5;
            this.RemoverEDICheckBox.UseVisualStyleBackColor = true;
            // 
            // TotalBondSuretyAmountCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.TotalBondSuretyAmountCalcEdit, "CustomsEntryInstructions.TotalBondSuretyAmount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).TotalBondSuretyAmount)));
            this.TotalBondSuretyAmountCalcEdit.DecimalPlaces = 0;
            this.TotalBondSuretyAmountCalcEdit.Decimals = 0;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalBondSuretyAmountCalcEdit, false);
            this.TotalBondSuretyAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(326, 41, true);
            this.TotalBondSuretyAmountCalcEdit.Name = "TotalBondSuretyAmountCalcEdit";
            this.TotalBondSuretyAmountCalcEdit.ReadOnly = true;
            this.TotalBondSuretyAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
            this.TotalBondSuretyAmountCalcEdit.TabIndex = 10;
            this.TotalBondSuretyAmountCalcEdit.Text = "0";
            this.TotalBondSuretyAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TotalBondSuretyAmountCalcEdit.TrackDisposedAccess = true;
            // 
            // RemoverBondGuaranteeValueCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.RemoverBondGuaranteeValueCalcEdit, "CustomsEntryInstructions.RemoverBondGuaranteeValue");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).RemoverBondGuaranteeValue)));
            this.RemoverBondGuaranteeValueCalcEdit.DecimalPlaces = 0;
            this.RemoverBondGuaranteeValueCalcEdit.Decimals = 0;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RemoverBondGuaranteeValueCalcEdit, false);
            this.RemoverBondGuaranteeValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 28, true);
            this.RemoverBondGuaranteeValueCalcEdit.Name = "RemoverBondGuaranteeValueCalcEdit";
            this.RemoverBondGuaranteeValueCalcEdit.ReadOnly = true;
            this.RemoverBondGuaranteeValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
            this.RemoverBondGuaranteeValueCalcEdit.TabIndex = 3;
            this.RemoverBondGuaranteeValueCalcEdit.Text = "0";
            this.RemoverBondGuaranteeValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.RemoverBondGuaranteeValueCalcEdit.TrackDisposedAccess = true;
            // 
            // TotalBNDLabel
            // 
            this.TotalBNDLabel.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("c18c6602-96e4-4b66-aba7-a1a8c79ca6e5", "Total BND");
            this.TotalBNDLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.TotalBNDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(326, 25, true);
            this.TotalBNDLabel.Name = "TotalBNDLabel";
            this.TotalBNDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 13, true);
            this.TotalBNDLabel.TabIndex = 9;
            this.TotalBNDLabel.UseMnemonic = false;
            // 
            // EDILabel
            // 
            this.EDILabel.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("de65ae69-b96c-4d80-97f7-e2098da461f6", "EDI");
            this.EDILabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.EDILabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 12, true);
            this.EDILabel.Name = "EDILabel";
            this.EDILabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 13, true);
            this.EDILabel.TabIndex = 4;
            this.EDILabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.EDILabel.UseMnemonic = false;
            // 
            // BondGuaranteeValueLabel1
            // 
            this.BondGuaranteeValueLabel1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("de65ae69-b96c-4d80-97f7-e2098da461f6", "Registered BGV");
            this.BondGuaranteeValueLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.BondGuaranteeValueLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 12, true);
            this.BondGuaranteeValueLabel1.Name = "BondGuaranteeValueLabel1";
            this.BondGuaranteeValueLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 13, true);
            this.BondGuaranteeValueLabel1.TabIndex = 2;
            this.BondGuaranteeValueLabel1.UseMnemonic = false;
            // 
            // RemoverFindBox
            // 
            this.RemoverFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.RemoverFindBox, "CustomsEntryInstructions.CEI_OH_Carrier");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ZA.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OH_Carrier)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).Lookups.CarrierOrganisations)));
            this.RemoverFindBox.BindToList = "Lookups+CarrierOrganisations";
            this.RemoverFindBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("2a643150-5b63-40c2-9ad8-d87061f4e10f", "Remover");
            this.RemoverFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 28, true);
            this.RemoverFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
            this.RemoverFindBox.Name = "RemoverFindBox";
            this.RemoverFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.RemoverFindBox.ParentType = null;
            this.RemoverFindBox.ShowDescriptionBox = false;
            this.RemoverFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
            this.RemoverFindBox.TabIndex = 1;
            // 
            // RemovalDetailsControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.RemovalDetailsGroupBox);
            this.Name = "RemovalDetailsControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 164, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.RemovalDetailsGroupBox.ResumeLayout(false);
            this.RemovalDetailsGroupBox.PerformLayout();
            this.AdditionalBondGroupBox.ResumeLayout(false);
            this.AdditionalBondGroupBox.PerformLayout();
            this.BondHolderFindBox.ResumeLayout(true);
            this.BondHolderFindBox.PerformLayout();
            this.SubContractorFindBox.ResumeLayout(true);
            this.SubContractorFindBox.PerformLayout();
            this.RemoverFindBox.ResumeLayout(true);
            this.RemoverFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox RemovalDetailsGroupBox;
		private ZArchitecture.GUI.ZGuidFindBox RemoverFindBox;
		private ZArchitecture.ZLabel BondGuaranteeValueLabel1;
		private ZArchitecture.ZCalcEdit RemoverBondGuaranteeValueCalcEdit;
		private ZArchitecture.GUI.ZCheckBox RemoverEDICheckBox;
		private ZArchitecture.ZLabel EDILabel;
		private ZArchitecture.ZCalcEdit TotalBondSuretyAmountCalcEdit;
		private ZArchitecture.ZLabel TotalBNDLabel;
		private ZArchitecture.GUI.ZGuidFindBox SubContractorFindBox;
		private ZArchitecture.GUI.ZCheckBox SubContractorCheckBox;
		private ZArchitecture.ZCalcEdit SubContractorBondGuaranteeValueCalcEdit;
		private ZArchitecture.GUI.ZGroupBox AdditionalBondGroupBox;
		private ZArchitecture.ZCalcEdit BondHolderBondGuaranteeValueCalcEdit;
		private ZArchitecture.GUI.ZGuidFindBox BondHolderFindBox;
		private ZArchitecture.ZLabel BondGuaranteeValueLabel2;
		private ZArchitecture.ZCalcEdit ProvisionalPaymentSuretyCalcEdit;
	}
}

using Enterprise.Customs.SG.V4.Business;

using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.SG.V4.GUI
{
	partial class SGTN41DeclarationUserControl
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				UnHookValueChanged(JobDeclaration as JobDeclaration);
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
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.FinalPortOfCallFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.FinalDestinationCountryFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.NextPortOfCallFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.zLabel11 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel10 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel9 = new Enterprise.ZArchitecture.ZLabel();
            this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.zLabel15 = new Enterprise.ZArchitecture.ZLabel();
            this.PlaceOfStorageFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.PlaceOfReleaseFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.PlaceOfReceiptFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.zLabel7 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel8 = new Enterprise.ZArchitecture.ZLabel();
            this.zGroupBox5 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.zCheckBox3 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.zCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.zDateEdit2 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.zTextBox5 = new Enterprise.ZArchitecture.ZTextBox();
            this.zTextBox4 = new Enterprise.ZArchitecture.ZTextBox();
            this.zDateEdit1 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
            this.zCodeFindBox1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.zLabel28 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel27 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel26 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel25 = new Enterprise.ZArchitecture.ZLabel();
            this.StartDateLabel = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel23 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel16 = new Enterprise.ZArchitecture.ZLabel();
            this.zGroupBox6 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.DutyExemptCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.OutwardCarrierAgentGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.ConsigneeGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.InwardCarrierAgentGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.zLabel19 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel14 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel22 = new Enterprise.ZArchitecture.ZLabel();
            this.HandlingAgentGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.ExporterGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.zLabel17 = new Enterprise.ZArchitecture.ZLabel();
            this.EndUserGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.ForwarderGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.zTextBox3 = new Enterprise.ZArchitecture.ZTextBox();
            this.zLabel21 = new Enterprise.ZArchitecture.ZLabel();
            this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
            this.ClaimantGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.zLabel18 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel13 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel12 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel6 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel20 = new Enterprise.ZArchitecture.ZLabel();
            this.SGDeclarationTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
            this.zTabPage1 = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.CPCGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.CPCViewEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.CPCGrid = new Enterprise.ZArchitecture.ZGrid();
            this.zGroupBox10 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.zCalcEdit11 = new Enterprise.ZArchitecture.ZCalcEdit();
            this.zLabel47 = new Enterprise.ZArchitecture.ZLabel();
            this.TotalOtherTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.zCalcEdit8 = new Enterprise.ZArchitecture.ZCalcEdit();
            this.zCalcEdit7 = new Enterprise.ZArchitecture.ZCalcEdit();
            this.zCalcEdit6 = new Enterprise.ZArchitecture.ZCalcEdit();
            this.zLabel45 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel44 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel43 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel42 = new Enterprise.ZArchitecture.ZLabel();
            this.TotalOtherTaxLabel = new Enterprise.ZArchitecture.ZLabel();
            this.zCalcEdit4 = new Enterprise.ZArchitecture.ZCalcEdit();
            this.zGroupBox3 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.CALicencesGrid = new Enterprise.ZArchitecture.ZGrid();
            this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.TradersRemarksGrid = new Enterprise.ZArchitecture.ZGrid();
            this.TradersRemarksTextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.TradersRemarksTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CertificateOfOriginTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.CofOGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.zGroupBox7 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.zLabel36 = new Enterprise.ZArchitecture.ZLabel();
            this.zDropEdit2 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.zLabel40 = new Enterprise.ZArchitecture.ZLabel();
            this.zCalcEdit5 = new Enterprise.ZArchitecture.ZCalcEdit();
            this.zGroupBox8 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.zTextBox11 = new Enterprise.ZArchitecture.ZTextBox();
            this.zLabel30 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
            this.zCodeFindBox2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.zTextBox10 = new Enterprise.ZArchitecture.ZTextBox();
            this.zLabel41 = new Enterprise.ZArchitecture.ZLabel();
            this.zTextBox6 = new Enterprise.ZArchitecture.ZTextBox();
            this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel31 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
            this.DonorCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.zLabel33 = new Enterprise.ZArchitecture.ZLabel();
            this.zDropEdit3 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ManufacturerGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.zCalcEdit2 = new Enterprise.ZArchitecture.ZCalcEdit();
            this.zGroupBox4 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.zLabel29 = new Enterprise.ZArchitecture.ZLabel();
            this.zCalcEdit3 = new Enterprise.ZArchitecture.ZCalcEdit();
            this.zDropEdit4 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.zLabel34 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
            this.zCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
            this.zCheckBox4 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.FinalPortOfCallFindBox.SuspendLayout();
            this.FinalDestinationCountryFindBox.SuspendLayout();
            this.NextPortOfCallFindBox.SuspendLayout();
            this.zGroupBox1.SuspendLayout();
            this.PlaceOfStorageFindBox.SuspendLayout();
            this.PlaceOfReleaseFindBox.SuspendLayout();
            this.PlaceOfReceiptFindBox.SuspendLayout();
            this.zGroupBox5.SuspendLayout();
            this.zDateEdit2.SuspendLayout();
            this.zDateEdit1.SuspendLayout();
            this.zDropEdit1.SuspendLayout();
            this.zCodeFindBox1.SuspendLayout();
            this.zGroupBox6.SuspendLayout();
            this.OutwardCarrierAgentGuidFindBox.SuspendLayout();
            this.ConsigneeGuidFindBox.SuspendLayout();
            this.InwardCarrierAgentGuidFindBox.SuspendLayout();
            this.HandlingAgentGuidFindBox.SuspendLayout();
            this.ExporterGuidFindBox.SuspendLayout();
            this.EndUserGuidFindBox.SuspendLayout();
            this.ForwarderGuidFindBox.SuspendLayout();
            this.ClaimantGuidFindBox.SuspendLayout();
            this.SGDeclarationTabControl.SuspendLayout();
            this.zTabPage1.SuspendLayout();
            this.zPanel1.SuspendLayout();
            this.CPCGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CPCGrid)).BeginInit();
            this.CPCGrid.SuspendLayout();
            this.zGroupBox10.SuspendLayout();
            this.zGroupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CALicencesGrid)).BeginInit();
            this.CALicencesGrid.SuspendLayout();
            this.zGroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TradersRemarksGrid)).BeginInit();
            this.TradersRemarksGrid.SuspendLayout();
            this.TradersRemarksTextGroupBox.SuspendLayout();
            this.CertificateOfOriginTabPage.SuspendLayout();
            this.zPanel2.SuspendLayout();
            this.CofOGroupBox.SuspendLayout();
            this.zGroupBox7.SuspendLayout();
            this.zDropEdit2.SuspendLayout();
            this.zGroupBox8.SuspendLayout();
            this.zCodeFindBox2.SuspendLayout();
            this.DonorCountryCodeFindBox.SuspendLayout();
            this.zDropEdit3.SuspendLayout();
            this.ManufacturerGuidFindBox.SuspendLayout();
            this.zGroupBox4.SuspendLayout();
            this.zDropEdit4.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.SG.V4.Business.JobDeclaration);
            // 
            // FinalPortOfCallFindBox
            // 
            this.FinalPortOfCallFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.FinalPortOfCallFindBox, "SG_RL_NKFinalPortOfCall");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_RL_NKFinalPortOfCall)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.SGLocoList)));
            this.FinalPortOfCallFindBox.BindToList = "Lookups+SGLocoList";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FinalPortOfCallFindBox, false);
            this.FinalPortOfCallFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 42, true);
            this.FinalPortOfCallFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
            this.FinalPortOfCallFindBox.Name = "FinalPortOfCallFindBox";
            this.FinalPortOfCallFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.FinalPortOfCallFindBox.ParentType = null;
            this.FinalPortOfCallFindBox.PreBoundMaxLength = 5;
            this.FinalPortOfCallFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 15, true);
            this.FinalPortOfCallFindBox.TabIndex = 3;
            // 
            // FinalDestinationCountryFindBox
            // 
            this.FinalDestinationCountryFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.FinalDestinationCountryFindBox, "SG_RN_NKFinalDestination");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_RN_NKFinalDestination)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.CountryCodeList)));
            this.FinalDestinationCountryFindBox.BindToList = "Lookups+CountryCodeList";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FinalDestinationCountryFindBox, false);
            this.FinalDestinationCountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 65, true);
            this.FinalDestinationCountryFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
            this.FinalDestinationCountryFindBox.Name = "FinalDestinationCountryFindBox";
            this.FinalDestinationCountryFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.FinalDestinationCountryFindBox.ParentType = null;
            this.FinalDestinationCountryFindBox.PreBoundMaxLength = 2;
            this.FinalDestinationCountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 15, true);
            this.FinalDestinationCountryFindBox.TabIndex = 5;
            // 
            // NextPortOfCallFindBox
            // 
            this.NextPortOfCallFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.NextPortOfCallFindBox, "SG_RL_NKNextPortOfCall");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_RL_NKNextPortOfCall)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.SGLocoList)));
            this.NextPortOfCallFindBox.BindToList = "Lookups+SGLocoList";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NextPortOfCallFindBox, false);
            this.NextPortOfCallFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 19, true);
            this.NextPortOfCallFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
            this.NextPortOfCallFindBox.Name = "NextPortOfCallFindBox";
            this.NextPortOfCallFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.NextPortOfCallFindBox.ParentType = null;
            this.NextPortOfCallFindBox.PreBoundMaxLength = 5;
            this.NextPortOfCallFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 15, true);
            this.NextPortOfCallFindBox.TabIndex = 1;
            // 
            // zLabel11
            // 
            this.zLabel11.AutoSize = true;
            this.zLabel11.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("5FB18934-9311-4350-9E08-BE44D185FC58", "Final Destination:");
            this.zLabel11.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 69, true);
            this.zLabel11.Name = "zLabel11";
            this.zLabel11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 12, true);
            this.zLabel11.TabIndex = 4;
            this.zLabel11.UseMnemonic = false;
            // 
            // zLabel10
            // 
            this.zLabel10.AutoSize = true;
            this.zLabel10.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("96EB2633-D297-441B-8F34-DEC3215395C9", "Final Port of Call:");
            this.zLabel10.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 46, true);
            this.zLabel10.Name = "zLabel10";
            this.zLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 12, true);
            this.zLabel10.TabIndex = 2;
            this.zLabel10.UseMnemonic = false;
            // 
            // zLabel9
            // 
            this.zLabel9.AutoSize = true;
            this.zLabel9.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("57EE2B70-9865-4275-A1D4-EEED75529852", "Next Port of Call:");
            this.zLabel9.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 23, true);
            this.zLabel9.Name = "zLabel9";
            this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 12, true);
            this.zLabel9.TabIndex = 0;
            this.zLabel9.UseMnemonic = false;
            // 
            // zGroupBox1
            // 
            this.zGroupBox1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("E51AFA56-4930-4B60-93A5-EA5C4ECA2020", "Places");
            this.zGroupBox1.Controls.Add(this.zLabel15);
            this.zGroupBox1.Controls.Add(this.PlaceOfStorageFindBox);
            this.zGroupBox1.Controls.Add(this.PlaceOfReleaseFindBox);
            this.zGroupBox1.Controls.Add(this.PlaceOfReceiptFindBox);
            this.zGroupBox1.Controls.Add(this.zLabel7);
            this.zGroupBox1.Controls.Add(this.zLabel8);
            this.zGroupBox1.Controls.Add(this.zLabel11);
            this.zGroupBox1.Controls.Add(this.FinalDestinationCountryFindBox);
            this.zGroupBox1.Controls.Add(this.zLabel10);
            this.zGroupBox1.Controls.Add(this.FinalPortOfCallFindBox);
            this.zGroupBox1.Controls.Add(this.zLabel9);
            this.zGroupBox1.Controls.Add(this.NextPortOfCallFindBox);
            this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 259, true);
            this.zGroupBox1.Name = "zGroupBox1";
            this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 161, true);
            this.zGroupBox1.TabIndex = 1;
            this.zGroupBox1.TabStop = false;
            // 
            // zLabel15
            // 
            this.zLabel15.AutoSize = true;
            this.zLabel15.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("088D7F36-F8D7-4194-B91F-5CAB6738BA6A", "Place of Release:");
            this.zLabel15.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel15.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 92, true);
            this.zLabel15.Name = "zLabel15";
            this.zLabel15.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 12, true);
            this.zLabel15.TabIndex = 6;
            this.zLabel15.UseMnemonic = false;
            // 
            // PlaceOfStorageFindBox
            // 
            this.PlaceOfStorageFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PlaceOfStorageFindBox, "SG_US_NKPlaceOfStorage");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_US_NKPlaceOfStorage)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.SGPlacesCodeList)));
            this.PlaceOfStorageFindBox.BindToList = "Lookups+SGPlacesCodeList";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PlaceOfStorageFindBox, false);
            this.PlaceOfStorageFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 134, true);
            this.PlaceOfStorageFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
            this.PlaceOfStorageFindBox.Name = "PlaceOfStorageFindBox";
            this.PlaceOfStorageFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.PlaceOfStorageFindBox.ParentType = null;
            this.PlaceOfStorageFindBox.PreBoundMaxLength = 6;
            this.PlaceOfStorageFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 15, true);
            this.PlaceOfStorageFindBox.TabIndex = 11;
            // 
            // PlaceOfReleaseFindBox
            // 
            this.PlaceOfReleaseFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PlaceOfReleaseFindBox, "SG_US_NKPlaceOfCargoRelease");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_US_NKPlaceOfCargoRelease)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.SGPlacesCodeList)));
            this.PlaceOfReleaseFindBox.BindToList = "Lookups+SGPlacesCodeList";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PlaceOfReleaseFindBox, false);
            this.PlaceOfReleaseFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 88, true);
            this.PlaceOfReleaseFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
            this.PlaceOfReleaseFindBox.Name = "PlaceOfReleaseFindBox";
            this.PlaceOfReleaseFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.PlaceOfReleaseFindBox.ParentType = null;
            this.PlaceOfReleaseFindBox.PreBoundMaxLength = 6;
            this.PlaceOfReleaseFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 15, true);
            this.PlaceOfReleaseFindBox.TabIndex = 7;
            // 
            // PlaceOfReceiptFindBox
            // 
            this.PlaceOfReceiptFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PlaceOfReceiptFindBox, "SG_US_NKPlaceOfReceipt");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_US_NKPlaceOfReceipt)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.SGPlacesCodeList)));
            this.PlaceOfReceiptFindBox.BindToList = "Lookups+SGPlacesCodeList";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PlaceOfReceiptFindBox, false);
            this.PlaceOfReceiptFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 111, true);
            this.PlaceOfReceiptFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
            this.PlaceOfReceiptFindBox.Name = "PlaceOfReceiptFindBox";
            this.PlaceOfReceiptFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.PlaceOfReceiptFindBox.ParentType = null;
            this.PlaceOfReceiptFindBox.PreBoundMaxLength = 6;
            this.PlaceOfReceiptFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 15, true);
            this.PlaceOfReceiptFindBox.TabIndex = 9;
            // 
            // zLabel7
            // 
            this.zLabel7.AutoSize = true;
            this.zLabel7.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("60E22B65-AF9A-4FB1-9841-00569B5091E7", "Place of Receipt:");
            this.zLabel7.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 115, true);
            this.zLabel7.Name = "zLabel7";
            this.zLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 12, true);
            this.zLabel7.TabIndex = 8;
            this.zLabel7.UseMnemonic = false;
            // 
            // zLabel8
            // 
            this.zLabel8.AutoSize = true;
            this.zLabel8.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("691CE16E-DA5B-4E16-8121-ACE5B1B6425A", "Place of Storage:");
            this.zLabel8.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 138, true);
            this.zLabel8.Name = "zLabel8";
            this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 12, true);
            this.zLabel8.TabIndex = 10;
            this.zLabel8.UseMnemonic = false;
            // 
            // zGroupBox5
            // 
            this.zGroupBox5.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("6530017E-F2E5-4CF2-AE10-A6B6204C0129", "Other");
            this.zGroupBox5.Controls.Add(this.zCheckBox3);
            this.zGroupBox5.Controls.Add(this.zCheckBox1);
            this.zGroupBox5.Controls.Add(this.zDateEdit2);
            this.zGroupBox5.Controls.Add(this.zTextBox5);
            this.zGroupBox5.Controls.Add(this.zTextBox4);
            this.zGroupBox5.Controls.Add(this.zDateEdit1);
            this.zGroupBox5.Controls.Add(this.zDropEdit1);
            this.zGroupBox5.Controls.Add(this.zTextBox1);
            this.zGroupBox5.Controls.Add(this.zCodeFindBox1);
            this.zGroupBox5.Controls.Add(this.zLabel28);
            this.zGroupBox5.Controls.Add(this.zLabel27);
            this.zGroupBox5.Controls.Add(this.zLabel26);
            this.zGroupBox5.Controls.Add(this.zLabel25);
            this.zGroupBox5.Controls.Add(this.StartDateLabel);
            this.zGroupBox5.Controls.Add(this.zLabel23);
            this.zGroupBox5.Controls.Add(this.zLabel16);
            this.zGroupBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(442, 3, true);
            this.zGroupBox5.Name = "zGroupBox5";
            this.zGroupBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 251, true);
            this.zGroupBox5.TabIndex = 3;
            this.zGroupBox5.TabStop = false;
            // 
            // zCheckBox3
            // 
            this.BindingSource.SetBindingMember(this.zCheckBox3, "SG_GoodsPreviouslyExemptedFromDuties");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_GoodsPreviouslyExemptedFromDuties)));
            this.zCheckBox3.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("9643A721-96F4-4646-B247-F554D26F9D30", "Were goods previously exempted from duties?");
            this.zCheckBox3.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
            this.zCheckBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 199, true);
            this.zCheckBox3.Name = "zCheckBox3";
            this.zCheckBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 17, true);
            this.zCheckBox3.TabIndex = 18;
            this.zCheckBox3.UseVisualStyleBackColor = true;
            // 
            // zCheckBox1
            // 
            this.BindingSource.SetBindingMember(this.zCheckBox1, "SG_GoodsImportedUnderMESorBWS");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_GoodsImportedUnderMESorBWS)));
            this.zCheckBox1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("8AB2EBE8-5630-497F-B52A-D3907607E7C2", "Were goods imported under MES or BWS?");
            this.zCheckBox1.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
            this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 176, true);
            this.zCheckBox1.Name = "zCheckBox1";
            this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 17, true);
            this.zCheckBox1.TabIndex = 17;
            this.zCheckBox1.UseVisualStyleBackColor = true;
            // 
            // zDateEdit2
            // 
            this.zDateEdit2.AllowDrop = true;
            this.zDateEdit2.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.zDateEdit2, "SG_EndDateTempImport");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_EndDateTempImport)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDateEdit2, false);
            this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 151, true);
            this.zDateEdit2.Name = "zDateEdit2";
            this.zDateEdit2.TabIndex = 13;
            // 
            // zTextBox5
            // 
            this.BindingSource.SetBindingMember(this.zTextBox5, "SG_ReplacementPermitNo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_ReplacementPermitNo)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox5, false);
            this.zTextBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 80, true);
            this.zTextBox5.Name = "zTextBox5";
            this.zTextBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 15, true);
            this.zTextBox5.TabIndex = 7;
            // 
            // zTextBox4
            // 
            this.BindingSource.SetBindingMember(this.zTextBox4, "SG_PreviousPermitNo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_PreviousPermitNo)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox4, false);
            this.zTextBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 57, true);
            this.zTextBox4.Name = "zTextBox4";
            this.zTextBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 15, true);
            this.zTextBox4.TabIndex = 5;
            // 
            // zDateEdit1
            // 
            this.zDateEdit1.AllowDrop = true;
            this.zDateEdit1.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.zDateEdit1, "SG_RemovalStartDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_RemovalStartDate)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDateEdit1, false);
            this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 128, true);
            this.zDateEdit1.Name = "zDateEdit1";
            this.zDateEdit1.TabIndex = 11;
            // 
            // zDropEdit1
            // 
            this.zDropEdit1.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEdit1, "SG_SupplyIndicator");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_SupplyIndicator)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.SupplyIndicators)));
            this.zDropEdit1.BindToList = "Lookups+SupplyIndicators";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDropEdit1, false);
            this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 103, true);
            this.zDropEdit1.Name = "zDropEdit1";
            this.zDropEdit1.PreBoundMaxLength = 1;
            this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 15, true);
            this.zDropEdit1.TabIndex = 9;
            // 
            // zTextBox1
            // 
            this.BindingSource.SetBindingMember(this.zTextBox1, "SG_TowingVoyageNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_TowingVoyageNumber)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox1, false);
            this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 34, true);
            this.zTextBox1.Name = "zTextBox1";
            this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 15, true);
            this.zTextBox1.TabIndex = 3;
            // 
            // zCodeFindBox1
            // 
            this.zCodeFindBox1.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zCodeFindBox1, "SG_TowingVesselName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_TowingVesselName)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.Vessels)));
            this.zCodeFindBox1.BindToList = "Lookups+Vessels";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCodeFindBox1, false);
            this.zCodeFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 11, true);
            this.zCodeFindBox1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefVessel;
            this.zCodeFindBox1.Name = "zCodeFindBox1";
            this.zCodeFindBox1.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.zCodeFindBox1.ParentType = null;
            this.zCodeFindBox1.PreBoundMaxLength = 35;
            this.zCodeFindBox1.ShowDescriptionBox = false;
            this.zCodeFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 15, true);
            this.zCodeFindBox1.TabIndex = 1;
            // 
            // zLabel28
            // 
            this.zLabel28.AutoSize = true;
            this.zLabel28.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("4D9CC43D-59BE-4BE9-B8A6-AE91EC37290C", "Towing Voyage No.:");
            this.zLabel28.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel28.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 39, true);
            this.zLabel28.Name = "zLabel28";
            this.zLabel28.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 12, true);
            this.zLabel28.TabIndex = 2;
            this.zLabel28.UseMnemonic = false;
            // 
            // zLabel27
            // 
            this.zLabel27.AutoSize = true;
            this.zLabel27.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("218C583E-1E7D-4AED-836B-DCBF87E84009", "Towing Vessel:");
            this.zLabel27.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel27.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 16, true);
            this.zLabel27.Name = "zLabel27";
            this.zLabel27.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 12, true);
            this.zLabel27.TabIndex = 0;
            this.zLabel27.UseMnemonic = false;
            // 
            // zLabel26
            // 
            this.zLabel26.AutoSize = true;
            this.zLabel26.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("1464B066-CCAC-46B1-BD73-1BD91837E399", "Supply Indicator:");
            this.zLabel26.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel26.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 108, true);
            this.zLabel26.Name = "zLabel26";
            this.zLabel26.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 12, true);
            this.zLabel26.TabIndex = 8;
            this.zLabel26.UseMnemonic = false;
            // 
            // zLabel25
            // 
            this.zLabel25.AutoSize = true;
            this.zLabel25.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("53E8BAFA-75E3-4372-B797-92EF525E1CE1", "Replacement Permit No.:");
            this.zLabel25.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel25.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 85, true);
            this.zLabel25.Name = "zLabel25";
            this.zLabel25.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 12, true);
            this.zLabel25.TabIndex = 6;
            this.zLabel25.UseMnemonic = false;
            // 
            // StartDateLabel
            // 
            this.StartDateLabel.AutoSize = true;
            this.StartDateLabel.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("51D11372-A4AD-43F2-8BC9-0E11A1CFB370", "Removal Start Date:");
            this.StartDateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.StartDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 131, true);
            this.StartDateLabel.Name = "StartDateLabel";
            this.StartDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 12, true);
            this.StartDateLabel.TabIndex = 10;
            this.StartDateLabel.UseMnemonic = false;
            // 
            // zLabel23
            // 
            this.zLabel23.AutoSize = true;
            this.zLabel23.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("5FC22398-F71E-443B-BE79-E7D9C8DBFF89", "Previous Permit No.:");
            this.zLabel23.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel23.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 62, true);
            this.zLabel23.Name = "zLabel23";
            this.zLabel23.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 12, true);
            this.zLabel23.TabIndex = 4;
            this.zLabel23.UseMnemonic = false;
            // 
            // zLabel16
            // 
            this.zLabel16.AutoSize = true;
            this.zLabel16.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("63CCAA9E-8CD8-43CB-9B81-95246A85066A", "End Date of Exhibition/Temporary Import Period:");
            this.zLabel16.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel16.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 154, true);
            this.zLabel16.Name = "zLabel16";
            this.zLabel16.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 12, true);
            this.zLabel16.TabIndex = 12;
            this.zLabel16.UseMnemonic = false;
            // 
            // zGroupBox6
            // 
            this.zGroupBox6.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("E929402C-F619-4DAA-BBE0-AE4C4FA68B14", "Organization Details:");
            this.zGroupBox6.Controls.Add(this.DutyExemptCheckBox);
            this.zGroupBox6.Controls.Add(this.OutwardCarrierAgentGuidFindBox);
            this.zGroupBox6.Controls.Add(this.ConsigneeGuidFindBox);
            this.zGroupBox6.Controls.Add(this.InwardCarrierAgentGuidFindBox);
            this.zGroupBox6.Controls.Add(this.zLabel19);
            this.zGroupBox6.Controls.Add(this.zLabel14);
            this.zGroupBox6.Controls.Add(this.zLabel22);
            this.zGroupBox6.Controls.Add(this.HandlingAgentGuidFindBox);
            this.zGroupBox6.Controls.Add(this.ExporterGuidFindBox);
            this.zGroupBox6.Controls.Add(this.zLabel17);
            this.zGroupBox6.Controls.Add(this.EndUserGuidFindBox);
            this.zGroupBox6.Controls.Add(this.ForwarderGuidFindBox);
            this.zGroupBox6.Controls.Add(this.zTextBox3);
            this.zGroupBox6.Controls.Add(this.zLabel21);
            this.zGroupBox6.Controls.Add(this.zTextBox2);
            this.zGroupBox6.Controls.Add(this.ClaimantGuidFindBox);
            this.zGroupBox6.Controls.Add(this.zLabel18);
            this.zGroupBox6.Controls.Add(this.zLabel13);
            this.zGroupBox6.Controls.Add(this.zLabel12);
            this.zGroupBox6.Controls.Add(this.zLabel6);
            this.zGroupBox6.Controls.Add(this.zLabel20);
            this.zGroupBox6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.zGroupBox6.Name = "zGroupBox6";
            this.zGroupBox6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 251, true);
            this.zGroupBox6.TabIndex = 1;
            this.zGroupBox6.TabStop = false;
            // 
            // DutyExemptCheckBox
            // 
            this.DutyExemptCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.DutyExemptCheckBox, "SG_DutyExempt");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_DutyExempt)));
            this.DutyExemptCheckBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("A85122EA-5B09-4216-B630-594E35B5750E", "Duty Exemption");
            this.DutyExemptCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(306, 175, true);
            this.DutyExemptCheckBox.Name = "DutyExemptCheckBox";
            this.DutyExemptCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 14, true);
            this.DutyExemptCheckBox.TabIndex = 16;
            this.DutyExemptCheckBox.UseVisualStyleBackColor = true;
            // 
            // OutwardCarrierAgentGuidFindBox
            // 
            this.OutwardCarrierAgentGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.OutwardCarrierAgentGuidFindBox, "OutwardShippingLineForwarderPK");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).OutwardShippingLineForwarderPK)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.Organisations)));
            this.OutwardCarrierAgentGuidFindBox.BindToList = "Lookups+Organisations";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OutwardCarrierAgentGuidFindBox, false);
            this.OutwardCarrierAgentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 219, true);
            this.OutwardCarrierAgentGuidFindBox.Name = "OutwardCarrierAgentGuidFindBox";
            this.OutwardCarrierAgentGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.OutwardCarrierAgentGuidFindBox.ParentType = null;
            this.OutwardCarrierAgentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 15, true);
            this.OutwardCarrierAgentGuidFindBox.TabIndex = 20;
            // 
            // ConsigneeGuidFindBox
            // 
            this.ConsigneeGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ConsigneeGuidFindBox, "JE_OH_Consignee");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).JE_OH_Consignee)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.ImportersList)));
            this.ConsigneeGuidFindBox.BindToList = "Lookups+ImportersList";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConsigneeGuidFindBox, false);
            this.ConsigneeGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 58, true);
            this.ConsigneeGuidFindBox.Name = "ConsigneeGuidFindBox";
            this.ConsigneeGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.ConsigneeGuidFindBox.ParentType = null;
            this.ConsigneeGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 15, true);
            this.ConsigneeGuidFindBox.TabIndex = 5;
            // 
            // InwardCarrierAgentGuidFindBox
            // 
            this.InwardCarrierAgentGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.InwardCarrierAgentGuidFindBox, "JE_OH_InwardCarrierAgent");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).JE_OH_InwardCarrierAgent)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.InwardCarrierAgentGuidFindBox, false);
            this.InwardCarrierAgentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 196, true);
            this.InwardCarrierAgentGuidFindBox.Name = "InwardCarrierAgentGuidFindBox";
            this.InwardCarrierAgentGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.InwardCarrierAgentGuidFindBox.ParentType = null;
            this.InwardCarrierAgentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 15, true);
            this.InwardCarrierAgentGuidFindBox.TabIndex = 18;
            // 
            // zLabel19
            // 
            this.zLabel19.AutoSize = true;
            this.zLabel19.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("996C9CB4-FFCC-4ED4-8357-B7C64C40B710", "Outward Carrier Agent:");
            this.zLabel19.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel19.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 223, true);
            this.zLabel19.Name = "zLabel19";
            this.zLabel19.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 12, true);
            this.zLabel19.TabIndex = 19;
            this.zLabel19.UseMnemonic = false;
            // 
            // zLabel14
            // 
            this.zLabel14.AutoSize = true;
            this.zLabel14.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("39F21073-EF6E-4E39-8FF3-F7CD15B38143", "Consignee:");
            this.zLabel14.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel14.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 62, true);
            this.zLabel14.Name = "zLabel14";
            this.zLabel14.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 12, true);
            this.zLabel14.TabIndex = 4;
            this.zLabel14.UseMnemonic = false;
            // 
            // zLabel22
            // 
            this.zLabel22.AutoSize = true;
            this.zLabel22.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("50D2F6A8-B7AC-4E0B-B7BA-D8201CD009D8", "Inward Carrier Agent:");
            this.zLabel22.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel22.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 200, true);
            this.zLabel22.Name = "zLabel22";
            this.zLabel22.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 12, true);
            this.zLabel22.TabIndex = 17;
            this.zLabel22.UseMnemonic = false;
            // 
            // HandlingAgentGuidFindBox
            // 
            this.HandlingAgentGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.HandlingAgentGuidFindBox, "JE_OH_HandlingAgent");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).JE_OH_HandlingAgent)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.HandlingAgentGuidFindBox, false);
            this.HandlingAgentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 81, true);
            this.HandlingAgentGuidFindBox.Name = "HandlingAgentGuidFindBox";
            this.HandlingAgentGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.HandlingAgentGuidFindBox.ParentType = null;
            this.HandlingAgentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 15, true);
            this.HandlingAgentGuidFindBox.TabIndex = 7;
            // 
            // ExporterGuidFindBox
            // 
            this.ExporterGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ExporterGuidFindBox, "JE_OH_Exporter");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).JE_OH_Exporter)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.SuppliersList)));
            this.ExporterGuidFindBox.BindToList = "Lookups+SuppliersList";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ExporterGuidFindBox, false);
            this.ExporterGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 35, true);
            this.ExporterGuidFindBox.Name = "ExporterGuidFindBox";
            this.ExporterGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.ExporterGuidFindBox.ParentType = null;
            this.ExporterGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 15, true);
            this.ExporterGuidFindBox.TabIndex = 3;
            // 
            // zLabel17
            // 
            this.zLabel17.AutoSize = true;
            this.zLabel17.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("3C62F7AC-CBFF-4201-AFCA-6B75206F5C04", "End User:");
            this.zLabel17.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel17.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 108, true);
            this.zLabel17.Name = "zLabel17";
            this.zLabel17.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 12, true);
            this.zLabel17.TabIndex = 8;
            this.zLabel17.UseMnemonic = false;
            // 
            // EndUserGuidFindBox
            // 
            this.EndUserGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.EndUserGuidFindBox, "JE_OH_Buyer");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).JE_OH_Buyer)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.ImportersList)));
            this.EndUserGuidFindBox.BindToList = "Lookups+ImportersList";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EndUserGuidFindBox, false);
            this.EndUserGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 104, true);
            this.EndUserGuidFindBox.Name = "EndUserGuidFindBox";
            this.EndUserGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.EndUserGuidFindBox.ParentType = null;
            this.EndUserGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 15, true);
            this.EndUserGuidFindBox.TabIndex = 9;
            // 
            // ForwarderGuidFindBox
            // 
            this.ForwarderGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ForwarderGuidFindBox, "JE_OH_Forwarder");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).JE_OH_Forwarder)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.ForwarderList)));
            this.ForwarderGuidFindBox.BindToList = "Lookups+ForwarderList";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ForwarderGuidFindBox, false);
            this.ForwarderGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 12, true);
            this.ForwarderGuidFindBox.Name = "ForwarderGuidFindBox";
            this.ForwarderGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.ForwarderGuidFindBox.ParentType = null;
            this.ForwarderGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 15, true);
            this.ForwarderGuidFindBox.TabIndex = 1;
            // 
            // zTextBox3
            // 
            this.BindingSource.SetBindingMember(this.zTextBox3, "SG_ClaimantCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_ClaimantCode)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox3, false);
            this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 173, true);
            this.zTextBox3.Name = "zTextBox3";
            this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
            this.zTextBox3.TabIndex = 15;
            // 
            // zLabel21
            // 
            this.zLabel21.AutoSize = true;
            this.zLabel21.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("42717455-80D0-479F-ACE1-D23705B8BF18", "Handling Agent:");
            this.zLabel21.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel21.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 85, true);
            this.zLabel21.Name = "zLabel21";
            this.zLabel21.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 12, true);
            this.zLabel21.TabIndex = 6;
            this.zLabel21.UseMnemonic = false;
            // 
            // zTextBox2
            // 
            this.BindingSource.SetBindingMember(this.zTextBox2, "SG_ClaimantName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_ClaimantName)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox2, false);
            this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 150, true);
            this.zTextBox2.Name = "zTextBox2";
            this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 15, true);
            this.zTextBox2.TabIndex = 13;
            // 
            // ClaimantGuidFindBox
            // 
            this.ClaimantGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ClaimantGuidFindBox, "JE_OH_Claimant");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).JE_OH_Claimant)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ClaimantGuidFindBox, false);
            this.ClaimantGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 127, true);
            this.ClaimantGuidFindBox.Name = "ClaimantGuidFindBox";
            this.ClaimantGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.ClaimantGuidFindBox.ParentType = null;
            this.ClaimantGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 15, true);
            this.ClaimantGuidFindBox.TabIndex = 11;
            // 
            // zLabel18
            // 
            this.zLabel18.AutoSize = true;
            this.zLabel18.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("E7265CFC-7A60-47BC-BA93-7F15D138F787", "Exporter:");
            this.zLabel18.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel18.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 39, true);
            this.zLabel18.Name = "zLabel18";
            this.zLabel18.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 12, true);
            this.zLabel18.TabIndex = 2;
            this.zLabel18.UseMnemonic = false;
            // 
            // zLabel13
            // 
            this.zLabel13.AutoSize = true;
            this.zLabel13.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("02AA6C45-3FB9-4F6B-9B15-BB4686B46DAB", "Claimant Code:");
            this.zLabel13.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 177, true);
            this.zLabel13.Name = "zLabel13";
            this.zLabel13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 12, true);
            this.zLabel13.TabIndex = 14;
            this.zLabel13.UseMnemonic = false;
            // 
            // zLabel12
            // 
            this.zLabel12.AutoSize = true;
            this.zLabel12.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("842D869B-4F43-484F-B4ED-BAF0D3E34B20", "Claimant Name:");
            this.zLabel12.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 154, true);
            this.zLabel12.Name = "zLabel12";
            this.zLabel12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 12, true);
            this.zLabel12.TabIndex = 12;
            this.zLabel12.UseMnemonic = false;
            // 
            // zLabel6
            // 
            this.zLabel6.AutoSize = true;
            this.zLabel6.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("1CB67163-13D8-4C78-8D7B-C085C088D5F8", "Claimant (Org.):");
            this.zLabel6.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 131, true);
            this.zLabel6.Name = "zLabel6";
            this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 12, true);
            this.zLabel6.TabIndex = 10;
            this.zLabel6.UseMnemonic = false;
            // 
            // zLabel20
            // 
            this.zLabel20.AutoSize = true;
            this.zLabel20.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("9D8204B1-90C8-4568-A741-4E4BA91382AF", "Forwarder:");
            this.zLabel20.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel20.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
            this.zLabel20.Name = "zLabel20";
            this.zLabel20.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 12, true);
            this.zLabel20.TabIndex = 0;
            this.zLabel20.UseMnemonic = false;
            // 
            // SGDeclarationTabControl
            // 
            this.SGDeclarationTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.SGDeclarationTabControl.Controls.Add(this.zTabPage1);
            this.SGDeclarationTabControl.Controls.Add(this.CertificateOfOriginTabPage);
            this.SGDeclarationTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SGDeclarationTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.SGDeclarationTabControl.Name = "SGDeclarationTabControl";
            this.SGDeclarationTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1205, 672, true);
            this.SGDeclarationTabControl.TabIndex = 11;
            // 
            // zTabPage1
            // 
            this.zTabPage1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("2A7900FE-830E-40FC-8A7D-35682ED7C990", "Additional Details");
            this.zTabPage1.Controls.Add(this.zPanel1);
            this.zTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
            this.zTabPage1.Name = "zTabPage1";
            this.zTabPage1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.zTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1197, 649, true);
            this.zTabPage1.TabIndex = 0;
            // 
            // zPanel1
            // 
            this.zPanel1.Controls.Add(this.CPCGroupBox);
            this.zPanel1.Controls.Add(this.zGroupBox10);
            this.zPanel1.Controls.Add(this.zGroupBox3);
            this.zPanel1.Controls.Add(this.zGroupBox2);
            this.zPanel1.Controls.Add(this.zGroupBox6);
            this.zPanel1.Controls.Add(this.zGroupBox5);
            this.zPanel1.Controls.Add(this.zGroupBox1);
            this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.zPanel1.Name = "zPanel1";
            this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1191, 643, true);
            this.zPanel1.TabIndex = 0;
            // 
            // CPCGroupBox
            // 
            this.CPCGroupBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("B73BEB33-FC38-49F7-9513-A53288227ADA", "Customs Procedure Code (CPC)");
            this.CPCGroupBox.Controls.Add(this.CPCViewEditButton);
            this.CPCGroupBox.Controls.Add(this.CPCGrid);
            this.CPCGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(442, 259, true);
            this.CPCGroupBox.Name = "CPCGroupBox";
            this.CPCGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 161, true);
            this.CPCGroupBox.TabIndex = 5;
            this.CPCGroupBox.TabStop = false;
            // 
            // CPCViewEditButton
            // 
            this.CPCViewEditButton.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("DB8C3B76-6613-43BA-9801-ACD8C5D16A31", "View/Edit");
            this.CPCViewEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(463, 132, true);
            this.CPCViewEditButton.Name = "CPCViewEditButton";
            this.CPCViewEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.CPCViewEditButton.TabIndex = 2;
            this.CPCViewEditButton.ToolTipCaption = null;
            this.CPCViewEditButton.UseVisualStyleBackColor = true;
            this.CPCViewEditButton.Click += new System.EventHandler(this.CPCViewEditButton_Click);
            // 
            // CPCGrid
            // 
            this.CPCGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.CPCGrid, "CPCs");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).CPCs)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.SGCPC)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).CPCs)).SyncRoot)).SG_APCCodeDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.SGCPC)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).CPCs)).SyncRoot)).SG_CPCCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.SGCPC)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).CPCs)).SyncRoot)).SG_PC1)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.SGCPC)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).CPCs)).SyncRoot)).SG_PC2)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.SGCPC)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).CPCs)).SyncRoot)).SG_PC3)));
            this.CPCGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("FDFD1877-49B5-4BE4-824C-E7F13586CC2D", "APC CPC");
            zDropEditColumnStyleInfo1.ColumnName = "SG_APCCodeDescription";
            zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
            zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("4079D008-3CF0-462F-9668-B1A08CE45DB4", "APC Code");
            zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            zDropEditColumnStyleInfo2.ColumnName = "SG_CPCCode";
            zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("BEE2D735-9BB3-4B7A-AB5E-2AFB47D41E16", "Processing Code 1");
            zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo1.ColumnName = "SG_PC1";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("8697F070-155D-41D6-B2AB-5F393E1FB5FF", "Processing Code 2");
            zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo2.ColumnName = "SG_PC2";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("568CF091-2AE4-4E35-9ADB-AE7764AED40C", "Processing Code 3");
            zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo3.ColumnName = "SG_PC3";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            this.CPCGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.CPCGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.CPCGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.CPCGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.CPCGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.CPCGrid.Dock = System.Windows.Forms.DockStyle.Top;
            this.CPCGrid.GridId = "38a9d85c-cdc8-4cf8-8a9f-faca69090037";
            this.CPCGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.CPCGrid.LayoutKey = "CALicencesGrid";
            this.CPCGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
            this.CPCGrid.Name = "CPCGrid";
            this.CPCGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 116, true);
            this.CPCGrid.TabIndex = 1;
            this.CPCGrid.DoubleClick += new System.EventHandler(this.CPCViewEditButton_Click);
            // 
            // zGroupBox10
            // 
            this.zGroupBox10.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("F21768B7-0D88-4156-B4DC-B78A6889018D", "Payment Summary");
            this.zGroupBox10.Controls.Add(this.zCalcEdit11);
            this.zGroupBox10.Controls.Add(this.zLabel47);
            this.zGroupBox10.Controls.Add(this.TotalOtherTaxCalcEdit);
            this.zGroupBox10.Controls.Add(this.zCalcEdit8);
            this.zGroupBox10.Controls.Add(this.zCalcEdit7);
            this.zGroupBox10.Controls.Add(this.zCalcEdit6);
            this.zGroupBox10.Controls.Add(this.zLabel45);
            this.zGroupBox10.Controls.Add(this.zLabel44);
            this.zGroupBox10.Controls.Add(this.zLabel43);
            this.zGroupBox10.Controls.Add(this.zLabel42);
            this.zGroupBox10.Controls.Add(this.TotalOtherTaxLabel);
            this.zGroupBox10.Controls.Add(this.zCalcEdit4);
            this.zGroupBox10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(784, 3, true);
            this.zGroupBox10.Name = "zGroupBox10";
            this.zGroupBox10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 161, true);
            this.zGroupBox10.TabIndex = 6;
            this.zGroupBox10.TabStop = false;
            // 
            // zCalcEdit11
            // 
            this.BindingSource.SetBindingMember(this.zCalcEdit11, "TotalCustomsValue");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).TotalCustomsValue)));
            this.zCalcEdit11.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit11, false);
            this.zCalcEdit11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 135, true);
            this.zCalcEdit11.Name = "zCalcEdit11";
            this.zCalcEdit11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
            this.zCalcEdit11.TabIndex = 11;
            this.zCalcEdit11.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.zCalcEdit11.TrackDisposedAccess = true;
            // 
            // zLabel47
            // 
            this.zLabel47.AutoSize = true;
            this.zLabel47.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("91BE06D8-A830-408C-AD52-DDDC291C00F3", "Customs Value:");
            this.zLabel47.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel47.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 139, true);
            this.zLabel47.Name = "zLabel47";
            this.zLabel47.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 12, true);
            this.zLabel47.TabIndex = 10;
            this.zLabel47.UseMnemonic = false;
            // 
            // TotalOtherTaxCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.TotalOtherTaxCalcEdit, "TotalOtherTaxPayable");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).TotalOtherTaxPayable)));
            this.TotalOtherTaxCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalOtherTaxCalcEdit, false);
            this.TotalOtherTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 112, true);
            this.TotalOtherTaxCalcEdit.Name = "TotalOtherTaxCalcEdit";
            this.TotalOtherTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
            this.TotalOtherTaxCalcEdit.TabIndex = 9;
            this.TotalOtherTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TotalOtherTaxCalcEdit.TrackDisposedAccess = true;
            // 
            // zCalcEdit8
            // 
            this.BindingSource.SetBindingMember(this.zCalcEdit8, "TotalExcisePayable");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).TotalExcisePayable)));
            this.zCalcEdit8.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit8, false);
            this.zCalcEdit8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 89, true);
            this.zCalcEdit8.Name = "zCalcEdit8";
            this.zCalcEdit8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
            this.zCalcEdit8.TabIndex = 7;
            this.zCalcEdit8.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.zCalcEdit8.TrackDisposedAccess = true;
            // 
            // zCalcEdit7
            // 
            this.BindingSource.SetBindingMember(this.zCalcEdit7, "TotalGSTPayable");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).TotalGSTPayable)));
            this.zCalcEdit7.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit7, false);
            this.zCalcEdit7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 66, true);
            this.zCalcEdit7.Name = "zCalcEdit7";
            this.zCalcEdit7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
            this.zCalcEdit7.TabIndex = 5;
            this.zCalcEdit7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.zCalcEdit7.TrackDisposedAccess = true;
            // 
            // zCalcEdit6
            // 
            this.BindingSource.SetBindingMember(this.zCalcEdit6, "TotalDutyPayable");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).TotalDutyPayable)));
            this.zCalcEdit6.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit6, false);
            this.zCalcEdit6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 43, true);
            this.zCalcEdit6.Name = "zCalcEdit6";
            this.zCalcEdit6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
            this.zCalcEdit6.TabIndex = 3;
            this.zCalcEdit6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.zCalcEdit6.TrackDisposedAccess = true;
            // 
            // zLabel45
            // 
            this.zLabel45.AutoSize = true;
            this.zLabel45.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("17D3701E-E34F-4DB5-B6CA-CCD8FA3019E0", "Total Payable:");
            this.zLabel45.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel45.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 24, true);
            this.zLabel45.Name = "zLabel45";
            this.zLabel45.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 12, true);
            this.zLabel45.TabIndex = 0;
            this.zLabel45.UseMnemonic = false;
            // 
            // zLabel44
            // 
            this.zLabel44.AutoSize = true;
            this.zLabel44.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("502FAC3E-8067-4381-AECD-A0702A75857D", "Total Duty:");
            this.zLabel44.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel44.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 47, true);
            this.zLabel44.Name = "zLabel44";
            this.zLabel44.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 12, true);
            this.zLabel44.TabIndex = 2;
            this.zLabel44.UseMnemonic = false;
            // 
            // zLabel43
            // 
            this.zLabel43.AutoSize = true;
            this.zLabel43.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("85E88DC4-0700-4317-B1E0-AD4395443026", "Total GST:");
            this.zLabel43.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel43.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 70, true);
            this.zLabel43.Name = "zLabel43";
            this.zLabel43.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 12, true);
            this.zLabel43.TabIndex = 4;
            this.zLabel43.UseMnemonic = false;
            // 
            // zLabel42
            // 
            this.zLabel42.AutoSize = true;
            this.zLabel42.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("E89455AA-66B5-43C6-A594-8C47B85468B8", "Total Excise:");
            this.zLabel42.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel42.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 93, true);
            this.zLabel42.Name = "zLabel42";
            this.zLabel42.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 12, true);
            this.zLabel42.TabIndex = 6;
            this.zLabel42.UseMnemonic = false;
            // 
            // TotalOtherTaxLabel
            // 
            this.TotalOtherTaxLabel.AutoSize = true;
            this.TotalOtherTaxLabel.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("A4380015-9A16-4B50-8C80-C9F5F1984465", "Total Other Tax:");
            this.TotalOtherTaxLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.TotalOtherTaxLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 116, true);
            this.TotalOtherTaxLabel.Name = "TotalOtherTaxLabel";
            this.TotalOtherTaxLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 12, true);
            this.TotalOtherTaxLabel.TabIndex = 8;
            this.TotalOtherTaxLabel.UseMnemonic = false;
            // 
            // zCalcEdit4
            // 
            this.BindingSource.SetBindingMember(this.zCalcEdit4, "TotalPayable");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).TotalPayable)));
            this.zCalcEdit4.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit4, false);
            this.zCalcEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 20, true);
            this.zCalcEdit4.Name = "zCalcEdit4";
            this.zCalcEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
            this.zCalcEdit4.TabIndex = 1;
            this.zCalcEdit4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.zCalcEdit4.TrackDisposedAccess = true;
            // 
            // zGroupBox3
            // 
            this.zGroupBox3.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("EA18F10C-9EF6-49FF-B5AB-FAC489C11AB7", "CA Licenses");
            this.zGroupBox3.Controls.Add(this.CALicencesGrid);
            this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(785, 170, true);
            this.zGroupBox3.Name = "zGroupBox3";
            this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 84, true);
            this.zGroupBox3.TabIndex = 4;
            this.zGroupBox3.TabStop = false;
            // 
            // CALicencesGrid
            // 
            this.CALicencesGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.CALicencesGrid, "CALicences");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).CALicences)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.CALicenceNumber)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).CALicences)).SyncRoot)).CY_Data)));
            this.CALicencesGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("DBDEDDD1-C1AD-4864-9B43-8B926132C698", "License Number");
            zTextBoxColumnStyleInfo4.ColumnName = "CY_Data";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.IsMandatory = true;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            this.CALicencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.CALicencesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CALicencesGrid.GridId = "6222d27e-fb52-4994-a0f2-18442708682a";
            this.CALicencesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.CALicencesGrid.LayoutKey = "CALicencesGrid";
            this.CALicencesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
            this.CALicencesGrid.Name = "CALicencesGrid";
            this.CALicencesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 69, true);
            this.CALicencesGrid.TabIndex = 0;
            // 
            // zGroupBox2
            // 
            this.zGroupBox2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("0B25C71F-05AC-4968-AE1A-103E83EAF76C", "Traders Remarks");
            this.zGroupBox2.Controls.Add(this.TradersRemarksGrid);
            this.zGroupBox2.Controls.Add(this.TradersRemarksTextGroupBox);
            this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 426, true);
            this.zGroupBox2.Name = "zGroupBox2";
            this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(983, 111, true);
            this.zGroupBox2.TabIndex = 2;
            this.zGroupBox2.TabStop = false;
            // 
            // TradersRemarksGrid
            // 
            this.TradersRemarksGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.TradersRemarksGrid, "TradersRemarks");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).TradersRemarks)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.TradersRemark)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).TradersRemarks)).SyncRoot)).CSI_Description)));
            this.TradersRemarksGrid.CaptionVisible = false;
            this.TradersRemarksGrid.ColumnHeadersVisible = false;
            zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo5.ColumnName = "CSI_Description";
            zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo5.IsSortable = false;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(560);
            this.TradersRemarksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.TradersRemarksGrid.GridId = "40e12684-e5af-402d-9a5a-95e9138716b5";
            this.TradersRemarksGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.TradersRemarksGrid.LayoutKey = "TradersRemarksGrid";
            this.TradersRemarksGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 14, true);
            this.TradersRemarksGrid.MaximumRows = 5;
            this.TradersRemarksGrid.Name = "TradersRemarksGrid";
            this.TradersRemarksGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 94, true);
            this.TradersRemarksGrid.TabIndex = 2;
            // 
            // TradersRemarksTextGroupBox
            // 
            this.TradersRemarksTextGroupBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("2E2DFE28-E4F5-4C31-866D-A338599336DD", "Remark Text");
            this.TradersRemarksTextGroupBox.Controls.Add(this.TradersRemarksTextBox);
            this.TradersRemarksTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(618, 8, true);
            this.TradersRemarksTextGroupBox.Name = "TradersRemarksTextGroupBox";
            this.TradersRemarksTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(359, 100, true);
            this.TradersRemarksTextGroupBox.TabIndex = 4;
            this.TradersRemarksTextGroupBox.TabStop = false;
            // 
            // TradersRemarksTextBox
            // 
            this.BindingSource.SetBindingMember(this.TradersRemarksTextBox, "TradersRemarks.CSI_Description");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.TradersRemark)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).TradersRemarks)).SyncRoot)).CSI_Description)));
            this.TradersRemarksTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("6cac03ab-a3c8-476f-962e-87d6312ace94", "Remark text");
            this.TradersRemarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.TradersRemarksTextBox.Multiline = true;
            this.TradersRemarksTextBox.Name = "TradersRemarksTextBox";
            this.TradersRemarksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(354, 81, true);
            this.TradersRemarksTextBox.TabIndex = 3;
            // 
            // CertificateOfOriginTabPage
            // 
            this.CertificateOfOriginTabPage.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("20FB8A18-254E-4259-93F2-6C4BC3388F5C", "Certificate of Origin");
            this.CertificateOfOriginTabPage.Controls.Add(this.zPanel2);
            this.CertificateOfOriginTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
            this.CertificateOfOriginTabPage.Name = "CertificateOfOriginTabPage";
            this.CertificateOfOriginTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.CertificateOfOriginTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1197, 649, true);
            this.CertificateOfOriginTabPage.TabIndex = 1;
            // 
            // zPanel2
            // 
            this.zPanel2.Controls.Add(this.CofOGroupBox);
            this.zPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.zPanel2.Name = "zPanel2";
            this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1191, 643, true);
            this.zPanel2.TabIndex = 1;
            // 
            // CofOGroupBox
            // 
            this.CofOGroupBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("68990A78-E294-4232-B429-9DF0C83A09DB", "Certificate of Origin Details");
            this.CofOGroupBox.Controls.Add(this.zGroupBox7);
            this.CofOGroupBox.Controls.Add(this.zGroupBox8);
            this.CofOGroupBox.Controls.Add(this.zGroupBox4);
            this.CofOGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CofOGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.CofOGroupBox.Name = "CofOGroupBox";
            this.CofOGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1191, 643, true);
            this.CofOGroupBox.TabIndex = 0;
            this.CofOGroupBox.TabStop = false;
            // 
            // zGroupBox7
            // 
            this.zGroupBox7.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("6BD51F2F-5565-4A38-8A2A-D000C55B35EB", "Certificate 2");
            this.zGroupBox7.Controls.Add(this.zLabel36);
            this.zGroupBox7.Controls.Add(this.zDropEdit2);
            this.zGroupBox7.Controls.Add(this.zLabel40);
            this.zGroupBox7.Controls.Add(this.zCalcEdit5);
            this.zGroupBox7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 245, true);
            this.zGroupBox7.Name = "zGroupBox7";
            this.zGroupBox7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 102, true);
            this.zGroupBox7.TabIndex = 2;
            this.zGroupBox7.TabStop = false;
            // 
            // zLabel36
            // 
            this.zLabel36.AutoSize = true;
            this.zLabel36.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("664CCFE0-6247-4567-9DD1-73B5221EE1BD", "Cert. Type:");
            this.zLabel36.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel36.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 20, true);
            this.zLabel36.Name = "zLabel36";
            this.zLabel36.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 12, true);
            this.zLabel36.TabIndex = 0;
            this.zLabel36.UseMnemonic = false;
            // 
            // zDropEdit2
            // 
            this.zDropEdit2.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEdit2, "SG_Cert2Type");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_Cert2Type)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.CertificateTypes)));
            this.zDropEdit2.BindToList = "Lookups+CertificateTypes";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDropEdit2, false);
            this.zDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 16, true);
            this.zDropEdit2.Name = "zDropEdit2";
            this.zDropEdit2.PreBoundMaxLength = 2;
            this.zDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 15, true);
            this.zDropEdit2.TabIndex = 1;
            // 
            // zLabel40
            // 
            this.zLabel40.AutoSize = true;
            this.zLabel40.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("A89674A8-7F1C-4E1E-B8F9-231DF5E014AC", "Copies:");
            this.zLabel40.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel40.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 46, true);
            this.zLabel40.Name = "zLabel40";
            this.zLabel40.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 12, true);
            this.zLabel40.TabIndex = 2;
            this.zLabel40.UseMnemonic = false;
            // 
            // zCalcEdit5
            // 
            this.BindingSource.SetBindingMember(this.zCalcEdit5, "SG_Cert2CopiesNo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_Cert2CopiesNo)));
            this.zCalcEdit5.DecimalPlaces = 0;
            this.zCalcEdit5.Decimals = 0;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit5, false);
            this.zCalcEdit5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 42, true);
            this.zCalcEdit5.Name = "zCalcEdit5";
            this.zCalcEdit5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 15, true);
            this.zCalcEdit5.TabIndex = 3;
            this.zCalcEdit5.Text = "0";
            this.zCalcEdit5.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.zCalcEdit5.TrackDisposedAccess = true;
            // 
            // zGroupBox8
            // 
            this.zGroupBox8.Controls.Add(this.zTextBox11);
            this.zGroupBox8.Controls.Add(this.zLabel30);
            this.zGroupBox8.Controls.Add(this.zLabel5);
            this.zGroupBox8.Controls.Add(this.zCodeFindBox2);
            this.zGroupBox8.Controls.Add(this.zTextBox10);
            this.zGroupBox8.Controls.Add(this.zLabel41);
            this.zGroupBox8.Controls.Add(this.zTextBox6);
            this.zGroupBox8.Controls.Add(this.zLabel1);
            this.zGroupBox8.Controls.Add(this.zLabel31);
            this.zGroupBox8.Controls.Add(this.zLabel2);
            this.zGroupBox8.Controls.Add(this.zLabel3);
            this.zGroupBox8.Controls.Add(this.DonorCountryCodeFindBox);
            this.zGroupBox8.Controls.Add(this.zLabel33);
            this.zGroupBox8.Controls.Add(this.zDropEdit3);
            this.zGroupBox8.Controls.Add(this.ManufacturerGuidFindBox);
            this.zGroupBox8.Controls.Add(this.zCalcEdit2);
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox8, false);
            this.zGroupBox8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 19, true);
            this.zGroupBox8.Name = "zGroupBox8";
            this.zGroupBox8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(741, 219, true);
            this.zGroupBox8.TabIndex = 0;
            this.zGroupBox8.TabStop = false;
            // 
            // zTextBox11
            // 
            this.BindingSource.SetBindingMember(this.zTextBox11, "SG_Cert1AdditionalDetails");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_Cert1AdditionalDetails)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox11, false);
            this.zTextBox11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(532, 89, true);
            this.zTextBox11.Multiline = true;
            this.zTextBox11.Name = "zTextBox11";
            this.zTextBox11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 73, true);
            this.zTextBox11.TabIndex = 15;
            // 
            // zLabel30
            // 
            this.zLabel30.AutoSize = true;
            this.zLabel30.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("BD310316-CE2D-4380-9AF0-FDB8D5B4B26F", "Additional Export Details:");
            this.zLabel30.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel30.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 93, true);
            this.zLabel30.Name = "zLabel30";
            this.zLabel30.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 12, true);
            this.zLabel30.TabIndex = 14;
            this.zLabel30.UseMnemonic = false;
            // 
            // zLabel5
            // 
            this.zLabel5.AutoSize = true;
            this.zLabel5.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("AF3E4068-B1E9-4FA1-9481-7FDCD23C8511", "Currency:");
            this.zLabel5.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 93, true);
            this.zLabel5.Name = "zLabel5";
            this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 12, true);
            this.zLabel5.TabIndex = 6;
            this.zLabel5.UseMnemonic = false;
            // 
            // zCodeFindBox2
            // 
            this.zCodeFindBox2.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zCodeFindBox2, "SG_RX_NKCertReferenceCurrency");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_RX_NKCertReferenceCurrency)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).AddInfoLookups.CertReferenceCurrencies)));
            this.zCodeFindBox2.BindToList = "AddInfoLookups+CertReferenceCurrencies";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCodeFindBox2, false);
            this.zCodeFindBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 89, true);
            this.zCodeFindBox2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
            this.zCodeFindBox2.Name = "zCodeFindBox2";
            this.zCodeFindBox2.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.zCodeFindBox2.ParentType = null;
            this.zCodeFindBox2.PreBoundMaxLength = 2;
            this.zCodeFindBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 15, true);
            this.zCodeFindBox2.TabIndex = 7;
            // 
            // zTextBox10
            // 
            this.BindingSource.SetBindingMember(this.zTextBox10, "SG_CertAdditionalInformation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_CertAdditionalInformation)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox10, false);
            this.zTextBox10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 141, true);
            this.zTextBox10.Multiline = true;
            this.zTextBox10.Name = "zTextBox10";
            this.zTextBox10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 71, true);
            this.zTextBox10.TabIndex = 11;
            // 
            // zLabel41
            // 
            this.zLabel41.AutoSize = true;
            this.zLabel41.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("C9D25F74-505F-4E8B-A497-76D994EBF5B1", "Additional Information:");
            this.zLabel41.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel41.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 145, true);
            this.zLabel41.Name = "zLabel41";
            this.zLabel41.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 12, true);
            this.zLabel41.TabIndex = 10;
            this.zLabel41.UseMnemonic = false;
            // 
            // zTextBox6
            // 
            this.BindingSource.SetBindingMember(this.zTextBox6, "SG_Cert1TransportDetails");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_Cert1TransportDetails)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox6, false);
            this.zTextBox6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(532, 13, true);
            this.zTextBox6.Multiline = true;
            this.zTextBox6.Name = "zTextBox6";
            this.zTextBox6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 73, true);
            this.zTextBox6.TabIndex = 13;
            // 
            // zLabel1
            // 
            this.zLabel1.AutoSize = true;
            this.zLabel1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("4216339A-9CE9-4822-8C4E-34B380DA7708", "Application Product Type:");
            this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 14, true);
            this.zLabel1.Name = "zLabel1";
            this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 12, true);
            this.zLabel1.TabIndex = 0;
            this.zLabel1.UseMnemonic = false;
            // 
            // zLabel31
            // 
            this.zLabel31.AutoSize = true;
            this.zLabel31.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("1E2FEDB8-70B4-4B28-9BF2-DD51A4DCC88C", "Transport Details.:");
            this.zLabel31.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel31.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 14, true);
            this.zLabel31.Name = "zLabel31";
            this.zLabel31.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 12, true);
            this.zLabel31.TabIndex = 12;
            this.zLabel31.UseMnemonic = false;
            // 
            // zLabel2
            // 
            this.zLabel2.AutoSize = true;
            this.zLabel2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("2C7A1998-BA74-4128-9055-9883E47A3CA8", "Donor Country/Region:");
            this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 67, true);
            this.zLabel2.Name = "zLabel2";
            this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 12, true);
            this.zLabel2.TabIndex = 4;
            this.zLabel2.UseMnemonic = false;
            // 
            // zLabel3
            // 
            this.zLabel3.AutoSize = true;
            this.zLabel3.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("2E82D3D1-DFEE-4FD8-B377-BD7ED0AEE131", "Year of Entry:");
            this.zLabel3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 119, true);
            this.zLabel3.Name = "zLabel3";
            this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 12, true);
            this.zLabel3.TabIndex = 8;
            this.zLabel3.UseMnemonic = false;
            // 
            // DonorCountryCodeFindBox
            // 
            this.DonorCountryCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DonorCountryCodeFindBox, "SG_RN_NKDonorCountry");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_RN_NKDonorCountry)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.CountryCodeList)));
            this.DonorCountryCodeFindBox.BindToList = "Lookups+CountryCodeList";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DonorCountryCodeFindBox, false);
            this.DonorCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 63, true);
            this.DonorCountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
            this.DonorCountryCodeFindBox.Name = "DonorCountryCodeFindBox";
            this.DonorCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.DonorCountryCodeFindBox.ParentType = null;
            this.DonorCountryCodeFindBox.PreBoundMaxLength = 2;
            this.DonorCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 15, true);
            this.DonorCountryCodeFindBox.TabIndex = 5;
            // 
            // zLabel33
            // 
            this.zLabel33.AutoSize = true;
            this.zLabel33.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("6203F1D2-F452-416A-AC83-6656B9C13E6F", "Manufacturer:");
            this.zLabel33.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel33.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 42, true);
            this.zLabel33.Name = "zLabel33";
            this.zLabel33.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 12, true);
            this.zLabel33.TabIndex = 2;
            this.zLabel33.UseMnemonic = false;
            // 
            // zDropEdit3
            // 
            this.zDropEdit3.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEdit3, "SG_ApplicationProductType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_ApplicationProductType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.ApplicationProductTypes)));
            this.zDropEdit3.BindToList = "Lookups+ApplicationProductTypes";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDropEdit3, false);
            this.zDropEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 13, true);
            this.zDropEdit3.Name = "zDropEdit3";
            this.zDropEdit3.PreBoundMaxLength = 2;
            this.zDropEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 15, true);
            this.zDropEdit3.TabIndex = 1;
            // 
            // ManufacturerGuidFindBox
            // 
            this.ManufacturerGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ManufacturerGuidFindBox, "JE_OH_Manufacturer");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).JE_OH_Manufacturer)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.Organisations)));
            this.ManufacturerGuidFindBox.BindToList = "Lookups+Organisations";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ManufacturerGuidFindBox, false);
            this.ManufacturerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 38, true);
            this.ManufacturerGuidFindBox.Name = "ManufacturerGuidFindBox";
            this.ManufacturerGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.ManufacturerGuidFindBox.ParentType = null;
            this.ManufacturerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 15, true);
            this.ManufacturerGuidFindBox.TabIndex = 3;
            // 
            // zCalcEdit2
            // 
            this.BindingSource.SetBindingMember(this.zCalcEdit2, "SG_EntryYear");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_EntryYear)));
            this.zCalcEdit2.DecimalPlaces = 0;
            this.zCalcEdit2.Decimals = 0;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit2, false);
            this.zCalcEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 115, true);
            this.zCalcEdit2.Name = "zCalcEdit2";
            this.zCalcEdit2.ShowGroupSeparators = false;
            this.zCalcEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 15, true);
            this.zCalcEdit2.TabIndex = 9;
            this.zCalcEdit2.Text = "0";
            this.zCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.zCalcEdit2.TrackDisposedAccess = true;
            // 
            // zGroupBox4
            // 
            this.zGroupBox4.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("DB9D82B9-0B4D-496B-ACC7-1DE8D6BA6FD4", "Certificate 1");
            this.zGroupBox4.Controls.Add(this.zLabel29);
            this.zGroupBox4.Controls.Add(this.zCalcEdit3);
            this.zGroupBox4.Controls.Add(this.zDropEdit4);
            this.zGroupBox4.Controls.Add(this.zLabel34);
            this.zGroupBox4.Controls.Add(this.zLabel4);
            this.zGroupBox4.Controls.Add(this.zCalcEdit1);
            this.zGroupBox4.Controls.Add(this.zCheckBox4);
            this.zGroupBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 245, true);
            this.zGroupBox4.Name = "zGroupBox4";
            this.zGroupBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 126, true);
            this.zGroupBox4.TabIndex = 1;
            this.zGroupBox4.TabStop = false;
            // 
            // zLabel29
            // 
            this.zLabel29.AutoSize = true;
            this.zLabel29.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("5B516D47-075F-4685-A96A-7D90FEFA4F22", "Cert. Type:");
            this.zLabel29.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel29.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 20, true);
            this.zLabel29.Name = "zLabel29";
            this.zLabel29.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 12, true);
            this.zLabel29.TabIndex = 0;
            this.zLabel29.UseMnemonic = false;
            // 
            // zCalcEdit3
            // 
            this.BindingSource.SetBindingMember(this.zCalcEdit3, "SG_Cert1PercCommContent");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_Cert1PercCommContent)));
            this.zCalcEdit3.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit3, false);
            this.zCalcEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 68, true);
            this.zCalcEdit3.Name = "zCalcEdit3";
            this.zCalcEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 15, true);
            this.zCalcEdit3.TabIndex = 5;
            this.zCalcEdit3.Text = "0.00";
            this.zCalcEdit3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.zCalcEdit3.TrackDisposedAccess = true;
            // 
            // zDropEdit4
            // 
            this.zDropEdit4.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEdit4, "SG_Cert1Type");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_Cert1Type)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.CertificateTypes)));
            this.zDropEdit4.BindToList = "Lookups+CertificateTypes";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDropEdit4, false);
            this.zDropEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 16, true);
            this.zDropEdit4.Name = "zDropEdit4";
            this.zDropEdit4.PreBoundMaxLength = 2;
            this.zDropEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 15, true);
            this.zDropEdit4.TabIndex = 1;
            // 
            // zLabel34
            // 
            this.zLabel34.AutoSize = true;
            this.zLabel34.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("974A2C9D-D472-4043-9A49-32386F1DC175", "% Comm. Content.:");
            this.zLabel34.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel34.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 72, true);
            this.zLabel34.Name = "zLabel34";
            this.zLabel34.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 12, true);
            this.zLabel34.TabIndex = 4;
            this.zLabel34.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.zLabel34.UseMnemonic = false;
            // 
            // zLabel4
            // 
            this.zLabel4.AutoSize = true;
            this.zLabel4.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("E3A79DA9-5094-4E2E-802A-DAC32898A73D", "Copies:");
            this.zLabel4.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 46, true);
            this.zLabel4.Name = "zLabel4";
            this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 12, true);
            this.zLabel4.TabIndex = 2;
            this.zLabel4.UseMnemonic = false;
            // 
            // zCalcEdit1
            // 
            this.BindingSource.SetBindingMember(this.zCalcEdit1, "SG_Cert1CopiesNo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_Cert1CopiesNo)));
            this.zCalcEdit1.DecimalPlaces = 0;
            this.zCalcEdit1.Decimals = 0;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit1, false);
            this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 42, true);
            this.zCalcEdit1.Name = "zCalcEdit1";
            this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 15, true);
            this.zCalcEdit1.TabIndex = 3;
            this.zCalcEdit1.Text = "0";
            this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.zCalcEdit1.TrackDisposedAccess = true;
            // 
            // zCheckBox4
            // 
            this.BindingSource.SetBindingMember(this.zCheckBox4, "SG_CertSendInvDetails");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_CertSendInvDetails)));
            this.zCheckBox4.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("c67961b0-9212-42f4-8bca-f8e121d4b523", "Send Invoice Details", "Will default for certain certificate types, can be overridden");
            this.zCheckBox4.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
            this.zCheckBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 94, true);
            this.zCheckBox4.Name = "zCheckBox4";
            this.zCheckBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 17, true);
            this.zCheckBox4.TabIndex = 17;
            this.zCheckBox4.UseVisualStyleBackColor = true;
            // 
            // SGTN41DeclarationUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.SGDeclarationTabControl);
            this.Name = "SGTN41DeclarationUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1205, 672, true);
            this.Controls.SetChildIndex(this.SGDeclarationTabControl, 0);
            this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.FinalPortOfCallFindBox.ResumeLayout(true);
            this.FinalPortOfCallFindBox.PerformLayout();
            this.FinalDestinationCountryFindBox.ResumeLayout(true);
            this.FinalDestinationCountryFindBox.PerformLayout();
            this.NextPortOfCallFindBox.ResumeLayout(true);
            this.NextPortOfCallFindBox.PerformLayout();
            this.zGroupBox1.ResumeLayout(false);
            this.zGroupBox1.PerformLayout();
            this.PlaceOfStorageFindBox.ResumeLayout(true);
            this.PlaceOfStorageFindBox.PerformLayout();
            this.PlaceOfReleaseFindBox.ResumeLayout(true);
            this.PlaceOfReleaseFindBox.PerformLayout();
            this.PlaceOfReceiptFindBox.ResumeLayout(true);
            this.PlaceOfReceiptFindBox.PerformLayout();
            this.zGroupBox5.ResumeLayout(false);
            this.zGroupBox5.PerformLayout();
            this.zDateEdit2.ResumeLayout(true);
            this.zDateEdit2.PerformLayout();
            this.zDateEdit1.ResumeLayout(true);
            this.zDateEdit1.PerformLayout();
            this.zDropEdit1.ResumeLayout(true);
            this.zDropEdit1.PerformLayout();
            this.zCodeFindBox1.ResumeLayout(true);
            this.zCodeFindBox1.PerformLayout();
            this.zGroupBox6.ResumeLayout(false);
            this.zGroupBox6.PerformLayout();
            this.OutwardCarrierAgentGuidFindBox.ResumeLayout(true);
            this.OutwardCarrierAgentGuidFindBox.PerformLayout();
            this.ConsigneeGuidFindBox.ResumeLayout(true);
            this.ConsigneeGuidFindBox.PerformLayout();
            this.InwardCarrierAgentGuidFindBox.ResumeLayout(true);
            this.InwardCarrierAgentGuidFindBox.PerformLayout();
            this.HandlingAgentGuidFindBox.ResumeLayout(true);
            this.HandlingAgentGuidFindBox.PerformLayout();
            this.ExporterGuidFindBox.ResumeLayout(true);
            this.ExporterGuidFindBox.PerformLayout();
            this.EndUserGuidFindBox.ResumeLayout(true);
            this.EndUserGuidFindBox.PerformLayout();
            this.ForwarderGuidFindBox.ResumeLayout(true);
            this.ForwarderGuidFindBox.PerformLayout();
            this.ClaimantGuidFindBox.ResumeLayout(true);
            this.ClaimantGuidFindBox.PerformLayout();
            this.SGDeclarationTabControl.ResumeLayout(false);
            this.SGDeclarationTabControl.PerformLayout();
            this.zTabPage1.ResumeLayout(false);
            this.zTabPage1.PerformLayout();
            this.zPanel1.ResumeLayout(false);
            this.zPanel1.PerformLayout();
            this.CPCGroupBox.ResumeLayout(false);
            this.CPCGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CPCGrid)).EndInit();
            this.CPCGrid.ResumeLayout(false);
            this.CPCGrid.PerformLayout();
            this.zGroupBox10.ResumeLayout(false);
            this.zGroupBox10.PerformLayout();
            this.zGroupBox3.ResumeLayout(false);
            this.zGroupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CALicencesGrid)).EndInit();
            this.CALicencesGrid.ResumeLayout(false);
            this.CALicencesGrid.PerformLayout();
            this.zGroupBox2.ResumeLayout(false);
            this.zGroupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TradersRemarksGrid)).EndInit();
            this.TradersRemarksGrid.ResumeLayout(false);
            this.TradersRemarksGrid.PerformLayout();
            this.TradersRemarksTextGroupBox.ResumeLayout(false);
            this.TradersRemarksTextGroupBox.PerformLayout();
            this.CertificateOfOriginTabPage.ResumeLayout(false);
            this.CertificateOfOriginTabPage.PerformLayout();
            this.zPanel2.ResumeLayout(false);
            this.zPanel2.PerformLayout();
            this.CofOGroupBox.ResumeLayout(false);
            this.CofOGroupBox.PerformLayout();
            this.zGroupBox7.ResumeLayout(false);
            this.zGroupBox7.PerformLayout();
            this.zDropEdit2.ResumeLayout(true);
            this.zDropEdit2.PerformLayout();
            this.zGroupBox8.ResumeLayout(false);
            this.zGroupBox8.PerformLayout();
            this.zCodeFindBox2.ResumeLayout(true);
            this.zCodeFindBox2.PerformLayout();
            this.DonorCountryCodeFindBox.ResumeLayout(true);
            this.DonorCountryCodeFindBox.PerformLayout();
            this.zDropEdit3.ResumeLayout(true);
            this.zDropEdit3.PerformLayout();
            this.ManufacturerGuidFindBox.ResumeLayout(true);
            this.ManufacturerGuidFindBox.PerformLayout();
            this.zGroupBox4.ResumeLayout(false);
            this.zGroupBox4.PerformLayout();
            this.zDropEdit4.ResumeLayout(true);
            this.zDropEdit4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox FinalPortOfCallFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox FinalDestinationCountryFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox NextPortOfCallFindBox;
		private Enterprise.ZArchitecture.ZLabel zLabel11;
		private Enterprise.ZArchitecture.ZLabel zLabel10;
		private Enterprise.ZArchitecture.ZLabel zLabel9;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox5;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox6;
		private Enterprise.ZArchitecture.ZLabel zLabel6;
		private Enterprise.ZArchitecture.ZLabel zLabel12;
		private Enterprise.ZArchitecture.ZLabel zLabel13;
		private Enterprise.ZArchitecture.ZLabel zLabel14;
		private Enterprise.ZArchitecture.ZLabel zLabel16;
		private Enterprise.ZArchitecture.ZLabel zLabel17;
		private Enterprise.ZArchitecture.ZLabel zLabel18;
		private Enterprise.ZArchitecture.ZLabel zLabel20;
		private Enterprise.ZArchitecture.ZLabel zLabel21;
		private Enterprise.ZArchitecture.ZLabel zLabel22;
		private Enterprise.ZArchitecture.ZLabel zLabel19;
		private Enterprise.ZArchitecture.ZLabel zLabel23;
		public Enterprise.ZArchitecture.ZLabel StartDateLabel;
		private Enterprise.ZArchitecture.ZLabel zLabel25;
		private Enterprise.ZArchitecture.ZLabel zLabel26;
		private Enterprise.ZArchitecture.ZLabel zLabel27;
		private Enterprise.ZArchitecture.ZLabel zLabel28;
		private Enterprise.ZArchitecture.GUI.ZTabPage zTabPage1;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanel1;
		public Enterprise.ZArchitecture.GUI.ZTabPage CertificateOfOriginTabPage;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanel2;
		private Enterprise.ZArchitecture.ZTextBox zTextBox2;
		private Enterprise.ZArchitecture.ZTextBox zTextBox3;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox2;
		private Enterprise.ZArchitecture.GUI.ZGroupBox CofOGroupBox;
		private Enterprise.ZArchitecture.ZLabel zLabel33;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox DonorCountryCodeFindBox;
		private Enterprise.ZArchitecture.ZLabel zLabel4;
		private Enterprise.ZArchitecture.ZLabel zLabel3;
		private Enterprise.ZArchitecture.ZLabel zLabel2;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit1;
		private Enterprise.ZArchitecture.ZTextBox zTextBox1;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox zCodeFindBox1;
		private Enterprise.ZArchitecture.ZTextBox zTextBox5;
		private Enterprise.ZArchitecture.ZTextBox zTextBox4;
		private Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit1;
		private Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit2;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox3;
		private Enterprise.ZArchitecture.ZGrid CALicencesGrid;
		private Enterprise.ZArchitecture.ZLabel zLabel30;
		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit3;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit2;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit1;
		private Enterprise.ZArchitecture.ZLabel zLabel29;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox4;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit3;
		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit4;
		private Enterprise.ZArchitecture.ZLabel zLabel34;
		private Enterprise.ZArchitecture.ZLabel zLabel31;
		private Enterprise.ZArchitecture.ZTextBox zTextBox6;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox8;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox7;
		private Enterprise.ZArchitecture.ZLabel zLabel36;
		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit2;
		private Enterprise.ZArchitecture.ZLabel zLabel40;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit5;
		private Enterprise.ZArchitecture.ZTextBox zTextBox10;
		private Enterprise.ZArchitecture.ZLabel zLabel41;
		private Enterprise.ZArchitecture.ZTextBox zTextBox11;
		private Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox1;
		public Enterprise.ZArchitecture.GUI.ZGuidFindBox ClaimantGuidFindBox;
		public Enterprise.ZArchitecture.GUI.ZGuidFindBox ConsigneeGuidFindBox;
		public Enterprise.ZArchitecture.GUI.ZGuidFindBox OutwardCarrierAgentGuidFindBox;
		public Enterprise.ZArchitecture.GUI.ZGuidFindBox InwardCarrierAgentGuidFindBox;
		public Enterprise.ZArchitecture.GUI.ZGuidFindBox HandlingAgentGuidFindBox;
		public Enterprise.ZArchitecture.GUI.ZGuidFindBox ForwarderGuidFindBox;
		public Enterprise.ZArchitecture.GUI.ZGuidFindBox ExporterGuidFindBox;
		public Enterprise.ZArchitecture.GUI.ZGuidFindBox EndUserGuidFindBox;
		public Enterprise.ZArchitecture.GUI.ZGuidFindBox ManufacturerGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox3;
		private Enterprise.ZArchitecture.ZLabel zLabel5;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox zCodeFindBox2;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox10;
		private Enterprise.ZArchitecture.ZLabel zLabel15;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox PlaceOfStorageFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox PlaceOfReleaseFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox PlaceOfReceiptFindBox;
		private Enterprise.ZArchitecture.ZLabel zLabel7;
		private Enterprise.ZArchitecture.ZLabel zLabel8;
		public Enterprise.ZArchitecture.ZCalcEdit TotalOtherTaxCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit8;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit7;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit6;
		private Enterprise.ZArchitecture.ZLabel zLabel45;
		private Enterprise.ZArchitecture.ZLabel zLabel44;
		private Enterprise.ZArchitecture.ZLabel zLabel43;
		private Enterprise.ZArchitecture.ZLabel zLabel42;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit4;
		public Enterprise.ZArchitecture.GUI.ZTemplateTabControl SGDeclarationTabControl;
		private Enterprise.ZArchitecture.GUI.ZGroupBox CPCGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox DutyExemptCheckBox;
		private ZArchitecture.ZCalcEdit zCalcEdit11;
		private ZArchitecture.ZLabel zLabel47;
		public ZArchitecture.ZLabel TotalOtherTaxLabel;
		private ZArchitecture.ZGrid CPCGrid;
		protected ZArchitecture.GUI.ZButton CPCViewEditButton;
		private Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox4;
		private ZArchitecture.ZGrid TradersRemarksGrid;
		private ZArchitecture.ZTextBox TradersRemarksTextBox;
		private ZArchitecture.GUI.ZGroupBox TradersRemarksTextGroupBox;
	}
}

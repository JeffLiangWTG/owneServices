using Enterprise.Customs.SG.V4.Business;

using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.SG.V4.GUI
{
	partial class SGDeclarationUserControl
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
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
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
            this.zLabel32 = new Enterprise.ZArchitecture.ZLabel();
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
            this.SeaStoresCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
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
            this.zGroupBox9 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.zCalcEdit10 = new Enterprise.ZArchitecture.ZCalcEdit();
            this.zLabel38 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel46 = new Enterprise.ZArchitecture.ZLabel();
            this.zCalcEdit14 = new Enterprise.ZArchitecture.ZCalcEdit();
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
            this.TradersRemarksTextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.TradersRemarksTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.TradersRemarksGrid = new Enterprise.ZArchitecture.ZGrid();
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
            this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
            this.zCodeFindBox2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.zLabel30 = new Enterprise.ZArchitecture.ZLabel();
            this.zTextBox10 = new Enterprise.ZArchitecture.ZTextBox();
            this.zLabel41 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
            this.zTextBox6 = new Enterprise.ZArchitecture.ZTextBox();
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
            this.zGroupBox9.SuspendLayout();
            this.zGroupBox10.SuspendLayout();
            this.zGroupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CALicencesGrid)).BeginInit();
            this.CALicencesGrid.SuspendLayout();
            this.zGroupBox2.SuspendLayout();
            this.TradersRemarksTextGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TradersRemarksGrid)).BeginInit();
            this.TradersRemarksGrid.SuspendLayout();
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
            this.zLabel11.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("4FA9E310-FB29-4CBD-9F91-B92F5F113F5E", "Final Destination:");
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
            this.zLabel10.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("781E0C41-DD45-4992-A6DC-6D60808FE0C7", "Final Port of Call:");
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
            this.zLabel9.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("462B34C9-6AE7-4EA6-817A-F939433BE65C", "Next Port of Call:");
            this.zLabel9.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 23, true);
            this.zLabel9.Name = "zLabel9";
            this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 12, true);
            this.zLabel9.TabIndex = 0;
            this.zLabel9.UseMnemonic = false;
            // 
            // zGroupBox1
            // 
            this.zGroupBox1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("EDABF204-E498-4551-8E23-3E0BE920EA4F", "Places");
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
            this.zLabel15.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("1E471769-E62A-47E0-A55C-7580FBFA3835", "Place of Release:");
            this.zLabel15.AutoSize = true;
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
            this.zLabel7.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("D27E5DBD-85CC-46B9-B01A-C9F5F2E4B675", "Place of Receipt:");
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
            this.zLabel8.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("EAD30CA7-094A-4BEA-B689-AA7E68739553", "Place of Storage:");
            this.zLabel8.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 138, true);
            this.zLabel8.Name = "zLabel8";
            this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 12, true);
            this.zLabel8.TabIndex = 10;
            this.zLabel8.UseMnemonic = false;
            // 
            // zGroupBox5
            // 
            this.zGroupBox5.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("4DFA62C4-EDA9-4EC3-B19C-9EAA8E0F2211", "Other");
            this.zGroupBox5.Controls.Add(this.zLabel32);
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
            this.zGroupBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 251, true);
            this.zGroupBox5.TabIndex = 3;
            this.zGroupBox5.TabStop = false;
            // 
            // zLabel32
            // 
            this.zLabel32.AutoSize = true;
            this.zLabel32.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("CDBACEA4-EE22-4671-8459-A3AFC06C4E07", "Of Exhibition/Temporary Import Period:");
            this.zLabel32.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel32.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 154, true);
            this.zLabel32.Name = "zLabel32";
            this.zLabel32.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 12, true);
            this.zLabel32.TabIndex = 14;
            this.zLabel32.UseMnemonic = false;
            // 
            // zCheckBox3
            // 
            this.BindingSource.SetBindingMember(this.zCheckBox3, "SG_GoodsPreviouslyExemptedFromDuties");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_GoodsPreviouslyExemptedFromDuties)));
            this.zCheckBox3.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("FB499EA3-8354-40B9-BB72-86AF91544B06", "Were goods previously exempted from duties?");
            this.zCheckBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 200, true);
            this.zCheckBox3.Name = "zCheckBox3";
            this.zCheckBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 17, true);
            this.zCheckBox3.TabIndex = 18;
            this.zCheckBox3.UseVisualStyleBackColor = true;
            // 
            // zCheckBox1
            // 
            this.BindingSource.SetBindingMember(this.zCheckBox1, "SG_GoodsImportedUnderMESorBWS");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_GoodsImportedUnderMESorBWS)));
            this.zCheckBox1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("AD80D116-1ED9-440B-B953-DCCE7F4CB544", "Were goods imported under MES or BWS?");
            this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 177, true);
            this.zCheckBox1.Name = "zCheckBox1";
            this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
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
            this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 150, true);
            this.zDateEdit2.Name = "zDateEdit2";
            this.zDateEdit2.TabIndex = 13;
            // 
            // zTextBox5
            // 
            this.BindingSource.SetBindingMember(this.zTextBox5, "SG_ReplacementPermitNo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_ReplacementPermitNo)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox5, false);
            this.zTextBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 81, true);
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
            this.zTextBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 58, true);
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
            this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 127, true);
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
            this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 104, true);
            this.zDropEdit1.Name = "zDropEdit1";
            this.zDropEdit1.PreBoundMaxLength = 1;
            this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 15, true);
            this.zDropEdit1.TabIndex = 9;
            // 
            // zTextBox1
            // 
            this.BindingSource.SetBindingMember(this.zTextBox1, "SG_TowingVoyageNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_TowingVoyageNumber)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox1, false);
            this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 35, true);
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
            this.zCodeFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 12, true);
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
            this.zLabel28.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("75A23758-E541-494D-873A-6762D758AB60", "Towing Voyage No.:");
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
            this.zLabel27.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("5DCA2F0A-3899-4422-B045-AAF3D69DD8E9", "Towing Vessel:");
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
            this.zLabel26.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("AF1D82A4-DA60-4924-82F2-707F5F5CDDE3", "Supply Indicator:");
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
            this.zLabel25.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("F30C5996-BF1D-4CD6-AF24-A0C1B8949C0B", "Replacement Permit No.:");
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
            this.StartDateLabel.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("92BAA2CC-9450-4104-B75F-7BD7C16D87AF", "Removal Start Date:");
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
            this.zLabel23.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("98DA4562-0D23-4AA2-A2DE-B853F85CFDAF", "Previous Permit No.:");
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
            this.zLabel16.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("2ACF1285-8F80-4067-AF83-B278D5EF7017", "End Date:");
            this.zLabel16.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel16.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 154, true);
            this.zLabel16.Name = "zLabel16";
            this.zLabel16.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 12, true);
            this.zLabel16.TabIndex = 12;
            this.zLabel16.UseMnemonic = false;
            // 
            // SeaStoresCheckBox
            // 
            this.BindingSource.SetBindingMember(this.SeaStoresCheckBox, "SG_IsSeaStore");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_IsSeaStore)));
            this.SeaStoresCheckBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("B7D662AD-8B4A-4C17-B7B5-D72C2F801484", "Sea Stores");
            this.SeaStoresCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
            this.SeaStoresCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 21, true);
            this.SeaStoresCheckBox.Name = "SeaStoresCheckBox";
            this.SeaStoresCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 19, true);
            this.SeaStoresCheckBox.TabIndex = 0;
            this.SeaStoresCheckBox.UseVisualStyleBackColor = true;
            // 
            // zGroupBox6
            // 
            this.zGroupBox6.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("E73053B7-39E6-426F-8E90-7A1AE691B330", "Organization Details:");
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
            this.DutyExemptCheckBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("6E05D889-6E18-41DE-86FC-E7E738133A6A", "Duty Exemption");
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
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.Organisations)));
            this.InwardCarrierAgentGuidFindBox.BindToList = "Lookups+Organisations";
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
            this.zLabel19.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("BBF0E04B-56A3-4BD8-9351-1AD42C1B670F", "Outward Carrier Agent:");
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
            this.zLabel14.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("0CB59AFD-EBB2-4716-9C7C-3F3DAD6C395F", "Consignee:");
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
            this.zLabel22.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("D1AB4CEA-7D6D-4C9F-95ED-F27D968938B6", "Inward Carrier Agent:");
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
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.ForwarderList)));
            this.HandlingAgentGuidFindBox.BindToList = "Lookups+ForwarderList";
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
            this.zLabel17.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("8BECF7CB-1549-4419-AA11-045717F0719C", "End User:");
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
            this.zLabel21.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("0AE31174-FEA9-4300-80A6-B991E6762CF7", "Handling Agent:");
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
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).Lookups.Organisations)));
            this.ClaimantGuidFindBox.BindToList = "Lookups+Organisations";
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
            this.zLabel18.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("4BF45A90-EFF9-46A4-AEE2-03FF579AF190", "Exporter:");
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
            this.zLabel13.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("A450FC9E-B9D9-4157-B19E-19B0CD03EFEB", "Claimant Code:");
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
            this.zLabel12.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("429A19A1-B94A-4E57-92C3-497F9F9C5ECE", "Claimant Name:");
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
            this.zLabel6.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("E7C38E50-2E9A-42BC-93F3-0B43B25D938C", "Claimant (Org.):");
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
            this.zLabel20.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("AC13D860-B54B-4068-A09B-43EA7F5F50D3", "Forwarder:");
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
            this.SGDeclarationTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1090, 599, true);
            this.SGDeclarationTabControl.TabIndex = 11;
            // 
            // zTabPage1
            // 
            this.zTabPage1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("1A0157BE-33E3-4CCF-B759-98D3FD3C3842", "Additional Details");
            this.zTabPage1.Controls.Add(this.zPanel1);
            this.zTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
            this.zTabPage1.Name = "zTabPage1";
            this.zTabPage1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.zTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1082, 576, true);
            this.zTabPage1.TabIndex = 0;
            // 
            // zPanel1
            // 
            this.zPanel1.Controls.Add(this.zGroupBox9);
            this.zPanel1.Controls.Add(this.zGroupBox10);
            this.zPanel1.Controls.Add(this.zGroupBox3);
            this.zPanel1.Controls.Add(this.zGroupBox2);
            this.zPanel1.Controls.Add(this.zGroupBox6);
            this.zPanel1.Controls.Add(this.zGroupBox5);
            this.zPanel1.Controls.Add(this.zGroupBox1);
            this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.zPanel1.Name = "zPanel1";
            this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1076, 570, true);
            this.zPanel1.TabIndex = 0;
            // 
            // zGroupBox9
            // 
            this.zGroupBox9.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("F0F4DD4C-81C9-4E09-903B-ABEE86130512", "Sea Stores");
            this.zGroupBox9.Controls.Add(this.zCalcEdit10);
            this.zGroupBox9.Controls.Add(this.zLabel38);
            this.zGroupBox9.Controls.Add(this.SeaStoresCheckBox);
            this.zGroupBox9.Controls.Add(this.zLabel46);
            this.zGroupBox9.Controls.Add(this.zCalcEdit14);
            this.zGroupBox9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(615, 259, true);
            this.zGroupBox9.Name = "zGroupBox9";
            this.zGroupBox9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 161, true);
            this.zGroupBox9.TabIndex = 5;
            this.zGroupBox9.TabStop = false;
            // 
            // zCalcEdit10
            // 
            this.BindingSource.SetBindingMember(this.zCalcEdit10, "SG_VoyageDuration");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_VoyageDuration)));
            this.zCalcEdit10.DecimalPlaces = 0;
            this.zCalcEdit10.Decimals = 0;
            this.zCalcEdit10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 66, true);
            this.zCalcEdit10.Name = "zCalcEdit10";
            this.zCalcEdit10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 15, true);
            this.zCalcEdit10.TabIndex = 4;
            this.zCalcEdit10.Text = "0";
            this.zCalcEdit10.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.zCalcEdit10.TrackDisposedAccess = true;
            // 
            // zLabel38
            // 
            this.zLabel38.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("BB9BAEBD-C77B-4467-B3FF-1C64323ABF65", "No of Crew:");
            this.zLabel38.AutoSize = true;
            this.zLabel38.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel38.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 47, true);
            this.zLabel38.Name = "zLabel38";
            this.zLabel38.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 12, true);
            this.zLabel38.TabIndex = 1;
            this.zLabel38.UseMnemonic = false;
            // 
            // zLabel46
            // 
            this.zLabel46.AutoSize = true;
            this.zLabel46.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("2022D39C-269D-41AA-A190-4121E56DE925", "Voyage Duration:");
            this.zLabel46.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel46.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 70, true);
            this.zLabel46.Name = "zLabel46";
            this.zLabel46.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 12, true);
            this.zLabel46.TabIndex = 3;
            this.zLabel46.UseMnemonic = false;
            // 
            // zCalcEdit14
            // 
            this.BindingSource.SetBindingMember(this.zCalcEdit14, "SG_NoOfCrew");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_NoOfCrew)));
            this.zCalcEdit14.DecimalPlaces = 0;
            this.zCalcEdit14.Decimals = 0;
            this.zCalcEdit14.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 43, true);
            this.zCalcEdit14.Name = "zCalcEdit14";
            this.zCalcEdit14.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 15, true);
            this.zCalcEdit14.TabIndex = 2;
            this.zCalcEdit14.Text = "0";
            this.zCalcEdit14.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.zCalcEdit14.TrackDisposedAccess = true;
            // 
            // zGroupBox10
            // 
            this.zGroupBox10.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("6FE2F976-7D1A-4750-8687-5F825DB85B3F", "Payment Summary");
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
            this.zGroupBox10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(784, 259, true);
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
            this.zLabel47.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("1713BC00-9075-4548-9CD6-BFEDCEE77EE5", "Customs Value:");
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
            this.zLabel45.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("1C652255-59B8-4850-ABD6-765FB62C92E4", "Total Payable:");
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
            this.zLabel44.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("72C2A2DB-75AE-4BD7-A675-02D0A5E491B6", "Total Duty:");
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
            this.zLabel43.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("8CC6F233-69CD-41CF-863C-4F2B266BC60C", "Total GST:");
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
            this.zLabel42.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("E9032424-95C8-4A62-B748-88BA94AC1752", "Total Excise:");
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
            this.TotalOtherTaxLabel.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("B64487F0-9C69-4B9A-80E2-E5D7DB2D7767", "Total Other Tax:");
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
            this.zGroupBox3.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("FC5EC733-F565-46FD-BAB2-B551313A5EBE", "CA Licenses");
            this.zGroupBox3.Controls.Add(this.CALicencesGrid);
            this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(442, 259, true);
            this.zGroupBox3.Name = "zGroupBox3";
            this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 161, true);
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
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("05F61A19-70D5-484D-9789-061AA85A4E30", "License Number");
            zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.IsMandatory = true;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            this.CALicencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.CALicencesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CALicencesGrid.GridId = "6222d27e-fb52-4994-a0f2-18442708682a";
            this.CALicencesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.CALicencesGrid.LayoutKey = "CALicencesGrid";
            this.CALicencesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
            this.CALicencesGrid.Name = "CALicencesGrid";
            this.CALicencesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 146, true);
            this.CALicencesGrid.TabIndex = 0;
            // 
            // zGroupBox2
            // 
            this.zGroupBox2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("CA014980-8ECC-43BF-8F33-1017FFFCB997", "Traders Remarks");
            this.zGroupBox2.Controls.Add(this.TradersRemarksTextGroupBox);
            this.zGroupBox2.Controls.Add(this.TradersRemarksGrid);
            this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 426, true);
            this.zGroupBox2.Name = "zGroupBox2";
            this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(983, 111, true);
            this.zGroupBox2.TabIndex = 2;
            this.zGroupBox2.TabStop = false;
            // 
            // TradersRemarksTextGroupBox
            // 
            this.TradersRemarksTextGroupBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("3977F463-D6E7-4538-AAF7-94649B259BF5", "Remark Text");
            this.TradersRemarksTextGroupBox.Controls.Add(this.TradersRemarksTextBox);
            this.TradersRemarksTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(610, 9, true);
            this.TradersRemarksTextGroupBox.Name = "TradersRemarksTextGroupBox";
            this.TradersRemarksTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 100, true);
            this.TradersRemarksTextGroupBox.TabIndex = 5;
            this.TradersRemarksTextGroupBox.TabStop = false;
            // 
            // TradersRemarksTextBox
            // 
            this.BindingSource.SetBindingMember(this.TradersRemarksTextBox, "TradersRemarks.CSI_Description");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.TradersRemark)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).TradersRemarks)).SyncRoot)).CSI_Description)));
            this.TradersRemarksTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("65d61b81-0ab2-43be-b08e-77691fb92808", "Remark text");
            this.TradersRemarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.TradersRemarksTextBox.Multiline = true;
            this.TradersRemarksTextBox.Name = "TradersRemarksTextBox";
            this.TradersRemarksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 81, true);
            this.TradersRemarksTextBox.TabIndex = 3;
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
            zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo2.ColumnName = "CSI_Description";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.IsSortable = false;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(560);
            this.TradersRemarksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.TradersRemarksGrid.GridId = "D8F976D9-5EB8-49E3-BF06-0CA122933817";
            this.TradersRemarksGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.TradersRemarksGrid.LayoutKey = "TradersRemarksGrid";
            this.TradersRemarksGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.TradersRemarksGrid.MaximumRows = 5;
            this.TradersRemarksGrid.Name = "TradersRemarksGrid";
            this.TradersRemarksGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 94, true);
            this.TradersRemarksGrid.TabIndex = 3;
            // 
            // CertificateOfOriginTabPage
            // 
            this.CertificateOfOriginTabPage.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("B19A6C02-5137-460F-AA19-FADFA3896B16", "Certificate of Origin");
            this.CertificateOfOriginTabPage.Controls.Add(this.zPanel2);
            this.CertificateOfOriginTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
            this.CertificateOfOriginTabPage.Name = "CertificateOfOriginTabPage";
            this.CertificateOfOriginTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.CertificateOfOriginTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1082, 576, true);
            this.CertificateOfOriginTabPage.TabIndex = 1;
            // 
            // zPanel2
            // 
            this.zPanel2.Controls.Add(this.CofOGroupBox);
            this.zPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.zPanel2.Name = "zPanel2";
            this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1076, 570, true);
            this.zPanel2.TabIndex = 1;
            // 
            // CofOGroupBox
            // 
            this.CofOGroupBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("8E621191-E0DF-4AE1-A79B-E37D789F805F", "Certificate of Origin Details");
            this.CofOGroupBox.Controls.Add(this.zGroupBox7);
            this.CofOGroupBox.Controls.Add(this.zGroupBox8);
            this.CofOGroupBox.Controls.Add(this.zGroupBox4);
            this.CofOGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CofOGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.CofOGroupBox.Name = "CofOGroupBox";
            this.CofOGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1076, 570, true);
            this.CofOGroupBox.TabIndex = 0;
            this.CofOGroupBox.TabStop = false;
            // 
            // zGroupBox7
            // 
            this.zGroupBox7.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("6D2ED7D2-DAC0-4845-84A7-BCAEDB299066", "Certificate 2");
            this.zGroupBox7.Controls.Add(this.zLabel36);
            this.zGroupBox7.Controls.Add(this.zDropEdit2);
            this.zGroupBox7.Controls.Add(this.zLabel40);
            this.zGroupBox7.Controls.Add(this.zCalcEdit5);
            this.zGroupBox7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 245, true);
            this.zGroupBox7.Name = "zGroupBox7";
            this.zGroupBox7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 100, true);
            this.zGroupBox7.TabIndex = 2;
            this.zGroupBox7.TabStop = false;
            // 
            // zLabel36
            // 
            this.zLabel36.AutoSize = true;
            this.zLabel36.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("C5C88F8C-1722-4573-9930-A38077E5ED75", "Cert. Type:");
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
            this.zLabel40.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("FEE5E7C3-F213-4FB7-B6B5-8DD4011C8DA8", "Copies:");
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
            this.zGroupBox8.Controls.Add(this.zLabel5);
            this.zGroupBox8.Controls.Add(this.zCodeFindBox2);
            this.zGroupBox8.Controls.Add(this.zLabel30);
            this.zGroupBox8.Controls.Add(this.zTextBox10);
            this.zGroupBox8.Controls.Add(this.zLabel41);
            this.zGroupBox8.Controls.Add(this.zLabel1);
            this.zGroupBox8.Controls.Add(this.zTextBox6);
            this.zGroupBox8.Controls.Add(this.zLabel31);
            this.zGroupBox8.Controls.Add(this.zLabel2);
            this.zGroupBox8.Controls.Add(this.zLabel3);
            this.zGroupBox8.Controls.Add(this.DonorCountryCodeFindBox);
            this.zGroupBox8.Controls.Add(this.zLabel33);
            this.zGroupBox8.Controls.Add(this.zDropEdit3);
            this.zGroupBox8.Controls.Add(this.ManufacturerGuidFindBox);
            this.zGroupBox8.Controls.Add(this.zCalcEdit2);
            this.zGroupBox8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 19, true);
            this.zGroupBox8.Name = "zGroupBox8";
            this.zGroupBox8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(741, 218, true);
            this.zGroupBox8.TabIndex = 0;
            this.zGroupBox8.TabStop = false;
            // 
            // zTextBox11
            // 
            this.BindingSource.SetBindingMember(this.zTextBox11, "SG_Cert1AdditionalDetails");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_Cert1AdditionalDetails)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox11, false);
            this.zTextBox11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(512, 89, true);
            this.zTextBox11.Multiline = true;
            this.zTextBox11.Name = "zTextBox11";
            this.zTextBox11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 73, true);
            this.zTextBox11.TabIndex = 15;
            // 
            // zLabel5
            // 
            this.zLabel5.AutoSize = true;
            this.zLabel5.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("F0FBD45A-19BA-465D-BABF-8F0CBB0041DD", "Currency:");
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
            // zLabel30
            // 
            this.zLabel30.AutoSize = true;
            this.zLabel30.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("E59467CB-5B48-451C-9AA5-915D06DAFEA3", "Additional Export Details:");
            this.zLabel30.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel30.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 93, true);
            this.zLabel30.Name = "zLabel30";
            this.zLabel30.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 12, true);
            this.zLabel30.TabIndex = 14;
            this.zLabel30.UseMnemonic = false;
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
            this.zLabel41.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("70D3E51F-C50D-4087-ADA6-F6A1F8889C15", "Additional Information:");
            this.zLabel41.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel41.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 145, true);
            this.zLabel41.Name = "zLabel41";
            this.zLabel41.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 12, true);
            this.zLabel41.TabIndex = 10;
            this.zLabel41.UseMnemonic = false;
            // 
            // zLabel1
            // 
            this.zLabel1.AutoSize = true;
            this.zLabel1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("A682330D-12D6-45C4-9EA3-28532E3AA3FD", "Application Product Type:");
            this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 17, true);
            this.zLabel1.Name = "zLabel1";
            this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 12, true);
            this.zLabel1.TabIndex = 0;
            this.zLabel1.UseMnemonic = false;
            // 
            // zTextBox6
            // 
            this.BindingSource.SetBindingMember(this.zTextBox6, "SG_Cert1TransportDetails");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).SG_Cert1TransportDetails)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox6, false);
            this.zTextBox6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(512, 13, true);
            this.zTextBox6.Multiline = true;
            this.zTextBox6.Name = "zTextBox6";
            this.zTextBox6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 73, true);
            this.zTextBox6.TabIndex = 13;
            // 
            // zLabel31
            // 
            this.zLabel31.AutoSize = true;
            this.zLabel31.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("E2C59224-E071-4333-B5C7-F3612382BB1C", "Transport Details.:");
            this.zLabel31.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel31.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 17, true);
            this.zLabel31.Name = "zLabel31";
            this.zLabel31.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 12, true);
            this.zLabel31.TabIndex = 12;
            this.zLabel31.UseMnemonic = false;
            // 
            // zLabel2
            // 
            this.zLabel2.AutoSize = true;
            this.zLabel2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("9AC9D8A2-0CEC-4222-A383-6F9FBB25D6CE", "Donor Country/Region:");
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
            this.zLabel3.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("B62E94F5-F693-420A-B671-83B27F654715", "Year of Entry:");
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
            this.zLabel33.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("CFDDED89-23DF-4226-B887-C6C97559A2BC", "Manufacturer:");
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
            this.zGroupBox4.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("487006AD-6797-4B18-98B8-A9646E816C1C", "Certificate 1");
            this.zGroupBox4.Controls.Add(this.zLabel29);
            this.zGroupBox4.Controls.Add(this.zCalcEdit3);
            this.zGroupBox4.Controls.Add(this.zDropEdit4);
            this.zGroupBox4.Controls.Add(this.zLabel34);
            this.zGroupBox4.Controls.Add(this.zLabel4);
            this.zGroupBox4.Controls.Add(this.zCalcEdit1);
            this.zGroupBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 245, true);
            this.zGroupBox4.Name = "zGroupBox4";
            this.zGroupBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 100, true);
            this.zGroupBox4.TabIndex = 1;
            this.zGroupBox4.TabStop = false;
            // 
            // zLabel29
            // 
            this.zLabel29.AutoSize = true;
            this.zLabel29.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("279FEB88-A334-4A33-A8B3-4CD689CCEFCD", "Cert. Type:");
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
            this.zLabel34.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("977C8EA7-9037-4E43-BA7A-3270B7C7B0DF", "% Comm. Content.:");
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
            this.zLabel4.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("D623A08F-7A85-410D-8740-22966DEE2794", "Copies:");
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
            // SGDeclarationUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.SGDeclarationTabControl);
            this.Name = "SGDeclarationUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1090, 599, true);
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
            this.zGroupBox9.ResumeLayout(false);
            this.zGroupBox9.PerformLayout();
            this.zGroupBox10.ResumeLayout(false);
            this.zGroupBox10.PerformLayout();
            this.zGroupBox3.ResumeLayout(false);
            this.zGroupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CALicencesGrid)).EndInit();
            this.CALicencesGrid.ResumeLayout(false);
            this.CALicencesGrid.PerformLayout();
            this.zGroupBox2.ResumeLayout(false);
            this.zGroupBox2.PerformLayout();
            this.TradersRemarksTextGroupBox.ResumeLayout(false);
            this.TradersRemarksTextGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TradersRemarksGrid)).EndInit();
            this.TradersRemarksGrid.ResumeLayout(false);
            this.TradersRemarksGrid.PerformLayout();
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
		private Enterprise.ZArchitecture.GUI.ZCheckBox SeaStoresCheckBox;
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
		private Enterprise.ZArchitecture.ZLabel zLabel32;
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
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox9;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit10;
		private Enterprise.ZArchitecture.ZLabel zLabel38;
		private Enterprise.ZArchitecture.ZLabel zLabel46;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit14;
		private Enterprise.ZArchitecture.GUI.ZCheckBox DutyExemptCheckBox;
		private ZArchitecture.ZCalcEdit zCalcEdit11;
		private ZArchitecture.ZLabel zLabel47;
		public ZArchitecture.ZLabel TotalOtherTaxLabel;
		private ZArchitecture.ZGrid TradersRemarksGrid;
		private ZArchitecture.GUI.ZGroupBox TradersRemarksTextGroupBox;
		private ZArchitecture.ZTextBox TradersRemarksTextBox;
	}
}

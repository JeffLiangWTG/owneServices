using System;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class DA63UserControl
	{
		void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DA63UserControl));
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.importVATPaidCalcEdit1 = new ZArchitecture.ZCalcEdit();
			this.importDutyPaidCalcEdit1 = new ZArchitecture.ZCalcEdit();
			this.importDutyPaidCalcEdit = new ZArchitecture.ZCalcEdit();
			this.importCustomsValueCalcEditCalcEdit1 = new ZArchitecture.ZCalcEdit();
			this.importCustomsValueCalcEdit = new ZArchitecture.ZCalcEdit();
			this.importCustomsQtyCalcDropEdit1 = new ZCalcDropEdit();
			this.importCustomsQtyCalcDropEdit = new ZCalcDropEdit();
			this.ImportTariffCodeFindBox1 = new Universal.GUI.TariffFindBox();
			this.ImportTariffCodeFindBox = new Universal.GUI.TariffFindBox();
			this.zLabel3 = new ZArchitecture.ZLabel();
			this.zLabel1 = new ZArchitecture.ZLabel();
			this.zLabel2 = new ZArchitecture.ZLabel();
			this.importVATPaidCalcEdit = new ZArchitecture.ZCalcEdit();
			this.additionalQuantityGroupBox = new ZGroupBox();
			this.firstAddUnitCalcDropEdit = new ZCalcDropEdit();
			this.secondAddUnitCalcDropEdit = new ZCalcDropEdit();
			this.additionalDutiesGroupBox = new ZGroupBox();
			this.additionalDutiesGrid = new ZArchitecture.ZGrid();
			this.bottomGroupBox = new ZGroupBox();
			this.topGroupBox = new ZGroupBox();
			this.previousConversionFactorCalcEdit = new ZArchitecture.ZCalcEdit();
			this.conversionFactorCalcEdit = new ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.importCustomsQtyCalcDropEdit1.SuspendLayout();
			this.importCustomsQtyCalcDropEdit.SuspendLayout();
			this.ImportTariffCodeFindBox1.SuspendLayout();
			this.ImportTariffCodeFindBox.SuspendLayout();
			this.additionalQuantityGroupBox.SuspendLayout();
			this.firstAddUnitCalcDropEdit.SuspendLayout();
			this.secondAddUnitCalcDropEdit.SuspendLayout();
			this.additionalDutiesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.additionalDutiesGrid)).BeginInit();
			this.additionalDutiesGrid.SuspendLayout();
			this.bottomGroupBox.SuspendLayout();
			this.topGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobComInvoiceLine);
			// 
			// importVATPaidCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.importVATPaidCalcEdit1, "VATFromRelatedImportBOE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobComInvoiceLine)(null)).VATFromRelatedImportBOE)));
			this.importVATPaidCalcEdit1.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.importVATPaidCalcEdit1, false);
			this.importVATPaidCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 155, true);
			this.importVATPaidCalcEdit1.Name = "importVATPaidCalcEdit1";
			this.importVATPaidCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.importVATPaidCalcEdit1.TabIndex = 14;
			this.importVATPaidCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// importDutyPaidCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.importDutyPaidCalcEdit1, "CustomsDutyFromRelatedImportBOE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobComInvoiceLine)(null)).CustomsDutyFromRelatedImportBOE)));
			this.importDutyPaidCalcEdit1.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.importDutyPaidCalcEdit1, false);
			this.importDutyPaidCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 129, true);
			this.importDutyPaidCalcEdit1.Name = "importDutyPaidCalcEdit1";
			this.importDutyPaidCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.importDutyPaidCalcEdit1.TabIndex = 10;
			this.importDutyPaidCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// importDutyPaidCalcEdit
			//
			this.BindingSource.SetBindingMember(this.importDutyPaidCalcEdit, JobComInvoiceLine.Schema.JI_ImportDutyPaid);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobComInvoiceLine)(null)).JI_ImportDutyPaid)));
			this.importDutyPaidCalcEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("82d5baab-e240-4cde-a13b-6da85e3a4c54", "Customs Duty");
			this.importDutyPaidCalcEdit.DecimalPlaces = 2;
			this.importDutyPaidCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 129, true);
			this.importDutyPaidCalcEdit.Name = "importDutyPaidCalcEdit";
			this.importDutyPaidCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.importDutyPaidCalcEdit.TabIndex = 9;
			this.importDutyPaidCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// importCustomsValueCalcEditCalcEdit1
			//
			this.BindingSource.SetBindingMember(this.importCustomsValueCalcEditCalcEdit1, "CustomsValueFromRelatedImportBOE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobComInvoiceLine)(null)).CustomsValueFromRelatedImportBOE)));
			this.importCustomsValueCalcEditCalcEdit1.DecimalPlaces = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.importCustomsValueCalcEditCalcEdit1, false);
			this.importCustomsValueCalcEditCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 105, true);
			this.importCustomsValueCalcEditCalcEdit1.Name = "importCustomsValueCalcEditCalcEdit1";
			this.importCustomsValueCalcEditCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.importCustomsValueCalcEditCalcEdit1.TabIndex = 8;
			this.importCustomsValueCalcEditCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// importCustomsValueCalcEdit
			//
			this.BindingSource.SetBindingMember(this.importCustomsValueCalcEdit, JobComInvoiceLine.Schema.JI_ImportCustomsValue);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobComInvoiceLine)(null)).JI_ImportCustomsValue)));
			this.importCustomsValueCalcEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("f0443eee-67fe-49eb-aad0-23de847a1b36", "Customs Value");
			this.importCustomsValueCalcEdit.DecimalPlaces = 0;
			this.importCustomsValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 105, true);
			this.importCustomsValueCalcEdit.Name = "importCustomsValueCalcEdit";
			this.importCustomsValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.importCustomsValueCalcEdit.TabIndex = 7;
			this.importCustomsValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// importCustomsQtyCalcDropEdit1
			//
			this.importCustomsQtyCalcDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.importCustomsQtyCalcDropEdit1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobComInvoiceLine)(null)).CustomsQtyFromRelatedImportBOE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobComInvoiceLine)(null)).CustomsQtyUQFromRelatedImportBOE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobComInvoiceLine)(null)).Lookups.CustomsUQList)));
			this.importCustomsQtyCalcDropEdit1.BindToAmount = "CustomsQtyFromRelatedImportBOE";
			this.importCustomsQtyCalcDropEdit1.BindToList = "Lookups.CustomsUQList";
			this.importCustomsQtyCalcDropEdit1.BindToUnit = "CustomsQtyUQFromRelatedImportBOE";
			this.importCustomsQtyCalcDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 80, true);
			this.importCustomsQtyCalcDropEdit1.Name = "importCustomsQtyCalcDropEdit1";
			this.importCustomsQtyCalcDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.importCustomsQtyCalcDropEdit1.TabIndex = 6;
			this.importCustomsQtyCalcDropEdit1.UnitPreBoundMaxLength = 2;
			//
			// importCustomsQtyCalcDropEdit
			//
			this.importCustomsQtyCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.importCustomsQtyCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobComInvoiceLine)(null)).JI_ImportCustomsQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobComInvoiceLine)(null)).JI_ImportCustomsQtyUQ)));
			this.importCustomsQtyCalcDropEdit.BindToAmount = "JI_ImportCustomsQty";
			this.importCustomsQtyCalcDropEdit.BindToUnit = "JI_ImportCustomsQtyUQ";
			this.importCustomsQtyCalcDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("fb41157a-9063-45a5-9af8-6c99a82d2827", "Customs Quantity");
			this.importCustomsQtyCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 80, true);
			this.importCustomsQtyCalcDropEdit.Name = "importCustomsQtyCalcDropEdit";
			this.importCustomsQtyCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.importCustomsQtyCalcDropEdit.TabIndex = 5;
			this.importCustomsQtyCalcDropEdit.UnitPreBoundMaxLength = 2;
			//
			// ImportTariffCodeFindBox1
			//
			this.ImportTariffCodeFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImportTariffCodeFindBox1, "TariffFromRelatedImportBOE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobComInvoiceLine)(null)).TariffFromRelatedImportBOE)));
			this.ImportTariffCodeFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 55, true);
			this.ImportTariffCodeFindBox1.Name = "ImportTariffCodeFindBox1";
			this.ImportTariffCodeFindBox1.PreBoundMaxLength = 10;
			this.ImportTariffCodeFindBox1.ShowDescriptionBox = false;
			this.ImportTariffCodeFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 20, true);
			this.ImportTariffCodeFindBox1.TabIndex = 4;
			this.ImportTariffCodeFindBox1.TariffType = "1P1";
			//
			// ImportTariffCodeFindBox
			//
			this.ImportTariffCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImportTariffCodeFindBox, JobComInvoiceLine.Schema.JI_ImportTariff);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobComInvoiceLine)(null)).JI_ImportTariff)));
			this.ImportTariffCodeFindBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("f5dd5ad6-316c-47d4-b3dc-3d5ed44086d8", "Tariff");
			this.ImportTariffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 55, true);
			this.ImportTariffCodeFindBox.Name = "ImportTariffCodeFindBox";
			this.ImportTariffCodeFindBox.PreBoundMaxLength = 10;
			this.ImportTariffCodeFindBox.ShowDescriptionBox = false;
			this.ImportTariffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 20, true);
			this.ImportTariffCodeFindBox.TabIndex = 3;
			this.ImportTariffCodeFindBox.TariffType = "1P1";
			//
			// zLabel3
			//
			this.zLabel3.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("42c4d5f8-3bfd-40c7-a239-db99fdecaa1c", "Original Values");
			this.zLabel3.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.zLabel3.IsFontBold = true;
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 37, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 16, true);
			this.zLabel3.TabIndex = 2;
			//
			// zLabel1
			//
			this.BindingSource.SetBindingMember(this.zLabel1, "RelatedImportBOENumberString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobComInvoiceLine)(null)).RelatedImportBOENumberString)));
			this.zLabel1.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 9, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 28, true);
			this.zLabel1.TabIndex = 0;
			//
			// zLabel2
			//
			this.zLabel2.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("7d412437-2b70-492a-be4a-b5c650b9da16", "DA63 Values");
			this.zLabel2.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.zLabel2.IsFontBold = true;
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 37, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 16, true);
			this.zLabel2.TabIndex = 1;
			//
			// importVATPaidCalcEdit
			//
			this.BindingSource.SetBindingMember(this.importVATPaidCalcEdit, JobComInvoiceLine.Schema.JI_ImportVATPaid);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobComInvoiceLine)(null)).JI_ImportVATPaid)));
			this.importVATPaidCalcEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("550b102d-93ca-423c-9a94-09524cfe7637", "Customs Vat");
			this.importVATPaidCalcEdit.DecimalPlaces = 2;
			this.importVATPaidCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 155, true);
			this.importVATPaidCalcEdit.Name = "importVATPaidCalcEdit";
			this.importVATPaidCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.importVATPaidCalcEdit.TabIndex = 13;
			this.importVATPaidCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// additionalQuantityGroupBox
			//
			this.additionalQuantityGroupBox.Controls.Add(this.firstAddUnitCalcDropEdit);
			this.additionalQuantityGroupBox.Controls.Add(this.secondAddUnitCalcDropEdit);
			this.additionalQuantityGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.additionalQuantityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(353, 16, true);
			this.additionalQuantityGroupBox.Name = "additionalQuantityGroupBox";
			this.additionalQuantityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(476, 149, true);
			this.additionalQuantityGroupBox.TabIndex = 16;
			this.additionalQuantityGroupBox.TabStop = false;
			this.additionalQuantityGroupBox.Text = "Additional Quantities";
			//
			// firstAddUnitCalcDropEdit
			//
			this.firstAddUnitCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.firstAddUnitCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobComInvoiceLine)(null)).JI_ImportCustomsQty2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobComInvoiceLine)(null)).JI_ImportCustomsQty2UQ)));
			this.firstAddUnitCalcDropEdit.BindToAmount = "JI_ImportCustomsQty2";
			this.firstAddUnitCalcDropEdit.BindToUnit = "JI_ImportCustomsQty2UQ";
			this.firstAddUnitCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 15, true);
			this.firstAddUnitCalcDropEdit.Name = "firstAddUnitCalcDropEdit";
			this.firstAddUnitCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.firstAddUnitCalcDropEdit.TabIndex = 0;
			this.firstAddUnitCalcDropEdit.UnitPreBoundMaxLength = 4;
			//
			// secondAddUnitCalcDropEdit
			//
			this.secondAddUnitCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.secondAddUnitCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobComInvoiceLine)(null)).JI_ImportCustomsQty3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobComInvoiceLine)(null)).JI_ImportCustomsQty3UQ)));
			this.secondAddUnitCalcDropEdit.BindToAmount = "JI_ImportCustomsQty3";
			this.secondAddUnitCalcDropEdit.BindToUnit = "JI_ImportCustomsQty3UQ";
			this.secondAddUnitCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 37, true);
			this.secondAddUnitCalcDropEdit.Name = "secondAddUnitCalcDropEdit";
			this.secondAddUnitCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.secondAddUnitCalcDropEdit.TabIndex = 1;
			this.secondAddUnitCalcDropEdit.UnitPreBoundMaxLength = 4;
			//
			// additionalDutiesGroupBox
			//
			this.additionalDutiesGroupBox.Controls.Add(this.additionalDutiesGrid);
			this.additionalDutiesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.additionalDutiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.additionalDutiesGroupBox.Name = "additionalDutiesGroupBox";
			this.additionalDutiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 149, true);
			this.additionalDutiesGroupBox.TabIndex = 15;
			this.additionalDutiesGroupBox.TabStop = false;
			this.additionalDutiesGroupBox.Text = "Additional Duties";
			//
			// additionalDutiesGrid
			//
			this.additionalDutiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.additionalDutiesGrid, "DA63AdditionalDuties");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobComInvoiceLine)(null)).DA63AdditionalDuties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DA63AdditionalDuty)(((System.Collections.IList)(((JobComInvoiceLine)(null)).DA63AdditionalDuties)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DA63AdditionalDuty)(((System.Collections.IList)(((JobComInvoiceLine)(null)).DA63AdditionalDuties)).SyncRoot)).CY_Value)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DA63AdditionalDuty)(((System.Collections.IList)(((JobComInvoiceLine)(null)).DA63AdditionalDuties)).SyncRoot)).OriginValue)));
			this.additionalDutiesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CY_Value";
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "OriginValue";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.additionalDutiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.additionalDutiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.additionalDutiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.additionalDutiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.additionalDutiesGrid.GridId = "2d8d05a6-3856-442f-8a52-75d3906589c5";
			this.additionalDutiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.additionalDutiesGrid.LayoutKey = "zGrid1";
			this.additionalDutiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.additionalDutiesGrid.Name = "additionalDutiesGrid";
			this.additionalDutiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 130, true);
			this.additionalDutiesGrid.TabIndex = 0;
			//
			// bottomGroupBox
			//
			this.bottomGroupBox.Controls.Add(this.additionalDutiesGroupBox);
			this.bottomGroupBox.Controls.Add(this.additionalQuantityGroupBox);
			this.bottomGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.bottomGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 212, true);
			this.bottomGroupBox.Name = "bottomGroupBox";
			this.bottomGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 168, true);
			this.bottomGroupBox.TabIndex = 17;
			this.bottomGroupBox.TabStop = false;
			//
			// topGroupBox
			//
			this.topGroupBox.Controls.Add(this.previousConversionFactorCalcEdit);
			this.topGroupBox.Controls.Add(this.conversionFactorCalcEdit);
			this.topGroupBox.Controls.Add(this.importVATPaidCalcEdit1);
			this.topGroupBox.Controls.Add(this.importVATPaidCalcEdit);
			this.topGroupBox.Controls.Add(this.importDutyPaidCalcEdit1);
			this.topGroupBox.Controls.Add(this.importDutyPaidCalcEdit);
			this.topGroupBox.Controls.Add(this.importCustomsValueCalcEditCalcEdit1);
			this.topGroupBox.Controls.Add(this.importCustomsValueCalcEdit);
			this.topGroupBox.Controls.Add(this.importCustomsQtyCalcDropEdit1);
			this.topGroupBox.Controls.Add(this.importCustomsQtyCalcDropEdit);
			this.topGroupBox.Controls.Add(this.ImportTariffCodeFindBox1);
			this.topGroupBox.Controls.Add(this.ImportTariffCodeFindBox);
			this.topGroupBox.Controls.Add(this.zLabel3);
			this.topGroupBox.Controls.Add(this.zLabel2);
			this.topGroupBox.Controls.Add(this.zLabel1);
			this.topGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.topGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topGroupBox.Name = "topGroupBox";
			this.topGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 212, true);
			this.topGroupBox.TabIndex = 18;
			this.topGroupBox.TabStop = false;
			// 
			// previousConversionFactorCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.previousConversionFactorCalcEdit, "ConversionFactorFromRelatedImportBOE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobComInvoiceLine)(null)).ConversionFactorFromRelatedImportBOE)));
			this.previousConversionFactorCalcEdit.CaptionResourceString = ((ResourceStringData)(resources.GetObject("previousConversionFactorCalcEdit.CaptionResourceString")));
			this.previousConversionFactorCalcEdit.DecimalPlaces = 5;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.previousConversionFactorCalcEdit, false);
			this.previousConversionFactorCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 181, true);
			this.previousConversionFactorCalcEdit.Name = "previousConversionFactorCalcEdit";
			this.previousConversionFactorCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.previousConversionFactorCalcEdit.TabIndex = 16;
			this.previousConversionFactorCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// conversionFactorCalcEdit
			//
			this.BindingSource.SetBindingMember(this.conversionFactorCalcEdit, JobComInvoiceLine.Schema.JI_ConversionFactor);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobComInvoiceLine)(null)).JI_ConversionFactor)));
			this.conversionFactorCalcEdit.CaptionResourceString = ((ResourceStringData)(resources.GetObject("conversionFactorCalcEdit.CaptionResourceString")));
			this.conversionFactorCalcEdit.DecimalPlaces = 5;
			this.conversionFactorCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 181, true);
			this.conversionFactorCalcEdit.Name = "conversionFactorCalcEdit";
			this.conversionFactorCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.conversionFactorCalcEdit.TabIndex = 15;
			this.conversionFactorCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// DA63UserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.bottomGroupBox);
			this.Controls.Add(this.topGroupBox);
			this.Name = "DA63UserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 380, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.importCustomsQtyCalcDropEdit1.ResumeLayout(true);
			this.importCustomsQtyCalcDropEdit1.PerformLayout();
			this.importCustomsQtyCalcDropEdit.ResumeLayout(true);
			this.importCustomsQtyCalcDropEdit.PerformLayout();
			this.ImportTariffCodeFindBox1.ResumeLayout(true);
			this.ImportTariffCodeFindBox1.PerformLayout();
			this.ImportTariffCodeFindBox.ResumeLayout(true);
			this.ImportTariffCodeFindBox.PerformLayout();
			this.additionalQuantityGroupBox.ResumeLayout(false);
			this.additionalQuantityGroupBox.PerformLayout();
			this.firstAddUnitCalcDropEdit.ResumeLayout(true);
			this.firstAddUnitCalcDropEdit.PerformLayout();
			this.secondAddUnitCalcDropEdit.ResumeLayout(true);
			this.secondAddUnitCalcDropEdit.PerformLayout();
			this.additionalDutiesGroupBox.ResumeLayout(false);
			this.additionalDutiesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.additionalDutiesGrid)).EndInit();
			this.additionalDutiesGrid.ResumeLayout(false);
			this.additionalDutiesGrid.PerformLayout();
			this.bottomGroupBox.ResumeLayout(false);
			this.bottomGroupBox.PerformLayout();
			this.topGroupBox.ResumeLayout(false);
			this.topGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

	}
}

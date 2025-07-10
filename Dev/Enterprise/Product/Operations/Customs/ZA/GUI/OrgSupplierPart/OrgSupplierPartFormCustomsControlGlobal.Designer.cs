using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class OrgSupplierPartFormCustomsControlGlobal
	{
		void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.GoodsOriginBoundFindBox = new ZArchitecture.GUI.ZCodeFindBox();
			this.PreferenceDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.tradeAgreementLabel = new ZArchitecture.ZLabel();
			this.ROOTypeDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.RulesOfOriginCertificateTextBox = new ZArchitecture.ZTextBox();
			this.GoodsTypeDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.EngineCapacityTextBox = new ZArchitecture.ZTextBox();
			this.VehicleFormatDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.VehicleTypeDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.additionalDutiesGroupBox = new ZArchitecture.GUI.ZGroupBox();
			this.additionalDutiesGrid = new ZArchitecture.ZGrid();
			this.VehicleColorTextBox = new ZArchitecture.ZTextBox();
			this.detailsPanel.SuspendLayout();
			this.DetailTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.CI_UsageCommentTextBox.SuspendLayout();
			this.AttributesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttributesLeftSplitContainer)).BeginInit();
			this.AttributesLeftSplitContainer.Panel1.SuspendLayout();
			this.AttributesLeftSplitContainer.Panel2.SuspendLayout();
			this.AttributesLeftSplitContainer.SuspendLayout();
			this.Attributes1GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes1Grid)).BeginInit();
			this.Attributes1Grid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttributesRightSplitContainer)).BeginInit();
			this.AttributesRightSplitContainer.Panel1.SuspendLayout();
			this.AttributesRightSplitContainer.Panel2.SuspendLayout();
			this.AttributesRightSplitContainer.SuspendLayout();
			this.Attributes2GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes2Grid)).BeginInit();
			this.Attributes2Grid.SuspendLayout();
			this.Attributes3GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes3Grid)).BeginInit();
			this.Attributes3Grid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PivotGrid)).BeginInit();
			this.PivotGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GoodsOriginBoundFindBox.SuspendLayout();
			this.PreferenceDropEdit.SuspendLayout();
			this.ROOTypeDropEdit.SuspendLayout();
			this.GoodsTypeDropEdit.SuspendLayout();
			this.VehicleFormatDropEdit.SuspendLayout();
			this.VehicleTypeDropEdit.SuspendLayout();
			this.additionalDutiesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.additionalDutiesGrid)).BeginInit();
			this.additionalDutiesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// detailsPanel
			// 
			this.detailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 90, true);
			this.detailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 277, true);
			// 
			// DetailTabControl
			// 
			this.DetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 277, true);
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.Controls.Add(this.GoodsOriginBoundFindBox);
			this.DetailsTabPage.Controls.Add(this.tradeAgreementLabel);
			this.DetailsTabPage.Controls.Add(this.PreferenceDropEdit);
			this.DetailsTabPage.Controls.Add(this.ROOTypeDropEdit);
			this.DetailsTabPage.Controls.Add(this.RulesOfOriginCertificateTextBox);
			this.DetailsTabPage.Controls.Add(this.GoodsTypeDropEdit);
			this.DetailsTabPage.Controls.Add(this.EngineCapacityTextBox);
			this.DetailsTabPage.Controls.Add(this.VehicleFormatDropEdit);
			this.DetailsTabPage.Controls.Add(this.VehicleTypeDropEdit);
			this.DetailsTabPage.Controls.Add(this.VehicleColorTextBox);
			this.DetailsTabPage.Controls.Add(this.additionalDutiesGroupBox);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(761, 250, true);
			this.DetailsTabPage.Controls.SetChildIndex(this.ClassificationDescriptionTextBox, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.CI_UsageCommentTextBox, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.additionalDutiesGroupBox, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.VehicleColorTextBox, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.VehicleTypeDropEdit, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.VehicleFormatDropEdit, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.EngineCapacityTextBox, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.GoodsTypeDropEdit, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.RulesOfOriginCertificateTextBox, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.ROOTypeDropEdit, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.PreferenceDropEdit, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.tradeAgreementLabel, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.GoodsOriginBoundFindBox, 0);
			// 
			// CI_UsageCommentTextBox
			// 
			this.CI_UsageCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 9, true);
			this.CI_UsageCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			// 
			// ClassificationDescriptionTextBox
			// 
			this.ClassificationDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 206, true);
			this.ClassificationDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 20, true);
			// 
			// AttributesLeftSplitContainer
			// 
			// 
			// AttributesRightSplitContainer
			// 
			// 
			// PivotGrid
			// 
			this.PivotGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 90, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.OrgSupplierPart);
			// 
			// GoodsOriginBoundFindBox
			// 
			this.GoodsOriginBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsOriginBoundFindBox, "PivotsForBinding.CI_RN_NKCountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusClassPartPivot)(((System.Collections.IList)(((Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_RN_NKCountryOfOrigin);
			this.GoodsOriginBoundFindBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("16A48ABF-0493-48E1-B534-EBF13F415B85", "Goods Origin");
			this.GoodsOriginBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 31, true);
			this.GoodsOriginBoundFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.GoodsOriginBoundFindBox.Name = "GoodsOriginBoundFindBox";
			this.GoodsOriginBoundFindBox.PreBoundMaxLength = 2;
			this.GoodsOriginBoundFindBox.ShouldResize = true;
			this.GoodsOriginBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.GoodsOriginBoundFindBox.TabIndex = 6;
			// 
			// PreferenceDropEdit
			// 
			this.PreferenceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PreferenceDropEdit, "PivotsForBinding.CI_PrimaryPreference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusClassPartPivot)(((System.Collections.IList)(((Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_PrimaryPreference);
			this.PreferenceDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("96CEAE19-D1B8-4638-961C-E5CB1B85EBD8", "Preference");
			this.PreferenceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 53, true);
			this.PreferenceDropEdit.Name = "PreferenceDropEdit";
			this.PreferenceDropEdit.PreBoundMaxLength = 8;
			this.PreferenceDropEdit.ShouldResizeByMaxLength = true;
			this.PreferenceDropEdit.ShowDescriptionBox = false;
			this.PreferenceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20, true);
			this.PreferenceDropEdit.TabIndex = 7;
			// 
			// TradeAgreementLabel
			// 
			this.BindingSource.SetBindingMember(this.tradeAgreementLabel, "PivotsForBinding.TradeAgreementLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusClassPartPivot)(((System.Collections.IList)(((Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).TradeAgreementLabel);
			this.tradeAgreementLabel.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif);
			this.tradeAgreementLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 53, true);
			this.tradeAgreementLabel.Name = "TradeAgreementLabel";
			this.tradeAgreementLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 18, true);
			this.tradeAgreementLabel.TabIndex = 8;
			// 
			// ROOTypeDropEdit
			// 
			this.ROOTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ROOTypeDropEdit, "PivotsForBinding.CI_PrimaryPreference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusClassPartPivot)(((System.Collections.IList)(((Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_PrimaryPreference);
			this.ROOTypeDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("6039481F-C839-4551-AAD0-62DFA74B7EB1", "ROO Type");
			this.ROOTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 53, true);
			this.ROOTypeDropEdit.Name = "ROOTypeDropEdit";
			this.ROOTypeDropEdit.ShouldResizeByMaxLength = true;
			this.ROOTypeDropEdit.ShowDescriptionBox = false;
			this.ROOTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ROOTypeDropEdit.TabIndex = 8;
			// 
			// RulesOfOriginCertificateTextBox
			// 
			this.BindingSource.SetBindingMember(this.RulesOfOriginCertificateTextBox, "PivotsForBinding.CI_ROOCert");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusClassPartPivot)(((System.Collections.IList)(((Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_ROOCert);
			this.RulesOfOriginCertificateTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("7670EA87-B1A2-44DF-AC7E-76AF554E0DBD", "ROO Certificate");
			this.RulesOfOriginCertificateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 75, true);
			this.RulesOfOriginCertificateTextBox.Name = "RulesOfOriginCertificateTextBox";
			this.RulesOfOriginCertificateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.RulesOfOriginCertificateTextBox.TabIndex = 9;
			// 
			// GoodsTypeDropEdit
			// 
			this.GoodsTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsTypeDropEdit, "PivotsForBinding.CI_NewUsed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusClassPartPivot)(((System.Collections.IList)(((Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_NewUsed);
			this.GoodsTypeDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("E5BC480D-68A3-4DF9-931F-07A9BF9B9533", "Goods Type");
			this.GoodsTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 97, true);
			this.GoodsTypeDropEdit.Name = "GoodsTypeDropEdit";
			this.GoodsTypeDropEdit.ShouldResizeByMaxLength = true;
			this.GoodsTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.GoodsTypeDropEdit.TabIndex = 10;
			// 
			// EngineCapacityTextBox
			// 
			this.BindingSource.SetBindingMember(this.EngineCapacityTextBox, "PivotsForBinding.CI_EngineCapacity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusClassPartPivot)(((System.Collections.IList)(((Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_EngineCapacity);
			this.EngineCapacityTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("8C0681CB-A50B-4002-9B0F-B3A065D10E9A", "Engine Capacity");
			this.EngineCapacityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 119, true);
			this.EngineCapacityTextBox.Name = "EngineCapacityTextBox";
			this.EngineCapacityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.EngineCapacityTextBox.TabIndex = 11;
			// 
			// VehicleFormatDropEdit
			// 
			this.VehicleFormatDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VehicleFormatDropEdit, "PivotsForBinding.CI_VehicleFormat");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusClassPartPivot)(((System.Collections.IList)(((Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_VehicleFormat);
			this.VehicleFormatDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("D19CAE3A-D9D4-4C05-B555-2065D9925CED", "Vehicle Format");
			this.VehicleFormatDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 141, true);
			this.VehicleFormatDropEdit.Name = "VehicleFormatDropEdit";
			this.VehicleFormatDropEdit.ShouldResizeByMaxLength = true;
			this.VehicleFormatDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 20, true);
			this.VehicleFormatDropEdit.TabIndex = 12;
			// 
			// VehicleTypeDropEdit
			// 
			this.VehicleTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VehicleTypeDropEdit, "PivotsForBinding.CI_VehicleType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusClassPartPivot)(((System.Collections.IList)(((Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_VehicleType);
			this.VehicleTypeDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ADF488A7-7F02-4768-AC04-C0DA1F460785", "Vehicle Type");
			this.VehicleTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 163, true);
			this.VehicleTypeDropEdit.Name = "VehicleTypeDropEdit";
			this.VehicleTypeDropEdit.ShouldResizeByMaxLength = true;
			this.VehicleTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 20, true);
			this.VehicleTypeDropEdit.TabIndex = 13;
			// 
			// AdditionalDutiesGroupBox
			// 
			this.additionalDutiesGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("e7cc7b71-1db1-483e-918e-82acbd7b9837", "Additional Tariffs");
			this.additionalDutiesGroupBox.Controls.Add(this.additionalDutiesGrid);
			this.additionalDutiesGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.additionalDutiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(324, 3, true);
			this.additionalDutiesGroupBox.Name = "AdditionalDutiesGroupBox";
			this.additionalDutiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 244, true);
			this.additionalDutiesGroupBox.TabIndex = 0;
			this.additionalDutiesGroupBox.TabStop = false;
			// 
			// AdditionalDutiesGrid
			// 
			this.additionalDutiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.additionalDutiesGrid, "PivotsForBinding.CusLineTariffDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusClassPartPivot)(((System.Collections.IList)(((Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CusLineTariffDetails);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusLineTariffDetail)(((Business.CusClassPartPivot)(((System.Collections.IList)(((Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CusLineTariffDetails.SyncRoot)).BZ_Type);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusLineTariffDetail)(((Business.CusClassPartPivot)(((System.Collections.IList)(((Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CusLineTariffDetails.SyncRoot)).BZ_Tariff);
			this.additionalDutiesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "BZ_Type";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "BZ_Tariff";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.Customs.Common.ModuleRegistration.CustomsModuleIDs.Universal.RefCusTariff;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.additionalDutiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.additionalDutiesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.additionalDutiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.additionalDutiesGrid.GridId = "8A90D169-24C0-47B3-9310-22D46AE9E0B7";
			this.additionalDutiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.additionalDutiesGrid.LayoutKey = "zGrid1";
			this.additionalDutiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.additionalDutiesGrid.Name = "AdditionalDutiesGrid";
			this.additionalDutiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 225, true);
			this.additionalDutiesGrid.TabIndex = 0;
			// 
			// VehicleColorTextBox
			// 
			this.BindingSource.SetBindingMember(this.VehicleColorTextBox, "PivotsForBinding.CI_Colour");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusClassPartPivot)(((System.Collections.IList)(((Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_Colour);
			this.VehicleColorTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("3A229DA7-1FB0-4739-93FA-076FAF564563", "Vehicle Color");
			this.VehicleColorTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 185, true);
			this.VehicleColorTextBox.Name = "VehicleColorTextBox";
			this.VehicleColorTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.VehicleColorTextBox.TabIndex = 14;
			// 
			// OrgSupplierPartFormCustomsControlGlobal
			// 
			this.Name = "OrgSupplierPartFormCustomsControlGlobal";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 405, true);
			this.detailsPanel.ResumeLayout(false);
			this.detailsPanel.PerformLayout();
			this.DetailTabControl.ResumeLayout(false);
			this.DetailTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.CI_UsageCommentTextBox.ResumeLayout(true);
			this.CI_UsageCommentTextBox.PerformLayout();
			this.AttributesTabPage.ResumeLayout(false);
			this.AttributesTabPage.PerformLayout();
			this.AttributesLeftSplitContainer.Panel1.ResumeLayout(false);
			this.AttributesLeftSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AttributesLeftSplitContainer)).EndInit();
			this.AttributesLeftSplitContainer.ResumeLayout(false);
			this.AttributesLeftSplitContainer.PerformLayout();
			this.Attributes1GroupBox.ResumeLayout(false);
			this.Attributes1GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes1Grid)).EndInit();
			this.Attributes1Grid.ResumeLayout(false);
			this.Attributes1Grid.PerformLayout();
			this.AttributesRightSplitContainer.Panel1.ResumeLayout(false);
			this.AttributesRightSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AttributesRightSplitContainer)).EndInit();
			this.AttributesRightSplitContainer.ResumeLayout(false);
			this.AttributesRightSplitContainer.PerformLayout();
			this.Attributes2GroupBox.ResumeLayout(false);
			this.Attributes2GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes2Grid)).EndInit();
			this.Attributes2Grid.ResumeLayout(false);
			this.Attributes2Grid.PerformLayout();
			this.Attributes3GroupBox.ResumeLayout(false);
			this.Attributes3GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes3Grid)).EndInit();
			this.Attributes3Grid.ResumeLayout(false);
			this.Attributes3Grid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PivotGrid)).EndInit();
			this.PivotGrid.ResumeLayout(false);
			this.PivotGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GoodsOriginBoundFindBox.ResumeLayout(true);
			this.GoodsOriginBoundFindBox.PerformLayout();
			this.PreferenceDropEdit.ResumeLayout(true);
			this.PreferenceDropEdit.PerformLayout();
			this.ROOTypeDropEdit.ResumeLayout(true);
			this.ROOTypeDropEdit.PerformLayout();
			this.GoodsTypeDropEdit.ResumeLayout(true);
			this.GoodsTypeDropEdit.PerformLayout();
			this.VehicleFormatDropEdit.ResumeLayout(true);
			this.VehicleFormatDropEdit.PerformLayout();
			this.VehicleTypeDropEdit.ResumeLayout(true);
			this.VehicleTypeDropEdit.PerformLayout();
			this.additionalDutiesGroupBox.ResumeLayout(false);
			this.additionalDutiesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.additionalDutiesGrid)).EndInit();
			this.additionalDutiesGrid.ResumeLayout(false);
			this.additionalDutiesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

	}
}

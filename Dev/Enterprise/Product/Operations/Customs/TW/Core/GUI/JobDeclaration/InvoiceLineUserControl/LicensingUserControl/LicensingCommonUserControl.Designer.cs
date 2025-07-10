
namespace Enterprise.Customs.TW.GUI
{
	partial class LicensingCommonUserControl
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
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            this.CommodityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.PreviousPermitNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CustomsThirdQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.JI_ProductThicknessTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.JI_ProductGradeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.TariffExtensionCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.GoodsTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ShippingIdentificationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ShippingIdentificationGrid = new Enterprise.ZArchitecture.ZGrid();
            this.PackagingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.JI_InnerPackDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.TW_InnerPackingMaterialDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.TW_InnerPackTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.CommodityGroupBox.SuspendLayout();
            this.CustomsThirdQuantityCalcDropEdit.SuspendLayout();
            this.GoodsTypeDropEdit.SuspendLayout();
            this.ShippingIdentificationGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ShippingIdentificationGrid)).BeginInit();
            this.ShippingIdentificationGrid.SuspendLayout();
            this.PackagingGroupBox.SuspendLayout();
            this.TW_InnerPackingMaterialDropEdit.SuspendLayout();
            this.TW_InnerPackTypeDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclaration);
            // 
            // CommodityGroupBox
            // 
            this.CommodityGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("FBD03DDD-EFB8-480D-8DA6-C14BEE6A4935", "Commodity");
            this.CommodityGroupBox.Controls.Add(this.PreviousPermitNoTextBox);
            this.CommodityGroupBox.Controls.Add(this.CustomsThirdQuantityCalcDropEdit);
            this.CommodityGroupBox.Controls.Add(this.JI_ProductThicknessTextBox);
            this.CommodityGroupBox.Controls.Add(this.JI_ProductGradeTextBox);
            this.CommodityGroupBox.Controls.Add(this.TariffExtensionCodeTextBox);
            this.CommodityGroupBox.Controls.Add(this.GoodsTypeDropEdit);
            this.CommodityGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.CommodityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.CommodityGroupBox.Name = "CommodityGroupBox";
            this.CommodityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 286, true);
            this.CommodityGroupBox.TabIndex = 0;
            this.CommodityGroupBox.TabStop = false;
            // 
            // PreviousPermitNoTextBox
            // 
            this.BindingSource.SetBindingMember(this.PreviousPermitNoTextBox, "FilteredInvoiceLines.PreviousPermitNo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousPermitNo)));
            this.PreviousPermitNoTextBox.CaptionResourceString = null;
            this.PreviousPermitNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 67, true);
            this.PreviousPermitNoTextBox.Name = "PreviousPermitNoTextBox";
            this.PreviousPermitNoTextBox.ShouldEscapeAllSpecialCharacters = false;
            this.PreviousPermitNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 17, true);
            this.PreviousPermitNoTextBox.TabIndex = 2;
            // 
            // CustomsThirdQuantityCalcDropEdit
            // 
            this.CustomsThirdQuantityCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CustomsThirdQuantityCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsThirdQuantity)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsThirdUnitQty)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.CustomsUQList)));
            this.CustomsThirdQuantityCalcDropEdit.BindToAmount = "FilteredInvoiceLines.JI_CustomsThirdQuantity";
            this.CustomsThirdQuantityCalcDropEdit.BindToList = "FilteredInvoiceLines.Lookups+CustomsUQList";
            this.CustomsThirdQuantityCalcDropEdit.BindToUnit = "FilteredInvoiceLines.JI_CustomsThirdUnitQty";
            this.CustomsThirdQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 17, true);
            this.CustomsThirdQuantityCalcDropEdit.Name = "CustomsThirdQuantityCalcDropEdit";
            this.CustomsThirdQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 17, true);
            this.CustomsThirdQuantityCalcDropEdit.TabIndex = 0;
            this.CustomsThirdQuantityCalcDropEdit.UnitPreBoundMaxLength = 3;
            // 
            // JI_ProductThicknessTextBox
            // 
            this.BindingSource.SetBindingMember(this.JI_ProductThicknessTextBox, "FilteredInvoiceLines.JI_ProductThickness");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ProductThickness)));
            this.JI_ProductThicknessTextBox.CaptionResourceString = null;
            this.JI_ProductThicknessTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 92, true);
            this.JI_ProductThicknessTextBox.Name = "JI_ProductThicknessTextBox";
            this.JI_ProductThicknessTextBox.ShouldEscapeAllSpecialCharacters = false;
            this.JI_ProductThicknessTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 17, true);
            this.JI_ProductThicknessTextBox.TabIndex = 3;
            // 
            // JI_ProductGradeTextBox
            // 
            this.BindingSource.SetBindingMember(this.JI_ProductGradeTextBox, "FilteredInvoiceLines.JI_ProductGrade");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ProductGrade)));
            this.JI_ProductGradeTextBox.CaptionResourceString = null;
            this.JI_ProductGradeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 117, true);
            this.JI_ProductGradeTextBox.Name = "JI_ProductGradeTextBox";
            this.JI_ProductGradeTextBox.ShouldEscapeAllSpecialCharacters = false;
            this.JI_ProductGradeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 17, true);
            this.JI_ProductGradeTextBox.TabIndex = 4;
            // 
            // TariffExtensionCodeTextBox
            // 
            this.BindingSource.SetBindingMember(this.TariffExtensionCodeTextBox, "FilteredInvoiceLines.JI_TariffExtensionCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_TariffExtensionCode)));
            this.TariffExtensionCodeTextBox.CaptionResourceString = null;
            this.TariffExtensionCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 142, true);
            this.TariffExtensionCodeTextBox.Name = "TariffExtensionCodeTextBox";
            this.TariffExtensionCodeTextBox.ShouldEscapeAllSpecialCharacters = false;
            this.TariffExtensionCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 17, true);
            this.TariffExtensionCodeTextBox.TabIndex = 5;
            // 
            // GoodsTypeDropEdit
            // 
            this.GoodsTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.GoodsTypeDropEdit, "FilteredInvoiceLines.JI_GoodsType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_GoodsType)));
            this.GoodsTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 42, true);
            this.GoodsTypeDropEdit.Name = "GoodsTypeDropEdit";
            this.GoodsTypeDropEdit.PreBoundMaxLength = 3;
            this.GoodsTypeDropEdit.ShouldResizeByMaxLength = true;
            this.GoodsTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 17, true);
            this.GoodsTypeDropEdit.TabIndex = 1;
            // 
            // ShippingIdentificationGroupBox
            // 
            this.ShippingIdentificationGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("d05a6f71-b07e-4a25-8e53-961430ef17a4", "Shipping Identification");
            this.ShippingIdentificationGroupBox.Controls.Add(this.ShippingIdentificationGrid);
            this.ShippingIdentificationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ShippingIdentificationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(630, 0, true);
            this.ShippingIdentificationGroupBox.Name = "ShippingIdentificationGroupBox";
            this.ShippingIdentificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 286, true);
            this.ShippingIdentificationGroupBox.TabIndex = 2;
            this.ShippingIdentificationGroupBox.TabStop = false;
            // 
            // ShippingIdentificationGrid
            // 
            this.ShippingIdentificationGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.ShippingIdentificationGrid, "FilteredInvoiceLines.ShippingIdentificationDataCollection");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ShippingIdentificationDataCollection)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.ShippingIdentificationData)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ShippingIdentificationDataCollection)).SyncRoot)).TW_ManufacturedLotNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.TW.Business.ShippingIdentificationData)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ShippingIdentificationDataCollection)).SyncRoot)).TW_ExpirationDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.TW.Business.ShippingIdentificationData)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ShippingIdentificationDataCollection)).SyncRoot)).TW_ManufacturedDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.ShippingIdentificationData)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ShippingIdentificationDataCollection)).SyncRoot)).TW_ProductLotNoAmount)));
            this.ShippingIdentificationGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.ColumnName = "TW_ManufacturedLotNo";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
            zDateEditColumnStyleInfo1.ColumnName = "TW_ExpirationDate";
            zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zDateEditColumnStyleInfo2.ColumnName = "TW_ManufacturedDate";
            zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "TW_ProductLotNoAmount";
            zCalcEditColumnStyleInfo1.Decimals = 4;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(165);
            this.ShippingIdentificationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.ShippingIdentificationGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.ShippingIdentificationGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
            this.ShippingIdentificationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.ShippingIdentificationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ShippingIdentificationGrid.GridId = "0c3615f7-af88-4403-a59b-aa1f898232d4";
            this.ShippingIdentificationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ShippingIdentificationGrid.LayoutKey = "ShippingIdentificationGrid";
            this.ShippingIdentificationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
            this.ShippingIdentificationGrid.Name = "ShippingIdentificationGrid";
            this.ShippingIdentificationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 270, true);
            this.ShippingIdentificationGrid.TabIndex = 0;
            // 
            // PackagingGroupBox
            // 
            this.PackagingGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("3BEBB538-E6B1-4640-AB6A-A23CB143E1C9", "Packaging");
            this.PackagingGroupBox.Controls.Add(this.JI_InnerPackDescriptionTextBox);
            this.PackagingGroupBox.Controls.Add(this.TW_InnerPackingMaterialDropEdit);
            this.PackagingGroupBox.Controls.Add(this.TW_InnerPackTypeDropEdit);
            this.PackagingGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.PackagingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 0, true);
            this.PackagingGroupBox.Name = "PackagingGroupBox";
            this.PackagingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 286, true);
            this.PackagingGroupBox.TabIndex = 1;
            this.PackagingGroupBox.TabStop = false;
            // 
            // JI_InnerPackDescriptionTextBox
            // 
            this.BindingSource.SetBindingMember(this.JI_InnerPackDescriptionTextBox, "FilteredInvoiceLines.JI_InnerPackDescription");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_InnerPackDescription)));
            this.JI_InnerPackDescriptionTextBox.CaptionResourceString = null;
            this.JI_InnerPackDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 67, true);
            this.JI_InnerPackDescriptionTextBox.Multiline = true;
            this.JI_InnerPackDescriptionTextBox.Name = "JI_InnerPackDescriptionTextBox";
            this.JI_InnerPackDescriptionTextBox.ShouldEscapeAllSpecialCharacters = false;
            this.JI_InnerPackDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 209, true);
            this.JI_InnerPackDescriptionTextBox.TabIndex = 2;
            // 
            // TW_InnerPackingMaterialDropEdit
            // 
            this.TW_InnerPackingMaterialDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TW_InnerPackingMaterialDropEdit, "FilteredInvoiceLines.JI_InnerPackingMaterial");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_InnerPackingMaterial)));
            this.TW_InnerPackingMaterialDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 42, true);
            this.TW_InnerPackingMaterialDropEdit.Name = "TW_InnerPackingMaterialDropEdit";
            this.TW_InnerPackingMaterialDropEdit.PreBoundMaxLength = 3;
            this.TW_InnerPackingMaterialDropEdit.ShouldResizeByMaxLength = true;
            this.TW_InnerPackingMaterialDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 17, true);
            this.TW_InnerPackingMaterialDropEdit.TabIndex = 1;
            // 
            // TW_InnerPackTypeDropEdit
            // 
            this.TW_InnerPackTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TW_InnerPackTypeDropEdit, "FilteredInvoiceLines.JI_InnerPackType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_InnerPackType)));
            this.TW_InnerPackTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 17, true);
            this.TW_InnerPackTypeDropEdit.Name = "TW_InnerPackTypeDropEdit";
            this.TW_InnerPackTypeDropEdit.PreBoundMaxLength = 3;
            this.TW_InnerPackTypeDropEdit.ShouldResizeByMaxLength = false;
            this.TW_InnerPackTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 17, true);
            this.TW_InnerPackTypeDropEdit.TabIndex = 0;
            // 
            // LicensingCommonUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ShippingIdentificationGroupBox);
            this.Controls.Add(this.PackagingGroupBox);
            this.Controls.Add(this.CommodityGroupBox);
            this.Name = "LicensingCommonUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 286, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.CommodityGroupBox.ResumeLayout(false);
            this.CommodityGroupBox.PerformLayout();
            this.CustomsThirdQuantityCalcDropEdit.ResumeLayout(true);
            this.CustomsThirdQuantityCalcDropEdit.PerformLayout();
            this.GoodsTypeDropEdit.ResumeLayout(true);
            this.GoodsTypeDropEdit.PerformLayout();
            this.ShippingIdentificationGroupBox.ResumeLayout(false);
            this.ShippingIdentificationGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ShippingIdentificationGrid)).EndInit();
            this.ShippingIdentificationGrid.ResumeLayout(false);
            this.ShippingIdentificationGrid.PerformLayout();
            this.PackagingGroupBox.ResumeLayout(false);
            this.PackagingGroupBox.PerformLayout();
            this.TW_InnerPackingMaterialDropEdit.ResumeLayout(true);
            this.TW_InnerPackingMaterialDropEdit.PerformLayout();
            this.TW_InnerPackTypeDropEdit.ResumeLayout(true);
            this.TW_InnerPackTypeDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox CommodityGroupBox;
		private ZArchitecture.GUI.ZCalcDropEdit CustomsThirdQuantityCalcDropEdit;
		private ZArchitecture.ZTextBox JI_ProductThicknessTextBox;
		private ZArchitecture.ZTextBox JI_ProductGradeTextBox;
		private ZArchitecture.ZTextBox TariffExtensionCodeTextBox;
		private ZArchitecture.GUI.ZDropEdit GoodsTypeDropEdit;
		private ZArchitecture.GUI.ZGroupBox ShippingIdentificationGroupBox;
		private ZArchitecture.ZGrid ShippingIdentificationGrid;
		private ZArchitecture.GUI.ZGroupBox PackagingGroupBox;
		private ZArchitecture.GUI.ZDropEdit TW_InnerPackTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit TW_InnerPackingMaterialDropEdit;
		private ZArchitecture.ZTextBox JI_InnerPackDescriptionTextBox;
		public ZArchitecture.ZTextBox PreviousPermitNoTextBox;
	}
}

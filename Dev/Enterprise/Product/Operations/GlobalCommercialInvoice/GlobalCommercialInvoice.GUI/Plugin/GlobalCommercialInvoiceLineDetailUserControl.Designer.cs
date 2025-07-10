namespace Enterprise.GlobalCommercialInvoice.GUI
{
	partial class GlobalCommercialInvoiceLineDetailUserControl
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
			this.InvoiceLineGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InvoiceNumberGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.InvoiceLinePriceCurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InvoiceLinePriceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.InvoiceNetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.InvoiceGrossWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.InvoiceVolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.InvoiceQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.InvoiceGoodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InvoiceTariff2FindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.InvoiceTariff1FindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.InvoiceProductCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InvoiceLineGroupBox.SuspendLayout();
			this.InvoiceNumberGuidDropEdit.SuspendLayout();
			this.InvoiceNetWeightCalcDropEdit.SuspendLayout();
			this.InvoiceGrossWeightCalcDropEdit.SuspendLayout();
			this.InvoiceVolumeCalcDropEdit.SuspendLayout();
			this.InvoiceQuantityCalcDropEdit.SuspendLayout();
			this.InvoiceTariff2FindBox.SuspendLayout();
			this.InvoiceTariff1FindBox.SuspendLayout();
			this.InvoiceProductCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine);
			// 
			// InvoiceLineGroupBox
			// 
			this.InvoiceLineGroupBox.Controls.Add(this.InvoiceNumberGuidDropEdit);
			this.InvoiceLineGroupBox.Controls.Add(this.InvoiceLinePriceCurrencyTextBox);
			this.InvoiceLineGroupBox.Controls.Add(this.InvoiceLinePriceCalcEdit);
			this.InvoiceLineGroupBox.Controls.Add(this.InvoiceNetWeightCalcDropEdit);
			this.InvoiceLineGroupBox.Controls.Add(this.InvoiceGrossWeightCalcDropEdit);
			this.InvoiceLineGroupBox.Controls.Add(this.InvoiceVolumeCalcDropEdit);
			this.InvoiceLineGroupBox.Controls.Add(this.InvoiceQuantityCalcDropEdit);
			this.InvoiceLineGroupBox.Controls.Add(this.InvoiceGoodsDescriptionTextBox);
			this.InvoiceLineGroupBox.Controls.Add(this.InvoiceTariff2FindBox);
			this.InvoiceLineGroupBox.Controls.Add(this.InvoiceTariff1FindBox);
			this.InvoiceLineGroupBox.Controls.Add(this.InvoiceProductCodeFindBox);
			this.InvoiceLineGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceLineGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceLineGroupBox.Name = "InvoiceLineGroupBox";
			this.InvoiceLineGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 541, true);
			this.InvoiceLineGroupBox.TabIndex = 0;
			this.InvoiceLineGroupBox.TabStop = false;
			this.InvoiceLineGroupBox.Text = "Line Details";
			// 
			// InvoiceNumberGuidDropEdit
			// 
			this.InvoiceNumberGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceNumberGuidDropEdit, "GIL_GIH_Header");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(null)).GIL_GIH_Header)));
			this.InvoiceNumberGuidDropEdit.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("27fbf1a8-160f-4e74-a730-8fddee7062ab", "Invoice No.");
			this.InvoiceNumberGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 35, true);
			this.InvoiceNumberGuidDropEdit.Name = "InvoiceNumberGuidDropEdit";
			this.InvoiceNumberGuidDropEdit.ShowDescriptionBox = false;
			this.InvoiceNumberGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 17, true);
			this.InvoiceNumberGuidDropEdit.TabIndex = 0;
			// 
			// InvoiceLinePriceCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.InvoiceLinePriceCurrencyTextBox, "LinePriceCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(null)).LinePriceCurrency)));
			this.InvoiceLinePriceCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(566, 117, true);
			this.InvoiceLinePriceCurrencyTextBox.Name = "InvoiceLinePriceCurrencyTextBox";
			this.InvoiceLinePriceCurrencyTextBox.ReadOnly = true;
			this.InvoiceLinePriceCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 17, true);
			this.InvoiceLinePriceCurrencyTextBox.TabIndex = 11;
			// 
			// InvoiceLinePriceCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.InvoiceLinePriceCalcEdit, "GIL_LinePrice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(null)).GIL_LinePrice)));
			this.InvoiceLinePriceCalcEdit.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("9a744308-a6da-44fc-af45-901341c96f70", "Price");
			this.InvoiceLinePriceCalcEdit.DecimalPlaces = 2;
			this.InvoiceLinePriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(437, 117, true);
			this.InvoiceLinePriceCalcEdit.Name = "InvoiceLinePriceCalcEdit";
			this.InvoiceLinePriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 17, true);
			this.InvoiceLinePriceCalcEdit.TabIndex = 9;
			this.InvoiceLinePriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.InvoiceLinePriceCalcEdit.TrackDisposedAccess = true;
			// 
			// InvoiceNetWeightCalcDropEdit
			// 
			this.InvoiceNetWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceNetWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(null)).GIL_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(null)).GIL_NetWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(null)).Lookups.NetWeightUQList)));
			this.InvoiceNetWeightCalcDropEdit.BindToAmount = "GIL_NetWeight";
			this.InvoiceNetWeightCalcDropEdit.BindToList = "Lookups.NetWeightUQList";
			this.InvoiceNetWeightCalcDropEdit.BindToUnit = "GIL_NetWeightUQ";
			this.InvoiceNetWeightCalcDropEdit.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("95df1c15-fcd3-4bcc-aff8-e7bcd79dd349", "Net Weight");
			this.InvoiceNetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(437, 96, true);
			this.InvoiceNetWeightCalcDropEdit.Name = "InvoiceNetWeightCalcDropEdit";
			this.InvoiceNetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 17, true);
			this.InvoiceNetWeightCalcDropEdit.TabIndex = 8;
			// 
			// InvoiceGrossWeightCalcDropEdit
			// 
			this.InvoiceGrossWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceGrossWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(null)).GIL_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(null)).GIL_GrossWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(null)).Lookups.GrossWeightUQList)));
			this.InvoiceGrossWeightCalcDropEdit.BindToAmount = "GIL_GrossWeight";
			this.InvoiceGrossWeightCalcDropEdit.BindToList = "Lookups.GrossWeightUQList";
			this.InvoiceGrossWeightCalcDropEdit.BindToUnit = "GIL_GrossWeightUQ";
			this.InvoiceGrossWeightCalcDropEdit.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("707df38f-64cd-41d1-a8a7-da5b69c05a30", "Gross Weight");
			this.InvoiceGrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(437, 76, true);
			this.InvoiceGrossWeightCalcDropEdit.Name = "InvoiceGrossWeightCalcDropEdit";
			this.InvoiceGrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 17, true);
			this.InvoiceGrossWeightCalcDropEdit.TabIndex = 7;
			// 
			// InvoiceVolumeCalcDropEdit
			// 
			this.InvoiceVolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceVolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(null)).GIL_Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(null)).GIL_VolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(null)).Lookups.VolumeUQList)));
			this.InvoiceVolumeCalcDropEdit.BindToAmount = "GIL_Volume";
			this.InvoiceVolumeCalcDropEdit.BindToList = "Lookups.VolumeUQList";
			this.InvoiceVolumeCalcDropEdit.BindToUnit = "GIL_VolumeUQ";
			this.InvoiceVolumeCalcDropEdit.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("9f24c56c-6d7e-4aac-b534-496e0cdd4a6e", "Volume");
			this.InvoiceVolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(437, 56, true);
			this.InvoiceVolumeCalcDropEdit.Name = "InvoiceVolumeCalcDropEdit";
			this.InvoiceVolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 17, true);
			this.InvoiceVolumeCalcDropEdit.TabIndex = 6;
			// 
			// InvoiceQuantityCalcDropEdit
			// 
			this.InvoiceQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(null)).GIL_InvoiceQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(null)).GIL_InvoiceUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(null)).Lookups.InvoiceUQList)));
			this.InvoiceQuantityCalcDropEdit.BindToAmount = "GIL_InvoiceQuantity";
			this.InvoiceQuantityCalcDropEdit.BindToList = "Lookups.InvoiceUQList";
			this.InvoiceQuantityCalcDropEdit.BindToUnit = "GIL_InvoiceUQ";
			this.InvoiceQuantityCalcDropEdit.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("960778a9-532c-42f3-88be-2defa0f6ee8f", "Invoice Qty");
			this.InvoiceQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(437, 35, true);
			this.InvoiceQuantityCalcDropEdit.Name = "InvoiceQuantityCalcDropEdit";
			this.InvoiceQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 17, true);
			this.InvoiceQuantityCalcDropEdit.TabIndex = 5;
			// 
			// InvoiceGoodsDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.InvoiceGoodsDescriptionTextBox, "GIL_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(null)).GIL_Description)));
			this.InvoiceGoodsDescriptionTextBox.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("94aafa94-b252-48a1-b1bc-bf81375d4c70", "Goods Description");
			this.InvoiceGoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 117, true);
			this.InvoiceGoodsDescriptionTextBox.Multiline = true;
			this.InvoiceGoodsDescriptionTextBox.Name = "InvoiceGoodsDescriptionTextBox";
			this.InvoiceGoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 66, true);
			this.InvoiceGoodsDescriptionTextBox.TabIndex = 4;
			// 
			// InvoiceTariff2FindBox
			// 
			this.InvoiceTariff2FindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceTariff2FindBox, "GIL_Tariff2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(null)).GIL_Tariff2)));
			this.InvoiceTariff2FindBox.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("58d72aa0-3ec6-4dec-b816-2480534877a9", "Tariff 2");
			this.InvoiceTariff2FindBox.ErrorForUnsupportedCountry = null;
			this.InvoiceTariff2FindBox.GetDataGrouping = null;
			this.InvoiceTariff2FindBox.GetEffectiveDate = null;
			this.InvoiceTariff2FindBox.GetTariffType = null;
			this.InvoiceTariff2FindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 96, true);
			this.InvoiceTariff2FindBox.Name = "InvoiceTariff2FindBox";
			this.InvoiceTariff2FindBox.NeedLoadNomenclatureWhenTariffNotFound = false;
			this.InvoiceTariff2FindBox.NeedLoadParentDataGroup = true;
			this.InvoiceTariff2FindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.InvoiceTariff2FindBox.ParentType = null;
			this.InvoiceTariff2FindBox.SelectNomenclatureModes = null;
			this.InvoiceTariff2FindBox.ShowDescriptionBox = false;
			this.InvoiceTariff2FindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.InvoiceTariff2FindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.InvoiceTariff2FindBox.TabIndex = 3;
			this.InvoiceTariff2FindBox.TariffType = null;
			// 
			// InvoiceTariff1FindBox
			// 
			this.InvoiceTariff1FindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceTariff1FindBox, "GIL_Tariff1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(null)).GIL_Tariff1)));
			this.InvoiceTariff1FindBox.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("c2a47843-7d67-4d56-950c-2919615fc310", "Tariff 1");
			this.InvoiceTariff1FindBox.ErrorForUnsupportedCountry = null;
			this.InvoiceTariff1FindBox.GetDataGrouping = null;
			this.InvoiceTariff1FindBox.GetEffectiveDate = null;
			this.InvoiceTariff1FindBox.GetTariffType = null;
			this.InvoiceTariff1FindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 76, true);
			this.InvoiceTariff1FindBox.Name = "InvoiceTariff1FindBox";
			this.InvoiceTariff1FindBox.NeedLoadNomenclatureWhenTariffNotFound = false;
			this.InvoiceTariff1FindBox.NeedLoadParentDataGroup = true;
			this.InvoiceTariff1FindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.InvoiceTariff1FindBox.ParentType = null;
			this.InvoiceTariff1FindBox.SelectNomenclatureModes = null;
			this.InvoiceTariff1FindBox.ShowDescriptionBox = false;
			this.InvoiceTariff1FindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.InvoiceTariff1FindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.InvoiceTariff1FindBox.TabIndex = 2;
			this.InvoiceTariff1FindBox.TariffType = null;
			// 
			// InvoiceProductCodeFindBox
			// 
			this.InvoiceProductCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceProductCodeFindBox, "GIL_Product");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(null)).GIL_Product)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(null)).Lookups.ProductCodeList)));
			this.InvoiceProductCodeFindBox.BindToList = "Lookups.ProductCodeList";
			this.InvoiceProductCodeFindBox.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("356923ad-1406-410a-aa32-d5d4e7ed8d88", "Product Code");
			this.InvoiceProductCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 55, true);
			this.InvoiceProductCodeFindBox.Name = "InvoiceProductCodeFindBox";
			this.InvoiceProductCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.InvoiceProductCodeFindBox.ParentType = null;
			this.InvoiceProductCodeFindBox.PreBoundMaxLength = 35;
			this.InvoiceProductCodeFindBox.ShowDescriptionBox = false;
			this.InvoiceProductCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 17, true);
			this.InvoiceProductCodeFindBox.TabIndex = 1;
			// 
			// GlobalCommercialInvoiceLineDetailUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.InvoiceLineGroupBox);
			this.Name = "GlobalCommercialInvoiceLineDetailUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 541, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InvoiceLineGroupBox.ResumeLayout(false);
			this.InvoiceLineGroupBox.PerformLayout();
			this.InvoiceNumberGuidDropEdit.ResumeLayout(true);
			this.InvoiceNumberGuidDropEdit.PerformLayout();
			this.InvoiceNetWeightCalcDropEdit.ResumeLayout(true);
			this.InvoiceNetWeightCalcDropEdit.PerformLayout();
			this.InvoiceGrossWeightCalcDropEdit.ResumeLayout(true);
			this.InvoiceGrossWeightCalcDropEdit.PerformLayout();
			this.InvoiceVolumeCalcDropEdit.ResumeLayout(true);
			this.InvoiceVolumeCalcDropEdit.PerformLayout();
			this.InvoiceQuantityCalcDropEdit.ResumeLayout(true);
			this.InvoiceQuantityCalcDropEdit.PerformLayout();
			this.InvoiceTariff2FindBox.ResumeLayout(true);
			this.InvoiceTariff2FindBox.PerformLayout();
			this.InvoiceTariff1FindBox.ResumeLayout(true);
			this.InvoiceTariff1FindBox.PerformLayout();
			this.InvoiceProductCodeFindBox.ResumeLayout(true);
			this.InvoiceProductCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox InvoiceLineGroupBox;
		private ZArchitecture.GUI.ZCodeFindBox InvoiceProductCodeFindBox;
		private Customs.Universal.GUI.TariffFindBox InvoiceTariff2FindBox;
		private Customs.Universal.GUI.TariffFindBox InvoiceTariff1FindBox;
		private ZArchitecture.ZTextBox InvoiceGoodsDescriptionTextBox;
		private ZArchitecture.ZCalcEdit InvoiceLinePriceCalcEdit;
		private ZArchitecture.GUI.ZCalcDropEdit InvoiceNetWeightCalcDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit InvoiceGrossWeightCalcDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit InvoiceVolumeCalcDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit InvoiceQuantityCalcDropEdit;
		private ZArchitecture.ZTextBox InvoiceLinePriceCurrencyTextBox;
		private ZArchitecture.GUI.ZGuidDropEdit InvoiceNumberGuidDropEdit;
	}
}

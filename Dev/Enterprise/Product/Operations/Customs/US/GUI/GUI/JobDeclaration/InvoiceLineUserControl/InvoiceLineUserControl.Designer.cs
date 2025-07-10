namespace Enterprise.Customs.US.GUI
{
	partial class USInvoiceLineUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.TariffCodeFindBox = new Enterprise.Customs.Common.GUI.TariffFindBox();
			this.NetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsValue_ConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.InvoiceLinesSummaryGroupBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.LineDetailTabControl.SuspendLayout();
			this.InvoiceDetailsGroupBox.SuspendLayout();
			this.ClassificationDetailsGroupBox.SuspendLayout();
			this.CurrentInvoicePanel.SuspendLayout();
			this.LineSummaryPanel.SuspendLayout();
			this.ContainersTabPage.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).BeginInit();
			this.LineDetailsTabPage.SuspendLayout();
			this.ClassificationPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// LineDetailTabControl
			// 
			this.LineDetailTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			// 
			// InvoiceDetailsGroupBox
			// 
			this.InvoiceDetailsGroupBox.Controls.Add(this.NetWeightCalcDropEdit);
			this.InvoiceDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 192, true);
			this.InvoiceDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 96, true);
			this.InvoiceDetailsGroupBox.TabIndex = 1;
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_RH_NKCommodity_CodeBoundFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.NetWeightCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_CountryOfOriginBoundFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.VolumeCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_WeightCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_LinePriceBoundCurrencyControl, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_DescriptionBoundTextBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.InvoiceQuantityCalcDropEdit, 0);
			// 
			// JI_CountryOfOriginBoundFindBox
			// 
			this.JI_CountryOfOriginBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(303, 41, true);
			// 
			// ClassificationDetailsGroupBox
			// 
			this.ClassificationDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ClassificationDetailsGroupBox.Controls.Add(this.TariffCodeFindBox);
			this.ClassificationDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.None;
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CustomsQuantityCalcDropEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.TariffCodeFindBox, 0);
			// 
			// LineChargesTabPage
			// 
			this.LineChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			// 
			// LineSummaryPanel
			// 
			this.LineSummaryPanel.Controls.Add(this.CustomsValue_ConvertToLocalCurrencyControl);
			this.LineSummaryPanel.Controls.SetChildIndex(this.oLabel8, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_DutyConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.CustomsValue_ConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_InsuranceConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_FreightConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_GSTConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_CIFConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_FOBConvertToLocalCurrencyControl, 0);
			// 
			// CustomsInvoiceLinesBoundGrid
			// 
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("USACSImportInvoiceLineUserControl|5c8c0774-bf4e-4672-a172-adaf830b71dc", "Cotton Fee Ex?", "Cotton Fee Exempt", "");
			zDropEditColumnStyleInfo1.ColumnName = "US_CottonFeeExempt";
			zDropEditColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("cc7e050c-f3b8-45c2-ac18-7b4f8d101ea2", "Export Date");
			zDateEditColumnStyleInfo1.ColumnName = "US_DateOfExport";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			// 
			// JI_Calc_CIFConvertToLocalCurrencyControl
			// 
			this.JI_Calc_CIFConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 156, true);
			this.JI_Calc_CIFConvertToLocalCurrencyControl.TabIndex = 13;
			// 
			// JI_Calc_InsuranceConvertToLocalCurrencyControl
			// 
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.InsuranceInLocalCurrency";
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Guid;
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 90, true);
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.TabIndex = 9;
			// 
			// JI_Calc_FreightConvertToLocalCurrencyControl
			// 
			this.JI_Calc_FreightConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.FreightInLocalCurrency";
			this.JI_Calc_FreightConvertToLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
			this.JI_Calc_FreightConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Guid;
			this.JI_Calc_FreightConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 67, true);
			this.JI_Calc_FreightConvertToLocalCurrencyControl.TabIndex = 7;
			// 
			// JI_Calc_FOBConvertToLocalCurrencyControl
			// 
			this.JI_Calc_FOBConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.FOBValueInLocalCurrency";
			this.JI_Calc_FOBConvertToLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
			this.JI_Calc_FOBConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Guid;
			this.JI_Calc_FOBConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 21, true);
			// 
			// JI_Calc_GSTConvertToLocalCurrencyControl
			// 
			this.JI_Calc_GSTConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 173, true);
			this.JI_Calc_GSTConvertToLocalCurrencyControl.TabIndex = 15;
			// 
			// JI_Calc_DutyConvertToLocalCurrencyControl
			// 
			this.JI_Calc_DutyConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 113, true);
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(303, 64, true);
			this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.VolumeCalcDropEdit.TabIndex = 11;
			// 
			// JI_WeightCalcDropEdit
			// 
			this.JI_WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(553, 40, true);
			this.JI_WeightCalcDropEdit.TabIndex = 7;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.JobDeclaration);
			// 
			// TariffCodeFindBox
			// 
			this.TariffCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffCodeFindBox, "FilteredInvoiceLines.JI_FormattedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_FormattedTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.Tariffs)));
			this.TariffCodeFindBox.BindToList = "FilteredInvoiceLines.Lookups+Tariffs";
			this.TariffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 16, true);
			this.TariffCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Tariff;
			this.TariffCodeFindBox.Name = "TariffCodeFindBox";
			this.TariffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 20, true);
			this.TariffCodeFindBox.TabIndex = 1;
			// 
			// NetWeightCalcDropEdit
			// 
			this.NetWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NetWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_NetWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.WeightUQList)));
			this.NetWeightCalcDropEdit.BindToAmount = "FilteredInvoiceLines.JI_NetWeight";
			this.NetWeightCalcDropEdit.BindToList = "FilteredInvoiceLines.Lookups+WeightUQList";
			this.NetWeightCalcDropEdit.BindToUnit = "FilteredInvoiceLines.JI_NetWeightUQ";
			this.NetWeightCalcDropEdit.Decimals = 2;
			this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(553, 64, true);
			this.NetWeightCalcDropEdit.Name = "NetWeightCalcDropEdit";
			this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.NetWeightCalcDropEdit.TabIndex = 13;
			this.NetWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// CustomsValue_ConvertToLocalCurrencyControl
			// 
			this.CustomsValue_ConvertToLocalCurrencyControl.AllowDrop = true;
			this.CustomsValue_ConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_CustomsValue";
			this.CustomsValue_ConvertToLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
			this.CustomsValue_ConvertToLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
			this.CustomsValue_ConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("USInvoiceLineUserControl|52b84437-78b3-4c9a-93fe-c533aff1ea10", "Customs Value");
			this.CustomsValue_ConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 44, true);
			this.CustomsValue_ConvertToLocalCurrencyControl.Name = "CustomsValue_ConvertToLocalCurrencyControl";
			this.CustomsValue_ConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.CustomsValue_ConvertToLocalCurrencyControl.TabIndex = 5;
			// 
			// USInvoiceLineUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "USInvoiceLineUserControl";
			this.InvoiceLinesSummaryGroupBox.ResumeLayout(false);
			this.BottomPanel.ResumeLayout(false);
			this.TopPanel.ResumeLayout(false);
			this.LineDetailTabControl.ResumeLayout(false);
			this.InvoiceDetailsGroupBox.ResumeLayout(false);
			this.ClassificationDetailsGroupBox.ResumeLayout(false);
			this.CurrentInvoicePanel.ResumeLayout(false);
			this.LineSummaryPanel.ResumeLayout(false);
			this.ContainersTabPage.ResumeLayout(false);
			this.ContainersGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).EndInit();
			this.LineDetailsTabPage.ResumeLayout(false);
			this.ClassificationPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		protected Enterprise.Customs.Common.GUI.TariffFindBox TariffCodeFindBox;
		protected Enterprise.ZArchitecture.GUI.ZCalcDropEdit NetWeightCalcDropEdit;
		private Enterprise.Customs.GUI.ConvertToLocalCurrencyControl CustomsValue_ConvertToLocalCurrencyControl;
	}
}

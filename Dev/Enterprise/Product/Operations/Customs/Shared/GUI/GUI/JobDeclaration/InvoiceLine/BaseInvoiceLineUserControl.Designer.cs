using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GUI
{
	partial class BaseInvoiceLineUserControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		public Enterprise.ZArchitecture.GUI.ZGroupBox InvoiceLinesSummaryGroupBox;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
		public Enterprise.ZArchitecture.GUI.ZTemplateTabControl LineDetailTabControl;
		public Enterprise.ZArchitecture.GUI.ZGroupBox InvoiceDetailsGroupBox;
		public Enterprise.ZArchitecture.GUI.ZCodeFindBox JI_CountryOfOriginBoundFindBox;
		public Enterprise.ZArchitecture.GUI.ZCodeFindBox JI_RH_NKCommodity_CodeBoundFindBox;
		public Enterprise.ZArchitecture.GUI.ZCalcFindBox JI_LinePriceBoundCurrencyControl;
		public Enterprise.ZArchitecture.GUI.ZGroupBox ClassificationDetailsGroupBox;
		public Enterprise.ZArchitecture.GUI.ZTabPage LineChargesTabPage;
		public Enterprise.ZArchitecture.GUI.ZTabPage LineComponentsTabPage;
		public Enterprise.ZArchitecture.GUI.ZPanel CurrentInvoicePanel;
		public Enterprise.ZArchitecture.GUI.ZPanel LineSummaryPanel;
		public Enterprise.ZArchitecture.ZLabel PendingApportionmentLabel;
		public Enterprise.ZArchitecture.GUI.ZTabPage ContainersTabPage;
		public ZArchitecture.ZCalcEdit BondedWHSOrderLineNumberCalcEdit;
		public ZArchitecture.ZTextBox BondedWHSOrderNumberTextBox;
		protected internal Enterprise.ZArchitecture.GUI.ZGroupBox ContainersGroupBox;
		protected internal Enterprise.ZArchitecture.ZGrid CusContainerInvoiceLineGrid;
		protected internal Enterprise.ZArchitecture.ZLabel CantCreateInvoiceLinesLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZTabPage LineDetailsTabPage;
		protected ZTabPage NewLineDetailsTabPage;
		protected InvoiceLineDetailsUserControl InvoiceLineDetailsUserControl;
		public Enterprise.ZArchitecture.GUI.ZPanel ClassificationPanel;
		public ZGrid CustomsInvoiceLinesBoundGrid;
		private IContainer components;
		protected internal ConvertToLocalCurrencyControl JI_Calc_CIFConvertToLocalCurrencyControl;
		protected internal ConvertToLocalCurrencyControl JI_Calc_InsuranceConvertToLocalCurrencyControl;
		protected internal ConvertToLocalCurrencyControl JI_Calc_FreightConvertToLocalCurrencyControl;
		protected internal ConvertToLocalCurrencyControl JI_Calc_FOBConvertToLocalCurrencyControl;
		public ConvertToLocalCurrencyControl JI_Calc_GSTConvertToLocalCurrencyControl;
		public ConvertToLocalCurrencyControl JI_Calc_DutyConvertToLocalCurrencyControl;
		protected internal ConvertToLocalCurrencyControl JI_Calc_BalanceConvertToLocalCurrencyControl;
		protected internal ConvertToLocalCurrencyControl JI_Calc_LinesEnteredConvertToLocalCurrencyControl;
		protected internal ConvertToLocalCurrencyControl JI_Calc_LinesTotalConvertToLocalCurrencyControl;
		public ZCalcDropEdit CustomsQuantityCalcDropEdit;
		public ZCalcDropEdit VolumeCalcDropEdit;
		public ZCalcDropEdit JI_WeightCalcDropEdit;
		public ZCalcDropEdit InvoiceQuantityCalcDropEdit;
		public LongTextControl JI_DescriptionBoundTextBox;
		protected internal Enterprise.ZArchitecture.ZLabel oLabel6;
		protected internal Enterprise.ZArchitecture.ZLabel oLabel8;
		protected CargoWise.Windows.UI.KSplitter Splitter;
		ZFilterStripBaseControl StripControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage CustomFieldsTabPage;
		InvoiceLineCustomFieldsControl CustomFieldsControl;
		InvoiceLineComponentsUserControl InvoiceLineComponentsControl;

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.Splitter = new CargoWise.Windows.UI.KSplitter();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ComplianceRisk.GUI.CommodityRiskStatusColumnStyleInfo zRiskStatusColumnStyleInfo = new Enterprise.ComplianceRisk.GUI.CommodityRiskStatusColumnStyleInfo();
			Enterprise.ComplianceRisk.GUI.LegalBooksColumnStyleInfo zComplianceAlerts = new Enterprise.ComplianceRisk.GUI.LegalBooksColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			c2PivotContainerWeightColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			c2PivotContainerGrossWeightColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			c2PivotContainerNetWeightColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			c2PivotContainerPackQtyColumnStyleInfo12 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			c2PivotContainerSplitValueColumnStyleInfo13 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			c2ContainerSplitCurrencyColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();

			this.InvoiceLinesSummaryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LineSummaryPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.JI_Calc_CIFConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.JI_Calc_FreightConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.JI_Calc_FOBConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.JI_Calc_GSTConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.JI_Calc_DutyConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.oLabel8 = new Enterprise.ZArchitecture.ZLabel();
			this.CurrentInvoicePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.JI_Calc_BalanceConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.oLabel6 = new Enterprise.ZArchitecture.ZLabel();
			this.PendingApportionmentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CustomsInvoiceLinesBoundGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LineDetailTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.LineDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.NewLineDetailsTabPage = new ZTabPage();
			this.InvoiceLineDetailsUserControl = new InvoiceLineDetailsUserControl();
			this.ClassificationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ClassificationDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomsQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.InvoiceDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JI_RH_NKCommodity_CodeBoundFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.VolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.JI_CountryOfOriginBoundFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JI_WeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.InvoiceQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.JI_DescriptionBoundTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.JI_LinePriceBoundCurrencyControl = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.ContainersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ContainersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CusContainerInvoiceLineGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CantCreateInvoiceLinesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BondedWHSOrderLineNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BondedWHSOrderNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StripControl = new ZFilterStripBaseControl(CustomsInvoiceLinesBoundGrid, FilterBusinessObject);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InvoiceLinesSummaryGroupBox.SuspendLayout();
			this.LineSummaryPanel.SuspendLayout();
			this.BondedWHSOrderLineNumberCalcEdit.SuspendLayout();
			this.BondedWHSOrderNumberTextBox.SuspendLayout();
			this.JI_Calc_CIFConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_FreightConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_FOBConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_GSTConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_DutyConvertToLocalCurrencyControl.SuspendLayout();
			this.CurrentInvoicePanel.SuspendLayout();
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.SuspendLayout();
			this.TopPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).BeginInit();
			this.CustomsInvoiceLinesBoundGrid.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.LineDetailTabControl.SuspendLayout();
			this.LineDetailsTabPage.SuspendLayout();
			this.NewLineDetailsTabPage.SuspendLayout();
			this.ClassificationPanel.SuspendLayout();
			this.ClassificationDetailsGroupBox.SuspendLayout();
			this.CustomsQuantityCalcDropEdit.SuspendLayout();
			this.InvoiceDetailsGroupBox.SuspendLayout();
			this.JI_RH_NKCommodity_CodeBoundFindBox.SuspendLayout();
			this.VolumeCalcDropEdit.SuspendLayout();
			this.JI_CountryOfOriginBoundFindBox.SuspendLayout();
			this.JI_WeightCalcDropEdit.SuspendLayout();
			this.InvoiceQuantityCalcDropEdit.SuspendLayout();
			this.JI_DescriptionBoundTextBox.SuspendLayout();
			this.JI_LinePriceBoundCurrencyControl.SuspendLayout();
			this.ContainersTabPage.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).BeginInit();
			this.CusContainerInvoiceLineGrid.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.IInvoicesProvider);
			//
			// InvoiceLinesSummaryGroupBox
			//
			this.InvoiceLinesSummaryGroupBox.Controls.Add(this.LineSummaryPanel);
			this.InvoiceLinesSummaryGroupBox.Controls.Add(this.CurrentInvoicePanel);
			this.InvoiceLinesSummaryGroupBox.Controls.Add(this.PendingApportionmentLabel);
			this.InvoiceLinesSummaryGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.InvoiceLinesSummaryGroupBox, false);
			this.InvoiceLinesSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 0, true);
			this.InvoiceLinesSummaryGroupBox.Name = "InvoiceLinesSummaryGroupBox";
			this.InvoiceLinesSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 320, true);
			this.InvoiceLinesSummaryGroupBox.TabIndex = 1;
			this.InvoiceLinesSummaryGroupBox.TabStop = false;
			//
			// LineSummaryPanel
			//
			this.LineSummaryPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.LineSummaryPanel.Controls.Add(this.JI_Calc_CIFConvertToLocalCurrencyControl);
			this.LineSummaryPanel.Controls.Add(this.JI_Calc_InsuranceConvertToLocalCurrencyControl);
			this.LineSummaryPanel.Controls.Add(this.JI_Calc_FreightConvertToLocalCurrencyControl);
			this.LineSummaryPanel.Controls.Add(this.JI_Calc_FOBConvertToLocalCurrencyControl);
			this.LineSummaryPanel.Controls.Add(this.JI_Calc_GSTConvertToLocalCurrencyControl);
			this.LineSummaryPanel.Controls.Add(this.JI_Calc_DutyConvertToLocalCurrencyControl);
			this.LineSummaryPanel.Controls.Add(this.oLabel8);
			this.LineSummaryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 101, true);
			this.LineSummaryPanel.Name = "LineSummaryPanel";
			this.LineSummaryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 212, true);
			this.LineSummaryPanel.TabIndex = 1;
			//
			// JI_Calc_CIFConvertToLocalCurrencyControl
			//
			this.JI_Calc_CIFConvertToLocalCurrencyControl.AllowDrop = true;
			this.JI_Calc_CIFConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_CIF";
			this.JI_Calc_CIFConvertToLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
			this.JI_Calc_CIFConvertToLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_NKLinePriceCurr";
			this.JI_Calc_CIFConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|9a7c44b5-b81e-4a69-b1ff-27a66431100d", "CIF", "CIF Value", "CIF value for current line item.");
			this.JI_Calc_CIFConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.JI_Calc_CIFConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 85, true);
			this.JI_Calc_CIFConvertToLocalCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.JI_Calc_CIFConvertToLocalCurrencyControl.Name = "JI_Calc_CIFConvertToLocalCurrencyControl";
			this.JI_Calc_CIFConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.JI_Calc_CIFConvertToLocalCurrencyControl.TabIndex = 9;
			//
			// JI_Calc_InsuranceConvertToLocalCurrencyControl
			//
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.AllowDrop = true;
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_InsuranceInInvoiceCurr";
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_NKLinePriceCurr";
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|9c49754a-69df-439b-b746-2caabc961e88", "Insurance", "Insurance Amount", "Insurance charges for current line item.");
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 63, true);
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.Name = "JI_Calc_InsuranceConvertToLocalCurrencyControl";
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.TabIndex = 7;
			//
			// JI_Calc_FreightConvertToLocalCurrencyControl
			//
			this.JI_Calc_FreightConvertToLocalCurrencyControl.AllowDrop = true;
			this.JI_Calc_FreightConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_FreightInInvoiceCurr";
			this.JI_Calc_FreightConvertToLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
			this.JI_Calc_FreightConvertToLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_NKLinePriceCurr";
			this.JI_Calc_FreightConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|df6cbd06-1b7d-4fba-a848-a9045e84277f", "Freight", "Freight Charges for current line item.");
			this.JI_Calc_FreightConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.JI_Calc_FreightConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 42, true);
			this.JI_Calc_FreightConvertToLocalCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.JI_Calc_FreightConvertToLocalCurrencyControl.Name = "JI_Calc_FreightConvertToLocalCurrencyControl";
			this.JI_Calc_FreightConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.JI_Calc_FreightConvertToLocalCurrencyControl.TabIndex = 5;
			//
			// JI_Calc_FOBConvertToLocalCurrencyControl
			//
			this.JI_Calc_FOBConvertToLocalCurrencyControl.AllowDrop = true;
			this.JI_Calc_FOBConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_FOB";
			this.JI_Calc_FOBConvertToLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
			this.JI_Calc_FOBConvertToLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_NKLinePriceCurr";
			this.JI_Calc_FOBConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|cbff40a7-668f-406e-888c-56abbdfbbb6b", "FOB Value", "FOB value for current line item.");
			this.JI_Calc_FOBConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.JI_Calc_FOBConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 21, true);
			this.JI_Calc_FOBConvertToLocalCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.JI_Calc_FOBConvertToLocalCurrencyControl.Name = "JI_Calc_FOBConvertToLocalCurrencyControl";
			this.JI_Calc_FOBConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.JI_Calc_FOBConvertToLocalCurrencyControl.TabIndex = 3;
			//
			// JI_Calc_GSTConvertToLocalCurrencyControl
			//
			this.JI_Calc_GSTConvertToLocalCurrencyControl.AllowDrop = true;
			this.JI_Calc_GSTConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_GSTVATAmountIncludingWHEstimate";
			this.JI_Calc_GSTConvertToLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
			this.JI_Calc_GSTConvertToLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
			this.JI_Calc_GSTConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|7A6F45EE - E38C - 4BF2 - 9742 - 476137622AC6", "{0} Amount", "{0} value for current line item");
			this.JI_Calc_GSTConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 127, true);
			this.JI_Calc_GSTConvertToLocalCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.JI_Calc_GSTConvertToLocalCurrencyControl.Name = "JI_Calc_GSTConvertToLocalCurrencyControl";
			this.JI_Calc_GSTConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.JI_Calc_GSTConvertToLocalCurrencyControl.TabIndex = 13;
			//
			// JI_Calc_DutyConvertToLocalCurrencyControl
			//
			this.JI_Calc_DutyConvertToLocalCurrencyControl.AllowDrop = true;
			this.JI_Calc_DutyConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_DutyAmountIncludingWHEstimate";
			this.JI_Calc_DutyConvertToLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
			this.JI_Calc_DutyConvertToLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
			this.JI_Calc_DutyConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|cbe9d0e2-2952-498b-8317-0bf8824d157a", "Duty", "Duty value for current line item.");
			this.JI_Calc_DutyConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 106, true);
			this.JI_Calc_DutyConvertToLocalCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.JI_Calc_DutyConvertToLocalCurrencyControl.Name = "JI_Calc_DutyConvertToLocalCurrencyControl";
			this.JI_Calc_DutyConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.JI_Calc_DutyConvertToLocalCurrencyControl.TabIndex = 11;
			//
			// oLabel8
			//
			this.oLabel8.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|bf705023-0188-4774-8c44-89dddc904e76", "Line Calculations");
			this.oLabel8.IsFontBold = true;
			this.oLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 3, true);
			this.oLabel8.Name = "oLabel8";
			this.oLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 12, true);
			this.oLabel8.TabIndex = 1;
			//
			// CurrentInvoicePanel
			//
			this.CurrentInvoicePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.CurrentInvoicePanel.Controls.Add(this.JI_Calc_BalanceConvertToLocalCurrencyControl);
			this.CurrentInvoicePanel.Controls.Add(this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl);
			this.CurrentInvoicePanel.Controls.Add(this.JI_Calc_LinesTotalConvertToLocalCurrencyControl);
			this.CurrentInvoicePanel.Controls.Add(this.oLabel6);
			this.CurrentInvoicePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.CurrentInvoicePanel.Name = "CurrentInvoicePanel";
			this.CurrentInvoicePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 83, true);
			this.CurrentInvoicePanel.TabIndex = 0;
			//
			// JI_Calc_BalanceConvertToLocalCurrencyControl
			//
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.AllowDrop = true;
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_Balance";
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_NKLinePriceCurr";
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|f3830763-aca2-48c0-9b29-dd6503465610", "Balance", "Remaining balance for entered lines on current invoice.");
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 56, true);
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.Name = "JI_Calc_BalanceConvertToLocalCurrencyControl";
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.TabIndex = 6;
			//
			// JI_Calc_LinesEnteredConvertToLocalCurrencyControl
			//
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.AllowDrop = true;
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_LinesEntered";
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_NKLinePriceCurr";
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|07cf5b31-ad00-419b-b256-6e4061fe52bb", "Lines Entered", "The total amount of lines entered so far.");
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 35, true);
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.Name = "JI_Calc_LinesEnteredConvertToLocalCurrencyControl";
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.TabIndex = 4;
			//
			// JI_Calc_LinesTotalConvertToLocalCurrencyControl
			//
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.AllowDrop = true;
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_LinesTotal";
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_NKLinePriceCurr";
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|f273fae5-11fa-40bc-80e1-c93b0fa9cdbf", "Expected", "Total Expected", "The expected line total amount.");
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 14, true);
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.Name = "JI_Calc_LinesTotalConvertToLocalCurrencyControl";
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.TabIndex = 2;
			//
			// oLabel6
			//
			this.oLabel6.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|8d810df2-fd43-465c-b99b-f180213081ca", "Current Invoice");
			this.oLabel6.IsFontBold = true;
			this.oLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			this.oLabel6.Name = "oLabel6";
			this.oLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 12, true);
			this.oLabel6.TabIndex = 0;
			this.oLabel6.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			//
			// PendingApportionmentLabel
			//
			this.PendingApportionmentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PendingApportionmentLabel.IsFontBold = true;
			this.PendingApportionmentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 14, true);
			this.PendingApportionmentLabel.Name = "PendingApportionmentLabel";
			this.PendingApportionmentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 299, true);
			this.PendingApportionmentLabel.TabIndex = 0;
			this.PendingApportionmentLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|C03CCEC9-431F-4CDA-80AF-71E9A0E2AD54",
				"Apportionment of Charges is pending. Selecting SAVE or alternatively selecting Brokerage -> Perform apportionment will run this feature. The system calculated apportionment is indicated within the 'Charges' tab once either option runs.");
			this.PendingApportionmentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			//
			// TopPanel
			//
			this.TopPanel.Controls.Add(this.CustomsInvoiceLinesBoundGrid);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 584, true);
			this.TopPanel.TabIndex = 0;
			//
			// CustomsInvoiceLinesBoundGrid
			//
			this.CustomsInvoiceLinesBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CustomsInvoiceLinesBoundGrid, "FilteredInvoiceLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Calc_Invoice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.SortedInvoiceList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PartNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.PartsList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.ClassificationList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_InvoiceQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_InvoiceUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.InvoiceUQList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsUnitQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.CustomsUQList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_LinePrice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CountryOfOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.CountryList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_RH_NKCommodity_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.Commodity_Codes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.WeightUQList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_NetWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.WeightUQList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_VolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.VolumeUQList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_OrderNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Calc_OrderLineNumberAndSubLine)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).UnitPrice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomAttrib1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomAttrib2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomAttrib3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomAttrib4)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomAttrib5)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomAttrib6)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomTextBlob1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_MatchingKey)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ClassUsageComment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_GS_NKClassUsageCommentReviewer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_IsClassUsageCommentRead)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.ContainerModeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).LinkedModuleCommodityRiskStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).LegalBookLink)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).LinkedModuleCommodityImportAlertDescription)));
			this.CustomsInvoiceLinesBoundGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JI_LineNo";
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.ToolTip = "Line Number";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zDropEditColumnStyleInfo1.BindToList = "Lookups.SortedInvoiceList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|6877319b-df5f-41f6-a445-6419eddd2e56", "Inv. No.", "Invoice Number", "Inv. # for Line", "Header Invoice No. related to this Invoice line.");
			zDropEditColumnStyleInfo1.ColumnName = "JI_Calc_Invoice";
			zDropEditColumnStyleInfo1.ToolTip = "Invoice this line relates to";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups.PartsList";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JI_PartNo";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.SupplierPart;
			zCodeFindBoxColumnStyleInfo1.ToolTip = "Product";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups.ClassificationList";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|35db7c43-4cb2-48ba-8a63-7d20ed53cf28", "Classification Lookup");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JI_CC";
			zGuidFindBoxColumnStyleInfo1.ToolTip = "Enter the lookup code for the line\'s classification lookup details here";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(121);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "JI_InvoiceQuantity";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|4a246416-f0cf-4aef-9a72-4ad43c21d7b5", "Invoice Quantity");
			zCalcEditColumnStyleInfo2.ToolTip = "Invoice line quantity";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo2.BindToList = "Lookups.InvoiceUQList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|a35099bc-42ef-454c-b6d1-acd44ac24a7e", "UQ");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "JI_InvoiceUQ";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|4a246416-f0cf-4aef-9a72-4ad43c21d7b5", "Invoice Quantity");
			zDropEditColumnStyleInfo2.ToolTip = "Invoice line unit of quantity";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "JI_CustomsQuantity";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|61a13238-8849-4304-9174-f4bf1a73073a", "Customs Quantity");
			zCalcEditColumnStyleInfo3.ToolTip = "Customs quantity for invoice line if different from invoice quantity";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.BindToList = "Lookups.CustomsUQList";
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|ffd0d82f-c967-4a3d-bb45-133bc1389490", "UQ");
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "JI_CustomsUnitQty";
			zDropEditColumnStyleInfo3.GroupName = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|61a13238-8849-4304-9174-f4bf1a73073a", "Customs Quantity");
			zDropEditColumnStyleInfo3.ToolTip = "Customs unit of quantity";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "JI_LinePrice";
			zCalcEditColumnStyleInfo4.ToolTip = "Invoice line price";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "JI_Description";
			zTextBoxColumnStyleInfo1.ToolTip = "Invoice line description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCodeFindBoxColumnStyleInfo2.BindToList = "Lookups.CountryList";
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|0C4C8B1C-4CF7-4D7C-A0E9-0B94C813FFE1", "ORG", "Goods Origin", "Country/Region of Origin of the goods");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "JI_CountryOfOrigin";
			zCodeFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			zCodeFindBoxColumnStyleInfo2.ToolTip = "Country of Origin of the goods being moved";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78);
			zCodeFindBoxColumnStyleInfo3.BindToList = "Lookups.Commodity_Codes";
			zCodeFindBoxColumnStyleInfo3.ColumnName = "JI_RH_NKCommodity_Code";
			zCodeFindBoxColumnStyleInfo3.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCommodityCode;
			zCodeFindBoxColumnStyleInfo3.ToolTip = "Commodity Code";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "JI_Weight";
			zCalcEditColumnStyleInfo5.GroupName = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|ef799b94-e141-40c8-aa9b-f2981531f701", "Gross Weight");
			zCalcEditColumnStyleInfo5.ToolTip = "Invoice line weight";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo4.BindToList = "Lookups.WeightUQList";
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|3b0ac72e-0082-4dc8-85c1-51f17fcf3d5f", "UQ");
			zDropEditColumnStyleInfo4.ColumnName = "JI_WeightUQ";
			zDropEditColumnStyleInfo4.GroupName = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|ef799b94-e141-40c8-aa9b-f2981531f701", "Gross Weight");
			zDropEditColumnStyleInfo4.ToolTip = "Invoice line weight unit of quantity";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "JI_NetWeight";
			zCalcEditColumnStyleInfo6.GroupName = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|5c82bef7-276f-4173-8f37-0852c4db879b", "Net Weight");
			zCalcEditColumnStyleInfo6.ToolTip = "Net Weight for Invoice Line";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(81);
			zDropEditColumnStyleInfo5.BindToList = "Lookups.WeightUQList";
			zDropEditColumnStyleInfo5.ColumnName = "JI_NetWeightUQ";
			zDropEditColumnStyleInfo5.GroupName = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|5c82bef7-276f-4173-8f37-0852c4db879b", "Net Weight");
			zDropEditColumnStyleInfo5.ToolTip = "Net Weight Unit for Invoice Line";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "JI_Volume";
			zCalcEditColumnStyleInfo7.GroupName = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|c1ffe0c0-6eb2-4794-abab-bcb50ab57390", "Volume");
			zCalcEditColumnStyleInfo7.ToolTip = "Volume of goods for invoice line";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo6.BindToList = "Lookups.VolumeUQList";
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|da857915-7189-4d11-99b8-d4caaa3c2fe7", "UQ");
			zDropEditColumnStyleInfo6.ColumnName = "JI_VolumeUQ";
			zDropEditColumnStyleInfo6.GroupName = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|c1ffe0c0-6eb2-4794-abab-bcb50ab57390", "Volume");
			zDropEditColumnStyleInfo6.ToolTip = "Invoice Line Unit Of Volume";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo2.ColumnName = "JI_OrderNumber";
			zTextBoxColumnStyleInfo2.ToolTip = "Invoice line order number";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|7a32cf6a-4053-418d-99ac-4f6ac724ea6c", "Order Line No.");
			zTextBoxColumnStyleInfo3.ColumnName = "JI_Calc_OrderLineNumberAndSubLine";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(94);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|cec4f9bb-b264-4d76-96c9-5821eba5d72d", "Unit Price");
			zCalcEditColumnStyleInfo8.ColumnName = "UnitPrice";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "JI_CustomAttrib1";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "JI_CustomAttrib2";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "JI_CustomAttrib3";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "JI_CustomAttrib4";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "JI_CustomAttrib5";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.ColumnName = "JI_CustomAttrib6";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo1.ColumnName = "JI_CustomTextBlob1";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo7.BindToList = "Lookups.ContainerModeList";
			zDropEditColumnStyleInfo7.ColumnName = "JI_ContainerMode";
			zDropEditColumnStyleInfo7.IsVisible = false;
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(99);
			zCodeFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|9537D75A-5F19-46A8-8500-E442CE50DD58", "Line Currency");
			zCodeFindBoxColumnStyleInfo4.ColumnName = "JI_RX_NKLinePriceCurr";
			zCodeFindBoxColumnStyleInfo4.BindToList = "Lookups+CurrencyList";
			zCodeFindBoxColumnStyleInfo4.IsVisible = false;
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("3d3450af-761f-4785-86a7-c84d31d82176", "Inv. Seq #", "Invoice Seq #", "Invoice Sequence Number");
			zTextBoxColumnStyleInfo13.ColumnName = "InvoiceHeader+JZ_InvoiceDisplaySequence";
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("6008109f-43bf-4222-8912-1a6d68a28c0d", "Matching Key");
			zTextBoxColumnStyleInfo14.ColumnName = "JI_MatchingKey";
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo14.CharacterCasing = CharacterCasing.Upper;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiLineTextBoxColumnInfo2.ColumnName = "JI_ClassUsageComment";
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo2.GroupName = Enterprise.Customs.GUI.Res.GetData("1a82b32a-8585-421e-bcff-f638e213c96b", "Usage Comment");
			zMultiLineTextBoxColumnInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("1a82b32a-8585-421e-bcff-f638e213c96b", "Usage Comment");
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zMultiLineTextBoxColumnInfo2.IsVisible = false;
			zMultiLineTextBoxColumnInfo2.IsMandatory = false;
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("C7BFB723-B872-4308-A7D0-516558540406", "Usage Comment Reviewer");
			zTextBoxColumnStyleInfo15.ColumnName = "JI_GS_NKClassUsageCommentReviewer";
			zTextBoxColumnStyleInfo15.IsVisible = false;
			zTextBoxColumnStyleInfo15.IsMandatory = false;
			zTextBoxColumnStyleInfo15.CharacterCasing = CharacterCasing.Upper;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("cf232dc0-e57f-435f-af2d-e3bba1ebb3dc", "Is Usage Comment Read?");
			zCheckBoxColumnStyleInfo2.ColumnName = "JI_IsClassUsageCommentRead";
			zCheckBoxColumnStyleInfo2.GroupName = Enterprise.Customs.GUI.Res.GetData("1a82b32a-8585-421e-bcff-f638e213c96b", "Usage Comment");
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo2.IsMandatory = false;
			zRiskStatusColumnStyleInfo.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("1A68FECB-654E-4C6C-8A87-42BA3EADCFE7", "Risk Status");
			zRiskStatusColumnStyleInfo.ColumnName = "LinkedModuleCommodityRiskStatusDescription";
			zRiskStatusColumnStyleInfo.IsVisible = false;
			zRiskStatusColumnStyleInfo.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			zRiskStatusColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zComplianceAlerts.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("13A4EFF7-20EC-46F9-B29F-D97D680F4A3F", "Compliance Alerts");
			zComplianceAlerts.ColumnName = "LegalBookLink";
			zComplianceAlerts.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zComplianceAlerts.IsVisible = false;
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("0468CFB2-1948-4D47-8233-216B99EB5B60", "Import Risk");
			zTextBoxColumnStyleInfo17.ColumnName = "LinkedModuleCommodityImportAlertDescription";
			zTextBoxColumnStyleInfo17.IsVisible = false;
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo17.IsReadOnly = true;
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zRiskStatusColumnStyleInfo);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zComplianceAlerts);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.CustomsInvoiceLinesBoundGrid.CopySelectedRowsAllowed = true;
			this.CustomsInvoiceLinesBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomsInvoiceLinesBoundGrid.GridId = "43ea2b1e-2774-4c86-9336-9833c63e5a51";
			this.CustomsInvoiceLinesBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CustomsInvoiceLinesBoundGrid.LayoutKey = "DataEntryCustomsInvoiceLinesBoundGrid";
			this.CustomsInvoiceLinesBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomsInvoiceLinesBoundGrid.Name = "CustomsInvoiceLinesBoundGrid";
			this.CustomsInvoiceLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 584, true);
			this.CustomsInvoiceLinesBoundGrid.TabIndex = 0;
			this.CustomsInvoiceLinesBoundGrid.ColourDeciding += new System.EventHandler<Enterprise.ZArchitecture.ColourDecidingEventArgs>(this.CustomsInvoiceLinesBoundGrid_ColourDeciding);
			this.CustomsInvoiceLinesBoundGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.InvoiceLine_MouseDown);
			//
			// BottomPanel
			//
			this.BottomPanel.Controls.Add(this.LineDetailTabControl);
			this.BottomPanel.Controls.Add(this.InvoiceLinesSummaryGroupBox);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 264, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 320, true);
			this.BottomPanel.TabIndex = 1;
			//
			// LineDetailTabControl
			//
			this.LineDetailTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.LineDetailTabControl.Controls.Add(this.LineDetailsTabPage);
			this.LineDetailTabControl.Controls.Add(this.NewLineDetailsTabPage);
			this.LineDetailTabControl.Controls.Add(this.ContainersTabPage);
			this.LineDetailTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LineDetailTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LineDetailTabControl.Name = "LineDetailTabControl";
			this.LineDetailTabControl.SelectedIndex = 0;
			this.LineDetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 320, true);
			this.LineDetailTabControl.TabIndex = 0;
			//
			// LineDetailsTabPage
			//
			this.LineDetailsTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|e421a144-f253-4d22-aca6-7b4ce3d90215", "Line Details");
			this.LineDetailsTabPage.Controls.Add(this.ClassificationPanel);
			this.LineDetailsTabPage.Controls.Add(this.InvoiceDetailsGroupBox);
			this.LineDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LineDetailsTabPage.Name = "LineDetailsTabPage";
			this.LineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 298, true);
			this.LineDetailsTabPage.TabIndex = 0;
			//
			// ClassificationPanel
			//
			this.ClassificationPanel.Controls.Add(this.ClassificationDetailsGroupBox);
			this.ClassificationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClassificationPanel.Name = "ClassificationPanel";
			this.ClassificationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 202, true);
			this.ClassificationPanel.TabIndex = 0;
			//
			// ClassificationDetailsGroupBox
			//
			this.ClassificationDetailsGroupBox.Controls.Add(this.CustomsQuantityCalcDropEdit);
			this.ClassificationDetailsGroupBox.Controls.Add(this.BondedWHSOrderNumberTextBox);
			this.ClassificationDetailsGroupBox.Controls.Add(this.BondedWHSOrderLineNumberCalcEdit);
			this.ClassificationDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ClassificationDetailsGroupBox, false);
			this.ClassificationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClassificationDetailsGroupBox.Name = "ClassificationDetailsGroupBox";
			this.ClassificationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 202, true);
			this.ClassificationDetailsGroupBox.TabIndex = 0;
			this.ClassificationDetailsGroupBox.TabStop = false;
			//
			// NewLineDetailsTabPage
			//
			this.NewLineDetailsTabPage.CaptionResourceString = Res.GetData("BaseInvoiceLineUserControl|7453CAFA-1126-405E-81FB-5B19EA42E672", "Line Details");
			this.NewLineDetailsTabPage.Controls.Add(this.InvoiceLineDetailsUserControl);
			this.NewLineDetailsTabPage.Location = ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NewLineDetailsTabPage.Name = "NewLineDetailsTabPage";
			this.NewLineDetailsTabPage.Size = ControlDpiScalingHelper.NewScaledSize(707, 298, true);
			this.NewLineDetailsTabPage.TabIndex = 0;
			//
			// InvoiceLineDetailsUserControl
			//
			this.InvoiceLineDetailsUserControl.Dock = DockStyle.Fill;
			this.InvoiceLineDetailsUserControl.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceLineDetailsUserControl.Name = "InvoiceLineDetailsUserControl";
			this.InvoiceLineDetailsUserControl.Size = ControlDpiScalingHelper.NewScaledSize(1163, 281, true);
			this.InvoiceLineDetailsUserControl.TabIndex = 0;
			this.BindingSource.SetBindingMember(this.InvoiceLineDetailsUserControl, "FilteredInvoiceLines");
			//
			// CustomsQuantityCalcDropEdit
			//
			this.CustomsQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsUnitQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.CustomsUQList)));
			this.CustomsQuantityCalcDropEdit.BindToAmount = "FilteredInvoiceLines.JI_CustomsQuantity";
			this.CustomsQuantityCalcDropEdit.BindToList = "FilteredInvoiceLines.Lookups+CustomsUQList";
			this.CustomsQuantityCalcDropEdit.BindToUnit = "FilteredInvoiceLines.JI_CustomsUnitQty";
			this.CustomsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 40, true);
			this.CustomsQuantityCalcDropEdit.Name = "CustomsQuantityCalcDropEdit";
			this.CustomsQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 17, true);
			this.CustomsQuantityCalcDropEdit.TabIndex = 2;
			this.CustomsQuantityCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// bondedWHSOrderNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.BondedWHSOrderNumberTextBox, "FilteredInvoiceLines.JI_BondedWHSOrderNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_BondedWHSOrderNumber)));
			this.BondedWHSOrderNumberTextBox.CaptionResourceString = null;
			this.BondedWHSOrderNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 94, true);
			this.BondedWHSOrderNumberTextBox.Name = "BondedWHSOrderNumberTextBox";
			this.BondedWHSOrderNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.BondedWHSOrderNumberTextBox.TabIndex = 3;
			this.BondedWHSOrderNumberTextBox.Visible = false;
			// 
			// BondedWHSOrderLineNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.BondedWHSOrderLineNumberCalcEdit, "FilteredInvoiceLines.JI_BondedWHSOrderLineNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_BondedWHSOrderLineNumber)));
			this.BondedWHSOrderLineNumberCalcEdit.CaptionResourceString = null;
			this.BondedWHSOrderLineNumberCalcEdit.DecimalPlaces = 0;
			this.BondedWHSOrderLineNumberCalcEdit.Decimals = 0;
			this.BondedWHSOrderLineNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 94, true);
			this.BondedWHSOrderLineNumberCalcEdit.Name = "BondedWHSOrderLineNumberCalcEdit";
			this.BondedWHSOrderLineNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.BondedWHSOrderLineNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.BondedWHSOrderLineNumberCalcEdit.TabIndex = 4;
			this.BondedWHSOrderLineNumberCalcEdit.Visible = false;
			//
			// InvoiceDetailsGroupBox
			//
			this.InvoiceDetailsGroupBox.Controls.Add(this.JI_RH_NKCommodity_CodeBoundFindBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.VolumeCalcDropEdit);
			this.InvoiceDetailsGroupBox.Controls.Add(this.JI_CountryOfOriginBoundFindBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.JI_WeightCalcDropEdit);
			this.InvoiceDetailsGroupBox.Controls.Add(this.InvoiceQuantityCalcDropEdit);
			this.InvoiceDetailsGroupBox.Controls.Add(this.JI_DescriptionBoundTextBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.JI_LinePriceBoundCurrencyControl);
			this.InvoiceDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.InvoiceDetailsGroupBox, false);
			this.InvoiceDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 202, true);
			this.InvoiceDetailsGroupBox.Name = "InvoiceDetailsGroupBox";
			this.InvoiceDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 96, true);
			this.InvoiceDetailsGroupBox.TabIndex = 0;
			this.InvoiceDetailsGroupBox.TabStop = false;
			//
			// JI_RH_NKCommodity_CodeBoundFindBox
			//
			this.JI_RH_NKCommodity_CodeBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_RH_NKCommodity_CodeBoundFindBox, "FilteredInvoiceLines.JI_RH_NKCommodity_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_RH_NKCommodity_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.Commodity_Codes)));
			this.JI_RH_NKCommodity_CodeBoundFindBox.BindToList = "FilteredInvoiceLines.Lookups+Commodity_Codes";
			this.JI_RH_NKCommodity_CodeBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(580, 64, true);
			this.JI_RH_NKCommodity_CodeBoundFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCommodityCode;
			this.JI_RH_NKCommodity_CodeBoundFindBox.Name = "JI_RH_NKCommodity_CodeBoundFindBox";
			this.JI_RH_NKCommodity_CodeBoundFindBox.PreBoundMaxLength = 4;
			this.JI_RH_NKCommodity_CodeBoundFindBox.ShowDescriptionBox = false;
			this.JI_RH_NKCommodity_CodeBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.JI_RH_NKCommodity_CodeBoundFindBox.TabIndex = 12;
			//
			// VolumeCalcDropEdit
			//
			this.VolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_VolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.VolumeUQList)));
			this.VolumeCalcDropEdit.BindToAmount = "FilteredInvoiceLines.JI_Volume";
			this.VolumeCalcDropEdit.BindToList = "FilteredInvoiceLines.Lookups+VolumeUQList";
			this.VolumeCalcDropEdit.BindToUnit = "FilteredInvoiceLines.JI_VolumeUQ";
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(541, 40, true);
			this.VolumeCalcDropEdit.Name = "VolumeCalcDropEdit";
			this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 17, true);
			this.VolumeCalcDropEdit.TabIndex = 7;
			this.VolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			//
			// JI_CountryOfOriginBoundFindBox
			//
			this.JI_CountryOfOriginBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_CountryOfOriginBoundFindBox, "FilteredInvoiceLines.JI_CountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CountryOfOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.CountryList)));
			this.JI_CountryOfOriginBoundFindBox.BindToList = "FilteredInvoiceLines.Lookups+CountryList";
			this.JI_CountryOfOriginBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(336, 40, true);
			this.JI_CountryOfOriginBoundFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.JI_CountryOfOriginBoundFindBox.Name = "JI_CountryOfOriginBoundFindBox";
			this.JI_CountryOfOriginBoundFindBox.PreBoundMaxLength = 2;
			this.JI_CountryOfOriginBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 17, true);
			this.JI_CountryOfOriginBoundFindBox.TabIndex = 5;
			//
			// JI_WeightCalcDropEdit
			//
			this.JI_WeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_WeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.WeightUQList)));
			this.JI_WeightCalcDropEdit.BindToAmount = "FilteredInvoiceLines.JI_Weight";
			this.JI_WeightCalcDropEdit.BindToList = "FilteredInvoiceLines.Lookups+WeightUQList";
			this.JI_WeightCalcDropEdit.BindToUnit = "FilteredInvoiceLines.JI_WeightUQ";
			this.JI_WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(336, 64, true);
			this.JI_WeightCalcDropEdit.Name = "JI_WeightCalcDropEdit";
			this.JI_WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 17, true);
			this.JI_WeightCalcDropEdit.TabIndex = 11;
			this.JI_WeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			//
			// InvoiceQuantityCalcDropEdit
			//
			this.InvoiceQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_InvoiceQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_InvoiceUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.InvoiceUQList)));
			this.InvoiceQuantityCalcDropEdit.BindToAmount = "FilteredInvoiceLines.JI_InvoiceQuantity";
			this.InvoiceQuantityCalcDropEdit.BindToList = "FilteredInvoiceLines.Lookups+InvoiceUQList";
			this.InvoiceQuantityCalcDropEdit.BindToUnit = "FilteredInvoiceLines.JI_InvoiceUQ";
			this.InvoiceQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 40, true);
			this.InvoiceQuantityCalcDropEdit.Name = "InvoiceQuantityCalcDropEdit";
			this.InvoiceQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 17, true);
			this.InvoiceQuantityCalcDropEdit.TabIndex = 3;
			this.InvoiceQuantityCalcDropEdit.UnitPreBoundMaxLength = 3;
			//
			// JI_DescriptionBoundTextBox
			//
			this.JI_DescriptionBoundTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_DescriptionBoundTextBox, "FilteredInvoiceLines.JI_Description");
			this.JI_DescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 16, true);
			this.JI_DescriptionBoundTextBox.Name = "JI_DescriptionBoundTextBox";
			this.JI_DescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 21, true);
			this.JI_DescriptionBoundTextBox.TabIndex = 1;
			//
			// JI_LinePriceBoundCurrencyControl
			//
			this.JI_LinePriceBoundCurrencyControl.AllowDrop = true;
			this.JI_LinePriceBoundCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_LinePrice";
			this.JI_LinePriceBoundCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
			this.JI_LinePriceBoundCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_NKLinePriceCurr";
			this.JI_LinePriceBoundCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.JI_LinePriceBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 64, true);
			this.JI_LinePriceBoundCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.JI_LinePriceBoundCurrencyControl.Name = "JI_LinePriceBoundCurrencyControl";
			this.JI_LinePriceBoundCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 20, true);
			this.JI_LinePriceBoundCurrencyControl.TabIndex = 9;
			//
			// ContainersTabPage
			//
			this.ContainersTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|5b8df0d0-ddc6-4f26-9691-cb8ab0da6559", "Containers");
			this.ContainersTabPage.Controls.Add(this.ContainersGroupBox);
			this.ContainersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ContainersTabPage.Name = "ContainersTabPage";
			this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 298, true);
			this.ContainersTabPage.TabIndex = 2;
			//
			// ContainersGroupBox
			//
			this.ContainersGroupBox.Controls.Add(this.CusContainerInvoiceLineGrid);
			this.ContainersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ContainersGroupBox, false);
			this.ContainersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainersGroupBox.Name = "ContainersGroupBox";
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 298, true);
			this.ContainersGroupBox.TabIndex = 0;
			this.ContainersGroupBox.TabStop = false;
			//
			// CusContainerInvoiceLineGrid
			//
			this.CusContainerInvoiceLineGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CusContainerInvoiceLineGrid, "FilteredInvoiceLines.ContainersForInvoiceLinesForBindingOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ContainersForInvoiceLinesForBindingOnly)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.NonPersistentCusContainer)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ContainersForInvoiceLinesForBindingOnly)).SyncRoot)).ContainerNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.NonPersistentCusContainer)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ContainersForInvoiceLinesForBindingOnly)).SyncRoot)).IsForInvoiceLine)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.NonPersistentCusContainer)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ContainersForInvoiceLinesForBindingOnly)).SyncRoot)).Seal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.NonPersistentCusContainer)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ContainersForInvoiceLinesForBindingOnly)).SyncRoot)).ContainerWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.NonPersistentCusContainer)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ContainersForInvoiceLinesForBindingOnly)).SyncRoot)).Mode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.NonPersistentCusContainer)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ContainersForInvoiceLinesForBindingOnly)).SyncRoot)).GrossWeightInKG)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.NonPersistentCusContainer)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ContainersForInvoiceLinesForBindingOnly)).SyncRoot)).NetWeightInKG)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.NonPersistentCusContainer)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ContainersForInvoiceLinesForBindingOnly)).SyncRoot)).PackQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.NonPersistentCusContainer)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ContainersForInvoiceLinesForBindingOnly)).SyncRoot)).SplitValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.NonPersistentCusContainer)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ContainersForInvoiceLinesForBindingOnly)).SyncRoot)).SplitValueCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.NonPersistentCusContainer)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ContainersForInvoiceLinesForBindingOnly)).SyncRoot)).CurrencyList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.NonPersistentCusContainer)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ContainersForInvoiceLinesForBindingOnly)).SyncRoot)).OwnerCountry)));
			this.CusContainerInvoiceLineGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|f8d2fc87-f0d0-4373-ad00-6e53d355e36a", "Container Number");
			zTextBoxColumnStyleInfo10.ColumnName = "ContainerNumber";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|9f452fc9-5fae-4e91-b699-705b07f9f4ee", "Is For Invoice Line?");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsForInvoiceLine";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|cf097b6a-4d1a-4fcd-baef-73722b816d00", "Seal");
			zTextBoxColumnStyleInfo11.ColumnName = "Seal";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			c2PivotContainerWeightColumnStyleInfo9.BindToDecimalPlaces = null;
			c2PivotContainerWeightColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|a85241f7-e306-48b3-a5e6-e84039ba09bf", "Container Wgt.");
			c2PivotContainerWeightColumnStyleInfo9.ColumnName = "ContainerWeight";
			c2PivotContainerWeightColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|fdf6f4c6-c995-4345-9770-25c3c96c8df8", "Mode");
			zTextBoxColumnStyleInfo12.ColumnName = "Mode";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			c2PivotContainerGrossWeightColumnStyleInfo10.BindToDecimalPlaces = null;
			c2PivotContainerGrossWeightColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|c02a84d9-b669-4dae-bd54-aaed44dab0ac", "Gross Wgt. In KG");
			c2PivotContainerGrossWeightColumnStyleInfo10.ColumnName = "GrossWeightInKG";
			c2PivotContainerGrossWeightColumnStyleInfo10.IsVisible = false;
			c2PivotContainerGrossWeightColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			c2PivotContainerNetWeightColumnStyleInfo11.BindToDecimalPlaces = null;
			c2PivotContainerNetWeightColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|f43ac2e4-a9b2-4f6d-a9d8-70d9c8daace3", "Net Wgt. In KG");
			c2PivotContainerNetWeightColumnStyleInfo11.ColumnName = "NetWeightInKG";
			c2PivotContainerNetWeightColumnStyleInfo11.IsVisible = false;
			c2PivotContainerNetWeightColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			c2PivotContainerPackQtyColumnStyleInfo12.BindToDecimalPlaces = null;
			c2PivotContainerPackQtyColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|e8751452-ce2a-42f7-9fe5-964145e39d20", "Pack Qty");
			c2PivotContainerPackQtyColumnStyleInfo12.ColumnName = "PackQty";
			c2PivotContainerPackQtyColumnStyleInfo12.IsVisible = false;
			c2PivotContainerPackQtyColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			c2PivotContainerSplitValueColumnStyleInfo13.BindToDecimalPlaces = null;
			c2PivotContainerSplitValueColumnStyleInfo13.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|09245c4f-f45f-4f21-a239-b2ca3a2fc041", "Split Value");
			c2PivotContainerSplitValueColumnStyleInfo13.ColumnName = "SplitValue";
			c2PivotContainerSplitValueColumnStyleInfo13.GroupName = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|151b98fe-3488-470f-92a6-18228d743aec", "Value");
			c2PivotContainerSplitValueColumnStyleInfo13.IsVisible = false;
			c2PivotContainerSplitValueColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			c2ContainerSplitCurrencyColumnStyleInfo2.BindToList = "CurrencyList";
			c2ContainerSplitCurrencyColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|dc076fe9-cca0-4789-82d2-3dbdf0583140", "Split Value Currency");
			c2ContainerSplitCurrencyColumnStyleInfo2.ColumnName = "SplitValueCurrency";
			c2ContainerSplitCurrencyColumnStyleInfo2.GroupName = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|151b98fe-3488-470f-92a6-18228d743aec", "Value");
			c2ContainerSplitCurrencyColumnStyleInfo2.IsVisible = false;
			c2ContainerSplitCurrencyColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			c2ContainerSplitCurrencyColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|87538e7e-6127-4cdc-a2c3-62a073239f6b", "Owner Country");
			zTextBoxColumnStyleInfo16.ColumnName = "OwnerCountry";
			zTextBoxColumnStyleInfo16.IsVisible = false;
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.CusContainerInvoiceLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.CusContainerInvoiceLineGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CusContainerInvoiceLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.CusContainerInvoiceLineGrid.ColumnStyles.Add(c2PivotContainerWeightColumnStyleInfo9);
			this.CusContainerInvoiceLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.CusContainerInvoiceLineGrid.ColumnStyles.Add(c2PivotContainerGrossWeightColumnStyleInfo10);
			this.CusContainerInvoiceLineGrid.ColumnStyles.Add(c2PivotContainerNetWeightColumnStyleInfo11);
			this.CusContainerInvoiceLineGrid.ColumnStyles.Add(c2PivotContainerPackQtyColumnStyleInfo12);
			this.CusContainerInvoiceLineGrid.ColumnStyles.Add(c2PivotContainerSplitValueColumnStyleInfo13);
			this.CusContainerInvoiceLineGrid.ColumnStyles.Add(c2ContainerSplitCurrencyColumnStyleInfo2);
			this.CusContainerInvoiceLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.CusContainerInvoiceLineGrid.CopySelectedRowsAllowed = true;
			this.CusContainerInvoiceLineGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CusContainerInvoiceLineGrid.GridId = "1eea3a76-d153-47d0-81d8-a88a6d6b1e48";
			this.CusContainerInvoiceLineGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CusContainerInvoiceLineGrid.LayoutKey = "zGrid1";
			this.CusContainerInvoiceLineGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.CusContainerInvoiceLineGrid.Name = "CusContainerInvoiceLineGrid";
			this.CusContainerInvoiceLineGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.CusContainerInvoiceLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 281, true);
			this.CusContainerInvoiceLineGrid.TabIndex = 0;
			//
			// CantCreateInvoiceLinesLabel
			//
			this.CantCreateInvoiceLinesLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CantCreateInvoiceLinesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CantCreateInvoiceLinesLabel.Name = "CantCreateInvoiceLinesLabel";
			this.CantCreateInvoiceLinesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 584, true);
			this.CantCreateInvoiceLinesLabel.TabIndex = 13;
			this.CantCreateInvoiceLinesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			//
			// Splitter
			//
			this.Splitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.Splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 242, true);
			this.Splitter.Name = "Splitter";
			this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 10, true);
			this.Splitter.TabIndex = 1;
			this.Splitter.TabStop = false;
			//
			// StipControl
			//
			this.StripControl.AllowDrop = true;
			this.StripControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.StripControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StripControl.Name = "ZGridExtendedStripControl";
			this.StripControl.AutoSize = true;
			this.StripControl.TabIndex = 14;
			//
			// BaseInvoiceLineUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.TopPanel);
			this.Controls.Add(this.CantCreateInvoiceLinesLabel);
			this.Controls.Add(this.Splitter);
			this.Controls.Add(this.StripControl);
			this.Name = "BaseInvoiceLineUserControl";
			this.Controls.SetChildIndex(this.StripControl, 0);
			this.Controls.SetChildIndex(this.CantCreateInvoiceLinesLabel, 0);
			this.Controls.SetChildIndex(this.TopPanel, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.Splitter, 0);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 584, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InvoiceLinesSummaryGroupBox.ResumeLayout(false);
			this.InvoiceLinesSummaryGroupBox.PerformLayout();
			this.LineSummaryPanel.ResumeLayout(false);
			this.LineSummaryPanel.PerformLayout();
			this.JI_Calc_CIFConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_CIFConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_FreightConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_FreightConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_FOBConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_FOBConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_GSTConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_GSTConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_DutyConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_DutyConvertToLocalCurrencyControl.PerformLayout();
			this.CurrentInvoicePanel.ResumeLayout(false);
			this.CurrentInvoicePanel.PerformLayout();
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).EndInit();
			this.CustomsInvoiceLinesBoundGrid.ResumeLayout(false);
			this.CustomsInvoiceLinesBoundGrid.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.LineDetailTabControl.ResumeLayout(false);
			this.LineDetailTabControl.PerformLayout();
			this.LineDetailsTabPage.ResumeLayout(false);
			this.LineDetailsTabPage.PerformLayout();
			this.NewLineDetailsTabPage.ResumeLayout(false);
			this.NewLineDetailsTabPage.PerformLayout();
			this.ClassificationPanel.ResumeLayout(false);
			this.ClassificationPanel.PerformLayout();
			this.ClassificationDetailsGroupBox.ResumeLayout(false);
			this.ClassificationDetailsGroupBox.PerformLayout();
			this.CustomsQuantityCalcDropEdit.ResumeLayout(true);
			this.CustomsQuantityCalcDropEdit.PerformLayout();
			this.InvoiceDetailsGroupBox.ResumeLayout(false);
			this.InvoiceDetailsGroupBox.PerformLayout();
			this.JI_RH_NKCommodity_CodeBoundFindBox.ResumeLayout(true);
			this.JI_RH_NKCommodity_CodeBoundFindBox.PerformLayout();
			this.VolumeCalcDropEdit.ResumeLayout(true);
			this.VolumeCalcDropEdit.PerformLayout();
			this.JI_CountryOfOriginBoundFindBox.ResumeLayout(true);
			this.JI_CountryOfOriginBoundFindBox.PerformLayout();
			this.JI_WeightCalcDropEdit.ResumeLayout(true);
			this.JI_WeightCalcDropEdit.PerformLayout();
			this.InvoiceQuantityCalcDropEdit.ResumeLayout(true);
			this.InvoiceQuantityCalcDropEdit.PerformLayout();
			this.JI_DescriptionBoundTextBox.ResumeLayout(true);
			this.JI_DescriptionBoundTextBox.PerformLayout();
			this.JI_LinePriceBoundCurrencyControl.ResumeLayout(true);
			this.JI_LinePriceBoundCurrencyControl.PerformLayout();
			this.ContainersTabPage.ResumeLayout(false);
			this.ContainersTabPage.PerformLayout();
			this.ContainersGroupBox.ResumeLayout(false);
			this.ContainersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).EndInit();
			this.CusContainerInvoiceLineGrid.ResumeLayout(false);
			this.CusContainerInvoiceLineGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion
	}
}

using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

public partial class NLImportSupplierHeaderUserControl : EU.GUI.EUImportSupplierHeaderUserControl
{
	public NLImportSupplierHeaderUserControl()
	{
		InitializeComponent();
		ReorderTabPages();
		JobComInvoiceHeadersBoundGrid.InnerGrid.AfterBind -= JobComInvoiceHeadersBoundGrid_AfterBind;
		JobComInvoiceHeadersBoundGrid.InnerGrid.AfterBind += JobComInvoiceHeadersBoundGrid_AfterBind;
		ChargesGroupBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("NLImportSupplierHeaderUserControl|3da4b650-6e2d-48cb-9eda-996f20c4b003", "[UCC 4/9] Invoice Charges");
		BaseGroupChargesGroupBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("NLImportSupplierHeaderUserControl|52ef45c3-f115-4258-b47e-a8cb8ffc5ae4", "[UCC 4/9] Group Charges (All Invoices)");
	}

	protected override void HookInvoiceHeaderEvents(JobComInvoiceHeader invoiceHeader)
	{
		base.HookInvoiceHeaderEvents(invoiceHeader);

		if (invoiceHeader != null)
		{
			invoiceHeader.InvoiceLines.HasChangesChanged += OnHasChangesChanged;
			invoiceHeader.InvoiceLines.CountChanged += OnCountChanged;
		}
	}

	protected override void UnHookInvoiceHeaderEvents(JobComInvoiceHeader invoiceHeader)
	{
		base.UnHookInvoiceHeaderEvents(invoiceHeader);
		if (invoiceHeader != null)
		{
			invoiceHeader.InvoiceLines.HasChangesChanged -= OnHasChangesChanged;
			invoiceHeader.InvoiceLines.CountChanged -= OnCountChanged;
		}
	}

	protected override void HookDeclarationEventsCore(BaseJobDeclaration declaration)
	{
		base.HookDeclarationEventsCore(declaration);
		declaration.CustomsEntryInstructions.HasChangesChanged += OnHasChangesChanged;
		declaration.CustomsEntryInstructions.CountChanged += OnCountChanged;
	}

	protected override void UnHookDeclarationEventsCore(BaseJobDeclaration declaration)
	{
		base.UnHookDeclarationEventsCore(declaration);
		declaration.CustomsEntryInstructions.HasChangesChanged -= OnHasChangesChanged;
		declaration.CustomsEntryInstructions.CountChanged -= OnCountChanged;
	}

	void JobComInvoiceHeadersBoundGrid_AfterBind(object sender, EventArgs e)
	{
		JobComInvoiceHeadersBoundGrid.InnerGrid.ListManager.CurrentItemChanged -= JobComInvoiceHeadersBoundGrid_ListManager_CurrentItemChanged;
		JobComInvoiceHeadersBoundGrid.InnerGrid.ListManager.CurrentItemChanged += JobComInvoiceHeadersBoundGrid_ListManager_CurrentItemChanged;
	}

	void JobComInvoiceHeadersBoundGrid_ListManager_CurrentItemChanged(object sender, EventArgs e)
	{
		if (CurrentInvoiceHeader != null)
		{
			SetValueIndicatorsTabPageVisible(CurrentInvoiceHeader);
		}
	}

	void OnHasChangesChanged(object sender, HasChangesChangedEventArgs e)
	{
		var invoice = CurrentInvoiceHeader;

		if (e.ObjectJustWasChanged && invoice != null && !invoice.IsDeleted && !((IBusinessObjectState)invoice).HasChangesNotIncludingChildren)
		{
			SetValueIndicatorsTabPageVisible(invoice);
		}
	}

	void OnCountChanged(object sender, CollectionCountChangedEventArgs e)
	{
		var invoice = CurrentInvoiceHeader;

		if (invoice != null && !invoice.IsDeleted && e.ItemRemoved)
		{
			SetValueIndicatorsTabPageVisible(invoice);
		}
	}

	void SetValueIndicatorsTabPageVisible(BaseJobComInvoiceHeader invoice)
	{
		var invoiceLines = invoice.InvoiceLines;
		var tabVisible = false;

		if (invoiceLines.Cast<Business.Declaration.JobComInvoiceLine>().Any(x => x.EntryInstruction?.ZG_IsHighValueOvrd ?? false))
		{
			tabVisible = true;
		}

		ValueIndicatorsTabPage.TabVisible = tabVisible;
	}

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();
		AddColumns();

		var currencyBoxColumnStyleInfo = JobComInvoiceHeadersBoundGrid.ColumnStyles.OfType<ZCodeFindBoxColumnStyleInfo>().Single(x => x.ColumnName == Business.Declaration.JobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency);
		var exchangeRateBoxColumnStyleInfo = JobComInvoiceHeadersBoundGrid.ColumnStyles.OfType<ZCalcEditColumnStyleInfo>().Single(x => x.ColumnName == Business.Declaration.JobComInvoiceHeader.Schema.JZ_InvoiceCurrExRate);
		var currencyCaption = Res.GetData("BD76BB97-A618-4966-A9AC-AD49F21513BH", "[UCC 4/11] Curr.");
		var exchangeRateCaption = Res.GetData("BD76BB97-A618-4966-A9AC-AD49F21513BG", "Exchange Rate", "[UCC 4/15] Exchange Rate");
		var intracommunityReceiverFindBoxColumnStyleInfo = JobComInvoiceHeadersBoundGrid.ColumnStyles.OfType<ZOrganisationFindBoxColumnStyleInfo>().Single(x => x.ColumnName == Business.Declaration.JobComInvoiceHeader.Schema.ConsigneeOrgPK);
		var intracommunityReceiverDropEditColumnStyleInfo = JobComInvoiceHeadersBoundGrid.ColumnStyles.OfType<ZGuidDropEditColumnStyleInfo>().Single(x => x.ColumnName == Business.Declaration.JobComInvoiceHeader.Schema.JZ_OA_ConsigneeAddress);
		var captionReceiver = Res.GetData("BD76BB97-A618-4966-A9AC-AD49F21513BF", "Intra-community Receiver");
		var captionReceiverAddress = Res.GetData("31B7223D-327D-4841-97EE-0384D171443B", "Intra-community Receiver Address");

		currencyBoxColumnStyleInfo.CaptionResourceString = currencyCaption;
		exchangeRateBoxColumnStyleInfo.CaptionResourceString = exchangeRateCaption;

		intracommunityReceiverFindBoxColumnStyleInfo.CaptionResourceString = captionReceiver;
		intracommunityReceiverFindBoxColumnStyleInfo.GroupName = captionReceiver;
		intracommunityReceiverFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
		intracommunityReceiverFindBoxColumnStyleInfo.IsVisible = true;

		intracommunityReceiverDropEditColumnStyleInfo.CaptionResourceString = captionReceiverAddress;
		intracommunityReceiverDropEditColumnStyleInfo.ToolTip = captionReceiverAddress.ToString();
		intracommunityReceiverDropEditColumnStyleInfo.GroupName = captionReceiver;
		intracommunityReceiverDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
		intracommunityReceiverDropEditColumnStyleInfo.IsVisible = true;

		var exchangeRateColumn = InvoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_ExchangeRate);
		exchangeRateColumn.CaptionResourceString = exchangeRateCaption;
		var currencyColumn = InvoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_RX_NKCurrency);
		currencyColumn.CaptionResourceString = currencyCaption;
		var baseGroupExchangeRateColumn = BaseGroupChargesGrid.GetColumnStyle(BaseGroupInvoiceCharge.Schema.J7_ExchangeRate);
		baseGroupExchangeRateColumn.CaptionResourceString = exchangeRateCaption;
		var baseGroupCurrencyColumn = BaseGroupChargesGrid.GetColumnStyle(BaseGroupInvoiceCharge.Schema.J7_RX_NKCurrency);
		baseGroupCurrencyColumn.CaptionResourceString = currencyCaption;
		var apportionedExchangeRateColumn = ApportionedChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_ExchangeRate);
		apportionedExchangeRateColumn.CaptionResourceString = exchangeRateCaption;
		var apportionedCurrencyColumn = ApportionedChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_RX_NKCurrency);
		apportionedCurrencyColumn.CaptionResourceString = currencyCaption;
	}

	void ReorderTabPages()
	{
		InvoiceTabControl.TabPages.Clear();
		InvoiceTabControl.TabPages.Remove(ComInvoiceDetailsTabPage);
		InvoiceTabControl.TabPages.Insert(ComInvoiceDetailsTabPage, 0);

		SupportingDocumentsTabPage.CaptionResourceString = Res.GetData("358495BA-7707-4BA1-96FD-8C373F437C83", "Supporting Documents");
		InvoiceTabControl.TabPages.Remove(SupportingDocumentsTabPage);
		InvoiceTabControl.TabPages.Insert(SupportingDocumentsTabPage, 1);

		InvoiceTabControl.TabPages.Remove(AdditionalInfoTabPage);
		InvoiceTabControl.TabPages.Insert(AdditionalInfoTabPage, 2);

		InvoiceTabControl.TabPages.Remove(PreviousDocumentsTabPage);
		InvoiceTabControl.TabPages.Insert(PreviousDocumentsTabPage, 3);

		InvoiceTabControl.TabPages.Remove(ValueIndicatorsTabPage);
		InvoiceTabControl.TabPages.Insert(ValueIndicatorsTabPage, 4);

		InvoiceTabControl.TabPages.Remove(CustomFieldsTabPage);
		InvoiceTabControl.TabPages.Insert(CustomFieldsTabPage, 5);
	}

	void AddColumns()
	{
		JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
		{
			ColumnName = Business.Declaration.JobComInvoiceHeader.Schema.JZ_UCR,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
		});
	}

	protected override Type GetValueIndicatorsUserControlType() => typeof(NLValueIndicatorsUserControl);

	protected override Type GetSupportingDocumentsUserControlType() => typeof(NLSupportingDocumentsUserControl);

	protected override Type GetAdditionalInfosUserControlType() => typeof(InvoiceLineAdditionalInfosUserControlWithGrid);

	protected override Type GetPreviousDocumentsUserControlType() => typeof(NLPreviousDocumentsUserControl);

	protected override ResourceStringData GetAdditionalInfoTabPageCaption(JobDeclaration declaration) => Res.GetData("FE6F0F39-EF3F-4C9F-9E5F-ADCF515BE4E9", "[44] Additional Documents");

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			JobComInvoiceHeadersBoundGrid.InnerGrid.AfterBind -= JobComInvoiceHeadersBoundGrid_AfterBind;
		}

		base.Dispose(disposing);
	}
}

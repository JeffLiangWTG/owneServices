using System;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.TW.GUI
{
	public partial class CustomsEntriesAndEntryLinesUserControl : Customs.GUI.ImportMessageUserControl
	{
		public CustomsEntriesAndEntryLinesUserControl()
		{
			InitializeComponent();
			LoadEntryLineAdditionalDataUserControl();
			VisibilControls();
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();

			ResetGridDefaultOrderAndVisibleColumns(EntryLineGrid, entryLineGridDefaultColumnsInSortOrder, entryLineGridDefaultColumnsInSortOrder);
		}

		void VisibilControls()
		{
			MessageTabPage.TabVisible = false;
		}

		protected override Type GetBaseMessagesTabUserControlType() => typeof(MessagesTabUserControl);

		EntryLineAdditionalDataUserControl EntryLineAdditionalDataUserControl => entryLineAdditionalDataUserControl ?? (entryLineAdditionalDataUserControl = new EntryLineAdditionalDataUserControl());
		EntryLineAdditionalDataUserControl entryLineAdditionalDataUserControl;

		void LoadEntryLineAdditionalDataUserControl()
		{
			EntryLineAdditionalDataUserControl.AllowDrop = true;
			EntryLineAdditionalDataUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			EntryLineAdditionalDataUserControl.Name = "EntryLineAdditionalDataUserControl";
			EntryLineAdditionalDataUserControl.TabIndex = 2;
			EntryLineAdditionalDataUserControl.Visible = true;
			EntryLinesTabPage.Controls.Add(EntryLineAdditionalDataUserControl);
		}

		void ResetGridDefaultOrderAndVisibleColumns(ZGrid grid, string[] orderColumns, string[] visibleColumns)
		{
			using (grid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				grid.ReOrderColumns(orderColumns);
				grid.SetAllColumnsVisible(false);
				grid.SetColumnVisible(true, visibleColumns);
			}
		}

		readonly string[] entryLineGridDefaultColumnsInSortOrder =
		[
			CusEntryLine.Schema.CL_LineNumber,
			CusEntryLine.Schema.FormattedTariff,
			CusEntryLine.Schema.CL_Procedure,
			CusEntryLine.Schema.CL_CustomsValue,
			CusEntryLine.Schema.CL_Calc_GoodsDescription,
			CusEntryLine.Schema.CL_EntryLineUnitPrice,
			CusEntryLine.Schema.CL_EntryLineQty,
			CusEntryLine.Schema.CL_EntryLineUQ,
			CusEntryLine.Schema.CL_EntryLineUQDescription,
			CusEntryLine.Schema.CL_Calc_NetWeightInKG,
			CusEntryLine.Schema.CL_Calc_CustomsSecondQuantity,
			CusEntryLine.Schema.CL_CustomsSecondUnitQty,
		];

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetCaptions();
		}

		void SetCaptions()
		{
			var isImport = ((JobDeclaration)CurrentDataItem)?.IsImport ?? false;
			TotalCustomsValueInLocalCurrencyControl.CaptionResourceString = ResourceStringHelper.GetTotalCustomsValueInLocalCurrencyCaption(isImport);
			TotalCustomsValueInInvoiceCurrencyControl.CaptionResourceString = ResourceStringHelper.GetTotalCustomsValueInInvoiceCurrencyCaption(isImport);
			TotalInternationalFreightAmountInInvoiceCurrencyControl.CaptionResourceString = ResourceStringHelper.GetTotalInternationalFreightAmountInInvoiceCurrencyResString(isImport);
			TotalInternationalInsuranceAmountInInvoiceCurrencyControl.CaptionResourceString = ResourceStringHelper.GetTotalInternationalInsuranceAmountInInvoiceCurrencyCaption(isImport);
			TotalAdditionsInInvoiceCurrencyControl.CaptionResourceString = ResourceStringHelper.GetTotalAdditionsInInvoiceCurrencyCaption(isImport);
			TotalDeductionsInInvoiceCurrencyControl.CaptionResourceString = ResourceStringHelper.GetTotalDeductionsInInvoiceCurrencyCaption(isImport);
		}
	}
}

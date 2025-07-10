using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class ReconInvoiceLineUserControl : ZUserControl
	{
		public ReconInvoiceLineUserControl()
		{
			InitializeComponent();

			OriginalTaxRateQuantityCalcEdit.AllowOverlap(TaxRateCalcEdit);
			ReconTaxRateCalcEdit.AllowOverlap(ReconTaxRateQuantityCalcEdit);
		}

		public new IInvoicesProvider CurrentDataItem
		{
			get { return (IInvoicesProvider)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				AddDefaultInvoiceForSingleEntry();
			}
		}

		void AddDefaultInvoiceForSingleEntry()
		{
			if (CurrentDataItem != null)
			{
				CurrentDataItem.AddDefaultInvoice();
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			TaxRateCalcEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);
			ReconTaxRateCalcEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);

			OriginalTaxRateQuantityCalcEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);
			ReconTaxRateQuantityCalcEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);

			if (dataSource != null)
			{
				TaxRateCalcEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, "FilteredInvoiceLines.IsOrigTaxRateSpecifiedManually", false, DataSourceUpdateMode.Never));
				ReconTaxRateCalcEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, "FilteredInvoiceLines.IsTaxRateSpecifiedManually", false, DataSourceUpdateMode.Never));

				OriginalTaxRateQuantityCalcEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, "FilteredInvoiceLines.IsOrigTaxRateQuantityRequired", false, DataSourceUpdateMode.Never));
				ReconTaxRateQuantityCalcEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, "FilteredInvoiceLines.IsTaxQtyRequired", false, DataSourceUpdateMode.Never));
			}
		}

		public const string IsVisibleForBindingString = "IsVisibleForBinding";

		public void ChangeVisibilityForACE(bool isACE)
		{
			var columns = new string[] { AddInfo.Schema.US_R_HTSChanged4ValueInd, AddInfo.Schema.US_R_ReconReasonText };
			InvoicesGrid.SetAvailability(isACE, columns);
			RefundedFeesTabPage.TabVisible = isACE;
		}
	}
}

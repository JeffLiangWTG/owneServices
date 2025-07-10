using System;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	/// <summary>
	/// This control allows invoice lines to select header-level packages for each invoice line. 
	/// It has "Is for invoice line" for the packagaes and it's equivilent to the same idea for the containers. 
	/// If you need packages at invoice line, there's no need to ask for them to be saved in table B5.  Instead 
	/// the user can tick which header-level package (table CW) should apply to which line and the pivots will be stored in table CHC. 
	/// </summary>
	public partial class BaseLineLevelPackingPivotControl : ZUserControl
	{
		public BaseLineLevelPackingPivotControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var invoiceLine = CurrentDataItem as BaseJobComInvoiceLine;
			if (invoiceLine != null)
			{
				invoiceLine.PackagePivotsChanged += new EventHandler(RefreshItRefreshPackageInvoiceLineGrid);
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			var invoiceLine = CurrentDataItem as BaseJobComInvoiceLine;
			if (invoiceLine != null)
			{
				invoiceLine.PackagePivotsChanged -= new EventHandler(RefreshItRefreshPackageInvoiceLineGrid);
			}
		}

		void RefreshItRefreshPackageInvoiceLineGrid(object sender, EventArgs e)
		{
			DjcPackageInvoiceLineGrid.Refresh();
		}
	}
}

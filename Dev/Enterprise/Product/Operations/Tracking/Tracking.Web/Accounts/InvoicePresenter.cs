using System.Web;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	/// <summary>
	/// Class that takes care of displaying invoices on different forms
	/// </summary>
	public class InvoicePresenter
	{
		public InvoicePresenter(TrackingSiteUser siteUser)
		{
			this.SiteUser = siteUser;
		}

		public const string ViewInvoiceCommand = "ViewInvoice";
		readonly TrackingSiteUser SiteUser;

		public void SetupGrid(ZGrid grid)
		{
			grid.ColumnProvider = new ARAPInvoicingColumnProvider(SiteUser);
			EnableItemCommand(grid);
		}

		public DataGridColumn[] GetSearchResultColumns()
		{
			ARAPInvoicingSearchColumnProvider columnProvider = new ARAPInvoicingSearchColumnProvider(SiteUser);
			columnProvider.CustomizeDictionary();
			return columnProvider.AllColumns.ToArray();
		}

		public void EnableItemCommand(ZDataGrid grid)
		{
			grid.ItemCommand += new DataGridCommandEventHandler(SearchResultsDataGrid_ItemCommand);
		}

#if DEBUG
		public
#endif
				void SearchResultsDataGrid_ItemCommand(object source, DataGridCommandEventArgs e)
		{
			if (e.CommandName == ViewInvoiceCommand && source is ZDataGrid grid)
			{
				if (!SiteUser.IsShipmentQuickViewUser)
				{
					var invoicePK = grid.GetPKByRowIndex(e.Item.ItemIndex);
					var url = GetCachedImageServiceUrl(invoicePK);

					if (!string.IsNullOrEmpty(url))
					{
						if (grid.Page is IZAjaxPage ajaxPage && ajaxPage.IsAsyncPostBack)
						{
							ajaxPage.UpdatePanelRedirect(ViewInvoiceCommand, url);
						}
						else
						{
							HttpContext.Current.Response.Redirect(url, false);
						}
					}
				}
			}
		}

		public string GetCachedImageServiceUrl(ZGuid invoicePK)
		{
			return !invoicePK.IsEmpty ? InvoiceRequestHandler.RequestHelper.GetHandlerUrl(invoicePK) : "";
		}

		#region SetupLocalChargesGrid

		public void SetupLocalChargesGrid(ZGrid grid)
		{
			grid.ColumnProvider = new ARAPInvoicingLineColumnProvider();
			EnableTotalCalculation(grid);
		}

		#endregion

		#region Total Calculation

		void EnableTotalCalculation(ZDataGrid grid)
		{
			grid.ShowFooter = true;
			grid.ItemDataBound += LocalChargesGrid_ItemDataBound;
		}

		void LocalChargesGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Footer)
			{
				e.Item.Cells[0].Text = Res.GetString("0eb6317a-a966-4e1d-aa8d-22a7fff992ab", "Total");
				ZDataGrid grid = sender as ZDataGrid;
				InvoicingLineBaseCollection lines = grid != null ? grid.DataSource as InvoicingLineBaseCollection : null;

				if (grid != null && lines != null && lines.Count > 0)
				{
					ZString totalCur = lines[0].AL_RX_NKTransactionCurrency;
					ZDecimal totalExTax = 0;
					ZDecimal totalTax = 0;
					ZDecimal totalAmount = 0;

					foreach (InvoicingLineBase line in lines)
					{
						totalExTax += line.AL_OSExTaxAmount;
						totalTax += line.AL_OSTaxAmount;
						totalAmount += line.AL_OverseasTotal;
					}

					e.Item.Cells[1].Text = totalCur.ToString();
					e.Item.Cells[2].Text = totalExTax.ToString(2);
					e.Item.Cells[3].Text = totalTax.ToString(2);
					e.Item.Cells[4].Text = totalAmount.ToString(2);
				}
				if (grid != null)
				{
					grid.ItemDataBound -= LocalChargesGrid_ItemDataBound;
				}
			}
		}

		#endregion
	}
}

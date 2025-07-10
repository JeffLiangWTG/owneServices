using System;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.GlobalCommercialInvoice.GUI
{
	public partial class GlobalCommercialInvoiceHeaderGridUserControl : ZUserControl
	{
		string[] ColumnNames;

		public GlobalCommercialInvoiceHeaderGridUserControl()
		{
			InitializeComponent();

			ColumnNames = InvoiceHeaderCollectionGrid.ColumnStyles.Cast<ZGridColumnInfo>()
				.Select((x) => x.ColumnName).ToArray();

			InvoiceHeaderCollectionGrid.AfterBind += OnGridAfterBind;
		}

		void OnGridAfterBind(object sender, EventArgs e)
		{
			InvoiceHeaderCollectionGrid.ReOrderColumns(ColumnNames);
			InvoiceHeaderCollectionGrid.AfterBind -= OnGridAfterBind;
			ColumnNames = null;
		}
	}
}

using System;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class OrderInvoiceImporterForm : ZChildForm
	{
		public OrderInvoiceImporterForm(OrderInvoiceImporter invoiceImporter)
			: base(invoiceImporter)
		{
			Importer = invoiceImporter;
		}

		public readonly OrderInvoiceImporter Importer;

		public void SelectAllElements()
		{
			if (OrdersToImportGrid != null && OrdersToImportGrid.InnerGrid != null)
			{
				for (int i = 0; i < OrdersToImportGrid.InnerGrid.ListManager.Count; i++)
				{
					OrdersToImportGrid.InnerGrid.Select(i);
				}
			}
		}

		#region Implementation

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			OrdersToImportGrid.EditFormClosed += new EventHandler(OrdersToImportGrid_EditFormClosed);
		}

		void SelectAllButton_Click(object sender, EventArgs e)
		{
			SelectAllElements();
		}

		void CloseImportButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected void ReconcileSelectedOrdersButton_Click(object sender, EventArgs e)
		{
			Order[] orders = new Order[OrdersToImportGrid.InnerGrid.SelectedElements.Length];
			for (int i = 0; i < OrdersToImportGrid.InnerGrid.SelectedElements.Length; i++)
			{
				orders[i] = (Order)OrdersToImportGrid.InnerGrid.SelectedElements[i];
			}

			ReconcileSelectedOrders(orders);
		}

		void ReconcileSelectedOrders(Order[] selectedOrders)
		{
			if (selectedOrders.Length > 0)
			{
				ShowComInvoiceReconciliationForm(selectedOrders);
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("c2142074-c00e-433e-81c4-7d61a6cf4fad", "No Order has been selected"), Res.GetString("caf35fca-f8ac-44dc-9cee-339c38cba124", "Select an Order"));
			}
		}

		protected void OrdersToImportGrid_EditFormClosed(object sender, EventArgs e)
		{
			Importer.ValidateOrders();
		}

#if DEBUG
		protected virtual void ShowComInvoiceReconciliationForm(Order[] selectedOrders)
#else
		void ShowComInvoiceReconciliationForm(Order[] selectedOrders)
#endif
		{
			try
			{
				Hide();
				var reconciliator = Importer.Declaration.GetNewComInvoiceReconciliator(selectedOrders, Importer.QuantityType);
				if (!Globals.IsTest)
				{
					ZFormModaliser.ShowDialogAndDispose(new ComInvoiceReconciliationForm(reconciliator));
				}
			}
			finally
			{
				Show();
			}
		}

		#region UpdateButton_Click

		protected void UpdateButton_Click(object sender, EventArgs e)
		{
			OnUpdateButtonClick();
		}

		void OnUpdateButtonClick()
		{
			if (OrdersToImportGrid.InnerGrid.SelectedElements.Length > 0)
			{
				for (int i = 0; i < OrdersToImportGrid.InnerGrid.SelectedElements.Length; i++)
				{
					var selectedOrder = (Order)OrdersToImportGrid.InnerGrid.SelectedElements[i];
					Importer.DefaultInvoiceNumberAndDate(selectedOrder);
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("c2142074-c00e-433e-81c4-7d61a6cf4fad", "No Order has been selected"), Res.GetString("caf35fca-f8ac-44dc-9cee-339c38cba124", "Select an Order"));
			}
		}

		#endregion

		#endregion
	}
}

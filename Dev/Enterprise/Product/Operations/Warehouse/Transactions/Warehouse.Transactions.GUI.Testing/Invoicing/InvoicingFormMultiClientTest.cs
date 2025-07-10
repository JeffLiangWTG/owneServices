using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Rating.Integration;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	[TestedType(typeof(InvoicingFormMultiClient))]
	public class InvoicingFormMultiClientTest : ZFormBasherTest
	{
		public void TestAutoRateAndPostInvoices()
		{
			var invoiceMultiClients = new WhsInvoicePeriodicMultiClientInvoice();
			using (var form = new InvoicingFormMultiClient(invoiceMultiClients))
			{
				bool invoiceCompleteCalled = false;
				invoiceMultiClients.InvoiceCreationComplete += delegate
				{ invoiceCompleteCalled = true; };
				var invoiceButton = GUITestHelper.FindControl<Button>(form.Controls, "AutoRateAndPostInvoicesButton");

				form.Show();
				invoiceMultiClients.InvoiceDate = ZDateTime.Empty;
				invoiceButton.PerformClick();
				AssertEquals(true, invoiceMultiClients.HasErrors);
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				invoiceMultiClients.InvoiceDate = ZDateTime.Today;
				invoiceButton.PerformClick();
				AssertEquals(false, invoiceMultiClients.HasErrors);
				AssertEquals("There are no clients that need to be invoiced at this time.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				AssertEquals(true, invoiceCompleteCalled);
				AssertEquals(false, form.Visible);
			}
		}

		public void TestAutoRateAndPostInvoices_AutoRateException()
		{
			var invoiceMultiClients = new WhsInvoicePeriodicMultiClientInvoice();
			using (var form = new InvoicingFormMultiClient(invoiceMultiClients))
			{
				bool invoiceCompleteCalled = false;
				invoiceMultiClients.InvoiceCreationComplete += delegate
				{ throw new AutoRaterException("Something went wrong"); };
				var invoiceButton = GUITestHelper.FindControl<Button>(form.Controls, "AutoRateAndPostInvoicesButton");

				form.Show();
				invoiceMultiClients.InvoiceDate = ZDateTime.Today;
				invoiceButton.PerformClick();
				AssertEquals(false, invoiceMultiClients.HasErrors);
				AssertEquals("Something went wrong", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				AssertEquals(false, invoiceCompleteCalled);
				AssertEquals(true, form.Visible);
			}
		}

		public void TestInvoiceWithoutPost()
		{
			var invoiceMultiClients = new WhsInvoicePeriodicMultiClientInvoice();
			using (var form = new InvoicingFormMultiClient(invoiceMultiClients))
			{
				var invoiceCompleteCalled = false;
				invoiceMultiClients.InvoiceCreationComplete += delegate
				{ invoiceCompleteCalled = true; };
				var invoiceButton = GUITestHelper.FindControl<Button>(form.Controls, "AutoRateInvoicesButton");

				form.Show();
				invoiceMultiClients.InvoiceDate = ZDateTime.Empty;
				invoiceButton.PerformClick();
				AssertEquals(true, invoiceMultiClients.HasErrors);
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				invoiceMultiClients.InvoiceDate = ZDateTime.Today;
				invoiceButton.PerformClick();
				AssertEquals(false, invoiceMultiClients.HasErrors);
				AssertEquals("There are no clients that need to be invoiced at this time.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				AssertEquals(true, invoiceCompleteCalled);
				AssertEquals(false, form.Visible);
			}
		}

		public void TestInvoiceWithoutPost_AutoRateException()
		{
			var invoiceMultiClients = new WhsInvoicePeriodicMultiClientInvoice();
			using (var form = new InvoicingFormMultiClient(invoiceMultiClients))
			{
				var invoiceCompleteCalled = false;
				invoiceMultiClients.InvoiceCreationComplete += delegate
				{ throw new AutoRaterException("Something went wrong"); };
				var invoiceButton = GUITestHelper.FindControl<Button>(form.Controls, "AutoRateInvoicesButton");

				form.Show();
				invoiceMultiClients.InvoiceDate = ZDateTime.Today;
				invoiceButton.PerformClick();
				AssertEquals(false, invoiceMultiClients.HasErrors);
				AssertEquals("Something went wrong", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				AssertEquals(false, invoiceCompleteCalled);
				AssertEquals(true, form.Visible);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new InvoicingFormMultiClient(new WhsInvoicePeriodicMultiClientInvoice());
		}

		#endregion
	}
}

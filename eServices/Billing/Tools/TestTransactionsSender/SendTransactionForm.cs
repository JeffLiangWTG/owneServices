using System;
using System.Net;
using System.Windows.Forms;
using CargoWise.Billing.API;
using CargoWise.Billing.Client;

namespace CargoWise.eServices.Billing.TestTransactionsSender
{
	public partial class SendTransactionForm : Form
	{
		public SendTransactionForm()
		{
			InitializeComponent();

			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
			serverComboBox.SelectedIndex = 0;
		}

		void OnServerComboBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			switch (serverComboBox.SelectedIndex)
			{
				case 0: // Test
					categoryTextBox.Enabled = true;
					priceItemCodeTextBox.Enabled = true;
					clientIDTextBox.Enabled = true;
					reportingSourceTextBox.Enabled = true;
					break;
				case 1: // Production
					categoryTextBox.Enabled = false;
					categoryTextBox.Text = @"TST";
					priceItemCodeTextBox.Enabled = false;
					priceItemCodeTextBox.Text = @"TST";
					clientIDTextBox.Enabled = false;
					clientIDTextBox.Text = @"TSTTSTTST";
					reportingSourceTextBox.Enabled = false;
					reportingSourceTextBox.Text = @"TTS";
					break;
			}
		}

		void OnSendButtonClick(object sender, EventArgs e)
		{
			try
			{
				using (var client = new BillingServiceClient(GetEndpointConfigurationName()))
				{
					var transaction = CreateTransaction();
					AddLog("Sending transaction [" + transaction + "]");
					client.AddTransaction(transaction);
					AddLog("Success");
				}
			}
			catch (ValidationException ex)
			{
				AddLog("Validation Failed:");
				foreach (var error in ex.Errors)
				{
					AddLog("  " + error);
				}
			}
			catch (Exception ex)
			{
				AddLog(ex.ToString());
			}
		}

		BillingTransaction CreateTransaction()
		{
			return new BillingTransaction
			{
				BillableCount = int.Parse(billableCountTextBox.Text),
				Branch = branchTextBox.Text,
				Category = categoryTextBox.Text,
				ClientID = clientIDTextBox.Text,
				ClientNumber = clientNumberTextBox.Text,
				ClientStaffCode = clientStaffCodeTextBox.Text,
				PriceItemCode = priceItemCodeTextBox.Text,
				Reference1 = reference1TextBox.Text,
				Reference2 = reference2TextBox.Text,
				Reference3 = reference3TextBox.Text,
				Reference4 = reference4TextBox.Text,
				Reference5 = reference5TextBox.Text,
				MessageTrackingID = MessageTrackingIDTextBox.Text,
				ReportingSource = reportingSourceTextBox.Text,
				ServiceOccuredUTC = serviceOccuredUTCDateTimePicker.Value,
				Version = int.Parse(versionTextBox.Text),
			};
		}

		string GetEndpointConfigurationName()
		{
			return serverComboBox.SelectedIndex == 0
				? "test_billing_service"
				: "production_billing_service";
		}

		void AddLog(string message)
		{
			logRichTextBox.Text += message + Environment.NewLine;
		}
	}
}

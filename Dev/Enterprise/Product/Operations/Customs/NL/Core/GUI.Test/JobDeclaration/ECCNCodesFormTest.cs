using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing
{
	[TestedType(typeof(ECCNCodesForm))]
	class ECCNCodesFormTest : ZFormBasherTest
	{
		public void TestValidate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_ApplicationCode = "CDS";
				var invoice = dec.Invoices.AddNew();
				var line = invoice.InvoiceLines.AddNew();
				var eccnCode = line.ECCNCodes.AddNew();
				using (var form = new ECCNCodesFormForTest(line))
				{
					var e = new CancelEventArgs(false);
					eccnCode.CY_Code = "zzzz";
					form.OnClosing(e);
					AssertEquals("No validation errors, should not be cancelled", false, e.Cancel);

					e = new CancelEventArgs(false);
					for (var i = 0; i < 9; i++)
					{
						line.ECCNCodes.AddNew();
					}
					form.OnClosing(e);
					AssertEquals("Validation errors exists, should be cancelled", true, e.Cancel);
				}
			}
		}

		public void TestCancel()
		{
			var dec = Factory.New<JobDeclaration>();
			var line = dec.InvoiceLines.AddNew();
			using (var form = new ECCNCodesFormForTest(line))
			{
				var initialItem = line.ECCNCodes.AddNew();
				initialItem.CY_Code = "zzz";
				form.Show();
				var newItem = line.ECCNCodes.AddNew();
				newItem.CY_Code = "yyy";
				AssertEquals("Should have 2 while editing", 2, line.ECCNCodes.Count);

				form.DialogResult = DialogResult.Cancel;
				form.Close();
				AssertEquals("Should have 1 again after cancel", 1, line.ECCNCodes.Count);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var dec = Factory.New<JobDeclaration>();
			var line = dec.InvoiceLines.AddNew();
			return new ECCNCodesForm(line);
		}

		protected class ECCNCodesFormForTest : ECCNCodesForm
		{
			public ECCNCodesFormForTest(JobComInvoiceLine invoiceLine)
				: base(invoiceLine)
			{
			}

			public new void OnClosing(CancelEventArgs e)
			{
				base.OnClosing(e);
			}
		}
	}
}

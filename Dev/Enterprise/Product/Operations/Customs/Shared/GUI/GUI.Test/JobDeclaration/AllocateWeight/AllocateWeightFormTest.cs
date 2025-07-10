using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(AllocateWeightForm))]
	sealed class AllocateWeightFormTest : ZFormBasherTest
	{
		public void TestMethodDropEditShowDescriptionBox()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var methodDropEdit = form.FindSingleOrDefault<ZDropEdit>(c => c.Name == "MethodDropEdit");
				Assert("ShowDescriptionBox should be true", methodDropEdit.ShowDescriptionBox);
			}
		}

		public void TestOKButtonClick()
		{
			InvoiceHeader.JobComInvoiceLines.RemoveAll();
			var invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_NetWeight = 1;
			invoiceLine.JI_Weight = 1;
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var okButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "OKButton");
				okButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals("Information No action is taken.", UnitTestUserNotification.Instance.LastMessage.ToString().Trim());
				AllocateWeight.NetWeight = 10;
				okButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals("Information Net weight cannot be allocated because all invoice lines have net weight.", UnitTestUserNotification.Instance.LastMessage.ToString().Trim());
				AllocateWeight.NetWeight = 0;
				AllocateWeight.GrossWeight = 10;
				okButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals("Information Gross weight cannot be allocated because all invoice lines have gross weight.", UnitTestUserNotification.Instance.LastMessage.ToString().Trim());
				AllocateWeight.NetWeight = 10;
				AllocateWeight.GrossWeight = 10;
				okButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals(@"Information Net weight cannot be allocated because all invoice lines have net weight.
Gross weight cannot be allocated because all invoice lines have gross weight.", UnitTestUserNotification.Instance.LastMessage.ToString().Trim());
				AllocateWeight.OverrideExisting = true;
				AllocateWeight.AllocateWeightMethod = AllocateWeightMethodList.Codes.Price;
				okButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals("Information Weight cannot be allocated by Price because all invoice prices of the selected lines are 0.", UnitTestUserNotification.Instance.LastMessage.ToString().Trim());
				AllocateWeight.AllocateWeightMethod = AllocateWeightMethodList.Codes.Quantity;
				okButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals("Information Weight cannot be allocated by Quantity because all invoice quantities of the selected lines are 0.", UnitTestUserNotification.Instance.LastMessage.ToString().Trim());
				InvoiceHeader.InvoiceLines.Cast<BaseJobComInvoiceLine>().ForEach(c =>
				{
					c.JI_LinePrice = 1;
					c.JI_InvoiceQuantity = 1;
				});
				okButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestWeightDividedByZeroCase()
		{
			InvoiceHeader.JobComInvoiceLines.RemoveAll();
			var invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_NetWeight = 1;
			invoiceLine1.JI_Weight = 1;
			invoiceLine1.JI_LinePrice = 1;
			invoiceLine1.JI_InvoiceQuantity = 1;
			var invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_NetWeight = 0;
			invoiceLine2.JI_Weight = 0;
			invoiceLine2.JI_LinePrice = 0;
			invoiceLine2.JI_InvoiceQuantity = 0;
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var okButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "OKButton");
				AllocateWeight.NetWeight = 10;
				AllocateWeight.GrossWeight = 10;
				AllocateWeight.AllocateWeightMethod = AllocateWeightMethodList.Codes.Price;
				okButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals("Information Weight cannot be allocated by Price because all invoice prices of the selected lines are 0.", UnitTestUserNotification.Instance.LastMessage.ToString().Trim());
				AllocateWeight.AllocateWeightMethod = AllocateWeightMethodList.Codes.Quantity;
				okButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals("Information Weight cannot be allocated by Quantity because all invoice quantities of the selected lines are 0.", UnitTestUserNotification.Instance.LastMessage.ToString().Trim());
			}
		}

		protected override Form GetFormToBashCore() => new AllocateWeightForm(AllocateWeight);

		AllocateWeight allocateWeight;
		AllocateWeight AllocateWeight
		{
			get
			{
				if (allocateWeight == null)
				{
					allocateWeight = new AllocateWeight(Declaration.Factory, InvoiceHeader.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>());
				}

				return allocateWeight;
			}
		}

		BaseJobDeclaration declaration;
		BaseJobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
					declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
					Factory.Save();
				}

				return declaration;
			}
		}

		BaseJobComInvoiceHeader InvoiceHeader => Declaration.Invoices[0];
	}
}

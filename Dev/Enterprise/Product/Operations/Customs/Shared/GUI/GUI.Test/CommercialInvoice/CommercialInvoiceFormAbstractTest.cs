using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	public abstract class CommercialInvoiceFormAbstractTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestDelete()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_InvoiceNumber = "ABCDEFG";
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.CommercialInvoice);
			try
			{
				controller.ShowDeleteForm(invoice);
				var form = (CommercialInvoiceForm)controller.LastShownForm;
				IPostingButtonsProvider buttonsProvider = form;
				AssertEquals("Post button text", "&Delete", buttonsProvider.CommandButtonPost.Text);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				buttonsProvider.CommandButtonPost.PerformClick();
				AssertEquals("NOT Invoice.IsDeleted", false, invoice.IsDeleted);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				buttonsProvider.CommandButtonPost.PerformClick();
				AssertEquals("Invoice.IsDeleted", true, invoice.IsDeleted);
			}
			finally
			{
				if (controller.LastShownForm != null)
				{
					controller.LastShownForm.Dispose();
				}
			}
		}

		[RequiresSTA]
		public void TestHasChanges()
		{
			using (var form = GetNewCommercialInvoiceForm())
			{
				var invoice = form.Invoice;
				AssertEquals("Has changes", false, form.BusinessEntityForHasChanges.HasChanges);
				invoice.JobDeclaration.HasChanges = true;
				invoice.JobDeclaration.DocsAndCartage.HasChanges = true;
				AssertEquals("Has changes", false, form.BusinessEntityForHasChanges.HasChanges);
				invoice.HasChanges = true;
				AssertEquals("Has changes", true, form.BusinessEntityForHasChanges.HasChanges);
			}
		}

		[RequiresSTA]
		public void TestBusinessObjectForValidation()
		{
			using (var form = GetNewCommercialInvoiceForm())
			{
				var invoice = form.Invoice;
				AssertEquals("Correct object for validation", invoice, ((ISaveInitiator)form).BusinessEntityForValidation);
			}
		}

		public override void TestMarkAsNeedingValidationIsNotCalledWhenFormLoads()
		{
			Assert(true);
		}

		protected abstract CommercialInvoiceForm GetNewCommercialInvoiceForm();

		protected sealed override Form GetFormToBashCore()
		{
			var form = GetNewCommercialInvoiceForm();
			form.ControllerID = ControllerIDs.CommercialInvoice;
			return form;
		}

		protected void CheckColumnHasBeenRemoved(BaseInvoiceLineUserControl control, string columnName)
		{
			AssertNull("Should not have column style for " + columnName, control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(columnName));
			AssertNull("Should not have column for " + columnName, control.CustomsInvoiceLinesBoundGrid.Columns[columnName]);
		}
	}
}

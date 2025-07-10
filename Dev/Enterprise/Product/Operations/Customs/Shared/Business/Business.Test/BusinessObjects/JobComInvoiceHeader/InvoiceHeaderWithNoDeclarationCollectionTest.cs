using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderWithNoDeclarationCollection))]
	sealed class InvoiceHeaderWithNoDeclarationCollectionTest : ActiveBusinessObjectCollectionTestCase<InvoiceHeaderWithNoDeclarationCollection>
	{
		public void TestAddNotificationWhenAdditionalFilterNotMet()
		{
			var invoices = new InvoiceHeaderWithNoDeclarationCollection(Factory) as IActiveBusinessObjectCollection;
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var dec = Factory.New<BaseJobDeclaration>();
			var attachAgainError = "The selected invoice is attached to a declaration job. Please select an unattached invoice.";
			invoice.JZ_JE = dec.PK;
			ZString error = invoices.GetAllNotificationsWhenAdditionalFilterNotMet(invoice);
			AssertEquals(attachAgainError, error);
			invoice.JZ_JE = ZGuid.Empty;
			error = invoices.GetAllNotificationsWhenAdditionalFilterNotMet(invoice);
			AssertNotEquals(attachAgainError, error);
		}

		public void TestFilterBusinessObjectDefaultsNotAttachedToDeclaration()
		{
			var invoices = new InvoiceHeaderWithNoDeclarationCollection(Factory);
			var filter = invoices.FilterBusinessObjectDefaults[InvoiceHeaderWithNoDeclarationCollection.AttachedToDeclarationFilterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"];
			AssertEquals(false, filter.IsRemovable);
			AssertEquals(InvoiceHeaderWithNoDeclarationCollection.NotAttachedToDeclarationCode, filter.Value);
		}
	}
}

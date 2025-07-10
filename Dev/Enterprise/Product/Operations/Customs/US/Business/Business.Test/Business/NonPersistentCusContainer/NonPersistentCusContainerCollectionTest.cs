using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NonPersistentCusContainerCollection))]
	public class NonPersistentCusContainerCollectionTest : Customs.Business.Testing.NonPersistentCusContainerCollectionTest<NonPersistentCusContainerCollection>
	{
		protected override NonPersistentCusContainerCollection GetCollectionToTest()
		{
			return new NonPersistentCusContainerCollection(InvoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new NonPersistentCusContainer(Declaration.InvoiceLines.AddNew());
		}

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (fInvoiceLine == null)
				{
					Declaration.Invoices.AddNew();
					fInvoiceLine = Declaration.InvoiceLines.AddNew();
				}
				return fInvoiceLine;
			}
		}
		JobComInvoiceLine fInvoiceLine;
	}
}

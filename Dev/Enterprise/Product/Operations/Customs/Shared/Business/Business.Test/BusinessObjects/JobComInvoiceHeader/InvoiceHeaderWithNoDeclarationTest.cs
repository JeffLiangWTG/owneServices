using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class InvoiceHeaderWithNoDeclarationTest : TestCaseWithFactory
	{
		public void TestDoesNotLoadInvoicesAttachedToDeclarations()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice1 = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
			var invoice2 = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();

			invoice1.JZ_JE = declaration.PK;
			invoice2.JZ_JE = ZGuid.Empty;

			var collection = new InvoiceHeaderWithNoDeclarationCollection(Factory);

			AssertEquals("Only load one", 1, collection.Count);
			AssertEquals("Right one", invoice2.PK, collection[0].PK);
		}

		public void TestSetDefaultValuesForNewChild()
		{
			InvoiceHeaderWithNoDeclarationCollection coll = new InvoiceHeaderWithNoDeclarationCollection(Factory);
			AssertEquals("Incoterm defaulted", Core.Constants.IncoTerms.FreeOnBoard, coll.AddNew().JZ_IncoTerm);
		}
	}
}

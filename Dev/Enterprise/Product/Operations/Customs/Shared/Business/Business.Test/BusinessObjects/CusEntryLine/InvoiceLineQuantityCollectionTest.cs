using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class InvoiceLineQuantityCollectionTest : TestCaseWithFactory
	{
		public void TestTypedIndexer()
		{
			InvoiceLineQuantity quantity = new InvoiceLineQuantity(1, "KG");
			InvoiceLineQuantityCollection qC = new InvoiceLineQuantityCollection();
			AssertEquals("PreCondition", 0, qC.Count);
			qC.AddQuantity(quantity);
		}

		public void TestCount()
		{
			InvoiceLineQuantityCollection qC = new InvoiceLineQuantityCollection();
			AssertEquals("PreCondition", 0, qC.Count);
			qC.AddQuantity(new InvoiceLineQuantity(1, "KG"));
			AssertEquals(1, qC.Count);
		}

		public void TestQuantityAccumulation()
		{
			InvoiceLineQuantityCollection qC = new InvoiceLineQuantityCollection();
			InvoiceLineQuantity kG = new InvoiceLineQuantity(1, "KG");
			qC.AddQuantity(kG);
			qC.AddQuantity(kG);
			AssertEquals(2m, qC[0].Quantity);
		}

		public void TestMatch()
		{
			InvoiceLineQuantityCollection qC = new InvoiceLineQuantityCollection();
			InvoiceLineQuantity kG = new InvoiceLineQuantity(1, "KG");
			AssertNull(qC.FindMatch(kG));
			qC.AddQuantity(kG);
			AssertNotNull(qC.FindMatch(kG));
		}
	}
}

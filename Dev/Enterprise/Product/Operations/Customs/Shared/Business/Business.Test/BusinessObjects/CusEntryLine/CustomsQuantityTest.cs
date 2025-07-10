using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CustomsQuantityTest : TestCase
	{
		public void TestConstructor()
		{
			ZDecimal qty = 1.52m;
			ZString uQ = "KG";
			InvoiceLineQuantity cQ = new InvoiceLineQuantity(qty, uQ);
			AssertEquals(qty, cQ.Quantity);
			AssertEquals(uQ, cQ.UnitOfQuantity);
		}

		public void TestClone()
		{
			ZDecimal qty = 1.52m;
			ZString uQ = "KG";
			InvoiceLineQuantity cQ = new InvoiceLineQuantity(qty, uQ);
			InvoiceLineQuantity cQ2 = (InvoiceLineQuantity)cQ.Clone();
			AssertEquals(cQ.Quantity, cQ2.Quantity);
			AssertEquals(cQ.UnitOfQuantity, cQ2.UnitOfQuantity);
		}
	}
}

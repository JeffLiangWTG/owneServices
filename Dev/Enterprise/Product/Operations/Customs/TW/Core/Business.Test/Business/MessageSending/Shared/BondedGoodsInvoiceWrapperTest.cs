using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class BondedGoodsInvoiceWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestBondedGoodsInvoice()
		{
			IBondedGoodsInvoice bondedGoodsInvoice = new BondedGoodsInvoiceWrapper("AA", 12m);
			NUnit.Framework.Assert.That(bondedGoodsInvoice.ID, NUnit.Framework.Is.EqualTo("AA").Using(CustomComparers.TypeComparison), "BondedGoodsInvoices.ID should be");
			NUnit.Framework.Assert.That(bondedGoodsInvoice.ValueAmount, NUnit.Framework.Is.EqualTo(12m).Using(CustomComparers.TypeComparison), "BondedGoodsInvoices.ValueAmount should be");
		}
	}
}

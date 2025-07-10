using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class BondedGoodsInvoicesTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(bondedGoodsInvoice.ID, NUnit.Framework.Is.EqualTo("XXXX1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestValueAmount()
		{
			NUnit.Framework.Assert.That(bondedGoodsInvoice.ValueAmount, NUnit.Framework.Is.EqualTo(123456m).Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			var entryHeader = testHelper.CreateEntryHeaderForN5203();
			var declaration = entryHeader.Declaration;
			var gui = declaration.GovernmentUniformInvoices.AddNew();
			gui.CY_Code = "XXXX1";
			gui.Amount = 123456;
			bondedGoodsInvoice = new Consignment(entryHeader).BondedGoods.BondedGoodsInvoices.Single();
		}

		IBondedGoodsInvoice bondedGoodsInvoice;
	}
}

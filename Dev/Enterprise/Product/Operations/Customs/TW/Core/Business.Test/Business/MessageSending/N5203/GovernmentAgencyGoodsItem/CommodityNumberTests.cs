using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CommodityNumberTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(commodityNumber.ID, NUnit.Framework.Is.EqualTo("ABF1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIdentifierTypeCode()
		{
			NUnit.Framework.Assert.That(commodityNumber.IdentifierTypeCode, NUnit.Framework.Is.EqualTo("BP").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			entryHeader = testHelper.CreateEntryHeaderForN5203();
			invoiceLine = testHelper.CreateInvoiceLineForN5203(entryHeader);
			invoiceLine.JI_CustomsOwnerPartNo = "ABF1";
		}

		CusEntryHeader entryHeader;
		CusEntryLine EntryLine => entryHeader.MergedLines.Cast<CusEntryLine>().First();
		ICommodity commodity => new Commodity(EntryLine, invoiceLine);
		JobComInvoiceLine invoiceLine;
		ICommodityNumber commodityNumber => commodity.CommodityNumbers.First();
	}
}

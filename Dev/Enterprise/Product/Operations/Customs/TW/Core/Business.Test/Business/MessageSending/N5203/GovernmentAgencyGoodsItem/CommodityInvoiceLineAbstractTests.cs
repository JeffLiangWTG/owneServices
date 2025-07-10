using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class CommodityInvoiceLineAbstractTests<TCommodityInvoiceLine> : TestCaseWithFactory
		where TCommodityInvoiceLine : CommodityInvoiceLine
	{
		[ExpectNoExceptions]
		public void TestChargesTypeCode()
		{
			NUnit.Framework.Assert.That(CommodityInvoiceLine.ChargesTypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestCurrencyTypeCode()
		{
			NUnit.Framework.Assert.That(CommodityInvoiceLine.CurrencyTypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public virtual void TestUnitPriceAmount()
		{
			CombineAssertions(() =>
			{
				invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				invoiceLine.JI_LinePrice = 156.25m;
				invoiceLine.JI_InvoiceQuantity = 1m;
				NUnit.Framework.Assert.That(CommodityInvoiceLine.UnitPriceAmount, NUnit.Framework.Is.EqualTo(156.25m).Using(CustomComparers.TypeComparison));

				invoiceLine.JI_EnteredUnitPrice = 123m;
				NUnit.Framework.Assert.That(CommodityInvoiceLine.UnitPriceAmount, NUnit.Framework.Is.EqualTo(123m).Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public virtual void TestItemChargeAmount()
		{
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			EntryLine.CL_CustomsValue = 625m;
			NUnit.Framework.Assert.That(CommodityInvoiceLine.ItemChargeAmount, NUnit.Framework.Is.EqualTo(625m).Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			entryHeader = testHelper.CreateEntryHeaderForN5203();
			invoiceLine = testHelper.CreateInvoiceLineForN5203(entryHeader);
		}

		CusEntryHeader entryHeader;
		protected CusEntryLine EntryLine => entryHeader.MergedLines.Cast<CusEntryLine>().First();
		protected JobComInvoiceLine invoiceLine;
		protected abstract IInvoiceLine CommodityInvoiceLine { get; }
	}
}

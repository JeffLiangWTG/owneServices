using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusEntryLineLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestEntryLine()
		{
			var parent = Factory.New<CusEntryLine>();
			NUnit.Framework.Assert.That(parent, NUnit.Framework.Is.EqualTo(parent.Lookups.EntryLine));
		}

		[ExpectNoExceptions]
		public void TestInvoiceUQList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TWCIU", "Taiwan Commercial Pack Units");
			helper.CreateCusCodeList("TW", "TWCIU", "AMP", "Ampere", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var cusEntryLine = Factory.New<CusEntryLine>();
			var invoiceUQList = cusEntryLine.Lookups.InvoiceUQList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(invoiceUQList.Count, NUnit.Framework.Is.EqualTo(1));
				NUnit.Framework.Assert.That(invoiceUQList[0].Code, NUnit.Framework.Is.EqualTo("AMP"));
				NUnit.Framework.Assert.That(invoiceUQList[0].Description, NUnit.Framework.Is.EqualTo("Ampere"));
			});
		}
	}
}

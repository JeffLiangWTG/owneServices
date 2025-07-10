using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class InvoiceWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestProperties()
		{
			IInvoice invoice = new InvoiceWrapper("1234", ZDateTime.BrettsBirthday.Date);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(invoice.ID, NUnit.Framework.Is.EqualTo("1234").Using(CustomComparers.TypeComparison), "ID");
				NUnit.Framework.Assert.That(invoice.IssueDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.BrettsBirthday.Date), "IssueDateTime");
			});
		}
	}
}

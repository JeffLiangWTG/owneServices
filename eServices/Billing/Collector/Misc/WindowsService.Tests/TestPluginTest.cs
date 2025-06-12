using System;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.Misc.WindowsService.Tests
{
	[TestFixture]
	class TestPluginTest
	{
		[Test]
		public void TestGeneratedTransaction()
		{
			var plugin = new Plugins.Test.Plugin();
			var start = DateTime.Today;
			var end = start.AddDays(1);
			var transaction = plugin.GetTransactions(start, end).Single();
			Assert.That(transaction.TimeStamp, Is.EqualTo(end));
			Assert.That(transaction.BillingTransaction.BillableCount, Is.EqualTo(1));
			Assert.That(transaction.BillingTransaction.Branch, Is.Null);
			Assert.That(transaction.BillingTransaction.Category, Is.EqualTo("TST"));
			Assert.That(transaction.BillingTransaction.ClientID, Is.EqualTo("TSTTSTTST"));
			Assert.That(transaction.BillingTransaction.ClientNumber, Is.Null);
			Assert.That(transaction.BillingTransaction.ClientStaffCode, Is.Null);
			Assert.That(transaction.BillingTransaction.PriceItemCode, Is.EqualTo("TST"));
			Assert.That(transaction.BillingTransaction.Reference1, Is.EqualTo("TEST"));
			Assert.That(transaction.BillingTransaction.Reference2, Is.EqualTo("TEST"));
			Assert.That(transaction.BillingTransaction.Reference3, Is.EqualTo("TEST"));
			Assert.That(transaction.BillingTransaction.Reference4, Is.EqualTo("TEST"));
			Assert.That(transaction.BillingTransaction.Reference5, Is.Null);
			Assert.That(transaction.BillingTransaction.ReportingSource, Is.EqualTo("MSC"));
			Assert.That(transaction.BillingTransaction.ServiceOccuredUTC, Is.EqualTo(start));
			Assert.That(transaction.BillingTransaction.Version, Is.EqualTo(0));
			Assert.That(transaction.BillingTransaction.MessageTrackingID, Is.EqualTo("55B2D0DA-8230-43BE-83BF-5C7D5766343E"));
		}
	}
}
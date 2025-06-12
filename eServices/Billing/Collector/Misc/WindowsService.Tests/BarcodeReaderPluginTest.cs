using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.Misc.WindowsService.Tests
{
	class BarcodeReaderPluginTest : AbstracteHubAuditRequestPluginTest<Plugins.BarcodeReader.Plugin>
	{
		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>()
				{
					new object[] {new Guid("7785531C-3934-4897-828F-71637A212BD4"), "HYE-DAU-AYA", DateTime.Parse("2014-09-30 23:09:16.013"), Guid.Empty, "CW1Support", "BRD", "10.61.161.69", "INC", new Guid("2D6231EB-FC92-4883-83EE-90BCA70E668F"), ""},
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(1));
			AssertTransaction(transactions[0], "BRD", "BRD", "HYEDAUAYA", "INC", "10.61.161.69", "2d6231eb-fc92-4883-83ee-90bca70e668f", "CW1Support", DateTime.Parse("2014-09-30 23:09:16.013"));
		}
	}
}

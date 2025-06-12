using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Tests.Common;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.Misc.WindowsService.Tests
{
	class WiseCloudBillingSolarwindsOct21toApr22Test : AbstracteHubAuditRequestPluginTest<Plugins.WiseCloudBillingSolarwindsOct21toApr22.Plugin>
	{
		static DateTime MinTimeStamp { get; } = new DateTime(2021, 11, 11, 16, 4, 0);
		static DateTime MaxTimeStamp { get; } = new DateTime(2021, 11, 11, 16, 21, 0);

		protected override string[] QueryColumns
		{
			get { return new[] { "SourceIP", "DestinationIP", "EgressBytes", "MinTimeStamp", "MaxTimeStamp" }; }
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>()
				{
					new object[] { 167980431, 167981274, 404159287, MinTimeStamp, MaxTimeStamp },
					new object[] { 167980555, 167981197, 25978406085, MinTimeStamp, MaxTimeStamp }
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(1));
			AssertTransaction(transactions[0], "HOS", "#HG", "A1S???PRD", "10.3.45.143", "10.3.48.218", MaxTimeStamp.ToString("G"), null, MinTimeStamp, 404);
		}

		protected override void SetUpInternal()
		{
			base.SetUpInternal();

			var path = TestHelper.PathToFileCreatedFromEmbeddedResource(typeof(WiseCloudBillingSolarwindsOct21toApr22Test).Assembly, "SampleMappingOct21toApr22.csv");

			mockPlugin.Object.UpdateSettings(
				new PluginSettings
				{
					Parameters = new[]
					{
						new PluginParameter("ConnectionString", "Test Connection String"),
						new PluginParameter("ReferenceFilePath", path),
						new PluginParameter("ReferenceFileServerUsername", "s_billingservice"),
						new PluginParameter("ReferenceFileServerPassword", "Test"),
						new PluginParameter("ReferenceFileServerDomainName", "CORP")
					}
				}
			);
		}
	}
}

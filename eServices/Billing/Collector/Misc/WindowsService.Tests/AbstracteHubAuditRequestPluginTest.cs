using System;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;

namespace CargoWise.eServices.Billing.Collector.Misc.WindowsService.Tests
{
	abstract class AbstracteHubAuditRequestPluginTest<T> : SqlBillingTransactionsPluginTest<T>
		where T : SqlBillingTransactionsPlugin, new()
	{
		protected override string[] QueryColumns
		{
			get { return new[] { "B0_PK", "B0_LicenceCode", "B0_RequestUTC", "B0_ClientSpecifiedIdentifier", "B0_UserName", "B0_TransactionType", "B0_RequestIP", "B0_TransactionSubType", "B0_TransactionIdentifier", "B0_ClientSpecifiedIdentifierType" }; }
		}

		protected void AssertTransaction(TimeStampedTransaction transaction, string category, string priceItemCode, string clientID,
			string reference1, string reference2, string reference3, string reference4, DateTime serviceOccuredUTC, 
			int billableCount = 1, string reference5 = null)
		{
			AssertTransaction(
				transaction,
				serviceOccuredUTC,
				billableCount,
				clientID,
				null,
				null,
				category,
				priceItemCode,
				reference1,
				reference2,
				reference3,
				reference4,
				reference5,
				"MSC",
				serviceOccuredUTC,
				null);
		}

		protected void AssertUsageTransaction(TimeStampedTransaction transaction, int usageCount,
			string usageCode,
			string enterpriseCode,
			string companyCode,
			string serverCode,
			string companyName,
			string branchCode,
			string environment,
			string additionalRefs,
			DateTime serviceOccuredUTC)
		{
			AssertUsageTransaction(
				transaction,
				serviceOccuredUTC,
				usageCount,
				usageCode,
				enterpriseCode,
				companyCode,
				serverCode,
				companyName,
				branchCode,
				environment,
				additionalRefs,
				serviceOccuredUTC);
		}
	}
}

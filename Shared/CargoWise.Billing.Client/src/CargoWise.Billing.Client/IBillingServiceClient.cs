using System;
using System.Collections.Generic;
using CargoWise.Billing.API;

namespace CargoWise.Billing.Client
{
	public interface IBillingServiceClient : IDisposable
	{
		void AddTransaction(BillingTransaction transaction);

		/// <summary>
		/// Add a range of transactions.
		/// If any are invalid the entire range is rejected.
		/// </summary>
		void AddTransactionRange(IEnumerable<BillingTransaction> transaction);

		void AddUsageTransaction(UsageTransaction transaction);

		void AddUsageTransactionRange(IEnumerable<UsageTransaction> transactions);

        bool Ping();

        IEnumerable<LicenseInfo> GetLatestLicenses();

    }
}

using System;
using System.Collections.Generic;
using System.Data;

namespace CargoWise.eServices.Billing.DataAccess
{
	public interface IBillingRepository
	{
		void Add(CargoWise.Billing.API.BillingTransaction transaction);
		void AddRange(IEnumerable<CargoWise.Billing.API.BillingTransaction> transactions);
		void InsertELKResubmitTransaction(string transactionJson);
		void InsertELKResubmitTransactions(IEnumerable<string> transactions);
		void DeleteELKTransaction(Guid transactionPk);
		DataTable SelectELKTransaction(int maxTransactions);
		int CountStaging();
		DateTime? OldestSystemCreateUTCInStaging();
        bool DoesELKBacklogExceedThreshold(int threshold);
        IEnumerable<CargoWise.Billing.API.LicenseInfo> GetLatestLicenses();
        void UpdateChargeable(DateTime? utcNow = null, int firstPeriodOfNewCollection = 202110, bool performMonthlyAggregation = true, bool updateBillingCube = false);
		void SPExecution(SPExecutionParam param);
	}
}

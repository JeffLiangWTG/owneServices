using System.ServiceModel;
using CargoWise.Billing.API;

namespace CargoWise.Billing.Service
{
	[ServiceContract]
	public interface IBillingService
	{
		[OperationContract]
		[FaultContract(typeof(ValidationFault))]
		void AddTransaction(BillingTransaction transaction);

		[OperationContract]
		[FaultContract(typeof(ValidationFault))]
		void AddTransactionRange(BillingTransaction[] transactions);

		[OperationContract]
		void AddUsageTransaction(UsageTransaction transaction);

		[OperationContract]
		void AddUsageTransactionRange(UsageTransaction[] transactions);

        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        bool Ping();

        [OperationContract]
        LicenseInfo[] GetLatestLicenses();
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using CargoWise.Billing.Kafka.API;
using CargoWise.Billing.Service;
using Common.Logging;
using API = CargoWise.Billing.API;

namespace CargoWise.eServices.Billing.WcfService
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
	[UnhandledExceptionBehavior(typeof(GlobalErrorHandler))]
	public sealed class BillingService : IBillingService
	{
		public BillingService()
		{
			IBillingKafkaClient kafkaClient = null;
			IConfigurationProvider configProvider = null;
			try
			{
				kafkaClient = Global.WindsorContainer.Resolve<IBillingKafkaClient>();
				configProvider = Global.WindsorContainer.Resolve<IConfigurationProvider>();
			}
			catch (Exception e)
			{
				Logger.Error(e);
			}

			handler = new BillingHandler(kafkaClient, Logger, configProvider);
		}

        public bool Ping()
        {
            return true;
        }

        internal BillingService(BillingHandler handler)
		{
			this.handler = handler;
		}

		public void AddTransaction(BillingTransaction transaction)
		{
			var internalTransaction = Transform(transaction);
			Logger.TraceFormat("Adding transaction [{0}]", internalTransaction);
			try
			{
				handler.SubmitTransaction(internalTransaction);
			}
			catch (API.ValidationException e)
			{
				var errorsList = e.Errors.ToList();
				Logger.Debug(
					format => format(
						"Billing transaction is invalid:{0}{1}",
						Environment.NewLine,
						string.Join(Environment.NewLine, errorsList.Select(error => "  - " + error))),
					e);
				throw new FaultException<ValidationFault>(new ValidationFault(errorsList), e.Message);
			}
		}

		public void AddTransactionRange(BillingTransaction[] transactionList)
		{
			var internalList = new List<API.BillingTransaction>(transactionList.Length);

			foreach (var transaction in transactionList)
			{
				var internalTransaction = Transform(transaction);
				Logger.TraceFormat("Adding transaction [{0}]", internalTransaction);
				internalList.Add(internalTransaction);
			}

			try
			{
				handler.SubmitTransactions(internalList);
			}
			catch (API.ValidationException e)
			{
				var errorsList = e.Errors.ToList();
				Logger.Debug(
					format => format(
						"{0}:{1}{2}",
						e.Message,
						Environment.NewLine,
						string.Join(Environment.NewLine, errorsList.Select(error => "  - " + error))),
					e);
				throw new FaultException<ValidationFault>(new ValidationFault(errorsList), e.Message);
			}
		}

		public void AddUsageTransaction(UsageTransaction transaction)
		{
			var internalTransaction = Transform(transaction);
			Logger.TraceFormat("Adding usage transaction [{0}]", internalTransaction);
			handler.AddUsageTransaction(internalTransaction);
		}

                public void AddUsageTransactionRange(UsageTransaction[] transactionList)
                {
                        var internalList = new List<API.UsageTransaction>(transactionList.Length);

			foreach (var transaction in transactionList)
			{
				var internalTransaction = Transform(transaction);
				Logger.TraceFormat("Adding transaction [{0}]", internalTransaction);
				internalList.Add(internalTransaction);
			}

                        handler.AddUsageTransactions(internalList);
                }

                public LicenseInfo[] GetLatestLicenses()
                {
                        return handler.GetLatestLicenses()
                                .Select(l => new LicenseInfo
                                {
                                        EnterpriseCode = l.EnterpriseCode,
                                        DatabaseNumber = l.DatabaseNumber,
                                        ServerCode = l.ServerCode,
                                        HostedLocation = l.HostedLocation,
                                        IsActive = l.IsActive,
                                        IsTeardownInProgress = l.IsTeardownInProgress
                                })
                                .ToArray();
                }

		static API.BillingTransaction Transform(BillingTransaction transaction)
		{
			return new API.BillingTransaction
			{
				BillableCount = transaction.BillableCount,
				Branch = transaction.Branch,
				Category = transaction.Category,
				ClientID = transaction.ClientID,
				ClientNumber = transaction.ClientNumber,
				ClientStaffCode = transaction.ClientStaffCode,
				PriceItemCode = transaction.PriceItemCode,
				Reference1 = transaction.Reference1,
				Reference2 = transaction.Reference2,
				Reference3 = transaction.Reference3,
				Reference4 = transaction.Reference4,
				Reference5 = transaction.Reference5,
				ReportingSource = transaction.ReportingSource,
				ServiceOccuredUTC = transaction.ServiceOccuredUTC,
				Version = transaction.Version,
				MessageTrackingID = transaction.MessageTrackingID,
				AdditionalRefs = transaction.AdditionalRefs
			};
		}

		static API.UsageTransaction Transform(UsageTransaction transaction)
		{
			return new API.UsageTransaction
			{
				UsageCount = transaction.UsageCount,
				AdditionalRefs = transaction.AdditionalRefs,
				EnterpriseCode = transaction.EnterpriseCode,
				ServerCode = transaction.ServerCode,
				Environment = transaction.Environment,
				CompanyCode = transaction.CompanyCode,
				CompanyName = transaction.CompanyName,
				BranchCode = transaction.BranchCode,
				UsageCode = transaction.UsageCode,
				ServiceOccuredUTC = transaction.ServiceOccuredUTC,
			};
		}

		readonly BillingHandler handler;
		static readonly ILog Logger = LogManager.GetLogger(typeof (BillingService));
	}
}

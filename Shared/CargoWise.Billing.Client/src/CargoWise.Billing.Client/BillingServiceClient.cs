using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using CargoWise.Billing.API;
using CargoWise.Billing.Client.BillingServiceReference;
using BillingTransaction = CargoWise.Billing.API.BillingTransaction;
using UsageTransaction = CargoWise.Billing.API.UsageTransaction;
using WebServiceBillingClient = CargoWise.Billing.Client.BillingServiceReference.BillingServiceClient;
using WebServiceBillingTransaction = CargoWise.Billing.Client.BillingServiceReference.BillingTransaction;
using WebServiceUsageTransaction = CargoWise.Billing.Client.BillingServiceReference.UsageTransaction;

namespace CargoWise.Billing.Client
{
	public class BillingServiceClient : IBillingServiceClient
	{
		static BillingServiceClient()
		{
			ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
		}

#if NETFRAMEWORK

		public BillingServiceClient()
			: this("billing_service")
		{
		}

		public BillingServiceClient(string endpointConfigurationName)
		{
			this.endpointConfigurationName = endpointConfigurationName;
		}

		readonly string endpointConfigurationName;

#endif

#if NETSTANDARD

		public BillingServiceClient(string remoteAddress)
		{
			this.remoteAddress = remoteAddress;
		}

		readonly string remoteAddress;

#endif

		public void AddTransaction(BillingTransaction transaction)
		{
			try
			{
				BillingTransactionValidator.ValidateTransaction(transaction);
				WebServiceClient.AddTransaction(TransformTransaction(transaction));
			}
			catch (FaultException<ValidationFault> e)
			{
				AbortWebServiceClient();
				throw new ValidationException(e.Message, e.Detail.Errors, e);
			}
			catch (Exception)
			{
				AbortWebServiceClient();
				throw;
			}
		}

		/// <summary>
		/// Add a range of transactions.
		/// If any are invalid the entire range is rejected.
		/// </summary>
		public void AddTransactionRange(IEnumerable<BillingTransaction> transactions)
		{
			try
			{
				BillingTransactionValidator.ValidateTransactions(transactions);
				int n = transactions.Count();
				var webTransactions = new WebServiceBillingTransaction[n];
				int i = 0;
				foreach (var transaction in transactions)
				{
					webTransactions[i++] = TransformTransaction(transaction);
				}
				WebServiceClient.AddTransactionRange(webTransactions);
			}
			catch (FaultException<ValidationFault> e)
			{
				AbortWebServiceClient();
				throw new ValidationException(e.Message, e.Detail.Errors, e);
			}
			catch (Exception)
			{
				AbortWebServiceClient();
				throw;
			}
		}

		public void AddUsageTransaction(UsageTransaction transaction)
		{
			try
			{
				WebServiceClient.AddUsageTransaction(TransformUsageTransaction(transaction));
			}
			catch (FaultException<ValidationFault> e)
			{
				AbortWebServiceClient();
				throw new ValidationException(e.Message, e.Detail.Errors, e);
			}
			catch (Exception)
			{
				AbortWebServiceClient();
				throw;
			}
		}

		public void AddUsageTransactionRange(IEnumerable<UsageTransaction> transactions)
		{
			try
			{
				int n = transactions.Count();
				var webTransactions = new WebServiceUsageTransaction[n];
				int i = 0;
				foreach (var transaction in transactions)
				{
					webTransactions[i++] = TransformUsageTransaction(transaction);
				}
				WebServiceClient.AddUsageTransactionRange(webTransactions);
			}
			catch (FaultException<ValidationFault> e)
			{
				AbortWebServiceClient();
				throw new ValidationException(e.Message, e.Detail.Errors, e);
			}
			catch (Exception)
			{
				AbortWebServiceClient();
				throw;
			}
		}

		public void Dispose()
		{
			try
			{
				CloseWebServiceClient();
			}
			catch (Exception)
			{
				AbortWebServiceClient();
			}
		}

		IBillingService WebServiceClient
		{
			get { return client ?? (client = CreateWebServiceClient()); }
		}

		void CloseWebServiceClient()
		{
			var communicationObject = client as ICommunicationObject;
			if (communicationObject != null)
				communicationObject.Close();
			client = null;
		}

		void AbortWebServiceClient()
		{
			var communicationObject = client as ICommunicationObject;
			if (communicationObject != null)
				communicationObject.Abort();
			client = null;
		}

		public bool Ping()
		{
			try
			{
				return WebServiceClient.Ping();
			}
			catch
			{
				return false;
			}
		}

#if NETFRAMEWORK

		internal virtual IBillingService CreateWebServiceClient()
		{
			if (string.IsNullOrEmpty(endpointConfigurationName))
			{
				throw new InvalidOperationException($"{nameof(endpointConfigurationName)} should be specified.");
			}

			return new WebServiceBillingClient(endpointConfigurationName);
		}

#endif

#if NETSTANDARD 

		internal virtual IBillingService CreateWebServiceClient()
		{
			if (string.IsNullOrEmpty(remoteAddress))
			{
				throw new InvalidOperationException($"{nameof(remoteAddress)} should be specified.");
			}

			return new WebServiceBillingClient(GetBasicHttpBinding(), new EndpointAddress(remoteAddress));
		}

		static System.ServiceModel.Channels.Binding GetBasicHttpBinding()
		{
			var result = new BasicHttpBinding
			{
				MaxBufferSize = int.MaxValue,
				ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max,
				MaxReceivedMessageSize = int.MaxValue,
				AllowCookies = true,
				Security = { Mode = BasicHttpSecurityMode.Transport }
			};
			return result;
		}

#endif

#if NETCOREAPP
        public BillingServiceClient(string remoteAddress)
        {
            this.remoteAddress = remoteAddress;
        }

        protected BillingServiceClient() { }

        readonly string remoteAddress;

        internal virtual IBillingService CreateWebServiceClient()
        {
            if (string.IsNullOrEmpty(remoteAddress))
            {
                throw new InvalidOperationException($"{nameof(remoteAddress)} should be specified.");
            }

            return new WebServiceBillingClient(GetBasicHttpBinding(), new EndpointAddress(remoteAddress));
        }

        static System.ServiceModel.Channels.Binding GetBasicHttpBinding()
        {
            var result = new BasicHttpBinding
            {
                MaxBufferSize = int.MaxValue,
                ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max,
                MaxReceivedMessageSize = int.MaxValue,
                AllowCookies = true,
                Security = { Mode = BasicHttpSecurityMode.Transport } // HTTPS
            };
            return result;
        }
#endif

		static WebServiceBillingTransaction TransformTransaction(BillingTransaction transaction)
		{
			return new WebServiceBillingTransaction
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

		static WebServiceUsageTransaction TransformUsageTransaction(UsageTransaction transaction)
		{
			return new WebServiceUsageTransaction
			{
				UsageCount = transaction.UsageCount,
				EnterpriseCode = transaction.EnterpriseCode,
				ServerCode = transaction.ServerCode,
				CompanyCode = transaction.CompanyCode,
				BranchCode = transaction.BranchCode,
				CompanyName = transaction.CompanyName,
				Environment = transaction.Environment,
				UsageCode = transaction.UsageCode,
				ServiceOccuredUTC = transaction.ServiceOccuredUTC,
				AdditionalRefs = transaction.AdditionalRefs
			};
		}

		IBillingService client;
	}
}

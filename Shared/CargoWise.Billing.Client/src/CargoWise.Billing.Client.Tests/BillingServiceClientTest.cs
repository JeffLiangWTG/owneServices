using System;
using System.Collections.Generic;
using System.ServiceModel;
using CargoWise.Billing.API;
using CargoWise.Billing.Tests.Common;
using Moq;
using IBillingService = CargoWise.Billing.Client.BillingServiceReference.IBillingService;
using ValidationFault = CargoWise.Billing.Client.BillingServiceReference.ValidationFault;
using WebServiceBillingTransaction = CargoWise.Billing.Client.BillingServiceReference.BillingTransaction;
using WebServiceUsageTransaction = CargoWise.Billing.Client.BillingServiceReference.UsageTransaction;

namespace CargoWise.Billing.Client.Tests
{
	[TestFixture]
	class BillingServiceClientTest
	{
		[Test]
		public void TestAddTransaction()
		{
			var transactions = new List<WebServiceBillingTransaction>();
			var mockWebServiceBillingClient = new Mock<IBillingService>();
			mockWebServiceBillingClient
				.Setup(_ => _.AddTransaction(It.IsAny<WebServiceBillingTransaction>()))
				.Callback<WebServiceBillingTransaction>(t => transactions.Add(t));
			var mockWebServiceBillingClientCommunicationObject = mockWebServiceBillingClient.As<ICommunicationObject>();
			var mockBillingClient = new Mock<BillingServiceClient>();
			mockBillingClient.Setup(_ => _.CreateWebServiceClient()).Returns(mockWebServiceBillingClient.Object);

			var transaction1 = GetValidTransaction();
			transaction1.Reference1 = "Transaction 1";
			mockBillingClient.Object.AddTransaction(transaction1);
			var transaction2 = GetValidTransaction();
			transaction2.Reference1 = "Transaction 2";
			mockBillingClient.Object.AddTransaction(transaction2);
			mockBillingClient.Object.Dispose();

			Assert.That(transactions.Count, Is.EqualTo(2));
			ObjectComparer.AssertPropertiesAreEqual(transaction1, transactions[0]);
            ObjectComparer.AssertPropertiesAreEqual(transaction2, transactions[1]);
			mockBillingClient.Verify(_ => _.CreateWebServiceClient(), Times.Once);
			mockWebServiceBillingClientCommunicationObject.Verify(_ => _.Close(), Times.Once);
		}

		[Test]
		public void TestAddUsageTransaction()
		{
			var transactions = new List<WebServiceUsageTransaction>();
			var mockWebServiceBillingClient = new Mock<IBillingService>();
			mockWebServiceBillingClient
				.Setup(_ => _.AddUsageTransaction(It.IsAny<WebServiceUsageTransaction>()))
				.Callback<WebServiceUsageTransaction>(t => transactions.Add(t));
			var mockWebServiceBillingClientCommunicationObject = mockWebServiceBillingClient.As<ICommunicationObject>();
			var mockBillingClient = new Mock<BillingServiceClient>();
			mockBillingClient.Setup(_ => _.CreateWebServiceClient()).Returns(mockWebServiceBillingClient.Object);

			var transaction1 = GetUsageTransaction("Test1");
			mockBillingClient.Object.AddUsageTransaction(transaction1);
			var transaction2 = GetUsageTransaction("Test2");
			mockBillingClient.Object.AddUsageTransaction(transaction2);
			mockBillingClient.Object.Dispose();

			Assert.That(transactions.Count, Is.EqualTo(2));
			ObjectComparer.AssertPropertiesAreEqual(transaction1, transactions[0]);
			ObjectComparer.AssertPropertiesAreEqual(transaction2, transactions[1]);
			mockBillingClient.Verify(_ => _.CreateWebServiceClient(), Times.Once);
			mockWebServiceBillingClientCommunicationObject.Verify(_ => _.Close(), Times.Once);
		}

		[Test]
		public void TestAddTransactionValidationFault()
		{
			const string errorMessage = "Some Error.";
			var errors = new[] { "Validation Error 1", "Validation Error 2" };

			var mockWebServiceBillingClient1 = new Mock<IBillingService>();
#if NETFRAMEWORK
			mockWebServiceBillingClient1
				.Setup(_ => _.AddTransaction(It.IsAny<WebServiceBillingTransaction>()))
				.Throws(new FaultException<ValidationFault>(new ValidationFault { Errors = errors }, new FaultReason(errorMessage)));
#else
			mockWebServiceBillingClient1
				.Setup(_ => _.AddTransaction(It.IsAny<WebServiceBillingTransaction>()))
				.Throws(new FaultException<ValidationFault>(
					new ValidationFault { Errors = errors },
					new FaultReason(errorMessage),
					new FaultCode("Sender"),
					null));
#endif
			var mockWebServiceBillingClientCommunicationObject1 = mockWebServiceBillingClient1.As<ICommunicationObject>();

			var mockWebServiceBillingClient2 = new Mock<IBillingService>();
			var mockWebServiceBillingClientCommunicationObject2 = mockWebServiceBillingClient2.As<ICommunicationObject>();

			var webServiceBillingClients = new Queue<IBillingService>();
			webServiceBillingClients.Enqueue(mockWebServiceBillingClient1.Object);
			webServiceBillingClients.Enqueue(mockWebServiceBillingClient2.Object);

			var mockBillingClient = new Mock<BillingServiceClient>();
			mockBillingClient.Setup(_ => _.CreateWebServiceClient()).Returns(webServiceBillingClients.Dequeue);

			// First call
			var exception = Assert.Throws<ValidationException>(delegate { mockBillingClient.Object.AddTransaction(GetValidTransaction()); });
			Assert.That(exception.Message, Is.EqualTo(errorMessage));
			Assert.That(exception.Errors, Is.EquivalentTo(errors));
			mockWebServiceBillingClientCommunicationObject1.Verify(_ => _.Abort(), Times.Once);

			// Second call
			mockBillingClient.Object.AddTransaction(GetValidTransaction());
			mockWebServiceBillingClient2.Verify(_ => _.AddTransaction(It.IsAny<WebServiceBillingTransaction>()), Times.Once);

			mockBillingClient.Object.Dispose();
			mockWebServiceBillingClientCommunicationObject2.Verify(_ => _.Close(), Times.Once);
		}

		[Test]
		public void TestAddTransactionGeneralException()
		{
			var exception = new Exception("General exception");

			var mockWebServiceBillingClient1 = new Mock<IBillingService>();
			mockWebServiceBillingClient1
				.Setup(_ => _.AddTransaction(It.IsAny<WebServiceBillingTransaction>()))
				.Throws(exception);
			var mockWebServiceBillingClientCommunicationObject1 = mockWebServiceBillingClient1.As<ICommunicationObject>();

			var mockWebServiceBillingClient2 = new Mock<IBillingService>();
			var mockWebServiceBillingClientCommunicationObject2 = mockWebServiceBillingClient2.As<ICommunicationObject>();

			var webServiceBillingClients = new Queue<IBillingService>();
			webServiceBillingClients.Enqueue(mockWebServiceBillingClient1.Object);
			webServiceBillingClients.Enqueue(mockWebServiceBillingClient2.Object);

			var mockBillingClient = new Mock<BillingServiceClient>();
			mockBillingClient.Setup(_ => _.CreateWebServiceClient()).Returns(webServiceBillingClients.Dequeue);

			// First call
			var thrownException = Assert.Throws<Exception>(delegate { mockBillingClient.Object.AddTransaction(GetValidTransaction()); });
			Assert.That(thrownException, Is.EqualTo(exception));
			mockWebServiceBillingClientCommunicationObject1.Verify(_ => _.Abort(), Times.Once);

			// Second call
			mockBillingClient.Object.AddTransaction(GetValidTransaction());
			mockWebServiceBillingClient2.Verify(_ => _.AddTransaction(It.IsAny<WebServiceBillingTransaction>()), Times.Once);

			mockBillingClient.Object.Dispose();
			mockWebServiceBillingClientCommunicationObject2.Verify(_ => _.Close(), Times.Once);
		}

		[Test]
		public void TestAddTransactionRange()
		{
			var transactions = new List<WebServiceBillingTransaction>();
			var mockWebServiceBillingClient = new Mock<IBillingService>();
			mockWebServiceBillingClient
				.Setup(_ => _.AddTransactionRange(It.IsAny<WebServiceBillingTransaction[]>()))
				.Callback<WebServiceBillingTransaction[]>(t => transactions.AddRange(t));
			var mockWebServiceBillingClientCommunicationObject = mockWebServiceBillingClient.As<ICommunicationObject>();
			var mockBillingClient = new Mock<BillingServiceClient>();
			mockBillingClient.Setup(_ => _.CreateWebServiceClient()).Returns(mockWebServiceBillingClient.Object);

			var transaction1 = GetValidTransaction();
			transaction1.Reference1 = "Transaction 1";
			var transaction2 = GetValidTransaction();
			transaction2.Reference1 = "Transaction 2";

			mockBillingClient.Object.AddTransactionRange(new[] { transaction1, transaction2 });
			mockBillingClient.Object.Dispose();

			Assert.That(transactions.Count, Is.EqualTo(2));
			ObjectComparer.AssertPropertiesAreEqual(transaction1, transactions[0]);
			ObjectComparer.AssertPropertiesAreEqual(transaction2, transactions[1]);
			mockBillingClient.Verify(_ => _.CreateWebServiceClient(), Times.Once);
			mockWebServiceBillingClientCommunicationObject.Verify(_ => _.Close(), Times.Once);
		}

		[Test]
		public void TestAddUsageTransactionRange()
		{
			var transactions = new List<WebServiceUsageTransaction>();
			var mockWebServiceBillingClient = new Mock<IBillingService>();
			mockWebServiceBillingClient
				.Setup(_ => _.AddUsageTransactionRange(It.IsAny<WebServiceUsageTransaction[]>()))
				.Callback<WebServiceUsageTransaction[]>(t => transactions.AddRange(t));
			var mockWebServiceBillingClientCommunicationObject = mockWebServiceBillingClient.As<ICommunicationObject>();
			var mockBillingClient = new Mock<BillingServiceClient>();
			mockBillingClient.Setup(_ => _.CreateWebServiceClient()).Returns(mockWebServiceBillingClient.Object);

			var transaction1 = GetUsageTransaction("Test1");
			var transaction2 = GetUsageTransaction("Test2");

			mockBillingClient.Object.AddUsageTransactionRange(new[] { transaction1, transaction2 });
			mockBillingClient.Object.Dispose();

			Assert.That(transactions.Count, Is.EqualTo(2));
			ObjectComparer.AssertPropertiesAreEqual(transaction1, transactions[0]);
			ObjectComparer.AssertPropertiesAreEqual(transaction2, transactions[1]);
			mockBillingClient.Verify(_ => _.CreateWebServiceClient(), Times.Once);
			mockWebServiceBillingClientCommunicationObject.Verify(_ => _.Close(), Times.Once);
		}

		[Test]
		public void TestAddTransactionRangeValidationFault()
		{
			const string errorMessage = "Some Error.";
			var errors = new[] { "Validation Error 1", "Validation Error 2" };

			var mockWebServiceBillingClient1 = new Mock<IBillingService>();
#if NETFRAMEWORK
			mockWebServiceBillingClient1
				.Setup(_ => _.AddTransactionRange(It.IsAny<WebServiceBillingTransaction[]>()))
				.Throws(new FaultException<ValidationFault>(new ValidationFault { Errors = errors }, new FaultReason(errorMessage)));
#else
			mockWebServiceBillingClient1
				.Setup(_ => _.AddTransactionRange(It.IsAny<WebServiceBillingTransaction[]>()))
				.Throws(new FaultException<ValidationFault>(
					new ValidationFault { Errors = errors },
					new FaultReason(errorMessage),
					new FaultCode("Sender"),
					null));
#endif
			var mockWebServiceBillingClientCommunicationObject1 = mockWebServiceBillingClient1.As<ICommunicationObject>();

			var mockWebServiceBillingClient2 = new Mock<IBillingService>();
			var mockWebServiceBillingClientCommunicationObject2 = mockWebServiceBillingClient2.As<ICommunicationObject>();

			var webServiceBillingClients = new Queue<IBillingService>();
			webServiceBillingClients.Enqueue(mockWebServiceBillingClient1.Object);
			webServiceBillingClients.Enqueue(mockWebServiceBillingClient2.Object);

			var mockBillingClient = new Mock<BillingServiceClient>();
			mockBillingClient.Setup(_ => _.CreateWebServiceClient()).Returns(webServiceBillingClients.Dequeue);

			// First call
			var exception = Assert.Throws<ValidationException>(delegate { mockBillingClient.Object.AddTransactionRange(new[] { GetValidTransaction() }); });
			Assert.That(exception.Message, Is.EqualTo(errorMessage));
			Assert.That(exception.Errors, Is.EquivalentTo(errors));
			mockWebServiceBillingClientCommunicationObject1.Verify(_ => _.Abort(), Times.Once);

			// Second call
			mockBillingClient.Object.AddTransactionRange(new[] { GetValidTransaction() });
			mockWebServiceBillingClient2.Verify(_ => _.AddTransactionRange(It.IsAny<WebServiceBillingTransaction[]>()), Times.Once);

			mockBillingClient.Object.Dispose();
			mockWebServiceBillingClientCommunicationObject2.Verify(_ => _.Close(), Times.Once);
		}

		[Test]
		public void TestAddTransactionRangeGeneralException()
		{
			var exception = new Exception("General exception");
			var mockWebServiceBillingClient1 = new Mock<IBillingService>();
			mockWebServiceBillingClient1
				.Setup(_ => _.AddTransactionRange(It.IsAny<WebServiceBillingTransaction[]>()))
				.Throws(exception);
			var mockWebServiceBillingClientCommunicationObject1 = mockWebServiceBillingClient1.As<ICommunicationObject>();

			var mockWebServiceBillingClient2 = new Mock<IBillingService>();
			var mockWebServiceBillingClientCommunicationObject2 = mockWebServiceBillingClient2.As<ICommunicationObject>();

			var webServiceBillingClients = new Queue<IBillingService>();
			webServiceBillingClients.Enqueue(mockWebServiceBillingClient1.Object);
			webServiceBillingClients.Enqueue(mockWebServiceBillingClient2.Object);

			var mockBillingClient = new Mock<BillingServiceClient>();
			mockBillingClient.Setup(_ => _.CreateWebServiceClient()).Returns(webServiceBillingClients.Dequeue);

			// First call
			var thrownException = Assert.Throws<Exception>(delegate { mockBillingClient.Object.AddTransactionRange(new[] { GetValidTransaction() }); });
			Assert.That(thrownException, Is.EqualTo(exception));
			mockWebServiceBillingClientCommunicationObject1.Verify(_ => _.Abort(), Times.Once);

			// Second call
			mockBillingClient.Object.AddTransactionRange(new[] { GetValidTransaction() });
			mockWebServiceBillingClient2.Verify(_ => _.AddTransactionRange(It.IsAny<WebServiceBillingTransaction[]>()), Times.Once);

			mockBillingClient.Object.Dispose();
			mockWebServiceBillingClientCommunicationObject2.Verify(_ => _.Close(), Times.Once);
		}

		static API.BillingTransaction GetValidTransaction()
		{
			return new API.BillingTransaction
			{
				BillableCount = 2,
				ClientID = "ABCDEFXYZ",
				ClientNumber = "98765432100123456789",
				ClientStaffCode = "ABC",
				Category = "TST",
				PriceItemCode = "DEF",
				Reference1 = "REFERENCE 1",
				Reference2 = "REFERENCE 2",
				Reference3 = "REFERENCE 3",
				ReportingSource = "XYZ",
				ServiceOccuredUTC = DateTime.UtcNow,
				Version = 1,
				MessageTrackingID = "55B2D0DA-8230-43BE-83BF-5C7D5766343E",
			};
		}
		static API.UsageTransaction GetUsageTransaction(string companyName = "Test")
		{
			return new UsageTransaction
			{
				BranchCode = "KLM",
				CompanyCode = "ABC",
				CompanyName = companyName,
				EnterpriseCode = "TST",
				Environment = "TST",
				ServerCode = "TST",
				UsageCode = "USG",
				UsageCount = 2,
				ServiceOccuredUTC = DateTime.UtcNow,
				AdditionalRefs = ""
			};
		}
	}
}

using System;
using System.Data.SqlClient;
using System.IO;
using Microsoft.BizTalk.Message.Interop;
using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.DataAccess.Tests.Sql
{
	public class SubscriptionAccessorTests
	{
		[Test]
		public void SelectSubscriptionByPartiesMessage_ConvertsFromSharedTypes()
		{
			Assert.Multiple(() =>
			{
				var subscriptions = new[]
				{
					new eServices.eHubDataAccess.Integration.SubscriptionInfo
					{
						ID = Guid.NewGuid(),
						Type = "type",
						Provider = "provider",
						Subscriber = "subscriber",
						Value = "value",
						Reference = "reference",
						ReferenceType = "referenceType",
						Subscribed = DateTime.Now,
						Expiry = DateTime.Now.AddDays(1)
					}
				};

				var message = new Mock<IBaseMessage>();
				var stream = new MemoryStream();
				message.Setup(x => x.BodyPart.Data).Returns(stream);

				var sharedSubscriptionAccessor = new Mock<eServices.eHubDataAccess.Integration.ISubscriptionAccessor>();
				sharedSubscriptionAccessor.Setup(x =>
					x.SelectSubscriptions(It.IsAny<Func<string, object>>(), It.IsAny<Stream>()))
						.Returns(subscriptions);
				sharedSubscriptionAccessor.Setup(x =>
					x.SelectSubscriptions(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
						.Returns(subscriptions);

				var eHubSubscriptionAccessor = new CargoWise.eHub.DataAccess.Sql.SubscriptionAccessor(sharedSubscriptionAccessor.Object);
				Assert.DoesNotThrow(() => _ = eHubSubscriptionAccessor.SelectSubscriptions(message.Object));
				Assert.DoesNotThrow(() => _ = eHubSubscriptionAccessor.SelectSubscriptions("subscriptionType", new[] { "provider0", "provider1" }));
			});
		}

		[Test]
		public void IsAutoSubscriptionRequired_CallsSharedDataAccess()
		{
			var sharedSubscriptionAccessor = new Mock<eServices.eHubDataAccess.Integration.ISubscriptionAccessor>();
			var eHubSubscriptionAccessor = new CargoWise.eHub.DataAccess.Sql.SubscriptionAccessor(sharedSubscriptionAccessor.Object);
			var message = new Mock<IBaseMessage>().Object;
			eHubSubscriptionAccessor
				.IsAutoSubscriptionRequired(message);
			sharedSubscriptionAccessor.Verify(x => x
				.IsAutoSubscriptionRequired(It.IsAny<Func<string, object>>()), Times.Once);
		}

		[Test]
		public void InsertSubscribedClients_STPK_CallsSharedDataAccess()
		{
			var sharedSubscriptionAccessor = new Mock<eServices.eHubDataAccess.Integration.ISubscriptionAccessor>();
			var eHubSubscriptionAccessor = new CargoWise.eHub.DataAccess.Sql.SubscriptionAccessor(sharedSubscriptionAccessor.Object);
			string[] recipientIds = { "recipient1", "recipient2" };
			eHubSubscriptionAccessor
				.InsertSubscribedClients((SqlTransaction)null, Guid.Empty, "senderId", recipientIds, "value", "reference", "referenceType");
			sharedSubscriptionAccessor.Verify(x => x
				.InsertSubscribedClients((SqlTransaction)null, Guid.Empty, "senderId", recipientIds, "value", "reference", "referenceType"), Times.Once);
		}

		[Test]
		public void InsertSubscribedClients_STID_CallsSharedDataAccess()
		{
			var sharedSubscriptionAccessor = new Mock<eServices.eHubDataAccess.Integration.ISubscriptionAccessor>();
			var eHubSubscriptionAccessor = new CargoWise.eHub.DataAccess.Sql.SubscriptionAccessor(sharedSubscriptionAccessor.Object);
			string[] recipientIds = { "recipient1", "recipient2" };
			eHubSubscriptionAccessor
				.InsertSubscribedClients((SqlTransaction)null, "subscriptionTypeID", "senderId", recipientIds, "value", "reference", "referenceType");
			sharedSubscriptionAccessor.Verify(x => x
				.InsertSubscribedClients((SqlTransaction)null, "subscriptionTypeID", "senderId", recipientIds, "value", "reference", "referenceType"), Times.Once);
		}

		[Test]
		public void InsertAutoSubscriptionsForSender_CallsSharedDataAccess()
		{
			var sharedSubscriptionAccessor = new Mock<eServices.eHubDataAccess.Integration.ISubscriptionAccessor>();
			var eHubSubscriptionAccessor = new CargoWise.eHub.DataAccess.Sql.SubscriptionAccessor(sharedSubscriptionAccessor.Object);
			var message = new Mock<IBaseMessage>();
			var stream = new MemoryStream();
			message.Setup(x => x.BodyPart.Data).Returns(stream);
			eHubSubscriptionAccessor
				.InsertAutoSubscriptionsForSender(message.Object);
			sharedSubscriptionAccessor.Verify(x => x
				.InsertAutoSubscriptionsForSender(It.IsAny<Func<string, object>>(), stream), Times.Once);
		}

		[Test]
		public void SelectSubscribedClients_CallsSharedDataAccess()
		{
			var sharedSubscriptionAccessor = new Mock<eServices.eHubDataAccess.Integration.ISubscriptionAccessor>();
			var eHubSubscriptionAccessor = new CargoWise.eHub.DataAccess.Sql.SubscriptionAccessor(sharedSubscriptionAccessor.Object);
			var message = new Mock<IBaseMessage>();
			var stream = new MemoryStream();
			message.Setup(x => x.BodyPart.Data).Returns(stream);
			eHubSubscriptionAccessor
				.SelectSubscribedClients(message.Object);
			sharedSubscriptionAccessor.Verify(x => x
				.SelectSubscribedClients(It.IsAny<Func<string, object>>(), stream), Times.Once);
		}

		[Test]
		public void SelectSubscriptions_CallsSharedDataAccess()
		{
			var sharedSubscriptionAccessor = new Mock<eServices.eHubDataAccess.Integration.ISubscriptionAccessor>();
			var eHubSubscriptionAccessor = new CargoWise.eHub.DataAccess.Sql.SubscriptionAccessor(sharedSubscriptionAccessor.Object);
			eHubSubscriptionAccessor
				.SelectSubscriptions("subscriptionType", "senderId", "value");
			sharedSubscriptionAccessor.Verify(x => x
				.SelectSubscriptions("subscriptionType", "senderId", "value"), Times.Once);
		}

		[Test]
		public void SelectSubscriptionsByValue_CallsSharedDataAccess()
		{
			var sharedSubscriptionAccessor = new Mock<eServices.eHubDataAccess.Integration.ISubscriptionAccessor>();
			var eHubSubscriptionAccessor = new CargoWise.eHub.DataAccess.Sql.SubscriptionAccessor(sharedSubscriptionAccessor.Object);
			eHubSubscriptionAccessor
				.SelectSubscriptionsByValue("subscriptionType", "value", "referenceType");
			sharedSubscriptionAccessor.Verify(x => x
				.SelectSubscriptionsByValue("subscriptionType", "value", "referenceType"), Times.Once);
		}

		[Test]
		public void SelectSubscriptions_Transaction_CallsSharedDataAccess()
		{
			var sharedSubscriptionAccessor = new Mock<eServices.eHubDataAccess.Integration.ISubscriptionAccessor>();
			var eHubSubscriptionAccessor = new CargoWise.eHub.DataAccess.Sql.SubscriptionAccessor(sharedSubscriptionAccessor.Object);
			var message = new Mock<IBaseMessage>();
			var stream = new MemoryStream();
			message.Setup(x => x.BodyPart.Data).Returns(stream);
			eHubSubscriptionAccessor
				.SelectSubscriptions(message.Object);
			sharedSubscriptionAccessor.Verify(x => x
				.SelectSubscriptions(It.IsAny<Func<string, object>>(), stream), Times.Once);
		}

		[Test]
		public void SelectSubscribedClients_Transaction_CallsSharedDataAccess()
		{
			var sharedSubscriptionAccessor = new Mock<eServices.eHubDataAccess.Integration.ISubscriptionAccessor>();
			var eHubSubscriptionAccessor = new CargoWise.eHub.DataAccess.Sql.SubscriptionAccessor(sharedSubscriptionAccessor.Object);
			eHubSubscriptionAccessor
				.SelectSubscribedClients((SqlTransaction)null, "senderId", "subscriptionType", "value");
			sharedSubscriptionAccessor.Verify(x => x
				.SelectSubscribedClients((SqlTransaction)null, "senderId", "subscriptionType", "value"), Times.Once);
		}

		[Test]
		public void SelectSubscribedClients_Transaction_Reference_CallsSharedDataAccess()
		{
			var sharedSubscriptionAccessor = new Mock<eServices.eHubDataAccess.Integration.ISubscriptionAccessor>();
			var eHubSubscriptionAccessor = new CargoWise.eHub.DataAccess.Sql.SubscriptionAccessor(sharedSubscriptionAccessor.Object);
			eHubSubscriptionAccessor
				.SelectSubscribedClients((SqlTransaction)null, "senderId", "subscriptionType", "value", "reference");
			sharedSubscriptionAccessor.Verify(x => x
				.SelectSubscribedClients((SqlTransaction)null, "senderId", "subscriptionType", "value", "reference"), Times.Once);
		}

		[Test]
		public void SelectUSCustomsClientsForFilerCode_CallsSharedDataAccess()
		{
			var sharedSubscriptionAccessor = new Mock<eServices.eHubDataAccess.Integration.ISubscriptionAccessor>();
			var eHubSubscriptionAccessor = new CargoWise.eHub.DataAccess.Sql.SubscriptionAccessor(sharedSubscriptionAccessor.Object);
			eHubSubscriptionAccessor
				.SelectUSCustomsClientsForFilerCode("filerCode", true);
			sharedSubscriptionAccessor.Verify(x => x
				.SelectUSCustomsClientsForFilerCode("filerCode", true), Times.Once);
		}

		[Test]
		public void SelectSubscriptions_Providers_CallsSharedDataAccess()
		{
			var sharedSubscriptionAccessor = new Mock<eServices.eHubDataAccess.Integration.ISubscriptionAccessor>();
			var eHubSubscriptionAccessor = new CargoWise.eHub.DataAccess.Sql.SubscriptionAccessor(sharedSubscriptionAccessor.Object);
			string[] providerIDs = { "provider1", "provider2" };
			eHubSubscriptionAccessor
				.SelectSubscriptions("subscriptionType", providerIDs, "value", "reference", "referenceType");
			sharedSubscriptionAccessor.Verify(x => x
				.SelectSubscriptions("subscriptionType", providerIDs, "value", "reference", "referenceType"), Times.Once);
		}
	}
}

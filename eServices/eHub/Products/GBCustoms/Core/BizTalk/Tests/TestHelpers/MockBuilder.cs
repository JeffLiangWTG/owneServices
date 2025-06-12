using System;
using System.Linq;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.Core.BT.Tests.TestHelpers
{
	internal static class MockBuilder
	{
		public static eHubTransactionsContext WithRegistrationType(
			this eHubTransactionsContext mockContext,
			Guid pk,
			string id)
		{
			var registrationType = new eHubRegistrationType
			{
				RT_PK = pk,
				RT_ID = id
			};

			var registrationTypes =
				mockContext.eHubRegistrationTypes ??
				new TestDbSet<eHubRegistrationType>();

			registrationTypes.Add(registrationType);

			mockContext
				.Stub(mock => mock.eHubRegistrationTypes)
				.Return(registrationTypes);

			return mockContext;
		}

		public static eHubTransactionsContext WithClientSystem(
			this eHubTransactionsContext mockContext,
			Guid pk,
			string id)
		{
			var clientSystem = new eHubClientSystem
			{
				EH_PK = pk,
				EH_ID = id
			};

			var clientSystems =
				mockContext.eHubClientSystems ??
				new TestDbSet<eHubClientSystem>();

			clientSystems.Add(clientSystem);

			mockContext
				.Stub(mock => mock.eHubClientSystems)
				.Return(clientSystems);

			return mockContext;
		}

		public static eHubTransactionsContext WithEmptyClientSystemRegistration(
			this eHubTransactionsContext mockContext)
		{
			mockContext
				.Stub(mock => mock.eHubClientSystemRegistrations)
				.Return(new TestDbSet<eHubClientSystemRegistration>());

			return mockContext;
		}

		public static eHubTransactionsContext WithClientSystemRegistration(
			this eHubTransactionsContext mockContext,
			Guid pk,
			Guid registrationTypePk,
			Guid clientSystemPk,
			string badge,
			string topic,
			byte flag1)
		{
			var clientSystemRegistration = new eHubClientSystemRegistration
			{
				CD_PK = pk,
				CD_RT = registrationTypePk,
				CD_EH = clientSystemPk,
				CD_Code = badge,
				CD_Attr1 = topic,
				CD_Flag1 = flag1,

				eHubRegistrationType = mockContext
					.eHubRegistrationTypes
					.SingleOrDefault(type => type.RT_PK == registrationTypePk),

				eHubClientSystem = mockContext
					.eHubClientSystems
					.SingleOrDefault(system => system.EH_PK == clientSystemPk)
			};

			var clientSystemRegistrations =
				mockContext.eHubClientSystemRegistrations ??
				new TestDbSet<eHubClientSystemRegistration>();

			clientSystemRegistrations.Add(clientSystemRegistration);

			mockContext
				.Stub(mock => mock.eHubClientSystemRegistrations)
				.Return(clientSystemRegistrations);

			return mockContext;
		}

		public static eHubTransactionsContext WithClient(this eHubTransactionsContext mockContext, Guid pk, string id)
		{
			var client = new eHubClient
			{
				CC_PK = pk,
				CC_ID = id
			};

			var clients = mockContext.eHubClients ?? new TestDbSet<eHubClient>();
			clients.Add(client);

			mockContext
				.Stub(mock => mock.eHubClients)
				.Return(clients);

			return mockContext;
		}

		public static eHubTransactionsContext WithClientRegistration(
			this eHubTransactionsContext mockContext,
			Guid registrationTypePk,
			Guid clientPk,
			string url,
			string authorization)
		{
			var clientRegistration = new eHubClientRegistration
			{
				CX_PK = Guid.NewGuid(),
				CX_RT = registrationTypePk,
				CX_CC = clientPk,
				CX_Code = authorization,
				CX_Attr1 = url,

				eHubRegistrationType = mockContext
					.eHubRegistrationTypes
					.SingleOrDefault(type => type.RT_PK == registrationTypePk),

				eHubClient = mockContext
					.eHubClients
					.SingleOrDefault(client => client.CC_PK == clientPk)
			};

			var clientRegistrations =
				mockContext.eHubClientRegistrations ??
				new TestDbSet<eHubClientRegistration>();

			clientRegistrations.Add(clientRegistration);

			mockContext
				.Stub(mock => mock.eHubClientRegistrations)
				.Return(clientRegistrations);

			return mockContext;
		}

		public static eHubTransactionsContext WithEmptyClientRegistration(this eHubTransactionsContext mockContext)
		{
			mockContext
				.Stub(mock => mock.eHubClientRegistrations)
				.Return(new TestDbSet<eHubClientRegistration>());

			return mockContext;
		}

		public static eHubTransactionsContext WithSubscriptionType(
			this eHubTransactionsContext mockContext,
			Guid pk,
			string id,
			TimeSpan expiryPeriod)
		{
			var subscriptionType = new eHubSubscriptionType
			{
				ST_PK = pk,
				ST_ID = id,
				ST_ExpiryDays = (int)expiryPeriod.TotalDays
			};

			var subscriptionTypes =
				mockContext.eHubSubscriptionTypes ??
				new TestDbSet<eHubSubscriptionType>();

			subscriptionTypes.Add(subscriptionType);

			mockContext
				.Stub(mock => mock.eHubSubscriptionTypes)
				.Return(subscriptionTypes);

			return mockContext;
		}

		public static eHubTransactionsContext WithSubscriptionValue(
			this eHubTransactionsContext mockContext,
			Guid pk,
			Guid subscriptionTypePk,
			Guid senderPk,
			Guid recipientPk,
			string value)
		{
			var subscriptionValue = new eHubSubscriptionValue
			{
				SV_PK = pk,
				SV_ST = subscriptionTypePk,
				SV_CC_Sender = senderPk,
				SV_CC_Recipient = recipientPk,
				SV_Value = value,

				eHubSubscriptionType = mockContext
					.eHubSubscriptionTypes
					.SingleOrDefault(type => type.ST_PK == subscriptionTypePk),

				eHubClient_Provider = mockContext
					.eHubClients
					.SingleOrDefault(client => client.CC_PK == senderPk),

				eHubClient_Subscriber = mockContext
					.eHubClients
					.SingleOrDefault(client => client.CC_PK == recipientPk)
			};

			var subscriptionValues =
				mockContext.eHubSubscriptionValues ??
				new TestDbSet<eHubSubscriptionValue>();

			subscriptionValues.Add(subscriptionValue);

			mockContext
				.Stub(mock => mock.eHubSubscriptionValues)
				.Return(subscriptionValues);

			return mockContext;

		}

		public static eHubTransactionsContext WithEmptySubscriptionValue(this eHubTransactionsContext mockContext)
		{
			mockContext
				.Stub(mock => mock.eHubSubscriptionValues)
				.Return(new TestDbSet<eHubSubscriptionValue>());

			return mockContext;
		}
	}
}

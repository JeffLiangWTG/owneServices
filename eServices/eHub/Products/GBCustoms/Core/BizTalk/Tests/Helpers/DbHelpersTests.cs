using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.GBCustoms.Core.BT.Helpers;
using CargoWise.eHub.Products.GBCustoms.Core.BT.Tests.TestHelpers;
using NUnit.Framework;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.Core.BT.Tests.Helpers
{
	[TestFixture]
	public class DbHelpersTests
	{
		[TestFixture]
		public class IsRegisteredWithProviderMethod
		{
			[Test]
			[TestCaseSource(typeof(TestSource), "ExistingRegistrationCases")]
			public bool WhenFindingExistingClientSystemRegistrationEntry_ShouldReturnValueBasedOnFlag1(byte flag1)
			{
				// Arrange.

				var registrationTypePk = Guid.NewGuid();
				var clientSystemPk = Guid.NewGuid();
				var clientSystemRegistrationPk = Guid.NewGuid();

				var mockContext = MockRepository
					.GenerateMock<eHubTransactionsContext>()
					.WithRegistrationType(registrationTypePk, "GBCustoms-CNSAccount")
					.WithClientSystem(clientSystemPk, "[_MOCK_CLIENT_SYSTEM_]")
					.WithClientSystemRegistration(
						clientSystemRegistrationPk,
						registrationTypePk,
						clientSystemPk,
						"[_MOCK_BADGE_]",
						"[_MOCK_TOPIC_]",
						flag1);

				DbHelpers.GetDbContext = () => mockContext;

				// Act.

				var isRegistered = DbHelpers.IsRegisteredWithProvider(
					"[_MOCK_CLIENT_SYSTEM_]",
					"[_MOCK_BADGE_]", ProviderType.CNS);

				// Assert.

				return isRegistered;
			}

			[Test]
			public void WhenGettingNonExistingRegistrationType_ShouldThrowInvalidOperationException()
			{
				var registrationTypePk = Guid.NewGuid();
				var clientSystemPk = Guid.NewGuid();
				var clientSystemRegistrationPk = Guid.NewGuid();

				var mockContext = MockRepository
					.GenerateMock<eHubTransactionsContext>()
					.WithRegistrationType(registrationTypePk, "[_MOCK_REGISTRATION_TYPE_]")
					.WithClientSystem(clientSystemPk, "[_MOCK_CLIENT_SYSTEM_]")
					.WithClientSystemRegistration(
						clientSystemRegistrationPk,
						registrationTypePk,
						clientSystemPk,
						"[_MOCK_BADGE_]",
						"[_MOCK_TOPIC_]",
						0);

				DbHelpers.GetDbContext = () => mockContext;

				// Act.

				var exception = Assert.Throws<InvalidOperationException>(() =>
				{
					var _ = DbHelpers.IsRegisteredWithProvider(
						"[_MOCK_CLIENT_SYSTEM_]",
						"[_MOCK_BADGE_]", ProviderType.MCP);
				});

				// Assert.

				Assert.That(
					exception.Message,
					Is.EqualTo("No eHub registration type with ID [GBCustoms-MCPAccount]!"));
			}

			[Test]
			public void WhenGettingNonExistingClientSystem_ShouldThrowInvalidOperationException()
			{
				var registrationTypePk = Guid.NewGuid();
				var clientSystemPk = Guid.NewGuid();
				var clientSystemRegistrationPk = Guid.NewGuid();

				var mockContext = MockRepository
					.GenerateMock<eHubTransactionsContext>()
					.WithRegistrationType(registrationTypePk, "GBCustoms-PentantAccount")
					.WithClientSystem(clientSystemPk, "[_MOCK_CLIENT_SYSTEM_]")
					.WithClientSystemRegistration(
						clientSystemRegistrationPk,
						registrationTypePk,
						clientSystemPk,
						"[_MOCK_BADGE_]",
						"[_MOCK_TOPIC_]",
						0);

				DbHelpers.GetDbContext = () => mockContext;

				// Act.

				var exception = Assert.Throws<InvalidOperationException>(() =>
				{
					var _ = DbHelpers.IsRegisteredWithProvider(
						"[_MOCK_DIFFERENT_CLIENT_SYSTEM_]",
						"[_MOCK_BADGE_]", ProviderType.Pentant);
				});

				// Assert.

				Assert.That(
					exception.Message,
					Is.EqualTo("No eHub client system with ID [[_MOCK_DIFFERENT_CLIENT_SYSTEM_]]!"));
			}

			[Test]
			public void WhenGettingNonExistingRegistrationForBadge_ShouldReturnFalse()
			{
				var registrationTypePk = Guid.NewGuid();
				var clientSystemPk = Guid.NewGuid();
				var clientSystemRegistrationPk = Guid.NewGuid();

				var mockContext = MockRepository
					.GenerateMock<eHubTransactionsContext>()
					.WithRegistrationType(registrationTypePk, "GBCustoms-CTCGBAccount")
					.WithClientSystem(clientSystemPk, "[_MOCK_CLIENT_SYSTEM_]")
					.WithClientSystemRegistration(
						clientSystemRegistrationPk,
						registrationTypePk,
						clientSystemPk,
						"[_MOCK_BADGE_]",
						"[_MOCK_TOPIC_]",
						0);

				DbHelpers.GetDbContext = () => mockContext;

				// Act.

				var isRegistered = DbHelpers.IsRegisteredWithProvider(
					"[_MOCK_CLIENT_SYSTEM_]",
					"[_MOCK_DIFFERENT_BADGE_]", ProviderType.CTCGB);

				// Assert.

				Assert.That(isRegistered, Is.False);
			}

			private static class TestSource
			{
				public static IEnumerable<TestCaseData> ExistingRegistrationCases
				{
					get
					{
						yield return new TestCaseData((byte)0)
							.Returns(true)
							.SetName("CASE 01: CD_Flag1 = 0 (Registered)");

						yield return new TestCaseData((byte)1)
							.Returns(true)
							.SetName("CASE 02: CD_Flag1 = 1 (Registering)");

						yield return new TestCaseData((byte)2)
							.Returns(false)
							.SetName("CASE 03: CD_Flag1 = 2 (Failed)");

						yield return new TestCaseData((byte)3)
							.Returns(false)
							.SetName("CASE 04: CD_Flag1 = 3 (Requested Manually)");

						yield return new TestCaseData((byte)42)
							.Returns(false)
							.SetName("CASE 05: CD_Flag1 = 42 (Unknown Value)");
					}
				}
			}
		}

		[TestFixture]
		public class FindCallbackRegistrationMethod
		{
			[Test]
			public void WhenGettingExistingClientRegistration_ShouldReturnUrlAndAuthorization()
			{
				// Arrange.

				var registrationTypePk = Guid.NewGuid();
				var productionClientPk = Guid.NewGuid();
				var testingClientPk = Guid.NewGuid();

				var mockContext = MockRepository
					.GenerateMock<eHubTransactionsContext>()
					.WithRegistrationType(registrationTypePk, "GBCustoms-MCP")
					.WithClient(productionClientPk, "[_MOCK_PRODUCTION_CLIENT_]")
					.WithClient(testingClientPk, "[_MOCK_TESTING_CLIENT_]")
					.WithClientRegistration(
						registrationTypePk,
						productionClientPk,
						"http://www.production.com",
						"[_MOCK_PRODUCTION_AUTH_]")
					.WithClientRegistration(
						registrationTypePk,
						testingClientPk,
						"http://www.testing.com",
						"[_MOCK_TESTING_AUTH_]");

				DbHelpers.GetDbContext = () => mockContext;

				// Act.

				var callbackRegistration = DbHelpers.FindCallbackRegistration("[_MOCK_PRODUCTION_CLIENT_]", ProviderType.MCP);

				// Assert.

				Assert.That(callbackRegistration, Is.Not.Null);
				Assert.That(callbackRegistration.Uri, Is.EqualTo(new Uri("http://www.production.com")));
				Assert.That(callbackRegistration.Authorization, Is.EqualTo("[_MOCK_PRODUCTION_AUTH_]"));
			}

			[Test]
			public void WhenGettingNonExistingRegistrationType_ShouldThrowInvalidOperationException()
			{
				// Arrange.

				var registrationTypePk = Guid.NewGuid();
				var clientPk = Guid.NewGuid();

				var mockContext = MockRepository
					.GenerateMock<eHubTransactionsContext>()
					.WithRegistrationType(registrationTypePk, "[_MOCK_REGISTRATION_TYPE_]")
					.WithClient(clientPk, "[_MOCK_CLIENT_]")
					.WithClientRegistration(registrationTypePk, clientPk, "http://www.mock.com", "[_MOCK_AUTH_]");

				DbHelpers.GetDbContext = () => mockContext;

				// Act.

				var exception = Assert.Throws<InvalidOperationException>(() =>
				{
					var _ = DbHelpers.FindCallbackRegistration("[_MOCK_CLIENT_]", ProviderType.MCP);
				});

				// Assert.

				Assert.That(
					exception.Message,
					Is.EqualTo("No eHub registration type with ID [GBCustoms-MCP]!"));
			}

			[Test]
			public void WhenGettingNonExistingClientRegistration_ShouldThrowInvalidOperationException()
			{
				// Arrange.

				var mockContext = MockRepository
					.GenerateMock<eHubTransactionsContext>()
					.WithRegistrationType(Guid.NewGuid(), "GBCustoms-MCP")
					.WithClient(Guid.NewGuid(), "[_MOCK_CLIENT_]")
					.WithEmptyClientRegistration();

				DbHelpers.GetDbContext = () => mockContext;

				// Act.

				var exception = Assert.Throws<InvalidOperationException>(() =>
				{
					var _ = DbHelpers.FindCallbackRegistration("[_MOCK_CLIENT_]", ProviderType.MCP);
				});

				// Assert.

				Assert.That(
					exception.Message,
					Is.EqualTo("No eHub client registration for registration type with ID [GBCustoms-MCP] for client with ID [[_MOCK_CLIENT_]]!"));
			}
		}

		[TestFixture]
		public class CreateClientRegistrationStatusQueryMethod
		{
			[Test]
			public void WhenGettingClientSystemWithoutExistingRegistration_ShouldCreateInsertQuery()
			{
				// Arrange.

				var registrationTypePk = Guid.NewGuid();
				var clientSystemPk = Guid.NewGuid();

				var mockContext = MockRepository
					.GenerateMock<eHubTransactionsContext>()
					.WithRegistrationType(registrationTypePk, "GBCustoms-MCPAccount")
					.WithClientSystem(clientSystemPk, "[_MOCK_CLIENT_SYSTEM_]")
					.WithEmptyClientSystemRegistration();

				DbHelpers.GetDbContext = () => mockContext;

				// Act.

				var actualQuery = DbHelpers.CreateClientRegistrationStatusQuery(
					"[_MOCK_CLIENT_SYSTEM_]",
					"[_MOCK_BADGE_]",
					"[_MOCK_TOPIC_]",
					42, ProviderType.MCP);

				// Assert.

				Assert.That(actualQuery, Is.Not.Null);

				actualQuery = actualQuery
					.ReplaceGuidColumn("CD_PK", "[_NEW_GUID_]")
					.ReplaceDateTimeColumn("CD_IssuedUTC", "[_ISSUED_TIMESTAMP_]")
					.ReplaceHexColumn("CD_RV", "[_HEX_VALUE_]");

				var expectedQuery = @"
<Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubClientSystemRegistration"">
  <Rows>
    <eHubClientSystemRegistration xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
      <CD_PK>[_NEW_GUID_]</CD_PK>
      <CD_RT>" + registrationTypePk + @"</CD_RT>
      <CD_EH>" + clientSystemPk + @"</CD_EH>
      <CD_Code>[_MOCK_BADGE_]</CD_Code>
      <CD_Attr1>[_MOCK_TOPIC_]</CD_Attr1>
      <CD_Flag1>42</CD_Flag1>
      <CD_IssuedUTC>[_ISSUED_TIMESTAMP_]</CD_IssuedUTC>
      <CD_RV>[_HEX_VALUE_]</CD_RV>
    </eHubClientSystemRegistration>
  </Rows>
</Insert>";

				Assert.That(
					actualQuery.Trim(),
					Is.EqualTo(expectedQuery.Trim()));
			}

			[Test]
			public void WhenGettingClientSystemWithExistingRegistration_ShouldCreateUpdateQuery()
			{
				// Arrange.

				var registrationTypePk = Guid.NewGuid();
				var clientSystemPk = Guid.NewGuid();
				var clientSystemRegistrationPk = Guid.NewGuid();

				var mockContext = MockRepository
					.GenerateMock<eHubTransactionsContext>()
					.WithRegistrationType(registrationTypePk, "GBCustoms-MCPAccount")
					.WithClientSystem(clientSystemPk, "[_MOCK_CLIENT_SYSTEM_]")
					.WithClientSystemRegistration(
						clientSystemRegistrationPk,
						registrationTypePk,
						clientSystemPk,
						"[_MOCK_BADGE_]",
						"[_MOCK_TOPIC_]",
						21);

				DbHelpers.GetDbContext = () => mockContext;

				// Act.

				var actualQuery = DbHelpers.CreateClientRegistrationStatusQuery(
					"[_MOCK_CLIENT_SYSTEM_]",
					"[_MOCK_BADGE_]",
					"[_MOCK_TOPIC_]",
					42, ProviderType.MCP);

				// Assert.

				Assert.That(actualQuery, Is.Not.Null);

				var expectedQuery = @"
<Update xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubClientSystemRegistration"">
  <Rows>
    <RowPair>
      <After>
        <CD_Flag1 xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">42</CD_Flag1>
      </After>
      <Before>
        <CD_PK xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">" + clientSystemRegistrationPk + @"</CD_PK>
      </Before>
    </RowPair>
  </Rows>
</Update>";

				Assert.That(
					actualQuery.Trim(),
					Is.EqualTo(expectedQuery.Trim()));
			}

			[Test]
			public void WhenGettingClientSystemWithMatchingBatchButNotTopic_ShouldCreateUpdateQueryModifyingStatusOnly()
			{
				// Arrange.

				var registrationTypePk = Guid.NewGuid();
				var clientSystemPk = Guid.NewGuid();
				var clientSystemRegistrationPk = Guid.NewGuid();

				var mockContext = MockRepository
					.GenerateMock<eHubTransactionsContext>()
					.WithRegistrationType(registrationTypePk, "GBCustoms-MCPAccount")
					.WithClientSystem(clientSystemPk, "[_MOCK_CLIENT_SYSTEM_]")
					.WithClientSystemRegistration(
						clientSystemRegistrationPk,
						registrationTypePk,
						clientSystemPk,
						"[_MOCK_BADGE_]",
						"[_MOCK_ANOTHER_TOPIC_]",
						21);

				DbHelpers.GetDbContext = () => mockContext;

				// Act.

				var actualQuery = DbHelpers.CreateClientRegistrationStatusQuery(
					"[_MOCK_CLIENT_SYSTEM_]",
					"[_MOCK_BADGE_]",
					"[_MOCK_TOPIC_]",
					42, ProviderType.MCP);

				// Assert.

				Assert.That(actualQuery, Is.Not.Null);

				var expectedQuery = @"
<Update xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubClientSystemRegistration"">
  <Rows>
    <RowPair>
      <After>
        <CD_Flag1 xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">42</CD_Flag1>
      </After>
      <Before>
        <CD_PK xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">" + clientSystemRegistrationPk + @"</CD_PK>
      </Before>
    </RowPair>
  </Rows>
</Update>";

				Assert.That(
					actualQuery.Trim(),
					Is.EqualTo(expectedQuery.Trim()));
			}
		}

		[TestFixture]
		public class CreateSubmissionReferenceQueryMethod
		{
			[Test]
			public void WhenGettingNonExistingReferenceId_ShouldCreateInsertQuery()
			{
				// Arrange.

				var referenceId = "[_MOCK_REFERENCE_ID_]";
				var messageTrackingId = "[_MOCK_TRACKING_ID_]";

				var messageBody = @"
<ns0:GBCustoms xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms"">
  <Header>[_MOCK_HEADER_]</Header>
  <Body>[_MOCK_BODY_]</Body>
</ns0:GBCustoms>";

				var sourcePartyPk = Guid.NewGuid();
				var destinationPartyPk = Guid.NewGuid();
				var subscriptionTypePk = Guid.NewGuid();

				var mockContext = MockRepository
					.GenerateMock<eHubTransactionsContext>()
					.WithClient(sourcePartyPk, "[_MOCK_SOURCE_]")
					.WithClient(destinationPartyPk, "[_MOCK_DESTINATION_]")
					.WithSubscriptionType(subscriptionTypePk, "GBCMCP", TimeSpan.FromDays(90))
					.WithEmptySubscriptionValue();

				DbHelpers.GetDbContext = () => mockContext;

				// Act.

				var actualQuery = DbHelpers.CreateSubmissionReferenceQuery(
					referenceId,
					messageTrackingId,
					messageBody,
					"[_MOCK_SOURCE_]",
					"[_MOCK_DESTINATION_]", "GBCMCP");

				// Assert.

				var expectedReferenceContent = @"
<ns0:GBCustomsTransportResponse xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms"">
  <CSPID>[_MOCK_REFERENCE_ID_]</CSPID>
  <eHubMessageTrackingId>[_MOCK_TRACKING_ID_]</eHubMessageTrackingId>
  <ns0:GBCustoms xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms"">
    <Header>[_MOCK_HEADER_]</Header>
  </ns0:GBCustoms>
</ns0:GBCustomsTransportResponse>";

				var expectedQuery = @"
<Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubSubscriptionValue"">
  <Rows>
    <eHubSubscriptionValue xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
      <SV_PK>[_NEW_GUID_FOR_PK_]</SV_PK>
      <SV_ST>" + subscriptionTypePk + @"</SV_ST>
      <SV_CC_Sender>" + destinationPartyPk + @"</SV_CC_Sender>
      <SV_CC_Recipient>" + sourcePartyPk + @"</SV_CC_Recipient>
      <SV_Value>[_MOCK_REFERENCE_ID_]</SV_Value>
      <SV_Reference><![CDATA[" + expectedReferenceContent.Trim() + @"]]></SV_Reference>
      <SV_SubscribedUTC>[_SUBSCRIBED_TIMESTAMP_]</SV_SubscribedUTC>
      <SV_ExpiryUTC>[_EXPIRY_TIMESTAMP_]</SV_ExpiryUTC>
    </eHubSubscriptionValue>
  </Rows>
</Insert>";

				actualQuery = actualQuery
					.ReplaceGuidColumn("SV_PK", "[_NEW_GUID_FOR_PK_]")
					.ReplaceDateTimeColumn("SV_SubscribedUTC", "[_SUBSCRIBED_TIMESTAMP_]")
					.ReplaceDateTimeColumn("SV_ExpiryUTC", "[_EXPIRY_TIMESTAMP_]");

				Assert.That(
					actualQuery.Trim(),
					Is.EqualTo(expectedQuery.Trim()));
			}

            public void WhenGettingNonExistingReferenceId_ShouldCreateInsertQueryWithoutMessage()
            {
                // Arrange.

                var referenceId = "[_MOCK_REFERENCE_ID_]";
                var messageTrackingId = "[_MOCK_TRACKING_ID_]";

                string messageBody = null;

                var sourcePartyPk = Guid.NewGuid();
                var destinationPartyPk = Guid.NewGuid();
                var subscriptionTypePk = Guid.NewGuid();

                var mockContext = MockRepository
                    .GenerateMock<eHubTransactionsContext>()
                    .WithClient(sourcePartyPk, "[_MOCK_SOURCE_]")
                    .WithClient(destinationPartyPk, "[_MOCK_DESTINATION_]")
                    .WithSubscriptionType(subscriptionTypePk, "GBCMCP", TimeSpan.FromDays(90))
                    .WithEmptySubscriptionValue();

                DbHelpers.GetDbContext = () => mockContext;

                // Act.

                var actualQuery = DbHelpers.CreateSubmissionReferenceQuery(
                    referenceId,
                    messageTrackingId,
                    messageBody,
                    "[_MOCK_SOURCE_]",
                    "[_MOCK_DESTINATION_]", "GBCMCP");

                // Assert.

                var expectedQuery = @"
<Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubSubscriptionValue"">
  <Rows>
    <eHubSubscriptionValue xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
      <SV_PK>[_NEW_GUID_FOR_PK_]</SV_PK>
      <SV_ST>" + subscriptionTypePk + @"</SV_ST>
      <SV_CC_Sender>" + destinationPartyPk + @"</SV_CC_Sender>
      <SV_CC_Recipient>" + sourcePartyPk + @"</SV_CC_Recipient>
      <SV_Value>[_MOCK_REFERENCE_ID_]</SV_Value>
      <SV_SubscribedUTC>[_SUBSCRIBED_TIMESTAMP_]</SV_SubscribedUTC>
      <SV_ExpiryUTC>[_EXPIRY_TIMESTAMP_]</SV_ExpiryUTC>
    </eHubSubscriptionValue>
  </Rows>
</Insert>";

                actualQuery = actualQuery
                    .ReplaceGuidColumn("SV_PK", "[_NEW_GUID_FOR_PK_]")
                    .ReplaceDateTimeColumn("SV_SubscribedUTC", "[_SUBSCRIBED_TIMESTAMP_]")
                    .ReplaceDateTimeColumn("SV_ExpiryUTC", "[_EXPIRY_TIMESTAMP_]");

                Assert.That(
                    actualQuery.Trim(),
                    Is.EqualTo(expectedQuery.Trim()));
            }

            [Test]
			public void WhenGettingExistingReferenceId_ShouldCreateEmptyQuery()
			{
				// Arrange.

				var sourcePartyPk = Guid.NewGuid();
				var destinationPartyPk = Guid.NewGuid();
				var subscriptionTypePk = Guid.NewGuid();

				var mockContext = MockRepository
					.GenerateMock<eHubTransactionsContext>()
					.WithClient(sourcePartyPk, "[_MOCK_SOURCE_]")
					.WithClient(destinationPartyPk, "[_MOCK_DESTINATION_]")
					.WithSubscriptionType(subscriptionTypePk, "GBCMCP", TimeSpan.FromDays(90))
					.WithSubscriptionValue(
						Guid.NewGuid(),
						subscriptionTypePk,
						destinationPartyPk,
						sourcePartyPk,
						"[_MOCK_REFERENCE_ID_]");

				DbHelpers.GetDbContext = () => mockContext;

				// Act.

				var actualQuery = DbHelpers.CreateSubmissionReferenceQuery(
					"[_MOCK_REFERENCE_ID_]",
					"[_MOCK_TRACKING_ID_]",
					"[_MOCK_MESSAGE_BODY_]",
					"[_MOCK_SOURCE_]",
					"[_MOCK_DESTINATION_]",
					"GBCMCP");

				// Assert.

				Assert.That(actualQuery, Is.Empty);
			}

			[Test]
			public void WhenGettingNonExistingSourceParty_ShouldThrowInvalidOperationException()
			{
				// Arrange.

				var mockContext = MockRepository
					.GenerateMock<eHubTransactionsContext>()
					.WithClient(Guid.NewGuid(), "[_MOCK_DESTINATION_]")
					.WithSubscriptionType(Guid.NewGuid(), "GBCMCP", TimeSpan.FromDays(90))
					.WithEmptySubscriptionValue();

				DbHelpers.GetDbContext = () => mockContext;

				// Act.

				var exception = Assert.Throws<InvalidOperationException>(() =>
				{
					var _ = DbHelpers.CreateSubmissionReferenceQuery(
						"[_MOCK_REFERENCE_ID_]",
						"[_MOCK_TRACKING_ID_]",
						"[_MOCK_MESSAGE_BODY_]",
						"[_MOCK_DESTINATION_]",
						"[_MOCK_SOURCE_]", "GBCMCP");
				});

				// Assert.

				Assert.That(exception.Message, Is.EqualTo("No eHub client with ID [[_MOCK_SOURCE_]]!"));
			}

			[Test]
			public void WhenGettingNonExistingDestinationParty_ShouldThrowInvalidOperationException()
			{
				// Arrange.

				var mockContext = MockRepository
					.GenerateMock<eHubTransactionsContext>()
					.WithClient(Guid.NewGuid(), "[_MOCK_SOURCE_]")
					.WithSubscriptionType(Guid.NewGuid(), "GBCMCP", TimeSpan.FromDays(90))
					.WithEmptySubscriptionValue();

				DbHelpers.GetDbContext = () => mockContext;

				// Act.

				var exception = Assert.Throws<InvalidOperationException>(() =>
				{
					var _ = DbHelpers.CreateSubmissionReferenceQuery(
						"[_MOCK_REFERENCE_ID_]",
						"[_MOCK_TRACKING_ID_]",
						"[_MOCK_MESSAGE_BODY_]",
						"[_MOCK_DESTINATION_]",
						"[_MOCK_SOURCE_]", "GBCMCP");
				});

				// Assert.

				Assert.That(exception.Message, Is.EqualTo("No eHub client with ID [[_MOCK_DESTINATION_]]!"));
			}

			[Test]
			public void WhenGettingNonExistingSubscriptionType_ShouldThrowInvalidOperationException()
			{
				// Arrange.

				var mockContext = MockRepository
					.GenerateMock<eHubTransactionsContext>()
					.WithClient(Guid.NewGuid(), "[_MOCK_SOURCE_]")
					.WithClient(Guid.NewGuid(), "[_MOCK_DESTINATION_]")
					.WithSubscriptionType(Guid.NewGuid(), "[_MOCK_SUBSCRIPTION_TYPE_]", TimeSpan.FromDays(90))
					.WithEmptySubscriptionValue();

				DbHelpers.GetDbContext = () => mockContext;

				// Act.

				var exception = Assert.Throws<InvalidOperationException>(() =>
				{
					var _ = DbHelpers.CreateSubmissionReferenceQuery(
						"[_MOCK_REFERENCE_ID_]",
						"[_MOCK_TRACKING_ID_]",
						"[_MOCK_MESSAGE_BODY_]",
						"[_MOCK_DESTINATION_]",
						"[_MOCK_SOURCE_]", "GBCMCP");
				});

				// Assert.

				Assert.That(exception.Message, Is.EqualTo("No eHub subscription type with ID [GBCMCP]!"));
			}

			[Test]
			public void WhenReferenceIdIsNull_ShouldThrowArgumentNullException()
			{
				// Arrange.

				// Act.

				var exception = Assert.Throws<ArgumentNullException>(() =>
				{
					var _ = DbHelpers.CreateSubmissionReferenceQuery(
						null,
						"[_MOCK_TRACKING_ID_]",
						"[_MOCK_MESSAGE_BODY_]",
						"[_MOCK_DESTINATION_]",
						"[_MOCK_SOURCE_]", "GBCMCP");
				});

				// Assert.

				Assert.That(exception.Message, Is.EqualTo("The response from the CSP did not contain an ID, the CSP service could be down.\r\nParameter name: referenceId"));
			}
		}

		[TestFixture]
		public class CreateSuccessfulTransportResponseQueryMethod
		{
			[Test]
			public void WhenGettingValueInputs_ShouldCreateInsertQuery()
			{
				// Arrange.

				var referenceId = "[_MOCK_REFERENCE_ID_]";
				var messageTrackingId = "[_MOCK_TRACKING_ID_]";
				var transactionId = "[_MOCK_TRANSACTION_ID_]";

				var messageBody = @"
<ns0:GBCustoms xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms"">
  <Header>[_MOCK_HEADER_]</Header>
  <Body>[_MOCK_BODY_]</Body>
</ns0:GBCustoms>";

				var sourcePartyPk = Guid.NewGuid();
				var destinationPartyPk = Guid.NewGuid();

				var mockContext = MockRepository
					.GenerateMock<eHubTransactionsContext>()
					.WithClient(sourcePartyPk, "[_MOCK_SOURCE_]")
					.WithClient(destinationPartyPk, "[_MOCK_DESTINATION_]");

				DbHelpers.GetDbContext = () => mockContext;

				// Act.

				var actualQuery = DbHelpers.CreateSuccessfulTransportResponseQuery(
					referenceId,
					messageTrackingId,
					messageBody,
					"[_MOCK_SOURCE_]",
					"[_MOCK_DESTINATION_]",
					transactionId);


				// Assert.

				Assert.That(actualQuery, Is.Not.Null.And.Not.Empty);

				var targetNamespace = (XNamespace)"http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo";
				var actualXml = XDocument.Parse(actualQuery);

				var actualPk = actualXml
					.Descendants(targetNamespace + "EI_PK")
					.Single()
					.Value;

				var actualMessageTrackingId = actualXml
					.Descendants(targetNamespace + "EI_MessageTrackingID")
					.Single()
					.Value;

				Assert.That(actualPk, Is.EqualTo(actualMessageTrackingId));

				var expectedContent = @"
<ns0:GBCustomsTransportResponse xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms"">
  <Status>Accepted</Status>
  <CSPID>[_MOCK_TRANSACTION_ID_]</CSPID>
  <eHubMessageTrackingId>[_MOCK_TRACKING_ID_]</eHubMessageTrackingId>
  <ns0:GBCustoms xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms"">
    <Header>[_MOCK_HEADER_]</Header>
    <Body>[_MOCK_BODY_]</Body>
  </ns0:GBCustoms>
</ns0:GBCustomsTransportResponse>";

				var expectedEncodedContent = string.Empty;

				using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(expectedContent.Trim())))
				{
					expectedEncodedContent = stream
						.CompressAndEncode()
						.ReadToEnd();
				}

				var expectedQuery = @"
<Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxMessage"">
  <Rows>
    <eHubInboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
      <EI_PK>[_NEW_GUID_FOR_PK_]</EI_PK>
      <EI_MessageTrackingID>[_NEW_GUID_FOR_TRACKING_ID_]</EI_MessageTrackingID>
      <EI_EnvelopeTrackingID>00000000-0000-0000-0000-000000000000</EI_EnvelopeTrackingID>
      <EI_CC_Sender>" + destinationPartyPk + @"</EI_CC_Sender>
      <EI_CC_Recipient>" + sourcePartyPk + @"</EI_CC_Recipient>
      <EI_MessageType>http://cargowise.com/ehub/products/GBCustoms#GBCustomsTransportResponse</EI_MessageType>
      <EI_IsFlatFile>false</EI_IsFlatFile>
      <EI_ApplicationCode>UDM</EI_ApplicationCode>
      <EI_Status>0</EI_Status>
      <EI_Content>" + expectedEncodedContent + @"</EI_Content>
      <EI_InsertUTC>[_INSERT_TIMESTAMP_]</EI_InsertUTC>
    </eHubInboxMessage>
  </Rows>
</Insert>";

				actualQuery = actualQuery
					.ReplaceGuidColumn("EI_PK", "[_NEW_GUID_FOR_PK_]")
					.ReplaceGuidColumn("EI_MessageTrackingID", "[_NEW_GUID_FOR_TRACKING_ID_]")
					.ReplaceDateTimeColumn("EI_InsertUTC", "[_INSERT_TIMESTAMP_]");

				Assert.That(
					actualQuery.Trim(),
					Is.EqualTo(expectedQuery.Trim()));
			}

			[Test]
			public void WhenGettingNonExistingSourceParty_ShouldThrowInvalidOperationException()
			{
				// Arrange.

				var referenceId = "[_MOCK_REFERENCE_ID_]";
				var messageTrackingId = "[_MOCK_TRACKING_ID_]";

				var messageBody = @"
<ns0:GBCustoms xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms"">
  <Header>[_MOCK_HEADER_]</Header>
  <Body>[_MOCK_BODY_]</Body>
</ns0:GBCustoms>";

				var mockContext = MockRepository
					.GenerateMock<eHubTransactionsContext>()
					.WithClient(Guid.NewGuid(), "[_MOCK_DESTINATION_]");

				DbHelpers.GetDbContext = () => mockContext;

				// Act.

				var exception = Assert.Throws<InvalidOperationException>(() =>
				{
					var _ = DbHelpers.CreateSuccessfulTransportResponseQuery(
						referenceId,
						messageTrackingId,
						messageBody,
						"[_MOCK_SOURCE_]",
						"[_MOCK_DESTINATION_]");
				});

				// Assert.

				Assert.That(exception.Message, Is.EqualTo("No eHub client with ID [[_MOCK_SOURCE_]]!"));
			}

			[Test]
			public void WhenGettingNonExistingDestinationParty_ShouldThrowInvalidOperationException()
			{
				// Arrange.

				var referenceId = "[_MOCK_REFERENCE_ID_]";
				var messageTrackingId = "[_MOCK_TRACKING_ID_]";

				var messageBody = @"
<ns0:GBCustoms xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms"">
  <Header>[_MOCK_HEADER_]</Header>
  <Body>[_MOCK_BODY_]</Body>
</ns0:GBCustoms>";

				var mockContext = MockRepository
					.GenerateMock<eHubTransactionsContext>()
					.WithClient(Guid.NewGuid(), "[_MOCK_SOURCE_]");

				DbHelpers.GetDbContext = () => mockContext;

				// Act.

				var exception = Assert.Throws<InvalidOperationException>(() =>
				{
					var _ = DbHelpers.CreateSuccessfulTransportResponseQuery(
						referenceId,
						messageTrackingId,
						messageBody,
						"[_MOCK_SOURCE_]",
						"[_MOCK_DESTINATION_]");
				});

				// Assert.

				Assert.That(exception.Message, Is.EqualTo("No eHub client with ID [[_MOCK_DESTINATION_]]!"));
			}
		}

		[TestFixture]
		public class CreateErrorTransportResponseQueryMethod
		{
			[Test]
			public void WhenGettingValidInputs_ShouldCreateInsertQuery()
			{
				// Arrange.

				var error = "[_MOCK_ERROR_]";
				var errorText = "[_MOCK_ERROR_TEXT_]";
				var responseText = "[_MOCK_RESPONSE_TEXT_]";
				var messageTrackingId = "[_MOCK_TRACKING_ID_]";

				var messageBody = @"
<ns0:GBCustoms xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms"">
  <Header>[_MOCK_HEADER_]</Header>
  <Body>[_MOCK_BODY_]</Body>
</ns0:GBCustoms>";

				var sourcePartyPk = Guid.NewGuid();
				var destinationPartyPk = Guid.NewGuid();

				var mockContext = MockRepository
					.GenerateMock<eHubTransactionsContext>()
					.WithClient(sourcePartyPk, "[_MOCK_SOURCE_]")
					.WithClient(destinationPartyPk, "[_MOCK_DESTINATION_]");

				DbHelpers.GetDbContext = () => mockContext;

				// Act.

				var actualQuery = DbHelpers.CreateErrorTransportResponseQuery(
					error,
					errorText,
					responseText,
					messageTrackingId,
					messageBody,
					"[_MOCK_SOURCE_]",
					"[_MOCK_DESTINATION_]");

				// Assert.

				Assert.That(actualQuery, Is.Not.Null.And.Not.Empty);

				var targetNamespace = (XNamespace)"http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo";
				var actualXml = XDocument.Parse(actualQuery);

				var actualPk = actualXml
					.Descendants(targetNamespace + "EI_PK")
					.Single()
					.Value;

				var actualMessageTrackingId = actualXml
					.Descendants(targetNamespace + "EI_MessageTrackingID")
					.Single()
					.Value;

				Assert.That(actualPk, Is.EqualTo(actualMessageTrackingId));

				var expectedContent = @"
<ns0:GBCustomsTransportResponse xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms"">
  <Status>Error</Status>
  <Error>[_MOCK_ERROR_]</Error>
  <ErrorText>Error Description: [_MOCK_ERROR_TEXT_]</ErrorText>
  <ResponseText>[_MOCK_RESPONSE_TEXT_]</ResponseText>
  <eHubMessageTrackingId>[_MOCK_TRACKING_ID_]</eHubMessageTrackingId>
  <ns0:GBCustoms xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms"">
    <Header>[_MOCK_HEADER_]</Header>
    <Body>[_MOCK_BODY_]</Body>
  </ns0:GBCustoms>
</ns0:GBCustomsTransportResponse>";

				var expectedEncodedContent = string.Empty;

				using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(expectedContent.Trim())))
				{
					expectedEncodedContent = stream
						.CompressAndEncode()
						.ReadToEnd();
				}

				var expectedQuery = @"
<Insert xmlns=""http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxMessage"">
  <Rows>
    <eHubInboxMessage xmlns=""http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo"">
      <EI_PK>[_NEW_GUID_FOR_PK_]</EI_PK>
      <EI_MessageTrackingID>[_NEW_GUID_FOR_TRACKING_ID_]</EI_MessageTrackingID>
      <EI_EnvelopeTrackingID>00000000-0000-0000-0000-000000000000</EI_EnvelopeTrackingID>
      <EI_CC_Sender>" + destinationPartyPk + @"</EI_CC_Sender>
      <EI_CC_Recipient>" + sourcePartyPk + @"</EI_CC_Recipient>
      <EI_MessageType>http://cargowise.com/ehub/products/GBCustoms#GBCustomsTransportResponse</EI_MessageType>
      <EI_IsFlatFile>false</EI_IsFlatFile>
      <EI_ApplicationCode>UDM</EI_ApplicationCode>
      <EI_Status>0</EI_Status>
      <EI_Content>" + expectedEncodedContent + @"</EI_Content>
      <EI_InsertUTC>[_INSERT_TIMESTAMP_]</EI_InsertUTC>
    </eHubInboxMessage>
  </Rows>
</Insert>";

				actualQuery = actualQuery
					.ReplaceGuidColumn("EI_PK", "[_NEW_GUID_FOR_PK_]")
					.ReplaceGuidColumn("EI_MessageTrackingID", "[_NEW_GUID_FOR_TRACKING_ID_]")
					.ReplaceDateTimeColumn("EI_InsertUTC", "[_INSERT_TIMESTAMP_]");

				Assert.That(
					actualQuery.Trim(),
					Is.EqualTo(expectedQuery.Trim()));
			}

			[Test]
			public void WhenGettingNonExistingSourceParty_ShouldThrowInvalidOperationException()
			{
				// Arrange.

				var error = "[_MOCK_ERROR_]";
				var errorText = "[_MOCK_ERROR_TEXT_]";
				var responseText = "[_MOCK_RESPONSE_TEXT_]";
				var messageTrackingId = "[_MOCK_TRACKING_ID_]";
				var messageBody = "[_MOCK_MESSAGE_BODY_]";

				var mockContext = MockRepository
					.GenerateMock<eHubTransactionsContext>()
					.WithClient(Guid.NewGuid(), "[_MOCK_DESTINATION_]");

				DbHelpers.GetDbContext = () => mockContext;

				// Act.

				var exception = Assert.Throws<InvalidOperationException>(() =>
				{
					var _ = DbHelpers.CreateErrorTransportResponseQuery(
						error,
						errorText,
						responseText,
						messageTrackingId,
						messageBody,
						"[_MOCK_SOURCE_]",
						"[_MOCK_DESTINATION_]");
				});

				// Assert.

				Assert.That(exception.Message, Is.EqualTo("No eHub client with ID [[_MOCK_SOURCE_]]!"));
			}

			[Test]
			public void WhenGettingNonExistingDestinationParty_ShouldThrowInvalidOperationException()
			{
				// Arrange.

				var error = "[_MOCK_ERROR_]";
				var errorText = "[_MOCK_ERROR_TEXT_]";
				var responseText = "[_MOCK_RESPONSE_TEXT_]";
				var messageTrackingId = "[_MOCK_TRACKING_ID_]";
				var messageBody = "[_MOCK_MESSAGE_BODY_]";

				var mockContext = MockRepository
					.GenerateMock<eHubTransactionsContext>()
					.WithClient(Guid.NewGuid(), "[_MOCK_SOURCE_]");

				DbHelpers.GetDbContext = () => mockContext;

				// Act.

				var exception = Assert.Throws<InvalidOperationException>(() =>
				{
					var _ = DbHelpers.CreateErrorTransportResponseQuery(
						error,
						errorText,
						responseText,
						messageTrackingId,
						messageBody,
						"[_MOCK_SOURCE_]",
						"[_MOCK_DESTINATION_]");
				});

				// Assert.

				Assert.That(exception.Message, Is.EqualTo("No eHub client with ID [[_MOCK_DESTINATION_]]!"));
			}
		}
	}
}

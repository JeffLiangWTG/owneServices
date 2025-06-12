using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;
using CargoWise.eServices.TestHelpers.Database.Common;
using CargoWise.eServices.TestHelpers.Database.Deployment;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests
{
	[TestFixture]
	abstract class PluginTest<TPlugin>
		where TPlugin : SqlBillingTransactionsPlugin, new()
	{
		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			eHubArchiveOnlineConnection = SqlServerHelper.OpenAdminSqlConnection("eHubArchiveOnline");
			eHubArchiveOnlineViewConnection = SqlServerHelper.OpenAdminSqlConnection("eHubArchiveOnlineView");
			eHubTransactionsConnection = SqlServerHelper.OpenAdminSqlConnection("eHubTransactions");
			TestFixtureSetUpCore();
		}

		[OneTimeTearDown]
		public void TestFixtureTearDown()
		{
			try
			{
				TestFixtureTearDownCore();
			}
			finally
			{
				if (eHubArchiveOnlineConnection != null)
				{
					eHubArchiveOnlineConnection.Close();
					eHubArchiveOnlineConnection = null;
				}
				if (eHubArchiveOnlineViewConnection != null)
				{
					eHubArchiveOnlineViewConnection.Close();
					eHubArchiveOnlineViewConnection = null;
				}
				if (eHubTransactionsConnection != null)
				{
					eHubTransactionsConnection.Close();
					eHubTransactionsConnection = null;
				}
			}
		}

		[SetUp]
		public void SetUp()
		{
			TestContext.WriteLine(Deployment.GetLog());

			eHubArchiveOnlineTransaction = eHubArchiveOnlineConnection.BeginTransaction(IsolationLevel.ReadUncommitted);
			eHubArchiveOnlineViewTransaction = eHubArchiveOnlineViewConnection.BeginTransaction(IsolationLevel.ReadUncommitted);
			eHubTransactionsTransaction = eHubTransactionsConnection.BeginTransaction(IsolationLevel.ReadUncommitted);
			SetUpCore();
		}

		[TearDown]
		public void TearDown()
		{
			try
			{
				TearDownCore();
			}
			finally
			{
				InvokeAll(
					() =>
					{
						if (eHubArchiveOnlineTransaction == null) return;
						eHubArchiveOnlineTransaction.Rollback();
						eHubArchiveOnlineTransaction.Dispose();
						eHubArchiveOnlineTransaction = null;
					},
					() =>
					{
						if (eHubArchiveOnlineViewTransaction == null) return;
						eHubArchiveOnlineViewTransaction.Rollback();
						eHubArchiveOnlineViewTransaction.Dispose();
						eHubArchiveOnlineViewTransaction = null;
					},
					() =>
					{
						if (eHubTransactionsTransaction == null) return;
						eHubTransactionsTransaction.Rollback();
						eHubTransactionsTransaction.Dispose();
						eHubTransactionsTransaction = null;
					});
			}
		}

		protected virtual void TestFixtureSetUpCore()
		{
		}

		protected virtual void SetUpCore()
		{
		}

		protected virtual void TestFixtureTearDownCore()
		{
		}

		protected virtual void TearDownCore()
		{
		}

		protected eHubClient AddClient(eHubClient client)
		{
			var sqlConnections = new[] { eHubTransactionsConnection, eHubArchiveOnlineViewConnection };
			var sqlTransactions = new[] { eHubTransactionsTransaction, eHubArchiveOnlineViewTransaction };

			for (var i = 0; i < sqlConnections.Length; i++)
			{
				using (var command = sqlConnections[i].CreateCommand())
				{
					command.Transaction = sqlTransactions[i];
					command.CommandText = @"
INSERT INTO [eHubClient] ([CC_PK], [CC_ID], [CC_FriendlyName], [CC_Odyssey_OH], [CC_DistributionZone], [CC_EmailAddress], [CC_Password], [CC_IsAirServiceProvider], [CC_AirlineCode], [CC_AirServiceProvider], [CC_AirlinePrefix], [CC_USCustomsRecipient], [CC_AS2_Code], [CC_SCAC_Code], [CC_OwnerCategory], [CC_SystemCategory], [CC_RR], [CC_RequireStatusResponse], [CC_NotificationForInboxRecipient])
VALUES (@PK, @ID, @FriendlyName, @Odyssey_OH, @DistributionZone, @EmailAddress, @Password, @IsAirServiceProvider, @AirlineCode, @AirServiceProvider, @AirlinePrefix, @USCustomsRecipient, @AS2_Code, @SCAC_Code, @OwnerCategory, @SystemCategory, @RR, @CC_RequireStatusResponse, @CC_NotificationForInboxRecipient)";
					AddParameter(command, "@PK", client.CC_PK);
					AddParameter(command, "@ID", client.CC_ID);
					AddParameter(command, "@FriendlyName", client.CC_FriendlyName);
					AddParameter(command, "@Odyssey_OH", client.CC_Odyssey_OH);
					AddParameter(command, "@DistributionZone", client.CC_DistributionZone);
					AddParameter(command, "@EmailAddress", client.CC_EmailAddress);
					AddParameter(command, "@Password", client.CC_Password);
					AddParameter(command, "@IsAirServiceProvider", client.CC_IsAirServiceProvider);
					AddParameter(command, "@AirlineCode", client.CC_AirlineCode);
					AddParameter(command, "@AirServiceProvider", client.CC_AirServiceProvider);
					AddParameter(command, "@AirlinePrefix", client.CC_AirlinePrefix);
					AddParameter(command, "@USCustomsRecipient", client.CC_USCustomsRecipient);
					AddParameter(command, "@AS2_Code", client.CC_AS2_Code);
					AddParameter(command, "@SCAC_Code", client.CC_SCAC_Code);
					AddParameter(command, "@OwnerCategory", client.CC_OwnerCategory);
					AddParameter(command, "@SystemCategory", client.CC_SystemCategory);
					AddParameter(command, "@RR", client.CC_RR);
					AddParameter(command, "@CC_RequireStatusResponse", client.CC_RequireStatusResponse);
					AddParameter(command, "@CC_NotificationForInboxRecipient", client.CC_NotificationForInboxRecipient);
					command.ExecuteNonQuery();
				}
			}

			return client;
		}

		protected eHubMessageType AddMessageType(eHubMessageType messageType)
		{
			using (var command = eHubArchiveOnlineViewConnection.CreateCommand())
			{
				command.Transaction = eHubArchiveOnlineViewTransaction;
				command.CommandText = @"
INSERT INTO [dbo].[eHubMessageType]
           ([DT_PK]
           ,[DT_Code]
           ,[DT_IsFlatFile]
           ,[DT_IsEDI]
           ,[DT_ReprocessSubMessage]
           ,[DT_PostAssembleMapping]
		   ,[DT_IsJson])
VALUES (@PK, @Code, @IsFlatFile, @IsEDI, @ReprocessSubMessage, @PostAssembleMapping, @DT_IsJson)";
				AddParameter(command, "@PK", messageType.DT_PK);
				AddParameter(command, "@Code", messageType.DT_Code);
				AddParameter(command, "@IsEDI", messageType.DT_IsEDI);
				AddParameter(command, "@IsFlatFile", messageType.DT_IsFlatFile);
				AddParameter(command, "@PostAssembleMapping", messageType.DT_PostAssembleMapping);
				AddParameter(command, "@ReprocessSubMessage", messageType.DT_ReprocessSubMessage);
				AddParameter(command, "@DT_IsJson", messageType.DT_IsJson);
				command.ExecuteNonQuery();
			}
			return messageType;
		}

		protected void AddMessageReferenceRegistry(eHubMessageReferenceRegistry referenceRegistry)
		{
			using (var command = eHubTransactionsConnection.CreateCommand())
			{
				command.Transaction = eHubTransactionsTransaction;
				command.CommandText = @"
INSERT INTO [dbo].[eHubMessageReferenceRegistry] ([CR_PK], [CR_CC_Client], [CR_ApplicationCode], [CR_MessageReference], [CR_Password])
VALUES (@PK, @Client, @ApplicationCode, @Reference, @Password)
";
				AddParameter(command, "@PK", referenceRegistry.CR_PK);
				AddParameter(command, "@Client", referenceRegistry.CR_CC_Client);
				AddParameter(command, "@ApplicationCode", referenceRegistry.CR_ApplicationCode);
				AddParameter(command, "@Reference", referenceRegistry.CR_MessageReference);
				AddParameter(command, "@Password", referenceRegistry.CR_Password);
				command.ExecuteNonQuery();
			}
		}

		protected void AddUSCustomsRegistry(eHubUSCustomsRegistry USCRegistry)
		{
			using (var command = eHubTransactionsConnection.CreateCommand())
			{
				command.Transaction = eHubTransactionsTransaction;
				command.CommandText = @"
INSERT INTO [dbo].[eHubUSCustomsRegistry] ([ER_PK], [ER_CC_Client], [ER_ApplicationCode], [ER_Name], [ER_Value], [ER_IsProduction])
VALUES (@PK, @Client, @ApplicationCode, @Name, @Value, @IsProduction)
";
				AddParameter(command, "@PK", USCRegistry.ER_PK);
				AddParameter(command, "@Client", USCRegistry.ER_CC_Client);
				AddParameter(command, "@ApplicationCode", USCRegistry.ER_ApplicationCode);
				AddParameter(command, "@Name", USCRegistry.ER_Name);
				AddParameter(command, "@Value", USCRegistry.ER_Value);
				AddParameter(command, "@IsProduction", USCRegistry.ER_IsProduction);
				command.ExecuteNonQuery();
			}
		}

		protected void AddTransformationSet(eHubTransformationSet transformationSet)
		{
			using (var command = eHubArchiveOnlineViewConnection.CreateCommand())
			{
				command.Transaction = eHubArchiveOnlineViewTransaction;
				command.CommandText = @"
INSERT INTO [dbo].[eHubTransformationSet] ([TS_PK], [TS_Name], [TS_CC_Sender], [TS_CC_Recipient], [TS_DT_Source], [TS_XPathPredicate], [TS_BillingInterfaceName], [TS_BillingElement], [TS_BillingXPathSource], [TS_BillingXPathTarget], [TS_BillSender], [TS_BillRecipient], [TS_CC_BillOther], [TS_BillingNumMessagesIncluded], [TS_BillingFee])
VALUES (@PK, @Name, @Sender, @Recipient, @DT_Source, @XPathPredicate, @InterfaceName, @Element, @XPathSource, @XPathTarget, @BillSender, @BillRecipient, @BillOther, @NumMessages, @Fee)
";
				AddParameter(command, "@PK", transformationSet.TS_PK);
				AddParameter(command, "@Name", transformationSet.TS_Name);
				AddParameter(command, "@Sender", transformationSet.TS_CC_Sender);
				AddParameter(command, "@Recipient", transformationSet.TS_CC_Recipient);
				AddParameter(command, "@DT_Source", transformationSet.TS_DTSource);
				AddParameter(command, "@XPathPredicate", transformationSet.TS_XPathPredicate);
				AddParameter(command, "@InterfaceName", transformationSet.TS_BillingInterfaceName);
				AddParameter(command, "@Element", transformationSet.TS_BillingElement);
				AddParameter(command, "@XPathSource", transformationSet.TS_BillingXPathSource);
				AddParameter(command, "@XPathTarget", transformationSet.TS_BillingXPathTarget);
				AddParameter(command, "@BillSender", transformationSet.TS_BillSender);
				AddParameter(command, "@BillRecipient", transformationSet.TS_BillRecipient);
				AddParameter(command, "@BillOther", transformationSet.TS_CC_BillOther);
				AddParameter(command, "@NumMessages", transformationSet.TS_BillingNumMessagesIncluded);
				AddParameter(command, "@Fee", transformationSet.TS_BillingFee);
				command.ExecuteNonQuery();
			}
		}

		protected void AddServiceProvider(eHubServiceProvider serviceProvider)
		{
			using (var command = eHubTransactionsConnection.CreateCommand())
			{
				command.Transaction = eHubTransactionsTransaction;
				command.CommandText = @"
INSERT INTO [dbo].[eHubServiceProvider] ([SP_CC_Service], [SP_CC_Provider], [SP_RR], [SP_PK])
VALUES (@service, @provider, @SP_RR, @SP_PK)
";
				AddParameter(command, "@service", serviceProvider.SP_CC_Service);
				AddParameter(command, "@provider", serviceProvider.SP_CC_Provider);
				AddParameter(command, "@SP_RR", serviceProvider.SP_RR);
				AddParameter(command, "@SP_PK", serviceProvider.SP_PK);
				command.ExecuteNonQuery();
			}
		}

		protected void AddArchiveMessage(eHubArchiveMessage message)
		{
			using (var command = eHubArchiveOnlineConnection.CreateCommand())
			{
				command.Transaction = eHubArchiveOnlineTransaction;
				command.CommandText = @"
DECLARE @AO_PK UNIQUEIDENTIFIER = NEWID()
DECLARE @AI_PK UNIQUEIDENTIFIER = (SELECT AI_PK FROM eHubArchiveInbox WHERE AI_InsertUTC = @ReceivedFromSenderUTC AND AI_TrackingID = @InboxMessageTrackingID)

IF @AI_PK IS NULL
BEGIN
	SET @AI_PK = COALESCE(@EI_PK, NEWID())
	INSERT INTO [eHubArchiveInbox] (
		AI_PK				   ,
		AI_CC_Sender           ,
		AI_CC_Recipient        ,
		AI_DT                  ,
		AI_MessageRaw          ,
		AI_MessageXML          ,
		AI_InsertUTC           ,
		AI_FileNameOverride    ,
		AI_UncompressedLength  ,
		AI_TrackingID          ,
		AI_ApplicationCode     ,
		AI_ErrorMessage        ,
		AI_Status              ,
    AI_EmailSubjectOverride,
	AI_EnvelopeTrackingID)
	VALUES (
		@AI_PK, 
		@CC_SenderInbox, 
		@CC_RecipientInbox, 
		@DT_SenderMessageType, 
		@SenderMessageRaw, 
		@SenderMessageXML, 
		@ReceivedFromSenderUTC, 
		@InboxFileNameOverride, 
		@SenderMessageUncompressedLength, 
		@InboxMessageTrackingID, 
		@ApplicationCode, 
		@ErrorMessage, 
		@Status, 
    @EmailSubjectOverride,
	NEWID())
END

INSERT INTO [eHubArchiveOutbox] (
    AO_PK                     ,
    AO_CC_Sender              ,
    AO_CC_Recipient           ,
    AO_DT                     ,
    AO_SameRawAsInbox         ,
    AO_SameXmlAsInbox         ,
    AO_MessageRaw             ,
    AO_MessageXML             ,
    AO_SentToRecipientUTC     ,
    AO_TrackingID             ,
    AO_FileNameOverride       ,
    AO_UncompressedLength     ,
    AO_InsertUTC              ,
    AO_ErrorMessage           ,
    AO_Status                 ,
    AO_EmailSubjectOverride   ,
    AO_TS                     ,
    AO_BatchEnvelopeTrackingID,
	AO_AI_InsertUTC)
VALUES (
    @AO_PK, 
    @CC_SenderOutbox, 
    @CC_RecipientOutbox, 
    @DT_RecipientMessageType, 
    0,
    0,
    @RecipientMessageRaw, 
    @RecipientMessageXML, 
    @SentToRecipientUTC, 
    @OutboxMessageTrackingID, 
    @OutboxFileNameOverride, 
    @RecipientMessageUncompressedLength, 
    @ReadyForDeliveryUTC, 
    @ErrorMessage, 
    @Status, 
    @EmailSubjectOverride, 
    @TS, 
    @BatchEnvelopeTrackingID,
	@ReceivedFromSenderUTC)

INSERT INTO [eHubArchivePivot] (
    AP_PK                 ,
    AP_ArchivedUTC        ,
    AP_AI                 ,
    AP_AO                 ,
	AP_AI_InsertUTC)
VALUES (@PK, @ArchivedUTC, @AI_PK, @AO_PK, @ReceivedFromSenderUTC)";
				AddParameter(command, "@PK", message.AM_PK);
				AddParameter(command, "@ApplicationCode", message.AM_ApplicationCode);
				AddParameter(command, "@CC_SenderInbox", message.AM_CC_SenderInbox);
				AddParameter(command, "@CC_RecipientOutbox", message.AM_CC_RecipientOutbox);
				AddParameter(command, "@DT_SenderMessageType", message.AM_DT_SenderMessageType);
				AddParameter(command, "@DT_RecipientMessageType", message.AM_DT_RecipientMessageType);
				AddParameter(command, "@SenderMessageRaw", message.AM_SenderMessageRaw);
				AddParameter(command, "@SenderMessageXML", message.AM_SenderMessageXML);
				AddParameter(command, "@RecipientMessageRaw", message.AM_RecipientMessageRaw);
				AddParameter(command, "@RecipientMessageXML", message.AM_RecipientMessageXML);
				AddParameter(command, "@ReceivedFromSenderUTC", message.AM_ReceivedFromSenderUTC);
				AddParameter(command, "@SentToRecipientUTC", message.AM_SentToRecipientUTC);
				AddParameter(command, "@ArchivedUTC", message.AM_ArchivedUTC);
				AddParameter(command, "@ErrorMessage", message.AM_ErrorMessage);
				AddParameter(command, "@Status", message.AM_Status);
				AddParameter(command, "@OutboxMessageTrackingID", message.AM_OutboxMessageTrackingID);
				AddParameter(command, "@EmailSubjectOverride", message.AM_EmailSubjectOverride);
				AddParameter(command, "@InboxFileNameOverride", message.AM_InboxFileNameOverride);
				AddParameter(command, "@SenderMessageUncompressedLength", message.AM_SenderMessageUncompressedLength);
				AddParameter(command, "@RecipientMessageUncompressedLength", message.AM_RecipientMessageUncompressedLength);
				AddParameter(command, "@BillingElementCount", message.AM_BillingElementCount);
				AddParameter(command, "@TS", message.AM_TS);
				AddParameter(command, "@OutboxFileNameOverride", message.AM_OutboxFileNameOverride);
				AddParameter(command, "@ReadyForDeliveryUTC", message.AM_ReadyForDeliveryUTC);
				AddParameter(command, "@EI_PK", message.AM_EI_PK);
				AddParameter(command, "@InboxMessageTrackingID", message.AM_InboxMessageTrackingID);
				AddParameter(command, "@BatchEnvelopeTrackingID", message.AM_BatchEnvelopeTrackingID);
				AddParameter(command, "@CC_RecipientInbox", message.AM_CC_RecipientInbox);
				AddParameter(command, "@CC_SenderOutbox", message.AM_CC_SenderOutbox);
				command.ExecuteNonQuery();
			}
		}

		protected void AddClientRegistration(eHubClientRegistration eHubClientRegistration)
		{
			using (var command = eHubTransactionsConnection.CreateCommand())
			{
				command.Transaction = eHubTransactionsTransaction;
				command.CommandText = @"
INSERT INTO [dbo].[eHubClientRegistration] ([CX_PK], [CX_CC], [CX_RT], [CX_Code], [CX_Qualifier], [CX_Attr1], [CX_Password1], 
[CX_Flag1], [CX_Flag2], [CX_ConfigXml], [CX_IssuedUTC], [CX_ExpiryUTC])
VALUES (@CX_PK, @CX_CC, @CX_RT, @CX_Code, @CX_Qualifier, @CX_Attr1, @CX_Password1, @CX_Flag1, @CX_Flag2,  @CX_ConfigXml, @CX_IssuedUTC, @CX_ExpiryUTC)
";
				AddParameter(command, "@CX_PK", eHubClientRegistration.CX_PK);
				AddParameter(command, "@CX_CC", eHubClientRegistration.CX_CC);
				AddParameter(command, "@CX_RT", eHubClientRegistration.CX_RT);
				AddParameter(command, "@CX_Code", eHubClientRegistration.CX_Code);
				AddParameter(command, "@CX_Qualifier", eHubClientRegistration.CX_Qualifier);
				AddParameter(command, "@CX_Attr1", eHubClientRegistration.CX_Attr1);
				AddParameter(command, "@CX_Password1", eHubClientRegistration.CX_Password1);
				AddParameter(command, "@CX_Flag1", eHubClientRegistration.CX_Flag1);
				AddParameter(command, "@CX_Flag2", eHubClientRegistration.CX_Flag2);
				AddParameter(command, "@CX_ConfigXml", eHubClientRegistration.CX_ConfigXml);
				AddParameter(command, "@CX_IssuedUTC", eHubClientRegistration.CX_IssuedUTC);
				AddParameter(command, "@CX_ExpiryUTC", eHubClientRegistration.CX_ExpiryUTC);
				command.ExecuteNonQuery();
			}
		}

		protected void AddRegistrationType(eHubRegistrationType eHubRegistrationType)
		{
			using (var command = eHubTransactionsConnection.CreateCommand())
			{
				command.Transaction = eHubTransactionsTransaction;
				command.CommandText = @"
INSERT INTO [dbo].[eHubRegistrationType] ([RT_PK], [RT_ID], [RT_Description], [RT_RegistrantType])
VALUES (@RT_PK, @RT_ID, @RT_Description, @RT_RegistrantType)
";
				AddParameter(command, "@RT_PK", eHubRegistrationType.RT_PK);
				AddParameter(command, "@RT_ID", eHubRegistrationType.RT_ID);
				AddParameter(command, "@RT_Description", eHubRegistrationType.RT_Description);
				AddParameter(command, "@RT_RegistrantType", eHubRegistrationType.RT_RegistrantType);
				command.ExecuteNonQuery();
			}
		}



		protected string GetMessageRaw(string messageFileName, string extension)
		{
			return EncoderDecoder.CompressAndEncode(GetMessageStream(messageFileName, extension)).ReadToEnd();
		}

		protected string GetMessageXml(string messageFileName)
		{
			return GetMessageStream(messageFileName, "xml").ReadToEnd();
		}

		protected static IEnumerable<TimeStampedTransaction> RunPlugin(DateTime start, DateTime end)
		{
			var plugin = new TPlugin();
			plugin.UpdateSettings(new PluginSettings
			{
				Parameters = new[]
				{
					new PluginParameter("ConnectionString", SqlServerHelper.GetAdminConnectionString("eHubArchiveOnlineView")),
					new PluginParameter("LogTransactionExceptions", "False")
				}
			});
			return plugin.GetTransactions(start, end);
		}

		static void AddParameter(SqlCommand command, string parameterName, object value)
		{
			command.Parameters.AddWithValue(parameterName, value ?? DBNull.Value);
		}

		static void InvokeAll(params Action[] actions)
		{
			var exceptions = new List<Exception>();
			foreach (var action in actions)
			{
				try
				{
					action();
				}
				catch (Exception e)
				{
					exceptions.Add(e);
				}
			}
			if (exceptions.Count > 0)
			{
				throw new AggregateException(exceptions);
			}
		}

		Stream GetMessageStream(string messageFileName, string extension)
		{
			return GetType().Assembly.GetManifestResourceStream(GetType(), "Messages." + messageFileName + "." + extension);
		}

		SqlConnection eHubTransactionsConnection;
		SqlTransaction eHubTransactionsTransaction;
		SqlConnection eHubArchiveOnlineConnection;
		SqlTransaction eHubArchiveOnlineTransaction;
		SqlConnection eHubArchiveOnlineViewConnection;
		SqlTransaction eHubArchiveOnlineViewTransaction;
	}
}

using System.Text;
using System.Xml.Linq;
using eServices.Dms.Core.ServiceDefaults;
using eServices.eHubDataModel.eHubTransactionsCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace eServices.Dms.Core.MessagesRepository;

public class DmsMessagesRepository_V0_1(eHubTransactionsContext context, IConfiguration configuration) : IDmsMessagesRepository_V0_1
{
	private string UriFormat = configuration["MessagesUriFormat_V0_1"]
		?? throw new InvalidOperationException("Missing configuration setting: MessagesUriFormat_V0_1");

	public async Task<IDmsMessageMetadata> PutMessageAsync(IDmsMessageMetadata headers, Stream body)
	{
		var id = Guid.NewGuid();
		using var bodyReader = new StreamReader(body, leaveOpen: true);

		var parentId = headers.TryGetValue(DmsHeaders.MessageParentId, out var parentIdHdr)
			&& Guid.TryParse(parentIdHdr.ToString(), out var parentIdGuid) ? parentIdGuid : default;

		var parameters = new List<SqlParameter>
		{
			new SqlParameter("@id", id),
			new SqlParameter("@senderId", headers[DmsHeaders.MessageSenderId].ToString()),
			new SqlParameter("@recipientId", headers.TryGetValue(DmsHeaders.MessageRecipientId, out var recipientId) ? recipientId.ToString() : DBNull.Value),
			new SqlParameter("@appCode", headers.TryGetValue(DmsHeaders.MessageAppCode, out var appCode) ? appCode.ToString() : "001"),
			new SqlParameter("@msgType", headers.TryGetValue(DmsHeaders.MessageType, out var msgType) ? msgType.ToString() : DBNull.Value),
			new SqlParameter("@trackingId", headers.TryGetValue(DmsHeaders.MessageTrackingId, out var trackingId) ? trackingId.ToString() : Guid.NewGuid().ToString()),
			new SqlParameter("@parentId", parentId.ToString()),
			new SqlParameter("@description", headers.TryGetValue(DmsHeaders.MessageDescription, out var description) ? description.ToString() : DBNull.Value),
			new SqlParameter("@fileName", headers.TryGetValue(DmsHeaders.MessageFileName, out var fileName) ? fileName.ToString() : DBNull.Value),
			new SqlParameter("@content", System.Data.SqlDbType.VarChar, -1) { Value = bodyReader }
		};

		if (parentId != default)
		{
			if (!(await context.eHubInboxMessage.AnyAsync(i => i.EI_PK == parentId)))
				throw new InvalidOperationException($"Parent message not found: {parentId}");

			await context.Database.ExecuteSqlRawAsync("""
			BEGIN TRANSACTION;
			INSERT INTO eHubInboxMessage
				(EI_PK
				,EI_CC_Sender
				,EI_CC_Recipient
				,EI_ApplicationCode
				,EI_MessageType
				,EI_MessageTrackingID
				,EI_EnvelopeTrackingID
				,EI_EmailSubjectOverride
				,EI_FileNameOverride
				,EI_IsFlatFile
				,EI_InsertUTC
				,EI_LastUpdateUTC
				,EI_Status)
			VALUES
				(@id
				,(SELECT CC_PK FROM eHubClient WHERE CC_ID=@senderId)
				,(SELECT CC_PK FROM eHubClient WHERE CC_ID=@recipientId)
				,@appCode
				,'eHubBatchMessage'
				,@trackingId
				,'00000000-0000-0000-0000-000000000000'
				,@description
				,@fileName
				,0
				,GETUTCDATE()
				,GETUTCDATE()
				,0);
			INSERT INTO eHubOutboxMessage
				(OI_PK
				,OI_CC_Sender
				,OI_CC_Recipient
				,OI_EI_InboxPK
				,OI_MessageTrackingID
				,OI_OverrideEmailSubject
				,OI_OverrideFilename
				,OI_Status
				,OI_DT_Target
				,OI_Content)
			VALUES
				(@id
				,(SELECT CC_PK FROM eHubClient WHERE CC_ID=@senderId)
				,(SELECT CC_PK FROM eHubClient WHERE CC_ID=@recipientId)
				,@id
				,@trackingId
				,@description
				,@fileName
				,0
				,(SELECT DT_PK FROM eHubMessageType WHERE DT_Code=@msgType)
				,@content);
			UPDATE eHubInboxMessage
				 SET EI_Status = 2
					,EI_LastUpdateUTC = GETUTCDATE()
				WHERE EI_PK = @parentId
			UPDATE eHubOutboxMessage
				 SET OI_Status = 1
					,OI_BatchEnvelopeTrackingID = @trackingId
					,OI_LastUpdateUTC = GETUTCDATE()
				WHERE OI_PK = @parentId
			COMMIT TRANSACTION;
			""", parameters);
		}
		else
		{
			await context.Database.ExecuteSqlRawAsync("""
			BEGIN TRANSACTION;
			INSERT INTO eHubInboxMessage
				(EI_PK
				,EI_CC_Sender
				,EI_CC_Recipient
				,EI_ApplicationCode
				,EI_MessageType
				,EI_MessageTrackingID
				,EI_EnvelopeTrackingID
				,EI_EmailSubjectOverride
				,EI_FileNameOverride
				,EI_IsFlatFile
				,EI_InsertUTC
				,EI_LastUpdateUTC
				,EI_Status)
			VALUES
				(@id
				,(SELECT CC_PK FROM eHubClient WHERE CC_ID=@senderId)
				,(SELECT CC_PK FROM eHubClient WHERE CC_ID=@recipientId)
				,@appCode
				,@msgType
				,@trackingId
				,'00000000-0000-0000-0000-000000000000'
				,@description
				,@fileName
				,0
				,GETUTCDATE()
				,GETUTCDATE()
				,0);
			INSERT INTO eHubOutboxMessage
				(OI_PK
				,OI_CC_Sender
				,OI_CC_Recipient
				,OI_EI_InboxPK
				,OI_MessageTrackingID
				,OI_OverrideEmailSubject
				,OI_OverrideFilename
				,OI_Status
				,OI_DT_Target
				,OI_Content)
			VALUES
				(@id
				,(SELECT CC_PK FROM eHubClient WHERE CC_ID=@senderId)
				,(SELECT CC_PK FROM eHubClient WHERE CC_ID=@recipientId)
				,@id
				,@trackingId
				,@description
				,@fileName
				,0
				,(SELECT DT_PK FROM eHubMessageType WHERE DT_Code=@msgType)
				,@content);
			COMMIT TRANSACTION;
			""", parameters);
		}
		return (await GetMessageMetadataAsync(id))!;
	}

	public async Task<IDmsMessageMetadata?> GetQueuedMessageAsync(string recipientId, Guid? batchId = null)
	{
		var outboxQry = context.eHubOutboxMessage
			.Where(i => i.OI_CC_RecipientNavigation != null && i.OI_CC_RecipientNavigation.CC_ID == recipientId && i.OI_Status == 0);
		if (batchId.HasValue)
			outboxQry = outboxQry.Where(o => o.OI_BatchEnvelopeTrackingID != batchId.ToString());
		var outbox = await outboxQry.OrderBy(i => i.OI_InsertUTC).FirstOrDefaultAsync();

		if (outbox is not null)
		{
			if (batchId.HasValue)
			{
				outbox.OI_BatchEnvelopeTrackingID = batchId.ToString();
				outbox.OI_LastUpdateUTC = DateTime.UtcNow;
				await context.SaveChangesAsync();
			}
			return await GetMessageMetadataAsync(Guid.Parse(outbox.OI_EI_InboxPK!));
		}
		else
		{
			return null;
		}
	}

	public async Task<IDmsMessageMetadata?> GetMessageMetadataAsync(Guid id, bool forceReload = false)
	{
		eHubInboxMessage? inbox = null!;
		eHubOutboxMessage? outbox = null!;

		if (forceReload)
		{
			if (context.ChangeTracker.Entries<eHubInboxMessage>().FirstOrDefault(i => i.Entity.EI_PK == id)
				is var inboxEntity and not null)
			{
				await inboxEntity.ReloadAsync();
				inbox = inboxEntity.Entity;
			}
			if (context.ChangeTracker.Entries<eHubOutboxMessage>().FirstOrDefault(o => o.Entity.OI_PK == id)
				is var outboxEntity and not null)
			{
				await outboxEntity.ReloadAsync();
				outbox = outboxEntity.Entity;
			}
		}

		inbox ??= await context.eHubInboxMessage
			.Include(i => i.EI_CC_SenderNavigation)
			.Include(i => i.EI_CC_RecipientNavigation)
			.FirstOrDefaultAsync(i => i.EI_PK == id);
		outbox ??= await context.eHubOutboxMessage
			.Include(i => i.OI_DT_TargetNavigation)
			.FirstOrDefaultAsync(i => i.OI_EI_InboxPK == id.ToString());

		if (inbox is null || outbox is null)
			return null!;

		var error =
			outbox.OI_Status == 255
				? await context.eHubError.FirstOrDefaultAsync(e => e.EE_OI_Outbox == outbox.OI_PK)
			: inbox.EI_Status == 255
				? await context.eHubError.FirstOrDefaultAsync(e => e.EE_EI_Inbox == inbox.EI_PK)
					: null;

		var headers = new DmsMessageMetadata();
		headers[DmsHeaders.MessageId] = inbox.EI_PK.ToString();
		headers[DmsHeaders.MessageUri] = string.Format(UriFormat, inbox.EI_PK);
		headers[DmsHeaders.MessageTimeReceived] = inbox.EI_InsertUTC.ToString("s");
		headers[DmsHeaders.MessageTimeLastUpdate] = inbox.EI_LastUpdateUTC.ToString("s");
		headers[DmsHeaders.MessageSenderId] = inbox.EI_CC_SenderNavigation.CC_ID;
		if (inbox.EI_CC_RecipientNavigation is not null)
			headers[DmsHeaders.MessageRecipientId] = inbox.EI_CC_RecipientNavigation.CC_ID;
		headers[DmsHeaders.MessageAppCode] = inbox.EI_ApplicationCode;
		if (outbox.OI_DT_TargetNavigation?.DT_Code is not null)
			headers[DmsHeaders.MessageType] = outbox.OI_DT_TargetNavigation.DT_Code;
		else if (inbox.EI_MessageType is not null)
			headers[DmsHeaders.MessageType] = inbox.EI_MessageType;
		headers[DmsHeaders.MessageTrackingId] = outbox.OI_MessageTrackingID;
		if (inbox.EI_EnvelopeTrackingID != Guid.Empty.ToString())
			headers[DmsHeaders.MessageParentId] = inbox.EI_EnvelopeTrackingID;
		if (outbox.OI_BatchEnvelopeTrackingID is not null)
			headers[DmsHeaders.MessageBatchId] = outbox.OI_BatchEnvelopeTrackingID;
		if (inbox.EI_EmailSubjectOverride is not null)
			headers[DmsHeaders.MessageDescription] = inbox.EI_EmailSubjectOverride;
		if (inbox.EI_FileNameOverride is not null)
			headers[DmsHeaders.MessageFileName] = inbox.EI_FileNameOverride;
		headers[DmsHeaders.MessageStatus] = (inbox.EI_Status, outbox.OI_Status) switch
		{
			(0, 0) => DmsStatus.Received,
			(1, 0) => DmsStatus.Processing,
			(2, 1) => DmsStatus.Processed,
			(3, 3) => DmsStatus.Delivered,
			(255, 255) => DmsStatus.Failed,
			_ => "Undefined"
		};
		if (inbox.EI_CC_SenderNavigation.CC_RequireStatusResponse)
			headers[DmsHeaders.MessageAckReqd] = "true";
		if (error is not null)
		{
			if (error.EE_Description is not null)
				headers[DmsHeaders.MessageError] = error.EE_Description;
			if (error.EE_ErrorDetail is not null
				&& XElement.Parse(error.EE_ErrorDetail).Element("IssueManagerReportId") is var reportId and not null)
				headers[DmsHeaders.MessageIssueManagerReportId] = reportId.Value;
		}

		return headers;
	}

	public async Task<Stream?> GetMessageContentAsync(Guid id)
	{
		var connection = context.Database.GetDbConnection();
		if (connection.State != System.Data.ConnectionState.Open)
			await connection.OpenAsync();
		using var command = connection.CreateCommand();
		command.Transaction = transaction?.GetDbTransaction();
		command.CommandText = "SELECT OI_Content FROM eHubOutboxMessage WHERE OI_PK=@id";
		command.Parameters.Add(new SqlParameter("@id", id));
		using var reader = await command.ExecuteReaderAsync();

		return await reader.ReadAsync() && !(await reader.IsDBNullAsync(0))
			? new TextReaderStream(reader.GetTextReader(0), Encoding.ASCII)
			: null;
	}

	public async Task<IDmsMessageMetadata?> PatchMessageMetadataAsync(Guid id, IDmsMessageMetadata headers)
	{
		var currentHeaders = await GetMessageMetadataAsync(id);

		if (currentHeaders is null)
			return null;

		var trackingId = currentHeaders[DmsHeaders.MessageTrackingId].ToString();
		var senderId = currentHeaders[DmsHeaders.MessageSenderId].ToString();
		var recipientId = currentHeaders[DmsHeaders.MessageRecipientId].ToString();
		var currentTime = DateTime.UtcNow.ToString("s");

		using var transaction = context.Database.CurrentTransaction is null
			? await context.Database.BeginTransactionAsync() : null;

		switch (headers[DmsHeaders.MessageStatus])
		{
			case DmsStatus.Delivered:
				if (currentHeaders[DmsHeaders.MessageStatus] == DmsStatus.Delivered)
					return currentHeaders;
				await context.Database.ExecuteSqlAsync($"""
					EXEC dbo.UpdateMessageDistributionStatus
						@MessageTrackingID={trackingId},
						@SenderID={senderId},
						@RecipientID={recipientId}
					""");
				break;
			case DmsStatus.Failed:
				if (currentHeaders[DmsHeaders.MessageStatus] == DmsStatus.Delivered)
					return currentHeaders;
				var errorId = Guid.NewGuid();
				var errorDesc = headers[DmsHeaders.MessageError].ToString();
				var errorDetail = new XElement("ErrorDetail",
					new XElement("IssueManagerReportId", headers[DmsHeaders.MessageIssueManagerReportId]!))
					.ToString();
				await context.Database.ExecuteSqlAsync($"""
					EXEC [dbo].[InsertError]
						@ErrorPK={errorId},
						@Source='001',
						@ErrorType='FAI',
						@Description={errorDesc},
						@ErrorDetail={errorDetail},
						@InboxPK={id},
						@OutboxPK={id},
						@CurrentDateTimeUTC={currentTime},
						@Alerted=1
					""");
				break;
			case DmsStatus.Processing:
			case DmsStatus.Processed:
				(int Inbox, int Outbox) status = headers[DmsHeaders.MessageStatus].ToString() switch
				{
					DmsStatus.Processing => (1, 0),
					DmsStatus.Processed => (2, 0),
					_ => throw new InvalidOperationException($"""
						Invalid value for message metadata:
						{DmsHeaders.MessageStatus}: {headers[DmsHeaders.MessageStatus]}
						""")
				};
				await context.Database.ExecuteSqlAsync($"""
					EXEC dbo.[UpdateMessageStatus]
						@InboxPK={id},
						@OutboxPK={id},
						@InboxStatus={status.Inbox},
						@OutboxStatus={status.Outbox}
					""");
				break;
			default:
				throw new InvalidOperationException($"""
					Invalid value for message metadata:
					{DmsHeaders.MessageStatus}: {currentHeaders[DmsHeaders.MessageStatus]}
					""");
		}

		if (transaction is not null)
			await transaction.CommitAsync();

		return (await GetMessageMetadataAsync(id, true))!;
	}

	public async Task<IDbContextTransaction> BeginTransactionAsync()
	{
		return transaction = await context.Database.BeginTransactionAsync();
	}

	public async Task CommitTransactionAsync()
	{
		if (transaction is not null)
			await transaction.CommitAsync();
	}

	private IDbContextTransaction? transaction;

	public async ValueTask DisposeAsync()
	{
		if (transaction is not null)
			await transaction.DisposeAsync();
	}
}



namespace eServices.Dms.Core.MessagesRepository;

public interface IDmsMessagesRepository_V0_1 : IAsyncDisposable
{
	public Task<IDmsMessageMetadata> PutMessageAsync(IDmsMessageMetadata headers, Stream body);
	public Task<IDmsMessageMetadata?> GetQueuedMessageAsync(string recipientId, Guid? batchId = null);
	public Task<IDmsMessageMetadata?> GetMessageMetadataAsync(Guid id, bool forceReload = false);
	public Task<Stream?> GetMessageContentAsync(Guid trackingId);
	public Task<IDmsMessageMetadata?> PatchMessageMetadataAsync(Guid id, IDmsMessageMetadata headers);
}

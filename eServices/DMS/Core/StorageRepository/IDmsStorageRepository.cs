using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore.Storage;

namespace eServices.Dms.Core.StorageRepository;

public interface IDmsStorageRepository : IAsyncDisposable
{
	Task<int> RegisterOwnerAsync(string owner);
	Task<bool> TableExistsAsync(string owner, string tableName, DmsTableTypes? type = null);
	Task<DmsStorageCatalog> CreateTableAsync(DmsStorageCatalog table);
	Task<DmsStorageCatalog> DeleteTableAsync(DmsStorageCatalog table);
	Task<JsonNode?> GetDocumentAsJsonAsync(string owner, string tableName, string key);
	Task<int> PutDocumentAsJsonAsync(string owner, string tableName, string key, JsonNode document, string user);
	Task<Stream?> GetObjectAsStreamAsync(string owner, string tableName, string key);
	Task<int> PutObjectAsStreamAsync(string owner, string tableName, string key, Stream blob, string user);
	Task<IDbContextTransaction> BeginTransactionAsync();
	Task CommitTransactionAsync();
}
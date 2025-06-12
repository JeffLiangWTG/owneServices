using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using OcmPoc.Infrastructure.MessageInterfaces.Documents;
using OcmPoc.Infrastructure.MessageInterfaces.Repositories;

namespace OcmPoc.Infrastructure.MessageRepository.Mongo
{
	public class CompressedMessageRepository : IMessageRepository
	{
		readonly IMessageRepository decoratedRepository;
		readonly int compressionThreshold = 1 << 15;

		public CompressedMessageRepository(IMessageRepository decoratedRepository)
		{
			this.decoratedRepository = decoratedRepository;
		}

		public async Task<Message> RetrieveAsync(Guid id)
		{
			var message = await decoratedRepository.RetrieveAsync(id);

			if (message.IsCompressed)
			{
				message = await DecompressAsync(message);
			}

			return message;
		}

		public async Task<Guid> StoreAsync(Message message)
		{
			if (message.Body.Length > compressionThreshold && !message.IsCompressed)
			{
				message = await CompressAsync(message);
			}

			await decoratedRepository.StoreAsync(message);
			return message.Id;
		}

		async Task<Message> CompressAsync(Message message)
		{
			using (var uncompressed = new MemoryStream(message.Body))
			using (var compressed = new MemoryStream())
			{
				using (var compressor = new DeflateStream(compressed, CompressionLevel.Fastest))
				{
					await uncompressed.CopyToAsync(compressor);
					await compressor.FlushAsync();
				}

				message.Body = compressed.ToArray();
				message.IsCompressed = true;
			}

			return message;
		}

		async Task<Message> DecompressAsync(Message message)
		{
			using (var compressed = new MemoryStream(message.Body))
			using (var uncompressed = new MemoryStream())
			{
				using (var decompressor = new DeflateStream(compressed, CompressionMode.Decompress))
				{
					await decompressor.CopyToAsync(uncompressed);
					await uncompressed.FlushAsync();
				}

				message.Body = uncompressed.ToArray();
				message.IsCompressed = false;
			}

			return message;
		}
	}
}

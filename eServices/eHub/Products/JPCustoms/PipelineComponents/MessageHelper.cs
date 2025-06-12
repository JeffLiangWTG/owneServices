using System.IO;
using CargoWise.eHub.Common.Extensions;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;

namespace CargoWise.eHub.Products.JPCustoms.PipelineComponents
{
	public class MessageHelper
	{
		public static void WrapMessageInReadOnlySeekableStream(IPipelineContext pipelineContext, IBaseMessage message)
		{
			var stream = message.BodyPart.GetOriginalDataStream();
			if (!stream.CanSeek)
			{
				var seekableStream = new ReadOnlySeekableStream(stream);
				if (pipelineContext != null) pipelineContext.ResourceTracker.AddResource(seekableStream);
				seekableStream.Position = 0;
				message.BodyPart.Data = seekableStream;
			}
		}

		public static void AddStreamAsABody(IPipelineContext pipelineContext, IBaseMessage message, Stream stream)
		{
			var seekableStream = new ReadOnlySeekableStream(stream);
			seekableStream.SeekBegin();
			if (pipelineContext != null) pipelineContext.ResourceTracker.AddResource(seekableStream);
			message.BodyPart.Data = seekableStream;
		}

		public static void CompressAndEncodeStream(IPipelineContext pipelineContext, IBaseMessage message)
		{
			var compressedAndEncodedStream = message.BodyPart.Data.CompressAndEncode();
			compressedAndEncodedStream.Position = 0;
			var seekableStream = new ReadOnlySeekableStream(compressedAndEncodedStream);
			if (pipelineContext != null) pipelineContext.ResourceTracker.AddResource(seekableStream);
			message.BodyPart.Data = seekableStream;
		}

		public static void DecodeAndDecompressStream(IPipelineContext pipelineContext, IBaseMessage message)
		{
			var decodeAndDecompressStream = message.BodyPart.Data.DecodeAndDecompress();
			decodeAndDecompressStream.Position = 0;
			var seekableStream = new ReadOnlySeekableStream(decodeAndDecompressStream);
			if (pipelineContext != null) pipelineContext.ResourceTracker.AddResource(seekableStream);
			message.BodyPart.Data = seekableStream;
		}

		public static void ReadMessage(IPipelineContext context, IBaseMessage message)
		{
			WrapMessageInReadOnlySeekableStream(context, message);
			new StreamReader(message.BodyPart.Data).ReadToEnd();
			message.BodyPart.Data.Position = 0;
		}
	}
}

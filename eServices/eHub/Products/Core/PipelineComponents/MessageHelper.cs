using System;
using System.IO;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Products.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;

namespace CargoWise.eHub.Products.Core.PipelineComponents
{
	public class MessageHelper
	{
		public static Guid GetMessageTrackingID(IBaseMessage message)
		{
			Guid trackingID = Guid.Empty;

			string trackingIDString = message.Context.ReadPropertyString<MessageTrackingID>();

			if (!string.IsNullOrEmpty(trackingIDString))
			{
				Guid.TryParse(trackingIDString, out trackingID);
			}

			if (trackingID == Guid.Empty)
			{
				trackingID = Guid.NewGuid();
			}

			return trackingID;
		}

		public static void ReadMessage(IPipelineContext context, IBaseMessage message)
		{
			WrapMessageInReadOnlySeekableStream(context, message);
			new StreamReader(message.BodyPart.Data).ReadToEnd();
			message.BodyPart.Data.Position = 0;
		}

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

		public static void EncodeStream(IPipelineContext pipelineContext, IBaseMessage message)
		{
			var encodedStream = message.BodyPart.Data.EncodeStream();
			encodedStream.Position = 0;
			var seekableStream = new ReadOnlySeekableStream(encodedStream);
			if (pipelineContext != null) pipelineContext.ResourceTracker.AddResource(seekableStream);
			message.BodyPart.Data = seekableStream;
		}
	}
}

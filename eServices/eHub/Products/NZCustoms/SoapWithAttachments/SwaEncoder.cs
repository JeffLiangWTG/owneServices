using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using CargoWise.eHub.Products.NZCustoms.Common;
using CargoWise.eHub.Products.NZCustoms.SoapWithAttachments.Mime;
using Common.Logging;

namespace CargoWise.eHub.Products.NZCustoms.SoapWithAttachments
{
	public class SwaEncoder : MessageEncoder
	{
		readonly ILog logger = LogManager.GetLogger(typeof(SwaEncoder));
		readonly MessageEncoder baseEncoder;

		public SwaEncoder(MessageEncoder encoder)
		{
			baseEncoder = encoder;
		}

		public virtual MimeParser MimeParser
		{
			get
			{
				return new MimeParser();
			}
		}


		public override string ContentType
		{
			get
			{
				VerifyOperationContext();

				if (OperationContext.Current.OutgoingMessageProperties.ContainsKey(SwaEncoderConstants.AttachmentProperty))
				{
					return MimeContent.ContentType;
				}

				return baseEncoder.ContentType;
			}
		}

		public override string MediaType { get { return MimeContent.MediaType; } }

		public override MessageVersion MessageVersion
		{
			get { return MessageVersion.Soap11; }
		}

		public override bool IsContentTypeSupported(string contentType)
		{
			if (contentType.ToLower().StartsWith(MimeContent.MediaType))
				return true;
			if (contentType.ToLower().StartsWith(MimePart.SoapPartContentType))
				return true;
			return false;
		}

		public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
		{
			VerifyOperationContext();

			byte[] msgContents = new byte[buffer.Count];
			Array.Copy(buffer.Array, buffer.Offset, msgContents, 0, msgContents.Length);
			bufferManager.ReturnBuffer(buffer.Array);
			var contentStream = new MemoryStream(msgContents);
			return ReadMessage(contentStream, int.MaxValue, contentType);
		}

		public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
		{
			VerifyOperationContext();

			if (contentType.ToLower().StartsWith(MimeContent.MediaType))
			{
				try
				{
					byte[] contentBytes = new byte[stream.Length];
					stream.Read(contentBytes, 0, contentBytes.Length);
					MimeContent content = MimeParser.DeserializeMimeContent(contentType, contentBytes);

					var startPartContent = new MemoryStream(content.StartPart.Content);

					Message msg = ReadMessage(startPartContent, int.MaxValue, content.StartPart.ContentType);

					int attachmentCounter = 1;

					foreach (var part in content.Parts)
					{
						if (part != content.StartPart)
						{
							msg.Properties.Add(string.Format(SwaEncoderConstants.AttachmentPropertyFormat, SwaEncoderConstants.AttachmentNameProperty, attachmentCounter), part.ContentId);
							msg.Properties.Add(string.Format(SwaEncoderConstants.AttachmentPropertyFormat, SwaEncoderConstants.AttachmentTypeProperty, attachmentCounter), part.ContentType);
							msg.Properties.Add(string.Format(SwaEncoderConstants.AttachmentPropertyFormat, SwaEncoderConstants.AttachmentProperty, attachmentCounter), part.Content);
							attachmentCounter++;
						}
					}
					msg.Properties.Add(SwaEncoderConstants.AttachmentCount, attachmentCounter);
					return msg;
				}
				catch(Exception ex)
				{
					stream.Position = 0;
					logger.ErrorFormat($"Unexpected Exception: {ex.ToString()}, {System.Environment.NewLine} Raw Message: {System.Environment.NewLine} {new StreamReader(stream).ReadToEnd()}");
					throw;
				}
			}
			else if (contentType.ToLower().StartsWith(MimePart.SoapPartContentType))
			{
				XmlReader reader = XmlReader.Create(stream);
				return Message.CreateMessage(reader, maxSizeOfHeaders, MessageVersion);
			}
			else
			{
				var messageText = new StreamReader(stream).ReadToEnd();

				throw new ApplicationException(
				string.Format(
					"Invalid content type for reading message: {0}! Supported content types are multipart/related and text/xml. Message Details: {1}", contentType, messageText));
			}
		}

		public override void WriteMessage(Message message, Stream stream)
		{
			throw new NotImplementedException("Write Message Short version not supported");
		}

		public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
		{
			VerifyOperationContext();
			message.Properties.Encoder = baseEncoder;

			ArraySegment<byte> messageArray = baseEncoder.WriteMessage(message, maxMessageSize, bufferManager, messageOffset);

			byte[] attachment = GetAttachment();
			if(attachment != null) logger.DebugFormat("Attachment size SWA {0}", attachment.Length);

			if (attachment == null) return messageArray;

			var mimeContent = CreateMimeContentWithSOAPPart();
			mimeContent.StartPart.Content = messageArray.Array.Skip(messageArray.Offset).Take(messageArray.Count).ToArray();

			logger.DebugFormat("Count log 1. {0}", mimeContent.ToString());
			AddAttachments(mimeContent, attachment);
			logger.DebugFormat("Count log 2. {0}", mimeContent.ToString());
			byte[] mimeContentBytes = MimeParser.SerializeMimeContent(mimeContent);
			logger.DebugFormat("Count log 3. {0}", mimeContent.ToString());

			// Write the mime content into the section of the buffer passed into the method
			byte[] targetBuffer = bufferManager.TakeBuffer(mimeContentBytes.Length + messageOffset);
			Array.Copy(mimeContentBytes, 0, targetBuffer, messageOffset, mimeContentBytes.Length);

			// Return the segment of the buffer to the framework
			return new ArraySegment<byte>(targetBuffer, messageOffset, mimeContentBytes.Length);
		}

		void AddAttachments(MimeContent mimeContent, byte[] attachment)
		{
			mimeContent.ClearPartsExceptStartOne();

			var memoryStream = new MemoryStream(attachment);
			var documents = LodgementDocumentCollection.Create(logger, memoryStream);

			foreach (var document in documents)
			{
				if (document.DocumentType == Constants.DeclarationDocumentType)
				{
					var atachmentMimePart = CreateDeclarationAttacmentMimePart();
					atachmentMimePart.Content = document.Content;
					mimeContent.AddPart(atachmentMimePart);

				}
				else
				{
					var atachmentMimePart = CreateAttacmentMimePart(document);
					atachmentMimePart.Content = document.Content;
					mimeContent.AddPart(atachmentMimePart);
				}
			}
		}

		protected MimePart CreateDeclarationAttacmentMimePart()
		{
			return new MimePart("application/octet-stream", Constants.DeclarationContentId);
		}

		protected virtual MimePart CreateAttacmentMimePart(LodgementDocument attachment)
		{
			return new MimePart(attachment.DocumentMediaType, "binary", attachment.FileName);
		}

		protected virtual MimeContent CreateMimeContentWithSOAPPart()
		{
			return MimeContent.CreateWithSoapPart();
		}

		protected virtual byte[] GetAttachment()
		{
			if (OperationContext.Current.OutgoingMessageProperties.ContainsKey(SwaEncoderConstants.AttachmentProperty))
			{
				return (byte[])OperationContext.Current.OutgoingMessageProperties[SwaEncoderConstants.AttachmentProperty];
			}

			return null;
		}

		protected virtual void VerifyOperationContext()
		{
			if (OperationContext.Current == null)
			{
				throw new ApplicationException
				(
					"No current OperationContext available! On clients please use OperationScope as follows to establish " +
					"an operation context: " + Environment.NewLine + Environment.NewLine +
					"using(OperationScope Scope = new OperationScope(YourProxy.InnerChannel) { YouProxy.MethodCall(...); }"
				);
			}
			if (OperationContext.Current.OutgoingMessageProperties.ContainsKey(SwaEncoderConstants.AttachmentProperty))
			{
				if (OperationContext.Current.OutgoingMessageProperties[SwaEncoderConstants.AttachmentProperty] != null)
				{
					if (!(OperationContext.Current.OutgoingMessageProperties[SwaEncoderConstants.AttachmentProperty] is byte[]))
					{
						throw new ArgumentException(string.Format(
							"OperationContext.Current.OutgoingMessageProperties[\"{0}\"] needs to be a byte[] array with the attachment content!",
							SwaEncoderConstants.AttachmentProperty));
					}
				}
			}
		}
	}
}

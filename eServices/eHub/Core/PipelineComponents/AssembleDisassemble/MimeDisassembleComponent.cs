using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using Common.Logging;
using Microsoft.BizTalk.Component;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Component.Utilities;
using Microsoft.BizTalk.Edi.Pipelines;
using Microsoft.BizTalk.Message.Interop;
using MimeKit;
using MimeKit.Cryptography;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace CargoWise.eHub.Core.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_DisassemblingParser)]
	[Guid("B7850691-079A-4348-94ED-32B38273C056")]
	public class MimeDisassembleComponent : IBaseComponent, IComponentUI, IDisassemblerComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return "Disassemble email messages by filename, content-type, and index with support of MIME, SMIME, PGP/MIME, and PGP/Inline in XML, X12, EDIFACT, and FlatFile formats enhanced by byte and regex replacements"; }
		}

		public string Name
		{
			get { return "Mime Disassembler"; }
		}

		public string Version
		{
			get { return "1.0"; }
		}

		#endregion

		#region IComponentUI Members

		public IntPtr Icon
		{
			get { return IntPtr.Zero; }
		}

		public IEnumerator Validate(object projectSystem)
		{
			return null;
		}

		#endregion

		#region IDisassemblerComponent Members

		ILog logger;

		internal static Func<IBaseMessage, ILog> GetLogger = (pInMsg) => LoggerHelpers.GetPipelineLogger(pInMsg);

		public void Disassemble(IPipelineContext pContext, IBaseMessage pInMsg)
		{
			if (Enable)
			{
				logger = GetLogger(pInMsg);

				try
				{
					LoggerHelpers.LogComponentStart(logger, this);
					logger.InfoFormat("Processing message with BizTalk MessageID: {0}", pInMsg.MessageID);

					if (logger.IsDebugEnabled)
					{
						LoggerHelpers.LogProperties(logger, this);
					}

					if (logger.IsTraceEnabled)
					{
						LoggerHelpers.LogMessageContextProperties(pInMsg, logger);
					}

					if (!BypassMimeDecoder)
					{
						pInMsg.BodyPart.GetOriginalDataStream().SeekBegin();
						var message = MimeMessage.Load(pInMsg.BodyPart.GetOriginalDataStream());
						if (!string.IsNullOrEmpty(message.Subject)) pInMsg.Context.Write("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06", message.Subject);

						logger.InfoFormat("MIME message loaded successfully");

						var partList = new List<MimePart>();
						TraverseMimeEntity(message.Body, partList);

						bool hasMatch = false;
						foreach (var part in partList.Where(p => !string.IsNullOrEmpty(p.FileName) && !string.IsNullOrEmpty(FileMask) && Regex.IsMatch(p.FileName, "^" + FileMask.Replace(".", "\\.").Replace("*", ".*?") + "$", RegexOptions.IgnoreCase)))
						{
							logger.InfoFormat("Found match by file mask '{0}': {1}", FileMask, part.FileName);

							ExtractContent(pContext, pInMsg, part);
							hasMatch = true;
						}

						if (FileMaskOnly && !hasMatch)
						{
							var messageTrackingID = pInMsg.Context.ReadPropertyString<MessageTrackingID>();
							if (!string.IsNullOrWhiteSpace(messageTrackingID))
							{
								GetOutboxAccessor().UpdateMessageStatus(messageTrackingID, inboxStatus: 3);
							}
							return;
						}

						if (!hasMatch && !string.IsNullOrEmpty(ContentType))
						{
							foreach (var part in partList.Where(p => Regex.IsMatch(p.ContentType.MimeType, "^" + ContentType.Replace(".", "\\.").Replace("*", ".*?") + "$", RegexOptions.IgnoreCase)))
							{
								logger.InfoFormat("Found match by content type '{0}': {1}", ContentType, part.ContentType.MimeType);

								ExtractContent(pContext, pInMsg, part);
								hasMatch = true;
							}
						}

						if (!hasMatch && partList.ElementAtOrDefault(Index) != null)
						{
							logger.InfoFormat("Found match by index '{0}'", Index);

							ExtractContent(pContext, pInMsg, partList[Index]);
						}
					}
					else
					{
						var memoryStream = new MemoryStream();
						pInMsg.BodyPart.GetOriginalDataStream().SeekBegin();
						pInMsg.BodyPart.GetOriginalDataStream().CopyTo(memoryStream);
						memoryStream.SeekBegin();
						CreateAndEnqueueMessage(pContext, pInMsg, memoryStream, null);
					}
				}
				catch (Exception ex)
				{
					logger.Error("Exception: ", ex);
					throw new ApplicationException(ex.Message + Environment.NewLine + ex.StackTrace);
				}
				finally
				{
					LoggerHelpers.LogComponentEnd(logger, this);
				}
			}
			else
			{
				MessageQueue.Enqueue(pInMsg);
			}
		}

		internal virtual IOutboxAccessor GetOutboxAccessor()
		{
			return DataAccessFactories.NewOutboxAccessorInstance();
		}

		public IBaseMessage GetNext(IPipelineContext pContext)
		{
			if (MessageQueue.Count > 0) return MessageQueue.Dequeue() as IBaseMessage;
			return null;
		}

		Queue MessageQueue
		{
			get { return messageQueue ?? (messageQueue = new Queue()); }
		}
		Queue messageQueue;

		void TraverseMimeEntity(MimeEntity mimeEntity, List<MimePart> partList)
		{
			if (mimeEntity is Multipart multipart)
			{
				if (mimeEntity is MultipartEncrypted multipartEncrypted)
				{
					logger.DebugFormat("Found PGP/MIME");

					using (var ctx = new PgpContext(PgpSecretKeyInBase64, PgpPassphrase))
					{
						TraverseMimeEntity(multipartEncrypted.Decrypt(ctx), partList);
					}
				}
				else
				{
					foreach (var item in multipart)
					{
						TraverseMimeEntity(item, partList);
					}
				}
			}
			else if (mimeEntity is MessagePart messagePart)
			{
				TraverseMimeEntity(messagePart.Message.Body, partList);
			}
			else if (mimeEntity is MimePart part)
			{
				if (mimeEntity is ApplicationPkcs7Mime smimePart && smimePart.SecureMimeType == SecureMimeType.EnvelopedData)
				{
					logger.DebugFormat("Found S/MIME");

					CryptographyContext.Register(typeof(WindowsSecureMimeContext));

					if (smimePart.Decrypt() is Multipart smimeMultipart)
					{
						foreach (var item in smimeMultipart)
						{
							TraverseMimeEntity(item, partList);
						}
					}
				}
				else
				{
					partList.Add(part);
				}
			}
		}

		void ExtractContent(IPipelineContext pContext, IBaseMessage pInMsg, MimePart mimePart)
		{
			if (mimePart is TextPart textPart)
			{
				if (textPart.Text.StartsWith("-----BEGIN PGP MESSAGE-----"))
				{
					logger.DebugFormat("Found PGP/Inline message");

					using (var encrypted = new MemoryStream(Encoding.ASCII.GetBytes(textPart.Text), false))
					{
						var decrypted = new MemoryStream();
						using (var ctx = new PgpContext(PgpSecretKeyInBase64, PgpPassphrase))
						{
							ctx.DecryptTo(encrypted, decrypted);
							decrypted.SeekBegin();
							CreateAndEnqueueMessage(pContext, pInMsg, decrypted, textPart.FileName);
						}
					}
				}
				else if (!string.IsNullOrWhiteSpace(textPart.Text))
				{
					logger.DebugFormat("Found text message");

					var decrypted = new MemoryStream();
					textPart.Content.DecodeTo(decrypted);
					decrypted.SeekBegin();
					CreateAndEnqueueMessage(pContext, pInMsg, decrypted, textPart.FileName);
				}
			}
			else
			{
				if (mimePart.FileName.EndsWith(".pgp"))
				{
					logger.DebugFormat("Found PGP/Inline file: {0}", mimePart.FileName);

					using (var encrypted = new MemoryStream())
					using (var writer = new StreamWriter(encrypted))
					{
						var decrypted = new MemoryStream();
						writer.WriteLine("-----BEGIN PGP MESSAGE-----\n");
						writer.Flush();
						mimePart.Content.Stream.CopyTo(encrypted);
						writer.WriteLine("-----END PGP MESSAGE-----");
						writer.Flush();

						encrypted.SeekBegin();

						using (var ctx = new PgpContext(PgpSecretKeyInBase64, PgpPassphrase))
						{
							ctx.DecryptTo(encrypted, decrypted);
							decrypted.SeekBegin();
							CreateAndEnqueueMessage(pContext, pInMsg, decrypted, Path.GetFileNameWithoutExtension(mimePart.FileName));
						}
					}
				}
				else
				{
					logger.DebugFormat("Found attachment");

					var decrypted = new MemoryStream();
					mimePart.Content.DecodeTo(decrypted);
					decrypted.SeekBegin();
					CreateAndEnqueueMessage(pContext, pInMsg, decrypted, mimePart.FileName);
				}
			}
		}

		void CreateAndEnqueueMessage(IPipelineContext pContext, IBaseMessage pInMsg, MemoryStream dataStream, string filename)
		{
			logger.InfoFormat("Enqueuing message");

			var outMsg = pContext.GetMessageFactory().CreateMessage();

			outMsg.Context = PipelineUtil.CloneMessageContext(pInMsg.Context);
			if (!string.IsNullOrEmpty(filename)) outMsg.Context.Write("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", filename);

			outMsg.AddPart(pInMsg.BodyPartName, pContext.GetMessageFactory().CreateMessagePart(), true);

			if (ByteDecoderEnabled && Regex.IsMatch(ByteDecoderInHex, @"^0x([0-9a-fA-F]{2})+\-(0x([0-9a-fA-F]{2})+)?(\,0x([0-9a-fA-F]{2})+\-(0x([0-9a-fA-F]{2})+)?)*$"))
			{
				outMsg.BodyPart.Data = new MemoryStream(ReplaceBytes(dataStream.ToArray(), ByteDecoderInHex).ToArray());
			}
			else
			{
				outMsg.BodyPart.Data = dataStream;
			}

			if (RegexEnabled)
			{
				Encoding regexEncoding;
				try
				{
					regexEncoding = Encoding.GetEncoding(RegexEncoding);
				}
				catch (ArgumentException)
				{
					regexEncoding = Encoding.UTF8;
				}

				var text = new StreamReader(outMsg.BodyPart.Data, regexEncoding, true).ReadToEnd();

				foreach (var regexAndStringPair in RegexString.Split(new string[] { "??" }, StringSplitOptions.RemoveEmptyEntries))
				{
					var regexAndString = regexAndStringPair.Split(new string[] { "**" }, StringSplitOptions.None);
					text = Regex.Replace(text, regexAndString[0], regexAndString[1]);
				}

				outMsg.BodyPart.Data = new MemoryStream(regexEncoding.GetBytes(text));
			}

			if (logger.IsTraceEnabled)
			{
				LoggerHelpers.LogMessageContextProperties(outMsg, logger);

				logger.TraceFormat("Content in Base64 after byte & regex decoders and before disassemble: {0}", Convert.ToBase64String((outMsg.BodyPart.Data as MemoryStream).ToArray()));
			}

			var xmlDasm = new XmlDasmComp();
			var ediDasm = new EdiDisassembler();
			var ffDasm = new FFDasmComp();
			if (MessageSchemaIsSpecified)
			{
				ffDasm.DocumentSpecName = new SchemaWithNone(MessageSchema.ToString());
			}

			try
			{
				if (xmlDasm.Probe(pContext, outMsg))
				{
					logger.InfoFormat("Found XML message");

					xmlDasm.AllowUnrecognizedMessage = true;
					xmlDasm.Disassemble(pContext, outMsg);
					var message = xmlDasm.GetNext(pContext);
					var messageXmlDasm = new XmlDasmComp();
					messageXmlDasm.AllowUnrecognizedMessage = true;
					messageXmlDasm.Disassemble(pContext, message);
					message = messageXmlDasm.GetNext(pContext);

					MessageQueue.Enqueue(message);
				}
				else if (ediDasm.Probe(pContext, outMsg))
				{
					logger.InfoFormat("Found EDI message");

					ediDasm.EdiDataValidation = false;
					ediDasm.EfactDelimiters = EdifactDelimiters;
					ediDasm.Disassemble(pContext, outMsg);
					IBaseMessage message;
					while ((message = ediDasm.GetNext(pContext)) != null)
					{
						MessageQueue.Enqueue(message);
					}
				}
				else if (MessageSchemaIsSpecified && ffDasm.Probe(pContext, outMsg))
				{
					logger.InfoFormat("Found FlatFile message");

					ffDasm.Disassemble(pContext, outMsg);
					var message = ffDasm.GetNext(pContext);
					if (message != null)
					{
						MessageQueue.Enqueue(message);
					}
				}
				else
				{
					logger.InfoFormat("No disassembler found");
					throw new ApplicationException("Received message could not be disassembled as no disassembler was found.");
				}
			}
			catch(Exception ex)
			{
				outMsg.Context.PromoteProperty<ErrorReport.ErrorType>("FailedMessage");
				outMsg.Context.WriteProperty<ErrorReport.Description>(ex.Message);
				outMsg.Context.PromoteProperty<ErrorReport.ReceivePortName>(pInMsg.Context.ReadPropertyString<BTS.ReceivePortName>());
				outMsg.Context.WriteProperty<BTS.MessageType>("AlertMessage");
				MessageQueue.Enqueue(outMsg);
			}

			logger.InfoFormat("Enqueued");
		}

		IEnumerable<byte> ReplaceBytes(byte[] bytes, string hexString)
		{
			var hexPairs = new List<Tuple<byte[], byte[]>>();

			foreach (var hex in hexString.Replace("0x", "").Split(','))
			{
				var pair = hex.Split('-');
				hexPairs.Add(Tuple.Create(HexToBytes(pair[0]), HexToBytes(pair[1])));
			}

			int skip = 0;
			foreach (var currentByte in bytes.Select((value, index) => new { index, value }))
			{
				if (skip > 0)
				{
					skip--;
					continue;
				}

				bool hasMatch = false;
				foreach (var pair in hexPairs)
				{
					if (bytes.Skip(currentByte.index).Take(pair.Item1.Length).SequenceEqual(pair.Item1))
					{
						foreach (var newBytes in pair.Item2)
						{
							yield return newBytes;
						}

						skip = pair.Item1.Length - 1;

						hasMatch = true;
						break;
					}
				}

				if (!hasMatch)
				{
					yield return currentByte.value;
				}
			}
		}

		byte[] HexToBytes(string hexString)
		{
			byte[] ret = new byte[hexString.Length / 2];
			for (int i = 0; i < ret.Length; i++)
			{
				int high = hexString[i * 2];
				int low = hexString[i * 2 + 1];
				high = (high & 0xf) + ((high & 0x40) >> 6) * 9;
				low = (low & 0xf) + ((low & 0x40) >> 6) * 9;

				ret[i] = (byte)((high << 4) | low);
			}

			return ret;
		}

		class PgpContext : OpenPgpContext
		{
			public string PgpPassphrase { get; set; }

			public PgpContext(string pgpSecretKeyInBase64, string pgpPassphrase) : base()
			{
				SecretKeyRingBundle = new PgpSecretKeyRingBundle(Convert.FromBase64String(pgpSecretKeyInBase64));
				PgpPassphrase = pgpPassphrase;
			}

			protected override string GetPasswordForKey(PgpSecretKey key)
			{
				return PgpPassphrase;
			}
		}

		#endregion

		#region IPersistPropertyBag Members

		public bool Enable { get; set; }
		public bool BypassMimeDecoder { get; set; }
		public bool ByteDecoderEnabled { get; set; }
		public string ByteDecoderInHex { get; set; }
		public bool RegexEnabled { get; set; }
		public string RegexEncoding { get; set; }
		public string RegexString { get; set; }
		public string PgpSecretKeyInBase64 { get; set; }
		public string PgpPassphrase { get; set; }
		public string FileMask { get; set; }
		public bool FileMaskOnly { get; set; }
		public string ContentType { get; set; }
		public int Index { get; set; }
		public string EdifactDelimiters { get; set; }
		public Schema MessageSchema { get; set; }
		private bool MessageSchemaIsSpecified { get { return (MessageSchema != null && !String.IsNullOrEmpty(MessageSchema.AssemblyName)); } }

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("B7850691-079A-4348-94ED-32B38273C056");
		}

		public void InitNew() { }

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			var var = LoadProperty(propertyBag, "Enable", errorLog);
			if (var != null) Enable = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "BypassMimeDecoder", errorLog);
			if (var != null) BypassMimeDecoder = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "ByteDecoderEnabled", errorLog);
			if (var != null) ByteDecoderEnabled = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "ByteDecoderInHex", errorLog);
			if (var != null) ByteDecoderInHex = (string)var;

			var = LoadProperty(propertyBag, "RegexEnabled", errorLog);
			if (var != null) RegexEnabled = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "RegexEncoding", errorLog);
			if (var != null) RegexEncoding = (string)var;

			var = LoadProperty(propertyBag, "RegexString", errorLog);
			if (var != null) RegexString = (string)var;

			var = LoadProperty(propertyBag, "PgpSecretKeyInBase64", errorLog);
			if (var != null) PgpSecretKeyInBase64 = (string)var;

			var = LoadProperty(propertyBag, "PgpPassphrase", errorLog);
			if (var != null) PgpPassphrase = (string)var;

			var = LoadProperty(propertyBag, "FileMask", errorLog);
			if (var != null) FileMask = (string)var;

			var = LoadProperty(propertyBag, "FileMaskOnly", errorLog);
			if (var != null) FileMaskOnly = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "BodyPartContentType", errorLog);
			if (var != null) ContentType = (string)var;

			var = LoadProperty(propertyBag, "BodyPartIndex", errorLog);
			if (var != null) Index = (int)var;

			var = LoadProperty(propertyBag, "EdifactDelimiters", errorLog);
			if (var != null) EdifactDelimiters = (string)var;

			var = LoadProperty(propertyBag, "MessageSchema", errorLog);
			if (var != null) MessageSchema = new Schema((string)var);
		}

		object LoadProperty(IPropertyBag propertyBag, string propertyName, int errorLog)
		{
			object result = null;
			try
			{
				propertyBag.Read(propertyName, out result, errorLog);
			}
			catch { }
			return result;
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val = Enable;
			propertyBag.Write("Enable", ref val);

			val = BypassMimeDecoder;
			propertyBag.Write("BypassMimeDecoder", ref val);

			val = ByteDecoderEnabled;
			propertyBag.Write("ByteDecoderEnabled", ref val);

			val = ByteDecoderInHex;
			propertyBag.Write("ByteDecoderInHex", ref val);

			val = RegexEnabled;
			propertyBag.Write("RegexEnabled", ref val);

			val = RegexEncoding;
			propertyBag.Write("RegexEncoding", ref val);

			val = RegexString;
			propertyBag.Write("RegexString", ref val);

			val = PgpSecretKeyInBase64;
			propertyBag.Write("PgpSecretKeyInBase64", ref val);

			val = PgpPassphrase;
			propertyBag.Write("PgpPassphrase", ref val);

			val = FileMask;
			propertyBag.Write("FileMask", ref val);

			val = FileMaskOnly;
			propertyBag.Write("FileMaskOnly", ref val);

			val = ContentType;
			propertyBag.Write("BodyPartContentType", ref val);

			val = Index;
			propertyBag.Write("BodyPartIndex", ref val);

			val = EdifactDelimiters;
			propertyBag.Write("EdifactDelimiters", ref val);

			val = MessageSchemaIsSpecified ? MessageSchema.ToString() : string.Empty;
			propertyBag.Write("MessageSchema", ref val);
		}

		#endregion
	}
}

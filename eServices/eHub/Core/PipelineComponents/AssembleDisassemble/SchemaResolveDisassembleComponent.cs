using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Component.Utilities;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;
using Microsoft.BizTalk.XPath;
using Newtonsoft.Json;

namespace CargoWise.eHub.Core.PipelineComponents
{
	/// <summary>
	/// Debatches batched messages and resolves message types whether flat file or Xml. If AS2 processing is enabled then the
	/// AS2 disassembler is called and the MDN enqueued.
	/// </summary>
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_DisassemblingParser)]
	[Guid("9D7A0A1B-F30D-4AA3-975D-3D122C74B8CB")]
	public class SchemaResolveDisassembleComponent : IBaseComponent, IComponentUI, IDisassemblerComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return string.Empty; }
		}

		public string Name
		{
			get { return "Inbound Message Disassembler"; }
		}

		public string Version
		{
			get { return "1.0"; }
		}

		#endregion

		#region IDisassemblerComponent Members

		public void Disassemble(IPipelineContext pipelineContext, IBaseMessage message)
		{
			Tracer.TraceStart(pipelineContext, message);
			try
			{
				Tracer.TraceInfo("DisassembleType: {0}", this.DisassembleType);
				Tracer.TraceInfo("EnvelopeSchema: {0}", this.EnvelopeSchemaIsSpecified ? this.EnvelopeSchema.ToString() : String.Empty);
				Tracer.TraceInfo("MessageSchema: {0}", this.MessageSchemaIsSpecified ? this.MessageSchema.ToString() : String.Empty);
				Tracer.TraceInfo("FlatFileDataXPath: {0}", this.FlatFileDataXPath);
				Tracer.TraceInfo("DiscardUnsubscribedMessages: {0}", this.DiscardUnsubscribedMessages);

				#region Validation

                if (this.DisassembleType.Equals(DasmType.XML) || this.DisassembleType.Equals(DasmType.JSON))
				{
					if (!EnvelopeSchemaIsSpecified && MessageSchemaIsSpecified)
						throw new ApplicationException("Envelope Schema must be specified");
					if (EnvelopeSchemaIsSpecified && !MessageSchemaIsSpecified && String.IsNullOrEmpty(this.FlatFileDataXPath))
						throw new ApplicationException("Envelope Schema specified without MessageSchema or FlatFile Data XPath");
				}
				else if (this.DisassembleType.Equals(DasmType.FlatFile))
				{
					if (EnvelopeSchemaIsSpecified)
						throw new ApplicationException("Envelope Schema not required for FlatFile disassemble");
					if (!MessageSchemaIsSpecified)
						throw new ApplicationException("Message Schema must be specified for FlatFile disassemble");
					if (FlatFileLineSplitEnabled)
					{
						if (FlatFileLineSplitNumber > 999 || FlatFileLineSplitNumber < 1)
							throw new ApplicationException("Invalid value for FlatFileLineSplitNumber");
						if (FlatFileLineSplitPreserveHeadlineEnabled && FlatFileLineSplitNumber == 1)
							throw new ApplicationException("Invalid value for FlatFileLineSplitNumber");
					}
                }
				else if (this.DisassembleType.Equals(DasmType.None))
				{
					if (ProcessSubscriptions)
						throw new ApplicationException("Cannot process subscriptions on non-disassembled file.");
				}
				else
				{
					throw new ApplicationException("DisassembleType not recognised.");
				}

				#endregion

				if (this.AS2Enabled)
				{
					IBaseMessage mdn = GetAS2DasmHelper().Disassemble(pipelineContext, message);
					if (mdn != null)
					{
						MessageQueue.Enqueue(mdn);
					}
				}

    			switch (this.DisassembleType)
				{
                    case DasmType.JSON: DeserializeFromJsonToXml(pipelineContext, message); break;
					case DasmType.XML: DisassembleXml(pipelineContext, message); break;
					case DasmType.FlatFile: DisassembleFlatFile(pipelineContext, message); break;
					case DasmType.None: GetMessageHelper().EnqueueMessage(pipelineContext, MessageQueue, message, message, ProcessSubscriptions, DiscardUnsubscribedMessages); break;
					default: throw new ApplicationException("DisassembleType not recognised.");
				}

				if (DiscardUnsubscribedMessages && MessageQueue.Count == 0)
				{
					string trackingID = message.Context.ReadPropertyString<MessageTrackingID>();
					if (!String.IsNullOrWhiteSpace(trackingID))
						GetOutboxAccessor().UpdateInboxMessageDistributionStatus(trackingID);
					Tracer.TraceInfo("Message discarded. MessageTrackingID = '{0}'", trackingID);
				}
				else
				{
					// Set the last interchange flags and sequence numbers
					int sequenceNumber = 1;
					foreach (object item in MessageQueue)
					{
						IBaseMessage queuedMessage = item as IBaseMessage;
						queuedMessage.Context.WriteProperty<BTS.InterchangeSequenceNumber>(sequenceNumber);
						bool lastMessage = (sequenceNumber == MessageQueue.Count);
						queuedMessage.Context.PromoteProperty<BTS.LastInterchangeMessage>(lastMessage);
						sequenceNumber++;
					}
				}

				Tracer.TraceEnd();
			}
			catch (Exception ex)
			{
				Tracer.TraceError(ex);
				throw;
			}
		}

		#region XML Dasm

		void DisassembleXml(IPipelineContext pipelineContext, IBaseMessage xmlMessage)
		{
			var msgHelper = GetMessageHelper();
			var dasmHelper = GetXmlDisassembler();
			var message = dasmHelper.Disassemble(pipelineContext, xmlMessage, EnvelopeSchema, MessageSchema);

			while (message != null)
			{
				if (EnvelopeSchemaIsSpecified)
				{
					var internalMessage = GetXmlDisassembler().Disassemble(pipelineContext, message);

					var originalDataStream = internalMessage.BodyPart.GetOriginalDataStream();
					originalDataStream.SeekBegin();

					Stream persistentStream = new VirtualStream(VirtualStream.MemoryFlag.AutoOverFlowToDisk);

					// Input reader settings
					var inputSettings = new XmlReaderSettings();
					inputSettings.IgnoreWhitespace = true;
					inputSettings.IgnoreComments = true;
					inputSettings.CloseInput = false;

					int bufferSize = Convert.ToInt32(ConfigurationManager.AppSettings["StreamingBufferSize"]);
					if (bufferSize == 0) throw new ConfigurationErrorsException("StreamingBufferSize configuration value is zero.  Check the .config file has the appSetting key value pair.");

					// Will be populated if data is flat file
					string flatFileSchemaType = internalMessage.Context.ReadPropertyString<FFSchemaType>();
					if (string.IsNullOrEmpty(flatFileSchemaType))
					{
						#region XML Data
						using (var xmlReader = XmlReader.Create(originalDataStream, inputSettings))
						{
							xmlReader.MoveToContent();
							xmlReader.Read();
							//Extract from CDATA
							xmlReader.WriteToStream(persistentStream);
						}
						//Decode and Decompress
						persistentStream.SeekBegin();
						internalMessage.BodyPart.Data = new ReadOnlySeekableStream(persistentStream.DecodeAndDecompress());
						
						if (int.TryParse(ConfigurationManager.AppSettings["MessageSizeLimit"], out int messageSizeLimit) && messageSizeLimit > 0
							&& internalMessage.BodyPart.Data.Length > messageSizeLimit)
						{
							throw new ApplicationException("XML content size is greater than the size limit " + messageSizeLimit);
						}

						// Disassemble
						var messageDasm = GetXmlDisassembler();
						var debatchedMessage = messageDasm.Disassemble(pipelineContext, internalMessage);
						msgHelper.EnqueueMessage(pipelineContext, MessageQueue, debatchedMessage, internalMessage, ProcessSubscriptions, DiscardUnsubscribedMessages);
						TraceDisassembledMessage(debatchedMessage, DasmType.XML);
						#endregion
					}
					else
					{
						#region Flat Data
						// Get the data at the xpath using XmlTextReader to preserve CR characters
						var xpaths = new XPathCollection();
						xpaths.Add(this.FlatFileDataXPath);
						var xmlReader = new XmlTextReader(originalDataStream);
						var xpathReader = new XPathReader(xmlReader, xpaths);
						if (xpathReader.ReadUntilMatch())
						{
							using (var textStream = new MemoryStream(xmlReader.Encoding.GetBytes(xpathReader.ReadElementContentAsString())))
							{
								if (FlatFileDataBaseDecodeEnabled)
								{
									try
									{
										persistentStream = textStream.DecodeAndDecompress();
										if (int.TryParse(ConfigurationManager.AppSettings["MessageSizeLimit"], out int messageSizeLimit) && messageSizeLimit > 0
											&& persistentStream.Length > messageSizeLimit)
										{
											throw new ApplicationException("File content size is greater than the size limit " + messageSizeLimit);
										}
									}
									catch (FormatException ex)
									{
										throw new ApplicationException("Flat File content does not seem to be Base-64 encoded. Try set FlatFileDataBaseDecodeEnabled to false.", ex);
									}
								}
								else
								{
									textStream.WriteTo(persistentStream);
								}
								textStream.Flush();
							}

							persistentStream.SeekBegin();
							internalMessage.BodyPart.Data = new ReadOnlySeekableStream(persistentStream);
						}
						else
						{
							throw new ApplicationException("FlatFile data not found at expected XPath.  Please review the document structure and/or pipeline configuration.");
						}

						// Disassemble using the message type passed by the client
						var documentSpec = pipelineContext.GetDocumentSpecByType(flatFileSchemaType);
						internalMessage.BodyPart.ContentType = "text/plain";
						var flatFileDasmHelper = GetFlatFileDisassembler();
						var debatchedMessage = flatFileDasmHelper.Disassemble(pipelineContext, internalMessage, new SchemaWithNone(documentSpec.DocSpecStrongName));
						msgHelper.EnqueueMessage(pipelineContext, MessageQueue, debatchedMessage, internalMessage, ProcessSubscriptions, DiscardUnsubscribedMessages);
						TraceDisassembledMessage(debatchedMessage, DasmType.FlatFile);
						#endregion
					}
				}
				else
				{
					// Disassemble again to promote properties now we know the message type
					var messageDasmHelper = GetXmlDisassembler();
					var debatchedMessage = messageDasmHelper.Disassemble(pipelineContext, message);

					if (XPathDebatchEnabled && !string.IsNullOrEmpty(XPathToDebatch))
					{
						var smartDebatcher = new CloneDebatchDisassembleComponent();
						smartDebatcher.Enabled = true;
						smartDebatcher.XPathToDebatch = XPathToDebatch;
						smartDebatcher.Disassemble(pipelineContext, debatchedMessage);

						var msg = smartDebatcher.GetNext(pipelineContext);
						while (msg != null)
						{
							msgHelper.EnqueueMessage(pipelineContext, MessageQueue, msg, msg, ProcessSubscriptions, DiscardUnsubscribedMessages);
							msg = smartDebatcher.GetNext(pipelineContext);
						}
					}
					else
					{
						msgHelper.EnqueueMessage(pipelineContext, MessageQueue, debatchedMessage, xmlMessage, ProcessSubscriptions, DiscardUnsubscribedMessages);
					}

					TraceDisassembledMessage(debatchedMessage, DasmType.XML);
					break;
				}
				message = dasmHelper.GetNext(pipelineContext);
			}
		}

		#endregion

		#region FlatFile Dasm

		void DisassembleFlatFile(IPipelineContext pipelineContext, IBaseMessage flatFileMessage)
		{
			var flatFileDasmHelper = GetFlatFileDisassembler();
			IBaseMessage[] debatchedMessages = null;

			if (this.FlatFileLineSplitEnabled)
			{

				var FFLS = new FlatFileLineSplitter(FlatFileLineSplitNumber, FlatFileLineSplitPreserveHeadlineEnabled);
				debatchedMessages = FFLS.Split(flatFileMessage, pipelineContext);
			}
			else
			{
				debatchedMessages = new IBaseMessage[1];
				debatchedMessages[0] = flatFileMessage;
			}


			IBaseMessage outMsg = null;
			foreach (var debatchedMessage in debatchedMessages)
			{
				outMsg = flatFileDasmHelper.Disassemble(pipelineContext, debatchedMessage, new SchemaWithNone(MessageSchema.ToString()));
				if (outMsg != null)
				{
					GetMessageHelper().EnqueueMessage(pipelineContext, MessageQueue, outMsg, flatFileMessage, ProcessSubscriptions, DiscardUnsubscribedMessages);
					TraceDisassembledMessage(outMsg, DasmType.FlatFile);
				}
			}
		}

		#endregion

        #region JSON Dasm

        void DeserializeFromJsonToXml(IPipelineContext pipelineContext, IBaseMessage message)
		{
            try
            {
                var serializer = new JsonSerializer();
				var xmlDoc = JSONConvertHelper.JSON2XML(message.BodyPart.Data.ReadToEnd());
                Stream persistentStream = new VirtualStream();
                xmlDoc.Save(persistentStream);
                persistentStream.SeekBegin();
                message.BodyPart.Data = persistentStream;
                DisassembleXml(pipelineContext, message);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message == "This document already has a 'DocumentElement' node.")
                    ThrowMissingJSONWrapper(ex);
                else
                    throw;
            }
            catch (InvalidCastException ex)
            {
                if (ex.Message == "Unable to cast object of type 'Newtonsoft.Json.Converters.XmlDocumentWrapper' to type 'Newtonsoft.Json.Converters.IXmlElement'.")
                    ThrowMissingJSONWrapper(ex);
                else
                    throw;
            }
		}

        void ThrowMissingJSONWrapper(Exception ex)
        {
            throw new ApplicationException("The json has multiple objects in the top level of root, please wrap inside an object such as \"{json:\"+data+\"}\" or \"{json:{'@xmlns':'cargowise.com/json/1', body:\"+data+\"}}\". Addionally, you could use \"Biztalk Receive Pipeline's AppendData and PrependData of Stream Wrapper stage\" to wrap the json data without modify the incoming message.", ex);
        }

        #endregion

        internal virtual IAS2DisassembleHelper GetAS2DasmHelper()
		{
			return new AS2DisassembleHelper();
		}

		internal virtual IMessageHelper GetMessageHelper()
		{
			return new MessageHelper();
		}

		internal virtual IXmlDisassembleHelper GetXmlDisassembler()
		{
			return new XmlDisassembleHelper();
		}

		internal virtual IFFDisassembleHelper GetFlatFileDisassembler()
		{
			return new FFDisassembleHelper();
		}

		internal virtual IOutboxAccessor GetOutboxAccessor()
		{
			return new OutboxAccessor();
		}

		void TraceDisassembledMessage(IBaseMessage disassembledMessage, DasmType messageFormat)
		{
			Tracer.TraceInfo("Message {0} ({1}): {2} ({3})",
				MessageQueue.Count,
				messageFormat.ToString(),
				disassembledMessage.Context.ReadPropertyString<BTS.MessageType>(),
				disassembledMessage.Context.ReadPropertyString<BTS.SchemaStrongName>());
		}

		public IBaseMessage GetNext(IPipelineContext сontext)
		{
			if (MessageQueue.Count > 0)
				return MessageQueue.Dequeue() as IBaseMessage;
			else
				return null;
		}

		Queue MessageQueue
		{
			get { return messageQueue ?? (messageQueue = new Queue()); }
		}
		Queue messageQueue;

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

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("9D7A0A1B-F30D-4AA3-975D-3D122C74B8CB");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			var var = LoadProperty(propertyBag, "MessageSchema", errorLog);
			if (var != null) MessageSchema = new Schema((string)var);

			var = LoadProperty(propertyBag, "EnvelopeSchema", errorLog);
			if (var != null) EnvelopeSchema = new Schema((string)var);

			var = LoadProperty(propertyBag, "DisassembleType", errorLog);
			if (var != null) DisassembleType = (DasmType)Enum.Parse(typeof(DasmType), (string)var);

			var = LoadProperty(propertyBag, "FlatFileDataXPath", errorLog);
			if (var != null) FlatFileDataXPath = (string)var;

			var = LoadProperty(propertyBag, "AS2Enabled", errorLog);
			if (var != null) AS2Enabled = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "FlatFileDataBaseDecodeEnabled", errorLog);
			if (var != null) FlatFileDataBaseDecodeEnabled = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "XPathDebatchEnabled", errorLog);
			if (var != null) XPathDebatchEnabled = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "XPathToDebatch", errorLog);
			if (var != null) XPathToDebatch = Convert.ToString(var);

			var = LoadProperty(propertyBag, "ProcessSubscriptions", errorLog);
			if (var != null) ProcessSubscriptions = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "FlatFileLineSplitEnabled", errorLog);
			if (var != null) FlatFileLineSplitEnabled = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "FlatFileLineSplitNumber", errorLog);
			if (var != null) FlatFileLineSplitNumber = Convert.ToInt16(var);

			var = LoadProperty(propertyBag, "FlatFileLineSplitPreserveHeadlineEnabled", errorLog);
			if (var != null) FlatFileLineSplitPreserveHeadlineEnabled = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "DiscardUnsubscribedMessages", errorLog);
			if (var != null) DiscardUnsubscribedMessages = Convert.ToBoolean(var);
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
			object val = MessageSchemaIsSpecified ? MessageSchema.ToString() : string.Empty;
			propertyBag.Write("MessageSchema", ref val);

			val = EnvelopeSchemaIsSpecified ? EnvelopeSchema.ToString() : string.Empty;
			propertyBag.Write("EnvelopeSchema", ref val);

			val = DisassembleType.ToString();
			propertyBag.Write("DisassembleType", ref val);

			val = FlatFileDataXPath;
			propertyBag.Write("FlatFileDataXPath", ref val);

			val = AS2Enabled;
			propertyBag.Write("AS2Enabled", ref val);

			val = FlatFileDataBaseDecodeEnabled;
			propertyBag.Write("FlatFileDataBaseDecodeEnabled", ref val);

			val = XPathDebatchEnabled;
			propertyBag.Write("XPathDebatchEnabled", ref val);

			val = XPathToDebatch;
			propertyBag.Write("XPathToDebatch", ref val);

			val = ProcessSubscriptions;
			propertyBag.Write("ProcessSubscriptions", ref val);

			val = FlatFileLineSplitEnabled;
			propertyBag.Write("FlatFileLineSplitEnabled", ref val);

			val = FlatFileLineSplitNumber;
			propertyBag.Write("FlatFileLineSplitNumber", ref val);

			val = FlatFileLineSplitPreserveHeadlineEnabled;
			propertyBag.Write("FlatFileLineSplitPreserveHeadlineEnabled", ref val);

			val = DiscardUnsubscribedMessages;
			propertyBag.Write("DiscardUnsubscribedMessages", ref val);

		}

		#endregion

		#region Properties

		/// <summary>
		/// Mandatory for the FlatFile and JSON <see cref="DasmType"/>.  Optional for XML and not required for EDI <see cref="DasmType"/>.
		/// </summary>
		public Schema MessageSchema { get; set; }

		/// <summary>
		/// Optional unless a debatching is required.
		/// </summary>
		public Schema EnvelopeSchema { get; set; }

		/// <summary>
		/// Disassemble mode.  FlatFile or XML.
		/// </summary>
		public DasmType DisassembleType { get; set; }

		/// <summary>
		/// See <see cref="DisassembleType"/>
		/// </summary>
		public enum DasmType
		{
			XML = 0,
			FlatFile = 1,
			JSON = 2,
			None = 3
		}

		/// <summary>
		/// The xpath to locate repeating FlatFile data elements within the incoming message.  This is the xpath within a debatched message
		/// not from the envelope root.
		/// </summary>
		public string FlatFileDataXPath { get; set; }

		/// <summary>
		/// Is the <see cref="EnvelopeSchema"/> property set
		/// </summary>
		private bool EnvelopeSchemaIsSpecified
		{
			get { return (EnvelopeSchema != null && !String.IsNullOrEmpty(EnvelopeSchema.AssemblyName)); }
		}

		/// <summary>
		/// Is the <see cref="MessageSchema"/> property set
		/// </summary>
		private bool MessageSchemaIsSpecified
		{
			get { return (MessageSchema != null && !String.IsNullOrEmpty(MessageSchema.AssemblyName)); }
		}

		/// <summary>
		/// Execute the AS2 disassembler to generate an MDN too?
		/// </summary> 
		public bool AS2Enabled { get; set; }

		public bool FlatFileDataBaseDecodeEnabled { get; set; }

		public bool XPathDebatchEnabled { get; set; }

		public string XPathToDebatch { get; set; }

		public bool ProcessSubscriptions { get; set; }

		public bool FlatFileLineSplitEnabled { get; set; }

		public int FlatFileLineSplitNumber { get; set; }

		public bool FlatFileLineSplitPreserveHeadlineEnabled { get; set; }

		public bool DiscardUnsubscribedMessages { get; set; }

		#endregion
	}
}

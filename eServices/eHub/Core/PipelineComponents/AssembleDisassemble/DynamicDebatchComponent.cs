using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.XPath;

namespace CargoWise.eHub.Core.PipelineComponents
{
    /// <summary>
    /// Uses an xpath expression to locate the MessageType of the enveloped message and debatch it (only one not multiple)
    /// </summary>
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_DisassemblingParser)]
    [Guid("A5C0B158-C89F-4A63-9665-2A0D68793038")]
	public class DynamicDebatchComponent : IBaseComponent, IComponentUI, IDisassemblerComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return string.Empty; }
		}

		public string Name
		{
			get { return "Dynamic Debatch Component"; }
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
                if (string.IsNullOrEmpty(this.MessageTypeXPath))
                    throw new ApplicationException("MessageTypeXpath must be specified.");
                Tracer.TraceInfo("MessageTypeXPath: {0}", this.MessageTypeXPath);

				if (string.IsNullOrEmpty(this.IsFlatFileXPath))
					throw new ApplicationException("IsFlatFileXPath must be specified.");
				Tracer.TraceInfo("IsFlatFileXPath: {0}", this.IsFlatFileXPath);

				if (string.IsNullOrEmpty(this.ContentXPath))
					throw new ApplicationException("ContentXPath must be specified.");
				Tracer.TraceInfo("ContentXPath: {0}", this.ContentXPath);

				var messageStream = message.BodyPart.GetOriginalDataStream();

				string messageType = string.Empty;
				GetValueFromStream(messageStream, MessageTypeXPath, ref messageType);
				messageStream.SeekBegin();

				bool isFlatFile = false;
				GetValueFromStream(messageStream, IsFlatFileXPath, ref isFlatFile);
				messageStream.SeekBegin();

                Tracer.TraceInfo("MessageType: {0}", messageType);

				message = GetXmlDisassembler().Disassemble(pipelineContext, message);
				messageStream.SeekBegin();

				var contentStream = GetValueFromStream(messageStream, ContentXPath);
				contentStream.SeekBegin();

				var contentMessage = pipelineContext.GetMessageFactory().CreateMessage();
				contentMessage.Context = PipelineUtil.CloneMessageContext(message.Context);
				var contentMessagePart = pipelineContext.GetMessageFactory().CreateMessagePart();
				string partName = string.Empty;
				message.GetPartByIndex(0, out partName);
				contentMessage.AddPart(partName, contentMessagePart, true);
				contentMessage.BodyPart.Charset = message.BodyPart.Charset;
				contentMessage.BodyPart.ContentType = message.BodyPart.ContentType;
				contentMessage.BodyPart.Data = contentStream.DecodeAndDecompress();

				MessageQueue.Enqueue(contentMessage);

				Tracer.TraceInfo("Message debatched");
                Tracer.TraceEnd();
            }
            catch (Exception ex)
            {
                Tracer.TraceError(ex);
                throw;
            }
		}

		public IBaseMessage GetNext(IPipelineContext сontext)
		{
			if (MessageQueue.Count > 0) return MessageQueue.Dequeue() as IBaseMessage;
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
            classID = new Guid("A5C0B158-C89F-4A63-9665-2A0D68793038");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			var var = LoadProperty(propertyBag, "MessageTypeXPath", errorLog);
			if (var != null) MessageTypeXPath = (string)var;

			var = LoadProperty(propertyBag, "IsFlatFileXPath", errorLog);
			if (var != null) IsFlatFileXPath = (string)var;

			var = LoadProperty(propertyBag, "ContentXPath", errorLog);
			if (var != null) ContentXPath = (string)var;
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
			object val = MessageTypeXPath;
			propertyBag.Write("MessageTypeXPath", ref val);

			val = IsFlatFileXPath;
			propertyBag.Write("IsFlatFileXPath", ref val);

			val = ContentXPath;
			propertyBag.Write("ContentXPath", ref val);
		}

		#endregion

		#region Properties

		public string MessageTypeXPath { get; set; }
		public string IsFlatFileXPath { get; set; }
		public string ContentXPath { get; set; }

		#endregion

		#region Implementation

		internal virtual IXmlDisassembleHelper GetXmlDisassembler()
		{ 
			return new XmlDisassembleHelper();
		}

		Stream GetValueFromStream(Stream stream, string xPath)
		{
			var result = new Microsoft.BizTalk.Streaming.VirtualStream();

			GetValueFromStream(stream, xPath, (XmlReader reader) =>
			{
				reader.Read();
				reader.WriteToStream(result);
			});

			return result;
		}

		void GetValueFromStream(Stream stream, string xPath, ref string val)
		{
			string result = string.Empty;
			GetValueFromStream(stream, xPath, (XmlReader reader) =>
			{
				result = reader.Value;
			});

			val = result;
		}

		void GetValueFromStream(Stream stream, string xPath, ref bool val)
		{
			object result = null;
			GetValueFromStream(stream, xPath, (XmlReader reader) =>
			{
				result = reader.Value;
			});

			if (result != null) val = Convert.ToBoolean(result);
		}

		void GetValueFromStream(Stream stream, string xPath, GetValueFromStreamDelegate action)
		{
			var settings = new XmlReaderSettings();
			settings.CloseInput = false;
			settings.IgnoreWhitespace = true;
			using (var xmlReader = XmlReader.Create(stream, settings))
			{
				var xpaths = new XPathCollection();
				xpaths.Add(xPath);
				using (var xpathReader = new XPathReader(xmlReader, xpaths))
				{
					while (xpathReader.ReadUntilMatch())
					{
						action(xmlReader);
						break;
					}
				}
			}
		}

		delegate void GetValueFromStreamDelegate(XmlReader reader);

		#endregion
	}
}

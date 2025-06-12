using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;
using Newtonsoft.Json;

namespace CargoWise.eHub.Core.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[Guid("5D365F8C-36B0-4198-8A55-131609D6B943")]
	public class MessageAssembleComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return "Assemble message in xml or native format depending on message type."; }
		}

		public string Name
		{
			get { return "Message Assembler"; }
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

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("5D365F8C-36B0-4198-8A55-131609D6B943");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object val = null;
			try
			{
				propertyBag.Read("Enabled", out val, errorLog);
			}
			catch { }
			if (val != null) Enabled = Convert.ToBoolean(val);
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val = Enabled;
			propertyBag.Write("Enabled", ref val);
		}

		#endregion

		#region IComponent Members

		public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage message)
		{
			Tracer.TraceStart(pipelineContext, message);
			Tracer.TraceInfo("Enabled: {0}", this.Enabled);

			try
			{
				if (!this.Enabled) return message;

				var originalStream = message.BodyPart.GetOriginalDataStream();
				var result = message;

				IMessageHelper messageHelper = GetMessageHelper();
				bool autosubscribing = messageHelper.IsAutoSubscriptionRequired(message);
				IBaseMessage subscribeMsg = null;
				if (autosubscribing)
				{
					subscribeMsg = pipelineContext.GetMessageFactory().CreateMessage();
					subscribeMsg.AddPart("Body", pipelineContext.GetMessageFactory().CreateMessagePart(), true);
					subscribeMsg.BodyPart.Data = new VirtualStream(VirtualStream.MemoryFlag.AutoOverFlowToDisk);
					message.BodyPart.Data.WriteTo(subscribeMsg.BodyPart.Data);
					message.BodyPart.Data.SeekBegin();
					subscribeMsg.BodyPart.Data.SeekBegin();
				}

				string messageType = message.Context.ReadPropertyString<BTS.MessageType>();
				Tracer.TraceInfo("MessageType: {0}", messageType);
				if (!string.IsNullOrEmpty(messageType))
				{
					var accessor = GetTransformationAccessor();
					result = AssembleMessage(pipelineContext, message, messageType, accessor);

					string postAssembleWrapper;
					if (accessor.IsPostAssembleMapping(messageType, out postAssembleWrapper))
					{
						Tracer.TraceInfo("PostAssembleAssemble");
						if (postAssembleWrapper != null)
						{
							Tracer.TraceInfo("WrapperMessageType: {0}", postAssembleWrapper);
							message.Context.PromoteProperty<BTS.MessageType>(postAssembleWrapper);
							messageType = postAssembleWrapper;
						}
						var msgNameParts = messageType.Split('#');
						if (msgNameParts.Length != 2)
							throw new ApplicationException(String.Format("Post Assemble wrapper message type is invalid. Type = '{0}'", messageType));
						var header = Encoding.UTF8.GetBytes(String.Format("<ns0:{0} xmlns:ns0=\"{1}\"><![CDATA[", msgNameParts[1], msgNameParts[0]));
						var footer = Encoding.UTF8.GetBytes(String.Format("]]></ns0:{0}>", msgNameParts[1]));
						var wrappedMsg = new VirtualStream();
						wrappedMsg.Write(header, 0, header.Length);
						result.BodyPart.Data.CopyTo(wrappedMsg);
						wrappedMsg.Write(footer, 0, footer.Length);
						wrappedMsg.Position = 0;
						result.BodyPart.Data = wrappedMsg;
						var transform = GetMultipleTransformationComponent();
						var newMsg = transform.Execute(pipelineContext, result);
						originalStream = newMsg.BodyPart.GetOriginalDataStream();
						messageType = newMsg.Context.ReadPropertyString<BTS.MessageType>();
						result = AssembleMessage(pipelineContext, newMsg, messageType, accessor);
					}
				}

				if (autosubscribing)
				{
					subscribeMsg.Context = PipelineUtil.CloneMessageContext(result.Context);
					messageHelper.InsertAutoSubscriptionsForSender(subscribeMsg);
				}

				var resultStream = new VirtualStream(VirtualStream.MemoryFlag.AutoOverFlowToDisk);
				var writer = XmlTextWriter.Create(resultStream);
				writer.WriteStartElement("ns0", "OutboxInsert", "http://cargowise.com/ehub/core/2010/06");

				WriteElement(writer, "PK", GetNewGuid());
				WriteElement(writer, "SenderID", message.Context.ReadPropertyString<BTS.SourceParty>());
				WriteElement(writer, "RecipientID", result.Context.ReadPropertyString<BTS.DestinationParty>());
				WriteElement(writer, "EnvelopeTrackingID", result.Context.ReadPropertyString<EnvelopeTrackingID>());
				WriteElement(writer, "MessageTrackingID", result.Context.ReadPropertyString<MessageTrackingID>());
				WriteElement(writer, "InternalTrackingID", result.Context.ReadPropertyString<InternalTrackingID>());
				WriteElement(writer, "TargetMessageType", messageType);
				WriteElement(writer, "OverrideEmailSubject", result.Context.ReadPropertyString<OverrideEmailSubject>());
				WriteElement(writer, "OverrideFilename", result.Context.ReadPropertyString<OverrideFilename>());
				WriteElement(writer, "TransformSetID", result.Context.ReadPropertyString<TransformSetID>());
				WriteElement(writer, "InsertUTC", GetCurrentUTCString());
				WriteElementStream(writer, "Content", result.BodyPart.GetOriginalDataStream().CompressAndEncode());
				WriteXmlElementStream(writer, "XmlContent", originalStream);
				WriteElement(writer, "UncompressedLength", result.BodyPart.GetOriginalDataStream().Position);

				writer.WriteEndElement();
				writer.Flush();

				resultStream.SeekBegin();
				result.BodyPart.Data = resultStream;
				return result;
			}
			catch (Exception ex)
			{
				Tracer.TraceError(ex);
				throw;
			}
			finally
			{
				Tracer.TraceEnd();
			}
		}

		IBaseMessage AssembleMessage(IPipelineContext pipelineContext, IBaseMessage message, string messageType, ITransformAccessor accessor)
		{
			var result = message;
			string toCharset;
			if (accessor.IsFlatFile(messageType, out toCharset))
			{
				Tracer.TraceInfo("IsFlatFile");
				result = GetFFAssembleHelper().Assemble(pipelineContext, message);
				if (!string.IsNullOrWhiteSpace(toCharset))
				{
					Tracer.TraceInfo("Converting flat file character set from '{0}' to '{1}'.", result.BodyPart.Charset, toCharset);
					var toStrm = new VirtualStream();
					var fromEnc = Encoding.GetEncoding(result.BodyPart.Charset);
					var toEnc = Encoding.GetEncoding(toCharset);
					var fromRdr = new StreamReader(result.BodyPart.GetOriginalDataStream(), fromEnc);
					var toWtr = new StreamWriter(toStrm, toEnc);
					int count;
					char[] buffer = new char[StreamExtensions.BufferSize];
					while ((count = fromRdr.Read(buffer, 0, buffer.Length)) > 0)
					{
						toWtr.Write(buffer, 0, count);
					}
					toWtr.Flush();
					toStrm.SeekBegin();
					result.BodyPart.Data = toStrm;
					result.BodyPart.Charset = toCharset;
				}
			}
			else if (accessor.IsEDI(messageType))
			{
				Tracer.TraceInfo("IsEDI");
				if (message.Context.ReadPropertyString<EdiOverride.OverrideEDIHeader>() == "true"
					&& message.Context.ReadPropertyString<EDI.DestinationPartyName>() == "Override")
					message.Context.WriteProperty<EDI.DestinationPartyName>(null);
				else
					message.Context.PromoteProperty<EDI.DestinationPartyName>(message.Context.ReadPropertyString<BTS.DestinationParty>());

				var ediAssembler = GetEDIAssembler();
				result = ediAssembler.Assemble(pipelineContext, message);

				var earlyTerminateEdifactUnb = message.Context.ReadPropertyString<EarlyTerminateEdifactUnb>() == "true";
				var overrideEDIHeader = message.Context.ReadPropertyString<EdiOverride.OverrideEDIHeader>() == "true";
				var valueUNB9 = message.Context.ReadPropertyString<EdiOverride.UNB9>();
				var valueUNB11 = message.Context.ReadPropertyString<EdiOverride.UNB11>();
				if (earlyTerminateEdifactUnb || (overrideEDIHeader && (valueUNB11 != null || valueUNB9 != null)))
				{
					string messageString = new StreamReader(result.BodyPart.GetOriginalDataStream(), Encoding.Default).ReadToEnd();
					var terminator = "'";
					if (overrideEDIHeader)
					{
						var replacement9 = $"UNB+$1+$2+$3+$4+$5+$6+$7+$8+{valueUNB9}$10$11'";
						var replacement11 = $"UNB+$1+$2+$3+$4+$5+$6+$7+$8+$9+$10+{valueUNB11}'";
						if (valueUNB9 != null) messageString = Regex.Replace(messageString, pattern9, replacement9);
						if (valueUNB11 != null) messageString = Regex.Replace(messageString, pattern11, replacement11);

						var overrideUNA6 = message.Context.ReadPropertyString<EdiOverride.UNA6>();
						var overrideUNA6suffix = message.Context.ReadPropertyString<EdiOverride.UNA6Suffix>();
						if (overrideUNA6 != null && overrideUNA6suffix != null)
						{
							terminator = overrideUNA6 + overrideUNA6suffix;
							messageString = messageString.Replace("'", terminator);
						}
					}
					if (earlyTerminateEdifactUnb)
					{
						Tracer.TraceInfo("EarlyTerminateEdifactUnb");
						var removeableUnbFields = Regex.Match(messageString, @"^UNB[^']*?(\+*)'", RegexOptions.Compiled).Groups[1];
						if (removeableUnbFields.Success)
							messageString = messageString.Remove(removeableUnbFields.Index, removeableUnbFields.Length);
					}
					result.BodyPart.Data = new MemoryStream(Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("ISO-8859-1"), Encoding.UTF8.GetBytes(messageString)));
				}
			}
			else if (accessor.IsJson(messageType))
			{
				Tracer.TraceInfo("IsJson");
				var jsonText = JSONConvertHelper.XML2JSON(result.BodyPart.GetOriginalDataStream());
				result.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes(jsonText));
			}
			return result;
		}

		internal virtual XmlEdiAssembleHelper GetEDIAssembler()
		{
			return new XmlEdiAssembleHelper();
		}

		internal virtual IComponent GetMultipleTransformationComponent()
		{
			return new MultipleTransformationComponent { Enabled = true };
		}

		#region Implementation

		void WriteElement(XmlWriter writer, string elementName, object value)
		{
			writer.WriteStartElement(elementName);
			writer.WriteString(value != null ? value.ToString() : string.Empty);
			writer.WriteEndElement();
		}

		void WriteXmlElementStream(XmlWriter writer, string elementName, Stream stream)
		{
			stream.SeekBegin();

			writer.WriteStartElement(elementName);
			writer.WriteRaw("<![CDATA[");
			var resultStream = stream.ReplaceTextBlock("<![CDATA[", "]]>", "CDATA section was removed from Xml. See Content field for full message.");
			new StreamReader(resultStream).WriteToXmlWriter(writer);
			writer.WriteRaw("]]>");
			writer.WriteEndElement();
		}

		void WriteElementStream(XmlWriter writer, string elementName, Stream stream)
		{
			stream.SeekBegin();

			writer.WriteStartElement(elementName);
			writer.WriteRaw("<![CDATA[");
			new StreamReader(stream).WriteToXmlWriter(writer);
			writer.WriteRaw("]]>");
			writer.WriteEndElement();
		}

		internal virtual ITransformAccessor GetTransformationAccessor()
		{
			return DataAccessFactories.NewTransformAccessorInstance();
		}

		internal virtual IMessageHelper GetMessageHelper()
		{
			return new MessageHelper();
		}

		internal virtual IFFAssembleHelper GetFFAssembleHelper()
		{
			return new FFAssembleHelper();
		}

		internal virtual Guid GetNewGuid()
		{
			return Guid.NewGuid();
		}

		internal virtual string GetCurrentUTCString()
		{
			return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
		}

		#endregion

		#endregion

		#region Properties

		public bool Enabled { get; set; }

		#endregion
		const string pattern9 = @"^UNB\+([^+']*)\+([^+']*)\+([^+']*)\+([^+']*)\+([^+']*)\+([^+']*)\+([^+']*)\+([^+']*)\+([^+']*)(\+[^+']*)?(\+[^+']*)?'";
		const string pattern11 = @"^UNB\+([^+']*)\+([^+']*)\+([^+']*)\+([^+']*)\+([^+']*)\+([^+']*)\+([^+']*)\+([^+']*)\+([^+']*)\+([^+']*)\+([^+']*)'";
	}
}

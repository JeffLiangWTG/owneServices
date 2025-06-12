using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using VirtualStream = Microsoft.BizTalk.Streaming.VirtualStream;

namespace CargoWise.eHub.Products.Core.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[Guid("35C2A01D-6CEF-4D60-87C5-BDCFDBD7DA3A")]
	public class OutboxAssembleProductComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		const string prefix = "ns0";
		const string ns = "http://schemas.microsoft.com/Sql/2008/05/TypedProcedures/dbo";

		#region IBaseComponent Members

		public string Description
		{
			get { return "Product: Outbox message assemble component."; }
		}

		public string Name
		{
			get { return "Product: Outbox Message Assembler"; }
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
			classID = new Guid("35C2A01D-6CEF-4D60-87C5-BDCFDBD7DA3A");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object val = null;
			Func<string, bool> getVal = p => { try { propertyBag.Read(p, out val, errorLog); } catch { } return val != null; };

			if (getVal("Enabled")) Enabled = Convert.ToBoolean(val);
			if (getVal("UseMessageTrackingIDAsInboxPK")) UseMessageTrackingIDAsInboxPK = Convert.ToBoolean(val);
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val;
			val = Enabled; propertyBag.Write("Enabled", ref val);
			val = UseMessageTrackingIDAsInboxPK; propertyBag.Write("UseMessageTrackingIDAsInboxPK", ref val);
		}

		#endregion

		#region IComponent Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage message)
		{
			if (Enabled)
			{
				if (pipelineContext == null)
				{
					throw new ArgumentNullException("PipelineContext");
				}

				var messageTrackingID = MessageHelper.GetMessageTrackingID(message);
				var originalStream = message.BodyPart.GetOriginalDataStream();
				var resultStream = new VirtualStream();

				var writer = XmlTextWriter.Create(resultStream);
				writer.WriteStartElement(prefix, "InsertMessageToOutbox", ns);

				string inboxPK;
				if (UseMessageTrackingIDAsInboxPK)
				{
					// inbox pk is configured to be the MessageTrackingID
					inboxPK = messageTrackingID.ToString();
				}
				else
				{
					// inbox pk is stored in the InternalTrackingID
					inboxPK = message.Context.ReadPropertyString<InternalTrackingID>();
				}

				WriteElement(writer, "PK", GetNewGuid());
				WriteElement(writer, "SenderID", message.Context.ReadPropertyString<BTS.SourceParty>());
				WriteElement(writer, "RecipientID", message.Context.ReadPropertyString<BTS.DestinationParty>());
				WriteElement(writer, "EnvelopeTrackingID", Guid.Empty.ToString());
				WriteElement(writer, "MessageTrackingID", messageTrackingID);
				WriteElement(writer, "InternalTrackingID", inboxPK);
				WriteElement(writer, "Status", 0);
				WriteElement(writer, "InsertUTC", GetCurrentUTCString());
				WriteElement(writer, "TargetMessageType", message.Context.ReadPropertyString<BTS.MessageType>());
				WriteElementStream(writer, "Content", message.BodyPart.GetOriginalDataStream().CompressAndEncode());
				WriteXmlElementStream(writer, "XMLContent", originalStream);
				WriteElement(writer, "UncompressedLength", message.BodyPart.GetOriginalDataStream().Position);
				writer.WriteEndElement();
				writer.Flush();

				resultStream.SeekBegin();
				message.BodyPart.Data = resultStream;
				pipelineContext.ResourceTracker.AddResource(resultStream);
			}

			return message;
		}

		#region Implementation

		void WriteElement(XmlWriter writer, string localName, object value)
		{
			writer.WriteStartElement(prefix, localName, ns);
			writer.WriteString(value != null ? value.ToString() : string.Empty);
			writer.WriteEndElement();
		}

		void WriteXmlElementStream(XmlWriter writer, string localName, Stream stream)
		{
			stream.SeekBegin();

			writer.WriteStartElement(prefix, localName, ns);
			writer.WriteRaw("<![CDATA[");
			var resultStream = stream.ReplaceTextBlock("<![CDATA[", "]]>", "CDATA section was removed from Xml. See Content field for full message.");
			new StreamReader(resultStream).WriteToXmlWriter(writer);
			writer.WriteRaw("]]>");
			writer.WriteEndElement();
		}

		void WriteElementStream(XmlWriter writer, string localName, Stream stream)
		{
			stream.SeekBegin();

			writer.WriteStartElement(prefix, localName, ns);
			writer.WriteRaw("<![CDATA[");
			new StreamReader(stream).WriteToXmlWriter(writer);
			writer.WriteRaw("]]>");
			writer.WriteEndElement();
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
		// UseMessageTrackingIDAsInboxPK = false is the new functionality, it is included as a property as some pipelines may need to return to the previous functionality if anything breaks
		public bool UseMessageTrackingIDAsInboxPK { get; set; }

		#endregion

	}
}

using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.PipelineComponents;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;

namespace CargoWise.eHub.Clients.TRX.PipelineComponents
{
	[Serializable]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("FA4CA8E8-6DA7-4AB4-8F26-16A3E684C374")]
	public class TRXWrapMessageComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return string.Empty; }
		}

		public string Name
		{
			get { return "TRXWrapMessageComponent"; }
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

		public IEnumerator Validate(object obj)
		{
			return null;
		}

		#endregion

		#region IComponent Members

		public IBaseMessage Execute(IPipelineContext context, IBaseMessage message)
		{
			if (!Enabled) return message;
			var originalStream = message.BodyPart.GetOriginalDataStream();

			var stream = new VirtualStream(VirtualStream.MemoryFlag.AutoOverFlowToDisk);
			var writer = XmlTextWriter.Create(stream, new XmlWriterSettings() { OmitXmlDeclaration = true });
			writer.WriteStartElement("Send", "http://cargowise.com/ehub/product/2013/04");
			writer.WriteElementString("senderId", message.Context.ReadPropertyString<BTS.SourceParty>());
			writer.WriteElementString("recepientId", message.Context.ReadPropertyString<BTS.DestinationParty>());
			writer.WriteStartElement("message");
			writer.WriteRaw("<![CDATA[");
			new StreamReader(message.BodyPart.GetOriginalDataStream()).WriteToXmlWriter(writer);
			writer.WriteRaw("]]>");
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.Flush();
			stream.Position = 0;

			context.ResourceTracker.AddResource(stream);

			message.BodyPart.Data = stream;
			return message;
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("FA4CA8E8-6DA7-4AB4-8F26-16A3E684C374");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object var = null;
			try { propertyBag.Read("Enabled", out var, errorLog); }
			catch { }
			if (var != null) Enabled = (bool)var;
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val = Enabled;
			propertyBag.Write("Enabled", ref val);
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }

		#endregion
	}
}

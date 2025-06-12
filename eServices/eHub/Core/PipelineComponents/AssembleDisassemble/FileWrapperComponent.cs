using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using CargoWise.eHub.Common.Extensions;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;

namespace CargoWise.eHub.Core.PipelineComponents
{
	[Serializable]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("6A7E15D9-32BF-4237-BC7B-80D7BA00B53A")]
	public class FileWrapperComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return "Map file stream to XML document."; }
		}

		public string Name
		{
			get { return "File Wrapper"; }
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

			var stream = new VirtualStream(VirtualStream.MemoryFlag.AutoOverFlowToDisk);
			var writer = XmlTextWriter.Create(stream, new XmlWriterSettings() { OmitXmlDeclaration = true });
			writer.WriteStartElement("FileWrapper", "http://cargowise.com/ehub/core/2013/08");
			writer.WriteElementString("FileName", System.IO.Path.GetFileName(message.Context.ReadPropertyString<FILE.ReceivedFileName>()));
			writer.WriteStartElement("FileStream");

			using (var newStream = message.BodyPart.GetOriginalDataStream().EncodeStream())
			{
				newStream.SeekBegin();
				new StreamReader(newStream).WriteToXmlWriter(writer);
			}

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
			classID = new Guid("6A7E15D9-32BF-4237-BC7B-80D7BA00B53A");
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

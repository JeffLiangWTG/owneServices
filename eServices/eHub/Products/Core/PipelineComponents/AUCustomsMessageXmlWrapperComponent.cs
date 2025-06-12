using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Products.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;

namespace CargoWise.eHub.Products.Core.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[Guid("73EB0F0F-370A-4367-94AC-42CAF49271AB")]
	public class AUCustomsMessageXmlWrapperComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Membersва

		public string Description
		{
			get { return "Product: Wrap AUCustoms reply message in xml envelop"; }
		}

		public string Name
		{
			get { return "Product: AUCustoms message xml wrapper component"; }
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

		#region IComponent Members

		public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage message)
		{
			if (Enabled)
			{
				if (pipelineContext == null)
				{
					throw new ArgumentNullException("PipelineContext");
				}

				MessageHelper.WrapMessageInReadOnlySeekableStream(pipelineContext, message);

				if (Encode) MessageHelper.EncodeStream(pipelineContext, message);

				string reference = message.Context.ReadPropertyString<Reference>();

				ReadOnlySeekableStream seekableStream = null;

				Stream messageStream = new VirtualStream();
				pipelineContext.ResourceTracker.AddResource(messageStream);
				var writer = XmlTextWriter.Create(messageStream, new XmlWriterSettings() {OmitXmlDeclaration = true});
				writer.WriteStartElement("AUCustomsReply", "http://cargowise.com/ehub/products/");
				writer.WriteElementString("Reference", reference);
				writer.WriteStartElement("Content");
				var reader = new StreamReader(message.BodyPart.Data);
				reader.WriteToXmlWriter(writer);
				writer.WriteEndElement();
				writer.WriteEndElement();
				writer.Flush();

				messageStream.Position = 0;
				seekableStream = new ReadOnlySeekableStream(messageStream);
				pipelineContext.ResourceTracker.AddResource(seekableStream);
				message.BodyPart.Data = seekableStream;

				message.BodyPart.Data.SeekBegin();
			}

			return message;
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("73EB0F0F-370A-4367-94AC-42CAF49271AB");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object val = null;
			Func<string, bool> getVal = p => { try { propertyBag.Read(p, out val, errorLog); } catch { } return val != null; };

			if (getVal("Enabled")) Enabled = Convert.ToBoolean(val);
			if (getVal("Encode")) Encode = Convert.ToBoolean(val);
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val;
			val = Enabled; propertyBag.Write("Enabled", ref val);
			val = Encode; propertyBag.Write("Encode", ref val);
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }
		public bool Encode { get; set; }

		#endregion
	}
}


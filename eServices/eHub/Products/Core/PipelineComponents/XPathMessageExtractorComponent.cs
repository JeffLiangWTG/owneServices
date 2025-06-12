using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Xml;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;
using Microsoft.BizTalk.XPath;

namespace CargoWise.eHub.Products.Core.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[Guid("F9D31E1B-973B-4231-9F96-4A7A6B0B5169")]
	public class XPathMessageExtractorComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Membersва

		public string Description
		{
			get { return "Product: Extract Message from Original message using XPath"; }
		}

		public string Name
		{
			get { return "Product: XPath message extractor component"; }
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
			if (!this.Enabled) return message;
			if (string.IsNullOrWhiteSpace(this.XPath)) return message;

			MessageHelper.WrapMessageInReadOnlySeekableStream(pipelineContext, message);
			if (SkipIfFlatFile)
			{
				bool isFlatFileSchema;
				bool.TryParse(message.Context.ReadPropertyString<IsFlatFileSchema>(), out isFlatFileSchema);
				if (isFlatFileSchema) return message;
			}

			var reader = XmlTextReader.Create(message.BodyPart.Data);
			var collection = new XPathCollection() { this.XPath }; ;
			var xPathReader = new XPathReader(reader, collection);

			if (xPathReader.ReadUntilMatch())
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					var outputStream = new VirtualStream();
					var writer = XmlWriter.Create(outputStream);
					var contentReader = xPathReader.ReadSubtree();
					writer.WriteNode(reader.ReadSubtree(), false);
					writer.Flush();
					outputStream.SeekBegin();
					var newStream = new ReadOnlySeekableStream(outputStream);
					if (pipelineContext != null) pipelineContext.ResourceTracker.AddResource(newStream);
					message.BodyPart.Data = newStream;
				}

				if (reader.NodeType == XmlNodeType.Text && reader.CanReadValueChunk)
				{
					var outputStream = new VirtualStream();

					reader.WriteToStream(outputStream);
					outputStream.SeekBegin();

					ReadOnlySeekableStream newStream;

					if (this.Decode)
					{
						var decodedStream = outputStream.DecodeStream();
						if (outputStream != null) outputStream.Close();
						decodedStream.SeekBegin();
						newStream = new ReadOnlySeekableStream(decodedStream);
					}
					else
					{
						newStream = new ReadOnlySeekableStream(outputStream);
					}

					if (pipelineContext != null) pipelineContext.ResourceTracker.AddResource(newStream);
					message.BodyPart.Data = newStream;
				}
			}

			message.BodyPart.Data.SeekBegin();
			return message;
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("F9D31E1B-973B-4231-9F96-4A7A6B0B5169");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object val = null;
			Func<string, bool> getVal = p => { try { propertyBag.Read(p, out val, errorLog); } catch { } return val != null; };

			if (getVal("Enabled")) Enabled = Convert.ToBoolean(val);
			if (getVal("XPath")) XPath = Convert.ToString(val);
			if (getVal("Decode")) Decode = Convert.ToBoolean(val);
			if (getVal("SkipIfFlatFile")) SkipIfFlatFile = Convert.ToBoolean(val);
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val;
			val = Enabled; propertyBag.Write("Enabled", ref val);
			val = XPath; propertyBag.Write("XPath", ref val);
			val = Decode; propertyBag.Write("Decode", ref val);
			val = SkipIfFlatFile; propertyBag.Write("SkipIfFlatFile", ref val);
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }
		public string XPath { get; set; }
		public bool Decode { get; set; }
		public bool SkipIfFlatFile { get; set; }

		#endregion
	}
}


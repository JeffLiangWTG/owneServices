using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;

namespace CargoWise.eHub.Products.JPCustoms.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[Guid("2CA18C9F-7FA0-42DF-B626-05E3C1890570")]
	public class JPCustomsGatewayMessageWrapperComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Membersва

		public string Description
		{
			get { return "Wrap JPCustoms message in JPCustomsGateway envelop"; }
		}

		public string Name
		{
			get { return "Wrap JPCustoms message in JPCustomsGateway envelop"; }
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
			message.Context.WriteProperty<BTS.AckRequired>(true);
			MessageHelper.WrapMessageInReadOnlySeekableStream(pipelineContext, message);
			string audit = message.Context.ReadPropertyString<OverrideFilename>();

			using (var editableStream = new VirtualStream())
			{
				message.BodyPart.Data.CopyTo(editableStream);
				ReplaceMessageLength(editableStream);
				editableStream.Position = 0;
				using (var contentStream = CompressAndEncode ? editableStream.CompressAndEncode() : editableStream)
				{
					var messageStream = WrapMessage(contentStream, audit);

					MessageHelper.AddStreamAsABody(pipelineContext, message, messageStream);
					return message;
				}
			}
		}

		public static Stream WrapMessage(Stream contentStream, string audit)
		{
			Stream messageStream = new VirtualStream();
			var writer = XmlWriter.Create(messageStream, new XmlWriterSettings() { OmitXmlDeclaration = true });
			writer.WriteStartElement("SendLodgement", "http://cargowise.com/ehub/products/jpcustoms");

			writer.WriteStartElement("message");
			var reader = new StreamReader(contentStream);
			reader.WriteToXmlWriter(writer);
			writer.WriteEndElement();

			writer.WriteElementString("audit", audit);
			writer.WriteEndElement();
			writer.Flush();

			messageStream.Position = 0;
			return messageStream;
		}

		public static void ReplaceMessageLength(Stream editableStream)
		{
			long messageLength = editableStream.Length;
			if (messageLength < 398) throw new ApplicationException("JP Customs message should be longer than 398");
			if (messageLength > 500000) throw new ApplicationException("JP Customs message can't be longer than 500.000");
			string messageLengthString = messageLength.ToString("000000");
			editableStream.Position = 392;
			editableStream.Write(Encoding.UTF8.GetBytes(messageLengthString), 0, 6);
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("2CA18C9F-7FA0-42DF-B626-05E3C1890570");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object val = null;
			Func<string, bool> getVal = p => { try { propertyBag.Read(p, out val, errorLog); } catch { } return val != null; };

			if (getVal("Enabled")) Enabled = Convert.ToBoolean(val);
			if (getVal("CompressAndEncode")) CompressAndEncode = Convert.ToBoolean(val);
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val;
			val = Enabled; propertyBag.Write("Enabled", ref val);
			val = CompressAndEncode; propertyBag.Write("CompressAndEncode", ref val);
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }
		public bool CompressAndEncode { get; set; }

		#endregion
	}
}


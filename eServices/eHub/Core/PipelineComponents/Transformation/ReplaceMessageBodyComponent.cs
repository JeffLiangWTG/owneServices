using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.XPath;

namespace CargoWise.eHub.Core.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("BA6E91C7-3AC8-4525-8B7D-776E8C026117")]
	public class ReplaceMessageBodyComponent : ComponentBase, IComponent
	{
		#region IBaseComponent Members

		protected override string DisplayName
		{
			get { return "Replace SelectOutboxMessageToDeliver result message Body"; }
		}

		protected override Guid ClassID
		{
			get { return new Guid("BA6E91C7-3AC8-4525-8B7D-776E8C026117"); }
		}

		#endregion

		#region IComponent Members

		public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage message)
		{
			if (!this.Enabled) return message;

			Tracer.TraceStart(pipelineContext, message);
			try
			{
				var msgStream = new VirtualStream();
				pipelineContext.ResourceTracker.AddResource(msgStream);

				using (var reader = XmlReader.Create(message.BodyPart.GetOriginalDataStream()))
				{
					reader.MoveToContent();
					reader.ReadToDescendant("Content", "http://schemas.microsoft.com/Sql/2008/05/TypedPolling/SelectOutboxMessageToDeliver");
					using (var decoder = new BufferReaderStream(reader.ReadElementContentAsBase64))
					using (var decompressor = new GZipStream(decoder, CompressionMode.Decompress))
						decompressor.CopyTo(msgStream);
				}

				msgStream.Position = 0;
				message.BodyPart.Data = msgStream;

				Tracer.TraceEnd();
				return message;
			}
			catch (Exception ex)
			{
				Tracer.TraceError(ex);
				throw;
			}
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }

		#endregion
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using CargoWise.eHub.Common.Extensions;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;
using Microsoft.BizTalk.XPath;

namespace CargoWise.eHub.Core.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_DisassemblingParser)]
	[Guid("9219D890-0C8B-4A9A-B31A-46160C582579")]
	class CloneDebatchDisassembleComponent : IBaseComponent, IComponentUI, IDisassemblerComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return string.Empty; }
		}

		public string Name
		{
			get { return "Clone Debatch Disassembler"; }
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

		#region IDisassemblerComponent Members

		Queue MessageQueue
		{
			get { return messageQueue ?? (messageQueue = new Queue()); }
		}
		Queue messageQueue;

		public void Disassemble(IPipelineContext pipelineContext, IBaseMessage message)
		{
			if (!Enabled)
			{
				MessageQueue.Enqueue(message);
				return;
			}

			if (string.IsNullOrEmpty(XPathToDebatch))
				throw new ApplicationException("XPathToDebatch must be specified");

			var originalDataStream = message.BodyPart.GetOriginalDataStream();
			var persistingStream = new VirtualStream(VirtualStream.MemoryFlag.AutoOverFlowToDisk);
			var seekablePersistentStream = new ReadOnlySeekableStream(originalDataStream, persistingStream);

			seekablePersistentStream.SeekBegin();
			foreach (var dataStream in GetDebatchedChiledStreams(XPathToDebatch, seekablePersistentStream))
			{
				var outMsg = pipelineContext.GetMessageFactory().CreateMessage();
				outMsg.AddPart("Body", pipelineContext.GetMessageFactory().CreateMessagePart(), true);
				outMsg.Context = PipelineUtil.CloneMessageContext(message.Context);

				outMsg.BodyPart.Data = dataStream;
				MessageQueue.Enqueue(outMsg);
			}
		}

		public IBaseMessage GetNext(IPipelineContext pipelineContext)
		{
			if (MessageQueue.Count > 0)
				return MessageQueue.Dequeue() as IBaseMessage;
			else
				return null;
		}

		Stream[] GetDebatchedChiledStreams(string xPath, Stream parentStream)
		{
			var result = new List<Stream>();

			parentStream.SeekBegin();
			var reader = XmlTextReader.Create(parentStream);

			var xPathSplit = new List<string>(xPath.Split(new[] { "/*" }, StringSplitOptions.RemoveEmptyEntries));
			string xPathToExecute = string.Empty;
			if (xPathSplit.Count > 1)
			{
				xPathToExecute = "/*" + xPathSplit[0] + "/*" + xPathSplit[1];
			}
			else xPathToExecute = "/*" + xPathSplit[0];

			var collection = new XPathCollection() { xPathToExecute };
			var xPathReader = new XPathReader(reader, collection);

			xPathSplit.Remove(xPathSplit[0]);
			var concatedXPAth = ConcatXPath(xPathSplit);

			while (xPathReader.ReadUntilMatch())
			{
				var childStream = new VirtualStream();
				var writer = XmlWriter.Create(childStream);
				var tempReader = reader.ReadSubtree();
				writer.WriteNode(tempReader, true);
				tempReader.Close();
				writer.Flush();

				if (!string.IsNullOrEmpty(concatedXPAth))
				{
					foreach (var resultStream in GetDebatchedChiledStreams(concatedXPAth, childStream))
					{
						long parentStreamPosition = parentStream.Position;
						result.Add(MergeStreams(parentStream, resultStream));
						parentStream.Position = parentStreamPosition;
					}
				}
				else result.Add(childStream);
			}

			return result.ToArray();
		}

		Stream MergeStreams(Stream parentStream, Stream childStream)
		{
			parentStream.SeekBegin();
			childStream.SeekBegin();

			var parentReader = XmlTextReader.Create(parentStream);
			parentReader.MoveToContent();
			var childReader = XmlTextReader.Create(childStream);
			childReader.MoveToContent();

			var resultStream = new VirtualStream();
			var resultWriter = XmlWriter.Create(resultStream, new XmlWriterSettings() { OmitXmlDeclaration = true });

			resultWriter.WriteStartElement(parentReader.Prefix, parentReader.LocalName, parentReader.NamespaceURI);

			bool childStreamProcessed = false;
			while (parentReader.Read())
			{
				if (parentReader.NodeType == XmlNodeType.Element)
				{
					if (parentReader.LocalName == childReader.LocalName)
					{
						if (!childStreamProcessed)
						{
							var tempReader = childReader.ReadSubtree();
							resultWriter.WriteNode(tempReader, false);
							tempReader.Close();
							childStreamProcessed = true;
						}
						parentReader.ReadSubtree().Close();
					}
					else
					{
						var tempReader = parentReader.ReadSubtree();
						resultWriter.WriteNode(tempReader, false);
						tempReader.Close();
					}
				}
			}
			resultWriter.WriteEndElement();
			resultWriter.Flush();

			resultStream.SeekBegin();
			return resultStream;
		}

		string ConcatXPath(List<string> xPathSplit)
		{
			string result = string.Empty;
			Array.ForEach<string>(xPathSplit.ToArray(), (string xPathElement) => { result += "/*" + xPathElement; });

			return result;
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("9219D890-0C8B-4A9A-B31A-46160C582579");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			var var = LoadProperty(propertyBag, "Enabled", errorLog);
			if (var != null) Enabled = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "XPathToDebatch", errorLog);
			if (var != null) XPathToDebatch = Convert.ToString(var);
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
			object val = Enabled;
			propertyBag.Write("Enabled", ref val);

			val = XPathToDebatch;
			propertyBag.Write("XPathToDebatch", ref val);
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }
		public string XPathToDebatch { get; set; }

		#endregion

	}
}

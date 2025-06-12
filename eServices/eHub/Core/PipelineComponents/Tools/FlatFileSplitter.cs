using System.Collections.Generic;
using System.Xml;
using CargoWise.eHub.Common.Extensions;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Component.Utilities;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;
using Microsoft.BizTalk.XPath;

namespace CargoWise.eHub.Core.PipelineComponents.Tools
{
	interface IFlatFileSplitter
	{
		IBaseMessage[] Split(IBaseMessage message, IPipelineContext pipelineContext);
	}

	class FlatFileSplitter : IFlatFileSplitter
	{
		public FlatFileSplitter(string xPath, string childElementName, string messageSchema)
		{
			this.XPath = xPath;
			this.ChildElementName = childElementName;
			this.MessageSchema = messageSchema;
		}

		readonly string XPath;
		readonly string ChildElementName;
		readonly string MessageSchema;

		public IBaseMessage[] Split(IBaseMessage message, IPipelineContext pipelineContext)
		{
			var dasm = new FFDisassembleHelper();
			var ffMessage = dasm.Disassemble(pipelineContext, message, new SchemaWithNone(MessageSchema));

			var debatcher = new CloneDebatchDisassembleComponent();
			debatcher.Enabled = true;
			debatcher.XPathToDebatch = XPath;
			debatcher.Disassemble(pipelineContext, ffMessage);

			IBaseMessage msg = null; ;
			while ((msg = debatcher.GetNext(pipelineContext)) != null)
			{
				var reader = XmlReader.Create(msg.BodyPart.GetOriginalDataStream());
				var xPaths = new XPathCollection() { XPath };
				var xPathReader = new XPathReader(reader, xPaths);
				string value = string.Empty;
				if (xPathReader.ReadUntilMatch())
				{
					xPathReader.Read();
					value = xPathReader.Value;
				}

				AddToDictionary(value, msg);
			}

			var result = new List<IBaseMessage>();
			foreach (var pair in MessageGroupDictionary)
			{
				result.Add(BatchMessages(pair.Value, pipelineContext));
			}

			return result.ToArray();
		}

		IBaseMessage BatchMessages(List<IBaseMessage> messageList, IPipelineContext pipelineContext)
		{
			var asm = new FFAssembleHelper();
			var streamWrapper = new StreamWrapperComponent();

			if (messageList.Count == 1)
			{
				messageList[0].BodyPart.GetOriginalDataStream().SeekBegin();

				return streamWrapper.Execute(pipelineContext, asm.Assemble(pipelineContext, messageList[0]));
			}

			var resultStream = new VirtualStream();
			var writer = XmlWriter.Create(resultStream);

			var firsMessageStream = messageList[0].BodyPart.GetOriginalDataStream();
			firsMessageStream.SeekBegin();
			var firsMessageReader = XmlReader.Create(firsMessageStream);
			firsMessageReader.MoveToContent();

			writer.WriteStartElement(firsMessageReader.Prefix, firsMessageReader.LocalName, firsMessageReader.NamespaceURI);

			WriteMessagePart(writer, firsMessageReader);

			for (int i = 1; i < messageList.Count; i++)
			{
				var messageStream = messageList[i].BodyPart.GetOriginalDataStream();
				messageStream.SeekBegin();
				var messageReader = XmlReader.Create(messageStream);
				messageReader.MoveToContent();

				WriteMessagePart(writer, messageReader);
			}

			writer.WriteEndElement();
			writer.Flush();
			resultStream.SeekBegin();

			var outMsg = pipelineContext.GetMessageFactory().CreateMessage();
			outMsg.AddPart("Body", pipelineContext.GetMessageFactory().CreateMessagePart(), true);
			outMsg.Context = PipelineUtil.CloneMessageContext(messageList[0].Context);
			outMsg.BodyPart.Data = resultStream;

			return streamWrapper.Execute(pipelineContext, asm.Assemble(pipelineContext, outMsg));
		}

		void WriteMessagePart(XmlWriter writer, XmlReader reader)
		{
			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Element && reader.LocalName == ChildElementName)
				{
					var tempReader = reader.ReadSubtree();
					writer.WriteNode(tempReader, false);
					tempReader.Close();
					break;
				}
			}
		}

		void AddToDictionary(string value, IBaseMessage msg)
		{
			if (MessageGroupDictionary.ContainsKey(value))
			{
				MessageGroupDictionary[value].Add(msg);
			}
			else
				MessageGroupDictionary.Add(value, new List<IBaseMessage> { msg });
		}


		Dictionary<string, List<IBaseMessage>> MessageGroupDictionary
		{
			get { return messageGroupDictionary ?? (messageGroupDictionary = new Dictionary<string, List<IBaseMessage>>()); }
		}
		Dictionary<string, List<IBaseMessage>> messageGroupDictionary;
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Component.Utilities;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;
using Microsoft.BizTalk.XPath;

namespace CargoWise.eHub.Core.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_DisassemblingParser)]
	[Guid("C31EA9ED-988C-4CF6-A1BC-FDEA8C487315")]
	public class FlatFileSchemaSplitDisassembleComponent : IBaseComponent, IComponentUI, IDisassemblerComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return string.Empty; }
		}

		public string Name
		{
			get { return "Flat File Schema Split Disassembler"; }
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

		public void Disassemble(IPipelineContext pipelineContext, IBaseMessage message)
		{
			var msgHelper = GetMessageHelper();

			if (!Enabled)
			{
				msgHelper.EnqueueMessage(pipelineContext, MessageQueue, message, message, ProcessSubscriptions, false);
				return;
			}

			if (!InitialSchemaSpecified)
				throw new ApplicationException("InitialSchema must be specified");
			if (string.IsNullOrEmpty(SplitElementXPath))
				throw new ApplicationException("SplitElementXPath must be specified");
			if (string.IsNullOrEmpty(ValueList))
				throw new ApplicationException("ValueList must be specified");
			if (string.IsNullOrEmpty(SchemaNameList))
				throw new ApplicationException("SchemaNameList must be specified");
			if (string.IsNullOrEmpty(DebatchElementXPath))
				throw new ApplicationException("DebatchElementXPath must be specified");
			if (string.IsNullOrEmpty(ChildElementName))
				throw new ApplicationException("ChildElementName must be specified");

			var values = new List<string>(ValueList.Split(';'));
			var schemas = new List<string>(SchemaNameList.Split(';'));
			var duplicateSchemas = schemas.Distinct().Count() == 1;

			if (values.Count != schemas.Count)
				throw new ApplicationException("Number of schemas must equals number of values");

			var splitter = GetFlatFileSplitter();
			foreach (var ffMessage in splitter.Split(GetClone(pipelineContext, message), pipelineContext))
			{
				var ffDasm = GetFFDisassembler();
				var streamWrapper = new StreamWrapperComponent();
				var messageToProcess = streamWrapper.Execute(pipelineContext, ffDasm.Disassemble(pipelineContext, GetClone(pipelineContext, GetClone(pipelineContext, ffMessage)), new SchemaWithNone(InitialSchema.ToString())));

				var reader = XmlReader.Create(messageToProcess.BodyPart.GetOriginalDataStream());
				var xPathCollection = new XPathCollection() { SplitElementXPath };
				var xPathReader = new XPathReader(reader, xPathCollection);

				var processedValuesList = new List<string>();
				bool initialSchemaMessageIncluded = false;

				var tempQueue = new Queue<IBaseMessage>();

				while (xPathReader.ReadUntilMatch())
				{
					if (xPathReader.NodeType == XmlNodeType.Element)
					{
						xPathReader.Read();
						string value = xPathReader.Value;
						int index = values.IndexOf(value);
						if (index != -1 && !processedValuesList.Contains(value))
						{
							string schemaName = schemas[index];

							//Workaround to prevent null as a result of disassemble. 
							if (message.BodyPart.Data.CanRead) { }

							var newMsg = ffDasm.Disassemble(pipelineContext, GetClone(pipelineContext, ffMessage), new SchemaWithNone(schemaName));
							if (newMsg == null) throw new ApplicationException(string.Format("Unable to disassemble message based on schema '{0}'", schemaName));
							tempQueue.Enqueue(newMsg);

							if (duplicateSchemas)
							{
								foreach (var val in values)
								{
									processedValuesList.Add(val);
								}
							}
							else
							{
								processedValuesList.Add(value);
							}
						}
						else if (!initialSchemaMessageIncluded && !processedValuesList.Contains(value))
						{
							tempQueue.Enqueue(messageToProcess);
							initialSchemaMessageIncluded = true;
						}
					}
				}
				messageToProcess.BodyPart.GetOriginalDataStream().SeekBegin();

				while (tempQueue.Count > 0)
				{
					var tempMessage = tempQueue.Dequeue();
					msgHelper.EnqueueMessage(pipelineContext, MessageQueue, tempMessage, message, ProcessSubscriptions, false);
				}
			}
		}

		internal virtual IBaseMessage GetClone(IPipelineContext pipelineContext, IBaseMessage message)
		{
			var result = pipelineContext.GetMessageFactory().CreateMessage();
			result.Context = PipelineUtil.CloneMessageContext(message.Context);
			var internalMessagePart = pipelineContext.GetMessageFactory().CreateMessagePart();
			string partName = string.Empty;
			message.GetPartByIndex(0, out partName);
			result.AddPart(partName, internalMessagePart, true);
			result.BodyPart.Charset = message.BodyPart.Charset;
			result.BodyPart.ContentType = message.BodyPart.ContentType;

			var stream = new VirtualStream();
			message.BodyPart.Data.WriteTo(stream);
			stream.SeekBegin();
			result.BodyPart.Data = stream;
			message.BodyPart.Data.SeekBegin();

			return result;
		}

		internal virtual IFlatFileSplitter GetFlatFileSplitter()
		{
			return new FlatFileSplitter(DebatchElementXPath, ChildElementName, InitialSchema.ToString());
		}

		internal virtual IMessageHelper GetMessageHelper()
		{
			return new MessageHelper();
		}

		internal virtual IFFDisassembleHelper GetFFDisassembler()
		{
			return new FFDisassembleHelper();
		}

		public IBaseMessage GetNext(IPipelineContext pipelineContext)
		{
			if (MessageQueue.Count > 0)
				return MessageQueue.Dequeue() as IBaseMessage;
			else
				return null;
		}

		Queue MessageQueue
		{
			get { return messageQueue ?? (messageQueue = new Queue()); }
		}
		Queue messageQueue;

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("C31EA9ED-988C-4CF6-A1BC-FDEA8C487315");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			var var = LoadProperty(propertyBag, "Enabled", errorLog);
			if (var != null) Enabled = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "InitialSchema", errorLog);
			if (var != null) InitialSchema = new Schema((string)var);

			var = LoadProperty(propertyBag, "SplitElementXPath", errorLog);
			if (var != null) SplitElementXPath = (string)var;

			var = LoadProperty(propertyBag, "ValueList", errorLog);
			if (var != null) ValueList = (string)var;

			var = LoadProperty(propertyBag, "SchemaNameList", errorLog);
			if (var != null) SchemaNameList = (string)var;

			var = LoadProperty(propertyBag, "ProcessSubscriptions", errorLog);
			if (var != null) ProcessSubscriptions = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, "DebatchElementXPath", errorLog);
			if (var != null) DebatchElementXPath = (string)var;

			var = LoadProperty(propertyBag, "ChildElementName", errorLog);
			if (var != null) ChildElementName = (string)var;
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

			val = InitialSchemaSpecified ? InitialSchema.ToString() : string.Empty;
			propertyBag.Write("InitialSchema", ref val);

			val = SplitElementXPath;
			propertyBag.Write("SplitElementXPath", ref val);

			val = ValueList;
			propertyBag.Write("ValueList", ref val);

			val = SchemaNameList;
			propertyBag.Write("SchemaNameList", ref val);

			val = ProcessSubscriptions;
			propertyBag.Write("ProcessSubscriptions", ref val);

			val = DebatchElementXPath;
			propertyBag.Write("DebatchElementXPath", ref val);

			val = ChildElementName;
			propertyBag.Write("ChildElementName", ref val);
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }
		public Schema InitialSchema { get; set; }
		public string SplitElementXPath { get; set; }
		public string ValueList { get; set; }
		public string SchemaNameList { get; set; }
		public bool ProcessSubscriptions { get; set; }
		public string DebatchElementXPath { get; set; }
		public string ChildElementName { get; set; }

		bool InitialSchemaSpecified
		{
			get { return (InitialSchema != null && !String.IsNullOrEmpty(InitialSchema.AssemblyName)); }
		}

		#endregion
	}
}

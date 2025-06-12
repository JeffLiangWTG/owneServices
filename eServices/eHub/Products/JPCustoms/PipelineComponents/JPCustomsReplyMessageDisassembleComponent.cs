using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Component.Utilities;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.JPCustoms.PipelineComponents
{
	[Serializable]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_DisassemblingParser)]
	[Guid("7A275CC8-61B7-49C2-B305-792BC8E2C921")]
	public class JPCustomsReplyMessageDisassembleComponent : IBaseComponent, IComponentUI, IDisassemblerComponent, IPersistPropertyBag
	{

		const string SAHRSchemaName = "CargoWise.eHub.Products.JPCustoms.Schemas.AHRResponseFlatFileSchema, CargoWise.eHub.Products.JPCustoms.Schemas, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350";
		const string SAMRSchemaName = "CargoWise.eHub.Products.JPCustoms.Schemas.AMRResponseFlatFileSchema, CargoWise.eHub.Products.JPCustoms.Schemas, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350";
		const string SATDSchemaName = "CargoWise.eHub.Products.JPCustoms.Schemas.ATDResponseFlatFileSchema, CargoWise.eHub.Products.JPCustoms.Schemas, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350";
		const string SCMVSchemaName = "CargoWise.eHub.Products.JPCustoms.Schemas._2017.CMVResponseFlatFileSchema, CargoWise.eHub.Products.JPCustoms.Schemas, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350";
		const string SAS111SchemaName = "CargoWise.eHub.Products.JPCustoms.Schemas.SAS111FlatFileSchema, CargoWise.eHub.Products.JPCustoms.Schemas, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350";
		const string SAS108SchemaName = "CargoWise.eHub.Products.JPCustoms.Schemas.SAS108FlatFileSchema, CargoWise.eHub.Products.JPCustoms.Schemas, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350";
		const string SAS135SchemaName = "CargoWise.eHub.Products.JPCustoms.Schemas.SAS135FlatFileSchema, CargoWise.eHub.Products.JPCustoms.Schemas, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350";
		const string SAS144SchemaName = "CargoWise.eHub.Products.JPCustoms.Schemas._2017.SAS144FlatFileSchema, CargoWise.eHub.Products.JPCustoms.Schemas, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350";
		const string SAS148SchemaName = "CargoWise.eHub.Products.JPCustoms.Schemas._2017.SAS148FlatFileSchema, CargoWise.eHub.Products.JPCustoms.Schemas, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350";
		const string SAS155SchemaName = "CargoWise.eHub.Products.JPCustoms.Schemas._2017.SAS155FlatFileSchema, CargoWise.eHub.Products.JPCustoms.Schemas, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350";
		const string SAS157SchemaName = "CargoWise.eHub.Products.JPCustoms.Schemas._2017.SAS157FlatFileSchema, CargoWise.eHub.Products.JPCustoms.Schemas, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350";
		const string TCCOutputSchemaName = "CargoWise.eHub.Products.JPCustoms.Schemas.TCCOutputFlatFileSchema, CargoWise.eHub.Products.JPCustoms.Schemas, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350";
		const string SystemCommonErrorSchemaName = "CargoWise.eHub.Products.JPCustoms.Schemas.SystemCommonErrorResponseFlatFileSchema, CargoWise.eHub.Products.JPCustoms.Schemas, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350";

		#region IBaseComponent Members

		public string Description
		{
			get { return "Disassemble JPCustoms Reply message"; }
		}

		public string Name
		{
			get { return "Disassemble JPCustoms Reply message"; }
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
			#region Route for not enabled
			if (!Enabled)
			{
				MessageQueue.Enqueue(message);
				return;
			}
			#endregion

			IBaseMessage result = null;
			var ffDasm = GetFFDisassembler();
			long dataLength = 0;
			if (message != null && message.BodyPart != null)
			{
				var dataStream = message.BodyPart.GetOriginalDataStream();
				dataLength = dataStream.Length;
				if (dataStream != null && dataLength >= 398)
				{
					byte[] readBuffer = new byte[15];
					var testReadResult = dataStream.Read(readBuffer, 0, 15);
					dataStream.Seek(0, SeekOrigin.Begin);
					if (testReadResult == 15)
					{
						var messageTypeIndicator = System.Text.Encoding.ASCII.GetString(readBuffer, 8, 7);
						var messageSchemaName = String.Empty;
						switch (messageTypeIndicator.Trim())
						{
							case "*SAHR":
							case "*SCHR":
								messageSchemaName = SAHRSchemaName;
								break;
							case "*SAMR":
							case "*SCMR":
								messageSchemaName = SAMRSchemaName;
								break;
							case "*SATD":
								messageSchemaName = SATDSchemaName;
								break;
							case "*SCMV":
								messageSchemaName = SCMVSchemaName;
								break;
							case "SAS1110":
							case "SAS1120":
								messageSchemaName = SAS111SchemaName;
								break;
							case "SAS1080":
								messageSchemaName = SAS108SchemaName;
								break;
							case "SAS1350":
								messageSchemaName = SAS135SchemaName;
								break;
							case "CAQ0010":
								messageSchemaName = TCCOutputSchemaName;
								break;
							case "*CCMSG":
								messageSchemaName = SystemCommonErrorSchemaName;
								break;
							case "SAS1440":
								messageSchemaName = SAS144SchemaName;
								break;
							case "SAS1480":
								messageSchemaName = SAS148SchemaName;
								break;
							case "SAS1550":
								messageSchemaName = SAS155SchemaName;
								break;
							case "SAS1570":
								messageSchemaName = SAS157SchemaName;
								break;
						}

						if (!string.IsNullOrEmpty(messageSchemaName))
						{
							result = ffDasm.Disassemble(pipelineContext, message, new SchemaWithNone(messageSchemaName));
						}
					}
				}
			}

			if (result != null)
			{
				if (dataLength > 0)
				{
					result.Context.WriteProperty<UncompressedLength>(dataLength);
				}
				MessageQueue.Enqueue(result);
			}
			else
			{
				throw new ApplicationException("The message is not recognized as a proper JPCustoms reply message");
			}
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

		#endregion

		#region IPersistPropertyBag Members

		public void InitNew()
		{
			throw new NotImplementedException();
		}

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("B512DDA5-00DA-4723-94DD-D81DDB6DCE36");
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object val = null;
			Func<string, bool> getVal = p => { try { propertyBag.Read(p, out val, errorLog); } catch { } return val != null; };

			if (getVal("Enabled")) Enabled = Convert.ToBoolean(val);
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val;
			val = Enabled; propertyBag.Write("Enabled", ref val);
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }

		Queue MessageQueue
		{
			get { return messageQueue ?? (messageQueue = new Queue()); }
		}
		Queue messageQueue;

		#endregion

	}
}


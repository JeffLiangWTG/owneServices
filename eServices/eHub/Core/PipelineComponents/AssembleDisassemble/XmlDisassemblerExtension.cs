using System;
using System.Collections;
using System.Runtime.InteropServices;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using Microsoft.BizTalk.Component;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_DisassemblingParser)]
	[Guid("C1E692C3-E69E-4CFE-8825-14685AC3C0F9")]
	public class XmlDisassemblerExtension : XmlDasmComp, IBaseComponent, IDisassemblerComponent, IPersistPropertyBag, IComponentUI
	{
		public XmlDisassemblerExtension() : base() { }

		#region IBaseComponent

		public new string Description
		{
			get { return string.Empty; }
		}

		public new string Name
		{
			get { return "XML Disassembler Extension"; }
		}

		public new string Version
		{
			get { return "1.0"; }
		}

		#endregion

		#region IDisassemblerComponent

		public new void Disassemble(IPipelineContext pContext, IBaseMessage pInMsg)
		{
			Tracer.TraceStart(pContext, pInMsg);
			Tracer.TraceInfo("AS2Enabled: {0}", this.AS2Enabled);

			try
			{
				Tracer.TraceInfo("Disassemble");

				var msgHelper = GetMessageHelper();

				if (this.AS2Enabled)
				{
					IBaseMessage mdn = GetAS2DasmHelper().Disassemble(pContext, pInMsg);
					if (mdn != null)
					{
						MessageQueue.Enqueue(mdn);
					}
				}

				BaseDisassemble(pContext, pInMsg);
				IBaseMessage message = BaseGetNext(pContext);
				msgHelper.EnqueueMessage(pContext, MessageQueue, message, message, false, false);
			}
			catch (Exception ex)
			{
				Tracer.TraceError(ex);
				throw;
			}
			finally
			{
				Tracer.TraceEnd();
			}
		}

		internal virtual void BaseDisassemble(IPipelineContext pContext, IBaseMessage pInMsg)
		{
			base.Disassemble(pContext, pInMsg);
		}

		internal virtual IBaseMessage BaseGetNext(IPipelineContext pContext)
		{
			return base.GetNext(pContext);
		}

		public new IBaseMessage GetNext(IPipelineContext pContext)
		{
			if (MessageQueue.Count > 0)
				return MessageQueue.Dequeue() as IBaseMessage;
			else
				return null;
		}

		internal virtual IAS2DisassembleHelper GetAS2DasmHelper()
		{
			return new AS2DisassembleHelper();
		}

		internal virtual IMessageHelper GetMessageHelper()
		{
			return new MessageHelper();
		}

		Queue MessageQueue
		{
			get { return messageQueue ?? (messageQueue = new Queue()); }
		}
		Queue messageQueue;

		#endregion

		#region IPersistPropertyBag

		public bool AS2Enabled { get; set; }

		public new void GetClassID(out Guid classID)
		{
			classID = new Guid("C1E692C3-E69E-4CFE-8825-14685AC3C0F9");
		}

		public new void Load(IPropertyBag propertyBag, int errorLog)
		{
			var var = LoadProperty(propertyBag, "AS2Enabled", errorLog);
			if (var != null) AS2Enabled = Convert.ToBoolean(var);

			base.Load(propertyBag, errorLog);
		}

		public new void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			base.Save(propertyBag, clearDirty, saveAllProperties);

			object val = AS2Enabled;
			propertyBag.Write("AS2Enabled", ref val);
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

		#endregion

		#region IComponentUI

		public new IntPtr Icon
		{
			get { return IntPtr.Zero; }
		}

		public new IEnumerator Validate(object projectSystem)
		{
			return null;
		}

		#endregion

	}
}

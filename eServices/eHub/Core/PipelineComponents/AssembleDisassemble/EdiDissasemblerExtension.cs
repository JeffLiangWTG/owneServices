namespace CargoWise.eHub.Core.PipelineComponents
{
	using System;
	using System.Collections;
	using System.Runtime.InteropServices;
    using CargoWise.eHub.Common;
    using CargoWise.eHub.Core.PipelineComponents.Tools;
	using CargoWise.eHub.Core.PropertySchemas;
	using CargoWise.eHub.DataAccess.Integration;
	using CargoWise.eHub.DataAccess.Sql;
	using Microsoft.BizTalk.Component.Interop;
	using Microsoft.BizTalk.Edi.Pipelines;
	using Microsoft.BizTalk.Message.Interop;

	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_DisassemblingParser)]
	[Guid("F2E12BD7-DEE6-4A40-B775-8D9B452AF881")]
	public class EdiDissasemblerExtension : EdiDisassembler, IBaseComponent, IDisassemblerComponent, IPersistPropertyBag, IComponentUI
	{
		public EdiDissasemblerExtension() : base() { }

		#region IBaseComponent

		public new string Description
		{
			get { return string.Empty; }
		}

		public new string Name
		{
			get { return "EDI Disassembler Extension"; }
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
			Tracer.TraceInfo("ProcessSubscriptions: {0}", this.ProcessSubscriptions);
			Tracer.TraceInfo("DiscardUnsubscribedMessages: {0}", this.DiscardUnsubscribedMessages);
			Tracer.TraceInfo("NormalizeLineEndings: {0}", this.NormalizeLineEndings);

			try
			{
				if (NormalizeLineEndings)
					pInMsg.BodyPart.Data = new NormalizedLineEndingsStream(pInMsg.BodyPart.GetOriginalDataStream(), string.Empty);

				Tracer.TraceInfo("Disassemble");
				BaseDisassemble(pContext, pInMsg);

				IBaseMessage message;
				var msgHelper = GetMessageHelper();
				int msgCount = 0;
				while ((message = BaseGetNext(pContext)) != null)
				{
					Tracer.TraceInfo("Enqueue message {0}", ++msgCount);
					if (PropertyInspector.IsAs2Mdn(message) || PropertyInspector.IsSystemGeneratedEdiAck(message) || string.IsNullOrWhiteSpace(message.Context.ReadPropertyString<BTS.MessageType>()))
					{
						MessageQueue.Enqueue(message);
					}
					else
					{
						msgHelper.EnqueueMessage(pContext, MessageQueue, message, message, ProcessSubscriptions,
							DiscardUnsubscribedMessages);
					}

				}
				if (DiscardUnsubscribedMessages && MessageQueue.Count == 0)
				{
					string trackingID = pInMsg.Context.ReadPropertyString<MessageTrackingID>();
					if (!String.IsNullOrWhiteSpace(trackingID))
						GetOutboxAccessor().UpdateInboxMessageDistributionStatus(trackingID);
					Tracer.TraceInfo("Message discarded. MessageTrackingID = '{0}'", trackingID);
				}
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

		internal virtual IMessageHelper GetMessageHelper()
		{
			return new MessageHelper();
		}

		internal virtual IOutboxAccessor GetOutboxAccessor()
		{
			return new OutboxAccessor();
		}

		Queue MessageQueue
		{
			get { return messageQueue ?? (messageQueue = new Queue()); }
		}
		Queue messageQueue;

		#endregion

		#region IPersistPropertyBag

		public bool ProcessSubscriptions { get; set; }

		public bool DiscardUnsubscribedMessages { get; set; }

		public bool NormalizeLineEndings { get; set; }

		public new void GetClassID(out Guid classID)
		{
			classID = new Guid("F2E12BD7-DEE6-4A40-B775-8D9B452AF881");
		}

		public new void Load(IPropertyBag propertyBag, int errorLog)
		{
			var var = LoadProperty(propertyBag, "ProcessSubscriptions", errorLog);
			if (var != null) ProcessSubscriptions = Convert.ToBoolean(var);
			var = LoadProperty(propertyBag, "DiscardUnsubscribedMessages", errorLog);
			if (var != null) DiscardUnsubscribedMessages = Convert.ToBoolean(var);
			var = LoadProperty(propertyBag, "NormalizeLineEndings", errorLog);
			if (var != null) NormalizeLineEndings = Convert.ToBoolean(var);

			base.Load(propertyBag, errorLog);
		}

		public new void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			base.Save(propertyBag, clearDirty, saveAllProperties);

			object val = ProcessSubscriptions;
			propertyBag.Write("ProcessSubscriptions", ref val);
			val = DiscardUnsubscribedMessages;
			propertyBag.Write("DiscardUnsubscribedMessages", ref val);
			val = NormalizeLineEndings;
			propertyBag.Write("NormalizeLineEndings", ref val);
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

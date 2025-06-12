using System;
using System.Collections;
using System.Runtime.InteropServices;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.BizTalk.Component;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents
{

	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_DisassemblingParser)]
	[Guid("418BB34A-2AAB-4B0E-9C0E-D73D13ECBCE2")]
	public class FlatFileDisassembleComponent : FFDasmComp, IBaseComponent, IDisassemblerComponent, IPersistPropertyBag, IComponentUI
	{
		#region IBaseComponent

		public new string Description
		{
			get { return string.Empty; }
		}

		public new string Name
		{
			get { return "Flat File Disassembler Extension"; }
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
			Tracer.TraceInfo("ProcessSubscriptions: {0}", ProcessSubscriptions);
			Tracer.TraceInfo("DiscardUnsubscribedMessages: {0}", DiscardUnsubscribedMessages);

			try
			{
				Tracer.TraceInfo("Disassemble");
				BaseDisassemble(pContext, pInMsg);

				var msgHelper = GetMessageHelper();
				int messageCount = 0;

				IBaseMessage message;
				while ((message = BaseGetNext(pContext)) != null)
				{
					Tracer.TraceInfo("Enqueue message {0}", ++messageCount);
					msgHelper.EnqueueMessage(pContext, MessageQueue, message, message, ProcessSubscriptions, DiscardUnsubscribedMessages);
				}

				if (DiscardUnsubscribedMessages && MessageQueue.Count == 0)
				{
					string trackingId = pInMsg.Context.ReadPropertyString<MessageTrackingID>();
					if (!string.IsNullOrWhiteSpace(trackingId))
						GetOutboxAccessor().UpdateInboxMessageDistributionStatus(trackingId);
					Tracer.TraceInfo("Message discarded. MessageTrackingID = '{0}'", trackingId);
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

		public new void GetClassID(out Guid classID)
		{
			classID = new Guid("418BB34A-2AAB-4B0E-9C0E-D73D13ECBCE2");
		}

		public new void Load(IPropertyBag propertyBag, int errorLog)
		{
			var var = LoadProperty(propertyBag, nameof(ProcessSubscriptions), errorLog);
			if (var != null) ProcessSubscriptions = Convert.ToBoolean(var);

			var = LoadProperty(propertyBag, nameof(DiscardUnsubscribedMessages), errorLog);
			if (var != null) DiscardUnsubscribedMessages = Convert.ToBoolean(var);

			base.Load(propertyBag, errorLog);
		}

		public new void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			base.Save(propertyBag, clearDirty, saveAllProperties);

			object  val = ProcessSubscriptions;
			propertyBag.Write(nameof(ProcessSubscriptions), ref val);

			val = DiscardUnsubscribedMessages;
			propertyBag.Write(nameof(DiscardUnsubscribedMessages), ref val);
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

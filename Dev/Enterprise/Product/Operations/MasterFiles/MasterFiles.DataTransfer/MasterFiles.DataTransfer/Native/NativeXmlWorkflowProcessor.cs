using System;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Native
{
	public class NativeXmlWorkflowProcessor : INativeXmlWorkflowProcessor
	{
		public NativeXmlWorkflowProcessor(ActionWrapper action, Lazy<MessageProcessorCommunicationModesResult> modes, IEventInfo eventInfo = null)
		{
			this.action = action;
			this.wrapped = action.ParentBO;
			this.modes = modes;
			this.eventInfo = eventInfo;
		}

		readonly ActionWrapper action;
		readonly IEventInfo eventInfo;
		readonly BusinessObject wrapped;
		readonly Lazy<MessageProcessorCommunicationModesResult> modes;

		IMessageProcessorCommunicationModesResult IMessageProcessor.GetDestinations() => modes.Value;

		void IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			var dataContext = DataContextFactory.New();
			new DataContextDataObjectWriter().PopulateDataObject(action, dataContext, eventInfo);

			var context = new DeliveryContext(action.FactoryForProcessing)
			{
				ParentInfo = EntityInfo.New(wrapped),
				ApplicationCode = ApplicationCodeList.Codes.NativeDataMessaging,
				MessageTypeCode = EDIMessageTypeList.Codes.XDC,
				MessageSubTypeCode = wrapped.GetType().GetNativeMessageSubTypeFromObjectType(),
				Notifications = notifications
			};

			var deliveryStreamWrapper = new DeliveryStreamWrapperNativeXML(context.ParentInfo, wrapped, dataContext);

			var jobNumber = JobNumberResolver.GetJobNumber(wrapped);
			var delivery = (!jobNumber.IsEmpty) ? new EDIMessageDelivery(jobNumber) : new EDIMessageDelivery();
			delivery.Deliver(context, modes.Value.CommunicationModes, deliveryStreamWrapper);
		}
	}
}


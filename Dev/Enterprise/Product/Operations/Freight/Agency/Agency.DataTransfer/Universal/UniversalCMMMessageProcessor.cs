using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Freight.Agency.DataTransfer.MessageProcessing;
using Enterprise.Messaging.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	public class UniversalCMMMessageProcessor : CMMMessageProcessor
	{
		public UniversalCMMMessageProcessor(BusinessObjectFactory factory)
			: base(new LoggingInformation())
		{
			this.factory = Argument.NotNull(factory, "factory");
		}

		readonly BusinessObjectFactory factory;
		UniversalEvent eventDataObject;

		internal class NonPersistentEDIMessage : EDIMessage
		{
			public NonPersistentEDIMessage(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void SetDefaultValues()
			{
				base.SetDefaultValues();
				EM_Status = EDIMessage.Status.Queued;
			}

			public override bool IsSavedByFactory
			{
				get { return false; }
			}
		}

		public bool ProcessEvent(UniversalEvent eventDO)
		{
			this.eventDataObject = eventDO;
			var message = factory.New<NonPersistentEDIMessage>();
			if (this.eventDataObject != null)
			{
				ProcessMessage(message);
			}

			return message.EM_Status == EDIMessage.Status.Recognised;
		}

		public new void ProcessMessage(EDIMessage message)
		{
			base.ProcessMessage(message);
		}

		protected override ICMMProcessingAdapter GetCMMProcessingAdapter(EDIMessage message)
		{
			return new UniversalCMMProcessingAdapter(factory, eventDataObject);
		}
	}
}



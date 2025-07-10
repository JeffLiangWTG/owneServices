using Enterprise.BatchProcessor;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.DataTransfer.MessageProcessing;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Agency.ServiceTasks
{
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	internal sealed class EdifactCMMMessageProcessor : CMMMessageProcessor
	{
		public EdifactCMMMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override ICMMProcessingAdapter GetCMMProcessingAdapter(EDIMessage message)
		{
			return new EdifactCMMProcessingAdapter(message);
		}

		protected override string DoProcessingReturningStatus(EDIMessage message)
		{
			if (AgencyRegistry.Instance.AllowDirectCODECOCOARRICMMMessaging.Value)
			{
				return base.DoProcessingReturningStatus(message);
			}
			else
			{
				Logger.Log("This message has failed as the registry setting to allow direct sending of Container Management messages with CODECO/COARRI has not been enabled. Please send CM messages through eHub/eAdaptor or contact Cargowise to enable CODECO/COARRI.");
				return EDIMessage.Status.Failed;
			}
		}
	}
}

































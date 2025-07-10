using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Freight.Agency.ServiceTasks
{
	internal sealed class CMMMessageTypeProcessor : ApplicationTypeMessageProcessor
	{
		public CMMMessageTypeProcessor(LoggingInformation logger)
			: base(logger) { }

		#region Implementation

		protected override string ApplicationCodeCore
		{
			get { return EDIInterchange.ApplicationCodes.ContainerManagement; }
		}

		protected override string MessageFriendlyNameCore
		{
			get { return Res.GetString("3e90e9a3-36bf-4f73-a986-e74b666ea333", "Container Management"); }
		}

		protected override void ProcessMessageCore(EDIMessage message)
		{
			new EdifactCMMMessageProcessor(Logger).ProcessMessage(message);
		}

		#endregion
	}
}

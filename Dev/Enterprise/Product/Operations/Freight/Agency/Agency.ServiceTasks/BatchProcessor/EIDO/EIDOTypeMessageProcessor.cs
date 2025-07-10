using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Freight.Agency.ServiceTasks
{
	internal sealed class EIDOTypeMessageProcessor : ApplicationTypeMessageProcessor
	{
		public EIDOTypeMessageProcessor(LoggingInformation logger)
			: base(logger) { }

		#region Implementation

		protected override string ApplicationCodeCore
		{
			get { return EDIInterchange.ApplicationCodes.EIDO; }
		}

		protected override string MessageFriendlyNameCore
		{
			get { return Res.GetString("5cd5cf51-f33a-4f0d-a844-d902f1cf890b", "E-IDO Response"); }
		}

		protected override void ProcessMessageCore(EDIMessage message)
		{
			new EIDOMessageProcessor(Logger).ProcessMessage(message);
		}

		#endregion
	}
}

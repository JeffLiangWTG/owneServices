using System.Threading;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.BatchProcessor
{
	public class InterchangeSender : BaseInterchangeSender
	{
		public InterchangeSender(LoggingInformation logger)
			: base(logger)
		{
		}

		public const string EcnNetworkEmailAddress = "nzcsedi@ecnetwork.co.nz";

		protected override void SendOutboundInterchanges(CancellationToken token)
		{
			NumberOfOperationsAttempted = 0;
			SendOutboundInterchanges(EDIInterchange.ApplicationCodes.NewZealandCustoms, token);
		}

		protected override bool SendInt(EDIInterchange interchange)
		{
			bool success = false;

			if (SendInterchangeEmailMessages(interchange))
			{
				interchange.LogInterchangeInProgressForAllMessages();
				Logger.Log("Interchange #" + interchange.EI_InterchangeNum + " sent");
				interchange.EI_Status = EDIInterchange.Status.Sent;
				success = true;
			}
			else
			{
				Logger.Log("Interchange #" + interchange.EI_InterchangeNum + " failed to send.");
				interchange.EI_Status = EDIInterchange.Status.Failed;
			}

			return success;
		}

		bool SendInterchangeEmailMessages(EDIInterchange interchange)
		{
			var interchangeSender = new OutgoingInterchange(interchange);
			return interchangeSender.Send(Logger, EcnNetworkEmailAddress);
		}

		#region Overrides

		protected override void PrepareInterchanges(string[] applicationCode)
		{
			//already prepared
		}

		protected override void PackageMessagesIntoInterchanges(NonDependentEDIMessageCollection messages)
		{
			//already packaged
		}

		#endregion
	}
}

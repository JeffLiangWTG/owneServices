using Enterprise.Freight.Forwarding.ServiceTasks.CargoIMP;
using Enterprise.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.SG.V4.ServiceTasks.CMDMessage
{
	public class CMDMessageSender : CargoIMPMessageSender
	{
		public CMDMessageSender(ILogger logger)
			: base(logger)
		{
		}

		protected override string ApplicationCode
		{
			get { return EDIInterchange.ApplicationCodes.SingaporeCMD; }
		}

		protected override string AttachmentFileName
		{
			get { return "CMD Message.txt"; }
		}

		protected override bool IsGoingViaCCN => true;
	}
}

using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using static Enterprise.Customs.TR.ETrade.Business.ETradeMessageHelper;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class ETradeQueryRemainingBillsforImpDecMessageProvider : IETradeQueryRemainingBillsforImpDec
	{
		public ETradeQueryRemainingBillsforImpDecMessageProvider(AsycudaManifestHeader header)
		{
			Header = Argument.NotNull(header, nameof(header));
		}
		protected readonly AsycudaManifestHeader Header;

		BusinessObject IMessageSender.Parent => Header;
		IBusinessObjectCollection IMessageSender.Messages => Header.Messages;

		public ZString JobReference => "ULU-" + Header.AMA_JobReference;

		public ZString RegistrationNo => Header.RegistrationNumber;
		ZString IETradeQueryRemainingBillsforImpDec.UserName => LastCredentials.Username;
		ZString IETradeQueryRemainingBillsforImpDec.UserPassword => LastCredentials.Password;

		Credentials LastCredentials => lastCredentials ?? (lastCredentials = GetLastCredentials(Header));
		Credentials lastCredentials;
	}
}

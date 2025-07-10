using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using static Enterprise.Customs.TR.ETrade.Business.ETradeMessageHelper;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class ETradeQueryForRegNoMessageProvider : IETradeQueryForRegNo
	{
		public ETradeQueryForRegNoMessageProvider(AsycudaManifestHeader header)
		{
			Header = Argument.NotNull(header, nameof(header));
		}
		protected readonly AsycudaManifestHeader Header;

		BusinessObject IMessageSender.Parent => Header;
		IBusinessObjectCollection IMessageSender.Messages => Header.Messages;
		ZString IMessageSender.JobReference => Header.AMA_JobReference;
		ZString IETradeQueryForRegNo.TemporaryRegistrationNo => Header.TempRegNo;
		ZString IETradeQueryForRegNo.UserName => LastCredentials.Username;
		ZString IETradeQueryForRegNo.UserPassword => LastCredentials.Password;

		Credentials LastCredentials => lastCredentials ?? (lastCredentials = GetLastCredentials(Header));
		Credentials lastCredentials;
	}
}

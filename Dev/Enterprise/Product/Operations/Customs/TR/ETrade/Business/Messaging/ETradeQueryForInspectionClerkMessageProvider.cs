using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using static Enterprise.Customs.TR.ETrade.Business.ETradeMessageHelper;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class ETradeQueryForInspectionClerkMessageProvider : IETradeQueryForInspectionClerk
	{
		public ETradeQueryForInspectionClerkMessageProvider(AsycudaManifestHeader header)
		{
			Header = Argument.NotNull(header, nameof(header));
		}
		protected readonly AsycudaManifestHeader Header;

		BusinessObject IMessageSender.Parent => Header;
		IBusinessObjectCollection IMessageSender.Messages => Header.Messages;
		ZString IMessageSender.JobReference => Header.AMA_JobReference;
		ZString IETradeQueryForInspectionClerk.RegistrationNo => Header.RegistrationNumber;
		ZString IETradeQueryForInspectionClerk.UserName => LastCredentials.Username;
		ZString IETradeQueryForInspectionClerk.UserPassword => LastCredentials.Password;

		Credentials LastCredentials => lastCredentials ?? (lastCredentials = GetLastCredentials(Header));
		Credentials lastCredentials;
	}
}

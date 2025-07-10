using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class ETradeSendForRegistrationNoMessageProvider : IETradeSendForRegistrationNo
	{
		public ETradeSendForRegistrationNoMessageProvider(AsycudaManifestHeader header)
		{
			Header = Argument.NotNull(header, nameof(header));
		}
		protected readonly AsycudaManifestHeader Header;

		BusinessObject IMessageSender.Parent => Header;
		IBusinessObjectCollection IMessageSender.Messages => Header.Messages;
		ZString IMessageSender.JobReference => "ULU-" + Header.AMA_JobReference;

		public ZString DeclarantNameAndTitle => GlbCompany.CurrentCompany.GC_Name;

		public ZString DeclarantIDTaxNo => GlbCompany.CurrentCompany.GC_BusinessRegNo;

		public ZString CustomsOffice => TRMessageHelper.RemoveCountryCodePrefix(Header.AMA_CustomsOffice);

		public ZString TemporaryRegistrationNo => Header.TempRegNo;
	}
}

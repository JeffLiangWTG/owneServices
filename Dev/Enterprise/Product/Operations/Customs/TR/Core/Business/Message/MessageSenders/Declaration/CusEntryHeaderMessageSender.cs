using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business
{
	public class CusEntryHeaderMessageSender : IMessageSender
	{
		public CusEntryHeaderMessageSender(CusEntryHeader cusEntryHeader)
		{
			CusEntryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
		}

		CusEntryHeader CusEntryHeader { get; }

		public BusinessObject Parent => CusEntryHeader;

		public IBusinessObjectCollection Messages => CusEntryHeader.Messages;

		public ZString JobReference => TRMessageConstants.ReferencePrefix + CusEntryHeader.Declaration.JE_DeclarationReference;
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business
{
	public class DeclarationAutoReceiveResponseMessageProvider : IMessageSender
	{
		readonly CusEntryHeader cusEntryHeader;

		public DeclarationAutoReceiveResponseMessageProvider(CusEntryHeader cusEntryHeader)
		{
			this.cusEntryHeader = cusEntryHeader;
		}

		public BusinessObject Parent => cusEntryHeader;

		public IBusinessObjectCollection Messages => cusEntryHeader.Messages;

		public ZString JobReference => TRMessageConstants.ReferencePrefix + cusEntryHeader.Declaration.JE_DeclarationReference;
	}
}

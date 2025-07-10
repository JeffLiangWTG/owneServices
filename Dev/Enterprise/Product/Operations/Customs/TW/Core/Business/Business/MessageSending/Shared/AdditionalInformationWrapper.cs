using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class AdditionalInformationWrapper : IAdditionalInformation
	{
		public AdditionalInformationWrapper(ZInt copyQuantity)
		{
			this.copyQuantity = copyQuantity;
		}

		public AdditionalInformationWrapper(ZString packingHouse)
		{
			this.packingHouse = packingHouse;
		}

		public AdditionalInformationWrapper(ZString statementCode, ZString statementDescription) : this(statementCode, statementDescription, ZString.Empty, ZString.Empty)
		{
		}

		public AdditionalInformationWrapper(ZString statementCode, ZString statementDescription, ZString approvalID, ZString content)
		{
			this.statementCode = statementCode;
			this.statementDescription = statementDescription;
			this.approvalID = approvalID;
			this.content = content;
		}

		readonly ZInt copyQuantity;
		readonly ZString statementCode;
		readonly ZString statementDescription;
		readonly ZString packingHouse;
		readonly ZString approvalID;
		readonly ZString content;

		ZInt IAdditionalInformation.CopyQuantity => copyQuantity;

		ZString IAdditionalInformation.StatementCode => statementCode;

		ZString IAdditionalInformation.StatementDescription => statementDescription;

		ZString IAdditionalInformation.PackingHouse => packingHouse;

		ZString IAdditionalInformation.ProcessNumber => null;

		ZString IAdditionalInformation.Content => content;

		ZString IAdditionalInformation.ApprovalID => approvalID;

		ZString IAdditionalInformation.DelProcessNumber => null;
	}
}

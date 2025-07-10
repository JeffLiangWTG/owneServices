using CargoWise.Types;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.ZA.Business.MessageBuilders.CUSCAR;
using Enterprise.Customs.ZA.Manifest.Business.EDIFACT;

namespace Enterprise.Customs.ZA.Manifest.Business.MessagingProcess
{
	sealed class CustomsMessenger : ICustomsMessenger
	{
		public static CustomsMessenger New(IEDIMessageCollectionOwner owner, ICusCarHeader builderCusCarHeader, CusCarHeader cuscarHeader, string messageSubType, ZString billIssuerCode)
		{
			return new CustomsMessenger(owner, CreateMessageGenerator(builderCusCarHeader, messageSubType, billIssuerCode), cuscarHeader);
		}

		static ICustomsMessageGenerator CreateMessageGenerator(ICusCarHeader cuscarHeader, string messageSubType, ZString billIssuerCode)
		{
			return new CUSCARMessageBuilder(cuscarHeader, messageSubType, billIssuerCode);
		}

		CustomsMessenger(IEDIMessageCollectionOwner owner, ICustomsMessageGenerator messageGenerator, CusCarHeader cuscarHeader)
		{
			this.owner = owner;
			this.messageGenerator = messageGenerator;
			this.cuscarHeader = cuscarHeader;
		}
		readonly IEDIMessageCollectionOwner owner;
		readonly ICustomsMessageGenerator messageGenerator;
		readonly CusCarHeader cuscarHeader;

		IEDIMessageCollectionOwner ICustomsMessenger.Owner => owner;

		ICustomsMessageGenerator ICustomsMessenger.MessageGenerator => messageGenerator;

		bool ICustomsMessenger.ProcessUpdates(ActionResult previousResult)
		{
			if (previousResult.Success)
			{
				cuscarHeader.SetNewCountryMessagingStatus(ZAMessageStatusList.Codes.AwaitingResponse);

				if (owner.MessageOwner is AsycudaBill bill)
				{
					bill.ABL_MessageStatus = ZAMessageStatusList.Codes.AwaitingResponse;
				}
			}

			return true;
		}

		bool ICustomsMessenger.ShouldCreateMessage(ActionResult previousResult) => true;
	}
}

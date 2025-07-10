using CargoWise.Common.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public static class ManufacturerIdentifierQueryBuilder
	{
		public static MQEDIMessage Generate(USMIDQuery messageData)
		{
			var block = (BlockControlGenerator)new ACEInputBlockControlGenerator(GlbBranch.CurrentBranch);
			var applicationIdentifier = ACEApplicationIdentifierCodeList.Codes.ManufacturerNameAndAddressQuery;
			block.B.ApplicationIdentifier = applicationIdentifier;
			var qmfDollar = new QMFDollar();
			qmfDollar.ManufacturerIDCode = messageData.US_MID;
			block.AddMessageBlock(qmfDollar);

			var message = block.CreateMessage<MQEDIMessage>(messageData.Factory);
			message.EM_ApplicationReference = messageData.US_AutoCreateOrganization ? YesNoDefaultList.Codes.Yes : YesNoDefaultList.Codes.No;
			return message;
		}

		public static void GenerateMessageAndAttachToOrganisation(OrgHeader organisation, ZString mID)
		{
			var messageData = new USMIDQuery(organisation.Factory);
			messageData.US_MID = mID;
			var wrapper = OrgHeaderWrapper.New(organisation);
			wrapper.Messages.Add(Generate(messageData));
		}
	}
}

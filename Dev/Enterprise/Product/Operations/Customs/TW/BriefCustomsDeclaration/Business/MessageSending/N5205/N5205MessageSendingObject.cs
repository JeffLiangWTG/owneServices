using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class N5205MessageSendingObject : MessageSendingObject, IN5205Declaration
	{
		public N5205MessageSendingObject(AsycudaManifestHeader header) : base(header)
		{
		}

		public override ZString MessageType => MessageTypeCodeList.Descriptions.EBC;

		public override ZString Description => (NoResString)"出口快遞貨物簡易申報單";

		IPartyDetails IN5205Declaration.Exporter => new N5205DeclarationExporter(MasterBill);

		IEnumerable<IBCDGoodsShipment> IN5205Declaration.GoodsShipments => Header.HouseBills.OrderBy(b => b.ABL_SequenceNumber).Select(bill => new BCDGoodsShipment(bill, this));

		protected override IPartyDetails GetAgentCore() => new PartyDetailsWrapper(Header.AMA_RecipientReference, MessageConstants.RoleCodeCB, Header.AMA_CustomsProfile.Right(1), null);

		protected override ITWMessageBuilder GetMessageBuilder()
		{
			return new N5205MessageBuilder();
		}
	}
}

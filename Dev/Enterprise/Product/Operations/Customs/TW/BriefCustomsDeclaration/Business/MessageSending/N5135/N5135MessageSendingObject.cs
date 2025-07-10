using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class N5135MessageSendingObject : MessageSendingObject,
		IN5135Declaration,
		IDutyTaxFee
	{
		public N5135MessageSendingObject(AsycudaManifestHeader header) : base(header)
		{
		}

		public override ZString MessageType => MessageTypeCodeList.Descriptions.IBC;

		public override ZString Description => (NoResString)"進口快遞貨物簡易申報單";

		protected override IPartyDetails GetAgentCore() => new PartyDetailsWrapper(Header.AMA_RecipientReference, MessageConstants.RoleCodeCB, Header.AMA_CustomsProfile.Right(1), null);

		IDutyTaxFee IN5135Declaration.DutyTaxFee => this;

		IEnumerable<IN5135GoodsShipment> IN5135Declaration.GoodsShipments => Header.HouseBills.OrderBy(b => b.ABL_SequenceNumber).Select(bill => new N5135GoodsShipment(bill, this));

		IPartyDetails IN5135Declaration.Importer => new BCDConsignee(Header.MasterBill);

		#region IDutyTaxFee members
		ZString IDutyTaxFee.DutyExemptionWaiverNote => ZString.Empty;

		ZString IDutyTaxFee.DutyMemoPrinted => ZString.Empty;

		ZString IDutyTaxFee.DutyMethodCode => Header.AMA_PaymentMethod;

		ZDecimal IDutyTaxFee.TotalDutyTaxFeeAmount => ZDecimal.Zero;

		ZString IDutyTaxFee.PaymentObligationGuaranteeReferenceID => Header.AMA_PaymentAccountNumber;

		ZDecimal IDutyTaxFee.TotalCashDutyTaxFeeAmount => ZDecimal.Zero;

		ZDecimal IDutyTaxFee.TotalNonCashDutyTaxFeeAmount => ZDecimal.Zero;
		#endregion

		protected override ITWMessageBuilder GetMessageBuilder()
		{
			return new N5135MessageBuilder();
		}
	}
}

using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class PTTMessageBuilder : FZMessageBuilder
	{
		public PTTMessageBuilder(IFZEventHeader header, PTTSendingOption sendingOption)
			: base(header)
		{
			this.sendingOption = sendingOption;
		}
		readonly PTTSendingOption sendingOption;

		protected override IEnumerable<MessageBlock> Build()
		{
			var admissionNumber = Header.FTZAdmissionNumber;
			foreach (IFZEventBill bill in Header.Bills)
			{
				foreach (var block in GenerateFZ10AndRelatedBlocks(bill, admissionNumber, () => GenerateFZ10(bill.BillOfLading, bill.Quantity, Header.DeliveryCode, bill.IRSIdentifier, bill.FIRMSCode), () => GenerateFZ12(bill.PIDUniqueIdentifier), bill.Remarks))
				{
					yield return block;
				}
			}
		}

		protected override FTZFZ10 GenerateFZ10ForSplitDetails(Func<FTZFZ10> generateFZ10, IConveyanceOrSplitDetails splitDetails)
		{
			var fz10Block = generateFZ10();
			fz10Block.ReceivedQuantity = ShouldSendQuantity ? (ZDecimal)splitDetails.Qty : ZDecimal.Zero;
			return fz10Block;
		}

		FTZFZ10 GenerateFZ10(ZString number, ZDecimal quantity, ZString deliveryCode, ZString iRSIdentifier, ZString firmsCode)
		{
			var actionCode = GetActionCode(sendingOption);
			var fz10 = GenerateFZ10WithMandatoryFields(2, number, actionCode);
			fz10.ReceivedQuantity = ShouldSendQuantity ? quantity : ZDecimal.Zero;
			fz10.DeliveryCode = deliveryCode;
			fz10.IRSIdentifierBondedCarrier = iRSIdentifier;
			fz10.FIRMS = firmsCode;
			return fz10;
		}

		bool ShouldSendQuantity => sendingOption == PTTSendingOption.SendPTTMessage;

		FTZFZ12 GenerateFZ12(ZString pidUniqueIdentifier)
		{
			FTZFZ12 result = null;

			if (sendingOption != PTTSendingOption.SendPTTMessage && !pidUniqueIdentifier.IsEmpty)
			{
				result = new FTZFZ12();
				result.ReferenceIdentifierQualifier = FTZPermitToTransferReferenceIdentifierList.Codes.PTTUniqueIdentifier;
				result.ReferenceIdentifier = pidUniqueIdentifier;
			}

			return result;
		}

		ZString GetActionCode(PTTSendingOption option)
		{
			var result = ZString.Empty;
			switch (option)
			{
				case PTTSendingOption.SendPTTMessage:
					result = FTZActionCodeList.Codes.F;
					break;
				case PTTSendingOption.CancellPTTMessage:
					result = FTZActionCodeList.Codes.K;
					break;
				case PTTSendingOption.SendPTTArrival:
					result = FTZActionCodeList.Codes.L;
					break;
				case PTTSendingOption.SendPTTUnArrival:
					result = FTZActionCodeList.Codes.M;
					break;
				default:
					break;
			}
			return result;
		}
	}
}

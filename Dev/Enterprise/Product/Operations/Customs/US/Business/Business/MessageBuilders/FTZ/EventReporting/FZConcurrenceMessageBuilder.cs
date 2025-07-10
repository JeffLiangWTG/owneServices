using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class FZConcurrenceMessageBuilder : FZMessageBuilder
	{
		public FZConcurrenceMessageBuilder(FZEventAction action)
				: base(action.EventHeader)
		{
			this.action = action;
		}
		public readonly FZEventAction action;

		protected override IEnumerable<MessageBlock> Build()
		{
			var actionQualifier = GetActionQualifier();
			var itemSendList = action.GetMessageSendingObjectsNeedSending();
			switch (actionQualifier)
			{
				case 1:
					yield return GenerateFZ10(actionQualifier, action.MessageSendingObjectsView[0].MB_ConcurrenceQty);
					foreach (var fz20 in Header.Bills.Cast<IFZEventBill>().SelectMany(bill => GenerateFZ20s(bill.ConcurRemarks)))
					{
						yield return fz20;
					}
					break;
				case 2:
					var admissionNumber = Header.FTZAdmissionNumber;
					foreach (IFZEventBill bill in Header.Bills)
					{
						var itemSend = itemSendList.FirstOrDefault(a => a.MB_Identifier == bill.CU_BillNum);
						if (itemSend != null)
						{
							var concurrenceQty = ((Bill)bill).IsMasterBill ? itemSend.MB_ConcurrenceQty : bill.Quantity;
							var fz10 = GenerateFZ10(bill, actionQualifier, concurrenceQty);
							foreach (var block in GenerateFZ10AndRelatedBlocks(bill, admissionNumber, () => fz10, null, bill.ConcurRemarks))
							{
								yield return block;
							}
						}
					}
					break;
				case 3:
					foreach (IFZEventBill bill in Header.LowestBills)
					{
						foreach (var itNo in bill.ITNumbers)
						{
							var itemSend = itemSendList.FirstOrDefault(a => a.MB_Identifier == itNo.ITNumber);
							if (itemSend != null)
							{
								yield return GenerateFZ10(itNo, bill.FIRMSCode, actionQualifier, itemSend.MB_ConcurrenceQty);
							}
						}
						foreach (var fz20 in GenerateFZ20s(bill.ConcurRemarks))
						{
							yield return fz20;
						}
					}
					break;
			}
		}

		FTZFZ10 GenerateFZ10(ZInt actionQualifier, ZDecimal concurrenceQty)
		{
			var fz10 = GenerateFZ10WithMandatoryFields(actionQualifier, Header.FTZAdmissionNumber, action.US_ActionCode);
			fz10.ReceivedQuantity = Header.Quantity;
			fz10.DeliveryCode = Header.DeliveryCode;
			fz10.ReceivedQuantity = concurrenceQty;
			return fz10;
		}

		//Action Qualifier "2", Action Code "B"
		FTZFZ10 GenerateFZ10(IFZEventBill bill, ZInt actionQualifier, ZDecimal concurrenceQty)
		{
			var fz10 = GenerateFZ10WithMandatoryFields(actionQualifier, bill.BillOfLading, action.US_ActionCode);
			fz10.ReceivedQuantity = bill.Quantity;
			fz10.DeliveryCode = action.EventHeader.DeliveryCode;
			fz10.FIRMS = bill.FIRMSCode;
			fz10.ReceivedQuantity = concurrenceQty;
			return fz10;
		}

		//Action Qualifier "3", Action Code "C"
		FTZFZ10 GenerateFZ10(IITNumber itNo, ZString fIRMSCode, ZInt actionQualifier, ZDecimal concurrenceQty)
		{
			var fz10 = GenerateFZ10WithMandatoryFields(actionQualifier, itNo.ITNumber, action.US_ActionCode);
			fz10.ReceivedQuantity = new ZDecimal(itNo.PackageQuantity);
			fz10.DeliveryCode = action.EventHeader.DeliveryCode;
			fz10.FIRMS = fIRMSCode;
			fz10.ReceivedQuantity = concurrenceQty;
			return fz10;
		}

		ZInt GetActionQualifier()
		{
			switch (action.US_ActionCode)
			{
				case FTZActionCodeList.Codes.A:
					return 1;
				case FTZActionCodeList.Codes.B:
					return 2;
				case FTZActionCodeList.Codes.C:
					return 3;
				default:
					return ZInt.Zero;
			}
		}
	}
}

using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class FZArrivalMessageBuilder : FZMessageBuilder
	{
		public FZArrivalMessageBuilder(IFZEventHeader header)
			: base(header)
		{
		}

		protected override IEnumerable<MessageBlock> Build()
		{
			var admissionNumber = Header.FTZAdmissionNumber;
			foreach (IFZEventBill bill in Header.Bills)
			{
				var fz10 = GenerateFZ10(bill.BillOfLading, bill.FIRMSCode);
				foreach (var block in GenerateFZ10AndRelatedBlocks(bill, admissionNumber, () => fz10, null, ZString.Empty))
				{
					yield return block;
				}
			}
		}

		FTZFZ10 GenerateFZ10(ZString number, ZString fIRMSCode)
		{
			var fz10 = GenerateFZ10WithMandatoryFields(2, number, FTZActionCodeList.Codes.I);
			fz10.FIRMS = fIRMSCode;
			return fz10;
		}
	}
}

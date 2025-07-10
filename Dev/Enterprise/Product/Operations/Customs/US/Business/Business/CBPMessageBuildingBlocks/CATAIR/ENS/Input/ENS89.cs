using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("89")]
	public partial class ENS89 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.ENS89, IBIRDHeaderRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			if (entry != null)
			{
				UpdateCharge(entry, ClassCode, TotalAmount);
				UpdateCharge(entry, ClassCode1, TotalAmount1);
				UpdateCharge(entry, ClassCode2, TotalAmount2);
				UpdateCharge(entry, ClassCode3, TotalAmount3);
				UpdateCharge(entry, ClassCode4, TotalAmount4);
			}
		}

		void UpdateCharge(CusEntryHeader entry, ZString chargeType, ZDecimal chargeAmount)
		{
			if (!chargeType.IsEmpty)
			{
				entry.Charges.UpdateOrAddCharge(chargeType, chargeAmount);
			}
		}

		#endregion
	}
}

using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class ENS34 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.ENS34, IBIRDHeaderRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			UpdateCharge(entry, ClassCode, Amount);

			UpdateCharge(entry, ClassCode1, Amount1);

			UpdateCharge(entry, ClassCode2, Amount2);

			UpdateCharge(entry, ClassCode3, Amount3);

			UpdateCharge(entry, ClassCode4, Amount4);

			UpdateCharge(entry, ClassCode5, Amount5);
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

using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	public partial class AENS34 : Abstract.AENS34, IBIRDHeaderRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			if (entry != null)
			{
				UpdateCharge(entry, AccountingClassCode1, HeaderFeeAmount1);
				UpdateCharge(entry, AccountingClassCode2, HeaderFeeAmount2);
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

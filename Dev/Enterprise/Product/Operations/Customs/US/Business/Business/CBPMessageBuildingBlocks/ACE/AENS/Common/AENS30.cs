using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	public partial class AENS30 : Abstract.AENS30, IBIRDHeaderRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			declaration.US_WHSDistrictPortCode = AssociatedWarehouseEntryDistrictPortCode;
			declaration.US_WHSEntryNumber = AssociatedWarehouseEntryNumber;
			declaration.US_WHSEntryFilerCode = AssociatedWarehouseEntryFilerCode;
			declaration.US_IsFinalWHS = FinalWarehouseWithdrawalIndicator == "Y";
		}

		#endregion
	}
}

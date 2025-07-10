using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class BCR0M : Messaging.Business.MessageBuildingBlocks.Input.Abstract.BCR0M, IBIRDHeaderRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			declaration.Factory.GetCachedValue<BIRDBillUpdateTool>().UpdateBill(declaration,
				MasterBillNumber,
				HouseBillNumber,
				SubHouseBillNumber,
				Quantity,
				Unit,
				ZDate.Empty,
				ZString.Empty,
				IssuerOfMasterBillNumber,
				IssuerCodeOfHouseBillNumber);
		}

		#endregion
	}
}

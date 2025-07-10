using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class ENS22 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.ENS22, IBIRDHeaderRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, CargoWise.ComponentModel.INotifications notifications)
		{
			new BIRDBillUpdateTool().UpdateBill(
				declaration,
				MasterBillNumber,
				HouseBillNumber,
				SubHouseBillNumber,
				Quantity,
				Unit,
				ITDate,
				InBondNumber,
				IssuerCodeOfMasterBillNumber,
				IssuerCodeOfHouseBillNumber);
		}

		#endregion
	}
}

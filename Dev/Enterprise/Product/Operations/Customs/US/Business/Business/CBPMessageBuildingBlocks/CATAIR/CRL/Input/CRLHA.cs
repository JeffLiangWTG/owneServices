using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class CRLHA : Messaging.Business.MessageBuildingBlocks.Input.Abstract.CRLHA, IBIRDHeaderRecord
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
				ImmediateTransportationITDate,
				InbondNumber,
				IssuerCodeOfMasterBillNumber,
				IssuerCodeOfHouseBillNumber);
		}

		#endregion
	}
}

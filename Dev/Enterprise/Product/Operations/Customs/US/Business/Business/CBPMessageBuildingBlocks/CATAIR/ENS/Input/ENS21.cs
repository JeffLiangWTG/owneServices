using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class ENS21 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.ENS21, IBIRDHeaderRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, CargoWise.ComponentModel.INotifications notifications)
		{
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MAN;
			declaration.US_BondAmount = BondAmount;
			declaration.US_BondProducerAccNo = BondProducerAccountNumber;
		}

		#endregion
	}
}

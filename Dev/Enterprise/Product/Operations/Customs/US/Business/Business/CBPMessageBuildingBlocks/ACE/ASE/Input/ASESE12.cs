using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	public partial class ASESE12 : Abstract.ASESE12, IBIRDHeaderRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			if (BondDesignationTypeCode == "A")
			{
				declaration.US_BondType2 = BondTypeCode;
				declaration.US_ADDCVDSuretyCode = SuretyCompanyCode;
				declaration.US_BondAmount2 = SingleTransactionBondAmount;
				declaration.US_BondProducerAccNo2 = SingleTransactionBondProducerAccountNumber;
			}
		}

		#endregion
	}
}

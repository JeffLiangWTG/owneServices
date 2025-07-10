using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	public partial class AENS31 : Abstract.AENS31, IBIRDHeaderRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			if (BondDesignationTypeCode != "A")
			{
				declaration.US_BondType = BondTypeCode;
				declaration.US_BondDesignationCode = BondDesignationTypeCode;
				declaration.US_BondSuperseding = ContinuousBondIndicator == "Y";
				declaration.US_SuretyCode = SuretyCompanyCode;
				declaration.US_BondAmount = SingleTransactionBondAmount;
				declaration.US_BondProducerAccNo = SingleTransactionBondProducerAccountNumber;
			}
			else
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

using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Testing
{
	sealed class ASESE12Test : TestCaseWithFactory
	{
		public void TestUpdateDeclaration()
		{
			var additionalBondRecord = new ASESE12()
			{
				BondTypeCode = "8",
				BondDesignationTypeCode = "A",
				SuretyCompanyCode = "233",
				SingleTransactionBondAmount = 200m,
				SingleTransactionBondProducerAccountNumber = "23456"
			};

			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationBuffer();
			((IBIRDHeaderRecord)additionalBondRecord).Update(declaration, notifications);
			AssertEquals("8", declaration.US_BondType2);
			AssertEquals("233", declaration.US_ADDCVDSuretyCode);
			AssertEquals(200m, declaration.US_BondAmount2);
			AssertEquals("23456", declaration.US_BondProducerAccNo2);
		}
	}
}

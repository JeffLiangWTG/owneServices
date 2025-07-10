using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class AENS31Test : TestCaseWithFactory
	{
		public void TestUpdateDeclaration()
		{
			var mainBondRecord = new AENS31()
			{
				BondTypeCode = "9",
				BondDesignationTypeCode = "B",
				ContinuousBondIndicator = "Y",
				SuretyCompanyCode = "891",
				SingleTransactionBondAmount = 100m,
				SingleTransactionBondProducerAccountNumber = "123456"
			};

			var additionalBondRecord = new AENS31()
			{
				BondTypeCode = "8",
				BondDesignationTypeCode = "A",
				ContinuousBondIndicator = "Y",
				SuretyCompanyCode = "233",
				SingleTransactionBondAmount = 200m,
				SingleTransactionBondProducerAccountNumber = "23456"
			};

			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationBuffer();
			((IBIRDHeaderRecord)mainBondRecord).Update(declaration, notifications);
			AssertEquals("9", declaration.US_BondType);
			AssertEquals("B", declaration.US_BondDesignationCode);
			AssertEquals(true, declaration.US_BondSuperseding);
			AssertEquals("891", declaration.US_SuretyCode);
			AssertEquals(100m, declaration.US_BondAmount);
			AssertEquals("123456", declaration.US_BondProducerAccNo);

			((IBIRDHeaderRecord)additionalBondRecord).Update(declaration, notifications);
			AssertEquals("8", declaration.US_BondType2);
			AssertEquals("233", declaration.US_ADDCVDSuretyCode);
			AssertEquals(200m, declaration.US_BondAmount2);
			AssertEquals("23456", declaration.US_BondProducerAccNo2);
		}
	}
}

using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class BondTypeListTest : NUnit.Framework.TestCase
	{
		public void TestIsRelevantForSTB()
		{
			Assert(BondTypeList.IsRelevantFor(BondTypeList.Codes.SingleTransactionBond, USAddInfoSchema.Constants.US_BondAmount));
			Assert(BondTypeList.IsRelevantFor(BondTypeList.Codes.SingleTransactionBond, USAddInfoSchema.Constants.US_BondAmount2));
			Assert(BondTypeList.IsRelevantFor(BondTypeList.Codes.SingleTransactionBond, USAddInfoSchema.Constants.US_BondProducerAccNo));
			Assert(BondTypeList.IsRelevantFor(BondTypeList.Codes.SingleTransactionBond, USAddInfoSchema.Constants.US_BondProducerAccNo2));
			Assert(BondTypeList.IsRelevantFor(BondTypeList.Codes.SingleTransactionBond, USAddInfoSchema.Constants.US_BondDispositionCode));
			Assert(!BondTypeList.IsRelevantFor(BondTypeList.Codes.SingleTransactionBond, USAddInfoSchema.Constants.US_BondSuperseding));
			Assert(BondTypeList.IsRelevantFor(BondTypeList.Codes.SingleTransactionBond, USAddInfoSchema.Constants.US_SuretyCode));
			Assert(BondTypeList.IsRelevantFor(BondTypeList.Codes.SingleTransactionBond, USAddInfoSchema.Constants.US_ADDCVDSuretyCode));
		}

		public void TestIsRelevantForContinuous()
		{
			Assert(!BondTypeList.IsRelevantFor(BondTypeList.Codes.ContinuousBond, USAddInfoSchema.Constants.US_BondAmount));
			Assert(!BondTypeList.IsRelevantFor(BondTypeList.Codes.ContinuousBond, USAddInfoSchema.Constants.US_BondAmount2));
			Assert(BondTypeList.IsRelevantFor(BondTypeList.Codes.ContinuousBond, USAddInfoSchema.Constants.US_BondProducerAccNo));
			Assert(BondTypeList.IsRelevantFor(BondTypeList.Codes.ContinuousBond, USAddInfoSchema.Constants.US_BondProducerAccNo2));
			Assert(!BondTypeList.IsRelevantFor(BondTypeList.Codes.ContinuousBond, USAddInfoSchema.Constants.US_BondDispositionCode));
			Assert(BondTypeList.IsRelevantFor(BondTypeList.Codes.ContinuousBond, USAddInfoSchema.Constants.US_BondSuperseding));
			Assert(BondTypeList.IsRelevantFor(BondTypeList.Codes.ContinuousBond, USAddInfoSchema.Constants.US_SuretyCode));
			Assert(BondTypeList.IsRelevantFor(BondTypeList.Codes.ContinuousBond, USAddInfoSchema.Constants.US_ADDCVDSuretyCode));
		}
	}
}

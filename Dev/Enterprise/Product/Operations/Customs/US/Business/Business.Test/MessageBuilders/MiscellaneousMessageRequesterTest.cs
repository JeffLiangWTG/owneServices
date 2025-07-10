using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class MiscellaneousMessageRequesterTest : TestCaseWithFactory
	{
		public void TestRequestStatement()
		{
			var qrBlock = new DSTQR
			{
				TransmissionDateOfStatementOrACHPaymentTransaction = ZDate.Today,
				PreliminaryStatementRequest = "Y",
				FinalStatementRequest = "Y",
				ACHPaymentRequest = "Y",
				PeriodicStatementPaymentAuthorizationRequest = "Y"
			};
			var message = requester.RequestStatement(false, ApplicationIdentifierCodeList.Codes.ABIStatementACHPaymentReroute, qrBlock, "9999");
			AssertNotNull(message);
			Assert(message.EM_MessageText.Contains("B019999"));
			Assert(message.EM_MessageText.Contains("Y  9999"));
		}

		public void TestRequestStatementACE()
		{
			var qrBlock = new DSTQR
			{
				TransmissionDateOfStatementOrACHPaymentTransaction = ZDate.Today,
				PreliminaryStatementRequest = "Y",
				FinalStatementRequest = "Y",
				ACHPaymentRequest = "Y",
				PeriodicStatementPaymentAuthorizationRequest = "Y"
			};
			var result = requester.RequestStatement(true, ApplicationIdentifierCodeList.Codes.ABIStatementACHPaymentReroute, qrBlock, "9999");
			Assert(result.EM_MessageText.StartsWith("B  9999"));
			Assert(result.EM_MessageText.Contains("Y  9999"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			requester = new MiscellaneousMessageRequester();
		}
		MiscellaneousMessageRequester requester;
	}
}

using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class ValidationHelperTest : TestCaseWithFactory
	{
		public void TestAssertFlightNumerFormat()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = Enterprise.Customs.US.Business.InBondTransportModeCodes.Codes.AirNonContainer;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_SplitFlightNo = "Z!*!";
			AssertHasMessageErrorContaining(moveHeader.BM_SplitFlightNoInfo, ValidationConstants.Header.InvalidFlightNumber.ToString());
		}

		public void TestAssertCarrierIDFormat()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = Enterprise.Customs.US.Business.InBondTransportModeCodes.Codes.AirNonContainer;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_InBondCarrierID = "22-3333355";
			AssertHasMessageErrorContaining(moveHeader.BM_InBondCarrierIDInfo, ValidationConstants.MoveHeader.InBondCarrierIDValid.ToString());
		}
	}
}

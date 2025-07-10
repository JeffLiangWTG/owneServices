namespace Enterprise.Customs.NZ.Business.Testing
{
	using CargoWise.EntityFramework.Testing;
	class TranshipmentRequestModeOfMovementTest : TestCaseWithFactory
	{
		public void TestIsSea()
		{
			Assert(!TranshipmentRequestModeOfMovement.IsSea(TranshipmentRequestModeOfMovement.Codes.Air));
			Assert(!TranshipmentRequestModeOfMovement.IsSea(TranshipmentRequestModeOfMovement.Codes.Road));
			Assert(TranshipmentRequestModeOfMovement.IsSea(TranshipmentRequestModeOfMovement.Codes.Sea));
			Assert(TranshipmentRequestModeOfMovement.IsSea(TranshipmentRequestModeOfMovement.Codes.SeaCV));
			Assert(TranshipmentRequestModeOfMovement.IsSea(TranshipmentRequestModeOfMovement.Codes.SeaOV));
			Assert(!TranshipmentRequestModeOfMovement.IsSea(TranshipmentRequestModeOfMovement.Codes.Air));
			Assert(!TranshipmentRequestModeOfMovement.IsSea(TranshipmentRequestModeOfMovement.Codes.Road));
		}

		public void TestIsAir()
		{
			Assert(TranshipmentRequestModeOfMovement.IsAir(TranshipmentRequestModeOfMovement.Codes.Air));
			Assert(!TranshipmentRequestModeOfMovement.IsAir(TranshipmentRequestModeOfMovement.Codes.Road));
			Assert(!TranshipmentRequestModeOfMovement.IsAir(TranshipmentRequestModeOfMovement.Codes.Sea));
			Assert(!TranshipmentRequestModeOfMovement.IsAir(TranshipmentRequestModeOfMovement.Codes.SeaCV));
			Assert(!TranshipmentRequestModeOfMovement.IsAir(TranshipmentRequestModeOfMovement.Codes.SeaOV));
			Assert(TranshipmentRequestModeOfMovement.IsAir(TranshipmentRequestModeOfMovement.Codes.Air));
			Assert(!TranshipmentRequestModeOfMovement.IsAir(TranshipmentRequestModeOfMovement.Codes.Road));
		}
	}
}

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FPCIncoTermTest : FOBIncoTermTest
	{
		protected override string IncotermToTest => TermsOfDeliveryList.Codes.FPC;
	}
}

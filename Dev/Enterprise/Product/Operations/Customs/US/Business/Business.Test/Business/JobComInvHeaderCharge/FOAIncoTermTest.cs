namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FOAIncoTermTest : FOBIncoTermTest
	{
		protected override string IncotermToTest => TermsOfDeliveryList.Codes.FOA;
	}
}

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FORIncoTermTest : FOBIncoTermTest
	{
		protected override string IncotermToTest => TermsOfDeliveryList.Codes.FOR;
	}
}

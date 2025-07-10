namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FOTIncoTermTest : FOBIncoTermTest
	{
		protected override string IncotermToTest => TermsOfDeliveryList.Codes.FOT;
	}
}

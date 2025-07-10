namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusContainerLookupsTest : Customs.Business.Testing.CusContainerLookupsTest
	{
		public void TestContainer()
		{
			var parent = Factory.New<CusContainer>();
			AssertEquals(parent.Lookups.Container, parent);
		}

		protected override Customs.Business.CusContainerLookups GetCusContainerLookups() => new CusContainerLookups(Factory.New<CusContainer>());
	}
}

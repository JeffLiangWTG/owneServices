namespace Enterprise.Customs.Business.Testing
{
	sealed class CusCodeDataSynchroniserTest : SynchroniserTestCase
	{
		public void TestCusCodeDataSynchroniser()
		{
			var source = Factory.New<DummyCusCodeData>();
			var destination = Factory.New<DummyCusCodeData>();
			var synchroniser = new CusCodeDataSynchroniser(destination, source);
			synchroniser.SetEnabled(true, false);
			source.CY_Code = "A";
			source.CY_Data = "B";
			AssertEquals("A", destination.CY_Code);
			AssertEquals("B", destination.CY_Data);
		}
	}
}

namespace Enterprise.Customs.NZ.Business.Testing
{
	sealed class RefCusPackListProviderTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestGetDeclarationPackTypeList()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "AE", "WB");
			Factory.Save();
			var provider = new RefCusPackListProvider();
			var list = provider.GetDeclarationPackTypeList(Factory);

			CombineAssertions(() =>
			{
				AssertEquals("Should include AE", true, list.ContainsCode("AE"));
				AssertEquals("Should include WB", true, list.ContainsCode("WB"));
			});
		}
	}
}

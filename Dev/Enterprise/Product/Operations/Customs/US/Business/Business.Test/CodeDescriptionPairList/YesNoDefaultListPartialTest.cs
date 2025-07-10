namespace Enterprise.Customs.US.Business.Testing
{
	sealed class YesNoDefaultListTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestGetCachedYesOnlyList()
		{
			var list = YesNoDefaultList.GetCachedYesOnlyList(Factory);
			Assert("should be cached", object.ReferenceEquals(list, YesNoDefaultList.GetCachedYesOnlyList(Factory)));
			AssertEquals(1, list.Count);
			AssertEquals(YesNoDefaultList.Descriptions.Yes, list.GetDescriptionFromCode(YesNoDefaultList.Codes.Yes));
		}

		public void TestGetCachedYesNoList()
		{
			var list = YesNoDefaultList.GetCachedYesNoList(Factory);
			Assert("should be cached", object.ReferenceEquals(list, YesNoDefaultList.GetCachedYesNoList(Factory)));
			AssertEquals(2, list.Count);
			AssertEquals(YesNoDefaultList.Descriptions.Yes, list.GetDescriptionFromCode(YesNoDefaultList.Codes.Yes));
			AssertEquals(YesNoDefaultList.Descriptions.No, list.GetDescriptionFromCode(YesNoDefaultList.Codes.No));
		}
	}
}

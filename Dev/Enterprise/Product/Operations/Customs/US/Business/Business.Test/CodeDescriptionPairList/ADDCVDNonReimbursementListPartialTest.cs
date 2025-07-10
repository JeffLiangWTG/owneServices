namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ADDCVDNonReimbursementListTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestGetCachedList()
		{
			var list = ADDCVDNonReimbursementList.GetCachedList(Factory);
			Assert("should be cached", object.ReferenceEquals(list, ADDCVDNonReimbursementList.GetCachedList(Factory)));
			AssertEquals(2, list.Count);
			AssertEquals(ADDCVDNonReimbursementList.Descriptions.Declared, list.GetDescriptionFromCode(ADDCVDNonReimbursementList.Codes.Declared));
			AssertEquals(ADDCVDNonReimbursementList.Descriptions.OnceOff, list.GetDescriptionFromCode(ADDCVDNonReimbursementList.Codes.OnceOff));
		}
	}
}

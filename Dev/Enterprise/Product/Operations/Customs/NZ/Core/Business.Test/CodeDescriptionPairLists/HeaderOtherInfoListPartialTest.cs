namespace Enterprise.Customs.NZ.Business.Testing
{
	using CargoWise.EntityFramework.Testing;

	public class HeaderOtherInfoListPartialTest : TestCaseWithFactory
	{
		public void TestIsReferenceDocument()
		{
			AssertEquals("Is Reference Document Code", true, HeaderOtherInfoList.IsReferenceDocument(HeaderOtherInfoList.Codes.Passport));
			AssertEquals("Is Reference Document Code", true, HeaderOtherInfoList.IsReferenceDocument(HeaderOtherInfoList.Codes.Certificate));
			AssertEquals("Is Reference Document Code - not this one", false, HeaderOtherInfoList.IsReferenceDocument(HeaderOtherInfoList.Codes.CitizenshipOfImporter1));
			AssertEquals("Is Reference Document Code", true, HeaderOtherInfoList.IsReferenceDocument(HeaderOtherInfoList.Codes.OtherDocument));
		}

		public void TestIsRequiredOtherInfoCode()
		{
			AssertEquals("IsRequiredOtherInfoCode - not this one", false, HeaderOtherInfoList.IsRequiredOtherInfoCode(HeaderOtherInfoList.Codes.Passport));
			AssertEquals("IsRequiredOtherInfoCode - not this one", false, HeaderOtherInfoList.IsRequiredOtherInfoCode(HeaderOtherInfoList.Codes.Certificate));
			AssertEquals("IsRequiredOtherInfoCode", true, HeaderOtherInfoList.IsRequiredOtherInfoCode(HeaderOtherInfoList.Codes.CitizenshipOfImporter1));
			AssertEquals("IsRequiredOtherInfoCode - not this one", false, HeaderOtherInfoList.IsRequiredOtherInfoCode(HeaderOtherInfoList.Codes.OtherDocument));
			AssertEquals("IsRequiredOtherInfoCode - not this one", false, HeaderOtherInfoList.IsRequiredOtherInfoCode(HeaderOtherInfoList.Codes.MAFContainerDeclaration));
			AssertEquals("IsRequiredOtherInfoCode - not this one", false, HeaderOtherInfoList.IsRequiredOtherInfoCode(HeaderOtherInfoList.Codes.ApprovedTransitionalFacility));
			AssertEquals("IsRequiredOtherInfoCode", true, HeaderOtherInfoList.IsRequiredOtherInfoCode(HeaderOtherInfoList.Codes.DeedOfCovenant));
			AssertEquals("IsRequiredOtherInfoCode", true, HeaderOtherInfoList.IsRequiredOtherInfoCode(HeaderOtherInfoList.Codes.ProhibitedOrRestrictedGoods));
		}
	}
}

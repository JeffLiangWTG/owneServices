using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgAddressTypeTest : TestCase
	{
		public void TestOrgAddressType()
		{
			AssertEquals("Receivables", OrgAddressType.Receivables, OrgAddressType.Find("ARM"));
			Assert("Miscellaneous", OrgAddressType.Miscellaneous == "MSC");
		}

		public void TestImplicitOpStringDoesNotThrowException()
		{
			AssertNull((string)((OrgAddressType)null));
		}

		public void TestCodePairList()
		{
			AssertEquals("OrgAddressType.List.Count", 13, OrgAddressType.CodePairList.Count);
			AssertEquals("Customs OrgAddressType", OrgConstants.AddressType.CustomsAddressOfRecord, OrgAddressType.CodePairList[10].Code);
			AssertEquals("EU Customs OrgAddressType", OrgConstants.AddressType.EUCustomsAddress, OrgAddressType.CodePairList[12].Code);
		}
	}
}

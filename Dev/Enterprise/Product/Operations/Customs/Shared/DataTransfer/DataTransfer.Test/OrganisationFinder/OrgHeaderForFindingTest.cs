using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Testing
{
	[TestedType(typeof(OrgHeaderForFinding))]
	sealed class OrgHeaderForFindingTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPatterMatchesType()
		{
			var orgForFinding = Factory.New<OrgHeaderForFinding>();
			AssertType(typeof(OrgPatternMatchCollectionForFinding), orgForFinding.PatternMatchesForThisOrg);
		}

		public void TestAllowEmptyAddresses()
		{
			var orgForFinding = Factory.New<OrgHeaderForFinding>();
			AssertEquals(false, orgForFinding.AllowEmptyAddresses);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var orgHeader = Factory.New<OrgHeaderForFinding>();
			orgHeader.OH_Code = "0001";
			return orgHeader;
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Testing
{
	[TestedType(typeof(OrgPatternMatchCollectionForFinding))]
	class OrgPatternMatchCollectionForFindingTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowEmptyAddresses()
		{
			var obj = GetCollectionToTest() as OrgPatternMatchCollectionForFinding;
			AssertEquals("AllowEmptyAddresses should be", true, obj.AllowEmptyAddresses);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var org = Factory.New<OrgHeaderForFinding>();
			org.AllowEmptyAddresses = true;
			return new OrgPatternMatchCollectionForFinding(org, new ZQuery());
		}
	}
}


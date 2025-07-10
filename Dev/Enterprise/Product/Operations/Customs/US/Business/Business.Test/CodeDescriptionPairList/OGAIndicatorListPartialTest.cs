using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class OGAIndicatorListTest : TestCaseWithFactory
	{
		public void TetsGetWithoutDisclaim()
		{
			var list = OGAIndicatorList.GetWithoutDisclaim(Factory);
			var list2 = OGAIndicatorList.GetWithoutDisclaim(Factory);
			AssertEquals(list, list2);
			AssertNotEquals(OGAIndicatorList.GetWithoutDisclaim(new BusinessObjectFactory()), list);
		}

		public void TestIsToBeDeclaredOrDisclaimed()
		{
			Assert(!OGAIndicatorList.IsToBeDisclaimed(OGAIndicatorList.Codes.Declared));
			Assert(OGAIndicatorList.IsToBeDisclaimed(OGAIndicatorList.Codes.Disclaimed));

			Assert(OGAIndicatorList.IsToBeDeclared(OGAIndicatorList.Codes.Declared));
			Assert(!OGAIndicatorList.IsToBeDeclared(OGAIndicatorList.Codes.Disclaimed));

			Assert(OGAIndicatorList.IsToBeDeclaredOrDisclaimed(OGAIndicatorList.Codes.Declared));
			Assert(OGAIndicatorList.IsToBeDeclaredOrDisclaimed(OGAIndicatorList.Codes.Disclaimed));
		}

		public void TestGetIdentifier()
		{
			AssertEquals("Declared", OGAIndicatorList.GetIdentifier(OGAIndicatorList.Codes.Declared));
			AssertEquals("Disclaimed", OGAIndicatorList.GetIdentifier(OGAIndicatorList.Codes.Disclaimed));
			AssertEquals("", OGAIndicatorList.GetIdentifier(ZString.Empty));
		}
	}
}

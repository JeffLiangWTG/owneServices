using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgWithSourceTest : TestCaseWithDummy
	{
		public void TestNewFromReturnsNullForEmptyOrInvalidGuid()
		{
			var address = Factory.New<OrgAddress>();
			address.OA_OH = ZGuid.Invalid;
			AssertNull(OrgWithSource.NewFrom<OrgHeader>(address.OA_OHInfo));

			address.OA_OH = ZGuid.Empty;
			AssertNull(OrgWithSource.NewFrom<OrgHeader>(address.OA_OHInfo));
		}

		public void TestExceptionErrorMessage()
		{
			var address = Factory.New<OrgAddress>();
			address.OA_OH = ZGuid.NewZGuid();

			AssertExceptionThrown<OrgWithSourceNotFoundException>(FormattableString.Invariant($"The Business Object could not be loaded: 'OA_OH' / '{address.OA_OH}'"), () => OrgWithSource.NewFrom<OrgHeader>(address.OA_OHInfo));
		}
	}
}

using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	class PkgPackageHeader_NoTransactionTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestSettingKPH_PackageIDShouldNotNormallySetSystemCreateFields()
		{
			var packageHeader = new BusinessObjectFactory().New<PkgPackageHeader>();
			packageHeader.KPH_PackageID = "ABC";
			AssertEquals(true, packageHeader.KPH_SystemCreateTimeUtc.IsEmpty);
			AssertEquals(true, packageHeader.KPH_SystemCreateUser.IsEmpty);
			AssertEquals(true, packageHeader.KPH_SystemLastEditTimeUtc.IsEmpty);
			AssertEquals(true, packageHeader.KPH_SystemLastEditUser.IsEmpty);
		}
	}
}

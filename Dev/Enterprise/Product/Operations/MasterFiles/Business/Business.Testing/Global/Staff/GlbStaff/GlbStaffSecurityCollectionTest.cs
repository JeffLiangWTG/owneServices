using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffSecurityCollection))]
	sealed class GlbStaffSecurityCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GlbStaffSecurityCollection>
	{
		protected override GlbStaffSecurityCollection GetCollectionToTest()
		{
			return new GlbStaffSecurityCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GlbStaffSecurity();
		}
	}
}

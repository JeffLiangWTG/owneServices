using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(GlbStaffCredentialCollection))]
	sealed class GlbExternalPasswordCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new GlbStaffCredentialCollection(Factory.NewWithValidTestData<GlbStaff>());
	}
}

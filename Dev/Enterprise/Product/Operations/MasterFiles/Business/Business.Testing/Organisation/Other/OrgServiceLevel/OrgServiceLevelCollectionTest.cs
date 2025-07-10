using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgServiceLevelCollection))]
	sealed class OrgServiceLevelCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgServiceLevelCollection>
	{
		protected override OrgServiceLevelCollection GetCollectionToTest()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			return new OrgServiceLevelCollection(Factory, header.PK);
		}
	}
}

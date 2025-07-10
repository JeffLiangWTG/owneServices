using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgRefFacilityCollection))]
	public class OrgRefFacilityCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new OrgRefFacilityCollection(Factory.New<OrgHeader>(), Factory);
	}
}

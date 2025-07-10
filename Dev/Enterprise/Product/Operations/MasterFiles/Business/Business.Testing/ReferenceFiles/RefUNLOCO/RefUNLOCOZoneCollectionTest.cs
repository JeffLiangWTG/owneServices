using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefUNLOCOZoneCollection))]
	sealed class RefUNLOCOZoneCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new RefUNLOCOZoneCollection(Factory.New<RefUNLOCO>());
		}
	}
}

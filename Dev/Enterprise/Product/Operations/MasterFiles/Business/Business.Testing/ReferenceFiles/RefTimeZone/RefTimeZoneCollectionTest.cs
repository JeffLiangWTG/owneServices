using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefTimeZoneCollection))]
	sealed class RefTimeZoneCollectionTest : ActiveBusinessObjectCollectionTestCase<RefTimeZoneCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<StandardTimeZone>();
		}
	}
}

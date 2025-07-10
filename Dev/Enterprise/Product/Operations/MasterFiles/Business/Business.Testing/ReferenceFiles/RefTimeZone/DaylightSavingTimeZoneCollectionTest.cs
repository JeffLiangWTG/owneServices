using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DaylightSavingTimeZoneCollection))]
	sealed class DaylightSavingTimeZoneCollectionTest : ActiveBusinessObjectCollectionTestCase<DaylightSavingTimeZoneCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<DaylightSavingTimeZone>();
		}
	}
}

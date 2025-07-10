using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefTimeZoneTypeDeciderTest : TestCaseWithFactory
	{
		#region GetTypeForNew

		public void TestGetTypeForNew()
		{
			RefTimeZone zone = Factory.New<RefTimeZone>();
			AssertEquals(typeof(StandardTimeZone), zone.GetType());
		}

		#endregion

		#region GetTypeForLoad

		public void GetTypeForLoad()
		{
			RefTimeZone timeZone1 = Factory.Load<RefTimeZone>(ZGuid.NewZGuid());
			AssertNull(timeZone1);

			StandardTimeZone standardZone = Factory.New<StandardTimeZone>();
			RefTimeZone timeZone2 = Factory.Load<RefTimeZone>(standardZone.PK);
			Assert(timeZone2 is StandardTimeZone);

			DaylightSavingTimeZone daylightSavingZone = Factory.New<DaylightSavingTimeZone>();

			RefTimeZone timeZone3 = Factory.Load<RefTimeZone>(daylightSavingZone.PK);
			Assert(timeZone3 is DaylightSavingTimeZone);
		}

		#endregion

		#region GetTypeForBinding

		public void TestGetTypeForBinding()
		{
			Type zoneType = new RefTimeZoneTypeDecider().GetTypeForBinding();
			AssertEquals(typeof(StandardTimeZone), zoneType);
		}

		#endregion
	}
}

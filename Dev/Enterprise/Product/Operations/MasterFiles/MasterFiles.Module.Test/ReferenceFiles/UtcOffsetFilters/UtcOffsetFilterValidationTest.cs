using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class UtcOffsetFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestUtcOffsetFilterValidation()
		{
			var expectedErrorMessage = "Please select a valid UTC offset from the list.";

			Filter.UtcOffsetFrom = "A";
			Filter.UtcOffsetTo = "B";
			AssertHasErrorContaining(Filter.UtcOffsetFromInfo, expectedErrorMessage);
			AssertHasErrorContaining(Filter.UtcOffsetToInfo, expectedErrorMessage);

			Filter.UtcOffsetFrom = "";
			Filter.UtcOffsetTo = "";
			AssertHasErrorContaining(Filter.UtcOffsetFromInfo, expectedErrorMessage);
			AssertHasErrorContaining(Filter.UtcOffsetToInfo, expectedErrorMessage);

			Filter.UtcOffsetFrom = "-780";
			Filter.UtcOffsetTo = "-780";
			AssertHasErrorContaining(Filter.UtcOffsetFromInfo, expectedErrorMessage);
			AssertHasErrorContaining(Filter.UtcOffsetToInfo, expectedErrorMessage);

			Filter.UtcOffsetFrom = "600";
			Filter.UtcOffsetTo = "-600";
			AssertNoNotifications(Filter.UtcOffsetFromInfo);
			AssertNoNotifications(Filter.UtcOffsetToInfo);
		}

		#region Implementation

		UtcOffsetFilter Filter
		{
			get
			{
				if (filter == null)
				{
					filter = new UtcOffsetFilter("UTC Offset", delegate
					{ return new ZQuery(); }, UtcOffsetList);
					filter.UtcOffsetFrom = "10";
					filter.UtcOffsetTo = "-10";
				}
				return filter;
			}
		}
		UtcOffsetFilter filter;

		List<ZShort> UtcOffsetList => new List<ZShort> { -660, -600, -540, -480, -420, -360, -300, -240, -180, -120, -60, 0, 60, 120, 180, 240, 300, 360, 420, 480, 540, 600, 660, 720, 780, 840 };
		#endregion
	}
}

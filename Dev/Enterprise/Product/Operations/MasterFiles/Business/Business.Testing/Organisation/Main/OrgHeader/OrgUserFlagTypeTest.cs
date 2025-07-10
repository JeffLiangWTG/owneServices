using System;
using System.Linq;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgUserFlagTypeTest : TransactionedTestCase
	{
		public void TestAll()
		{
			for (var i = 1; i <= Env.Registry.OrgUserFlagCount; i++)
			{
				Env.Registry.RawRegistry.FindByName("OrgUserFlag" + i + "Label").SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "My Label " + i);
			}

			var allTypes = OrgUserFlagType.All;
			AssertEquals("Count", Env.Registry.OrgUserFlagCount, allTypes.Count());
			for (var i = 1; i <= Env.Registry.OrgUserFlagCount; i++)
			{
				var expectedFlagNumber = i.ToString().PadLeft(2, '0');
				AssertCollectionContains("Should contain item for Flag # " + i, allTypes, x => x.FlagNumber == expectedFlagNumber);

				var flagType = allTypes.First(x => x.FlagNumber == expectedFlagNumber);
				CombineAssertions("Properties for User Flag #" + i, () =>
				{
					AssertEquals("OrgHeaderColumn", "OH_IsUserFlag" + i, flagType.OrgHeaderColumn.Name);
					AssertEquals("Label", "My Label " + i, flagType.Label);
				});
			}
		}
	}
}

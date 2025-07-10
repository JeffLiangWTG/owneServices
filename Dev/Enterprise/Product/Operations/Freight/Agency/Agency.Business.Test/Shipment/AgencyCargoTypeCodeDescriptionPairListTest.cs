using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyCargoTypeCodeDescriptionPairListTest : TestCaseWithFactory
	{
		public void TestCargoTypesList()
		{
			const string expected = @"FCL - Full Container Load
BLK - Bulk
LQD - Liquid
BBK - Break Bulk
ROR - Roll On/Roll Off";
			AssertMultilineASCIIEquals("", expected, new AgencyCargoTypeCodeDescriptionPairList().ElementsAsString);
		}

		public void TestTopLevelPackCargoTypes()
		{
			var expected = new ZString[] { Constants.ContainerModes.RollOnRollOff, Constants.ContainerModes.BreakBulk, Constants.ContainerModes.Bulk, Constants.ContainerModes.Liquid };
			AssertContainsExactElementsInAnyOrder(expected, AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes);
		}
	}
}

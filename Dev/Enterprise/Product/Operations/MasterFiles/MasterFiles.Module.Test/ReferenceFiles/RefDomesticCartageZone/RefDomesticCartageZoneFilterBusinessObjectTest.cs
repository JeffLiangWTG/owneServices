using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefDomesticCartageZoneFilterBusinessObject))]
	sealed class RefDomesticCartageZoneFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestIsBeyond()
		{
			RefDomesticCartageZone zone1 = Factory.New<RefDomesticCartageZone>();
			zone1.F1_CityTownPostCode = "11111";
			zone1.F1_IsBeyond = true;
			RefDomesticCartageZone zone2 = Factory.New<RefDomesticCartageZone>();
			zone2.F1_CityTownPostCode = "22222";
			zone2.F1_IsBeyond = false;
			Factory.Save();

			RefDomesticCartageZone[] result;
			RefDomesticCartageZoneFilterBusinessObject filter = new RefDomesticCartageZoneFilterBusinessObject();
			ModuleFlagsFilter strip = (ModuleFlagsFilter)filter["Is Beyond"];

			strip.Property0 = ZBool.True;
			result = Factory.Load<RefDomesticCartageZone>(strip.Query);
			AssertCollectionContains("Zone1", zone1, result);
			AssertCollectionNotContains("Zone2", zone2, result);

			strip.Property0 = ZBool.False;
			result = Factory.Load<RefDomesticCartageZone>(strip.Query);
			AssertCollectionContains("Zone1", zone1, result);
			AssertCollectionContains("Zone2", zone2, result);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefDomesticCartageZoneFilterBusinessObject();
		}

		#endregion
	}
}

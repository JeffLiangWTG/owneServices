using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Test;

class UrsCarrierTest : TestCaseWithFactory
{
	public void TestUrsCarrierReferenceLineProperties()
	{
		var line1 = Factory.New<RefAirline>();
		line1.RM_TwoCharacterCode = "11";
		line1.RM_AirlineName1 = "AB";
		var line2 = Factory.New<RefAirline>();
		line2.RM_TwoCharacterCode = "11";
		line1.RM_AirlineName1 = "ABC";
		Factory.Save();
		var carrier = UrsCarrier.FromIATA("11");
		AssertContainsExactElementsInAnyOrder([line1.PK, line2.PK], carrier.RefAirlines.Select(l => l.PK));
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefVesselNameFinderTest : TestCaseWithFactory
	{
		public void TestFindVesselNameByLloydsNumber()
		{
			var testFinder = new RefVesselNameFinder();
			string lloydsNumber = "8811924";
			RefVessel expectedVessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, SQLComparisonOperator.Equal, lloydsNumber));

			AssertEquals("FindVesselNameByLloydsNumber should return", expectedVessel.RV_Code, testFinder.FindVesselNameByLloydsNumber(lloydsNumber, Factory));
			AssertEquals("FindVesselNameByLloydsNumber should return", ZString.Empty, testFinder.FindVesselNameByLloydsNumber("31337", Factory));
		}
	}
}

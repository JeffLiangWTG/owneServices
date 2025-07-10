using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AMS.Messaging.Business.StowPlan;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(StowPlanVesselData))]
	class StowPlanVesselDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIStowPlanVesselDataMembers()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Vessel name";
			vessel.RV_LloydsNumber = "IMO";
			vessel.RV_CarrierCode = "SCAC";

			IStowPlanVesselData vesselData = new StowPlanVesselData(vessel);
			AssertEquals("Vessel name", vesselData.VesselName);
			AssertEquals("IMO", vesselData.IMONumber);
			AssertEquals("SCAC", vesselData.VesselOperator);
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			return new StowPlanVesselData(Factory.New<RefVessel>());
		}
	}
}

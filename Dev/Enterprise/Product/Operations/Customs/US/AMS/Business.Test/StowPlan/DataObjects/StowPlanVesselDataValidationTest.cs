using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class StowPlanVesselDataValidationTest : TestCaseWithFactory
	{
		public void TestCheckIMONumber()
		{
			vesselData.Validation.ValidateIMONumber();
			AssertHasMessageErrorContaining(vesselData.IMONumberInfo, MandatoryValidation.YouHaveNotEntered);
			vessel.RV_LloydsNumber = "IMO";
			vesselData.Validation.ValidateIMONumber();
			AssertNoMessageErrorContaining(vesselData.IMONumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckVesselName()
		{
			vesselData.Validation.ValidateVesselName();
			AssertHasMessageErrorContaining(vesselData.VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
			vessel.RV_Code = "Vessel name";
			vesselData.Validation.ValidateVesselName();
			AssertNoMessageErrorContaining(vesselData.VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckVesselOperator()
		{
			vesselData.Validation.ValidateVesselOperator();
			AssertHasMessageErrorContaining(vesselData.VesselOperatorInfo, MandatoryValidation.YouHaveNotEntered);
			vessel.RV_CarrierCode = "SCAC";
			vesselData.Validation.ValidateVesselOperator();
			AssertNoMessageErrorContaining(vesselData.VesselOperatorInfo, MandatoryValidation.YouHaveNotEntered);
		}

		RefVessel vessel;
		StowPlanVesselData vesselData;
		protected override void SetUp()
		{
			base.SetUp();
			vessel = Factory.New<RefVessel>();
			vesselData = new StowPlanVesselData(vessel);
		}
	}
}

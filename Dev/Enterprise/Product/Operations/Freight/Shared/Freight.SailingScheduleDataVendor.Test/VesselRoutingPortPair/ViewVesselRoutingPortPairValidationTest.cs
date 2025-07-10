using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	sealed class ViewVesselRoutingPortPairValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateE9_RL_NKLoadPort()
		{
			VesselRoutingPortPair portPair = Factory.New<VesselRoutingPortPair>();
			portPair.E9_RL_NKLoadPort = "AUSYD";
			portPair.E9_RL_NKLoadPort = "";
			AssertHasWarning(portPair.E9_RL_NKLoadPortInfo, "Add foreign ports using the controls below");
		}

		public void TestValidateE9_RL_NKDischargePort()
		{
			VesselRoutingPortPair portPair = Factory.New<VesselRoutingPortPair>();
			portPair.E9_RL_NKDischargePort = "AUSYD";
			portPair.E9_RL_NKDischargePort = "";
			AssertHasWarning(portPair.E9_RL_NKDischargePortInfo, "Add foreign ports using the controls below");
		}
	}
}

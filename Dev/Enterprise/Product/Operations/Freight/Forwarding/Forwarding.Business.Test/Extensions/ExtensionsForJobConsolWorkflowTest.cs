using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ExtensionsForJobConsolWorkflowTest : TestCaseWithFactory
	{
		public void TestAll()
		{
			ProcessTaskTemplate template = null;
			ForwardingConsol consol = null;
			Assert(!consol.IsAir());
			Assert(!consol.IsSea());
			Assert(!consol.IsRail());
			Assert(!consol.IsImportTo(Core.Constants.CountryCodes.Australia));
			Assert(!template.IsAir());
			Assert(!template.IsSea());
			Assert(!template.IsRail());
			Assert(!template.IsEmpty());
			Assert(!template.IsImportTo(Core.Constants.CountryCodes.Australia));
			consol = Factory.New<ForwardingConsol>();
			template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			template.P0_SubType1 = Core.Constants.TransportModes.Air;
			Assert(consol.IsAir());
			Assert(template.IsAir());
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			template.P0_SubType1 = Core.Constants.TransportModes.Sea;
			Assert(consol.IsSea());
			Assert(template.IsSea());
			consol.JK_TransportMode = Core.Constants.TransportModes.Rail;
			template.P0_SubType1 = Core.Constants.TransportModes.Rail;
			Assert(consol.IsRail());
			Assert(template.IsRail());
			template.P0_SubType1 = "";
			Assert(template.IsEmpty());
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			template.P0_DischargePortCountry = "AU";
			template.P0_LoadPortCountry = "US";
			Assert(consol.IsImportTo(Core.Constants.CountryCodes.Australia));
			Assert(template.IsImportTo(Core.Constants.CountryCodes.Australia));
		}
	}
}

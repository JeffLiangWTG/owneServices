using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbBranchDefaultPortValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckGBP_DefaultTo()
		{
			defaultPort.Validation.ValidateGBP_DefaultTo();
			AssertHasRowError(defaultPort, "Duplicate rows are not allowed. This row is duplicate of another row.");

			defaultPort.GBP_DefaultTo = GlbBranchDefaultToList.Codes.ConsolLastDischarge;
			AssertNoRowErrors(defaultPort);

			defaultPort.GBP_DefaultTo = "ZZZZZZ";
			defaultPort.Validation.ValidateGBP_DefaultTo();
			AssertHasError(defaultPort.GBP_DefaultToInfo, "Enter a valid selection.");

			defaultPort.GBP_DefaultTo = "";
			defaultPort.Validation.ValidateGBP_DefaultTo();
			AssertHasError(defaultPort.GBP_DefaultToInfo, "Please enter a value.");
		}

		public void TestCheckGBP_TransportMode()
		{
			defaultPort.Validation.ValidateGBP_TransportMode();
			AssertHasRowError(defaultPort, "Duplicate rows are not allowed. This row is duplicate of another row.");

			defaultPort.GBP_TransportMode = Core.Constants.TransportModes.Sea;
			AssertNoRowErrors(defaultPort);

			defaultPort.GBP_TransportMode = "ZZZ";
			defaultPort.Validation.ValidateGBP_TransportMode();
			AssertHasError(defaultPort.GBP_TransportModeInfo, "Enter a valid selection.");

			defaultPort.GBP_TransportMode = "";
			defaultPort.Validation.ValidateGBP_TransportMode();
			AssertHasError(defaultPort.GBP_TransportModeInfo, "Please enter a value.");
		}

		public void TestCheckGBP_ContainerMode()
		{
			defaultPort.Validation.ValidateGBP_ContainerMode();
			AssertHasRowError(defaultPort, "Duplicate rows are not allowed. This row is duplicate of another row.");

			defaultPort.GBP_ContainerMode = Core.Constants.ContainerModes.Loose;
			AssertNoRowErrors(defaultPort);

			defaultPort.GBP_ContainerMode = "ZZZ";
			defaultPort.Validation.ValidateGBP_ContainerMode();
			AssertHasError(defaultPort.GBP_ContainerModeInfo, "Enter a valid selection.");

			defaultPort.GBP_ContainerMode = "";
			defaultPort.Validation.ValidateGBP_ContainerMode();
			AssertHasError(defaultPort.GBP_ContainerModeInfo, "Please enter a value.");
		}

		public void TestCheckGBP_GBP_RL_NKPort()
		{
			defaultPort.GBP_RL_NKPort = "AUADL";
			defaultPort.Validation.ValidateGBP_RL_NKPort();
			AssertHasError(defaultPort.GBP_RL_NKPortInfo, "The Default Port Code must be either Home Port or one of the Additional Related Ports.");

			defaultPort.GBP_RL_NKPort = "AUA5T";
			defaultPort.Validation.ValidateGBP_RL_NKPort();
			AssertHasWarning(defaultPort.GBP_RL_NKPortInfo, "The selected UNLOCO for transport mode Air does not have airport. Please check UNLOCO's 'Has Airport' attribute.");

			defaultPort.GBP_TransportMode = Core.Constants.TransportModes.Sea;
			defaultPort.Validation.ValidateGBP_RL_NKPort();
			AssertHasWarning(defaultPort.GBP_RL_NKPortInfo, "The selected UNLOCO for transport mode Sea does not have seaport. Please check UNLOCO's 'Has Seaport' attribute.");

			defaultPort.GBP_RL_NKPort = "";
			defaultPort.Validation.ValidateGBP_RL_NKPort();
			AssertHasError(defaultPort.GBP_RL_NKPortInfo, "Please enter a value.");
		}

		#region Implementation

		GlbBranch branch;
		GlbBranchDefaultPort defaultPort;

		protected override void SetUp()
		{
			base.SetUp();
			branch = Factory.New<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUBNE";
			var extraPort1 = branch.ExtraPorts.AddNew();
			extraPort1.GY_RL_NKAdditionalBranchRelatedPort = "AUSYD";
			var extraPort2 = branch.ExtraPorts.AddNew();
			extraPort2.GY_RL_NKAdditionalBranchRelatedPort = "AUMEL";
			var extraPort3 = branch.ExtraPorts.AddNew();
			extraPort3.GY_RL_NKAdditionalBranchRelatedPort = "AUA5T";

			branch.AddDefaultPort(GlbBranchDefaultToList.Codes.ConsolFirstLoad, "AIR", "ALL", "AUSYD");
			defaultPort = branch.AddDefaultPort(GlbBranchDefaultToList.Codes.ConsolFirstLoad, "AIR", "ALL", "AUMEL");
		}

		#endregion
	}
}

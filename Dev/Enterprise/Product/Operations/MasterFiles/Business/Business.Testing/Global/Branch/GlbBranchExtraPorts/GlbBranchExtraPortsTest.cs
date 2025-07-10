using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbBranchExtraPorts))]
	sealed class GlbBranchExtraPortsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPortName()
		{
			GlbBranch branch = Factory.New<GlbBranch>();
			GlbBranchExtraPorts extraPort = branch.ExtraPorts.AddNew();
			AssertEquals("", extraPort.PortName);

			extraPort.GY_RL_NKAdditionalBranchRelatedPort = "AUSYD";
			AssertEquals("Sydney", extraPort.PortName);
		}

		public void TestDefaultPortsAreValidatedWhenExtraPortIsChanged()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUBNE";
			var port1 = branch.AddDefaultPort(GlbBranchDefaultToList.Codes.ConsolFirstLoad, "SEA", "FCL", "AUSYD");
			var port2 = branch.AddDefaultPort(GlbBranchDefaultToList.Codes.ConsolFirstLoad, "SEA", "ALL", "AUMEL");
			Factory.Save();

			branch.DefaultPorts.RunPreSaveValidation();
			Assert(!port1.ShouldValidateOnSave);
			Assert(!port2.ShouldValidateOnSave);

			var extraPort1 = branch.ExtraPorts.Cast<GlbBranchExtraPorts>().FirstOrDefault(p => p.GY_RL_NKAdditionalBranchRelatedPort == port1.GBP_RL_NKPort);
			AssertNotNull(extraPort1);
			extraPort1.GY_RL_NKAdditionalBranchRelatedPort = "AUADL";

			Assert(port1.ShouldValidateOnSave);
			Assert(port2.ShouldValidateOnSave);
		}

		#region Test Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			GlbBranch branch = Factory.New<GlbBranch>();
			return branch.ExtraPorts.AddNew();
		}

		#endregion
	}
}

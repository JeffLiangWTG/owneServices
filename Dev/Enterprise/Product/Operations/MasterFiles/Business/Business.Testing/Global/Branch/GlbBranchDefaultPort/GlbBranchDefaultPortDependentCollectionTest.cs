using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbBranchDefaultPortDependentCollection))]
	public class GlbBranchDefaultPortDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetDefaultPort()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUBNE";
			var port1 = branch.AddDefaultPort(GlbBranchDefaultToList.Codes.ConsolFirstLoad, "SEA", "FCL", "AUSYD");
			var port2 = branch.AddDefaultPort(GlbBranchDefaultToList.Codes.ConsolFirstLoad, "SEA", "ALL", "AUMEL");
			Factory.Save();

			var defaultPorts = branch.DefaultPorts;
			AssertEquals(port1, defaultPorts.GetDefaultPort(GlbBranchDefaultToList.Codes.ConsolFirstLoad, "SEA", "FCL"));
			AssertEquals(port2, defaultPorts.GetDefaultPort(GlbBranchDefaultToList.Codes.ConsolFirstLoad, "SEA", "LCL"));
			AssertNull(defaultPorts.GetDefaultPort(GlbBranchDefaultToList.Codes.ConsolFirstLoad, "AIR", "LSE"));
			AssertNull(defaultPorts.GetDefaultPort(GlbBranchDefaultToList.Codes.ShipmentOrigin, "SEA", "FCL"));
		}

		public void TestGetDefaultPort_EmptyContainerMode()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUBNE";
			var port = branch.AddDefaultPort(GlbBranchDefaultToList.Codes.ConsolFirstLoad, "RAI", "ALL", "AUSYD");
			Factory.Save();

			var defaultPorts = branch.DefaultPorts;
			AssertEquals(port, defaultPorts.GetDefaultPort(GlbBranchDefaultToList.Codes.ConsolFirstLoad, "RAI", ""));
			AssertEquals(port, defaultPorts.GetDefaultPort(GlbBranchDefaultToList.Codes.ConsolFirstLoad, "RAI", null));
		}

		public void TestContainsDuplicatedCombination()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUBNE";
			branch.AddDefaultPort(GlbBranchDefaultToList.Codes.ConsolFirstLoad, "SEA", "FCL", "AUSYD");
			branch.AddDefaultPort(GlbBranchDefaultToList.Codes.ConsolFirstLoad, "SEA", "ALL", "AUMEL");

			var newPort = branch.AddDefaultPort(GlbBranchDefaultToList.Codes.ConsolFirstLoad, "SEA", "FCL", "AUADL");
			Assert(branch.DefaultPorts.ContainsDuplicatedCombination(newPort));
			AssertHasRowError(newPort, "Duplicate rows are not allowed. This row is duplicate of another row.");

			newPort = branch.AddDefaultPort(GlbBranchDefaultToList.Codes.ShipmentDestination, "SEA", "ALL", "AUADL");
			Assert(!branch.DefaultPorts.ContainsDuplicatedCombination(newPort));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlbBranchDefaultPortDependentCollection(Factory.NewWithValidTestData<GlbBranch>());
		}
	}
}



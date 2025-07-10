using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbBranchDefaultPort))]
	public class GlbBranchDefaultPortTest : EnterpriseBusinessObjectTestCase
	{
		#region Test Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			GlbBranch branch = Factory.New<GlbBranch>();
			return branch.DefaultPorts.AddNew();
		}

		#endregion

		public void TestEqualsDefaultCombination()
		{
			var defaultPort = GlbBranchDefaultPortTestHelper.CreateNewDefaultPort(Factory, GlbBranchDefaultToList.Codes.ConsolFirstLoad, "SEA", "FCL", "AUSYD");
			var anotherDefaultPort = GlbBranchDefaultPortTestHelper.CreateNewDefaultPort(Factory, GlbBranchDefaultToList.Codes.ConsolFirstLoad, "SEA", "FCL", "AUMEL");

			Assert(defaultPort.EqualsDefaultCombination(anotherDefaultPort));

			anotherDefaultPort.GBP_TransportMode = "AIR";
			Assert(!defaultPort.EqualsDefaultCombination(anotherDefaultPort));
		}
	}
}

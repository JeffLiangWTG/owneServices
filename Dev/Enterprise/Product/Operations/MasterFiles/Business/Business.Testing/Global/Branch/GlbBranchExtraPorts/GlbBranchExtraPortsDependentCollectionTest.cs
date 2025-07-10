using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbBranchExtraPortsDependentCollection))]
	class GlbBranchExtraPortsDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			GlbBranch glbBranch = Factory.NewWithValidTestData<GlbBranch>();
			return new GlbBranchExtraPortsDependentCollection(glbBranch);
		}

		public void TestDefaultPortsAreValidatedWhenExtraPortIsDeleted()
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
			branch.ExtraPorts.RemoveAndDelete(extraPort1);

			Assert(port1.ShouldValidateOnSave);
			Assert(port2.ShouldValidateOnSave);
		}
	}
}

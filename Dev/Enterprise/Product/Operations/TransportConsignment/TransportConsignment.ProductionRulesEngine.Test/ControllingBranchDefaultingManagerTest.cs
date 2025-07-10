using System;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportConsignment.Business;

namespace Enterprise.TransportConsignment.ProductionRulesEngine.Testing
{
	public class ControllingBranchDefaultingManagerTest : ValueDefaultingManagerTest
	{
		protected override string Context => "LCB";

		protected override Guid PK1 => Branch1.PK.ToGuid();

		protected override Guid PK2 => Branch2.PK.ToGuid();

		protected override ValueDefaultingManager GetDefaultingManager(ILandTransportFactLoaderProvider factLoaderProvider)
		{
			return new ControllingBranchDefaultingManager(factLoaderProvider);
		}

		GlbBranch Branch1 => branch1 ?? (branch1 = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany));
		GlbBranch branch1;

		GlbBranch Branch2 => branch2 ?? (branch2 = TestObjectCreator.CreateBranch("DEF", GlbCompany.CurrentCompany));
		GlbBranch branch2;
	}
}

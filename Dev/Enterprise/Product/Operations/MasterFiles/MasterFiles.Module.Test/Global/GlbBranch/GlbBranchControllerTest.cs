using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbBranchController))]
	sealed class GlbBranchControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GlbBranch;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			Business.GlbBranch branch = Factory.New<Business.GlbBranch>();
			BusinessObject[] companies = Factory.Load(typeof(Business.GlbCompany), new ZQuery());
			if (companies.Length > 0)
			{
				branch.GB_GC = ((Business.GlbCompany)companies[0]).PK;
			}
			Factory.Save();
			return branch;
		}
	}
}

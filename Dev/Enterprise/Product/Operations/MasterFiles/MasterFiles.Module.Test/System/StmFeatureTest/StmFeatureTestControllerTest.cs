using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(StmFeatureTestController))]
	sealed class StmFeatureTestControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var bizO = Factory.NewWithValidTestData<StmFeatureTest>();
			var group = Factory.NewWithValidTestData<GlbGroup>();
			bizO.SFT_GG_Group = group.PK;
			Factory.Save();
			return bizO;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.StmFeatureTest;
		}
	}
}

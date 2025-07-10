using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(SalesTeamController))]
	sealed class TestSalesTeamController : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.SalesTeam;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			SalesTeam salesTeam = Factory.New<SalesTeam>();
			Factory.Save();
			return salesTeam;
		}
	}
}

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccGroupsController))]
	sealed class AccGroupsControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AccGroups;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			AccGroups groups = Factory.New<AccGroups>();
			Factory.Save();
			return groups;
		}
	}
}

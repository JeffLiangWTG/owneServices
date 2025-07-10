using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ActiveUsersController))]
	sealed class ActiveUsersControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ActiveUsers;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new ActiveUser(Factory) { AU_HeartbeatId = ZGuid.NewZGuid() };
		}
	}
}

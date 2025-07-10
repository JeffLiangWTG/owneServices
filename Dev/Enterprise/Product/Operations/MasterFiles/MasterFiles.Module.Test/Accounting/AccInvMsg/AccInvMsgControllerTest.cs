using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccInvMsgController))]
	sealed class AccInvMsgControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AccInvMsg;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			AccInvMsg msg = Factory.NewWithValidTestData<AccInvMsg>();
			Factory.Save();
			return msg;
		}
	}
}

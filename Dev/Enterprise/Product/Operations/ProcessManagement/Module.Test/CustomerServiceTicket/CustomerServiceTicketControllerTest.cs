using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Module.Test
{
	[TestedType(typeof(CustomerServiceTicketController))]
	class CustomerServiceTicketControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CustomerServiceTicket;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var bizo = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			Factory.Save();

			return bizo;
		}
	}
}

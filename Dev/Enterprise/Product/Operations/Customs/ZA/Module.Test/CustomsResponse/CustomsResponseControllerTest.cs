using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(CustomsResponseController))]
	sealed class CustomsResponseControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ZAControllerIDs.CustomsResponse;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var message = Factory.NewWithValidTestData<CUSRESEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();
			return message;
		}
	}
}

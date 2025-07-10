using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class BIRDTransportModeTest : TestCaseWithFactory
	{
		public void TestSetTransportMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();

			BIRDTransportMode.SetTransportMode(declaration, TransportModeCodes.Codes.AirContainer, notifications);
			Assert(!notifications.HasNotifications());
			AssertEquals("JE_TransportMode is calculated", TransportTypeList.Codes.Air, declaration.JE_TransportMode);
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.Containerised, declaration.JE_ContainerMode);

			declaration.JE_ContainerMode = ZString.Empty;
			BIRDTransportMode.SetTransportMode(declaration, TransportModeCodes.Codes.AirNonContainer, notifications);
			AssertEquals("JE_TransportMode is calculated", TransportTypeList.Codes.Air, declaration.JE_TransportMode);
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			notifications.Clear();
			BIRDTransportMode.SetTransportMode(declaration, TransportModeCodes.Codes.AirNonContainer, notifications);
			AssertEquals("JE_TransportMode is calculated", TransportTypeList.Codes.Air, declaration.JE_TransportMode);
			AssertEquals("JE_ContainerMode remains as user has entered", Core.Constants.ContainerModes.LCL, declaration.JE_ContainerMode);
			Assert(!notifications.HasNotifications());
		}
	}
}

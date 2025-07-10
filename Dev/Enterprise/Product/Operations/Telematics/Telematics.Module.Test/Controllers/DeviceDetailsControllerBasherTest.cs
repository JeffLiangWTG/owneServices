using Enterprise.Telematics.Module.Controllers;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Module.Test
{
	[TestedType(typeof(DeviceDetailsController))]
	public class DeviceDetailsControllerBasherTest : ZControllerBasherTest
	{
		#region ZControllerBasherTest

		protected override ControllerID GetControllerID() => ControllerIDs.DeviceDetails;

		#endregion
	}
}

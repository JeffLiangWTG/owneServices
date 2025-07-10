using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(AgencyPortMessagesController))]
	internal class AgencyPortMessagesControllerBasherTest : ZControllerBasherTest
	{
		#region Implementation
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AgencyPortMessaging;
		}
		#endregion
	}
}

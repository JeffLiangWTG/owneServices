using Enterprise.Environment;
using Enterprise.TransportConsignment.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Module.Testing
{
	[TestedType(typeof(DtbRoutePlannerController))]
	public class DtbRoutePlannerControllerTest : ZPopupControllerBasherTest
	{
		#region TestSecurityCheckpoint

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.DtbRoutePlanner, GetNewController().GetCheckPointForNew(new DtbRoutePlanner(Factory)));
		}

		#endregion

		#region TestNewDisplayModeIsSaved

		public void TestNewDisplayModeIsSaved()
		{
			using (var form = GetNewController().ShowNewForm())
			{
				AssertEquals(ODisplayMode.NewSaved, form.DisplayMode);
			}
		}

		#endregion

		#region Implementation

		DtbRoutePlannerController GetNewController()
		{
			return new DtbRoutePlannerController();
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.DtbRoutePlanner;
		}

		#endregion
	}
}

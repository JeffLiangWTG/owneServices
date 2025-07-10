using Enterprise.Environment;
using Enterprise.Freight.GUI.OnlineSailingSchedules;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(OnlineSailingSchedulesController))]
	sealed class OnlineSailingSchedulesControllerTest : ZSingletonControllerBasherTest
	{
		public void TestCheckPointForNew()
		{
			AssertEquals("Security check point", Env.Security.OnlineSailingSchedules, Controller.CheckPointForNew);
		}

		public void TestGetForm()
		{
			using (OnlineSchedulesForm shownForm = (OnlineSchedulesForm)Controller.ShowNewForm())
			{
				AssertNotNull("Correct form type must be shown", shownForm);
			}
		}

		public void TestDisplayMode()
		{
			using (OnlineSchedulesForm shownForm = (OnlineSchedulesForm)Controller.ShowNewForm())
			{
				AssertEquals(ODisplayMode.Browse, shownForm.DisplayMode);
			}
		}

		#region Test Classes

		class TestScheduleFeedServiceController : OnlineSailingSchedulesController
		{
			public new SecurityCheckpoint CheckPointForNew => base.CheckPointForNew;
		}

		#endregion

		#region Implementation

		new TestScheduleFeedServiceController Controller
		{
			get { return controller ?? (controller = new TestScheduleFeedServiceController()); }
		}
		TestScheduleFeedServiceController controller;

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.OnlineSailingSchedules;
		}

		#endregion
	}
}

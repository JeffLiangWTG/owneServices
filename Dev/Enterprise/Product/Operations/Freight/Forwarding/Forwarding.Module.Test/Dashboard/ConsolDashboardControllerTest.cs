using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(ConsolDashboardController))]
	class ConsolDashboardControllerTest : ZSingletonControllerBasherTest
	{
		public void TestCheckPointForNew()
		{
			AssertEquals("Security check point", Env.Security.ConsolPlanningBoard, Controller.CheckPointForNew);
		}

		[RequiresSTA]
		public void TestGetForm()
		{
			using (var shownForm = Controller.ShowNewForm() as ConsolDashboardForm)
			{
				AssertNotNull("Correct form type must be shown", shownForm);
				AssertEquals("Dashboard should be using readonly factory", true, shownForm.BusinessEntity.Factory is ReadOnlyBusinessObjectFactory);
			}
		}

		[RequiresSTA]
		public void TestDisplayMode()
		{
			using (var shownForm = Controller.ShowNewForm() as ConsolDashboardForm)
			{
				AssertEquals(ODisplayMode.Browse, shownForm.DisplayMode);
			}
		}

		#region Test Classes

		class TestConsolDashboardController : ConsolDashboardController
		{
			public new SecurityCheckpoint CheckPointForNew => base.CheckPointForNew;
		}

		#endregion

		#region Implementation

		new TestConsolDashboardController Controller
		{
			get { return controller ?? (controller = new TestConsolDashboardController()); }
		}
		TestConsolDashboardController controller;

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ConsolPlanningBoard;
		}

		#endregion
	}
}

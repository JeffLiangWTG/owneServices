using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	[TestedType(typeof(CartageRunSheetDashboardController))]
	public class CartageRunSheetDashboardControllerTest : ZPopupControllerBasherTest
	{
		public void TestCheckPointForNew()
		{
			AssertEquals("Security check point", Env.Security.LocalTransportRunSheetDashboard, Controller.CheckPointForNew);
		}

		public void TestGetForm()
		{
			using (RunSheetDashboardForm shownForm = (RunSheetDashboardForm)Controller.ShowNewForm())
			{
				AssertNotNull("Correct form type should be shown", shownForm);
				var cartageRunSheetDashboards = (RunSheetDashboardCollection)shownForm.DataSource;
				AssertNotNull("Local TransportRunSheetDashboard DataSource should have been created", cartageRunSheetDashboards);
				AssertEquals("Local Transport Behaviour strategy should have been set. Document Supporter behaviour is required", typeof(CartageBehaviorStrategyProvider), CommonCartageBehaviorStrategyProvider.GetProvider(cartageRunSheetDashboards.Factory).GetType());
			}
		}

		class TestCartageRunSheetDashboardController : CartageRunSheetDashboardController
		{
			public new SecurityCheckpoint CheckPointForNew
			{
				get
				{
					return base.CheckPointForNew;
				}
			}
		}

		new TestCartageRunSheetDashboardController Controller
		{
			get
			{
				return controller ?? (controller = new TestCartageRunSheetDashboardController());
			}
		}

		TestCartageRunSheetDashboardController controller;
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CartageRunSheetDashboard;
		}
	}
}

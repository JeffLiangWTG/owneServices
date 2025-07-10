using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	[TestedType(typeof(CartageLegPlannerController))]
	public class CartageLegPlannerControllerTest : ZPopupControllerBasherTest
	{
		public void TestCheckPointForNew()
		{
			AssertEquals("Security check point", Env.Security.LocalTransportLegPlanner, Controller.CheckPointForNew);
		}

		public void TestGetForm()
		{
			using (CartageLegPlannerForm shownForm = (CartageLegPlannerForm)Controller.ShowNewForm())
			{
				AssertNotNull("Correct form type should be shown", shownForm);
				var cartageLegPlanner = (CartageLegPlannerCollection)shownForm.DataSource;
				AssertNotNull("CartageLegPlanner DataSource should have been created", cartageLegPlanner[0]);
				AssertEquals("Local Transport Behaviour strategy should have been set. Document Supporter behaviour is required", typeof(CartageBehaviorStrategyProvider), CommonCartageBehaviorStrategyProvider.GetProvider(cartageLegPlanner.Factory).GetType());
			}
		}

		class TestCartageLegPlannerController : CartageLegPlannerController
		{
			public new SecurityCheckpoint CheckPointForNew
			{
				get
				{
					return base.CheckPointForNew;
				}
			}
		}

		new TestCartageLegPlannerController Controller
		{
			get
			{
				return controller ?? (controller = new TestCartageLegPlannerController());
			}
		}

		TestCartageLegPlannerController controller;
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CartageLegPlanner;
		}
	}
}

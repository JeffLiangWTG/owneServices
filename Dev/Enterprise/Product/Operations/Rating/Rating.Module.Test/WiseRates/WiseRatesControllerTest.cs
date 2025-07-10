using Enterprise.Environment;
using Enterprise.Rating.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(WiseRatesController))]
	class WiseRatesControllerTest : ZSingletonControllerBasherTest
	{
		public void TestCheckPointForNew()
		{
			AssertEquals("Security check point", Env.Security.WiseRatesSearch, Controller.CheckPointForNew);
		}

		public void TestGetForm()
		{
			using (var shownForm = (WiseRatesForm)Controller.ShowNewForm())
			{
				AssertNotNull("Correct form type must be shown", shownForm);
			}
		}

		#region Test Classes

		class WiseRatesControllerForTest : WiseRatesController
		{
			public new SecurityCheckpoint CheckPointForNew => base.CheckPointForNew;
		}

		#endregion

		#region Implementation

		new WiseRatesControllerForTest Controller => controller ?? (controller = new WiseRatesControllerForTest());
		WiseRatesControllerForTest controller;

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WiseRates;
		}

		#endregion
	}
}

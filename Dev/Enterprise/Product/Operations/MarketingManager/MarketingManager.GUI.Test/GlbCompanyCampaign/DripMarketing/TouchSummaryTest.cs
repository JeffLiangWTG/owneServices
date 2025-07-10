using CargoWise.Main.Navigation.WPF.Test;
using NUnit.Framework;
using Control = System.Windows.Controls.Control;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(TouchSummary))]
	class TouchSummaryTest : WPFControlBasherTest
	{
		protected override Control GetControlToBashCore()
		{
			return new TouchSummary();
		}

		[ExpectNoExceptions]
		public void TestViewModel()
		{
			var control = new TouchSummary();
			AssertNotNull(control.ViewModel);
		}
	}
}

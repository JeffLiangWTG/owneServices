using CargoWise.EntityFramework.Testing;

namespace Enterprise.Rating.GUI.Testing
{
	public class WizardSteppingEventArgsTest : TestCaseWithFactory
	{
		public void TestArgs()
		{
			WizardSteppingEventArgs args = new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Back);
			AssertEquals(WizardSteppingEventArgs.Direction.Back, args.MovementDirection);

			args = new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Forward);
			AssertEquals(WizardSteppingEventArgs.Direction.Forward, args.MovementDirection);
		}
	}
}

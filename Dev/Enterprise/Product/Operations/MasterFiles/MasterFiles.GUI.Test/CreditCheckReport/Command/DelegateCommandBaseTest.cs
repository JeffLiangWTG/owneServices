using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class DelegateCommandBaseTest : TestCase
	{
		public void TestExecuteCommand()
		{
			var commandExecuted = false;
			var command = new DelegateCommand(() => commandExecuted = true);
			command.Execute(null);

			Assert(commandExecuted);
		}

		public void TestCanExecuteCommand()
		{
			var canExecuteCalled = false;
			var commandExecuted = false;
			var command = new DelegateCommand(() => commandExecuted = true);
			command.CanExecuteChanged += (sender, args) => canExecuteCalled = true;
			command.CanExecuteCommand = false;
			command.Execute(null);

			Assert(canExecuteCalled);
			Assert(!commandExecuted);
		}

		public void TestExecuteCommandWithParam()
		{
			string param = null;
			var command = new DelegateCommandWithParam((p) => param = (string)p);
			command.Execute("some value");

			AssertEquals("some value", param);
		}

		public void TestCanExecuteCommandWithParam()
		{
			var canExecuteCalled = false;
			var commandExecuted = false;
			var command = new DelegateCommandWithParam((p) => commandExecuted = true);
			command.CanExecuteChanged += (sender, args) => canExecuteCalled = true;
			command.CanExecuteCommand = false;
			command.Execute(10);

			Assert(canExecuteCalled);
			Assert(!commandExecuted);
		}
	}
}

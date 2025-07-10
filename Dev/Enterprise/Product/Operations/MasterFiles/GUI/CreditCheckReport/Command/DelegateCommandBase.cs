using System;
using System.Windows.Input;
using CargoWise.Common;

namespace Enterprise.MasterFiles.GUI
{
	public abstract class DelegateCommandBase : ICommand
	{
		bool canExecuteCommand;

		protected DelegateCommandBase()
		{
			canExecuteCommand = true;
		}

		public bool CanExecuteCommand
		{
			get => canExecuteCommand;
			set
			{
				canExecuteCommand = value;
				CanExecuteChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public bool CanExecute(object parameter)
		{
			return CanExecuteCommand;
		}

		public void Execute(object parameter)
		{
			if (CanExecuteCommand)
			{
				ExecuteCore(parameter);
			}
		}

		protected abstract void ExecuteCore(object parameter);

		public event EventHandler CanExecuteChanged;
	}

	public class DelegateCommand : DelegateCommandBase
	{
		readonly Action commandAction;

		public DelegateCommand(Action commandAction)
		{
			Argument.NotNull(commandAction, nameof(commandAction));
			this.commandAction = commandAction;
		}

		protected override void ExecuteCore(object parameter)
		{
			commandAction();
		}
	}

	public class DelegateCommandWithParam : DelegateCommandBase
	{
		readonly Action<object> commandAction;

		public DelegateCommandWithParam(Action<object> commandAction)
		{
			Argument.NotNull(commandAction, nameof(commandAction));
			this.commandAction = commandAction;
		}

		protected override void ExecuteCore(object parameter)
		{
			commandAction(parameter);
		}
	}
}

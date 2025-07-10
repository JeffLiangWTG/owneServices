using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using WinzorFramework;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;

public class WinFormsWinzorTestAttribute : Attribute, IWrapTestMethod
{
	public TestCommand Wrap(TestCommand command)
	{
		return new WinFormsWinzorTestCommand(command);
	}

	class WinFormsWinzorTestCommand : DelegatingTestCommand
	{
		public WinFormsWinzorTestCommand(TestCommand innerCommand)
			: base(innerCommand)
		{
		}

		[SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "Implementing non-async interface")]
		public override TestResult Execute(TestExecutionContext testExecutionContext)
		{
			TestResult result = null;
			using var testContext = new EnterpriseTestContext();
			using var dispatcherContext = new CargoWiseTestWinzorDispatcherContext(testContext);
			EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
			{
				using (WinzorDispatcher.Current.WithContext(dispatcherContext))
				{
					result = innerCommand.Execute(testExecutionContext);
				}
			}).GetAwaiter().GetResult();
			dispatcherContext.WaitForAllRenderTasks();
			return result;
		}
	}
}

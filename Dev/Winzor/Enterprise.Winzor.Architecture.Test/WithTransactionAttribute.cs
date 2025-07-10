using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using WinzorFramework;

namespace Enterprise.Winzor.Architecture.Test;

public class WithTransactionAttribute : Attribute, IWrapTestMethod
{
	public TestCommand Wrap(TestCommand command)
	{
		return new WithTransactionTestCommand(command);
	}

	class WithTransactionTestCommand : DelegatingTestCommand
	{
		public WithTransactionTestCommand(TestCommand innerCommand)
			: base(innerCommand)
		{
		}

		[SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "Implementing non-async interface")]
		public override TestResult Execute(TestExecutionContext context)
		{
			var testCase = new TransactionedTestCase(context, innerCommand);
			var ec = ExecutionContext.Capture();
			EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
			{
				ExecutionContext.Restore(ec);
				testCase.RunBare();
			}).GetAwaiter().GetResult();
			return testCase.TestResult;
		}
	}

	[NUnit.Framework.DoNotAddToTestTree]
	class TransactionedTestCase : NUnit.Framework.TransactionedTestCase
	{
		public TransactionedTestCase(TestExecutionContext context, TestCommand testCommand)
		{
			this.context = context;
			this.testCommand = testCommand;
			Name = nameof(TestPlaceholderForNeedingAGuiTest);
		}

		[SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "Running task from non-async API")]
		protected override void RunTest()
		{
			var cts = new CancellationTokenSource();
			var task = Task.Run(() =>
			{
				try
				{
					TestResult = testCommand.Execute(context);
				}
				finally
				{
					cts.Cancel();
				}
			});
			WinzorDispatcher.Current.RunMessageLoop(cts);
			task.GetAwaiter().GetResult();
		}

		public TestResult TestResult { get; private set; }

		public void TestPlaceholderForNeedingAGuiTest()
		{
		}

		readonly TestExecutionContext context;
		readonly TestCommand testCommand;
	}
}

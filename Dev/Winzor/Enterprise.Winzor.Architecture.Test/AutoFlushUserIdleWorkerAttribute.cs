using System;
using System.Diagnostics.CodeAnalysis;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

[assembly: Enterprise.Winzor.Architecture.Test.AutoFlushUserIdleWorker]

namespace Enterprise.Winzor.Architecture.Test;

public sealed class AutoFlushUserIdleWorkerAttribute : Attribute, IApplyToContext
{
	public void ApplyToContext(TestExecutionContext context)
	{
		context.UpstreamActions.Add(new AutoFlushUserIdleWorkerTestAction());
	}

	class AutoFlushUserIdleWorkerTestAction : ITestAction
	{
		public ActionTargets Targets => ActionTargets.Test;

		[SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "Implementing non-async interface")]
		public void AfterTest(NUnit.Framework.Interfaces.ITest test)
		{
			EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(UserIdleWorker.Flush).GetAwaiter().GetResult();
		}

		public void BeforeTest(NUnit.Framework.Interfaces.ITest test)
		{
		}
	}
}

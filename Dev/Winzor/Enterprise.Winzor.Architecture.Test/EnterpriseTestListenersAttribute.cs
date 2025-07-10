using System;
using System.Linq;
using CargoWise.Common.Testing.MemoryManagement;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

[assembly: Enterprise.Winzor.Architecture.Test.EnterpriseTestListeners]

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits

namespace Enterprise.Winzor.Architecture.Test;

public sealed class EnterpriseTestListenersAttribute : Attribute, IApplyToContext
{
	public void ApplyToContext(TestExecutionContext context)
	{
		context.UpstreamActions.Insert(0, new TestAction(this));
		context.UpstreamActions.Insert(0, new SuiteAction(this));
	}

	void BeforeAllTests()
	{
		EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			if (testListeners == null)
			{
				testListeners = UnitTestListenersFactory.GetTestListeners().Where(IncludeTestListener).ToArray();

				foreach (var testListener in testListeners)
				{
					testListener.StartAllTests(DateTime.Now);
				}
			}
		}).GetAwaiter().GetResult();
	}

	void BeforeTest(NUnit.Framework.Interfaces.ITest test)
	{
		EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			foreach (var testListener in testListeners)
			{
				testListener.BeforeEachTest(DateTime.Now);
			}
			foreach (var testListener in testListeners)
			{
				testListener.StartTest(null, DateTime.Now);
			}
		}).GetAwaiter().GetResult();
	}

	void AfterAllTests()
	{
		EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			foreach (var testListener in testListeners)
			{
				testListener.EndAllTests(DateTime.Now);
			}
		}).GetAwaiter().GetResult();
	}

	void AfterTest(NUnit.Framework.Interfaces.ITest test)
	{
		EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			foreach (var testListener in testListeners)
			{
				testListener.EndTest(null, DateTime.Now);
			}
			foreach (var testListener in testListeners)
			{
				testListener.AfterEachTest(DateTime.Now);
			}
		}).GetAwaiter().GetResult();
	}

	bool IncludeTestListener(NUnit.Framework.ITestListener testListener)
	{
		return !(testListener is TaskTestListener)
			&& !(testListener is TestLeakListener)
			&& !(testListener is ClientOverrideInitUninitTestListener);
	}

	NUnit.Framework.ITestListener[] testListeners;

	class SuiteAction : ITestAction
	{
		public SuiteAction(EnterpriseTestListenersAttribute attribute)
		{
			this.attribute = attribute;
		}

		public ActionTargets Targets => ActionTargets.Suite;

		public void BeforeTest(NUnit.Framework.Interfaces.ITest test) => attribute.BeforeAllTests();

		public void AfterTest(NUnit.Framework.Interfaces.ITest test) => attribute.AfterAllTests();

		readonly EnterpriseTestListenersAttribute attribute;
	}

	class TestAction : ITestAction
	{
		public TestAction(EnterpriseTestListenersAttribute attribute)
		{
			this.attribute = attribute;
		}

		public ActionTargets Targets => ActionTargets.Test;

		public void BeforeTest(NUnit.Framework.Interfaces.ITest test) => attribute.BeforeTest(test);

		public void AfterTest(NUnit.Framework.Interfaces.ITest test) => attribute.AfterTest(test);

		readonly EnterpriseTestListenersAttribute attribute;
	}
}

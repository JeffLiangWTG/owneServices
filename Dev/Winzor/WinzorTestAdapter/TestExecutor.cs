using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Blazor.Common;
using CWNUnit.TestAdapter;
using Enterprise.Dat.Implementation;
using Enterprise.Testing;
using Enterprise.Winzor.Architecture;
using Enterprise.Winzor.Architecture.Test;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;
using NUnitTestResult = NUnit.Framework.TestResult;
using TestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;
using TestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace WinzorTestAdapter;

[ExtensionUri(ExecutorUriString)]
public class TestExecutor : ITestExecutor
{
	public const string ExecutorUriString = "executor://winzor_testexecutor";

	[SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "SetupCargoWise is setting up BaseSourcePath")]
	public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
	{
		InitializeTestingState();
		SetupCargoWise();
		SetupTestRunner();

		try
		{
			var runExplicit = tests.All(t => t.IsExplicit());
			TestSuite testSuite = null;
			foreach (var test in tests)
			{
				if (isCancelled || (test.IsExplicit() && !runExplicit))
				{
					var result = new TestResult(test) { Outcome = TestOutcome.Skipped };
					frameworkHandle.RecordResult(result);
					continue;
				}

				var outcome = TestOutcome.None;
				frameworkHandle.RecordStart(test);
				var stopwatch = Stopwatch.StartNew();
				try
				{
					var testDescriptor = TestUtilities.TestCaseToTestDescriptor(test);
					var assemblyToTest = Assembly.Load(testDescriptor.Identifier.ScopeName);
					var typeToTest = assemblyToTest.GetType(testDescriptor.Identifier.ElementName, true);
					if (testSuite?.TestClass != typeToTest)
					{
						testSuite = new TestSuite(typeToTest);
					}

					InvokeWinzorDispatcher(cwTestResult.BeforeTestInstantiated);
					DoRunTest(testSuite, testDescriptor.Identifier.TargetName);
					InvokeWinzorDispatcher(cwTestResult.AfterTestSetToNull);
					var tr = testRunnerTestListener.GetTestResult(testDescriptor);

					if (testErrorListener.Errors is { Count: > 0 })
					{
						outcome = TestOutcome.Failed;
						var result = new TestResult(test) { Outcome = outcome, Duration = stopwatch.Elapsed };
						var exception = TestUtilities.AggregateExceptions(new TargetInvocationException(testErrorListener.Errors.First()), testErrorListener.Errors);
						TestUtilities.ExceptionToTestResult(exception, result);
						if (tr.FailureDetails is { Length: > 0 })
						{
							result.ErrorMessage = string.Join("\r\n", tr.FailureDetails);
						}
						frameworkHandle.RecordResult(result);
					}
					else
					{
						outcome = TestOutcome.Passed;
						var result = new TestResult(test) { Outcome = outcome, Duration = stopwatch.Elapsed };
						frameworkHandle.RecordResult(result);
					}
				}
				catch (Exception ex)
				{
					outcome = TestOutcome.Failed;
					var result = new TestResult(test) { Duration = stopwatch.Elapsed };
					var errorException = TestUtilities.AggregateExceptions(ex, testErrorListener.Errors);
					TestUtilities.ExceptionToTestResult(errorException, result);
					frameworkHandle.RecordResult(result);
				}
				finally
				{
					frameworkHandle.RecordEnd(test, outcome);
				}
			}
		}
		finally
		{
			InvokeWinzorDispatcher(() => { ((IDisposable)cwTestResult)?.Dispose(); });
		}
	}

	void DoRunTest(TestSuite testSuite, string testName)
	{
		using var winzorTestContext = new EnterpriseTestContext();
		using var dispatcherContext = new CargoWiseTestWinzorDispatcherContext(winzorTestContext);

		InvokeWinzorDispatcher(() =>
		{
			var test = testSuite.NewTest(testName);
			try
			{
				using var context = EnterpriseTestSetup.WinzorDispatcher.WithContext(dispatcherContext);
				test.Run(cwTestResult);
			}
			catch (Exception ex)
			{
				((ITestListener)testErrorListener).AddError(ex, test);
			}
		});

		dispatcherContext.WaitForAllRenderTasks();
	}

	void SetupTestRunner()
	{
		InvokeWinzorDispatcher(() =>
		{
			var testListeners = new List<ITestListener>();
			testErrorListener = new TestErrorListener();
			testRunnerTestListener = new TestRunnerTestListener(new UnitTestErrorDescriptionListFactory());
			testListeners.Add(testErrorListener);
			testListeners.Add(testRunnerTestListener);
			testListeners.AddRange(UnitTestListenersFactory.GetTestListeners());
			cwTestResult = new NUnitTestResult(testListeners.ToArray());
			SnailTestAttribute.IncludeSnailTests = true;
		});
	}

	TestErrorListener testErrorListener;
	NUnitTestResult cwTestResult;
	TestRunnerTestListener testRunnerTestListener;

	[SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "SetupCargoWise is setting up BaseSourcePath")]
	static void SetupCargoWise()
	{
		if (isSetup)
		{
			return;
		}

		isSetup = true;
		var cargoWiseAuthStateProvider = new CargoWiseAuthStateProvider(NullLogger<CargoWiseAuthStateProvider>.Instance, It.IsAny<ITokenValidatorWrapper>());
		var serviceProvider = InitializationConfigurationHelper.ConfigTestServices(cargoWiseAuthStateProvider, null);
		Initialization.ConfigureCargoWise(
			EnterpriseTestSetup.WinzorDispatcher,
			serviceProvider.GetRequiredService<IWinzorCargoWiseLoginHandler>(),
			serviceProvider.GetRequiredService<UserMonitorRegistry>());

		InvokeWinzorDispatcher(() =>
		{
			var datSourcePath = Environment.GetEnvironmentVariable("DAT_TestSourcePath");
			if (!string.IsNullOrEmpty(datSourcePath))
			{
				if (!Path.EndsInDirectorySeparator(datSourcePath))
				{
					datSourcePath += Path.DirectorySeparatorChar;
				}
				NUnit.Framework.TestCase.BaseSourcePath = datSourcePath;
			}

			var datSupplementaryContentPath = Environment.GetEnvironmentVariable("DAT_TestSupplementaryContentPath");
			if (!string.IsNullOrEmpty(datSupplementaryContentPath))
			{
				NUnit.Framework.TestCase.SetSupplementaryContentPath(datSupplementaryContentPath);
			}
		});
	}

	static void InitializeTestingState()
	{
		Globals.IsUserInteractive = true;
		TestingState.IsRunningTests = true;
		TestingState.IsRunningOnDAT = Initialization.DatIsTesting;
	}

	static bool isSetup;

	static void InvokeWinzorDispatcher(Action action)
	{
#pragma warning disable VSTHRD002
		EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(action).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
	}

	#region RunTests given sources

	[SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "SetupCargoWise is setting up BaseSourcePath")]
	public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
	{
		var sink = new TestCaseDiscoverySink();
		new TestDiscoverer().DiscoverTests(sources, null, null, sink);
		RunTests(sink.TestCases, runContext, frameworkHandle);
	}

	class TestCaseDiscoverySink : ITestCaseDiscoverySink
	{
		public void SendTestCase(TestCase discoveredTest)
		{
			TestCases.Add(discoveredTest);
		}

		public readonly List<TestCase> TestCases = [];
	}

	#endregion

	bool isCancelled;

	public void Cancel()
	{
		isCancelled = true;
	}

	static TestExecutor()
	{
		AssemblyResolver.Setup();
	}
}

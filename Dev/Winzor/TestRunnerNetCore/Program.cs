using System;
using CargoWise.Definitions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using WinzorTestAdapter;

var filename = args[args.Length - 3];
var typeName = args[args.Length - 2];
var methodName = args[args.Length - 1];

var testCase = new TestCase(typeName + "." + methodName, new Uri(TestExecutor.ExecutorUriString), filename);
var testExecutor = new TestExecutor();
var frameworkHandle = new FrameworkHandle();
testExecutor.RunTests(new[] { testCase }, null, frameworkHandle);

Environment.Exit(frameworkHandle.Failed ? ExitCodes.TestRunnerInAnotherProcessFailedExitCode : ExitCodes.Success);

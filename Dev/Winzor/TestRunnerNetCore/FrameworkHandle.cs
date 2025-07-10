using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

public class FrameworkHandle : IFrameworkHandle
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Baseline")]
	public void RecordResult(TestResult testResult)
	{
		if (testResult.Outcome == TestOutcome.Failed)
		{
			Console.WriteLine(testResult.ErrorMessage);
			Console.WriteLine(testResult.ErrorStackTrace);
			Failed = true;
		}
	}

	public bool Failed { get; private set; }

	public bool EnableShutdownAfterTestRun { get; set; }

	public int LaunchProcessWithDebuggerAttached(string filePath, string workingDirectory, string arguments, IDictionary<string, string> environmentVariables)
	{
		throw new NotImplementedException();
	}

	public void RecordAttachments(IList<AttachmentSet> attachmentSets)
	{
	}

	public void RecordEnd(TestCase testCase, TestOutcome outcome)
	{
	}

	public void RecordStart(TestCase testCase)
	{
	}

	public void SendMessage(TestMessageLevel testMessageLevel, string message)
	{
	}
}

using System;
using System.Diagnostics.CodeAnalysis;

namespace Workflow.CodeGeneration
{
	[SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Console programs should have console output")]
	[SuppressMessage("CargoWiseOne", "CW1161:ResSGetStringAnalyzer", Justification = "Non-customer facing strings.")]
	class Program
	{
		static void Main(string[] args)
		{
			WorkflowDescriptorGenerator.Generate();
			Console.WriteLine("CodeGeneration: Complete.");
		}
	}
}

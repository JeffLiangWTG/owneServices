using System;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.ZAReferenceData.CmdLine
{
	public class ConsoleLogger : ILogger
	{
		public void LogError(string message)
		{
			Console.Error.WriteLine(message);
		}

		public void LogInfo(string message)
		{
			Console.WriteLine(message);
		}
	}
}

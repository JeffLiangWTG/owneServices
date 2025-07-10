using System;
using System.Diagnostics;
using System.Globalization;
using CargoWise.RefDbRepo.LLIReferenceData.Business.Vessel;
using CargoWise.RefDbRepo.LLIReferenceData.Services;
using CargoWise.RefDbRepo.LLIReferenceData.Services.Vessel;

namespace CargoWise.RefDbRepo.LLIReferenceData.CmdLine;

public static class VesselListProgram
{
	static Action<string[]> Action = args =>
	{
		var apiUserName = args.Length != 0 ? args[0] : ApplicationConfig.VesselApiUserName;
		var apiPassword = args.Length > 1 ? args[1] : ApplicationConfig.VesselApiPassword;
		var parallelRequests = args.Length > 2 ? int.Parse(args[2], CultureInfo.InvariantCulture) : ApplicationConfig.MaxParallelHttpRequests;

		var startTime = Stopwatch.GetTimestamp();
		var vesselsData = VesselListRetriever.GetVesselsDataAsync(apiUserName, apiPassword, parallelRequests).GetAwaiter().GetResult();
		Console.WriteLine($"{vesselsData.Vessels.Count} vessel records are fetched in {Stopwatch.GetElapsedTime(startTime)}");

		startTime = Stopwatch.GetTimestamp();
		VesselParser.ParseAndExport(vesselsData, ApplicationConfig.OutputPath, DateTime.Now);
		Console.WriteLine($"Parse and export comleted in {Stopwatch.GetElapsedTime(startTime)}");
		Console.WriteLine($"Output file is saved to {ApplicationConfig.OutputPath}");
	};

	public static IDisposable SetAction(Action<string[]> action)
	{
		var resetter = new ActionResestter();
		Action = action;
		return resetter;
	}

	public static void Run(string[] args)
	{
		Action(args);
	}

	class ActionResestter : IDisposable
	{
		readonly Action<string[]> originalAction = Action;

		public void Dispose()
		{
			Action = originalAction;
		}
	}
}

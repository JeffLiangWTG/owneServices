using System;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.ZAReferenceData.CmdLine
{
	public abstract class Runner
	{
		protected Runner(ILogger logger)
		{
			Logger = logger;
		}
		protected ILogger Logger { get; private set; }

		public void Run()
		{
			RunCore();
		}

		protected abstract void RunCore();

		public static Runner Create(string function)
		{
			var logger = new ConsoleLogger();

			switch (function.ToUpperInvariant())
			{
				case Constants.ProgramFunctions.Tariffs:
					return new TariffRunner(logger);
				case Constants.ProgramFunctions.Carriers:
					return new CarrierRunner(logger);
				case Constants.ProgramFunctions.ExchangeRates:
					return new ExchangeRateRunner(logger);
				default:
					throw new ArgumentException(Invariant($"Function not supported: {function}"));
			}
		}
	}
}

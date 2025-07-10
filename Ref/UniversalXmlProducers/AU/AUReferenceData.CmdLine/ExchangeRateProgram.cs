using System;
using System.Globalization;
using System.Threading;
using CargoWise.RefDbRepo.AUReferenceData.Business.ExchangeRate;

namespace CargoWise.RefDbRepo.AUReferenceData.CmdLine
{
	public class ExchangeRateProgram
	{
		public void Run(string outputPath)
		{
			var parser = GetParser();

			var attempts = 0;
			string errorMessage;
			while (attempts < MaxAttempts)
			{
				if (attempts > 0)
				{
					var delay = RetryDelay;
					errorMessage = $"ExchangeRate Parser will run again in {(int)(delay / 1000)} seconds.\r\n";
					Console.Error.WriteLine(errorMessage);
					Thread.Sleep(delay);
				}

				parser.Parse(outputPath);
				if (!parser.HasErrorNotification)
				{
					Console.Out.WriteLine("ExchangeRate Parser completed successfully.");
					break;
				}

				attempts++;
				errorMessage = $"#{attempts} ExchangeRate Parser encountered the following errors:\r\n{parser.GetErrorNotification()}";
				Console.Error.WriteLine(errorMessage);
			}
		}

		protected virtual XCHAGRATEParser GetParser()
		{
			return new XCHAGRATEParser();
		}

		protected virtual int RetryDelay => Convert.ToInt32(Services.ApplicationConfig.ExchangeRateRetryDelay ?? "300000", NumberFormatInfo.InvariantInfo); // 5 minutes
		protected virtual int MaxAttempts => Convert.ToInt32(Services.ApplicationConfig.ExchangeRateMaxAttempts ?? "3", NumberFormatInfo.InvariantInfo);
	}
}

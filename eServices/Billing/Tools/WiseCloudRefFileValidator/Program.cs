using System;
using System.Linq;
using CargoWise.eServices.Billing.Collector.Misc.WindowsService.Plugins.WiseCloudSQLAccess;
using Microsoft.Extensions.Logging;
using WTG.ErrorReporting;

namespace CargoWise.eServices.Billing.WiseCloudRefFileValidator
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			if (!args.Any())
			{
				Console.Write("Please provide a file to validate.");
				return;
			}

			var result = ValidateCsv(args[0]);

			if (result.Contains("\u0011"))
			{
				var lines = result.Split('\u0011');
				foreach (var line in lines)
				{
					var defaultColor = Console.ForegroundColor;
					if (line.StartsWith("\u0012"))
					{
						Console.ForegroundColor = ConsoleColor.White;
						Console.Write("INFO: ");
						WriteToConsoleWithColorizedCustomerID(line.Replace("\u0012", string.Empty), defaultColor);
					}
					else if (line.StartsWith("\u0013"))
					{
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.Write("WARNING: ");
						WriteToConsoleWithColorizedCustomerID(line.Replace("\u0013", string.Empty), defaultColor);
					}
					else if (line.StartsWith("\u0014"))
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.Write("ERROR: ");
						WriteToConsoleWithColorizedCustomerID(line.Replace("\u0014", string.Empty), defaultColor);
					}
				}

			}
			else
			{
				Console.Write(result ?? "Validation completed with no errors.");
			}
		}

		static void WriteToConsoleWithColorizedCustomerID(string line, ConsoleColor defaultColor)
		{
			var parts = line.Split('{', '}');

			if (parts.Length == 3)
			{
				Console.ForegroundColor = defaultColor;
				Console.Write(parts[0]);

				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write(parts[1]);

				Console.ForegroundColor = defaultColor;
				Console.WriteLine(parts[2]);
			}
			else
			{
				Console.ForegroundColor = defaultColor;
				Console.WriteLine(line);
			}
		}

		internal static string ValidateCsv(string path)
		{
			try
			{
				var logger = new StringLogger();
				WiseCloudSQLAccessReferenceFileProcessor.ProcessReferenceFile(path, new ErrorReportingClient(new Uri("", UriKind.Relative)), logger);
				return logger.Logged;
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}
	}
}

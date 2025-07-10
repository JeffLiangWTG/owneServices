using System;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.IFTRINMessageProcessor
{
	static class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 1)
			{
				var country = args[0];
				if (!string.IsNullOrWhiteSpace(country))
				{
					var outputPath = ApplicationConfig.OutputPath;
					using (var stagingRepo = new StagingRepository(ApplicationConfig.ConnectionStrings))
					{
						IFTRINMessageProcessor processor = null;
						switch (country)
						{
							case DataSourceConstants.Country.Singapore:
								processor = new SGIFTRINMessageProcessor(stagingRepo, outputPath);
								break;
						}
						if (processor == null)
						{
							Console.Error.WriteLine($"There is no IFTRIN Processor support for '{country}'.");
						}
						else
						{
							processor.Process();
						}
					}
				}
				else
				{
					Console.Error.WriteLine($"Invalid argument entered: {country}");
				}
			}
			else
			{
				Console.Error.WriteLine("Missing or invalid arguments; please specify the country to run against.");
			}
		}
	}
}

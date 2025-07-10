using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.SEReferenceData.CodeLists.Business;

namespace CargoWise.RefDbRepo.SEReferenceData.CmdLine
{
	class CodeListsProgram
	{
		public static void Run(string[] args, string outputFilePath)
		{
			var (functionsToExclude, continueProcessing) = ValidateArguments(args);
			if (continueProcessing)
			{
				var dateTimeProvider = new DateTimeProvider();
				var functionsToRun = GetFunctionsToRun(outputFilePath, dateTimeProvider);

				if (functionsToExclude.Any())
				{
					foreach (var functionToExclude in functionsToExclude)
					{
						functionsToRun.Remove(functionToExclude);
					}
				}

				Parallel.Invoke(functionsToRun.Select(x => x.Value).ToArray());
			}
		}

		static Dictionary<string, Action> GetFunctionsToRun(string outputFilePath, DateTimeProvider dateTimeProvider) => new();

		static (IEnumerable<string> FunctionsToExclude, bool ValidArguments) ValidateArguments(string[] args)
		{
			var validArguments = true;
			var codeListsToExclude = Enumerable.Empty<string>();
			var validCodeLists = CodeListConstants.ValidCodeLists;
			var numberOfArguments = args.Length;

			if (numberOfArguments == 1 && args[0].ToUpper(CultureInfo.InvariantCulture).StartsWith("-EXCLUDE:", StringComparison.InvariantCulture))
			{
				codeListsToExclude = args[0].Remove(0, 9).Split(',');
				var invalidCodeListsToExclude = codeListsToExclude.Where(function => !validCodeLists.Contains(function));
				if (invalidCodeListsToExclude.Any())
				{
					Console.Error.WriteLine($"Invalid CodeList Types to exclude entered: {string.Join(",", invalidCodeListsToExclude)}");
					validArguments = false;
				}
			}
			else if (numberOfArguments > 0)
			{
				Console.Error.WriteLine($"Invalid arguments entered -EXCLUDE:[functions]. Valid Code List Types are {string.Join(",", validCodeLists)}");
				validArguments = false;
			}

			return (codeListsToExclude, validArguments);
		}
	}
}

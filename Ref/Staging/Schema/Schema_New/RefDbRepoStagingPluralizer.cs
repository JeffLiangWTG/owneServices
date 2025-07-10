using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Humanizer;
using Microsoft.EntityFrameworkCore.Design;

[assembly: InternalsVisibleTo("CargoWise.RefDbRepo.Staging.Schema_New.Test")]
namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	class RefDbRepoStagingPluralizer : IPluralizer
	{
		public string Pluralize(string identifier)
		{
			if (identifier.StartsWith("QRTZ_", StringComparison.OrdinalIgnoreCase))
			{
				return identifier;
			}
			return identifier.Pluralize(inputIsKnownToBeSingular: false);
		}

		public string Singularize(string identifier)
		{
			var wordsToKeepPlural = new List<string> { "Data", "Quota", "States" };
			if (wordsToKeepPlural.Any(word => identifier.EndsWith(word, StringComparison.OrdinalIgnoreCase)) || identifier.StartsWith("QRTZ_", StringComparison.OrdinalIgnoreCase))
			{
				return identifier;
			}
			return identifier.Singularize(inputIsKnownToBePlural: false);
		}
	}
}

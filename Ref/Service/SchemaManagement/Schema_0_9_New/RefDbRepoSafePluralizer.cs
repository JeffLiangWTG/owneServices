using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Humanizer;
using Microsoft.EntityFrameworkCore.Design;

[assembly: InternalsVisibleTo("CargoWise.RefDbRepo.Service.SchemaManagement.Test")]
namespace CargoWise.RefDbRepo.Service.Schema_0_9_New
{
	class RefDbRepoSafePluralizer : IPluralizer
	{
		public string Pluralize(string identifier)
		{
			return identifier.Pluralize(inputIsKnownToBeSingular: false);
		}

		public string Singularize(string identifier)
		{
			var wordsToKeepPlural = new List<string> { "Data", "Quota", "States" };
			if (wordsToKeepPlural.Any(word => identifier.EndsWith(word, StringComparison.OrdinalIgnoreCase)))
			{
				return identifier;
			}
			return identifier.Singularize(inputIsKnownToBePlural: false);
		}
	}
}

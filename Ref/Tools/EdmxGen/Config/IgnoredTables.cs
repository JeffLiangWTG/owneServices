using System.Collections.Generic;

namespace CargoWise.RefDbRepo.EdmxGen
{
	public static class IgnoredTables
	{
		public static string[] Safe => new string[]
		{
			"sysdiagrams",
		};

		public static string[] Staging => new string[]
		{
			"SystemData"
		};

		public static string[] StagingAssociations => new string[]
		{
		};
	}
}

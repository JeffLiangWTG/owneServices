using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using WTG.DevTools.Common;

namespace CargoWise.RefDbRepo.Tools.Common
{
	public static class GitFactory
	{
		public static string GetLocalPathFromServerMappingString(string serverMapPath)
		{
			Argument.NotNullOrEmpty(serverMapPath, nameof(serverMapPath));

			var result = string.Empty;
			GitRepositoryEntry[] gitEntries;
			using (var gitRepositoryDatabase = new GitRepositoryDatabase())
			{
				gitEntries = gitRepositoryDatabase.FindRepositories(serverMapPath)?.ToArray();
			}
			if (gitEntries.Length == 1)
			{
				result = gitEntries[0]?.LocalGitPath;
			}
			else if (gitEntries.Length > 1)
			{
				var selected = -1;
				do
				{
					Console.WriteLine("Please select one repo:");
					for (var i = 0; i < gitEntries.Length; i++)
					{
						Console.WriteLine($"{i} : {gitEntries[i]?.LocalGitPath}");
					}
					Console.Write("Your choice is: ");
					_ = int.TryParse(Console.ReadLine(), out selected);
				}
				while (selected < 0 || selected >= gitEntries.Length);
				result = gitEntries[selected]?.LocalGitPath;
			}
			return !string.IsNullOrEmpty(result) ? Path.GetDirectoryName(result) : string.Empty;
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.T4Runner
{
	public static class TTSearch
	{
		readonly static string[] TTDirectories = new string[]
		{
			@"..\..\..\Service\SafeDataUpdateService\NewSafeDataUpdateService\Controllers",
			@"..\..\..\Service\NewService\Controllers",
			@"..\..\..\Staging\Service\NewService\Controllers",
			@"..\..\..\Common\Infrastructure\SafeDataClient"
		};

		public static IEnumerable<string> FindTTDirectories()
		{
			var directories = new List<string>();

			directories.AddRange(TTDirectories);

			foreach (var file in GetSharedTTFiles())
			{
				var fileDir = Path.GetDirectoryName(file);
				if (!directories.Contains(fileDir))
				{
					directories.Add(Path.GetDirectoryName(file));
				}
			}

			return directories;
		}

		public static string[] GetTTFiles(string directory)
		{
			Argument.NotNullOrEmpty(directory, nameof(directory));
			var ttFiles = Directory.GetFiles(directory, "*.tt", SearchOption.TopDirectoryOnly);
			return ttFiles;
		}

		static string[] GetSharedTTFiles()
		{
			return Directory.GetFiles(Application.SharedRefDataCommonPath, "*.tt", SearchOption.AllDirectories);
		}
	}
}

using System;
using System.IO;
using System.IO.Compression;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public static class SqlScriptHelper
	{
		public static string GetSqlScriptFromZippedFile(string zipFilePath, string subfolder, string objectName)
		{
			var localDirectory = Path.GetDirectoryName(new Uri(MigrationHelper.GetExecutingAssemblyLocation()).LocalPath);
			var zipPath = Path.Combine(localDirectory, zipFilePath);
			using (var archive = ZipFile.OpenRead(zipPath))
			{
				var zipEntry = archive.GetEntry($"{subfolder}\\{objectName}.sql");
				if (zipEntry == null)
				{
					throw new FileNotFoundException("Cannot find file " + objectName);
				}
				else
				{
					using (var stream = zipEntry.Open())
					{
						using (var sr = new StreamReader(stream))
						{
							var sqlScript = sr.ReadToEnd();
							return sqlScript;
						}
					}
				}
			}
		}
	}
}

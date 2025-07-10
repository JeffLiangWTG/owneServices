using System;
using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.Common.DbUpgrade
{
	// design smell, do not know where this function belongs ?
	// Common here should only for Client-Server, Common between Server and Staging should be in different solution
	public static class FolderHelper
	{
		public static string GetBinFolder()
		{
			var codeBase = Assembly.GetExecutingAssembly().Location;
			var uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			return Path.GetDirectoryName(path);
		}
	}
}

using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public static class FolderHelper
	{
		public static string GetBinFolder() => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	}
}

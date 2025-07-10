using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.MXRefLocoMap.Business
{
	public static class Utilities
	{
		public static string CurrentFolder()
		{
			var location = Assembly.GetExecutingAssembly().Location;
			return Path.GetDirectoryName(location);
		}
	}
}

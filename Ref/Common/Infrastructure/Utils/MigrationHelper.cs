using System.Reflection;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public static class MigrationHelper
	{
		public static string GetExecutingAssemblyLocation()
		{
			return Assembly.GetExecutingAssembly().Location;
		}
	}
}

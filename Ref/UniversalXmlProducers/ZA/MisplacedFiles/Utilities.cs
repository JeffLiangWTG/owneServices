using System.IO;
using System.Reflection;

namespace ZAReferenceData
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

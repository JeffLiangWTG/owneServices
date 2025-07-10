using System;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.ApplicationConfigurationGenerator
{
	class Program
	{
		static async Task Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine($"Incorrect number of args passed.");
				return;
			}
			var rootFolder = args[0];
			var saveTo = args[1];
			if (rootFolder.Length == 0 || saveTo.Length == 0)
			{
				return;
			}

			var applicationConfigurationGenerator = new ApplicationConfigurationGenerator(rootFolder, saveTo);
			await applicationConfigurationGenerator.Run();
		}
	}
}

using System;
using System.Linq;

namespace CargoWise.RefDbRepo.ProducerStatusCodeListGenerator
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Any() && !string.IsNullOrEmpty(args[0]))
			{
				var saveTo = args[0];
				var producerStatusCodeListGenerator = new ProducerStatusCodeListGenerator(saveTo);
				producerStatusCodeListGenerator.Run();
			}
			else
			{
				Console.WriteLine("Incorrect args passed.");
			}
		}
	}
}

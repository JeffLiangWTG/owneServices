using System;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.JPReferenceData.CmdLine
{
	class Program
	{
		static int Main(string[] args)
		{
			Run(args);
			return (int)ProducerStatus.Success;
		}

		static void Run(string[] args)
		{
			if (args.Length != 0)
			{
				var argument = args[0].ToUpperInvariant();
				var programType = typeof(NaccsProgram).GetNestedTypes()
					.Union(new[] { typeof(ExchangeRateProgram) })
					.FirstOrDefault(c => c.GetCustomAttribute<StartupArgumentAttribute>().Argument.Equals(argument, StringComparison.OrdinalIgnoreCase));

				if (programType != null)
				{
					var method = programType.GetMethod(nameof(Run), BindingFlags.Static | BindingFlags.Public);
					method.Invoke(programType, null);
				}
				else
				{
					throw new ArgumentException($"Invalid argument entered: {argument}");
				}
			}
			else
			{
				throw new ArgumentException("No arguments entered.");
			}
		}
	}
}

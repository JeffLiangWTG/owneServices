using System;
using System.Globalization;
using CargoWise.RefDbRepo.XmlProducer.Common;
using Constants = CargoWise.RefDbRepo.STLBillingCollector.Business.Constants;

namespace CargoWise.RefDbRepo.STLBillingCollector.CmdLine
{
	class Program
	{
		static int Main(string[] args)
		{
			ProduceXml(args);
			return (int)ProducerStatus.Success;
		}
		static void ProduceXml(string[] args)
		{
			if (args.Length != 0)
			{
				var functionToRun = args[0];
				switch (functionToRun)
				{
					case Constants.ProgramFunctions.STLCollector:
						STLCollectorProgram.Run();
						break;
					default:
						throw new ArgumentException($"Invalid argument entered: {functionToRun}");
				}
			}
			else
			{
				throw new ArgumentException("No arguments entered.");
			}
		}
	}
}

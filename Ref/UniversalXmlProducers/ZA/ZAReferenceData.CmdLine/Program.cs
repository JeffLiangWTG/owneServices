using System;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.ZAReferenceData.CmdLine
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
			if (args.Length > 0)
			{
				Runner.Create(args[0]).Run();
			}
			else
			{
				throw new ArgumentException("No arguments entered.");
			}
		}
	}
}

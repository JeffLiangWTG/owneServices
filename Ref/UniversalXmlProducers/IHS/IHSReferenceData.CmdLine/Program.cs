using System;
using System.Linq;
using CargoWise.RefDbRepo.IHSReferenceData.Business;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.IHSReferenceData.CmdLine
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
				var func = args[0].ToUpperInvariant();

				switch (func)
				{
					case Constants.ProgramFunctions.IHSVessel:
						{
							VesselListProgram.Run(args.Skip(1).ToArray());
							break;
						}

					default:
						{
							throw new ArgumentException($"Invalid argument entered: {func}");
						}
				}
			}
			else
			{
				throw new ArgumentException("No arguments entered.");
			}
		}
	}
}

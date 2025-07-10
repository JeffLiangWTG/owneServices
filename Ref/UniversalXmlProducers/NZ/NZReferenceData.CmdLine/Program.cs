using System;
using System.Globalization;
using CargoWise.RefDbRepo.NZReferenceData.Business;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.NZReferenceData.CmdLine
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
				var func = args[0].ToUpper(CultureInfo.InvariantCulture);

				switch (func)
				{
					case Constants.ProgramFunctions.NZVessel:
						{
							new VesselListProgram().Run();
							break;
						}

					case Constants.ProgramFunctions.NZSupplier:
						{
							new SupplierListProgram().Run();
							break;
						}

					case Constants.ProgramFunctions.NZTariff:
						{
							new TariffProgram().Run();
							break;
						}

					case Constants.ProgramFunctions.NZConcession:
						{
							new ConcessionProgram().Run();
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

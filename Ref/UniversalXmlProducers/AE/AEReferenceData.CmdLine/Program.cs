using System;
using System.Globalization;
using CargoWise.RefDbRepo.AEReferenceData.Business;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.AEReferenceData.CmdLine;

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
			var functionToRun = args[0].ToUpper(CultureInfo.InvariantCulture);
			switch (functionToRun)
			{
				case Constants.ProgramFunctions.DubaiDecRefData:
					DubaiRefDataExcelRunner.Run();
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

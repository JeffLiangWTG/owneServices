using System.Collections.Generic;
using System.IO;
using System.Net;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Schema;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater
{
	public class IATAUpdater
	{
		public List<IATA> IATAs { get; private set; }

		public IATAUpdater()
		{
			IATAs = new List<IATA>();
		}

		public void Read(string filePath)
		{
			using (var file = File.OpenRead(filePath))
			using (var reader = new StreamReader(file))
			{
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine();
					ReadIATALine(line);
				}
			}
		}

		void ReadIATALine(string line)
		{
			Argument.NotNull(line, nameof(line));

			var splittedData = line.Split('\t');

			if (splittedData[9] == "A" || splittedData[9] == "H" || splittedData[9] == "C")
			{
				IATAs.Add(SplitToIATA(splittedData));
			}
		}

		static IATA SplitToIATA(string[] splittedData)
		{
			Argument.NotNull(splittedData, nameof(splittedData));

			return new IATA()
			{
				IATACode = splittedData[0],
				IATACityCode = splittedData[1],
				PortName = splittedData[2],
				CityName = splittedData[3],
				StateName = splittedData[4],
				CountryName = splittedData[5],
				LatitudeCoordinates = splittedData[6],
				LongitudeCoordinates = splittedData[7],
				TimeZoneName = splittedData[8],
				FunctionType = splittedData[9]
			};
		}
	}
}

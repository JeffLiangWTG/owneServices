using System;
using System.Xml;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class CMRSeaImpendingArrivalsParser : BaseCMRReferenceDataParser
	{
		protected override string FileNamePrefix => ApplicationConfig.CMRSeaImpendingArrivalsFilePrefix;

		protected override string OutputXMLName => "RefVesselZZ_AU_CMRSeaImpendingArrivals.xml";

		protected override string DataSource => "AU CMR SEAIMPAR";

		protected override IXmlWriterConfiguration BuildXMLConfiguration(DateTime publishedDate) => CMRXMLWriterConfigurationBuilder.BuildCMRSeaImpendingArrivalsConfiguration();

		static PropertyMapping<RefVesselZZ>[] VesselZZMappings => new PropertyMapping<RefVesselZZ>[]
		{
			new PropertyMapping<RefVesselZZ>(entity => entity.ZZO_Code, 57, 35),
			new PropertyMapping<RefVesselZZ>(entity => entity.ZZO_LloydsNumber, 41, 8),
			new PropertyMapping<RefVesselZZ>(entity => entity.ZZO_RadioCallSign, 184, 1), // using this property to store the Withdrawn Indicator
		};

		static PropertyMapping<RefVesselArrival>[] VesselArrivalMappings => new PropertyMapping<RefVesselArrival>[]
		{
			new PropertyMapping<RefVesselArrival>(entity => entity.ZYA_VoyageNumber, 50, 6),
			new PropertyMapping<RefVesselArrival>(entity => entity.ZYA_ArrivalDate, 22, 12),
			new PropertyMapping<RefVesselArrival>(entity => entity.ZYA_ArrivalPort, 35, 5),
		};

		LineToEntityConverter<RefVesselZZ> VesselZZConverter => vesselZZConverter ??= new LineToEntityConverter<RefVesselZZ>(VesselZZMappings, 1);
		LineToEntityConverter<RefVesselZZ> vesselZZConverter;

		LineToEntityConverter<RefVesselArrival> VesselArrivalConverter => vesselArrivalConverter ??= new LineToEntityConverter<RefVesselArrival>(VesselArrivalMappings, 1);
		LineToEntityConverter<RefVesselArrival> vesselArrivalConverter;

		protected override void ParseCore(IXmlWriter xmlWriter, string content)
		{
			foreach (var line in content.NonEmptyLines())
			{
				var code = ProcessLine(line);
				if (code != null)
				{
					xmlWriter.PopulateData(code);
				}
			}
		}

		protected RefVesselZZ ProcessLine(string line)
		{
			RefVesselZZ vessel = null;
			vessel = VesselZZConverter.Convert(line);
			var vesselArrival = VesselArrivalConverter.Convert(line);
			if (vessel.ZZO_RadioCallSign != "W")
			{
				if (vessel.ZZO_Code != null && vesselArrival.ZYA_VoyageNumber != null)
				{
					vessel.RefVesselArrivals = [vesselArrival];
				}
				else
				{
					Console.Error.WriteLine(InsufficientInfoErrorMessage, line);
					vessel = null;
				}
			}
			else
			{
				Console.Error.WriteLine(CodeHasExpiredErrorMessage, line);
				vessel = null;
			}

			return vessel;
		}
	}
}

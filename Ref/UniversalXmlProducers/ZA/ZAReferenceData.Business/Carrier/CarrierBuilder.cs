using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Carriers;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.ZAReferenceData.Business.Carrier
{
	public class CarrierBuilder : XmlBuilder<CarrierVesselData, RefCarrierCode>
	{
		public CarrierBuilder(CarrierVesselData sourceData, ILogger logger) : base(sourceData, logger)
		{
		}

		protected override string DataSource => "ZACarriers";

		protected override string FilePrefix => "ZA_RefCarrierCode";

		protected override UpdateType UpdateType => UpdateType.Full;

		protected override XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var writerConfig = new XmlWriterConfiguration();

			var carrierConfig = new EntityTypeConfiguration<RefCarrierCode>(true);
			carrierConfig.IncludeColumn(x => x.ZZ4_Code, true);
			carrierConfig.IncludeColumn(x => x.ZZ4_Description);
			carrierConfig.IncludeColumnWithConstantValue(x => x.ZZ4_ZZZ_NKDataGrouping, true, Constants.ZADataGrouping);
			carrierConfig.IncludeColumnWithDefaultValue(x => x.ZZ4_IsSea, false, false);
			carrierConfig.IncludeColumnWithDefaultValue(x => x.ZZ4_IsAir, false, false);
			carrierConfig.IncludeColumn(x => x.RefCarrierCodeAttributes);
			carrierConfig.IncludeColumn(x => x.RefCarrierVesselPivots);
			writerConfig.IncludeEntityTypeConfiguration(carrierConfig);

			var carrierAttribConfig = new EntityTypeConfiguration<RefCarrierCodeAttribute>(true);
			carrierAttribConfig.IncludeColumn(x => x.ZZG_Name, true);
			carrierAttribConfig.IncludeColumn(x => x.ZZG_Value, true);
			writerConfig.IncludeEntityTypeConfiguration(carrierAttribConfig);

			var pivCarVes = new EntityTypeConfiguration<RefCarrierVesselPivot>(true);
			pivCarVes.IncludeColumnWithConstantValue(x => x.ZZQ_ZZO_ZZZ_NKDataGrouping, true, Constants.ZADataGrouping);
			pivCarVes.IncludeColumn(x => x.ZZQ_ZZO_NKCode, true);
			pivCarVes.IncludeColumn(x => x.ZZQ_ZZO_NKRadioCallSign, true);
			writerConfig.IncludeEntityTypeConfiguration(pivCarVes);

			return writerConfig;
		}

		protected override List<RefCarrierCode> ConvertToRefModels()
		{
			var refCarriers = new List<RefCarrierCode>();

			MergeVesselCarriers(sourceData);

			foreach (var carrier in sourceData.Data)
			{
				var vessels = sourceData.Vessels.Where(x => x.Carrier.CarrierCode == carrier.CarrierCode).ToList();

				foreach (var vessel in vessels)
				{
					carrier.IsAir |= vessel.Carrier.IsAir;
					carrier.IsSea |= vessel.Carrier.IsSea;
					carrier.MergeAttributes(vessel.Carrier.Attributes);
				}

				var newCarrier = new RefCarrierCode
				{
					ZZ4_Code = carrier.CarrierCode,
					ZZ4_Description = carrier.CarrierName,
					ZZ4_IsAir = carrier.IsAir,
					ZZ4_IsSea = carrier.IsSea,
					RefCarrierCodeAttributes = carrier.Attributes.Select(x => new RefCarrierCodeAttribute { ZZG_Name = x, ZZG_Value = x}).ToArray(),
					RefCarrierVesselPivots = vessels.Select(x => new RefCarrierVesselPivot
					{
						ZZQ_ZZO_NKRadioCallSign = SubstringSafe(x.RadioCallSign ?? string.Empty, 10),
						ZZQ_ZZO_NKCode = SubstringSafe(x.VesselName ?? string.Empty, 35)
					}).ToArray()
				};

				refCarriers.Add(newCarrier);
			}

			return refCarriers;
		}

		protected override IEnumerable<Dependency> GetDependencies(DateTime publicationDateTime)
		{
			if (sourceData.Vessels.Any())
			{
				yield return new Dependency(VesselBuilder.XmlDataSource, publicationDateTime, DependencyType.Preferred);
			}
		}

		static void MergeVesselCarriers(CarrierVesselData source)
		{
			foreach (var vessel in source.Vessels)
			{
				if (!source.Data.Any(x => x.CarrierCode == vessel.Carrier.CarrierCode))
				{
					source.Data.Add(vessel.Carrier);
				}
			}
		}
	}
}

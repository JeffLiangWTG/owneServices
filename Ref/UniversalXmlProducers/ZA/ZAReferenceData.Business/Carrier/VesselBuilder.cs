using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Carriers;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.ZAReferenceData.Business.Carrier
{
	public class VesselBuilder : XmlBuilder<BaseData<VesselData>, RefVesselZZ>
	{
		public VesselBuilder(BaseData<VesselData> sourceData, ILogger logger) : base(sourceData, logger)
		{
		}

		protected override string DataSource => XmlDataSource;

		protected override string FilePrefix => "ZA_RefVesselZZ";

		protected override UpdateType UpdateType => UpdateType.Full;

		protected override XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var writerConfig = new XmlWriterConfiguration();

			var vesselConfig = new EntityTypeConfiguration<RefVesselZZ>(true);
			vesselConfig.IncludeColumn(x => x.ZZO_Code, true);
			vesselConfig.IncludeColumn(x => x.ZZO_RadioCallSign, true);
			vesselConfig.IncludeColumnWithConstantValue(x => x.ZZO_ZZZ_NKDataGrouping, true, Constants.ZADataGrouping);
			vesselConfig.IncludeColumnWithConstantValue(x => x.ZZO_VesselType, false, "CV");
			writerConfig.IncludeEntityTypeConfiguration(vesselConfig);

			return writerConfig;
		}

		protected override List<RefVesselZZ> ConvertToRefModels()
		{
			var refVessels = new List<RefVesselZZ>();

			foreach(var vessel in sourceData.Data)
			{
				refVessels.Add(new RefVesselZZ
				{
					ZZO_RadioCallSign = SubstringSafe(vessel.RadioCallSign ?? string.Empty, 10),
					ZZO_Code = SubstringSafe(vessel.VesselName ?? string.Empty, 35),
				});
			}

			return refVessels;
		}

		public static string XmlDataSource => "ZAVessels";
	}
}

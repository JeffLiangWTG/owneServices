using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.TimeZoneParser
{
	public class TimezoneXmlProducer : XmlProducer<RefTimeZoneSet>
	{
		public TimezoneXmlProducer(bool fullTimezoneSetSchema)
		{
			XmlWriter = new XmlWriter(GetWriterConfiguration(fullTimezoneSetSchema));
		}
		public override string FilePath => ApplicationConfig.TimeZoneXMLFile;
		public override string DataSource => ApplicationConfig.TimeZoneDataSource;

		public override void ExportToXmlInBatch(IEnumerable<RefTimeZoneSet> collection, DateTime publishTime, string filePath = null)
		{
			var dataSource = $"{DataSource}";
			InitializeWriter(publishTime, dataSource);

			ExportToXml(collection, filePath ?? FilePath);
		}

		static IXmlWriterConfiguration GetWriterConfiguration(bool fullTimezoneSetSchema)
		{
			var timezoneSetConfiguration = new EntityTypeConfiguration<RefTimeZoneSet>(true);
			timezoneSetConfiguration.IncludeColumn(x => x.R3_TimeZoneSetName, true);
			timezoneSetConfiguration.IncludeColumn(x => x.R3_IsActive, false);
			timezoneSetConfiguration.IncludeColumn(x => x.RefTimeZones, false);

			var timezoneConfiguration = new EntityTypeConfiguration<RefTimeZone>(true);
			timezoneConfiguration.IncludeColumn(x => x.R2_OffsetMinutesFromUTC, false);
			timezoneConfiguration.IncludeColumn(x => x.R2_CivilianTimeZoneCode, false);
			timezoneConfiguration.IncludeColumn(x => x.R2_Type, true);
			timezoneConfiguration.IncludeColumn(x => x.RefTimeZoneRules, false);

			var timezoneRuleConfiguration = new EntityTypeConfiguration<RefTimeZoneRule>(true);
			timezoneRuleConfiguration.IncludeColumn(x => x.R4_DaylightSavingDate, false);
			timezoneRuleConfiguration.IncludeColumn(x => x.R4_DaylightSavingDayWeekDate, false);
			timezoneRuleConfiguration.IncludeColumn(x => x.R4_DaylightSavingDayCount, false);
			timezoneRuleConfiguration.IncludeColumn(x => x.R4_DaylightSavingMonth, fullTimezoneSetSchema);
			timezoneRuleConfiguration.IncludeColumn(x => x.R4_DaylightSavingDayName, false);
			timezoneRuleConfiguration.IncludeColumn(x => x.R4_FromYear, true);
			timezoneRuleConfiguration.IncludeColumn(x => x.R4_StartOrEndRule, true);
			timezoneRuleConfiguration.IncludeColumn(x => x.R4_ToYear, false);
			timezoneRuleConfiguration.IncludeColumnWithDefaultValue(x => x.R4_TypeOfTime, false, "LOC");

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(timezoneSetConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(timezoneConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(timezoneRuleConfiguration);

			return writerConfiguration;
		}
	}
}

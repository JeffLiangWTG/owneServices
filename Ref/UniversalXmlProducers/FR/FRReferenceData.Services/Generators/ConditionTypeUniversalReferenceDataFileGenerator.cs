using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class ConditionTypeUniversalReferenceDataFileGenerator : BaseUniversalReferenceDataFileGenerator<RefCusConditionType>
	{
		public override string OutputFile => ApplicationConfig.Instance.FRConditionTypesOutputFile;

		protected override List<RefCusConditionType> GetDataCollection(DateTime publicationDate, ref Errors error) 
		{
			var provider = new RITADataProvider(new RITADataDownloader());
			return provider.GetFRConditionTypes();
		}

		public override void ManageExceptionIfNoResult() => UniversalDataHelper.SendEmail($"No {DataSource} information was found.", "RITA web site didn't return any result for condition types. You can manually check for Experts/Donnéees de référence/Type de measures on RITA web site.");

		protected override XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			var conditionType = new EntityTypeConfiguration<RefCusConditionType>(true);
			conditionType.IncludeColumnWithConstantValue(x => x.ZX2_ConditionClass, false, UniversalDataHelper.Constants.Control);
			conditionType.IncludeColumn(x => x.ZX2_ConditionType, true);
			conditionType.IncludeColumn(x => x.ZX2_Description, false);
			conditionType.IncludeColumnWithConstantValue(x => x.ZX2_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(conditionType);

			return xmlWriterConfiguration;
		}

		public override string DataSource => "FR - Condition Types";

		protected override UpdateType updateType => UpdateType.Partial;
	}
}

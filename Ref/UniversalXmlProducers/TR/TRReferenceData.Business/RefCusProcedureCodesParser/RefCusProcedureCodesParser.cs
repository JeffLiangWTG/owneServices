using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;

namespace CargoWise.RefDbRepo.TRReferenceData.Business.RefCusProcedureCodesParser
{
	public class RefCusProcedureCodesParser : ReferenceDataParser
	{
		readonly string dataFileName;

		public RefCusProcedureCodesParser(string dataFileName)
		{
			this.dataFileName = Argument.NotNull(dataFileName, nameof(dataFileName));
		}

		protected override string DataSource => "TR RefCusProcedure codes";

		protected override DateTime PublicationDateTime => new DateTime(2023, 01, 01, 00, 00, 00);

		protected override RefDataRepoModelEntityType[] GetEntities()
		{
			var entities = RefCusProcedureCodesLoader.LoadData(dataFileName);

			foreach (var entity in entities)
			{
				if (entity.RefCusProcedureAttributes != null)
				{
					entity.RefCusProcedureAttributes = entity.RefCusProcedureAttributes
						.Where(attr => !(string.IsNullOrWhiteSpace(attr.ZXB_Name) && string.IsNullOrWhiteSpace(attr.ZXB_Value)))
						.ToArray();
				}
			}
			return entities.ToArray();
		}

		protected override XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var refCusProcedureConfiguration = new EntityTypeConfiguration<RefCusProcedure>(true);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_Category, true);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_ProcedureCode, true);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_PreviousProcedureCode, true);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_Concession, true);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_Description, false);
			refCusProcedureConfiguration.IncludeColumnWithConstantValue(x => x.ZZ6_ZZZ_NKDataGrouping, true,Constants.CountryCodeTR);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_ShipmentType, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_CalculateDuty, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_Group, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_LandedCost, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_IntoWarehouse, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_OutOfWarehouse, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_StartDate, false);
			refCusProcedureConfiguration.IncludeColumnWithConstantValue(x => x.ZZ6_EndDate, false, Constants.MaximumSmallDateTime);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_IntoTemporaryImport, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_OutOfTemporaryImport, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_IntoTemporaryExport, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_OutOfTemporaryExport, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_IntoInwardProcessing, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_OutOfInwardProcessing, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_IntoOutwardProcessing, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_OutofOutwardProcessing, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_CalculateVAT, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_IsGuaranteeConsumed, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_IsGuaranteeReleased, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_IsTransit, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_IntoVATWarehouse, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_OutOfVATWarehouse, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.RefCusProcedureLanguages);
			refCusProcedureConfiguration.IncludeColumn(x => x.RefCusProcedureAttributes);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusProcedureConfiguration);

			var refCusProcedureLanguageConfiguration = new EntityTypeConfiguration<RefCusProcedureLanguage>(true);
			refCusProcedureLanguageConfiguration.IncludeColumn(x => x.ZXV_ZX6_NKLanguage,true);
			refCusProcedureLanguageConfiguration.IncludeColumn(x => x.ZXV_Description);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusProcedureLanguageConfiguration);

			var refCusProcedureAttributeConfiguration = new EntityTypeConfiguration<RefCusProcedureAttribute>(true);
			refCusProcedureAttributeConfiguration.IncludeColumn(x => x.ZXB_Name, true);
			refCusProcedureAttributeConfiguration.IncludeColumn(x => x.ZXB_Value);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusProcedureAttributeConfiguration);

			return writerConfiguration;
		}
	}
}

using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public static class CodeListHelper
	{
		public static XmlWriterConfiguration GetRefCusProcedureWriterConfiguration(string shipmentType)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var refCusProcedureConfiguration = new EntityTypeConfiguration<RefCusProcedure>(true);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_ProcedureCode, true);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_PreviousProcedureCode, true);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_Concession, true);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_Description, false);
			refCusProcedureConfiguration.IncludeColumnWithConstantValue(x => x.ZZ6_ShipmentType, false, shipmentType);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_Group, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_CalculateDuty, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_LandedCost, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_IntoWarehouse, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_OutOfWarehouse, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_CalculateVAT, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_StartDate, false);
			refCusProcedureConfiguration.IncludeColumn(x => x.ZZ6_EndDate, false);
			refCusProcedureConfiguration.IncludeColumnWithConstantValue(x => x.ZZ6_ZZZ_NKDataGrouping, true, "ES");
			writerConfiguration.IncludeEntityTypeConfiguration(refCusProcedureConfiguration);
			return writerConfiguration;
		}
	}
}

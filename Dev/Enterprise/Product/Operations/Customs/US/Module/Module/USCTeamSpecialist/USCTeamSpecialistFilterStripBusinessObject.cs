using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	public class USCTeamSpecialistFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter("District/Port Code", USCTeamSpecialistSchema.UJ_DistrictPortCode);
			result.AddTextFilter("Tariff From", USCTeamSpecialistSchema.UJ_TariffNumberFrom);
			result.AddTextFilter("Tariff To", USCTeamSpecialistSchema.UJ_TariffNumberTo);
			result.AddTextFilter("Country Code (From)", USCTeamSpecialistSchema.UJ_CountryFrom);
			result.AddTextFilter("Country Code (To)", USCTeamSpecialistSchema.UJ_CountryTo);
			result.AddTextFilter("Team Number", USCTeamSpecialistSchema.UJ_FieldImportSpecialistTeamNumber);
			result.AddTextFilter("Importer Name", USCTeamSpecialistSchema.UJ_ImporterOfRecordName);

			return result;
		}
	}
}

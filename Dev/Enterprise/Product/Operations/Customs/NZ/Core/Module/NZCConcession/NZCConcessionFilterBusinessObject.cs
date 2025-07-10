using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Module
{
	public class NZCConcessionFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			filters.AddNumberFilter("Concession Code", NZCConcessionSchema.U2_Code)
				.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.NZCConcessionFilterBusinessObject|ConcessionCode", "Concession Code");
			filters.AddNumberFilter("Description", NZCConcessionSchema.U2_Description)
				.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.NZCConcessionFilterBusinessObject|Description", "Description");
			filters.AddNumberFilter("Tariff Heading", GetTariffPortionQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(NZCConcessionClassificationLinkSchema.U3_TariffPortion)
				.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.NZCConcessionFilterBusinessObject|TariffHeading", "Tariff Heading");

			filters.AddDateFilter("Validity", NZCConcessionSchema.U2_DateActiveFrom)
				.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.NZCConcessionFilterBusinessObject|Validity", "Validity");

			return filters;
		}

		ZQuery GetTariffPortionQuery(SQLComparisonOperator @operator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(NZCConcession));
			ZDBOnlySubQuery linkSubQuery = new ZDBOnlySubQuery(typeof(NZCConcessionClassificationLink), NZCConcessionClassificationLinkSchema.U3_U2_Concession);
			linkSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, NZCConcessionClassificationLinkSchema.U3_TariffPortion, @operator, value);
			query.AddSubQuery(linkSubQuery, JoinCondition.And);
			return query;
		}
	}
}

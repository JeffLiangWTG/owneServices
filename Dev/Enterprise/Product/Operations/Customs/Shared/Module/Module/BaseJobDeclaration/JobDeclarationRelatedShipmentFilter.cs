using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	class JobDeclarationRelatedShipmentFilter : ModuleGuidFilter
	{
		public JobDeclarationRelatedShipmentFilter(BusinessObjectFactory factory)
			: base(DeclarationFilterConstants.RelatedShipment, ModuleIDs.JobShipment, JobShipmentSchema.PK, new ForwardingShipmentCollection(factory))
		{
		}

		protected override ZQuery GetQuery()
		{
			return GetQueryForSelectedFiltersFromLayout();
		}

		protected override void AddSelectedFiltersSubquery(FilterStripBusinessObject filterBusinessObject, ZDBOnlyQuery query, ZDBOnlySubQuery subQuery)
		{
			query.AddSubQuery(JobDeclarationSchema.JE_JS, subQuery, JoinCondition.And);
			query.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_JS, null);
		}
	}
}

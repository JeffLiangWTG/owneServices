using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefTimeZoneEndRuleCollection : RefTimeZoneRuleCollection
	{
		public RefTimeZoneEndRuleCollection(DaylightSavingTimeZone parent) : base(parent)
		{
		}

		protected override void SetDefaultsForNewElementCore(RefTimeZoneRule newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.R4_StartOrEndRule = RefTimeZoneRule.EndRuleCode;
			ZQuery filter = ((IBusinessObjectCollection)this).CompleteFilter;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			query.AddToFilter(RefTimeZoneRuleSchema.R4_StartOrEndRule, RefTimeZoneRule.EndRuleCode);
			return query;
		}
	}
}

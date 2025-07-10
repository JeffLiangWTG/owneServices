using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefTimeZoneStartRuleCollection : RefTimeZoneRuleCollection
	{
		public RefTimeZoneStartRuleCollection(DaylightSavingTimeZone parent) : base(parent)
		{
		}

		protected override void SetDefaultsForNewElementCore(RefTimeZoneRule newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.R4_StartOrEndRule = RefTimeZoneRule.StartRuleCode;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			query.AddToFilter(RefTimeZoneRuleSchema.R4_StartOrEndRule, RefTimeZoneRule.StartRuleCode);
			return query;
		}
	}
}

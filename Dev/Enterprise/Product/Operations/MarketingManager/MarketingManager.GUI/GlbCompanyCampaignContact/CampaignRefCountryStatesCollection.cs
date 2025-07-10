using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	[ZArchitecture.ComponentModel.ModuleID(ModuleId.RefCountryStates)]
	public class CampaignRefCountryStatesCollection : ActiveBusinessObjectCollection<CampaignRefCountryStates>, IActiveBusinessObjectCollection
	{
		public CampaignRefCountryStatesCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CampaignRefCountryStatesCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		void IActiveBusinessObjectCollection.SetDefaultsForNewElement(BusinessObject newElement)
		{
		}

		protected override IFindBoxListProvider FindBoxListProvider => new CampaignRefCountryStatesCollectionProvider(this);
	}

	class CampaignRefCountryStatesCollectionProvider : FindBoxListProvider
	{
		public CampaignRefCountryStatesCollectionProvider(CampaignRefCountryStatesCollection collection)
			: base(collection)
		{
		}

		protected override void AddDescriptionEqualsFilter(ZQuery query, string description)
		{
			query.AddToFilter(JoinCondition.And, RefCountryStatesSchema.RW_Description, SQLComparisonOperator.Equal, description);
		}

		protected override void AddDescriptionStartsWithFilter(ZQuery query, string description)
		{
			query.AddToFilter(JoinCondition.And, RefCountryStatesSchema.RW_Description, SQLComparisonOperator.StartsWith, description);
			query.OrderBy = RefCountryStatesSchema.Constants.RW_Description;
		}
	}
}

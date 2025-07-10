using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CustomsNumberStmNumberRangeCollection : ActiveBusinessObjectCollection<CustomsNumberStmNumberRange>
	{
		public CustomsNumberStmNumberRangeCollection(CustomsNumberViewStmNumsBusinessProvider provider)
			: base(provider.Factory, GetRelationshipFilter(provider))
		{
			Provider = provider;
		}

		public readonly CustomsNumberViewStmNumsBusinessProvider Provider;

		static ZQuery GetRelationshipFilter(CustomsNumberViewStmNumsBusinessProvider provider)
		{
			var ownerFilter = new ZQuery(StmNumberRangeSchema.SNR_Owner, provider.GetOwnerPKs());
			var nameFilter = new ZQuery(StmNumberRangeSchema.SNR_Name, SQLComparisonOperator.StartsWith, CustomsNumberViewStmNums.GenerateNamePrefix(provider.ProviderKey));
			nameFilter.AddToFilter(ownerFilter, JoinCondition.And);
			return nameFilter;
		}

		#region Implementation

		protected override bool AllowNew
		{
			get { return false; }
		}

		protected override bool MatchesFilterCore(CustomsNumberStmNumberRange element, bool fetchOnlyFromLocalCache)
		{
			return Provider.MatchesFilter(element.SNR_Owner);
		}

		protected override void SetDefaultsForNewElementCore(CustomsNumberStmNumberRange newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.Provider = this.Provider;
		}

		protected override void OnAdded(CustomsNumberStmNumberRange businessObject)
		{
			base.OnAdded(businessObject);
			businessObject.Provider = this.Provider;
		}

		protected override void OnLoadedIntoCollectionCore(CustomsNumberStmNumberRange loadedObject)
		{
			base.OnLoadedIntoCollectionCore(loadedObject);
			loadedObject.Provider = this.Provider;
		}

		#endregion
	}
}

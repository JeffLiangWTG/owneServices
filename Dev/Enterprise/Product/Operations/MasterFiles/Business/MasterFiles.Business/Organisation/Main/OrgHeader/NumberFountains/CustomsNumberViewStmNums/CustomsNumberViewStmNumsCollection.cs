using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CustomsNumberViewStmNumsCollection : ActiveBusinessObjectCollection<CustomsNumberViewStmNums>
	{
		public CustomsNumberViewStmNumsCollection(CustomsNumberViewStmNumsBusinessProvider provider)
			: base(provider.Factory, GetRelationshipFilter(provider))
		{
			Provider = provider;
		}

		public readonly CustomsNumberViewStmNumsBusinessProvider Provider;

		static ZQuery GetRelationshipFilter(CustomsNumberViewStmNumsBusinessProvider provider)
		{
			var ownerFilter = new ZQuery(ViewStmNumsSchema.SN_Owner, provider.GetOwnerPKs());
			var nameFilter = new ZQuery(ViewStmNumsSchema.SN_Name, SQLComparisonOperator.StartsWith, CustomsNumberViewStmNums.GenerateNamePrefix(provider.ProviderKey));
			nameFilter.AddToFilter(ownerFilter, JoinCondition.And);
			return nameFilter;
		}

		#region Implementation

		protected override bool AllowNew
		{
			get { return false; }
		}

		protected override void OnLoadingIntoCollectionCore(CustomsNumberViewStmNums loadingObject)
		{
			loadingObject.Provider = Provider;
			base.OnLoadingIntoCollectionCore(loadingObject);
		}

		protected override void OnAddIntoRelationshipCore(BusinessObject businessObject)
		{
			base.OnAddIntoRelationshipCore(businessObject);
			var stmNum = ((CustomsNumberViewStmNums)businessObject);
			stmNum.Provider = Provider;
			var name = stmNum.GenerateName();
			if (stmNum.SN_Name != name)
			{
				stmNum.SN_Name = name;
			}
		}

		protected override void SetDefaultsForNewElementCore(CustomsNumberViewStmNums newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.Provider = Provider;
			newElement.SN_Owner = Provider.Parent.PK;
		}

		protected override bool MatchesFilterCore(CustomsNumberViewStmNums element, bool fetchOnlyFromLocalCache)
		{
			return Provider.MatchesFilter(element.SN_Owner);
		}

		#endregion
	}
}

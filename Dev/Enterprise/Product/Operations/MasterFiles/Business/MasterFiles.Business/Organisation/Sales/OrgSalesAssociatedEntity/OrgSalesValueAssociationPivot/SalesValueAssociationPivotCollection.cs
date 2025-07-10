using CargoWise.EntityFramework;
using Enterprise.Integration.Rating;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class SalesValueAssociationPivotCollection : ActiveBusinessObjectCollection<OrgSalesValueAssociationPivot>
	{
		#region Constructor

		public SalesValueAssociationPivotCollection(ISalesValue salesValue, bool onlyIncludeForCurrentCompany)
			: base(salesValue.Factory, GetRelationship(salesValue, onlyIncludeForCurrentCompany))
		{
			this.salesValue = salesValue;
			this.onlyIncludeForCurrentCompany = onlyIncludeForCurrentCompany;
		}

		protected SalesValueAssociationPivotCollection(ISalesValue salesValue, BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
			this.salesValue = salesValue;
		}

		static ICollectionRelationship GetRelationship(ISalesValue salesValue, bool onlyIncludeForCurrentCompany)
		{
			if (onlyIncludeForCurrentCompany)
			{
				return new CompanySalesValueAssociationPivotCollectionRelationship(salesValue);
			}
			else
			{
				return new GlobalSalesValueAssociationPivotCollectionRelationship(salesValue);
			}
		}

		readonly bool onlyIncludeForCurrentCompany;

		#endregion

		readonly ISalesValue salesValue;

		#region Collection State

		protected override object[] GetCollectionState()
		{
			return new object[] { onlyIncludeForCurrentCompany };
		}

		#endregion

		#region Defaults

		protected override void SetDefaultsForNewElementCore(OrgSalesValueAssociationPivot newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.SVP_TradeId = salesValue.Identifier;
			newElement.SVP_TradeTableCode = salesValue.TablePrefix;
		}

		#endregion

		#region Add

		public OrgSalesValueAssociationPivot AddNew(ISalesValueAssociatedEntity associatedEntity)
		{
			var result = AddNew();
			result.AssociatedEntity = associatedEntity;

			return result;
		}

		#endregion

		#region Allowed Actions

		protected override bool AllowNew
		{
			get { return false; }
		}

		#endregion
	}

	public class GlobalSalesValueAssociationPivotCollectionRelationship : CollectionRelationship
	{
		public GlobalSalesValueAssociationPivotCollectionRelationship(ISalesValue salesValue)
			: base(typeof(OrgSalesValueAssociationPivot), GetFilter(salesValue))
		{
		}

		protected GlobalSalesValueAssociationPivotCollectionRelationship(ZQuery filter)
			: base(typeof(OrgSalesValueAssociationPivot), filter)
		{
		}

		static ZQuery GetFilter(ISalesValue salesValue)
		{
			var query = new ZQuery();
			query.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_TradeId, salesValue.Identifier);
			query.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_TradeTableCode, salesValue.TablePrefix);
			return query;
		}
	}

	public class CompanySalesValueAssociationPivotCollectionRelationship : CollectionRelationship
	{
		public CompanySalesValueAssociationPivotCollectionRelationship(ISalesValue salesValue)
			: base(typeof(OrgSalesValueAssociationPivot), GetFilter(salesValue))
		{
			this.salesValue = salesValue;
		}

		protected CompanySalesValueAssociationPivotCollectionRelationship(ISalesValue salesValue, ZQuery filter)
			: base(typeof(OrgSalesValueAssociationPivot), filter)
		{
			this.salesValue = salesValue;
		}

		readonly ISalesValue salesValue;

		static ZQuery GetFilter(ISalesValue salesValue)
		{
			var query = new ZDBOnlyQuery(typeof(OrgSalesValueAssociationPivot));

			query.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_TradeId, salesValue.Identifier);
			query.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_TradeTableCode, salesValue.TablePrefix);

			var rateEntryType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(RateEntrySchema.Constants.Prefix);
			var rateEntryCurrentCompanySubquery = new ZDBOnlySubQuery(rateEntryType, OrgSalesValueAssociationPivotSchema.SVP_ActivityId);
			var rateHeaderCurrentCompanySubquery = new ZDBOnlySubQuery(typeof(IRatingHeader), RateEntrySchema.TI_TH);
			rateHeaderCurrentCompanySubquery.AddToFilter(RatingHeaderSchema.TH_GC, GlbCompany.CurrentCompany.PK);
			rateEntryCurrentCompanySubquery.AddSubQuery(rateHeaderCurrentCompanySubquery, JoinCondition.And);

			var rateCurrentCompanyFilter = new ZDBOnlyQuery(typeof(OrgSalesValueAssociationPivot));
			rateCurrentCompanyFilter.AddToFilter(new ZQuery(OrgSalesValueAssociationPivotSchema.SVP_ActivityTableCode, SQLComparisonOperator.NotEqual, RateEntrySchema.Constants.Prefix));
			rateCurrentCompanyFilter.AddSubQuery(rateEntryCurrentCompanySubquery, JoinCondition.Or);
			query.AddToFilter(rateCurrentCompanyFilter);

			return query;
		}

		protected override bool MatchesRelationshipFilterCore(BusinessObject businessObject, bool ignoreActiveFilter, bool fetchOnlyFromLocalCache)
		{
			if (businessObject.IsInDatabase)
			{
				return base.MatchesRelationshipFilterCore(businessObject, ignoreActiveFilter, fetchOnlyFromLocalCache);
			}
			else
			{
				var pivot = (OrgSalesValueAssociationPivot)businessObject;
				return
					pivot.SVP_TradeId == salesValue.Identifier
					&& pivot.SVP_TradeTableCode == salesValue.TablePrefix;
			}
		}
	}
}

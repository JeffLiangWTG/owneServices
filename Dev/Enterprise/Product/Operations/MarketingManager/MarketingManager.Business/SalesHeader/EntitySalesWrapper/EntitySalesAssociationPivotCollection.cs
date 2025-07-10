using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class EntitySalesAssociationPivotCollection : SalesValueAssociationPivotCollection
	{
		public EntitySalesAssociationPivotCollection(EntitySalesWrapper entitySales)
			: base(entitySales, entitySales.Factory, new EntitySalesAssociationPivotCollectionRelationship(entitySales))
		{
			this.entitySales = entitySales;
		}
		readonly EntitySalesWrapper entitySales;

		protected override bool MatchesFilterCore(OrgSalesValueAssociationPivot element, bool fetchOnlyFromLocalCache)
		{
			bool result;
			if (element.IsInDatabase)
			{
				result = base.MatchesFilterCore(element, fetchOnlyFromLocalCache);
			}
			else
			{
				result = element.SVP_TradeTableCode == OrgSalesSchema.Constants.Prefix
					&& element.SVP_TradeId == entitySales.PK;
			}

			if (result)
			{
				var companyPk = entitySales.CompanyFilter;
				var entity = element.AssociatedEntity;
				if (companyPk.IsEmpty || entity == null)
				{
					return true;
				}
				else
				{
					return entity.CompanyPk == null || entity.CompanyPk == companyPk;
				}
			}

			return result;
		}

		class EntitySalesAssociationPivotCollectionRelationship : CollectionRelationship
		{
			public EntitySalesAssociationPivotCollectionRelationship(EntitySalesWrapper entitySales)
				: base(typeof(OrgSalesValueAssociationPivot), GetFilter(entitySales))
			{
				this.entitySales = entitySales;
			}
			readonly EntitySalesWrapper entitySales;

			static ZQuery GetFilter(EntitySalesWrapper entitySales)
			{
				var query = new ZDBOnlyQuery(typeof(OrgSalesValueAssociationPivot));
				query.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_TradeTableCode, OrgSalesSchema.Constants.Prefix);
				query.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_TradeId, entitySales.PK);
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
					return pivot.SVP_TradeTableCode == OrgSalesSchema.Constants.Prefix
						&& pivot.SVP_TradeId == entitySales.PK;
				}
			}
		}
	}
}

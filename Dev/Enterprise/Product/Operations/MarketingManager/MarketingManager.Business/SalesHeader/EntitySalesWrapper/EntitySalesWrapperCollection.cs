using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	/// <summary>
	/// Estimated sales only
	/// </summary>
	public class EntitySalesWrapperCollection : BusinessObjectCollection<EntitySalesWrapper>
	{
		public EntitySalesWrapperCollection(ISalesValueAssociatedEntity entity)
			: base(entity.Factory)
		{
			Argument.NotNull(entity, "entity");

			this.Entity = entity;
		}

		public override void Load()
		{
			Load(GetAssociatedSalesFilter(Org, Entity));
		}

		public readonly ISalesValueAssociatedEntity Entity;

		public OrgHeader Org => GetOrg(Entity);

		static OrgHeader GetOrg(ISalesValueAssociatedEntity entity)
		{
			return (entity as OrgHeader)
				?? (entity.OrgPkInfo != null ? entity.Factory.Load<OrgHeader>((ZGuid)entity.OrgPkInfo.Value) : null);
		}

		static ZQuery GetAssociatedSalesFilter(OrgHeader org, ISalesValueAssociatedEntity entity)
		{
			var query = new ZDBOnlyQuery(typeof(EntitySalesWrapper));
			query.AddToFilter(OrgSalesSchema.OW_IsTraded, ZBool.False);

			if (!(entity is OrgHeader))
			{
				query.AddToFilter(GetHasSalesAssociationQuery(entity));
			}
			else
			{
				query.AddToFilter(OrgSalesCollection.GetRelationshipFilter(org));
			}

			return query;
		}

		static ZQuery GetHasSalesAssociationQuery(ISalesValueAssociatedEntity entity)
		{
			var query = new ZDBOnlyQuery(typeof(EntitySalesWrapper));

			var hasAssociationPivotQuery = new ZDBOnlySubQuery(typeof(OrgSalesValueAssociationPivot), OrgSalesValueAssociationPivotSchema.SVP_TradeId);
			hasAssociationPivotQuery.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_ActivityTableCode, entity.TablePrefix);
			hasAssociationPivotQuery.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_ActivityId, entity.Identifier);
			hasAssociationPivotQuery.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_TradeTableCode, OrgSalesSchema.Constants.Prefix);
			query.AddSubQuery(hasAssociationPivotQuery, JoinCondition.And);

			return query;
		}

		#region Relationship

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			var entitySales = (EntitySalesWrapper)child;
			entitySales.Entity = Entity;
			base.SetCollectionRelationships(child);
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (!IsLoading)
			{
				var sales = (EntitySalesWrapper)bizOAdded;
				var product = (OrgSalesProduct)sales.Product;
				if (product != null && product.AllowedAssociationTargets.HasFlag(OrgSalesProductAssociationTarget.OrgSales))
				{
					OrgSalesValueAssociationPivot.AddPivotIfNotExist(Entity, sales);
				}
			}
		}

		protected override void RemoveCollectionRelationshipsCore(BusinessObject child, bool forDelete)
		{
			base.RemoveCollectionRelationshipsCore(child, forDelete);

			var sales = (EntitySalesWrapper)child;
			if (!forDelete)
			{
				foreach (var deletableTradeDetail in sales.EntityTradeDetailsCollection.Cast<EntityTradeDetailWrapper>().Where(x => !x.HasMultipleSalesAssociations).ToArray())
				{
					sales.EntityTradeDetailsCollection.RemoveAndDelete(deletableTradeDetail);
				}
				sales.EntityTradeDetailsCollection.RemoveAll();
			}

			var product = (OrgSalesProduct)sales.Product;
			if (product != null && product.AllowedAssociationTargets.HasFlag(OrgSalesProductAssociationTarget.OrgSales))
			{
				OrgSalesValueAssociationPivot.DeleteAll(Entity, sales);
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var entitySales = (EntitySalesWrapper)child;
			entitySales.OW_OH_Primary = Org.PK;
		}

		#endregion
	}
}

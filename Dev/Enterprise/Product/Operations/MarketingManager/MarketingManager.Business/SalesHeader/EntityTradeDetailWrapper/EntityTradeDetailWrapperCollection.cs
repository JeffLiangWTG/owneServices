using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class EntityTradeDetailWrapperCollection : DependentBusinessObjectCollection<EntityTradeDetailWrapper, EntitySalesWrapper>
	{
		#region Constructor

		public EntityTradeDetailWrapperCollection(ISalesValueAssociatedEntity entity)
			: base(entity.Factory)
		{
			this.entity = entity;
		}

		public EntityTradeDetailWrapperCollection(EntitySalesWrapper entitySales)
			: base(entitySales, GetAdditionalFilter(entitySales.Entity, entitySales.Product))
		{
			Argument.NotNull(entitySales, "entitySales");

			this.entitySales = entitySales;
		}

		#endregion

		#region Properties

		readonly ISalesValueAssociatedEntity entity;
		readonly EntitySalesWrapper entitySales;

		ISalesValueAssociatedEntity Entity
		{
			get { return entity ?? entitySales.Entity; }
		}

		public EntitySalesWrapper EntitySales
		{
			get { return entitySales; }
		}

		OrgSalesProduct SalesProduct
		{
			get { return (OrgSalesProduct)entitySales?.Product; }
		}

		#endregion

		#region Relationship

		static ZQuery GetAdditionalFilter(ISalesValueAssociatedEntity entity, IOrgSalesProduct salesProduct)
		{
			if (entity == null || entity is OrgHeader)
			{
				return new ZQuery();
			}

			var product = (OrgSalesProduct)salesProduct;
			var onlyIncludeTradeDetailsWithAssociation = product != null && product.AllowedAssociationTargets.HasFlag(OrgSalesProductAssociationTarget.OrgTradeDetail);
			if (!onlyIncludeTradeDetailsWithAssociation)
			{
				return new ZQuery();
			}

			var query = new ZDBOnlyQuery(typeof(EntityTradeDetailWrapper));
			var hasAssociationPivotQuery = new ZDBOnlySubQuery(typeof(OrgSalesValueAssociationPivot), OrgSalesValueAssociationPivotSchema.SVP_TradeId);
			hasAssociationPivotQuery.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_TradeTableCode, OrgTradeDetailSchema.Constants.Prefix);
			hasAssociationPivotQuery.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_ActivityTableCode, entity.TablePrefix);
			hasAssociationPivotQuery.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_ActivityId, entity.Identifier);
			query.AddSubQuery(hasAssociationPivotQuery, JoinCondition.And);

			return query;
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			var tradeDetail = (EntityTradeDetailWrapper)dependent;
			tradeDetail.Entity = Entity;
			base.SetCollectionRelationships(dependent);

			if (salesMatchingPropertyChanged != null)
			{
				AddSalesMatchingPropertyChangedHandlers(tradeDetail);
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			if (!IsLoading)
			{
				SetOpportunityTradeDetailStatus(Entity, (EntityTradeDetailWrapper)child);
			}
		}

		public static void SetOpportunityTradeDetailStatus(ISalesValueAssociatedEntity entity, EntityTradeDetailWrapper tradeDetail)
		{
			if (entity != null && entity is OrgOpportunity opportunity)
			{
				var tradeStatus = OrganisationsDataRegistry.Instance.OpportunityStatus.Value.GetTradeStatusFromCode(opportunity.P8_Status);

				if (!tradeStatus.IsEmpty)
				{
					tradeDetail.PA_Status = tradeStatus;
				}
				else
				{
					tradeDetail.PA_Status = OpportunityTradeStatus.Codes.Active;
				}
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (!IsLoading && Entity != null)
			{
				var tradeDetail = (EntityTradeDetailWrapper)bizOAdded;
				OrgSalesValueAssociationPivot.AddPivotIfNotExist(Entity, tradeDetail);
			}
			RefreshEntitySalesStats();
		}

		protected override void RemoveCollectionRelationshipsCore(BusinessObject child, bool forDelete)
		{
			var tradeDetail = (EntityTradeDetailWrapper)child;
			if (Entity != null)
			{
				OrgSalesValueAssociationPivot.DeleteAll(Entity, tradeDetail);
				HasChanges = true;
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			var tradeDetail = (EntityTradeDetailWrapper)bizO;
			if (salesMatchingPropertyChanged != null)
			{
				RemoveSalesMatchingPropertyChangedHandlers(tradeDetail);
			}
			base.OnRemoved(bizO);
			RefreshEntitySalesStats();
		}

		void RefreshEntitySalesStats()
		{
			if (entitySales != null && !entitySales.IsActual)
			{
				entitySales.RefreshTotalAnnualStats();
			}
		}

		#endregion

		#region Sales Matching Property Changed Event

		void AddSalesMatchingPropertyChangedHandlers(EntityTradeDetailWrapper tradeDetail)
		{
			var salesProduct = SalesProduct;
			if (salesProduct != null && salesProduct.AllowedAssociationTargets.HasFlag(OrgSalesProductAssociationTarget.OrgTradeDetail))
			{
				foreach (var propertyName in salesProduct.SalesMatchingOptions.TradeDetailPropertiesForMatching.Select(x => x.Item1))
				{
					var propertyInfo = tradeDetail.FindPropertyInfo(propertyName);
					propertyInfo.ValueChanged -= SalesMatchingPropertyInfo_ValueChanged;
					propertyInfo.ValueChanged += SalesMatchingPropertyInfo_ValueChanged;
				}
			}
		}

		void RemoveSalesMatchingPropertyChangedHandlers(EntityTradeDetailWrapper tradeDetail)
		{
			var salesProduct = SalesProduct;
			if (salesProduct != null && salesProduct.AllowedAssociationTargets.HasFlag(OrgSalesProductAssociationTarget.OrgTradeDetail))
			{
				foreach (var propertyName in salesProduct.SalesMatchingOptions.TradeDetailPropertiesForMatching.Select(x => x.Item1))
				{
					var propertyInfo = tradeDetail.FindPropertyInfo(propertyName);
					propertyInfo.ValueChanged -= SalesMatchingPropertyInfo_ValueChanged;
				}
			}
		}

		void SalesMatchingPropertyInfo_ValueChanged(object sender, EventArgs e)
		{
			if (salesMatchingPropertyChanged != null)
			{
				salesMatchingPropertyChanged(sender, e);
			}
		}

		public event EventHandler SalesMatchingPropertyChanged
		{
			add
			{
				if (salesMatchingPropertyChanged == null)
				{
					foreach (EntityTradeDetailWrapper tradeDetail in this)
					{
						AddSalesMatchingPropertyChangedHandlers(tradeDetail);
					}
				}

				salesMatchingPropertyChanged += value;
			}
			remove
			{
				salesMatchingPropertyChanged -= value;
				if (salesMatchingPropertyChanged == null)
				{
					foreach (EntityTradeDetailWrapper tradeDetail in this)
					{
						RemoveSalesMatchingPropertyChangedHandlers(tradeDetail);
					}
				}
			}
		}
		event EventHandler salesMatchingPropertyChanged;

		public event EventHandler DefaultTradeDetailRemovedBySalesMatching;

		public void OnDefaultTradeDetailRemovedBySalesMatching()
		{
			DefaultTradeDetailRemovedBySalesMatching?.Invoke(this, EventArgs.Empty);
		}

		#endregion
	}
}

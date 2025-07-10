using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class EntitySalesWrapperCollectionProductView : BusinessObjectCollectionView<EntitySalesWrapper>
	{
		#region Constructor

		public EntitySalesWrapperCollectionProductView(EntitySalesWrapperCollection collectionToFilter, OrgSalesProduct salesProduct)
			: base(collectionToFilter)
		{
			this.salesProduct = salesProduct;
			Rebuild();
		}

		protected override void RebuildOnConstruction()
		{
			// defer rebuild as salesProduct not populated yet
		}

		public OrgSalesProduct SalesProduct
		{
			get { return salesProduct; }
		}
		readonly OrgSalesProduct salesProduct;

		public new EntitySalesWrapperCollection CollectionToFilter
		{
			get { return (EntitySalesWrapperCollection)base.CollectionToFilter; }
		}

		#endregion

		#region Default Values

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var sales = (EntitySalesWrapper)child;
			var master = Master;
			if (master != null)
			{
				sales.OW_OH_Primary = master.PK;
				if (SalesProduct != null && !SalesProduct.MP_IsSystemDefined && master.ClosestPort != null)
				{
					sales.OW_OriginID = master.ClosestPort.PK;
				}
			}

			if (SettingDefaultsForNewChild != null)
			{
				SettingDefaultsForNewChild(this, new EntitySalesEventArgs(sales));
			}
		}

		public event EventHandler<EntitySalesEventArgs> SettingDefaultsForNewChild;

		#endregion

		#region Relationship

		public OrgHeader Master
		{
			get { return CollectionToFilter.Org; }
		}

		protected override bool ShouldWeAddBusinessObjectStraightToView(BusinessObject businessObject)
		{
			return true;
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var sales = (OrgSales)element;
			return
				sales.OW_MP_Product == (salesProduct != null ? salesProduct.PK : ZGuid.Empty);
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			var sales = (OrgSales)child;
			using (sales.GetValidationSuspender())
			{
				sales.OW_MP_Product = (salesProduct != null) ? salesProduct.PK : ZGuid.Empty;
			}

			if (salesMatchingPropertyChanged != null)
			{
				AddSalesMatchingPropertyChangedHandlers(sales);
			}
		}

		#endregion

		#region Events

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			var sales = (EntitySalesWrapper)bizOAdded;

			if (estimatedValueChanged != null)
			{
				AddEstimateValueChangedHandlers(sales);
			}

			sales.HasChangesChanged += ProspectSales_HasChangesChanged;

			base.OnAdded(bizOAdded);

			OnEstimatedValueChanged();
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			var sales = (EntitySalesWrapper)bizO;

			if (estimatedValueChanged != null)
			{
				RemoveEstimateValueChangedHandlers(sales);
			}

			sales.HasChangesChanged -= ProspectSales_HasChangesChanged;

			if (salesMatchingPropertyChanged != null)
			{
				RemoveSalesMatchingPropertyChangedHandlers(sales);
			}

			base.OnRemoved(bizO);

			OnEstimatedValueChanged();
		}

		#region Sales HasChanges Changed

		void ProspectSales_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (ProspectSalesHasChangesChanged != null)
			{
				ProspectSalesHasChangesChanged(this, new EntitySalesEventArgs((EntitySalesWrapper)sender));
			}
		}

		public event EventHandler<EntitySalesEventArgs> ProspectSalesHasChangesChanged;

		#endregion

		#region Estimate Value Changed Event

		void AddEstimateValueChangedHandlers(EntitySalesWrapper entitySales)
		{
			entitySales.OW_AnnualRevenueInfo.ValueChanged -= SalesRevenueInfo_ValueChanged;
			entitySales.OW_MonthlyRevenueInfo.ValueChanged -= SalesRevenueInfo_ValueChanged;
			entitySales.TotalRevenueCurrencyCodeInfo.ValueChanged -= SalesRevenueInfo_ValueChanged;

			entitySales.OW_AnnualRevenueInfo.ValueChanged += SalesRevenueInfo_ValueChanged;
			entitySales.OW_MonthlyRevenueInfo.ValueChanged += SalesRevenueInfo_ValueChanged;
			entitySales.TotalRevenueCurrencyCodeInfo.ValueChanged += SalesRevenueInfo_ValueChanged;
		}

		void RemoveEstimateValueChangedHandlers(EntitySalesWrapper entitySales)
		{
			entitySales.OW_AnnualRevenueInfo.ValueChanged -= SalesRevenueInfo_ValueChanged;
			entitySales.OW_MonthlyRevenueInfo.ValueChanged -= SalesRevenueInfo_ValueChanged;
			entitySales.TotalRevenueCurrencyCodeInfo.ValueChanged -= SalesRevenueInfo_ValueChanged;
		}

		void SalesRevenueInfo_ValueChanged(object sender, EventArgs e)
		{
			OnEstimatedValueChanged();
		}

		protected void OnEstimatedValueChanged()
		{
			if (estimateValueChangedSuspender != null)
			{
				estimateValueChangedSuspender.ShouldInvokeEsimtatedValueEventOnDispose = true;
			}
			else
			{
				FireEstimatedValueChangedEvent();
			}
		}

		void FireEstimatedValueChangedEvent()
		{
			if (estimatedValueChanged != null)
			{
				estimatedValueChanged(this, EventArgs.Empty);
			}
		}

		public event EventHandler EstimatedValueChanged
		{
			add
			{
				if (estimatedValueChanged == null)
				{
					foreach (EntitySalesWrapper entitySales in this)
					{
						AddEstimateValueChangedHandlers(entitySales);
					}
				}

				estimatedValueChanged += value;
			}
			remove
			{
				estimatedValueChanged -= value;
				if (estimatedValueChanged == null)
				{
					foreach (EntitySalesWrapper entitySales in this)
					{
						RemoveEstimateValueChangedHandlers(entitySales);
					}
				}
			}
		}
		event EventHandler estimatedValueChanged;

		EstimateValueChangedSuspender estimateValueChangedSuspender;

		class EstimateValueChangedSuspender : IDisposable
		{
			public EstimateValueChangedSuspender(EntitySalesWrapperCollectionProductView collection)
			{
				this.collection = collection;
				this.collection.estimateValueChangedSuspender = this;
			}

			readonly EntitySalesWrapperCollectionProductView collection;

			public bool ShouldInvokeEsimtatedValueEventOnDispose;

			public void Dispose()
			{
				if (ShouldInvokeEsimtatedValueEventOnDispose)
				{
					collection.FireEstimatedValueChangedEvent();
				}

				this.collection.estimateValueChangedSuspender = null;
			}
		}

		#endregion

		#region Sales Matching Property Changed Event

		void AddSalesMatchingPropertyChangedHandlers(OrgSales sales)
		{
			if (salesProduct != null && salesProduct.AllowedAssociationTargets.HasFlag(OrgSalesProductAssociationTarget.OrgSales))
			{
				foreach (var propertyName in salesProduct.SalesMatchingOptions.SalesPropertiesForMatching.Select(x => x.Item1))
				{
					var propertyInfo = sales.FindPropertyInfo(propertyName);
					propertyInfo.ValueChanged -= SalesMatchingPropertyInfo_ValueChanged;
					propertyInfo.ValueChanged += SalesMatchingPropertyInfo_ValueChanged;
				}
			}
		}

		void RemoveSalesMatchingPropertyChangedHandlers(OrgSales sales)
		{
			if (salesProduct != null && salesProduct.AllowedAssociationTargets.HasFlag(OrgSalesProductAssociationTarget.OrgSales))
			{
				foreach (var propertyName in salesProduct.SalesMatchingOptions.SalesPropertiesForMatching.Select(x => x.Item1))
				{
					var propertyInfo = sales.FindPropertyInfo(propertyName);
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
					foreach (OrgSales sales in this)
					{
						AddSalesMatchingPropertyChangedHandlers(sales);
					}
				}

				salesMatchingPropertyChanged += value;
			}
			remove
			{
				salesMatchingPropertyChanged -= value;
				if (salesMatchingPropertyChanged == null)
				{
					foreach (OrgSales sales in this)
					{
						RemoveSalesMatchingPropertyChangedHandlers(sales);
					}
				}
			}
		}
		event EventHandler salesMatchingPropertyChanged;

		#endregion

		#endregion

		#region List Changed

		protected override DisposableList GetAdditionalListChangedSuspenders()
		{
			var list = new DisposableList(1);
			list.Add(new EstimateValueChangedSuspender(this));
			return list;
		}

		#endregion
	}

	public class EntitySalesEventArgs : EventArgs
	{
		public EntitySalesEventArgs(EntitySalesWrapper entitySales)
		{
			Argument.NotNull(entitySales, "entitySales");
			EntitySales = entitySales;
		}

		public readonly EntitySalesWrapper EntitySales;
	}
}

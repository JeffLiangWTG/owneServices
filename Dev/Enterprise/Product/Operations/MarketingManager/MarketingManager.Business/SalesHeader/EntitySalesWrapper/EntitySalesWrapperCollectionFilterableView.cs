using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.Business
{
	[ModuleID(ModuleId.Sales)]
	public class EntitySalesWrapperCollectionFilterableView : BusinessObjectCollectionView<EntitySalesWrapper>
	{
		public EntitySalesWrapperCollectionFilterableView(EntitySalesWrapperCollectionProductView collectionToFilter, ZGuid companyFilter)
			: base(collectionToFilter)
		{
			this.productView = collectionToFilter;
			this.companyFilter = companyFilter;
			Rebuild();
		}

		protected override void RebuildOnConstruction()
		{
			// Defer rebuild until constructor sets productView
		}

		readonly EntitySalesWrapperCollectionProductView productView;

		public new EntitySalesWrapperCollectionProductView CollectionToFilter
		{
			get { return (EntitySalesWrapperCollectionProductView)base.CollectionToFilter; }
		}

		OrgHeader Org
		{
			get { return CollectionToFilter.Master; }
		}

		#region Filters

		public ZString StatusFilter
		{
			get { return statusFilter; }
			set
			{
				if (statusFilter != value)
				{
					statusFilter = value;
					Rebuild();
				}
			}
		}
		ZString statusFilter;

		public ZBool ShouldMatchOnBuyerSupplier
		{
			get { return shouldMatchOnBuyerSupplier; }
			set
			{
				if (shouldMatchOnBuyerSupplier != value)
				{
					shouldMatchOnBuyerSupplier = value;

					if (!StatusFilter.IsEmpty)
					{
						Rebuild();
					}
					else
					{
						RefreshSalesActualsInformation(CollectionToFilter.Cast<EntitySalesWrapper>());
					}
				}
			}
		}
		ZBool shouldMatchOnBuyerSupplier;

		public ZGuid CompanyFilter
		{
			get => companyFilter;
			set
			{
				if (companyFilter != value)
				{
					companyFilter = value;
					Rebuild();
				}
			}
		}
		ZGuid companyFilter;

		#endregion

		#region Relationship

		protected override void RebuildCore()
		{
			var salesCollection = CollectionToFilter.Cast<EntitySalesWrapper>();
			RefreshProspectCompany(salesCollection);
			if (IsActualsSupported)
			{
				RefreshSalesActualsInformation(salesCollection);
			}

			base.RebuildCore();
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var sales = (EntitySalesWrapper)element;
			if (IsActualsSupported && !StatusFilter.IsEmpty && sales.ActualsInformation.Status != StatusFilter)
			{
				return false;
			}

			if (!CompanyFilter.IsEmpty)
			{
				return sales.ProspectCompanyPks != null && sales.ProspectCompanyPks.Any(x => x.IsEmpty || x == CompanyFilter);
			}

			return true;
		}

		bool IsActualsSupported => productView.SalesProduct.IsActualsSupported;

		#endregion

		#region Sales Actuals Info Bulk Populater

		OrgSalesActualsInformationBulkPopulater SalesActualsInformationBulkPopulater
		{
			get { return salesActualsInformationBulkPopulater ?? (salesActualsInformationBulkPopulater = new OrgSalesActualsInformationBulkPopulater(Factory, Org.PK)); }
		}
		OrgSalesActualsInformationBulkPopulater salesActualsInformationBulkPopulater;

		void RefreshSalesActualsInformation(IEnumerable<EntitySalesWrapper> sales)
		{
			SalesActualsInformationBulkPopulater.Execute(sales, ShouldMatchOnBuyerSupplier);
			SalesActualsInformationRefreshed?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler SalesActualsInformationRefreshed;

		#endregion

		#region Prospect Company Bulk Populater

		OrgSalesProspectCompanyBulkPopulater ProspectCompanyBulkPopulater
		{
			get { return prospectCompanyBulkPopulater ?? (prospectCompanyBulkPopulater = new OrgSalesProspectCompanyBulkPopulater(Factory, Org.PK)); }
		}
		OrgSalesProspectCompanyBulkPopulater prospectCompanyBulkPopulater;

		void RefreshProspectCompany(IEnumerable<EntitySalesWrapper> sales)
		{
			ProspectCompanyBulkPopulater.Execute(sales);
		}

		#endregion
	}
}

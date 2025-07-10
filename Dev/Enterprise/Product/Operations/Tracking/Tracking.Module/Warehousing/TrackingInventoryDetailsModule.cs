using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Module
{
	/// <summary>
	/// Summary description for TrackingInventoryModule.
	/// </summary>
	public class TrackingInventoryDetailsModule : ZFilterStripGridModule
	{
		public TrackingInventoryDetailsModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID => WebModuleIDs.TrackingInventoryDetails;

		public override Type GridCollectionType => typeof(TrackingWhsInventoryCollection);

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem
			=> WebDataRegistry.Instance.DefaultFilterLayoutWarehouseInventory;

		protected override sealed ZString FilterStripLayoutContext
			=> WebModuleIDs.TrackingInventory.Name; // We want this to be the same for Inventory & InventoryDetails

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			OrgHeader loggedInWebUsersOrg = null;
			if (Page.SiteUser != null && Page.SiteUser.IsLoggedIn)
			{
				loggedInWebUsersOrg = ((OrgContactWebUser)Page.SiteUser).LoggedInOrganisation;
			}

			var result = new TrackingInventoryFilterBusinessObject(loggedInWebUsersOrg);
			result.AddModuleFiltersCreatedHook((x) => SetupFilterStripBusinessObject(x));

			return result;
		}

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO)
		{
			var orgRestrictionFilter = ZQuery.NoResultQuery;

			var siteUser = (OrgContactWebUser)Page.SiteUser;
			if (siteUser != null && siteUser.LoggedInOrganisation != null)
			{
				orgRestrictionFilter = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingWhsInventory>();
			}

			return orgRestrictionFilter;
		}

		void SetupFilterStripBusinessObject(IFilterStripBusinessObject filterStripBizO)
		{
			SetupFromQueryStringModuleGuidFilter((FilterStripBusinessObject)filterStripBizO, (NoResString)"Warehouse", TrackingConstants.QueryStringKeys.WarehouseRefKey);
			SetupFromQueryStringModuleGuidFilter((FilterStripBusinessObject)filterStripBizO, (NoResString)"Product", TrackingConstants.QueryStringKeys.WhsProductRefKey);
		}

		void SetupFromQueryStringModuleGuidFilter(FilterStripBusinessObject filterStripBizO, string filterDesc, string queryStringKey)
		{
#if DEBUG
			if (!Globals.IsTest)
			{
#endif
				var guidFilter = filterStripBizO[filterDesc] as ModuleGuidFilter;
				string refString = Page.Request.QueryString[queryStringKey];
				ZGuid refPK;

				if (guidFilter != null && !string.IsNullOrEmpty(refString) && ZGuid.TryParse(refString, out refPK))
				{
					guidFilter.Property = refPK;
					guidFilter.IsActive = true;
					guidFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
				}
#if DEBUG
			}
#endif
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				if (FilterStripBizO != null)
				{
					FilterStripBizO.ModuleFiltersCreated -= SetupFilterStripBusinessObject;
				}
			}

			base.Dispose(isDisposing);
		}

		#region Grid Columns

		protected override GridColumnProvider GetColumnProvider()
		{
			var siteUser = Page != null ? Page.SiteUser as TrackingSiteUser : null;
			PartAttributeManager partManager = null;
			if (siteUser != null && siteUser.IsLoggedIn)
			{
				partManager = siteUser.LoggedInOrganisation.PartAttributeManager;
			}

			return new TrackingInventoryDetailsColumnProvider(partManager);
		}

		protected override int SelectionColumnIndex => 0;

		#endregion

		#region Sorting

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			var sortInfos = new List<ColumnAndSortOrder>();
			sortInfos.Add(new ColumnAndSortOrder(TrackingWhsInventory.Schema.TrackingSerialNumber, DefaultSortOrder));
			sortInfos.Add(new ColumnAndSortOrder(TrackingWhsInventory.Schema.TrackingPartAttrib3, DefaultSortOrder));
			sortInfos.Add(new ColumnAndSortOrder(TrackingWhsInventory.Schema.TrackingPartAttrib2, DefaultSortOrder));
			sortInfos.Add(new ColumnAndSortOrder(TrackingWhsInventory.Schema.TrackingPartAttrib1, DefaultSortOrder));
			sortInfos.Add(new ColumnAndSortOrder(TrackingWhsInventory.Schema.TrackingPalletID, DefaultSortOrder));
			sortInfos.Add(new ColumnAndSortOrder(TrackingWhsInventory.Schema.TrackingProductCode, DefaultSortOrder));
			sortInfos.Add(new ColumnAndSortOrder(TrackingWhsInventory.Schema.TrackingReceiptReference, DefaultSortOrder));
			sortInfos.Add(new ColumnAndSortOrder(TrackingWhsInventory.Schema.TrackingArrivalDate, DefaultSortOrder));

			return sortInfos.ToArray();
		}

		#endregion

		protected override SchemaPKColumn RelevantPersistantPKColumn => null;
	}
}

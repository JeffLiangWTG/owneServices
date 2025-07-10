using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Module
{
	public class TrackingDeclarationsModule : TrackingShipmentsModule
	{
		public TrackingDeclarationsModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{ }

		OrgContactWebUser CurrentUser
		{
			get { return (OrgContactWebUser)Page.SiteUser; }
		}

		#region Overrides

		public override ZGuid GetEDocsBulkDownloadRelevantPK(ZDataGrid grid, int itemIndex)
		{
			var collection = grid.DataSource as IBusinessObjectCollection;
			var bizoPK = grid.GetPKByRowIndex(itemIndex);
			var bizo = collection.FindByPK(bizoPK);
			var persistentPk = new ZGuid(bizo[RelevantPersistantPKColumn]);
			var declaration = Factory.Load<BaseJobDeclaration>(persistentPk);
			var relevantPK = declaration != null && !declaration.JE_JS.IsEmpty ? declaration.JE_JS : persistentPk;
			return relevantPK;
		}

		protected override SchemaPKColumn RelevantPersistantPKColumn
		{
			get { return ShipmentDeclarationSchema.PersistentBizOPK; }
		}

		protected override Type GetCancellableCollectionElementType()
		{
			return typeof(BaseJobDeclaration);
		}

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutDeclaration; }
		}

		#region ID

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.TrackingDeclarations; }
		}

		#endregion

		#region GridCollectionType

		public override Type GridCollectionType
		{
			get { return typeof(TrackingDeclarationCollection); }
		}

		#endregion

		#region GetNewFilterStripBusinessObject

		TrackingJobDeclarationFilterBusinessObjectFactory JobDeclarationFilterStripFactory => jobDeclarationFilterStripFactory ?? (jobDeclarationFilterStripFactory = new TrackingJobDeclarationFilterBusinessObjectFactory());
		TrackingJobDeclarationFilterBusinessObjectFactory jobDeclarationFilterStripFactory;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			var filterStripBizo = JobDeclarationFilterStripFactory.GetJobDeclarationFilterBusinessObject(UserCountryCode, CurrentUser.LoggedInOrganisation);
			filterStripBizo.AddModuleFiltersCreatedHook((x) => FilterStripBizo_ModuleFiltersCreated(x));

			return filterStripBizo;
		}

		void FilterStripBizo_ModuleFiltersCreated(FilterStripBusinessObject filterStripBizO)
		{
			if (filterStripBizO.ModuleFilters[TrackingDeclarationFilterConstants.CreatedTime] == null)
			{
				var createdTimeModuleFilter = filterStripBizO.ModuleFilters.AddDateFilter(TrackingDeclarationFilterConstants.CreatedTime, JobDeclarationSchema.JE_SystemCreateTimeUtc);
				createdTimeModuleFilter.Category = FilterCategories.AuditInformation;
				createdTimeModuleFilter.MultilingualDescription = ResString.GetMultilingualString("TrackingDeclaration|DeclarationFilterControl|CreatedTime", "Created Time");
			}
		}

		ZString UserCountryCode
		{
			get { return CurrentUser.GetCountryCode(); }
		}

		#endregion

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO)
		{
			return OrgRestrictionFilterFactory.Instance.GetFilter<TrackingDeclaration>();
		}

		protected override IBusinessObjectCollection LoadCollectionCore(FilterBusinessObject filterBizO, ZQuery query, IBusinessObjectCollection collection, bool ignoreCache)
		{
			var decCollection = collection as TrackingDeclarationCollection;
			if (decCollection != null)
			{
				var sortInfo = GetSortInfos(filterBizO).Single();
				query.AddToFilter(GetCurrentLoggedInUserFilter(filterBizO));
				decCollection.Load(query);
				decCollection.Sort(sortInfo.OrderByColumnName, sortInfo.SortDirection);
			}

			return collection;
		}

		protected override GridColumnProvider GetColumnProvider()
		{
			return DeclarationColumnProviderTypeDecider.GetColumnProviderForCountry(UserCountryCode);
		}

		#endregion
	}
}

using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.GUI
{
	public class GlbCompanyCampaignContactModule : ZFilterGridModule, IGlbCompanyCampaignContactModule
	{
		public event EventHandler FireOnPerformSearch;

		void OnFirePerformSearch()
		{
			if (FireOnPerformSearch != null)
			{
				FireOnPerformSearch(null, EventArgs.Empty);
			}
		}

		public bool MoreContactsAvailable;

		public ZString SearchRecordsFoundMessage
		{
			get
			{
				ZString result;

				if (GridCollection.Count == 0)
				{
					result = GlbCompanyCampaign.NotificationConstants.NoMatchingRecordsMessage;
				}
				else
				{
					result = string.Format(CultureInfo.CurrentCulture, NotificationConstants.BatchMatchingRecordsMessage, GridCollection.Count);
				}

				if (Campaign.G0_DeDuplicateContacts)
				{
					result += " " + Res.GetString("108a6759-edd3-4163-8578-6d342acbf5be", "Duplicate contacts excluded.");
				}

				return result;
			}
		}

		protected override int MaxRowsToLoad => Campaign?.MaxDisplayRecords ?? base.MaxRowsToLoad;

		#region Overrides

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.GlbCompanyCampaignContact);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			var campaign = Campaign;
			var moduleId = campaign?.DripMarketingFilterRuleModule ?? GlbCompanyCampaign.DefaultDripMarketingFilterRuleModule;

			return (FilterBusinessObject)RelatedModuleFiltersHelper.GetNewFilterBusinessObject(moduleId, campaign);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GlbCompanyCampaignContactFilterControl(GridCollection, (GlbCompanyCampaignContactFilterBusinessObject)FilterBusinessObject, Campaign);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return Campaign != null ? new GlbCampaignContactCollection(Campaign) : new GlbCampaignContactCollection(Factory);
		}

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		void EnsureCampaignFilterSavedCorrectlyIfRequired()
		{
			var filterBizO = (GlbCompanyCampaignContactFilterBusinessObject)this.FilterBusinessObject;

			if (filterBizO != null && filterBizO.LastUsedLayout != null && filterBizO.LastUsedLayout != null && !filterBizO.LastUsedLayout.IsDeleted)
			{
				if (filterBizO.LastUsedLayout.S9_FilterNameMultilingual.IsEmpty || filterBizO.LastUsedLayout.S9_ModuleID.EqualsIgnoringCase(DripMarketingFilterRuleModuleName))
				{
					filterBizO.SetCampaignFilterLayoutContext();
				}
				else
				{
					filterBizO.layoutsHelper.BizObjPK = Env.CurrentUser.PK;
				}
			}
		}

		protected virtual ZString DripMarketingFilterRuleModuleName
		{
			get
			{
				return ModuleIDs.DripMarketingFilterRule.Name;
			}
		}

		//	Method overriden to ensure that campaign specific filter is saved correctly
		protected override void Dispose(bool isDisposing)
		{
			EnsureCampaignFilterSavedCorrectlyIfRequired();
			base.Dispose(isDisposing);
		}

#if DEBUG
		public
#endif
		IZForm LastShownForm;

		protected override IZForm ShowViewForm(BusinessObject selectedBusinessObject)
		{
			LastShownForm = base.ShowViewForm(selectedBusinessObject);
			return LastShownForm;
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GlbCompanyCampaignContact; }
		}

		public override bool AllowNew
		{
			get
			{
				return false;
			}
		}

		public override bool AllowDelete
		{
			get
			{
				return false;
			}
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.RelationshipCampaignManager; }
		}

		public override Security.SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CampaignManagement; }
		}

		protected override FilteredGridLoader CreateSearchManager()
		{
			var result = base.CreateSearchManager();
			result.ThrowExceptionOnMaximumRowsLoaded = false;
			return result;
		}

		protected override void OnAfterPerformSearchCore()
		{
			OnFirePerformSearch();
		}

		protected override ResultCountMessage GetNewResultCountMessage()
		{
			if (Campaign != null)
			{
				int displayGridRecord = Campaign.MaxDisplayRecords;
				int recommendedRowsToLoad = Campaign.G0_BatchCountDefault;
				return new ResultCountMessage((IFilterControl)EmbeddedControl, MaxRowsToLoad, (recommendedRowsToLoad >= MaxRowsToLoad ? displayGridRecord : recommendedRowsToLoad));
			}

			return new ResultCountMessage((IFilterControl)EmbeddedControl, Int32.MaxValue, Int32.MaxValue);
		}

		protected override ZQuery GetDisplayResultsQuery()
		{
			ZQuery query = base.GetDisplayResultsQuery();
			if (query.MaximumRows == Int32.MaxValue)
			{
				query.MaximumRows = null;
			}
			return query;
		}

		#endregion

		#region IGlbCompanyCampaignContactModule Members

		public GlbCompanyCampaign Campaign { get; set; }

		MasterFiles.Integration.IGlbCompanyCampaign IGlbCompanyCampaignContactModule.Campaign
		{
			get => Campaign;
			set => Campaign = (GlbCompanyCampaign)value;
		}

		#endregion

		public static class NotificationConstants
		{
			public static string NoMatchingRecordsMessage
			{
				get { return Res.GetString("fb6d4caa-6524-4ff1-a516-64b2156ef08e", "There are no records that match your search."); }
			}

			public static string MultipleMatchingRecordsMessage
			{
				get { return Res.GetString("c52a85c6-db22-474e-a4fa-8fc2c57e84c0", "Found {0:G} records that match your search criteria.\r\nFirst {1:G} records shown based on the specified batch count."); }
			}

			public static string BatchMatchingRecordsMessage
			{
				get { return Res.GetString("E6CF4FCE-E758-4271-9554-51FCB4642E49", "First {0:G} records shown based on the specified batch count."); }
			}
		}
	}
}

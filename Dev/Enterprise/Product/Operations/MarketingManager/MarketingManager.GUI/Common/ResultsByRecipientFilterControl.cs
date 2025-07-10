using System;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public partial class ResultsByRecipientFilterControl : ZFilterStripControl
	{
		readonly GlbCompanyCampaign campaignForFilter;
		readonly LearningCentreCampaignItemFilterBusinessObject filterStrip;

		public ResultsByRecipientFilterControl(GlbCompanyCampaign campaign, LearningCentreCampaignItemFilterBusinessObject filterBusinessObject)
			: base(campaign.CampaignsItemsSentForDisplayOnly, filterBusinessObject)
		{
			InitializeComponent();
			campaignForFilter = campaign;
			filterStrip = filterBusinessObject;
			PerformSearch += ResultsByRecipientFilterControl_PerformSearch;
		}

		void ResultsByRecipientFilterControl_PerformSearch(object sender, EventArgs e)
		{
			var collection = campaignForFilter.CampaignsItemsSentForDisplayOnly;
			var factory = SearchManager.GetNewFactory();
			var type = collection.TypeOfElements;
			var query = filterStrip.Filter;
			query.AddToFilter(new ZQuery(GlbCompanyCampaignItemSchema.G8_G0, campaignForFilter.PK));
			var result = SearchManager.PerformSearch(factory, type, query);

			if (result.Type == PerformSearchResultType.Success)
			{
				SearchManager.PushItemsIntoCollection(collection, result, null);
			}
		}

		protected FilteredGridLoader SearchManager => searchManager ?? (searchManager = CreateSearchManager());
		FilteredGridLoader searchManager;

		FilteredGridLoader CreateSearchManager()
			=> new FilteredGridLoader(FilterBusinessObject, new ResultCountMessage(this, MaxRowsToLoad, MaxRowsToLoad), false, ModuleIDs.GlbCompanyCampaignItem, () => new BusinessObjectFactory(), typeof(GlbCompanyCampaignItem));

		int MaxRowsToLoad => SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;
	}
}

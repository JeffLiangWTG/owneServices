using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.GUI
{
	public class GlbCompanyCampaignItemModule : ZFilterGridModule, IGlbCompanyCampaignItemModule
	{
		#region Events

		public event EventHandler PerformedSearch;

		void OnPerformedSearch()
		{
			if (PerformedSearch != null)
			{
				PerformedSearch(null, System.EventArgs.Empty);
			}
		}

		#endregion

		#region Overrides

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.GlbCompanyCampaignItem);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new GlbCompanyCampaignItemFilterBusinessObject((GlbCompanyCampaign)Campaign);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GlbCompanyCampaignItemFilterControl(GridCollection, (GlbCompanyCampaignItemFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			GlbCompanyCampaignItemCampaignDependentCollection collection = Campaign != null ? new GlbCompanyCampaignItemCampaignDependentCollection((GlbCompanyCampaign)Campaign) : new GlbCompanyCampaignItemCampaignDependentCollection(Factory);
			collection.Load(ZQuery.NoResultQuery);
			return collection;
		}

		protected override BusinessObjectFactory GetNewFactory()
		{
			return Campaign != null ? ((GlbCompanyCampaign)Campaign).Factory : new BusinessObjectFactory();
		}

		public override bool AllowDelete => base.AllowDelete && Campaign != null && Campaign.IsTargetList;

		protected override void HandleDeleteClickCore(object sender, EventArgs e)
		{
			if (!AllowDelete)
			{
				return;
			}

			if (SelectedBusinessObjects.Length == 0)
			{
				ShowNoSelectedMessage();
				return;
			}

			var message = Res.GetString("0200a3b3-df68-4d00-a392-155409e170de", "You are about to remove the selected contact(s) from the Target List. Would you like to continue?");
			var caption = Res.GetString("48ad0f8f-0b48-4331-8efe-7c1a906d412f", "Delete Campaign Contact");
			if (DialogResult.Yes == Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question))
			{
				var selectedItems = SelectedBusinessObjects.Cast<GlbCompanyCampaignItem>().ToArray();
				foreach (var item in selectedItems)
				{
					item.Delete();
				}
			}
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GlbCompanyCampaignItem; }
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.RelationshipCampaignManager; }
		}

		public override Security.SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CampaignManagement; }
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		protected override ResultCountMessage GetNewResultCountMessage()
		{
			return new ResultCountMessage((IFilterControl)EmbeddedControl, MaxDisplayRecords, MaxDisplayRecords);
		}

		protected override void OnAfterPerformSearchCore()
		{
			OnPerformedSearch();
		}

		protected override FilteredGridLoader CreateSearchManager()
			=> new GlbCompanyCampaignFilteredGridLoader(FilterBusinessObject, ResultCountMessage, ModuleDecisionProvider, ID, GetNewFactory, GridCollection.TypeOfElements);

		class GlbCompanyCampaignFilteredGridLoader : FilteredGridLoader
		{
			public GlbCompanyCampaignFilteredGridLoader(FilterStripBusinessObject filterBusinessObject, ResultCountMessage handler, IModuleDecisionProvider provider, ModuleIdentifier moduleId, Func<BusinessObjectFactory> createFactory, Type typeOfElements)
				: base(filterBusinessObject, handler, provider, moduleId, createFactory, typeOfElements)
			{
			}

			protected override void RemoveResultsFromGridCollection(IBusinessObjectCollection gridCollection)
			{
				((BusinessObjectCollection)gridCollection).Load(ZQuery.NoResultQuery);
			}
		}

#if DEBUG
		public
#endif
 IZForm LastShownForm;

		protected override IZForm ShowEditForm(BusinessObject selectedBusinessObject)
		{
			LastShownForm = base.ShowEditForm(selectedBusinessObject);
			return LastShownForm;
		}

		public void DoubleClick()
		{
			base.HandleEnterOrDoubleClick();
		}

		internal static int MaxDisplayRecords => OrganisationsDataRegistry.Instance.DisplayGridMaxRecords.Value;

		protected override int MaxRowsToLoad => MaxDisplayRecords;

		#endregion

		#region IGlbCompanyCampaignItemModule

		public MasterFiles.Integration.IGlbCompanyCampaign Campaign
		{
			get;
			set;
		}

		#endregion
	}
}

using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class GlbCompanyCampaignContactFilterControl : ZFilterStripControl
	{
		public GlbCompanyCampaignContactFilterControl(IBusinessObjectCollection collection, GlbCompanyCampaignContactFilterBusinessObject filterBusinessObject, GlbCompanyCampaign campaign)
			: base(collection, filterBusinessObject)
		{
			Campaign = campaign;
			InitializeComponent();
		}
		protected GlbCompanyCampaign Campaign;
		protected StmModuleFilter currentFilter;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new GlbCompanyCampaignContactFilterBusinessObject FilterBusinessObject
		{
			get { return (GlbCompanyCampaignContactFilterBusinessObject)base.FilterBusinessObject; }
		}

		public void SetFilterData(StmModuleFilter filter)
		{
			ClearGridResults();
			SerialiseFilterCustomisation();
			currentFilter = filter;
			(FilterBusinessObject as ISetCampaignFilterLayoutContext).SetContext();
			LoadLastSelectedLayoutIfExists();
		}

		protected override void OnLoad(EventArgs e)
		{
			IsLoadingForLayoutActivation = true;

			FindButtonTextChanging += GlbCompanyCampaignContactFilterControl_FindButtonTextChanging;

			using (BeginContactDataSourceChanging())
			{
				if (!IsImporting)
				{
					SetDefaultDataSource();
				}
			}

			base.OnLoad(e);

			if (Campaign != null)
			{
				Campaign.ContactDataSourceInfo.ValueChanged += ContactDataSourceInfo_ValueChanged;

				if (Campaign.ImportOrgCode.IsEmpty && Campaign.ImportContactName.IsEmpty)
				{
					LoadLastSelectedLayoutIfExists();
				}
			}
			HideLastSelectedLayoutUnderFindButton();

			IsLoadingForLayoutActivation = false;
			IsImporting = false;
		}

		internal bool IsLoadingForLayoutActivation;

#if DEBUG
		internal
#endif
		void GlbCompanyCampaignContactFilterControl_FindButtonTextChanging(object sender, EventArgs e)
		{
			if (!IsLoadingForLayoutActivation && Campaign != null)
			{
				Campaign.FilterLayoutHasChanges = true;
			}
		}

		protected class ContactDataSourceChanger : IDisposable
		{
			public ContactDataSourceChanger(GlbCompanyCampaignContactFilterBusinessObject filterBusinessObject)
			{
				this.filterBusinessObject = filterBusinessObject;
			}

			readonly GlbCompanyCampaignContactFilterBusinessObject filterBusinessObject;

			protected virtual void Dispose(bool isDisposing)
			{
			}

			public void Dispose()
			{
				filterBusinessObject.IsContactDataSourceChanging = false;
				Dispose(true);
				GC.SuppressFinalize(this);
			}
		}

		IDisposable BeginContactDataSourceChanging()
		{
			FilterBusinessObject.IsContactDataSourceChanging = true;
			return new ContactDataSourceChanger(FilterBusinessObject);
		}

		protected virtual void SetDefaultDataSource()
		{
			var defaultDataSource = (Campaign != null && Campaign.IsInDatabase) ? ZString.Empty : ContactDataSourceList.Codes.ClientIntelligence;
			CampaignContactFilterDataSourceHelper.UpdateFilterBusinessObjectLayoutContext(FilterBusinessObject, Campaign, defaultDataSource);
		}

		void ContactDataSourceInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!Campaign.ContactDataSourceInfo.HasErrors() && !FilterBusinessObject.IsContactDataSourceChanging)
			{
				((IFilterStripBusinessObjectInternals)FilterBusinessObject).LayoutContext = FilterBusinessObject.CurrentLayoutContext;
				LoadDefaultLayoutAndHideItUnderFindButton();
			}
		}

		void LoadDefaultLayoutAndHideItUnderFindButton()
		{
			base.ReloadFindDropList();
			HideLastSelectedLayoutUnderFindButton();
			LoadLastSelectedLayoutIfExists();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "A programmatic constant for filter description")]
		void LoadLastSelectedLayoutIfExists()
		{
			var lastSelectedLayout = currentFilter ?? FilterBusinessObject.GetLastUsedLayout();

			if (lastSelectedLayout != null)
			{
				FilterBusinessObject.LoadLayout(lastSelectedLayout);

				if (lastSelectedLayout.S9_FilterNameMultilingual.IsEmpty)
				{
					UpdateFindButtonText(null);
				}
				else
				{
					UpdateFindButtonText(lastSelectedLayout);
				}
			}
			else
			{
				var systemDefaultLayout = FilterBusinessObject.FindLayout("System Default Layout", true);
				if (systemDefaultLayout != null)
				{
					FilterBusinessObject.LoadLayout(systemDefaultLayout);
					UpdateFindButtonText(systemDefaultLayout);
				}
				else
				{
					FilterBusinessObject.ResetCampaignModuleFilters();
					ResetFilterStrips();
					FilterBusinessObject.LoadLayout(null);
				}
			}
		}

		void HideLastSelectedLayoutUnderFindButton()
		{
			foreach (ToolStripMenuItem item in ToolStripFindDropButton.DropDownItems)
			{
				var itemFilter = item.Tag as StmModuleFilter;
				if (itemFilter != null && itemFilter.S9_FilterNameMultilingual.IsEmpty)
				{
					ToolStripFindDropButton.DropDownItems.Remove(item);
					break;
				}
			}
		}

		void CreateNewFilterIfNeeded(int index)
		{
			if (index > 0)
			{
				AddNewFilterStrip();
			}
		}

		bool IsImporting;

		public void InitializeFilterStripsOnImport()
		{
			base.OnLoad(EventArgs.Empty);

			IsImporting = true;

			int index = 0;

			if (!Campaign.ImportOrgCode.IsEmpty || !Campaign.ImportContactName.IsEmpty || !Campaign.ImportInquiryPK.IsEmpty)
			{
				ResetFilterStrips();
			}

			if (!Campaign.ImportInquiryPK.IsEmpty)
			{
				FilterBusinessObject.FilterStrips[index].FilterDescription = ((ModuleGuidFilter)FilterBusinessObject["Inquiry PK"]).Description;
				((ModuleGuidFilter)FilterBusinessObject.FilterStrips[index].CurrentModuleFilter).Property = Campaign.ImportInquiryPK;
				((ModuleGuidFilter)FilterBusinessObject.FilterStrips[index].CurrentModuleFilter).ReadOnly = true;
				CampaignContactFilterDataSourceHelper.UpdateFilterBusinessObjectLayoutContext(FilterBusinessObject, Campaign, ContactDataSourceList.Codes.Inquiries);
				index++;
			}

			if (!Campaign.ImportOrgCode.IsEmpty)
			{
				CreateNewFilterIfNeeded(index);
				FilterBusinessObject.FilterStrips[index].FilterDescription = ((ModuleNkFilter)FilterBusinessObject["Organization"]).Description;
				((ModuleNkFilter)FilterBusinessObject.FilterStrips[index].CurrentModuleFilter).SqlComparisonOperator = SQLComparisonOperator.Equal;
				((ModuleNkFilter)FilterBusinessObject.FilterStrips[index].CurrentModuleFilter).Property = Campaign.ImportOrgCode;
				index++;
			}

			if (!Campaign.ImportContactName.IsEmpty)
			{
				CreateNewFilterIfNeeded(index);
				FilterBusinessObject.FilterStrips[index].FilterDescription = ((ModuleTextFilter)FilterBusinessObject["Contact Name"]).Description;
				((ModuleTextFilter)FilterBusinessObject.FilterStrips[index].CurrentModuleFilter).SqlComparisonOperator = SQLComparisonOperator.Equal;
				((ModuleTextFilter)FilterBusinessObject.FilterStrips[index].CurrentModuleFilter).Property = Campaign.ImportContactName;
			}
		}

		public void SaveDefaultLayout()
		{
			SerialiseFilterCustomisation();
			if (!Campaign.CampaignHasChanges && Campaign.FilterLayoutHasChanges)
			{
				Campaign.FilterLayoutHasChanges = false;
				Campaign.Factory.Save();
			}

			if (Campaign.IsInDatabase)
			{
				var layout = ToolStripFindDropButton.Tag as StmModuleFilter;

				FilterBusinessObject.SetCampaignFilterLayoutContext();

				StmModuleFilter newLayout = new DataGridLayoutManager().SavePreconfiguredLayout(FilterBusinessObject, "", false, true, SaveColumnLayout.Ignore);
				if (layout == null)
				{
					layout = newLayout;
					Grid.CurrentColumnLayout = layout;
				}
				FilterBusinessObject.SaveLastUsedLayout(layout.PK);
			}
		}

		void SerialiseFilterCustomisation()
		{
			if (currentFilter != null && !currentFilter.IsDeleted)
			{
				((IModifyModuleAndGridLayout)FilterBusinessObject).SerialiseLayoutAndWriteTo(currentFilter);
			}
		}

		protected override ZBool ShouldPerformSearch()
		{
			ZBool result = base.ShouldPerformSearch();
			if (Campaign != null && Campaign.IsUsingCampaignTrackingDataSource)
			{
				string message;
				if (!Campaign.IsTouchCampaign)
				{
					Campaign.Validation.ValidateSourceCampaignPK();
					result = (ZBool)!Campaign.SourceCampaignPKInfo.HasErrors();
					message = Campaign.SourceCampaignPKInfo.GetErrors().GetFirstMessage();
				}
				else
				{
					Campaign.Validation.ValidateTouchSourceCampaignPKsForValidation();
					result = (ZBool)!Campaign.TouchSourceCampaignPKsForValidationInfo.HasErrors();
					message = Campaign.TouchSourceCampaignPKsForValidationInfo.GetErrors().GetFirstMessage();
				}

				if (!result && !string.IsNullOrEmpty(message))
				{
					Globals.Message.Show(message);
				}
			}
			return result;
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new CampaignFilterStrip();
		}

		protected override int MaximumAllowableQueriesPerSqlStatement
		{
			get { return Campaign != null ? Campaign.MaxDisplayRecords : Int32.MaxValue; }
		}

		protected override void SaveLayout()
		{
			var initial = FilterBusinessObject.layoutsHelper.BizObjPK;
			FilterBusinessObject.layoutsHelper.BizObjPK = Enterprise.Environment.Env.CurrentUser.PK;
			base.SaveLayout();

			FilterBusinessObject.layoutsHelper.BizObjPK = initial;
		}

		public override void ToolStripFindDropButton_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			var layout = e.ClickedItem.Tag as StmModuleFilter;
			var initial = FilterBusinessObject.layoutsHelper.BizObjPK;
			if (layout != null && !layout.S9_FilterNameMultilingual.IsEmpty)
			{
				FilterBusinessObject.layoutsHelper.BizObjPK = Enterprise.Environment.Env.CurrentUser.PK;
			}
			base.ToolStripFindDropButton_DropDownItemClicked(sender, e);

			FilterBusinessObject.layoutsHelper.BizObjPK = initial;
		}

#if DEBUG
		public ZToolStripSplitButton ToolStripFindDropButtonExposed
		{
			get { return base.ToolStripFindDropButton; }
		}

#endif

		protected override int MaxFilterStripPanelHeight
		{
			get
			{
				return !IsDripMarketingMode
					? base.MaxFilterStripPanelHeight
					: 300;
			}
		}

		public bool IsDripMarketingMode { get; internal set; }

		public void HideDropDownItems()
		{
			ToolStripFindDropButton.DropDownItems.Clear();
		}
	}
}

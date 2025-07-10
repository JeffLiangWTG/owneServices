using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignLookups : AutoGlbCompanyCampaignLookups
	{
		public GlbCompanyCampaignLookups(BusinessObjectFactory factory)
			: this((AutoGlbCompanyCampaign)null)
		{
			this.factory = factory;
		}

		public GlbCompanyCampaignLookups(AutoGlbCompanyCampaign parent)
			: base(parent)
		{
		}

		public new GlbCompanyCampaign Parent
		{
			get { return (GlbCompanyCampaign)base.Parent; }
		}

		#region CampaignTypeList

		public ReadOnlyCodeDescriptionPairList CampaignTypeList => GetNewCampaignTypeList();

		public ReadOnlyCodeDescriptionPairList DripMarketingTouchTypeList => GetDripMarketingTouchTypeList();

		public ReadOnlyCodeDescriptionPairList InsideSalesTouchTypeList => GetInsideSalesTouchTypeList();

		protected virtual CampaignTypeList GetNewCampaignTypeList()
		{
			return new CampaignTypeList();
		}

		DripMarketingTouchTypeList GetDripMarketingTouchTypeList()
		{
			return new DripMarketingTouchTypeList();
		}

		InsideSalesTouchTypeList GetInsideSalesTouchTypeList()
		{
			return new InsideSalesTouchTypeList();
		}

		#endregion

		public ReadOnlyCodeDescriptionPairList CampaignStages
		{
			get { return OrganisationsDataRegistry.Instance.CampaignStageList.Value; }
		}

		public virtual ReadOnlyCodeDescriptionPairList ActiveMediaTypesList
		{
			get
			{
				return Factory.GetCachedValue("GlbCompanyCampaignLookups.ActiveMediaTypesList", () =>
				{
					return OrganisationsDataRegistry.Instance.CampaignCategory2List.Value.GetActiveCodeDescriptionPairList();
				});
			}
		}

		public virtual ReadOnlyCodeDescriptionPairList MediaTypesList
		{
			get
			{
				return Factory.GetCachedValue("GlbCompanyCampaignLookups.MediaTypesListForDisplay", () =>
				{
					return OrganisationsDataRegistry.Instance.CampaignCategory2List.Value.GetCodeDescriptionPairList();
				});
			}
		}

		public RefUNLOCOCollection EmailSenderUnlocoList
		{
			get { return Factory.GetCachedValue("GlbCompanyCampaignLookups.EmailSenderUnlocoList", () => { return new RefUNLOCOCollection(Factory); }); }
		}

		public virtual string MediaTypeLabel
		{
			get
			{
				return OrganisationsDataRegistry.Instance.CampaignCategory2Label.Value.ToString();
			}
		}

		public virtual ReadOnlyCodeDescriptionPairList ActiveMediaCategoryList
		{
			get
			{
				return Factory.GetCachedValue("GlbCompanyCampaignLookups.ActiveMediaCategoryList", () =>
				{
					return OrganisationsDataRegistry.Instance.CampaignCategory1List.Value.GetActiveCodeDescriptionPairList();
				});
			}
		}

		public virtual ReadOnlyCodeDescriptionPairList MediaCategoryList
		{
			get
			{
				return Factory.GetCachedValue("GlbCompanyCampaignLookups.MediaCategoryList", () =>
				{
					return OrganisationsDataRegistry.Instance.CampaignCategory1List.Value.GetCodeDescriptionPairList();
				});
			}
		}

		public ReadOnlyCodeDescriptionPairList PublishedList
		{
			get
			{
				return Factory.GetCachedValue("GlbCompanyCampaignLookups.PublishedList" + Parent.IsHRCampaign, () =>
				{
					var codeList = OrganisationsDataRegistry.Instance.SubscriptionRules.Value.GetCodeDescriptionPairList();

					foreach (SubscriptionRule item in OrganisationsDataRegistry.Instance.SubscriptionRules.Value)
					{
						if (item.CampaignType == (Parent.IsHRCampaign ?
						SubscriptionRuleCampaignTypeList.Codes.ClientRelationshipManagement :
						SubscriptionRuleCampaignTypeList.Codes.HumanResourcesManagement))
						{
							codeList.RemoveCode(item.Code);
						}
					}

					return codeList;
				});
			}
		}

		public ReadOnlyCodeDescriptionPairList PublishedDescriptionList
		{
			get
			{
				return Factory.GetCachedValue("GlbCompanyCampaignLookups.PublishedDescriptionList" + Parent.IsHRCampaign, () =>
				{
					var descCodePairList = new CodeDescriptionPairList();

					foreach (SubscriptionRule item in OrganisationsDataRegistry.Instance.SubscriptionRules.Value)
					{
						if (item.CampaignType == (Parent.IsHRCampaign ?
						SubscriptionRuleCampaignTypeList.Codes.HumanResourcesManagement :
						SubscriptionRuleCampaignTypeList.Codes.ClientRelationshipManagement))
						{
							descCodePairList.Add(new CodeDescriptionPair(item.Description, item.Code));
						}
					}

					return descCodePairList;
				});
			}
		}

		public SubscriptionRule DefaultPublishedList
		{
			get
			{
				return Parent.IsHRCampaign
					? OrganisationsDataRegistry.Instance.SubscriptionRules.Value.DefaultHRM
					: OrganisationsDataRegistry.Instance.SubscriptionRules.Value.DefaultCRM;
			}
		}

		public virtual CodeDescriptionPairList EmailSenderOptionList
		{
			get
			{
				return new EmailSenderOptionCodeDescriptionList();
			}
		}

		public ReadOnlyCodeDescriptionPairList EmailSenderRoleList
		{
			get
			{
				return Enterprise.ZArchitecture.Environment.DataRegistry.Instance.OrgStaffMemberAssignmentRoles;
			}
		}

		public virtual string MediaCategoryLabel
		{
			get
			{
				return OrganisationsDataRegistry.Instance.CampaignCategory1Label.Value.ToString();
			}
		}

		public OrgHeaderCollection FilteredOrganisations
		{
			get { return filteredOrganisations ?? (filteredOrganisations = new OrgHeaderCollection(Factory)); }
		}
		OrgHeaderCollection filteredOrganisations;

		public CodeDescriptionPairList ContactDataSourceList
		{
			get { return new ContactDataSourceList(Parent); }
		}

		public virtual VoteExamSurveyAnswerTypeList DefaultAnswerTypes
		{
			get { return new VoteExamSurveyAnswerTypeList(Parent); }
		}

		public OrgOpportunityCollection Opportunities
		{
			get { return new OrgOpportunityCollection(Factory); }
		}

		public GlbCompanyCampaignCollection SourceCampaigns
		{
			get { return Parent.IsHRCampaign ? new HRGlbCompanyCampaignCollection(Factory) : new GlbCompanyCampaignCollection(Factory); }
		}

		protected override BusinessObjectFactory Factory
		{
			get { return (Parent != null) ? base.Factory : factory; }
		}

		readonly BusinessObjectFactory factory;
	}
}

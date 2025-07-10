using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Recruiter;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	#region GlbCompanyCampaignTypeDecider

	public class GlbCompanyCampaignTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(GlbCompanyCampaign);
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var campaignType = row[GlbCompanyCampaignSchema.Constants.G0_BroadcastVoteSurveyExam].ToString();

			if (campaignType == Core.Constants.Recruiter.LearningCentreCampaignType)
			{
				return ObjectFactory.GetType<ILearningCentreCampaign>();
			}
			else
			{
				var isSalesAndMarketing = row[GlbCompanyCampaignSchema.Constants.G0_IsSalesAndMarketing].ToString();

				if (isSalesAndMarketing == false.ToString())
				{
					return ObjectFactory.GetType<IHRGlbCompanyCampaign>();
				}
				else
				{
					return typeof(GlbCompanyCampaign);
				}
			}
		}

		public override Type GetTypeForNew()
		{
			return typeof(GlbCompanyCampaign);
		}
	}

	#endregion

	[UserDefinedValues]
	[CodeProperty(GlbCompanyCampaign.Schema.G0_CampaignID), DescriptionProperty(GlbCompanyCampaign.Schema.G0_CampaignName)]
	public class GlbCompanyCampaign : AutoGlbCompanyCampaign,
		IGlbCompanyCampaign,
		IDocManagerSupport,
		IDocumentSupportable,
		ITemplateCopyable,
		IWorkflowProvider,
		ICustomFieldProvider,
		ISalesRelationActivity,
		ISuperRelatableActivity,
		IImportParentRelatedActivityInfoOnNew,
		INotifyPropertyChanged,
		IImportChildRelatedActivityInfoOnAttach,
		IImportChildRelatedActivityInfoOnDetach
	{
		public static readonly GlbCompanyCampaignTypeDecider TypeDecider = new GlbCompanyCampaignTypeDecider();

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public GlbCompanyCampaign(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ContactDataSource = ContactDataSourceList.Codes.ClientIntelligence;
			CampaignHasChanges = false;
		}

		#region Schema

		public new class Schema : AutoGlbCompanyCampaign.Schema
		{
			public const string CampaignID = "CampaignID";
			public const string IsDocumentAttached = "IsDocumentAttached";
			public const string CampaignUnitsSent = "CampaignUnitsSent";
			public const string CostPerUnit = "CostPerUnit";
			public const string TotalCost = "TotalCost";
			public const string SearchRecordsFoundMessage = "SearchRecordsFoundMessage";
			public const string FilteredOrgPK = "FilteredOrgPK";
			public const string FilteredContactPK = "FilteredContactPK";
		}

		#endregion

		#region BusinessObject Overrides

		const int DefaultBatchCount = 100;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			G0_GC = GlbCompany.CurrentCompany.PK;
			G0_RX_NKCampaignCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			G0_BatchCountDefault = DefaultBatchCount;
			G0_QuestionsPerWebPage = 20;
			useCampaignName = true;
			G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			useEmailSenderAddressAsReplyTo = true;
			G0_IsSalesAndMarketing = true;
			G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			G0_StoreEmailInEDocs = OrganisationsDataRegistry.Instance.PermanentlySaveAgainstRecipientDefault.Value;
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				if (IsDeleted)
				{
					return Res.GetString("ecb50101-7e70-43a3-ac92-67f644b8bbb4", "Campaign");
				}
				else
				{
					return Res.GetString("e708d3df-6597-4748-b758-827661dec951", "Campaign ({0})", G0_CampaignID);
				}
			}
		}

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				if (IsDeleted)
				{
					return Res.GetString("ecb50101-7e70-43a3-ac92-67f644b8bbb4", "Campaign");
				}
				else
				{
					return G0_CampaignID + " - " + G0_CampaignNameMultilingual;
				}
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			if (IsTouchCampaign && (FilterLayoutHasChanges || TransitionRulesToThisCampaign.Any(rule => rule.HasChanges)))
			{
				G0_SystemLastEditTimeUtc = ZDateTime.UtcNow;
				G0_SystemLastEditUser = Env.CurrentUser.Initials;

				DefibrillateTransitions(false);
			}

			if (IsMasterCampaign)
			{
				CalculateTouchGrouping();
			}

			if (ShouldCreateTasksAndMilestonesFromTemplate)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
				OnSaveAssociatedObject();
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void CalculateTouchGrouping()
		{
			foreach (var horizontal in Horizontals)
			{
				foreach (var campaign in horizontal.Campaigns.OrderBy(i => i.G0_VerticalId))
				{
					var campaignGroup = !campaign.G0_GCG_Group.IsEmpty
						? campaign.CurrentGroup
						: null;

					//no grouping
					if (campaignGroup == null && campaign.CurrentGroupColor == 0)
					{
						continue;
					}

					if (campaignGroup == null || campaignGroup.GCG_GroupColor != campaign.CurrentGroupColor)
					{
						//removed from group
						if (campaignGroup != null && campaign.CurrentGroupColor.IsEmpty)
						{
							campaign.G0_GCG_Group = ZGuid.Empty;
							campaign.transitionRulesToThisCampaign = null;
							campaign.transitionRulesFromThisCampaign = null;
							campaign.sendSettings = null;
							campaign.nextTouches = null;
							nextTouches = null;

							if (!AllTouches.Any(t => t.G0_GCG_Group == campaignGroup.PK))
							{
								campaignGroup.Delete();
							}

							campaign.TransitionRulesToThisCampaign.AddNew();
							continue;
						}

						var allGroups = horizontal.Campaigns.Where(c => !c.G0_GCG_Group.IsEmpty).Select(c => c.CurrentGroup);
						var existingGroup = allGroups.FirstOrDefault(g => g.GCG_GroupColor == campaign.CurrentGroupColor);

						//creating a new group
						if (existingGroup == null || existingGroup.IsDeleted)
						{
							campaignGroup = Factory.New<GlbCompanyCampaignGroup>();
							campaignGroup.GCG_GroupColor = campaign.CurrentGroupColor;

							foreach (var rule in campaign.TransitionRulesToThisCampaign.ToArray())
							{
								rule.GCD_G0_NextTouch = ZGuid.Empty;
								rule.GCD_GCG_Group = campaignGroup.PK;

								var query = new ZQuery(StmModuleFilterUserDataSchema.S0_S9, rule.FilterRule.PK);
								var userData = Factory.Load<StmModuleFilterUserData>(query);
								if (userData.Length > 0)
								{
									userData[0].S0_RelatedEntityID = rule.GCD_GCG_Group;
								}
							}

							campaign.SendSettings.GSC_G0_Campaign = ZGuid.Empty;
							campaign.SendSettings.GSC_GCG_Group = campaignGroup.PK;

							campaign.G0_GCG_Group = campaignGroup.PK;
							campaign.transitionRulesToThisCampaign = null;
							campaign.transitionRulesFromThisCampaign = null;
							campaign.sendSettings = null;
							campaign.nextTouches = null;
							nextTouches = null;
						}
						else //assigning to existing group
						{
							if (campaign.G0_GCG_Group.IsEmpty)
							{
								campaign.TransitionRulesToThisCampaign.DeleteAll();
								campaign.SendSettings.Delete();
							}

							campaign.transitionRulesToThisCampaign = null;
							campaign.transitionRulesFromThisCampaign = null;
							campaign.sendSettings = null;
							campaign.nextTouches = null;
							nextTouches = null;

							campaign.G0_GCG_Group = existingGroup.PK;

							if (campaignGroup != null)
							{
								if (!AllTouches.Any(t => t.G0_GCG_Group == campaignGroup.PK))
								{
									campaignGroup.Delete();
								}
							}
						}
					}
				}

				ReOrderHorizontal(horizontal);
			}
		}

		void ReOrderHorizontal(CampaignHorizontal horizontal)
		{
			var foundGroups = new List<ZGuid>();
			for (int i = 0; i < horizontal.Campaigns.Count; i++)
			{
				var campaign = horizontal.Campaigns[i];
				if (!campaign.G0_GCG_Group.IsEmpty)
				{
					if (!foundGroups.Contains(campaign.G0_GCG_Group))
					{
						foundGroups.Add(campaign.G0_GCG_Group);
					}
					else
					{
						bool finished = false;
						do
						{
							int index = horizontal.Campaigns.IndexOf(campaign);

							if (index > 0)
							{
								var prev = horizontal.Campaigns[index - 1];
								if (prev.G0_GCG_Group != campaign.G0_GCG_Group)
								{
									ChangeTouchPosition(campaign, prev);
								}
								else
								{
									finished = true;
								}
							}
							else
							{
								finished = true;
							}
						} while (!finished);
					}
				}
			}
		}

		public bool ChangeTouchPosition(GlbCompanyCampaign campaign, GlbCompanyCampaign target)
		{
			if (!IsMasterCampaign)
			{
				return false;
			}

			var currentVertical = campaign.G0_VerticalId;
			if (currentVertical.IsEmpty)
			{
				campaign.G0_VerticalId = ZString.AlphabeticCharacters[0].ToString();
				return true;
			}

			if (campaign.G0_HorizontalId != target.G0_HorizontalId)
			{
				return false;
			}

			var horizontalSource = Horizontals.FirstOrDefault(h => h.Id == campaign.G0_HorizontalId);
			var horizontalDestination = Horizontals.FirstOrDefault(h => h.Id == target.G0_HorizontalId);

			if (horizontalSource == null || horizontalDestination == null)
			{
				return false;
			}

			int removeIndex = IndexOf(horizontalSource.Campaigns, campaign.PK);
			int targetIndex = IndexOf(horizontalDestination.Campaigns, target.PK);

			if (removeIndex < targetIndex || horizontalSource != horizontalDestination)
			{
				horizontalDestination.Campaigns.Insert(targetIndex + 1, campaign);
				horizontalSource.Campaigns.RemoveAt(removeIndex);
			}
			else
			{
				int remIdx = removeIndex + 1;
				if (horizontalSource.Campaigns.Count + 1 > remIdx)
				{
					horizontalDestination.Campaigns.Insert(targetIndex, campaign);
					horizontalSource.Campaigns.RemoveAt(remIdx);
				}
			}

			RecalculateIds(horizontalSource.Campaigns);
			if (horizontalSource != horizontalDestination)
			{
				RecalculateIds(horizontalDestination.Campaigns);
			}

			campaign.G0_HorizontalId = target.G0_HorizontalId;

			return true;
		}

		void RecalculateIds(IList<GlbCompanyCampaign> campaigns)
		{
			int index = 0;
			for (int i = 0; i < campaigns.Count; i++)
			{
				var campaign = campaigns[i];
				var proposedId = ZString.AlphabeticCharacters[index++].ToString();
				if (!campaign.G0_VerticalId.EqualsIgnoringCase(proposedId))
				{
					campaigns.RemoveAt(i);
					campaign.G0_VerticalId = proposedId;
					campaigns.Insert(i, campaign);
				}
			}
		}

		int IndexOf(IReadOnlyList<GlbCompanyCampaign> collection, ZGuid pk)
		{
			for (int i = 0; i < collection.Count; i++)
			{
				if (collection[i].PK == pk)
				{
					return i;
				}
			}

			return -1;
		}

		ZDBOnlyQuery GetQueryToRecalculateQueuedTransitions()
		{
			var query = new ZDBOnlyQuery(typeof(GlbCompanyCampaignItem));
			query.AddToFilter(GlbCompanyCampaignItemSchema.G8_TrackingStatus, TrackingStatusCodes.Codes.QUE);
			query.AddToFilter(GlbCompanyCampaignItemSchema.G8_IsCheckTransitionRequired, ZBool.False);

			var campaignSubQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaign), GlbCompanyCampaignItemSchema.G8_G0);
			campaignSubQuery.AddToFilter(GlbCompanyCampaignSchema.G0_HorizontalId, SQLComparisonOperator.GreaterThanOrEqualTo, G0_HorizontalId);

			campaignSubQuery.AddToFilter(GlbCompanyCampaignSchema.G0_G0_Master, G0_G0_Master);

			query.AddSubQuery(campaignSubQuery, JoinCondition.And);
			return query;
		}

		protected virtual bool ShouldCreateTasksAndMilestonesFromTemplate
		{
			get { return HasChanges; }
		}

		public bool FilterLayoutHasChanges
		{
			get { return filterLayoutHasChanges; }
			set
			{
				filterLayoutHasChanges = value;
				bool originalCampaignHasChanges = CampaignHasChanges;
				HasChanges = filterLayoutHasChanges || CampaignHasChanges;
				CampaignHasChanges = originalCampaignHasChanges;
			}
		}
		bool filterLayoutHasChanges;

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;
				CampaignHasChanges = value;
			}
		}

		public bool CampaignHasChanges
		{
			get
			{
				if (!FilterLayoutHasChanges && !HasChanges)
				{
					campaignHasChanges = false;
				}
				return campaignHasChanges;
			}
			private set
			{
				if (!value || !IsSettingHasChangesSuspended)
				{
					campaignHasChanges = value;
				}
			}
		}
		bool campaignHasChanges;

		public override void OnLoaded()
		{
			base.OnLoaded();
			MarkAsNeedingValidation();
			useCampaignName = G0_EmailSubject == (ZString)G0_CampaignNameMultilingual;
			useEmailSenderAddressAsReplyTo = G0_ReplyToEmail.IsEmpty;

			var group = CurrentGroup;
			if (group != null)
			{
				using (SuspendSettingHasChanges())
				{
					currentGroupColor = group.GCG_GroupColor;
				}
			}
		}

		#endregion

		#region Related Business Objects

		#region Sent Items

		[ChildEditable]
		public GlbCompanyCampaignItemCampaignDependentCollection CampaignsItemsSent
		{
			get
			{
				if (campaignsItemsSent == null)
				{
					campaignsItemsSent = GetNewCampaignItemCollection();
					campaignsItemsSent.Load();
					RegisterEditableChildObject(campaignsItemsSent);
				}

				return campaignsItemsSent;
			}
		}

		protected virtual GlbCompanyCampaignItemCampaignDependentCollection GetNewCampaignItemCollection()
		{
			return new GlbCompanyCampaignItemCampaignDependentCollection(this);
		}

		internal GlbCompanyCampaignItemCampaignDependentCollection campaignsItemsSent;

		public virtual Type TypeOfSentItem => typeof(GlbCompanyCampaignItem);

		internal GlbCompanyCampaignItemCampaignDependentCollection CampaignItemsSentNotSaved;

		[ChildEditable]
		public GlbCompanyCampaignItemCampaignDependentCollection CampaignsItemsSentForDisplayOnly
		{
			get
			{
				if (campaignsItemsSentForDisplayOnly == null)
				{
					campaignsItemsSentForDisplayOnly = GetNewCampaignItemCollection();
					RegisterEditableChildObject(campaignsItemsSentForDisplayOnly);
				}

				return campaignsItemsSentForDisplayOnly;
			}
		}

		GlbCompanyCampaignItemCampaignDependentCollection campaignsItemsSentForDisplayOnly;

		#endregion

		#region Budget Items

		[ChildEditable(false)]
		public GlbCompanyCampaignBudgetItemCollection BudgetItems
		{
			get
			{
				if (fBudgetItems == null)
				{
					fBudgetItems = new GlbCompanyCampaignBudgetItemCollection(this);
					fBudgetItems.Load();
					((IBindingList)fBudgetItems).ListChanged += new ListChangedEventHandler(GlbCompanyCampaign_ListChanged);
					RegisterEditableChildObject(fBudgetItems);
				}

				return fBudgetItems;
			}
		}

		GlbCompanyCampaignBudgetItemCollection fBudgetItems;

		void GlbCompanyCampaign_ListChanged(object sender, ListChangedEventArgs e)
		{
			this.TotalCostInfo.RefreshBinding();
		}

		#endregion

		#region Filter Items

		public GlbCampaignContactCollection FilteredContacts
		{
			get
			{
				if (filteredContacts == null)
				{
					filteredContacts = new GlbCampaignContactCollection(this);
				}

				return filteredContacts;
			}
		}
		GlbCampaignContactCollection filteredContacts;

		#endregion

		#region Attachment Items

		[ChildEditable(true)]
		public GlbCompanyCampaignAttachmentItemCollection CampaignAttachments
		{
			get
			{
				if (fCampaignAttachments == null)
				{
					fCampaignAttachments = new GlbCompanyCampaignAttachmentItemCollection(this);
					fCampaignAttachments.Load();
					RegisterEditableChildObject(fCampaignAttachments);
				}

				return fCampaignAttachments;
			}
		}

		GlbCompanyCampaignAttachmentItemCollection fCampaignAttachments;

		#endregion

		#region Opportunities

		[ChildEditable]
		public OrgOpportunityCampaignDependentCollection Opportunities
		{
			get
			{
				if (fOpportunities == null)
				{
					fOpportunities = new OrgOpportunityCampaignDependentCollection(this);
					RegisterEditableChildObject(fOpportunities);
				}

				return fOpportunities;
			}
		}

		OrgOpportunityCampaignDependentCollection fOpportunities;

		public OpportunityCreationTemplate OpportunityCreationTemplate
		{
			get
			{
				if (opportunityCreationTemplate == null)
				{
					opportunityCreationTemplate = new OpportunityCreationTemplate(this);
					RegisterEditableChildObject(opportunityCreationTemplate);
				}
				return opportunityCreationTemplate;
			}
		}
		OpportunityCreationTemplate opportunityCreationTemplate;

		#endregion

		#region Tracked Links

		[ChildEditable]
		public GlbCompanyCampaignLinkCollection TrackedLinks
		{
			get
			{
				if (trackedLinks == null)
				{
					trackedLinks = new GlbCompanyCampaignLinkCollection(this);
					RegisterEditableChildObject(trackedLinks);
				}

				return trackedLinks;
			}
		}

		GlbCompanyCampaignLinkCollection trackedLinks;

		#endregion

		#region Unsubscribe

		public GlbCompanyCampaignSubscriptionForCampaignCollection UnsubscribedCollection
		{
			get
			{
				if (unsubscribedCollection == null)
				{
					unsubscribedCollection = new GlbCompanyCampaignSubscriptionForCampaignCollection(this);
					unsubscribedCollection.SetCountedReadOnlyIncludingChildren(true);
				}
				return unsubscribedCollection;
			}
		}

		GlbCompanyCampaignSubscriptionForCampaignCollection unsubscribedCollection;

		#endregion

		#endregion

		#region Contact Searching

		#region Loading

		public void LoadFilteredContacts(GlbCampaignContactCollection campaignContactCollection)
		{
			ZQuery filterToRun = ContactFilterWithBatchCount;

			AddOrgRestrictionFilterIfApplicable(filterToRun);

			TotalRecordCount = Factory.GetDatabaseCount(typeof(CampaignContact), filterToRun);
			if (NoFilterDefinedAndNoBatchCount && TotalRecordCount > SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value)
			{
				filterToRun = ZQuery.NoResultQuery;
				TotalRecordCount = 0;
			}
			else
			{
				if (G0_DeDuplicateContacts)
				{
					campaignContactCollection.RemoveDuplicatesBasedOnEmailAddress(filterToRun);
				}
				else
				{
					campaignContactCollection.Load(filterToRun);
				}
			}

			filteredContacts = campaignContactCollection;
		}

		public void ReloadFilteredContacts(GlbCampaignContactCollection campaignContactCollection)
		{
			LoadFilteredContacts(campaignContactCollection);
		}

		public void AddOrgRestrictionFilterIfApplicable(ZQuery query)
		{
			if (!Env.Security.OrganisationAllowSearchOutsideLoginCountry.IsAllowed)
			{
				ZDBOnlyQuery viewQuery = new ZDBOnlyQuery(typeof(CampaignContact));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
				subQuery.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				viewQuery.AddSubQuery(ViewCampaignContactSchema.VCC_OH, subQuery, JoinCondition.And);
				query.AddToFilter(viewQuery, JoinCondition.And);
			}
		}

		int TotalRecordCount;

		public bool NoFilterDefinedAndNoBatchCount
		{
			get { return AdditionalFilter.IsEmpty && SendToAll; }
		}

		#endregion

		#region Filters

		#region SuppressResourceStringsCheckRegion

		public ZQuery ExcludeDuplicateQuery(ZQuery innerQuery)
		{
			ZDBOnlyQuery excludeDuplicateQuery = new ZDBOnlyQuery(typeof(CampaignContact));
			var excludeDuplicateSql = string.Format(CultureInfo.InvariantCulture, @"{0} IN
	(
		select PkForMinRank
		FROM
		(
			SELECT {1}, PkForMinRank = min({0})
			FROM
			{3}.{2} contacts
			LEFT JOIN
			(
				SELECT DISTINCT SentEmail = {1} FROM {3}.{2} WHERE {0} IN
				(
					SELECT {4} FROM {7}.{5} WHERE {6} = @CampaignPK
				)
				AND
				{1} != ''
			) sentItems ON contacts.VCC_Email = sentItems.SentEmail
			WHERE
			(
				sentItems.SentEmail is null
			)
			AND
			(
				{1} != '' OR {0} NOT IN
				(
					SELECT {4} FROM {7}.{5} WHERE {6} = @CampaignPK AND {4} is not null
				)
			)
			AND
",
				ViewCampaignContactSchema.Constants.PK,
				ViewCampaignContactSchema.Constants.VCC_Email,
				ViewCampaignContactSchema.Constants.TableName,
				ViewCampaignContactSchema.Constants.SqlSchemaName,
				GlbCompanyCampaignItemSchema.Constants.G8_RecipientID,
				GlbCompanyCampaignItemSchema.Constants.TableName,
				GlbCompanyCampaignItemSchema.Constants.G8_G0,
				GlbCompanyCampaignItemSchema.Constants.SqlSchemaName
			);

			var endingSqlPart = string.Format(CultureInfo.InvariantCulture, @"GROUP BY {0}, case {0} when '' then {1} else null end
		) a
	)",
		ViewCampaignContactSchema.Constants.VCC_Email,
		ViewCampaignContactSchema.Constants.PK);

			var parameters = new ZSqlParameterCollection();

			var innerQueryText = innerQuery.FilterString;
			if (!string.IsNullOrWhiteSpace(innerQueryText))
			{
				parameters.AddRange(innerQuery.Params);
				excludeDuplicateSql += innerQueryText + System.Environment.NewLine + endingSqlPart;
			}
			else
			{
				excludeDuplicateSql += "1=1 " + endingSqlPart;
			}

			parameters.Add(ZSqlParameter.New("@CampaignPK", PK, GlbCompanyCampaignSchema.PK, SQLComparisonOperator.Equal));
			excludeDuplicateQuery.AddFilterAndZSQLParameterCollection(excludeDuplicateSql, parameters, ignoreParameterSuffix: true);

			return excludeDuplicateQuery;
		}

		#endregion

		ZQuery ContactFilterWithBatchCount
		{
			get
			{
				ZQuery contactQuery = ContactFilter;
				contactQuery.MaximumRows = G0_BatchCountDefault;
				return contactQuery;
			}
		}

		public ZQuery GetContactFilter
		{
			get
			{
				return ContactFilterWithBatchCount;
			}
		}

		protected virtual ZQuery ContactFilter
		{
			get
			{
				ZQuery contactQuery = new ZQuery();
				contactQuery.AddToFilter(ContactAndOrgActiveQuery);
				if (!G0_DeDuplicateContacts && !DisableContactsNotSentToQueryCheck)
				{
					contactQuery.AddToFilter(ContactsNotSentToQuery);
				}
				if (!AdditionalFilter.IsEmpty && !additionalFilter.IsNoResultQuery)
				{
					contactQuery.AddToFilter(AdditionalFilter);
				}
				return contactQuery;
			}
		}

		public ZQuery AdditionalFilter
		{
			get
			{
				if (additionalFilter == null)
				{
					additionalFilter = new ZQuery();
				}
				return additionalFilter;
			}
			set
			{
				additionalFilter = value;
			}
		}
		ZQuery additionalFilter;

		public string FormattedFilterText
		{
			get { return AdditionalFilter.LiteralTextADOFormatted; }
		}

		#region Active Query

		ZQuery ContactAndOrgActiveQuery
		{
			get
			{
				var query = new ZDBOnlyQuery(typeof(CampaignContact));
				query.AddToFilter(ViewCampaignContactSchema.VCC_OrgIsActive, ZBool.True);
				query.AddToFilter(JoinCondition.Or, ViewCampaignContactSchema.VCC_OH, null);

				return query;
			}
		}

		#endregion

		#region Contacts Not Sent

		public bool DisableContactsNotSentToQueryCheck
		{
			get;
			set;
		}

		ZQuery ContactsNotSentToQuery
		{
			get
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
				ZDBOnlySubQuery campaignItemSubQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignItem), GlbCompanyCampaignItemSchema.G8_RecipientID, true);
				campaignItemSubQuery.AddToFilter(GlbCompanyCampaignItemSchema.G8_G0, PK);
				query.AddSubQuery(campaignItemSubQuery, JoinCondition.And);

				return query;
			}
		}

		#endregion

		#endregion

		#endregion

		#region Properties

		#region G0_Type

		[List("Lookups.ActiveMediaTypesList")]
		public override ZString G0_Type
		{
			get { return base.G0_Type; }
			set
			{
				base.G0_Type = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCampaignID();
				}
			}
		}

		public bool G0_Type_ReadOnly
		{
			get { return IsTouchCampaign; }
		}

		public ZString MediaTypeDescription
		{
			get { return Lookups.MediaTypesList.GetDescriptionFromCode(G0_Type); }
		}

		#endregion

		#region G0_Type Label

		public ZString MediaTypeLabel
		{
			get { return Lookups.MediaTypeLabel; }
		}

		#endregion

		#region G0_Category

		[List("Lookups.ActiveMediaCategoryList")]
		public override ZString G0_Category
		{
			get { return base.G0_Category; }
			set
			{
				base.G0_Category = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCampaignID();
				}
			}
		}

		public bool G0_Category_ReadOnly
		{
			get { return IsTouchCampaign; }
		}

		public ZString MediaCategoryDescription
		{
			get { return Lookups.MediaCategoryList.GetDescriptionFromCode(G0_Category); }
		}

		[List("Lookups.PublishedList")]
		public override ZString G0_PublishedListCode
		{
			get { return base.G0_PublishedListCode; }
			set { base.G0_PublishedListCode = value; }
		}

		[List("Lookups.PublishedDescriptionList")]
		public ZString PublishedListCodeDescription
		{
			get
			{
				ZString desc = Lookups.PublishedList.GetDescriptionFromCode(G0_PublishedListCode);
				if (string.IsNullOrEmpty(desc))
				{
					var defaultList = Lookups.DefaultPublishedList;
					if (defaultList != null)
					{
						desc = defaultList.Description;
						base.G0_PublishedListCode = defaultList.Code;
					}
				}
				return desc;
			}

			set { G0_PublishedListCode = Lookups.PublishedList.GetCodeFromDescription(value); }
		}

		#endregion

		#region G0_EmailSenderOption

		[List("Lookups.EmailSenderOptionList")]
		public override ZString G0_EmailSenderOption
		{
			get
			{
				return base.G0_EmailSenderOption;
			}
			set
			{
				if (!base.G0_EmailSenderOption.Equals(value))
				{
					base.G0_EmailSenderOption = value;
					G0_SenderEmail = "";

					if (G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.COR)
					{
						G0_EmailSenderName = "";
						G0_EmailSenderRole = ZString.Empty;
						G0_ReplyToEmail = UseEmailSenderAddressAsReplyTo ? ZString.Empty : CoordinatorEmailAddress;
						G0_RL_NKEmailSenderUNLOCO = "";
					}
					else if (G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.EML)
					{
						G0_EmailSenderRole = ZString.Empty;
						G0_ReplyToEmail = UseEmailSenderAddressAsReplyTo ? ZString.Empty : SenderEmailAddress;
					}
					else if (G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.ORG)
					{
						G0_ReplyToEmail = "";
						G0_EmailSenderName = "";
						G0_RL_NKEmailSenderUNLOCO = "";
					}
					else if (G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.SPS)
					{
						G0_ReplyToEmail = "";
						G0_EmailSenderName = "";
						G0_EmailSenderRole = ZString.Empty;
						G0_RL_NKEmailSenderUNLOCO = "";
					}
				}
			}
		}

		#endregion

		#region G0_EmailSenderRole

		[List("Lookups.EmailSenderRoleList")]
		public override ZString G0_EmailSenderRole
		{
			get
			{
				return base.G0_EmailSenderRole;
			}
			set
			{
				base.G0_EmailSenderRole = value;
			}
		}

		protected bool G0_EmailSenderRole_ReadOnly => G0_EmailSenderOption.IsEmpty || G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.COR;

		#endregion

		#region G0_ReplyToEmail

		[EmailAddress]
		public override ZString G0_ReplyToEmail
		{
			get
			{
				return base.G0_ReplyToEmail;
			}
			set
			{
				base.G0_ReplyToEmail = value;
			}
		}

		protected bool G0_ReplyToEmail_ReadOnly
		{
			get { return UseEmailSenderAddressAsReplyTo == ZBool.True; }
		}

		#endregion

		#region G0_RL_NKEmailSenderUNLOCO

		[List("Lookups.EmailSenderUnlocoList")]
		public override ZString G0_RL_NKEmailSenderUNLOCO
		{
			get { return base.G0_RL_NKEmailSenderUNLOCO; }
			set { base.G0_RL_NKEmailSenderUNLOCO = value; }
		}

		#endregion

		#region G0_EstimatedStartedDate

		public override ZDateTime G0_EstimatedStartedDate
		{
			get { return base.G0_EstimatedStartedDate; }
			set
			{
				base.G0_EstimatedStartedDate = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCampaignID();
				}
			}
		}

		#endregion

		#region G0_GS_NKCampaignCoordinator

		public override ZString G0_GS_NKCampaignCoordinator
		{
			get { return base.G0_GS_NKCampaignCoordinator; }
			set
			{
				base.G0_GS_NKCampaignCoordinator = value;
				if (G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.COR)
				{
					G0_SenderEmail = "";
					Validation.ValidateCoordinatorEmailAddress();
				}
			}
		}

		#endregion

		#region UseEmailSenderAddressAsReplyTo

		public ZBool UseEmailSenderAddressAsReplyTo
		{
			get
			{
				return useEmailSenderAddressAsReplyTo;
			}
			set
			{
				SetNonPersistentPropertyValue(UseEmailSenderAddressAsReplyToInfo, ref useEmailSenderAddressAsReplyTo, value);
				if (value == ZBool.True)
				{
					G0_ReplyToEmail = "";
				}
				else
				{
					G0_ReplyToEmail = SenderEmailAddress;
				}
			}
		}

		public ZPropertyInfo UseEmailSenderAddressAsReplyToInfo
		{
			get { return GetZPropertyInfo(nameof(UseEmailSenderAddressAsReplyTo)); }
		}

		ZBool useEmailSenderAddressAsReplyTo;

		#endregion

		#region SenderEmailAddress

		[EmailAddress]
		public ZString SenderEmailAddress
		{
			get
			{
				if (G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.SPS)
				{
					return ZString.Empty;
				}

				if (G0_EmailSenderOption != EmailSenderOptionCodeDescriptionList.Codes.EML && base.G0_SenderEmail.IsEmpty && CampaignCoordinator != null)
				{
					return CampaignCoordinator.GS_EmailAddress;
				}

				if (G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.ORG)
				{
					return ZString.Empty;
				}

				return G0_SenderEmail;
			}
			set
			{
				G0_SenderEmail = value;
			}
		}

		public ZPropertyInfo SenderEmailAddressInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(SenderEmailAddress), x => G0_SenderEmailInfo); }
		}

		protected internal bool SenderEmailAddress_ReadOnly => !(G0_EmailSenderOption.IsEmpty || G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.EML);

		#endregion

		#region CoordinatorEmailAddress

		[List("CoordinatorEmailAddressList")]
		[MaxLength(GlbCompanyCampaign.Schema.G0_SenderEmailMaxLength)]
		public ZString CoordinatorEmailAddress
		{
			get
			{
				if (base.G0_SenderEmail.IsEmpty && CampaignCoordinator != null)
				{
					return CampaignCoordinator.GS_EmailAddress;
				}
				return G0_SenderEmail;
			}
			set
			{
				if (CampaignCoordinator != null && value == CampaignCoordinator.GS_EmailAddress)
				{
					G0_SenderEmail = "";
				}
				else
				{
					G0_SenderEmail = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateCoordinatorEmailAddress();
				}

				CoordinatorEmailAddressInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CoordinatorEmailAddressInfo
		{
			get { return GetZPropertyInfo(nameof(CoordinatorEmailAddress)); }
		}

		public CodeDescriptionPairList CoordinatorEmailAddressList
		{
			get
			{
				var addressList = new CodeDescriptionPairList();

				if (CampaignCoordinator != null)
				{
					foreach (var email in CampaignCoordinator.EmailAddresses)
					{
						addressList.AddPair(email.GSE_Type, email.GSE_EmailAddress);
					}
				}

				return addressList;
			}
		}

		#endregion

		#region

		public override ZBool G0_UseLastEmailSenderAddress
		{
			get { return base.G0_UseLastEmailSenderAddress; }
			set
			{
				base.G0_UseLastEmailSenderAddress = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateContactDataSource();
				}
			}
		}

		#endregion

		#region SenderName

		public ZString SenderName
		{
			get
			{
				if ((G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.COR || base.G0_EmailSenderName.IsEmpty) && CampaignCoordinator != null)
				{
					return CampaignCoordinator.GS_FullName;
				}

				if (G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.ORG || G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.SPS)
				{
					return ZString.Empty;
				}

				return G0_EmailSenderName;
			}
			set
			{
				G0_EmailSenderName = value;
			}
		}

		public ZWrappedPropertyInfo SenderNameInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(SenderName), x => G0_EmailSenderNameInfo); }
		}

		protected internal bool SenderName_ReadOnly => !(G0_EmailSenderOption.IsEmpty || G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.EML);

		#endregion

		#region SenderPool

		[ChildEditable(true)]
		public GlbCompanyCampaignSenderPool SenderPool => senderPool ?? (senderPool = GetSenderPool());

		GlbCompanyCampaignSenderPool senderPool;

		GlbCompanyCampaignSenderPool GetSenderPool()
		{
			var pool = new GlbCompanyCampaignSenderPool(this);
			RegisterEditableChildObject(pool);
			pool.SetCountedReadOnlyIncludingChildren(false);
			return pool;
		}

		#endregion

		#region ReplyToEmailAddress

		public ZString ReplyToEmailAddress
		{
			get
			{
				return UseEmailSenderAddressAsReplyTo ? SenderEmailAddress : G0_ReplyToEmail;
			}
		}

		#endregion

		#region G0_Category Label

		public ZString MediaCategoryLabel
		{
			get { return Lookups.MediaCategoryLabel; }
		}

		#endregion

		#region G0_SystemCreateTimeLocal

		public ZDateTime G0_SystemCreateTimeLocal
		{
			get { return G0_SystemCreateTimeUtc.ToLocalBranchTime(); }
		}

		#endregion

		#region G0_SystemLastEditTimeLocal

		public ZDateTime G0_SystemLastEditTimeLocal
		{
			get { return G0_SystemLastEditTimeUtc.ToLocalBranchTime(); }
		}

		#endregion

		#region Campaign ID

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		[MaxLength(6 + 1 + GlbCompanyCampaign.Schema.G0_CategoryMaxLength + 1 + GlbCompanyCampaign.Schema.G0_TypeMaxLength + 1 + GlbCompanyCampaign.Schema.G0_CampaignNameMaxLength + 1 + 3 + 1)]
		public virtual ZString CampaignID
		{
			get
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(G0_EstimatedStartedDate.IsEmpty ? ZString.Empty : (ZString)G0_EstimatedStartedDate.ToString("yyMMdd"));
				sb.Append("_");
				sb.Append(G0_Category);
				sb.Append("_");
				sb.Append(G0_Type);
				sb.Append("_");
				sb.Append(G0_CampaignName);

				if (IsTouchCampaign)
				{
					sb.Append("_");
					sb.Append(TouchId);
				}

				return sb.ToString();
			}
		}

		public ZPropertyInfo CampaignIDInfo
		{
			get { return GetZPropertyInfo(Schema.CampaignID); }
		}

		public static void GetInfoFromCampaignID(ZString campaignID, out ZDateTime estimatedStartedDate, out ZString category, out ZString type, out ZString campaignName)
		{
			ZString[] values = campaignID.Split('_');

			if (values.Length == 4)
			{
				ZDateTime.TryParseExact(values[0].ToString(), out estimatedStartedDate, "yyMMdd");
				category = values[1];
				type = values[2];
				campaignName = values[3];
			}
			else
			{
				estimatedStartedDate = ZDate.Empty;
				category = ZString.Empty;
				type = ZString.Empty;
				campaignName = ZString.Empty;
			}
		}

		#endregion

		#region Total Cost

		public ZDecimal TotalCost
		{
			get
			{
				ZDecimal flat = 0m;
				ZDecimal unit = 0m;
				foreach (GlbCompanyCampaignBudgetItem item in BudgetItems)
				{
					flat += item.LocalFlatAmount;
					unit += item.LocalPerUnitAmount;
				}

				return flat + (CampaignUnitsSent * unit);
			}
		}

		public ZPropertyInfo TotalCostInfo
		{
			get { return GetZPropertyInfo(Schema.TotalCost); }
		}

		#endregion

		#region Cost Per Unit

		public ZDecimal CostPerUnit
		{
			get { return (ZDecimal)(TotalCost / (CampaignUnitsSent == 0 ? (ZInt)1 : CampaignUnitsSent)); }
		}

		public ZPropertyInfo CostPerUnitInfo
		{
			get { return GetZPropertyInfo(Schema.CostPerUnit); }
		}

		#endregion

		#region Campaign Units Sent

		public ZInt CampaignUnitsSent
		{
			get { return CampaignsItemsSent.Count; }
		}

		public ZPropertyInfo CampaignUnitsSentInfo
		{
			get { return GetZPropertyInfo(Schema.CampaignUnitsSent); }
		}

		#endregion

		#region Document Attached

		public ZBool IsDocumentAttached
		{
			get { return !HtmlDocumentBlob.IsEmpty; }
		}

		public ZPropertyInfo IsDocumentAttachedInfo
		{
			get { return GetZPropertyInfo(Schema.IsDocumentAttached); }
		}

		#endregion

		#region Campaign Name

		[TranslatableDataField(Schema.TableName, Schema.G0_CampaignName, -1, Schema.G0_CampaignName, Type = typeof(GlbCompanyCampaign), Asmid = ResString.AssemblyId)]
		[ReadOnlyMember(nameof(IsVoteSurveyCampaignAndItemsSent))]
		public override ZString G0_CampaignName
		{
			get { return base.G0_CampaignName; }
			set
			{
				base.G0_CampaignName = value;
				if (UseCampaignName)
				{
					G0_EmailSubject = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateCampaignID();
				}
			}
		}

		public MultilingualString G0_CampaignNameMultilingual => GetMultilingual(G0_CampaignNameInfo);

		public ZString G0_CampaignNameLocalized
		{
			get { return (ZString)G0_CampaignNameMultilingual.GetLocalizedValue(Res.CurrentLanguage); }
		}

		#endregion

		#region Campaign Comment

		[TranslatableDataField(Schema.TableName, Schema.G0_CampaignComment, Schema.G0_CampaignCommentMaxLength, Schema.G0_CampaignComment, Type = typeof(GlbCompanyCampaign), Asmid = ResString.AssemblyId)]
		public override ZString G0_CampaignComment
		{
			get { return base.G0_CampaignComment; }
			set { base.G0_CampaignComment = value; }
		}

		public MultilingualString G0_CampaignCommentMultilingual => GetMultilingual(G0_CampaignCommentInfo);

		public ZString G0_CampaignCommentLocalized
		{
			get { return (ZString)G0_CampaignCommentMultilingual.GetLocalizedValue(Res.CurrentLanguage); }
		}

		#endregion

		#region Email Subject

		[ReadOnlyMember(nameof(IsEmailSubjectReadOnly))]
		public override ZString G0_EmailSubject
		{
			get { return base.G0_EmailSubject; }
			set
			{
				base.G0_EmailSubject = value.SubstringSafe(0, GlbCompanyCampaignSchema.G0_EmailSubject.MaxLength);
			}
		}

		#endregion

		#region Use Campaign Name

		bool useCampaignName;

		[ReadOnlyMember(nameof(IsVoteSurveyCampaignAndItemsSent))]
		public bool UseCampaignName
		{
			get
			{
				return useCampaignName;
			}
			set
			{
				useCampaignName = value;
				if (useCampaignName)
				{
					G0_EmailSubject = G0_CampaignNameMultilingual;
				}
				G0_EmailSubjectInfo.RefreshBinding();
			}
		}

		#endregion

		#region Send To All

		public ZBool SendToAll
		{
			get { return G0_BatchCountDefault == MaxDisplayRecords ? ZBool.True : ZBool.False; }
			set
			{
				G0_BatchCountDefault = value ? MaxDisplayRecords : DefaultBatchCount;
				BatchCountForDisplayInfo.RefreshBinding();
				SendToAllInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SendToAllInfo
		{
			get { return GetZPropertyInfo(nameof(SendToAll)); }
		}

		public int MaxDisplayRecords
		{
			get { return int.MaxValue - 1; }  //	We remove 1 to avoid overflow
		}

		#endregion

		#region Batch Count for Display

		[ReadOnlyMember(nameof(SendToAll))]
		public ZInt BatchCountForDisplay
		{
			get { return G0_BatchCountDefault == MaxDisplayRecords ? ZInt.Zero : G0_BatchCountDefault; }
			set
			{
				G0_BatchCountDefault = value;
				BatchCountForDisplayInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BatchCountForDisplayInfo
		{
			get { return GetZPropertyInfo(nameof(BatchCountForDisplay)); }
		}

		#endregion

		#region Data Blob

		public override ZBlob G0_DocumentBlob
		{
			get
			{
				return base.G0_DocumentBlob;
			}
			set
			{
				htmlDocumentBlob = ZBlob.Empty;
				htmlText = null;
				base.G0_DocumentBlob = value;
				Validation.ValidateIsDocumentAttached();
			}
		}

		public void SetDocumentData(ZBlob htmlDocument)
		{
			G0_DocumentBlob = ZipDocumentBlobData(htmlDocument);
		}

		public bool EmailContentIsNotSetButRequired => EmailContentIsNotSet && !IsTargetList && !IsMasterCampaign && !IsOpportunityCreationCampaign;

		public bool EmailContentIsNotSet => CampaignEmailTemplateEditor.IsHtmlEmptyOrTemplateEmpty(HtmlDocumentBlob);

		#region HTML Blob

		public ZBlob HtmlDocumentBlob
		{
			get
			{
				if (htmlDocumentBlob.IsEmpty)
				{
					ParseDocumentBlobData();
				}
				return htmlDocumentBlob;
			}
			set { SetDocumentData(value); }
		}

		public ZWrappedPropertyInfo HtmlDocumentBlobInfo => GetWrappedZPropertyInfo(nameof(HtmlDocumentBlob), x => G0_DocumentBlobInfo);

		public string HtmlTextToSend => htmlText ?? (htmlText = CampaignEmailTemplateEditor.DecodeDocumentBlobAndGetHtmForEmail(HtmlDocumentBlob, this));

		string htmlText;
		ZBlob htmlDocumentBlob;

		const string DocumentFileName = "DocumentHtml.data";

		#endregion

		#region Data loading/saving

		void ParseDocumentBlobData()
		{
			htmlDocumentBlob = GetDataFromDocumentBlob(G0_DocumentBlob, DocumentFileName);
		}

		static ZBlob GetDataFromDocumentBlob(ZBlob documentBlob, string fileName)
		{
			if (documentBlob.IsEmpty)
			{
				return ZBlob.Empty;
			}

			try
			{
				using (var zipStream = new MemoryStream(documentBlob))
				using (var stream = new MemoryStream())
				{
					var extractor = new ZipExtractor();
					extractor.ExtractZipStream(zipStream, stream, fileName);
					return new ZBlob(stream.ToArray());
				}
			}
			catch (Exception e)
			{
				if (e.IsCriticalException() || string.CompareOrdinal(e.Source, SharpzipLibExceptionSourceName) != 0) // The "ResourceStringAnalyzerTask" task failed unexpectedly error if using 'when' construction
				{
					throw;
				}
				return ZBlob.Empty;
			}
		}

		static ZBlob ZipDocumentBlobData(ZBlob htmlBlobToSave)
		{
			if (htmlBlobToSave.IsEmpty)
			{
				return ZBlob.Empty;
			}

			using (var htmlStream = new MemoryStream(htmlBlobToSave))
			{
				using (var zipStream = new MemoryStream())
				{
					var filesToZip = new[]
					{
						new ZipStream(DocumentFileName, htmlStream)
					};

					var creator = new ZipCreator();
					creator.ZipStream(filesToZip, zipStream);
					return new ZBlob(zipStream.ToArray());
				}
			}
		}

		const string SharpzipLibExceptionSourceName = "ICSharpCode.SharpZipLib";

		#endregion

		#endregion

		#region CampaignTypeCaption

		public virtual ZString CampaignTypeCaption
		{
			get
			{
				ZString campaignTypeDescription = Lookups.CampaignTypeList.GetDescriptionFromCode(G0_BroadcastVoteSurveyExam);
				return Res.GetString("6cd663af-0f7c-402a-9819-94c3eace4024", "{0} Campaign", campaignTypeDescription);
			}
		}

		#endregion

		#region StatModel

		public ClickStatModel StatModel
		{
			get
			{
				if (statModel == null)
				{
					statModel = new ClickStatModel(this);
					statModel.ReportBy = ReportByList.Codes.Context;
				}
				return statModel;
			}
		}
		ClickStatModel statModel;

		#endregion

		#region DataSources

		[List("Lookups.SourceCampaigns")]
		public ZGuid SourceCampaignPK
		{
			get { return sourceCampaignPK; }
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(SourceCampaignPKInfo, ref sourceCampaignPK, value);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateSourceCampaignPK();
				}
			}
		}
		ZGuid sourceCampaignPK;

		public ZPropertyInfo SourceCampaignPKInfo
		{
			get { return GetZPropertyInfo(nameof(SourceCampaignPK)); }
		}

		ZGuid[] touchSourceCampaignPKs;
		[List("Lookups.SourceCampaigns")]
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public ZGuid[] TouchSourceCampaignPKs
		{
			get { return touchSourceCampaignPKs ?? new[] { SourceCampaignPK }; }
			set
			{
				touchSourceCampaignPKs = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateTouchSourceCampaignPKsForValidation();
				}
			}
		}

		public ZString TouchSourceCampaignPKsForValidation
		{
			get { return touchSourceCampaignPKsForValidation; }
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(TouchSourceCampaignPKsForValidationInfo, ref touchSourceCampaignPKsForValidation, value);
				}
			}
		}

		public ZPropertyInfo TouchSourceCampaignPKsForValidationInfo
		{
			get { return GetZPropertyInfo(nameof(TouchSourceCampaignPKsForValidation)); }
		}

		[List("Lookups.ContactDataSourceList")]
		[BusinessObjectTestExclude]
		[MaxLength(25)]
		public virtual ZString ContactDataSource
		{
			get { return contactDataSource; }
			set
			{
				using (SuspendSettingHasChanges())
				{
					contactDataSource = IsTouchCampaign && !IsMasterCampaign
						? ContactDataSourceList.Codes.CampaignTracking
						: value;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateContactDataSource();
					Validation.ValidateG0_UseLastEmailSenderAddress();
				}
				if (ContactDataSourceInfo.HasErrors())
				{
					contactDataSource = IsTouchCampaign && !IsMasterCampaign
						? ContactDataSourceList.Codes.CampaignTracking
						: ContactDataSourceList.Codes.ClientIntelligence;
				}
				ContactDataSourceInfo.RefreshBinding();
			}
		}

		ZString contactDataSource;

		public ZPropertyInfo ContactDataSourceInfo
		{
			get { return GetZPropertyInfo(nameof(ContactDataSource)); }
		}

		public virtual string ContactDataSourceAsFilterModuleIDSuffix
		{
			get { return Lookups.ContactDataSourceList.GetDescriptionFromCode(ContactDataSource); }
		}

		public bool IsUsingInquiryDataSource
		{
			get { return ContactDataSource == ContactDataSourceList.Codes.Inquiries; }
		}

		public bool IsUsingClientIntelligenceDataSource
		{
			get { return ContactDataSource == ContactDataSourceList.Codes.ClientIntelligence; }
		}

		public ZBool IsUsingCampaignTrackingDataSource
		{
			get { return ContactDataSource == ContactDataSourceList.Codes.CampaignTracking; }
		}

		public ZBool IsHRCampaign
		{
			get { return !G0_IsSalesAndMarketing; }
		}

		#endregion

		#region ReadOnly's

		public bool G0_GS_NKCampaignManager_ReadOnly
		{
			get { return IsTouchCampaign || (IsInDatabase && !Env.Security.CampaignManagementEditModifyStaffAssignment.IsAllowed); }
		}

		public bool G0_GS_NKCampaignCoordinator_ReadOnly
		{
			get { return IsTouchCampaign || (IsInDatabase && !Env.Security.CampaignManagementEditModifyStaffAssignment.IsAllowed); }
		}

		public bool G0_Stage_ReadOnly
		{
			get { return IsTouchCampaign; }
		}

		public bool G0_EstimatedStartedDate_ReadOnly
		{
			get { return IsTouchCampaign; }
		}

		public bool G0_EstimatedCompletedDate_ReadOnly
		{
			get { return IsTouchCampaign; }
		}

		public bool G0_ActualStartedDate_ReadOnly
		{
			get { return IsTouchCampaign; }
		}

		public bool G0_ActualCompletedDate_ReadOnly
		{
			get { return IsTouchCampaign; }
		}

		public bool G0_IsSalesAndMarketing_ReadOnly => true;

		public bool G0_GroupRatio_ReadOnly
		{
			get { return CurrentGroupColor != 0; }
		}

		#endregion

		public ZBool HasEnded
		{
			get { return G0_ActualCompletedDate <= ZDateTime.Now; }
		}

		protected bool IsVoteSurveyCampaignAndItemsSent
		{
			get { return (IsVoteCampaign || IsSurveyCampaign) && CampaignsItemsSent.Count > 0; }
		}

		protected bool IsEmailSubjectReadOnly
		{
			get { return IsVoteSurveyCampaignAndItemsSent || UseCampaignName; }
		}

		public GlbCompanyCampaignItemSchedule CampaignItemSchedule
		{
			get
			{
				if (campaignItemSchedule == null)
				{
					campaignItemSchedule = new GlbCompanyCampaignItemSchedule(this);
					RegisterEditableChildObject(campaignItemSchedule);
				}
				return campaignItemSchedule;
			}
		}
		GlbCompanyCampaignItemSchedule campaignItemSchedule;

		public CampaignEmailTemplateEditor TemplateEditor
		{
			get
			{
				if (templateEditor == null)
				{
					templateEditor = GetNewCampaignEmailTemplateEditor();
					templateEditor.TemplateBlob = HtmlDocumentBlob;
					RegisterEditableChildObject(templateEditor);
				}
				return templateEditor;
			}
		}
		CampaignEmailTemplateEditor templateEditor;

		protected virtual CampaignEmailTemplateEditor GetNewCampaignEmailTemplateEditor()
		{
			return new CampaignEmailTemplateEditor(this);
		}

		public GlbCompanyCampaignItem SimulationCampaignItem => simulationCampaignItem ?? (simulationCampaignItem = CreateNewSimulationCampaignItem());
		GlbCompanyCampaignItem simulationCampaignItem;

		GlbCompanyCampaignItem CreateNewSimulationCampaignItem()
		{
			var readonlyFactory = Factory.GetCachedReadOnlyFactory();
			var campaignInNewFactory = readonlyFactory.New<GlbCompanyCampaign>();
			campaignInNewFactory.G0_GC = G0_GC;
			return campaignInNewFactory.CampaignsItemsSent.AddNew();
		}

		#endregion

		#region Save

		protected override void RunPreSaveValidationCore()
		{
			using (SuspendValidationOnNonPersistentProperties())
			{
				base.RunPreSaveValidationCore();
			}
		}

		public IDisposable SuspendValidationOnNonPersistentProperties()
		{
			IsValidationOnNonPersistentPropertiesSuspended = true;
			return new DisposableAction(delegate
			{ IsValidationOnNonPersistentPropertiesSuspended = false; });
		}

		protected internal bool IsValidationOnNonPersistentPropertiesSuspended
		{
			private set;
			get;
		}

		public override void OnSaving()
		{
			RelatedChildActivityPivotCollection.RelinkRelatedSuperAndSubActivities();
			RelatedParentActivityPivotCollection.RelinkRelatedSuperAndSubActivities();
			PopulateG0_CampaignIDOnSaving();
			base.OnSaving();
		}

		public event EventHandler Saved;
		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded && !IsInDatabase)
			{
				G0_CampaignID = ZString.Empty;
			}

			if (saveSucceeded && IsLinkTrackCampaign && statModel != null)
			{
				statModel.RefreshLinkTrackedUrl();
			}

			G0_BroadcastVoteSurveyExamInfo.RefreshBinding();

			if (saveSucceeded && Saved != null)
			{
				Saved(this, EventArgs.Empty);
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (ShouldTransfer)
			{
				dripEventTriggered = false;

				if (saveSucceeded)
				{
					TransitionAndSchedule();
				}
			}
		}

		public event EventHandler SaveAssociatedObject;
		protected void OnSaveAssociatedObject()
		{
			if (SaveAssociatedObject != null)
			{
				SaveAssociatedObject(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			IsDeletingRelatedBusinessObjects = true;
			try
			{
				BudgetItems.RemoveAndDeleteAll();
				CampaignsItemsSent.RemoveAndDeleteAll();
				WorkflowItems.RemoveAndDeleteAll();
				Questions.DeleteAll();
				InactiveQuestions.DeleteAll();
				RelatedChildActivityPivotCollection.DeleteAll();
				RelatedParentActivityPivotCollection.DeleteAll();
				TransitionRulesFromThisCampaign.DeleteAll();
				TransitionRulesToThisCampaign.DeleteAll();
				SenderPool.DeleteAll();
				UnsubscribedCollection.RemoveAllFromRelationship();
				SendSettings?.Delete();
				DeleteRelatedUserLayouts();
				DeleteCore();
			}
			finally
			{
				IsDeletingRelatedBusinessObjects = false;
			}

			base.Delete();
		}

		protected virtual void DeleteCore()
		{
		}

		internal bool IsDeletingRelatedBusinessObjects { get; private set; }

		void DeleteRelatedUserLayouts()
		{
			var filters = Factory.Load<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.S9_RelatedEntityID, PK));
			filters.ToList().ForEach(f => f.DeleteWithUserData());
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new GlbCompanyCampaignFetchStrategy(this);
		}

		#endregion

		#region IWorkflowProvider Members

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowType; }
		}

		protected virtual ZString WorkflowType => new CRMCampaignWorkflowDescriptor().Code;

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		ProcessTaskCollection WorkflowItems => GetWorkflowItems;

		protected virtual ProcessTaskCollection GetWorkflowItems
		{
			get
			{
				if (fWorkflowItems == null)
				{
					fWorkflowItems = this.GetOrCreateProcessTaskCollection(() => new CRMCampaignProcessTasksCollection(this));
					RegisterEditableChildObject(fWorkflowItems);
				}
				return fWorkflowItems;
			}
		}

		protected ProcessTaskCollection fWorkflowItems;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			// Must match CRMCampaignWorkflowDescriptor.SubTypeInformation
			// and CampaignFormCustomisationSettingsProvider.GetPropertiesThatAffectWorkflow
			ColumnValueRanker result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, G0_Category, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, G0_Type, ZString.Empty);
			return result;
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		#endregion

		#region Document Field Definitions

		public const string CampaignURLDocFieldName = "CampaignURL";

		public DocumentFieldDefinitionFormBizo DocumentFieldDefinitionBizO
		{
			get
			{
				if (fDocumentFieldDefinitionBizO == null)
				{
					fDocumentFieldDefinitionBizO = new DocumentFieldDefinitionFormBizo(DocumentFieldDefinitions);
				}
				return fDocumentFieldDefinitionBizO;
			}
		}

		DocumentFieldDefinitionFormBizo fDocumentFieldDefinitionBizO;

		internal DocumentFieldDefinitionCollection DocumentFieldDefinitions
		{
			get
			{
				if (fDocumentFieldDefinitions == null)
				{
					fDocumentFieldDefinitions = new DocumentFieldAttributeFinder().FindProperties(ObjectFactory.GetType<DocumentWrappers.IDocCompanyCampaignItem>());
				}
				return fDocumentFieldDefinitions;
			}
		}

		DocumentFieldDefinitionCollection fDocumentFieldDefinitions;

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return new GlbCompanyCampaignDocumentSupporter(this); }
		}

		public class GlbCompanyCampaignDocumentSupporter : DocumentSupporter
		{
			public GlbCompanyCampaignDocumentSupporter(GlbCompanyCampaign campaign)
				: base(campaign)
			{
			}

			protected override void InitialiseCore(IDocumentEvents documentEventSource)
			{
				base.InitialiseCore(documentEventSource);
				documentEventSource.DocumentPrintRequested += new DocumentCancelEventHandler(DocumentEventSource_DocumentPrintRequested);
			}

			internal void DocumentEventSource_DocumentPrintRequested(object sender, DocumentCancelEventArgs e)
			{
				e.Cancel = true;
				GlbCompanyCampaignSender campaignSender = new GlbCompanyCampaignSender(CompanyCampaign, CompanyCampaign.FilteredContacts.Cast<CampaignContact>().ToList());
				campaignSender.ShouldContinueWithSending += new GlbCompanyCampaignSender.CheckContinueWithSendingHandler(delegate
				{ return true; });
				campaignSender.CheckAndSendCampaigns();
				campaignSender.ShouldContinueWithSending -= new GlbCompanyCampaignSender.CheckContinueWithSendingHandler(delegate
				{ return true; });
			}

			protected GlbCompanyCampaign CompanyCampaign
			{
				get { return (GlbCompanyCampaign)BusinessObject; }
			}

			protected override Core.Constants.DataContext[] GetSupportedDataContexts()
			{
				return new Core.Constants.DataContext[] { Core.Constants.DataContext.CompanyCampaign };
			}

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get { return Env.Security.CampaignManagementCustomiseDocuments; }
			}

			public override BusinessContext BusinessContext
			{
				get { return BusinessContext.CompanyCampaign; }
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				return null;
			}
		}

		#endregion

		#region Notification Constants

		public static class NotificationConstants
		{
			public static string NoMatchingRecordsMessage
			{
				get { return Res.GetString("141936dc-55ab-44f4-92d5-9f34305db86b", "There are no records that match your search."); }
			}

			public static string TooManyRecordsMessage
			{
				get { return Res.GetString("f144b47f-c732-4062-9e65-02adf689996f", "There are too many records that match your search. Please refine your filter."); }
			}
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo => DocManagerInfo;

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (fDocManagerInfo == null)
				{
					fDocManagerInfo = new GlbCompanyCampaignDocManagerInfo(this);
					fDocManagerInfo.UseBusinessEntityFactoryAsInternal = true;
				}
				return fDocManagerInfo;
			}
		}

		GlbCompanyCampaignDocManagerInfo fDocManagerInfo;

		#endregion

		#region IRelatableActivity Members

		ZString IRelatableActivity.ActivityType
		{
			get { return RelatableActivityTypeList.Codes.CampaignManagement; }
		}

		IOrgHeader IRelatableActivity.Client
		{
			get { return null; }
		}

		ZBool IRelatableActivity.ClientHasChanges
		{
			get { return false; }
		}

		IOrgContact IRelatableActivity.Contact
		{
			get { return null; }
		}

		ZBool IRelatableActivity.ContactHasChanges
		{
			get { return false; }
		}

		ZString IRelatableActivity.Summary
		{
			get { return string.Join("; ", new[] { G0_CampaignNameMultilingual, G0_Category, G0_Type, G0_Stage }.Where(x => !x.IsEmpty)); }
		}

		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
			dripEventTriggered =
				!(relatedActivity is GlbCompanyCampaign)
				&& !(relatedActivity is GlbCompanyCampaignItem)
				&& (IsMasterCampaign || IsTouchCampaign);
		}

		ZBool IRelatableActivity.SupportViewRelatedCommunications => ZBool.True;

		public IRelatedChildActivityPivotCollection RelatedChildActivityPivotCollection
		{
			get
			{
				if (relatedChildActivityPivotCollection == null)
				{
					relatedChildActivityPivotCollection = new SuperActivityRelatedChildActivityPivotCollection(this);
				}
				return relatedChildActivityPivotCollection;
			}
		}
		RelatedChildActivityPivotCollection relatedChildActivityPivotCollection;

		public IRelatedParentActivityPivotCollection RelatedParentActivityPivotCollection
		{
			get
			{
				if (relatedParentActivityPivotCollection == null)
				{
					relatedParentActivityPivotCollection = new SuperActivityRelatedParentActivityPivotCollection(this);
				}
				return relatedParentActivityPivotCollection;
			}
		}
		RelatedParentActivityPivotCollection relatedParentActivityPivotCollection;

		public SentCampaignItemsRelatedChildActivityPivotCollection PostCampaignItemPivotCollection
		{
			get
			{
				if (postCampaignItemPivotCollection == null)
				{
					postCampaignItemPivotCollection = new SentCampaignItemsRelatedChildActivityPivotCollection(this);
				}
				return postCampaignItemPivotCollection;
			}
		}
		SentCampaignItemsRelatedChildActivityPivotCollection postCampaignItemPivotCollection;

		#endregion

		#region ISalesRelationActivity Members

		public SalesRelationModel SalesRelationModel
		{
			get
			{
				if (salesRelationModel == null)
				{
					salesRelationModel = new GlbCompanyCampaignSalesRelationModel(this);
					RegisterEditableChildObject(salesRelationModel);
				}

				return salesRelationModel;
			}
			set
			{
				salesRelationModel = value;
				RegisterEditableChildObject(salesRelationModel);
			}
		}
		SalesRelationModel salesRelationModel;

		ISalesRelationModel ISalesRelationActivity.SalesRelationModel
		{
			get { return SalesRelationModel; }
		}

		ZString ISalesRelationActivity.ActivityNotePropertyName
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ISuperRelatableActivity Members

		ISubRelatableActivity ISuperRelatableActivity.GetMatchingSubActivity(IRelatableActivity activity)
		{
			return GetMatchingCampaignItem(activity);
		}

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return false; }
		}

		#endregion

		#region Matching Item

		public GlbCompanyCampaignItem GetMatchingCampaignItem(IRelatableActivity activity)
		{
			var campaignItemParentActivity = activity.RelatedParentActivityPivotCollection.Activities.OfType<GlbCompanyCampaignItem>().FirstOrDefault();
			if (campaignItemParentActivity != null && activity.Contact != null && activity.Contact.PK == campaignItemParentActivity.Recipient.PK)
			{
				return campaignItemParentActivity;
			}
			else
			{
				var campaignItemsSentNotSaved = CampaignItemsSentNotSaved != null ? CampaignItemsSentNotSaved.Cast<GlbCompanyCampaignItem>() : Enumerable.Empty<GlbCompanyCampaignItem>();
				var campaignsItemsSentCombined = CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().Union(campaignItemsSentNotSaved);

				if (activity is SalesEnquiry)
				{
					var matchingItemRecipient = campaignsItemsSentCombined.FirstOrDefault(x => x.G8_RecipientTableCode == OrgColdCallRegisterSchema.Constants.Prefix && x.G8_RecipientID == activity.PK);
					if (matchingItemRecipient != null)
					{
						return matchingItemRecipient;
					}
				}

				GlbCompanyCampaignItem highestMatchingItem = null;
				int highestMatchingItemScore = 0;

				foreach (GlbCompanyCampaignItem item in campaignsItemsSentCombined)
				{
					var matchScore = GetMatchScore(item, activity);
					if (matchScore > highestMatchingItemScore)
					{
						highestMatchingItem = item;
						highestMatchingItemScore = matchScore;
					}
					if (highestMatchingItemScore == 2)
					{
						break;
					}
				}

				return highestMatchingItem;
			}
		}

		internal static int GetMatchScore(GlbCompanyCampaignItem item, IRelatableActivity activity)
		{
			var activityAsSalesEnquiry = activity as SalesEnquiry;
			if (activityAsSalesEnquiry != null
				&& item.G8_RecipientTableCode == OrgColdCallRegisterSchema.Constants.Prefix
				&& item.G8_RecipientID == activity.PK)
			{
				return 2;
			}

			var itemAsRelatableActivity = (IRelatableActivity)item;
			if (itemAsRelatableActivity.Client == null || activity.Client == null || itemAsRelatableActivity.Client.PK != activity.Client.PK)
			{
				return 0;
			}

			if ((itemAsRelatableActivity.Contact == null && activityAsSalesEnquiry == null) || activity.Contact == null)
			{
				return 0;
			}

			if (itemAsRelatableActivity.Contact != null && itemAsRelatableActivity.Contact.PK == activity.Contact.PK)
			{
				return 2;
			}
			else if (itemAsRelatableActivity.Contact != null && !itemAsRelatableActivity.Contact.OC_Email.IsEmpty && itemAsRelatableActivity.Contact.OC_Email.EqualsIgnoringCase(activity.Contact.OC_Email))
			{
				return 1;
			}

			return 0;
		}

		#endregion

		#region IImportParentRelatedActivityInfoOnNew

		public ZString ImportOrgCode { get; private set; }
		public ZString ImportContactName { get; private set; }
		public ZGuid ImportInquiryPK { get; private set; }

		bool IImportParentRelatedActivityInfoOnNew.ImportParentInfo(IRelatableActivity parentActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			if (parentActivity.Client != null)
			{
				ImportOrgCode = parentActivity.Client.OH_Code;
			}

			if (parentActivity.Contact != null)
			{
				if (!parentActivity.Contact.OC_ContactName.IsEmpty)
				{
					ImportContactName = parentActivity.Contact.OC_ContactName;
				}
			}

			var parentActivityAsSalesEnquiry = parentActivity as SalesEnquiry;
			if (parentActivityAsSalesEnquiry != null && parentActivity.Contact == null)
			{
				ImportInquiryPK = parentActivityAsSalesEnquiry.PK;

				if (!parentActivityAsSalesEnquiry.O1_ContactName.IsEmpty)
				{
					ImportContactName = parentActivityAsSalesEnquiry.O1_ContactName;
				}

				ContactDataSource = ContactDataSourceList.Codes.Inquiries;
			}

			if (parentActivity.ActivityType == RelatableActivityTypeList.Codes.CampaignManagement)
			{
				if (parentActivity is GlbCompanyCampaignItem campaignItem)
				{
					SourceCampaignPK = campaignItem.CompanyCampaign.PK;
					if (campaignItem.RecipientAsSalesEnquiry != null)
					{
						ImportContactName = campaignItem.Recipient.Name;
					}
				}
				else if (parentActivity is GlbCompanyCampaign campaign)
				{
					SourceCampaignPK = campaign.PK;
				}

				ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
				SourceCampaignPKInfo.RefreshBinding();
			}

			return true;
		}

		#endregion

		#region ITemplateCopyable Members

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			GlbCompanyCampaign newCampaign = (GlbCompanyCampaign)Clone();

			foreach (GlbCompanyCampaignBudgetItem budgetItem in BudgetItems)
			{
				newCampaign.BudgetItems.Add(budgetItem.Clone());
			}

			newCampaign.G0_ActualStartedDate = ZDateTime.Empty;
			newCampaign.G0_ActualCompletedDate = ZDateTime.Empty;
			newCampaign.G0_EstimatedStartedDate = ZDateTime.Empty;
			newCampaign.G0_EstimatedCompletedDate = ZDateTime.Empty;
			newCampaign.G0_CampaignID = ZString.Empty;

			newCampaign.useCampaignName = newCampaign.G0_EmailSubject == (ZString)newCampaign.G0_CampaignNameMultilingual;

			foreach (var userData in LoadUserDataLayouts())
			{
				var newFilter = Factory.New<StmModuleFilter>();
				newFilter.CopyPersistentValuesFrom(userData.Layout);
				newFilter.S9_RelatedEntityID = newCampaign.PK;

				var newUserData = (StmModuleFilterUserData)userData.Clone();
				newUserData.S0_S9 = newFilter.PK;
				newUserData.S0_RelatedEntityID = newCampaign.PK;
			}

			CloneCampaignLastUsedLayout(newCampaign);

			newCampaign.IsUsingCloneFactory = true;

			return newCampaign;
		}

		public ZBool IsUsingCloneFactory { get; private set; }

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		StmModuleFilterUserData[] LoadUserDataLayouts()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(StmModuleFilterUserData));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(StmModuleFilter), StmModuleFilterSchema.PK);
			subQuery.AddToFilter(StmModuleFilterSchema.S9_FilterName, ZString.Empty);
			subQuery.AddToFilter(StmModuleFilterSchema.S9_GC, null);
			subQuery.AddToFilter(StmModuleFilterSchema.S9_RelatedEntityID, PK);
			subQuery.AddToFilter(StmModuleFilterSchema.S9_FilterType, SQLComparisonOperator.NotEqual, StmModuleFilterTypes.Codes.FilterRule);
			query.AddSubQuery(StmModuleFilterUserDataSchema.S0_S9, subQuery, JoinCondition.And);
			query.AddToFilter(StmModuleFilterUserDataSchema.S0_RelatedEntityID, PK);
			return Factory.Load<StmModuleFilterUserData>(query);
		}

		void CloneCampaignLastUsedLayout(GlbCompanyCampaign cloneCampaign)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(StmDataSchema.SD_Owner, PK);
			var stmData = Factory.LoadTop1<StmData>(query);
			if (stmData != null)
			{
				StmData dataClone = (StmData)stmData.Clone();
				dataClone.SD_Owner = cloneCampaign.PK;
			}
		}

		#endregion

		#region ICustomFieldProvider

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
			return new CustomBusinessObject(Factory, this, properties);
		}

		#endregion

		#region Vote / Exam / Survey

		#region Type

		public override ZString G0_BroadcastVoteSurveyExam
		{
			get { return base.G0_BroadcastVoteSurveyExam; }
			set
			{
				if (base.G0_BroadcastVoteSurveyExam != value)
				{
					if (CampaignsItemsSent.Count > 0)
					{
						ErrorReporter.ReportOnce("GlbCompanyCampaign_ShouldNotChangeTypeOnceSent", "Campaign type should not be changed once the campaign emails are sent");
					}

					CancelEventArgs args = new CancelEventArgs();
					OnCampaignTypeChanging(args);

					if (!args.Cancel)
					{
						base.G0_BroadcastVoteSurveyExam = value;
						Questions.DeleteAll();
						if (IsVoteCampaign)
						{
							Questions.AddNewVoteQuestionHeader();
						}

						G0_RandomizeWithinHeader = IsVoteCampaign || IsExamCampaign;

						if (IsBroadcastCampaign)
						{
							G0_StoreEmailInEDocs = OrganisationsDataRegistry.Instance.PermanentlySaveAgainstRecipientDefault.Value;
						}
						else
						{
							G0_StoreEmailInEDocs = false;
						}
					}

					Validation.ValidateG0_DocumentBlob();

					SetDefaultAnswerType();
				}
			}
		}

		protected virtual void SetDefaultAnswerType()
		{
			G0_DefaultAnswerType = (IsSurveyCampaign) ? VoteExamSurveyAnswerTypeList.Codes.YesNo : "";
		}

		public bool G0_BroadcastVoteSurveyExam_ReadOnly
		{
			get { return ((IsLinkTrackCampaign || IsTargetList) && IsInDatabase) || AreCampaignItemsSent; }
		}

		public bool IsLinkTrackCampaign
		{
			get { return G0_BroadcastVoteSurveyExam == CampaignTypeList.Codes.LinkTracking; }
		}

		public bool IsBroadcastCampaign
		{
			get => G0_BroadcastVoteSurveyExam == CampaignTypeList.Codes.Broadcast;
		}

		public bool IsTargetList
		{
			get { return G0_BroadcastVoteSurveyExam == CampaignTypeList.Codes.TargetList; }
		}

		public bool NeedsTrackedLinks
		{
			get { return !IsTargetList && !IsMasterCampaign && !TrackedLinks.Any() && !CampaignsItemsSent.Any() && TemplateEditor.TrackedLinks.Any(); }
		}

		protected bool AreCampaignItemsSent
		{
			get
			{
				if (campaignsItemsSent != null)
				{
					return campaignsItemsSent.Count > 0;
				}
				else
				{
					return Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(GlbCompanyCampaignItem)), new ZQuery(GlbCompanyCampaignItemSchema.G8_G0, PK));
				}
			}
		}

		void OnCampaignTypeChanging(CancelEventArgs args)
		{
			if (CampaignTypeChanging != null)
			{
				CampaignTypeChanging(this, args);
			}
		}

		public event CancelEventHandler CampaignTypeChanging;

		#endregion

		#region Flags

		public ZBool IsVoteCampaign
		{
			get { return IsVoteCampaignType(G0_BroadcastVoteSurveyExam); }
		}

		public ZPropertyInfo IsVoteCampaignInfo
		{
			get { return GetZPropertyInfo(nameof(IsVoteCampaign)); }
		}

		public ZBool IsSurveyCampaign
		{
			get { return IsSurveyCampaignType(G0_BroadcastVoteSurveyExam); }
		}

		public ZPropertyInfo IsSurveyCampaignInfo
		{
			get { return GetZPropertyInfo(nameof(IsSurveyCampaign)); }
		}

		public bool RequiresCampaignURL
		{
			get { return IsVoteCampaign || IsSurveyCampaign; }
		}

		public bool IsCampaignURLSettingsValid
		{
			get { return !((ZString)WebDataRegistry.Instance.WebCampaignUrl.GetValueWithoutFallback(G0_GC.ToGuid(), Guid.Empty, Guid.Empty)).IsEmpty; }
		}

		protected virtual bool IsVoteCampaignType(string campaignType)
		{
			return campaignType == CampaignTypeList.Codes.Voting;
		}

		protected virtual bool IsSurveyCampaignType(string campaignType)
		{
			return campaignType == CampaignTypeList.Codes.Survey;
		}

		public virtual bool RandomiseQuestionAndMultipleChoiceOrder => IsVoteCampaign && G0_RandomizeWithinHeader;

		public bool IsExamCampaign => G0_BroadcastVoteSurveyExam == Core.Constants.Recruiter.LearningCentreCampaignType;

		public List<string> EmailCampaigns => emailCampaigns ?? (emailCampaigns = GetEmailCampaignsList());
		public List<string> emailCampaigns;

		protected virtual List<string> GetEmailCampaignsList()
		{
			return new List<string>() { CampaignTypeList.Codes.Broadcast, CampaignTypeList.Codes.Survey, CampaignTypeList.Codes.Voting, InsideSalesTouchTypeList.Codes.PreApproachEmail };
		}

		public bool IsEmailCampaign => EmailCampaigns.Contains(G0_BroadcastVoteSurveyExam);

		#endregion

		#region Questions Collection

		[ChildEditable(true)]
		public VoteExamSurveyQuestionCollection Questions
		{
			get
			{
				if (fQuestions == null)
				{
					fQuestions = GetNewQuestionCollection(this, true);
					RegisterEditableChildObject(fQuestions);
				}
				return fQuestions;
			}
		}

		[ChildEditable(true)]
		public VoteExamSurveyQuestionCollection InactiveQuestions
		{
			get
			{
				if (inactiveQuestions == null)
				{
					inactiveQuestions = GetNewQuestionCollection(this, false);
					RegisterEditableChildObject(inactiveQuestions);
				}
				return inactiveQuestions;
			}
		}

		protected virtual VoteExamSurveyQuestionCollection GetNewQuestionCollection(GlbCompanyCampaign master, bool isActive)
		{
			return new VoteExamSurveyQuestionCollection(this, isActive);
		}

		public VoteExamSurveyQuestion VoteHeader
		{
			get { return (IsVoteCampaign && Questions.Count > 0) ? Questions[0] : null; }
		}

		public VoteExamSurveyQuestion[] GetActualQuestions()
		{
			return GetActualQuestions(new ZQuery());
		}

		public VoteExamSurveyQuestion[] GetActualQuestions(ZQuery filter)
		{
			return ActualQuestionsForBinding.Find(filter).ToArray();
		}

		public VoteExamSurveyQuestion[] GetActualQuestions(string countryCode)
		{
			ZQuery countrySpecificFilter = new ZQuery();
			if (!string.IsNullOrEmpty(countryCode))
			{
				countrySpecificFilter.AddToFilter(VoteExamSurveyQuestionSchema.HY_RN_NKCountryCode, "");
				countrySpecificFilter.AddToFilter(JoinCondition.Or, VoteExamSurveyQuestionSchema.HY_RN_NKCountryCode, countryCode);
			}
			return GetActualQuestions(countrySpecificFilter);
		}

		[ChildEditable(true)]
		public VoteExamSurveyQuestionSet ActualQuestionsForBinding
		{
			get { return (IsVoteCampaign && VoteHeader != null) ? VoteHeader.SubQuestions : Questions; }
		}

		public VoteExamSurveySummaryCollection VoteExamSurveySummaries
		{
			get
			{
				if (surveySummaries == null)
				{
					surveySummaries = new VoteExamSurveySummaryCollection(this.ActualQuestionsForBinding);
					surveySummaries.Load();
				}
				return surveySummaries;
			}
		}

		VoteExamSurveyQuestionCollection fQuestions;
		VoteExamSurveyQuestionCollection inactiveQuestions;
		VoteExamSurveySummaryCollection surveySummaries;

		#endregion

		#region VoteExamSurveyDetails

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public virtual IEnumerable<KeyValuePair<string, string>> GetVoteExamSurveyDetails(string countryCode)
		{
			yield return new KeyValuePair<string, string>(VoteExamSurveyNameCaption, VoteExamSurveyName);
			yield return new KeyValuePair<string, string>(Res.GetString("4addaff8-33b7-4974-8dc8-e5b4862cf376", "Description"), VoteExamSurveyDescription);
			yield return new KeyValuePair<string, string>(QuestionsCountCaption, GetQuestionsCountAsString(countryCode));
		}

		protected virtual ZString VoteExamSurveyNameCaption
		{
			get
			{
				string result = "";

				if (IsVoteCampaign)
				{
					result = Res.GetString("d97ef1aa-e97d-40c6-abf4-33f9ed362dba", "Vote Name");
				}
				else if (IsSurveyCampaign)
				{
					result = Res.GetString("e6068778-54fa-473c-b2d6-a9cb78544770", "Survey Name");
				}

				return result;
			}
		}

		protected virtual internal ZString VoteExamSurveyName
		{
			get { return G0_CampaignNameLocalized; }
		}

		protected virtual internal ZString VoteExamSurveyDescription
		{
			get { return G0_CampaignCommentLocalized; }
		}

		protected ZString QuestionsCountCaption
		{
			get
			{
				return (IsVoteCampaign)
					? Res.GetString("881b25c5-580c-43df-b78d-88c6018eac20", "No. of Votes Required")
					: Res.GetString("1b737004-7509-4447-b21e-22c04ea954d1", "No. of Questions");
			}
		}

		protected virtual ZString GetQuestionsCountAsString(string countryCode)
		{
			string result = "";

			if (IsVoteCampaign && VoteHeader != null)
			{
				int minNominations = VoteHeader.HY_Min;
				int maxNominations = VoteHeader.HY_Max;
				result = (minNominations != maxNominations)
					? Res.GetString("f7967962-22a5-44e0-8e52-aa40c95c6ee5", "{0} (up to {1})", minNominations, maxNominations)
					: minNominations.ToString();
			}
			else
			{
				int allQuestionsCount = GetActualQuestions(countryCode).Where(q => q.HY_AnswerType != VoteExamSurveyAnswerTypeList.Codes.Header).Count();
				result = allQuestionsCount.ToString();
			}

			return result;
		}

		#endregion

		[List("Lookups.DefaultAnswerTypes")]
		public override ZString G0_DefaultAnswerType
		{
			get { return base.G0_DefaultAnswerType; }
			set { base.G0_DefaultAnswerType = value; }
		}

		#endregion

		#region IGlbCompanyCampaign Members

		ZGuid IGlbCompanyCampaign.PK
		{
			get { return PK; }
		}

		ISalesRelationModel IGlbCompanyCampaign.SalesRelationModel
		{
			get { return SalesRelationModel; }
		}

		public ZString CurrentExamVersion => CurrentSettings?.CurrentExamVersion ?? ZString.Empty;
		public CampaignSettings CurrentSettings { get; private set; }

		public void SetCurrentSettings(CampaignSettings settings)
		{
			CurrentSettings = settings;
			OnSettingsChanged();
		}

		protected virtual void OnSettingsChanged()
		{
		}

		public virtual ZString DefaultExamVersion
		{
			get
			{
				return ZString.Empty;
			}
		}

		public class CampaignSettings
		{
			public CampaignSettings(string examVersion)
			{
				CurrentExamVersion = examVersion;
			}

			public string CurrentExamVersion { get; }
		}

		public ZShort MaxQuestionsPerExam
		{
			get { return ZShort.Zero; }
		}

		#endregion

		#region Drip Marketing

		#region Properties

		public override ZGuid G0_G0_Master
		{
			get { return base.G0_G0_Master; }
			set
			{
				if (base.G0_G0_Master != value)
				{
					base.G0_G0_Master = value;

					if (SendSettings != null)
					{
						var otherTouch = GetOtherTouchesInSameHorizontal().FirstOrDefault();
						if (otherTouch != null && otherTouch.SendSettings != null)
						{
							SendSettings.CopyHorizontalSettings(otherTouch.SendSettings);
						}
					}
				}
			}
		}

		GlbCompanyCampaignSendSettings sendSettings;
		public GlbCompanyCampaignSendSettings SendSettings
		{
			get
			{
				if (!IsTouchCampaign)
				{
					return null;
				}

				if (sendSettings == null)
				{
					if (G0_GCG_Group.IsEmpty)
					{
						var query = new ZQuery(GlbCompanyCampaignSendSettingsSchema.GSC_G0_Campaign, PK);
						sendSettings = Factory.LoadTop1<GlbCompanyCampaignSendSettings>(query);

						if (sendSettings == null)
						{
							sendSettings = GetNewSettings();
							sendSettings.GSC_G0_Campaign = PK;
						}
					}
					else
					{
						var query = new ZQuery(GlbCompanyCampaignSendSettingsSchema.GSC_GCG_Group, G0_GCG_Group);
						sendSettings = Factory.LoadTop1<GlbCompanyCampaignSendSettings>(query);

						if (sendSettings == null)
						{
							sendSettings = Factory.New<GlbCompanyCampaignSendSettings>();
							sendSettings.GSC_GCG_Group = G0_GCG_Group;
						}
					}

					RegisterEditableChildObject(sendSettings);
				}

				return sendSettings;
			}
		}

		public void RecalculateItemsStandAloneCampaign()
		{
			var queuedItems = GetQueuedItems().ToList();
			if (queuedItems.Any())
			{
				var campaignSender = new GlbCompanyCampaignSender(this);
				campaignSender.SetCampaignItemsSender(queuedItems);
			}
		}

		IEnumerable<GlbCompanyCampaignItem> GetQueuedItems()
		{
			return CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().Where(item => item.G8_TrackingStatus == TrackingStatusCodes.Codes.QUE && !item.G8_ScheduleTimeUtc.IsEmpty);
		}

		public bool HasQueuedItems => GetQueuedItems().Any();

		protected virtual GlbCompanyCampaignSendSettings GetNewSettings()
		{
			return Factory.New<GlbCompanyCampaignSendSettings>();
		}

		public bool IsMasterCampaign => IsDripMarketingCampaign || IsInsideSalesCampaign;

		public bool IsDripMarketingCampaign => G0_BroadcastVoteSurveyExam == CampaignTypeList.Codes.DripMarketing;

		public bool IsInsideSalesCampaign => G0_BroadcastVoteSurveyExam == CampaignTypeList.Codes.InsideSales;

		public bool IsOpportunityCreationCampaign => G0_BroadcastVoteSurveyExam == InsideSalesTouchTypeList.Codes.OpportunityCreation;

		public bool IsPreApproachEmailCampaign => G0_BroadcastVoteSurveyExam == InsideSalesTouchTypeList.Codes.PreApproachEmail;

		public bool IsTouchCampaign
		{
			get { return !G0_G0_Master.IsEmpty; }
		}

		GlbCompanyCampaignCollectionStrongyTyped allTouches;
		public GlbCompanyCampaignCollectionStrongyTyped AllTouches
		{
			get
			{
				if (!IsMasterCampaign)
				{
					allTouches = new GlbCompanyCampaignCollectionStrongyTyped(Factory, ZQuery.NoResultQuery);
				}

				if (allTouches == null)
				{
					allTouches = new GlbCompanyCampaignCollectionStrongyTyped(this, new ZQuery());
					RegisterEditableChildObject(allTouches);
					allTouches.CollectionCountChange += AllTouchesOnCollectionCountChange;
				}

				return allTouches;
			}
		}

		void AllTouchesOnCollectionCountChange(object sender, CollectionCountChangedEventArgs args)
		{
			if (args.ItemAdded)
			{
				var added = args.BizObject as GlbCompanyCampaign;

				if (added != null)
				{
					added.G0_G0_Master = PK;
					added.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;

					if (added.TransitionRulesToThisCampaign.Count == 0)
					{
						added.TransitionRulesToThisCampaign.AddNew();
					}
				}
			}

			horizontals = null;
		}

		GlbCompanyCampaignCollection nextTouches;
		public GlbCompanyCampaignCollection NextTouches
		{
			get
			{
				if (nextTouches == null)
				{
					var query = new ZDBOnlyQuery(typeof(GlbCompanyCampaign));
					query.AddToFilter(GlbCompanyCampaignSchema.G0_G0_Master, IsMasterCampaign ? PK : G0_G0_Master);

					var innerquery = new ZDBOnlyQuery(typeof(GlbCompanyCampaign));

					var subquery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignDripMarketing), GlbCompanyCampaignDripMarketingSchema.GCD_G0_NextTouch);
					subquery.AddToFilter(GlbCompanyCampaignDripMarketingSchema.GCD_G0_ParentTouch, PK);

					var subsubQuery = new ZQuery(GlbCompanyCampaignDripMarketingSchema.GCD_ParentHorizontalId, G0_HorizontalId);
					subsubQuery.AddToFilter(GlbCompanyCampaignDripMarketingSchema.GCD_G0_ParentTouch, DBNull.Value);

					subquery.AddToFilter(subsubQuery, JoinCondition.Or);

					var subqueryGroup = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignDripMarketing), GlbCompanyCampaignDripMarketingSchema.GCD_GCG_Group);
					subqueryGroup.AddToFilter(GlbCompanyCampaignDripMarketingSchema.GCD_G0_ParentTouch, PK);
					subqueryGroup.AddToFilter(subsubQuery, JoinCondition.Or);

					innerquery.AddSubQuery(subquery, JoinCondition.And);
					innerquery.AddSubQuery(GlbCompanyCampaignSchema.G0_GCG_Group, subqueryGroup, JoinCondition.Or);

					query.AddToFilter(innerquery);

					var localNextTouches = new GlbCompanyCampaignCollection(Factory, query);
					localNextTouches.Load();
					nextTouches = localNextTouches;
				}

				return nextTouches;
			}
		}

		public IEnumerable<GlbCompanyCampaign> GetOtherTouchesInSameHorizontal()
		{
			if (!IsTouchCampaign)
			{
				return Enumerable.Empty<GlbCompanyCampaign>();
			}

			var otherTouchesInSameHorizontalQuery = new ZQuery(GlbCompanyCampaignSchema.G0_HorizontalId, G0_HorizontalId);
			otherTouchesInSameHorizontalQuery.AddToFilter(GlbCompanyCampaignSchema.G0_G0_Master, G0_G0_Master);
			otherTouchesInSameHorizontalQuery.AddToFilter(GlbCompanyCampaignSchema.PK, SQLComparisonOperator.NotEqual, PK);

			return Factory.Load<GlbCompanyCampaign>(otherTouchesInSameHorizontalQuery);
		}

		public bool HasPreviousTouches
		{
			get
			{
				if (!IsTouchCampaign)
				{
					return false;
				}

				var previousTouchesQuery = new ZQuery(GlbCompanyCampaignSchema.G0_G0_Master, G0_G0_Master);
				previousTouchesQuery.AddToFilter(GlbCompanyCampaignSchema.G0_HorizontalId, SQLComparisonOperator.LessThan, G0_HorizontalId);
				previousTouchesQuery.AddToFilter(GlbCompanyCampaignSchema.PK, SQLComparisonOperator.NotEqual, PK);

				return Factory.Exists(typeof(GlbCompanyCampaign), previousTouchesQuery);
			}
		}

		GlbCompanyCampaign masterCampaign;
		public GlbCompanyCampaign MasterCampaign
		{
			get { return masterCampaign ?? (masterCampaign = Factory.Load<GlbCompanyCampaign>(G0_G0_Master)); }
		}

		public ZString TouchId
		{
			get { return G0_HorizontalId > 0 ? G0_HorizontalId + G0_VerticalId : string.Empty; }
		}

		public ZPropertyInfo TouchIdInfo
		{
			get { return GetZPropertyInfo(nameof(TouchId)); }
		}

		public ZString TouchFullName
		{
			get { return Res.GetString("2cea06c0-5a27-456a-ae01-be47d8e45aef", "Touch [{0}] - {1}", TouchId, G0_CampaignNameMultilingual); }
		}

		public ZPropertyInfo TouchFullNameInfo
		{
			get { return GetZPropertyInfo(nameof(TouchFullName)); }
		}

		List<CampaignHorizontal> horizontals;

		public IEnumerable<CampaignHorizontal> Horizontals
		{
			get
			{
				if (horizontals == null)
				{
					horizontals = new List<CampaignHorizontal>();

					foreach (var horizontalGroup in AllTouches.GroupBy(t => t.G0_HorizontalId))
					{
						var hor = new CampaignHorizontal(horizontalGroup.Key);
						horizontals.Add(hor);
						foreach (var touch in horizontalGroup.OrderBy(t => t.G0_VerticalId))
						{
							hor.AddCampaign(touch);
						}
					}
				}

				return horizontals.OrderBy(h => h.Id);
			}
		}

		public override ZByte G0_HorizontalId
		{
			get { return base.G0_HorizontalId; }
			set
			{
				if (G0_HorizontalId != value)
				{
					nextTouches = null; // invalidate collection

					if (TransitionRulesFromThisCampaign != null)
					{
						foreach (var rule in TransitionRulesFromThisCampaign.Where(r => r.GCD_G0_ParentTouch == PK))
						{
							rule.GCD_ParentHorizontalId = value;
						}
					}

					base.G0_HorizontalId = value;

					if (SendSettings != null)
					{
						var otherTouch = GetOtherTouchesInSameHorizontal().FirstOrDefault();
						if (otherTouch != null && otherTouch.SendSettings != null)
						{
							SendSettings.CopyHorizontalSettings(otherTouch.SendSettings);
						}
					}
				}
			}
		}

		public GlbCompanyCampaignGroup CurrentGroup
		{
			get { return Factory.Load<GlbCompanyCampaignGroup>(G0_GCG_Group); }
		}

		ZInt currentGroupColor;
		public ZInt CurrentGroupColor
		{
			get { return currentGroupColor; }
			set
			{
				SetNonPersistentPropertyValue(CurrentGroupColorInfo, ref currentGroupColor, value);
				OnPropertyChanged(nameof(G0_GroupRatio_ReadOnly));
				OnPropertyChanged(nameof(CurrentGroupColor));

				if (currentGroupColor != 0 && G0_GroupRatio == 0)
				{
					G0_GroupRatio = 1;
				}
			}
		}

		public ZPropertyInfo CurrentGroupColorInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentGroupColor)); }
		}

		public override ZByte G0_GroupRatio
		{
			get { return base.G0_GroupRatio; }
			set
			{
				base.G0_GroupRatio = value;
				OnPropertyChanged(nameof(G0_GroupRatio));
			}
		}

		#endregion

		#region Transition rules

		GlbCompanyCampaignDripMarketingCollection transitionRulesFromThisCampaign;

		public GlbCompanyCampaignDripMarketingCollection TransitionRulesFromThisCampaign
		{
			get
			{
				if (!IsTouchCampaign && !IsMasterCampaign)
				{
					transitionRulesFromThisCampaign = new EmptyDripMarketingCollection(Factory);
				}
				else if (transitionRulesFromThisCampaign == null || transitionRulesFromThisCampaign is EmptyDripMarketingCollection)
				{
					transitionRulesFromThisCampaign = new TransistionRulesFromTouchCollection(this);
					RegisterEditableChildObject(transitionRulesFromThisCampaign);
				}

				return transitionRulesFromThisCampaign;
			}
		}

		GlbCompanyCampaignDripMarketingCollection transitionRulesToThisCampaign;

		public GlbCompanyCampaignDripMarketingCollection TransitionRulesToThisCampaign
		{
			get
			{
				if (!IsTouchCampaign && !IsMasterCampaign)
				{
					transitionRulesToThisCampaign = new EmptyDripMarketingCollection(Factory);
				}
				else if (transitionRulesToThisCampaign == null || transitionRulesToThisCampaign is EmptyDripMarketingCollection)
				{
					transitionRulesToThisCampaign = new TransistionRulesToTouchCollection(this);
					RegisterEditableChildObject(transitionRulesToThisCampaign);
				}

				return transitionRulesToThisCampaign;
			}
		}

		public CampaignHorizontal AddHorizontal()
		{
			var maxId = Horizontals.Any() ? Horizontals.Max(h => h.Id) : 0;
			var newHorizontal = new CampaignHorizontal((ZByte)(maxId + 1));
			horizontals.Add(newHorizontal);

			return newHorizontal;
		}

		internal CampaignSummaryStats summaryStats;

		public CampaignSummaryStats SummaryStats
		{
			get
			{
				if (!IsMasterCampaign && !IsTouchCampaign)
				{
					return null;
				}

				if (summaryStats == null)
				{
					LoadSummaryStats();
				}

				return summaryStats;
			}
		}

		void LoadSummaryStats()
		{
			summaryStats = null;
			if (IsMasterCampaign)
			{
				summaryStats = CampaignSummaryStats.Load(PK, Horizontals.Any() ? Horizontals.Max(h => h.Id) : 0);
			}
			else
			{
				MasterCampaign.SummaryStats.OnStatsChanged += OnSummaryStatsOnOnStatsChanged;

				summaryStats = MasterCampaign.SummaryStats.GetSubSummary(this);
			}

			OnPropertyChanged(nameof(SummaryStats));
		}

		void OnSummaryStatsOnOnStatsChanged(IEnumerable<GlbCompanyCampaignItem> updated)
		{
			summaryStats = MasterCampaign.SummaryStats.GetSubSummary(this);
			OnPropertyChanged(nameof(SummaryStats));
		}

		public void RefreshStats()
		{
			if (IsMasterCampaign && !Horizontals.Any())
			{
				return;
			}

			OnTransitionProgressChanged(ResString.GetMultilingualString("0ddd2db3-a7b6-4d7b-8804-5fc177a57f9d", "Loading Summary Stats..."));
			LoadSummaryStats();
			OnEndTransitionProgress();

			if (!IsMasterCampaign)
			{
				return;
			}

			var touchesArray = AllTouches.ToArray();
			for (var i = 0; i < touchesArray.Length; i++)
			{
				var touch = touchesArray[i];
				OnTransitionProgressChanged(ResString.GetMultilingualString("ba2ae711-15e1-4ced-92e4-a6c78620ae00", "Refreshing Summary Stats..."), 100 * (i + 1) / touchesArray.Length);
				touch.MasterCampaign.SummaryStats.OnStatsChanged -= OnSummaryStatsOnOnStatsChanged;
				touch.RefreshStats();
				touch.StatModel.ReloadLinksAndClicks();

				if (i == touchesArray.Length - 1)
				{
					OnEndTransitionProgress();
				}
			}
		}

		#endregion

		#region Transition

		public void DefibrillateTransitions(bool shouldSave = true)
		{
			if (!IsTouchCampaign)
			{
				return;
			}

			MasterCampaign.RefreshStats();

			List<ZGuid> pks = new List<ZGuid>();
			foreach (var campaign in TransitionRulesToThisCampaign.SelectMany(r => r.ParentHorizontalsTouches))
			{
				pks.AddRange(
					campaign.SummaryStats.FailedToTransition
						.Where(
							f => !f.IsBlocked
								 && !f.IsSuspended
								 && f.TrackingStatus != TrackingStatusCodes.Codes.NDR)
						.Select(res => res.CampaignItemId).ToArray());
			}

			var queryPKs = new ZQuery(GlbCompanyCampaignItemSchema.PK, pks);
			var resultQuery = GetQueryToRecalculateQueuedTransitions();

			if (!queryPKs.IsNoResultQuery)
			{
				resultQuery.AddToFilter(queryPKs, JoinCondition.Or);
			}

			var items = Factory.Load<GlbCompanyCampaignItem>(resultQuery);
			foreach (var campaignItem in items)
			{
				campaignItem.G8_IsCheckTransitionRequired = true;
			}

			if (shouldSave && items.Length > 0)
			{
				Factory.Save();
			}
		}

		public bool CanTransfer
		{
			get
			{
				return !IsTransferring && (IsMasterCampaign || IsTouchCampaign);
			}
		}

		bool ShouldTransfer
		{
			get { return CanTransfer && dripEventTriggered; }
		}

		bool dripEventTriggered;
		public bool IsTransferring { get; private set; }

		#region Transition progress

		public event EventHandler<TransitionProgressEventArgs> TransitionProgressChanged;
		public event EventHandler EndTransitionProgress;

		protected virtual void OnTransitionProgressChanged(string statusMessage = null, int percentComplete = 0)
		{
			TransitionProgressChanged?.Invoke(this, new TransitionProgressEventArgs(statusMessage, percentComplete));
		}

		protected void OnEndTransitionProgress()
		{
			EndTransitionProgress?.Invoke(this, EventArgs.Empty);
		}

		public class TransitionProgressEventArgs : EventArgs
		{
			public TransitionProgressEventArgs(string statusMessage = null, int percentComplete = 0)
			{
				StatusMessage = statusMessage;
				PercentComplete = percentComplete;
			}

			public string StatusMessage { get; }
			public int PercentComplete { get; }
		}

		#endregion

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public List<Tuple<ZGuid, TransitionStatus>> TransitionAndSchedule(List<String> errorList = null)
		{
			return TransitionAndSchedule(Array.Empty<ZGuid>(), errorList);
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public virtual List<Tuple<ZGuid, TransitionStatus>> TransitionAndSchedule(IEnumerable<ZGuid> transferItemPKs, List<String> errorList = null)
		{
			var result = new List<Tuple<ZGuid, TransitionStatus>>();

			try
			{
				IsTransferring = true;

				if (TransitionRulesFromThisCampaign.Count == 0)
				{
					return result;
				}

				OnTransitionProgressChanged(ResString.GetMultilingualString("942472dc-c01f-47e2-b0bd-919da0620872", "Calculating transitions..."));
				var transfers = GetTransfers(transferItemPKs);
				var hadRetransfers = false;
				var retransferFactory = new BusinessObjectFactory();
				var transferPlan = new Dictionary<GlbCompanyCampaign, List<ScheduleData>>();
				var retransferPlan = new Dictionary<GlbCompanyCampaign, List<ScheduleData>>();

				var groupedTransfers = transfers.GroupBy(t => t.DestinationGroup).ToList();
				var i = 0;

				foreach (var groupedTransfer in groupedTransfers)
				{
					var group = groupedTransfer.Key;
					var transfersInGroup = groupedTransfer.ToArray();

					foreach (var transfer in transfersInGroup)
					{
						OnTransitionProgressChanged(ResString.GetMultilingualString("0ff21e8f-a8b6-425e-ab4e-28244a00ee6d", "Processing transitions..."), 100 * i++ / transfers.Count);

						if (transfer.SourceCampaignItem.RecipientFromView == null)
						{
							continue;
						}

						var sameDestination = false;
						var retransfer = false;

						foreach (var nextTouch in NextTouches)
						{
							var query = new ZQuery(GlbCompanyCampaignItemSchema.G8_G0, nextTouch.PK);
							query.AddToFilter(GlbCompanyCampaignItemSchema.G8_RecipientID, transfer.SourceCampaignItem.G8_RecipientID);

							var existing = Factory.LoadTop1<GlbCompanyCampaignItem>(query);

							if (existing != null)
							{
								sameDestination = IsSameDestination(transfer, existing, (GlbCompanyCampaign)nextTouch);
								if (sameDestination || existing.G8_TrackingStatus != TrackingStatusCodes.Codes.QUE)
								{
									break;
								}

								var destination = transfer.DestinationCampaign;
								if (transfer.DestinationGroup != null)
								{
									destination = FindTransferDestinationInGroup(group, retransferPlan, transfersInGroup.Length);
									if (destination != null)
									{
										if (!retransferPlan.ContainsKey(destination))
										{
											retransferPlan.Add(destination, new List<ScheduleData>());
										}
										retransferPlan[destination].Add(new ScheduleData(existing.RecipientFromView, ZDateTime.Empty, 0));
									}
								}

								if (destination != null)
								{
									retransfer = true;
									existing.G8_G0 = destination.PK;

									var existingInOtherFactory = retransferFactory.Load<GlbCompanyCampaignItem>(existing.PK);
									existingInOtherFactory.G8_G0 = destination.PK;
									existingInOtherFactory.G8_ScheduleTimeUtc = ZDateTime.Empty;
									existingInOtherFactory.G8_BatchNumber = 0;

									hadRetransfers = true;
								}

								break;
							}
						}

						if (!sameDestination)
						{
							if (!retransfer)
							{
								GlbCompanyCampaign destination = null;
								if (transfer.DestinationCampaign != null)
								{
									destination = transfer.DestinationCampaign;
								}
								else if (transfer.DestinationGroup != null)
								{
									destination = FindTransferDestinationInGroup(group, transferPlan, transfersInGroup.Length) ?? transfer.DestinationGroup.Touches[0];
								}

								var scheduleData =
									destination.SendSettings.GetNextTouchScheduleData(destination, transfer.SourceCampaignItem.RecipientFromView, transferPlan, transfer.SourceCampaignItem.SenderUNLOCO);

								if (!transferPlan.ContainsKey(destination))
								{
									transferPlan.Add(destination, new List<ScheduleData>());
								}

								transferPlan[destination].Add(scheduleData);

								result.Add(transfer.SourceCampaignItem.PK, TransitionStatus.Success);
								transfer.SourceCampaignItem.G8_IsCheckTransitionRequired = false;
							}
							else
							{
								result.Add(transfer.SourceCampaignItem.PK, TransitionStatus.ReTransfer);
								transfer.SourceCampaignItem.G8_IsCheckTransitionRequired = false;
							}
						}
						else
						{
							result.Add(transfer.SourceCampaignItem.PK, TransitionStatus.Success);
							transfer.SourceCampaignItem.G8_IsCheckTransitionRequired = false;
						}
					}
				}

				SaveToQueue(transferPlan, errorList);

				if (hadRetransfers)
				{
					OnTransitionProgressChanged(ResString.GetMultilingualString("bd95ed69-21f6-45d2-a369-7cd5d00e9cd9", "Saving changes to the database..."));
					retransferFactory.Save();
				}
			}
			finally
			{
				OnEndTransitionProgress();
				IsTransferring = false;
			}

			return result;
		}

		static bool IsSameDestination(Transition transfer, GlbCompanyCampaignItem existing, GlbCompanyCampaign nextTouch)
		{
			return (transfer.DestinationCampaign != null && existing.G8_G0 == transfer.DestinationCampaign.PK)
					 || (transfer.DestinationGroup != null && nextTouch.G0_GCG_Group == transfer.DestinationGroup.PK);
		}

		static GlbCompanyCampaign FindTransferDestinationInGroup(GlbCompanyCampaignGroup group, Dictionary<GlbCompanyCampaign, List<ScheduleData>> transferPlan, int transfersInGroupCount)
		{
			GlbCompanyCampaign result = null;

			var transferableGroupTouches = group.Touches.Where(t => t.G0_GroupRatio > 0);

			if (transferableGroupTouches.Any())
			{
				int totalItems = group.Touches.Sum(t => t.SummaryStats.TotalCount) + transfersInGroupCount;
				decimal totalShares = group.Touches.Sum(t => t.G0_GroupRatio);

				result = transferableGroupTouches.Select(touch => new
				{
					Touch = touch,
					Delta = totalItems * touch.G0_GroupRatio / totalShares
							- touch.SummaryStats.TotalCount
							- (transferPlan.ContainsKey(touch) ? transferPlan[touch].Count : 0)
				}).MaxBy(arg => arg.Delta).Touch;
			}

			return result;
		}

		void SaveToQueue(Dictionary<GlbCompanyCampaign, List<ScheduleData>> transferPlan, List<String> errorList)
		{
			var message = ResString.GetMultilingualString("0f66fe05-5e79-4f15-af5c-d51acc8e6120", "Saving to the queue...");
			OnTransitionProgressChanged(message);
			var current = 0;
			var total = transferPlan.Select(pair => pair.Value.Count).Sum();

			OnTransitionProgressChanged(message);
			foreach (var transferItem in transferPlan)
			{
				var targetCampaign = transferItem.Key;
				var scheduleData = transferItem.Value;
				if (!scheduleData.Any())
				{
					continue;
				}

				foreach (var scheduleDatum in scheduleData)
				{
					scheduleDatum.Contact.ScheduleData = scheduleDatum;
				}

				var campaignSender = new GlbCompanyCampaignSender(targetCampaign, scheduleData.Select(x => x.Contact), true);

				campaignSender.ShouldContinueWithSending += (send, resend) => true;
				campaignSender.ItemSent += (sender, args) => { OnTransitionProgressChanged(message, 100 * current++ / total); };
				targetCampaign.IsTransferring = true;
				if (errorList != null)
				{
					campaignSender.MessageOnCampaignSending += (sender, e) =>
					{
						if (e != null && e.IsError)
						{
							errorList.Add(FormattableString.Invariant($"{targetCampaign.CampaignID}: {e.Summary} {e.Message}"));
						}
					};
				}
				campaignSender.CheckAndSendCampaigns(true);

				if (targetCampaign.SendSettings != null)
				{
					var targetCampaignSendSettings = targetCampaign.SendSettings;
					if (targetCampaignSendSettings.IsBatchSchedule && !targetCampaignSendSettings.HasScheduledItems)
					{
						targetCampaignSendSettings.SetBatchRecurrenceQueuedItems();
					}
				}

				if (errorList != null)
				{
					foreach (var error in campaignSender.SendErrors)
					{
						errorList.Add(error);
					}
				}

				targetCampaign.IsTransferring = false;
			}
		}

		ZString touchSourceCampaignPKsForValidation;

		public virtual ModuleIdentifier DripMarketingFilterRuleModule => DefaultDripMarketingFilterRuleModule;

		public static ModuleIdentifier DefaultDripMarketingFilterRuleModule => ModuleIDs.DripMarketingFilterRule;

		internal List<Transition> GetTransfers(IEnumerable<ZGuid> transferItemPKs)
		{
			if (TransitionRulesFromThisCampaign.Count == 0)
			{
				return new List<Transition>();
			}

			var transfers = new List<Transition>();

			var campaignItemsToTransfer = GetCampaignItemsToTransfer(transferItemPKs);
			var transferRules = TransitionRulesFromThisCampaign
				.Where(r => r.NextTouches != null && r.NextTouches.Count > 0)
				.OrderBy(r => r.NextTouches[0].G0_HorizontalId)
				.ThenBy(r => r.NextTouches[0].G0_VerticalId).ToArray();

			for (var i = 0; i < transferRules.Length; i++)
			{
				const int maximumContactsInOneFactoryLoad = 5000;
				var rule = transferRules[i];
				var message = ResString.GetMultilingualString("a1de78f2-8c20-495f-8dcb-6df97a4c5f2e", "Calculating transition rule {0} from {1}...", i + 1, transferRules.Length);

				var suspenders = new List<IDisposable>();
				var deduplicates = new Dictionary<ZGuid, ZBool>();

				foreach (var touch in rule.NextTouches)
				{
					suspenders.Add(touch.SuspendSettingHasChanges());
					deduplicates.Add(touch.PK, touch.G0_DeDuplicateContacts);
					touch.G0_DeDuplicateContacts = false;
					touch.DisableContactsNotSentToQueryCheck = true;
				}

				try
				{
					try
					{
						var campaignItemsToTransferArray = campaignItemsToTransfer.ToArray();
						var currentTransferNumber = 0;
						var totalTransfersNumber = campaignItemsToTransferArray.Length;

						while (currentTransferNumber < totalTransfersNumber && campaignItemsToTransfer.Count > 0)
						{
							var partOfcampaignItemsToTransferArray = campaignItemsToTransferArray.Skip(currentTransferNumber).Take(maximumContactsInOneFactoryLoad).ToArray();
							var campaignContactsQuery = CreateCampaignContactsQuery(rule, partOfcampaignItemsToTransferArray);
							var campaignContactPksMatchingFilter = new HashSet<ZGuid>(Factory.Load<CampaignContact>(campaignContactsQuery).Select(x => x.PK));

							foreach (var campaignItem in partOfcampaignItemsToTransferArray)
							{
								OnTransitionProgressChanged(message, 100 * currentTransferNumber / campaignItemsToTransferArray.Length);

								if (campaignContactPksMatchingFilter.Contains(campaignItem.G8_RecipientID))
								{
									campaignItemsToTransfer.Remove(campaignItem);

									transfers.Add(rule.TouchGroup != null
										? new Transition(campaignItem, rule.TouchGroup)
										: new Transition(campaignItem, rule.NextTouches[0]));
								}

								currentTransferNumber++;

								if (campaignItemsToTransfer.Count == 0)
								{
									break;
								}
							}
						}
					}
					finally
					{
						foreach (var touch in rule.NextTouches)
						{
							touch.G0_DeDuplicateContacts = deduplicates[touch.PK];
							touch.DisableContactsNotSentToQueryCheck = false;
						}
					}

					if (campaignItemsToTransfer.Count == 0)
					{
						break;
					}
				}
				finally
				{
					foreach (var suspender in suspenders)
					{
						suspender.Dispose();
					}
				}
			}

			return transfers;
		}

		ZQuery CreateCampaignContactsQuery(GlbCompanyCampaignDripMarketing rule, IEnumerable<GlbCompanyCampaignItem> campaignItemsToTransfer)
		{
			var filterBizO = RelatedModuleFiltersHelper.GetNewFilterBusinessObjectWithOverridenModuleId(DripMarketingFilterRuleModule, ModuleIDs.GlbCompanyCampaignContact, rule.NextTouches[0]);
			if (filterBizO is IFilterStripBusinessObject filterStripBizO)
			{
				filterStripBizO.ModuleFiltersCreated += ApplyStrategy_ModuleFiltersCreated;
			}
			((ISetCampaignFilterLayoutContext)filterBizO).SetContext();
			filterBizO.QueryObjectType = typeof(CampaignContact);
			filterBizO.LoadLayout(rule.FilterRule);

			var campaignContactsQuery = new ZQuery(filterBizO.Filter);
			campaignContactsQuery.AddToFilter(ViewCampaignContactSchema.PK, campaignItemsToTransfer.Select(x => x.G8_RecipientID));
			return campaignContactsQuery;
		}

		void ApplyStrategy_ModuleFiltersCreated(IFilterStripBusinessObject filterStripBizo)
		{
			var strategy = ObjectFactory.Get<IFilterModuleStrategy>("GlbCompanyCampaignContactFilterModuleStrategy");
			if (strategy != null)
			{
				strategy.RunOnModuleFiltersCreated(filterStripBizo.ModuleFilterCollection, typeof(CampaignContact), filterStripBizo.Factory);
			}
		}

		protected virtual INumberFountainProxy NumberFountain => Env.NumberFountains.GlbCompanyCampaignID;

		void PopulateG0_CampaignIDOnSaving()
		{
			if (!IsInDatabase && G0_CampaignID.IsEmpty)
			{
				G0_CampaignID = NumberFountain.GetNextFormatted(Factory);
			}
		}

		HashSet<GlbCompanyCampaignItem> GetCampaignItemsToTransfer(IEnumerable<ZGuid> transferItemPKs)
		{
			IEnumerable<GlbCompanyCampaignItem> items;

			if (transferItemPKs != null && transferItemPKs.Any())
			{
				var query = new ZQuery(GlbCompanyCampaignItemSchema.PK, transferItemPKs);
				items = Factory.Load<GlbCompanyCampaignItem>(query);
			}
			else
			{
				items = CampaignsItemsSent.Cast<GlbCompanyCampaignItem>();
			}

			return new HashSet<GlbCompanyCampaignItem>(items.Where(c => c.CanTransition));
		}

		internal class Transition
		{
			internal Transition(GlbCompanyCampaignItem sourceCampaignItem, GlbCompanyCampaign destinationCampaign)
			{
				SourceCampaignItem = sourceCampaignItem;
				DestinationCampaign = destinationCampaign;
			}

			internal Transition(GlbCompanyCampaignItem sourceCampaignItem, GlbCompanyCampaignGroup destinationGroup)
			{
				SourceCampaignItem = sourceCampaignItem;
				DestinationGroup = destinationGroup;
			}

			public GlbCompanyCampaignItem SourceCampaignItem { get; }
			public GlbCompanyCampaign DestinationCampaign { get; }
			public GlbCompanyCampaignGroup DestinationGroup { get; }
		}

		#endregion

		#endregion

		#region Property Change Event Handler

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		#region Touch Summary Visibility

		public bool IsTouchSummaryVisible => !IsOpportunityCreationCampaign;

		public string TouchSummaryDescriptionVerified => IsOpportunityCreationCampaign ? TrackingStatusCodes.Descriptions.OPC : TrackingStatusCodes.Descriptions.VER;

		public string TouchSummaryDescriptionUnverified => IsOpportunityCreationCampaign ? TrackingStatusCodes.Descriptions.OPQ : TrackingStatusCodes.Descriptions.UNV;

		#endregion

		#region IImportChildRelatedActivityInfoOnAttach

		bool IImportChildRelatedActivityInfoOnAttach.ImportChildInfo(IRelatableActivity childActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			if (childActivity is OrgOpportunity childOpportunity)
			{
				childOpportunity.P8_G0 = PK;
			}

			return true;
		}

		#endregion

		#region IImportChildRelatedActivityInfoOnDetach

		bool IImportChildRelatedActivityInfoOnDetach.ImportChildInfo(IRelatableActivity childActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			if (childActivity is OrgOpportunity childOpportunity)
			{
				childOpportunity.P8_G0 = ZGuid.Empty;
			}

			return true;
		}

		#endregion

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new GlbCompanyCampaignNumberFountainUniqueIndexFailureHandling(this)); }
		}
		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		class GlbCompanyCampaignNumberFountainUniqueIndexFailureHandling : NumberFountainUniqueIndexFailureHandler
		{
			public GlbCompanyCampaignNumberFountainUniqueIndexFailureHandling(GlbCompanyCampaign campaign)
				: base(GlbCompanyCampaignSchema.Constants.Indexes.NR_UX__G0_CampaignID, campaign)
			{
			}

			protected override INumberFountainProxy NumberFountainToFix => (BizObjCausingError as GlbCompanyCampaign).NumberFountain;
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			G0_CampaignName = ZGuid.NewZGuid().ToString();
			G0_CampaignID = new ZString(ZGuid.NewZGuid().ToString()).SubstringSafe(0, GlbCompanyCampaignSchema.G0_CampaignID.MaxLength);
		}

#endif
	}
}

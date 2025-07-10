using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.GUI
{
	public class DeduplicationOrganisationResultDetail : DeduplicationResultDetail<DeduplicationOrgHeader, DuplicationOrganisationCandidate>
	{
		public DeduplicationOrganisationResultDetail()
			: base()
		{
		}

		public DeduplicationOrganisationResultDetail(
			DeduplicationOrgHeader master,
			IEnumerable<DeduplicationOrgHeader> targets,
			IEnumerable<ScoringResult> scoringResults,
			IEnumerable<PatternMatchingResultModel> ptmResultModels,
			Action closeParentAction)
			: base(master, targets, scoringResults, ptmResultModels, closeParentAction)
		{
		}

		public DeduplicationOrganisationResultDetail(
			DeduplicationOrgHeader master,
			IEnumerable<DeduplicationOrgHeader> targets,
			IEnumerable<ScoringResult> scoringResults,
			IEnumerable<PatternMatchingResultModel> ptmResultModels,
			bool isExcludingInactiveResults,
			bool isExcludingOtherCountries,
			bool isShowIgnoredResults,
			Dictionary<ScoringResult, PatternMatchingResult> resultDictionary)
			: base(master, targets, scoringResults, ptmResultModels, isExcludingInactiveResults, isExcludingOtherCountries, isShowIgnoredResults, resultDictionary)
		{
		}

		public override SecurityCheckpoint ViewEntityFormCheckPoint => Env.Security.OrganisationView;
		public override SecurityCheckpoint DuplicateDetectionMergeCheckPoint => Env.Security.OrgDuplicateDetectionMerge;
		public override SecurityCheckpoint IgnoreDuplicatesCheckPoint => Env.Security.OrgDuplicateDetectionIgnoreDuplicatesForEveryone;
		public override string MasterCode => Master?.OH_Code;
		public override string CandidateCode => CurrentSelectedCandidate?.Code;

		protected override DuplicationOrganisationCandidate EmptyCandidate => DuplicationOrganisationCandidate.Empty;

		IDataProvider<IDeduplicationGlowObject> Provider => provider ?? (provider = new OrgHeaderDataProvider(Master));
		IDataProvider<IDeduplicationGlowObject> provider;

		#region Filter Method

		public override void FilterAndUpdate()
		{
			if (UnfilteredResults != null && UnfilteredResults.Any())
			{
				var filter = UnfilteredResults.Where(x => MeetGeneralPotentialDuplicationsCondition(x) && MeetDynamicFilterConditions(x) || x.IsDummy);
				var duplications = new DuplicationCandidateCollection<DuplicationOrganisationCandidate>();
				duplications.AddRange(filter);
				SetInfoForDuplicationCandidates(duplications);
				DuplicationCandidates.RemoveAll();
				DuplicationCandidates.AddRange(duplications);
				UpdateDuplicationsCount();
			}
		}

		protected override void SetDummyDuplication()
		{
			if (!UnfilteredResults.Any(d => (MeetGeneralPotentialDuplicationsCondition(d))) && Targets.Any())
			{
				var presentModel = new DeduplicationPresenterModel
				{
					Confidence = ConfidenceRating.Undefined,
					MainScore = 0.99,
				};
				var target = Targets.First();
				var presenterModelName = GetDeduplicationPresenterModel(
					DeduplicationDisplayMode.List
					, DeduplicationProvider.Constants.Name
					, DeduplicationProvider.Constants.OrganisationNames
					, Master.OH_FullName
					, target.OH_PK);
				var address = Master.OrgAddresses.First();
				var fullAddress = ((DeduplicationOrgAddress)address).AddressFull;
				var presenterModelAddress = GetDeduplicationPresenterModel(
					DeduplicationDisplayMode.Detailed
					, DeduplicationProvider.Constants.Address
					, DeduplicationProvider.Constants.Addresses
					, fullAddress
					, target.OrgAddresses.First().OA_PK);

				var dummyDeduplicationOrgHeader = new DeduplicationOrgHeader(string.Empty)
				{
					OH_Code = target.OH_Code,
					OH_PK = target.OH_PK,
					IsDummy = true,
					OH_IsActive = true
				};
				var dummyCandidate = new DuplicationOrganisationCandidate(presentModel, dummyDeduplicationOrgHeader)
				{
					TargetPK = target.OH_PK,
					IsSameCountryAsMaster = true,
					DeduplicationPresenterModels = new List<DeduplicationPresenterModel> { presenterModelName, presenterModelAddress }
				};
				SetPotentialDuplicationModelInfo(dummyCandidate);
				DuplicationCandidates.Add(dummyCandidate);
				SelectedCandidatePK = dummyCandidate.TargetPK;
			}
		}

		DeduplicationPresenterModel GetDeduplicationPresenterModel(DeduplicationDisplayMode displayMode, string colName, string childGroupTypeName, string childMasterValue, Guid childTargetID)
		{
			return new DeduplicationPresenterModel
			{
				ChildComparisonResultScore = 0.99,
				ChildConfidenceRating = ConfidenceRating.Undefined,
				ChildDisplayModeForType = displayMode,
				ChildDisplayNameForMasterColumns = colName,
				ChildDisplayNameForTargetColumns = colName,
				ChildGroupNameForType = childGroupTypeName,
				ChildMasterID = Guid.Empty,
				ChildMasterValue = childMasterValue,
				ChildScore = 0.99,
				ChildScoringResultGroupRating = ConfidenceRating.Undefined,
				ChildTargetID = childTargetID,
				ChildTargetValue = string.Empty,
				GlowTargetType = typeof(DeduplicationOrgHeader),
				GroupNameForType = DeduplicationProvider.Constants.Organisations,
				MainScore = 0.99,
				Master = string.Empty,
				MasterType = typeof(DeduplicationOrgHeader),
				MasterValue = string.Empty,
				Target = string.Empty,
				TargetID = Guid.Empty,
				TargetValue = string.Empty
			};
		}

		public Func<List<FilterItemUserControl>> GetDynamicFilterItemUserControls { get; set; }

		bool MeetDynamicFilterConditions(DuplicationOrganisationCandidate candidate)
		{
			var filters = GetDynamicFilterItemUserControls?.Invoke();

			if (filters == null || filters.Count == 0)
			{
				return true;
			}

			var conditionMet = true;
			foreach (var filterItemUserControl in filters)
			{
				var filterType = filterItemUserControl.OrgFilterItemDataSource.OrgFilterTypeDescription.ToString();
				if (!string.IsNullOrWhiteSpace(filterType))
				{
					if (filterType == OrgFilterTypeList.Descriptions.OrgTypes)
					{
						conditionMet = IsOrganizationCheckBoxTypeFilterConditionMet(candidate.DeduplicationOrgHeader, GetSelectedCheckBoxes(filterItemUserControl), filterItemUserControl);
						if (!conditionMet)
						{
							break;
						}
					}
					else
					{
						conditionMet = IsOrganizationInputTypeFilterConditionMet(candidate.DeduplicationOrgHeader,
							filterItemUserControl.OrgFilterItemDataSource.OrgFilterTypeDescription,
							filterItemUserControl.OrgFilterItemDataSource.OrgFilterOptionDescription,
							filterItemUserControl.OrgFilterItemDataSource.OrgFilterKeyword);
						if (!conditionMet)
						{
							break;
						}
					}
				}
			}

			return conditionMet;
		}

		bool IsOrganizationCheckBoxTypeFilterConditionMet(DeduplicationOrgHeader deduplicationOrgHeader, IEnumerable<string> checkBoxOptions, FilterItemUserControl filterItemUserControl)
		{
			if (deduplicationOrgHeader == null)
			{
				return false;
			}

			var header = deduplicationOrgHeader.Master as OrgHeader;
			foreach (var checkBoxOption in checkBoxOptions)
			{
				if (header == null)
				{
					return false;
				}

				var companyData = header.CompanyData;
				if (checkBoxOption == TextConstant.Receivables.GetUnresolvedString())
				{
					var isReceivables = companyData != null && companyData.OB_IsDebtor;
					if (!isReceivables)
					{
						return false;
					}
				}
				else if (checkBoxOption == TextConstant.Payables.GetUnresolvedString())
				{
					var isPayables = companyData != null && companyData.OB_IsCreditor;
					if (!isPayables)
					{
						return false;
					}
				}
				else if (checkBoxOption == TextConstant.Consignee.GetUnresolvedString() && !header.OH_IsConsignee)
				{
					return false;
				}
				else if (checkBoxOption == TextConstant.Consignor.GetUnresolvedString() && !header.OH_IsConsignor)
				{
					return false;
				}
				else if (checkBoxOption == TextConstant.Carrier.GetUnresolvedString() && !header.OH_IsShippingProvider)
				{
					return false;
				}
				else if (checkBoxOption == TextConstant.Forwarder.GetUnresolvedString() && !header.OH_IsForwarder)
				{
					return false;
				}
				else if (checkBoxOption == TextConstant.TransportClient.GetUnresolvedString() && !header.OH_IsTransportClient)
				{
					return false;
				}
				else if (checkBoxOption == TextConstant.Warehouse.GetUnresolvedString() && !header.OH_IsWarehouseClient)
				{
					return false;
				}
				else if (checkBoxOption == TextConstant.Broker.GetUnresolvedString() && !header.OH_IsBroker)
				{
					return false;
				}
				else if (checkBoxOption == TextConstant.Services.GetUnresolvedString() && !header.OH_IsMiscFreightServices)
				{
					return false;
				}
				else if (checkBoxOption == TextConstant.Competitor.GetUnresolvedString() && !header.OH_IsCompetitor)
				{
					return false;
				}
				else if (checkBoxOption == TextConstant.Sales.GetUnresolvedString() && !header.OH_IsSalesLead)
				{
					return false;
				}
				else if (checkBoxOption == TextConstant.ControllingAgent.GetUnresolvedString() && !header.OH_IsControllingAgent)
				{
					return false;
				}
				else if (checkBoxOption == TextConstant.ControllingCustomer.GetUnresolvedString() && !header.OH_IsControllingCustomer)
				{
					return false;
				}
			}

			return true;
		}

		bool IsOrganizationInputTypeFilterConditionMet(DeduplicationOrgHeader deduplicationOrgHeader, string orgFilterName, string orgFilterOption, string orgFilterKeyword)
		{
			if (deduplicationOrgHeader == null)
			{
				return false;
			}

			var propertiesList = new List<string>();
			if (orgFilterName.Equals(OrgFilterTypeList.Descriptions.Email))
			{
				deduplicationOrgHeader.OrgAddresses.ForEach(o => propertiesList.Add(o.OA_Email));
			}
			else if (orgFilterName.Equals(OrgFilterTypeList.Descriptions.Name))
			{
				propertiesList.Add(deduplicationOrgHeader.OH_FullName);
			}
			else if (orgFilterName.Equals(OrgFilterTypeList.Descriptions.MainUNLOCO))
			{
				propertiesList.Add(deduplicationOrgHeader.OH_RL_NKClosestPort);
			}

			return DynamicInputTypeFilter(orgFilterOption, orgFilterKeyword, propertiesList);
		}

		bool DynamicInputTypeFilter(string orgFilterOption, string orgFilterKeyword, IEnumerable<string> properties)
		{
			var meet = true;

			if (orgFilterOption == OrgFilterOptionList.Descriptions.Contains)
			{
				meet = properties.Any(p => p != null && p.Contains(orgFilterKeyword, StringComparison.OrdinalIgnoreCase));
			}
			else if (orgFilterOption == OrgFilterOptionList.Descriptions.NotContain)
			{
				meet = properties.All(p => p != null && !p.Contains(orgFilterKeyword, StringComparison.OrdinalIgnoreCase));
			}
			else if (orgFilterOption == OrgFilterOptionList.Descriptions.ExactMatch)
			{
				meet = properties.All(p => p != null && p.Equals(orgFilterKeyword, StringComparison.OrdinalIgnoreCase));
			}
			else if (orgFilterOption == OrgFilterOptionList.Descriptions.NotEqual)
			{
				meet = properties.All(p => p != null && !p.Equals(orgFilterKeyword, StringComparison.OrdinalIgnoreCase));
			}
			else if (orgFilterOption == OrgFilterOptionList.Descriptions.StartsWith)
			{
				meet = properties.All(p => p != null && p.StartsWith(orgFilterKeyword, StringComparison.OrdinalIgnoreCase));
			}
			else if (orgFilterOption == OrgFilterOptionList.Descriptions.NotStartWith)
			{
				meet = properties.All(p => p != null && !p.StartsWith(orgFilterKeyword, StringComparison.OrdinalIgnoreCase));
			}

			return meet;
		}

		List<string> GetSelectedCheckBoxes(FilterItemUserControl filterItemUserControl)
		{
			var selectedCheckBoxes = new List<string>();
			filterItemUserControl.OrgFilterItemDataSource.CheckBoxItems.ForEach(u =>
			{
				if (u.IsSelected)
				{
					selectedCheckBoxes.Add(u.Code);
				}
			});

			return selectedCheckBoxes;
		}

		#endregion

		public override bool ConfirmMergeCandidateIntoMaster(IEnumerable<DuplicationModelDetailGroup> groups, Func<DuplicationModelDetailGroup, DuplicationModelDetailCollection> collectionGetter)
		{
			if (IsEmpty)
			{
				return false;
			}

			return MergeOrganisationCore(
				Master.Master as OrgHeader ?? Factory.Load<OrgHeader>(Master.OH_PK),
				Factory.Load<OrgHeader>(SelectedCandidatePK),
				groups,
				collectionGetter);
		}

		public override bool ConfirmMergeMasterIntoCandidate(IEnumerable<DuplicationModelDetailGroup> groups, Func<DuplicationModelDetailGroup, DuplicationModelDetailCollection> collectionGetter)
		{
			if (IsEmpty)
			{
				return false;
			}

			return MergeOrganisationCore(
				Factory.Load<OrgHeader>(SelectedCandidatePK),
				Master.Master as OrgHeader ?? Factory.Load<OrgHeader>(Master.OH_PK),
				groups,
				collectionGetter);
		}

		public OrgHeaderDuplicationFinderProxy FinderProxy
		{
			get
			{
				var orgheaderMaster = Master?.Master;
				return orgheaderMaster != null ? OrgHeaderDuplicationFinderProxy.GetInstance((OrgHeader)orgheaderMaster) : default;
			}
		}

		protected override IEnumerable<DeduplicationOrgHeader> GetRestTargetGlows(ReadOnlyBusinessObjectFactory readOnlyBusinessObjectFactory, HashSet<Guid> restResultModelPKs, BusinessObject bizO)
		{
			if (isAdminPanel)
			{
				return Array.Empty<DeduplicationOrgHeader>();
			}

			var restTargetBizOs = OrgHeaderDuplicationFinder.LoadOrgTargetBizOs(readOnlyBusinessObjectFactory, new HashSet<Guid>(restResultModelPKs), Master, (OrgHeader)bizO, false, null);
			return restTargetBizOs.Select(OrgHeaderDuplicationFinder.ConvertOrgToGlowModel).Cast<DeduplicationOrgHeader>();
		}

		protected override Guid GetParentPK(PatternMatchingResultModel resultModel)
		{
			return resultModel.OrgPK;
		}

		protected override IEnumerable<DuplicationOrganisationCandidate> GetDuplicationCandidatesCore() => Provider
			.GetDeduplicationDataSource(GenerateNewPresenterModels(), Targets, resultDictionary)
			.GetDuplicationCandidates()
			.Cast<DuplicationOrganisationCandidate>();

		protected override void SetPotentialDuplicationModelInfo(DuplicationOrganisationCandidate item)
		{
			var matchingResult = resultDictionary?.Values.FirstOrDefault(r => !r.IsDeleted && r.PMT_TargetPK == item.TargetPK);
			if (matchingResult != null)
			{
				item.SetAdminPanelProperties(matchingResult);
			}
		}

		protected override List<ScoringResult> GetScoringResultsCore()
		{
			return Provider.GetBusinessObjectBatcher(ScoringResults, PtmResultModels.ToArray(), Targets).ScoreResults();
		}

		public override ZGuid SelectedCandidatePK
		{
			get => base.SelectedCandidatePK;
			set
			{
				base.SelectedCandidatePK = value;
				this.NotifyPropertyChanged();

				if (!isAdminPanel)
				{
					return;
				}

				if (value != ZGuid.Empty)
				{
					var newSelectCandidate = (DuplicationOrganisationCandidate)DuplicationCandidates.SingleOrDefault(x => ((DuplicationOrganisationCandidate)x).TargetPK == value);
					ShowRemoveIgnoresButton = newSelectCandidate?.IsIgnored ?? false;
					ShowTargetRelatedButtons = !newSelectCandidate?.IsDummy ?? false;
				}
				else
				{
					ShowRemoveIgnoresButton = false;
					ShowTargetRelatedButtons = false;
				}
			}
		}

		public override bool ShowCheckBox => isAdminPanel;

		public override bool ShowRemoveIgnoresButton
		{
			get => ignoreCountBiggerThanZero && isAdminPanel;
			protected set
			{
				ignoreCountBiggerThanZero = value;
				this.NotifyPropertyChanged();
			}
		}

		public override bool ShowIgnoreAllButton => isAdminPanel && !IsEmpty && ShowTargetRelatedButtons;

		public override bool ShowNotMatchButton => Env.Security.OrgDetailsViewNotaMatchButton.IsAllowed;

		public override bool ShowRemoveAllIgnoresButton => ignoreCountBiggerThanZero && DuplicationCandidates.Count > 1;

		public override void IgnoreOrNotMatch(string status, bool forAll)
		{
			if (!Env.Security.OrgDuplicateDetectionIgnoreDuplicates.IsAllowed)
			{
				Env.Security.OrgDuplicateDetectionIgnoreDuplicates.ShowError();
				return;
			}

			if (!IgnoreDuplicatesCheckPoint.IsAllowed && forAll)
			{
				IgnoreDuplicatesCheckPoint.ShowError();
				return;
			}

			if (!IgnoreOrNotMatchCore(status, forAll, out var message))
			{
				Globals.Message.ShowInformation(message);
			}
			else
			{
				SetInfoForDuplicationCandidates(DuplicationCandidates);
			}
		}

		protected override void IgnoreTarget(DuplicationOrganisationCandidate target, string status)
		{
			var targetHeader = Factory.Load<OrgHeader>(target.TargetPK);
			if (!DoesHeaderExist(targetHeader))
			{
				Globals.Message.ShowError(CommonMessage.DuplicateRecordRemovedPressCtrlG);
			}
			else
			{
				FinderProxy.AddIgnore(targetHeader, UserIgnoreStatus.FromString(status), GlbStaff.CurrentUser.GS_Code);
				if (!isAdminPanel)
				{
					RemoveSelectedDuplication();
					UpdateDuplicationsCount();
				}
				ShowRemoveIgnoresButton = true;
			}
		}

		public override void RemoveIgnores(bool forAll)
		{
			if (Master?.Master != null)
			{
				if (Master.Master.IsDeleted)
				{
					Globals.Message.ShowInformation(ResString.GetMultilingualString("50E8EAD7-818E-4FBD-BEE5-20EFF07389A9", "Remove ignores could not be completed successfully as current organization has been merged or deleted. Please close this form."));
				}
				else
				{
					List<DuplicationOrganisationCandidate> targets = null;
					if (forAll)
					{
						if (DuplicationCandidates != null)
						{
							targets = DuplicationCandidates.Cast<DuplicationOrganisationCandidate>().ToList();
						}
					}
					else
					{
						if (SelectedCandidatePK != Guid.Empty)
						{
							targets = new List<DuplicationOrganisationCandidate>() { CurrentSelectedCandidate };
						}
					}
					var proxy = FinderProxy;
					foreach (var target in targets)
					{
						var orgTarget = Factory.Load<OrgHeader>(target.TargetPK);
						proxy.RemoveTemporaryIgnore(orgTarget);
						proxy.RemovePermanentIgnore(orgTarget);
					}
					Factory.Save();

					CreateAndSaveBillingSafe(() =>
					{
						OrgMergeBillingCreator.CreateBillingTransaction(Db.Connection, OrgMergeBillingConstants.Action.RemoveIgnores, SourceWindow, new[] { Master.PK.ToString(), forAll ? OrgMergeBillingConstants.ALLTargets : SelectedCandidatePK.ToString() });
					});

					SetInfoForDuplicationCandidates(DuplicationCandidates);
					SelectedCandidatePK = SelectedCandidatePK;
				}
			}
		}

		public override bool ShowDeactivateMasterButton => (Master?.Master?.IsActive ?? false) && isAdminPanel;

		public override bool ShowDeactivateButton => CurrentSelectedCandidate != null && CurrentSelectedCandidate.IsActive && isAdminPanel && ShowTargetRelatedButtons;

		public override bool ShowLinkButton => CurrentSelectedCandidate != null && ShowTargetRelatedButtons;

		public override void ToggleExclusion()
		{
			try
			{
				var master = Master.Master;
				master.IsExcludedFromDeduplication = !master.IsExcludedFromDeduplication;
				PropagateDeduplicationAction(DeduplicationAction.MasterExclusionToggled);
				OrgMergeBillingCreator.CreateBillingTransaction(Db.Connection, master.IsExcludedFromDeduplication ? OrgMergeBillingConstants.Action.Exclude : OrgMergeBillingConstants.Action.Include, SourceWindow);
				this.NotifyPropertyChanged(nameof(master.IsExcludedFromDeduplication));
			}
			catch (InvalidOperationException x)
			{
				Globals.Message.ShowInformation(x.Message);
			}
		}

		public override void OpenMaster()
		{
			if (Master != null)
			{
				var bizO = Master.Master as BusinessObject;
				this.ShowFormForMaster(bizO, Master.BizoType);
				PropagateDeduplicationAction(DeduplicationAction.OpenMaster);
			}
		}

		public override void OpenTarget(string parameter)
		{
			string message;
			var bizO = GetBusinessObject(out message);
			if (message == null)
			{
				this.ShowFormForCandidate(bizO, Master.BizoType);
				PropagateDeduplicationAction(DeduplicationAction.OpenTarget);
			}
			else
			{
				Globals.Message.ShowInformation(message);
			}
		}

		protected override void RegeneratePatternTables(BusinessObject targetBizo)
		{
			var header = targetBizo as OrgHeader;

			if (header != null)
			{
				var recalculator = new PatternMatchingRecalculator<OrgHeader>(header);

				recalculator.Regenerate(ObjectFactory.Get<IMasterDataProvider>().GetDeduplicationOrgHeader(header));
			}
		}

		public override void DeactivateMaster()
		{
			if (Master != null)
			{
				var masterOrg = Factory.Load<OrgHeader>(Master.PK);
				if (masterOrg != null && ConfirmDeactivateOrg(masterOrg.OH_Code))
				{
					OrgDeactivateHelper.SecurityCheckAndDeactivateOrganization(masterOrg, masterOrg, () =>
					{
						masterOrg.OH_IsActive = false;
						Factory.Save();

						if (Master is DeduplicationOrgHeader dedupOrgHeader)
						{
							dedupOrgHeader.OH_IsActive = false;
						}

						this.NotifyPropertyChanged(nameof(ShowDeactivateMasterButton));
					});
				}
			}
		}

		public override void DeactivateTarget()
		{
			if (SelectedCandidatePK != ZGuid.Empty)
			{
				var targetOrg = Factory.Load<OrgHeader>(SelectedCandidatePK);

				if (targetOrg != null)
				{
					if (ConfirmDeactivateOrg(targetOrg.OH_Code))
					{
						OrgDeactivateHelper.SecurityCheckAndDeactivateOrganization(targetOrg, targetOrg, () =>
						{
							if (Targets?.FirstOrDefault(u => u.PK == targetOrg.PK) is DeduplicationOrgHeader dedupOrgHeader)
							{
								dedupOrgHeader.OH_IsActive = false;
							}

							targetOrg.OH_IsActive = false;
							CurrentSelectedCandidate.IsActive = false;
							Factory.Save();

							SetInfoForDuplicationCandidates(DuplicationCandidates);
							SelectedCandidatePK = SelectedCandidatePK;

							CreateAndSaveBillingSafe(() =>
							{
								OrgMergeBillingCreator.CreateBillingTransaction(Db.Connection, OrgMergeBillingConstants.Action.Deactivate, SourceWindow);
							});
						});
					}
				}
			}
		}

		bool ConfirmDeactivateOrg(string orgHeaderCode) => DialogResult.Yes == Globals.Message.Show(Res.GetString("be61006a-81cf-4d5c-b734-a8aedd9c2f83", @"Are you sure you want to deactivate the organization '{0}'?", orgHeaderCode), Res.GetString("0ee3d907-896d-4a3c-abe6-045712565d0d", "Deactivate Organization"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

		public override void ExecuteLink()
		{
			var securityOrgModify = Master.IsInDatabase ? Env.Security.OrgDetailsModifyRelatedParties : Env.Security.OrgDetailsNewModifyRelatedParties;
			if (!securityOrgModify.IsAllowed)
			{
				securityOrgModify.ShowError();
				return;
			}

			var targetOrganisation = Factory.Load<OrgHeader>(SelectedCandidatePK);
			if (targetOrganisation == null)
			{
				Globals.Message.ShowError(CommonMessage.DuplicateRecordRemovedPressCtrlG);
				return;
			}

			var masterOrg = Master.Master as OrgHeader;
			if (masterOrg != null)
			{
				var organisationLink = new OrgHeaderLink(Factory, Globals.Message, masterOrg, targetOrganisation);

				using (var form = new OrgHeaderLinkForm(organisationLink))
				{
					var dialogResult = ZFormModaliser.ShowDialogAndDispose(form);
					if (dialogResult != DialogResult.Cancel)
					{
						var findRelationshipQuery = new ZQuery(OrgRelatedPartySchema.PR_OH_Parent, Master.PK); // search all child.PK that is org1 or org2
						findRelationshipQuery.AddToFilter(JoinCondition.Or, OrgRelatedPartySchema.PR_OH_Parent, targetOrganisation.PK);
						var relatedList = Factory.Load<OrgRelatedParty>(findRelationshipQuery);

						if (relatedList != null && relatedList.Length > 0)
						{
							var related1 = relatedList.FirstOrDefault();
							var related2 = relatedList.LastOrDefault();
							if (related1.PR_OH_RelatedParty == Master.PK || related1.PR_OH_RelatedParty == targetOrganisation.PK || related2.PR_OH_RelatedParty == Master.PK || related2.PR_OH_RelatedParty == targetOrganisation.PK) // if they are linked together
							{
								SelectedCandidatePK = DuplicationCandidates.Cast<DuplicationCandidate>()
										.Any(u => u.TargetPK == targetOrganisation.PK)
										? targetOrganisation.PK
										: ZGuid.Empty;
								RemoveSelectedDuplication();
							}
							if (related1.PR_OH_RelatedParty == related2.PR_OH_RelatedParty && related1.PR_OH_Parent != related2.PR_OH_Parent)
							{
								SelectedCandidatePK = DuplicationCandidates.Cast<DuplicationCandidate>()
										.Any(u => u.TargetPK == targetOrganisation.PK)
										? targetOrganisation.PK
										: ZGuid.Empty;
								DuplicationCandidates.Remove(DuplicationCandidates.Cast<DuplicationOrganisationCandidate>().FirstOrDefault(x => x.TargetPK == related1.PR_OH_RelatedParty));
								RemoveSelectedDuplication();
							}

							CreateAndSaveBillingSafe(() =>
							{
								OrgMergeBillingCreator.CreateBillingTransaction(Db.Connection, OrgMergeBillingConstants.Action.Link, SourceWindow, new[] { Master.PK.ToString(), targetOrganisation.PK.ToString(), related1.PR_OH_RelatedParty.ToString() });
							});
						}
					}
				}
				PropagateDeduplicationAction(DeduplicationAction.Link);
			}
		}

		bool MergeOrganisationCore(OrgHeader orgToRetain, OrgHeader orgToDissolve, IEnumerable<DuplicationModelDetailGroup> groups, Func<DuplicationModelDetailGroup, DuplicationModelDetailCollection> collectionGetter)
		{
			if (!DuplicateDetectionMergeCheckPoint.IsAllowed)
			{
				DuplicateDetectionMergeCheckPoint.ShowError();
				return false;
			}

			if (orgToRetain is null || !orgToRetain.IsInDatabase || orgToDissolve is null || !orgToDissolve.IsInDatabase)
			{
				Globals.Message.ShowWarning(ResString.GetMultilingualString("4fe2e9d8-3c73-44ba-8d06-8ea2fce9905b", "Organization must be saved to the database before merging can occur."));
				return false;
			}

			var modelDetails = groups.Where(group => group.IsSupportMerge).SelectMany(group => collectionGetter.Invoke(group).OfType<DuplicationModelDetail>());
			var mergeOrganisation = new MergeOrgHeader(Factory, orgToDissolve, orgToRetain);

			foreach (var oldOrgAddress in mergeOrganisation.OldOrgAddressesCollection.Cast<MergeOrgAddress>())
			{
				var model = modelDetails.FirstOrDefault(x => x.SelfBizoPk == oldOrgAddress.OldAddressPK);
				if (model != null)
				{
					if (model.MergeMode == DedupeMergeMode.Add)
					{
						oldOrgAddress.Action = MergeModeValue.Add;
					}
					else if (model.MergeMode == DedupeMergeMode.Merge)
					{
						oldOrgAddress.Action = MergeModeValue.Merge;
						oldOrgAddress.NewAddressPK = model.OpponentBizoPk;
					}
				}
			}

			foreach (var oldContact in mergeOrganisation.OldOrgContactCollection.Cast<MergeOrgContact>())
			{
				var model = modelDetails.FirstOrDefault(x => x.SelfBizoPk == oldContact.OldContactPK);
				if (model != null)
				{
					if (model.MergeMode == DedupeMergeMode.Add)
					{
						oldContact.Action = MergeModeValue.Add;
					}
					else if (model.MergeMode == DedupeMergeMode.Merge)
					{
						oldContact.Action = MergeModeValue.Merge;
						oldContact.NewContactPK = model.OpponentBizoPk;
					}
				}
			}

			var mergeResult = OrganizationMergeHelper.Merge(mergeOrganisation, out var elapsedMilliseconds);
			HandleMergeResult(mergeResult, elapsedMilliseconds, orgToRetain.PK, orgToDissolve.PK, CurrentSelectedCandidate);

			PropagateDeduplicationAction(DeduplicationAction.Merge);

			return mergeResult is MergeResult.Success;
		}

		void HandleMergeResult(MergeResult mergeResult, long elapsedMilliseconds, ZGuid retainedOrgPK, ZGuid dissolvedOrgPK, DuplicationCandidate candidate)
		{
			CreateAndSaveBillingSafe(() =>
			{
				switch (mergeResult)
				{
					case MergeResult.Success:
						OrgMergeBillingCreator.CreateBillingTransaction(Db.Connection, OrgMergeBillingConstants.Action.MergeSuccess, SourceWindow, candidate.Confidence.ToString(), new[] { retainedOrgPK.ToString(), dissolvedOrgPK.ToString(), $"{elapsedMilliseconds}ms" }); // Partition Info in Billing Reference
						break;
					case MergeResult.Failed:
					case MergeResult.MergedWithErrors:
					case MergeResult.FailedWithCriticalError:
						OrgMergeBillingCreator.CreateBillingTransaction(Db.Connection, OrgMergeBillingConstants.Action.MergeFailure, SourceWindow, candidate.Confidence.ToString(), new[] { retainedOrgPK.ToString(), dissolvedOrgPK.ToString(), mergeResult.ToString() });
						break;
					case MergeResult.Cancelled:
						OrgMergeBillingCreator.CreateBillingTransaction(Db.Connection, OrgMergeBillingConstants.Action.MergeCancel, SourceWindow, candidate.Confidence.ToString(), new[] { retainedOrgPK.ToString(), dissolvedOrgPK.ToString() });
						break;
				}
			});

			if (mergeResult == MergeResult.Success)
			{
				if (retainedOrgPK != candidate.TargetPK)
				{
					RemoveSelectedDuplication();
				}

				CloseParent();
				PropagatePerformSearch(this, null);

				ZApplication.GetOpenForms().OfType<ZOrganisationsForm>().FirstOrDefault(x => ((OrgHeader)x.DataSource)?.PK == dissolvedOrgPK)?.Close();
				ZApplication.GetOpenForms().OfType<ZOrganisationsForm>().FirstOrDefault(x => ((OrgHeader)x.DataSource)?.PK == retainedOrgPK)?.ReloadForm();
			}
		}

		string SourceWindow => isAdminPanel ? OrgMergeBillingConstants.Source.DeduplicationResultsViewerForm : OrgMergeBillingConstants.Source.DeduplicationResultsViewerForm;

		void CreateAndSaveBillingSafe(Action actionWithFactorySave)
		{
			try
			{
				actionWithFactorySave?.Invoke();
			}
			catch (Exception ex) when (ex is ZSaveConcurrencyException || ex is ZCannotSaveException || ex is ZSaveException)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		protected override Action<string, bool, IEnumerable<DuplicationOrganisationCandidate>> CreateIgnoreBillingTransaction => (status, isForAll, targets) =>
		{
			Argument.NotNull(targets, nameof(targets));
			var ignoreStatus = UserIgnoreStatus.FromString(status);
			if (ignoreStatus.Name.Equals(PatternMatchingResult.StatusCodes.PermanentIgnore))
			{
				CreateAndSaveBillingSafe(() =>
				{
					if (isForAll)
					{
						OrgMergeBillingCreator.CreateBillingTransaction(Db.Connection, OrgMergeBillingConstants.Action.IgnoreForEveryone, SourceWindow, new[] { Master.PK.ToString(), OrgMergeBillingConstants.ALLTargets });
					}
					else
					{
						if (targets.Count() != 1)
						{
							throw new ArgumentOutOfRangeException(nameof(targets));
						}

						var currentSelectedDuplication = targets.Single();
						OrgMergeBillingCreator.CreateBillingTransaction(Db.Connection, OrgMergeBillingConstants.Action.IgnoreForEveryone, SourceWindow, currentSelectedDuplication.Confidence.ToString(), Master.PK.ToString(), currentSelectedDuplication.PK.ToString());
					}
				});
			}
		};
	}
}

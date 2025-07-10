using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public class DeniedPartyScreeningPresentationManager : IDeniedPartyScreeningPresentationManager
	{
		#region Create Menus

		public void CreateMenusForJob(ZForm parentForm, Func<string> screeningNotEnabledMessage = null)
		{
			CreateMenus(parentForm, false, screeningNotEnabledMessage: screeningNotEnabledMessage);
		}

		public void CreateMenusForJob(ZForm parentForm, IBusiness businessEntity, Func<bool> parentEntityHasChanges)
		{
			CreateMenus(parentForm, false, businessEntity, parentEntityHasChanges);
		}

		public void CreateMenusForScreeningEntity(ZForm parentForm)
		{
			CreateMenus(parentForm, true);
		}

		void CreateMenus(ZForm parentForm, bool isScreeningEntity, IBusiness businessEntity = null, Func<bool> parentEntityHasChanges = null, Func<string> screeningNotEnabledMessage = null)
		{
			Argument.NotNull(parentForm, "Parent Form");

			businessEntity = businessEntity ?? parentForm.BusinessEntity;
			var hasAnyExcludedLists = DeniedPartyScreenerAsync.HasExcludedList(businessEntity?.Factory);

			if (!isScreeningEntity && ComplianceRiskHelper.CheckIfComplianceRiskEnabled(businessEntity.GetType(), allowViewType: false))
			{
				return;
			}

			if (parentForm is IFileMenuItemsProvider menuProvider)
			{
				if (menuProvider.ActionsMenuItem.MenuItems.Count > 0 &&
					menuProvider.ActionsMenuItem.MenuItems[menuProvider.ActionsMenuItem.MenuItems.Count - 1].Text != "-")
				{
					ZFormMenuStrategy.AddActionsMenuItem(menuProvider, "-", null);
				}
			}

			if (isScreeningEntity)
			{
				ZFormMenuStrategy.AddActionsMenuItem(parentForm, new ZMenuItem(ResString.GetMultilingualString("DeniedPartyPresentationManager|Screen", "Screen"), async delegate
				{
					var partyProvider = businessEntity as IScreeningPartyProvider;
					if (ScreeningIsAllowedForCurrentDataSource(businessEntity) && partyProvider != null)
					{
						var parties = partyProvider.ScreeningParties;
						await PerformScreening(parentForm, DpsSourceWithParties.GetSingleSourceList(businessEntity as BusinessObject, parties), parties, isScreeningEntity, forceAllLists: false, parentEntityHasChanges: parentEntityHasChanges);
					}
				}));

				if (hasAnyExcludedLists)
				{
					ZFormMenuStrategy.AddActionsMenuItem(parentForm, new ZMenuItem(ResString.GetMultilingualString("DeniedPartyPresentationManager|Screen_ForceFull", "Screen (Full List)"), async delegate
					{
						var partyProvider = businessEntity as IScreeningPartyProvider;
						if (ScreeningIsAllowedForCurrentDataSource(businessEntity) && partyProvider != null)
						{
							var parties = partyProvider.ScreeningParties;
							await PerformScreening(parentForm, DpsSourceWithParties.GetSingleSourceList(businessEntity as BusinessObject, parties), parties, isScreeningEntity, forceAllLists: true, parentEntityHasChanges: parentEntityHasChanges);
						}
					}));
				}

				new DeniedPartyScreeningActionsProvider(parentForm).AddEntitiesMenuItem();
			}
			else
			{
				ZFormMenuStrategy.AddActionsMenuItem(parentForm, new ZMenuItem(ResString.GetMultilingualString("DeniedPartyPresentationManager|ViewComplianceStatus", "View Compliance Status"), async delegate
				{
					var partyProvider = businessEntity as IScreeningPartyProvider;
					if (partyProvider != null)
					{
						if (!ShouldDisplayScreeningNotEnabledMessage(screeningNotEnabledMessage))
						{
							await PerformScreening(parentForm, isScreeningEntity: false, forceAllLists: false, parentBizo: businessEntity as BusinessObject, parentEntityHasChanges: parentEntityHasChanges);
						}
					}
				}));

				ZFormMenuStrategy.AddActionsMenuItem(parentForm, new ZMenuItem(ResString.GetMultilingualString("DeniedPartyPresentationManager|ResynchronizeScreeningStatus", "Resynchronize Screening Status"), delegate
				{
					if (businessEntity is BusinessObject businessObject)
					{
						if (!ShouldDisplayScreeningNotEnabledMessage(screeningNotEnabledMessage))
						{
							var partyProvider = businessEntity as IScreeningPartyProvider ?? parentForm.BusinessEntity as IScreeningPartyProvider;
							ReloadScreeningParties(partyProvider, businessObject);
							ResynchronizeScreeningStatus(true, new[] { businessObject }, parentEntityHasChanges);
						}
					}
				}));
			}
		}

		#endregion

		#region Create Module Menus

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void CreateModuleMenusForJob(ZFilterGridModule module, List<MenuItem> moduleActionMenu, Func<ZFilterGridModule, (BusinessObject[] BusinessObjects, bool HasInvalidItems)> getSelectedBusinessObjects = null, Func<string> screeningNotEnabledMessage = null)
		{
			CreateModuleMenus(module, moduleActionMenu, false, getSelectedBusinessObjects, screeningNotEnabledMessage);
		}

		public void CreateModuleMenusForScreeningEntity(ZFilterGridModule module, List<MenuItem> moduleActionMenu)
		{
			CreateModuleMenus(module, moduleActionMenu, true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		void CreateModuleMenus(ZFilterGridModule module, List<MenuItem> moduleActionMenu, bool isScreeningEntity, Func<ZFilterGridModule, (BusinessObject[] BusinessObjects, bool HasInvalidItems)> getSelectedBusinessObjects = null, Func<string> screeningNotEnabledMessage = null)
		{
			getSelectedBusinessObjects = getSelectedBusinessObjects ?? (u => (u.GetSelectedBusinessObjects(), false));

			Argument.NotNull(module, "Filter Grid Module");
			Form parentForm = module.LocateMainForm();

			if (!isScreeningEntity && ComplianceRiskHelper.CheckIfComplianceRiskEnabled(module.TypeOfTopLevelBusinessObject, allowViewType: true))
			{
				return;
			}

			if (moduleActionMenu.Count > 0 && moduleActionMenu[moduleActionMenu.Count - 1].Text != "-")
			{
				moduleActionMenu.Add(new ZMenuItem("-"));
			}

			if (isScreeningEntity)
			{
				var initialSelection = module.TypeOfTopLevelBusinessObject.IsAssignableFrom(typeof(OrgHeader)) ? StandaloneScreeningSupportedCandidate.Org : StandaloneScreeningSupportedCandidate.Vessel;

				moduleActionMenu.Add(new ZMenuItem(ResString.GetMultilingualString("DeniedPartyPresentationManager|ModuleScreen", "Screen"), async delegate
				{
					var result = getSelectedBusinessObjects(module);
					await ModuleScreening(parentForm, result.BusinessObjects, isScreeningEntity, false, result.HasInvalidItems);
				}));

				moduleActionMenu.Add(new ZMenuItem(ResString.GetMultilingualString("DeniedPartyPresentationManager|ModuleScreenStandAlone", "Screen (Stand Alone)"), delegate
				{
					if (DpsSecurityRights.IsGrantedScreeningWithShowError())
					{
						ZFormModaliser.Show(new StandAloneScreeningForm(forceFullListRatherThanCutDownList: false, initialSelection: initialSelection), parentForm);
					}
				}));

				if (DeniedPartyScreenerAsync.HasExcludedList(module.FilterBusinessObject?.Factory))
				{
					moduleActionMenu.Add(new ZMenuItem(ResString.GetMultilingualString("DeniedPartyPresentationManager|ModuleScreen_ForceFull", "Screen (Full List)"), async delegate
					{
						var result = getSelectedBusinessObjects(module);
						await ModuleScreening(parentForm, result.BusinessObjects, isScreeningEntity, true, result.HasInvalidItems);
					}));

					moduleActionMenu.Add(new ZMenuItem(ResString.GetMultilingualString("DeniedPartyPresentationManager|ModuleScreenStandAlone_ForceFull", "Screen (Stand Alone) (Full List)"), delegate
					{
						if (DpsSecurityRights.IsGrantedScreeningFullListWithShowError())
						{
							ZFormModaliser.Show(new StandAloneScreeningForm(forceFullListRatherThanCutDownList: true, initialSelection: initialSelection), parentForm);
						}
					}));
				}
			}
			else
			{
				moduleActionMenu.Add(new ZMenuItem(ResString.GetMultilingualString("DeniedPartyPresentationManager|ModuleViewComplianceStatus", "View Compliance Status"), async delegate
				{
					if (!ShouldDisplayScreeningNotEnabledMessage(screeningNotEnabledMessage))
					{
						var result = getSelectedBusinessObjects(module);
						await ModuleScreening(parentForm, result.BusinessObjects, false, false, result.HasInvalidItems);
					}
				}));

				moduleActionMenu.Add(new ZMenuItem(ResString.GetMultilingualString("DeniedPartyPresentationManager|ModuleResynchronizeScreeningStatus", "Resynchronize Screening Status"), delegate
				{
					if (!ShouldDisplayScreeningNotEnabledMessage(screeningNotEnabledMessage))
					{
						var result = getSelectedBusinessObjects(module);
						ResynchronizeScreeningStatusForModule(result.BusinessObjects, result.HasInvalidItems);
					}
				}));
			}
		}

		async Task ModuleScreening(Form parentForm, BusinessObject[] selectedBusinessObjects, bool isScreeningEntity, bool forceAllLists, bool hasInvalidItems)
		{
			if (selectedBusinessObjects.Length > 0 && !hasInvalidItems)
			{
				var sourceBusinessObjectWithPartiesList = new List<IDpsSourceWithParties>();
				var screeningParties = new List<ScreeningParty>();
				using (new ZWaitCursorChanger())
				{
					foreach (BusinessObject bizo in selectedBusinessObjects)
					{
						if (bizo is IScreeningPartyProvider provider)
						{
							var sourceBusinessObjectWithParties = new DpsSourceWithParties(bizo, provider.ScreeningParties);

							sourceBusinessObjectWithPartiesList.Add(sourceBusinessObjectWithParties);
							screeningParties.AddRange(sourceBusinessObjectWithParties.ScreenParties);
						}
					}
				}
				if (screeningParties.Count > 0)
				{
					await PerformScreening(parentForm, sourceBusinessObjectWithPartiesList, screeningParties.ToArray(), isScreeningEntity, forceAllLists);
				}
			}
			else if (hasInvalidItems)
			{
				ShowMessageHasSelectedInvalidItems();
			}
			else
			{
				Globals.Message.Show(Res.GetString("3e4224cb-7429-495d-b45a-396c026152cf", "Please select at least 1 row to screen against Denied Party Listings."));
			}
		}

		#endregion

		#region Implementation

		bool ScreeningIsAllowedForCurrentDataSource(IBusiness businessEntity)
		{
			var result = true;
			if (businessEntity is OrgHeader header &&
				(DeniedPartyScreeningHelper.IsSystemDefinedUnmatchedOrganizationWithShowMessage(header) ||
				DeniedPartyScreeningHelper.IsInactiveEntityWithShowMessage(header, header.OH_IsActive)))
			{
				result = false;
			}

			return result;
		}

		void ReloadScreeningParties(IScreeningPartyProvider partyProvider, BusinessObject parentBizo)
		{
			if (parentBizo != null && partyProvider != null && parentBizo is not OrgHeader && parentBizo is not RefVessel)
			{
				partyProvider.ScreeningParties.ForEach(party =>
				{
					if (party?.ScreeningEntity != null && !party.ScreeningEntity.HasChanges && !(party.ScreeningEntity is NonPersistentBusinessObject))
					{
						party.ScreeningEntity.ReloadSafe();
					}
				});
			}
		}

		public async Task PerformScreening(ZForm parentForm, bool isScreeningEntity, bool forceAllLists, BusinessObject parentBizo = null, Func<bool> parentEntityHasChanges = null, Func<string> screeningNotEnabledMessage = null)
		{
			IScreeningPartyProvider partyProvider;
			if (parentBizo == null)
			{
				parentBizo = parentForm.BusinessEntity as BusinessObject;
				partyProvider = parentForm.BusinessEntity as IScreeningPartyProvider;
			}
			else
			{
				partyProvider = parentBizo as IScreeningPartyProvider;
			}

			if (!ShouldDisplayScreeningNotEnabledMessage(screeningNotEnabledMessage))
			{
				if (parentBizo != null && partyProvider != null)
				{
					if (!isScreeningEntity)
					{
						ReloadScreeningParties(partyProvider, parentBizo);
						ResynchronizeScreeningStatus(false, new[] { parentBizo }, parentEntityHasChanges);
					}
					var parties = partyProvider.ScreeningParties;
					await PerformScreening(parentForm, DpsSourceWithParties.GetSingleSourceList(parentBizo, parties), parties, isScreeningEntity, forceAllLists, parentEntityHasChanges: parentEntityHasChanges);
				}
			}
		}

		public async Task PerformScreening(object parentForm, List<IDpsSourceWithParties> iSourceBizOs, IScreeningParty[] screeningParties, bool isScreeningEntity, bool forceAllLists, bool suppressDeveloperException = false, Func<bool> parentEntityHasChanges = null, IComplianceRiskAction complianceRiskAction = null)
		{
			var sourceBizOs = iSourceBizOs?.Cast<DpsSourceWithParties>().ToList();

			if (await DeniedPartyScreeningTermsAgreement.HasBeenAcknowledged((Form)parentForm))
			{
				if (!DpsSecurityRights.IsGrantedScreeningWithShowError() || !DpsSecurityRights.IsGrantedScreeningPermanentClearWithShowError(sourceBizOs) || forceAllLists && !DpsSecurityRights.IsGrantedScreeningFullListWithShowError())
				{
					return;
				}

				if (sourceBizOs != null && (sourceBizOs.Any(u => u.SourceBizO.HasChanges) || sourceBizOs.Any(u => !u.SourceBizO.IsInDatabase)) || (parentEntityHasChanges?.Invoke() ?? false))
				{
					ShowMessageNeedToSaveTheForm();
					return;
				}

				ScreeningParty[] uniqueParties = DeniedPartyScreenerAsync.MergeDuplicateParties((ScreeningParty[])screeningParties).ToArray();

				if (uniqueParties.Length > 0)
				{
					var showMessageIfNoPartiesToScreenAction = new Func<bool, Task>(async isRescreen =>
					{
						var partiesToScreen = uniqueParties.Where(party => !party.IsCurrentScreeningStatusValid && party.IsActive).ToArray();
						if (partiesToScreen.Any() || !isScreeningEntity)
						{
							await ExecuteScreening((Form)parentForm, sourceBizOs, isRescreen, isScreeningEntity, forceAllLists, partiesToScreen, complianceRiskAction);
						}
						else
						{
							Globals.Message.Show(Res.GetString("dc05fa2b-4114-4c3a-b998-10c0fe3ebbdb", @"There are no parties to screen for this record, either because all parties have already been screened or the selected parties are inactive."));
						}
					});

					if (isScreeningEntity)
					{
						uniqueParties = DeniedPartyScreeningWorker.ExcludePermanentClearParties(uniqueParties);
						uniqueParties.ForEach(party => party.IsCurrentScreeningStatusValid = false);

						await showMessageIfNoPartiesToScreenAction(false);
					}
					else
					{
						uniqueParties.ForEach(party => party.CalculateScreeningStatusIsValid());

						var partyComplianceForm = GetPartyComplianceForm(uniqueParties, showMessageIfNoPartiesToScreenAction, complianceRiskAction);
						ZFormModaliser.Show(partyComplianceForm, (Form)parentForm);
					}
				}
				else
				{
					Globals.Message.Show(Res.GetString("dc05fa2b-4114-4c3a-b998-10c0fe3ebbdb", @"There are no parties to screen for this record, either because all parties have already been screened or the selected parties are inactive."));
				}

				ScreeningStatusUpdater.RefreshDeniedPartyStatusUpdatedLog(sourceBizOs?.Select(o => o.SourceBizO).ToList());
			}

			complianceRiskAction?.UpdateVisibilityIfNeeded();
		}

		#region Resynchronize Screening Status

		internal protected void ResynchronizeScreeningStatusForModule(BusinessObject[] selectedBusinessObjects, bool hasInvalidItems)
		{
			if (selectedBusinessObjects?.Length > 0 && !hasInvalidItems)
			{
				ResynchronizeScreeningStatus(true, selectedBusinessObjects, null);
			}
			else if (hasInvalidItems)
			{
				ShowMessageHasSelectedInvalidItems();
			}
			else
			{
				Globals.Message.Show(Res.GetString("4AC83EA3-4DC5-4ED1-85DF-4551FF4E19BB", "Please select at least 1 row to resynchronize."));
			}
		}

		void ShowMessageHasSelectedInvalidItems()
		{
			Globals.Message.ShowWarning(Res.GetString("512214ef-6385-4689-8a6f-c6eac3b6605d", "One or more of the rows you have selected are not valid for screening."));
		}

		public void ResynchronizeScreeningStatus(bool verboseMode, BusinessObject[] selectedBusinessObjects, Func<bool> parentEntityHasChanges)
		{
			if (!Env.Security.OrgDeniedPartyScreeningAllowResynchronize.IsAllowed)
			{
				if (verboseMode)
				{
					Env.Security.OrgDeniedPartyScreeningAllowResynchronize.ShowError();
				}

				return;
			}

			if (selectedBusinessObjects != null)
			{
				var bizOsSupportScreening = selectedBusinessObjects.Where(u => u is IScreeningPartyProvider).ToArray();

				if (bizOsSupportScreening.Length > 0)
				{
					if (bizOsSupportScreening.Any(u => u.HasChanges || !u.IsInDatabase || IsTemplateRecordHasChanges(u))
						|| (parentEntityHasChanges?.Invoke() ?? false))
					{
						if (verboseMode)
						{
							ShowMessageNeedToSaveTheForm();
						}

						return;
					}

					var messages = new List<string>();

					foreach (var bizO in bizOsSupportScreening)
					{
						var codeAndType = DpsMessageInfo.GetJobCodeInfo(bizO);
						var partyProvider = (IScreeningPartyProvider)bizO;
						var rawStatus = partyProvider.ScreeningStatus;
						var updateTo = ScreeningStatusUpdater.GetScreenStatusUpdateTo(partyProvider);

						if (rawStatus != ScreeningStatusesList.Codes.JobCleared && rawStatus != ScreeningStatusesList.Codes.JobBlockedExternal && rawStatus != ScreeningStatusesList.Codes.JobClearedExternal)
						{
							if (updateTo != rawStatus)
							{
								DpsScreeningStatusResynchronizer.SaveWithVerbose(verboseMode, bizO);
								messages.Add(Res.GetString("354AC3CB-0492-4075-9FD8-EBD983287242", "{0} screening status has been resynchronized from {1} to {2}.", codeAndType.JobCode, rawStatus, partyProvider.ScreeningStatus));
							}
							else
							{
								messages.Add(Res.GetString("CB96E946-A093-4E28-A5C5-CE0B22C9F467", "{0} is synchronized, no need for resynchronization.", codeAndType.JobCode));
							}
						}
						else
						{
							string GetDescription(string status) => status switch
							{
								ScreeningStatusesList.Codes.JobCleared => ScreeningStatusesList.Descriptions.JobCleared,
								ScreeningStatusesList.Codes.JobBlockedExternal => ScreeningStatusesList.Descriptions.JobBlockedExternal,
								_ => ScreeningStatusesList.Descriptions.JobClearedExternal
							};

							messages.Add(Res.GetString("55C6816C-B388-4514-B9A3-F4FE6EF291A6", "{0} is {1}, no need for resynchronization.", codeAndType.JobCode, GetDescription(rawStatus)));
						}

						DpsScreeningStatusResynchronizer.SynchronizeScreeningStatusToDeclarationsAndSave(verboseMode, bizO);
					}

					if (verboseMode)
					{
						Globals.Message.Show(string.Join(System.Environment.NewLine, messages));
					}
				}
			}
		}

		bool IsTemplateRecordHasChanges(BusinessObject bizo)
		{
			return bizo is ITemplateRecordProvider { TemplateRecord: BusinessObject b } && b.HasChanges;
		}

		#endregion

		void ShowMessageNeedToSaveTheForm()
		{
			Globals.Message.Show(Res.GetString("3e7250bd-fb96-4cc9-801d-fec5cbf4646a", "Please save the form before screening for denied parties."));
		}

		protected internal PartyComplianceForm GetPartyComplianceForm(ScreeningParty[] uniqueParties, Func<bool, Task> showMessageIfNoPartiesToScreenAction, IComplianceRiskAction complianceRiskAction)
		{
			var partyComplianceWrapperCollection = new PartyComplianceWrapperFilteredCollection(uniqueParties);
			var partyComplianceForm = new PartyComplianceForm(partyComplianceWrapperCollection, complianceRiskAction != null);

			partyComplianceForm.Closed += async (sender, e) =>
			{
				if (partyComplianceForm.PerformButtonClick != PartyComplianceButton.Cancel)
				{
					var isForceRescreen = partyComplianceForm.PerformButtonClick == PartyComplianceButton.ForceRescreen;

					if (partyComplianceForm.SelectedParties != null)
					{
						var partiesNeedToScreen = DeniedPartyScreeningWorker.ExcludePermanentClearParties(partyComplianceForm.SelectedParties);

						uniqueParties.ForEach(party =>
						{
							party.IsCurrentScreeningStatusValid = !partiesNeedToScreen.Contains(party);
						});
					}
					else if (isForceRescreen)
					{
						uniqueParties = DeniedPartyScreeningWorker.ExcludePermanentClearParties(uniqueParties);

						uniqueParties.ForEach(party => party.IsCurrentScreeningStatusValid = false);
					}

					if (showMessageIfNoPartiesToScreenAction != null)
					{
						await showMessageIfNoPartiesToScreenAction.Invoke(isForceRescreen);
					}
				}

				complianceRiskAction?.UpdateVisibilityIfNeeded();
			};

			return partyComplianceForm;
		}

		protected internal async Task ExecuteScreening(Form parentForm, List<DpsSourceWithParties> sourceBizOs, bool isRescreen, bool isScreeningEntity, bool forceAllLists, ScreeningParty[] uniqueParties, IComplianceRiskAction complianceRiskAction = null)
		{
			using (new ZWaitCursorChanger())
			{
				var resultItems = await DeniedPartyScreeningWorker.SubmitRequestWithResponse(parentForm, uniqueParties);
				var screenedItemsCount = 0;

				foreach (var resultItem in resultItems)
				{
					if (resultItem.Response != null)
					{
						screenedItemsCount++;

						if (resultItem.Response.ResponseCode != DpsResponseCode.Successful)
						{
							Globals.Message.Show(resultItem.Response.ExtraMessage ?? Res.GetString("8F265A12-5B4B-44DA-ACDE-4DA0EE3CE5D3", "Error Occur During Screening, Response Code: {0}", resultItem.Response.ResponseCode));
							return;
						}

						if (resultItem.ScreeningParty?.ScreeningEntity != null && resultItem.ScreeningParty.ScreeningEntity.IsDeleted)
						{
							Globals.Message.Show(Res.GetString("D76A939E-71AE-4D19-A432-2EDC63BD6EB9", "{0} has been deleted, please check the data and screen again.", resultItem.ScreeningParty.Code));
							return;
						}
					}
				}

				var factory = new BusinessObjectFactory();
				var resultManager = ProcessResults(resultItems, sourceBizOs, false, factory, isRescreen, forceAllLists);
				var messageInfo = new DpsMessageInfo(sourceBizOs, !(parentForm is BaseOrganisationsForm), screenedItemsCount);

				if (resultManager != default && resultManager.ResultStatuses.Any())
				{
					SaveResultStatuses(resultManager.ResultStatuses.ToArray(), messageInfo, factory);

					if (resultManager.ChangedBizOs.Any() && !OrganisationsDataRegistry.Instance.DeniedPartyScreeningEnableLogWalkerServiceTaskToUpdateRelatedJobs.Value)
					{
						PromptProgressBarAndUpdateRelatedJobs(parentForm, resultManager.ChangedBizOs.ToArray());
					}
				}
				else
				{
					if (!resultManager.IsNewDpsForm)
					{
						DpsLog.SaveWithExceptionHandler(factory);
					}
				}

				if (!isRescreen && !isScreeningEntity)
				{
					ResynchronizeScreeningStatusForJobs(sourceBizOs, messageInfo);
				}

				if (complianceRiskAction == null)
				{
					messageInfo.ShowMessageIfNeeded();
				}
				else
				{
					complianceRiskAction?.SynchronizeAndSaveIfNeeded();
					complianceRiskAction?.ShowMessageIfNeeded();
				}
			}
		}

		protected virtual (List<DpsResultStatus> ResultStatuses, List<BusinessObject> ChangedBizOs, bool IsNewDpsForm) ProcessResults(List<DpsResponseWithScreeningParty> resultItems, List<DpsSourceWithParties> sourceBizOs, bool v, BusinessObjectFactory factory, bool isRescreen, bool forceAllLists)
		{
			return DpsResultsManager.ProcessResults(resultItems, sourceBizOs, false, factory, isRescreen, forceAllLists);
		}

		internal void ResynchronizeScreeningStatusForJobs(List<DpsSourceWithParties> sourceBizOs, DpsMessageInfo messageInfo)
		{
			if (sourceBizOs?.Count > 0)
			{
				foreach (var sourceBizO in sourceBizOs)
				{
					ResynchronizeScreeningStatusForJobsCore(sourceBizO, messageInfo);

					if (sourceBizO.SourceBizO is Enterprise.Integration.Forwarding.IForwardingShipment shipment)
					{
						foreach (var declaration in shipment.Declarations)
						{
							if (declaration.JE_OverrideFreightDefaults
								&& declaration is BusinessObject declarationBizO
								&& declarationBizO is IScreeningPartyProvider declarationPartyProvider)
							{
								ResynchronizeScreeningStatusForJobsCore(new DpsSourceWithParties(declarationBizO, declarationPartyProvider.ScreeningParties), messageInfo);
							}
						}
					}
				}
			}
		}

		void ResynchronizeScreeningStatusForJobsCore(DpsSourceWithParties sourceBizOWithParties, DpsMessageInfo messageInfo)
		{
			if (messageInfo.FinalStatusList.All(u => u.PK != sourceBizOWithParties.SourceBizO.PK) && NeedResynchronize(sourceBizOWithParties.SourceBizO))
			{
				DpsScreeningStatusResynchronizer.SaveWithVerbose(true, sourceBizOWithParties.SourceBizO);
				messageInfo.AddResynchronizeStatus(sourceBizOWithParties, (sourceBizOWithParties.SourceBizO as IScreeningStatusProvider)?.ScreeningStatus);
			}
		}

		static bool NeedResynchronize(BusinessObject sourceBizO)
		{
			return sourceBizO != null
				   && sourceBizO is IScreeningPartyProvider screeningStatusProvider
				   && screeningStatusProvider.ScreeningStatus != ScreeningStatusesList.Codes.JobCleared
				   && screeningStatusProvider.ScreeningStatus != ScreeningStatusUpdater.GetScreenStatusUpdateTo(screeningStatusProvider);
		}

		internal void SaveResultStatuses(DpsResultStatus[] resultStatuses, DpsMessageInfo messageInfo, BusinessObjectFactory factory)
		{
			var sortedResult = new List<DpsResultStatus>();
			var jeResult = new List<DpsResultStatus>();
			var jkResult = new List<DpsResultStatus>();
			var jsResult = new List<DpsResultStatus>();

			foreach (var result in resultStatuses)
			{
				switch (result.ScreeningEntity.TablePrefix)
				{
					case JobDeclarationSchema.Constants.Prefix:
						jeResult.Add(result);
						break;
					case JobConsolSchema.Constants.Prefix:
						jkResult.Add(result);
						break;
					case JobShipmentSchema.Constants.Prefix:
						jsResult.Add(result);
						break;
					default:
						sortedResult.Add(result);
						break;
				}
			}

			var screenedTransports = resultStatuses.Where(r => r.ScreeningEntity.TablePrefix.Equals(JobConsolTransportSchema.Constants.Prefix)).ToList();

			if (screenedTransports.Any())
			{
				foreach (var transport in screenedTransports)
				{
					var query = new ZQuery(JobConsolTransportSchema.JW_Vessel, (transport.ScreeningEntity as ITransport).JW_Vessel);
					var transportsWithSameScreenedVessel = factory.Load<Transport>(query);
					foreach (var transportWithSameScreenedVessel in transportsWithSameScreenedVessel)
					{
						sortedResult.Add(new DpsResultStatus(transportWithSameScreenedVessel, transport.Status));
					}
				}
			}

			sortedResult.AddRange(jeResult);
			sortedResult.AddRange(jkResult);
			sortedResult.AddRange(jsResult);

			foreach (var result in sortedResult)
			{
				var bizo = factory.Load(result.ScreeningEntity.TablePrefix, result.ScreeningEntity.PK); // load from the same factory then do the saving
				ProcessScreenResultStatus(bizo, result.Status, messageInfo);
			}

			try
			{
				factory.Save();
			}
			catch (Exception ex) when (ex is ZSaveConcurrencyException || ex is ZCannotSaveException || ex is ZSaveException)
			{
				if (ex is ZSaveConcurrencyException zSaveConcurrency)
				{
					zSaveConcurrency.BusinessObjects.ForEach(bizO =>
					{
						if (!(bizO is NonPersistentBusinessObject))
						{
							bizO.ReloadSafe();
						}
					});
				}
				else
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
		}

		internal void ProcessScreenResultStatus(BusinessObject bizo, string status, DpsMessageInfo messageInfo)
		{
			if (bizo != null && bizo is IScreeningPartyProvider provider)
			{
				var oldStatus = provider.ScreeningStatus;
				var originalNotLinkedVessels = GetOriginalUnLinkedVessel(messageInfo);

				if (bizo.TablePrefix == JobConsolTransportSchema.Constants.Prefix && !TrySetTransportParentType(originalNotLinkedVessels.OfType<ITransport>(), provider, bizo))
				{
					return;
				}
				else if (bizo.TablePrefix == JobDeclarationSchema.Constants.Prefix || bizo.TablePrefix == JobConsolSchema.Constants.Prefix || bizo.TablePrefix == JobShipmentSchema.Constants.Prefix)
				{
					var updateStatusTo = ScreeningStatusUpdater.UpdateJobStatusFromItsScreeningParties(provider);
					messageInfo?.AddFinalStatus(bizo.PK, updateStatusTo);
				}
				else
				{
					provider.ScreeningStatus = status;
					messageInfo?.AddFinalStatus(bizo.PK, provider.ScreeningStatus);
				}

				if (bizo.TablePrefix != HVLVConsignmentSchema.Constants.Prefix)
				{
					DpsWorkflowTrackingEvent.AddNew(bizo, oldStatus, provider.ScreeningStatus);
				}
			}
		}

		List<IScreeningPartyForVessel> GetOriginalUnLinkedVessel(DpsMessageInfo messageInfo) => messageInfo?.SourceBizOs?.SelectMany(bizoWithParties => bizoWithParties.ScreenParties?.Where(party => party.NotLinkedVessel != null).Select(party => party.NotLinkedVessel)).ToList();

		bool TrySetTransportParentType(IEnumerable<ITransport> originalNotLinkedVessels, IScreeningPartyProvider provider, BusinessObject bizo)
		{
			var originalBizo = originalNotLinkedVessels?.Cast<BusinessObject>().FirstOrDefault(o => o.PK == bizo.PK);
			var result = false;
			if (originalBizo != null)
			{
				((ITransport)provider).ParentType = ((ITransport)originalBizo).ParentType;
				result = true;
			}
			return result;
		}

		void PromptProgressBarAndUpdateRelatedJobs(Form parentForm, BusinessObject[] businessObjects)
		{
			var progressForm = new ProgressForm();
			try
			{
				var itemNumber = 0;
				var itemsCount = businessObjects.Length;

				progressForm.ShowCancelButton = false;
				progressForm.ShowModalTo(parentForm);

				ScreeningStatusUpdater.ProgressUpdaterDelegate progressUpdater = (string partyCode) =>
				{
					progressForm.SetStatusAndPercentComplete(Res.GetString("003e0dcb-c15a-4fd2-93e6-93e6b5466659", "Processing jobs related to party: {0}", partyCode),
						(int)(itemNumber / (double)itemsCount * 100));
					itemNumber++;
				};
				ScreeningStatusUpdater.UpdateRelatedJobsForChangedParties(businessObjects, progressUpdater);

				progressForm.SetStatusAndPercentComplete(Res.GetString("063a6394-828c-4fd1-a520-8dad89f5c194", "Processed all jobs related to all parties."), 100);
			}
			finally
			{
				progressForm.Close();
			}
		}

		bool ShouldDisplayScreeningNotEnabledMessage(Func<string> screeningNotEnabledMessage)
		{
			var result = false;
			var message = screeningNotEnabledMessage?.Invoke() ?? string.Empty;
			if (!string.IsNullOrEmpty(message))
			{
				Globals.Message.Show(message);
				result = true;
			}

			return result;
		}

		#endregion
	}
}

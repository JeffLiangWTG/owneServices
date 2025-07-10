using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.DeniedPartyScreening.GUI.Res;
using ResString = Enterprise.DeniedPartyScreening.GUI.ResString;

namespace Enterprise.DeniedPartyScreening
{
	static class DpsCountryWideSanctions
	{
		internal static void AddActionsMenuItem(DeniedPartyScreeningActionsProvider provider)
		{
			if (provider.IsFormActionsMenuItem && provider.GetFormIdentifier == RefCountrySchema.Constants.TableName)
			{
				var country = (provider.ParentForm.BusinessEntity as RefCountry);
				var parentMenuItemSanctions = new ZMenuItem(ResString.GetMultilingualString("03A46117-DF5C-4B0B-A028-1440F72C7900", "Country Sanctions"));

				parentMenuItemSanctions.MenuItems.AddRange(new MenuItem[]
				{
				new ZMenuItem(ResString.GetMultilingualString("E0EFB3E4-BA40-46F1-B27C-FD7042EE341C", "Screen"), async delegate
				{
					if (IsValidToProcessSanctions(country, UserDecisions.Screen))
					{
						var request = DeniedPartyScreeningWorker.GetSourceWithParties(new BusinessObject[] { provider.ParentForm.BusinessEntity as RefCountry });

						await Screen(provider, request.SourceWithParties, request.ScreeningParties);
						SetSanctionsMenuItemVisibilty(country, parentMenuItemSanctions);
					}
				}),
				new ZMenuItem(ResString.GetMultilingualString("84AEFE40-7A99-46C2-B038-AEB1F8C43351", "Mark as Sanctioned"), delegate
				{
					if (IsValidToProcessSanctions(country, UserDecisions.MarkAsSanctioned))
					{
						country.RN_IsSanctioned = true;
						AddNewLogStatusAndSaveWithExceptionHandler(country, DeniedPartyConstants.LogsScreeningStatus.UserDecisionsMarkAsSanctioned, Res.GetString("F806A8B5-AEDB-4AE3-AA19-D58F929EAC47", "User's Decisions to Mark as Sanctioned"));
						SetSanctionsMenuItemVisibilty(country, parentMenuItemSanctions);
					}
				}),
				new ZMenuItem(ResString.GetMultilingualString("BE281245-CAD5-40F9-8AB2-39856F27846A", "Remove Sanctions"), delegate
				{
					if (IsValidToProcessSanctions(country, UserDecisions.RemoveSanctions))
					{
						country.RN_IsSanctioned = false;
						AddNewLogStatusAndSaveWithExceptionHandler(country, DeniedPartyConstants.LogsScreeningStatus.UserDecisionsRemoveSanctions, Res.GetString("BB10D850-F1C6-4466-B7E6-C927E609D213", "User's Decisions to Remove Sanctions"));
						SetSanctionsMenuItemVisibilty(country, parentMenuItemSanctions);
					}
				})
				});

				ZFormMenuStrategy.AddActionsMenuItem(provider.ParentForm, parentMenuItemSanctions);

				SetSanctionsMenuItemVisibilty(country, parentMenuItemSanctions);
			}
			else if (provider.IsModuleActionsMenuItem && provider.GetModuleIdentifier == ModuleIDs.RefCountry)
			{
				provider.ParentModuleActionsMenuItem.Add(new ZMenuItem(ResString.GetMultilingualString("331CD5B7-DBCA-4771-9E65-F6BB1319C31D", "Screen"), async delegate
				{
					if (provider.ModuleHasSelectedBusinessObjectsWithShowMessage())
					{
						var request = DeniedPartyScreeningWorker.GetSourceWithParties(provider.ParentModuleFilterGrid.GetSelectedBusinessObjects());

						await Screen(provider, request.SourceWithParties, request.ScreeningParties);
					}
				}));
			}
		}

		static void SetSanctionsMenuItemVisibilty(RefCountry country, ZMenuItem parentMenu)
		{
			parentMenu.MenuItems[1].Visible = !country.RN_IsSanctioned;
			parentMenu.MenuItems[2].Visible = country.RN_IsSanctioned;
		}

		static ZBool IsValidToProcessSanctions(RefCountry country, UserDecisions action)
		{
			var result = false;

			if (country.HasChanges || !country.IsInDatabase)
			{
				Globals.Message.Show(Res.GetString("FB7C1392-2CA2-4074-94D3-01B6B1389C4F", "Please save the form before Denied Party Screening or Performing Sanctions"));
			}
			else if (action == UserDecisions.Screen)
			{
				result = DpsSecurityRights.IsGrantedCountriesManageSanctionsWithShowError();
			}
			else if (action == UserDecisions.MarkAsSanctioned)
			{
				result = DpsSecurityRights.IsGrantedCountriesManageSanctionsWithShowError() &&
					DeniedPartyScreeningCompliance.HasAcknowledgedRisk(Res.GetString("C8B51C3D-5FF1-4B58-9DA5-0E34FB806727", @"You are about to apply sanctions to {0}.

Doing so will cause all entities with this country present to be blocked by Denied Party Screening. For more information please refer to the Denied Party Screening Reference Guide.

This operation must only be done by those who understand the impact it will have on their compliance process.", country.RN_Desc));
			}
			else
			{
				result = DeniedPartyScreeningCompliance.HasAcknowledgedRisk(Res.GetString("A9A140DC-C073-43E2-A466-F9D7048C4D30", @"You are about to remove sanctions applied to {0}.

Doing so will free entities with this country present from being blocked by Denied Party Screening. For more information please refer to the Denied Party Screening Reference Guide.

This operation must only be done by those who understand the impact it will have on their compliance process.", country.RN_Desc));
			}

			return result;
		}

		static void AddNewLogStatusAndSaveWithExceptionHandler(RefCountry country, ZString logStatus, ZString reason)
		{
			DpsLog.AddNewWithDefaultValues(country.Factory, country, logStatus, reason);
			DpsLog.SaveWithExceptionHandler(country.Factory);
		}

		public static async Task Screen(DeniedPartyScreeningActionsProvider provider, List<DpsSourceWithParties> sourceWithParties, List<ScreeningParty> screeningParties)
		{
			var parentForm = provider.GetParentForm();
			if (await DeniedPartyScreeningTermsAgreement.HasBeenAcknowledged(parentForm))
			{
				using (new ZWaitCursorChanger())
				{
					var responseItems = await DeniedPartyScreeningWorker.SubmitRequestWithResponse(parentForm, screeningParties.ToArray());
					var factory = new BusinessObjectFactory();
					var resultsManager = DpsResultsManager.ProcessResults(responseItems, sourceWithParties, standAlone: false, factory, false, false);

					if (resultsManager.ResultStatuses.Any())
					{
						screeningParties.ForEach(party =>
						{
							var result = resultsManager.ResultStatuses.Where(p => p.ScreeningEntity == party.ScreeningEntity);
							if (result.Any())
							{
								var resultParty = result.Single();
								if (resultParty.Status == ScreeningStatusesList.Codes.Matched || resultParty.Status == ScreeningStatusesList.Codes.Clear)
								{
									((RefCountry)resultParty.ScreeningEntity).RN_IsSanctioned = resultParty.Status == ScreeningStatusesList.Codes.Matched;
									resultParty.ScreeningEntity.Factory.Save();
								}
							}
						});
					}
					factory.Save();
				}
			}
		}

		enum UserDecisions
		{
			Screen,
			MarkAsSanctioned,
			RemoveSanctions,
		}
	}
}

using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public class NMFSControlVisibilityHelper : IDisposable
	{
		NMFSLine currentLine;
		readonly ZGrid harvestingDetailsGrid;
		readonly ZGroupBox documentDetailsGroupBox;
		readonly ZGroupBox harvestingVesselsGroupBox;
		readonly string saveHarvestingDetailsGridGridId;

		internal NMFSLine CurrentLine
		{
			get
			{
				return currentLine;
			}
			set
			{
				if (currentLine != value)
				{
					var currentProgramType = CurrentProgramType;
					var currentSourceType = CurrentSourceType;

					currentLine = value;
					if (currentLine != null)
					{
						if (currentLine.US_ProgramType != currentProgramType)
						{
							ChangeNMFSVisibility();
						}
						else if (currentLine.US_SourceType != currentSourceType)
						{
							ChangeDetailColumnsAndVesselGridVisibility();
						}
					}
					HookNMFSLineEvents();
				}
			}
		}

		void SetHarvestingDetailsGridGridId()
		{
			harvestingDetailsGrid.GridId = CurrentProgramType + saveHarvestingDetailsGridGridId;
			harvestingDetailsGrid.CurrentColumnLayout = null;
		}

		public NMFSControlVisibilityHelper(ZGrid harvestingDetailsGrid, ZGroupBox documentDetailsGroupBox, ZGroupBox harvestingVesselsGroupBox)
		{
			this.harvestingDetailsGrid = harvestingDetailsGrid;
			this.documentDetailsGroupBox = documentDetailsGroupBox;
			this.harvestingVesselsGroupBox = harvestingVesselsGroupBox;
			saveHarvestingDetailsGridGridId = this.harvestingDetailsGrid.GridId;
			ChangeNMFSVisibility();
		}

		void HookNMFSLineEvents()
		{
			if (CurrentLine != null)
			{
				CurrentLine.US_ProgramTypeInfo.ValueChanged += US_ProgramTypeInfo_ValueChanged;
				CurrentLine.US_SourceTypeInfo.ValueChanged += US_SourceTypeInfo_ValueChanged;
			}
		}

		internal void UnHookNMFSLineEvents()
		{
			if (CurrentLine != null)
			{
				CurrentLine.US_ProgramTypeInfo.ValueChanged -= US_ProgramTypeInfo_ValueChanged;
				CurrentLine.US_SourceTypeInfo.ValueChanged -= US_SourceTypeInfo_ValueChanged;
			}
		}

		void US_SourceTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeDetailColumnsAndVesselGridVisibility();
		}

		internal void ChangeDetailColumnsAndVesselGridVisibility()
		{
			if (CurrentLine != null)
			{
				var sourceType = CurrentLine.US_SourceType;

				if (sourceType == SourceTypeCodesList.Codes.HatcheryBasedAquaculture)
				{
					harvestingDetailsGrid.AddToAvailableColumns(new[] { NMFSHarvestingDetail.Schema.US_GeographicLocation });
					harvestingDetailsGrid.RemoveFromAvailableColumns(new[] { NMFSHarvestingDetail.Schema.US_OceanAreaOfCatch, NMFSHarvestingDetail.Schema.US_OceanAreaOfCatchDesc, NMFSHarvestingDetail.Schema.US_NoSmallVessels, NMFSHarvestingDetail.Schema.US_FirstLandingCountry });
				}
				else if (sourceType == SourceTypeCodesList.Codes.SmallVesselHarvest)
				{
					harvestingDetailsGrid.AddToAvailableColumns(new[] { NMFSHarvestingDetail.Schema.US_OceanAreaOfCatch, NMFSHarvestingDetail.Schema.US_OceanAreaOfCatchDesc, NMFSHarvestingDetail.Schema.US_NoSmallVessels, NMFSHarvestingDetail.Schema.US_FirstLandingCountry });
					harvestingDetailsGrid.RemoveFromAvailableColumns(new[] { NMFSHarvestingDetail.Schema.US_GeographicLocation });
				}
				else
				{
					harvestingDetailsGrid.AddToAvailableColumns(new[] { NMFSHarvestingDetail.Schema.US_OceanAreaOfCatch, NMFSHarvestingDetail.Schema.US_OceanAreaOfCatchDesc });
					harvestingDetailsGrid.RemoveFromAvailableColumns(new[] { NMFSHarvestingDetail.Schema.US_GeographicLocation, NMFSHarvestingDetail.Schema.US_NoSmallVessels, NMFSHarvestingDetail.Schema.US_FirstLandingCountry });
				}

				harvestingVesselsGroupBox.Visible = CurrentProgramType != NMFSProgramCodeList.Codes.COA && sourceType == SourceTypeCodesList.Codes.HarvestOfCaptureFisheries;
			}
		}

		void US_ProgramTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeNMFSVisibility();
		}

		string CurrentProgramType
		{
			get
			{
				var result = string.Empty;
				if (CurrentLine != null && !CurrentLine.IsDeleted)
				{
					var lookups = CurrentLine.AddInfoLookups;
					if (lookups != null && lookups.NMFSPrograms.ContainsCode(CurrentLine.US_ProgramType))
					{
						result = CurrentLine.US_ProgramType;
					}
				}

				return result;
			}
		}

		string CurrentSourceType
		{
			get
			{
				var result = string.Empty;
				if (CurrentLine != null && !CurrentLine.IsDeleted)
				{
					var lookups = CurrentLine.AddInfoLookups;
					if (lookups != null && lookups.NMFSPrograms.ContainsCode(CurrentLine.US_SourceType))
					{
						result = CurrentLine.US_SourceType;
					}
				}

				return result;
			}
		}

		void ChangeNMFSVisibility()
		{
			var currentProgramType = CurrentProgramType;
			var hasHarvestingDetailsGridDataSource = harvestingDetailsGrid.DataSource != null;
			if (hasHarvestingDetailsGridDataSource)
			{
				harvestingDetailsGrid.SaveUserLayoutSettings();
			}
			using (harvestingDetailsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				SetHarvestingDetailsGridGridId();
				if (currentProgramType == NMFSProgramCodeList.Codes.HMS || currentProgramType == NMFSProgramCodeList.Codes.AMR)
				{
					harvestingDetailsGrid.SetAvailability(false, NMFSHarvestingDetail.Schema.US_ContainsYellowfinTuna);
					harvestingDetailsGrid.SetAvailability(false, NMFSHarvestingDetail.Schema.US_GearStartDate);
					harvestingDetailsGrid.SetAvailability(false, NMFSHarvestingDetail.Schema.US_GearDescription);
					harvestingDetailsGrid.SetAvailability(false, NMFSHarvestingDetail.Schema.US_ContactPartyType);
					harvestingDetailsGrid.SetAvailability(false, NMFSHarvestingDetail.Schema.US_OA_ContactParty);
					harvestingDetailsGrid.SetAvailability(false, NMFSHarvestingDetail.Schema.ContactPartyOrgPK);
					harvestingDetailsGrid.SetAvailability(false, NMFSHarvestingDetail.Schema.US_NoSmallVessels);
					harvestingDetailsGrid.SetAvailability(false, NMFSHarvestingDetail.Schema.US_FirstLandingCountry);
					harvestingDetailsGrid.SetAvailability(false, NMFSHarvestingDetail.Schema.US_GeographicLocation);
					harvestingDetailsGrid.SetAvailability(true, NMFSHarvestingDetail.Schema.US_VesselCountry);

					harvestingDetailsGrid.ReOrderColumns(
						[
							NMFSHarvestingDetail.Schema.US_HarvestedCountry,
							NMFSHarvestingDetail.Schema.US_OceanAreaOfCatch,
							NMFSHarvestingDetail.Schema.US_VesselCountry,
							NMFSHarvestingDetail.Schema.US_GearType,
							NMFSHarvestingDetail.Schema.US_OceanAreaOfCatchDesc,
							NMFSHarvestingDetail.Schema.US_GearTypeDesc
						]);

					documentDetailsGroupBox.Visible = true;
					harvestingVesselsGroupBox.Visible = false;
				}
				else if (currentProgramType == NMFSProgramCodeList.Codes.SIM || currentProgramType == NMFSProgramCodeList.Codes.COA)
				{
					harvestingDetailsGrid.SetAvailability(false, NMFSHarvestingDetail.Schema.US_ContainsYellowfinTuna);
					harvestingDetailsGrid.SetAvailability(true, NMFSHarvestingDetail.Schema.US_GearStartDate);
					harvestingDetailsGrid.SetAvailability(true, NMFSHarvestingDetail.Schema.US_GearDescription);
					harvestingDetailsGrid.SetAvailability(true, NMFSHarvestingDetail.Schema.US_ContactPartyType);
					harvestingDetailsGrid.SetAvailability(true, NMFSHarvestingDetail.Schema.US_OA_ContactParty);
					harvestingDetailsGrid.SetAvailability(true, NMFSHarvestingDetail.Schema.ContactPartyOrgPK);
					harvestingDetailsGrid.SetAvailability(true, NMFSHarvestingDetail.Schema.US_NoSmallVessels);
					harvestingDetailsGrid.SetAvailability(true, NMFSHarvestingDetail.Schema.US_GeographicLocation);
					harvestingDetailsGrid.SetAvailability(true, NMFSHarvestingDetail.Schema.US_FirstLandingCountry);
					harvestingDetailsGrid.SetAvailability(false, NMFSHarvestingDetail.Schema.US_VesselCountry);
					harvestingDetailsGrid.ReOrderColumns(
						[
							NMFSHarvestingDetail.Schema.US_HarvestedCountry,
							NMFSHarvestingDetail.Schema.US_OceanAreaOfCatch,
							NMFSHarvestingDetail.Schema.US_GearStartDate,
							NMFSHarvestingDetail.Schema.US_GearType,
							NMFSHarvestingDetail.Schema.US_GearDescription,
							NMFSHarvestingDetail.Schema.US_ContactPartyType,
							NMFSHarvestingDetail.Schema.ContactPartyOrgPK,
							NMFSHarvestingDetail.Schema.US_VesselCountry,
							NMFSHarvestingDetail.Schema.US_OceanAreaOfCatchDesc,
							NMFSHarvestingDetail.Schema.US_GearTypeDesc
						]);

					documentDetailsGroupBox.Visible = false;
					ChangeDetailColumnsAndVesselGridVisibility();
				}
				else
				{
					harvestingDetailsGrid.SetAvailability(true, NMFSHarvestingDetail.Schema.US_ContainsYellowfinTuna);
					harvestingDetailsGrid.SetAvailability(true, NMFSHarvestingDetail.Schema.US_VesselCountry);
					harvestingDetailsGrid.SetColumnVisible(true, NMFSHarvestingDetail.Schema.US_ContainsYellowfinTuna);
					harvestingDetailsGrid.SetAvailability(false, NMFSHarvestingDetail.Schema.US_GearStartDate);
					harvestingDetailsGrid.SetAvailability(false, NMFSHarvestingDetail.Schema.US_GearDescription);
					harvestingDetailsGrid.SetAvailability(false, NMFSHarvestingDetail.Schema.US_ContactPartyType);
					harvestingDetailsGrid.SetAvailability(false, NMFSHarvestingDetail.Schema.US_OA_ContactParty);
					harvestingDetailsGrid.SetAvailability(false, NMFSHarvestingDetail.Schema.ContactPartyOrgPK);
					harvestingDetailsGrid.SetAvailability(false, NMFSHarvestingDetail.Schema.US_NoSmallVessels);
					harvestingDetailsGrid.SetAvailability(false, NMFSHarvestingDetail.Schema.US_GeographicLocation);
					harvestingDetailsGrid.SetAvailability(false, NMFSHarvestingDetail.Schema.US_FirstLandingCountry);
					harvestingDetailsGrid.ReOrderColumns(
						[
							NMFSHarvestingDetail.Schema.US_HarvestedCountry,
							NMFSHarvestingDetail.Schema.US_OceanAreaOfCatch,
							NMFSHarvestingDetail.Schema.US_VesselCountry,
							NMFSHarvestingDetail.Schema.US_GearType,
							NMFSHarvestingDetail.Schema.US_ContainsYellowfinTuna,
							NMFSHarvestingDetail.Schema.US_OceanAreaOfCatchDesc,
							NMFSHarvestingDetail.Schema.US_GearTypeDesc
						]);

					documentDetailsGroupBox.Visible = true;
					harvestingVesselsGroupBox.Visible = false;
				}
			}
			if (hasHarvestingDetailsGridDataSource)
			{
				harvestingDetailsGrid.LoadUserLayoutSettings();
			}
			if (currentLine != null)
			{
				currentLine.HarvestingDetails.RefreshBinding();
			}
		}

		#region Dispose

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnHookNMFSLineEvents();
			}
		}

		#endregion
	}
}

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class BaseJobVoyDestinationValidation : JobVoyDestinationValidation
	{
		public BaseJobVoyDestinationValidation(AutoJobVoyDestination parent)
			: base(parent)
		{
			this.Parent = (VoyageDestination)parent;
		}
		new readonly VoyageDestination Parent;

		#region JB_RL_NKPortOfDischarge

		protected override void CheckJB_RL_NKPortOfDischarge()
		{
			base.CheckJB_RL_NKPortOfDischarge();

			MandatoryValidation.CheckEntered(Parent.JB_RL_NKPortOfDischargeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JB_RL_NKPortOfDischargeInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.JB_RL_NKPortOfDischargeInfo, Parent.Factory.Load<VoyageDestination>(new ZQuery(JobVoyDestinationSchema.JB_JV, Parent.JB_JV)));
			if (!Parent.JB_RL_NKPortOfDischarge.IsEmpty && Parent.Voyage != null)
			{
				if (Parent.IsSea)
				{
					if (Parent.PortOfDischarge != null && !Parent.PortOfDischarge.RL_HasSeaport)
					{
						Parent.JB_RL_NKPortOfDischargeInfo.AddWarning(Res.GetString("bb66ff1f-13a1-40bf-9229-cd1b0216d47b", "{0} does not have a sea port.", Parent.JB_RL_NKPortOfDischarge));
					}

					if (Parent.Voyage != null && Parent.Voyage.HasSelectedTradeLanes && !PortOfDischargeExistInTradeLanes)
					{
						Parent.JB_RL_NKPortOfDischargeInfo.AddError(Res.GetString("86d0822f-ca09-4c24-a922-022261e6cb00", "This port does not match any of the specified trade lanes."));
					}
				}
				else if (Parent.Voyage.IsAir)
				{
					if (Parent.PortOfDischarge != null && !Parent.PortOfDischarge.RL_HasAirport)
					{
						Parent.JB_RL_NKPortOfDischargeInfo.AddWarning(Res.GetString("c4eb21bf-06ea-426d-8b91-111a9618bab9", "{0} does not have an air port.", Parent.JB_RL_NKPortOfDischarge));
					}
				}
			}
		}

		bool PortOfDischargeExistInTradeLanes
		{
			get
			{
				RefUNLOCO destinationPort = new RefUNLOCO.Loader(Parent.Factory).Load(Parent.JB_RL_NKPortOfDischarge);

				if (destinationPort != null)
				{
					foreach (JobTradeLaneVoyage tradeLaneVoyage in Parent.Voyage.TradeLanes)
					{
						if (tradeLaneVoyage.TradeLane != null)
						{
							ILocation location1 = LocationHelper.GetLocationFromString(tradeLaneVoyage.TradeLane.EJ_Location1, Parent.Factory);
							ILocation location2 = LocationHelper.GetLocationFromString(tradeLaneVoyage.TradeLane.EJ_Location2, Parent.Factory);

							bool isValidPort = (location2 != null && location2.CompletelyCovers(destinationPort)) ||
								(tradeLaneVoyage.TradeLane.EJ_Direction == DirectionTypeList.Codes.BothWays &&
								location1 != null && location1.CompletelyCovers(destinationPort));

							if (isValidPort)
							{
								return true;
							}
						}
					}
				}
				return false;
			}
		}

		#endregion

		#region JB_E_ARV

		protected override void CheckJB_E_ARV()
		{
			base.CheckJB_E_ARV();

			if (ETAIsMandatory)
			{
				MandatoryValidation.CheckEntered(Parent.JB_E_ARVInfo);
			}
			else
			{
				MandatoryValidation.WarnIfNotEntered(Parent.JB_E_ARVInfo);
			}

			if (!InvalidReferencedPortPairs.IsEmpty)
			{
				Parent.JB_E_ARVInfo.AddError(
					Res.GetString("3fc9c46d-ab36-a283-4181-2470e173ad4a", "ETA is invalid for {0} Port Pairs on this schedule: {1}.", Parent.Voyage.SailingTextLowerCase, InvalidReferencedPortPairs));
			}

			if (Parent.Voyage != null && !Parent.JB_E_ARV.IsEmpty && !Parent.JB_E_ARVInfo.HasErrors())
			{
				foreach (VoyageOrigin origin in Parent.Voyage.Origins)
				{
					if (origin.JA_E_DEP.IsValid && Parent.JB_E_ARV < origin.JA_E_DEP.AddDays(-1))
					{
						if (Parent.Voyage.IsAir)
						{
							if (!Parent.JB_E_ARVInfo.HasErrors())
							{
								Parent.JB_E_ARVInfo.AddError(Res.GetString("4b742e49-15a3-4a4f-b089-edfa64aa2735", "ETA cannot be more than a day before ETD of any Sailing Origins. You may have the wrong Vessel or Voyage."));
								break;
							}
						}
					}
					else
					{
						if (origin.JA_E_DEP.IsValid && Parent.JB_E_ARV > origin.JA_E_DEP.AddMonths(6))
						{
							if (!Parent.JB_E_ARVInfo.HasErrors())
							{
								Parent.JB_E_ARVInfo.AddError(Res.GetString("931d2238-f180-4d73-bf8e-4c04987753eb", "ETA cannot be more than 6 months after an ETD for any origin for this sailing.\r\nThe vessel / voyage is already on file for other origin / destination ports.\r\nCheck the sailing schedules for date information or check your data entry.\r\nYou may have entered an incorrect vessel or voyage number."));
								break;
							}
						}
					}
				}
			}

			if (!Parent.JB_E_ARVInfo.HasErrors())
			{
				ValidateJB_AvailabilityDate();
			}

			if (!Parent.JB_S_ARVInfo.HasErrors())
			{
				ValidateJB_S_ARV();
			}
		}

		#region ETAIsMandatory

		bool ETAIsMandatory
		{
			get
			{
				bool result = false;

				if (Parent.Voyage != null && Parent.Voyage.HasCountryAllocations)
				{
					result = true;
				}
				else if (Parent.PortOfDischarge != null)
				{
					if (Parent.Voyage != null)
					{
						if ((Parent.Voyage.JV_AirSeaRoad == Core.Constants.TransportModes.Road
							|| Parent.Voyage.JV_AirSeaRoad == Core.Constants.TransportModes.Rail)
							&& Parent.PortOfDischarge.RL_Code == GlbBranch.CurrentBranch.GB_RL_NKHomePort)
						{
							result = true;
						}
						else if (Parent.Voyage.JV_AirSeaRoad == Core.Constants.TransportModes.Sea
							|| Parent.Voyage.JV_AirSeaRoad == Core.Constants.TransportModes.Air)
						{
							foreach (VoyageOrigin origin in Voyage.Origins)
							{
								if (origin.PortOfLoading != null
									&& Destination != null
									&& (ImportExportHelper.IsBranchCountry(Destination.JB_RL_NKPortOfDischarge)
									|| ImportExportHelper.IsImport(origin.JA_RL_NKPortOfLoading, Destination.JB_RL_NKPortOfDischarge)))
								{
									result = true;
									break;
								}
							}
						}
					}
					else if (Parent.PortOfDischarge.RL_RN_NKCountryCode == GlbBranch.CurrentBranch.Country.Code)
					{
						result = true;
					}
				}
				return result;
			}
		}

		#endregion

		#region InvalidReferencedPortPairs

		/// <summary>
		/// An ETA change is not safe if it will delete sailings that cannot be deleted.
		/// </summary>
		/// <returns></returns>
		ZString InvalidReferencedPortPairs
		{
			get
			{
				ZStringBuilder invalidReferencedPortPairs = new ZStringBuilder();

				if (Voyage != null)
				{
					foreach (JobSailing sailing in Destination.FetchSailings())
					{
						if (sailing.Origin != null && JobVoyageSailingsHelper.IsInvalidDateCombinationForSailing(sailing.Origin, Destination, Voyage.JV_AirSeaRoad) && sailing.IsReferenced())
						{
							invalidReferencedPortPairs.Append(sailing.PortPair);
						}
					}
				}

				return invalidReferencedPortPairs.ToStringWithNewLineBetweenAppends();
			}
		}

		#endregion

		#endregion

		#region JB_A_ARV

		protected override void CheckJB_A_ARV()
		{
			base.CheckJB_A_ARV();

			if (!Parent.JB_A_ARV.IsEmpty)
			{
				if (!Parent.JB_E_ARV.IsEmpty)
				{
					if ((Parent.JB_A_ARV > Parent.JB_E_ARV && Parent.JB_E_ARV.AddMonths(1) < Parent.JB_A_ARV)
						|| (Parent.JB_A_ARV < Parent.JB_E_ARV && Parent.JB_E_ARV.AddMonths(-1) > Parent.JB_A_ARV))
					{
						Parent.JB_A_ARVInfo.AddError(Res.GetString("312fd5cc-0906-469d-b2f1-5d2e9b2cea6f", "ATA must be within 1 month of ETA."));
					}
				}
			}

			if (!Parent.JB_RL_NKPortOfDischarge.IsEmpty && !Parent.JB_RL_NKPortOfDischargeInfo.HasErrors() && Parent.PortOfDischarge != null)
			{
				TransportValidation.CheckRelatedPortLocalTimeNotSetToFuture(Parent.JB_A_ARVInfo, Parent.PortOfDischarge);
			}

			if (!Parent.JB_A_ARVInfo.HasErrors())
			{
				ValidateJB_AvailabilityDate();
			}
		}

		#endregion

		#region JB_S_ARV

		protected override void CheckJB_S_ARV()
		{
			base.CheckJB_S_ARV();
			ValidateScheduleArrivalDate();
		}

		#endregion

		#region JB_AvailabilityDate

		protected override void CheckJB_AvailabilityDate()
		{
			base.CheckJB_AvailabilityDate();

			if (!Parent.JB_AvailabilityDate.IsEmpty)
			{
				if (!Parent.JB_A_ARV.IsEmpty)
				{
					if (Parent.JB_AvailabilityDate < Parent.JB_A_ARV)
					{
						Parent.JB_AvailabilityDateInfo.AddError(Res.GetString("766acfcf-6483-4297-982e-74f204020722", "CTO Availability date cannot be prior to the ATA."));
					}
				}
				else if (!Parent.JB_E_ARV.IsEmpty)
				{
					if (Parent.JB_AvailabilityDate < Parent.JB_E_ARV)
					{
						Parent.JB_AvailabilityDateInfo.AddError(Res.GetString("b7cbffcd-7ce1-4d8c-856e-269d417ffc04", "CTO Availability date cannot be prior to the ETA."));
					}
				}
			}
		}

		#endregion

		#region JB_StorageDate

		protected override void CheckJB_StorageDate()
		{
			base.CheckJB_StorageDate();

			if (!Parent.JB_AvailabilityDate.IsEmpty && !Parent.JB_StorageDate.IsEmpty)
			{
				if (Parent.JB_StorageDate < Parent.JB_AvailabilityDate)
				{
					Parent.JB_StorageDateInfo.AddError(Res.GetString("afdf725e-649a-481e-a79c-6810cefe1ce0", "CTO Storage Date must be after CTO Availability Date"));
				}
			}
		}

		#endregion

		#region Validation

		void ValidateScheduleArrivalDate()
		{
			if (Parent.Voyage != null && Parent.Voyage.IsAir && !Parent.JB_S_ARV.IsEmpty && !Parent.JB_E_ARV.IsEmpty)
			{
				if (Parent.JB_S_ARV > Parent.JB_E_ARV && Parent.JB_E_ARV.AddDays(1) < Parent.JB_S_ARV)
				{
					Parent.JB_S_ARVInfo.AddWarning(Res.GetString("41e0088e-63f5-493c-9efc-a1598ad21bb6", "STA is more than a day after ETA"));
				}
				else if (Parent.JB_S_ARV < Parent.JB_E_ARV && Parent.JB_E_ARV.AddDays(-1) > Parent.JB_S_ARV)
				{
					Parent.JB_S_ARVInfo.AddWarning(Res.GetString("dd0984cd-fea3-4fc6-b73d-cc0afbdaf5d3", "ETA is more than a day after STA"));
				}
			}
		}

		#endregion

		#region Implementation

		protected VoyageDestination Destination
		{
			get { return Parent; }
		}

		protected JobVoyage Voyage
		{
			get { return Destination == null ? null : Destination.Voyage; }
		}

		#endregion
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobVoyOriginValidation
//
//    This class should be used for overriding validation in AutoJobVoyOriginValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using AutoJobVoyOrigin = Enterprise.Freight.Common.Business.AutoJobVoyOrigin;

namespace Enterprise.Freight.Business
{
	public class BaseJobVoyOriginValidation : JobVoyOriginValidation
	{
		public BaseJobVoyOriginValidation(AutoJobVoyOrigin parent)
			: base(parent) { }

		#region ValidateJA_A_ARV

		protected override void CheckJA_A_ARV()
		{
			base.CheckJA_A_ARV();

			if (!Parent.JA_A_DEP.IsEmpty && !Parent.JA_A_ARV.IsEmpty && Parent.JA_A_ARV > Parent.JA_A_DEP)
			{
				Parent.JA_A_ARVInfo.AddError(Res.GetString("2b071ccd-69af-4219-a2bd-d18b01bf747c", "ATA can not be after ATD."));
			}

			if (!Origin.JA_RL_NKPortOfLoading.IsEmpty && !Origin.JA_RL_NKPortOfLoadingInfo.HasErrors() && Origin.PortOfLoading != null)
			{
				TransportValidation.CheckRelatedPortLocalTimeNotSetToFuture(Origin.JA_A_ARVInfo, Origin.PortOfLoading);
			}
		}

		#endregion

		#region ValidateJA_E_ARV

		protected override void CheckJA_E_ARV()
		{
			base.CheckJA_E_ARV();

			if (Parent.JA_E_DEP != ZDateTime.Empty && Parent.JA_E_ARV != ZDateTime.Empty && Parent.JA_E_ARV > Parent.JA_E_DEP)
			{
				Parent.JA_E_ARVInfo.AddError(Res.GetString("0897882c-4135-4a81-af10-49c6b435e3e8", "ETA can not be after ETD."));
			}
		}

		#endregion

		#region ValidateJA_E_DEP

		protected override void CheckJA_E_DEP()
		{
			base.CheckJA_E_DEP();

			if (ETDIsMandatory)
			{
				MandatoryValidation.CheckEntered(Origin.JA_E_DEPInfo);
			}
			else
			{
				MandatoryValidation.WarnIfNotEntered(Origin.JA_E_DEPInfo);
			}

			if (!InvalidReferencedPortPairs.IsEmpty)
			{
				Origin.JA_E_DEPInfo.AddError(
					Res.GetString("e5cf5796-93dd-6a9c-4ad1-7ba6965c9cb8", "ETD is invalid for {0} Port Pairs on this schedule: {1}.", Voyage.SailingTextLowerCase, InvalidReferencedPortPairs));
			}

			if (Voyage != null && !Origin.JA_E_DEP.IsEmpty && !Origin.JA_E_DEPInfo.HasErrors())
			{
				foreach (VoyageDestination destination in Voyage.Destinations)
				{
					if (destination.JB_E_ARV.IsValid)
					{
						if (Origin.JA_E_DEP > destination.JB_E_ARV.AddDays(1))
						{
							if (!AllowETAsMoreThanOneDayBeforeETD)
							{
								Origin.JA_E_DEPInfo.AddError(Res.GetString("c21c081e-db21-4a94-aba7-65577e91c041", "ETD cannot be more than a day after ETA of any Sailing Destinations. You may have the wrong Vessel or Voyage"));
								break;
							}
						}
						else if (Origin.JA_E_DEP < destination.JB_E_ARV.AddMonths(-6))
						{
							Origin.JA_E_DEPInfo.AddError(Res.GetString("a8a0c3cf-b1ca-4ed5-a299-1a1dc2395fb6", "ETD cannot be more than 6 months before an ETA for any destination for this sailing.\r\nThe vessel / voyage is already on file for other origin / destination ports.\r\nCheck the sailing schedules for date information or check your data entry.\r\nYou may have entered an incorrect vessel or voyage number."));
							break;
						}
					}
				}
			}
		}

		#region AllowETAsMoreThanOneDayBeforeETD

		protected virtual bool AllowETAsMoreThanOneDayBeforeETD
		{
			get { return true; }
		}

		#endregion

		#region InvalidReferencedPortPairs

		/// <summary>
		/// An ETD change is not safe if it will delete sailings that cannot be deleted.
		/// </summary>
		/// <returns></returns>
		protected ZString InvalidReferencedPortPairs
		{
			get
			{
				ZStringBuilder invalidReferencedPortPairs = new ZStringBuilder();

				if (Voyage != null)
				{
					foreach (JobSailing sailing in Origin.FetchSailings())
					{
						if (sailing.Destination != null && JobVoyageSailingsHelper.IsInvalidDateCombinationForSailing(Origin, sailing.Destination, Voyage.JV_AirSeaRoad) && sailing.IsReferenced())
						{
							invalidReferencedPortPairs.Append(sailing.PortPair);
						}
					}
				}

				return invalidReferencedPortPairs.ToStringWithNewLineBetweenAppends();
			}
		}

		#endregion

		#region ETDIsMandatory

		protected virtual bool ETDIsMandatory
		{
			get { return IsLocalCountry || IsLocalRegionCountry || IsETDMandatoryDueToCorrespondingPorts; }
		}

		#endregion

		#region IsHomePort

		protected bool IsHomePort
		{
			get { return Origin.PortOfLoading != null && Origin.PortOfLoading.RL_Code == GlbBranch.CurrentBranch.GB_RL_NKHomePort; }
		}

		#endregion

		#region IsLocalRegionCountry

		protected bool IsLocalRegionCountry
		{
			get
			{
				bool result = false;

				if (Voyage != null)
				{
					foreach (VoyageDestination destination in Voyage.Destinations)
					{
						result = ImportExportHelper.IsExport(Origin.JA_RL_NKPortOfLoading, destination.JB_RL_NKPortOfDischarge);
					}
				}

				return result;
			}
		}

		#endregion

		#region IsLocalCountry

		protected bool IsLocalCountry
		{
			get { return ImportExportHelper.IsBranchCountry(Origin.JA_RL_NKPortOfLoading); }
		}

		#endregion

		#region IsETDMandatoryDueToCorrespondingPorts

		public bool IsETDMandatoryDueToCorrespondingPorts
		{
			get
			{
				bool result = false;

				if (Voyage != null && Voyage.Destinations != null)
				{
					foreach (VoyageDestination destination in Voyage.Destinations)
					{
						if (destination.PortOfDischarge != null
							&& Origin != null
							&& !ImportExportHelper.IsImport(Origin.JA_RL_NKPortOfLoading, destination.JB_RL_NKPortOfDischarge))
						{
							result = true;
							break;
						}
					}
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region ValidateJA_RL_NKPortOfLoading

		protected override void CheckJA_RL_NKPortOfLoading()
		{
			base.CheckJA_RL_NKPortOfLoading();
			MandatoryValidation.CheckEntered(Origin.JA_RL_NKPortOfLoadingInfo);
			ListValidation.ErrorIfInvalidCode(Origin.JA_RL_NKPortOfLoadingInfo, Origin.Lookups.PortOfLoadings);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Origin.JA_RL_NKPortOfLoadingInfo, Parent.Factory.Load<VoyageOrigin>(new ZQuery(JobVoyOriginSchema.JA_JV, Parent.JA_JV)));

			if (!Origin.JA_RL_NKPortOfLoadingInfo.HasErrors() && Origin.Voyage != null)
			{
				foreach (JobSailing sailing in Origin.Voyage.Sailings.Cast<JobSailing>().Where(x => x.JX_JA_RL_NKPortOfLoading == Parent.JA_RL_NKPortOfLoading))
				{
					var warning = sailing.DestinationSupplyChainSecurityConfiguration.GetWarningForProhibitedRouting(sailing);
					if (!warning.IsEmpty && !Parent.JA_RL_NKPortOfLoadingInfo.HasWarning(warning))
					{
						Parent.JA_RL_NKPortOfLoadingInfo.AddWarning(warning);
					}
				}
			}
		}

		#endregion

		#region ValidateJA_A_DEP

		protected override void CheckJA_A_DEP()
		{
			base.CheckJA_A_DEP();

			if (!Origin.JA_E_DEP.IsEmpty && !Origin.JA_A_DEP.IsEmpty)
			{
				if ((Origin.JA_A_DEP > Origin.JA_E_DEP && Origin.JA_E_DEP.AddMonths(1) < Origin.JA_A_DEP)
					|| (Origin.JA_A_DEP < Origin.JA_E_DEP && Origin.JA_E_DEP.AddMonths(-1) > Origin.JA_A_DEP))
				{
					Origin.JA_A_DEPInfo.AddError(Res.GetString("82762fef-ffbd-4d74-a4a0-0b1a9302fcb2", "ATD must be within 1 month of ETD."));
				}
			}

			if (!Origin.JA_RL_NKPortOfLoading.IsEmpty && !Origin.JA_RL_NKPortOfLoadingInfo.HasErrors() && Origin.PortOfLoading != null)
			{
				TransportValidation.CheckRelatedPortLocalTimeNotSetToFuture(Origin.JA_A_DEPInfo, Origin.PortOfLoading);
			}
		}

		#endregion

		#region ValidateJA_ReceivalCommences

		protected override void CheckJA_ReceivalCommences()
		{
			base.CheckJA_ReceivalCommences();

			if (Parent.JA_ReceivalCommences > Parent.JA_CutOff)
			{
				Parent.JA_ReceivalCommencesInfo.AddError(Res.GetString("2da66680-7af9-4805-b12f-5ef44f4bda54", "CTO Receival Start date must be before the CTO Cut Off date."));
			}

			if (!Parent.JA_ReceivalCommences.IsEmpty && !Parent.JA_E_DEP.IsEmpty)
			{
				if (IsDateGreater(Parent.JA_ReceivalCommences, Parent.JA_E_DEP))
				{
					Parent.JA_ReceivalCommencesInfo.AddError(Res.GetString("891d5abd-c875-49b1-b12b-e0b44d2970d5", "CTO Receival Start date cannot be after ETD."));
				}
			}
		}

		#endregion

		#region ValidateJA_CutOff

		protected override void CheckJA_CutOff()
		{
			base.CheckJA_CutOff();

			if (Parent.JA_ReceivalCommences > Parent.JA_CutOff)
			{
				Parent.JA_CutOffInfo.AddError(Res.GetString("2da66680-7af9-4805-b12f-5ef44f4bda54", "CTO Receival Start date must be before the CTO Cut Off date."));
			}

			if (!Parent.JA_CutOff.IsEmpty && !Parent.JA_E_DEP.IsEmpty)
			{
				if (IsDateGreater(Parent.JA_CutOff, Parent.JA_E_DEP))
				{
					Parent.JA_CutOffInfo.AddError(Res.GetString("cca64a93-fe16-4454-ab79-105eec6d7a0c", "CTO Cut off date cannot be after ETD."));
				}
			}
		}

		#endregion

		#region ValidateJA_ReeferReceivalCommences

		protected override void CheckJA_ReeferReceivalCommences()
		{
			base.CheckJA_ReeferReceivalCommences();

			if (Parent.JA_ReeferReceivalCommences > Parent.JA_ReeferCutOff)
			{
				Parent.JA_ReeferReceivalCommencesInfo.AddError(Res.GetString("6db64b41-498b-4300-a130-b07f3faacc94", "Reefer Receival Start date must be before the Reefer Cut Off date."));
			}

			if (!Parent.JA_ReeferReceivalCommences.IsEmpty && !Parent.JA_E_DEP.IsEmpty)
			{
				if (IsDateGreater(Parent.JA_ReeferReceivalCommences, Parent.JA_E_DEP))
				{
					Parent.JA_ReeferReceivalCommencesInfo.AddError(Res.GetString("187b26bc-3db7-4155-9be0-abedcc880cff", "Reefer Receival Start date cannot be after ETD."));
				}
			}
		}

		#endregion

		#region ValidateJA_ReeferCutOff

		protected override void CheckJA_ReeferCutOff()
		{
			base.CheckJA_CutOff();

			if (Parent.JA_ReeferReceivalCommences > Parent.JA_ReeferCutOff)
			{
				Parent.JA_ReeferCutOffInfo.AddError(Res.GetString("6db64b41-498b-4300-a130-b07f3faacc94", "Reefer Receival Start date must be before the Reefer Cut Off date."));
			}

			if (!Parent.JA_ReeferCutOff.IsEmpty && !Parent.JA_E_DEP.IsEmpty)
			{
				if (IsDateGreater(Parent.JA_ReeferCutOff, Parent.JA_E_DEP))
				{
					Parent.JA_ReeferCutOffInfo.AddError(Res.GetString("1d4dbb70-5e19-43c8-94d0-68165f1f7ed7", "Reefer Cut off date cannot be after ETD."));
				}
			}
		}

		#endregion

		#region ValidateJA_EmptyReceivalCommences

		protected override void CheckJA_EmptyReceivalCommences()
		{
			base.CheckJA_EmptyReceivalCommences();

			if (Parent.JA_EmptyReceivalCommences > Parent.JA_EmptyCutOff)
			{
				Parent.JA_EmptyReceivalCommencesInfo.AddError(Res.GetString("134ad160-c501-4305-9924-ddf6fa312bde", "Empty Receival Start date must be before the Empty Cut Off date."));
			}

			if (!Parent.JA_EmptyReceivalCommences.IsEmpty && !Parent.JA_E_DEP.IsEmpty)
			{
				if (IsDateGreater(Parent.JA_EmptyReceivalCommences, Parent.JA_E_DEP))
				{
					Parent.JA_EmptyReceivalCommencesInfo.AddError(Res.GetString("4196c445-efc7-432c-a9ef-723ca1966995", "Empty Receival Start date cannot be after ETD."));
				}
			}
		}

		#endregion

		#region ValidateJA_EmptyCutOff

		protected override void CheckJA_EmptyCutOff()
		{
			base.CheckJA_CutOff();

			if (Parent.JA_EmptyReceivalCommences > Parent.JA_EmptyCutOff)
			{
				Parent.JA_EmptyCutOffInfo.AddError(Res.GetString("134ad160-c501-4305-9924-ddf6fa312bde", "Empty Receival Start date must be before the Empty Cut Off date."));
			}

			if (!Parent.JA_EmptyCutOff.IsEmpty && !Parent.JA_E_DEP.IsEmpty)
			{
				if (IsDateGreater(Parent.JA_EmptyCutOff, Parent.JA_E_DEP))
				{
					Parent.JA_EmptyCutOffInfo.AddError(Res.GetString("57fc40a5-c9d3-43f2-adac-c11bbf85a101", "Empty Cut off date cannot be after ETD."));
				}
			}
		}

		#endregion

		#region ValidateJA_VGMCutOff

		protected override void CheckJA_VGMCutOff()
		{
			base.CheckJA_VGMCutOff();

			if (!Parent.JA_VGMCutOff.IsEmpty && !Parent.JA_E_DEP.IsEmpty)
			{
				if (IsDateGreater(Parent.JA_VGMCutOff, Parent.JA_E_DEP))
				{
					Parent.JA_VGMCutOffInfo.AddError(Res.GetString("266d2712-2ea2-4fdb-9a4d-88b48dcfbcb1", "VGM Cut off date cannot be after ETD."));
				}
			}
		}

		#endregion

		#region Validate Hazardous Receivals

		protected override void CheckJA_DGReceivalCommences()
		{
			base.CheckJA_DGReceivalCommences();

			if (Parent.JA_DGReceivalCommences > Parent.JA_DGCutOff)
			{
				Parent.JA_DGReceivalCommencesInfo.AddError(Res.GetString("e60abd50-ba05-4670-84a8-6b0f876eb5ab", "HAZ Receival Start date must be before the HAZ Cut Off date."));
			}

			if (IsDateGreater(Parent.JA_DGReceivalCommences, Parent.JA_E_DEP))
			{
				Parent.JA_DGReceivalCommencesInfo.AddError(Res.GetString("4c6e020b-b04a-4a53-bee6-8861ebbc5b79", "HAZ Receival Start date cannot be after ETD."));
			}
		}

		protected override void CheckJA_DGCutOff()
		{
			base.CheckJA_DGCutOff();

			if (Parent.JA_DGCutOff < Parent.JA_DGReceivalCommences)
			{
				Parent.JA_DGCutOffInfo.AddError(Res.GetString("e60abd50-ba05-4670-84a8-6b0f876eb5ab", "HAZ Receival Start date must be before the HAZ Cut Off date."));
			}

			if (IsDateGreater(Parent.JA_DGCutOff, Parent.JA_E_DEP))
			{
				Parent.JA_DGCutOffInfo.AddError(Res.GetString("a9587072-9e42-4c37-9948-8659080e8d6f", "HAZ Cut off date cannot be after ETD."));
			}
		}

		#endregion

		#region Implementation

		protected VoyageOrigin Origin
		{
			get { return (VoyageOrigin)Parent; }
		}

		protected JobVoyage Voyage
		{
			get { return Origin == null ? null : Origin.Voyage; }
		}

		protected VoyageCountry VoyageCountry
		{
			get { return Origin == null ? null : Origin.VoyageCountry; }
		}

		protected virtual bool IsDateGreater(ZDateTime datetime1, ZDateTime datetime2)
		{
			return datetime1 > datetime2;
		}

		#endregion
	}
}

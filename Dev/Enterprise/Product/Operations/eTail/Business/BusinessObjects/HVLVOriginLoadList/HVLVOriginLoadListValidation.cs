//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHVLVOriginLoadListValidation
//
//    This class should be used for overriding validation in AutoHVLVOriginLoadListValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.Business
{
	public class HVLVOriginLoadListValidation : AutoHVLVOriginLoadListValidation
	{
		public HVLVOriginLoadListValidation(AutoHVLVOriginLoadList parent) : base(parent)
		{
		}

		new HVLVOriginLoadList Parent
		{
			get { return (HVLVOriginLoadList)base.Parent; }
		}

		#region Origin Depot

		protected override void CheckHVL_OA_OriginDepot()
		{
			base.CheckHVL_OA_OriginDepot();
			MandatoryValidation.CheckEntered(Parent.HVL_OA_OriginDepotInfo);
		}

		#endregion

		#region Destination Depot

		protected override void CheckHVL_OA_DestinationDepot()
		{
			base.CheckHVL_OA_DestinationDepot();
			MandatoryValidation.CheckEntered(Parent.HVL_OA_DestinationDepotInfo);
		}

		#endregion

		#region Origin Port

		protected override void CheckHVL_RL_NKOrigin()
		{
			base.CheckHVL_RL_NKOrigin();
			MandatoryValidation.CheckEntered(Parent.HVL_RL_NKOriginInfo);
		}

		#endregion

		#region Destination Port

		protected override void CheckHVL_RL_NKDestination()
		{
			base.CheckHVL_RL_NKDestination();
			MandatoryValidation.CheckEntered(Parent.HVL_RL_NKDestinationInfo);
		}

		#endregion

		#region Transport Mode

		protected override void CheckHVL_TransportMode()
		{
			base.CheckHVL_TransportMode();

			if (!NotLodged)
			{
				MandatoryValidation.CheckEntered(Parent.HVL_TransportModeInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.HVL_TransportModeInfo, Parent.Lookups.HVL_TransportMode_List);
		}

		#endregion

		#region Carrier

		protected override void CheckHVL_OH_Carrier()
		{
			base.CheckHVL_OH_Carrier();

			if (!NotLodged)
			{
				MandatoryValidation.CheckEntered(Parent.HVL_OH_CarrierInfo);
			}
		}

		#endregion

		#region Service Level

		protected override void CheckHVL_RS_NKServiceLevel()
		{
			base.CheckHVL_RS_NKServiceLevel();

			if (!NotLodged)
			{
				MandatoryValidation.CheckEntered(Parent.HVL_RS_NKServiceLevelInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.HVL_RS_NKServiceLevelInfo, Parent.Lookups.ServiceLevels);
		}

		#endregion

		#region Status

		bool NotLodged => Parent.HVL_Status == HVLVOriginLoadListStatus.Codes.Open
						  || Parent.HVL_Status == HVLVOriginLoadListStatus.Codes.Closed
						  || Parent.HVL_Status == HVLVOriginLoadListStatus.Codes.Pending;

		protected override void CheckHVL_Status()
		{
			base.CheckHVL_Status();
			MandatoryValidation.CheckEntered(Parent.HVL_StatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.HVL_StatusInfo, Parent.Lookups.HVL_Status_List);

			if (Parent.IsLodgedOrConsolidated && !HVLVOriginLoadList.IsFunctionalTesting)
			{
				Parent.HVL_StatusInfo.AddError(Res.GetString("5fd165b4-fd3c-4d01-a07e-99b65fce5ee0", "HVLV Origin Load List status cannot be updated to {0}.", Parent.HVL_Status));
			}
		}

		#endregion

		#region Master Bill Number

		protected override void CheckHVL_MasterBillNumber()
		{
			if (!Parent.HVL_IsNeutralMaster)
			{
				base.CheckHVL_Status();

				if (!NotLodged)
				{
					MandatoryValidation.CheckEntered(Parent.HVL_MasterBillNumberInfo);
				}

				if (Parent.HVL_TransportMode == TransportModes.Air)
				{
					var warning = MasterBillValidator.GetMAWBFormatValidMessage(Parent.HVL_MasterBillNumber, Parent.Factory);
					if (!warning.IsEmpty)
					{
						Parent.HVL_MasterBillNumberInfo.AddWarning(warning);
					}
				}
				else
				{
					if (Parent.HVL_MasterBillNumber.StartsWith(" ", System.StringComparison.CurrentCultureIgnoreCase))
					{
						Parent.HVL_MasterBillNumberInfo.AddMessageError(Res.GetString("F040E61B-CEE2-4A74-9517-5D0431FF83BB", "Ocean Bill should not contain leading spaces."));
					}
					else if (ReferenceNumberShouldBeSplitIntoNumbers(Parent.HVL_MasterBillNumber))
					{
						Parent.HVL_MasterBillNumberInfo.AddError(Res.GetString("FBB10F82-DB54-4FB0-971D-0E1F7732427E", "Commas, spaces, colons, and semicolons are not valid characters on a Bill of Lading."));
					}
				}

				if (IsSuitableForForwardAirMessage(Parent) && IsForwardAirCarrierOnLoadList(Parent))
				{
					var stmNums = Parent.Carrier?.OrgFountains.FirstOrDefault(c => c.SN_Type == OrgConstants.NumberFountains.Code.ForwardAirBillNumbers);

					if (stmNums != null)
					{
						const int leftNumbers = 10;

						var fountain = stmNums.TryGetNumberFountain();
						if (fountain != null)
						{
							var preliminaryNumber = fountain.PeekPreliminary(Parent.Factory);

							var availableNumbers = preliminaryNumber > stmNums.SN_MaximumValue || preliminaryNumber < stmNums.SN_MinimumValue ? 0 : stmNums.SN_MaximumValue - preliminaryNumber + 1;
							if (availableNumbers <= leftNumbers)
							{
								var message = availableNumbers > 0
									? Res.GetString("CF3E4D71-F17E-4DC9-819C-C15087BFBE51",
										"The BOL number range allocated by Forward Air is about to expire. You have {0} numbers left. Please contact Forward Air to obtain a new set of numbers, which can be added to the Forward Air Carrier organization under Details > Config > Number Ranges.",
										availableNumbers)
									: Res.GetString("0F49F1CE-50CC-4D6A-BC6D-92343A794954",
										"The BOL number range allocated by Forward Air is to expire. Please contact Forward Air to obtain a new set of numbers, which can be added to the Forward Air Carrier organization under Details > Config > Number Ranges.");

								Parent.HVL_MasterBillNumberInfo.AddWarning(message);
							}
						}
					}
				}
			}
		}

		static bool IsSuitableForForwardAirMessage(HVLVOriginLoadList loadList)
		{
			return IsUSOrCanadaExportOnLoadList(loadList)
				&& (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == CountryCodes.UnitedStates
				|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == CountryCodes.Canada);
		}

		static bool IsUSOrCanadaExportOnLoadList(HVLVOriginLoadList loadList)
		{
			return loadList != null
				&& loadList.HVL_TransportMode == TransportModes.Road
				&& IsLoadPortUSOrCanada(loadList.HVL_RL_NKOrigin);
		}

		static bool IsLoadPortUSOrCanada(ZString loadPort)
		{
			return loadPort.SubstringSafe(0, 2) == CountryCodes.UnitedStates || loadPort.SubstringSafe(0, 2) == CountryCodes.Canada;
		}

		static bool IsForwardAirCarrierOnLoadList(HVLVOriginLoadList loadList)
		{
			var result = false;

			if (loadList != null)
			{
				var carrier = loadList.Carrier;
				if (carrier != null)
				{
					result = carrier.CustomsCodes.Cast<OrgCusCode>()
						.Any(c => c.OK_CodeType == OrgCusCode.CodeTypes.EHubOrganisationID
								&& c.OK_CustomsRegNo == OrgConstants.NumberFountains.Code.ForwardAirBillNumbers);
				}
			}

			return result;
		}

		bool ReferenceNumberShouldBeSplitIntoNumbers(ZString referenceNumber)
		{
			return Parent.HVL_TransportMode == TransportModes.Sea && !string.IsNullOrEmpty(referenceNumber) && referenceNumber.IndexOfAny(BookingReferenceSeperatorCharacters.ToArray()) != -1;
		}

		static ReadOnlyCollection<char> BookingReferenceSeperatorCharacters => new ReadOnlyCollection<char>(new char[] { ' ', ';', ':', ',' });

		#endregion

		#region Voyage Flight

		protected override void CheckHVL_VoyageFlight()
		{
			base.CheckHVL_VoyageFlight();
			if (Parent.HVL_TransportMode == TransportModes.Air
				|| Parent.HVL_TransportMode == TransportModes.Sea
				|| Parent.HVL_TransportMode == TransportModes.Rail)
			{
				MandatoryValidation.CheckEntered(Parent.HVL_VoyageFlightInfo);
			}

			switch (Parent.HVL_TransportMode)
			{
				case TransportModes.Sea:
					var pattern = (NoResString)"^([Vv][.,\'\" ]*[0-9]*)$"; // Hard-coded constant
					var isInvalidNumber = Regex.IsMatch(Parent.HVL_VoyageFlight, pattern);

					if (isInvalidNumber)
					{
						Parent.HVL_VoyageFlightInfo.AddWarning(Res.GetString("BFD76FD1-8935-4C55-9922-B4E34FD510DD", "Voyage number should not start with a 'V' followed by numbers or punctuation characters. The system will add this 'V' automatically."));
					}

					if (!Parent.HVL_VoyageFlightInfo.HasErrors()
						&& !Parent.HVL_VoyageFlight.IsEmpty
						&& !Parent.HVL_VesselName.IsEmpty
						&& FindOtherLoadlistsWithSameVesselVoyageCombination().Any())
					{
						Parent.HVL_VoyageFlightInfo.AddWarning(Res.GetString("B0C53276-5905-4E85-9CBA-332CAF638E6D", "Another Sailing Schedule already exists for the given Vessel/Voyage Number combination."));
					}

					break;
				case TransportModes.Air:
					if (!FlightCodeValidator.IsValid(Parent.HVL_VoyageFlight))
					{
						Parent.HVL_VoyageFlightInfo.AddWarning(Res.GetString("5282C49A-9E63-43F8-9B8F-B9ACAF49C160", "Flight numbers have a specific set of rules which are followed by all airlines.\r\nThe first 2 characters of the Flight No. must start with: \r\n - A letter followed by a number\r\n - A number followed by a letter\r\n - Two letters\r\nAnd must then be followed by between 1 and 4 numbers.\r\nAnd an optional letter."));
					}

					break;
				case TransportModes.Rail:
					if (!Parent.HVL_VesselName.IsEmpty && FindOtherLoadlistsWithSameVesselVoyageCombination().Any())
					{
						Parent.HVL_VoyageFlightInfo.AddError(Res.GetString("9DC9887F-8DE4-49BD-9D53-97A11D03137F", "Journey number must be unique to a Journey name and cannot be repeated."));
					}

					break;
				default:
					break;
			}
		}

		#endregion

		#region Vessel

		protected override void CheckHVL_VesselName()
		{
			base.CheckHVL_VesselName();
			if (Parent.HVL_TransportMode == TransportModes.Sea)
			{
				MandatoryValidation.CheckEntered(Parent.HVL_VesselNameInfo);
				ListValidation.ErrorIfInvalidCode(Parent.HVL_VesselNameInfo);

				if (!Parent.HVL_VesselNameInfo.HasErrors())
				{
					if (FindOtherLoadlistsWithSameVesselVoyageCombination().Any(l => l.HVL_OH_Carrier == Parent.HVL_OH_Carrier))
					{
						Parent.HVL_VesselNameInfo.AddError(Res.GetString("C36E246C-D34E-4D97-92F0-A2B32F511005", "Vessel/Voyage Number/Carrier combination must be unique and cannot be repeated for non-archived sailings."));
					}
				}
			}
		}

		#endregion

		#region Container Type

		protected override void CheckHVL_RC_ContainerType()
		{
			base.CheckHVL_RC_ContainerType();

			if (!NotLodged)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.HVL_RC_ContainerTypeInfo);
			}

			ListValidation.WarnIfInvalidPK(Parent.HVL_RC_ContainerTypeInfo, Parent.Lookups.ContainerTypes);
		}

		#endregion

		#region Container Number

		protected override void CheckHVL_ContainerNumber()
		{
			base.CheckHVL_ContainerNumber();

			if (!NotLodged)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.HVL_ContainerNumberInfo);
			}
		}

		#endregion

		#region Dep

		protected override void CheckHVL_E_Dep()
		{
			base.CheckHVL_E_Dep();
			if (Parent.HVL_E_Arv < Parent.HVL_E_Dep)
			{
				Parent.HVL_E_DepInfo.AddError(ResString.GetMultilingualString("e17922c5-4414-44eb-a9a5-49b26b9a34bb", "The departure date must be before the arrival date."));
			}
		}

		#endregion

		#region Arv

		protected override void CheckHVL_E_Arv()
		{
			base.CheckHVL_E_Arv();
			if (Parent.HVL_E_Dep > Parent.HVL_E_Arv)
			{
				Parent.HVL_E_ArvInfo.AddError(ResString.GetMultilingualString("dc3547e4-75b4-49f1-b090-95b9a018ff94", "The arrival date must be after the departure date."));
			}
		}

		#endregion

		HVLVOriginLoadList[] FindOtherLoadlistsWithSameVesselVoyageCombination()
		{
			var query = new ZQuery(HVLVOriginLoadListSchema.HVL_VesselName, Parent.HVL_VesselName);
			query.AddToFilter(HVLVOriginLoadListSchema.HVL_VoyageFlight, Parent.HVL_VoyageFlight);
			query.AddToFilter(HVLVOriginLoadListSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

			return Parent.Factory.Load<HVLVOriginLoadList>(query);
		}
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusUSLVClearanceValidation
//
//    This class should be used for overriding validation in AutoCusUSLVClearanceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using BillValidator = Enterprise.Customs.US.Business.BillValidator;
using TransportTypeList = Enterprise.Customs.US.Business.TransportTypeList;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVClearanceValidation : AutoCusUSLVClearanceValidation
	{
		public CusUSLVClearanceValidation(AutoCusUSLVClearance parent) : base(parent)
		{
		}

		protected override void CheckULH_TransportMode()
		{
			base.CheckULH_TransportMode();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ULH_TransportModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ULH_TransportModeInfo, Parent.Lookups.ULH_TransportModeList);
		}

		protected override void CheckULH_ContainerMode()
		{
			base.CheckULH_ContainerMode();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ULH_ContainerModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ULH_ContainerModeInfo, Parent.Lookups.ULH_ContainerModeList);
		}

		protected override void CheckULH_PortOfDischarge()
		{
			base.CheckULH_PortOfDischarge();
			if (Parent.ULH_PortOfDischarge.IsEmpty && Parent.ULH_TransportMode == TransportTypeList.Codes.Mail)
			{
				Parent.ULH_PortOfDischargeInfo.AddMessageError(Res.GetString("390C122D-264E-452A-B292-89DD80FC2E61", "Discharge is mandatory when Mode of Transport is MAI."));
			}

			if (Parent.ULH_TransportMode != TransportTypeList.Codes.Air)
			{
				if (Parent.ULH_PortOfDischarge.IsEmpty &&
					USScheduleResolver.GetMatchesForSchedule(Schedule.D, Parent.ULH_RL_NKPortOfDischarge, Parent.ULH_TransportMode, Parent.Factory).Count > 1)
				{
					Parent.ULH_PortOfDischargeInfo.AddMessageError(string.Format(MultipleMatches, Schedule.D));
				}
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.ULH_PortOfDischargeInfo);
		}

		protected override void CheckULH_US_NKCentralizedExamSite()
		{
			base.CheckULH_US_NKCentralizedExamSite();
			ListValidation.MessageErrorIfInvalidCode(Parent.ULH_US_NKCentralizedExamSiteInfo);
		}

		protected override void CheckULH_US_NKLocationOfGoods()
		{
			base.CheckULH_US_NKLocationOfGoods();
			ListValidation.MessageErrorIfInvalidCode(Parent.ULH_US_NKLocationOfGoodsInfo);
		}

		protected override void CheckULH_DischargeDate()
		{
			base.CheckULH_DischargeDate();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ULH_DischargeDateInfo);
		}

		protected override void CheckULH_PortOfEntry()
		{
			base.CheckULH_PortOfEntry();
			var parent = (CusUSLVClearance)Parent;
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.ULH_PortOfEntryInfo);
			if (parent.ULH_RemoteLocationFiling && parent.ULH_PortOfEntry == parent.ULH_PreparerDistrictPort)
			{
				parent.ULH_PortOfEntryInfo.AddWarning(RLFPortOfEntrySameAsPreparerPort);
			}
		}

		const string RLFPortOfEntrySameAsPreparerPort = "Port of Entry and Preparer Port should normally differ on an RLF entry.";

		protected override void CheckULH_PortOfLoading()
		{
			base.CheckULH_PortOfLoading();

			if (Parent.ULH_TransportMode == TransportTypeList.Codes.Sea)
			{
				if (Parent.ULH_PortOfLoading.IsEmpty &&
					USScheduleResolver.GetMatchesForSchedule(Schedule.K, Parent.ULH_RL_NKPortOfLoading, Parent.ULH_TransportMode, Parent.Factory).Count > 1)
				{
					Parent.ULH_PortOfLoadingInfo.AddMessageError(string.Format(MultipleMatches, Schedule.K));
				}
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.ULH_PortOfLoadingInfo);
		}

		internal const string MultipleMatches = "Multiple Schedule {0} port code matches have been found for this UNLoco. Please select the appropriate code from list.";

		protected override void CheckULH_RL_NKPortOfDischarge()
		{
			base.CheckULH_RL_NKPortOfDischarge();
			ListValidation.MessageErrorIfInvalidCode(Parent.ULH_RL_NKPortOfDischargeInfo);
		}

		protected override void CheckULH_RL_NKPortOfLoading()
		{
			base.CheckULH_RL_NKPortOfLoading();
			ListValidation.MessageErrorIfInvalidCode(Parent.ULH_RL_NKPortOfLoadingInfo);
		}

		protected override void CheckULH_IORReference()
		{
			base.CheckULH_IORReference();
			var parent = (CusUSLVClearance)Parent;
			if (parent.ULH_IORReference.IsEmpty)
			{
				if (!parent.ULH_IORType.IsEmpty)
				{
					parent.ULH_IORReferenceInfo.AddMessageError(Res.GetString("60ef6cb8-7df8-4e4c-9ee7-d66842e4bad5", "Registration Number is mandatory when Registration Type has been entered"));
				}
				else if (parent.HasPGAOnAnyConsignment)
				{
					parent.ULH_IORReferenceInfo.AddMessageError(Res.GetString("4A346D93-AC8F-4C75-A109-730B6B02266C", "Mandatory when PGA data exists"));
				}
			}
		}

		protected override void CheckULH_IORType()
		{
			base.CheckULH_IORType();
			var parent = (CusUSLVClearance)Parent;
			if (parent.ULH_IORType.IsEmpty)
			{
				if (!parent.ULH_IORReference.IsEmpty)
				{
					parent.ULH_IORTypeInfo.AddMessageError(Res.GetString("02b19416-8f7e-4be7-b08c-d4998ff885f0", "Registration Type is mandatory when Registration Number has been entered"));
				}
				else if (parent.HasPGAOnAnyConsignment)
				{
					parent.ULH_IORTypeInfo.AddMessageError(Res.GetString("E005762A-8C35-429E-B303-42E895A682D0", "Mandatory when PGA data exists"));
				}
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(parent.ULH_IORTypeInfo);
			}
		}

		protected override void CheckULH_ContactName()
		{
			base.CheckULH_ContactName();
			var parent = (CusUSLVClearance)Parent;
			if (parent.ULH_ContactName.IsEmpty)
			{
				parent.ULH_ContactNameInfo.AddMessageError(Res.GetString("9C64E11F-7C70-4FC5-B47A-395EB1BACFBF", "Please enter a Contact Name."));
			}
		}

		protected override void CheckULH_ContactPhone()
		{
			base.CheckULH_ContactPhone();
			var parent = (CusUSLVClearance)Parent;
			if (parent.ULH_ContactPhone.IsEmpty)
			{
				parent.ULH_ContactPhoneInfo.AddMessageError(Res.GetString("FB5E5C6C-F464-4C28-9BE8-8CD718CFC89B", "Please enter a Contact Phone."));
			}
		}

		protected override void CheckULH_MasterBillIssuerSCAC()
		{
			base.CheckULH_MasterBillIssuerSCAC();
			if (!Parent.ULH_MasterBillIssuerSCAC.IsEmpty)
			{
				new IssuerCarrierSCACValidator(Parent.Factory).ValidateSCACCode(Parent.ULH_MasterBillIssuerSCACInfo, Parent.ULH_TransportMode, "Issuer", true);
				ValidateUnknownCarrierSCAC(Parent.ULH_MasterBillIssuerSCACInfo);
			}
			else
			{
				if (TransportTypeList.IsMasterBillSCACMandatory(Parent.ULH_TransportMode))
				{
					Parent.ULH_MasterBillIssuerSCACInfo.AddMessageError(SCACCodeForMasterBillRequiredIfSeaRailAirOrTruck);
				}
			}
		}

		protected override void CheckULH_CarrierSCAC()
		{
			base.CheckULH_CarrierSCAC();
			if (!Parent.ULH_CarrierSCAC.IsEmpty)
			{
				new IssuerCarrierSCACValidator(Parent.Factory).ValidateSCACCode(Parent.ULH_CarrierSCACInfo, Parent.ULH_TransportMode, "Carrier", true);
				ValidateUnknownCarrierSCAC(Parent.ULH_CarrierSCACInfo);
			}
			else
			{
				if (IsCarrierSCACRequired)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.ULH_CarrierSCACInfo, Parent.ULH_CarrierSCACInfo.HumanReadableName);
				}
			}
		}

		public static void ValidateUnknownCarrierSCAC(ZPropertyInfo carrierInfo)
		{
			if ((ZString)carrierInfo.Value == "UNKN")
			{
				carrierInfo.AddMessageError(US.Business.ValidationConstants.UseZZZZForUnknownCarrierSCAC);
			}
		}

		bool IsCarrierSCACRequired
		{
			get
			{
				var parent = (CusUSLVClearance)Parent;
				switch (parent.ULH_Calc_USTransportMode)
				{
					case TransportModeCodes.Codes.VesselContainer:
					case TransportModeCodes.Codes.VesselNonContainer:
					case TransportModeCodes.Codes.RailContainer:
					case TransportModeCodes.Codes.RailNonContainer:
					case TransportModeCodes.Codes.AirContainer:
					case TransportModeCodes.Codes.AirNonContainer:
						return true;
					default:
						return false;
				}
			}
		}

		internal const string SCACCodeForMasterBillRequiredIfSeaRailAirOrTruck = "Standard Carrier Alpha Code (SCAC) required for Master Bill (when Mode of Transport is Sea, Rail, Air, or Truck.)";

		protected override void CheckULH_MasterBill()
		{
			base.CheckULH_MasterBill();

			var parent = (CusUSLVClearance)Parent;
			MandatoryValidation.MessageErrorIfNotEntered(parent.ULH_MasterBillInfo);

			if (parent.IsAir)
			{
				ZString message = new CusUSLVAirWayBillValidator(parent).GetWarningMessage(Parent.ULH_MasterBill);
				if (!message.IsEmpty)
				{
					Parent.ULH_MasterBillInfo.AddWarning(message);
				}
			}
			else if (parent.IsSea)
			{
				if (parent.ULH_MasterBill.Length > BillValidator.Constants.MaximumBillLength)
				{
					parent.ULH_MasterBillInfo.AddWarning(string.Format(OceanBillTooLong, BillValidator.Constants.MaximumBillLength));
				}
				if (!parent.ULH_MasterBill.IsLettersAndNumbersOnlyOrEmpty)
				{
					parent.ULH_MasterBillInfo.AddWarning(OceanBillInvalidCharacters);
				}
			}
		}

		protected override void CheckULH_DepartureDate()
		{
			base.CheckULH_DepartureDate();
			var parent = (CusUSLVClearance)Parent;
			if (parent.ULH_DepartureDate.IsEmpty && parent.HasConsignmentWithForeignCurrency)
			{
				parent.ULH_DepartureDateInfo.AddMessageError(Res.GetString("01D3AA57-7A85-44A8-AD46-455F6A68FE13", "Departure Date must be entered where an exchange rate is required."));
			}
		}

		protected override void CheckULH_EntryDate()
		{
			base.CheckULH_EntryDate();
			var parent = (CusUSLVClearance)Parent;
			if (parent.ULH_EntryDate < (ZDateTime.Now.AddDays(-90)))
			{
				parent.ULH_EntryDateInfo.AddMessageError(DateAtEntryPortPastLimit);
			}
			else if (parent.ULH_EntryDate > (ZDateTime.Now.AddDays(60)))
			{
				parent.ULH_EntryDateInfo.AddMessageError(DateAtEntryPortFutureLimit);
			}
		}

		const string DateAtEntryPortPastLimit = "The Arrival Date is older than 90 days and no GO Number is entered. Please Verify";
		const string DateAtEntryPortFutureLimit = "If the Arrival Date is more than 60 days in the future, measured from the submission date, Cargo Certification is not permitted";

		protected override void CheckULH_EntryFilerCode()
		{
			base.CheckULH_EntryFilerCode();
			if (Parent.ULH_EntryFilerCode.IsEmpty)
			{
				Parent.ULH_EntryFilerCodeInfo.AddError(Res.GetString("53c26b2c-6af6-483d-b689-a9f041b70aa0", "An entry filer code has not been set up for this branch or company. Please set up one in Admin->System->Registry {0}", ((IRegistryItemInternals)USCustomsDataRegistry.Instance.EntryFiler).Location));
			}
		}

		protected override void CheckULH_VoyageFlightNo()
		{
			base.CheckULH_VoyageFlightNo();

			var parent = (CusUSLVClearance)Parent;
			if (parent.IsAir)
			{
				CheckVoyageFlightNoForAir(parent);
			}
			else if (parent.IsSea)
			{
				CheckVoyageFlightNoForSea(parent);
			}
		}

		protected override void CheckULH_OH_Importer()
		{
			base.CheckULH_OH_Importer();
			var parent = (CusUSLVClearance)Parent;

			if(parent.HasEntryTypeInformalFreeDutiableOnAnyConsignment)
			{
				OrganisationValidation.ValidateMatchedCustomsRegoNoForOrganisation(parent.ULH_OH_ImporterInfo, OrgMatchedCustomsRegNoType.EIN, IORCustomsRegNoRequired, true, true);
				new AuthorityToActValidator().Validate(parent, parent.Importer, parent.ULH_OH_ImporterInfo, Core.Constants.RefDocTypes.PowerOfAttorney, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, "", ExtraMatchingConditionForImportDirectionAndPortOfEntry(Parent.ULH_PortOfEntry ));
			}
		}

		void CheckVoyageFlightNoForAir(CusUSLVClearance parent)
		{
			if (!parent.VoyageFlightNumber.IsEmpty)
			{
				var validationMessage = USAirlineNumberValidator.CheckAirFlightNo(parent.VoyageFlightNumber, parent.Factory);
				if (!string.IsNullOrEmpty(validationMessage))
				{
					parent.ULH_VoyageFlightNoInfo.AddMessageError(validationMessage);
				}

				if (parent.VoyageFlightNumber.Length > 5)
				{
					parent.ULH_VoyageFlightNoInfo.AddWarning(VoyageFlightNoTruncatedAir);
				}

				if (!RefAirline.IsValidAirline2LetterCode(parent.Factory, parent.AirlineCode))
				{
					parent.ULH_VoyageFlightNoInfo.AddWarning(AirlineCodeNotRecognized);
				}
			}
		}

		void CheckVoyageFlightNoForSea(CusUSLVClearance parent)
		{
			if (parent.VoyageFlightNumber.Length > 5)
			{
				parent.ULH_VoyageFlightNoInfo.AddWarning(VoyageFlightNoTruncated);
			}
		}

		internal const string OceanBillTooLong = "Ocean Bill is too long. Messages will be sent using the first {0} characters only. If you have prefixed this Bill No. with the Issuer's SCAC code, you can remove the SCAC code component as this value is sent seperately in the entry.";
		internal const string OceanBillInvalidCharacters = "Ocean Bill contains non-alphanumeric characters. Messages will be sent without those characters.";
		internal const string VoyageFlightNoTruncated = "Only the first five characters will be sent in the messages.";
		internal const string VoyageFlightNoTruncatedAir = "Only the first five characters after the carrier code will be sent in the messages.";
		internal const string VoyageFlightNoInvalidFormat = "Please enter a valid Flight Number. The numeric component of the Flight Number must be three or four numbers, or three or four numbers followed by an alpha character. eg: QF001, UA0254B, 5X001, 001, 001A, 1750, etc.";
		internal const string AirlineCodeNotRecognized = "The airline code is not recognized.";
		internal const string IORCustomsRegNoRequired = "The Importer of Record should have EIN, SSN or CBP assigned number.";

		protected override void CheckULH_PreparerDistrictPort()
		{
			base.CheckULH_PreparerDistrictPort();
			ListValidation.MessageErrorIfInvalidCode(Parent.ULH_PreparerDistrictPortInfo);
			if (Parent.ULH_RemoteLocationFiling)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ULH_PreparerDistrictPortInfo);
			}
		}

		public static List<(Predicate<JobRequiredDocument>, string)> ExtraMatchingConditionForImportDirectionAndPortOfEntry(string portOfEntry)
		{
			return new List<(Predicate<JobRequiredDocument>, string)>()
			{
				AuthorityToActValidator.HasNoDirectionAttributeOrHasMatchedDirectionAttribute(ImportExportCodeList.Codes.Import),
				AuthorityToActValidator.HasNoPortOfEntryAttributeOrHasMatchedPortOfEntryAttribute(portOfEntry)
			};
		}
	}
}

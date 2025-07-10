using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class EXPJobDeclarationValidation : JobDeclarationValidation
	{
		public EXPJobDeclarationValidation(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void CheckJE_MessageSubType()
		{
			// JE_MessageSubType is not used for US.
		}

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();
			AESCountryCodeValidator.ValidateOrgCountryForMyanmar(Parent.JE_OH_ImporterInfo, Parent.Importer, Parent.US_DateOfExport);
		}

		protected override void CheckJE_RL_NKFinalDestination()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RL_NKFinalDestinationInfo);

			if (Parent.IsUSTerritoryTreatedAsDomesticState)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JE_RL_NKFinalDestinationInfo, Parent.Lookups.PortOfArrivals, MessageErrorPortCodeInvalidMultilingual);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JE_RL_NKFinalDestinationInfo, Parent.Lookups.FinalDestinations, MessageErrorPortCodeInvalidMultilingual);
			}
		}

		protected override void CheckJE_TotalWeight()
		{
			if (Parent.IsHandCarry)
			{
				if (Parent.JE_TotalWeight > 0)
				{
					Parent.JE_TotalWeightInfo.AddWarning(JobDeclaration.Constants.MessageErrorOrWarningGrossWeightNotAllowedWhenMOTIsPHC);
				}
			}
			else
			{
				base.CheckJE_TotalWeight();
			}
		}

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();
			ValidateJE_OH_ShippingLine();
		}

		protected override void CheckJE_OH_Forwarder()
		{
			base.CheckJE_OH_Forwarder();
			var forwarder = Parent.Forwarder;
			if (forwarder != null)
			{
				AESCountryCodeValidator.ValidateOrgCountryForMyanmar(Parent.JE_OH_ForwarderInfo, forwarder, Parent.US_DateOfExport);

				var customsRegNo = forwarder.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber });
				if (customsRegNo.IsEmpty)
				{
					customsRegNo = forwarder.MainAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, Core.Constants.CountryCodes.UnitedStates);
				}

				if (customsRegNo.IsEmpty)
				{
					Parent.JE_OH_ForwarderInfo.AddMessageError(ForwarderEIN_DUNSCodeRequired);
				}

				var forwarderContact = new DefaultContactFinder(forwarder, false).DefaultContact(ContactType.ExportFreightAgent, Parent.JE_TransportMode);
				var contactName = ZString.Empty;
				var contactPhone = forwarder.MainAddress.OA_Phone;
				if (forwarderContact != null)
				{
					contactName = MessageBlockStringDataCorrector.KeepOnlyValidCharacters(forwarderContact.OC_ContactName, ABICharacterTypeString.Constants.Alphabetic, OrgContact.Schema.OC_ContactNameMaxLength);
					if (!forwarderContact.OC_Phone.IsEmpty)
					{
						contactPhone = forwarderContact.OC_Phone;
					}
				}

				if (contactName.IsEmpty)
				{
					Parent.JE_OH_ForwarderInfo.AddMessageError(ForwarderContactNameMissing);
				}

				if (MessageBlockStringDataCorrector.KeepOnlyValidCharacters(forwarder.MainAddress.OA_City, ABICharacterTypeString.Constants.Alphabetic, OrgAddress.Schema.OA_Address1MaxLength).IsEmpty)
				{
					Parent.JE_OH_ForwarderInfo.AddMessageError(AESAddressValidator.CityRequired);
				}

				contactPhone = contactPhone.KeepNumericCharacters();
				if (contactPhone.IsEmpty)
				{
					Parent.JE_OH_ForwarderInfo.AddMessageError(ForwarderContactPhoneMissing);
				}

				AESAddressValidator.Validate(Parent.JE_OH_ForwarderInfo, forwarder.MainAddress);
			}
			else
			{
				Parent.JE_OH_ForwarderInfo.AddMessageError(ForwarderRequired);
			}
		}

		internal const string ForwarderRequired = "A Forwarder is required for AES reporting.";
		internal const string ForwarderEIN_DUNSCodeRequired = "This Forwarder does not have an EIN configured nor does it have a DUNS associated with the main address. Press F3 in the field and go to Config > Registration Numbers/Codes to enter the relevant number.";
		internal const string ForwarderContactNameMissing = "This Forwarder does not have an official export agent contact. Press F3 in the field and go to Contact > Documents To Receive, to input the required information.";
		internal const string ForwarderContactPhoneMissing = "This Forwarder does not have a phone number set up. Press F3 in the field and go to the Organization Main Details to enter their phone number.";

		protected override bool IsVoyageFlightNoRequired
		{
			get { return Parent.IsSea; }
		}

		protected override void CheckJE_ExportDate()
		{
			base.CheckJE_ExportDate();
			var maximumDaysCanBeLate = 5;

			if (Parent.US_CommodityFilingOption == AESCommodityFilingOptionList.Codes._4Postdeparture && Parent.JE_ExportDate.IsValid && ZDateTime.Today > Parent.JE_ExportDate.AddDays(maximumDaysCanBeLate))
			{
				Parent.JE_ExportDateInfo.AddWarning(string.Format(PostdepartureReportTimeMessage, maximumDaysCanBeLate));
			}
			else if (Parent.US_CommodityFilingOption == AESCommodityFilingOptionList.Codes._2Predeparture && ZDateTime.Today > Parent.JE_ExportDate)
			{
				Parent.JE_ExportDateInfo.AddWarning(PredepartureReportTimeMessage);
			}
		}
		internal const string PostdepartureReportTimeMessage = "Complete commodity data should be reported no later than {0} calendar days from the date of exportation.";
		internal const string PredepartureReportTimeMessage = "Complete commodity data should be reported prior to departure.";

		protected override void CheckJE_OH_Consignee()
		{
			base.CheckJE_OH_Consignee();
			AESCountryCodeValidator.ValidateOrgCountryForMyanmar(Parent.JE_OH_ConsigneeInfo, Parent.IntermConsignee, Parent.US_DateOfExport);
		}

		protected override void CheckJE_OA_ConsigneeAddress()
		{
			base.CheckJE_OA_ConsigneeAddress();
			AESCountryCodeValidator.ValidateOrgCountryForMyanmar(Parent.JE_OA_ConsigneeAddressInfo, Parent.ConsigneeOrgAddress, Parent.US_DateOfExport);
		}
	}
}

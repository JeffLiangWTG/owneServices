//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSOMCAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSOMCAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USOMCAddInfoValidation : AutoUSOMCAddInfoValidation
	{
		public USOMCAddInfoValidation(AutoUSOMCAddInfo parent) : base(parent)
		{
		}

		protected OMCHeader Header
		{
			get { return (OMCHeader)Parent.Parent; }
		}

		bool IsPGAValidation
		{
			get
			{
				var result = false;
				var fwsHeader = Header;
				if (fwsHeader != null)
				{
					var invoiceLine = fwsHeader.InvoiceLine;
					var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
					result = declaration != null && declaration.IsPGAValidationOn();
				}
				return result;
			}
		}

		protected override void CheckUS_DepartureDate()
		{
			base.CheckUS_DepartureDate();
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DepartureDateInfo);
			}
		}

		protected override void CheckUS_ElectronicImageSubmitted()
		{
			base.CheckUS_ElectronicImageSubmitted();
			if (!Header.US_ElectronicImageSubmitted && IsPGAValidation &&
				ConformanceDeclarationCodeList.IsDefaultElectronicImageSubmitted(Header.US_DeclarationCode))
			{
				Parent.US_ElectronicImageSubmittedInfo.AddMessageError(ConfirmSubmittedALLDocument);
			}
		}
		internal const string ConfirmSubmittedALLDocument = "Please make sure inspection document has been submitted.";

		protected override void CheckUS_NetWeight()
		{
			base.CheckUS_NetWeight();

			if (Parent.US_NetWeight.IsEmpty)
			{
				if (IsPGAValidation)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NetWeightInfo);
				}
			}
			else
			{
				if (Parent.US_NetWeight < 0)
				{
					Parent.US_NetWeightInfo.AddMessageError(EnterNumberGreaterThanZero);
				}
			}

			ValidateUS_NetWeightUQ();
		}

		internal const string EnterNumberGreaterThanZero = "Please enter a number greater than 0.";

		protected override void CheckUS_NetWeightUQ()
		{
			base.CheckUS_NetWeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_NetWeightUQInfo, Header.AddInfoLookups.UnitOfMeasureList);

			if (!Parent.US_NetWeight.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NetWeightUQInfo);
			}
		}

		protected override void CheckUS_OA_Exporter()
		{
			base.CheckUS_OA_Exporter();
			if (IsPGAValidation)
			{
				if (Parent.US_OA_Exporter.IsEmpty)
				{
					Parent.US_OA_ExporterInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Exporter"));
				}
				else
				{
					OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OA_ExporterInfo, Header.ExporterAddress);
					OrganisationValidation.ValidateCharactorsForAddressDescription(Parent.US_OA_ExporterInfo, Header.ExporterAddress);
				}
			}
		}

		protected override void CheckUS_OA_ResponsibleGovernmentOfficial()
		{
			base.CheckUS_OA_ResponsibleGovernmentOfficial();
			if (IsPGAValidation)
			{
				if (Parent.US_OA_ResponsibleGovernmentOfficial.IsEmpty && ConformanceDeclarationCodeList.IsBox8Mandatory(Header.US_DeclarationCode))
				{
					Parent.US_OA_ResponsibleGovernmentOfficialInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Responsib Gov't Official"));
				}
				else
				{
					var responsibleGovernmentOfficialAddress = Header.ResponsibleGovernmentOfficialAddress;
					OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OA_ResponsibleGovernmentOfficialInfo, responsibleGovernmentOfficialAddress);
					OrganisationValidation.ValidateCharactorsForAddressDescription(Parent.US_OA_ResponsibleGovernmentOfficialInfo, responsibleGovernmentOfficialAddress);
				}
			}
		}

		protected override void CheckUS_SourceCountry()
		{
			base.CheckUS_SourceCountry();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_SourceCountryInfo, Header.AddInfoLookups.USCountries);
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_SourceCountryInfo);
			}
		}

		protected override void CheckUS_ExporterPGAContactName()
		{
			base.CheckUS_ExporterPGAContactName();
			ValidatePGAContactName(Parent.US_ExporterPGAContactNameInfo);
		}

		void ValidatePGAContactName(ZPropertyInfo info)
		{
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(info);
			}
		}

		protected override void CheckUS_ExporterPGAContactEmail()
		{
			base.CheckUS_ExporterPGAContactEmail();
			ValidatePGAContactEmail(Parent.US_ExporterPGAContactEmailInfo, Parent.US_ExporterPGAContactPhoneNo);
		}

		void ValidatePGAContactEmail(ZPropertyInfo emailInfo, ZString phoneNo)
		{
			if (IsPGAValidation)
			{
				if (!emailInfo.Value.IsEmpty)
				{
					if (!EmailAddressValidation.IsEmailAddressValid(emailInfo.Value.ToString()))
					{
						emailInfo.AddWarning("Invalid email format");
					}
				}
				else
				{
					if (phoneNo.IsEmpty)
					{
						emailInfo.AddMessageError("Either Phone number or Email Address is required");
					}
				}
			}
		}

		protected override void CheckUS_ExporterPGAContactPhoneNo()
		{
			base.CheckUS_ExporterPGAContactPhoneNo();
			ValidatePGAContactPhoneNoAndEmail(Parent.US_ExporterPGAContactPhoneNoInfo, Parent.US_ExporterPGAContactEmail);
		}

		void ValidatePGAContactPhoneNoAndEmail(ZPropertyInfo phoneInfo, ZString email)
		{
			if (IsPGAValidation && phoneInfo.Value.IsEmpty)
			{
				if (email.IsEmpty)
				{
					phoneInfo.AddMessageError("Either Phone number or Email Address is required");
				}
			}
		}

		protected override void CheckUS_OfficialPGAContactEmail()
		{
			base.CheckUS_OfficialPGAContactEmail();
			if (ConformanceDeclarationCodeList.IsBox8Mandatory(Header.US_DeclarationCode))
			{
				ValidatePGAContactEmail(Parent.US_OfficialPGAContactEmailInfo, Parent.US_OfficialPGAContactPhoneNo);
			}
		}

		protected override void CheckUS_OfficialPGAContactPhoneNo()
		{
			base.CheckUS_OfficialPGAContactPhoneNo();
			if (ConformanceDeclarationCodeList.IsBox8Mandatory(Header.US_DeclarationCode))
			{
				ValidatePGAContactPhoneNoAndEmail(Parent.US_OfficialPGAContactPhoneNoInfo, Parent.US_OfficialPGAContactEmail);
			}
		}

		protected override void CheckUS_OfficialPGAContactName()
		{
			base.CheckUS_OfficialPGAContactName();
			if (ConformanceDeclarationCodeList.IsBox8Mandatory(Header.US_DeclarationCode))
			{
				ValidatePGAContactName(Parent.US_OfficialPGAContactNameInfo);
			}
		}

		protected override void CheckUS_ExporterCertificationDate()
		{
			base.CheckUS_ExporterCertificationDate();
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ExporterCertificationDateInfo);
			}
		}

		protected override void CheckUS_OfficialCertificationDate()
		{
			base.CheckUS_OfficialCertificationDate();
			if (IsPGAValidation && ConformanceDeclarationCodeList.IsBox8Mandatory(Header.US_DeclarationCode))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_OfficialCertificationDateInfo);
			}
		}

		protected override void CheckUS_DeclarationCode()
		{
			base.CheckUS_DeclarationCode();
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DeclarationCodeInfo);
			}
		}

		protected override void CheckUS_OA_AquacultureFacility()
		{
			base.CheckUS_OA_AquacultureFacility();

			if (IsPGAValidation)
			{
				var header = Header;
				if (header.AquacultureFacilityAddress != null)
				{
					OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OA_AquacultureFacilityInfo, Header.AquacultureFacilityAddress);
					OrganisationValidation.ValidateCharactorsForAddressDescription(header.US_OA_AquacultureFacilityInfo, header.AquacultureFacilityAddress);
				}
			}
		}
	}
}

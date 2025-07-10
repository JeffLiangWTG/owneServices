using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class ReconJobDocAddressValidation : JobDocAddressValidation
	{
		public ReconJobDocAddressValidation(AutoJobDocAddress parent, JobDeclaration declaration)
			: base(parent)
		{
			this.reconDeclaration = declaration.ReconDeclaration;
		}

		readonly ReconDeclaration reconDeclaration;
		bool IsAddressMandatory
		{
			get
			{
				switch (Parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.NotifyParty:
						return IsMandatoryForDocRecipient;
					case DocAddressTypes.Codes.ClaimantAddress:
						return IsMandatoryForClaimant;
					default:
						return false;
				}
			}
		}
		bool IsMandatoryForDocRecipient => reconDeclaration.IsACE && reconDeclaration.SummaryDocProvidedStatement;
		bool IsMandatoryForClaimant => reconDeclaration.IsACE && (reconDeclaration.NAFTA303ClaimStatement || reconDeclaration.ProtestOrPetitionFiledStatement);

		ZString AddressMandatoryMessage
		{
			get
			{
				switch (Parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.NotifyParty:
						return SummaryDocRecipientAddressMandatory;
					case DocAddressTypes.Codes.ClaimantAddress:
						return ClaimantAddressMandatory;
					default:
						return ZString.Empty;
				}
			}
		}

		bool AddressShouldNotBeEntered
		{
			get
			{
				switch (Parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.NotifyParty:
					case DocAddressTypes.Codes.ClaimantAddress:
						return reconDeclaration.IsACE && reconDeclaration.US_IssueCode != ReconIssueCodeList.Codes.FTA;
					default:
						return false;
				}
			}
		}

		ZString AddressShouldOnlyBeEnteredInFTAReconMessage
		{
			get
			{
				switch (Parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.NotifyParty:
						return SummaryDocRecipientAddressOnlyValidInFTA;
					case DocAddressTypes.Codes.ClaimantAddress:
						return ClaimantAddressOnlyValidInFTA;
					default:
						return ZString.Empty;
				}
			}
		}

		internal const string SummaryDocRecipientAddressOnlyValidInFTA = "Summary Document Recipient can only be sent for Free Trade Agreement Recons";
		internal const string ClaimantAddressOnlyValidInFTA = "Claimant can only be sent for Free Trade Agreement Recons";

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			if (IsAddressMandatory)
			{
				if (Parent.OrganisationPK.IsEmpty && !Parent.E2_AddressOverride)
				{
					CheckIfAddressDetailHasBeenEntered(Parent.OrganisationPKInfo);
				}
				else if (!Parent.OrganisationPK.IsEmpty)
				{
					OrganisationValidation.ValidateMatchedCustomsRegoNoForOrganisation(Parent.OrganisationPKInfo, OrgMatchedCustomsRegNoType.EIN, string.Format(CultureInfo.InvariantCulture, OrganisationValidation.EIN_SSN_CBNCodeRequired, "Document Recipient"), false, false);
				}
			}
			if (!Parent.E2_AddressOverride)
			{
				CheckMandatoryValidation(Parent.OrganisationPKInfo);
			}
		}

		public const string AtLeastOneProtestFiledShouldBeTicked = "Claimant is required only if there is at least one original entry for which protest has been filed.";

		protected override void CheckE2_Address1()
		{
			base.CheckE2_Address1();
			if (IsAddressMandatory && (Parent.E2_AddressOverride || Parent.OrganisationPK.IsValid))
			{
				CheckIfAddressDetailHasBeenEntered(Parent.E2_Address1Info);
			}
			if (Parent.E2_AddressOverride)
			{
				CheckMandatoryValidation(Parent.E2_Address1Info);
				if (!Parent.E2_Address1.IsEmpty)
				{
					ABICharactersValidator.ValidateCharacters(Parent.E2_Address1Info, Parent.E2_Address1, true, "Address 1");
				}
			}
		}

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();
			if (!Parent.E2_AddressOverride && Parent.Address != null)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(Parent.E2_OA_AddressInfo, Parent.Address);
			}
		}

		void CheckMandatoryValidation(ZPropertyInfo addressInfo)
		{
			if (AddressShouldNotBeEntered && !addressInfo.Value.IsEmpty)
			{
				addressInfo.AddMessageError(AddressShouldOnlyBeEnteredInFTAReconMessage);
			}
			if (Parent.E2_AddressType == DocAddressTypes.Codes.ClaimantAddress && !addressInfo.Value.IsEmpty && !reconDeclaration.ProtestOrPetitionFiledStatement)
			{
				addressInfo.AddMessageError(AtLeastOneProtestFiledShouldBeTicked);
			}
		}

		protected override void CheckE2_GovRegNumType()
		{
			base.CheckE2_GovRegNumType();
			if (IsAddressMandatory && Parent.E2_AddressOverride)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.E2_GovRegNumTypeInfo);
				MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_GovRegNumTypeInfo);
			}
		}

		protected override void CheckE2_GovRegNum()
		{
			base.CheckE2_GovRegNum();
			if (IsAddressMandatory && Parent.E2_AddressOverride)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_GovRegNumInfo);
			}
		}

		protected override void CheckE2_CompanyName()
		{
			base.CheckE2_CompanyName();
			if (IsAddressMandatory && Parent.E2_AddressOverride)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_CompanyNameInfo);
			}
		}

		protected override void CheckE2_City()
		{
			base.CheckE2_City();
			if (IsAddressMandatory && Parent.E2_AddressOverride)
			{
				CheckIfAddressDetailHasBeenEntered(Parent.E2_CityInfo);
			}
		}

		protected override void CheckE2_Postcode()
		{
			base.CheckE2_Postcode();
			if (IsAddressMandatory && Parent.E2_AddressOverride)
			{
				CheckIfAddressDetailHasBeenEntered(Parent.E2_PostcodeInfo);
			}
		}

		protected override void CheckE2_RN_NKCountryCode()
		{
			base.CheckE2_RN_NKCountryCode();
			if (IsAddressMandatory && Parent.E2_AddressOverride)
			{
				CheckIfAddressDetailHasBeenEntered(Parent.E2_RN_NKCountryCodeInfo);
			}
		}

		void CheckIfAddressDetailHasBeenEntered(ZPropertyInfo addressDetailInfo)
		{
			if (addressDetailInfo.Value.IsEmpty)
			{
				addressDetailInfo.AddMessageError(AddressMandatoryMessage);
			}
		}

		internal const string SummaryDocRecipientAddressMandatory = "Please enter address details for Summary Document Recipient";
		internal const string ClaimantAddressMandatory = "Please enter address details for Claimant";
	}
}

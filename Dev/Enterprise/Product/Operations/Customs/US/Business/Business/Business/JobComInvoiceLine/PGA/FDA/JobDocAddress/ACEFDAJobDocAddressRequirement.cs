using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.Business
{
	public class ACEFDAJobDocAddressRequirement : JobDocAddressRequirement
	{
		public ACEFDAJobDocAddressRequirement(ACEFDAJobDocAddress docAddress)
			: base()
		{
			this.docAddress = docAddress;

			this.docAddress.E2_AddressTypeInfo.AdditionalValidation += ValidateE2_AddressType;
			this.docAddress.E2_CompanyNameInfo.AdditionalValidation += ValidateE2_CompanyName;
			this.docAddress.E2_Address1Info.AdditionalValidation += ValidateE2_Address1;
			this.docAddress.E2_Address2Info.AdditionalValidation += ValidateE2_Address2;
			this.docAddress.E2_RN_NKCountryCodeInfo.AdditionalValidation += ValidateE2_RN_NKCoutryCode;
			this.docAddress.E2_StateInfo.AdditionalValidation += ValidateE2_State;
			this.docAddress.E2_CityInfo.AdditionalValidation += ValidateE2_City;
			this.docAddress.E2_PostcodeInfo.AdditionalValidation += ValidateE2_PostCode;
			this.docAddress.OrganisationPKInfo.AdditionalValidation += ValidateOrganization;

			this.GetRegistrationNumberResult = GetRegistrationNumber;
		}

		readonly ACEFDAJobDocAddress docAddress;

		RegistrationNumberResult GetRegistrationNumber(JobDocAddress docAddress)
		{
			return new RegistrationNumberResult(docAddress.Factory, true, delegate
			{
				var result = new RegistrationNumber { NumberType = ZString.Empty, Number = ZString.Empty };
				var programCode = (docAddress.Parent is ACEFDA aceFDA) ? aceFDA.US_ProgramCode : ZString.Empty;

				var cusCode = docAddress.DocAddressType == DocAddressType.FSVPImporter ?
					OrgCusCodeForFDA.FindCustomsNumberAndIDForFSVP(docAddress.Address) :
					OrgCusCodeForFDA.FindCustomsNumberAndID(docAddress.Address, programCode);

				var numberType = OrgCusCodeForFDA.GetOrgCusCodeTypeViaEntityIdentificationCode(cusCode.ID);
				if (!numberType.IsEmpty)
				{
					result.NumberType = numberType;
					result.Number = cusCode.Number;
				}

				return result;
			});
		}

		#region Validation

		void ValidateE2_AddressType()
		{
			MandatoryValidation.CheckEntered(docAddress.E2_AddressTypeInfo, "Address Type");
			ListValidation.MessageErrorIfInvalidCode(docAddress.E2_AddressTypeInfo, docAddress.Lookups.AddressTypeList);
			E2_AddressTypeCheckForDuplicates();
			E2_AddressTypeCheckForMultipleProducerTypes();
		}

		void E2_AddressTypeCheckForDuplicates()
		{
			var docAddresses = docAddress.Parent?.DocAddresses as ACEFDAJobDocAddressDependentCollection;
			if (!docAddress.E2_AddressType.IsEmpty && docAddresses != null && docAddresses.Find(docAddress.E2_AddressType).Count() > 1)
			{
				docAddress.E2_AddressTypeInfo.AddError(string.Format(DuplicateAddressTypesErrorMessage, docAddress.AddressDescription));
			}
		}
		internal const string DuplicateAddressTypesErrorMessage = "Duplicate address type specified. There can be only one {0}.";

		void E2_AddressTypeCheckForMultipleProducerTypes()
		{
			var producerTypes = ProducerTypes;

			if (producerTypes.Contains<string>(docAddress.E2_AddressType))
			{
				var docAddresses = docAddress.Parent?.DocAddresses as ACEFDAJobDocAddressDependentCollection;
				if (!docAddress.E2_AddressType.IsEmpty && docAddresses != null && docAddresses.Find(producerTypes).Count() > 1)
				{
					docAddress.E2_AddressTypeInfo.AddError(MultipleProducerTypesErrorMessage);
				}
			}
		}
		internal static string[] ProducerTypes => new[] { DocAddressTypes.Codes.Grower, DocAddressTypes.Codes.Consolidator, DocAddressTypes.Codes.Manufacturer };
		internal const string MultipleProducerTypesErrorMessage = "Multiple producer types specified. There can be only one Grower, Consolidator or Manufacturer.";

		void ValidateE2_CompanyName()
		{
			MandatoryValidation.CheckEntered(docAddress.E2_CompanyNameInfo);
			ABICharactersValidator.ValidateCharacters(docAddress.E2_CompanyNameInfo);
		}

		void ValidateE2_Address1()
		{
			MandatoryValidation.CheckEntered(docAddress.E2_Address1Info);
			ABICharactersValidator.ValidateCharacters(docAddress.E2_Address1Info);
		}

		void ValidateE2_Address2()
		{
			ABICharactersValidator.ValidateCharacters(docAddress.E2_Address2Info);
		}

		void ValidateE2_RN_NKCoutryCode()
		{
			MandatoryValidation.CheckEntered(docAddress.E2_RN_NKCountryCodeInfo);
		}

		void ValidateE2_State()
		{
			MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyHasValue(docAddress.E2_StateInfo, docAddress.E2_RN_NKCountryCodeInfo, (ZString)Core.Constants.CountryCodes.UnitedStates);
			OrganisationValidation.ValidateStateForPGAAddress(docAddress.Factory, docAddress.E2_StateInfo, docAddress.E2_RN_NKCountryCode, docAddress.E2_State, USACEFDAAddInfoValidation.ShouldCheckStateForAddress);
		}

		void ValidateE2_City()
		{
			MandatoryValidation.CheckEntered(docAddress.E2_CityInfo);
			ABICharactersValidator.ValidateCharacters(docAddress.E2_CityInfo);
		}

		void ValidateE2_PostCode()
		{
			ABICharactersValidator.ValidateCharacters(docAddress.E2_PostcodeInfo);
			ZipCodeValidation.ValidateForEmptyZIPForUSAddress(docAddress.E2_PostcodeInfo, docAddress);
		}

		void ValidateOrganization()
		{
			if (!docAddress.E2_AddressOverride)
			{
				MessageErrorIfNotEnglish(docAddress.OrganisationPKInfo, docAddress.E2_CompanyNameInfo);
				MessageErrorIfNotEnglish(docAddress.OrganisationPKInfo, docAddress.E2_Address1Info);
				MessageErrorIfNotEnglish(docAddress.OrganisationPKInfo, docAddress.E2_Address2Info);
				MessageErrorIfNotEnglish(docAddress.OrganisationPKInfo, docAddress.E2_CityInfo);
				MessageErrorIfNotEnglish(docAddress.OrganisationPKInfo, docAddress.E2_PostcodeInfo);
			}
		}

		void MessageErrorIfNotEnglish(ZPropertyInfo propertyToAddErrorMessageOn, ZPropertyInfo propertyToCheck)
		{
			var stringValue = (ZString)propertyToCheck.Value;
			if (!stringValue.IsEnglishOnlyOrEmpty && !stringValue.RemoveDiacritics().IsEnglishOnlyOrEmpty)
			{
				propertyToAddErrorMessageOn.AddMessageError(EnglishAddressCharactersValidation.GetNotificationMessage(propertyToCheck));
			}
		}

		#endregion
	}
}

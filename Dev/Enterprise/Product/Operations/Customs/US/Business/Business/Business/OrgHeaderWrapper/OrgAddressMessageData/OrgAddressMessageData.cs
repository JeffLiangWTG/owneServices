using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class OrgAddressMessageData : AutoOrgAddressMessageData
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public OrgAddressMessageData(OrgHeaderWrapper wrapper)
			: base(wrapper.Factory)
		{
			this.wrapper = wrapper;
			SetDefaultValues(wrapper);
			allowViewOrgPersonalInformation = Env.Security.OrgDetailsViewPersonalInformation.IsAllowed;
		}
		internal readonly OrgHeaderWrapper wrapper;
		internal const string ForeignBasedImporterStateCode = "FN";
		internal const string ForeignBasedImporterStateDescription = "Foreign";
		readonly bool allowViewOrgPersonalInformation;

		#region Properties

		public bool HasPermissionToSendImporterBondNumber
		{
			get
			{
				return allowViewOrgPersonalInformation ||
					(!SocialSecurityNumberValidator.IsValidSSN(US_ImporterNumber) &&
					!RelatedBusinessItems.OfType<RelatedBusinessData>().Any(x => SocialSecurityNumberValidator.IsValidSSN(x.US_Number)) &&
					!PIIs.OfType<PersonIdentityInformationData>().Any(x => SocialSecurityNumberValidator.IsValidSSN(x.US_SSN)));
			}
		}

		[List(nameof(Lookups) + "." + nameof(OrgAddressMessageDataLookups.Addresses))]
		[RelatedBusinessObject("FirstAddress")]
		public override ZGuid US_OA_Address1
		{
			get { return base.US_OA_Address1; }
			set
			{
				base.US_OA_Address1 = value;

				IAddressDetails firstAddress = FirstAddress;

				US_LineOneAddress1 = firstAddress != null ? firstAddress.AddressLine1.Left(US_LineOneAddress1Info.MaxLength) : ZString.Empty;
				US_RN_NKCountry1 = firstAddress != null ? ((ZString)Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(firstAddress.Country)).Left(US_RN_NKCountry1Info.MaxLength) : ZString.Empty;
				US_LineTwoAddress1 = firstAddress != null && IsUSImporter(US_RN_NKCountry1) ? firstAddress.AddressLine2.Left(US_LineTwoAddress1Info.MaxLength) : ZString.Empty;
				US_CityAddress1 = firstAddress != null ? firstAddress.City.Left(US_CityAddress1Info.MaxLength) : ZString.Empty;
				US_ZipAddress1 = firstAddress != null && IsZipMandatory(US_RN_NKCountry1) ? firstAddress.PostCode.KeepAlphanumericCharacters().Left(US_ZipAddress1Info.MaxLength) : ZString.Empty;
				US_StateAddress1 = GetState(firstAddress, US_RN_NKCountry1);
			}
		}

		[List(nameof(Lookups) + "." + nameof(OrgAddressMessageDataLookups.StateAddress1List))]
		public override ZString US_StateAddress1
		{
			get { return base.US_StateAddress1; }
			set { base.US_StateAddress1 = value; }
		}

		[List(nameof(Lookups) + "." + nameof(OrgAddressMessageDataLookups.StateAddress2List))]
		public override ZString US_StateAddress2
		{
			get { return base.US_StateAddress2; }
			set { base.US_StateAddress2 = value; }
		}

		/// <summary>
		/// 5106 and (Importer/Consignee Create/Update) requirements
		/// =================
		/// For U.S. addresses, the first five positions of the ZIP code are required and must be numeric. 
		/// Canadian postal codes must be in the following format: ANAbNAN (where A = alphabetic; N = numeric; b – blank space).
		/// For Mexico (state code FN, ISO code MX), postal codes must be 5 characters and must be numeric.
		/// The postal codes for U.S., Canada, and Mexico will be edited for validity.  An error message is system generated if this edit is not passed.
		/// Combinations of up to 9-position alphanumeric codes in the ZIP code field are allowed for other foreign addresses (state code FN).
		/// 
		/// *****For a Mexican address, space fill****.  
		/// For all other foreign addresses, use an up-to-9-position character code.
		/// </summary>
		/// <param name="country"></param>
		/// <returns></returns>
		internal bool IsZipMandatory(ZString country)
		{
			if (OrgCountryZipCodeInfo.CountryZipCodeInfoMap.ContainsKey(country))
			{
				if (OrgCountryZipCodeInfo.CountryZipCodeInfoMap[country].isRequred)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		string GetState(IAddressDetails addressDetails, string country)
		{
			string result = "";

			if (addressDetails != null)
			{
				if (addressDetails.Country == Core.Constants.CountryCodes.PuertoRico)
				{
					result = Core.Constants.CountryCodes.PuertoRico;
				}
				else if (IsUSImporter(country) || IsCAImporter(country) || (IsMXImporter(country)))
				{
					result = GetStateCodeFromCodeOrDesc(addressDetails.State, country);
				}
				else
				{
					result = string.IsNullOrEmpty(country) ? "" : ForeignBasedImporterStateCode;
				}
			}

			return result;
		}

		ZString GetStateCodeFromCodeOrDesc(ZString codeOrDesc, ZString countryCode)
		{
			var result = ZString.Empty;
			if (IsMXImporter(countryCode) || IsCAImporter(countryCode) || IsUSImporter(countryCode))
			{
				var stateQuery = new ZQuery(RefCountryStatesSchema.RW_Code, codeOrDesc);
				stateQuery.AddToFilter(JoinCondition.Or, RefCountryStatesSchema.RW_Description, codeOrDesc);
				stateQuery.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, countryCode);
				var countryState = Factory.LoadTop1<RefCountryStates>(stateQuery);
				if (countryState != null)
				{
					result = countryState.RW_Code;
				}

				if (IsMXImporter(countryCode))
				{
					result = Lookups.MappedMXStateCode.GetCodeFromDescription(result);
				}

				if (result.IsEmpty)
				{
					result = Lookups.GetStateAddressList(countryCode).GetCodeFromDescription(codeOrDesc);
				}
			}
			else
			{
				result = (codeOrDesc == ForeignBasedImporterStateCode || codeOrDesc == ForeignBasedImporterStateDescription) ? (ZString)ForeignBasedImporterStateCode : ZString.Empty;
			}
			return result.Left(2);
		}

		[List(nameof(Lookups) + "." + nameof(OrgAddressMessageDataLookups.Addresses))]
		[RelatedBusinessObject("SecondAddress")]
		public override ZGuid US_OA_Address2
		{
			get { return base.US_OA_Address2; }
			set
			{
				base.US_OA_Address2 = value;

				IAddressDetails secondAddress = SecondAddress;

				US_LineOneAddress2 = secondAddress != null ? secondAddress.AddressLine1.Left(US_LineOneAddress2Info.MaxLength) : ZString.Empty;
				US_RN_NKCountry2 = secondAddress != null ? ((ZString)Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(secondAddress.Country)).Left(US_RN_NKCountry2Info.MaxLength) : ZString.Empty;
				US_LineTwoAddress2 = secondAddress != null && IsUSImporter(US_RN_NKCountry2) ? secondAddress.AddressLine2.Left(US_LineTwoAddress2Info.MaxLength) : ZString.Empty;
				US_CityAddress2 = secondAddress != null ? secondAddress.City.Left(US_CityAddress2Info.MaxLength) : ZString.Empty;
				US_ZipAddress2 = secondAddress != null && IsZipMandatory(US_RN_NKCountry2) ? secondAddress.PostCode.KeepAlphanumericCharacters().Left(US_ZipAddress2Info.MaxLength) : ZString.Empty;
				US_StateAddress2 = GetState(secondAddress, US_RN_NKCountry2);

				if (!IsAddressType2Required)
				{
					US_AddressType2 = ZString.Empty;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(OrgAddressMessageDataLookups.ActionCodeList))]
		public override ZString US_ActionCode
		{
			get { return base.US_ActionCode; }
			set
			{
				base.US_ActionCode = value;

				if (US_ActionCode == ImporterADDActionCodeList.Codes.RequestCBPNumber)
				{
					if (US_HaveSSNIndicator)
					{
						if (US_ImporterNumber.IsEmpty)
						{
							US_ImporterNumber = wrapper.organisation.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.USACodeTypes.SocialSecurityNumber).Left(US_ImporterNumberInfo.MaxLength);
						}
					}
					else
					{
						US_ImporterNumber = ZString.Empty;
					}
				}
				else
				{
					US_ImporterNumber = wrapper.organisation.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber).Left(US_ImporterNumberInfo.MaxLength);

					if (US_ImporterNumber.IsEmpty && US_ActionCode == ImporterADDActionCodeList.Codes.ChangeImporter)
					{
						US_ImporterNumber = wrapper.organisation.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.USACodeTypes.CBPAssignedNumber).Left(US_ImporterNumberInfo.MaxLength);
					}

					US_HaveSSNIndicator = false;
					US_NoSSNIndicator = false;
					US_NotAppliedIndicator = false;
					US_NoIRSIndicator = false;
					US_NotResidentIndicator = false;
				}
			}
		}

		public override ZBool US_HaveSSNIndicator
		{
			get { return base.US_HaveSSNIndicator; }
			set
			{
				var oldValue = US_HaveSSNIndicator;
				base.US_HaveSSNIndicator = value;
				if (oldValue != value && !IsCopying)
				{
					if (US_ActionCode == ImporterADDActionCodeList.Codes.RequestCBPNumber)
					{
						if (US_HaveSSNIndicator)
						{
							if (US_ImporterNumber.IsEmpty)
							{
								US_ImporterNumber = wrapper.organisation.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.USACodeTypes.SocialSecurityNumber).Left(US_ImporterNumberInfo.MaxLength);
							}
						}
						else
						{
							US_ImporterNumber = ZString.Empty;
						}
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(OrgAddressMessageDataLookups.ImporterTypesList))]
		public override ZString US_ImporterType
		{
			get { return base.US_ImporterType; }
			set { base.US_ImporterType = value; }
		}

		[List(nameof(Lookups) + "." + nameof(OrgAddressMessageDataLookups.NameQualifierList))]
		public override ZString US_NameQualifier
		{
			get { return base.US_NameQualifier; }
			set { base.US_NameQualifier = value; }
		}

		[List(nameof(Lookups) + "." + nameof(OrgAddressMessageDataLookups.CountryList))]
		public override ZString US_RN_NKCountry1
		{
			get { return base.US_RN_NKCountry1; }
			set { base.US_RN_NKCountry1 = value; }
		}

		[List(nameof(Lookups) + "." + nameof(OrgAddressMessageDataLookups.CountryList))]
		public override ZString US_RN_NKCountry2
		{
			get { return base.US_RN_NKCountry2; }
			set { base.US_RN_NKCountry2 = value; }
		}

		[MaxLength(Schema.US_ImporterNameMaxLength)]
		public override ZString US_ImporterName
		{
			get { return base.US_ImporterName; }
			set { base.US_ImporterName = value; }
		}

		public override ZBool US_UtlOtherIndicator
		{
			get { return base.US_UtlOtherIndicator; }
			set
			{
				var oldValue = US_UtlOtherIndicator;
				base.US_UtlOtherIndicator = value;
				if (oldValue != value && !IsCopying)
				{
					if (!IsUtlizationOtherDescriptionRequired)
					{
						US_UtlOtherDescription = ZString.Empty;
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(OrgAddressMessageDataLookups.MailingAddressTypeList))]
		public override ZString US_AddressType1
		{
			get { return base.US_AddressType1; }
			set
			{
				var oldValue = US_AddressType1;
				base.US_AddressType1 = value;
				if (oldValue != value && !IsCopying)
				{
					if (!IsAddressExplanation1Required)
					{
						US_AddressExplanation1 = ZString.Empty;
					}
				}
			}
		}

		public override ZString US_AddressExplanation1
		{
			get
			{
				var result = base.US_AddressExplanation1;
				if (!IsAddressExplanation1Required && result.IsEmpty)
				{
					result = Lookups.MailingAddressTypeList.GetDescriptionFromCode(US_AddressType1);
				}

				return result;
			}
			set { base.US_AddressExplanation1 = value; }
		}

		[List(nameof(Lookups) + "." + nameof(OrgAddressMessageDataLookups.PhysicalAddressTypeList))]
		public override ZString US_AddressType2
		{
			get { return base.US_AddressType2; }
			set
			{
				var oldValue = US_AddressType2;
				base.US_AddressType2 = value;
				if (oldValue != value && !IsCopying)
				{
					if (!IsAddressExplanation2Required)
					{
						US_AddressExplanation2 = ZString.Empty;
					}
				}
			}
		}

		public override ZString US_AddressExplanation2
		{
			get
			{
				var result = base.US_AddressExplanation2;
				if (!IsAddressExplanation2Required && result.IsEmpty)
				{
					result = Lookups.PhysicalAddressTypeList.GetDescriptionFromCode(US_AddressType2);
				}

				return result;
			}
			set { base.US_AddressExplanation2 = value; }
		}

		public bool US_AddressType2_ReadOnly
		{
			get { return !IsAddressType2Required; }
		}

		internal bool IsAddressType2Required
		{
			get { return US_OA_Address2.IsValid; }
		}

		[List(nameof(Lookups) + "." + nameof(OrgAddressMessageDataLookups.NumberOfEntriesList))]
		public override ZString US_NumberOfEntries
		{
			get { return base.US_NumberOfEntries; }
			set { base.US_NumberOfEntries = value; }
		}

		[List(nameof(Lookups) + "." + nameof(OrgAddressMessageDataLookups.ProgramCodesList))]
		public override ZString US_ProgramCode1
		{
			get { return base.US_ProgramCode1; }
			set { base.US_ProgramCode1 = value; }
		}

		[List(nameof(Lookups) + "." + nameof(OrgAddressMessageDataLookups.ProgramCodesList))]
		public override ZString US_ProgramCode2
		{
			get { return base.US_ProgramCode2; }
			set { base.US_ProgramCode2 = value; }
		}

		[List(nameof(Lookups) + "." + nameof(OrgAddressMessageDataLookups.ProgramCodesList))]
		public override ZString US_ProgramCode3
		{
			get { return base.US_ProgramCode3; }
			set { base.US_ProgramCode3 = value; }
		}

		[List(nameof(Lookups) + "." + nameof(OrgAddressMessageDataLookups.ProgramCodesList))]
		public override ZString US_ProgramCode4
		{
			get { return base.US_ProgramCode4; }
			set { base.US_ProgramCode4 = value; }
		}

		[List(nameof(Lookups) + "." + nameof(OrgAddressMessageDataLookups.CountryList))]
		public override ZString US_BankCountry
		{
			get { return base.US_BankCountry; }
			set { base.US_BankCountry = value; }
		}

		[List(nameof(Lookups) + "." + nameof(OrgAddressMessageDataLookups.BankCountryStateList))]
		public override ZString US_BankState
		{
			get { return base.US_BankState; }
			set { base.US_BankState = value; }
		}

		[List(nameof(Lookups) + "." + nameof(OrgAddressMessageDataLookups.CountryList))]
		public override ZString US_CountryISOCode
		{
			get { return base.US_CountryISOCode; }
			set
			{
				var oldValue = US_CountryISOCode;
				base.US_CountryISOCode = value;
				if (oldValue != value && !IsCopying)
				{
					if (US_CountryISOCode.IsEmpty)
					{
						US_StateCode = ZString.Empty;
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(OrgAddressMessageDataLookups.CertificateCountryStateList))]
		public override ZString US_StateCode
		{
			get { return base.US_StateCode; }
			set { base.US_StateCode = value; }
		}

		[List(nameof(Lookups) + "." + nameof(OrgAddressMessageDataLookups.Contacts))]
		[RelatedBusinessObject("CertifyIndividual")]
		public override ZGuid US_OC_CertifyIndividual
		{
			get { return base.US_OC_CertifyIndividual; }
			set
			{
				var oldValue = US_OC_CertifyIndividual;
				base.US_OC_CertifyIndividual = value;
				if (oldValue != value && !IsCopying)
				{
					var certifyIndividual = CertifyIndividual;
					if (certifyIndividual != null)
					{
						US_IndividualName = ContactNameHelper.GetFormattedName(certifyIndividual.OC_ContactName);
						US_IndividualPhone = certifyIndividual.OC_Phone.GetLocalPhoneNumber(wrapper.organisation.CountryCode).Left(Schema.US_IndividualPhoneMaxLength);
						US_IndividualTitle = certifyIndividual.OC_Title.Left(Schema.US_IndividualTitleMaxLength);
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(OrgAddressMessageDataLookups.CusAgents))]
		public override ZString US_GS_Broker
		{
			get { return base.US_GS_Broker; }
			set
			{
				var oldValue = US_GS_Broker;
				base.US_GS_Broker = value;
				if (oldValue != value && !IsCopying)
				{
					var broker = Broker;
					if (broker != null)
					{
						US_BrokerName = broker.GS_FullName.Left(AutoUSImportMessageSendingAction.Schema.US_SE_ContactNameMaxLength);
						US_BrokerPhone = MessageSenderContactDetailsDefaultingHelper.GetPhoneNumber(broker).Left(Schema.US_BrokerPhoneMaxLength);
					}
				}
			}
		}

		#endregion

		#region ReadOnly

		public bool US_LineOneAddress1_ReadOnly
		{
			get { return IsAddress1DetailReadOnly; }
		}

		public bool US_LineTwoAddress1_ReadOnly
		{
			get { return IsAddress1DetailReadOnly; }
		}

		public bool US_CityAddress1_ReadOnly
		{
			get { return IsAddress1DetailReadOnly; }
		}

		public bool US_StateAddress1_ReadOnly
		{
			get { return IsAddress1DetailReadOnly; }
		}

		public bool US_ZipAddress1_ReadOnly
		{
			get { return IsAddress1DetailReadOnly; }
		}

		public bool US_RN_NKCountry1_ReadOnly
		{
			get { return IsAddress1DetailReadOnly; }
		}

		bool IsAddress1DetailReadOnly
		{
			get { return !US_OA_Address1.IsValid; }
		}

		public bool US_LineOneAddress2_ReadOnly
		{
			get { return IsAddress2DetailReadOnly; }
		}

		public bool US_LineTwoAddress2_ReadOnly
		{
			get { return IsAddress2DetailReadOnly; }
		}

		public bool US_CityAddress2_ReadOnly
		{
			get { return IsAddress2DetailReadOnly; }
		}

		public bool US_StateAddress2_ReadOnly
		{
			get { return IsAddress2DetailReadOnly; }
		}

		public bool US_ZipAddress2_ReadOnly
		{
			get { return IsAddress2DetailReadOnly; }
		}

		public bool US_RN_NKCountry2_ReadOnly
		{
			get { return IsAddress2DetailReadOnly; }
		}

		bool IsAddress2DetailReadOnly
		{
			get { return !US_OA_Address2.IsValid; }
		}

		public bool US_AddressExplanation1_ReadOnly
		{
			get { return !IsAddressExplanation1Required; }
		}

		internal bool IsAddressExplanation1Required
		{
			get { return US_AddressType1 == ImporterAddressTypesList.Codes._08; }
		}

		public bool US_AddressExplanation2_ReadOnly
		{
			get { return !IsAddressExplanation2Required; }
		}

		internal bool IsAddressExplanation2Required
		{
			get { return US_AddressType2 == ImporterAddressTypesList.Codes._08; }
		}

		public bool US_UtlOtherDescription_ReadOnly
		{
			get { return !IsUtlizationOtherDescriptionRequired; }
		}

		internal bool IsUtlizationOtherDescriptionRequired
		{
			get { return US_UtlOtherIndicator; }
		}

		#endregion

		#region Related Objects

		public OrgAddress FirstAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_Address1); }
		}

		public OrgAddress SecondAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_Address2); }
		}

		public OrgContact CertifyIndividual
		{
			get { return Factory.Load<OrgContact>(US_OC_CertifyIndividual); }
		}

		public GlbStaff Broker
		{
			get { return Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, US_GS_Broker)); }
		}

		public PersonIdentityInformationDataCollection PIIs
		{
			get
			{
				if (fPIIs == null)
				{
					fPIIs = new PersonIdentityInformationDataCollection(this);
					RegisterEditableChildObject(fPIIs);
				}
				return fPIIs;
			}
		}
		PersonIdentityInformationDataCollection fPIIs;

		public RelatedBusinessDataCollection RelatedBusinessItems
		{
			get
			{
				if (fRelatedBusinessItems == null)
				{
					fRelatedBusinessItems = new RelatedBusinessDataCollection(this);
					RegisterEditableChildObject(fRelatedBusinessItems);
				}
				return fRelatedBusinessItems;
			}
		}
		RelatedBusinessDataCollection fRelatedBusinessItems;

		internal HugeSequenceNumberGenerator PIISequenceGenerator
		{
			get { return fPIISequenceGenerator ?? (fPIISequenceGenerator = new HugeSequenceNumberGenerator(new PersonIdentityInformationSequenceNumberHeader(() => new TypedEnumerable<ISequenceNumberLine>(PIIs)))); }
		}
		HugeSequenceNumberGenerator fPIISequenceGenerator;

		internal HugeSequenceNumberGenerator RelatedBusinessSequenceGenerator
		{
			get { return fRelatedBusinessSequenceGenerator ?? (fRelatedBusinessSequenceGenerator = new HugeSequenceNumberGenerator(new RelatedBusinessSequenceNumberHeader(() => new TypedEnumerable<ISequenceNumberLine>(RelatedBusinessItems)))); }
		}
		HugeSequenceNumberGenerator fRelatedBusinessSequenceGenerator;

		public OrgAddressMessageDataLookups Lookups
		{
			get { return new OrgAddressMessageDataLookups(this); }
		}

		#endregion

		#region Methods

		public bool IsUSImporter(ZString uS_RN_NKCountry)
		{
			return Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(uS_RN_NKCountry) == Core.Constants.CountryCodes.UnitedStates;
		}

		public bool IsCAImporter(ZString uS_RN_NKCountry)
		{
			return uS_RN_NKCountry == Core.Constants.CountryCodes.Canada;
		}

		public bool IsMXImporter(ZString uS_RN_NKCountry)
		{
			return uS_RN_NKCountry == Core.Constants.CountryCodes.Mexico;
		}

		public bool IsForeignImporter(ZString uS_RN_NKCountry)
		{
			return !IsUSImporter(uS_RN_NKCountry);
		}

		public bool IsNAFTACountryImporter(ZString uS_RN_NKCountry)
		{
			return IsUSImporter(uS_RN_NKCountry) || IsCAImporter(uS_RN_NKCountry) || IsMXImporter(uS_RN_NKCountry);
		}

		static internal ZString GetZipCodeToARightLength(ZString zipCodeFromOrg, ZString country, int maxLength)
		{
			//US zip can have '-' between the first five and the last four digits.
			var result = zipCodeFromOrg;
			if (country == Core.Constants.CountryCodes.UnitedStates)
			{
				result = zipCodeFromOrg.Replace("-", "");
			}
			return result.Left(maxLength);
		}

		void SetDefaultValues(OrgHeaderWrapper wrapper)
		{
			using (SuspendSettingHasChanges())
			{
				var lastOutgoingMessage = (MQEDIMessage)wrapper.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.AddNewCBPFormCBPF5106DatatotheImporterFile);
				if (lastOutgoingMessage == null)
				{
					var organisation = wrapper.organisation;

					US_ImporterNumber = organisation.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber).Left(US_ImporterNumberInfo.MaxLength);

					if (!US_ImporterNumber.IsEmpty && wrapper.ZO_IsEINNumberVerifiedIndicator == YesNoDefaultList.Codes.Yes)
					{
						US_ActionCode = ImporterADDActionCodeList.Codes.ChangeImporter;
					}
					else
					{
						if (!US_ImporterNumber.IsEmpty || organisation.CountryCode == Core.Constants.CountryCodes.UnitedStates)
						{
							US_ActionCode = ImporterADDActionCodeList.Codes.AddImporterNumber;
						}
						else
						{
							US_ActionCode = ImporterADDActionCodeList.Codes.RequestCBPNumber;
						}
					}

					US_ImporterType = wrapper.ZO_ImporterType;
					US_ImporterName = organisation.OH_FullName.Left(Schema.US_ImporterNameMaxLength);

					var mailingAddress = organisation.GetCustomsAddressDetailsFallingBackToMainAddress();
					var mainBusinessAddress = organisation.MainAddress;

					var firstAddress = mailingAddress ?? mainBusinessAddress;
					US_OA_Address1 = firstAddress.PK;

					if (firstAddress != mainBusinessAddress)
					{
						if (firstAddress.EffectiveCompanyNameTruncated != organisation.OH_FullNameTruncated)
						{
							US_AlternativeImporterName = firstAddress.EffectiveCompanyNameTruncated.Left(Schema.US_AlternativeImporterNameMaxLength);
						}

						US_OA_Address2 = mainBusinessAddress.PK;
					}

					var mainAddress = wrapper.organisation.MainAddress;
					US_ImporterPhoneNumber = mainAddress?.OA_Phone.GetLocalPhoneNumber(mainAddress.Country.Code).Left(Schema.US_ImporterPhoneNumberMaxLength) ?? ZString.Empty;
					US_ImporterEmail = mainAddress?.OA_Email.Left(Schema.US_ImporterEmailMaxLength) ?? ZString.Empty;
					US_ImporterFaxNumber = mainAddress?.OA_Fax.GetLocalPhoneNumber(mainAddress.Country.Code).Left(Schema.US_ImporterFaxNumberMaxLength) ?? ZString.Empty;
					US_ImporterWebsite = wrapper.organisation.MainWebURL?.PU_URL ?? ZString.Empty;
					US_NAICSCode = wrapper.organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.NorthAmericanIndustryClassificationSystem, Core.Constants.CountryCodes.UnitedStates);
					US_DUNS = wrapper.organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, Core.Constants.CountryCodes.UnitedStates);
					US_FilerCode = wrapper.organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.EntryFilerCode, Core.Constants.CountryCodes.UnitedStates);
					var establishedDate = wrapper.organisation.MiscServ.OM_CMEstablishedDate;
					US_YearEstablished = !establishedDate.IsEmpty ? (ZString)establishedDate.Year.ToString("0000", CultureInfo.InvariantCulture) : ZString.Empty;
				}
				else
				{
					var messageImporter = new OrgAddressMessageDataMessageImporter(lastOutgoingMessage, this);
					messageImporter.Populate();
				}
				US_GS_Broker = GlbStaff.CurrentUser.GS_Code;
			}
		}

		#endregion

	}
}

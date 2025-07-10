using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class OrgCusCodeValidation : MasterFiles.Business.OrgCusCodeValidation, Integration.Customs.US.IOrgCusCodeValidation
	{
		public OrgCusCodeValidation(OrgCusCode parent)
			: base(parent)
		{
		}

		protected new OrgCusCode Parent
		{
			get { return (OrgCusCode)base.Parent; }
		}

		OrgHeader parentOrganisation;
		protected OrgHeader ParentOrganisation
		{
			get
			{
				if (parentOrganisation == null)
				{
					ZQuery filter = new ZQuery(OrgHeaderSchema.PK, Parent.OK_OH);
					parentOrganisation = Parent.Factory.LoadTop1<OrgHeader>(filter);
				}
				return parentOrganisation;
			}
		}

		OrgHeaderWrapper parentOrganisationWrapper;
		protected OrgHeaderWrapper ParentOrganisationWrapper
		{
			get
			{
				if (parentOrganisationWrapper == null)
				{
					parentOrganisationWrapper = OrgHeaderWrapper.New(ParentOrganisation);
				}
				return parentOrganisationWrapper;
			}
		}

		protected override void CheckOK_CodeType()
		{
			base.CheckOK_CodeType();

			if (!OrgCusCode.AllowViewSocialSecurityNumber(Parent.OK_CodeType) && (!Parent.IsInDatabase || Parent.OK_CodeTypeInfo.HasChanges))
			{
				Parent.OK_CodeTypeInfo.AddError(GetFailingMessage(Env.Security.OrgDetailsViewPersonalInformation, null));
			}

			if (ParentOrganisation != null)
			{
				int cusCodesFound = 0;
				ZString[] codeTypes = new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber };

				foreach (ZString codeType in codeTypes)
				{
					cusCodesFound += ParentOrganisation.CustomsCodes.GetOrgCusCodesForCodeAndCountry(codeType, Core.Constants.CountryCodes.UnitedStates).Length;
				}

				if (cusCodesFound > 1)
				{
					Parent.OK_CodeTypeInfo.AddError(OnlyOneNumberCanBeEntered);
				}

				if (Parent.OK_CodeType == OrgCusCode.USACodeTypes.DeprecatedSpecialAddressNotification && Parent.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.UnitedStates)
				{
					Parent.OK_CodeTypeInfo.AddError(DeprecatedSpecialAddressNotificationMsg);
				}
			}
		}
		internal const string OnlyOneNumberCanBeEntered = "Only one of EIN/SSN/CBP number can be entered on an organisation.";
		internal const string DeprecatedSpecialAddressNotificationMsg = "Special Address Notification (SAN) number is obsolete. Please use CBPF 4811 Notify Party/CBPF 4811 Notify Party ID on Config -> US Defaults instead.";

		protected override void CheckOK_CustomsRegNo()
		{
			if (!OrgCusCode.AllowViewSocialSecurityNumber(Parent.OK_CodeType))
			{
				return;
			}

			base.CheckOK_CustomsRegNo();

			if (!Parent.OK_CustomsRegNoInfo.HasErrors())
			{
				switch (Parent.OK_CodeType)
				{
					case OrgCusCode.CodeTypes.CarrierCode:
					case OrgCusCode.CodeTypes.TruckCarrierCode:
						ValidateCarrierCode();
						break;
					case OrgCusCode.USACodeTypes.CBPAssignedNumber:
						RunCustomsRegNoValidation(CBPAssignedNumberValidator.Validate, CargoWise.ComponentModel.NotificationType.Error);
						break;
					case OrgCusCode.USACodeTypes.EmployerIdentificationNumber:
						ValidateEINNumber(Parent.OK_CustomsRegNoInfo);
						break;
					case OrgCusCode.USACodeTypes.SocialSecurityNumber:
						RunCustomsRegNoValidation(SocialSecurityNumberValidator.Validate, CargoWise.ComponentModel.NotificationType.Error);
						break;
					case OrgCusCode.USACodeTypes.ForeignRegistrationNumber:
						ValidateForeignRegistrationNumber(Parent.OK_CustomsRegNoInfo);
						break;
					case OrgCusCode.USACodeTypes.NMFCParticipant:
						ValidateNMFCParticipant();
						break;
					case OrgCusCode.USACodeTypes.ManufacturerID:
						ValidateManufacturerID((ZPropertyInfoString)Parent.OK_CustomsRegNoInfo, Parent.Factory, Parent.PremisesAddress, checkDuplicates: true);
						break;
					case OrgCusCode.USACodeTypes.FoodFacilityRegistrationNumber:
						ValidateFoodFacilityRegistrationNumber();
						break;
					case OrgCusCode.USACodeTypes.FIRMSCode:
						RunCustomsRegNoValidation(FIRMSCodeValidator.Validate, CargoWise.ComponentModel.NotificationType.Warning);
						break;
					case OrgCusCode.CodeTypes.DataUniversalNumberingSystem:
						RunCustomsRegNoValidation(DataUniversalNumberingSystemValidator.GetDUNSNumberError, CargoWise.ComponentModel.NotificationType.Warning);
						break;
					case OrgCusCode.USACodeTypes.DataUniversalNumberingSystemPlus4:
						RunCustomsRegNoValidation(DataUniversalNumberingSystemPlus4Validator.Validate, CargoWise.ComponentModel.NotificationType.Warning);
						break;
					case OrgCusCode.USACodeTypes.EncryptedConsigneeNumber:
						RunCustomsRegNoValidation(EncryptedConsigneeNumberValidator.Validate, CargoWise.ComponentModel.NotificationType.Warning);
						break;
					case OrgCusCode.USACodeTypes.ShipperRegistrationNumber:
						RunCustomsRegNoValidation(ShipperRegistrationNumberValidator.Validate, CargoWise.ComponentModel.NotificationType.Warning);
						break;
					case OrgCusCode.USACodeTypes.ACEAssignedNumber:
						RunCustomsRegNoValidation(ACEIdentifierValidator.Validate, CargoWise.ComponentModel.NotificationType.Warning);
						break;
					case OrgCusCode.USACodeTypes.ABIRoutingCode:
						RunCustomsRegNoValidation(ABIFilerValidator.Validate, CargoWise.ComponentModel.NotificationType.Warning);
						break;
					case OrgCusCode.USACodeTypes.FreeAndSecureTradeCode:
						RunCustomsRegNoValidation(FASTIdentifierValidator.Validate, CargoWise.ComponentModel.NotificationType.Warning);
						break;
					case OrgCusCode.CodeTypes.FDAEstablishmentIdentifier:
						ValidateFDAEstablishmentIdentifier();
						break;
					case OrgCusCode.USACodeTypes.TireManufacturerCode:
						ValidateTireManufacturerCode();
						break;
					case OrgCusCode.USACodeTypes.GlazingManufacturerCode:
						ValidateGlazingManufacturerCode();
						break;
					case OrgCusCode.USACodeTypes.DDTCRegistrationNumber:
						ValidateDDTNumber();
						break;
					case OrgCusCode.USACodeTypes.ACASOriginatorCode:
						ValidateACASNumber();
						break;
					case OrgCusCode.USACodeTypes.TTIRegistrationNumber:
						ValidateTTINumber();
						break;
					case OrgCusCode.USACodeTypes.TTEPermitNumber:
						ValidateTTENumber();
						break;
					case OrgCusCode.CodeTypes.DEA:
						ValidateDEARegistrationNumber();
						break;
					case OrgCusCode.USACodeTypes.APHISAssignedNumber:
						ValidateAPHISAssignedNumber();
						break;
					case OrgCusCode.USACodeTypes.IFTPPermitNumber:
						ValidateIFTPPermitNumber();
						break;
					case OrgCusCode.USACodeTypes.AMSRegistrationNumber:
						ValidateAMSRegistrationNumber();
						break;
					case OrgCusCode.USACodeTypes.FDAForeignSellerRegistrationNumber:
						ValidateFDAForeignSellerRegistrationNumber();
						break;
					case OrgCusCode.CodeTypes.CommercialAndGovernmentEntity:
						MandatoryValidation.CheckEntered(Parent.OK_CustomsRegNoInfo);
						if (!CAGCodeIssuer.GetIsCAGCodeValid(Parent) && !Parent.OK_CodeTypeInfo.HasErrors() && !Parent.OK_RN_NKCodeCountryInfo.HasErrors())
						{
							Parent.OK_CustomsRegNoInfo.AddError(Res.GetString("60E014D3-7679-428A-BC0A-3559F148F26E", "Commercial And Government Entity Code is not valid."));
						}
						break;
					case OrgCusCode.USACodeTypes.LegalEntityIdentifier:
						ValidateLegalEntityIdentifier();
						break;
					case OrgCusCode.USACodeTypes.GlobalLocationNumber:
						ValidateGlobalLocationNumber();
						break;
					case OrgCusCode.USACodeTypes.ForeignProducerIdentifier:
					case OrgCusCode.USACodeTypes.ForeignProducerIdentifierBeer:
					case OrgCusCode.USACodeTypes.ForeignProducerIdentifierSpirits:
					case OrgCusCode.USACodeTypes.ForeignProducerIdentifierWine:
						ValidateForeignProducerIdentifier();
						break;
					case OrgCusCode.USACodeTypes.AirAMSOriginatorCode:
						ValidateAirAMSOriginatorCode();
						break;
				}
			}
		}

		void ValidateTTINumber()
		{
			if (Parent.OK_CodeType == OrgCusCode.USACodeTypes.TTIRegistrationNumber)
			{
				ZString message = TTIRegistrationNumberValidator.Validate(Parent.OK_CustomsRegNo);
				if (!message.IsEmpty)
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(message);
				}
			}
		}

		void ValidateIFTPPermitNumber()
		{
			if (Parent.OK_CodeType == OrgCusCode.USACodeTypes.IFTPPermitNumber)
			{
				if (Parent.OK_CustomsRegNo.Length > AutoUSNMFSLineAddInfo.Schema.US_IFTPPermitNumberMaxLength)
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(ValidateIFTPPermitNumberFormat);
				}
			}
		}
		internal const string ValidateIFTPPermitNumberFormat = "Maximum length of Registration No for Code Type of 'IFT' is only 33.";

		void ValidateAPHISAssignedNumber()
		{
			if (Parent.OK_CustomsRegNo.Length != 3 && Parent.OK_CustomsRegNo.Length != 4)
			{
				Parent.OK_CustomsRegNoInfo.AddMessageError(APHISAssignedNumberFormat);
			}
		}
		internal const string APHISAssignedNumberFormat = "APHIS Establishment Number should be in the format, XXX or XXXX (where X is alpha-numeric).";

		void RunCustomsRegNoValidation(Func<ZString, string> validate, INotificationType notificationType)
		{
			ZString message = validate(Parent.OK_CustomsRegNo);
			if (!message.IsEmpty)
			{
				Parent.OK_CustomsRegNoInfo.AddNotification(notificationType, message);
			}
		}

		void ValidateFDAEstablishmentIdentifier()
		{
			var message = EstablishmentIdentifierValidator.Validate(Parent.OK_CustomsRegNo);
			if (!string.IsNullOrEmpty(message))
			{
				Parent.OK_CustomsRegNoInfo.AddWarning(message);
			}

			var filter = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.FDAEstablishmentIdentifier);
			filter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, Parent.OK_CustomsRegNo);
			filter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.UnitedStates);
			filter.AddToFilter(OrgCusCodeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			filter.OrderBy = OrgCusCodeSchema.OK_OH.Name + "," + OrgCusCodeSchema.OK_OA_PremisesAddress.Name;
			var cusCodes = Parent.Factory.Load<OrgCusCode>(filter);

			if (cusCodes.Length > 0)
			{
				var cusCode = cusCodes[0];
				Parent.OK_CustomsRegNoInfo.AddWarning(string.Format(FDAEstablishmentIdentifiersAlreadyInUsed, cusCodes[0].CompanyCodeAndPremisesAddresses));
			}
		}
		public const string FDAEstablishmentIdentifiersAlreadyInUsed = "This FDA Establishment ID is already used for {0}.";

		public static void ValidateManufacturerID(ZPropertyInfoString info, BusinessObjectFactory factory, IAddressDetails address, bool checkDuplicates)
		{
			var mid = info.Value;
			var validator = new ManufacturerIDValidator(factory);
			if (!ManufacturerIDValidator.IsMinMaxAndAlphaNumeric(mid))
			{
				info.AddErrorIfEnforced(ManufacturerIDValidator.Constants.Format, OrganisationRegistry.RegistrationNumberFormatFields.USMID);
			}
			else if (!ManufacturerIDValidator.IsOnlyUpperCase(mid))
			{
				info.AddErrorIfEnforced(ManufacturerIDValidator.Constants.UpperCase, OrganisationRegistry.RegistrationNumberFormatFields.USMID);
			}
			else if (address != null && !address.Country.IsEmpty &&
					 !ManufacturerIDValidator.IsMIDValidForCountry(mid, address.Country))
			{
				info.AddMessageError(ManufacturerIDValidator.Constants.Country);
			}
			else if (!validator.IsISOCountryCodeValid(mid))
			{
				info.AddWarning(ManufacturerIDValidator.Constants.ISOCode);
			}
			if (checkDuplicates)
			{
				var orgCusCode = info.BizObj as OrgCusCode;
				if (orgCusCode != null)
				{
					OrgCusCodeValidation.ValidateDuplicate(orgCusCode, Constants.CountryCodes.UnitedStates, getMessage, true, OrganisationRegistry.RegistrationNumberFormatFields.USMID);
				}
				else
				{
					OrgCusCodeValidation.ValidateDuplicate(info.BizObj.Factory, info, OrgCusCode.USACodeTypes.ManufacturerID, Constants.CountryCodes.UnitedStates, getMessage, true, OrganisationRegistry.RegistrationNumberFormatFields.USMID);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static readonly Func<OrgCusCode[], ZString> getMessage = (OrgCusCode[] duplicateCodes) =>
		{
			var duplicates = duplicateCodes
				.Where(code => code.Header != null && code.Header.OH_IsActive)
				.Select(code => code.CompanyCodeAndPremisesAddresses)
				.OrderBy(str => str)
				.ToArray();
			var error = "";
			if (duplicates.Length > 0)
			{
				error = "This Manufacturer ID already exists on ";
				if (duplicates.Length > 1)
				{
					error += "other organizations:" + System.Environment.NewLine + " - " +
						string.Join(System.Environment.NewLine + " - ", duplicates.Take(10));
					if (duplicates.Length > 10)
					{
						error += System.Environment.NewLine + "..." + System.Environment.NewLine +
							"Please go to Admin -> Organization module and find all organizations with this manufacturer ID using 'Registration Number' module filter.";
					}
				}
				else
				{
					error += "organization " + duplicates[0];
				}
			}
			return error;
		};

		void ValidateFoodFacilityRegistrationNumber()
		{
			if (Parent.OK_CustomsRegNo.KeepNumericCharacters().Length != 11)
			{
				Parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(ValidationConstants.PriorNotice.FoodFacilityRegistrationNumberFormat, OrganisationRegistry.RegistrationNumberFormatFields.USPFR);
			}
		}

		void ValidateNMFCParticipant()
		{
			if (OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.Value.GetBoolFromCode(OrganisationRegistry.RegistrationNumberFormatFields.USNMF))
			{
				ListValidation.ErrorIfInvalidCode(Parent.OK_CustomsRegNoInfo, Parent.Lookups.NMFCParticipantList);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(Parent.OK_CustomsRegNoInfo, Parent.Lookups.NMFCParticipantList);
			}
		}

		protected override void ValidateCarrierCode()
		{
			if (Parent.OK_CustomsRegNo.Length > 4)
			{
				Parent.OK_CustomsRegNoInfo.AddErrorIfEnforced("Maximum length of Registration No for Code Type of '" + Parent.OK_CodeType + "' is 4.", OrganisationRegistry.RegistrationNumberFormatFields.USCCC);
			}
		}

		void ValidateDDTNumber()
		{
			if (Parent.OK_CodeType == OrgCusCode.USACodeTypes.DDTCRegistrationNumber)
			{
				if (Parent.OK_CustomsRegNo.Length != 6)
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(DDTRegistrationNumberFormatError);
				}
			}
		}
		public const string DDTRegistrationNumberFormatError = "DDTC Registration Number should be a 6 alpha numeric number.";

		void ValidateACASNumber()
		{
			if (Parent.OK_CodeType == OrgCusCode.USACodeTypes.ACASOriginatorCode)
			{
				if (!Regex.IsMatch(Parent.OK_CustomsRegNo, @"^WTG[PT][A-Z0-9]{3}$", RegexOptions.IgnoreCase))
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(ACARegistrationNumberFormatError);
				}
			}
		}
		internal const string ACARegistrationNumberFormatError = "ACAS code should start with WTGP for Productions systems or WTGT for Test systems, followed by three additional characters.  E.g. WTGPXX1 or WTGTXX1";

		void ValidateTTENumber()
		{
			if (Parent.OK_CodeType == OrgCusCode.USACodeTypes.TTEPermitNumber)
			{
				if (!Regex.IsMatch(Parent.OK_CustomsRegNo, @"^((\w{2}-\w{3}-\w{2}-\w{5})|((\w{2}-){2}\w{5})|(\w{3}-\w{2}-\w{5})|(\w{2}-?\w{9})|(\w{2}-\w{2}-\w{3}-\w{1}))$"))
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(TTEPermitNumberFormatError);
				}
			}
		}

		public const string TTEPermitNumberFormatError = "TTE Permit Number should be in the format XX-XXX-XX-XXXXX or XX-XX-XXXXX or XXX-XX-XXXXX or XX-XX-XXX-X or EIN.";

		void ValidateAMSRegistrationNumber()
		{
			if (Parent.OK_CodeType == OrgCusCode.USACodeTypes.AMSRegistrationNumber)
			{
				if (!Regex.IsMatch(Parent.OK_CustomsRegNo, @"^[0-9]{3,4}$") && !Regex.IsMatch(Parent.OK_CustomsRegNo, @"^[0-9]{10}$"))
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(AMSRegistrationNumberFormatError);
				}
			}
		}
		public const string AMSRegistrationNumberFormatError = "AMS Assigned ID Number should be 3, 4 or 10 digits";

		internal static void ValidateEINNumber(ZPropertyInfo customsRegNoInfo)
		{
			ZString message = EmployerIdentificationNumberValidator.Validate((ZString)customsRegNoInfo.Value);
			if (!message.IsEmpty)
			{
				customsRegNoInfo.AddErrorIfEnforced(message, OrganisationRegistry.RegistrationNumberFormatFields.USEIN);
			}
		}

		void ValidateDEARegistrationNumber()
		{
			if (Parent.OK_CodeType == OrgCusCode.CodeTypes.DEA)
			{
				if (Parent.OK_CustomsRegNo.Length != 9 || !Parent.OK_CustomsRegNo.IsLettersAndNumbersOnlyOrEmpty)
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(DEARegistrationNumberFormatError);
				}
			}
		}
		internal const string DEARegistrationNumberFormatError = "DEA Registration Number should be a 9 alpha numeric number.";

		internal static void ValidateForeignRegistrationNumber(ZPropertyInfo customsRegNoInfo)
		{
			ZString number = ((ZString)customsRegNoInfo.Value).KeepAlphanumericCharacters();
			bool isValid = number.Length == 11;

			if (!isValid)
			{
				customsRegNoInfo.AddWarning(ForeignRegistrationNumberRightFormat);
			}
		}
		internal const string ForeignRegistrationNumberRightFormat = "Foreign Registration Number should be an 11 alpha numeric number.";

		void ValidateTireManufacturerCode()
		{
			var manufacturerCode = Parent.OK_CustomsRegNo.KeepAlphanumericCharacters();
			if (manufacturerCode.Length != 2 && manufacturerCode.Length != 3)
			{
				Parent.OK_CustomsRegNoInfo.AddMessageError(TireManufacturerCodeRightFormat);
			}
		}
		internal const string TireManufacturerCodeRightFormat = "Tire Manufacturer Code should be in the format, XX or XXX (where X is alpha-numeric).";

		void ValidateGlazingManufacturerCode()
		{
			if (!Regex.IsMatch(Parent.OK_CustomsRegNo, @"^[0-9]{1,4}$", RegexOptions.IgnoreCase))
			{
				Parent.OK_CustomsRegNoInfo.AddMessageError(GlazingManufacturerCodeRightFormat);
			}
		}
		internal const string GlazingManufacturerCodeRightFormat = "Glazing Manufacturer Code should be in the format, N or NN or NNN or NNNN (where N is a number).";

		void ValidateFDAForeignSellerRegistrationNumber()
		{
			if (!Parent.OK_CustomsRegNo.IsNumbersOnlyOrEmpty || Parent.OK_CustomsRegNo.Length != 9)
			{
				Parent.OK_CustomsRegNoInfo.AddWarning(FDAForeignSellerRegistrationNumberFormat);
			}
		}
		internal const string FDAForeignSellerRegistrationNumberFormat = "FDA Foreign Seller Registration Number should be 9 digits.";

		void ValidateLegalEntityIdentifier()
		{
			if (!Parent.OK_CustomsRegNo.IsLettersAndNumbersOnlyOrEmpty || Parent.OK_CustomsRegNo.Length != 20)
			{
				Parent.OK_CustomsRegNoInfo.AddWarning(LegalEntityIdentifierFormat);
			}
		}
		internal const string LegalEntityIdentifierFormat = "Legal Entity Identifier should be a 20 alpha numeric number.";

		void ValidateGlobalLocationNumber()
		{
			if (!Parent.OK_CustomsRegNo.IsNumbersOnlyOrEmpty || Parent.OK_CustomsRegNo.Length != 13)
			{
				Parent.OK_CustomsRegNoInfo.AddWarning(GlobalLocationNumberFormat);
			}
		}
		internal const string GlobalLocationNumberFormat = "Global Location Number should be 13 digits.";

		void ValidateForeignProducerIdentifier()
		{
			var identifierPrefix = ZString.Empty;
			var regexExpression = ZString.Empty;
			var warningMessage = ZString.Empty;

			switch (Parent.OK_CodeType)
			{
				case OrgCusCode.USACodeTypes.ForeignProducerIdentifier:
					identifierPrefix = "I";
					regexExpression = @"^TTB-FP-[a-zA-Z0-9]{7}$";
					warningMessage = FPIRegistrationNumberFormatError;
					break;
				case OrgCusCode.USACodeTypes.ForeignProducerIdentifierBeer:
					identifierPrefix = "B";
					regexExpression = @"^B\S{2,11}\d{2}$";
					warningMessage = ZString.Format(FPB_FPS_FPW_ForeignProducerIdentifierFormat, "Beer", identifierPrefix);
					break;
				case OrgCusCode.USACodeTypes.ForeignProducerIdentifierSpirits:
					identifierPrefix = "S";
					regexExpression = @"^S\S{2,11}\d{2}$";
					warningMessage = ZString.Format(FPB_FPS_FPW_ForeignProducerIdentifierFormat, "Spirits", identifierPrefix);
					break;
				case OrgCusCode.USACodeTypes.ForeignProducerIdentifierWine:
					identifierPrefix = "W";
					regexExpression = @"^W\S{2,11}\d{2}$";
					warningMessage = ZString.Format(FPB_FPS_FPW_ForeignProducerIdentifierFormat, "Wine or Cider", identifierPrefix);
					break;
			}

			if (!identifierPrefix.IsEmpty && !Regex.IsMatch(Parent.OK_CustomsRegNo, regexExpression))
			{
				Parent.OK_CustomsRegNoInfo.AddWarning(warningMessage);
			}
		}
		internal const string FPIRegistrationNumberFormatError = "The FPI as assigned by TTB should be in the format TTB-FP-XXXXXXX where XXXXXXX is any combination of alphanumeric characters.";
		internal const string FPB_FPS_FPW_ForeignProducerIdentifierFormat = "Foreign Producer Identifier for {0} should starts with {1}, followed by up to 6 characters producer name and up to 5 characters postal code then 2 digits calendar year.";

		void ValidateAirAMSOriginatorCode()
		{
			if (!Regex.IsMatch(Parent.OK_CustomsRegNo, @"^[a-zA-Z0-9]{7}$"))
			{
				Parent.OK_CustomsRegNoInfo.AddError(AirAMSOriginatorCodeFormat);
			}
		}
		internal const string AirAMSOriginatorCodeFormat = "Air AMS Originator Code should be a 7 alpha numeric characters.";
	}
}

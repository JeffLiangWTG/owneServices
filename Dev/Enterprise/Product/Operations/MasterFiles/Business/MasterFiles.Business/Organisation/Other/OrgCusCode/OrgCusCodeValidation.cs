using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.CountryCompliance.Portugal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public static class ZPropertyInfoExtensions
	{
		public static void AddErrorIfEnforced(this ZPropertyInfo propertyInfo, string message, string field)
		{
			if (OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.Value.GetBoolFromCode(field))
			{
				propertyInfo.AddError(message);
			}
			else
			{
				propertyInfo.AddWarning(message);
			}
		}
	}

	public class OrgCusCodeValidation : AutoOrgCusCodeValidation
	{
		public OrgCusCodeValidation(AutoOrgCusCode parent)
			: base(parent)
		{
			this.Parent = (OrgCusCode)parent;
		}

		public static bool AllowDuplicates(ZString type, ZString countryCode)
		{
			return !type.IsEmpty
				&& type != OrgCusCode.CodeTypes.GS1
				&& type != OrgCusCode.CodeTypes.ContainerChainCommunityCode
				&& !(type == OrgCusCode.CodeTypes.ControlledPremisesID && countryCode != Constants.CountryCodes.Japan)
				&& !(type == OrgCusCode.USACodeTypes.ManufacturerID && countryCode == Constants.CountryCodes.UnitedStates);
		}

		public static void ValidateDuplicate(OrgCusCode parent, ZString countryCode, ZString message, bool useEnforce = false, string field = "")
		{
			ValidateDuplicate(parent, countryCode, (y) => message, useEnforce, field);
		}

		public static ZQuery GetDuplicateQuery(ZString countryCode, ZString type, ZString customsRegNo, ZGuid? parentPK = null)
		{
			countryCode = type == OrgCusCode.CodeTypes.GS1 ? ZString.Empty : countryCode;
			var query = new ZQuery(OrgCusCodeSchema.OK_CodeType, type);
			query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, customsRegNo);
			if (!countryCode.IsEmpty)
			{
				query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, countryCode);
			}
			if (parentPK.HasValue)
			{
				query.AddToFilter(OrgCusCodeSchema.PK, SQLComparisonOperator.NotEqual, parentPK.Value);
			}
			return query;
		}

		public static void ValidateDuplicate(OrgCusCode parent, ZString countryCode, Func<OrgCusCode[], ZString> getMessage, bool useEnforce = false, string field = "")
		{
			Argument.NotNull(parent, nameof(parent));
			Argument.NotNull(getMessage, nameof(getMessage));
			Argument.NotNull(field, nameof(field));
			if (parent != null)
			{
				ValidateDuplicate(parent.Factory, parent.OK_CustomsRegNoInfo, parent.OK_CodeType, countryCode, getMessage, useEnforce, field, parent.PK);
			}
		}

		public static void ValidateDuplicate(BusinessObjectFactory factory, ZPropertyInfo info, ZString codeType, ZString countryCode, Func<OrgCusCode[], ZString> getMessage, bool useEnforce = false, string field = "", ZGuid? parentPK = null)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(getMessage, nameof(getMessage));
			Argument.NotNull(field, nameof(field));

			if (!AllowDuplicates(codeType, countryCode))
			{
				var duplicateCodes = factory.Load<OrgCusCode>(GetDuplicateQuery(countryCode, codeType, (ZString)info.Value, parentPK));
				if (duplicateCodes.Length > 0)
				{
					var message = getMessage(duplicateCodes);
					if (!message.IsEmpty)
					{
						if (useEnforce)
						{
							info.AddErrorIfEnforced(message, field);
						}
						else
						{
							info.AddError(message);
						}
					}
				}
			}
		}

		public static void ValidateCustomsCodeForEU(OrgCusCode orgCusCode)
		{
			if (!orgCusCode.CountryIsMemberOf(EconomicGroupList.Codes.EuropeanUnion))
			{
				return;
			}

			var code = orgCusCode.OK_CustomsRegNo.ToUpper();
			var codeCountry = orgCusCode.CodeCountry;
			var countryCode = codeCountry == null ? ZString.Empty : codeCountry.RN_Code;

			if (orgCusCode.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator)
			{
				EU.ValidateAEO(code, countryCode, orgCusCode.OK_CustomsRegNoInfo);
			}
			else if (orgCusCode.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber)
			{
				EU.ValidateTEN(code, orgCusCode.OK_CustomsRegNoInfo);
			}
		}

		public static void ValidateCustomsCodeEORI(OrgCusCode orgCusCode)
		{
			var code = orgCusCode.OK_CustomsRegNo.ToUpper();
			var codeCountry = orgCusCode.CodeCountry;
			var countryCode = codeCountry == null ? ZString.Empty : codeCountry.RN_Code;

			if (orgCusCode.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.Turn)
			{
				EU.ValidateTurnCode(code, countryCode, orgCusCode.OK_CustomsRegNoInfo);
			}
			else if (orgCusCode.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori)
			{
				EU.ValidateEoriCode(code, countryCode, orgCusCode.OK_CustomsRegNoInfo);
			}
		}

		#region Validation Overrides

		bool IsUANorUENforUPEClient
		{
			get
			{
				var result = false;
				var codeType = Parent.OK_CodeTypeInfo.Value;
				if ((codeType.Equals("UAN") || codeType.Equals("UEN")))
				{
					if (upeClientDllExists.HasValue)
					{
						result = upeClientDllExists.Value;
					}
					else
					{
						var applicationStartupDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
						var upeDllFileName = Path.Combine(applicationStartupDirectory, "ZClientUPE.dll");
						if (File.Exists(upeDllFileName))
						{
							var upeClientAssembly = ClientHookLoader.Instance.GetAssemblyFromFileName(upeDllFileName);
							if (upeClientAssembly != null)
							{
								result = ClientHookLoader.Instance.GetClientHookFromAssembly(upeClientAssembly).Client == Clients.UPE;
							}
						}

						upeClientDllExists = result;
					}
				}

				return result;
			}
		}

		bool? upeClientDllExists;

		protected override void CheckOK_CodeType()
		{
			base.CheckOK_CodeType();

			var parent = Parent;
			MandatoryValidation.CheckEntered(parent.OK_CodeTypeInfo);
			if (!IsUANorUENforUPEClient)
			{
				ListValidation.ErrorIfInvalidCode(parent.OK_CodeTypeInfo);
			}

			ValidateCusCodeIsUnique();
			ValidateCusCodeAllowedBySecurity();
			ValidateCusCodeMeetCannotCoexistsRule();
			ValidateCusCodeMeetDependencyRule();
			PortugalValidatorHelper.AddErrorIfOrgCusCodeChangeIsNotAllowed(parent.OK_CodeTypeInfo, Res.GetString("1403ed39-9c7d-4836-b14b-1c82c93c99cf", "You cannot edit this code type.At least one transaction has been posted in a Portugal Login Company in this database using this Organization."));

			if (!parent.OK_CodeTypeInfo.HasErrors())
			{
				ValidateRegistrationForCustomsCarrierCodeIsUniqueForEachCountry();
				ValidateRegistrationForCargoWiseOneCarrierCodeIsUnique();
				ValidateRegistrationForBoleroTitleRegisterIDIsUnique();
				ValidateRegistrationForCargoWiseRoadTransportProviderCodeIsUnique();
				ValidateUniversalCodesExistOnce();

				if (parent.CountryIs(Constants.CountryCodes.China))
				{
					ValidateCodeTypeForCN();
				}
				else if (parent.CodeCountry?.IsIcs2Member ?? false)
				{
					ValidateCodeTypeForIcs2Members();
				}
			}
		}

		protected override void CheckOK_CustomsRegNo()
		{
			base.CheckOK_CustomsRegNo();

			if (!Parent.CountryIs(Constants.CountryCodes.Singapore) || Parent.OK_CodeType != OrgCusCode.SingaporeCodeTypes.DirectDelivery)
			{
				MandatoryValidation.CheckEntered(Parent.OK_CustomsRegNoInfo);
			}

			if (!Parent.OK_CustomsRegNoInfo.HasErrors())
			{
				if (Parent.CustomsRegNoLookupList.Count > 0)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.OK_CustomsRegNoInfo);
				}

				PortugalValidatorHelper.AddErrorIfOrgCusCodeChangeIsNotAllowed(Parent.OK_CustomsRegNoInfo, Res.GetString("70ef7fff-fc74-4406-a9c9-b489e5c2ac98", "You cannot edit this registration number. At least one transaction has been posted in a {0} Login Company in this database using this Organization.", Parent.CodeCountry?.RN_Desc ?? Parent.OK_RN_NKCodeCountry));
				PortugalValidatorHelper.AddErrorIfOrgCusCodeIsDuplicate(Parent.OK_CustomsRegNoInfo, Res.GetString("0ca3c336-279a-4d57-865a-70f77833a889", "Organizations can only have one {0} {1} Number. This organization already has a recorded {0} {1} number.", Parent.OK_RN_NKCodeCountry, Parent.OK_CodeType));

				ValidateCustomsCode();
				ValidateWorldCargoAssociationNumber();
				ValidateTirIdentificationNumberFormat();
				ValidateControlledPremisesID();
				ValidateGS1CompanyPrefix();
				ValidateCargoWiseOneCarrierCode();
				ValidateNorthAmericanIndustryClassificationSystem();
				ValidateStandardIndustrialClassification();
				ValidateDoDAAC();
				ValidateCAGCode();
				ValidateCarrierShippingLine();
				ValidateCW1CarrierShippingLine();
				ValidateREXCode();
				ValidateConflictingRegistrationCodes();
				ValidateEORICode();
				ValidateContainerChainCommunityCodeRegistrationNumber();
				ValidateUKMCode();
			}
		}

		public void ValidateConflictingRegistrationCodes()
		{
			var cusCode = Parent;
			if (cusCode.CodeCountry != null && cusCode.Organisation != null && IsPrimaryUniqueRegistrationCode(cusCode))
			{
				var parameters = new ZSqlParameterCollection
				{
					ZSqlParameter.New("@orgPK", cusCode.Organisation.PK, OrgCusCodeSchema.OK_OH),
					ZSqlParameter.New("@regNo", Regex.Replace(cusCode.OK_CustomsRegNoInfo.Value.ToString(), @"\.|\(|-|\)|\/|\,|\s",string.Empty), OrgCusCodeSchema.OK_CustomsRegNo),
					ZSqlParameter.New("@codeType", cusCode.OK_CodeType, OrgCusCodeSchema.OK_CodeType),
					ZSqlParameter.New("@countryCode", cusCode.OK_RN_NKCodeCountry, OrgCusCodeSchema.OK_RN_NKCodeCountry)
				};

				var sql = @"SELECT TOP (10) OH_Code
FROM dbo.OrgCusCode
JOIN dbo.OrgHeader ON OH_PK = OK_OH
WHERE OK_OH <> @orgPK
AND OK_CodeType = @codeType
AND OK_RN_NKCodeCountry = @countryCode
AND OK_UnSignedCustomsRegNo = @regNo";

				var collection = new DynamicBusinessObjectCollection(cusCode.Factory);
				collection.Load(sql, parameters);

				if (collection.Any())
				{
					var message = Res.GetString("b757a4d9-485f-44ff-b6c0-37f3739bc60b", "This Registration Number is already in use by at least one organization. The organizations are: {0} (Maximum of 10 organizations shown). Please check that the organizations are not the same.", string.Join(", ", collection.Select(p => p[OrgHeaderSchema.OH_Code]).OrderBy(p => p)));
					cusCode.OK_CustomsRegNoInfo.AddWarning(message);
				}
			}
		}

		bool IsPrimaryUniqueRegistrationCode(OrgCusCode cusCode)
		{
			var nonUniqueCodes = OrgCusCodeCountryFactory.GetIOrgCusCodeNonUniqueProvider(cusCode.OK_RN_NKCodeCountry)?.GetNonUniqueCodes();
			if (nonUniqueCodes?.Contains(cusCode.OK_CodeType) ?? false)
			{
				return false;
			}
			else
			{
				var primaryCodes = OrgRegistrationNumberTypeList.GetApplicableNumberTypes(cusCode.CodeCountry);

				return primaryCodes.Any(p => p == cusCode.OK_CodeType);
			}
		}

		void ValidateDoDAAC()
		{
			if (Parent.OK_CodeType == OrgCusCode.USACodeTypes.DepartmentOfDefenseActivityAddressCode)
			{
				MandatoryValidation.CheckEntered(Parent.OK_CustomsRegNoInfo);

				if (!Parent.OK_CustomsRegNo.IsLettersAndNumbersOnlyOrEmpty || Parent.OK_CustomsRegNo.Length != 6)
				{
					Parent.OK_CustomsRegNoInfo.AddError(Res.GetString("508981F9-5EEE-4F6F-B051-58708CF2E8DD", "Invalid code entered. Department of Defense Activity Address Code must be 6 alpha-numeric characters."));
				}
			}
		}

		void ValidateCAGCode()
		{
			if (Parent.OK_CodeType == OrgCusCode.CodeTypes.CommercialAndGovernmentEntity)
			{
				MandatoryValidation.CheckEntered(Parent.OK_CustomsRegNoInfo);

				if (!Parent.OK_CodeTypeInfo.HasErrors() && !Parent.OK_RN_NKCodeCountryInfo.HasErrors() && !CAGCodeIssuer.GetIsCAGCodeValid(Parent))
				{
					Parent.OK_CustomsRegNoInfo.AddError(Res.GetString("BD054206-A70B-42B1-8987-8D76E06B20A5", "Commercial And Government Entity Code is not valid."));
				}
			}
		}

		void ValidateCarrierShippingLine()
		{
			if (Parent.OK_CodeType == OrgCusCode.CodeTypes.CarrierCode && !Parent.OK_CustomsRegNo.IsEmpty && Parent.CountryIs(Constants.CountryCodes.UnitedStates))
			{
				var matchesExistingSCAC = Parent.Factory.LoadTop1<RefShippingLine>(
					new ZQuery(RefShippingLineSchema.RSL_StandardCarrierAlphaCode, Parent.OK_CustomsRegNo));
				if (matchesExistingSCAC == null)
				{
					Parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("89cb1462-2582-4df6-91b5-df971d3c10c7",
						"This is not a valid SCAC. Refer to Shipping Lines under Reference Files for a list of valid codes."));
				}

				if (Parent.OK_CustomsRegNo.Length != AutoRefShippingLine.Schema.RSL_StandardCarrierAlphaCodeMaxLength)
				{
					Parent.OK_CustomsRegNoInfo.AddError(Res.GetString("AB8A9592-11CF-40B2-A35D-63833EC32703",
						"SCAC must be {0} character length.", AutoRefShippingLine.Schema.RSL_StandardCarrierAlphaCodeMaxLength));
				}
			}
		}

		void ValidateCW1CarrierShippingLine()
		{
			if (Parent.OK_CodeType == OrgCusCode.CodeTypes.CargoWiseOneCarrierCode && !Parent.OK_CustomsRegNo.IsEmpty)
			{
				var matchesExistingC1C = Parent.Factory.LoadTop1<RefShippingLine>(
						new ZQuery(RefShippingLineSchema.RSL_CargoWiseOneCode, Parent.OK_CustomsRegNo));
				if (matchesExistingC1C == null)
				{
					Parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("81168773-b065-4be2-9ee4-fe9217119bb4",
						"This is not a valid SCAC. Refer to Shipping Lines under Reference Files for a list of valid codes."));
				}
				else if (matchesExistingC1C.RSL_StandardCarrierAlphaCode.IsEmpty)
				{
					Parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("03de3a33-e57d-4399-bd90-8fea97a631b4",
						"This SCAC does not match the chosen C1C code. Please delete or correct this entry."));
				}
			}
		}

		void ValidateREXCode()
		{
			if (Parent.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.RegisteredExporterNumber)
			{
				if (!Regex.IsMatch(Parent.OK_CustomsRegNo, "^" + Parent.CodeCountry.Code + "REX[A-Z0-9]{1,30}$"))
				{
					Parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("3b05dda0-cc9b-401f-938f-37d482893d6c", "REX code is composed by Country/Region Code + REX + Identification number, max total length of 35 uppercase characters"));
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void ValidateCustomsCode()
		{
			var countrySpecificValidationProvider = OrgCusCodeCountryFactory.GetIOrgCusCodeCustomsRegNoValidationProvider(Parent.OK_RN_NKCodeCountry);
			if (countrySpecificValidationProvider != null)
			{
				countrySpecificValidationProvider.Validate(Parent);
			}

			#region Please do not add new country-specific logic below. Add new logic through country-specific classes created by OrgCusCodeCountryFactory.
			else
			{
				// SouthAfrica validation is done in Enterprise.Customs.ZA.Business.OrgCusCodeValidation
				// UnitedStates validation is done in Enterprise.Customs.US.Business.OrgCusCodeValidation
				// Australia validation is done in Enterprise.Customs.AU.Business.OrgCusValidation
				if (Parent.CountryIs(Constants.CountryCodes.Canada))
				{
					ValidateCustomsCodeForCA();
				}
				else if (Parent.CountryIs(Constants.CountryCodes.NewZealand))
				{
					ValidateCustomsCodeForNZ();
				}
				else if (Parent.CountryIs(Constants.CountryCodes.Singapore))
				{
					ValidateCRNForSG();
					ValidateUENForSG();
					ValidateQCIForSG();
					ValidatePSTForSG();
					ValidateIBGForSG();
					ValidateDIRForSG();
					ValidateAEOForSG();
				}
				else if (Parent.CountryIs(Constants.CountryCodes.Iceland))
				{
					ValidateCustomsCodeForIS();
				}
				else if (Parent.CountryIs(Constants.CountryCodes.Angola))
				{
					ValidateCustomsCodeForAO();
				}
				else if (Parent.CountryIs(Constants.CountryCodes.Netherlands))
				{
					ValidateCustomsCodeForNL();
				}
				else if (Parent.CountryIs(Constants.CountryCodes.UnitedKingdom))
				{
					ValidateCustomsCodeForGB();
				}
				else if (Parent.CountryIs(Constants.CountryCodes.Denmark))
				{
					ValidateCustomsCodeForDK();
				}
				else if (Parent.CountryIs(Constants.CountryCodes.Japan))
				{
					ValidateCustomsCodeForJP();
				}
				else if (Parent.CountryIs(Constants.CountryCodes.HongKong))
				{
					ValidateCustomsCodeForHK();
				}
				else if (Parent.CountryIs(Constants.CountryCodes.Israel))
				{
					ValidateCustomsCodeForIL();
				}
				else if (Parent.CountryIs(Constants.CountryCodes.China))
				{
					ValidateCustomsCodeForCN();
				}
				else if (Parent.CountryIs(Constants.CountryCodes.Belarus))
				{
					ValidateCustomsCodeAEOForBY();
				}
				if (Parent.CountryIsMemberOf(EconomicGroupList.Codes.EuropeanUnion))
				{
					OrgCusCodeValidation.ValidateCustomsCodeForEU(Parent);
				}
				OrgCusCodeValidation.ValidateCustomsCodeEORI(Parent);
			}

			#endregion

			var codeCountry = Parent.CodeCountry;
			if (codeCountry != null && codeCountry.IsRecognisedByEuropeanUnionAsIssuerOfThirdCountryUniqueIdentificationNumbersOrAEO)
			{
				ValidateCustomsCodeForFriendOfEu();
			}
		}

		void ValidateGS1CompanyPrefix()
		{
			if (Parent.OK_CodeType == OrgCusCode.CodeTypes.GS1)
			{
				if (!Parent.OK_CustomsRegNo.IsNumbersOnlyOrEmpty)
				{
					Parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(Res.GetString("335c9037-c529-4827-8584-5f5bf302c682", "A GS1 Company prefix can only contain numbers 0-9."), OrganisationRegistry.RegistrationNumberFormatFields.GS1);
				}

				var length = Parent.OK_CustomsRegNo.Length;
				if (length < Constants.GS1.PrefixMinLength || length > Constants.GS1.PrefixMaxLength)
				{
					Parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(Res.GetString("0e007981-6b5b-4f14-a931-29bfbe61d201", "A GS1 Company prefix must have a length of {0}-{1} digits.", Constants.GS1.PrefixMinLength, Constants.GS1.PrefixMaxLength), OrganisationRegistry.RegistrationNumberFormatFields.GS1);
				}

				ValidateDuplicate(Parent, Parent.OK_RN_NKCodeCountry, (duplicateCodes) =>
				{
					var duplicate = duplicateCodes.Select(x => x.Header)
						.Where(header => header != null && header.OH_IsActive)
						.OrderBy(header => header.OH_Code)
						.FirstOrDefault();
					return duplicate == null ? string.Empty : Res.GetString("43ae6058-37fe-4e92-b7ec-1714d21296c0", "The GS1 Company prefix '{0}' has already been used for Organization '{1}'.", Parent.OK_CustomsRegNo, duplicate.OH_Code);
				},
				useEnforce: false,
				field: OrganisationRegistry.RegistrationNumberFormatFields.GS1);
			}
		}

		protected override void CheckOK_RN_NKCodeCountry()
		{
			base.CheckOK_RN_NKCodeCountry();
			ValidateOK_CodeType();
			ValidateOK_CountryDefault();
			ValidateOK_CustomsRegNo();
			ListValidation.ErrorIfInvalidCode(Parent.OK_RN_NKCodeCountryInfo);
			PortugalValidatorHelper.AddErrorIfOrgCusCodeChangeIsNotAllowed(Parent.OK_RN_NKCodeCountryInfo, Res.GetString("95c6daac-650a-46c0-a4b6-b39ceb3c2b17", "You cannot edit this country/region.At least one transaction has been posted in a Portugal Login Company in this database using this Organization."));

			if (Parent.CodeCountry?.IsIcs2Member ?? false)
			{
				ValidateCodeCountryForEU();
			}
		}

		protected override void CheckOK_OA_PremisesAddress()
		{
			base.CheckOK_OA_PremisesAddress();

			if (!Parent.OK_OA_PremisesAddress.IsEmpty && !Parent.PremisesAddressIsAllowed)
			{
				Parent.OK_OA_PremisesAddressInfo.AddError(Res.GetString("471ad5de-f457-4f74-99ab-bcec30eb94f6", "An address can only be entered for code types that have a physical premise associated with them."));
			}
			else if (Parent.OK_OA_PremisesAddress.IsEmpty && Parent.PremisesAddressIsRequired)
			{
				Parent.OK_OA_PremisesAddressInfo.AddError(Res.GetString("7a7bab81-4d2d-49dc-9ece-2a9c062b564f", "An address is required for code type '{0}'.", Parent.OK_CodeType));
			}

			ValidateOK_CodeType();
			ValidateEBSForFRPremisesAddress();
		}

		protected override void CheckOK_CountryDefault()
		{
			base.CheckOK_CountryDefault();
			ValidateCountryDefaultIsUniqueForCountry();
		}

		#endregion

		#region Validation Methods

		#region Controlled Premises ID

		public void ValidateControlledPremisesID()
		{
			if (Parent.OK_CodeType == OrgCusCode.CodeTypes.ControlledPremisesID)
			{
				var ccpField = Parent.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Australia ? OrganisationRegistry.RegistrationNumberFormatFields.AUCCP : Parent.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.NewZealand ? OrganisationRegistry.RegistrationNumberFormatFields.NZCCP : OrganisationRegistry.RegistrationNumberFormatFields.CCP;
				ValidateDuplicate(Parent, Parent.OK_RN_NKCodeCountry, (duplicateCodes) =>
				{
					var duplicate = duplicateCodes.Select(x => x.Header)
						.Where(header => header != null && header.OH_IsActive)
						.OrderBy(header => header.OH_Code)
						.FirstOrDefault();
					return duplicate == null ? string.Empty : Res.GetString("f44de3db-70fe-4a22-b093-4245030e374e", "This Controlled Premises ID is already registered for the organization {0} ({1})", duplicate.OH_Code, duplicate.OH_FullName);
				}, true, ccpField);

				if (Parent.CodeCountry?.IsPartOfEuropeanUnion ?? false)
				{
					EU.ValidateCCP(Parent.OK_CustomsRegNo, Parent.OK_RN_NKCodeCountry, Parent.OK_CustomsRegNoInfo);
				}
			}
		}

		#endregion

		protected virtual void ValidateCarrierCode()
		{
			if (!Regex.IsMatch(Parent.OK_CustomsRegNo, @"^[a-z]{1,4}$", RegexOptions.IgnoreCase))
			{
				Parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(Res.GetString("5f32d321-f972-4f72-a67f-dbaec89fe7a3", "A valid Carrier Code is composed of between 1 and 4 alpha characters (AAAA)."), OrganisationRegistry.RegistrationNumberFormatFields.CarrierCode);
			}
		}

		protected void ValidateCargoWiseOneCarrierCode()
		{
			if (Parent.OK_CodeType == OrgCusCode.CodeTypes.CargoWiseOneCarrierCode)
			{
				if (!Regex.IsMatch(Parent.OK_CustomsRegNo, @"^C1[a-z0-9]{2}$", RegexOptions.IgnoreCase))
				{
					Parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(Res.GetString("0c649e94-47c4-448c-a3db-3be06fc040dc", "A valid {0} Carrier Code is composed of the prefix C1 followed by two alpha-numeric characters.", "CargoWise"), OrganisationRegistry.RegistrationNumberFormatFields.CargoWiseOneCarrierCode);
				}
			}
		}

		void ValidateNorthAmericanIndustryClassificationSystem()
		{
			if (Parent.OK_CodeType == OrgCusCode.CodeTypes.NorthAmericanIndustryClassificationSystem)
			{
				if (!Parent.OK_CustomsRegNo.IsNumbersOnlyOrEmpty || Parent.OK_CustomsRegNo.Length < 2 || Parent.OK_CustomsRegNo.Length > 6)
				{
					Parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(Res.GetString("039D236B-C25C-4B71-A65C-546C05B55F40", "A valid North American Industry Classification System code is composed of 2 to 6 digits"), OrganisationRegistry.RegistrationNumberFormatFields.NAICS);
				}
			}
		}

		void ValidateStandardIndustrialClassification()
		{
			if (Parent.OK_CodeType == OrgCusCode.CodeTypes.StandardIndustrialClassification)
			{
				var provider = OrgCusCodeCountryFactory.GetIOrgCusCodeStandardIndustrialClassificationNoValidationProvider(Parent.OK_RN_NKCodeCountry);

				if (provider != null)
				{
					provider.ValidateStandardIndustrialClassification(Parent);
				}
				else if (!Parent.OK_CustomsRegNo.IsNumbersOnlyOrEmpty || Parent.OK_CustomsRegNo.Length != 4)
				{
					Parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(Res.GetString("CA7369C4-3068-4EEE-B380-5C857E8EB0BF", "A valid Standard Industrial Classification code is composed of 4 digits"), OrganisationRegistry.RegistrationNumberFormatFields.SIC);
				}
			}
		}

		void ValidateTirIdentificationNumberFormat()
		{
			if (Parent.OK_CodeType == OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers)
			{
				if (!IsTirReferenceValid(Parent.OK_CustomsRegNo))
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("C17AE480-CB55-4EB5-B119-EAC377D1EACF", "Invalid TIR Carnet holder ID number format. It should be like AAA/BBB/XX...X where AAA represents a 3 letter code of the country/region where the persons utilizing TIR Carnets are authorized, BBB represents a 3 digit code of the national association through which the holder of the TIR Carnet has been authorized, XX...X represents consecutive numbers(digits) identifying the person authorized to utilize a TIR Carnet (e.g. GBR/022/1234567)"));
				}
			}
		}

		static public bool IsTirReferenceValid(ZString tirReference)
		{
			return Regex.IsMatch(tirReference, @"^[A-Z]{3}\/[0-9]{3}\/[0-9]{1,9}$");
		}

		#region Country Specific

		#region BY

		protected void ValidateCustomsCodeAEOForBY()
		{
			if (Parent.OK_CodeType == OrgCusCode.BelarusCodeTypes.AEO)
			{
				if (!Regex.IsMatch(Parent.OK_CustomsRegNo, "^[0-9]{4}$", RegexOptions.IgnoreCase))
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("43502FE5-F892-4660-8366-030BD89D8A0F", "AEO Codes should consist of 4 numeric characters."));
				}
			}
		}

		#endregion

		#region EU

		void ValidateCodeCountryForEU()
		{
			var parent = Parent;
			var organisation = parent.Organisation;

			if (organisation != null)
			{
				if (parent.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator)
				{
					var eoris = organisation.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
					if (eoris.Any())
					{
						if (parent.OK_RN_NKCodeCountry != eoris[0].OK_RN_NKCodeCountry)
						{
							parent.OK_RN_NKCodeCountryInfo.AddWarning(Res.GetString("499AFBBB-8EBC-4010-8ED7-B0B748928070", "EORI and AEO issuing countries/regions usually are the same, please check."));
						}
					}
				}
				if (parent.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori)
				{
					var existingEoriCustomsCodesCountries = organisation.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori).Select(x => x.OK_RN_NKCodeCountry).Distinct().OrderBy(x => x);
					if (existingEoriCustomsCodesCountries.Count() > 1)
					{
						if (existingEoriCustomsCodesCountries.Count() == 2 && (parent.CountryIsMemberOf(EconomicGroupList.Codes.EuropeanUnion) || parent.OK_RN_NKCodeCountry == CountryCodes.UnitedKingdom))
						{
							var countriesToArray = existingEoriCustomsCodesCountries.ToArray();
							var firstCountryIsGB = IsCountryGB(countriesToArray[0]);
							var firstCountryIsOnlyEU = IsCountryOnlyEU(countriesToArray[0]);
							var secondCountryIsGB = IsCountryGB(countriesToArray[1]);
							var secondCountryIsOnlyEU = IsCountryOnlyEU(countriesToArray[1]);

							if (!(firstCountryIsGB || firstCountryIsOnlyEU) || !(secondCountryIsGB || secondCountryIsOnlyEU) || (firstCountryIsOnlyEU && secondCountryIsOnlyEU))
							{
								parent.OK_RN_NKCodeCountryInfo.AddError(Res.GetString("8243B339-FEB8-4FB9-9743-1C8A60A182A5", "An EU country or GB EORI code can't live with other EORI codes whose countries/regions are not an EU country nor GB."));
							}
						}
						else
						{
							parent.OK_RN_NKCodeCountryInfo.AddError(Res.GetString("2DABA4CA-101C-4375-A091-08B30440FE5C", "You can't have EORI codes set for distinct countries/regions, unless they are one for an EU country and one for GB."));
						}
					}
				}
			}

			ZBool IsCountryGB(ZString countryCode) => countryCode == CountryCodes.UnitedKingdom;
			ZBool IsCountryOnlyEU(ZString countryCode) => !IsCountryGB(countryCode) && (GetRefCountryFormCode(countryCode)?.RN_EconomicGrouping ?? ZString.Empty) == EconomicGroupList.Codes.EuropeanUnion;
			RefCountry GetRefCountryFormCode(ZString countryCode) => parent.Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
		}

		void ValidateCodeTypeForIcs2Members()
		{
			var organisation = Parent.Organisation;
			if (organisation != null)
			{
				if (Parent.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator)
					{
					var aeos = organisation.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator);
					var euAeos = aeos.Where(x => x.CountryIsMemberOf(EconomicGroupList.Codes.EuropeanUnion));
					if (euAeos.Count() > 1)
					{
						Parent.OK_CodeTypeInfo.AddError(Res.GetString("63576DD0-891D-454F-B00F-A03029CDD426", "Only one AEO in EU countries/regions should be entered per Organization."));
					}
				}
			}
		}

		void ValidateCustomsCodeForFriendOfEu()
		{
			if (Parent.OK_CodeType == OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU && Parent.OK_CustomsRegNo.Length != 15)
			{
				Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("5e3b7911-865d-429c-87d4-c8fdfc755e88", "TCUIN should be 15 characters long. You should exclude the country/region prefix."));
			}
		}

		public static class EU
		{
			public const string BlankEoriRegSuffix = "000";
			public const string IETWCodePrefix = "IETW0000";

			public static bool ValidateEoriCode(ZString eoriNumber, ZString countryCodeToCheckAsPrefix, ZPropertyInfo propertyOnWhichToWarn)
			{
				bool result = true;
				if (!countryCodeToCheckAsPrefix.IsEmpty && eoriNumber.StartsWith(countryCodeToCheckAsPrefix))
				{
					propertyOnWhichToWarn.AddMessageError(Res.GetString("7c4fb5c2-b496-4a27-9607-699cdfb31854", "The EORI code does not need to start with the country/region code, this prefix will be added automatically when needed for messaging."));
					result = false;
				}
				result = ValidateEoriLength(eoriNumber, propertyOnWhichToWarn);
				return result;
			}

			public static bool ValidateEoriLength(ZString eoriNumber, ZPropertyInfo propertyOnWhichToWarn)
			{
				bool result = true;
				if (eoriNumber.Length > 15)
				{
					propertyOnWhichToWarn.AddMessageError(Res.GetString("B9350841-F6AD-48A1-8E4F-7FB3680F2A90", "Maximum Length of the EORI code is 15, this excludes the country/region code prefix which is automatically added."));
					result = false;
				}
				return result;
			}

			public static bool ValidateTurnCode(ZString turnNumber, ZString countryCodeToCheckAsPrefix, ZPropertyInfo propertyOnWhichToWarn)
			{
				bool result = true;
				if (turnNumber.StartsWith(countryCodeToCheckAsPrefix))
				{
					propertyOnWhichToWarn.AddMessageError(Res.GetString("7c4fb5c2-b496-4a27-9607-699cdfb31854", "The EORI code does not need to start with the country/region code, this prefix will be added automatically when needed for messaging."));
					result = false;
				}
				if (!turnNumber.EndsWith(BlankEoriRegSuffix))
				{
					propertyOnWhichToWarn.AddWarning(Res.GetString("3c772bd6-48c4-45b5-bb3f-731353635d4a", "The EORI code you have supplied ends with something other than '000'. This code will NOT be sent in boxes 2, 8, or 14, instead the last digits will be replaced with '000' and the suffix sent in box 44. If you wish to supply a registration number that will be sent verbatim, select type='EOR'."));
				}
				return result;
			}

			internal static void ValidateAEO(ZString code, ZString countryCode, ZPropertyInfo zPropertyInfo)
			{
				//format = GBAEOF123456789
				if (code.StartsWith(countryCode))
				{
					zPropertyInfo.AddMessageError(Res.GetString("0913d985-0294-48f5-8dc9-ab4456b5837d", "Certificate must not start with the country/region code. Select the issuing country/region from the drop-down menu."));
				}
				if (!code.StartsWith("AEO"))
				{
					zPropertyInfo.AddMessageError(Res.GetString("9ba0d76b-c81e-4926-a449-fbc1aeb4c5d8", "Certificate must start with 'AEO'. Do not put the country/region code here."));
				}
				if (code.Length < 7)
				{
					zPropertyInfo.AddMessageError(Res.GetString("47fad636-b06a-4c7e-b398-3c85d1e93c1a", "Code is too short"));
				}
				else
				{
					List<string> certificateTypes = new List<string> { "S", "C", "F" };
					if (!certificateTypes.Contains(code.SubstringSafe(3, 1)))
					{
						zPropertyInfo.AddMessageError(Res.GetString("d4b758ec-5f63-4dab-9edb-caf6b3df7488", "Certificate type (character 4) not recognized. Should be 'S', 'F' or 'C'"));
					}
				}
			}

			internal static void ValidateTEN(ZString code, ZPropertyInfo zPropertyInfo)
			{
				// 13characters
				// Format[A - Z]{ 2}[a-zA-Z0-9]{11}
				// 2 char Member State Code and 11 alphanumeric

				var countryCodeRegex = new Regex("^[A-Z][A-Z]");
				if (!countryCodeRegex.IsMatch(code))
				{
					zPropertyInfo.AddMessageError(Res.GetString("9afd5f12-1a99-48c8-a369-459ed980d907", "Number must start with a 2 character Country/Region Code."));
				}

				var numericCodeRegex = new Regex("^[a-zA-Z0-9]{11}");
				if (!numericCodeRegex.IsMatch(code.SubstringSafe(2)))
				{
					zPropertyInfo.AddMessageError(Res.GetString("f71052e4-a7eb-44ce-9990-8514e93f3378", "Number must have 11 alphanumeric characters after the 2 character Country/Region Code."));
				}

				if (code.Length > 13)
				{
					zPropertyInfo.AddMessageError(Res.GetString("8167f7b7-84c9-4abb-aeaa-8f97e23614ad", "Number is too long. Must have 13 Characters."));
				}
			}

			public static void ValidateCCP(ZString code, ZString countryCode, ZPropertyInfo propertyOnWhichToWarn)
			{
				var leadsWithAFExceptions = new ZString[]
				{
					Core.Constants.CountryCodes.Ireland, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.CountryCodes.Spain, Core.Constants.CountryCodes.France
				};
				if (code.Length > 1)
				{
					if (!leadsWithAFExceptions.Contains(countryCode))
					{
						var firstChar = char.ToUpperInvariant(code[0]);
						if (firstChar < 'A' || firstChar > 'F')
						{
							propertyOnWhichToWarn.AddMessageError(Res.GetString("43a0bc5a-306e-4c8f-8ace-3be845431fb5", "The CCP code must start with a character from A to F."));
						}
					}
				}

				var endsWithCountryExceptions = new ZString[]
				{
					Core.Constants.CountryCodes.Spain, Core.Constants.CountryCodes.France
				};

				if (
					!code.EndsWith(countryCode, StringComparison.OrdinalIgnoreCase)
					&& !endsWithCountryExceptions.Contains(countryCode)
					&& !(countryCode == Core.Constants.CountryCodes.Ireland && code.StartsWith(IETWCodePrefix, StringComparison.OrdinalIgnoreCase))
					)
				{
					propertyOnWhichToWarn.AddMessageError(Res.GetString("283a070f-2422-4b1e-bbb3-2b97fcbe8894", "The CCP code must end with the Country/Region of Issue."));
				}

				var fixedLengthOf10 = new ZString[]
				{
					Core.Constants.CountryCodes.Italy
				};
				if (fixedLengthOf10.Contains(countryCode))
				{
					var warehouseNumber = code.Length > 4 ? code.Substring(1, code.Length - 4) : ZString.Empty;
					if (warehouseNumber.Length != 6 || !warehouseNumber.IsNumbersOnlyOrEmpty)
					{
						propertyOnWhichToWarn.AddMessageError(Res.GetString("a059dccc-9f19-4118-bcca-055d5558d633", "Total length of the CCP code must be 10. And the second to the seventh place should be 6 digits."));
					}
					if (!propertyOnWhichToWarn.HasNotifications())
					{
						var actualCheckDigit = code.SubstringSafe(code.Length - 3, 1);
						var expectedCheckDigit = ITCINValidator.CalculateCheckDigitAgainstSixCharLengthString(warehouseNumber);
						if (actualCheckDigit != expectedCheckDigit)
						{
							propertyOnWhichToWarn.AddMessageError(Res.GetString("d018be58-32cf-405e-956f-ea8fafcaf9f7", "The check digit in the CCP Code is incorrect. Current value is: {0}, expected {1}.", actualCheckDigit, expectedCheckDigit));
						}
					}
				}
				else
				{
					var lengthBetween6And17Exceptions = new ZString[]
					{
						Core.Constants.CountryCodes.Spain
					};
					if (code.Length > 17 || code.Length < 6 && !lengthBetween6And17Exceptions.Contains(countryCode))
					{
						propertyOnWhichToWarn.AddMessageError(Res.GetString("d8018c70-6e57-4e00-9926-478d9682ee21", "Total length of the CCP code must be at most 17 and at least 6."));
					}
				}
			}
		}

		#endregion

		#region GB
		void ValidateCustomsCodeForGB()
		{
			var codeType = Parent.OK_CodeType;
			var code = Parent.OK_CustomsRegNo.ToUpper();
			if (codeType == OrgCusCode.CodeTypes.VATCode)
			{
				if (GB.IsApplicableForNorthernIreland(Parent))
				{
					GB.ValidateNorthernIrelandVatCode(code, Parent.OK_CustomsRegNoInfo);
				}
				else
				{
					GB.ValidateVatCode(code, Parent.OK_CustomsRegNoInfo);
				}
			}
			else if (codeType == OrgCusCode.CodeTypes.VGMRegistrationNumber)
			{
				if (!Regex.IsMatch(Parent.OK_CustomsRegNo, @"^[0-9]{4}\/[a-zA-Z]{2}\/(?:0[1-9]|1[0-2])[0-9]{2}$"))
				{
					Parent.OK_CustomsRegNoInfo.AddError(Res.GetString("f583d35f-1184-48d9-b301-133b934f846b", "A valid VGM number for GB has the format 1234/AA/MMYY. Where 1234 is the approval number, AA is the country/region code and MMYY is the expiry date."));
				}

				if (Parent.Header != null && Parent.Header.CustomsCodes.Cast<OrgCusCode>().Count(cusCode =>
						cusCode.CountryIs(Constants.CountryCodes.UnitedKingdom)
						&& cusCode.OK_CodeType == OrgCusCode.CodeTypes.VGMRegistrationNumber
						&& Parent.OK_CustomsRegNo == cusCode.OK_CustomsRegNo) > 1)
				{
					Parent.OK_CustomsRegNoInfo.AddError(Res.GetString("d44954ff-3962-42c5-a4b8-0e47ffb9346e", "This number already exists for a different address."));
				}
			}
		}

		public static class GB
		{
			public static bool ValidateVatCode(ZString vatNumber, ZPropertyInfo propertyOnWhichToWarn)
			{
				bool result = true;
				if (!Regex.IsMatch(vatNumber, @"^\d{9}$"))   // 9 digits
				{
					propertyOnWhichToWarn.AddWarning(Res.GetString("4e9cbc5a-bebc-4966-90a3-f4b7a2f4cb3f", "UK VAT codes should be 9 digits. For EORI numbers, including dummy or special EORI numbers, select code type TRN or EOR."));
					result = false;
				}
				return result;
			}

			public static bool IsApplicableForNorthernIreland(OrgCusCode orgCusCode)
			{
				var closestPort = orgCusCode.Header.OH_RL_NKClosestPort;
				var refUnloco = orgCusCode.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, closestPort);
				return refUnloco != null && refUnloco.IsInNorthernIreland;
			}

			public static bool ValidateNorthernIrelandVatCode(ZString vatNumber, ZPropertyInfo propertyInfo)
			{
				if (!Regex.IsMatch(vatNumber, @"^(XI|)(\d{9}|\d{12}|DG\d{3}|HA\d{3})$"))
				{
					propertyInfo.AddWarning(Res.GetString("a197e0ab-3f0b-40c5-a786-98334c140e4c", "VAT Business Registration Number structures for Northern Ireland are either 'XI999 9999 99', 'XI999 9999 99 999', 'XIDG999', 'XIHA999', '999 9999 99', '999 9999 99 999', 'DG999' or 'HA999'."));
					return false;
				}

				return true;
			}
		}
		#endregion

		#region NZ

		protected void ValidateCustomsCodeForNZ()
		{
			if (Parent.OK_CodeType == OrgCusCode.CodeTypes.CustomsClientCode || Parent.OK_CodeType == OrgCusCode.CodeTypes.SupplierCode)
			{
				string errorMessage = new NZCustomsCodeValidator(Parent.OK_CodeType).GetCasperCodeValidationErrors(Parent.OK_CustomsRegNo);
				if (!string.IsNullOrEmpty(errorMessage))
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(errorMessage);
				}
			}
			else if (Parent.OK_CodeType == OrgCusCode.CodeTypes.ControlledPremisesID)
			{
				new NZCCPValidator(Parent.OK_CustomsRegNoInfo).ValidateCCPAddError();
			}
			else if (Parent.OK_CodeType == OrgCusCode.NZCodeTypes.AEO)
			{
				if (!Regex.IsMatch(Parent.OK_CustomsRegNo, "^[0-9a-z]{4}$", RegexOptions.IgnoreCase))
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("a63f8afc-f113-4969-96bd-d8d20edb5138", "NZ AEO number should consist of 4 alphanumeric characters."));
				}
			}
		}

		#endregion

		#region SG

		protected void ValidateCRNForSG()
		{
			if (Parent.OK_CodeType == OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber)
			{
				Parent.OK_CustomsRegNoInfo.AddWarning(
					Res.GetString("1ad06d5e-f77a-4878-811c-27c4514e4418", @"CRN is no longer in use, please enter a UEN number.

CRN should be entered with the Type UEN in the following cases:
  Transit (99991000000G)
  Others (99999990000C)
  Personal Effect (99999000000N)
  SAF Camps (Existing CR No)"));
			}
		}

		protected void ValidateQCIForSG()
		{
			if (Parent.OK_CodeType == OrgCusCode.SingaporeCodeTypes.QualifiedCompanyIdentificationCode)
			{
				if (Parent.OK_CustomsRegNo.Length != 3)
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("B45935CB - 89EF - 43E2 - B01E - 4EF8B513FBCD", "A valid QCI registration number / code must contain 3 characters."));
				}
			}
		}

		protected void ValidatePSTForSG()
		{
			if (Parent.OK_CodeType == OrgCusCode.SingaporeCodeTypes.PartyStatusType)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.OK_CustomsRegNoInfo);
			}
		}

		protected void ValidateIBGForSG()
		{
			if (Parent.OK_CodeType == OrgCusCode.SingaporeCodeTypes.InterbankGIRO)
			{
				if (Parent.OK_CustomsRegNo.Length > 30)
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("dad82428-b3e7-414f-97ae-ac021750842a", "A valid Interbank GIRO code must contain no more than 30 characters."));
				}
				else if (!Parent.OK_CustomsRegNo.IsLettersAndNumbersOnlyOrEmpty)
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("67eda029-26f4-49de-b809-3bac649414c1", "The Interbank GIRO code contains invalid characters."));
				}
			}
		}

		protected void ValidateDIRForSG()
		{
			if (Parent.OK_CodeType == OrgCusCode.SingaporeCodeTypes.DirectDelivery)
			{
				if (!Parent.OK_CustomsRegNo.IsEmpty && Parent.OK_CustomsRegNo != "Y")
				{
					Parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("a62231b6-4f3e-4a98-ab2f-f82f421a8bc2", "Any value will enable Direct Delivery. Remove the entry to disable."));
				}
			}
		}

		protected void ValidateAEOForSG()
		{
			if (Parent.OK_CodeType == OrgCusCode.SingaporeCodeTypes.AEO)
			{
				if (!Regex.IsMatch(Parent.OK_CustomsRegNo, "^[0-9a-z]{12}$", RegexOptions.IgnoreCase))
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("9eee7d6c-ce26-4264-af36-41bdba639e7f", "SG AEO number should consist of 12 alphanumeric characters."));
				}
			}
		}

		protected void ValidateUENForSG()
		{
			if (Parent.OK_CodeType == OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber)
			{
				bool isUENMatch = Regex.IsMatch(Parent.OK_CustomsRegNo, "^[0-9]{8}[a-zA-Z]$")
					|| Regex.IsMatch(Parent.OK_CustomsRegNo, "^[0-9]{9}[a-zA-Z]$")
					|| Regex.IsMatch(Parent.OK_CustomsRegNo, "^[S-T]{1}[0-9]{2}[a-zA-Z]{2}[0-9]{4}[a-zA-Z]$");

				if (!isUENMatch)
				{
					bool isCRNMatch = Regex.IsMatch(Parent.OK_CustomsRegNo, "^[a-zA-Z0-9][0-9]{10}[a-zA-Z]{1}$");

					if (!isCRNMatch)
					{
						Parent.OK_CustomsRegNoInfo.AddWarning(UENInvalidFormat);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Specific Technical Formatting")]
		public const string UENInvalidFormat = @"Format of Unique Entity No (UEN) may not be correct.

Valid UEN formats are; nnnnnnnnX (9 characters), yyyynnnnnX (10 characters) or TyyPQnnnnX (10 characters).
(yy(yy) = year, PQ = type of entity, n = numeric, X = check alphabet).

In the case that the CRN number is used in lieu of the UEN, the following numbers are valid;
Transit (99991000000G)
Others (99999990000C)
Personal Effect (99999000000N)
SAF Camps (Existing CR No).
The CRN number for SAF Camps should be formatted as follows xnnnnnnnnna (x = alpha numeric, n = numeric, a = alpha)";

		#endregion

		#region IS

		protected void ValidateCustomsCodeForIS()
		{
			if (Parent.OK_CodeType == OrgCusCode.IcelandCodeTypes.CustomsOfficeCode && !Parent.OK_CustomsRegNo.IsLettersOnlyOrEmpty)
			{
				string errorMessage = Res.GetString("2cd3fe8d-40c7-4c0b-ab03-1692816b7642", "Only alpha symbols are allowed.");
				Parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(errorMessage, OrganisationRegistry.RegistrationNumberFormatFields.ISCOC);
			}
		}

		#endregion

		#region CA

		protected void ValidateCustomsCodeForCA()
		{
			switch (Parent.OK_CodeType)
			{
				case OrgCusCode.CodeTypes.CarrierCode:
					ValidateCACarrierCode();
					break;
				case OrgCusCode.CACodeTypes.AuthorizationID:
					ValidateCAAuthorizationID();
					break;
				case OrgCusCode.CACodeTypes.ExportLicenceNumber:
					ValidateCAExportLicenceNumber();
					break;
				case OrgCusCode.CACodeTypes.BusinessNumberForCorporateIncomeTax:
					ValidateBusinessNumberForCorporateIncomeTax();
					break;
				case OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax:
					ValidateBusinessNumberForGoodsServicesHarmonizedSalesTax();
					break;
				case OrgCusCode.CACodeTypes.BusinessNumberForImportExport:
					ValidateBusinessNumberForImportExport();
					break;
				case OrgCusCode.CACodeTypes.BusinessNumberCustomsBroker:
					ValidateBusinessNumberCustomsBroker();
					break;
				case OrgCusCode.CACodeTypes.BusinessNumberImporterNonCommercial:
					ValidateBusinessNumberImporterNonCommercial();
					break;
				case OrgCusCode.CACodeTypes.BusinessNumberForExport:
					ValidateBusinessNumberForExport();
					break;
				case OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments:
					ValidateBusinessNumberForLowValueShipments();
					break;
				case OrgCusCode.CACodeTypes.BusinessNumberForPayrollDeductions:
					ValidateBusinessNumberForPayrollDeductions();
					break;
				case OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial:
					ValidateBusinessNumberImporterCommercial();
					break;
				case OrgCusCode.CACodeTypes.WorldManufacturerIdentifier:
					ValidateWorldManufacturerIdentifier();
					break;
			}
		}

		/// <summary>
		/// The Carrier Code is composed of 4 alphanumeric characters.
		/// </summary>
		void ValidateCACarrierCode()
		{
			if (Parent.Header != null && Parent.Header.OH_IsShippingProvider)
			{
				ZString errorMessage = CanadianCustomsCodeValidator.GetCarrierCodeError(Parent.OK_CustomsRegNo);
				if (!errorMessage.IsEmpty)
				{
					Parent.OK_CustomsRegNoInfo.AddWarning(errorMessage);
				}
			}
		}

		/// <summary>
		/// The Authorization number is composed of 2 alpha/4 numeric digits e.g. SC1234
		/// </summary>
		protected void ValidateCAAuthorizationID()
		{
			ZString warningMessage = CanadianCustomsCodeValidator.GetAuthorizationIDError(Parent.OK_CustomsRegNo);
			if (!warningMessage.IsEmpty)
			{
				Parent.OK_CustomsRegNoInfo.AddWarning(warningMessage);
			}
		}

		/// <summary>
		/// The Export Licence number is composed of 6 alpha
		/// </summary>
		protected void ValidateCAExportLicenceNumber()
		{
			ZString messageError = CanadianCustomsCodeValidator.GetExportLicenceNumberError(Parent.OK_CustomsRegNo);
			if (!messageError.IsEmpty)
			{
				Parent.OK_CustomsRegNoInfo.AddMessageError(messageError);
			}
		}
		/// <summary>
		/// The Business Number (BN) consists of two parts: the registration number and the account identifier. The entire number has 15 characters:
		/// - Nine digits to identify the business; and
		/// - The two letters 'RP' and Four digits to identify each account a business may have.
		/// </summary>
		protected void ValidateBusinessNumberForPayrollDeductions()
		{
			ZString warningMessage = CanadianCustomsCodeValidator.GetBusinessNumberForPayrollDeductionsError(Parent.OK_CustomsRegNo);
			if (!warningMessage.IsEmpty)
			{
				Parent.OK_CustomsRegNoInfo.AddWarning(warningMessage);
			}
		}

		/// <summary>
		/// The Business Number (BN) consists of two parts: the registration number and the account identifier. The entire number has 15 characters:
		/// - Nine digits to identify the business; and
		/// - The two letters 'RM' and Four digits to identify each account a business may have.
		/// </summary>
		protected void ValidateBusinessNumberForImportExport()
		{
			ZString warningMessage = CanadianCustomsCodeValidator.GetBusinessNumberForImportExportError(Parent.OK_CustomsRegNo);
			if (!warningMessage.IsEmpty)
			{
				Parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(warningMessage, OrganisationRegistry.RegistrationNumberFormatFields.CABRM);
			}
		}

		/// <summary>
		/// The Business Number (BN) consists of two parts: the registration number and the account identifier. The entire number has 15 characters:
		/// - Nine digits to identify the business; and
		/// - The two letters 'RM' and Four digits to identify each account a business may have.
		/// </summary>
		protected void ValidateBusinessNumberCustomsBroker()
		{
			var warningMessage = CanadianCustomsCodeValidator.GetBusinessNumberCustomsBrokerError(Parent.OK_CustomsRegNo);
			if (!warningMessage.IsEmpty)
			{
				Parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(warningMessage, OrganisationRegistry.RegistrationNumberFormatFields.CABRB);
			}
		}

		/// <summary>
		/// The Business Number (BN) consists of two parts: the registration number and the account identifier. The entire number has 15 characters:
		/// - Nine digits to identify the business; and
		/// - The two letters 'RM' and Four digits to identify each account a business may have.
		/// </summary>
		protected void ValidateBusinessNumberImporterNonCommercial()
		{
			var warningMessage = CanadianCustomsCodeValidator.GetBusinessNumberImporterNonCommercialError(Parent.OK_CustomsRegNo);
			if (!warningMessage.IsEmpty)
			{
				Parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(warningMessage, OrganisationRegistry.RegistrationNumberFormatFields.CABNC);
			}
		}

		/// <summary>
		/// The Business Number (BN) consists of two parts: the registration number and the account identifier. The entire number has 15 characters:
		/// - Nine digits to identify the business; and
		/// - The two letters 'RM' and Four digits to identify each account a business may have.
		/// </summary>
		protected void ValidateBusinessNumberForExport()
		{
			ZString warningMessage = CanadianCustomsCodeValidator.GetBusinessNumberForExportError(Parent.OK_CustomsRegNo);
			if (!warningMessage.IsEmpty)
			{
				Parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(warningMessage, OrganisationRegistry.RegistrationNumberFormatFields.CABRE);
			}
		}

		/// <summary>
		/// The Business Number (BN) consists of two parts: the registration number and the account identifier. The entire number has 15 characters:
		/// - Nine digits to identify the business; and
		/// - The two letters 'RM' and Four digits to identify each account a business may have.
		/// </summary>
		protected void ValidateBusinessNumberForLowValueShipments()
		{
			var warningMessage = CanadianCustomsCodeValidator.GetBusinessNumberForLowValueShipmentsError(Parent.OK_CustomsRegNo);
			if (!warningMessage.IsEmpty)
			{
				Parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(warningMessage, OrganisationRegistry.RegistrationNumberFormatFields.CABRL);
			}
		}

		/// <summary>
		/// The Business Number (BN) consists of two parts: the registration number and the account identifier. The entire number has 15 characters:
		/// - Nine digits to identify the business; and
		/// - The two letters 'RT' and Four digits to identify each account a business may have.
		/// </summary>
		protected void ValidateBusinessNumberForGoodsServicesHarmonizedSalesTax()
		{
			ZString warningMessage = CanadianCustomsCodeValidator.GetBusinessNumberForGoodsServicesHarmonizedSalesTaxError(Parent.OK_CustomsRegNo);
			if (!warningMessage.IsEmpty)
			{
				Parent.OK_CustomsRegNoInfo.AddWarning(warningMessage);
			}
		}

		/// <summary>
		/// The Business Number (BN) consists of two parts: the registration number and the account identifier. The entire number has 15 characters:
		/// - Nine digits to identify the business; and
		/// - The two letters 'RC' and Four digits to identify each account a business may have.
		/// </summary>
		protected void ValidateBusinessNumberForCorporateIncomeTax()
		{
			ZString warningMessage = CanadianCustomsCodeValidator.GetBusinessNumberForCorporateIncomeTaxError(Parent.OK_CustomsRegNo);
			if (!warningMessage.IsEmpty)
			{
				Parent.OK_CustomsRegNoInfo.AddWarning(warningMessage);
			}
		}

		/// <summary>
		///  The Importer Number number is composed of 9 alphanumeric characters.
		/// </summary>
		void ValidateBusinessNumberImporterCommercial()
		{
			var warningMessage = CanadianCustomsCodeValidator.GetBusinessNumberImporterCommercialError(Parent.OK_CustomsRegNo);
			if (!warningMessage.IsEmpty)
			{
				Parent.OK_CustomsRegNoInfo.AddWarning(warningMessage);
			}
		}

		/// <summary>
		///  The World Manufacturer Identifie should be no longer than 6 characters.
		/// </summary>
		void ValidateWorldManufacturerIdentifier()
		{
			var warningMessage = CanadianCustomsCodeValidator.GetWorldManufacturerIdentifierError(Parent.OK_CustomsRegNo);
			if (!warningMessage.IsEmpty)
			{
				Parent.OK_CustomsRegNoInfo.AddWarning(warningMessage);
			}
		}

		#endregion

		#region AO

		protected void ValidateCustomsCodeForAO()
		{
			if (Parent.OK_CodeType == OrgCusCode.AngolaCodeTypes.NumeroDeIdentificacioFiscal)
			{
				if (Parent.OK_CustomsRegNo.Length != 10)
				{
					Parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(Res.GetString("5e295ce3-fa8a-4fda-9cab-1414f8a4224b", "NIF length must be 10 characters in the following format ##########."), OrganisationRegistry.RegistrationNumberFormatFields.AONIF);
				}
				else if (!Regex.IsMatch(Parent.OK_CustomsRegNo, "^[0-9]{10}$"))
				{
					Parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("b974db1d-36cc-4953-b5a3-bfd083414852", "Format of NIF / Tax Identification Number (NIF) is not correct. It should contain 10 digits, e.g: '1234567890'."));
				}
			}
		}

		#endregion

		#region NL

		protected void ValidateCustomsCodeForNL()
		{
			if (Parent.OK_CodeType == OrgCusCode.NetherlandsCodeTypes.ChamberOfCommerceNumber)
			{
				if (Parent.OK_CustomsRegNo.Length != 8)
				{
					Parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(Res.GetString("4e6bd87f-a341-4360-a0cc-a851aa2a2e5b", "CCN length must be 8 characters in the following format ########."), OrganisationRegistry.RegistrationNumberFormatFields.NLCCN);
				}
				else if (!Regex.IsMatch(Parent.OK_CustomsRegNo, "^[0-9]{8}$"))
				{
					Parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("28964c8e-42a8-4b18-9a59-02bc5e97a447", "Format of Chamber of Commerce (CCN) is not correct. It should contain 8 digits, e.g: '12345678'."));
				}
			}
			else if (Parent.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.BTW)
			{
				if (!Regex.IsMatch(Parent.OK_CustomsRegNo, @"^(NL|)[A-Za-z0-9+*]{12}$"))
				{
					Parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("37a7b010-bfa2-40a6-84f3-2133150776e7", "VAT Business Registration Number structures for Netherlands are either 'NLXXXXXXXXXXXX' or 'XXXXXXXXXXXX'."));
				}
			}
		}

		#endregion

		#region DK

		void ValidateCustomsCodeForDK()
		{
			if (Parent.OK_CodeType == OrgCusCode.DenmarkCodeTypes.ProductionNumber)
			{
				if (Parent.OK_CustomsRegNo.Length != 10 || !Parent.OK_CustomsRegNo.IsNumbersOnlyOrEmpty)
				{
					Parent.OK_CustomsRegNoInfo.AddErrorIfEnforced(Res.GetString("4f25061f-111c-436e-a135-5a50439026ed", "Production Number should consist of 10 digits."), OrganisationRegistry.RegistrationNumberFormatFields.DKPNR);
				}
			}
			else if (Parent.OK_CodeType == OrgCusCode.CodeTypes.VATCode)
			{
				if (!Regex.IsMatch(Parent.OK_CustomsRegNo, @"^(DK|)\d{8}$"))
				{
					Parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("fff67ff1-07e0-4702-a3a0-8c678b4e6c4c", "VAT Business Registration Number structures for Denmark are either 'DK99999999' or '99999999'."));
				}
			}
		}

		#endregion

		#region JP

		void ValidateCustomsCodeForJP()
		{
			if (Parent.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator)
			{
				if (Parent.OK_CustomsRegNo.Length != 17)
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("{B751553B-C2C4-43CA-A68B-FF66BF504439", "AEO Code should consist of 17 characters."));
				}
			}
			else if (Parent.OK_CodeType == OrgCusCode.CodeTypes.CarrierCode)
			{
				var code = Parent.OK_CustomsRegNo;
				if (code.KeepAlphanumericCharacters() != code || code.Length > 4 || code.Length < 3)
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("FF31BAEC-07A3-41F8-A36F-7CBC6075AA96", "The Carrier Code should be 3 or 4 Alphanumeric Characters"));
				}
			}
			else if (Parent.OK_CodeType == OrgCusCode.CodeTypes.CustomsClientCode)
			{
				var code = Parent.OK_CustomsRegNo;
				if (code.KeepAlphanumericCharacters() != code || code.Length != 5)
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("8D84A245-6132-4806-BC6B-2C87A4D2696B", "The Customs Client Code should be 5 Alphanumeric Characters"));
				}
			}
		}

		#endregion

		#region CN

		void ValidateCodeTypeForCN()
		{
			if (Parent.Header != null)
			{
				bool hasBST, hasVAT, hasVAG, hasVAS;
				hasBST = hasVAT = hasVAG = hasVAS = false;

				foreach (OrgCusCode cusCode in Parent.Header.CustomsCodes)
				{
					if (!cusCode.CountryIs(Constants.CountryCodes.China))
					{
						continue;
					}

					switch (cusCode.OK_CodeType)
					{
						case OrgCusCode.ChinaCodeTypes.BST:
							hasBST = true;
							break;

						case OrgCusCode.CodeTypes.VATCode:
							hasVAT = true;
							break;

						case OrgCusCode.ChinaCodeTypes.VAG:
							hasVAG = true;
							break;

						case OrgCusCode.ChinaCodeTypes.VAS:
							hasVAS = true;
							break;
					}
				}

				if (hasBST && hasVAT)
				{
					Parent.OK_CodeTypeInfo.AddErrorIfEnforced(Res.GetString("6111ec37-242b-4535-af13-c4f0e605c163", "Either VAT or BST code can be entered, but not both."), OrganisationRegistry.RegistrationNumberFormatFields.CNBSTVAT);
				}

				if (hasVAG && hasVAS)
				{
					Parent.OK_CodeTypeInfo.AddErrorIfEnforced(Res.GetString("c0214965-925f-4f36-b9c0-d7610c281eec", "Either VAG or VAS code can be entered, but not both."), OrganisationRegistry.RegistrationNumberFormatFields.CNVAGVAS);
				}

				if (hasVAG && !hasVAT)
				{
					Parent.OK_CodeTypeInfo.AddError(Res.GetString("d095e0d2-7182-4de4-9392-e045011a8bbc", "VAG should be used with VAT together."));
				}

				if (hasVAS && !hasVAT)
				{
					Parent.OK_CodeTypeInfo.AddError(Res.GetString("759c82ba-c09d-4d5c-b8aa-e3674d7927ba", "VAS should be used with VAT together."));
				}
			}
		}

		void ValidateCustomsCodeForCN()
		{
			var regNo = Parent.OK_CustomsRegNo;

			switch (Parent.OK_CodeType)
			{
				case OrgCusCode.ChinaCodeTypes.AEO:
					if (!Regex.IsMatch(regNo, "^[0-9]{10}$"))
					{
						Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("4ee8d86f-b322-4358-af19-926cc29d9695", "CN AEO number should consist of 10 digits."));
					}
					break;
				case OrgCusCode.ChinaCodeTypes.MMR:
				case OrgCusCode.ChinaCodeTypes.SMR:
					if (regNo.Length > 18)
					{
						Parent.OK_CustomsRegNoInfo.AddError(Res.GetString("77B1D2C4-D19A-43C6-8CA8-9AC6E46790C5", "The maximum length of Meat Or Seafood Manufacturer Registration Number is 18."));
					}
					break;
			}
		}

		#endregion

		#region HK

		protected void ValidateCustomsCodeForHK()
		{
			if (Parent.OK_CodeType == OrgCusCode.CodeTypes.VGMRegistrationNumber)
			{
				if (!Regex.IsMatch(Parent.OK_CustomsRegNo, "^GMV[0-9]{9}$"))
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("273333cb-feb4-4a0c-9150-d1494b45f60e", "VGM Registration number for HK should start with \"GMV\" following by a set of numbers."));
				}
			}
			else if (Parent.OK_CodeType == OrgCusCode.HKCodeTypes.AEO)
			{
				if (!Regex.IsMatch(Parent.OK_CustomsRegNo, "^[0-9a-z]{10}$", RegexOptions.IgnoreCase))
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("318e1c8e-560c-48d9-a262-3b726db4f106", "HK AEO number should consist of 10 alphanumeric characters."));
				}
			}
		}

		#endregion

		#region IL

		void ValidateCustomsCodeForIL()
		{
			if (Parent.OK_CodeType == OrgCusCode.SingaporeCodeTypes.AEO)
			{
				if (!Regex.IsMatch(Parent.OK_CustomsRegNo, "^[0-9a-z]{9}$", RegexOptions.IgnoreCase))
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("ea6b5729-9ae6-4fea-a3a2-99f117909fe3", "IL AEO number should consist of 9 alphanumeric characters."));
				}
			}
		}

		#endregion

		#endregion

		#region Misc Helpers

		void ValidateWorldCargoAssociationNumber()
		{
			if (Parent.OK_CodeType == OrgCusCode.CodeTypes.WorldCargoAssociationNumber)
			{
				if (!Regex.IsMatch(Parent.OK_CustomsRegNo, @"^[0-9]{5,6}$"))
				{
					Parent.OK_CustomsRegNoInfo.AddError(Res.GetString("420e1bd0-277f-4571-aa55-0d0c8ee9bd52", "A valid World Cargo Association number is composed of 5 or 6 digits."));
				}
			}
		}

		void ValidateCusCodeMeetCannotCoexistsRule()
		{
			var listOfCodesCannotCoexistCountrySpecific = GetCodesCannotCoexist();
			if (listOfCodesCannotCoexistCountrySpecific.Length > 0)
			{
				foreach (var codesCannotCoexistCountrySpecific in listOfCodesCannotCoexistCountrySpecific)
				{
					var parentCodeType = Parent.OK_CodeType;
					if (codesCannotCoexistCountrySpecific.Contains(parentCodeType) &&
						Parent.Header != null &&
						Parent.Header.CustomsCodes.Cast<OrgCusCode>().Any(x => x != Parent
																		&& x.OK_RN_NKCodeCountry == Parent.OK_RN_NKCodeCountry
																		&& codesCannotCoexistCountrySpecific.Contains(x.OK_CodeType)
																		&& x.OK_CodeType != parentCodeType))
					{
						string errorMessage = Res.GetString("20FD06E5-1315-4C30-86EE-947F6441F9E1", "The following ({0}) Registration codes cannot coexist: {1}", Parent.OK_RN_NKCodeCountry, string.Join(", ", codesCannotCoexistCountrySpecific));
						Parent.OK_CodeTypeInfo.AddError(errorMessage);
					}
				}
			}
		}

		public HashSet<string>[] GetCodesCannotCoexistInParentCountryExposedForInternalDisplay() => GetCodesCannotCoexist();

		protected virtual HashSet<string>[] GetCodesCannotCoexist()
		{
			var mainTypes = OrgCusCodeCountryFactory.GetIOrgCusCodeUniqueValidation(Parent.OK_RN_NKCodeCountry)?.GetCodesCannotCoexist();
			if (mainTypes != null)
			{
				return mainTypes;
			}

			#region Instead of relying on below block please implement IOrgCusCodeUniqueValidation.GetCodesCannotCoexist() for the specific country and use OrgCusCodeCountryFactory to instantiate a country specific file.
			switch (Parent.OK_RN_NKCodeCountry)
			{
				case Constants.CountryCodes.Indonesia:
					return new[] { new HashSet<string>() { OrgCusCode.IndonesiaCodeTypes.PP2, OrgCusCode.IndonesiaCodeTypes.PP3 } };
				case Constants.CountryCodes.Spain:
					return new[] { new HashSet<string>() { OrgCusCode.SpainCodeTypes.NIF, OrgCusCode.SpainCodeTypes.DNI, OrgCusCode.SpainCodeTypes.IGC } };
				case Constants.CountryCodes.Germany:
					return new[] { new HashSet<string>() { GermanyOrgCusCodeInfo.OrgCusCodes.EMCSParticipantIdentificationNumber, GermanyOrgCusCodeInfo.OrgCusCodes.EMCSWarehouseParticipantIdentificationNumber } };
				default:
					return new[] { new HashSet<string>() };
			}
			#endregion
		}

		void ValidateCusCodeMeetDependencyRule()
		{
			if (Parent.Header != null)
			{
				var dependencies = OrgCusCodeCountryFactory.GetIOrgCusCodeDependencyProvider(Parent.OK_RN_NKCodeCountry)?.GetOrgCusCodeDependencies(Parent);
				if (dependencies != null)
				{
					var unsatisfiedDependencies = dependencies.Where(x => Parent.Header.CustomsCodes.GetOrgCusCodesForCodeAndCountry(x.ParentCode, Parent.OK_RN_NKCodeCountry)?.Length == 0);
					foreach (var dependency in unsatisfiedDependencies)
					{
						Parent.OK_CodeTypeInfo.AddError(dependency.ErrorMessage);
					}

					return;
				}
			}

			if (GetCodesCannotCoexist().Length > 0)
			{
				if (Parent.Header != null)
				{
					var parentCodeType = Parent.OK_CodeType;
					var codeDependencyForParent = GetCodeDependencyInParentCountry().Where(x => x.Item1 == parentCodeType);
					var dependencyRuleList = codeDependencyForParent.Where(t => Parent.Header.CustomsCodes.Cast<OrgCusCode>().Any(x => t.Item1 == x.OK_CodeType)).Distinct().ToList();
					var parentCodeList = dependencyRuleList.Where(t => !Parent.Header.CustomsCodes.Cast<OrgCusCode>().Any(x => t.Item2 == x.OK_CodeType)).Select(t => t.Item1).ToList();

					if (parentCodeList.Any(x => x == parentCodeType))
					{
						Parent.OK_CodeTypeInfo.AddError(Res.GetString("e8fd1404-59f2-4e47-9767-76ecb8f62dd4", "Registration code(s) {0} must exist to add Registration code {1}", string.Join(", ", codeDependencyForParent.Select(x => x.Item2).ToArray()), parentCodeType));
					}
				}
			}
		}

		List<Tuple<string, string>> GetCodeDependencyInParentCountry()
		{
			var codelists = new List<Tuple<string, string>>();
			switch (Parent.OK_RN_NKCodeCountry)
			{
				case Constants.CountryCodes.Indonesia:
					codelists.Add(new Tuple<string, string>(OrgCusCode.IndonesiaCodeTypes.PP2, OrgCusCode.IndonesiaCodeTypes.PPN));
					codelists.Add(new Tuple<string, string>(OrgCusCode.IndonesiaCodeTypes.PP3, OrgCusCode.IndonesiaCodeTypes.PPN));
					break;
				default:
					break;
			}
			return codelists;
		}

		protected void ValidateCusCodeIsUnique()
		{
			var parent = Parent;
			var orgHeader = parent.Header;
			if (orgHeader != null)
			{
				if (ValidateCargoWiseOneCarrierCodeIsUnique()
					|| ValidateDomesticCarrierCodeIsUnique()
					|| ValidateJNPIsUnique()
					|| ValidateCargoWiseRoadTransportProviderCodeIsUnique())
				{
					return;
				}

				var codeType = parent.OK_CodeType;
				var codeTypeInfo = parent.OK_CodeTypeInfo;
				var premisesAddress = parent.OK_OA_PremisesAddress;
				var customsRegNo = parent.OK_CustomsRegNo;

				if (codeType == OrgCusCode.CodeTypes.DataUniversalNumberingSystem)
				{
					if (orgHeader.CustomsCodes.Cast<OrgCusCode>().Any(x => x != parent && x.OK_CodeType == codeType &&
						x.OK_OA_PremisesAddress == premisesAddress))
					{
						codeTypeInfo.AddError(Res.GetString("5dbda6c4-5848-4e2f-bde0-20efabe7f1f4", "Only one DUN code can be issued per address."));
					}
					return;
				}

				if (codeType == OrgCusCode.CodeTypes.BoleroTitleRegisterID)
				{
					if (orgHeader.CustomsCodes.Cast<OrgCusCode>().Any(x =>
						x != parent
						&& x.OK_CodeType == codeType
						&& x.OK_CustomsRegNo.EqualsIgnoringCase(customsRegNo)
						&& x.OK_OA_PremisesAddress != premisesAddress))
					{
						codeTypeInfo.AddError(Res.GetString("8CEB333F-5E00-45A7-8A69-A3184016BFC0", "TRI Code {0} can only be registered once.", customsRegNo.ToUpper()));
						return;
					}
					if (orgHeader.CustomsCodes.Cast<OrgCusCode>().Any(x =>
						x != parent
						&& x.OK_CodeType == codeType
						&& x.OK_OA_PremisesAddress == premisesAddress))
					{
						codeTypeInfo.AddError(Res.GetString("A671B8D4-5105-485E-9E20-0EA2C70FC762", "Only one code of this type can be registered per Address."));
						return;
					}
					return;
				}

				if (codeType == OrgCusCode.CodeTypes.ContainerChainCommunityCode)
				{
					ValidateContainerChainCommunityCodeAddress();
					return;
				}

				var oneCodeOfTypeMessage = Res.GetString("029FD264-A192-4a21-BA5B-0BB084E2FB98", "There can only be one code of this type for each country/region.");
				foreach (var cusCode in orgHeader.CustomsCodes.Cast<OrgCusCode>().Where(x => x != parent && x.OK_CodeType == codeType && x.OK_RN_NKCodeCountry == parent.OK_RN_NKCodeCountry))
				{
					if (codeType == GermanyOrgCusCodeInfo.OrgCusCodes.IMA)
					{
						codeTypeInfo.AddMessageError(GermanValidationHelper.OnlyOneRecordAllowedCountryAndTypeMessage(parent));
						break;
					}

					if (cusCode.OK_CodeType == Country.GetConsumptionTaxRegistrationOrgCusCode(cusCode.OK_RN_NKCodeCountry))
					{
						codeTypeInfo.AddError(oneCodeOfTypeMessage);
						break;
					}
					else
					{
						var recordWithErrorFound = false;
						if (parent.PremisesAddressIsAllowed)
						{
							if (cusCode.OK_OA_PremisesAddress == premisesAddress)
							{
								codeTypeInfo.AddError(Res.GetString("5e92092d-36df-4209-9b10-2ef24aefe6a5", "Each code of this type must have a different country/region and address combination."));
								recordWithErrorFound = true;
							}
						}
						else
						{
							codeTypeInfo.AddError(oneCodeOfTypeMessage);
							recordWithErrorFound = true;
						}

						if (parent.RequiresUniqueRegistrationNumber && string.Compare(customsRegNo, cusCode.OK_CustomsRegNo, StringComparison.OrdinalIgnoreCase) == 0)
						{
							codeTypeInfo.AddError(Res.GetString("38FF0D15-8477-4D0F-AE57-DFF7D3FD96D6", "Each code of this type must have a different country/region and registration number combination."));
							recordWithErrorFound = true;
						}

						if (recordWithErrorFound)
						{
							break;
						}
					}
				}
			}
		}

		void ValidateContainerChainCommunityCodeAddress()
		{
			var addressAlreadyHasCommunityCodeQuery = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.ContainerChainCommunityCode);
			addressAlreadyHasCommunityCodeQuery.AddToFilter(OrgCusCodeSchema.OK_OA_PremisesAddress, Parent.OK_OA_PremisesAddress);
			addressAlreadyHasCommunityCodeQuery.AddToFilter(OrgCusCodeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			if (Parent.Factory.Exists(typeof(OrgCusCode), addressAlreadyHasCommunityCodeQuery))
			{
				Parent.OK_CodeTypeInfo.AddError(Res.GetString("459b6c95-2ea9-48bf-9aa6-d5fafe3bd96e", "Only one Container Chain community code can be issued per address."));
			}
		}

		void ValidateContainerChainCommunityCodeRegistrationNumber()
		{
			if (Parent.OK_CodeType == OrgCusCode.CodeTypes.ContainerChainCommunityCode)
			{
				ValidateDuplicate(Parent, Parent.OK_RN_NKCodeCountry, (duplicateCodes) =>
				{
					var duplicate = duplicateCodes
						.Select(x => x.Header)
						.FirstOrDefault(header => header?.OH_IsActive ?? false);
					return duplicate == null ? string.Empty : Res.GetString("e98fe989-27b3-478f-884f-c4292b88242c", "This Container Chain community code is already registered for the organization {0} ({1})", duplicate.OH_Code, duplicate.OH_FullName);
				});
			}
		}

		protected void ValidateEBSForFRPremisesAddress()
		{
			var parent = Parent;

			if (parent.OK_RN_NKCodeCountry == CountryCodes.France && parent.Organisation != null && parent.OK_CodeType == OrgCusCode.FranceCodeTypes.EoriBranchSuffix)
			{
				var address = parent.PremisesAddress;
				if (address != null && !address.IsEUCustomsAddress)
				{
					parent.OK_OA_PremisesAddressInfo.AddMessageError(Res.GetString("57881B59-1AB0-445C-89E3-75D5E1995A03", "Only addresses flagged as EU customs address can be associated to EBS."));
				}
			}
		}

		protected void ValidateEORICode()
		{
			var parent = Parent;
			if (parent.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori)
			{
				if (parent.Header != null)
				{
					var codeList = parent.Header.CustomsCodes.Cast<OrgCusCode>().Where(x => x != parent && x.OK_CodeType == parent.OK_CodeType);

					if (codeList.Any(x => x.OK_CustomsRegNo == parent.OK_CustomsRegNo))
					{
						parent.OK_CustomsRegNoInfo.AddError(Res.GetString("44A3A0D3-C956-4B54-A09B-FC8C9FE68F57", "This Registration Number is already in use by Organization Customs Code of this type."));
					}
				}
			}
		}

		protected void ValidateUKMCode()
		{
			var parent = Parent;
			if (parent.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.UKInternalMarketSchemeCode)
			{
				if (parent.Header != null)
				{
					var codeList = parent.Header.CustomsCodes.Cast<OrgCusCode>().Where(x => x != parent && x.OK_CodeType == parent.OK_CodeType);

					if (codeList.Any(x => x.OK_CustomsRegNo == parent.OK_CustomsRegNo))
					{
						parent.OK_CustomsRegNoInfo.AddError(Res.GetString("45A3A0D3-C956-4B54-A09B-FC8C9FE68F57", "This Registration Number is already in use by Organization Customs Code of this type."));
					}
				}
			}
		}

		bool ValidateCargoWiseOneCarrierCodeIsUnique()
		{
			if (IsDuplicateOf(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode))
			{
				Parent.OK_CodeTypeInfo.AddError(Res.GetString("adafcffb-3caf-4094-bc24-3bd48cd3fdc7", "Only one CargoWise Carrier Code could be issued per organization."));
				return true;
			}

			return false;
		}

		bool ValidateDomesticCarrierCodeIsUnique()
		{
			if (IsDuplicateOf(OrgCusCode.CodeTypes.DomesticCarrierCode))
			{
				Parent.OK_CodeTypeInfo.AddError(Res.GetString("86b2231d-7bae-4a7c-95e2-44cbc60672ba", "Only one Domestic Carrier Code could be issued per organization."));
				return true;
			}

			return false;
		}

		bool ValidateJNPIsUnique()
		{
			if (IsDuplicateOf(OrgCusCode.CodeTypes.JNP))
			{
				Parent.OK_CodeTypeInfo.AddError(Res.GetString("12704497-482d-4871-a4a1-0e289408af60", "Only one JNP code can be issued per organization."));
				return true;
			}

			return false;
		}

		bool ValidateCargoWiseRoadTransportProviderCodeIsUnique()
		{
			if (IsDuplicateOf(OrgCusCode.CodeTypes.CargoWiseRoadTransportProviderCode))
			{
				Parent.OK_CodeTypeInfo.AddError(Res.GetString("22406f72-0336-4ac4-920f-9a0c617c6893", "Only one CargoWise Road Transport Provider code could be issued per organization."));
				return true;
			}

			return false;
		}

		bool IsDuplicateOf(string type)
		{
			return string.Compare(Parent.OK_CodeType, type, StringComparison.OrdinalIgnoreCase) == 0
				&& Parent.Header.CustomsCodes.Cast<OrgCusCode>().Any(code => code != Parent
					&& string.Compare(code.OK_CodeType, type, StringComparison.OrdinalIgnoreCase) == 0);
		}

		void ValidateUniversalCodesExistOnce()
		{
			if (Parent.OK_CodeType == OrgCusCode.CodeTypes.UniversalOfficeCode)
			{
				foreach (OrgCusCode code in Parent.Header.CustomsCodes)
				{
					if (code.PK != Parent.PK && code.OK_CodeType == OrgCusCode.CodeTypes.UniversalOfficeCode)
					{
						Parent.OK_CodeTypeInfo.AddError(Res.GetString("86c946af-46f9-4166-b987-f26a53d151a7", "Universal Office Code can only be specified once, irrelevant of the country/region specified."));
						break;
					}
				}
			}
			else if (Parent.OK_CodeType == OrgCusCode.CodeTypes.UniversalNettingCode)
			{
				foreach (OrgCusCode code in Parent.Header.CustomsCodes)
				{
					if (code.PK != Parent.PK && code.OK_CodeType == OrgCusCode.CodeTypes.UniversalNettingCode)
					{
						Parent.OK_CodeTypeInfo.AddError(Res.GetString("f7ffa87e-e478-4fd6-986f-bab149c6852f", "Universal Netting Code can only be specified once, irrelevant of the country/region specified."));
						break;
					}
				}
			}
		}

		protected void ValidateRegistrationForCustomsCarrierCodeIsUniqueForEachCountry()
		{
			if (Parent.IsApplicableToUSCustoms)
			{
				ValidateRegistrationForCustomsCarrierCodeIsUniqueForEachCountry(OrgCusCode.CodeTypes.TruckCarrierCode);
			}
			ValidateRegistrationForCustomsCarrierCodeIsUniqueForEachCountry(OrgCusCode.CodeTypes.CarrierCode);
		}

		protected void ValidateRegistrationForCustomsCarrierCodeIsUniqueForEachCountry(ZString codeType)
		{
			if (Parent.OK_CodeType == codeType)
			{
				var filter = new ZQuery(OrgCusCodeSchema.OK_CodeType, codeType);
				filter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, Parent.OK_CustomsRegNo);
				filter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Parent.OK_RN_NKCodeCountry);
				var cusCodes = Parent.Factory.Load<OrgCusCode>(filter);
				if (cusCodes.Length > 1)
				{
					Parent.OK_CodeTypeInfo.AddWarning(Res.GetString("8e9bb7e2-6332-4b37-8c45-d9bd3e7f61e6", "This Registration No is used in another organization in the same country/region"));
				}
			}
		}

		internal void ValidateRegistrationForCargoWiseOneCarrierCodeIsUnique()
		{
			if (Parent.OK_CodeType == OrgCusCode.CodeTypes.CargoWiseOneCarrierCode)
			{
				var filter = new ZQuery(OrgCusCodeSchema.OK_CodeType, Parent.OK_CodeType);
				filter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, Parent.OK_CustomsRegNo);

				var cusCodes = Parent.Factory.Load<OrgCusCode>(filter);
				if (cusCodes.Length > 1)
				{
					var existingOrgOH_Code = (cusCodes[0].Organisation.OH_Code != Parent.Organisation.OH_Code ? cusCodes[0].Organisation.OH_Code : cusCodes[1].Organisation.OH_Code);

					Parent.OK_CodeTypeInfo.AddError(Res.GetString("B06C9622-0825-44A2-B7C0-DC9F9A5ECAD4",
						"C1C Code {0} is already entered against Organization {1}. \r\nPlease remove it from that Organization in order to save it against this one",
						Parent.OK_CustomsRegNo, existingOrgOH_Code));
				}
			}
		}

		internal void ValidateRegistrationForBoleroTitleRegisterIDIsUnique()
		{
			if (Parent.OK_CodeType == OrgCusCode.CodeTypes.BoleroTitleRegisterID)
			{
				var filter = new ZQuery(OrgCusCodeSchema.OK_CodeType, Parent.OK_CodeType);
				filter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, Parent.OK_CustomsRegNo);
				filter.AddToFilter(OrgCusCodeSchema.OK_OH, SQLComparisonOperator.NotEqual, Parent.OK_OH);

				var cusCodes = Parent.Factory.Load<OrgCusCode>(filter);
				if (cusCodes.Length > 0)
				{
					var existingOrgOH_Code = cusCodes[0].Organisation.OH_Code;
					Parent.OK_CodeTypeInfo.AddError(Res.GetString("2A372C96-D67E-47AE-96FD-C115AD55A874",
						"TRI Code {0} is already entered against Organization {1}. It can only be added once.",
						Parent.OK_CustomsRegNo, existingOrgOH_Code));
				}
			}
		}

		internal void ValidateRegistrationForCargoWiseRoadTransportProviderCodeIsUnique()
		{
			if (Parent.OK_CodeType == OrgCusCode.CodeTypes.CargoWiseRoadTransportProviderCode)
			{
				var filter = new ZQuery(OrgCusCodeSchema.OK_CodeType, Parent.OK_CodeType);
				filter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, Parent.OK_CustomsRegNo);

				var cusCodes = Parent.Factory.Load<OrgCusCode>(filter);
				if (cusCodes.Length > 1)
				{
					var existingOrgOH_Code = (cusCodes[0].Organisation.OH_Code != Parent.Organisation.OH_Code ? cusCodes[0].Organisation.OH_Code : cusCodes[1].Organisation.OH_Code);

					Parent.OK_CodeTypeInfo.AddError(Res.GetString("7427A039-CAB1-4DF0-A751-E0736F87F10A",
						"C1R Code {0} is already entered against Organization {1}. \r\nPlease remove it from that Organization in order to save it against this one",
						Parent.OK_CustomsRegNo, existingOrgOH_Code));
				}
			}
		}

		protected void ValidateCountryDefaultIsUniqueForCountry()
		{
			if (Parent.Header != null)
			{
				foreach (OrgCusCode cusCode in Parent.Header.CustomsCodes)
				{
					if (cusCode != Parent &&
						cusCode.OK_RN_NKCodeCountry == Parent.OK_RN_NKCodeCountry &&
						Parent.OK_CountryDefault &&
						cusCode.OK_CountryDefault)
					{
						Parent.OK_CountryDefaultInfo.AddError(Res.GetString("fb1459bb-e804-4e1b-946c-1c8d0a7fd096", "There can only be one default selected for each country/region."));
						break;
					}
				}
			}
		}

		#endregion

		void ValidateCusCodeAllowedBySecurity()
		{
			if (Parent.IsInDatabase && (ZString)Parent.OK_CodeTypeInfo.OriginalValue == (ZString)Parent.OK_CodeTypeInfo.Value)
			{
				return;
			}

			ISecurityCheckpoint failingCheckpoint = null;
			string companyARAPInfo = null;

			if (Parent.Header != null)
			{
				var isPrimaryCodeType = Parent.IsCurrentCompanyCodeTypePrimary;

				if (Parent.Header.IsInDatabase)
				{
					var checkpointInfo = Parent.Header.GetFailingCheckpointAndCompanyARAPInfoWhenModifyRegistrationNumber(isPrimaryCodeType);
					failingCheckpoint = checkpointInfo.FailingCheckpoint;
					companyARAPInfo = checkpointInfo.CompanyARAPInfo;
				}
				else
				{
					if (isPrimaryCodeType && !Env.Security.OrgConfigNewModifyFinancialRegistrationNos.IsAllowed)
					{
						failingCheckpoint = Env.Security.OrgConfigNewModifyFinancialRegistrationNos;
					}
					else if (!isPrimaryCodeType && !Env.Security.OrgConfigNewModifyNonFinancialRegistrationNos.IsAllowed)
					{
						failingCheckpoint = Env.Security.OrgConfigNewModifyNonFinancialRegistrationNos;
					}
				}
			}

			if (failingCheckpoint != null)
			{
				Parent.OK_CodeTypeInfo.AddError(GetFailingMessage(failingCheckpoint, companyARAPInfo));
			}
		}

		public static string GetFailingMessage(ISecurityCheckpoint point, string companyARAPInfo)
		{
			return Res.GetString("582fc2a4-63fc-48d3-842c-113b1b01d4e9", @"You do not have the appropriate security rights to select this Code.
If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: {0}. {1}", point.DisplayTextPathToSecurityRight, companyARAPInfo);
		}

		#endregion

		readonly new OrgCusCode Parent;
	}
}

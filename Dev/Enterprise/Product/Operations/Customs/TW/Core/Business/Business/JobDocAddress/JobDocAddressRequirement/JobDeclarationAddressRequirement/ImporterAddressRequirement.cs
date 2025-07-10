using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.Business
{
	public class ImporterAddressRequirement : TWJobDocAddressRequirement
	{
		public ImporterAddressRequirement(DocAddressType defaultDocAddressType, ContactType defaultContactType)
			: base(defaultDocAddressType, defaultContactType)
		{
		}

		protected override void Initialize()
		{
			base.Initialize();
			this.ValidateIDCodeType += ValidateIDCodeTypeCore;
			this.ValidateCBPCodeType += ValidateCBPCodeTypeCore;
			this.ValidateIDCode += ValidateIDCodeCore;
			this.ValidateCBPCode += ValidateCBPCodeCore;
			this.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address += ValidateE2_OA_Address;
			this.ValidateAddress1 += ValidateE2_Address1;
			this.ValidateCompanyName += ValidateE2_CompanyName;
			this.ValidateCity += ValidateE2_City;
			this.ValidatePostCode += ValidateE2_Postcode;
			this.ValidateState += ValidateCheckE2_State;
			this.ValidateCountry += ValidateE2_RN_NKCountryCode;
			this.ValidateOrganisationPK += ValidateOrganization;
		}

		void ValidateIDCodeTypeCore(TWJobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			var propertyInfo = parent.IDCodeTypeInfo;
			if (parent.E2_AddressOverride && parent.IsImport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}
		}

		void ValidateCBPCodeTypeCore(TWJobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			var propertyInfo = parent.CBPCodeTypeInfo;
			if (parent.Parent is JobDeclaration jobDeclaration && !parent.CBPCode.IsEmpty)
			{
				var declarationType = jobDeclaration.CusEntryInstruction.CEI_Style;
				IEnumerable<ZString> types;
				validImporterBondedTypeMap.TryGetValue(declarationType, out types);
				if ((types?.Any() ?? false) && !types.Contains((ZString)propertyInfo.Value))
				{
					propertyInfo.AddMessageError(Res.GetString("38B3A474-482F-4A43-A920-27B686EC74AC", "Importer Bonded ID should be of type {0} for Declaration Type {1}.", ZString.Join((NoResString)" or ", types.ToArray()), declarationType));
				}
			}
		}

		void ValidateIDCodeCore(TWJobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (!parent.IDCodeReadonly && parent.E2_AddressOverride && parent.IsImport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.IDCodeInfo);
			}
		}

		void ValidateCBPCodeCore(TWJobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (parent.Parent is JobDeclaration jobDeclaration)
			{
				var propertyInfo = parent.CBPCodeInfo;
				var entryInstruction = jobDeclaration.CusEntryInstruction;
				var declarationType = entryInstruction.CEI_Style;
				ValidateBondedIdIsRequired(propertyInfo, declarationType, jobDeclaration);
				ValidateBondedIdIsNotRequired(parent, propertyInfo, declarationType);
				ValidateBondedIdCanNotCoExistOrEmptyWithToBondedWarehouse(propertyInfo, declarationType, entryInstruction);
				ValidateBondedIdCanNotCoExistWithBondedFactories(propertyInfo, declarationType, jobDeclaration);
			}
		}

		void ValidateE2_OA_Address(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent)
			{
				var propertyInfo = parent.E2_OA_AddressInfo;
				if (!parent.E2_AddressOverride && parent.Organisation != null)
				{
					MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
					var address = parent.Address;
					if (address != null && !SharedHelper.GetEnglishLanguageCodes().Contains(address.OA_Language))
					{
						propertyInfo.AddMessageError(Res.GetString("a8a86312-9a84-4b30-a74d-6c4f9cbddfcd", "Please select an English address."));
					}
					CheckE2_OA_AddressCompanyNameAndAddressLength(parent, propertyInfo);
				}
			}
		}

		void CheckE2_OA_AddressCompanyNameAndAddressLength(TWJobDocAddress parent, ZPropertyInfo targetInfo)
		{
			var address = parent.Address;
			IPartyDetails partyDetailsWrapper = new PartyDetailsWrapper("", "", "", address);
			var englishName = partyDetailsWrapper.Name;
			var englishAddress = partyDetailsWrapper.Address.Line;
			if (englishName.Length > ValidationConstants.TWJobDocAddressValidationMessages.ForeignNameMaxlength)
			{
				targetInfo.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxForeignCompanyNameCharactersLength(ValidationConstants.TWJobDocAddressValidationMessages.ForeignNameMaxlength));
			}
			if (englishAddress.Length > ValidationConstants.TWJobDocAddressValidationMessages.ForeignAddressMaxlength)
			{
				targetInfo.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxForeignAddressCharactersLength(ValidationConstants.TWJobDocAddressValidationMessages.ForeignAddressMaxlength));
			}

			if (parent.IsImport)
			{
				var chineseName = partyDetailsWrapper.ChineseName;
				var chineseAddress = partyDetailsWrapper.Address.ChineseLine;
				if (chineseName.Length > ValidationConstants.TWJobDocAddressValidationMessages.ChineseNameMaxlength)
				{
					targetInfo.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxChineseCompanyNameCharactersLength(ValidationConstants.TWJobDocAddressValidationMessages.ChineseNameMaxlength));
				}
				if (chineseAddress.Length > ValidationConstants.TWJobDocAddressValidationMessages.ChineseAddressMaxlength)
				{
					targetInfo.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxChineseAddressCharactersLength(ValidationConstants.TWJobDocAddressValidationMessages.ChineseAddressMaxlength));
				}
			}
		}

		void ValidateBondedIdIsRequired(ZPropertyInfo propertyInfo, ZString declarationType, JobDeclaration jobDeclaration)
		{
			if (propertyInfo.Value.IsEmpty)
			{
				if (jobDeclaration.JE_RL_NKFinalDestination.Left(2) == CountryCodes.Taiwan && importerBondedIdMightRequiredFromAFreeTradeZoneDeclarationTypeList.Contains(declarationType))
				{
					propertyInfo.AddWarning(Res.GetString("D8FFF0D1-68BD-440B-8D50-0ECC5ACFC9FE", "Importer Bonded ID might be required for Declaration Type {0} when goods are being imported into a Free Trade Zone.", declarationType));
				}
				else if (importerBondedIdIsRequiredDeclarationTypeList.Contains(declarationType))
				{
					propertyInfo.AddMessageError(Res.GetString("477D142E-D5B0-459A-8D6A-8017BA138FFD", "Importer Bonded ID is required for Declaration Type {0}.", declarationType));
				}
			}
		}

		void ValidateBondedIdIsNotRequired(TWJobDocAddress parent, ZPropertyInfo propertyInfo, ZString declarationType)
		{
			if (parent.E2_AddressOverride)
			{
				if (!propertyInfo.Value.IsEmpty && importerBondedIdIsNotRequiredByDeclarationTypeList.Contains(declarationType))
				{
					propertyInfo.AddMessageError(Res.GetString("9780C254-7A3B-48EE-B08D-45308018C24E", "Importer Bonded ID is not required for Declaration Type {0}.", declarationType));
				}
			}
		}

		void ValidateBondedIdCanNotCoExistOrEmptyWithToBondedWarehouse(ZPropertyInfo propertyInfo, ZString declarationType, CusEntryInstruction entryInstruction)
		{
			if (ImporterBondedIdCanNotCoExistOrEmptyWithToBondedWarehouseList.Contains(declarationType))
			{
				var warehouse2 = entryInstruction.CEI_OA_Warehouse2;
				if ((warehouse2.IsEmpty && propertyInfo.Value.IsEmpty) || (!warehouse2.IsEmpty && !propertyInfo.Value.IsEmpty))
				{
					propertyInfo.AddMessageError(ValidationConstants.TWJobDocAddress.ImporterBondedIdCanNotCoExistOrEmptyWithToBondedWarehouse);
				}
			}
		}

		void ValidateBondedIdCanNotCoExistWithBondedFactories(ZPropertyInfo propertyInfo, ZString declarationType, JobDeclaration jobDeclaration)
		{
			if (declarationType == Constants.DeclarationTypes.Import.G7)
			{
				var bondedFactories = jobDeclaration.BondedFactories;
				var anyBondedFactories = bondedFactories.Any(c => !c.E2_OA_Address.IsEmpty);
				if (anyBondedFactories && !propertyInfo.Value.IsEmpty)
				{
					propertyInfo.AddMessageError(ValidationConstants.TWJobDocAddress.ImporterBondedIdCanNotCoExistWithBondedFactories);
				}
			}
		}

		void ValidateE2_Address1(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent)
			{
				var targetInfo = parent.E2_Address1Info;

				if (parent.E2_AddressOverride)
				{
					var englishAddressData = new AddressData(parent, SharedHelper.GetEnglishLanguageCodes());
					if (englishAddressData.EnglishAddressFormat.Length > ValidationConstants.TWJobDocAddressValidationMessages.ForeignAddressMaxlength)
					{
						targetInfo.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxForeignAddressCharactersLength(ValidationConstants.TWJobDocAddressValidationMessages.ForeignAddressMaxlength));
					}
				}
			}
		}

		void ValidateE2_CompanyName(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			var targetInfo = parent.E2_CompanyNameInfo;
			if (parent.E2_AddressOverride)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}

			var maxLangth = 80;
			if (parent.E2_CompanyName.Length > maxLangth)
			{
				targetInfo.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxCharactersLength(maxLangth));
			}
		}

		void ValidateE2_City(JobDocAddressValidation validation) { }

		void ValidateE2_Postcode(JobDocAddressValidation validation) { }

		void ValidateCheckE2_State(JobDocAddressValidation validation)
		{
			if (validation is TWJobDocAddressValidation twValidation)
			{
				twValidation.ValidateState();
			}
		}

		void ValidateE2_RN_NKCountryCode(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent && parent.E2_AddressOverride && parent.IsExport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.E2_RN_NKCountryCodeInfo);
			}
		}

		void ValidateOrganization(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent && !parent.E2_AddressOverride && parent.IsImport && parent.Organisation != null && parent.IDCode.IsEmpty)
			{
				parent.OrganisationPKInfo.AddMessageError(Res.GetString("de98e099-fa68-4f13-a22c-11e954a01521", "A valid TW-VAT or TW-PAS or TW-PID number is required for Importer. To create a valid TW-VAT or TW-PAS or TW-PID, visit Organization > Details > Config > Registration."));
			}
		}

		readonly ImmutableDictionary<string, IEnumerable<ZString>> validImporterBondedTypeMap = ImmutableDictionary.CreateRange(new Dictionary<string, IEnumerable<ZString>> {
			{ Constants.DeclarationTypes.Export.B1, TWJobDocAddressValidation.BondIdCodeTypes },
			{ Constants.DeclarationTypes.Export.B2, TWJobDocAddressValidation.BondIdCodeTypes },
			{ Constants.DeclarationTypes.Import.B6, TWJobDocAddressValidation.BondIdCodeTypes },
			{ Constants.DeclarationTypes.Export.B8, new ZString[] { OrgCusCode.TaiwanCodeTypes.FTZ } },
			{ Constants.DeclarationTypes.Export.B9, new ZString[] { OrgCusCode.TaiwanCodeTypes.FTZ } },
			{ Constants.DeclarationTypes.Export.D5, new ZString[] { OrgCusCode.TaiwanCodeTypes.FTZ } },
			{ Constants.DeclarationTypes.Import.D7, TWJobDocAddressValidation.BondIdCodeTypes },
			{ Constants.DeclarationTypes.Import.L1, new ZString[] { OrgCusCode.CodeTypes.ControlledPremisesID } },
			{ Constants.DeclarationTypes.Import.F1, new ZString[] { OrgCusCode.TaiwanCodeTypes.FTZ } },
			{ Constants.DeclarationTypes.Import.F3, new ZString[] { OrgCusCode.TaiwanCodeTypes.FTZ } },
			{ Constants.DeclarationTypes.Export.F4, new ZString[] { OrgCusCode.TaiwanCodeTypes.FTZ } },
			{ Constants.DeclarationTypes.Import.G1, new ZString[] { OrgCusCode.TaiwanCodeTypes.EPZ } },
			{ Constants.DeclarationTypes.Import.G7, TWJobDocAddressValidation.BondIdCodeTypes },
		});

		readonly IEnumerable<ZString> importerBondedIdMightRequiredFromAFreeTradeZoneDeclarationTypeList = new ZString[] {
			Constants.DeclarationTypes.Export.B8,
			Constants.DeclarationTypes.Export.B9,
			Constants.DeclarationTypes.Export.D5
		};

		readonly IEnumerable<ZString> importerBondedIdIsRequiredDeclarationTypeList = new ZString[] {
			Constants.DeclarationTypes.Export.B1,
			Constants.DeclarationTypes.Import.B6,
			Constants.DeclarationTypes.Export.F4,
			Constants.DeclarationTypes.Import.F1,
			Constants.DeclarationTypes.Import.F3
		};

		readonly IEnumerable<ZString> importerBondedIdIsNotRequiredByDeclarationTypeList = new ZString[] {
			Constants.DeclarationTypes.Export.D1,
			Constants.DeclarationTypes.Import.D2,
			Constants.DeclarationTypes.Import.D8,
			Constants.DeclarationTypes.Import.F2,
			Constants.DeclarationTypes.Export.F5,
			Constants.DeclarationTypes.Import.G2,
		};

		[ThreadSafe]
		public static readonly IEnumerable<ZString> ImporterBondedIdCanNotCoExistOrEmptyWithToBondedWarehouseList = new ZString[] {
			Constants.DeclarationTypes.Export.B2,
			Constants.DeclarationTypes.Import.D7,
		};
	}
}

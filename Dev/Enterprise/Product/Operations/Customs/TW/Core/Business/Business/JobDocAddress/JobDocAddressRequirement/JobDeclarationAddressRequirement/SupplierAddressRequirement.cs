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
	public class SupplierAddressRequirement : TWJobDocAddressRequirement
	{
		public SupplierAddressRequirement(DocAddressType defaultDocAddressType, ContactType defaultContactType)
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
			if (parent.E2_AddressOverride && parent.IsExport)
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
				validSupplierBondedTypeMap.TryGetValue(declarationType, out types);
				if ((types?.Any() ?? false) && !types.Contains((ZString)propertyInfo.Value))
				{
					propertyInfo.AddMessageError(Res.GetString("A36FF4D6-DABD-4287-8900-87FCD4A3372C", "Supplier Bonded ID should be of type {0} for Declaration Type {1}.", ZString.Join((NoResString)" or ", types.ToArray()), declarationType));
				}
			}
		}

		void ValidateIDCodeCore(TWJobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (!parent.IDCodeReadonly && parent.E2_AddressOverride && parent.IsExport)
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
				ValidateBondedIdCanNotCoExistOrEmptyWithBondedFactories(propertyInfo, declarationType, jobDeclaration);
			}
		}

		void ValidateBondedIdIsRequired(ZPropertyInfo propertyInfo, ZString declarationType, JobDeclaration jobDeclaration)
		{
			if (propertyInfo.Value.IsEmpty)
			{
				if (jobDeclaration.JE_RL_NKOrigin.Left(2) == CountryCodes.Taiwan)
				{
					if (supplierBondedIdIsRequiredFromAFreeTradeZoneDeclarationTypeList.Contains(declarationType))
					{
						propertyInfo.AddMessageError(Res.GetString("97F9D5C7-740C-4F5E-9B27-55EA3A1274A8", "Supplier Bonded ID is required for Declaration Type {0} when goods are coming from a Free Trade Zone.", declarationType));
					}
					else if (supplierBondedIdMightBeRequiredFromAFreeTradeZoneDeclarationTypeList.Contains(declarationType))
					{
						propertyInfo.AddWarning(Res.GetString("A3DD97DB-FCF5-4EB1-A72E-BA270DC03C9F", "Supplier Boned ID might be required for Declaration Type {0} when goods are coming from a Free Trade Zone.", declarationType));
					}
				}
				else if (supplierBondedIdIsRequiredDeclarationTypeList.Contains(declarationType))
				{
					propertyInfo.AddMessageError(Res.GetString("7536C294-2FD5-4F06-8A47-59401A9062DC", "Supplier Bonded ID is required for Declaration Type {0}.", declarationType));
				}
			}
		}

		void ValidateBondedIdIsNotRequired(TWJobDocAddress parent, ZPropertyInfo propertyInfo, ZString declarationType)
		{
			if (parent.E2_AddressOverride)
			{
				if (!propertyInfo.Value.IsEmpty && supplierBondedIdIsNotRequiredByDeclarationTypeList.Contains(declarationType))
				{
					propertyInfo.AddMessageError(Res.GetString("7FFEA76E-E8CF-4411-A4AF-2AA5C7F4327C", "Supplier Bonded ID is not required for Declaration Type {0}.", declarationType));
				}
			}
		}

		void ValidateBondedIdCanNotCoExistOrEmptyWithBondedFactories(ZPropertyInfo propertyInfo, ZString declarationType, JobDeclaration jobDeclaration)
		{
			if (SupplierBondedIdCanNotCoExistOrEmptyWithToBondedWarehouseList.Contains(declarationType))
			{
				var bondedFactories = jobDeclaration.BondedFactories;
				var anyBondedFactories = bondedFactories.Any(c => !c.E2_OA_Address.IsEmpty);
				if ((!anyBondedFactories && propertyInfo.Value.IsEmpty) || (anyBondedFactories && !propertyInfo.Value.IsEmpty))
				{
					propertyInfo.AddMessageError(ValidationConstants.TWJobDocAddress.SupplierBondedIdCanNotCoExistOrEmptyWithBondedFactories);
				}
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

		void ValidateE2_Address1(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent)
			{
				var targetInfo = parent.E2_Address1Info;
				if (parent.IsImport)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}

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

		void ValidateE2_City(JobDocAddressValidation validation)
		{
		}

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
			if (validation.Parent is TWJobDocAddress parent && parent.E2_AddressOverride && parent.IsImport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.E2_RN_NKCountryCodeInfo);
			}
		}

		void ValidateOrganization(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent && !parent.E2_AddressOverride && parent.IsExport && parent.Organisation != null && parent.IDCode.IsEmpty)
			{
				parent.OrganisationPKInfo.AddMessageError(Res.GetString("06af0a9c-a67e-4295-a633-046c8807002c", "A valid TW-VAT or TW-PAS or TW-PID number is required for Supplier. To create a valid TW-VAT or TW-PAS or TW-PID, visit Organization > Details > Config > Registration."));
			}
		}

		readonly ImmutableDictionary<string, IEnumerable<ZString>> validSupplierBondedTypeMap = ImmutableDictionary.CreateRange(new Dictionary<string, IEnumerable<ZString>> {
			{ Constants.DeclarationTypes.Export.B2, TWJobDocAddressValidation.BondIdCodeTypes },
			{ Constants.DeclarationTypes.Import.B6, new ZString[] { OrgCusCode.TaiwanCodeTypes.FTZ } },
			{ Constants.DeclarationTypes.Export.B8, TWJobDocAddressValidation.BondIdCodeTypes },
			{ Constants.DeclarationTypes.Export.B9, TWJobDocAddressValidation.BondIdCodeTypes },
			{ Constants.DeclarationTypes.Import.D8, new ZString[] { OrgCusCode.TaiwanCodeTypes.FTZ } },
			{ Constants.DeclarationTypes.Import.F2, new ZString[] { OrgCusCode.TaiwanCodeTypes.FTZ } },
			{ Constants.DeclarationTypes.Import.F3, new ZString[] { OrgCusCode.TaiwanCodeTypes.FTZ } },
			{ Constants.DeclarationTypes.Export.F4, new ZString[] { OrgCusCode.TaiwanCodeTypes.FTZ } },
			{ Constants.DeclarationTypes.Export.F5, new ZString[] { OrgCusCode.TaiwanCodeTypes.FTZ } },
			{ Constants.DeclarationTypes.Import.G2, TWJobDocAddressValidation.BondIdCodeTypes },
		});

		readonly IEnumerable<ZString> supplierBondedIdIsNotRequiredByDeclarationTypeList = new ZString[] {
			Constants.DeclarationTypes.Export.B1,
			Constants.DeclarationTypes.Export.D1,
			Constants.DeclarationTypes.Import.D2,
			Constants.DeclarationTypes.Export.D5,
			Constants.DeclarationTypes.Import.D7,
			Constants.DeclarationTypes.Import.L1,
			Constants.DeclarationTypes.Import.F1,
			Constants.DeclarationTypes.Import.G1,
			Constants.DeclarationTypes.Import.G7,
		};

		[ThreadSafe]
		public static readonly IEnumerable<ZString> SupplierBondedIdCanNotCoExistOrEmptyWithToBondedWarehouseList = new ZString[] {
			Constants.DeclarationTypes.Export.B8,
			Constants.DeclarationTypes.Export.B9,
			Constants.DeclarationTypes.Export.F5,
		};

		readonly IEnumerable<ZString> supplierBondedIdIsRequiredFromAFreeTradeZoneDeclarationTypeList = new ZString[] {
			Constants.DeclarationTypes.Import.F3,
			Constants.DeclarationTypes.Export.F4
		};

		readonly IEnumerable<ZString> supplierBondedIdMightBeRequiredFromAFreeTradeZoneDeclarationTypeList = new ZString[] {
			Constants.DeclarationTypes.Import.D8,
			Constants.DeclarationTypes.Import.B6,
		};

		readonly IEnumerable<ZString> supplierBondedIdIsRequiredDeclarationTypeList = new ZString[] {
			Constants.DeclarationTypes.Import.G2,
			Constants.DeclarationTypes.Export.B2,
			Constants.DeclarationTypes.Import.F2,
			Constants.DeclarationTypes.Import.F3
		};
	}
}

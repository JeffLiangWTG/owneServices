using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using static System.FormattableString;

namespace Enterprise.Customs.ZA.Business.Business.Utilities
{
	public class UCRHelper
	{
		public ZString CalculateUCR(CusEntryInstruction cusEntryInstruction)
		{
			var result = ZString.Empty;
			var jobDeclaration = cusEntryInstruction?.JobDeclaration;

			if (jobDeclaration != null)
			{
				var lastCharOfYear = GetLastCharOfYear;                                                 // last number of the year [1 character]
				var countryCode = GetCountryCode(jobDeclaration);                                   // Goods Origin - Country ISO code (alpha-2 code)[2 characters]
				var entityType = GetEntityType(cusEntryInstruction);                                // Type of Entity code of the supplier/consignor [1 character]
				var entityCode = GetEntityCode(entityType, jobDeclaration);                         // Entity code of the supplier/consignor [8-13 characters]
				var refType = GetRefType(cusEntryInstruction);                                  // Reference Type of the consignment [3 characters]
				var refNum = GetRefNum(cusEntryInstruction, jobDeclaration).Left(27 - entityCode.Length);                    // Unique reference for the consignment [max 19 characters]
				var scope = GetScope(cusEntryInstruction);                                      // Usage [1 character]

				result = string.Concat(lastCharOfYear, countryCode, entityCode, entityType, refType, refNum, scope);
			}

			return result;
		}

		public ZString GetEntityCode(ZString entityType, JobDeclaration jobDeclaration)
		{
			var result = ZString.Empty;

			if (jobDeclaration != null)
			{
				DecisionReason = ZString.Empty;

				var countryCode = GetCountryCode(jobDeclaration);
				var supplierOrg = jobDeclaration.Supplier;

				switch (entityType)
				{
					case EntityTypeList.Codes.CustomsCode:
						result = GetRegNoForSupplierOrg(supplierOrg, countryCode, OrgCusCode.CodeTypes.SupplierCode);
						break;
					case EntityTypeList.Codes.TaxCode:
						result = GetRegNoForSupplierOrg(supplierOrg, countryCode, OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.TaxFileCode);
						break;
					case EntityTypeList.Codes.CompanyRegistrationNumber:
						result = GetRegNoForSupplierOrg(supplierOrg, countryCode, OrgCusCode.CodeTypes.GovBusinessCode);
						break;
					case EntityTypeList.Codes.IDPassportNumber:
						result = GetRegNoForSupplierOrg(supplierOrg, countryCode, OrgCusCode.SouthAfricaCodeTypes.IDNumber, OrgCusCode.CodeTypes.PassportID);
						break;
				}
			}

			return result;
		}

		public void ValidateUCR(CusEntryInstruction parent, JobDeclaration jobDeclaration)
		{
			var ucrNumber = parent.CEI_UCROverride;
			var info = parent.CEI_UCROverrideInfo;

			if (jobDeclaration.IsBLNSValidationRequired)
			{
				if (!parent.CEI_IsUCROverridden && !parent.ShouldUpdateUCRNumber && ucrNumber.IsEmpty)
				{
					info.AddWarning(ValidationConstants.EntryInstruction.UnableToCalculateUCR);
				}

				if (!ucrNumber.IsEmpty)
				{
					if (!ucrNumber.IsLettersAndNumbersOnlyOrEmpty || !((ZInt)ucrNumber.Length).IsInRange(17, 35))
					{
						if (!jobDeclaration.IsImport)
						{
							info.AddMessageError(ValidationConstants.EntryInstruction.InvalidUCRNumberOverride);
						}
					}
					else
					{
						if (!ucrNumber.SubstringSafe(0, 1).IsNumbersOnlyOrEmpty)
						{
							info.AddWarning(ValidationConstants.EntryInstruction.InvalidUCRYear);
						}

						var orignCountryCode = GetCountryCode(jobDeclaration);
						if (!parent.CEI_IsUCROverridden && orignCountryCode.IsEmpty)
						{
							info.AddWarning(ValidationConstants.EntryInstruction.InvalidUCRCountry);
						}
						else
						{
							var countryCode = ucrNumber.Substring(1, 2);
							if (!countryCode.IsLettersOnlyOrEmpty || countryCode != orignCountryCode)
							{
								info.AddWarning(ValidationConstants.EntryInstruction.InvalidUCRCountryOverride);
							}
						}

						//we cannot test anything between these two values, as we do not know the length of the entity code or reference number.
						if (jobDeclaration != null)
						{
							var scope = ucrNumber.SubstringSafe(ucrNumber.Length - 1, 1);
							if (!jobDeclaration.Lookups.ScopeList.ContainsCode(scope))
							{
								info.AddWarning(ValidationConstants.EntryInstruction.InvalidUCRScope);
							}
						}
					}
				}
			}

			var entryNumberPk = parent.EntryHeader != null ? parent.EntryHeader.PK : ZGuid.Empty;
			var jobNumber = ValidationHelper.GetJobNumberOfDuplicateUCR(parent.Factory, ucrNumber, entryNumberPk);
			if (jobNumber != "")
			{
				if (!jobDeclaration.IsImport)
				{
					info.AddMessageError(ZString.Format(ValidationConstants.EntryInstruction.UCRNumberAlreadyUsedOnJob, ucrNumber, jobNumber));
				}
			}
		}

		public ZString DecisionReason { get; private set; }

		public static BaseJobComInvoiceHeader BestInvoice(JobDeclaration declaration)
		{
			return declaration?.Invoices.Where(x => !x.IsDeleted && !x.JZ_InvoiceNumber.KeepAlphanumericCharacters().IsEmpty).OrderBy(x => x.JZ_InvoiceNumber).FirstOrDefault();
		}

		ZString GetRegNoForSupplierOrg(OrgHeader org, ZString countryCode, ZString cusCode, string fallbackCusCode = "")
		{
			var result = ZString.Empty;
			if (org == null)
			{
				DeclarationMustHaveSupplierOrg();
			}
			else
			{
				result = org.CustomsCodes.GetCustomsRegNo(cusCode, countryCode);

				var hasFallbackCusCode = !fallbackCusCode.IsNullOrEmpty();
				if (result.IsEmpty && hasFallbackCusCode)
				{
					result = org.CustomsCodes.GetCustomsRegNo(fallbackCusCode, countryCode);
				}

				var entityCodeType = hasFallbackCusCode
					? (ZString)Invariant($"{cusCode} or {fallbackCusCode}")
					: cusCode;

				CheckEntityCode(result, countryCode, entityCodeType);
			}

			return result;
		}

		void DeclarationMustHaveSupplierOrg()
		{
			DecisionReason = Res.GetString("24996AC6-68A1-4588-8477-C086A24A8899", "The Declaration must have a valid Main Supplier to use this Entity Type.");
		}

		void CheckEntityCode(ZString result, ZString countryCode, ZString cusCode)
		{
			if (!IsEntityCodeValid(result))
			{
				DecisionReason = Res.GetString("D6D1D1D3-BC5A-4F87-B6C4-7D26F12B7ACD", "The Main Supplier must have a {0} Registration Code of type {1} between 8 and 13 characters inclusive.", countryCode, cusCode);
			}
		}

		static ZString GetEntityType(CusEntryInstruction cusEntryInstruction) => cusEntryInstruction.CEI_EntityType;

		static ZString GetRefType(CusEntryInstruction cusEntryInstruction) => cusEntryInstruction.CEI_RefType;

		static ZString GetRefNum(CusEntryInstruction cusEntryInstruction, JobDeclaration jobDeclaration)
		{
			ZString result;

			var refType = GetRefType(cusEntryInstruction);
			switch (refType)
			{
				case RefTypeList.Codes.Invoice:
					result = BestInvoice(jobDeclaration)?.JZ_InvoiceNumber.KeepAlphanumericCharacters().SubstringSafe(0, 19) ?? ZString.Empty;
					break;
				case RefTypeList.Codes.DeclarantGenerated:
					result = jobDeclaration?.JE_DeclarationReference.Substring(Math.Max(0, jobDeclaration.JE_DeclarationReference.Length - CusEntryInstruction.Schema.CEI_UCROrderNumberMaxLength)) ?? ZString.Empty;
					break;
				default:
					result = cusEntryInstruction.CEI_UCROrderNumber;
					break;
			}

			return result;
		}

		static ZString GetScope(CusEntryInstruction cusEntryInstruction) => cusEntryInstruction.CEI_Scope;

		static ZString GetLastCharOfYear => (ZDate.Today.Year % 10).ToString(CultureInfo.CurrentCulture);

		static ZString GetCountryCode(JobDeclaration jobDeclaration) => jobDeclaration?.Origin?.RL_RN_NKCountryCode ?? ZString.Empty;

		static bool IsEntityCodeValid(ZString result) => CorrectLength(result, 8, 13);

		static bool CorrectLength(ZString str, int minLength, int maxLength) => ((ZInt)str.Length).IsInRange(minLength, maxLength);
	}
}

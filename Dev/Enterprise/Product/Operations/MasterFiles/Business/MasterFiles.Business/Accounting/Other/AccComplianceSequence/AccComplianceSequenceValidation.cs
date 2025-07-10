using System;
using System.Data;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Environment;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccComplianceSequenceValidation : AutoAccComplianceSequenceValidation
	{
		readonly string overlappingBookExists = ResString.GetMultilingualString("992cbd81-efba-4f3a-a535-ce2fc07cdd47", "An active book already exists for selected branch/department or company within the validity period with the subtype");
		readonly string numberSeriesMustNotOverlap = ResString.GetMultilingualString("ed318313-34a3-42a8-b214-30fd4822cf88", "Number series and prefix is not unique for the compliance sub type");
		readonly string numberSeriesNotUniqueWhenCombineWithPrefix = ResString.GetMultilingualString("6bf79e55-0f52-43da-a59f-276bc45bc54d", "Number series is not unique when combine with prefix");
		readonly string lengthOfPrefixAndMaxNumDigitsTooLong = ResString.GetMultilingualString("077d8a46-ca41-4aa0-8e43-c04a9055b3b5", "Length of prefix plus max number of digits should not exceeds the length of transaction reference");
		readonly string validDateShouldBeSmallerThanExpireDate = ResString.GetMultilingualString("5b9b9922-9315-4737-ac6a-d870b387f535", "Valid date should be smaller than expire date");

		public AccComplianceSequenceValidation(AutoAccComplianceSequence parent)
			: base(parent)
		{
		}

		protected new AccComplianceSequence Parent
		{
			get { return (AccComplianceSequence)base.Parent; }
		}

		GlbCompany Company => Parent?.Company ?? GlbCompany.CurrentCompany;

		protected override void CheckXD_NumberFormat()
		{
			base.CheckXD_NumberFormat();

			if (!Parent.XD_NumberFormatInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.XD_NumberFormatInfo);
			}

			if (!Parent.XD_NumberFormatInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.XD_NumberFormatInfo);
			}

			if (!Parent.XD_NumberFormatInfo.HasErrors() && Parent.XD_NumberFormatInfo.HasChanges && Parent.XD_NextNumber != Parent.XD_StartNumber)
			{
				Parent.XD_NumberFormatInfo.AddError(Res.GetString("0812125d-2406-4436-bed8-af16739ad5de", "The Number Format cannot be changed as number sequence has been allocated from this compliance sequence book."));
			}

			if (!Parent.XD_NumberFormatInfo.HasErrors() && Parent.XD_NumberFormat != AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code)
			{
				var collection = AccountingMasterFilesRegistry.Instance.ComplianceNumberSequenceConfiguration.GetValueWithoutFallback(Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (collection != null && collection.Count > 0)
				{
					var configuration = collection.GetComplianceNumberSequenceConfigurationByCode(Parent.XD_NumberFormat);
					var totalLength = configuration.GetTotalLengthOfIncludedElements(Parent.XD_MaximumNumberDigits, Parent.XD_Prefix.Length, Parent.XD_SequenceClass);

					var transactionReferenceMaxLength = AccTransactionHeaderSchema.AH_TransactionReference.MaxLength;
					if (totalLength > transactionReferenceMaxLength)
					{
						Parent.XD_NumberFormatInfo.AddError(Res.GetString("93e0995f-b455-493b-aa6f-1483a1709d0f", "This compliance invoice book cannot use the selected number format as the length of the compliance sequence number will exceed the total length of {0} characters.", transactionReferenceMaxLength));
					}
				}
			}

			if (!Parent.XD_NumberFormatInfo.HasErrors() && (Parent.XD_NumberFormatInfo.HasChanges || !Parent.IsInDatabase))
			{
				var defaultConfig = Parent.DefaultMandatoryComplianceNumberSequenceConfiguration;
				if (defaultConfig != null && Parent.XD_NumberFormat != defaultConfig.Code)
				{
					Parent.XD_NumberFormatInfo.AddError(Res.GetString("3f4e0d03-083a-4c84-bd51-a2821d31235f",
						"The book must use the default mandatory number format: {0}", defaultConfig.Code));
				}
			}
		}

		protected override void CheckXD_AllocationLevel()
		{
			base.CheckXD_AllocationLevel();

			if (!Parent.XD_AllocationLevelInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.XD_AllocationLevelInfo);
			}

			if (!Parent.XD_AllocationLevelInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.XD_AllocationLevelInfo);
			}
		}

		protected override void CheckXD_SequenceClass()
		{
			base.CheckXD_SequenceClass();
			CheckSecurity(Parent.XD_SequenceClassInfo, Env.Security.ComplianceSequencesModifySequenceClass);

			if (!Parent.XD_SequenceClassInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.XD_SequenceClassInfo);
			}

			if (!Parent.XD_SequenceClassInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.XD_SequenceClassInfo);
			}

			if (!Parent.XD_SequenceClassInfo.HasErrors() && CheckOverlappingBooks())
			{
				Parent.XD_SequenceClassInfo.AddError(overlappingBookExists);
			}

			if (!Parent.XD_SequenceClassInfo.HasErrors())
			{
				var parentSubType = AccComplianceSequence.FindParentSubTypeInRegistry(Parent.XD_SequenceClass);
				if (!parentSubType.IsEmpty)
				{
					Parent.XD_SequenceClassInfo.AddError(Res.GetString("5018bb15-865a-4dc5-8179-a7087da1c9f9", @"Compliance Sequence Book cannot be created for sub type {0}. 
Transactions with sub type {0} will be assigned Compliance numbers from the {1} Compliance Invoice Book", Parent.XD_SequenceClass, parentSubType));
				}
			}
		}

		protected override void CheckXD_Code()
		{
			base.CheckXD_Code();
			CheckSecurity(Parent.XD_CodeInfo, Env.Security.ComplianceSequencesModifyCode);

			if (!Parent.XD_CodeInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.XD_CodeInfo);
			}

			if (!Parent.XD_CodeInfo.HasErrors())
			{
				Regex nonAlphaNumericRegex = new Regex(@"^[a-z0-9]+\s*$", RegexOptions.IgnoreCase);
				if (!nonAlphaNumericRegex.IsMatch(Parent.XD_Code))
				{
					Parent.XD_CodeInfo.AddError(Res.GetString("c1abc7cf-6d46-4595-9ee4-96bfe646b28c", "{0} is not a valid Code.", Parent.XD_Code));
				}
			}

			if (!Parent.XD_CodeInfo.HasErrors())
			{
				var query = new ZQuery(AccComplianceSequenceSchema.XD_Code, Parent.XD_Code);
				query.AddToFilter(AccComplianceSequenceSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				query.AddToFilter(JoinCondition.And, AccComplianceSequenceSchema.XD_GC_Company, SQLComparisonOperator.Equal, Parent.XD_GC_Company);
				if (Parent.Factory.Exists(typeof(AccComplianceSequence), query))
				{
					Parent.XD_CodeInfo.AddError(Res.GetString("128E6395-832A-4A77-A8EA-4499950D4540", "A book already has the same code in this company."));
				}
			}
		}

		protected override void CheckXD_Description()
		{
			base.CheckXD_Description();
			CheckSecurity(Parent.XD_DescriptionInfo, Env.Security.ComplianceSequencesModifyDescription);

			if (!Parent.XD_DescriptionInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.XD_DescriptionInfo);
			}
		}

		protected override void CheckXD_Prefix()
		{
			base.CheckXD_Prefix();
			CheckSecurity(Parent.XD_PrefixInfo, Env.Security.ComplianceSequencesModifyPrefix);

			if (!Parent.XD_PrefixInfo.HasErrors())
			{
				if (Parent.XD_Prefix.IsEmpty)
				{
					var allowPrefixEmpty = ObjectFactory.Get<IAccountingCountryComplianceGlobalFactory>()
						.GetFeatureInterface<IComplianceInvoiceBookRegime>(Company.GC_RN_NKCountryCode)?
						.AllowSeriesPrefixEmpty(AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value) ?? true;
					if (allowPrefixEmpty)
					{
						Parent.XD_PrefixInfo.AddWarning(Res.GetString("595b50d4-209a-49fd-87d6-0214d8b7dc78", "Prefix is blank"));
					}
					else
					{
						Parent.XD_PrefixInfo.AddError(Res.GetString("CF74C235-80C2-48B5-919C-38B1A7EC71B4", "Please enter a Series Prefix"));
					}
				}
			}

			if (!Parent.XD_PrefixInfo.HasErrors())
			{
				if (CheckLengthOfPrefixPlusMaxNumberOfDigitsIsTooLong())
				{
					Parent.XD_PrefixInfo.AddError(lengthOfPrefixAndMaxNumDigitsTooLong);
				}
			}

			if (!Parent.XD_PrefixInfo.HasErrors())
			{
				if (CheckOverlapSeriesExists())
				{
					Parent.XD_PrefixInfo.AddError(numberSeriesMustNotOverlap);
				}
			}

			if (!Parent.XD_PrefixInfo.HasErrors())
			{
				if (CheckComplianceSeriesNumberPlusPrefixNotUnique())
				{
					Parent.XD_PrefixInfo.AddError(numberSeriesNotUniqueWhenCombineWithPrefix);
				}
			}

			if (!Parent.XD_PrefixInfo.HasErrors() && !Parent.IsAllocated)
			{
				var regex = ObjectFactory.Get<ICountryComplianceFactoryIntegration>()?.GetICountryComplianceInfo(Company.GC_RN_NKCountryCode)?.GetComplianceSequencePrefixRegex();
				var errorMessage = ObjectFactory.Get<ICountryComplianceFactoryIntegration>()?.GetICountryComplianceInfo(Company.GC_RN_NKCountryCode)?.GetComplianceSequencePrefixErrorMessage();

				if (!string.IsNullOrEmpty(regex) && !string.IsNullOrEmpty(errorMessage) && !new Regex(regex).IsMatch(Parent.XD_Prefix))
				{
					Parent.XD_PrefixInfo.AddError(errorMessage);
				}
			}
		}

		protected override void CheckXD_StartNumber()
		{
			base.CheckXD_StartNumber();
			CheckSecurity(Parent.XD_StartNumberInfo, Env.Security.ComplianceSequencesModifyStartNo);

			MandatoryValidation.CheckEntered(Parent.XD_StartNumberInfo);
			if (Parent.XD_StartNumber > Parent.XD_EndNumber && Parent.XD_EndNumber != 0)
			{
				Parent.XD_StartNumberInfo.AddError(Res.GetString("FB32D64D-DEF7-4137-AEE0-E18F6FDFBA5B", "Start number cannot be greater than end number"));
			}
			if (Parent.XD_StartNumber > Parent.XD_NextNumber && Parent.XD_NextNumber != 0)
			{
				Parent.XD_StartNumberInfo.AddError(Res.GetString("CF206C14-C190-4A4E-B487-5B06EBA8F03D", "Start number cannot be greater than next number"));
			}
			if (Parent.XD_StartNumber.ToString().Length > Parent.XD_MaximumNumberDigits)
			{
				Parent.XD_StartNumberInfo.AddError(Res.GetString("7456BD23-23EA-4993-887B-9323CE249C31", "The start number exceeds the number of digits ({0} digits) set for the sequence", Parent.XD_MaximumNumberDigits));
			}
			if (!Parent.XD_StartNumberInfo.HasErrors())
			{
				if (CheckOverlapSeriesExists())
				{
					Parent.XD_StartNumberInfo.AddError(numberSeriesMustNotOverlap);
				}
			}
			if (!Parent.XD_StartNumberInfo.HasErrors())
			{
				if (CheckComplianceSeriesNumberPlusPrefixNotUnique())
				{
					Parent.XD_StartNumberInfo.AddError(numberSeriesNotUniqueWhenCombineWithPrefix);
				}
			}
		}

		protected override void CheckXD_PrintingAuthorizationNumber()
		{
			base.CheckXD_PrintingAuthorizationNumber();
			CheckSecurity(Parent.XD_PrintingAuthorizationNumberInfo, Env.Security.ComplianceSequencesModifyPrintingAuthNo);

			if (ObjectFactory.Get<ICountryComplianceFactory>()?.GetITransactionAuthorizationNumber(Parent.Company.GC_RN_NKCountryCode)?.IsTransactionAuthorizationNumberEnabled(Parent.XD_GC_Company, ZDateTime.Now) ?? false)
			{
				if (Parent.IsInDatabase && Parent.XD_PrintingAuthorizationNumberInfo.HasChanges && Parent.PrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference)
				{
					Parent.XD_PrintingAuthorizationNumberInfo.AddError(Res.GetString("C369B7DA-E1BC-48DA-9C81-3B419FE6BAA2", "Editing or deleting the ATCUD code is no longer possible after the compliance book has been used"));
				}
				else if (Parent.XD_PrintingAuthorizationNumber.IsEmpty)
				{
					Parent.XD_PrintingAuthorizationNumberInfo.AddError(Res.GetString("ACA6C74F-6AE6-48C9-BEB4-A1D3F7D3BB80", "The ATCUD code is mandatory in the compliance books, fill it in the 'Print authorization number' field"));
				}
				else
				{
					var complianceSequencesValidationProvider = ObjectFactory.Get<ICountryComplianceFactory>().GetICountryComplianceInfoBase(Parent.Company.GC_RN_NKCountryCode) as IComplianceSequenceValidationProvider;
					if (complianceSequencesValidationProvider != null)
					{
						if (!complianceSequencesValidationProvider.IsPrintingAuthorizationNumberLengthValid(Parent.XD_PrintingAuthorizationNumber))
						{
							Parent.XD_PrintingAuthorizationNumberInfo.AddError(Res.GetString("103ba9fd-3488-4e8a-8496-001e38a23a88", @"Length is invalid, it must be at least 8 characters long. Please ensure you are entering the correct Series Validation Code issued by the AT for this book."));
						}
						else if (!complianceSequencesValidationProvider.IsPrintingAuthorizationNumberFormatValid(Parent.XD_PrintingAuthorizationNumber))
						{
							Parent.XD_PrintingAuthorizationNumberInfo.AddError(Res.GetString("937f6885-30d7-4537-9873-c15d416eb1ce", @"Value is invalid. Only capital consonant letters and numbers from 2 to 9 are allowed. No spaces allowed. Please ensure you are entering the correct Series Validation Code issued by the AT for this book."));
						}
					}
				}
			}
		}

		protected override void CheckXD_EndNumber()
		{
			base.CheckXD_EndNumber();
			CheckSecurity(Parent.XD_EndNumberInfo, Env.Security.ComplianceSequencesModifyLastNo);

			MandatoryValidation.CheckEntered(Parent.XD_EndNumberInfo);
			if (Parent.XD_EndNumber <= Parent.XD_StartNumber)
			{
				Parent.XD_EndNumberInfo.AddError(Res.GetString("06529B5D-3FD4-4CE4-AFB3-55737C390AB0", "End number must be greater than start number"));
			}
			if (Parent.XD_EndNumber < Parent.XD_NextNumber && Parent.XD_IsActive)
			{
				Parent.XD_EndNumberInfo.AddError(Res.GetString("36D783B5-B6F9-4755-B0C6-DE9B516D9E8C", "End number must be greater than or equal to next number"));
			}
			if (Parent.XD_EndNumber.ToString().Length > Parent.XD_MaximumNumberDigits)
			{
				Parent.XD_EndNumberInfo.AddError(Res.GetString("4D9B56D8-8D27-4181-8C40-677E25E40E48", "The end number exceeds the number of digits ({0} digits) set for the sequence", Parent.XD_MaximumNumberDigits));
			}
			if (!Parent.XD_EndNumberInfo.HasErrors())
			{
				if (CheckOverlapSeriesExists())
				{
					Parent.XD_EndNumberInfo.AddError(numberSeriesMustNotOverlap);
				}
			}
			if (!Parent.XD_EndNumberInfo.HasErrors())
			{
				if (CheckComplianceSeriesNumberPlusPrefixNotUnique())
				{
					Parent.XD_EndNumberInfo.AddError(numberSeriesNotUniqueWhenCombineWithPrefix);
				}
			}
		}

		protected override void CheckXD_NextNumber()
		{
			base.CheckXD_NextNumber();

			MandatoryValidation.CheckEntered(Parent.XD_NextNumberInfo);
			if (Parent.XD_NextNumber < Parent.XD_StartNumber || (Parent.XD_NextNumber > Parent.XD_EndNumber && Parent.XD_IsActive))
			{
				Parent.XD_NextNumberInfo.AddError(Res.GetString("E93B258A-B1E1-4F33-A197-2C6AF780F513", "Next number must be between start number and end number"));
			}
			if (Parent.XD_NextNumber.ToString().Length > Parent.XD_MaximumNumberDigits)
			{
				Parent.XD_NextNumberInfo.AddError(Res.GetString("C2E4C404-26CB-4F72-9EB7-F29A0796F6E3", "The next number exceeds the number of digits ({0} digits) set for the sequence", Parent.XD_MaximumNumberDigits));
			}
		}

		protected override void CheckXD_MaxChargesPerTransaction()
		{
			base.CheckXD_MaxChargesPerTransaction();

			if (Parent.XD_RollupBehaviourWhenMaxExceeded != Core.Constants.ComplianceRollupBehaviourType.MultiPageNoLimitation
				&& !Parent.XD_RollupBehaviourWhenMaxExceeded.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.XD_MaxChargesPerTransactionInfo);

				if (!Parent.XD_MaxChargesPerTransactionInfo.HasErrors())
				{
					if (Parent.XD_MaxChargesPerTransaction < 0 ||
						Parent.XD_MaxChargesPerTransaction > AccountingMasterFilesRegistry.Instance.MaximumNumberOfChargesToPrintPerComplianceDocument.GetValueWithoutFallback(Company.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty))
					{
						Parent.XD_MaxChargesPerTransactionInfo.AddError(Res.GetString("cbde4280-18b7-495c-844f-1ead6d0ce49a", "Max change lines is not in a valid range"));
					}
				}
			}
		}

		protected override void CheckXD_IsActive()
		{
			base.CheckXD_IsActive();

			if (Parent.XD_IsActive && Parent.XD_IsActiveInfo.OriginalValue.Equals(false))
			{
				var error = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetComplianceSequencePresentationProvider().CanReactivate(Parent);
				if (!string.IsNullOrEmpty(error))
				{
					Parent.XD_IsActiveInfo.AddError(error);
				}
			}
		}

		protected override void CheckXD_RollupBehaviourWhenMaxExceeded()
		{
			base.CheckXD_RollupBehaviourWhenMaxExceeded();

			if (!Parent.XD_SU_MenuItem.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.XD_RollupBehaviourWhenMaxExceededInfo);

				if (!Parent.XD_RollupBehaviourWhenMaxExceededInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidCode(Parent.XD_RollupBehaviourWhenMaxExceededInfo);
				}
			}
		}

		protected override void CheckXD_SU_MenuItem()
		{
			base.CheckXD_SU_MenuItem();
			CheckSecurity(Parent.XD_SU_MenuItemInfo, Env.Security.ComplianceSequencesModifyComplianceTemplate);

			if (!Parent.XD_SU_MenuItemInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidPK(Parent.XD_SU_MenuItemInfo);
			}
		}

		protected override void CheckXD_GB_BranchOwner()
		{
			base.CheckXD_GB_BranchOwner();
			CheckSecurity(Parent.XD_GB_BranchOwnerInfo, Env.Security.ComplianceSequencesModifyBranchCode);

			if (Parent.AllocationStrategy.IsBranchApplicable)
			{
				if (!Parent.XD_GB_BranchOwnerInfo.HasErrors())
				{
					MandatoryValidation.CheckEntered(Parent.XD_GB_BranchOwnerInfo);
				}

				if (!Parent.XD_GB_BranchOwnerInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidPK(Parent.XD_GB_BranchOwnerInfo);
				}
			}
			else
			{
				if (!Parent.XD_GB_BranchOwnerInfo.HasErrors())
				{
					MandatoryValidation.CheckNotEntered(Parent.XD_GB_BranchOwnerInfo);
				}
			}

			if (!Parent.XD_GB_BranchOwnerInfo.HasErrors() && CheckOverlappingBooks())
			{
				Parent.XD_GB_BranchOwnerInfo.AddError(overlappingBookExists);
			}
		}

		protected override void CheckXD_GE_Department()
		{
			base.CheckXD_GE_Department();
			CheckSecurity(Parent.XD_GE_DepartmentInfo, Env.Security.ComplianceSequencesModifyDepartmentCode);

			if (Parent.AllocationStrategy.IsDepartmentApplicable)
			{
				if (!Parent.XD_GE_DepartmentInfo.HasErrors())
				{
					MandatoryValidation.CheckEntered(Parent.XD_GE_DepartmentInfo);
				}

				if (!Parent.XD_GE_DepartmentInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidPK(Parent.XD_GE_DepartmentInfo);
				}
			}
			else
			{
				if (!Parent.XD_GE_DepartmentInfo.HasErrors())
				{
					MandatoryValidation.CheckNotEntered(Parent.XD_GE_DepartmentInfo);
				}
			}

			if (!Parent.XD_GE_DepartmentInfo.HasErrors() && CheckOverlappingBooks())
			{
				Parent.XD_GE_DepartmentInfo.AddError(overlappingBookExists);
			}
		}

		protected override void CheckXD_SQ_DocumentPrintQueue()
		{
			base.CheckXD_SQ_DocumentPrintQueue();
			CheckSecurity(Parent.XD_SQ_DocumentPrintQueueInfo, Env.Security.ComplianceSequencesModifyPrinterQueue);

			if (!Parent.XD_SU_MenuItem.IsEmpty && Parent.XD_RollupBehaviourWhenMaxExceeded != Core.Constants.ComplianceRollupBehaviourType.MultiPageNoLimitation)
			{
				MandatoryValidation.CheckEntered(Parent.XD_SQ_DocumentPrintQueueInfo);
			}
		}

		protected override void CheckXD_MaximumNumberDigits()
		{
			base.CheckXD_MaximumNumberDigits();
			CheckSecurity(Parent.XD_MaximumNumberDigitsInfo, Env.Security.ComplianceSequencesModifyMaxNumberDigits);

			MandatoryValidation.CheckEntered(Parent.XD_MaximumNumberDigitsInfo);
			if (!Parent.XD_MaximumNumberDigitsInfo.HasErrors())
			{
				if (Parent.XD_MaximumNumberDigits.ToZInt() > AccComplianceSequenceSchema.XD_StartNumber.Precision) // make 9 the max length?
				{
					Parent.XD_MaximumNumberDigitsInfo.AddError(Res.GetString("aa94f2d6-47c9-4deb-b3bc-104686e28b0f", "Max Number Digits exceeds the maximum length of 9"));
				}
			}

			if (!Parent.XD_MaximumNumberDigitsInfo.HasErrors() && CheckLengthOfPrefixPlusMaxNumberOfDigitsIsTooLong())
			{
				Parent.XD_MaximumNumberDigitsInfo.AddError(lengthOfPrefixAndMaxNumDigitsTooLong);
			}

			if (!Parent.XD_MaximumNumberDigitsInfo.HasErrors())
			{
				if (CheckOverlapSeriesExists())
				{
					Parent.XD_MaximumNumberDigitsInfo.AddError(numberSeriesMustNotOverlap);
				}
			}

			if (!Parent.XD_MaximumNumberDigitsInfo.HasErrors())
			{
				if (CheckComplianceSeriesNumberPlusPrefixNotUnique())
				{
					Parent.XD_MaximumNumberDigitsInfo.AddError(numberSeriesNotUniqueWhenCombineWithPrefix);
				}
			}

			if (!Parent.XD_MaximumNumberDigitsInfo.HasErrors() && Parent.XD_MaximumNumberDigitsInfo.HasChanges)
			{
				Parent.XD_MaximumNumberDigitsInfo.AddWarning(Res.GetString("31e544cf-7b02-43de-865e-7a812cd801ac", "Change will have effects on new sequence number only, any existing number will not be affected"));
			}
		}

		void CheckSecurity(ZPropertyInfo propertyInfo, SecurityCheckpoint securityCheckpoint)
		{
			if (!propertyInfo.HasErrors())
			{
				if (!securityCheckpoint.IsAllowed && !propertyInfo.Value.Equals(propertyInfo.OriginalValue))
				{
					propertyInfo.AddError(securityCheckpoint.ErrorMessageForNotAllowed);
				}
			}
		}

		bool CheckOverlappingBooks()
		{
			bool result = false;
			if (Parent.XD_IsActive && !Parent.XD_SequenceClass.IsEmpty && !Parent.AllocationStrategy.AllowOverlappingBooks)
			{
				result = Parent.CheckOverlappingBooks();
			}
			return result;
		}

		public bool CheckComplianceSeriesNumberPlusPrefixNotUnique()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(factory);
			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			parameters.Add(ZSqlParameter.New("@StartNum", Parent.StartNumberIncludingPrefixNumberPart, new SchemaStringColumn(Schema.GenericTableSchema, "StartNum", 0, SqlDbType.VarChar, null, true, 20)));
			parameters.Add(ZSqlParameter.New("@EndNum", Parent.EndNumberIncludingPrefixNumberPart, new SchemaStringColumn(Schema.GenericTableSchema, "EndNum", 0, SqlDbType.VarChar, null, true, 20)));
			parameters.Add(ZSqlParameter.New("@PrefixExcludesNum", Parent.PrefixExcludesNumberPart, new SchemaStringColumn(Schema.GenericTableSchema, "EndNum", 0, SqlDbType.VarChar, null, true, 20)));
			parameters.Add(ZSqlParameter.New("@Length", Parent.XD_Prefix.Length + Parent.XD_MaximumNumberDigits, new SchemaIntColumn(Schema.GenericTableSchema, (NoResString)"Length", 0, 0, false)));
			parameters.Add(ZSqlParameter.New("@SequenceClass", Parent.XD_SequenceClass, AccComplianceSequenceSchema.XD_SequenceClass));
			parameters.Add(ZSqlParameter.New("@CompanyPK", Parent.XD_GC_Company, AccComplianceSequenceSchema.XD_GC_Company));
			parameters.Add(ZSqlParameter.New("@RegexNonNumber", "%[^0-9]%", new SchemaStringColumn(Schema.GenericTableSchema, "RegexNonNumber", 0, SqlDbType.VarChar, null, true, 20)));
			parameters.Add(ZSqlParameter.New("@RegexNumber", "%[0-9]%", new SchemaStringColumn(Schema.GenericTableSchema, "RegexNumber", 0, SqlDbType.VarChar, null, true, 20)));
			parameters.Add(ZSqlParameter.New("@Space", " ", new SchemaStringColumn(Schema.GenericTableSchema, (NoResString)"Space", 0, SqlDbType.VarChar, null, true, 5)));
			parameters.Add(ZSqlParameter.New("@SequencePK", GetExcludeValidationPK(), AccComplianceSequenceSchema.PK));

			collection.Load(
@"IF EXISTS
(
	Select XD_Prefix, REVERSE(substring(REVERSE(XD_Prefix), 1, PATINDEX(@RegexNonNumber, REVERSE(XD_Prefix))-1)) + Replace(STR(XD_StartNumber, XD_MaximumNumberDigits, 0), @Space, 0) As StartNumber, 
	REVERSE(substring(REVERSE(XD_Prefix), 1, PATINDEX(@RegexNonNumber, REVERSE(XD_Prefix))-1)) + Replace(STR(XD_EndNumber, XD_MaximumNumberDigits, 0), @Space, 0) As EndNumber 
	From dbo.AccComplianceSequence 
	Where 
		-- case 1 - XD_Prefix contains mixture of letters and numbers
		LEN(XD_Prefix) + XD_MaximumNumberDigits = @Length AND
		PATINDEX(@RegexNonNumber, XD_Prefix) > 0 AND		
		(
		 (REVERSE(substring(REVERSE(XD_Prefix), 1, PATINDEX(@RegexNonNumber, REVERSE(XD_Prefix))-1)) + Replace(STR(XD_StartNumber, XD_MaximumNumberDigits, 0), @Space, 0) >= Convert(bigint, @StartNum) AND 
		 REVERSE(substring(REVERSE(XD_Prefix), 1, PATINDEX(@RegexNonNumber, REVERSE(XD_Prefix))-1)) + Replace(STR(XD_StartNumber, XD_MaximumNumberDigits, 0), @Space, 0) <= Convert(bigint, @EndNum))
		 OR
		 (REVERSE(substring(REVERSE(XD_Prefix), 1, PATINDEX(@RegexNonNumber, REVERSE(XD_Prefix))-1)) + Replace(STR(XD_EndNumber, XD_MaximumNumberDigits, 0), @Space, 0) >= Convert(bigint, @StartNum) AND 
		 REVERSE(substring(REVERSE(XD_Prefix), 1, PATINDEX(@RegexNonNumber, REVERSE(XD_Prefix))-1)) + Replace(STR(XD_EndNumber, XD_MaximumNumberDigits, 0), @Space, 0) <= Convert(bigint, @EndNum))
		 OR
		 (REVERSE(substring(REVERSE(XD_Prefix), 1, PATINDEX(@RegexNonNumber, REVERSE(XD_Prefix))-1)) + Replace(STR(XD_StartNumber, XD_MaximumNumberDigits, 0), @Space, 0) <= Convert(bigint, @StartNum) AND 
		 REVERSE(substring(REVERSE(XD_Prefix), 1, PATINDEX(@RegexNonNumber, REVERSE(XD_Prefix))-1)) + Replace(STR(XD_EndNumber, XD_MaximumNumberDigits, 0), @Space, 0) >= Convert(bigint, @EndNum))
		)
		AND substring(XD_Prefix, 1,  LEN(XD_Prefix) - PATINDEX(@RegexNonNumber, REVERSE(XD_Prefix)) + 1) = @PrefixExcludesNum
		AND XD_SequenceClass = @SequenceClass
		AND XD_GC_Company = @CompanyPK
		--AND XD_GB_BranchOwner = @BranchPK
		AND XD_Prefix <> ''
		AND XD_PK != @SequencePK
		
	UNION
		-- case 2 - XD_Prefix contains numbers only
		Select XD_Prefix, XD_Prefix + Replace(STR(XD_StartNumber, XD_MaximumNumberDigits, 0), @Space, 0) As StartNumber, 
		XD_Prefix + Replace(STR(XD_EndNumber, XD_MaximumNumberDigits, 0),  @Space, 0) As EndNumber
		From dbo.AccComplianceSequence 
		Where 
		LEN(XD_Prefix) + XD_MaximumNumberDigits = @Length AND
		PATINDEX(@RegexNonNumber, XD_Prefix) = 0 AND
		(
		  (XD_Prefix + Replace(STR(XD_StartNumber, XD_MaximumNumberDigits, 0), @Space, 0) >= Convert(bigint, @StartNum) AND 
			XD_Prefix + Replace(STR(XD_StartNumber, XD_MaximumNumberDigits, 0), @Space, 0) <= Convert(bigint, @EndNum))
			OR
			(XD_Prefix + Replace(STR(XD_EndNumber, XD_MaximumNumberDigits, 0), @Space, 0) >= Convert(bigint, @StartNum) AND 
			XD_Prefix + Replace(STR(XD_EndNumber, XD_MaximumNumberDigits, 0), @Space, 0) <= Convert(bigint, @EndNum))
			OR
			(XD_Prefix + Replace(STR(XD_StartNumber, XD_MaximumNumberDigits, 0), @Space, 0) <= Convert(bigint, @StartNum) AND 
			XD_Prefix + Replace(STR(XD_EndNumber, XD_MaximumNumberDigits, 0), @Space, 0) >= Convert(bigint, @EndNum))
		)
		AND XD_SequenceClass = @SequenceClass
		AND XD_GC_Company = @CompanyPK
		--AND XD_GB_BranchOwner = @BranchPK
		AND XD_Prefix <> ''
		AND XD_PK != @SequencePK

	UNION
		-- case 3 - XD_Prefix is blank
		Select '', Replace(STR(XD_StartNumber, XD_MaximumNumberDigits, 0), @Space, 0) As StartNumber, 
		Replace(STR(XD_EndNumber, XD_MaximumNumberDigits, 0),  @Space, 0) As EndNumber
		From dbo.AccComplianceSequence 
		Where 
		XD_MaximumNumberDigits = @Length AND		
		(
		  (Replace(STR(XD_StartNumber, XD_MaximumNumberDigits, 0), @Space, 0) >= Convert(bigint, @StartNum) AND 
			Replace(STR(XD_StartNumber, XD_MaximumNumberDigits, 0), @Space, 0) <= Convert(bigint, @EndNum))
			OR
			(Replace(STR(XD_EndNumber, XD_MaximumNumberDigits, 0), @Space, 0) >= Convert(bigint, @StartNum) AND 
			Replace(STR(XD_EndNumber, XD_MaximumNumberDigits, 0), @Space, 0) <= Convert(bigint, @EndNum))
			OR
			(Replace(STR(XD_StartNumber, XD_MaximumNumberDigits, 0), @Space, 0) <= Convert(bigint, @StartNum) AND 
			Replace(STR(XD_EndNumber, XD_MaximumNumberDigits, 0), @Space, 0) >= Convert(bigint, @EndNum))
		)
		AND XD_SequenceClass = @SequenceClass
		AND XD_GC_Company = @CompanyPK
		--AND XD_GB_BranchOwner = @BranchPK
		AND XD_PK != @SequencePK
		AND XD_Prefix = ''
		)
SELECT 1 AS IsExists ELSE SELECT 0 AS IsExists", parameters);

			return ((ZInt)collection[0]["IsExists"] == 1);
		}

		protected virtual ZGuid GetExcludeValidationPK()
		{
			return Parent.PK;
		}

		bool CheckLengthOfPrefixPlusMaxNumberOfDigitsIsTooLong()
		{
			return Parent.XD_Prefix.Length + Parent.XD_MaximumNumberDigits.ToZInt() > AutoAccTransactionHeader.Schema.AH_TransactionReferenceMaxLength;
		}

		bool CheckOverlapSeriesExists()
		{
			bool result = false;
			if (!Parent.XD_SequenceClass.IsEmpty && Parent.XD_EndNumber.ToString().Length <= Parent.XD_MaximumNumberDigits && Parent.XD_StartNumber.ToString().Length <= Parent.XD_MaximumNumberDigits)
			{
				var query = new ZQuery();
				query.AddToFilter(AccComplianceSequenceSchema.XD_SequenceClass, Parent.XD_SequenceClass);
				query.AddToFilter(AccComplianceSequenceSchema.XD_GC_Company, Parent.XD_GC_Company);

				query.AddToFilter(AccComplianceSequenceSchema.XD_Prefix, Parent.XD_Prefix);

				query.AddToFilter(AccComplianceSequenceSchema.XD_StartNumber, SQLComparisonOperator.LessThanOrEqualTo, Parent.XD_EndNumber);
				query.AddToFilter(AccComplianceSequenceSchema.XD_EndNumber, SQLComparisonOperator.GreaterThanOrEqualTo, Parent.XD_StartNumber);

				query.AddToFilter(AccComplianceSequenceSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				AccComplianceSequence[] collection = Parent.Factory.Load<AccComplianceSequence>(query);
				foreach (var sequence in collection)
				{
					if (sequence.StartNumberIncludingPrefixNumberPart.Length == Parent.StartNumberIncludingPrefixNumberPart.Length)
					{
						result = true;
					}

					break;
				}
			}
			return result;
		}

		protected override void CheckXD_StartDate()
		{
			CheckEnteredByAllocationDateOption(Parent.XD_StartDateInfo);

			if (!Parent.XD_StartDateInfo.HasErrors() && CheckValidDateShouldBeSmallerThanExpireDate())
			{
				Parent.XD_StartDateInfo.AddError(validDateShouldBeSmallerThanExpireDate);
			}

			if (!Parent.XD_StartDateInfo.HasErrors() && CheckOverlappingBooks())
			{
				Parent.XD_StartDateInfo.AddError(overlappingBookExists);
			}
		}

		protected override void CheckXD_ExpiryDate()
		{
			CheckEnteredByAllocationDateOption(Parent.XD_ExpiryDateInfo);

			if (!Parent.XD_ExpiryDateInfo.HasErrors() && CheckValidDateShouldBeSmallerThanExpireDate())
			{
				Parent.XD_ExpiryDateInfo.AddError(validDateShouldBeSmallerThanExpireDate);
			}

			if (!Parent.XD_ExpiryDateInfo.HasErrors() && CheckOverlappingBooks())
			{
				Parent.XD_ExpiryDateInfo.AddError(overlappingBookExists);
			}
		}

		void CheckEnteredByAllocationDateOption(ZPropertyInfo dateInfo)
		{
			if (Parent.IsComplianceNumberAllocationDateMandatory)
			{
				MandatoryValidation.CheckEntered(dateInfo);
			}
		}

		bool CheckValidDateShouldBeSmallerThanExpireDate()
		{
			return !Parent.XD_ExpiryDate.IsEmpty && !Parent.XD_StartDate.IsEmpty && Parent.XD_StartDate > Parent.XD_ExpiryDate;
		}
	}
}

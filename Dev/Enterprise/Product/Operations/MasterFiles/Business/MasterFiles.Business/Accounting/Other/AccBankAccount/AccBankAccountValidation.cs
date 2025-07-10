using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccBankAccountValidation : AutoAccBankAccountValidation
	{
		public AccBankAccountValidation(AutoAccBankAccount parent) : base(parent)
		{
		}

		AccBankAccount BankAccount => Parent as AccBankAccount;

		protected override void CheckAB_PaymentProvider()
		{
			base.CheckAB_PaymentProvider();
			if (BankAccount.IsEPaymentAccount)
			{
				MandatoryValidation.CheckEntered(Parent.AB_PaymentProviderInfo);
				ValidateUniqueProviderPerCompany();
			}
			if (!Parent.AB_PaymentProviderInfo.HasErrors() && !Parent.AB_PaymentProvider.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.AB_PaymentProviderInfo);
			}
		}

		void ValidateUniqueProviderPerCompany()
		{
			if (BankAccount.IsEPaymentAccount)
			{
				var filter = new ZQuery(AccBankAccountSchema.AB_GC, GlbCompany.CurrentCompany.PK);
				filter.AddToFilter(JoinCondition.And, AccBankAccountSchema.AB_PaymentProvider, SQLComparisonOperator.Equal, Parent.AB_PaymentProvider);
				filter.AddToFilter(JoinCondition.And, AccBankAccountSchema.AB_AccountType, SQLComparisonOperator.Equal, Parent.AB_AccountType);
				filter.AddToFilter(JoinCondition.And, AccBankAccountSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.Exists(typeof(AccBankAccount), filter))
				{
					Parent.AB_PaymentProviderInfo.AddError(Res.GetString("65a2e7ed-8174-4fe4-be60-8794a9ae4c66", "An E-Payment Account with Provider \"{0}\" already exists in the current login company.", Parent.AB_PaymentProvider));
				}
			}
		}

		protected override void CheckAB_SWIFT()
		{
			base.CheckAB_SWIFT();
			if (!Parent.AB_SWIFTInfo.HasErrors() && !Parent.AB_SWIFT.IsEmpty)
			{
				if (!AccValidationHelper.CheckBankSWIFT(Parent.AB_SWIFT))
				{
					Parent.AB_SWIFTInfo.AddError(Res.GetString("65591dfb-66a3-4571-a6b4-7fef4f62f2c7", "{0} is not a valid SWIFT Code.", Parent.AB_SWIFT));
				}
			}
		}

		protected override void CheckAB_AccountNumber()
		{
			base.CheckAB_AccountNumber();
			if (!BankAccount.IsCashAccount && !Parent.AB_AccountNumberInfo.HasErrors() && !Parent.AB_AccountNumber.IsEmpty)
			{
				AccValidationHelper.CheckBankIBAN(Parent.AB_AccountNumberInfo, Parent.AB_RN_NKBankAccountCountry);
			}

			if (!Parent.AB_AccountNumberInfo.HasErrors())
			{
				AccValidationHelper.CheckIbanForEUCountry(Parent.Factory, Parent.AB_AccountNumberInfo,
					Parent.AB_RN_NKBankAccountCountry);
			}
		}

		protected override void CheckAB_AccountEFTUserID()
		{
			base.CheckAB_AccountEFTUserID();
			if (!Parent.AB_AccountEFTUserIDInfo.HasErrors())
			{
				if (Parent.AB_AutoDDRFormat == Core.Constants.DDRFileFormat.WBC && Parent.AB_AccountEFTUserID.Length > 6)
				{
					Parent.AB_AccountEFTUserIDInfo.AddError(Res.GetString("2e6e06e3-c04f-463e-8738-4458b1fae92d", "The maximum length is 6 characters when DDR File Format is '{0}'.", Core.Constants.DDRFileFormat.WBC));
				}
			}
		}

		protected override void CheckAB_BankAccountName()
		{
			base.CheckAB_BankAccountName();
			MandatoryValidation.CheckEntered(Parent.AB_BankAccountNameInfo);
		}

		protected override void CheckAB_Code()
		{
			base.CheckAB_Code();
			MandatoryValidation.CheckEntered(Parent.AB_CodeInfo);
			if (!Parent.AB_CodeInfo.HasErrors())
			{
				CheckAB_CodeIsAlphaNumeric();
				CheckAB_CodeIsUnique();
			}
		}

		protected void CheckAB_CodeIsAlphaNumeric()
		{
			Regex nonAlphaNumericRegex = new Regex(@"^[a-z0-9]+\s*$", RegexOptions.IgnoreCase);
			if (!nonAlphaNumericRegex.IsMatch(Parent.AB_Code))
			{
				Parent.AB_CodeInfo.AddError(Res.GetString("f1679d81-0d0f-4958-9b68-548ff8be8029", "{0} is not a valid Code.", Parent.AB_Code));
			}
		}

		protected void CheckAB_CodeIsUnique()
		{
			var filter = new ZQuery(AccBankAccountSchema.AB_Code, Parent.AB_Code);
			filter.AddToFilter(JoinCondition.And, AccBankAccountSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

			if (Parent.Factory.Exists(typeof(AccBankAccount), filter))
			{
				Parent.AB_CodeInfo.AddError(Res.GetString("f1ac6236-8cfe-498d-ae90-a66b1d1f9d4c", "This Bank Code is already used by another Bank Account"));
			}
		}

		protected override void CheckAB_GB()
		{
			base.CheckAB_GB();
			if (!Parent.AB_GBInfo.HasErrors())
			{
				if (Parent.AB_GB != ZGuid.Empty)
				{
					AccChequeBook[] chequeBooks = GetAttachedChequeBooks(Parent.PK);

					if (chequeBooks != null)
					{
						foreach (AccChequeBook cheque in chequeBooks)
						{
							if (cheque.AK_GB != Parent.AB_GB)
							{
								Parent.AB_GBInfo.AddError(Res.GetString("88f9e7c9-0a91-4617-8245-3af86a216878", "Branch code cannot be change as this account is referenced by one or more check books with different branches"));
								return;
							}
						}
					}
				}
			}
		}

		protected virtual AccChequeBook[] GetAttachedChequeBooks(ZGuid pK)
		{
			ZQuery chequeFilter = new ZQuery(AccChequeBookSchema.AK_AB, pK);
			AccChequeBook[] chequeBooks = (AccChequeBook[])Parent.Factory.Load(typeof(AccChequeBook), chequeFilter);
			return chequeBooks;
		}

		protected override void CheckAB_Desc()
		{
			base.CheckAB_Desc();
			MandatoryValidation.CheckEntered(Parent.AB_DescInfo);
		}

		protected override void CheckAB_AG()
		{
			base.CheckAB_AG();

			if (!Parent.AB_AGInfo.HasErrors())
			{
				ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_AB, Parent.PK);
				filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_PostToGL, Core.Constants.BooleanTrueString);
				filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_GC, Parent.AB_GC);
				AccTransactionHeader header = Parent.Factory.LoadTop1<AccTransactionHeader>(filter);
				if (header != null)
				{
					if ((ZGuid)Parent.AB_AGInfo.OriginalValue != Parent.AB_AG)
					{
						AccGLHeader oldGlHeader = Parent.Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.PK, Parent.AB_AGInfo.OriginalValue));
						Parent.AB_AGInfo.AddError(Res.GetString("4bdf3e40-738c-4583-8366-19c3648d91b5", "The General Ledger account cannot be changed because there are transactions posted for this bank account. Reset the General Ledger account to {0}.", oldGlHeader.AG_AccountNum));
					}
				}
			}
			if (!Parent.AB_AGInfo.HasErrors())
			{
				ZQuery filter = new ZQuery(AccBankAccountSchema.AB_AG, Parent.AB_AG);
				filter.AddToFilter(JoinCondition.And, AccBankAccountSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				filter.AddToFilter(JoinCondition.And, AccBankAccountSchema.AB_GC, SQLComparisonOperator.Equal, Parent.AB_GC);
				AccBankAccount account = (AccBankAccount)Parent.Factory.LoadTop1(typeof(AccBankAccount), filter);
				if (account != null && account.PK != Parent.PK)
				{
					Parent.AB_AGInfo.AddError(Res.GetString("39011267-C147-4499-BE16-2458968D75F7", "This GL Account is already used by another Bank Account in the current login company."));
				}
			}
		}

		protected override void CheckAB_BankName()
		{
			base.CheckAB_BankName();
			if (!BankAccount.IsCashAccount)
			{
				MandatoryValidation.CheckEntered(Parent.AB_BankNameInfo);
			}
		}

		protected override void CheckAB_BankAddress()
		{
			base.CheckAB_BankAddress();
			if (!BankAccount.IsCashAccount)
			{
				MandatoryValidation.CheckEntered(Parent.AB_BankAddressInfo);
			}
		}

		protected override void CheckAB_BSB()
		{
			base.CheckAB_BSB();

			if (BankAccount.IsCashAccount)
			{
				return;
			}

			if (Parent.AB_AllowAutoDDR)
			{
				MandatoryValidation.CheckEntered(Parent.AB_BSBInfo);
			}

			if (!Parent.AB_BSBInfo.HasErrors())
			{
				if (BankAccount != null)
				{
					if (!BankAccount.IsValidBSBNumber(Parent.AB_BSB))
					{
						Parent.AB_BSBInfo.AddError(BankAccount.GetInvalidBSBNumberErrorMessage());
					}
					else if (Parent.AB_AutoDDRFormat == Constants.DDRFileFormat.BNZ ||
						Parent.AB_AutoDDRFormat == Constants.DDRFileFormat.ASB ||
						Parent.AB_AutoDDRFormat == Constants.DDRFileFormat.WNZ ||
						(Parent.AB_AutoDDRFormat == Constants.DDRFileFormat.ANZ &&
						GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.NewZealand))
					{
						if (!Regex.IsMatch(Parent.AB_BSB, @"^[0-9]{6}$"))
						{
							Parent.AB_BSBInfo.AddError(Res.GetString("84D15B37-D73A-4d9d-88FF-EE2D89737625", "The BSB when using the '{0}' Direct Debit System must have the pattern 'XXXXXX'", Parent.AB_AutoDDRFormat));
						}
					}
				}
			}
		}

		protected override void CheckAB_AccountNum()
		{
			base.CheckAB_AccountNum();
			if (!BankAccount.IsEPaymentAccount && !BankAccount.IsCashAccount)
			{
				MandatoryValidation.CheckEntered(Parent.AB_AccountNumInfo);
			}
			if (!Parent.AB_AccountNumInfo.HasErrors() && Parent.AB_AllowAutoDDR)
			{
				if (Parent.AB_AutoDDRFormat == Constants.DDRFileFormat.WNZ)
				{
					if (Parent.AB_AccountNum.Length > 12)
					{
						Parent.AB_AccountNumInfo.AddError(Res.GetString("4d21597f-7436-4df2-a186-8605f9408225", "The account number must be 12 or less characters in length"));
					}
				}
				else if (Parent.AB_AutoDDRFormat == Constants.DDRFileFormat.ASB)
				{
					if (Parent.AB_AccountNum.Length != 9)
					{
						Parent.AB_AccountNumInfo.AddError(Res.GetString("f0d9d5a8-d3fc-4217-b0c6-091d7eef9bd1", "The account number must be 9 characters in length for ASB Bank DDR format"));
					}
				}
				else if (Parent.AB_AutoDDRFormat == Constants.DDRFileFormat.BCS)
				{
					if (!Regex.IsMatch(Parent.AB_AccountNum, @"^[0-9]{8}$"))
					{
						Parent.AB_AccountNumInfo.AddError(Res.GetString("8f82d2dc-092b-4e47-ad2e-be0f77d9e050", "The account number must comprise exactly 8 numeric characters for BCS Bank DDR format"));
					}
				}
				else if (Parent.AB_AccountNum.Length > 9 && Parent.AB_AutoDDRFormat != Constants.DDRFileFormat.CUS && Parent.AB_AutoDDRFormat != Constants.DDRFileFormat.BTM)
				{
					Parent.AB_AccountNumInfo.AddError(Res.GetString("7EFB6A54-6D4E-497f-8E25-37E63E9E4D20", "The account number must be 9 or less characters in length"));
				}
			}

			if (BankAccount.IsCreditCardOrLinkedAccount && BankAccount.AB_DebitCreditCardNumber.IsEmpty)
			{
				BankAccount.AB_AccountNumInfo.AddError(Res.GetString("4cc4bb4e-42ed-455a-b44f-07ef983affc6", "Click the 'Enter Credit Card Number' button to enter a credit card number."));
			}
		}

		protected override void CheckAB_FullAccountNumber()
		{
			base.CheckAB_FullAccountNumber();
			CountryComplianceFactory.GetIBankAccountValidator(Parent.AB_RN_NKBankAccountCountry)?.ValidateFullAccountNumber(Parent.AB_FullAccountNumberInfo);
		}

		protected override void CheckAB_BankAbbreviation()
		{
			base.CheckAB_BankAbbreviation();
			if (!BankAccount.IsEPaymentAccount && !BankAccount.IsCashAccount)
			{
				MandatoryValidation.CheckEntered(Parent.AB_BankAbbreviationInfo);
			}
		}

		protected override void CheckAB_AutoDDRFormat()
		{
			base.CheckAB_AutoDDRFormat();
			if (Parent.AB_AllowAutoDDR)
			{
				MandatoryValidation.CheckEntered(Parent.AB_AutoDDRFormatInfo);
				if (!Parent.AB_AutoDDRFormatInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidCode(Parent.AB_AutoDDRFormatInfo);
				}
			}
		}

		protected override void CheckAB_RN_NKBankAccountCountry()
		{
			base.CheckAB_RN_NKBankAccountCountry();
			MandatoryValidation.CheckEntered(Parent.AB_RN_NKBankAccountCountryInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AB_RN_NKBankAccountCountryInfo);
		}

		protected override void CheckAB_RX_NKAccountCurrency()
		{
			base.CheckAB_RX_NKAccountCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.AB_RX_NKAccountCurrencyInfo);

			if (Parent.AB_RX_NKAccountCurrencyInfo.HasChanges)
			{
				if (Parent.Factory.LoadTop1<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_AB, Parent.PK).AddToFilter(AccTransactionHeaderSchema.AH_GC, Parent.AB_GC)) != null)
				{
					Parent.AB_RX_NKAccountCurrencyInfo.AddError(Res.GetString("df7f0bfa-358c-457f-ad04-9ffca5a654aa", "Bank Account Currency cannot be changed. At least one posted transaction references this Bank Account and its existing currency"));
				}
				else if (DoesHotChequeExistForThisBank())
				{
					Parent.AB_RX_NKAccountCurrencyInfo.AddError(Res.GetString("39450695-dfcf-40c8-b531-f90bd7966f97", "Bank Account Currency cannot be changed. At least one hot cheque references this Bank Account and its existing currency"));
				}
				else if (DoesConsolCostExistForThisBank())
				{
					Parent.AB_RX_NKAccountCurrencyInfo.AddError(Res.GetString("349e8932-4ffb-41e4-a22a-bac09653ab37", "Bank Account Currency cannot be changed. At least one consol cost references this Bank Account and its existing currency"));
				}
				else if (Parent.Factory.LoadTop1<JobCharge>(new ZQuery(JobChargeSchema.JR_AB, Parent.PK)) != null)
				{
					Parent.AB_RX_NKAccountCurrencyInfo.AddError(Res.GetString("45dc8b87-d3e7-4928-830b-67babb61303b", "Bank Account Currency cannot be changed. At least one job charge references this Bank Account and its existing currency"));
				}
				else if (Parent.Factory.LoadTop1<AccPaymentApproval>(new ZQuery(AccPaymentApprovalSchema.AV_AB, Parent.PK)) != null)
				{
					Parent.AB_RX_NKAccountCurrencyInfo.AddError(Res.GetString("eab5b593-141b-4c59-83d0-f322e458d47f", "Bank Account Currency cannot be changed. At least one payment approval references this Bank Account and its existing currency"));
				}
			}

			if (!Parent.AB_RX_NKAccountCurrencyInfo.HasErrors())
			{
				switch (Parent.AB_AccountType)
				{
					case AccountTypeCodeDescriptionPairList.Codes.BNK:
					case AccountTypeCodeDescriptionPairList.Codes.CCD:
					case AccountTypeCodeDescriptionPairList.Codes.LNK:
					case AccountTypeCodeDescriptionPairList.Codes.CSH:
						MandatoryValidation.CheckEntered(Parent.AB_RX_NKAccountCurrencyInfo);
						break;
				}
			}
		}

		bool DoesHotChequeExistForThisBank()
		{
			DynamicBusinessObjectCollection hotCheques = new DynamicBusinessObjectCollection(Parent.Factory);
			string query = string.Format("SELECT TOP 1 {0} FROM {1} JOIN {2} ON {3} = {4} WHERE {5} = @BankPK"
							, AccHotChequeSchema.Constants.PK, AccHotChequeSchema.Constants.TableName, AccChequeBookSchema.Constants.TableName, AccHotChequeSchema.AQ_AK.Name, AccChequeBookSchema.PK.Name, AccChequeBookSchema.AK_AB.Name);
			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add("@BankPK", Parent.PK, AccHotChequeSchema.AQ_AK);
			hotCheques.Load(query, @params);
			return hotCheques.Count > 0;
		}

		bool DoesConsolCostExistForThisBank()
		{
			DynamicBusinessObjectCollection hotCheques = new DynamicBusinessObjectCollection(Parent.Factory);
			string query = string.Format("SELECT TOP 1 {0} FROM {1} WHERE {2} = @BankPK", JobConsolCostSchema.Constants.PK, JobConsolCostSchema.Constants.TableName, JobConsolCostSchema.E6_AB_BankAccount.Name);
			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add("@BankPK", Parent.PK, JobConsolCostSchema.E6_AB_BankAccount);
			hotCheques.Load(query, @params);
			return hotCheques.Count > 0;
		}

		protected override void CheckAB_ChequeNumDigits()
		{
			base.CheckAB_ChequeNumDigits();
			if (Parent.AB_ChequeNumDigits < 1 || Parent.AB_ChequeNumDigits > AccTransactionHeaderSchema.AH_ChequeOrReference.MaxLength)
			{
				Parent.AB_ChequeNumDigitsInfo.AddError(Res.GetString("8a30ea42-0c2e-4a34-93b4-641a408a4469", "The number of check digits must be between 1 and {0}", AccTransactionHeaderSchema.AH_ChequeOrReference.MaxLength.ToString()));
			}
		}

		protected override void CheckAB_IsDefaultReceiptBankAccount()
		{
			base.CheckAB_IsDefaultReceiptBankAccount();
			ZQuery filter = new ZQuery(AccBankAccountSchema.AB_RX_NKAccountCurrency, Parent.AB_RX_NKAccountCurrency);
			filter.AddToFilter(JoinCondition.And, AccBankAccountSchema.AB_GC, SQLComparisonOperator.Equal, Parent.AB_GC);
			filter.AddToFilter(JoinCondition.And, AccBankAccountSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

			if (Parent.AB_GB != ZGuid.Empty) // want all bank accounts with the same branch
			{
				filter.AddToFilter(JoinCondition.And, AccBankAccountSchema.AB_GB, SQLComparisonOperator.Equal, Parent.AB_GB);
			}
			else // want all bank accounts with null branch
			{
				filter.AddToFilter(JoinCondition.And, AccBankAccountSchema.AB_GB, SQLComparisonOperator.Equal, null);
			}
			filter.AddToFilter(JoinCondition.And, AccBankAccountSchema.AB_IsDefaultReceiptBankAccount, SQLComparisonOperator.Equal, ZBool.True);
			AccBankAccount account = (AccBankAccount)Parent.Factory.LoadTop1(typeof(AccBankAccount), filter);
			if (account != null && Parent.AB_IsDefaultReceiptBankAccount)
			{
				Parent.AB_IsDefaultReceiptBankAccountInfo.AddError(Res.GetString("172b3139-0e49-4330-b321-c18e0b86520c", "You cannot set more than one bank as default bank account for a particular currency and specific branch or all branch"));
			}
		}

		protected override void CheckAB_AccountType()
		{
			base.CheckAB_AccountType();
			MandatoryValidation.CheckEntered(Parent.AB_AccountTypeInfo);
		}

		protected override void CheckAB_IsActive()
		{
			base.CheckAB_IsActive();

			if (!Parent.AB_IsActive && Parent.AB_IsDefaultReceiptBankAccount)
			{
				Parent.AB_IsActiveInfo.AddError(DefaultReceiptBankAccountCannotBeInactive);
			}

			if (!Parent.AB_IsActive)
			{
				addBankToThisAccountWarning();
				addDefaultBankAccountForDebtorGroupWarning();
				addDefaultBankAccountInRegistryWarning();
			}
		}

		internal static string DefaultReceiptBankAccountCannotBeInactive
		{
			get
			{
				return Res.GetString("23cddec2-7946-471a-91ed-bd0cedcba7bf", "It is not possible to set a Bank Account to inactive while it is set as a default receipt bank account.");
			}
		}

		protected void addBankToThisAccountWarning()
		{
			string warningMessage = BankToThisAccountWarningMessage(Parent);
			if (!string.IsNullOrEmpty(warningMessage))
			{
				Parent.AB_IsActiveInfo.AddWarning(warningMessage);
			}
		}

		internal static string BankToThisAccountWarningMessage(AutoAccBankAccount parent)
		{
			string query = @"select " + OrgHeaderSchema.Constants.OH_Code + @"
							from " + OrgCompanyDataSchema.Constants.SqlSchemaName + "." + OrgCompanyDataSchema.Constants.TableName + @"
							inner join " + OrgHeaderSchema.Constants.SqlSchemaName + "." + OrgHeaderSchema.Constants.TableName + @"
								on " + OrgCompanyDataSchema.Constants.OB_OH + " = " + OrgHeaderSchema.Constants.PK + @"
							where " + OrgCompanyDataSchema.Constants.OB_AB_ARPayToAccount + @" = @BankAccount
							order by " + OrgHeaderSchema.Constants.OH_Code;

			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			parameters.Add("@BankAccount", parent.PK.ToGuid(), AccBankAccountSchema.PK);

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(parent.Factory);
			collection.Load(query, parameters);

			if (collection.Count > 0)
			{
				string message = Res.GetString("ffc7d7e9-790b-4a76-8ce3-b2bd0859ebe1", "This Bank Account is nominated as the 'Bank to This Account' for the following Organizations:") + " ";
				string organisations = string.Empty;
				int count = 0;
				foreach (DynamicBusinessObject result in collection)
				{
					count++;
					if (count > 5)
					{
						organisations += Res.GetString("f3c88d57-0f8c-43f9-b068-a1b238ffdf58", ", ... To view a complete list of Organizations using this Bank Account view the Organization - A/R Profile report under Maintain -> Reference Files -> Reports");
						break;
					}

					if (!string.IsNullOrEmpty(organisations))
					{
						organisations += ", ";
					}

					organisations += result[OrgHeaderSchema.OH_Code];
				}
				organisations += ".";
				return message + organisations;
			}
			else
			{
				return string.Empty;
			}
		}

		protected void addDefaultBankAccountForDebtorGroupWarning()
		{
			string warningMessage = DefaultBankAccountForDebtorGroupWarningMessage(Parent);
			if (!string.IsNullOrEmpty(warningMessage))
			{
				Parent.AB_IsActiveInfo.AddWarning(warningMessage);
			}
		}

		internal static string DefaultBankAccountForDebtorGroupWarningMessage(AutoAccBankAccount parent)
		{
			string query = @"select distinct " + OrgDebtorGroupSchema.Constants.OJ_Code + @"
							from " + OrgDebtorGroupBankCurrentOverrideSchema.Constants.SqlSchemaName + "." + OrgDebtorGroupBankCurrentOverrideSchema.Constants.TableName + @"
							inner join " + OrgDebtorGroupBankDefaultSchema.Constants.SqlSchemaName + "." + OrgDebtorGroupBankDefaultSchema.Constants.TableName + @"
								on " + OrgDebtorGroupBankDefaultSchema.Constants.PK + " = " + OrgDebtorGroupBankCurrentOverrideSchema.Constants.PB_P6 + @"
							inner join " + OrgDebtorGroupSchema.Constants.SqlSchemaName + "." + OrgDebtorGroupSchema.Constants.TableName + @"
								on " + OrgDebtorGroupBankDefaultSchema.Constants.P6_OJ + " = " + OrgDebtorGroupSchema.Constants.PK + @"
							where " + OrgDebtorGroupBankCurrentOverrideSchema.Constants.PB_AB + @" = @BankAccount
							union
							select distinct " + OrgDebtorGroupSchema.Constants.OJ_Code + @"
							from " + OrgDebtorGroupBankDefaultSchema.Constants.SqlSchemaName + "." + OrgDebtorGroupBankDefaultSchema.Constants.TableName + @"
							inner join " + OrgDebtorGroupSchema.Constants.SqlSchemaName + "." + OrgDebtorGroupSchema.Constants.TableName + @"
								on " + OrgDebtorGroupBankDefaultSchema.Constants.P6_OJ + " = " + OrgDebtorGroupSchema.Constants.PK + @"
							where " + OrgDebtorGroupBankDefaultSchema.Constants.P6_AB + @" = @BankAccount
							order by " + OrgDebtorGroupSchema.Constants.OJ_Code;

			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			parameters.Add("@BankAccount", parent.PK.ToGuid(), AccBankAccountSchema.PK);

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(parent.Factory);
			collection.Load(query, parameters);

			if (collection.Count > 0)
			{
				string message = Res.GetString("6e85a94a-90f0-4d67-ab93-0689db17d546", "This Bank Account is nominated as a Default Bank Account for the following Debtor Groups:") + " ";
				string debtorGroups = string.Empty;
				int count = 0;
				foreach (DynamicBusinessObject result in collection)
				{
					count++;
					if (count > 10)
					{
						debtorGroups += ", ..";
						break;
					}

					if (!string.IsNullOrEmpty(debtorGroups))
					{
						debtorGroups += ", ";
					}

					debtorGroups += result[OrgDebtorGroupSchema.OJ_Code];
				}
				debtorGroups += ".";
				return message + debtorGroups;
			}
			else
			{
				return string.Empty;
			}
		}

		protected void addDefaultBankAccountInRegistryWarning()
		{
			string warningMessage = DefaultBankAccountInRegistryWarningMessage(Parent);
			if (!string.IsNullOrEmpty(warningMessage))
			{
				Parent.AB_IsActiveInfo.AddWarning(warningMessage);
			}
		}

		internal static string DefaultBankAccountInRegistryWarningMessage(AutoAccBankAccount parent)
		{
			BankAccountBasedOnCurrencyCollection collection = OrganisationsDataRegistry.Instance.BankAccountsBasedOnCurrency.Value;

			foreach (BankAccountBasedOnCurrency bankAccountBasedOnCurrency in collection)
			{
				if (bankAccountBasedOnCurrency.BankAccount == parent.PK)
				{
					return Res.GetString("596ae709-fa9d-4566-a793-13488ec4d0c9", "This Bank Account is nominated as a Default Bank Account in the following registry setting: Organizations > Default Values > Bank Accounts for AR Documents and Receipting.");
				}
			}
			return string.Empty;
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCreditCardExpiryMonth();
			ValidateCreditCardExpiryYear();
		}

		protected override void CheckAB_DebitCreditCardName()
		{
			base.CheckAB_DebitCreditCardName();
			AccBankAccount bankAccount = Parent as AccBankAccount;
			if (bankAccount.IsCreditCardOrLinkedAccount)
			{
				MandatoryValidation.CheckEntered(bankAccount.AB_DebitCreditCardNameInfo);
			}
		}

		public virtual void ValidateCreditCardExpiryMonth()
		{
			AccBankAccount bankAccount = Parent as AccBankAccount;
			ValidateCalculatedProperty(bankAccount.CreditCardExpiryMonthInfo);
		}

		protected virtual void CheckCreditCardExpiryMonth()
		{
			AccBankAccount bankAccount = Parent as AccBankAccount;
			if (bankAccount.IsCreditCardOrLinkedAccount)
			{
				ListValidation.ErrorIfInvalidCode(bankAccount.CreditCardExpiryMonthInfo);
				if (bankAccount.CreditCardExpiryMonth == string.Empty)
				{
					bankAccount.CreditCardExpiryMonthInfo.AddError(Res.GetString("4008a55f-2102-4de9-8f31-69910ce4a2b3", "Enter a credit card expiry month."));
				}
				checkCreditCardExpiry(bankAccount.CreditCardExpiryMonthInfo);
			}
		}

		public virtual void ValidateCreditCardExpiryYear()
		{
			AccBankAccount bankAccount = Parent as AccBankAccount;
			ValidateCalculatedProperty(bankAccount.CreditCardExpiryYearInfo);
		}

		protected virtual void CheckCreditCardExpiryYear()
		{
			AccBankAccount bankAccount = Parent as AccBankAccount;
			if (bankAccount.IsCreditCardOrLinkedAccount)
			{
				int result;
				if (!int.TryParse(bankAccount.CreditCardExpiryYear, out result))
				{
					bankAccount.CreditCardExpiryYearInfo.AddError(Res.GetString("dbf3f91a-e068-4555-8fab-fb738de26c5a", "Credit Card Expiry Year must be an integer."));
				}
				if (bankAccount.CreditCardExpiryYear.Length != 2)
				{
					bankAccount.CreditCardExpiryYearInfo.AddError(Res.GetString("48475223-ef14-486e-bb48-e96b26444ebf", "Credit Card Expiry Year must be 2 digits."));
				}
				checkCreditCardExpiry(bankAccount.CreditCardExpiryYearInfo);
			}
		}

		protected void checkCreditCardExpiry(ZPropertyInfo info)
		{
			if (Parent.AB_DebitCreditCardExpiry.Length == 4)
			{
				string month = Parent.AB_DebitCreditCardExpiry.Substring(0, 2);
				string year = "20" + Parent.AB_DebitCreditCardExpiry.Substring(2, 2);
				int monthInt = -1;
				int.TryParse(month, out monthInt);
				int yearInt = -1;
				int.TryParse(year, out yearInt);
				if ((monthInt != -1) && (yearInt != -1))
				{
					if ((ZDateTime.Now.Year > yearInt)
						|| ((ZDateTime.Now.Year == yearInt) && (ZDateTime.Now.Month > monthInt)))
					{
						ZString message = Res.GetString("8c723584-6a64-4b8c-a156-85a33944a3de", "Credit card has expired.");
						info.AddWarning(message);
					}
				}
			}
		}
	}
}

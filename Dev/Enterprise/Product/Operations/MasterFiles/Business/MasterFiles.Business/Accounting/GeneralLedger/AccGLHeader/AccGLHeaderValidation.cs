using System;
using System.Data;
using System.Text;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccGLHeaderValidation : AutoAccGLHeaderValidation
	{
		public AccGLHeaderValidation(AutoAccGLHeader parent)
			: base(parent)
		{
		}

		protected new AccGLHeader Parent
		{
			get { return base.Parent as AccGLHeader; }
		}

		protected override void CheckAG_Column()
		{
			MandatoryValidation.CheckEntered(Parent.AG_ColumnInfo, Res.GetString("85283f49-06db-445f-b154-9a6f9c984616", "GL Section"));
			ListValidation.ErrorIfInvalidCode(Parent.AG_ColumnInfo);
		}

		protected override void CheckAG_DisallowDirectPosting()
		{
			if (!Parent.AG_DisallowDirectPosting)
			{
				if (IsUsedInElectronicProcessingChargeDisbursementClearingAccountRegistry())
				{
					Parent.AG_DisallowDirectPostingInfo.AddError(Res.GetString("5BC063E9-3B98-41A4-8948-715F317E5EEB", "This is a system defined GL Account used in Electronic Processing Fee management and cannot be allowed for Direct Posting."));
				}
			}
			else
			{
				if (IsUsedInElectronicProcessingChargePayableClearingAccountRegistry())
				{
					Parent.AG_DisallowDirectPostingInfo.AddError(Res.GetString("9E289AE8-D3EE-448A-AAD6-FB15AAE7EF7A", "This is a system defined GL Account used in Electronic Processing Fee management and must be allowed for Direct Posting."));
				}
			}
		}

		protected override void CheckAG_AccountNum()
		{
			MandatoryValidation.CheckEntered(Parent.AG_AccountNumInfo);
			if (!Parent.AG_DebitCreditInfo.HasErrors())
			{
				GLAccountCommonValidator.ValidateAccountNumber();
			}
		}

		protected GLAccountCommonValidationHelper fValidationHelper;
		protected GLAccountCommonValidationHelper GLAccountCommonValidator
		{
			get
			{
				if (fValidationHelper == null)
				{
					fValidationHelper = new AccGLHeaderValidationHelper(Parent, Parent.Factory);
				}
				return fValidationHelper;
			}
		}

		protected override void CheckAG_DebitCredit()
		{
			MandatoryValidation.CheckEntered(Parent.AG_DebitCreditInfo);

			if (!Parent.AG_DebitCreditInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.AG_DebitCreditInfo);
			}
		}

		protected override void CheckAG_Description()
		{
			MandatoryValidation.CheckEntered(Parent.AG_DescriptionInfo);
			TranslatableDataFieldAttribute.Validate(Parent.AG_DescriptionInfo);
		}

		protected override void CheckAG_CashFlowType()
		{
			if (!Parent.AG_CashFlowTypeInfo.HasErrors())
			{
				if (Parent.AG_AccountType == Core.Constants.AccountType.BalanceSheetAccount || Parent.AG_AccountType == Core.Constants.AccountType.ProfitAndLossAccount)
				{
					MandatoryValidation.CheckEntered(Parent.AG_CashFlowTypeInfo);
					ListValidation.ErrorIfInvalidCode(Parent.AG_CashFlowTypeInfo);
					if (Parent.AG_CashFlowType == CashFlowCodeLists.Codes.CSH)
					{
						var bankAccount = Parent.Factory.LoadTop1<AccBankAccount>(new ZQuery(AccBankAccountSchema.AB_AG, Parent.PK));
						if (bankAccount == null)
						{
							Parent.AG_CashFlowTypeInfo.AddWarning(Res.GetString("ebeeabc3-9657-4a9c-948e-f42e85858916", "You have selected CSH. Please note that 'CSH' should only be used for GL Accounts relating to Maintain > Accounts > Bank Accounts."));
						}
					}
					else if (Parent.AG_CashFlowType == CashFlowCodeLists.Codes.EXX)
					{
						string query = @"SELECT COUNT(*) FROM dbo.StmData
										WHERE SD_GuidValue = @GuidValue 
										AND SD_Name = 'GL_BANKCURRENCY_ADJUSTMENT_ACCOUNT'";
						using (DbCommand command = Db.Connection.Command(query)) // Required Bank Currency Adjustment Account Check, No access to AccountingConfigurationRegistry
						{
							command.AddParameter("@GuidValue", SqlDbType.UniqueIdentifier, Parent.PK.ToGuid());
							int count = (int)command.ExecuteScalar();
							if (count == 0)
							{
								Parent.AG_CashFlowTypeInfo.AddWarning(Res.GetString("9cdbf609-f4e0-496b-9e67-57d188f6e456", "EXX can not be chosen for accounts other than the one defined in registry Bank Currency Adjustment Account"));
							}
						}
					}
				}
				else if (!Parent.AG_CashFlowType.IsEmpty)
				{
					Parent.AG_CashFlowTypeInfo.AddError(Res.GetString("9a14d18b-00c2-49ad-8b94-6a6cc95f6747", "cash flow type is only applicable for 'P&L' and 'BSH' type GL Account."));
				}
			}
		}

		protected override void CheckAG_AccountType()
		{
			MandatoryValidation.CheckEntered(Parent.AG_AccountTypeInfo);
			if (Parent.AG_AccountType != Core.Constants.AccountType.BalanceSheetAccount && Parent.AG_AccountType != Core.Constants.AccountType.ProfitAndLossAccount &&
				(IsUsedInElectronicProcessingChargeDisbursementClearingAccountRegistry() || IsUsedInElectronicProcessingChargePayableClearingAccountRegistry()))
			{
				Parent.AG_AccountTypeInfo.AddError(Res.GetString("0B5D89AC-6B51-447D-BA83-D526C9270431", "This is a system defined GL Account used in Electronic Processing Fee management and Account Type must be set to \"BSH - Balance Sheet\" or \"P&L - Profit and Loss\"."));
			}

			if (!Parent.AG_AccountTypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.AG_AccountTypeInfo);
			}

			if (!Parent.AG_AccountTypeInfo.HasErrors())
			{
				if (Parent.AG_AccountType != Core.Constants.AccountType.Alternate && IsAlternateNumAlreadyUsed(Parent.PK))
				{
					Parent.AG_AccountTypeInfo.AddError(Res.GetString("0ab2bf59-d1c2-42e0-9019-d7267a0858ac", "This GL Account is used as alternate account and can't be changed to another type."));
				}
			}

			if (!Parent.AG_AccountTypeInfo.HasErrors() && Parent.IsInDatabase && Parent.AG_AccountTypeInfo.HasChanges &&
				(Parent.AG_AccountTypeInfo.OriginalValue.ToString() == Core.Constants.AccountType.ProfitAndLossAccount && Parent.AG_AccountTypeInfo.Value.ToString() != Core.Constants.AccountType.BalanceSheetAccount ||
				 Parent.AG_AccountTypeInfo.OriginalValue.ToString() == Core.Constants.AccountType.BalanceSheetAccount && Parent.AG_AccountTypeInfo.Value.ToString() != Core.Constants.AccountType.ProfitAndLossAccount))
			{
				string linesErrorMessage = CheckIsUsedInTransactionLines();
				string otherRefErrorMessage = string.Empty;

				if (string.IsNullOrEmpty(linesErrorMessage))
				{
					string message = CheckIsUsedInBankAccounts();
					if (!string.IsNullOrEmpty(message))
					{
						otherRefErrorMessage += message + "; ";
					}

					message = CheckIsUsedInChargeCodes();
					if (!string.IsNullOrEmpty(message))
					{
						otherRefErrorMessage += message + "; ";
					}

					message = CheckIsUsedInAccChargeGLPostingOverride();
					if (!string.IsNullOrEmpty(message))
					{
						otherRefErrorMessage += message + "; ";
					}

					message = CheckIsUsedInRegistry();
					if (!string.IsNullOrEmpty(message))
					{
						otherRefErrorMessage += message + "; ";
					}
				}

				if (!string.IsNullOrEmpty(linesErrorMessage) || !string.IsNullOrEmpty(otherRefErrorMessage))
				{
					Parent.AG_AccountTypeInfo.AddError((Res.GetString("0f33c671-cec0-412d-8cb4-f5e89a4dcfd0", "This account cannot be converted to account type {0} because it is currently in use as a {1}. It is used by", Parent.AG_AccountTypeInfo.Value, Parent.AG_AccountTypeInfo.OriginalValue) +
						" " + (!string.IsNullOrEmpty(linesErrorMessage) ? linesErrorMessage : otherRefErrorMessage)).TrimEnd(';', ' ') + ".");
				}
			}

			if (!Parent.AG_AccountTypeInfo.HasErrors() && Parent.IsInDatabase && Parent.AG_AccountTypeInfo.HasChanges &&
				Parent.AG_AccountTypeInfo.OriginalValue.ToString() == Core.Constants.AccountType.Note)
			{
				string linesErrorMessage = CheckIsUsedInTransactionLinesForNoteJournal();
				if (!string.IsNullOrEmpty(linesErrorMessage))
				{
					var converErrorMessage = new ZStringBuilder();
					converErrorMessage.Append(Res.GetString("D8BDB269-35DC-41C2-A789-51470E55E4CC", "This account cannot be converted to account type {0} because it is currently in use as a {1}. It is used by ", Parent.AG_AccountTypeInfo.Value, Parent.AG_AccountTypeInfo.OriginalValue));
					converErrorMessage.Append(linesErrorMessage);
					converErrorMessage.Append(".");
					Parent.AG_AccountTypeInfo.AddError(converErrorMessage.ToString());
				}
			}

			ValidateAG_IsGlobal();
		}

		bool IsGlobalAccountTypeUsed
		{
			get
			{
				return Parent.AG_AccountType == Core.Constants.AccountType.Consolidation || Parent.AG_AccountType == Core.Constants.AccountType.Header
					|| Parent.AG_AccountType == Core.Constants.AccountType.Alternate || Parent.AG_AccountType == Core.Constants.AccountType.Total;
			}
		}

		string CheckIsUsedInTransactionLinesForNoteJournal()
		{
			return CheckIsUsedInTransactionLines(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLNoteJournal);
		}

		string CheckIsUsedInTransactionLines(SchemaColumn additionalFilterSchemaColumn = null, string additionalFilterValue = "")
		{
			StringBuilder result = new StringBuilder();

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Parent.Factory);

			string selectSQL = @"select {0}, COUNT(*) as Count, MIN({1}) as Min, MAX({1}) as Max  
from {2} inner join {3} on {4} = {5} inner join {6} on {7} = {8}
where {9} = @AGPK {10}
group by {0}";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@AGPK", Parent.PK, AccGLHeaderSchema.PK);

			string additionalFilterQuery = string.Empty;
			if (additionalFilterSchemaColumn != null)
			{
				additionalFilterQuery = String.Format((NoResString)"and {0} = @filterValue", additionalFilterSchemaColumn.Name);
				sqlParams.Add("@filterValue", additionalFilterValue, additionalFilterSchemaColumn);
			}

			var completeSQL = string.Format(selectSQL,
				 GlbCompanySchema.Constants.GC_Code,
				 AccTransactionLinesSchema.Constants.AL_PostDate,
				 AccTransactionLinesSchema.Constants.TableName,
				 AccTransactionHeaderSchema.Constants.TableName,
				 AccTransactionLinesSchema.Constants.AL_AH,
				 AccTransactionHeaderSchema.Constants.PK,
				 GlbCompanySchema.Constants.TableName,
				 AccTransactionHeaderSchema.Constants.AH_GC,
				 GlbCompanySchema.Constants.PK,
				 AccTransactionLinesSchema.Constants.AL_AG,
				 additionalFilterQuery);

			collection.Load(completeSQL, sqlParams);

			foreach (DynamicBusinessObject row in collection)
			{
				result.Append(Res.GetString("059633b4-ae49-4f43-a8c3-70d17eddaf7c", "transaction lines in company {0} posted between {1} and {2}",
					(ZString)row[GlbCompanySchema.Constants.GC_Code],
					(ZDateTime)row["Min"],
					(ZDateTime)row["Max"]) + ", ");
			}

			return result.ToString().TrimEnd(',', ' ');
		}

		string CheckIsUsedInBankAccounts()
		{
			StringBuilder result = new StringBuilder();

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Parent.Factory);

			string selectSQL = String.Format(@"select {0} from {1} where {2} = '{3}'",
				AccBankAccountSchema.Constants.AB_Code,
				AccBankAccountSchema.Constants.TableName,
				AccBankAccountSchema.Constants.AB_AG,
				Parent.PK);

			collection.Load(selectSQL);

			foreach (DynamicBusinessObject row in collection)
			{
				result.Append(Res.GetString("f60e8870-7e27-4cb1-802a-1bb1dd5e328e", "bank account '{0}'", (ZString)row[AccBankAccountSchema.Constants.AB_Code]) + ", ");
			}

			return result.ToString().TrimEnd(',', ' ');
		}

		string CheckIsUsedInChargeCodes()
		{
			StringBuilder result = new StringBuilder();

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Parent.Factory);

			string selectSQL = String.Format(@"select {0}, {1} 
from {2} inner join {3} on {4} = {5}
where {6} = '{7}' or {8} = '{7}' or {9} = '{7}' or {10} = '{7}'",
				 GlbCompanySchema.Constants.GC_Code,
				 AccChargeCodeSchema.Constants.AC_Code,
				 AccChargeCodeSchema.Constants.TableName,
				 GlbCompanySchema.Constants.TableName,
				 AccChargeCodeSchema.Constants.AC_GC,
				 GlbCompanySchema.Constants.PK,
				 AccChargeCodeSchema.Constants.AC_AG_RevenueAccount,
				 Parent.PK,
				 AccChargeCodeSchema.Constants.AC_AG_WIPAccount,
				 AccChargeCodeSchema.Constants.AC_AG_CostAccount,
				 AccChargeCodeSchema.Constants.AC_AG_AccrualAccount);

			collection.Load(selectSQL);

			foreach (DynamicBusinessObject row in collection)
			{
				result.Append(Res.GetString("08f38772-5a65-4d33-8f68-e2dd85dc61ea", "charge code {0} in company {1}",
					(ZString)row[AccChargeCodeSchema.Constants.AC_Code],
					(ZString)row[GlbCompanySchema.Constants.GC_Code]) + ", ");
			}

			return result.ToString().TrimEnd(',', ' ');
		}

		string CheckIsUsedInAccChargeGLPostingOverride()
		{
			StringBuilder result = new StringBuilder();

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Parent.Factory);

			string selectSQL = String.Format(@"select {0}, {1} 
from {2} inner join {3} on {4} = {5} inner join {6} on {7} = {8}
where {9} = '{10}' or {11} = '{10}' or {12} = '{10}' or {13} = '{10}'",
				 GlbCompanySchema.Constants.GC_Code,
				 AccChargeCodeSchema.Constants.AC_Code,
				 AccChargeGLPostingOverrideSchema.Constants.TableName,
				 AccChargeCodeSchema.Constants.TableName,
				 AccChargeGLPostingOverrideSchema.Constants.Y1_AC,
				 AccChargeCodeSchema.Constants.PK,
				 GlbCompanySchema.Constants.TableName,
				 AccChargeCodeSchema.Constants.AC_GC,
				 GlbCompanySchema.Constants.PK,
				 AccChargeGLPostingOverrideSchema.Constants.Y1_AG_REV,
				 Parent.PK,
				 AccChargeGLPostingOverrideSchema.Constants.Y1_AG_WIP,
				 AccChargeGLPostingOverrideSchema.Constants.Y1_AG_ACR,
				 AccChargeGLPostingOverrideSchema.Constants.Y1_AG_CST);

			collection.Load(selectSQL);

			foreach (DynamicBusinessObject row in collection)
			{
				result.Append(Res.GetString("84026fd6-2851-4836-8f05-18c1975a90d1", "charge posting override for charge code {0} in company {1}",
					(ZString)row[AccChargeCodeSchema.Constants.AC_Code],
					(ZString)row[GlbCompanySchema.Constants.GC_Code]) + ", ");
			}

			return result.ToString().TrimEnd(',', ' ');
		}

		string CheckIsUsedInRegistry()
		{
			StringBuilder result = new StringBuilder();
			string[] registryItemCaptions = ObjectFactory.Get<IAccounting>().GetCaptionsOfRegistryItemsUsingGLHeader(Parent.PK.ToGuid());

			if (registryItemCaptions.Length > 0)
			{
				result.Append(Res.GetString("4a507918-191d-40f1-9cac-73fdf78baeb0", "accounting configuration registry item(s)") + " ");
				foreach (string name in registryItemCaptions)
				{
					result.Append("'" + name + "', ");
				}
			}

			return result.ToString().TrimEnd(',', ' ');
		}

		bool IsUsedInElectronicProcessingChargeDisbursementClearingAccountRegistry() => Parent.PK.ToGuid() == ObjectFactory.Get<IAccountingRegistryProvider>().ElectronicProcessingChargeDisbursementClearingAccount;

		bool IsUsedInElectronicProcessingChargePayableClearingAccountRegistry() => Parent.PK.ToGuid() == ObjectFactory.Get<IAccountingRegistryProvider>().ElectronicProcessingChargePayableClearingAccount;

		protected override void CheckAG_AG_AlternateNum()
		{
			if (!Parent.AG_AG_AlternateNumInfo.HasErrors() && IsAlternateNumAlreadyUsed(Parent.AG_AG_AlternateNum))
			{
				Parent.AG_AG_AlternateNumInfo.AddError(Res.GetString("9537a5fa-7afe-45a6-ae39-587ce8a36c89", "This alternate number is already used for another GL Account"));
			}
		}

		bool IsAlternateNumAlreadyUsed(ZGuid gLAccountPK)
		{
			ZQuery query = new ZQuery(AccGLHeaderSchema.AG_AG_AlternateNum, gLAccountPK);
			query.AddToFilter(AccGLHeaderSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			AccGLHeader glAccount = Parent.Factory.LoadTop1<AccGLHeader>(query);
			return (glAccount != null);
		}

		protected override void CheckAG_AG_PercentNum()
		{
			ListValidation.ErrorIfInvalidPK(Parent.AG_AG_PercentNumInfo);

			if (!Parent.AG_AG_PercentNumInfo.HasErrors() && !Parent.AG_AccountNumInfo.HasErrors())
			{
				if (Parent.PercentNum != null)
				{
					ZDecimal accountNumber = GLAccountCommonValidationHelper.ConvertToDecimal(Parent.AG_Calc_AccountNumberWithPrefix);
					ZDecimal percentAccountNum = GLAccountCommonValidationHelper.ConvertToDecimal(Parent.PercentNum.AG_Calc_AccountNumberWithPrefix);

					if (accountNumber != -1 && percentAccountNum != -1 && accountNumber >= percentAccountNum)
					{
						ZString errorMessage = Res.GetString("f8252ab4-d5fd-4336-8433-cd9e5bafd1ff", "Percentage number ({0}) must be greater than Account number ({1}).",
							Parent.PercentNum.AG_Calc_AccountNumberWithPrefix, Parent.AG_Calc_AccountNumberWithPrefix);

						Parent.AG_AG_PercentNumInfo.AddError(errorMessage);
					}
				}
			}
		}

		protected override void CheckAG_AG_ConsolidationNum()
		{
			ListValidation.ErrorIfInvalidPK(Parent.AG_AG_ConsolidationNumInfo);

			if (!Parent.AG_AG_ConsolidationNumInfo.HasErrors() && !Parent.AG_AccountNumInfo.HasErrors())
			{
				if (Parent.ConsolidationNum != null)
				{
					ZDecimal accountNumber = GLAccountCommonValidationHelper.ConvertToDecimal(Parent.AG_Calc_AccountNumberWithPrefix);
					ZDecimal consolidationNumber = GLAccountCommonValidationHelper.ConvertToDecimal(Parent.ConsolidationNum.AG_Calc_AccountNumberWithPrefix);

					if (accountNumber != -1 && consolidationNumber != -1 && accountNumber >= consolidationNumber)
					{
						ZString errorMessage = Res.GetString("a4a964a1-1ca9-4f1e-9278-a4792bd58c53", "Consolidation number ({0}) must be greater than Account number ({1})",
							Parent.ConsolidationNum.AG_Calc_AccountNumberWithPrefix, Parent.AG_Calc_AccountNumberWithPrefix);

						Parent.AG_AG_ConsolidationNumInfo.AddError(errorMessage);
					}
				}
			}
		}

		protected override void CheckAG_AG_HeaderDependsOnTotal()
		{
			ListValidation.ErrorIfInvalidPK(Parent.AG_AG_HeaderDependsOnTotalInfo);

			if (!Parent.AG_AccountNumInfo.HasErrors() && !Parent.AG_AG_HeaderDependsOnTotalInfo.HasErrors())
			{
				if (Parent.HeaderDependsOnTotal != null)
				{
					ZDecimal accountNum = GLAccountCommonValidationHelper.ConvertToDecimal(Parent.AG_Calc_AccountNumberWithPrefix);
					ZDecimal totalAccountNum = GLAccountCommonValidationHelper.ConvertToDecimal(Parent.HeaderDependsOnTotal.AG_Calc_AccountNumberWithPrefix);

					if (totalAccountNum != -1 && accountNum != -1 && accountNum >= totalAccountNum)
					{
						ZString errorMessage = Res.GetString("afb79b2e-09e3-4318-821b-793e3ddc1d12", "TTL Account ({0}) must be greater than current Account number ({1}).",
							Parent.HeaderDependsOnTotal.AG_Calc_AccountNumberWithPrefix, Parent.AG_Calc_AccountNumberWithPrefix);

						Parent.AG_AG_HeaderDependsOnTotalInfo.AddError(errorMessage);
					}
				}

				ZQuery filter = new ZQuery(AccGLHeaderSchema.AG_AG_HeaderDependsOnTotal, SQLComparisonOperator.Equal, Parent.AG_AG_HeaderDependsOnTotal);
				filter.AddToFilter(AccGLHeaderSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				AccGLHeader topTotalReference = Parent.Factory.LoadTop1<AccGLHeader>(filter);

				if (topTotalReference != null && topTotalReference.PK != Parent.PK)
				{
					Parent.AG_AG_HeaderDependsOnTotalInfo.AddError(ErrorMsgMoreThanOneHDRPointingToTTL);
				}
			}
		}

		public static string ErrorMsgMoreThanOneHDRPointingToTTL
		{
			get { return Res.GetString("c9b1b869-5f85-45db-bb43-5ab32acdf40c", "No more than 1 HDR Account can point to a TTL Account"); }
		}

		protected override void CheckAG_TotalLevel()
		{
			if (!Parent.AG_TotalLevelInfo.HasErrors() && !Parent.AG_TotalLevelInfo.ReadOnly)
			{
				if (Parent.AG_TotalLevel < 1 || Parent.AG_TotalLevel > 999)
				{
					Parent.AG_TotalLevelInfo.AddError(Res.GetString("ffbdbdbb-5ecc-48e5-b28f-034f3fd36657", "Total Level should be between 1 and 999"));
				}
			}
		}

		protected override void CheckAG_PrintSequence()
		{
			if (!Parent.AG_PrintSequenceInfo.HasErrors())
			{
				if (Parent.AG_PrintSequence < 0 || Parent.AG_PrintSequence > 999)
				{
					Parent.AG_PrintSequenceInfo.AddError(Res.GetString("6595cfa7-eff1-4495-9517-f8fbb99f860e", "Print Sequence should be between 0 and 999"));
				}
			}
		}

		protected override void CheckAG_IsGlobal()
		{
			if (!Parent.AG_IsGlobal)
			{
				if (IsUsedInElectronicProcessingChargeDisbursementClearingAccountRegistry() || IsUsedInElectronicProcessingChargePayableClearingAccountRegistry())
				{
					Parent.AG_IsGlobalInfo.AddError(Res.GetString("6C6D2C71-0C97-4499-8671-926B6842BD40", "This is a system defined GL Account used in Electronic Processing Fee management and cannot be set to Company Level. Please set it to Global."));
				}
				if (Parent.AG_ControlAccount)
				{
					Parent.AG_IsGlobalInfo.AddError(Res.GetString("546df9d8-748f-490f-a435-e278bf8eaadf", "A Control Account must be valid for all companies. Please tick the Is Global checkbox."));
				}
				if (Parent.IsGLAccountUsedInSystemLevelRegistry || Parent.IsGLAccountUsedInCompanyLevelRegistry)
				{
					Parent.AG_IsGlobalInfo.AddError(Res.GetString("5cd6cfed-b470-4060-a9a7-2e21175a9860", "All GL Accounts set up in the Registry, such as Control, Linked, P/L Appropriation accounts, etc. must be a Global GL Account."));
				}
				if (Parent.IsGLAccountUsedForGlobalChargeCode)
				{
					Parent.AG_IsGlobalInfo.AddError(Res.GetString("b796683b-f55f-42d5-9415-d4489851764b", "GL Accounts used on Global Charge Codes must be a Global GL Account."));
				}
				if (IsGlobalAccountTypeUsed)
				{
					Parent.AG_IsGlobalInfo.AddError(Res.GetString("435b843b-aca5-460a-ab67-9385af813be0", "TTL, HDR, CLN and ALT account types must be Global."));
				}

				if (Parent.CompanyFilters.Count == 0)
				{
					Parent.AG_IsGlobalInfo.AddError(Res.GetString("2cdab617-9f54-4de4-b3e0-02e5a1828bf5", "Company filters must be set up."));
				}
			}
		}

		protected override void CheckAG_IsActive()
		{
			if (!Parent.AG_IsActive)
			{
				if (IsUsedInElectronicProcessingChargeDisbursementClearingAccountRegistry() || IsUsedInElectronicProcessingChargePayableClearingAccountRegistry())
				{
					Parent.AG_IsActiveInfo.AddError(Res.GetString("EEC977D2-BDBC-4C54-A4CE-6556A35544D2", "This is a system defined GL Account used in Electronic Processing Fee management and cannot be set to inactive. Please set it to active."));
				}
				if (IsLinkedToActiveChargeCode())
				{
					Parent.AG_IsActiveInfo.AddError(Res.GetString("d996ff78-95ce-4ac1-9e48-026b82eb8087", "This GL Account is used on active Charge Codes."));
				}
			}
		}

		bool IsLinkedToActiveChargeCode()
		{
			return IsLinkedToActiveChargeCodeAsGLAccount() || IsLinkedToActiveChargeCodeAsGLPostingOverride();
		}

		bool IsLinkedToActiveChargeCodeAsGLAccount()
		{
			var queryUnion = GetGLHeaderSubQuery(typeof(AccChargeCode), AccChargeCodeSchema.PK, AccChargeCodeSchema.AC_AG_CostAccount, Parent.PK);
			queryUnion.AddAsUnionQuery(GetGLHeaderSubQuery(typeof(AccChargeCode), AccChargeCodeSchema.PK, AccChargeCodeSchema.AC_AG_AccrualAccount, Parent.PK));
			queryUnion.AddAsUnionQuery(GetGLHeaderSubQuery(typeof(AccChargeCode), AccChargeCodeSchema.PK, AccChargeCodeSchema.AC_AG_RevenueAccount, Parent.PK));
			queryUnion.AddAsUnionQuery(GetGLHeaderSubQuery(typeof(AccChargeCode), AccChargeCodeSchema.PK, AccChargeCodeSchema.AC_AG_WIPAccount, Parent.PK));

			var chargeQuery = new ZDBOnlyQuery(typeof(AccChargeCode));
			chargeQuery.AddSubQuery(queryUnion, JoinCondition.And);

			return Parent.Factory.ExistsInDatabase(AccChargeCodeSchema.Constants.TableName, chargeQuery);
		}

		bool IsLinkedToActiveChargeCodeAsGLPostingOverride()
		{
			var queryUnion = GetGLHeaderSubQuery(typeof(AccChargeGLPostingOverride), AccChargeGLPostingOverrideSchema.Y1_AC, AccChargeGLPostingOverrideSchema.Y1_AG_CST, Parent.PK);
			queryUnion.AddAsUnionQuery(GetGLHeaderSubQuery(typeof(AccChargeGLPostingOverride), AccChargeGLPostingOverrideSchema.Y1_AC, AccChargeGLPostingOverrideSchema.Y1_AG_ACR, Parent.PK));
			queryUnion.AddAsUnionQuery(GetGLHeaderSubQuery(typeof(AccChargeGLPostingOverride), AccChargeGLPostingOverrideSchema.Y1_AC, AccChargeGLPostingOverrideSchema.Y1_AG_REV, Parent.PK));
			queryUnion.AddAsUnionQuery(GetGLHeaderSubQuery(typeof(AccChargeGLPostingOverride), AccChargeGLPostingOverrideSchema.Y1_AC, AccChargeGLPostingOverrideSchema.Y1_AG_WIP, Parent.PK));

			var chargeQuery = new ZDBOnlyQuery(typeof(AccChargeCode));
			chargeQuery.AddToFilter(AccChargeCodeSchema.AC_IsActive, true);
			chargeQuery.AddSubQuery(queryUnion, JoinCondition.And);

			return Parent.Factory.ExistsInDatabase(AccChargeCodeSchema.Constants.TableName, chargeQuery);
		}

		ZDBOnlySubQuery GetGLHeaderSubQuery(Type objetcType, SchemaColumn fKColumn, SchemaColumn queryColumn, ZGuid value)
		{
			var subQuery = new ZDBOnlySubQuery(objetcType, fKColumn);
			subQuery.AddFilterAndZSQLParameterCollection(ZString.Format("@glHeaderPK IS NOT NULL and {0} = @glHeaderPK", queryColumn.Name), new ZSqlParameterCollection(ZSqlParameter.New("@glHeaderPK", value, queryColumn)));

			if (queryColumn.TableName == AccChargeCode.Schema.TableName)
			{
				subQuery.AddToFilter(AccChargeCodeSchema.AC_IsActive, true);
			}
			return subQuery;
		}

		protected override void CheckAG_ControlAccount()
		{
			var isLinkedToBankAccount = Parent.Factory.LoadTop1<AccBankAccount>(new ZQuery(AccBankAccountSchema.AB_AG, Parent.PK)) != null;
			if (!Parent.AG_ControlAccount)
			{
				if (isLinkedToBankAccount)
				{
					Parent.AG_ControlAccountInfo.AddError(Res.GetString("4F39ECF3-34DD-4C91-8450-689810EDA4A8", "GL Accounts used on Bank Accounts must be a Control Account."));
				}
			}
			else
			{
				if (IsUsedInElectronicProcessingChargeDisbursementClearingAccountRegistry() || IsUsedInElectronicProcessingChargePayableClearingAccountRegistry())
				{
					Parent.AG_ControlAccountInfo.AddError(Res.GetString("0E2CFDE5-550C-48C7-99EE-41477C2F23DC", "This is a system defined GL Account used in Electronic Processing Fee management and cannot be set to Control Account."));
				}
			}
		}

		protected override void CheckAG_StatisticalUnits()
		{
			if (Parent.AG_AccountType == Core.Constants.AccountType.Note)
			{
				MandatoryValidation.CheckEntered(Parent.AG_StatisticalUnitsInfo, Res.GetString("57420B7A-9CBA-43B5-BD94-A01F0FDACFA1", "GL Units"));

				if (!Parent.AG_StatisticalUnitsInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidCode(Parent.AG_StatisticalUnitsInfo);
				}

				if (!Parent.AG_StatisticalUnitsInfo.HasErrors() && Parent.IsInDatabase && Parent.AG_StatisticalUnitsInfo.HasChanges)
				{
					var linesErrorMessage = CheckIsUsedInTransactionLinesForNoteJournal();
					if (!string.IsNullOrEmpty(linesErrorMessage))
					{
						var converErrorMessage = new ZStringBuilder();
						converErrorMessage.Append(Res.GetString("BB46E12B-3E4C-4AA3-8576-478AA99FD32F", "This account cannot be converted to units {0} because it is currently in use as a {1}. It is used by ", Parent.AG_StatisticalUnitsInfo.Value, Parent.AG_StatisticalUnitsInfo.OriginalValue));
						converErrorMessage.Append(linesErrorMessage);
						converErrorMessage.Append(".");
						Parent.AG_StatisticalUnitsInfo.AddError(converErrorMessage.ToString());
					}
				}
			}
		}
	}
}

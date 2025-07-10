using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	public class PermitTestDataHelper
	{
		public PermitTestDataHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		public BaseCusPermitHeader CreatePermitHeader(ZString country, ZGuid permitHolder, ZString permitNumber, ZDate startDate, ZDate endDate, ZString qtyValIndicator, ZString type, string subType = "", decimal qtyBalance = 0.0m, decimal valBalance = 0.0m, string uom = "", string reference = PermitTransactionTypeList.Codes.OBL, ZGuid? appliesToAddressPK = null)
		{
			var permitTypeDecider = new CusPermitHeaderTypeDecider();
			var permitType = permitTypeDecider.GetTypeForCountryCode(country);

			var permit = (BaseCusPermitHeader)factory.NewWithValidTestData(permitType);
			permit.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Permit;
			permit.CPH_RN_NKCountryCode = country;
			permit.CPH_Number = permitNumber;
			permit.CPH_StartDate = startDate;
			permit.CPH_EndDate = endDate;
			permit.CPH_OH_PermitHolder = permitHolder;
			permit.CPH_QtyValIndicator = qtyValIndicator;
			permit.CPH_Type = type;
			permit.CPH_SubType = subType;
			permit.CPH_UnitOfMeasure = uom;
			if (appliesToAddressPK.HasValue)
			{
				permit.CPH_OA_AppliesTo = appliesToAddressPK.Value;
			}

			if (qtyBalance > ZDecimal.Zero || valBalance > ZDecimal.Zero)
			{
				var transaction = permit.CusPermitLineTransactions.AddNew();
				transaction.CPL_Reference = reference;
				transaction.CPL_TranQty = qtyBalance;
				transaction.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
				transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
				transaction.CPL_TranValue = valBalance;
				transaction.CPL_TransactionDate = startDate;
			}

			return permit;
		}

		public BaseCusPermitRule CreatePermitRule(BaseCusPermitHeader permit, ZString ruleCode, ZString valueFrom, ZString valueTo)
		{
			var rule = permit.CusPermitRules.AddNew();
			rule.CPR_RuleCode = ruleCode;
			rule.CPR_ValueFrom = valueFrom;
			rule.CPR_ValueTo = valueTo;
			return rule;
		}

		public BaseCusPermitRuleException CreatePermitRuleException(BaseCusPermitRule rule, ZString valueFrom, ZString valueTo)
		{
			var exception = rule.CusPermitRuleExceptions.AddNew();
			exception.CPE_ValueFrom = valueFrom;
			exception.CPE_ValueTo = valueTo;
			return exception;
		}

		public BaseCusPermitLineTransaction CreatePermitLineTransaction(BaseCusPermitHeader permit, ZString reference, ZString comment, ZString appId, ZString transactionCategory, ZString transactionType, ZDecimal value, ZDecimal quantity, string transactionStatus = null, int referenceNumberLine = 0)
		{
			var lineTransaction = permit.CusPermitLineTransactions.AddNew();
			lineTransaction.CPL_Comment = comment;
			lineTransaction.CPL_Reference = reference;
			lineTransaction.CPL_AppId = appId;
			lineTransaction.CPL_TranQty = quantity;
			lineTransaction.CPL_TransactionCategory = transactionCategory;
			lineTransaction.CPL_TransactionType = transactionType;
			lineTransaction.CPL_TranValue = value;
			if (transactionStatus != null)
			{
				lineTransaction.CPL_TransactionStatus = transactionStatus;
			}
			lineTransaction.CPL_ReferenceNumberLine = referenceNumberLine;

			return lineTransaction;
		}

		public static void AssertPermitLineTransaction(BaseCusPermitLineTransaction lineTransaction, ZString reference, ZString comment, ZString appId, ZString transactionCategory, ZString transactionType, ZDecimal value, ZDecimal quantity, string transationStatus = null, int referenceNumberLine = 0)
		{
			TestCase.CombineAssertions(() =>
			{
				TestCase.AssertEquals("CPL_Reference", reference, lineTransaction.CPL_Reference);
				TestCase.AssertEquals("CPL_Comment", comment, lineTransaction.CPL_Comment);
				TestCase.AssertEquals("CPL_AppId", appId, lineTransaction.CPL_AppId);
				TestCase.AssertEquals("CPL_TransactionCategory", transactionCategory, lineTransaction.CPL_TransactionCategory);
				TestCase.AssertEquals("CPL_TransactionType", transactionType, lineTransaction.CPL_TransactionType);
				TestCase.AssertEquals("CPL_TranQty", quantity, lineTransaction.CPL_TranQty);
				TestCase.AssertEquals("CPL_TranValue", value, lineTransaction.CPL_TranValue);
				if (transationStatus != null)
				{
					TestCase.AssertEquals("CPL_TransactionStatus", transationStatus, lineTransaction.CPL_TransactionStatus);
				}
				TestCase.AssertEquals("CPL_ReferenceNumberLine", referenceNumberLine, lineTransaction.CPL_ReferenceNumberLine);
			});
		}

		public ZQuery GetPermitLineTransactionQuery(ZString reference, ZString comment, ZString appId, ZString transactionCategory, ZString transactionType)
		{
			var query = new ZQuery(CusPermitLineTransactionSchema.CPL_Reference, reference);
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_Comment, comment);
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_AppId, appId);
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_TransactionCategory, transactionCategory);
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_TransactionType, transactionType);
			return query;
		}
	}
}

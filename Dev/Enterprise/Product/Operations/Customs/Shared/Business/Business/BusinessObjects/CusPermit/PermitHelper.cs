using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public static class PermitHelper
	{
		public static void RollbackPermitTransactionsIfSendingCancel(EDIMessage outgoingMessage, Action<SharedCusPermitLineTransaction> loggingAction, Func<EDIMessage, ZString> getPermitAppId, ZString procedure, ZString countryCode, string status = "")
		{
			if (outgoingMessage?.EM_LinkedObject is IAllowPermitProcessing allowPermitProcessing)
			{
				var reference = allowPermitProcessing.GetPermitReference();
				var referenceNumberLine = allowPermitProcessing.GetPermitReferenceNumberLine();
				var outgoingAppId = getPermitAppId(outgoingMessage);

				Func<SharedCusPermitLineTransaction, bool> criteria = x => true;
				RollbackPermitTransactionsForReferenceWithAdditionalCriteria(outgoingMessage.Factory, countryCode, reference, PermitHelper.GetPermitComment(allowPermitProcessing, null), outgoingAppId, procedure, criteria, loggingAction, referenceNumberLine, status);
			}
		}

		public static void RollbackPermitTransactions(EDIMessage incomingMessage, EDIMessage outgoingMessage, Action<SharedCusPermitLineTransaction> loggingAction, Func<EDIMessage, ZString> getPermitAppId, ZString procedure, ZString countryCode, bool messageOnly = true, string status = "")
		{
			if (outgoingMessage?.EM_LinkedObject is IAllowPermitProcessing allowPermitProcessing)
			{
				var reference = allowPermitProcessing.GetPermitReference();
				var referenceNumberLine = allowPermitProcessing.GetPermitReferenceNumberLine();
				var incomingAppId = getPermitAppId(incomingMessage);
				var outgoingAppId = getPermitAppId(outgoingMessage);

				Func<SharedCusPermitLineTransaction, bool> criteria;
				if (messageOnly)
				{
					criteria = x => x.CPL_AppId == outgoingAppId;
				}
				else
				{
					criteria = x => true;
				}

				RollbackPermitTransactionsForReferenceWithAdditionalCriteria(incomingMessage.Factory, countryCode, reference, GetPermitComment(allowPermitProcessing, null), incomingAppId, procedure, criteria, loggingAction, referenceNumberLine, status);
			}
		}

		public static void RollbackPermitTransactionsForReferenceWithAdditionalCriteria(BusinessObjectFactory factory, ZString countryCode, ZString reference, ZString comment, ZString appId, ZString procedure, Func<SharedCusPermitLineTransaction, bool> additionalCriteria, Action<SharedCusPermitLineTransaction> additionalAction, int referenceNumberLine = 0, string status = "")
		{
			if (additionalCriteria == null)
			{
				additionalCriteria = x => true;
			}

			var relatedPermitHeaders = GetRelatedAndTransactionsApplicablePermitHeaders(factory, countryCode, reference, referenceNumberLine);
			foreach (var permitHeader in relatedPermitHeaders)
			{
				RollbackPermitTransactionsForReferenceWithAdditionalCriteria(permitHeader, reference, comment, appId, procedure, additionalCriteria, additionalAction, referenceNumberLine, status);
			}
		}

		public static void RollbackPermitTransactionsForReferenceWithAdditionalCriteria(SharedCusPermitHeader permitHeader, ZString reference, ZString comment, ZString appId, ZString procedure, Func<SharedCusPermitLineTransaction, bool> additionalCriteria, Action<SharedCusPermitLineTransaction> additionalAction, int referenceNumberLine = 0, string status = "")
		{
			var existingTransactionsForEntry = permitHeader.GetTransactions().Where(x => x.CPL_Reference == reference &&
				x.CPL_ReferenceNumberLine == referenceNumberLine &&
				x.CPL_TransactionStatus != PermitTransactionStatusList.Codes.Deleted &&
				additionalCriteria(x));
			var existingValue = existingTransactionsForEntry.Sum(x => x.CPL_TranValue);
			var existingQuantity = existingTransactionsForEntry.Sum(x => x.CPL_TranQty);
			var transaction = permitHeader.AddTransaction(reference, comment, appId, procedure, ZDecimal.Zero - existingValue, ZDecimal.Zero - existingQuantity, status, referenceNumberLine);

			additionalAction?.Invoke(transaction);
		}

		public static void UpdatePendingTransactions(EDIMessage incomingMessage, EDIMessage outgoingMessage, Func<EDIMessage, ZString> getPermitAppId, ZString countryCode, bool messageOnly = true, string status = "", Action<SharedCusPermitLineTransaction> additionalAction = null)
		{
			UpdatePendingTransactions(incomingMessage.Factory, outgoingMessage, getPermitAppId, countryCode, messageOnly, status, additionalAction);
		}

		public static void UpdatePendingTransactions(BusinessObjectFactory factory, EDIMessage outgoingMessage, Func<EDIMessage, ZString> getPermitAppId, ZString countryCode, bool messageOnly = true, string status = "", Action<SharedCusPermitLineTransaction> additionalAction = null)
		{
			if (outgoingMessage?.EM_LinkedObject is IAllowPermitProcessing messageHeader)
			{
				var reference = messageHeader.GetPermitReference();
				var referenceNumberLine = messageHeader.GetPermitReferenceNumberLine();

				var outgoingAppId = getPermitAppId(outgoingMessage);

				Func<SharedCusPermitLineTransaction, bool> criteria;
				if (messageOnly)
				{
					criteria = x => x.CPL_AppId == outgoingAppId;
				}
				else
				{
					criteria = x => true;
				}
				UpdatePendingTransactionsWithAdditionalCriteria(factory, countryCode, reference, criteria, status, referenceNumberLine, additionalAction);
			}
		}

		public static void UpdatePendingTransactionsWithAdditionalCriteria(BusinessObjectFactory factory, ZString countryCode, ZString reference, Func<SharedCusPermitLineTransaction, bool> additionalCriteria, string status = "", int referenceNumberLine = 0, Action<SharedCusPermitLineTransaction> additionalAction = null)
		{
			if (additionalCriteria == null)
			{
				additionalCriteria = x => true;
			}

			var relatedPermitHeaders = GetRelatedAndTransactionsApplicablePermitHeaders(factory, countryCode, reference, referenceNumberLine);
			foreach (var permitHeader in relatedPermitHeaders)
			{
				foreach (var transaction in permitHeader.GetTransactions().Where(x => x.CPL_Reference == reference &&
																								x.CPL_ReferenceNumberLine == referenceNumberLine &&
																								x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending &&
																								additionalCriteria(x)))
				{
					transaction.CPL_TransactionStatus = status;
					additionalAction?.Invoke(transaction);
				}
			}
		}

		public static SharedCusPermitLineTransaction[] GetRelatedPermitTransactions(BusinessObjectFactory factory, ZString countryCode, ZString reference, ZInt referenceNumberLine, string permitType = null)
		{
			var permitQuery = new ZDBOnlySubQuery(typeof(SharedCusPermitHeader), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			permitQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, countryCode);

			if (permitType != null)
			{
				permitQuery.AddToFilter(CusPermitHeaderSchema.CPH_Type, permitType);
			}

			var query = new ZDBOnlyQuery(typeof(SharedCusPermitLineTransaction));
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, reference);
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_ReferenceNumberLine, referenceNumberLine);
			query.AddSubQuery(permitQuery, JoinCondition.And);

			return factory.Load<SharedCusPermitLineTransaction>(query);
		}

		public static ZString GetPermitComment(IAllowPermitProcessing header, PermitRecord permitRecord) => header?.GetPermitComment(permitRecord) ?? ZString.Empty;

		public static void AddPermitRecordsToCancel(BusinessObjectFactory factory, ZString countryCode, ZString reference, ZInt referenceNumberLine, IList<PermitRecord> permitRecords)
		{
			var relatedPermitHeaders = GetRelatedAndTransactionsApplicablePermitHeaders(factory, countryCode, reference, referenceNumberLine);
			foreach (var relatedPermitHeader in relatedPermitHeaders)
			{
				var permitRecord = permitRecords.FirstOrDefault(x => x.PermitHeader == relatedPermitHeader);
				if (permitRecord == null)
				{
					permitRecord = new PermitRecord()
					{
						PermitHeader = relatedPermitHeader,
						Value = ZDecimal.Zero,
						Quantity = ZDecimal.Zero
					};
					permitRecords.Add(permitRecord);
				}
			}
		}

		public static bool IsPermitInUsed(BusinessObjectFactory factory, ZString countryCode, ZString permitType, ZString reference, int referenceNumberLine = 0)
		{
			var result = false;
			var openingTransaction = GetRelatedPermitTransactions(factory, countryCode, reference, referenceNumberLine, permitType).FirstOrDefault(x => x.CPL_TransactionType == PermitTransactionTypeList.Codes.OBL);
			if (openingTransaction != null)
			{
				var permitHeader = openingTransaction.PermitHeader;
				result = permitHeader != null && (permitHeader.ValueBalance != openingTransaction.CPL_TranValue || permitHeader.QuantityBalance != openingTransaction.CPL_TranQty);
			}
			return result;
		}

		public static bool MatchEntry(this SharedCusPermitLineTransaction line, IAllowPermitProcessing header, PermitRecord permitrecord)
		{
			return line.CPL_Reference == GetPermitReferenceForEntry(header) &&
					line.CPL_ReferenceNumberLine == GetPermitReferenceNumberLineForEntry(header) &&
					line.CPL_TransactionType == PermitTransactionTypeList.Codes.TRA &&
					line.CPL_Comment == GetPermitComment(header, permitrecord);
		}

		public static SharedCusPermitHeader[] GetRelatedAndTransactionsApplicablePermitHeaders(BusinessObjectFactory factory, ZString countryCode, ZString reference, ZInt referenceNumberLine, string permitType = null)
		{
			return GetRelatedPermitTransactions(factory, countryCode, reference, referenceNumberLine, permitType)
				.Select(x => x.PermitHeader)
				.Distinct()
				.Where(x => x.IsTransactionsApplicable())
				.ToArray();
		}

		public static ZString GetPermitReferenceForEntry(IAllowPermitProcessing header) => header?.GetPermitReference() ?? ZString.Empty;

		public static ZInt GetPermitReferenceNumberLineForEntry(IAllowPermitProcessing header) => header?.GetPermitReferenceNumberLine() ?? 0;
	}
}

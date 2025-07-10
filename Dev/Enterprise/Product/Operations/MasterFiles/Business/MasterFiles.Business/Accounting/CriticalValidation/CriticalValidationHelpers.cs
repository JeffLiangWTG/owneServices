using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Accounting.CriticalValidation
{
	public static class CriticalValidationHelpers
	{
		public delegate string GetElementInfoDelegate<T>(T element);

		public static string ArrayChangedWarningMessage
		{
			get { return Res.GetString("f7d88710-b651-4840-a740-26d9bc93f532", "Charge collection has unexpected changes."); }
		}

		public static ResourceString GetAmountGreaterThanMaximumAllowedAmountMessage(ZGuid companyPK, MaximumAmountLevel maximumAmountLevel, params ZDecimal[] amounts)
		{
			var companyToGuid = companyPK.IsValid ? companyPK.ToGuid() : Guid.Empty;
			var registryValue = AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount.GetFallBackValueAtAllLevels(companyToGuid, Guid.Empty, Guid.Empty);
			var maximumAllowedAmount = maximumAmountLevel == MaximumAmountLevel.Header ? registryValue.MaximumAllowedHeaderAmount : registryValue.MaximumAllowedLineAmount;

			foreach (var amount in amounts)
			{
				if (Math.Abs(amount) > maximumAllowedAmount)
				{
					var maximumAllowedAmountString = maximumAllowedAmount.ToString(registryValue.AmountDecimalPlaces);
					switch (maximumAmountLevel)
					{
						case MaximumAmountLevel.Header:
							return CriticalValidationMessageTemplate.GetTransactionHeaderAmountExceedMaximumAllowedAmountErrorMessage(maximumAllowedAmountString);
						case MaximumAmountLevel.Line:
							return CriticalValidationMessageTemplate.GetTransactionLineAmountExceedMaximumAllowedAmountErrorMessage(maximumAllowedAmountString);
						case MaximumAmountLevel.Charge:
							return CriticalValidationMessageTemplate.GetJobChargeAmountExceedMaximumAllowedAmountErrorMessage(maximumAllowedAmountString);
						default:
							break;
					}
				}
			}

			return null;
		}

		public enum MaximumAmountLevel
		{
			Header,
			Line,
			Charge
		}

		public static void ReportArrayChanges<T>(T[] currentArray, T[] initialArray, string errorKey, string messagePrefix, bool throwException, GetElementInfoDelegate<T> getElementInfo)
		{
			T[] addedElements;
			T[] removedElements;
			GetDifferentArrayElements(currentArray, initialArray, out addedElements, out removedElements);

			StringBuilder errorMessage = new StringBuilder();
			if (addedElements.Length > 0 || removedElements.Length > 0)
			{
				errorMessage.AppendLine(messagePrefix);
			}
			if (addedElements.Length > 0)
			{
				errorMessage.AppendLine(Res.GetString("80D80B27-7B09-403b-9ADD-D893D8EC6CF5", "Elements added to the collection:"));
			}
			foreach (T element in addedElements)
			{
				errorMessage.AppendLine(getElementInfo(element));
			}
			if (removedElements.Length > 0)
			{
				errorMessage.AppendLine(Res.GetString("D4801CCB-CABB-4a05-9996-0629D886573A", "Elements removed from the collection:"));
			}
			foreach (T element in removedElements)
			{
				errorMessage.AppendLine(getElementInfo(element));
			}
			ReportInvalidOperation(errorKey, errorMessage.ToString(), throwException);
		}

		public static void ReportInvalidOperation(string key, string message, bool throwException)
		{
			if (message.Length > 0)
			{
				try
				{
					throw new InvalidOperationException(message);
				}
				catch (InvalidOperationException ex)
				{
					ErrorReporter.ReportOnce(key, message, ex);
					if (throwException)
					{
						throw;
					}
				}
			}
		}

		static void GetDifferentArrayElements<T>(T[] currentArray, T[] initialArray, out T[] addedElements, out T[] removedElements)
		{
			HashSet<T> addedElementsSet = new HashSet<T>(currentArray);
			addedElementsSet.UnionWith(initialArray);
			HashSet<T> removedElementsSet = new HashSet<T>(addedElementsSet);

			addedElementsSet.ExceptWith(initialArray);
			addedElements = addedElementsSet.ToArray();

			removedElementsSet.ExceptWith(currentArray);
			removedElements = removedElementsSet.ToArray();
		}

		public static void SetConflictWithCriticalFieldsBusinessContext(BusinessObject obj)
		{
			obj.SetContext(BusinessContext.ConflictWithCriticalFields);
		}

		public static void AddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways(AccTransactionLines line, ZGuid oldOHValue, JobCharge relatedCharge = null)
		{
			var lineTypes = new[] { TransactionLineTypes.Accrual, TransactionLineTypes.WIP };
			if (line == null || !lineTypes.Contains(line.AL_LineType.ToString()))
			{
				return;
			}

			if (line.IsInDatabase)
			{
				CriticalValidationInfoCollectorService.GetOrCreateService(line.Factory).AddInfoWhenAllowed(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine_InDb,
					CollectInfo,
					CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession);
			}
			else
			{
				CriticalValidationInfoCollectorService.GetOrCreateService(line.Factory).AddInfoWhenAllowed(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine,
					CollectInfo,
					CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
			}

			string CollectInfo()
			{
				relatedCharge = relatedCharge ?? line.LoadRelatedJobCharge(!line.IsInDatabase);
				if (relatedCharge == null)
				{
					return null;
				}

				ZGuid orgPK;
				string chargeOrgType;
				if (line.PK == relatedCharge.JR_AL_ARLine)
				{
					orgPK = relatedCharge.JR_OH_SellAccount;
					chargeOrgType = (NoResString)"Sell";
				}
				else
				{
					orgPK = relatedCharge.JR_OH_CostAccount;
					chargeOrgType = (NoResString)"Cost";
				}

				if (line.AL_OH == orgPK)
				{
					return null;
				}

				var info = new ZStringBuilder();
				info.AppendLine(FormattableString.Invariant($"RelatedCharge PK: {relatedCharge.PK}, Charge {chargeOrgType} Organization: {orgPK}, Line Organization: {line.AL_OH}, Old Organization: {oldOHValue}"));
				info.AppendLine(new StackTrace().ToString());
				return info.ToString();
			}
		}

		public static bool CheckGLJournalEntriesNumberHasBeenAssigned(AccTransactionHeader header)
		{
			var helper = ObjectFactory.Get<IAccGeneralLedgerDataCriticalValidationHelper>();
			return helper?.IsGLJournalEntriesNumberHasBeenAssigned(header) ?? false;
		}
	}
}

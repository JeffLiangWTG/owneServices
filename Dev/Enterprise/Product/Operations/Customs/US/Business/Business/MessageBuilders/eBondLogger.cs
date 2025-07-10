using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public static class eBondLogger
	{
		public static void AddAutoSendEvent(Logs logs, ImportMessageSendingMessageType orgAmdWithdrawal, bool hasMessageError)
		{
			var messageSendType = orgAmdWithdrawal == ImportMessageSendingMessageType.Original ? "ORG" : "REP";
			var reference = eBondAutoSendMessageReference + ":" + messageSendType + (hasMessageError ? SendWithMessageError : string.Empty);

			var existLog = GetAutoSendEvent(logs);
			if (existLog == null)
			{
				logs.AddNew(Events.Authorised, reference);
			}
			else if (existLog.SL_Reference != reference)
			{
				existLog.SL_Reference = reference;
			}
		}

		public static bool CanAutoSendMessage(JobDeclaration declaration)
		{
			var isMainBondAdded = declaration.IsSingleTransactionBond && BondDispositionCodeList.IsAdded(declaration.US_BondDispositionCode);

			return isMainBondAdded && (declaration.US_BondType2.IsEmpty || BondDispositionCodeList.IsAdded(declaration.US_BondDispositionCode2));
		}

		public static bool ShouldAddAutoSendLog(JobDeclaration declaration)
		{
			return declaration.IsACECargoCertificationMode
				&& USCustomsDataRegistry.Instance.IseBondAutoSendACEMessage.GetValueWithoutFallback(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty)
				&&
				(
					declaration.IsSingleTransactionBond && !BondDispositionCodeList.IsAdded(declaration.US_BondDispositionCode)
					|| declaration.IsAdditionalSingleTransactionBond && !BondDispositionCodeList.IsAdded(declaration.US_BondDispositionCode2)
				);
		}

		public static StmALog GetAutoSendEvent(Logs logs)
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Authorised.Code);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, eBondAutoSendMessageReference);
			query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			return logs.Find(query).FirstOrDefault();
		}

		public static void CancelAutoSendLog(Logs logs)
		{
			var stmLog = GetAutoSendEvent(logs);
			if (stmLog != null)
			{
				stmLog.Cancel();
			}
		}

		public static IEnumerable<StmALog> GetATHlogs(JobDeclaration declaration)
		{
			var simplifiedEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var crlAutoSendEvent = simplifiedEntry != null ? GetAutoSendEvent(simplifiedEntry.Logs) : null;
			if (crlAutoSendEvent != null)
			{
				yield return crlAutoSendEvent;
			}
			var entrySummaryEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var ensAutoSendEvent = entrySummaryEntry != null ? GetAutoSendEvent(entrySummaryEntry.Logs) : null;
			if (ensAutoSendEvent != null)
			{
				yield return ensAutoSendEvent;
			}
		}

		public static string[] GetLoggedATHUser(JobDeclaration declaration)
		{
			var athLogs = GetATHlogs(declaration);
			return athLogs.Select(x => x.User?.GS_EmailAddress.ToString() ?? "").Where(x => !string.IsNullOrEmpty(x)).Distinct().ToArray();
		}

		public const string eBondAutoSendMessageReference = "Auto-Send Entry on eBond Clearance";
		public const string MessageSendTypeOriginal = "ORG";
		public const string MessageSendTypeReplacement = "REP";
		public const string SendWithMessageError = " Send with Message Error";
	}
}

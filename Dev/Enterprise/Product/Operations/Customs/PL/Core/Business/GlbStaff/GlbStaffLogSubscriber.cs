using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using CusPollingTransactionType = Enterprise.Core.Constants.Customs.CusPollingTransactionType.Codes;

namespace Enterprise.Customs.PL.Business;

[Serializable]
public sealed class GlbStaffLogSubscriber : LogSubscriber
{
	public override string Name => "PlGlbStaffSubscriber";

	public override string FriendlyName => (NoResString)"PL Staff log subscriber";

	public override string[] EventTypes => new[] { Events.EditedARecordCode, Events.SetToActiveCode, Events.SetToInactiveCode };

	public override string[] TableNames => new[] { GlbStaff.Schema.TableName };

	protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
	{
		foreach (var (factory, eventCode, staffPK, staffBranchPK, passwordPK, passwordBranchPK, mustHaveTransaction, transactionPK) in queuedLogs.Select(LoadLogData))
		{
			if (staffPK.IsEmpty || passwordPK.IsEmpty)
			{
				continue;
			}

			if (eventCode == Events.EditedARecordCode && passwordBranchPK != staffBranchPK)
			{
				UpdateExternalPasswordBranch(factory, passwordPK, staffBranchPK);
			}

			AddOrRemoveTransactionIfRequired(factory, passwordPK, mustHaveTransaction, transactionPK);
		}
	}

	sealed record LogData(BusinessObjectFactory Factory, ZString Event, ZGuid StaffPK, ZGuid StaffBranchPK, ZGuid PasswordPK, ZGuid PasswordBranchPK, ZBool MustHaveTransaction, ZGuid TransactionPK)
	{
		public static LogData CreateNotFoundResult(BusinessObjectFactory factory, ZString eventType)
			=> new (factory, eventType, StaffPK: default, StaffBranchPK: default, PasswordPK: default, PasswordBranchPK: default, MustHaveTransaction: default, TransactionPK: default);
	}

	static LogData LoadLogData(IQueuedLog queuedLog)
	{
		var factory = queuedLog.Factory;
		var query = new DynamicBusinessObjectCollection(factory);
		query.Load(@$"
SELECT TOP 1 GS_GB_HomeBranch, GP_PK, GP_GB, CPT_PK,
    (CASE WHEN GS_IsActive <> 0 AND NOT (GP_PK IS NULL) AND GP_MailBoxID <> '' AND GP_CurrentPassword <> '' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END) AS MUST_HAVE_A_TRANSACTION
FROM GlbStaff S
LEFT JOIN GlbExternalPassword P ON P.GP_PasswordType = '{PasswordTypesList.Codes.PLB}' AND P.GP_GS = S.GS_PK
LEFT JOIN CusPollingTransaction T ON T.CPT_TYPE = '{CusPollingTransactionType.PLC}' AND T.CPT_ParentID = P.GP_PK
WHERE GS_PK = '{queuedLog.ParentID}'");
		var row = query.FirstOrDefault();
		if (row is null)
		{
			return LogData.CreateNotFoundResult(factory, queuedLog.SJ_SE_NKEvent);
		}
		var staffBranchPK = row["GS_GB_HomeBranch"] is ZGuid branchPK ? branchPK : default;
		var passwordPK = row["GP_PK"] is ZGuid password ? password : default;
		if (passwordPK.IsEmpty)
		{
			return new LogData(factory, queuedLog.SJ_SE_NKEvent, queuedLog.ParentID, staffBranchPK, PasswordPK: default, PasswordBranchPK: default, MustHaveTransaction: default, TransactionPK: default);
		}
		var passwordBranchPK = row["GP_GB"] is ZGuid branch ? branch : default;
		var transactionPK = row["CPT_PK"] is ZGuid transaction ? transaction : default;
		var mustHaveTransaction = row["MUST_HAVE_A_TRANSACTION"] is ZBool boolValue ? boolValue : default;
		return new LogData(factory, queuedLog.SJ_SE_NKEvent, queuedLog.ParentID, staffBranchPK, passwordPK, passwordBranchPK, mustHaveTransaction, transactionPK);
	}

	void UpdateExternalPasswordBranch(BusinessObjectFactory factory, ZGuid passwordPK, ZGuid staffBranchPK)
	{
		var password = factory.LoadTop1<GlbExternalPassword>(new ZQuery(GlbExternalPasswordSchema.PK, passwordPK));
		if (password != null)
		{
			password.GP_GB = staffBranchPK;
		}
	}

	static void AddOrRemoveTransactionIfRequired(BusinessObjectFactory factory, ZGuid passwordPK, ZBool mustHaveTransaction, ZGuid transactionPK)
	{
		if (mustHaveTransaction)
		{
			if (transactionPK.IsEmpty)
			{
				factory.TryCreateNewCusPollingTransaction(passwordPK);
			}
		}
		else if (!transactionPK.IsEmpty)
		{
			factory.DeleteCusPollingTransactions(passwordPK);
		}
	}
}

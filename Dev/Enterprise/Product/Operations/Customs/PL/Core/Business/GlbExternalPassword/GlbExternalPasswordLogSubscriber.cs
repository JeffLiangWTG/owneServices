using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.PL.Business;

[Serializable]
public sealed class GlbExternalPasswordLogSubscriber : LogSubscriber
{
	public override string Name => "PlGlbExtPasswSubscriber";

	public override string FriendlyName => (NoResString)"PLB ExternalPassword log subscriber";

	public override string[] EventTypes => new[] { Events.AddedARecordToTheSystem.Code, Events.EditedARecord.Code, Events.DeletedARecordInTheSystem.Code };

	public override string[] TableNames => new[] { GlbExternalPassword.Schema.TableName };

	protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
	{
		foreach (var queuedLog in queuedLogs.Where(x => x.Reference == PasswordTypesList.Codes.PLB && !x.ParentID.IsEmpty))
		{
			switch (queuedLog.SJ_SE_NKEvent)
			{
				case Events.AddedARecordToTheSystemCode:
					ProcessCreatedNewExternalPassword(queuedLog.Factory, passwordPK: queuedLog.ParentID);
					break;

				case Events.EditedARecordCode:
					ProcessExternalPasswordUpdated(queuedLog.Factory, passwordPK: queuedLog.ParentID);
					break;

				case Events.DeletedARecordInTheSystemCode:
					queuedLog.Factory.DeleteCusPollingTransactions(passwordPK: queuedLog.ParentID);
					break;
			}
		}
	}

	void ProcessCreatedNewExternalPassword(BusinessObjectFactory factory, ZGuid passwordPK)
	{
		var externalPassword = factory.LoadTop1<GlbExternalPassword>(new ZQuery(GlbExternalPasswordSchema.PK, passwordPK));
		if (externalPassword == null)
		{
			return;
		}
		externalPassword.GP_GB = externalPassword.Staff.GS_GB_HomeBranch;

		var mustHaveTransaction = !externalPassword.GP_MailBoxID.IsEmpty && !externalPassword.GP_CurrentPassword.IsEmpty;
		if (mustHaveTransaction)
		{
			factory.TryCreateNewCusPollingTransaction(passwordPK);
		}
	}

	void ProcessExternalPasswordUpdated(BusinessObjectFactory factory, ZGuid passwordPK)
	{
		var query = new DynamicBusinessObjectCollection(factory);
		query.Load(@$"
SELECT TOP 1 (CASE WHEN GP_MailBoxID <> '' AND GP_CurrentPassword <> '' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END) AS MUST_HAVE_A_TRANSACTION
FROM GlbExternalPassword WHERE GP_PK = '{passwordPK}'");
		var row = query.FirstOrDefault();
		if (row == null)
		{
			return;
		}
		if ((ZBool)row["MUST_HAVE_A_TRANSACTION"])
		{
			if (!factory.Exists(typeof(CusPollingTransaction), CusPollingTransactionsHelper.GetQueryCusPollingTransactionsByPasswordPK(passwordPK)))
			{
				factory.TryCreateNewCusPollingTransaction(passwordPK);
			}
		}
		else
		{
			factory.DeleteCusPollingTransactions(passwordPK);
		}
	}
}

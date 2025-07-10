using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class DraftCommissionAgreementLogs
	{
		public static ZGuid GetParentVersionPk(EnterpriseBusinessObject draft)
		{
			IStmALogParent stmaLogParent = draft;
			ZGuid pk = ZGuid.Empty;

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_Parent, draft.PK);
			query.AddToFilter(StmALogSchema.SL_Table, stmaLogParent.LogsParentTableName);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Constants.EventCode);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, Constants.ReferencePrefix);

			var log = draft.Factory.LoadTop1<StmALog>(query);
			if (log != null)
			{
				var pkString = log.SL_Reference.Substring(Constants.ReferencePrefix.Length);
				if (!ZGuid.TryParse(pkString, out pk))
				{
					pk = ZGuid.Empty;
				}
			}

			return pk;
		}

		public static void AddDraftLog(EnterpriseBusinessObject draft, EnterpriseBusinessObject parentVersion)
		{
			IStmALogParent stmaLogParent = draft;

			var factory = draft.Factory;
			var log = factory.New<StmALog>();
			using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
			{
				log.SL_Parent = draft.PK;
				log.SL_Table = stmaLogParent.LogsParentTableName;
				log.SL_SE_NKEvent = Constants.EventCode;
				log.SL_Reference = Constants.ReferencePrefix + parentVersion.PK.ToString();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Logging Consant")]
		internal static class Constants
		{
			public const string EventCode = Events.MiscellaneousEventCode;
			public const string ReferencePrefix = "Parent:";
		}
	}
}

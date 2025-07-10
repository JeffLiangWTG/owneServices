using System;
using CargoWise.Data;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class RelatedJobComplianceRiskStatusUpdater
	{
		readonly string orgSpScriptName;
		readonly string vesselSpScriptName;

		public RelatedJobComplianceRiskStatusUpdater()
		{
		}

		public RelatedJobComplianceRiskStatusUpdater(string orgSpScriptName, string vesselSpScriptName)
		{
			this.orgSpScriptName = orgSpScriptName;
			this.vesselSpScriptName = vesselSpScriptName;
		}

		public void UpdateForOrg(Guid orgPK, Guid companyPK, Action<DbCommand, DbConnection> excuteCommandAction)
		{
			Update(GetCommandForOrgUpdate(orgPK, companyPK), excuteCommandAction);
		}

		public void UpdateForVessel(Guid vesselPK, Guid companyPK, Action<DbCommand, DbConnection> excuteCommandAction)
		{
			Update(GetCommandForVesselUpdate(vesselPK, companyPK), excuteCommandAction);
		}

		void Update(DbCommand cmd, Action<DbCommand, DbConnection> excuteCommandAction)
		{
			using (cmd)
			{
				if (excuteCommandAction != null)
				{
					excuteCommandAction(cmd, Db.Connection);
				}
				else
				{
					cmd.CommandTimeout = OrganisationsDataRegistry.Instance.DeniedPartyScreeningTimeoutForSQLCommandQuery.Value;
					cmd.ExecuteNonQuery();
				}
			}
		}

		protected virtual DbCommand GetCommandForOrgUpdate(Guid orgPK, Guid companyPK)
		{
			var command = Db.Connection.Command(orgSpScriptName);
			command.CommandType = System.Data.CommandType.StoredProcedure;
			command.CommandTimeout = OrganisationsDataRegistry.Instance.DeniedPartyScreeningTimeoutForSQLCommandQuery.Value;
			command.AddParameter("@entityPk", System.Data.SqlDbType.UniqueIdentifier, orgPK);
			command.AddParameter("@companyPK", System.Data.SqlDbType.UniqueIdentifier, companyPK);
			command.AddParameter("@jobEndDate", System.Data.SqlDbType.DateTimeOffset, 0, 0, 0, DateTimeOffset.Now.Date.AddDays(-OrganisationsDataRegistry.Instance.ComplianceJobEndDateLimit.Value));
			command.AddParameter("@userCode", System.Data.SqlDbType.VarChar, 3, Environment.Env.CurrentUser.Initials);

			return command;
		}

		protected virtual DbCommand GetCommandForVesselUpdate(Guid vesselPK, Guid companyPK)
		{
			var command = Db.Connection.Command(vesselSpScriptName);
			command.CommandType = System.Data.CommandType.StoredProcedure;
			command.CommandTimeout = OrganisationsDataRegistry.Instance.DeniedPartyScreeningTimeoutForSQLCommandQuery.Value;
			command.AddParameter("@vesselPK", System.Data.SqlDbType.UniqueIdentifier, vesselPK);
			command.AddParameter("@companyPK", System.Data.SqlDbType.UniqueIdentifier, companyPK);
			command.AddParameter("@jobEndDate", System.Data.SqlDbType.DateTimeOffset, 0, 0, 0, DateTimeOffset.Now.Date.AddDays(-OrganisationsDataRegistry.Instance.ComplianceJobEndDateLimit.Value));
			command.AddParameter("@userCode", System.Data.SqlDbType.VarChar, 3, Environment.Env.CurrentUser.Initials);

			return command;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants.EventReferenceParameters;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.DataTransfer
{
	[Serializable]
	public class UpdateRelatedJobSubscriber : LogSubscriber
	{
		public override string Name => nameof(UpdateRelatedJobSubscriber);

		public override string[] EventTypes => new[] { AutoEvents.DeniedPartyStatusUpdated.Code };

		public override string[] TableNames => new[] { OrgHeaderSchema.Constants.TableName, RefVesselSchema.Constants.TableName };

		public override string FriendlyName => (NoResString)"Update Related Job Subscriber";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			var updateRelatedJobsEnabled = OrganisationsDataRegistry.Instance.DeniedPartyScreeningEnableLogWalkerServiceTaskToUpdateRelatedJobs.Value;
			var complianceRiskStatusUpdater = GetComplianceRiskStatusUpdater();
			string shipmentUpdateOption = forwardingRegistryInvoker.ShipmentDpsStatusUpdateSetting.Value.Option;
			string consolUpdateOption = forwardingRegistryInvoker.ConsolDpsStatusUpdateSetting.Value.Option;

			foreach (IQueuedLog queuedLog in queuedLogs)
			{
				if (TryGetShouldUpdateJob(queuedLog.SJ_Reference, updateRelatedJobsEnabled, out var companyPK))
				{
					var connection = ((IDbConnected)queuedLog.Factory).Connection;
					if (queuedLog.SJ_ParentTableCode == OrgHeaderSchema.Constants.Prefix)
					{
						using (var command = connection.Command("UpdateRelatedJobsForOrgOrDocAddress"))
						{
							command.CommandType = CommandType.StoredProcedure;
							command.AddParameter("@entityPK", SqlDbType.UniqueIdentifier, queuedLog.SJ_ParentID.ToGuid());
							command.AddParameter("@earliestDT", SqlDbType.DateTime, DateTime.UtcNow.AddDays(-7));
							command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK.ToGuid());
							command.AddParameter("@updateShipments", SqlDbType.Bit, ShouldUpdateByDatesAndBilling(shipmentUpdateOption) ? 1 : 0);
							command.AddParameter("@updateConsols", SqlDbType.Bit, ShouldUpdateByDatesAndBilling(consolUpdateOption) ? 1 : 0);
							command.AddParameter("@jobUpdatePeriod", SqlDbType.SmallDateTime, ZDateTime.UtcNow.AddMonths(-OrganisationsDataRegistry.Instance.JobUpdatePeriod.Value).ToDateTime());
							command.AddParameter("@userCode", SqlDbType.VarChar, 3, GlbStaff.CurrentUser.GS_Code.ToString());
							command.AddParameter("@freightCpwEnabled", SqlDbType.Bit, ComplianceRiskHelper.IsFreightEnabledComplianceWise ? 1 : 0);

							ExecuteCommand(command, connection);
						}

						if (ShouldUpdateByPhases(consolUpdateOption))
						{
							using (var command = BuildUpdateRelatedJobsByPhaseCmd(connection, queuedLog.SJ_ParentID, "UpdateRelatedConsolsForOrgOrDocAddressByPhases", ConsolPhasesToUpdate, companyPK))
							{
								ExecuteCommand(command, connection);
							}
						}

						if (ShouldUpdateByPhases(shipmentUpdateOption))
						{
							using (var command = BuildUpdateRelatedJobsByPhaseCmd(connection, queuedLog.SJ_ParentID, "UpdateRelatedShipmentsForOrgOrDocAddressByPhases", ShipmentPhasesToUpdate, companyPK))
							{
								ExecuteCommand(command, connection);
							}
						}

						complianceRiskStatusUpdater.UpdateForOrg(queuedLog.SJ_ParentID.ToGuid(), companyPK.ToGuid(), ExecuteCommand);
					}
					else if (queuedLog.SJ_ParentTableCode == RefVesselSchema.Constants.Prefix)
					{
						using (var command = connection.Command("UpdateRelatedJobsForVessel"))
						{
							command.CommandType = CommandType.StoredProcedure;
							command.AddParameter("@vesselPK", SqlDbType.UniqueIdentifier, queuedLog.SJ_ParentID.ToGuid());
							command.AddParameter("@earliestDT", SqlDbType.DateTime, DateTime.UtcNow.AddDays(-7));
							command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK.ToGuid());
							command.AddParameter("@jobUpdatePeriod", SqlDbType.SmallDateTime, ZDateTime.UtcNow.AddMonths(-OrganisationsDataRegistry.Instance.JobUpdatePeriod.Value).ToDateTime());
							command.AddParameter("@userCode", SqlDbType.VarChar, 3, GlbStaff.CurrentUser.GS_Code.ToString());
							command.AddParameter("@freightCpwEnabled", SqlDbType.Bit, ComplianceRiskHelper.IsFreightEnabledComplianceWise ? 1 : 0);

							ExecuteCommand(command, connection);
						}

						complianceRiskStatusUpdater.UpdateForVessel(queuedLog.SJ_ParentID.ToGuid(), companyPK.ToGuid(), ExecuteCommand);
					}
				}
			}
		}

		public static bool ShouldUpdateByDatesAndBilling(string option) => option == "DAB" || option == "COM"; // Constant update option codes

		public static bool ShouldUpdateByPhases(string option) => option == "PHS" || option == "COM"; // Constant update option codes

		protected virtual RelatedJobsComplianceRiskStatusUpdater GetComplianceRiskStatusUpdater() => new RelatedJobsComplianceRiskStatusUpdater();

		protected void ExecuteCommand(DbCommand command, DbConnection connection)
		{
			try
			{
				command.ExecuteNonQuery();
			}
			catch (SqlException timeoutEx) when (timeoutEx.Message.Contains((NoResString)"Timeout"))
			{
				ExceptionReporter.Instance.ReportDeveloperException((NoResString)"Stored Procedure executed time out.", timeoutEx);
			}
			catch (SqlException ex)
			{
				throw new ZDataException(ex, null, connection);
			}
		}

		DbCommand BuildUpdateRelatedJobsByPhaseCmd(DbConnection connection, ZGuid orgPK, string procedureName, DataTable phaseTable, ZGuid companyPK)
		{
			DbCommand cmd = connection.Command(procedureName);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandTimeout = OrganisationsDataRegistry.Instance.DeniedPartyScreeningTimeoutForSQLCommandQuery.Value;
			cmd.AddParameter("@entityPK", SqlDbType.UniqueIdentifier, orgPK.ToGuid());
			cmd.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK.ToGuid());
			cmd.AddParameter("@userCode", SqlDbType.VarChar, 3, GlbStaff.CurrentUser.GS_Code.ToString());
			cmd.AddTableValuedParameter("@phaseList", "dbo.TVP_Char_3", phaseTable);			
			return cmd;
		}

		DataTable ConsolPhasesToUpdate => GetPhasesArrayAsTable(forwardingRegistryInvoker.ConsolDpsStatusUpdateSetting);

		DataTable ShipmentPhasesToUpdate => GetPhasesArrayAsTable(forwardingRegistryInvoker.ShipmentDpsStatusUpdateSetting);

		DataTable GetPhasesArrayAsTable(dynamic registryItem)
		{
			var result = new DataTable();
			result.Locale = CultureInfo.InvariantCulture;
			result.Columns.Add((NoResString)"Value", typeof(string));

			string updateOption = registryItem.Value.Option;
			if (updateOption != "DAB")
			{
				var settings = registryItem.Value.JobUpdateSettings as IEnumerable<dynamic>;
				var phaseCodes = settings.Where(x => x.ShouldUpdate).Select(x => x.Code.ToString()).Cast<string>();
				foreach (var code in phaseCodes)
				{
					result.Rows.Add(code);
				}
			}

			return result;
		}

		bool TryGetShouldUpdateJob(string reference, bool updateRelatedJobsEnabled, out ZGuid companyPK)
		{
			var result = false;
			companyPK = ZGuid.Empty;
			if (!string.IsNullOrWhiteSpace(reference))
			{
				var paramPairs = reference.Split(new[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
				if (paramPairs.Length > 0)
				{
					var keyValuePair = new Dictionary<string, string>();
					paramPairs.ForEach(o =>
					{
						var keyAndValue = o.Split(new[] { '=' }, System.StringSplitOptions.RemoveEmptyEntries);
						if (keyAndValue.Length == 2)
						{
							keyValuePair.Add(keyAndValue[0], keyAndValue[1]);
						}
					});

					if (keyValuePair.TryGetValue(Codes.New, out var newStatus) &&
						keyValuePair.TryGetValue(Codes.Old, out var oldStatus) &&
						keyValuePair.TryGetValue(Codes.Company, out var company) &&
						newStatus != oldStatus &&
						(updateRelatedJobsEnabled || keyValuePair.TryGetValue(Codes.Type, out var type) && type == ScreeningType.RescreenAdvice))
					{
						result = true;
						ZGuid.TryParse(company, out companyPK);
					}
				}
			}

			return result;
		}

		readonly dynamic forwardingRegistryInvoker = Type.GetType(forwardingRegistryTypeString).GetProperty("Instance").GetValue(null);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Name of registry type, only for dev people")]
		const string forwardingRegistryTypeString = "Enterprise.Freight.Forwarding.Registry.ForwardingConfigurationRegistry, Enterprise.Freight.Forwarding.Registry";
	}
}

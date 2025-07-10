using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using IForwardingConsol = Enterprise.Integration.Forwarding.IForwardingConsol;
using IForwardingShipment = Enterprise.Integration.Forwarding.IForwardingShipment;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.MasterFiles.Business
{
	public static class ScreeningStatusUpdater
	{
		public delegate void ProgressUpdaterDelegate(string msg);

		public static void UpdateJobStatusFromItsScreeningParties(IScreeningPartyProvider parent, IShouldUpdateScreeningStatus shouldUpdateStatusProvider, ZString original)
		{
			Argument.NotNull(parent, "parent");
			Argument.NotNull(shouldUpdateStatusProvider, "shouldUpdateStatusProvider");

			if (shouldUpdateStatusProvider.ShouldUpdateScreeningStatus)
			{
				try
				{
					if (parent.ScreeningStatus != ScreeningStatusesList.Codes.JobCleared || parent.ScreeningStatus == original)
					{
						UpdateJobStatusFromItsScreeningParties(parent);
					}
				}
				finally
				{
					shouldUpdateStatusProvider.ShouldUpdateScreeningStatus = false;
				}
			}
		}

		public static string UpdateJobStatusFromItsScreeningParties(IScreeningPartyProvider parent)
		{
			if (parent != null)
			{
				parent.ScreeningStatus = GetScreenStatusUpdateTo(parent);
			}

			return parent?.ScreeningStatus;
		}

		public static ZString GetScreenStatusUpdateTo(IScreeningPartyProvider parent)
		{
			var worstScreeningStatus = parent.GetWorstScreeningStatus();

			if (worstScreeningStatus.IsEmpty || worstScreeningStatus.Equals(ScreeningStatusesList.Codes.Clear) || worstScreeningStatus.Equals(ScreeningStatusesList.Codes.PermanentClear))
			{
				worstScreeningStatus = ScreeningStatusesList.Codes.Clear;
			}

			return worstScreeningStatus;
		}

		public static void ApplyJobClearStatus(BusinessObject parentBizO, string reason)
		{
			var shouldUpdateScreeningStatusProvider = parentBizO as IShouldUpdateScreeningStatus;
			var screeningStatusProvider = parentBizO as IScreeningStatusProvider;

			if (shouldUpdateScreeningStatusProvider != null && screeningStatusProvider != null && screeningStatusProvider.ScreeningStatus != ScreeningStatusesList.Codes.JobCleared)
			{
				var oldStatus = screeningStatusProvider.ScreeningStatus;
				shouldUpdateScreeningStatusProvider.ShouldUpdateScreeningStatus = true;
				screeningStatusProvider.ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
				var result = new List<Tuple<ZString, IForwardingShipment>>();
				var consol = parentBizO as IForwardingConsol;
				if (consol != null)
				{
					consol.Shipments.ToList().ForEach(o => SetAllNotClearSubShipmentsToJobClear(o, result));
				}
				else
				{
					var shipment = parentBizO as IForwardingShipment;
					if (shipment != null && shipment.IsMasterShipmentRepresentingAllChildShipments)
					{
						shipment.CoLoadShipments.ToList().ForEach(o => SetAllNotClearSubShipmentsToJobClear(o, result));
					}
				}

				FactorySaveSafe(parentBizO.Factory);
				DpsWorkflowTrackingEvent.AddNewWithScreeningLog(parentBizO, oldStatus, screeningStatusProvider.ScreeningStatus, reason);
				result.ForEach(o =>
				{
					DpsWorkflowTrackingEvent.AddNewWithScreeningLog(o.Item2 as BusinessObject, o.Item1, o.Item2.JS_ScreeningStatus, reason);
				});
			}
		}

		static void SetAllNotClearSubShipmentsToJobClear(IForwardingShipment shipment, List<Tuple<ZString, IForwardingShipment>> result)
		{
			Contract.Requires(result != null);

			if (shipment != null && !(shipment as BusinessObject).IsDeleted)
			{
				var currentScreeningStatus = shipment.JS_ScreeningStatus;
				var isCurrentScreeningStatusNotClear = !(currentScreeningStatus.Equals(ScreeningStatusesList.Codes.Clear) || currentScreeningStatus.Equals(ScreeningStatusesList.Codes.PermanentClear) || currentScreeningStatus.Equals(ScreeningStatusesList.Codes.JobCleared));
				if (isCurrentScreeningStatusNotClear)
				{
					result.Add(new Tuple<ZString, IForwardingShipment>(shipment.JS_ScreeningStatus, shipment));
					shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
				}

				if (shipment.CoLoadShipments.Any() && shipment.IsMasterShipmentRepresentingAllChildShipments)
				{
					shipment.CoLoadShipments.ToList().ForEach(o => SetAllNotClearSubShipmentsToJobClear(o, result));
				}
			}
		}

		public static void RefreshDeniedPartyStatusUpdatedLog(List<BusinessObject> sourceBizOs)
		{
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };

			if (sourceBizOs?.Count > 0)
			{
				var factoriesAlreadySaved = new List<BusinessObjectFactory>();
				foreach (var bizo in sourceBizOs)
				{
					var bizoInNewFactory = newFactory.Load(bizo.TablePrefix, bizo.PK);
					if (bizoInNewFactory is IScreeningStatusProvider screeningStatusProvider)
					{
						var shouldSave = false;
						var currentScreeningStatus = screeningStatusProvider.ScreeningStatus;
						var latestDPELog = bizo.GetLogs()?.MostRecentLogByEventTime(AutoEvents.DeniedPartyStatusUpdated);
						if (latestDPELog != null)
						{
							var paramsDic = StmALog.GetParametersFromReference(latestDPELog.SL_Reference);
							paramsDic.TryGetValue(Params.Codes.New, out var latestStatusInDB);
							if (latestStatusInDB != currentScreeningStatus)
							{
								DpsWorkflowTrackingEvent.AddNew(bizo, latestStatusInDB, currentScreeningStatus);
								shouldSave = true;
							}
						}
						else
						{
							DpsWorkflowTrackingEvent.AddNew(bizo, string.Empty, currentScreeningStatus);
							shouldSave = true;
						}

						if (shouldSave && !factoriesAlreadySaved.Contains(bizo.Factory))
						{
							factoriesAlreadySaved.Add(bizo.Factory);
						}
					}
				}

				foreach (var factory in factoriesAlreadySaved)
				{
					FactorySaveSafe(factory);
				}
			}
		}

		public static void SetShouldUpdateScreeningStatusByOrgHeader(IShouldUpdateScreeningStatus shouldUpdateStatusProvider, string currentStatus, IFactory factory, ZGuid headerPK)
		{
			if (currentStatus != ScreeningStatusesList.Codes.JobCleared || (headerPK.IsValid && ShouldUpdateScreeningStatusForOrgHeader(factory, headerPK)))
			{
				shouldUpdateStatusProvider.ShouldUpdateScreeningStatus = true;
			}
		}

		static bool ShouldUpdateScreeningStatusForOrgHeader(IFactory factory, ZGuid headerPK)
		{
			var header = factory.Load<OrgHeader>(headerPK);
			return header == null
				|| (string)header.OH_ScreeningStatus != ScreeningStatusesList.Codes.JobCleared
				&& (string)header.OH_ScreeningStatus != ScreeningStatusesList.Codes.Clear
				&& (string)header.OH_ScreeningStatus != ScreeningStatusesList.Codes.PermanentClear;
		}

		public static void SetShouldUpdateScreeningStatusByOrgAddress(IShouldUpdateScreeningStatus shouldUpdateStatusProvider, string currentStatus, IFactory factory, ZGuid orgAddressPK)
		{
			if (currentStatus != ScreeningStatusesList.Codes.JobCleared || (orgAddressPK.IsValid && ShouldUpdateScreeningStatusForOrgAddress(factory, orgAddressPK)))
			{
				shouldUpdateStatusProvider.ShouldUpdateScreeningStatus = true;
			}
		}

		static bool ShouldUpdateScreeningStatusForOrgAddress(IFactory factory, ZGuid orgAddressPK)
		{
			var orgAddress = factory.Load<OrgAddress>(orgAddressPK);
			return orgAddress?.Header == null
				|| (string)orgAddress.Header.OH_ScreeningStatus != ScreeningStatusesList.Codes.JobCleared
				&& (string)orgAddress.Header.OH_ScreeningStatus != ScreeningStatusesList.Codes.Clear
				&& (string)orgAddress.Header.OH_ScreeningStatus != ScreeningStatusesList.Codes.PermanentClear;
		}

		public static void SetShouldUpdateScreeningStatusWhenPartAddToBizO(IShouldUpdateScreeningStatus shouldUpdateStatusProvider, BusinessObject parentBizO, BusinessObject bizOAdded)
		{
			if (parentBizO.IsDeleted || bizOAdded == null || bizOAdded.IsDeleted || ShouldSetShouldUpdateScreeningStatusToTrue(parentBizO, bizOAdded))
			{
				shouldUpdateStatusProvider.ShouldUpdateScreeningStatus = true;
			}
		}

		static bool ShouldSetShouldUpdateScreeningStatusToTrue(BusinessObject parentBizO, BusinessObject bizOAdded)
		{
			var parentScreenStatus = (parentBizO as IScreeningStatusProvider)?.ScreeningStatus.ToString();
			var addedScreenStatus = (bizOAdded as IScreeningStatusProvider)?.ScreeningStatus.ToString();
			return !(parentScreenStatus == ScreeningStatusesList.Codes.JobCleared && (addedScreenStatus == ScreeningStatusesList.Codes.JobCleared || addedScreenStatus == ScreeningStatusesList.Codes.Clear || addedScreenStatus == ScreeningStatusesList.Codes.PermanentClear));
		}

		public static void SetShouldUpdateScreeningStatusWhenPartRemovedFromBizO(IShouldUpdateScreeningStatus shouldUpdateStatusProvider, BusinessObject parentBizO)
		{
			if (parentBizO.IsDeleted || (parentBizO as IScreeningStatusProvider)?.ScreeningStatus.ToString() != ScreeningStatusesList.Codes.JobCleared)
			{
				shouldUpdateStatusProvider.ShouldUpdateScreeningStatus = true;
			}
		}

		public static void UpdateRelatedJobsForChangedParties(BusinessObject[] businessObjects, ProgressUpdaterDelegate progressUpdater)
		{
			foreach (var businessObject in businessObjects)
			{
				if (businessObject is OrgHeader header)
				{
					progressUpdater?.Invoke(header.OH_Code.ToString());
					UpdateRelatedJobs(header);
				}
				else if (businessObject is RefVessel vessel)
				{
					progressUpdater?.Invoke(vessel.RV_Code.ToString());
					UpdateRelatedJobs(vessel);
				}
			}
		}

		public static void UpdateRelatedJobsForChangedParties(OrgHeader[] orgHeaders, ProgressUpdaterDelegate progressUpdater)
		{
			foreach (var orgHeader in orgHeaders)
			{
				if (progressUpdater != null)
				{
					progressUpdater(orgHeader.OH_Code.ToString());
				}

				UpdateRelatedJobs(orgHeader);
			}
		}

		public static void UpdateRelatedJobsForChangedParty(OrgHeader orgHeader, ProgressUpdaterDelegate progressUpdater)
		{
			if (progressUpdater != null)
			{
				progressUpdater(orgHeader.OH_Code.ToString());
			}

			UpdateRelatedJobs(orgHeader);
		}

		public static void UpdateRelatedJobsForChangedParty(RefVessel vessel, ProgressUpdaterDelegate progressUpdater)
		{
			if (progressUpdater != null)
			{
				progressUpdater(vessel.RV_Code.ToString());
			}

			UpdateRelatedJobs(vessel);
		}

		public static int UpdateRelatedJobs(OrgHeader orgHeader)
		{
			var result = -1;
			string shipmentUpdateOption = ForwardingRegistryInvoker.ShipmentDpsStatusUpdateSetting.Value.Option;
			string consolUpdateOption = ForwardingRegistryInvoker.ConsolDpsStatusUpdateSetting.Value.Option;

			result = UpdateRelatedJobsByDatesAndBillings(orgHeader, ShouldUpdateByDatesAndBilling(shipmentUpdateOption), ShouldUpdateByDatesAndBilling(consolUpdateOption));

			if (ShouldUpdateByPhases(consolUpdateOption))
			{
				result = UpdateRelatedJobsByPhase(orgHeader, "UpdateRelatedConsolsForOrgOrDocAddressByPhases", ConsolPhasesToUpdate);
			}

			if (ShouldUpdateByPhases(shipmentUpdateOption))
			{
				result = UpdateRelatedJobsByPhase(orgHeader, "UpdateRelatedShipmentsForOrgOrDocAddressByPhases", ShipmentPhasesToUpdate);
			}

			new RelatedJobsComplianceRiskStatusUpdater().UpdateForOrg(orgHeader.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid(), null);

			return result;
		}

		public static bool ShouldUpdateByDatesAndBilling(string option) => option == "DAB" || option == "COM";

		public static bool ShouldUpdateByPhases(string option) => option == "PHS" || option == "COM";

		public static int UpdateRelatedJobsByDatesAndBillings(OrgHeader orgHeader, bool updateShipments = true, bool updateConsols = true)
		{
			DbCommand cmd = Db.Connection.Command("UpdateRelatedJobsForOrgOrDocAddress"); // Have to use stored procedure instead of BusinessObjectFactory as there might be a big amount of jobs to be updated.
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandTimeout = OrganisationsDataRegistry.Instance.DeniedPartyScreeningTimeoutForSQLCommandQuery.Value;
			cmd.AddParameter("@entityPK", SqlDbType.UniqueIdentifier, orgHeader.PK.ToGuid());
			cmd.AddParameter("@earliestDT", SqlDbType.DateTime, ZDateTime.Today.AddDays(-7).ToDateTime());
			cmd.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
			cmd.AddParameter("@jobUpdatePeriod", SqlDbType.SmallDateTime, ZDateTime.UtcNow.AddMonths(-OrganisationsDataRegistry.Instance.JobUpdatePeriod.Value).ToDateTime());
			cmd.AddParameter("@userCode", SqlDbType.VarChar, 3, GlbStaff.CurrentUser.GS_Code.ToString());
			cmd.AddParameter("@freightCpwEnabled", SqlDbType.Bit, ComplianceRiskHelper.IsFreightEnabledComplianceWise ? 1 : 0);

			if (!updateShipments)
			{
				cmd.AddParameter("@updateShipments", SqlDbType.Bit, 0);
			}

			if (!updateConsols)
			{
				cmd.AddParameter("@updateConsols", SqlDbType.Bit, 0);
			}

			return cmd.ExecuteProcedureWithReturnValue();
		}

		static int UpdateRelatedJobsByPhase(OrgHeader orgHeader, string procedureName, DataTable phaseTable)
		{
			DbCommand cmd = Db.Connection.Command(procedureName);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandTimeout = OrganisationsDataRegistry.Instance.DeniedPartyScreeningTimeoutForSQLCommandQuery.Value;
			cmd.AddParameter("@entityPK", SqlDbType.UniqueIdentifier, orgHeader.PK.ToGuid());
			cmd.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
			cmd.AddParameter("@userCode", SqlDbType.VarChar, 3, GlbStaff.CurrentUser.GS_Code.ToString());
			cmd.AddTableValuedParameter("@phaseList", "dbo.TVP_Char_3", phaseTable);

			return cmd.ExecuteProcedureWithReturnValue();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Name of registry type, only for dev people")]
		const string forwardingRegistryTypeString = "Enterprise.Freight.Forwarding.Registry.ForwardingConfigurationRegistry, Enterprise.Freight.Forwarding.Registry";

		static dynamic ForwardingRegistryInvoker => System.Type.GetType(forwardingRegistryTypeString).GetProperty("Instance").GetValue(null);

		public static string ShipmentUpdateOption => ForwardingRegistryInvoker.ShipmentDpsStatusUpdateSetting.Value.Option;
		public static string ConsolUpdateOption => ForwardingRegistryInvoker.ConsolDpsStatusUpdateSetting.Value.Option;

		static DataTable GetPhasesArrayAsTable(dynamic registryItem)
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

		public static DataTable ConsolPhasesToUpdate => GetPhasesArrayAsTable(ForwardingRegistryInvoker.ConsolDpsStatusUpdateSetting);

		public static DataTable ShipmentPhasesToUpdate => GetPhasesArrayAsTable(ForwardingRegistryInvoker.ShipmentDpsStatusUpdateSetting);

		public static int UpdateRelatedJobs(RefVessel vessel)
		{
			var sql = @$"
UPDATE dbo.JobConsolTransport
SET
	JW_VesselScreeningStatus = @screeningStatus,
	JW_SystemLastEditTimeUtc = GETUTCDATE(),
	JW_SystemLastEditUser = '{GlbStaff.CurrentUser.GS_Code.ToString()}'
FROM
	dbo.JobConsolTransport
WHERE
	JW_Vessel = @vesselCode";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.CommandType = CommandType.Text;
				cmd.CommandTimeout = OrganisationsDataRegistry.Instance.DeniedPartyScreeningTimeoutForSQLCommandQuery.Value;
				cmd.AddParameter("@vesselCode", SqlDbType.VarChar, vessel.RV_Code.ToString());
				cmd.AddParameter("@screeningStatus", SqlDbType.VarChar, vessel.RV_ScreeningStatus.ToString());

				cmd.ExecuteNonQuery();
			}

			new RelatedJobsComplianceRiskStatusUpdater().UpdateForVessel(vessel.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid(), null);

			using (var cmd = Db.Connection.Command("UpdateRelatedJobsForVessel"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandTimeout = OrganisationsDataRegistry.Instance.DeniedPartyScreeningTimeoutForSQLCommandQuery.Value;
				cmd.AddParameter("@vesselPK", SqlDbType.UniqueIdentifier, vessel.PK.ToGuid());
				cmd.AddParameter("@earliestDT", SqlDbType.DateTime, ZDateTime.Today.AddDays(-7).ToDateTime());
				cmd.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
				cmd.AddParameter("@jobUpdatePeriod", SqlDbType.SmallDateTime, ZDateTime.UtcNow.AddMonths(-OrganisationsDataRegistry.Instance.JobUpdatePeriod.Value).ToDateTime());
				cmd.AddParameter("@userCode", SqlDbType.VarChar, 3, GlbStaff.CurrentUser.GS_Code.ToString());
				cmd.AddParameter("@freightCpwEnabled", SqlDbType.Bit, ComplianceRiskHelper.IsFreightEnabledComplianceWise ? 1 : 0);

				return cmd.ExecuteProcedureWithReturnValue();
			}
		}

		public static ZString GetJobScreeningStatus(IEnumerable<ZString> statuses)
		{
			var jobStatus = ScreeningStatusesList.Codes.Release;

			foreach (var status in statuses)
			{
				if (status == ScreeningStatusesList.Codes.Unknown || status == ScreeningStatusesList.Codes.Matched || status == ScreeningStatusesList.Codes.NotScreened)
				{
					jobStatus = ScreeningStatusesList.Codes.Block;
					break;
				}
			}
			return jobStatus;
		}

		#region WorstScreeningParty

		public static ZString GetWorstScreeningStatus(IEnumerable<IScreeningPartyProvider> parties)
		{
			var worstStatus = ScreeningStatusesList.Codes.PermanentClear;
			var worstRating = -1;

			foreach (var party in parties.WhereNotNull())
			{
				var status = party.GetWorstScreeningStatusUnlessManuallyCleared();
				var rating = ScreeningStatusRating(status);

				if (rating > worstRating)
				{
					worstStatus = status;
					worstRating = rating;
				}
			}

			return worstStatus;
		}

		public static ZString GetWorstScreeningStatus(IEnumerable<ZString> statuses)
		{
			var worstStatus = ScreeningStatusesList.Codes.PermanentClear;
			var worstRating = -1;

			foreach (var status in statuses)
			{
				var rating = ScreeningStatusRating(status);

				if (rating > worstRating)
				{
					worstStatus = status;
					worstRating = rating;
				}
			}

			return worstStatus;
		}

		public static int ScreeningStatusRating(ZString status)
		{
			var statusRating = -1;

			switch (status)
			{
				case ScreeningStatusesList.Codes.PermanentClear:
					statusRating = -1;
					break;
				case ScreeningStatusesList.Codes.Clear:
					statusRating = 0;
					break;
				case ScreeningStatusesList.Codes.JobCleared:
					statusRating = 1;
					break;
				case ScreeningStatusesList.Codes.Unknown:
					statusRating = 2;
					break;
				case ScreeningStatusesList.Codes.RequiresReview:
					statusRating = 3;
					break;
				case ScreeningStatusesList.Codes.NotScreened:
					statusRating = 4;
					break;
				case ScreeningStatusesList.Codes.Matched:
					statusRating = 5;
					break;
			}

			return statusRating;
		}

		#endregion

		public static List<ScreeningPartiesSnapshot> GetScreeningPartiesSnapshot(ScreeningParty[] screeningParties, BusinessObject bizO)
		{
			var parties = screeningParties.Where(s => !s.Key.IsEmpty && s.Parent == bizO);
			var uniqueParties = new Dictionary<ZGuid, ZString>();
			var snapShots = new List<ScreeningPartiesSnapshot>();

			parties.GroupBy(u => u.Key).ForEach(p =>
			{
				var def = p.FirstOrDefault();

				var isOverride = false;
				if (def.DocAddress != null)
				{
					isOverride = def.DocAddress.E2_AddressOverride;
				}

				var code = def.Code;
				var des = string.Join("|", p.Select(i => i.Description));

				var res = isOverride ? string.Format(CultureInfo.InvariantCulture, (NoResString)"Override Address({0})", des)
							: string.Format(CultureInfo.InvariantCulture, "{0}({1})", code, des);

				if (!uniqueParties.ContainsKey(p.Key))
				{
					uniqueParties.Add(p.Key, res);

					snapShots.Add(new ScreeningPartiesSnapshot
					{
						Description = res,
						Key = p.Key
					});
				}
			});

			return snapShots;
		}

		static void FactorySaveSafe(BusinessObjectFactory factory)
		{
			try
			{
				factory.Save();
			}
			catch (Exception ex) when (ex is ZSaveConcurrencyException || ex is ZCannotSaveException || ex is ZSaveException)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		public static void ProcessInvalidateLocalDataChanges(IDpsEntityProvider entityProvider, Action updateExtensions)
		{
			if (entityProvider.ScreeningStatus != ScreeningStatusesList.Codes.PermanentClear)
			{
				entityProvider.InvalidateByLocalDataChanges();

				if (entityProvider.NeedsScreening)
				{
					if (entityProvider.ScreeningStatus != ScreeningStatusesList.Codes.Unknown)
					{
						updateExtensions();

						entityProvider.ShouldUpdateRelatedJobs = !OrganisationsDataRegistry.Instance.DeniedPartyScreeningEnableLogWalkerServiceTaskToUpdateRelatedJobs.Value;

						DpsWorkflowTrackingEvent.AddNew((BusinessObject)entityProvider, entityProvider.OriginalScreeningStatus, ScreeningStatusesList.Codes.Unknown);
					}

					entityProvider.ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				}
			}
		}
	}
}

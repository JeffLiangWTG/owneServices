using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.Business
{
	public static class ShipmentExtension
	{
		public static bool HasTransferredLog(this ForwardingShipment shipment, string moduleCode)
		{
			shipment.Logs.GetAllLogs().Reload(true);
			return shipment.Logs.HasLogWith(log =>
				!log.SL_IsCancelled
				&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var type)
				&& type == moduleCode
				&& log.SL_SE_NKEvent == AutoEvents.Transferred.Code);
		}

		public static bool HasSeaAMSTransferredLog(this ForwardingShipment shipment)
		{
			shipment.Logs.GetAllLogs().Reload(true);
			return shipment.Logs.HasLogWith(log =>
				!log.SL_IsCancelled
				&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var type)
				&& type == CustomsModuleCodes.Codes.AutomatedManifestSystem
				&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Mode, out var mode)
				&& mode == TransportModes.Sea);
		}

		public static bool HasAirAMSTransferredLog(this ForwardingShipment shipment)
		{
			shipment.Logs.GetAllLogs().Reload(true);
			return shipment.Logs.HasLogWith(log =>
				!log.SL_IsCancelled
				&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var type)
				&& type == CustomsModuleCodes.Codes.AutomatedManifestSystem
				&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Mode, out var mode)
				&& mode == TransportModes.Air);
		}

		public static HVLVConsignmentHeader GetOrCreateHVLVConsignmentHeader(this ForwardingShipment shipment)
		{
			return HVLVConsignmentHeader.GetOrCreate(shipment);
		}

		public static HVLVConsignmentHeader GetHVLVConsignmentHeader(this ForwardingShipment shipment)
		{
			return HVLVConsignmentHeader.Get(shipment);
		}

		public static void SetSecurityFilingFirstUsageTimeForAllItems(this ForwardingShipment shipment)
		{
			var utcNow = ZDateTime.UtcNow;
			shipment.HVLVItems.ForEach(item =>
			{
				if (item.HVI_SecurityFilingFirstUsageTimeUtc.IsEmpty)
				{
					item.HVI_SecurityFilingFirstUsageTimeUtc = utcNow;
				}
			});
		}

		public static void SetLastUsageCodeForAllItems(this ForwardingShipment shipment, string lastUsageCode)
		{
			shipment.HVLVItems.ForEach(item => item.HVI_LastUsageCode = lastUsageCode);
		}

		public static IEnumerable<HVLVConsignment> GetHVLVConsignmentsWithItemLoadedOnShipment(this ForwardingShipment shipment)
		{
			var consignments = new HashSet<ZGuid>();
			var items = shipment.HVLVItems;

			foreach (HVLVItem item in items)
			{
				if (!consignments.Contains(item.HVI_HVC_Consignment))
				{
					var consignment = item.Consignment;

					if (consignment.HVC_IsActive)
					{
						consignments.Add(item.HVI_HVC_Consignment);
						yield return consignment;
					}
				}
			}
		}

		public static string SCACCode(this ForwardingShipment shipment, BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(shipment.PK + "|SCACCodeForUSISF", () => shipment.GetValidSCACIssuerCodes(shipment.TransportMode).FirstOrDefault());
		}

		public static bool IsShipmentDestinationUS(this ForwardingShipment shipment)
		{
			return CountryCodes.GetCustomsCountryOfJurisdiction(shipment.Destination.Country.Code) == CountryCodes.UnitedStates;
		}

		public static StmALog GetLatestCargoReportingLog(this ForwardingShipment shipment)
		{
			shipment.Logs.GetAllLogs().Reload(true);
			return shipment.Logs.Find(IsCargoReportingLog).MaxBySafe(x => x.SL_EventTime);
		}

		public static StmALog GetLatestCargoReportedLog(this ForwardingShipment shipment)
		{
			shipment.Logs.GetAllLogs().Reload(true);
			return shipment.Logs.Find(IsCargoReportedLog).MaxBySafe(x => x.SL_EventTime);
		}

		static bool IsCargoReportingLog(StmALog log) => IsLogVisibleToCurrentUser(log)
			&& log.SL_SE_NKEvent == AutoEvents.HVLVReadyCode
			&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, out var reason)
			&& HLRReasonsForCargoReporting.Any(x => x == reason);

		static bool IsCargoReportedLog(StmALog log) => IsLogVisibleToCurrentUser(log)
			&& log.SL_SE_NKEvent == AutoEvents.HVLVReadyCode
			&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, out var reason)
			&& HLRReasonForCargoReportCreated.Any(x => x == reason);

		static IEnumerable<string> HLRReasonsForCargoReporting
		{
			get
			{
				yield return EventReferenceParameterReasons.CargoReporting;
				yield return EventReferenceParameterReasons.AmendmentProcessing;
			}
		}

		static bool IsLogVisibleToCurrentUser(StmALog log)
		{
			var result = false;
			var logBranch = log.Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, log.SL_GB_NKBranch);
			if (logBranch != null)
			{
				var logCompany = logBranch.Company;
				if (logCompany.GC_RN_NKCountryCode == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
				{
					result = true;
				}
			}

			return result;
		}

		public static bool IsCargoReportCreated(this ForwardingShipment shipment)
		{
			shipment.Logs.GetAllLogs().Reload(true);
			return shipment.Logs.HasLogWith(log => IsLogVisibleToCurrentUser(log)
				&& !log.SL_IsCancelled
				&& log.SL_SE_NKEvent == AutoEvents.HVLVReadyCode
				&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, out var reason)
				&& HLRReasonForCargoReportCreated.Any(x => x == reason));
		}

		static IEnumerable<string> HLRReasonForCargoReportCreated
		{
			get
			{
				yield return EventReferenceParameterReasons.CargoReportCreated;
				yield return EventReferenceParameterReasons.CargoReportAmended;
			}
		}
	}
}

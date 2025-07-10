using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class ForwardingShipmentExtensions
	{
		public static bool IsFHLShipment(this ForwardingShipment shipment)
		{
			return shipment != null
				&& shipment.IsAWBHeaderAccessible
				&& !shipment.IsBlindCoLoadMaster
				&& (shipment.CoLoadMasterShipment == null
					|| shipment.CoLoadMasterShipment.IsBlindCoLoadMaster
					|| (!shipment.CoLoadMasterShipment.IsMasterShipmentRepresentingAllChildShipments && !shipment.CoLoadMasterShipment.IsHighVolumeLowValueMaster)
					|| (shipment.CoLoadMasterShipment.IsHighVolumeLowValueMaster && HVLVDataRegistry.Instance.EnableHVLVFHLMessaging.Value));
		}

		public static IEnumerable<ForwardingShipment> GetShipmentsForBill(this ForwardingShipment shipment)
		{
			var shipments = new List<ForwardingShipment>();
			shipments.Add(shipment);
			if (shipment.IsBuyersConsolLead)
			{
				shipments.AddRange(shipment.CoLoadShipments.Cast<ForwardingShipment>());
			}
			return shipments;
		}

		public static bool IsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder(this ForwardingShipment shipment)
		{
			if (shipment == null
				|| !shipment.IsInDatabase
				|| shipment.JS_TransportMode != Core.Constants.TransportModes.Sea
				|| shipment.IsTemplate
				|| shipment.JS_SystemCreateTimeUtc.IsEmpty) // edge case related to loading Quoted Booking from saved template record
			{
				return false;
			}

			var subQuery = new ZQuery();
			subQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, $"|NEW={ShipmentStatusList.Codes.ElectronicBooking}");
			subQuery.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, $"|NEW={ShipmentStatusList.Codes.ElectronicShippingInstruction}");
			subQuery.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, $"|NEW={ShipmentStatusList.Codes.WebBooking}");

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdated.Code);
			logQuery.AddToFilter(StmALogSchema.SL_Table, JobShipmentSchema.Constants.TableName);
			logQuery.AddToFilter(StmALogSchema.SL_Parent, shipment.PK);
			logQuery.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			logQuery.AddToFilter(subQuery);

			return shipment.Factory.Exists(typeof(StmALog), logQuery);
		}

		public static ZString GetAdditionalInspectionTypeCodeDefault(this ForwardingShipment shipment)
		{
			if (shipment == null || !shipment.AviationSecurity.SupplyChainSecurityConfiguration.IsHighRiskApplicable)
			{
				return ZString.Empty;
			}
			return FreightDataRegistry.AviationSecurity_Unknown_Code;
		}

		public static string GetChargesCacheKey(this ForwardingShipment shipment)
		{
			return $"Charges_{shipment.PK}";
		}

		public static void ClearCachedDocumentData(this ForwardingShipment shipment)
		{
			shipment.Factory.ClearCachedValue<JobCharge[]>(shipment.GetChargesCacheKey());
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CusMAWB = Enterprise.Customs.NZ.Business.Express.CusMAWB;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public static class Extensions
	{
		public static CusMAWB[] FindMatchingMAWBs(this CusMAWB.Loader loader, Shipment shipment, Shipment subShipment)
		{
			CusMAWB[] cusMAWBs = null;
			shipment.PortOfLoading.TryGetUNLOCOAsUpperCase(loader.Factory, out var loadPort);
			shipment.PortOfDischarge.TryGetUNLOCOAsUpperCase(loader.Factory, out var dischargePort);

			if (subShipment != null)
			{
				cusMAWBs = FindMatchingMAWBsForHVLV(loader, shipment, subShipment);
			}

			if (cusMAWBs == null || cusMAWBs.Length == 0)
			{
				var findFilter = new ZQuery();
				findFilter.AddToFilter(CusMAWBSchema.CM_ApplicationCode, loader.ApplicationCodes);
				findFilter.AddToFilter(CusMAWBSchema.CM_MAWB, shipment.GetMasterBill());
				findFilter.AddToFilter(CusMAWBSchema.CM_FlightNo, shipment.VoyageFlightNo.GetValueOrDefault());
				findFilter.AddToFilter(CusMAWBSchema.CM_IsCTOMAWB, ZBool.False);
				findFilter.AddToFilter(CusMAWBSchema.CM_MasterHouseBill, shipment.GetColoadBill(subShipment).GetValueOrDefault());
				findFilter.AddToFilter(CusMAWBSchema.CM_IsActive, true);
				findFilter.OrderBy = CusMAWBSchema.Constants.CM_SystemCreateTimeUtc + " desc";
				var mAWBRecyclePeriod = FreightDataRegistry.Instance.MAWBRecyclePeriod.Value;
				if (mAWBRecyclePeriod > 0)
				{
					findFilter.AddToFilter(CusMAWBSchema.CM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now.AddMonths(-mAWBRecyclePeriod));
				}
				if (CusMAWB.CheckIsImport(loadPort, dischargePort))
				{
					findFilter.AddToFilter(CusMAWBSchema.CM_ArrivalDate,
						SQLComparisonOperator.EqualToDatePartOnly,
						shipment.GetDischargeDateForAir(shipment.GetArrivalTransportLeg()).GetValueOrDefault());
				}
				else if (CusMAWB.CheckIsExport(loadPort, dischargePort))
				{
					findFilter.AddToFilter(CusMAWBSchema.CM_DepartureDate,
						SQLComparisonOperator.EqualToDatePartOnly,
						shipment.GetLoadingDateForAir(shipment.GetDepartureTransportLeg()).GetValueOrDefault());
				}

				cusMAWBs = loader.Factory.Load<CusMAWB>(findFilter);
			}

			return cusMAWBs;
		}

		static CusMAWB[] FindMatchingMAWBsForHVLV(CusMAWB.Loader loader, Shipment shipment, Shipment subShipment)
		{
			CusMAWB[] results;
			var query = new ZQuery(CusMAWBSchema.CM_MAWB, shipment.GetMasterBill());
			query.AddToFilter(CusMAWBSchema.CM_IsActive, true);
			query.OrderBy = CusMAWBSchema.Constants.CM_SystemCreateTimeUtc + " desc";
			if (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.Value)
			{
				results = loader.Factory.Load<CusMAWB>(query);

				if (results.Length > 0)
				{
					results = results.Where(b => b.Logs.GetAllLogs().Cast<StmALog>().Any(log => log.Parameters.TryGetValue(EventReferenceParameters.Codes.Type, out var type)
						&& type == Core.Constants.ShipmentTypes.HighVolumeLowValue)).ToArray();
				}
			}
			else
			{
				query.AddToFilter(CusMAWBSchema.CM_MasterHouseBill, shipment.GetColoadBill(subShipment).GetValueOrDefault());
				results = loader.Factory.Load<CusMAWB>(query);

				if (results.Length > 0)
				{
					var shipmentJobNumber = subShipment.DataContext?.DataSourceCollection?.FirstOrDefault()?.Key;
					if (shipmentJobNumber.HasValue)
					{
						results = results.Where(b => b.Logs.GetAllLogs().Cast<StmALog>().Any(l =>
							l.Parameters.TryGetValue(EventReferenceParameters.Codes.Type, out var type) && type == Core.Constants.ShipmentTypes.HighVolumeLowValue
							&& (l.Parameters.TryGetValue(EventReferenceParameters.Codes.JobNumber, out var jobNumber) && jobNumber == shipmentJobNumber.ToString()
							|| l.Parameters.TryGetValue(EventReferenceParameters.Codes.ReferenceNumber, out var refNumber) && refNumber == shipmentJobNumber.ToString()))).ToArray();
						}
				}
			}

			return results;
		}

		public static CusSCAOceanBill[] FindMatchingSCAOceanBills(this CusSCAOceanBill.Loader loader, Shipment shipment, Shipment subShipment)
		{
			var findFilter = new ZQuery();
			findFilter.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, loader.GetApplicationCodes());
			findFilter.AddToFilter(CusSCAOceanBillSchema.CB_OceanBill, shipment.WayBillNumber.GetValueOrDefault());
			findFilter.AddToFilter(CusSCAOceanBillSchema.CB_LloydsIMO, shipment.LloydsIMO.GetValueOrDefault());
			findFilter.AddToFilter(CusSCAOceanBillSchema.CB_Voyage, shipment.VoyageFlightNo.GetValueOrDefault());

			if (!shipment.IsHVLV() || !HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.Value)
			{
				findFilter.AddToFilter(CusSCAOceanBillSchema.CB_MasterHouseBill, shipment.GetColoadBill(subShipment).GetValueOrDefault());
			}

			findFilter.AddToFilter(CusSCAOceanBillSchema.CB_IsActive, true);
			findFilter.OrderBy = CusSCAOceanBillSchema.Constants.CB_SystemCreateTimeUtc + " desc";

			return loader.Factory.Load<CusSCAOceanBill>(findFilter);
		}

		public static IEnumerable<Shipment> GetLowestLevelShipment(this Shipment shipment, bool isThisTopLevelShipment = true)
		{
			if (shipment.SubShipmentCollection != null)
			{
				foreach (var subShipment in shipment.SubShipmentCollection)
				{
					foreach (var subSubShipment in subShipment.GetLowestLevelShipment(false))
					{
						yield return subSubShipment;
					}
				}
			}
			else if (!isThisTopLevelShipment)
			{
				yield return shipment;
			}
		}

		public static TransportLeg GetDepartureTransportLeg(this Shipment shipment)
		{
			var loadPort = shipment.PortOfLoading != null ? shipment.PortOfLoading.Code.GetValueOrDefault() : ZString.Empty;
			return shipment.TransportLegCollection?.OrderBy(x => x.LegOrder).FirstOrDefault(x =>
				{
					var legLoadPort = x.PortOfLoading != null ? x.PortOfLoading.Code.GetValueOrDefault() : ZString.Empty;
					return legLoadPort == loadPort;
				});
		}

		public static TransportLeg GetArrivalTransportLeg(this Shipment shipment)
		{
			var discPort = shipment.PortOfDischarge != null ? shipment.PortOfDischarge.Code.GetValueOrDefault() : ZString.Empty;
			return shipment.TransportLegCollection?.OrderBy(x => x.LegOrder).FirstOrDefault(x =>
			{
				var legDiscPort = x.PortOfDischarge != null ? x.PortOfDischarge.Code.GetValueOrDefault() : ZString.Empty;
				return legDiscPort == discPort;
			});
		}

		internal static bool ShouldMarkBillAsUnprocessed_HVLV(this Customs.Business.CusHAWB hawb, ZGuid shipmentPK)
		{
			bool shouldMarkBillAsUnprocessed;
			if (hawb.CS_JS.IsEmpty)
			{
				shouldMarkBillAsUnprocessed = true;
			}
			else if (!hawb.CS_IsHVLV)
			{
				shouldMarkBillAsUnprocessed = false;
			}
			else
			{
				shouldMarkBillAsUnprocessed = !(HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.Value && !hawb.BillIsOnShipment(shipmentPK));
			}

			return shouldMarkBillAsUnprocessed;
		}

		static bool BillIsOnShipment(this Customs.Business.CusHAWB hawb, ZGuid shipmentPK)
		{
			return shipmentPK.IsValid && hawb.CS_JS == shipmentPK;
		}

		internal static bool ShouldDeleteUnprocessedHouseBill_HVLV(this BaseCusSCAHouse house, Func<ZGuid?> shipmentPKGetter)
		{
			bool shouldDeleteUnprocessedHouseBill;
			if (house.CA_JS.IsEmpty)
			{
				shouldDeleteUnprocessedHouseBill = true;
			}
			else if (!house.CA_IsHVLV)
			{
				shouldDeleteUnprocessedHouseBill = false;
			}
			else
			{
				var shipmentPK = shipmentPKGetter();
				shouldDeleteUnprocessedHouseBill = !(HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.Value && !house.BillIsOnShipment(shipmentPK));
			}

			return shouldDeleteUnprocessedHouseBill;
		}

		static bool BillIsOnShipment(this BaseCusSCAHouse house, ZGuid? shipmentPK)
		{
			var isBillOnShipment = false;

			if (shipmentPK.HasValue && shipmentPK.Value.IsValid)
			{
				isBillOnShipment = (house.CA_JS == shipmentPK.Value);
			}

			return isBillOnShipment;
		}
	}
}
